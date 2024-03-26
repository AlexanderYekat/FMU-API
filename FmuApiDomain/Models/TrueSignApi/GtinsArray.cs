﻿namespace FmuApiDomain.Models.TrueSignApi
{
    public class GtinsArray
    {
        public List<string> Gtins { get; private set; } = new();

        public GtinsArray()
        {

        }

        public GtinsArray(List<string> gtins) 
        {
            Gtins = gtins;
        }
    }
}
