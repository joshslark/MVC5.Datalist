using System;
using System.Collections.Generic;

namespace Datalist
{
    public abstract class MvcDatalist
    {
        public String? Url { get; set; }
        public String? Name { get; set; }
        public String? Title { get; set; }
        public String? Dialog { get; set; }
        public Boolean Multi { get; set; }
        public Boolean ReadOnly { get; set; }
        public String? Placeholder { get; set; }

        public DatalistFilter Filter { get; set; }
        public IList<DatalistColumn> Columns { get; set; }
        public IList<String> AdditionalFilters { get; set; }

        protected MvcDatalist()
        {
            AdditionalFilters = new List<String>();
            Columns = new List<DatalistColumn>();
            Filter = new DatalistFilter();
        }

        public abstract DatalistData GetData();
    }
}
