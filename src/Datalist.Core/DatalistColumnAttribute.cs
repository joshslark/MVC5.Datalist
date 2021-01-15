using System;

namespace Datalist
{
    public class DatalistColumnAttribute : Attribute
    {
        public Boolean Filterable { get; set; }
        public Int32 Position { get; set; }
        public Boolean Hidden { get; set; }
        public String? Format { get; set; }

        public DatalistColumnAttribute()
        {
            Filterable = true;
        }
        public DatalistColumnAttribute(Int32 position)
        {
            Filterable = true;
            Position = position;
        }
    }
}
