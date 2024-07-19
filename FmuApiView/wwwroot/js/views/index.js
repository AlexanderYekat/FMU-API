import { InitProxy } from '../../js/utils/proxy.js';
//import { InitUiProto } from '/../js/utils/ui.js';
import { SettingsView } from "./fmuApiSettings/fmuApisettings.js";

const windowInnerHeight = window.innerHeight;
const bodyId = "body";

InitProxy();
//InitUiProto();

webix.ready(function () {
    webix.ui({
        container: "app",
        id: "root",
        responsive: true,
        rows: [
            {
                view: "toolbar",
                padding: 5,
                height: 60,
                elements:
                    [
                        {
                            view: "label",
                            id: "toolbarLabel",
                            label: "FMU-API"
                        }
                    ]
            },
            {
                cols:
                    [
                        {
                            view: "sidebar",
                            id: "sidebar",
                            width: 200,
                            height: windowInnerHeight - 80,
                            collapsed: false,
                            data:
                                [
                                    {
                                        id: "config",
                                        value: "Настройка",
                                        data: [
                                            {
                                                id: "LServerConfig",
                                                value: "Сервера"
                                            },
                                            {
                                                id: "lOrganizations",
                                                value: "Организации"
                                            },
                                            {
                                                id: "lHostsToPing",
                                                value: "Проверка интернета"
                                            },
                                            {
                                                id: "lDatabase",
                                                value: "База данных"
                                            },
                                            {
                                                id: "lMinimalPrices",
                                                value: "Минимальные цены"
                                            },
                                            {
                                                id: "lFrontolConnection",
                                                value: "Frontol"
                                            },
                                            {
                                                id: "lFrontolAlcoUnit",
                                                value: "Frontol AlcoUnit"
                                            },
                                            {
                                                id: "lTruesignService",
                                                value: "Сервис получения токена ЧЗ"
                                            },
                                            {
                                                id: "lTimeoutConfig",
                                                value: "Таймауты"
                                            },
                                            {
                                                id: "lLoggingLabel",
                                                value: "Логгирование"
                                            },
                                        ]
                                    },
                                    {
                                        id: "chekcMark",
                                        value: "Проверка марки"
                                    }
                                ],
                            on:
                            {
                                onAfterSelect: function (id) {
                                    let elem = $$(id);

                                    if (elem == undefined)
                                        return;
                                    
                                    $$(bodyId).showView(id);

                                },
                                onAfterOpen: function(id) {
                                    switch (id) {
                                        case "config":
                                            webix.ui(SettingsView(bodyId), $$(bodyId));
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        },
                        {
                            id: bodyId
                        }
                    ]
            }

        ]
    });

    $$('sidebar').open("config");
});

webix.event(window, "resize", function(e) {
    $$("root").resize();
})

webix.protoUI({
    name: "subform",
    defaults: {
        borderless: true,
        paddingX: 0,
        paddingY: 0
    },
    getValue: function () {
        return this.getValues();
    },
    setValue: function (values) {
        this.setValues(values);
    },
}, webix.ui.form);

webix.protoUI({
	name:"formtable",
	setValue:function(value){
        this.clearAll(); 
        this.parse(value)
    },
	getValue:function() {
         return this.serialize();
    }
}, webix.ui.datatable);

webix.protoUI({
	name:"formlists",
	setValue:function(value){
        this.clearAll(); 
        this.parse(value)
    },
	getValue:function() {
         return this.serialize();
    }
}, webix.ui.list);