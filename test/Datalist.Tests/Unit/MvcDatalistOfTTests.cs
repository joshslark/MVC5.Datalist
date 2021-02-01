using Datalist.Tests.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Datalist.Tests.Unit
{
    public class MvcDatalistOfTTests
    {
        private TestDatalist<TestModel> datalist;

        public MvcDatalistOfTTests()
        {
            datalist = new TestDatalist<TestModel>();
            datalist.Filter.Rows = 20;

            for (Int32 i = 0; i < 200; i++)
            {
                datalist.Models.Add(new TestModel
                {
                    Id = i + "I",
                    Count = i + 10,
                    Value = i + "V",
                    ParentId = "1000",
                    Date = new DateTime(2014, 12, 10).AddDays(i)
                });
            }
            datalist.Models.Add(new TestModel
            {
                Id = "ProductGroup",
                Count = 0,
                Value = "UNITYSALES__Product_Group__c",
                ParentId = "1000",
                Date = DateTime.Today
            });
        }

        [Fact]
        public void GetId_NoProperty()
        {
            Assert.Empty(new TestDatalist<NoIdModel>().GetId(new NoIdModel()));
        }

        [Fact]
        public void GetId_Value()
        {
            String actual = datalist.GetId(new TestModel { Id = "Test" });
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetLabel_NoColumns()
        {
            datalist.Columns.Clear();

            Assert.Empty(datalist.GetLabel(new TestModel()));
        }

        [Fact]
        public void GetLabel_Value()
        {
            String actual = datalist.GetLabel(new TestModel { Value = "Test" });
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AttributedProperties_GetsOrderedProperties()
        {
            IEnumerable<PropertyInfo> actual = datalist.AttributedProperties;
            IEnumerable<PropertyInfo> expected = typeof(TestModel).GetProperties()
                .Where(property => property.GetCustomAttribute<DatalistColumnAttribute>(false) != null)
                .OrderBy(property => property.GetCustomAttribute<DatalistColumnAttribute>(false)!.Position);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MvcDatalist_AddsColumns()
        {
            List<DatalistColumn> columns = new List<DatalistColumn>
            {
                new DatalistColumn("Value", "") { Hidden = false, Filterable = true },
                new DatalistColumn("Date", "Date") { Hidden = false, Filterable = true },
                new DatalistColumn("Count", "Value") { Hidden = false, Filterable = false }
            };

            using IEnumerator<DatalistColumn> expected = columns.GetEnumerator();
            using IEnumerator<DatalistColumn> actual = datalist.Columns.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(expected.Current.Key, actual.Current.Key);
                Assert.Equal(expected.Current.Header, actual.Current.Header);
                Assert.Equal(expected.Current.Hidden, actual.Current.Hidden);
                Assert.Equal(expected.Current.CssClass, actual.Current.CssClass);
                Assert.Equal(expected.Current.Filterable, actual.Current.Filterable);
            }
        }

        [Fact]
        public void GetColumnKey_NullProperty_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => datalist.GetColumnKey(null!));

            Assert.Equal("property", actual.ParamName);
        }

        [Fact]
        public void GetColumnKey_ReturnsPropertyName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty(nameof(TestModel.Count))!;

            String actual = datalist.GetColumnKey(property);
            String expected = property.Name;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnHeader_NullProperty_ReturnsEmpty()
        {
            Assert.Empty(datalist.GetColumnHeader(null!));
        }

        [Fact]
        public void GetColumnHeader_NoDisplayName_ReturnsEmpty()
        {
            PropertyInfo property = typeof(TestModel).GetProperty(nameof(TestModel.Value))!;

            Assert.Empty(datalist.GetColumnHeader(property));
        }

        [Fact]
        public void GetColumnHeader_ReturnsDisplayName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty(nameof(TestModel.Date))!;

            String? expected = property.GetCustomAttribute<DisplayAttribute>()?.Name;
            String? actual = datalist.GetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnHeader_ReturnsDisplayShortName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty(nameof(TestModel.Count))!;

            String? expected = property.GetCustomAttribute<DisplayAttribute>()?.ShortName;
            String? actual = datalist.GetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnCssClass_ReturnsEmpty()
        {
            Assert.Empty(datalist.GetColumnCssClass(null!));
        }

        [Fact]
        public void GetData_FiltersByIds()
        {
            datalist.Filter.Ids.Add("9I");
            datalist.Filter.Ids.Add("15I");
            datalist.Filter.Sort = "Count";
            datalist.Filter.Search = "Term";
            datalist.Filter.Selected.Add("17I");
            datalist.Filter.AdditionalFilters.Add("Value", "5V");

            DatalistData actual = datalist.GetData();

            Assert.Equal(new DateTime(2014, 12, 19).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("9V", actual.Rows[0]["Label"]);
            Assert.Equal("9V", actual.Rows[0]["Value"]);
            Assert.Equal("19", actual.Rows[0]["Count"]);
            Assert.Equal("9I", actual.Rows[0]["Id"]);

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Rows[1]["Date"]);
            Assert.Equal("15V", actual.Rows[1]["Label"]);
            Assert.Equal("15V", actual.Rows[1]["Value"]);
            Assert.Equal("25", actual.Rows[1]["Count"]);
            Assert.Equal("15I", actual.Rows[1]["Id"]);

            Assert.Equal(datalist.Columns, actual.Columns);
            Assert.Equal(2, actual.Rows.Count);
            Assert.Empty(actual.Selected);
        }

        [Fact]
        public void GetData_FiltersByCheckIds()
        {
            datalist.Filter.Sort = "Count";
            datalist.Filter.CheckIds.Add("9I");
            datalist.Filter.CheckIds.Add("15I");
            datalist.Filter.AdditionalFilters.Add("ParentId", "1000");

            DatalistData actual = datalist.GetData();

            Assert.Equal(new DateTime(2014, 12, 19).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("9V", actual.Rows[0]["Label"]);
            Assert.Equal("9V", actual.Rows[0]["Value"]);
            Assert.Equal("19", actual.Rows[0]["Count"]);
            Assert.Equal("9I", actual.Rows[0]["Id"]);

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Rows[1]["Date"]);
            Assert.Equal("15V", actual.Rows[1]["Label"]);
            Assert.Equal("15V", actual.Rows[1]["Value"]);
            Assert.Equal("25", actual.Rows[1]["Count"]);
            Assert.Equal("15I", actual.Rows[1]["Id"]);

            Assert.Equal(datalist.Columns, actual.Columns);
            Assert.Equal(2, actual.Rows.Count);
            Assert.Empty(actual.Selected);
        }

        [Fact]
        public void GetData_FiltersByNotSelected()
        {
            datalist.Filter.Sort = "Count";
            datalist.Filter.Search = "133V";
            datalist.Filter.Selected.Add("15I");
            datalist.Filter.Selected.Add("125I");

            datalist.GetData();

            DatalistData actual = datalist.GetData();

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Selected[0]["Date"]);
            Assert.Equal("15V", actual.Selected[0]["Label"]);
            Assert.Equal("15V", actual.Selected[0]["Value"]);
            Assert.Equal("25", actual.Selected[0]["Count"]);
            Assert.Equal("15I", actual.Selected[0]["Id"]);

            Assert.Equal(new DateTime(2015, 4, 14).ToString("d"), actual.Selected[1]["Date"]);
            Assert.Equal("125V", actual.Selected[1]["Label"]);
            Assert.Equal("125V", actual.Selected[1]["Value"]);
            Assert.Equal("135", actual.Selected[1]["Count"]);
            Assert.Equal("125I", actual.Selected[1]["Id"]);

            Assert.Equal(new DateTime(2015, 4, 22).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("133V", actual.Rows[0]["Label"]);
            Assert.Equal("133V", actual.Rows[0]["Value"]);
            Assert.Equal("143", actual.Rows[0]["Count"]);
            Assert.Equal("133I", actual.Rows[0]["Id"]);

            Assert.Equal(datalist.Columns, actual.Columns);
            Assert.Equal(2, actual.Selected.Count);
        }

        [Fact]
        public void GetData_FiltersByAdditionalFilters()
        {
            datalist.Filter.Search = "6V";
            datalist.Filter.AdditionalFilters.Add("Count", 16);

            datalist.GetData();

            DatalistData actual = datalist.GetData();

            Assert.Equal(new DateTime(2014, 12, 16).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("6V", actual.Rows[0]["Label"]);
            Assert.Equal("6V", actual.Rows[0]["Value"]);
            Assert.Equal("16", actual.Rows[0]["Count"]);
            Assert.Equal("6I", actual.Rows[0]["Id"]);

            Assert.Equal(datalist.Columns, actual.Columns);
            Assert.Empty(actual.Selected);
            Assert.Single(actual.Rows);
        }

        [Fact]
        public void GetData_FiltersBySearch()
        {
            datalist.Filter.Search = "33V";
            datalist.Filter.Sort = "Count";

            datalist.GetData();

            DatalistData actual = datalist.GetData();

            Assert.Equal(new DateTime(2015, 1, 12).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("33V", actual.Rows[0]["Label"]);
            Assert.Equal("33V", actual.Rows[0]["Value"]);
            Assert.Equal("43", actual.Rows[0]["Count"]);
            Assert.Equal("33I", actual.Rows[0]["Id"]);

            Assert.Equal(datalist.Columns, actual.Columns);
            Assert.Equal(15, actual.Rows.Count);
            Assert.Empty(actual.Selected);
        }

        [Fact]
        public void GetData_SearchFindsProductGroup()
        {
            datalist.Filter.Search = "ProductGroup";
            datalist.Filter.Sort = "Count";

            datalist.GetData();

            DatalistData actual = datalist.GetData();

            Assert.Equal("Product_Group__c", actual.Rows[0]["Label"]);

            Assert.Equal(datalist.Columns, actual.Columns);
            Assert.InRange(actual.Rows.Count, 1, 15);
            Assert.Empty(actual.Selected);
        }

        [Fact]
        public void GetData_Sorts()
        {
            datalist.Filter.Order = DatalistSortOrder.Asc;
            datalist.Filter.Search = "55V";
            datalist.Filter.Sort = "Count";

            datalist.GetData();

            DatalistData actual = datalist.GetData();

            Assert.Equal(new DateTime(2015, 2, 3).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("55V", actual.Rows[0]["Label"]);
            Assert.Equal("55V", actual.Rows[0]["Value"]);
            Assert.Equal("65", actual.Rows[0]["Count"]);
            Assert.Equal("55I", actual.Rows[0]["Id"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void FilterBySearch_SkipsEmptySearch(String search)
        {
            datalist.Filter.Search = search;

            IQueryable<TestModel> actual = datalist.FilterBySearch(datalist.GetModels());
            IQueryable<TestModel> expected = datalist.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNotExistingProperties()
        {
            datalist.Columns.Clear();
            datalist.Filter.Search = "1";
            datalist.Columns.Add(new DatalistColumn("Test", "Test"));

            IQueryable<TestModel> actual = datalist.FilterBySearch(datalist.GetModels());
            IQueryable<TestModel> expected = datalist.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_UsesContainsSearch()
        {
            datalist.Filter.Search = "1";

            IQueryable<TestModel> expected = datalist.GetModels().Where(model => model.Id!.Contains("1")).Take(5);
            IQueryable<TestModel> actual = datalist.FilterBySearch(datalist.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNonStringProperties()
        {
            datalist.Columns.Clear();
            datalist.Filter.Search = "1";
            datalist.Columns.Add(new DatalistColumn("Count", ""));

            IQueryable<TestModel> actual = datalist.FilterBySearch(datalist.GetModels());
            IQueryable<TestModel> expected = datalist.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNotFilterableColumns()
        {
            datalist.Columns.Clear();
            datalist.Filter.Search = "1";
            datalist.Columns.Add(new DatalistColumn("Value", "") { Filterable = false });

            IQueryable<TestModel> actual = datalist.FilterBySearch(datalist.GetModels());
            IQueryable<TestModel> expected = datalist.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_SkipsNullValues()
        {
            datalist.Filter.AdditionalFilters.Add("Id", null);

            IQueryable<TestModel> actual = datalist.FilterByAdditionalFilters(datalist.GetModels());
            IQueryable<TestModel> expected = datalist.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_Filters()
        {
            datalist.Filter.AdditionalFilters.Add("Id", "9I");
            datalist.Filter.AdditionalFilters.Add("Count", new[] { 19, 30 });
            datalist.Filter.AdditionalFilters.Add("Date", new DateTime(2014, 12, 19));

            IQueryable<TestModel> actual = datalist.FilterByAdditionalFilters(datalist.GetModels());
            IQueryable<TestModel> expected = datalist.GetModels().Where(model =>
                model.Id == "9I" && model.Count == 19 && model.Date == new DateTime(2014, 12, 19));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NoIdProperty_Throws()
        {
            TestDatalist<NoIdModel> testDatalist = new TestDatalist<NoIdModel>();

            DatalistException exception = Assert.Throws<DatalistException>(() => testDatalist.FilterByIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = datalist.FilterByIds(datalist.GetModels(), ids);
            IQueryable<TestModel> expected = datalist.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_Guids()
        {
            TestDatalist<GuidModel> testDatalist = new TestDatalist<GuidModel>();
            for (Int32 i = 0; i < 20; i++)
                testDatalist.Models.Add(new GuidModel { Id = Guid.NewGuid() });
            List<String> ids = new List<String> { testDatalist.Models[4].Id.ToString(), testDatalist.Models[9].Id.ToString() };

            IQueryable<GuidModel> expected = testDatalist.GetModels().Where(model => ids.Contains(model.Id.ToString()));
            IQueryable<GuidModel> actual = testDatalist.FilterByIds(testDatalist.GetModels(), ids);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NumberKey()
        {
            TestDatalist<Int32Model> testDatalist = new TestDatalist<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testDatalist.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testDatalist.FilterByIds(testDatalist.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testDatalist.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NotSupportedIdType_Throws()
        {
            DatalistException exception = Assert.Throws<DatalistException>(() => new TestDatalist<ObjectModel>().FilterByIds(Array.Empty<ObjectModel>().AsQueryable(), new String[0]));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type has to be a string, guid or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NoIdProperty_Throws()
        {
            TestDatalist<NoIdModel> testDatalist = new TestDatalist<NoIdModel>();

            DatalistException exception = Assert.Throws<DatalistException>(() => testDatalist.FilterByNotIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = datalist.FilterByNotIds(datalist.GetModels(), ids);
            IQueryable<TestModel> expected = datalist.GetModels().Where(model => !ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_Guids()
        {
            TestDatalist<GuidModel> testDatalist = new TestDatalist<GuidModel>();
            for (Int32 i = 0; i < 20; i++)
                testDatalist.Models.Add(new GuidModel { Id = Guid.NewGuid() });
            List<String> ids = new List<String> { testDatalist.Models[4].Id.ToString(), testDatalist.Models[9].Id.ToString() };

            IQueryable<GuidModel> expected = testDatalist.GetModels().Where(model => !ids.Contains(model.Id.ToString()));
            IQueryable<GuidModel> actual = testDatalist.FilterByNotIds(testDatalist.GetModels(), ids);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NumberKey()
        {
            TestDatalist<Int32Model> testDatalist = new TestDatalist<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testDatalist.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testDatalist.FilterByNotIds(testDatalist.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testDatalist.GetModels().Where(model => !new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NotSupportedIdType_Throws()
        {
            DatalistException exception = Assert.Throws<DatalistException>(() => new TestDatalist<ObjectModel>().FilterByNotIds(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type has to be a string, guid or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NoIdProperty_Throws()
        {
            TestDatalist<NoIdModel> testDatalist = new TestDatalist<NoIdModel>();

            DatalistException exception = Assert.Throws<DatalistException>(() => testDatalist.FilterByCheckIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = datalist.FilterByCheckIds(datalist.GetModels(), ids);
            IQueryable<TestModel> expected = datalist.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NumberKey()
        {
            TestDatalist<Int32Model> testDatalist = new TestDatalist<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testDatalist.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testDatalist.FilterByCheckIds(testDatalist.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testDatalist.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NotSupportedIdType_Throws()
        {
            DatalistException exception = Assert.Throws<DatalistException>(() => new TestDatalist<ObjectModel>().FilterByCheckIds(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type has to be a string, guid or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NoIdProperty_Throws()
        {
            TestDatalist<NoIdModel> testDatalist = new TestDatalist<NoIdModel>();

            DatalistException exception = Assert.Throws<DatalistException>(() => testDatalist.FilterBySelected(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = datalist.FilterBySelected(datalist.GetModels(), ids);
            IQueryable<TestModel> expected = datalist.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NumberKey()
        {
            TestDatalist<Int32Model> testDatalist = new TestDatalist<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testDatalist.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testDatalist.FilterBySelected(testDatalist.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testDatalist.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NotSupportedIdType_Throws()
        {
            DatalistException exception = Assert.Throws<DatalistException>(() => new TestDatalist<ObjectModel>().FilterBySelected(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type has to be a string, guid or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_ByColumn()
        {
            datalist.Filter.Sort = "Count";

            IQueryable<TestModel> expected = datalist.GetModels().OrderBy(model => model.Count);
            IQueryable<TestModel> actual = datalist.Sort(datalist.GetModels());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Sort_NoSortColumns(String column)
        {
            datalist.Columns.Clear();
            datalist.Filter.Sort = column;

            IQueryable<TestModel> expected = datalist.GetModels();
            IQueryable<TestModel> actual = datalist.Sort(datalist.GetModels());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Sort_NoSortOrderedColumns(String column)
        {
            datalist.Columns.Clear();
            datalist.Filter.Sort = column;

            IQueryable<TestModel> actual = datalist.Sort(datalist.GetModels().OrderByDescending(model => model.Id).Where(model => true));
            IQueryable<TestModel> expected = datalist.GetModels().OrderByDescending(model => model.Id);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(-1, -1, 0, 1)]
        [InlineData(-1, 0, 0, 1)]
        [InlineData(-1, 1, 0, 1)]
        [InlineData(-1, 100, 0, 100)]
        [InlineData(-1, 101, 0, 100)]
        [InlineData(0, -1, 0, 1)]
        [InlineData(0, 0, 0, 1)]
        [InlineData(0, 1, 0, 1)]
        [InlineData(0, 100, 0, 100)]
        [InlineData(0, 101, 0, 100)]
        [InlineData(50, -1, 50, 1)]
        [InlineData(50, 0, 50, 1)]
        [InlineData(50, 1, 50, 1)]
        [InlineData(50, 100, 50, 100)]
        [InlineData(50, 101, 50, 100)]
        public void Page_Rows(Int32 offset, Int32 rows, Int32 expectedOffset, Int32 expectedRows)
        {
            datalist.Filter.Rows = rows;
            datalist.Filter.Offset = offset;

            IQueryable<TestModel> expected = datalist.GetModels().Skip(expectedOffset).Take(expectedRows);
            IQueryable<TestModel> actual = datalist.Page(datalist.GetModels());

            Assert.Equal(expectedOffset, datalist.Filter.Offset);
            Assert.Equal(expectedRows, datalist.Filter.Rows);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormDatalistData_Columns()
        {
            Object actual = datalist.FormDatalistData(datalist.GetModels(), new[] { new TestModel() }.AsQueryable(), datalist.GetModels()).Columns;
            Object expected = datalist.Columns;

            Assert.Same(expected, actual);
        }

        [Fact]
        public void FormDatalistData_Selected()
        {
            IQueryable<TestModel> notSelected = new TestModel[0].AsQueryable();
            IQueryable<TestModel> selected = datalist.GetModels().Skip(6).Take(3);

            using IEnumerator<Dictionary<String, String>> actual = datalist.FormDatalistData(datalist.GetModels(), selected, notSelected).Selected.GetEnumerator();
            using IEnumerator<Dictionary<String, String>> expected = new List<Dictionary<String, String>>
            {
                new Dictionary<String, String>
                {
                    ["Id"] = "6I",
                    ["Label"] = "6V",
                    ["Date"] = new DateTime(2014, 12, 16).ToString("d"),
                    ["Count"] = "16",
                    ["Value"] = "6V"
                },
                new Dictionary<String, String>
                {
                    ["Id"] = "7I",
                    ["Label"] = "7V",
                    ["Date"] = new DateTime(2014, 12, 17).ToString("d"),
                    ["Count"] = "17",
                    ["Value"] = "7V"
                },
                new Dictionary<String, String>
                {
                    ["Id"] = "8I",
                    ["Label"] = "8V",
                    ["Date"] = new DateTime(2014, 12, 18).ToString("d"),
                    ["Count"] = "18",
                    ["Value"] = "8V"
                }
            }.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
                Assert.Equal(expected.Current, actual.Current);
        }

        [Fact]
        public void FormDatalistData_Rows()
        {
            IQueryable<TestModel> selected = new TestModel[0].AsQueryable();
            IQueryable<TestModel> notSelected = datalist.GetModels().Skip(6).Take(3);

            using IEnumerator<Dictionary<String, String>> actual = datalist.FormDatalistData(datalist.GetModels(), selected, notSelected).Rows.GetEnumerator();
            using IEnumerator<Dictionary<String, String>> expected = new List<Dictionary<String, String>>
            {
                new Dictionary<String, String>
                {
                    ["Id"] = "6I",
                    ["Label"] = "6V",
                    ["Date"] = new DateTime(2014, 12, 16).ToString("d"),
                    ["Count"] = "16",
                    ["Value"] = "6V"
                },
                new Dictionary<String, String>
                {
                    ["Id"] = "7I",
                    ["Label"] = "7V",
                    ["Date"] = new DateTime(2014, 12, 17).ToString("d"),
                    ["Count"] = "17",
                    ["Value"] = "7V"
                },
                new Dictionary<String, String>
                {
                    ["Id"] = "8I",
                    ["Label"] = "8V",
                    ["Date"] = new DateTime(2014, 12, 18).ToString("d"),
                    ["Count"] = "18",
                    ["Value"] = "8V"
                }
            }.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
                Assert.Equal(expected.Current, actual.Current);
        }

        [Fact]
        public void FormData_EmptyValues()
        {
            datalist.Columns.Clear();
            datalist.Columns.Add(new DatalistColumn("Test", ""));

            Dictionary<String, String?> row = datalist.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", "Label", "Id" }, row.Keys);
            Assert.Equal(new[] { null, "", "1" }, row.Values);
        }

        [Fact]
        public void FormData_Values()
        {
            Dictionary<String, String?> row = datalist.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", DateTime.Now.Date.ToString("d"), "4", "Test", "1" }, row.Values);
            Assert.Equal(new[] { "Value", "Date", "Count", "Label", "Id" }, row.Keys);
        }

        [Fact]
        public void FormData_OverridenValues()
        {
            datalist.GetId = (model) => $"Test {model.Id}";
            datalist.GetLabel = (model) => $"Test label {model.Id}";
            Dictionary<String, String?> row = datalist.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", DateTime.Now.Date.ToString("d"), "4", "Test label 1", "Test 1" }, row.Values);
            Assert.Equal(new[] { "Value", "Date", "Count", "Label", "Id" }, row.Keys);
        }
    }
}
