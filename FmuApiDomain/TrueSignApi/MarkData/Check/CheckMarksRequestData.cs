namespace FmuApiDomain.TrueSignApi.MarkData.Check
{
    public class CheckMarksRequestData
    {
        public List<string> Codes { get; set; } = [];
        //изменения отправки номера ФН в запросе //
        public string FiscalDriveNumber { get; set; } = string.Empty;
        //изменения отправки номера ФН в запросе \\
        public CheckMarksRequestData() { }

        public CheckMarksRequestData(string mark)
        {
            Codes.Add(mark.Replace("\\u001d", "\u001d"));
        }

        public CheckMarksRequestData(List<string> marks)
        {
            foreach (string mark in marks)
            {
                Codes.Add(mark.Replace("\\u001d", "\u001d"));
            }
        }

        public void LoadMarks(List<string> marks)
        {
            Codes.Clear();

            foreach (var mark in marks)
            {
                Codes.Add(mark);
            }
        }
    }
}
