using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCTariffVersionLoader
	{
		public NZCTariffVersionLoader(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public ZString ErrorMessage
		{
			get
			{
				if (errorMessage == null)
				{
					try
					{
						int dataVersion = GetDataVersion();

						errorMessage = (dataVersion >= MinimumDataVersionRequired)
							? ""
							: String.Format(
								CultureInfo.InvariantCulture,
								"NZ Tariff minimum data version [{0}] requirement not met. The current Tariff data version is [{1}].\r\nPlease update your Tariff data by running ediTariff -> " + Core.Constants.ProductName + " -> Export NZ data to " + Core.Constants.ProductName + ".",
								MinimumDataVersionRequired,
								dataVersion);
					}
					catch (SqlException ex)
					{
						errorMessage = ex.Message;
					}
				}

				return errorMessage;
			}
		}
		string errorMessage;

		protected
#if DEBUG
		virtual
#endif
		string SelectVersionSql
		{
			get
			{
				return @"SELECT TOP 1 CONVERT(int, CONVERT(nvarchar(10), CONVERT(varbinary(max), SD_BinaryValue)))
						 FROM [{0}]..StmData
						 WHERE SD_Name = 'DATABASE_DATA_VERSION'";
			}
		}

		int GetDataVersion()
		{
			int result = 0;
			var connection = factory.Connection;
			string dbName = ((IPhysicalRefDbLocation)factory.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Tariff, "NZ");

			if (!String.IsNullOrWhiteSpace(dbName))
			{
				string sqlText = String.Format(CultureInfo.InvariantCulture, SelectVersionSql, dbName);

				try
				{
					using (var command = connection.Command(sqlText))
					{
						result = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
					}
				}
				catch (SqlException ex)
				{
					bool isInvalidObjectStmDataError = (
						new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName
						&& Regex.IsMatch(ex.Message, @"\bStmData\b", RegexOptions.IgnoreCase)
						&& connection.DatabaseExists(dbName));

					if (isInvalidObjectStmDataError)
					{
						// If StmData table does not exist
						result = 0;
					}
					else
					{
						throw;
					}
				}
			}

			return result;
		}

		public const int MinimumDataVersionRequired = 1857;
		readonly IDbConnected factory;
	}
}
