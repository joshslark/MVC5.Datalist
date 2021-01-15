using System;
using System.ComponentModel.DataAnnotations;

namespace Datalist.Tests.Objects
{
    public class Int64Model
    {
        [Key]
        [DatalistColumn]
        public Int64 Value { get; set; }
    }
}
