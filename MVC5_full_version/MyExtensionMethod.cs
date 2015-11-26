using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace MVC5_full_version
{
    public static class MyExtensionMethod
    {

        public static List<TSource> ToList<TSource>(this DataTable dataTable) where TSource : new()
        {
            var dataList = new List<TSource>();

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var objFieldNames = (from PropertyInfo aProp in typeof(TSource).GetProperties(flags)
                                 select new
                                 {
                                     Name = aProp.Name,
                                     Type = Nullable.GetUnderlyingType(aProp.PropertyType) ??
                             aProp.PropertyType
                                 }).ToList();
            var dataTblFieldNames = (from DataColumn aHeader in dataTable.Columns
                                     select new
                                     {
                                         Name = aHeader.ColumnName,
                                         Type = aHeader.DataType
                                     }).ToList();
            var commonFields = objFieldNames.Intersect(dataTblFieldNames).ToList();

            foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
            {
                var aTSource = new TSource();
                foreach (var aField in commonFields)
                {
                    PropertyInfo propertyInfos = aTSource.GetType().GetProperty(aField.Name);
                    var value = (dataRow[aField.Name] == DBNull.Value) ?
                    null : dataRow[aField.Name]; //if database field is nullable
                    propertyInfos.SetValue(aTSource, value, null);
                }
                dataList.Add(aTSource);
            }
            return dataList;
        }

        public static DataTable ToDataTable<T>(this List<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        public static DataRow ConvertToDataRow<T>(this T item, DataTable table)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataRow row = table.NewRow();
            foreach (PropertyDescriptor prop in properties)
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            return row;
        }

        public static T ConvertToEntity<T>(this DataRow tableRow) where T : new()
        {
            // Create a new type of the entity I want
            Type t = typeof(T);
            T returnObject = new T();

            foreach (DataColumn col in tableRow.Table.Columns)
            {
                string colName = col.ColumnName;

                // Look for the object's property with the columns name, ignore case
                PropertyInfo pInfo = t.GetProperty(colName.ToLower(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                // did we find the property ?
                if (pInfo != null)
                {
                    object val = tableRow[colName];

                    // is this a Nullable<> type
                    bool IsNullable = (Nullable.GetUnderlyingType(pInfo.PropertyType) != null);
                    if (IsNullable)
                    {
                        if (val is System.DBNull)
                        {
                            val = null;
                        }
                        else
                        {
                            // Convert the db type into the T we have in our Nullable<T> type
                            val = Convert.ChangeType(val, Nullable.GetUnderlyingType(pInfo.PropertyType));
                        }
                    }
                    else
                    {
                        // Convert the db type into the type of the property in our entity
                        SetDefaultValue(ref val, pInfo.PropertyType);
                        if (pInfo.PropertyType.IsEnum && !pInfo.PropertyType.IsGenericType)
                        {
                            val = Enum.ToObject(pInfo.PropertyType, val);
                        }
                        else
                            val = Convert.ChangeType(val, pInfo.PropertyType);
                    }
                    // Set the value of the property with the value from the db
                    if (pInfo.CanWrite)
                        pInfo.SetValue(returnObject, val, null);
                }
            }

            // return the entity object with values
            return returnObject;
        }

        private static void SetDefaultValue(ref object val, Type propertyType)
        {
            if (val is DBNull)
            {
                val = GetDefault(propertyType);
            }
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static string GetDefaultDateTimeFormat(this string date)
        {
            try
            {
                return Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            }
            catch (Exception ex)
            {
                return date;
            }
        }
        public static string ToDefaultDateTimeFormat(this string date)
        {
            try
            {
                string datetime = Convert.ToDateTime(date).ToString("d", CultureInfo.CreateSpecificCulture("en-US"));
                return Convert.ToDateTime(datetime).ToString("dd-MMM-yyyy");
            }
            catch (Exception ex)
            {
                return date;
            }
        }
    }
    public static class DataTableExtensions
    {
        /// <summary>
        /// Creates data table from source data.
        /// </summary>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            DataTable table = new DataTable();

            //// get properties of T
            var binding = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;
            var options =  PropertyReflectionOptions.IgnoreEnumerable | PropertyReflectionOptions.IgnoreIndexer;

            var properties = ReflectionExtensions.GetProperties<T>(binding, options).ToList();

            //// create table schema based on properties
            foreach (var property in properties)
            {
                table.Columns.Add(property.Name, property.PropertyType);
            }

            //// create table data from T instances
            object[] values = new object[properties.Count];

            foreach (T item in source)
            {
                for (int i = 0; i < properties.Count; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }

                table.Rows.Add(values);
            }

            return table;
        }
    }
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Gets properties of T
        /// </summary>
        public static IEnumerable<PropertyInfo> GetProperties<T>(BindingFlags binding, PropertyReflectionOptions options = PropertyReflectionOptions.All)
        {
            var properties = typeof(T).GetProperties(binding);

            bool all = (options & PropertyReflectionOptions.All) != 0;
            bool ignoreIndexer = (options & PropertyReflectionOptions.IgnoreIndexer) != 0;
            bool ignoreEnumerable = (options & PropertyReflectionOptions.IgnoreEnumerable) != 0;

            foreach (var property in properties)
            {
                if (!all)
                {
                    if (ignoreIndexer && IsIndexer(property))
                    {
                        continue;
                    }

                    if (ignoreIndexer && !property.PropertyType.Equals(typeof(string)) && IsEnumerable(property))
                    {
                        continue;
                    }
                }

                yield return property;
            }
        }

        /// <summary>
        /// Check if property is indexer
        /// </summary>
        private static bool IsIndexer(PropertyInfo property)
        {
            var parameters = property.GetIndexParameters();

            if (parameters != null && parameters.Length > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if property implements IEnumerable
        /// </summary>
        private static bool IsEnumerable(PropertyInfo property)
        {
            return property.PropertyType.GetInterfaces().Any(x => x.Equals(typeof(System.Collections.IEnumerable)));
        }
    }

    [Flags]
    public enum PropertyReflectionOptions : int
    {
        /// <summary>
        /// Take all.
        /// </summary>
        All = 0,

        /// <summary>
        /// Ignores indexer properties.
        /// </summary>
        IgnoreIndexer = 1,

        /// <summary>
        /// Ignores all other IEnumerable properties
        /// except strings.
        /// </summary>
        IgnoreEnumerable = 2
    }
}