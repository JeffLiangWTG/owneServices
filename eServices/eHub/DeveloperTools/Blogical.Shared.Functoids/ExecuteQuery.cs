using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;
using System.Collections;
using System.Reflection;
using Blogical.Shared.Functoids.Helpers;
using System.Diagnostics;

namespace Blogical.Shared.Functoids
{
    /// <summary>
    /// The ExecuteQuery functoid is used for executing queries from the database. 
    /// Each query will only be executed once. 
    /// The functoid requires the following input parameters in this order: 
    ///     the connection-string key,
    ///     a flag "true" or "false" telling the functoid to search cache or not,
    ///     the name of the source that is providing the data (table or stored procedure), 
    ///     [a column name, lookup value] or [sp psarameter, parameter value]. 
    /// The functoid can take a maximum of 6 pairs of either [column name and value] or [sp psarameter, parameter value].
    /// </summary>
    public class ExecuteQuery: BaseFunctoid
    {
        /// <summary>
        /// Creates a new instance of the ExecuteQuery-class.
        /// </summary>
        public ExecuteQuery(): base()
        {
            try
            {
                //ID for this functoid
                this.ID = 6010;
                                
                // Resource assembly must be ProjectName.ResourceName if building with VS.Net
                SetupResourceAssembly("Blogical.Shared.Functoids.Properties.Resources", Assembly.GetExecutingAssembly());

                // Pass the resource ID names for functoid name, tooltip
                // description and the 16x16 bitmap for the Map palette
                SetName("ExecuteQuery_NAME");
                SetTooltip("ExecuteQuery_TOOLTIP");
                SetDescription("ExecuteQuery_DESCRIPTION");
                SetBitmap("ExecuteQuery_ICON");
                
                // Put this string handling function under the String 
                // Functoid tab in the Visual Studio toolbox for functoids
                this.Category = FunctoidCategory.DatabaseLookup;

                // 3 required parameters, 12 optional parameters
                this.SetMinParams(3);
                this.SetMaxParams(15);
                
                // Functoid accepts 15 inputs
                for (int num1 = 0; num1 < 15; num1++)
                {
                    AddInputConnectionType(ConnectionType.AllExceptRecord);
                }

                // Set the output connection type
                this.OutputConnectionType = ConnectionType.AllExceptRecord;

                // Set the function name that needs to be called
                // when this functoid is invoked.  The resulting assembly
                // must be present in the Global Assembly Cache
                // to ensure its availability.
                SetExternalFunctionName(GetType().Assembly.FullName,
                    "Blogical.Shared.Functoids.ExecuteQuery",
                    "GetData");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Executes the query and stores the result in memory for later extraction.
        /// </summary>
        /// <param name="connStrKey">Connection-string key to database</param>
        /// <param name="cache">"true"/"false" Flag to tell whether to search cached results or not.</param>
        /// <param name="source">The source that is providing the data</param>
        /// <returns>A key to the in-memory result.</returns>
        public string GetData(string connStrKey, string cache, string source)
        {
            ArrayList arr = new ArrayList();
            return DataWrapper.GetData(connStrKey, bool.Parse(cache), source, arr);
        }

        /// <summary>
        /// Executes the query and stores the result in memory for later extraction.
        /// </summary>
        /// <param name="connStrKey">Connection-string key to database</param>
        /// <param name="cache">"true"/"false" Flag to tell whether to search cached results or not.</param>
        /// <param name="source">The source that is providing the data</param>
        /// <param name="col1">The column to filter rows on.</param>
        /// <param name="val1">The filter-value we search col1 for.</param>
        /// <returns>A key to the in-memory result.</returns>
        public string GetData(string connStrKey, string cache, string source, string col1, string val1)
        {
            ArrayList arr = new ArrayList();
            arr.Add(col1);
            arr.Add(val1);
            return DataWrapper.GetData(connStrKey, bool.Parse(cache), source, arr);
        }

        /// <summary>
        /// Executes the query and stores the result in memory for later extraction.
        /// </summary>
        /// <param name="connStrKey">Connection-string key to database</param>
        /// <param name="cache">"true"/"false" Flag to tell whether to search cached results or not.</param>
        /// <param name="source">The source that is providing the data</param>
        /// <param name="col1">The first column to filter rows on.</param>
        /// <param name="val1">The filter-value we search col1 for.</param>
        /// <param name="col2">The second column to filter rows on.</param>
        /// <param name="val2">The filter-value we search col2 for.</param>
        /// <returns>A key to the in-memory result.</returns>
        public string GetData(string connStrKey, string cache, string source, string col1, string val1, string col2, string val2)
        {
            ArrayList arr = new ArrayList();
            arr.Add(col1);
            arr.Add(val1);
            arr.Add(col2);
            arr.Add(val2);

            return DataWrapper.GetData(connStrKey, bool.Parse(cache), source, arr);
        }

        /// <summary>
        /// Executes the query and stores the result in memory for later extraction.
        /// </summary>
        /// <param name="connStrKey">Connection-string key to database</param>
        /// <param name="cache">"true"/"false" Flag to tell whether to search cached results or not.</param>
        /// <param name="source">The source that is providing the data</param>
        /// <param name="col1">The first column to filter rows on.</param>
        /// <param name="val1">The filter-value we search col1 for.</param>
        /// <param name="col2">The second column to filter rows on.</param>
        /// <param name="val2">The filter-value we search col2 for.</param>
        /// <param name="col3">The third column to filter rows on.</param>
        /// <param name="val3">The filter-value we search col3 for.</param>
        /// <returns>A key to the in-memory result.</returns>
        public string GetData(string connStrKey, string cache, string source, string col1, string val1, string col2, string val2, string col3, string val3)
        {
            ArrayList arr = new ArrayList();
            arr.Add(col1);
            arr.Add(val1);
            arr.Add(col2);
            arr.Add(val2);
            arr.Add(col3);
            arr.Add(val3);
            return DataWrapper.GetData(connStrKey, bool.Parse(cache), source, arr);
        }

        /// <summary>
        /// Executes the query and stores the result in memory for later extraction.
        /// </summary>
        /// <param name="connStrKey">Connection-string key to database</param>
        /// <param name="cache">"true"/"false" Flag to tell whether to search cached results or not.</param>
        /// <param name="source">The source that is providing the data</param>
        /// <param name="col1">The first column to filter rows on.</param>
        /// <param name="val1">The filter-value we search col1 for.</param>
        /// <param name="col2">The second column to filter rows on.</param>
        /// <param name="val2">The filter-value we search col2 for.</param>
        /// <param name="col3">The third column to filter rows on.</param>
        /// <param name="val3">The filter-value we search col3 for.</param>
        /// <param name="col4">The fourth column to filter rows on.</param>
        /// <param name="val4">The filter-value we search col4 for.</param>
        /// <returns>A key to the in-memory result.</returns>
        public string GetData(string connStrKey, string cache, string source, string col1, string val1, string col2, string val2, string col3, string val3, string col4, string val4)
        {
            ArrayList arr = new ArrayList();
            arr.Add(col1);
            arr.Add(val1);
            arr.Add(col2);
            arr.Add(val2);
            arr.Add(col3);
            arr.Add(val3);
            arr.Add(col4);
            arr.Add(val4);
            return DataWrapper.GetData(connStrKey, bool.Parse(cache), source, arr);
        }

        /// <summary>
        /// Executes the query and stores the result in memory for later extraction.
        /// </summary>
        /// <param name="connStrKey">Connection-string key to database</param>
        /// <param name="cache">"true"/"false" Flag to tell whether to search cached results or not.</param>
        /// <param name="source">The source that is providing the data</param>
        /// <param name="col1">The first column to filter rows on.</param>
        /// <param name="val1">The filter-value we search col1 for.</param>
        /// <param name="col2">The second column to filter rows on.</param>
        /// <param name="val2">The filter-value we search col2 for.</param>
        /// <param name="col3">The third column to filter rows on.</param>
        /// <param name="val3">The filter-value we search col3 for.</param>
        /// <param name="col4">The fourth column to filter rows on.</param>
        /// <param name="val4">The filter-value we search col4 for.</param>
        /// <param name="col5">The fifth column to filter rows on.</param>
        /// <param name="val5">The filter-value we search col5 for.</param>
        /// <returns>A key to the in-memory result.</returns>
        public string GetData(string connStrKey, string cache, string source, string col1, string val1, string col2, string val2, string col3, string val3, string col4, string val4, string col5, string val5)
        {
            ArrayList arr = new ArrayList();
            arr.Add(col1);
            arr.Add(val1);
            arr.Add(col2);
            arr.Add(val2);
            arr.Add(col3);
            arr.Add(val3);
            arr.Add(col4);
            arr.Add(val4);
            arr.Add(col5);
            arr.Add(val5);
            return DataWrapper.GetData(connStrKey, bool.Parse(cache), source, arr);
        }

        /// <summary>
        /// Executes the query and stores the result in memory for later extraction.
        /// </summary>
        /// <param name="connStrKey">Connection-string key to database</param>
        /// <param name="cache">"true"/"false" Flag to tell whether to search cached results or not.</param>
        /// <param name="source">The source that is providing the data</param>
        /// <param name="col1">The first column to filter rows on.</param>
        /// <param name="val1">The filter-value we search col1 for.</param>
        /// <param name="col2">The second column to filter rows on.</param>
        /// <param name="val2">The filter-value we search col2 for.</param>
        /// <param name="col3">The third column to filter rows on.</param>
        /// <param name="val3">The filter-value we search col3 for.</param>
        /// <param name="col4">The fourth column to filter rows on.</param>
        /// <param name="val4">The filter-value we search col4 for.</param>
        /// <param name="col5">The fifth column to filter rows on.</param>
        /// <param name="val5">The filter-value we search col5 for.</param>
        /// <param name="col6">The sixth column to filter rows on.</param>
        /// <param name="val6">The filter-value we search col6 for.</param>
        /// <returns>A key to the in-memory result.</returns>
        public string GetData(string connStrKey, string cache, string source, string col1, string val1, string col2, string val2, string col3, string val3, string col4, string val4, string col5, string val5, string col6, string val6)
        {
            ArrayList arr = new ArrayList();
            arr.Add(col1);
            arr.Add(val1);
            arr.Add(col2);
            arr.Add(val2);
            arr.Add(col3);
            arr.Add(val3);
            arr.Add(col4);
            arr.Add(val4);
            arr.Add(col5);
            arr.Add(val5);
            arr.Add(col6);
            arr.Add(val6);
            return DataWrapper.GetData(connStrKey, bool.Parse(cache), source, arr);
        }
    }
}
