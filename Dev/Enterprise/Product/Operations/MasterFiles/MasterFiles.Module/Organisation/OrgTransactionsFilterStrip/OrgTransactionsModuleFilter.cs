using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	class OrgTransactionsModuleFilter : ModuleDateFilter
	{
		#region Construction

		public OrgTransactionsModuleFilter(ZString description)
			: base(description, delegate
			{ return new ZQuery(); })
		{
		}

		#endregion

		#region NumberOfTransactions

		public ZInt NumberOfTransactions
		{
			get { return numberOfTransactions; }
			set
			{
				numberOfTransactions = value;
				NumberOfTransactionsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NumberOfTransactionsInfo
		{
			get { return GetZPropertyInfo(nameof(NumberOfTransactions)); }
		}

		ZInt numberOfTransactions = 1;

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			NumberOfTransactions = 1;
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			ZQuery query = new ZQuery();

			if (!FromDate.IsEmpty && !ToDate.IsEmpty && !NumberOfTransactions.IsEmpty)
			{
				query.AddToFilter(GetOrgTransactionsQuery(FromDate, ToDate, NumberOfTransactions));
			}
			return query;
		}

		ZDBOnlyQuery GetOrgTransactionsQuery(ZDateTime dateFrom, ZDateTime dateTo, ZInt numberOfInvoices)
		{
			ZDBOnlyQuery mainQuery = new ZDBOnlyQuery(typeof(OrgHeader));

			ZString filterString = "( Select " + " count(*) from " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName
									+ " where " + OrgHeader.Schema.PK + " = " + AccTransactionHeader.Schema.AH_OH
									+ " and " + AccTransactionHeader.Schema.AH_PostDate + " > " + " @dateFrom and "
									+ AccTransactionHeader.Schema.AH_PostDate + " < @dateTo " + ") >= " + numberOfInvoices;

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();

			parameters.Add("@dateFrom", dateFrom, AccTransactionHeaderSchema.AH_PostDate);
			parameters.Add("@dateTo", dateTo, AccTransactionHeaderSchema.AH_PostDate);

			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_OH);
			subQuery.AddFilterAndZSQLParameterCollection(filterString, parameters);
			mainQuery.AddSubQuery(subQuery, JoinCondition.And);

			return mainQuery;
		}

		#endregion

		#region XML Serialization

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == "NumberOfTransactions")
			{
				NumberOfTransactions = ZInt.ParseSafe(reader.ReadElementString("NumberOfTransactions"), 0);
			}
		}

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("NumberOfTransactions", NumberOfTransactions.ToString());
		}

		#endregion
	}
}
