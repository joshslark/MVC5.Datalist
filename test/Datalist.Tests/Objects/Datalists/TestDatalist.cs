using System.Collections.Generic;
using System.Linq;

namespace Datalist.Tests.Objects
{
    public class TestDatalist<T> : MvcDatalist<T> where T : class
    {
        public IList<T> Models { get; }

        public TestDatalist()
        {
            Models = new List<T>();
        }

        public override IQueryable<T> GetModels()
        {
            return Models.AsQueryable();
        }
    }
}
