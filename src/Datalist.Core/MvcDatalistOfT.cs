using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Design;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using FuzzyString;

namespace Datalist
{
    public abstract class MvcDatalist<T> : MvcDatalist where T : class
    {
        public Func<T, String> GetId { get; set; }
        public Func<T, String> GetLabel { get; set; }
        public virtual IEnumerable<PropertyInfo> AttributedProperties
        {
            get
            {
                return typeof(T)
                    .GetProperties()
                    .Where(property => property.IsDefined(typeof(DatalistColumnAttribute), false))
                    .OrderBy(property => property.GetCustomAttribute<DatalistColumnAttribute>(false)!.Position);
            }
        }

        protected MvcDatalist()
        {
            GetId = (model) => GetValue(model, "Id") ?? "";
            GetLabel = (model) => GetValue(model, Columns.Where(col => !col.Hidden).Select(col => col.Key).FirstOrDefault() ?? "") ?? "";

            foreach (PropertyInfo property in AttributedProperties)
            {
                DatalistColumnAttribute column = property.GetCustomAttribute<DatalistColumnAttribute>(false)!;
                Columns.Add(new DatalistColumn(GetColumnKey(property), GetColumnHeader(property))
                {
                    CssClass = GetColumnCssClass(property),
                    Filterable = column.Filterable,
                    Hidden = column.Hidden
                });
            }
        }
        public virtual String GetColumnKey(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            return property.Name;
        }
        public virtual String GetColumnHeader(PropertyInfo property)
        {
            return property?.GetCustomAttribute<DisplayAttribute>(false)?.GetShortName() ?? "";
        }
        public virtual String GetColumnCssClass(PropertyInfo property)
        {
            return "";
        }

        public override DatalistData GetData()
        {
            IQueryable<T> models = GetModels();
            IQueryable<T> selected = new T[0].AsQueryable();
            IQueryable<T> notSelected = Sort(FilterByRequest(models));

            if (Filter.Offset == 0 && Filter.Ids.Count == 0 && Filter.Selected.Count > 0)
                selected = Sort(FilterBySelected(models, Filter.Selected));

            return FormDatalistData(notSelected, selected, Page(notSelected));
        }
        public abstract IQueryable<T> GetModels();

        private IQueryable<T> FilterByRequest(IQueryable<T> models)
        {
            if (Filter.Ids.Count > 0)
                return FilterByIds(models, Filter.Ids);

            if (Filter.Selected.Count > 0)
                models = FilterByNotIds(models, Filter.Selected);

            if (Filter.CheckIds.Count > 0)
                models = FilterByCheckIds(models, Filter.CheckIds);

            if (Filter.AdditionalFilters.Count > 0)
                models = FilterByAdditionalFilters(models);

            return FilterBySearch(models);
        }
        public virtual IQueryable<T> FilterBySearch(IQueryable<T> models)
        {
            if (String.IsNullOrEmpty(Filter.Search))
                return models;

            List<String> queries = new List<String>();
            List<string> properties = new List<string>();
            foreach (String property in Columns.Where(column => !column.Hidden && column.Filterable).Select(column => column.Key))
                if (typeof(T).GetProperty(property)?.PropertyType == typeof(String))
                {

                    queries.Add($"({property} != null && {property}.ToLower().Contains(@0))");
                    properties.Add(property);
                }

            if (queries.Count == 0)
                return models;

            IQueryable<T> filteredModels = models.Where(model => ScoreSearchMatch(Filter.Search, model, properties) > 0.5);
            filteredModels = filteredModels.OrderByDescending(model => ScoreSearchMatch(Filter.Search, model, properties));
            var scores = filteredModels.Select(model => new { model, score = ScoreSearchMatch(Filter.Search, model, properties) });

            return filteredModels;
        }
        private double ScoreSearchMatch(String? searchInput, T model, List<string> modelProperties)
        {
            List<double> scores = new List<double>();
            String? lowercaseSearchInput = searchInput?.ToLower();
            foreach (string property in modelProperties)
            {
                String? propertyValue = GetValue(model, property);
                String? lowercasePropertyValue = propertyValue?.ToLower();
                if (propertyValue == null)
                {
                    scores.Add(0.0);
                }
                else if (propertyValue.Equals(searchInput))
                {
                    scores.Add(propertyValue.Length);
                }
                else if (lowercasePropertyValue?.Equals(lowercaseSearchInput) ?? false)
                {
                    scores.Add(propertyValue.Length - 0.1);
                }
                else if (propertyValue.StartsWith(searchInput))
                {
                    int score = searchInput?.Length ?? 0;
                    double lowerScore = score - 0.2;
                    scores.Add(lowerScore);
                }
                else if (lowercasePropertyValue?.StartsWith(lowercaseSearchInput) ?? false)
                {
                    int score = searchInput?.Length ?? 0;
                    double lowerScore = score - 0.3;
                    scores.Add(lowerScore);
                }
                else
                {
                    double score = searchInput.RatcliffObershelpSimilarity(propertyValue);
                    scores.Add(score);
                }
			}

            return scores.Max();
		}
		public virtual IQueryable<T> FilterByAdditionalFilters(IQueryable<T> models)
        {
            foreach (KeyValuePair<String, Object?> filter in Filter.AdditionalFilters.Where(item => item.Value != null))
                if (filter.Value is IEnumerable && !(filter.Value is String))
                    models = models.Where($"@0.Contains(outerIt.{filter.Key})", filter.Value).AsQueryable();
                else
                    models = models.Where($"({filter.Key} != null && {filter.Key} == @0)", filter.Value);

            return models;
        }
        public virtual IQueryable<T> FilterByIds(IQueryable<T> models, IList<String> ids)
        {
            PropertyInfo? key = typeof(T).GetProperties()
                .FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute))) ?? typeof(T).GetProperty("Id");

            if (key == null)
                throw new DatalistException($"'{typeof(T).Name}' type does not have key or property named 'Id', required for automatic id filtering.");

            if (IsFilterable(key.PropertyType))
                return models.Where($"@0.Contains(outerIt.{key.Name})", Parse(key.PropertyType, ids));

            throw new DatalistException($"'{typeof(T).Name}.{key.Name}' property type has to be a string, guid or a number.");
        }
        public virtual IQueryable<T> FilterByNotIds(IQueryable<T> models, IList<String> ids)
        {
            PropertyInfo? key = typeof(T).GetProperties()
                .FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute))) ?? typeof(T).GetProperty("Id");

            if (key == null)
                throw new DatalistException($"'{typeof(T).Name}' type does not have key or property named 'Id', required for automatic id filtering.");

            if (IsFilterable(key.PropertyType))
                return models.Where($"!@0.Contains(outerIt.{key.Name})", Parse(key.PropertyType, ids));

            throw new DatalistException($"'{typeof(T).Name}.{key.Name}' property type has to be a string, guid or a number.");
        }
        public virtual IQueryable<T> FilterByCheckIds(IQueryable<T> models, IList<String> ids)
        {
            return FilterByIds(models, ids);
        }
        public virtual IQueryable<T> FilterBySelected(IQueryable<T> models, IList<String> ids)
        {
            return FilterByIds(models, ids);
        }

        public virtual IQueryable<T> Sort(IQueryable<T> models)
        {
            if (String.IsNullOrWhiteSpace(Filter.Sort))
                if (DatalistQuery.IsOrdered(models))
                    return models;
                else
                    return models.OrderBy(model => 0);

            return models.OrderBy(Filter.Sort + " " + Filter.Order);
        }

        public virtual IQueryable<T> Page(IQueryable<T> models)
        {
            Filter.Offset = Math.Max(0, Filter.Offset);
            Filter.Rows = Math.Max(1, Math.Min(Filter.Rows, 100));

            return models.Skip(Filter.Offset).Take(Filter.Rows);
        }

        public virtual DatalistData FormDatalistData(IQueryable<T> filtered, IQueryable<T> selected, IQueryable<T> notSelected)
        {
            DatalistData data = new DatalistData();
            data.Columns = Columns;

            foreach (T model in selected)
                data.Selected.Add(FormData(model));

            foreach (T model in notSelected)
                data.Rows.Add(FormData(model));

            return data;
        }

        public virtual Dictionary<String, String?> FormData(T model)
        {
            Dictionary<String, String?> data = new Dictionary<String, String?>();

            foreach (DatalistColumn column in Columns)
                data[column.Key] = GetValue(model, column.Key);

            data["Label"] = GetLabel(model);
            data["Id"] = GetId(model);

            return data;
        }

        private String? GetValue(T model, String propertyName)
        {
            PropertyInfo? property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return null;

            DatalistColumnAttribute column = property.GetCustomAttribute<DatalistColumnAttribute>(false);
            if (column?.Format != null)
                return String.Format(column.Format, property.GetValue(model));

            return property.GetValue(model)?.ToString();
        }
        private Object Parse(Type type, IList<String> ids)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            IList values = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;

            foreach (String value in ids)
                values.Add(converter.ConvertFrom(value));

            return values;
        }
        private Boolean IsFilterable(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return true;
                default:
                    return type == typeof(Guid);
            }
        }
    }
}
