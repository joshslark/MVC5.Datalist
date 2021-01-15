using System;

namespace Datalist.Tests.Objects
{
    public class NoIdModel
    {
        [DatalistColumn]
        public String? Title { get; set; }
    }
}
