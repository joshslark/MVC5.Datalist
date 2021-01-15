using System;
using System.Collections.Generic;

namespace Datalist
{
    public class DatalistData
    {
        public IList<DatalistColumn> Columns { get; set; }
        public List<Dictionary<String, String?>> Rows { get; set; }
        public List<Dictionary<String, String?>> Selected { get; set; }

        public DatalistData()
        {
            Columns = new List<DatalistColumn>();
            Rows = new List<Dictionary<String, String?>>();
            Selected = new List<Dictionary<String, String?>>();
        }
    }
}
