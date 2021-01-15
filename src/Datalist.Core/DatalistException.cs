using System;

namespace Datalist
{
    public class DatalistException : Exception
    {
        public DatalistException(String message)
            : base(message)
        {
        }
    }
}
