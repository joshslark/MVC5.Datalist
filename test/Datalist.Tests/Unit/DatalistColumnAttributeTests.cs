using Xunit;

namespace Datalist.Tests.Unit
{
    public class DatalistColumnAttributeTests
    {
        [Fact]
        public void DatalistColumnAttribute()
        {
            DatalistColumnAttribute attribute = new DatalistColumnAttribute();

            Assert.True(attribute.Filterable);
        }

        [Fact]
        public void DatalistColumnAttribute_Position()
        {
            DatalistColumnAttribute attribute = new DatalistColumnAttribute(-5);

            Assert.Equal(-5, attribute.Position);
            Assert.True(attribute.Filterable);
        }
    }
}
