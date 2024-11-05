﻿namespace FmuApiDomain.Configuration.Options.Organisation
{
    public class OrganisationConfigurution
    {
        public List<PrintGroupData> PrintGroups { get; set; } = [];

        public void FillIfEMpty()
        {
            if (PrintGroups.Count > 0)
                return;

            PrintGroups.Add(new PrintGroupData { Id = 1 });
        }

        public int GroupIDByINN(string inn)
        {
            if (PrintGroups.Count == 0)
                return 0;

            PrintGroupData? row = PrintGroups.FirstOrDefault(x => x.INN == inn);

            if (row == null)
                //return PrintGroups[0].XAPIKEY;
                return -1;

            return row.Id;
        }

        public string XapiKey()
        {
            if (PrintGroups.Count == 0)
                return "";

            return PrintGroups[0].XAPIKEY;
        }

        public string XapiKey(int id)
        {
            if (PrintGroups.Count == 0)
                return "";

            //изменения для поиска API-KEY по ИНН //
            if (id == -1) {
                return "";
            }
            //изменения для поиска API-KEY по ИНН \\

            PrintGroupData? row = PrintGroups.FirstOrDefault(x => x.Id == id);

            if (row == null)
                return PrintGroups[0].XAPIKEY;

            return row.XAPIKEY;
        }

        public void SetXapiKey(string xapikey)
        {
            SetXapiKey(xapikey, 1, "");
        }

        public void SetXapiKey(string xapikey, int id)
        {
            SetXapiKey(xapikey, id, "");
        }

        public void SetXapiKey(string xapikey, int id, string inn)
        {
            PrintGroupData? row = PrintGroups.FirstOrDefault(x => x.Id == id);

            if (row == null)
                row = new PrintGroupData { Id = id, XAPIKEY = xapikey, INN = inn };
            else
            {
                row.XAPIKEY = xapikey;
                row.INN = inn;
            }
        }
        public void DeleteXapiKay(int id)
        {
            PrintGroupData? row = PrintGroups.FirstOrDefault(x => x.Id == id);

            if (row == null)
                return;

            PrintGroups.Remove(row);
        }

        public void DeleteXapiKay(string xapikey)
        {
            PrintGroupData? row = PrintGroups.FirstOrDefault(x => x.XAPIKEY == xapikey);

            if (row == null)
                return;

            PrintGroups.Remove(row);
        }
    }
}
