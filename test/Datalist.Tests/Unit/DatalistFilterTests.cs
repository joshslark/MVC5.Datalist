using Xunit;

namespace Datalist.Tests.Unit
{
    public class DatalistFilterTests
    {
        [Fact]
        public void DatalistFilter_CreatesEmpty()
        {
            DatalistFilter filter = new DatalistFilter();

            Assert.Empty(filter.AdditionalFilters);
            Assert.Empty(filter.Selected);
            Assert.Empty(filter.CheckIds);
            Assert.Empty(filter.Ids);
        }
    }
}
