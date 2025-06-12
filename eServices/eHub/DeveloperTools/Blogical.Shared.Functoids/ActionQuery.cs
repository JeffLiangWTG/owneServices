using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.BizTalk.BaseFunctoids;
using System.Collections;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Blogical.Shared.Functoids.Helpers;

namespace Blogical.Shared.Functoids
{
	/// <summary>
	/// The ActionQuery functoid is used to execute a stored procedure and return a single value
	/// The functoid requires the following input parameters in this order: 
	///     1) the connection-string key,
	///     2) Name of return parameter (only one return parameter possible)
	///     [a column name, lookup value] or [sp psarameter, parameter value]. 
	/// The functoid can take a maximum of 6 pairs of  [sp psarameter, parameter value].
	/// </summary>
	public class ActionQuery : BaseFunctoid
	{
		/// <summary>
		/// Initializes a new instance of ActionQuery.
		/// </summary>
		public ActionQuery()
			: base()
		{
			try
			{
				//ID for this functoid
				this.ID = 6012;

			   

				// Resource assembly must be ProjectName.ResourceName if building with VS.Net
				SetupResourceAssembly("Blogical.Shared.Functoids.Properties.Resources", Assembly.GetExecutingAssembly());

				// Pass the resource ID names for functoid name, tooltip
				// description and the 16x16 bitmap for the Map palette
				SetName("ActionQuery_NAME");
				SetTooltip("ActionQuery_TOOLTIP");
				SetDescription("ActionQuery_DESCRIPTION");
				SetBitmap("ActionQuery_ICON");

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
					"Blogical.Shared.Functoids.ActionQuery",
					"GetValue");

			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		/// <summary>
		/// Executes ExecuteProcedure function and returns result value
		/// </summary>
		/// <param name="connectionString">Connection-string key to database</param>
		/// <param name="procedure">Procedure to execute</param>
		/// <param name="retCol">The name of the return parameter value</param>
		/// <returns>The value contained return in the return parameter.</returns>
		public string GetValue(string connectionString, string procedure, string retCol)
		{
			ArrayList whereParams = new ArrayList();
			return ExecuteProcedure(connectionString, procedure, retCol, whereParams);

		}

		/// <summary>
		/// Executes ExecuteProcedure function and returns result value
		/// </summary>
		/// <param name="connectionString">Connection-string key to database</param>
		///   <param name="procedure">Procedure to execute</param>
		/// <param name="retCol">The name of the return parameter value</param>
		/// <param name="whereCol1">The first column to filter rows on.</param>
		/// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
		/// <returns>The value contained return in the return parameter.</returns>
		public string GetValue(string connectionString, string procedure, string retCol,
							   string whereCol1, string whereVal1)
		{
			ArrayList whereParams = new ArrayList();
			whereParams.Add(whereCol1);
			whereParams.Add(whereVal1);
			return ExecuteProcedure(connectionString, procedure, retCol, whereParams);
		}

		/// <summary>
		/// Executes ExecuteProcedure function and returns result value
		/// </summary>
		/// <param name="connectionString">Connection-string key to database</param>
		///  <param name="procedure">Procedure to execute</param>
		/// <param name="retCol">The name of the return parameter value</param>
		/// <param name="whereCol1">The first column to filter rows on.</param>
		/// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
		/// <param name="whereCol2">The second column to filter rows on.</param>
		/// <param name="whereVal2">The filter-value we search WhereCol2 for.</param>
		/// <returns>The value contained return in the return parameter.</returns>
		public string GetValue(string connectionString, string procedure, string retCol,
							   string whereCol1, string whereVal1,
							   string whereCol2, string whereVal2)
		{
			ArrayList whereParams = new ArrayList();
			whereParams.Add(whereCol1);
			whereParams.Add(whereVal1);
			whereParams.Add(whereCol2);
			whereParams.Add(whereVal2);
			return ExecuteProcedure(connectionString, procedure, retCol, whereParams);
		}

		/// <summary>
		/// Executes ExecuteProcedure function and returns result value
		/// </summary>
		/// <param name="connectionString">Connection-string key to database</param>
		///  <param name="procedure">Procedure to execute</param>
		/// <param name="retCol">The name of the return parameter value</param>
		/// <param name="whereCol1">The first column to filter rows on.</param>
		/// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
		/// <param name="whereCol2">The second column to filter rows on.</param>
		/// <param name="whereVal2">The filter-value we search WhereCol2 for.</param>
		/// <param name="whereCol3">The third column to filter rows on.</param>
		/// <param name="whereVal3">The filter-value we search WhereCol3 for.</param>
		/// <returns>The value contained return in the return parameter.</returns>
		public string GetValue(string connectionString, string procedure, string retCol,
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
			return ExecuteProcedure(connectionString, procedure, retCol, whereParams);
		}

		/// <summary>
		/// Executes ExecuteProcedure function and returns result value
		/// </summary>
		/// <param name="connectionString">Connection-string key to database</param>
		///  <param name="procedure">Procedure to execute</param>
		/// <param name="retCol">The name of the return parameter value</param>
		/// <param name="whereCol1">The first column to filter rows on.</param>
		/// <param name="whereVal1">The filter-value we search whereCol1 for.</param>
		/// <param name="whereCol2">The second column to filter rows on.</param>
		/// <param name="whereVal2">The filter-value we search WhereCol2 for.</param>
		/// <param name="whereCol3">The third column to filter rows on.</param>
		/// <param name="whereVal3">The filter-value we search WhereCol3 for.</param>
		/// <param name="whereCol4">The fourth column to filter rows on.</param>
		/// <param name="whereVal4">The filter-value we search WhereCol4 for.</param>
		/// <returns>The value contained return in the return parameter.</returns>
		public string GetValue(string connectionString, string procedure, string retCol,
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
			return ExecuteProcedure(connectionString, procedure, retCol, whereParams);
		}

		/// <summary>
		/// Executes ExecuteProcedure function and returns result value
		/// </summary>
		/// <param name="connectionString">Connection-string key to database</param>
		/// <param name="procedure">Procedure to execute</param>
		/// <param name="retCol">The name of the return parameter value</param>
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
		/// <returns>The value contained return in the return parameter.</returns>
		public string GetValue(string connectionString, string procedure, string retCol,
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
			return ExecuteProcedure(connectionString, procedure, retCol, whereParams);
		}

		/// <summary>
		/// Executes ExecuteProcedure function and returns result value
		/// </summary>
		/// <param name="connectionString">Connection-string key to database</param>
		/// <param name="procedure">Procedure to execute</param>
		/// <param name="retCol">The name of the return parameter value</param>
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
		/// <returns>The value contained return in the return parameter.</returns>
		public string GetValue(string connectionString, string procedure, string retCol,
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
			return ExecuteProcedure(connectionString, procedure, retCol, whereParams);
		}

		/// <summary>
		/// Executes procedure and returns OUT parameter value
		/// </summary>
		/// <param name="connectionString">Connection-string key to database</param>
		/// <param name="procedure">Procedure to execute</param>
		/// <param name="retCol">The name of the return parameter value. 
		/// If the column contains an equal sign the value after the equal sign is presumed to be a default value
		/// </param>
		/// <param name="whereParams">An arrayList containing pairs (column name and column value) of parameters for the where statement</param>
		private string ExecuteProcedure(string connectionString, string procedure, string retCol, ArrayList whereParams)
		{
			string retValue = "";
			string retDefault = "null";
			string[] retColArr = retCol.Split('=');

			SqlConnection conn = null;

			//Check if refCol contains reference to default return value
			if (retColArr.Length > 1)
			{
				retDefault = retColArr[1];
				retCol = retColArr[0];
			}


			try
			{
				conn = new SqlConnection(DataWrapper.ParseConfigConnectionString(connectionString));

				if (procedure.StartsWith("exec"))
				{
					procedure = procedure.Replace("exec", "").Trim();
				}

				SqlCommand cmd = new SqlCommand(procedure, conn);
				cmd.CommandType = CommandType.StoredProcedure;

				for (int i = 0; i < whereParams.Count; i = i + 2)
				{
					//(string)whereParams[i] param
					//(string)whereParams[i + 1] value
					cmd.Parameters.AddWithValue((string)whereParams[i], (string)whereParams[i + 1]);

				}

				cmd.Parameters.Add(retCol, SqlDbType.VarChar, 8000);
				cmd.Parameters[retCol].Direction = ParameterDirection.Output;

				conn.Open();

				cmd.ExecuteNonQuery();


				retValue = Convert.ToString(cmd.Parameters[retCol].Value);

				if (retValue.Trim().Length == 0)
				{
					retValue = retDefault;
				}


				conn.Close();

			}
			catch (Exception ex)
			{
		   
				throw new Exception("ActionQuery.ExecuteProcedure", ex);

			}

			return retValue;
		}
	}
}
