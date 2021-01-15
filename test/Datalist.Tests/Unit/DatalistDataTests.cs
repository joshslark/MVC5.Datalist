using Xunit;

namespace Datalist.Tests.Unit
{
    public class DatalistDataTests
    {
        [Fact]
        public void DatalistData_CreatesEmpty()
        {
            DatalistData actual = new DatalistData();

            Assert.Empty(actual.Selected);
            Assert.Empty(actual.Columns);
            Assert.Empty(actual.Rows);
        }
    }
}
