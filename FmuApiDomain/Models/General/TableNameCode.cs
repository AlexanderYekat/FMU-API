﻿using System.ComponentModel.DataAnnotations.Schema;

namespace FmuApiDomain.Models.General
{
    public class TableNameCode : TableWIthId
    {
        [Column("CODE")]
        public int Code { get; set; }
        [Column("NAME")]
        public string? Name { get; set; }
    }
}
