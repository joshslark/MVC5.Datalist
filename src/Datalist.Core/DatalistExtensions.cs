using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Datalist
{
    public static class DatalistExtensions
    {
        public static IHtmlString AutoComplete<TModel>(this HtmlHelper<TModel> html,
            String name, MvcDatalist model, Object? value = null, Object? htmlAttributes = null)
        {
            TagBuilder datalist = CreateDatalist(model, name, htmlAttributes);
            datalist.AddCssClass("datalist-browseless");

            datalist.InnerHtml = CreateDatalistValues(html, model, name, value);
            datalist.InnerHtml += CreateDatalistControl(html, model, name);

            return new MvcHtmlString(datalist.ToString());
        }
        public static IHtmlString AutoCompleteFor<TModel, TProperty>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, MvcDatalist model, Object? htmlAttributes = null)
        {
            String name = ExpressionHelper.GetExpressionText(expression);
            TagBuilder datalist = CreateDatalist(model, name, htmlAttributes);
            datalist.AddCssClass("datalist-browseless");

            datalist.InnerHtml = CreateDatalistValues(html, model, expression);
            datalist.InnerHtml += CreateDatalistControl(html, model, name);

            return new MvcHtmlString(datalist.ToString());
        }

        public static IHtmlString Datalist<TModel>(this HtmlHelper<TModel> html,
            String name, MvcDatalist model, Object? value = null, Object? htmlAttributes = null)
        {
            TagBuilder datalist = CreateDatalist(model, name, htmlAttributes);

            datalist.InnerHtml = CreateDatalistValues(html, model, name, value);
            datalist.InnerHtml += CreateDatalistControl(html, model, name);
            datalist.InnerHtml += CreateDatalistBrowser(name);

            return new MvcHtmlString(datalist.ToString());
        }
        public static IHtmlString DatalistFor<TModel, TProperty>(this HtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, MvcDatalist model, Object? htmlAttributes = null)
        {
            String name = ExpressionHelper.GetExpressionText(expression);
            TagBuilder datalist = CreateDatalist(model, name, htmlAttributes);

            datalist.InnerHtml = CreateDatalistValues(html, model, expression);
            datalist.InnerHtml += CreateDatalistControl(html, model, name);
            datalist.InnerHtml += CreateDatalistBrowser(name);

            return new MvcHtmlString(datalist.ToString());
        }

        private static TagBuilder CreateDatalist(MvcDatalist datalist, String name, Object? htmlAttributes)
        {
            IDictionary<String, Object?> attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            attributes["data-filters"] = String.Join(",", datalist.AdditionalFilters);
            attributes["data-readonly"] = datalist.ReadOnly ? "true" : "false";
            attributes["data-multi"] = datalist.Multi ? "true" : "false";
            attributes["data-order"] = datalist.Filter.Order.ToString();
            attributes["data-offset"] = datalist.Filter.Offset;
            attributes["data-search"] = datalist.Filter.Search;
            attributes["data-rows"] = datalist.Filter.Rows;
            attributes["data-sort"] = datalist.Filter.Sort;
            attributes["data-dialog"] = datalist.Dialog;
            attributes["data-title"] = datalist.Title;
            attributes["data-url"] = datalist.Url;
            attributes["data-for"] = name;

            TagBuilder group = new TagBuilder("div");
            group.MergeAttributes(attributes);
            group.AddCssClass("datalist");

            if (datalist.ReadOnly)
                group.AddCssClass("datalist-readonly");

            return group;
        }

        private static String CreateDatalistValues<TModel, TProperty>(HtmlHelper<TModel> html, MvcDatalist datalist, Expression<Func<TModel, TProperty>> expression)
        {
            Object value = ModelMetadata.FromLambdaExpression(expression, html.ViewData).Model;
            String name = ExpressionHelper.GetExpressionText(expression);

            if (datalist.Multi)
                return CreateDatalistValues(html, datalist, name, value);

            IDictionary<String, Object> attributes = new Dictionary<String, Object>();
            attributes["class"] = "datalist-value";
            attributes["autocomplete"] = "off";

            TagBuilder container = new TagBuilder("div");
            container.AddCssClass("datalist-values");
            container.Attributes["data-for"] = name;

            container.InnerHtml = html.HiddenFor(expression, attributes).ToString();

            return container.ToString();
        }
        private static String CreateDatalistValues(HtmlHelper html, MvcDatalist datalist, String name, Object? value)
        {
            IDictionary<String, Object> attributes = new Dictionary<String, Object>();
            attributes["class"] = "datalist-value";
            attributes["autocomplete"] = "off";

            TagBuilder container = new TagBuilder("div");
            container.AddCssClass("datalist-values");
            container.Attributes["data-for"] = name;

            if (datalist.Multi)
            {
                IEnumerable<Object>? values = (value as IEnumerable)?.Cast<Object>();
                if (values == null) return container.ToString();

                TagBuilder input = new TagBuilder("input");
                input.Attributes["type"] = "hidden";
                input.MergeAttributes(attributes);
                input.Attributes["name"] = name;

                StringBuilder inputs = new StringBuilder();
                foreach (Object val in values)
                {
                    input.Attributes["value"] = html.FormatValue(val, null);

                    inputs.Append(input.ToString(TagRenderMode.SelfClosing));
                }

                container.InnerHtml = inputs.ToString();
            }
            else
            {
                container.InnerHtml = html.Hidden(name, value, attributes).ToString();
            }

            return container.ToString();
        }
        private static String CreateDatalistControl(HtmlHelper html, MvcDatalist datalist, String name)
        {
            Dictionary<String, Object> attributes = new Dictionary<String, Object>();
            TagBuilder search = new TagBuilder("input");
            TagBuilder control = new TagBuilder("div");
            TagBuilder loader = new TagBuilder("div");
            TagBuilder error = new TagBuilder("div");

            if (datalist.Name != null) attributes["id"] = ExpressionHelper.GetExpressionText(datalist.Name);
            if (datalist.Placeholder != null) attributes["placeholder"] = datalist.Placeholder;
            if (datalist.Name != null) attributes["name"] = datalist.Name;
            if (datalist.ReadOnly) attributes["readonly"] = "readonly";
            attributes["class"] = "datalist-input";
            attributes["autocomplete"] = "off";

            if (datalist.ReadOnly) search.Attributes["readonly"] = "readonly";
            loader.AddCssClass("datalist-control-loader");
            error.AddCssClass("datalist-control-error");
            control.AddCssClass("datalist-control");
            control.Attributes["data-for"] = name;
            search.MergeAttributes(attributes);
            error.InnerHtml = "!";

            if (datalist.Name != null)
                control.InnerHtml = html.TextBox(datalist.Name, null, attributes).ToString();
            else
                control.InnerHtml = search.ToString(TagRenderMode.SelfClosing);

            control.InnerHtml += loader.ToString();
            control.InnerHtml += error.ToString();
            control.InnerHtml += CreateDatalistClear(name);

            return control.ToString();
        }
        private static String CreateDatalistBrowser(String name)
        {
            TagBuilder browser = new TagBuilder("button");
            browser.AddCssClass("datalist-browser");
            browser.Attributes["data-for"] = name;
            browser.Attributes["type"] = "button";

            TagBuilder icon = new TagBuilder("span");
            icon.AddCssClass("datalist-icon");

            browser.InnerHtml = icon.ToString();

            return browser.ToString();
        }

        private static String CreateDatalistClear(String name)
        {
            TagBuilder clear = new TagBuilder("button");
            clear.AddCssClass("datalist-clear");
            clear.Attributes["data-for"] = name;
            clear.Attributes["type"] = "button";

            TagBuilder icon = new TagBuilder("span");
            icon.AddCssClass("fa");
            icon.AddCssClass("fa-times");

            clear.InnerHtml = icon.ToString();

            return clear.ToString();
        }
    }
}
