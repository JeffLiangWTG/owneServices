using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module
{
	public class CO2eStatusAndCO2eKgRangeNumberFilter : ModuleNumberRangeFilter
	{
		public CO2eStatusAndCO2eKgRangeNumberFilter(ZString description, Type businessObjectType)
		: base(description, delegate { return new ZQuery(); })
		{
			DefaultCO2eStatus = Common.Business.CO2eStatusList.Codes.Current;
			this.businessObjectType = businessObjectType;
			QueryDelegate = GetCO2eNumberRangeQuery;
			Decimals = 3;
		}

		#region Properties

		readonly Type businessObjectType;

		string ParentTableCode => businessObjectType == null ? string.Empty : ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(BusinessObjectFactory.GetTableNameFromType(businessObjectType));

		public CodeDescriptionPairList CO2eStatusList => co2eStatusList ?? (co2eStatusList = new CO2eStatusList());
		CodeDescriptionPairList co2eStatusList;

		ZString defaultCO2eStatus;

		public ZString DefaultCO2eStatus
		{
			get
			{
				return defaultCO2eStatus;
			}
			set
			{
				defaultCO2eStatus = value;
				CO2eStatus = value;
			}
		}

		ZString co2eStatus;

		[List("CO2eStatusList")]
		public ZString CO2eStatus
		{
			get
			{
				return co2eStatus;
			}
			set
			{
				if (SetNonPersistentPropertyValue(CO2eStatusInfo, ref co2eStatus, value))
				{
					CO2eStatusInfo.RefreshBinding();
				}

				if (co2eStatus != Common.Business.CO2eStatusList.Codes.Current)
				{
					ResetPropertyValue();
				}
			}
		}

		public ZPropertyInfo CO2eStatusInfo => GetZPropertyInfo(Constants.CO2eStatus);

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			var query = new ZQuery();
			if (businessObjectType == null || CO2eStatus.IsEmpty)
			{
				query.IsNoResultQuery = true;
			}
			else if (CO2eStatus == Common.Business.CO2eStatusList.Codes.Current)
			{
				query.AddToFilter(base.GetQuery());
			}
			else
			{
				query.AddToFilter(GetCO2eStatusQuery(CO2eStatus));
			}
			return query;
		}

		ZQuery GetCO2eNumberRangeQuery(INumericZType value1, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(businessObjectType);

			var subQuery = new ZDBOnlySubQuery(typeof(IJobCO2e), JobCO2eSchema.JCO_ParentID);
			subQuery.AddToFilter(JobCO2eSchema.JCO_ParentTableCode, ParentTableCode);
			subQuery.AddToFilter(JobCO2eSchema.JCO_Status, Common.Business.CO2eStatusList.Codes.Current);
			var sqlFilter = string.Format(CultureInfo.InvariantCulture, @"ROUND(JCO_TotalCO2e, 3) BETWEEN {0} AND {1}", value1.ToString(), value2.ToString());
			subQuery.AddFilterAndZSQLParameterCollection(sqlFilter, new ZSqlParameterCollection());

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetCO2eStatusQuery(ZString status)
		{
			var result = new ZDBOnlyQuery(businessObjectType);

			var filterByNotCalculated = status == Common.Business.CO2eStatusList.Codes.NotCalculated;
			var subQuery = new ZDBOnlySubQuery(typeof(IJobCO2e), JobCO2eSchema.JCO_ParentID, filterByNotCalculated);
			subQuery.AddToFilter(JobCO2eSchema.JCO_ParentTableCode, ParentTableCode);
			if (!filterByNotCalculated)
			{
				subQuery.AddToFilter(JobCO2eSchema.JCO_Status, status);
			}

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region Implementation

		protected override void ClearCore()
		{
			base.ClearCore();
			CO2eStatus = DefaultCO2eStatus;
		}

		#endregion

		static class Constants
		{
			public const string CO2eStatus = "CO2eStatus";
		}

		#region XML Serialization

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			if (reader.Name == Constants.CO2eStatus)
			{
				CO2eStatus = reader.ReadElementContentAsString();
			}

			base.DeserializePropertiesFromXml(reader);
		}

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString(Constants.CO2eStatus, CO2eStatus);
			base.SerializePropertiesToXml(writer);
		}

		#endregion
	}
}
