﻿namespace FmuApiDomain.Models.TrueSignApi.MarkData.Check
{
    public class CheckMarksRequestData
    {
        public List<string> Codes { get; set; } = [];

        public CheckMarksRequestData(List<string> marks)
        {
            foreach (string mark in marks)
            {
                Codes.Add(mark.Replace("\\u001d", "\u001d"));
            }
        }

        public CheckMarksRequestData() { }

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
