using Datalist.Tests.Objects;
using NSubstitute;
using System;
using System.Collections;
using System.Globalization;
using System.Web.Mvc;
using Xunit;

namespace Datalist.Tests.Unit
{
    public class DatalistExtensionsTests
    {
        private TestDatalist<TestModel> datalist;
        private HtmlHelper<TestModel> html;

        public DatalistExtensionsTests()
        {
            html = MockHtmlHelper();
            datalist = new TestDatalist<TestModel>();

            datalist.Filter.Rows = 11;
            datalist.Dialog = "Dialog";
            datalist.Filter.Offset = 22;
            datalist.Name = "DatalistName";
            datalist.Filter.Sort = "First";
            datalist.Placeholder = "Search";
            datalist.Filter.Search = "Test";
            datalist.AdditionalFilters.Clear();
            datalist.AdditionalFilters.Add("Add1");
            datalist.AdditionalFilters.Add("Add2");
            datalist.Title = "Dialog datalist title";
            datalist.Url = "http://localhost/Datalist";
            datalist.Filter.Order = DatalistSortOrder.Desc;
        }

        [Fact]
        public void AutoComplete_Readonly()
        {
            datalist.ReadOnly = true;

            String actual = html.AutoComplete("Test", datalist, "Value", new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist-browseless datalist-readonly datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"false\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"true\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"Value\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" readonly=\"readonly\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoComplete_FromDatalist()
        {
            String actual = html.AutoComplete("Test", datalist, "Value", new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist-browseless datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"false\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"false\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"Value\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoComplete_FromMultiDatalist()
        {
            datalist.Multi = true;
            (html as HtmlHelper).ViewData.ModelState.Add("Values", new ModelState { Value = new ValueProviderResult("1", "1", CultureInfo.CurrentCulture) });

            String actual = html.AutoComplete("Test", datalist, new[] { "Value1", "Value2" }, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist-browseless datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"true\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"false\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" name=\"Test\" type=\"hidden\" value=\"Value1\" />" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" name=\"Test\" type=\"hidden\" value=\"Value2\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoCompleteFor_Readonly()
        {
            datalist.ReadOnly = true;

            String actual = html.AutoCompleteFor(model => model.ParentId, datalist, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist-browseless datalist-readonly datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"ParentId\" data-multi=\"false\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"true\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#39;s parent ID\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" readonly=\"readonly\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoCompleteFor_FromDatalist()
        {
            String actual = html.AutoCompleteFor(model => model.ParentId, datalist, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist-browseless datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"ParentId\" data-multi=\"false\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"false\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#39;s parent ID\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoCompleteFor_FromMultiDatalist()
        {
            datalist.Multi = true;
            html.ViewData.Model.Values = new[] { "Value1's", "Value2's" };
            (html as HtmlHelper).ViewData.ModelState.Add("Values", new ModelState { Value = new ValueProviderResult("1", "1", CultureInfo.CurrentCulture) });

            String actual = html.AutoCompleteFor(model => model.Values, datalist, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist-browseless datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Values\" data-multi=\"true\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"false\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"Values\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" name=\"Values\" type=\"hidden\" value=\"Value1&#39;s\" />" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" name=\"Values\" type=\"hidden\" value=\"Value2&#39;s\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"Values\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Datalist_Readonly()
        {
            datalist.ReadOnly = true;

            String actual = html.Datalist("Test", datalist, 1, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist-readonly datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"false\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"true\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"1\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" readonly=\"readonly\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"datalist-browser\" data-for=\"Test\" type=\"button\">" +
                        "<span class=\"datalist-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Datalist_FromDatalist()
        {
            String actual = html.Datalist("Test", datalist, 1, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"false\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"false\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"1\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"datalist-browser\" data-for=\"Test\" type=\"button\">" +
                        "<span class=\"datalist-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Datalist_FromMultiDatalist()
        {
            datalist.Multi = true;

            String actual = html.Datalist("Test", datalist, 1, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"true\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"false\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"Test\">" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"datalist-browser\" data-for=\"Test\" type=\"button\">" +
                        "<span class=\"datalist-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DatalistFor_Readonly()
        {
            datalist.ReadOnly = true;

            String actual = html.DatalistFor(model => model.ParentId, datalist, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist-readonly datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"ParentId\" data-multi=\"false\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"true\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#39;s parent ID\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" readonly=\"readonly\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"datalist-browser\" data-for=\"ParentId\" type=\"button\">" +
                        "<span class=\"datalist-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DatalistFor_FromDatalist()
        {
            String actual = html.DatalistFor(model => model.ParentId, datalist, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"ParentId\" data-multi=\"false\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"false\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"datalist-value\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#39;s parent ID\" />" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"datalist-browser\" data-for=\"ParentId\" type=\"button\">" +
                        "<span class=\"datalist-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DatalistFor_FromMultiDatalist()
        {
            datalist.Multi = true;

            String actual = html.DatalistFor(model => model.Values, datalist, new { @class = "classes", attribute = "attr" }).ToString();
            String expected =
                "<div attribute=\"attr\" class=\"datalist classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Values\" data-multi=\"true\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"false\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog datalist title\" data-url=\"http://localhost/Datalist\">" +
                    "<div class=\"datalist-values\" data-for=\"Values\">" +
                    "</div>" +
                    "<div class=\"datalist-control\" data-for=\"Values\">" +
                        "<input autocomplete=\"off\" class=\"datalist-input\" id=\"DatalistName\" name=\"DatalistName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"datalist-control-loader\"></div>" +
                        "<div class=\"datalist-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"datalist-browser\" data-for=\"Values\" type=\"button\">" +
                        "<span class=\"datalist-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        private HtmlHelper<TestModel> MockHtmlHelper()
        {
            ViewDataDictionary<TestModel> viewData = new ViewDataDictionary<TestModel>(new TestModel());
            IViewDataContainer container = Substitute.For<IViewDataContainer>();
            viewData.Model.ParentId = "Model's parent ID";
            container.ViewData = viewData;

            ViewContext viewContext = Substitute.For<ViewContext>();
            viewContext.HttpContext.Items.Returns(new Hashtable());
            viewContext.ViewData = viewData;

            return new HtmlHelper<TestModel>(viewContext, container);
        }
    }
}
