using System;
using System.ComponentModel.DataAnnotations;

namespace Datalist.Tests.Objects
{
    public class Int32Model
    {
        [Key]
        [DatalistColumn]
        public Int32 Value { get; set; }
    }
}
