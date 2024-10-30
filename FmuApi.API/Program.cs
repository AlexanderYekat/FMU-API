using FmuApiApplication.Services.Fmu;
using FmuApiApplication.Services.Installer;
using FmuApiApplication.Services.TrueSign;
using FmuApiApplication.Workers;
using Microsoft.AspNetCore.Mvc.Controllers;
using CouchDB.Driver.DependencyInjection;
using FmuApiApplication.Services.AcoUnit;
using FmuApiSettings;
using FmuApiApplication.Utilites;
using Serilog;
using FmuApiAPI;
using FmuApiCouhDb;
using FmuApiCouhDb.CrudServices;
using System.Net;
using FmuFrontolDb;
using Microsoft.EntityFrameworkCore;
using FmuApiApplication.Services.Frontol;
using AutoUpdateWorkerService;

var slConsole = new LoggerConfiguration()
    .MinimumLevel.Debug().WriteTo
                 .Console()
                 .CreateLogger();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSerilog(slConsole);
});

ILogger < Program> logger = loggerFactory.CreateLogger<Program>();

_ = (args.Length == 0 ? "" : args[0]) switch
{
    "--service" => RunHttpApiService(),
    "--install" => await InstallAsWindowsServiceAsync(),
    "--uninstall" => UninstallWindowsService(),
    _ => ShowAppInfo()
};

bool RunHttpApiService()
{
    string dataFolder = StringHelper.ArgumentValue(args, "--dataFolder", "");
    Constants.Init(dataFolder);

    WebApplicationOptions webApplicationOptions = new()
    {
        ContentRootPath = AppContext.BaseDirectory,
        ApplicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName
    };

    WebApplicationBuilder? builder = WebApplication.CreateBuilder(webApplicationOptions);

    ConfigureLogging(builder);

    var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
    logger = loggerFactory.CreateLogger<Program>();
    
    logger.LogInformation("\n\n");
    logger.LogInformation("********************************************************");
    logger.LogInformation("*                                                      *");
    logger.LogInformation("*              ЗАПУСК FMU API v{Version}              *", version);
    logger.LogInformation("*                                                      *");
    logger.LogInformation("********************************************************");
    logger.LogInformation("\n\n");

    builder.WebHost.UseUrls($"http://+:{Constants.Parametrs.ServerConfig.ApiIpPort}");

    var services = builder.Services;

    services.Configure<RouteOptions>(option =>
    {
        option.AppendTrailingSlash = true;
        option.LowercaseQueryStrings = true;
        option.LowercaseUrls = true;
    });

    services.AddHttpClient();

    services.AddScoped<CheckMarks>();

    services.AddScoped<FrontolDocument>();
    services.AddScoped<ProductInfo>();

    services.AddHttpClient<AlcoUnitGateway>("alcoUnit", options =>
    {
        options.BaseAddress = new Uri(Constants.Parametrs.FrontolAlcoUnit.NetAdres);
        options.Timeout = TimeSpan.FromSeconds(20);
    });
    services.AddScoped<AlcoUnitGateway>();

    services.AddHostedService<CdnLoaderWorker>();
    AutoUpdateWorker.AddService(services);

    if (Constants.Parametrs.TrueSignTokenService.ConnectionAddres != string.Empty)
        services.AddHostedService<TrueSignTokenServiceLoaderWorker>();

    if (Constants.Parametrs.HostsToPing.Count > 0)
    {
        services.AddHttpClient("internetCheck");
        services.AddHostedService<InternetConnectionCheckWorker>();
    }

    ConfigureCors(services);

    CouchDbRegisterService.AddService(services);
    // Добавьте базовые заглушки для CouchDB если он отключен
    //if (!Constants.Parametrs.Database.ConfigurationIsEnabled)
    //{
    //    builder.Services.AddScoped<ICouchContext>(sp => new NullCouchContext());
    //}    
    FrontolDbRegisterService.AddService(services);

    ConfigureSwagger(services);

    if (OperatingSystem.IsWindows())
    {
        builder.Host.UseWindowsService();
    }

    builder.Services.AddRazorPages();

    var app = builder.Build();

    logger.LogInformation("===========================================");
    logger.LogInformation("FMU API v{Version} успешно инициализирован", version);
    logger.LogInformation("===========================================");

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors();

    app.UseStaticFiles();
    app.UseRouting();

    app.MapRazorPages();
    app.MapControllers();

    app.Run();

    return true;
}

void ConfigureLogging(WebApplicationBuilder builder)
{
    if (!Constants.Parametrs.Logging.IsEnabled)
    {
        logger.LogWarning("Логирование отключено в настройках!");
        return;
    }

    string logFileName = string.Concat(Constants.DataFolderPath, "\\log\\fmu-api.log");
    
    // Проверяем доступность папки для логов
    var logDir = Path.GetDirectoryName(logFileName);
    if (!Directory.Exists(logDir))
    {
        try
        {
            Directory.CreateDirectory(logDir!);
        }
        catch (Exception ex)
        {
            logger.LogError("Не удалось создать папку для логов: {Error}", ex.Message);
            return;
        }
    }

    var logConfig = Constants.Parametrs.Logging.LogLevel.ToLower() switch
    {
        "verbose" => LoggerConfig.Verbose(logFileName, Constants.Parametrs.Logging.LogDepth),
        "debug" => LoggerConfig.Debug(logFileName, Constants.Parametrs.Logging.LogDepth),
        "information" => LoggerConfig.Information(logFileName, Constants.Parametrs.Logging.LogDepth),
        "warning" => LoggerConfig.Warning(logFileName, Constants.Parametrs.Logging.LogDepth),
        "error" => LoggerConfig.Error(logFileName, Constants.Parametrs.Logging.LogDepth),
        "fatal" => LoggerConfig.Fatal(logFileName, Constants.Parametrs.Logging.LogDepth),
        _ => LoggerConfig.Information(logFileName, Constants.Parametrs.Logging.LogDepth)
    };

    builder.Logging.AddSerilog(logConfig);
    
    logger.LogInformation("Логирование настроено. Файл логов: {LogFile}", logFileName);
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(option =>
    {
        option.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        option.IgnoreObsoleteActions();
        option.IgnoreObsoleteProperties();
        option.CustomSchemaIds(type => type.FullName);
        option.TagActionsBy(api =>
        {
            if (api.GroupName != null)
            {
                return [api.GroupName];
            }

            ControllerActionDescriptor? controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                return [controllerActionDescriptor.ControllerName];
            }

            throw new InvalidOperationException("Unable to determine tag for endpoint.");
        });
        option.DocInclusionPredicate((name, api) => true);
    });

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

void ConfigureCors(IServiceCollection services)
{
    List<string> hostAdreses = [];

    hostAdreses.Add($"http://{Dns.GetHostName()}:{Constants.Parametrs.ServerConfig.ApiIpPort}");
    hostAdreses.Add($"http://localhost:{Constants.Parametrs.ServerConfig.ApiIpPort}");
    hostAdreses.Add($"http://127.0.0.1:{Constants.Parametrs.ServerConfig.ApiIpPort}");

    services.AddCors(opt =>
    {
        opt.AddDefaultPolicy(
            policy =>
            {
                policy.WithOrigins(hostAdreses.ToArray()).AllowAnyMethod();
            });
    });
}

bool ShowAppInfo()
{
    logger.LogInformation(
        "Доступные команды:\r\n" +
        "  --service       Запуск приложения как HTTP API сервиса\r\n" +
        "    --dataFolder  Путь к папке с данными (необязательный параметр)\r\n" +
        "  --install      Установить приложение как службу Windows\r\n" +
        "    --xapikey    API ключ для авторизации\r\n" +
        "  --uninstall    Удалить службу Windows");

    return true;
}

async Task<bool> InstallAsWindowsServiceAsync()
{
    WindowsInstallerService installerService = new(logger);
    return await installerService.InstallAsync(args);
}

bool UninstallWindowsService()
{
    WindowsInstallerService installerService = new(logger);
    return installerService.Uninstall();
}