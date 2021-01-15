using System;
using System.ComponentModel.DataAnnotations;

namespace Datalist.Tests.Objects
{
    public class ObjectModel
    {
        [Key]
        public Object? Id { get; set; }
    }
}
