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
    /// The Value Extractor functoid is used to extract the appropriate column value from a recordset 
    /// returned by the Execute Query functoid. 
    /// This functoid requires two input parameters: a link to the ExecuteQuery functoid and a column name.
    /// </summary>
    public class ExtractData : BaseFunctoid
    {
        /// <summary>
        /// Initializes a new instance of ExtractData.
        /// </summary>
        public ExtractData(): base()
        {
            try
            {
                //ID for this functoid
                this.ID = 6011;

                // Resource assembly must be ProjectName.ResourceName if building with VS.Net
                SetupResourceAssembly("Blogical.Shared.Functoids.Properties.Resources", Assembly.GetExecutingAssembly());

                // Pass the resource ID names for functoid name, tooltip
                // description and the 16x16 bitmap for the Map palette
                SetName("ExtractData_NAME");
                SetTooltip("ExtractData_TOOLTIP");
                SetDescription("ExtractData_DESCRIPTION");
                SetBitmap("ExtractData_ICON");

                // Put this string handling function under the String 
                // Functoid tab in the Visual Studio toolbox for functoids
                this.Category = FunctoidCategory.DatabaseLookup;

                // 2 required parameters, no optional parameters
                this.SetMinParams(2);
                this.SetMaxParams(14);

                // Functoid accepts 14 inputs
                for (int num1 = 0; num1 < 14; num1++)
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
                    "Blogical.Shared.Functoids.ExtractData",
                    "GetValue");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Extracts data from the in-memory store created by the ExecuteData-functoid.
        /// </summary>
        /// <param name="query">The key to the data-object (the ExecuteQuery result)</param>
        /// <param name="retCol">The name of the column we're interested in.</param>
        /// <returns>The value contained in the column specified in retCol.</returns>
        public string GetValue(string query, string retCol)
        {
            ArrayList whereParams = new ArrayList();
            return DataWrapper.ExtractData(query, retCol, whereParams);

        }

        /// <summary>
        /// Extracts data from the in-memory store created by the ExecuteData-functoid.
        /// </summary>
        /// <param name="query">The key to the data-object (the ExecuteQuery result)</param>
        /// <param name="retCol">The name of the column we're interested in.</param>
        /// <param name="whereCol1">The first column to filter rows on.</param>
        /// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
        /// <returns>The value contained in the column specified in retCol.</returns>
        public string GetValue(string query, string retCol, 
                               string whereCol1, string whereVal1)
        {
            ArrayList whereParams = new ArrayList();
            whereParams.Add(whereCol1);
            whereParams.Add(whereVal1);
            return DataWrapper.ExtractData(query, retCol, whereParams);
        }

        /// <summary>
        /// Extracts data from the in-memory store created by the ExecuteData-functoid.
        /// </summary>
        /// <param name="query">The key to the data-object (the ExecuteQuery result)</param>
        /// <param name="retCol">The name of the column we're interested in.</param>
        /// <param name="whereCol1">The first column to filter rows on.</param>
        /// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
        /// <param name="whereCol2">The second column to filter rows on.</param>
        /// <param name="whereVal2">The filter-value we search WhereCol2 for.</param>
        /// <returns>The value contained in the column specified in retCol.</returns>
        public string GetValue(string query, string retCol, 
                               string whereCol1, string whereVal1, 
                               string whereCol2, string whereVal2)
        {
            ArrayList whereParams = new ArrayList();
            whereParams.Add(whereCol1);
            whereParams.Add(whereVal1);
            whereParams.Add(whereCol2);
            whereParams.Add(whereVal2);

           

            return DataWrapper.ExtractData(query, retCol, whereParams);
        }

        /// <summary>
        /// Extracts data from the in-memory store created by the ExecuteData-functoid.
        /// </summary>
        /// <param name="query">The key to the data-object (the ExecuteQuery result)</param>
        /// <param name="retCol">The name of the column we're interested in.</param>
        /// <param name="whereCol1">The first column to filter rows on.</param>
        /// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
        /// <param name="whereCol2">The second column to filter rows on.</param>
        /// <param name="whereVal2">The filter-value we search WhereCol2 for.</param>
        /// <param name="whereCol3">The third column to filter rows on.</param>
        /// <param name="whereVal3">The filter-value we search WhereCol3 for.</param>
        /// <returns>The value contained in the column specified in retCol.</returns>
        public string GetValue(string query, string retCol, 
                               string whereCol1, string whereVal1, 
                               string whereCol2, string whereVal2, 
                               string whereCol3, string whereVal3)
        {
            ArrayList whereParams = new ArrayList();
            whereParams.Add(whereCol1);
            whereParams.Add(whereVal1);
            whereParams.Add(whereCol2);
            whereParams.Add(whereVal2);
            whereParams.Add(whereCol3);
            whereParams.Add(whereVal3);
            return DataWrapper.ExtractData(query, retCol, whereParams);
        }

        /// <summary>
        /// Extracts data from the in-memory store created by the ExecuteData-functoid.
        /// </summary>
        /// <param name="query">The key to the data-object (the ExecuteQuery result)</param>
        /// <param name="retCol">The name of the column we're interested in.</param>
        /// <param name="whereCol1">The first column to filter rows on.</param>
        /// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
        /// <param name="whereCol2">The second column to filter rows on.</param>
        /// <param name="whereVal2">The filter-value we search WhereCol2 for.</param>
        /// <param name="whereCol3">The third column to filter rows on.</param>
        /// <param name="whereVal3">The filter-value we search WhereCol3 for.</param>
        /// <param name="whereCol4">The fourth column to filter rows on.</param>
        /// <param name="whereVal4">The filter-value we search WhereCol4 for.</param>
        /// <returns>The value contained in the column specified in retCol.</returns>
        public string GetValue(string query, string retCol, 
                               string whereCol1, string whereVal1, 
                               string whereCol2, string whereVal2, 
                               string whereCol3, string whereVal3, 
                               string whereCol4, string whereVal4)
        {
            ArrayList whereParams = new ArrayList();
            whereParams.Add(whereCol1);
            whereParams.Add(whereVal1);
            whereParams.Add(whereCol2);
            whereParams.Add(whereVal2);
            whereParams.Add(whereCol3);
            whereParams.Add(whereVal3);
            whereParams.Add(whereCol4);
            whereParams.Add(whereVal4);
            return DataWrapper.ExtractData(query, retCol, whereParams);
        }

        /// <summary>
        /// Extracts data from the in-memory store created by the ExecuteData-functoid.
        /// </summary>
        /// <param name="query">The key to the data-object (the ExecuteQuery result)</param>
        /// <param name="retCol">The name of the column we're interested in.</param>
        /// <param name="whereCol1">The first column to filter rows on.</param>
        /// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
        /// <param name="whereCol2">The second column to filter rows on.</param>
        /// <param name="whereVal2">The filter-value we search WhereCol2 for.</param>
        /// <param name="whereCol3">The third column to filter rows on.</param>
        /// <param name="whereVal3">The filter-value we search WhereCol3 for.</param>
        /// <param name="whereCol4">The fourth column to filter rows on.</param>
        /// <param name="whereVal4">The filter-value we search WhereCol4 for.</param>
        /// <param name="whereCol5">The fifth column to filter rows on.</param>
        /// <param name="whereVal5">The filter-value we search WhereCol5 for.</param>
        /// <returns>The value contained in the column specified in retCol.</returns>
        public string GetValue(string query, string retCol, 
                               string whereCol1, string whereVal1, 
                               string whereCol2, string whereVal2,
                               string whereCol3, string whereVal3, 
                               string whereCol4, string whereVal4, 
                               string whereCol5, string whereVal5)
        {
            ArrayList whereParams = new ArrayList();
            whereParams.Add(whereCol1);
            whereParams.Add(whereVal1);
            whereParams.Add(whereCol2);
            whereParams.Add(whereVal2);
            whereParams.Add(whereCol3);
            whereParams.Add(whereVal3);
            whereParams.Add(whereCol4);
            whereParams.Add(whereVal4);
            whereParams.Add(whereCol5);
            whereParams.Add(whereVal5);
            return DataWrapper.ExtractData(query, retCol, whereParams);
        }

        /// <summary>
        /// Extracts data from the in-memory store created by the ExecuteData-functoid.
        /// </summary>
        /// <param name="query">The key to the data-object (the ExecuteQuery result)</param>
        /// <param name="retCol">The name of the column we're interested in.</param>
        /// <param name="whereCol1">The first column to filter rows on.</param>
        /// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
        /// <param name="whereCol2">The second column to filter rows on.</param>
        /// <param name="whereVal2">The filter-value we search WhereCol2 for.</param>
        /// <param name="whereCol3">The third column to filter rows on.</param>
        /// <param name="whereVal3">The filter-value we search WhereCol3 for.</param>
        /// <param name="whereCol4">The fourth column to filter rows on.</param>
        /// <param name="whereVal4">The filter-value we search WhereCol4 for.</param>
        /// <param name="whereCol5">The fifth column to filter rows on.</param>
        /// <param name="whereVal5">The filter-value we search WhereCol5 for.</param>
        /// <param name="whereCol6">The sixth column to filter rows on.</param>
        /// <param name="whereVal6">The filter-value we search WhereCol6 for.</param>
        /// <returns>The value contained in the column specified in retCol.</returns>
        public string GetValue(string query, string retCol, 
                               string whereCol1, string whereVal1, 
                               string whereCol2, string whereVal2, 
                               string whereCol3, string whereVal3, 
                               string whereCol4, string whereVal4, 
                               string whereCol5, string whereVal5, 
                               string whereCol6, string whereVal6)
        {
            ArrayList whereParams = new ArrayList();
            whereParams.Add(whereCol1);
            whereParams.Add(whereVal1);
            whereParams.Add(whereCol2);
            whereParams.Add(whereVal2);
            whereParams.Add(whereCol3);
            whereParams.Add(whereVal3);
            whereParams.Add(whereCol4);
            whereParams.Add(whereVal4);
            whereParams.Add(whereCol5);
            whereParams.Add(whereVal5);
            whereParams.Add(whereCol6);
            whereParams.Add(whereVal6);
            return DataWrapper.ExtractData(query, retCol, whereParams);
        }
    }
}
