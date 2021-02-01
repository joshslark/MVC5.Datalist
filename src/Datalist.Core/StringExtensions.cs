using System;
using System.Linq;

namespace Datalist.Core
{
    public static class StringExtensions
    {        
        public static double RatcliffObershelpSimilarity(this string source, string target)
        {
            return (2 * Convert.ToDouble(source.Where(ch => target.Contains(ch)).Count())) / (Convert.ToDouble(source.Length + target.Length));
        }
    }
}
