using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Diagnostics;
using System.IO;



namespace Blogical.Shared.Functoids.Helpers
{
	/// <summary>
	/// Exposes functionality to get data from database or cache, 
	/// also holds results in-memory to enable fetching data from results cross-functoid.
	/// </summary>
	public class DataWrapper 
	{
		#region Private Fields

		/// <summary>
		/// Holds result fetched by ExecuteQuery for ExtractData.
		/// </summary>
		[ThreadStatic]
		private static Hashtable _hashTable;

		/// <summary>
		///Fernando Pires 2011-01-16
		///Local variable to hold timeout in minutes. default 1 min.
		/// </summary>
		private static int cacheTimeOutMinutes = CacheTimeOutMinutes();

		#endregion

		#region Private Constants

		/// <summary>
		/// Format-string for the data-object key.
		/// </summary>
		private const string KEY_FORMAT = "{0}_{1}";
		/// <summary>
		/// Used for indicate the connection string is to be found in the BTSNTSvc.exe.config file
		/// </summary>
		private const string CONFIG_MONIKER = "CONFIG://";

		#endregion

		#region Private Properties

		/// <summary>
		/// Gives us a safe and easy way to access the HelperHashtable.
		/// </summary>
		/// <value>The cache contained in the HttpRuntime-class</value>
		private static Hashtable hashTable
		{
			get
			{
				if (_hashTable == null)
				{
					_hashTable = Hashtable.Synchronized(new Hashtable());
				}
				return _hashTable;
			}
		}

		/// <summary>
		/// Gives us a safe and easy way to access the cache.
		/// </summary>
		/// <value>The cache contained in the HttpRuntime-class</value>
		private static Cache cache
		{
			get
			{
				return CacheManager.Cache;
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates the key to this query.
		/// </summary>
		/// <param name="connectionString">The database connection-string key</param>
		/// <param name="query">The query executed.</param>
		/// <returns>A key to this query.</returns>
		private static string CreateQueryKey(string connectionString, string query) {
			return string.Format(KEY_FORMAT, connectionString, query);            
		}

		/// <summary>
		/// Executes the query in the database and returns the result.
		/// </summary>
		/// <param name="connectionString">The database connection-string key</param>
		/// <param name="sqlCommand">The query executed.</param>
		/// <returns>The first datarow-match fetched from the database. 
		/// If error occurs or no row is fetched, a string value of "#" will be returned.</returns>
		private static DataTable GetValueFromDatabase(string connectionString, string sqlCommand)
		{
			SqlConnection sqlConnection = null;
			try
			{

				sqlConnection = new SqlConnection(connectionString);
				SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand, sqlConnection);
				sqlConnection.Open();

				DataSet ds = new DataSet();
				adapter.Fill(ds, "ResultTable");
				sqlConnection.Close();

				
				return ds.Tables["ResultTable"];
			}
			catch (Exception ex)
			{
				string message = string.Format("DataWrapper.GetValueFromDatabase: Error querying \"{0}\" for \"{1}\". Message: {2}", connectionString, sqlCommand, ex.Message);
				throw new ApplicationException(message, ex);
			}
			
		}

		/// <summary>
		/// Constructs the query-string to execute in the database.
		/// </summary>
		/// <param name="table">The name of the table we're looking in.</param>
		/// <param name="columnsAndValues">ArrayList with filter-columns and -values</param>
		/// <returns>A query-string.</returns>
		private static string GetSqlQuery(string table, ArrayList columnsAndValues)
		{
			string sqlQuery = "select * from " + table;

			if (columnsAndValues.Count > 0)
			{
				sqlQuery += " where ";
				sqlQuery += columnsAndValues[0].ToString() + "='" + columnsAndValues[1].ToString() + "'";

				for (int i = 2; i < columnsAndValues.Count; i = i + 2)
				{
					sqlQuery += " and " + columnsAndValues[i].ToString() + "='" + columnsAndValues[i + 1].ToString() + "'";
				}
			}
			return sqlQuery;

		}

		/// <summary>
		/// Constructs the execcute procedure command to execute in the database.
		/// </summary>
		/// <param name="sp">The name of the stored procedure.</param>
		/// <param name="columnsAndValues">ArrayList with parameters and -values</param>
		/// <returns>An execute procedure command.</returns>
		private static string GetTsqlQuery(string sp, ArrayList columnsAndValues)
		{
			string tSqlQuery = sp;

			if (columnsAndValues.Count > 0)
			{
				tSqlQuery += " " + columnsAndValues[0].ToString() + " = '" + columnsAndValues[1].ToString().Replace("'", "''") + "'";

				for (int i = 2; i < columnsAndValues.Count; i = i + 2)
				{
					tSqlQuery += ", " + columnsAndValues[i].ToString() + "='" + columnsAndValues[i + 1].ToString().Replace("'", "''") + "'";
				}
			}
			return tSqlQuery;
		}

		/// <summary>
		/// Constructs the query to execute in the database.
		/// </summary>
		/// <param name="source">The source that is providing us with data</param>
		/// <param name="colsAndVals">ArrayList with filter-column values or procedure parameters</param>
		/// <returns>A query to call the database with</returns>
		private static string GetQuery(string source, ArrayList colsAndVals)
		{
			return !(source.Length >= 5 && source.ToLower().StartsWith("exec ")) ? GetSqlQuery(source, colsAndVals) : GetTsqlQuery(source, colsAndVals);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets a value from a column in a data-object persisted in memory.
		/// </summary>
		/// <param name="queryKey">The key to the data-object</param>
		/// <param name="column">The name of the column.</param>
		/// <param name="whereParams">An arrayList containing pairs (column name and column value) of parameters for the where statement</param>
		/// <returns>The value of the data contained in the column specified.</returns>
		public static string ExtractData(string queryKey, string column, ArrayList whereParams)
		{

			try
			{
				
				//Fernando Pires 2011-01-17
				DataObject dataObject  = (DataObject)hashTable[queryKey];

				//Change by Fernando Pires 2011-01-17
				//DataTable table = hashTable[queryKey] as DataTable;
				DataTable table = dataObject.Data;
				DataRow[] rows = null;
				DataRow row = null;

				//Fernando Pires 2012-01-16
				//Variables to handle check of default value
				string defaultValue = "#";
				string[] columnArr = column.Split('=');

				//Fernando Pires 2012-01-16
				//Check if refCol contains reference to default return value
				if (columnArr.Length > 1)
				{
					defaultValue = columnArr[1]; //Get default value
					column = columnArr[0]; //Get column name
				}
			
				//Check if the column name to return actually exist in the table.
				if (!table.Columns.Contains(column))
					throw new Exception("The column: " + column + " does not exist in the table.");

				//If the arraylist whereParams exists the functoid is set in the mode where the 
				//whole table is extracted instead of only the row.
				
				if (whereParams.Count > 0)
				{
					for (int i = 0; i < whereParams.Count; i = i + 2)
					{
						//Check if the column name in the arraylist actually exist in the table.
						if (!table.Columns.Contains((string)whereParams[i]))
						{
							throw new Exception("The column: " + whereParams[i] + " does not exist in the table.");
						}
					}
				}

				 if (table.Rows.Count > 0)
				{
					if (whereParams.Count > 0)
					{
						string expression = null;

						for (int i = 0; i < whereParams.Count; i = i + 2)
						{
							if (table.Columns[(string)whereParams[i]].DataType == typeof(String) ||
								table.Columns[(string)whereParams[i]].DataType == typeof(Char) ||
								table.Columns[(string)whereParams[i]].DataType == typeof(DateTime) ||
								table.Columns[(string)whereParams[i]].DataType == typeof(Guid))
							{

								//set correct format if the value is a Guid.
								Guid tmp = new Guid();
								if (GuidTryParse(((string)whereParams[i + 1]), out tmp))
								{
									whereParams[i + 1] = tmp.ToString("D");
								}

								//Attach single quotations to the value
								whereParams[i + 1] = string.Format("'{0}'", whereParams[i + 1]);
							}

							//if we are at the end or there are only 2 parameters there should be no AND
							if (i + 2 >= whereParams.Count || whereParams.Count == 2)
							{
								expression = expression + whereParams[i] + "=" + whereParams[i + 1];
							}
							else
							{
								expression = expression + whereParams[i] + "=" + whereParams[i + 1] + " AND ";
							}
						}

						rows = table.Select(expression);
					}
					else
					{
						rows = table.Select();
					}

					if (rows.Length > 0)
						row = rows[0];
				}

			   
				//Fernando Pires 2012-01-16
				//Change from static #
				if (row == null)
				{
					return defaultValue;
				}

				return row[column].ToString();

			}
			catch (Exception ex)
			{
				throw new Exception("DataWrapper.ExtractData", ex);
			}

		   
		}

		/// <summary>
		/// Converts the string representation of a Guid to its Guid 
		/// equivalent. A return value indicates whether the operation 
		/// succeeded. 
		/// </summary>
		/// <param name="s">A string containing a Guid to convert.</param>
		/// <param name="result">This parameter contains the Guid if it is valid
		/// otherwise it will be empty.</param>
		/// <returns>True/False</returns>
		public static bool GuidTryParse(string s, out Guid result)
		{
			if (s == null)
				throw new ArgumentNullException("s");
			Regex format = new Regex(
				"^[A-Fa-f0-9]{32}$|" +
				"^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
				"^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
			Match match = format.Match(s);

			if (match.Success)
			{
				result = new Guid(s);
				return true;
			}
			else
			{
				result = Guid.Empty;
				return false;
			}
		}

		//Fernando Pires added 2011-01-16
		private static int CacheTimeOutMinutes()
		{
			int cacheTimeOutMinutes = 1;
			
			//Appsettings in .config to hold timeout in minutes.
			object appSettingsValue = ConfigurationManager.AppSettings["Blogical.Shared.Functoids.CacheTimeOut"];

			//Try to parse the value in .config value if it exists
			if (appSettingsValue != null)
			{
				int.TryParse((string)appSettingsValue, out cacheTimeOutMinutes); 
			}

			return cacheTimeOutMinutes;

		}

	 

		/// <summary>
		/// If this query has been executed earlier and searchCache = true then the result is fetched from the cache
		/// otherwise query is executed in the database an result is stored in cache.
		/// </summary>
		/// <param name="connStrKey">The connection-string key to the database.</param>
		/// <param name="searchCache">Boolean flag indicating whether cache shall be searched.</param>
		/// <param name="source">The source that is providing us with data</param>
		/// <param name="columnsAndValues">ArrayList containing filter-column values or procedure parameters.</param>
		/// <returns>The key to the data stored in memory.</returns>
		public static string GetData(string connStrKey, bool searchCache, string source, ArrayList columnsAndValues)
		{
			string queryKey = string.Empty;
			DataObject dataObject = new DataObject();

			connStrKey = ParseConfigConnectionString(connStrKey);

			string query = GetQuery(source, columnsAndValues);
			queryKey = CreateQueryKey(connStrKey, query);
			object cachedValue = null;

			cachedValue = hashTable[queryKey];

			//Fernando Pires added 2011-01-16
			//Checks timeout value for specific queryKey
			if (cachedValue != null)
			{
				dataObject = (DataObject)cachedValue;

				if (dataObject.LoadTime < DateTime.Now)
				{
					cache.Remove(queryKey);
					hashTable.Remove(queryKey);
					cachedValue = null;
				}

			}

			// Check if local hashtable contains key
			if (cachedValue == null)
				{
					// If not, then should we search the global cache for it
					if (searchCache)
					{
						cachedValue = cache[queryKey]; // Try to find it
					}

					// If we couldnt find the value we are looking for
					if (cachedValue == null)
					{
						// Get it from the database
						//Fernando Pires changed 2011-01-16
						dataObject.LoadTime = DateTime.Now.AddMinutes(cacheTimeOutMinutes);
						dataObject.Data = GetValueFromDatabase(connStrKey, query);
						cachedValue = dataObject;

						if (searchCache)
						{
							// Add it to cache
							cache.Add(queryKey,
								cachedValue,

								null,
								System.Web.Caching.Cache.NoAbsoluteExpiration, // Sliding!

								//Fernando Pires 2011-01-16
								//Timeout now comes from AppSetting
								new TimeSpan(0, cacheTimeOutMinutes, 0), // One minute
								System.Web.Caching.CacheItemPriority.Normal,
								CacheItemRemoved);

							System.Diagnostics.Trace.WriteLine("Query :'" + queryKey + "' added to cache.");
						}
					}
					
				   

					hashTable.Add(queryKey, cachedValue);

				}
 
			
			return queryKey;
		   
		}

		internal static string ParseConfigConnectionString(string connStrKey)
		{
			// If the connection string starts with "config://"
			if (connStrKey.ToUpper().StartsWith(CONFIG_MONIKER))
			{
				try
				{
					connStrKey = ConfigurationManager.ConnectionStrings[connStrKey.Substring(9)].ConnectionString;
				}
				catch
				{
					throw new Exception(string.Format("Connection string [{0}] does not exist."));
				}
			}
			return connStrKey;
		}

		/// <summary>
		/// Method that is called when an item is removed from cache, only does logging.
		/// </summary>
		/// <param name="key">Key removed.</param>
		/// <param name="val">Value removed.</param>
		/// <param name="r">Reason for removal.</param>
		public static void CacheItemRemoved(String key, Object val, CacheItemRemovedReason r)
		{
			System.Diagnostics.Trace.WriteLine("Query: " + key + " was removed. Reason: " + r.ToString());
		}

		#endregion
	}
}
