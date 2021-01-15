using Xunit;

namespace Datalist.Tests.Unit
{
    public class DatalistExceptionTests
    {
        [Fact]
        public void DatalistException_Message()
        {
            Assert.Equal("Test", new DatalistException("Test").Message);
        }
    }
}
