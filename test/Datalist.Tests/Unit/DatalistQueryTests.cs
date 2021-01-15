using System;
using System.Linq;
using Xunit;

namespace Datalist.Tests.Unit
{
    public class DatalistQueryTests
    {
        [Fact]
        public void IsOrdered_False()
        {
            Assert.False(DatalistQuery.IsOrdered(new Object[0].OrderBy(model => 0).AsQueryable()));
        }

        [Fact]
        public void IsOrdered_True()
        {
            Assert.True(DatalistQuery.IsOrdered(new Object[0].AsQueryable().OrderBy(model => 0)));
        }
    }
}
