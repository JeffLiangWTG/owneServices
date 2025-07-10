using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[SingleObjectAroundARow]
	public class USCQuota : AutoUSCQuota
	{
		public USCQuota(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Description Properties

		public ZString PeriodProcessDateIndicatorDesc
		{
			get
			{
				return Lookups.PeriodProcessDateIndicatorList.GetDescriptionFromCode(UT_PeriodProcessDateIndicator) ?? string.Empty;
			}
		}

		public ZPropertyInfo PeriodProcessDateIndicatorDescInfo
		{
			get { return GetZPropertyInfo(nameof(PeriodProcessDateIndicatorDesc)); }
		}

		public ZString QuotaLimitTypeDesc
		{
			get
			{
				return Lookups.QuotaLimitTypeList.GetDescriptionFromCode(UT_QuotaLimitType) ?? string.Empty;
			}
		}

		public ZPropertyInfo QuotaLimitTypeDescInfo
		{
			get { return GetZPropertyInfo(nameof(QuotaLimitTypeDesc)); }
		}

		public ZString QuotaStatusDesc
		{
			get
			{
				return Lookups.QuotaStatusList.GetDescriptionFromCode(UT_QuotaStatus) ?? string.Empty;
			}
		}

		public ZPropertyInfo QuotaStatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(QuotaStatusDesc)); }
		}

		public ZString QuotaTypeDesc
		{
			get
			{
				return Lookups.QuotaTypeList.GetDescriptionFromCode(UT_QuotaType) ?? string.Empty;
			}
		}

		public ZPropertyInfo QuotaTypeDescInfo
		{
			get { return GetZPropertyInfo(nameof(QuotaTypeDesc)); }
		}

		#endregion

		#region Loader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoUSCQuota.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public USCQuota Load(ZString code, ZString country, ZString firstNamesake, ZString secondNamesake, ZDateTime beginDate, ZDateTime endDate)
			{
				return (USCQuota)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), GetQueryFor(code, country, firstNamesake, secondNamesake, beginDate, endDate));
			}

			public USCQuota[] LoadAll(ZString code, ZString country, ZString firstNamesake, ZString secondNamesake, ZDateTime beginDate, ZDateTime endDate)
			{
				return (USCQuota[])Factory.Load(GetTypeOfBusinessObjectToLoad(), GetQueryFor(code, country, firstNamesake, secondNamesake, beginDate, endDate));
			}

			public ZQuery GetBestMatchQueryFor(ZString tariffNumber, ZString secondTariffNumber, ZString country, ZDateTime presentationDate, ZDateTime exportDate)
			{
				ZQuery query = GetQueryFor(tariffNumber, country, "", "");
				query.AddToFilter(USCQuotaSchema.UT_SecondTariffNo, secondTariffNumber);

				ZQuery dateQuery = new ZQuery();

				if (presentationDate.IsValid)
				{
					dateQuery.AddToFilter(GetQueryForBestMatch(presentationDate, PeriodProcessingDateIndicatorList.Codes.PresentationDate), JoinCondition.Or);
				}

				if (exportDate.IsValid && presentationDate != exportDate)
				{
					dateQuery.AddToFilter(GetQueryForBestMatch(exportDate, PeriodProcessingDateIndicatorList.Codes.ExportDate), JoinCondition.Or);
				}

				query.AddToFilter(dateQuery);

				return query;
			}

			public USCQuota LoadBestMatchFor(ZString tariffNumber, ZString secondTariffNumber, ZString country, ZDateTime presentationDate, ZDateTime exportDate)
			{
				return (USCQuota)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), GetBestMatchQueryFor(tariffNumber, secondTariffNumber, country, presentationDate, exportDate));
			}

			ZQuery GetQueryFor(ZString code, ZString country, ZString firstNamesake, ZString secondNamesake, ZDateTime beginDate, ZDateTime endDate)
			{
				ZQuery query = GetQueryFor(code, country, firstNamesake, secondNamesake);
				query.AddToFilter(USCQuotaSchema.UT_BeginDate, beginDate);
				query.AddToFilter(USCQuotaSchema.UT_EndDate, endDate);
				return query;
			}

			ZQuery GetQueryFor(ZString code, ZString country, ZString firstNamesake, ZString secondNamesake)
			{
				ZQuery result = new ZQuery(USCQuotaSchema.UT_Code, code);

				if (!firstNamesake.IsEmpty)
				{
					result.AddToFilter(USCQuotaSchema.UT_FirstNamesake, firstNamesake);
				}

				if (!secondNamesake.IsEmpty)
				{
					result.AddToFilter(USCQuotaSchema.UT_SecondNamesake, secondNamesake);
				}

				ZQuery countryQuery = new ZQuery(USCQuotaSchema.UT_UC_NKOriginCountry, ZString.Empty);
				countryQuery.AddToFilter(JoinCondition.Or, USCQuotaSchema.UT_UC_NKOriginCountry, country);

				result.AddToFilter(countryQuery, JoinCondition.And);

				return result;
			}

			ZQuery GetQueryForBestMatch(ZDateTime date, ZString periodDateIndicator)
			{
				ZQuery result = new ZQuery(USCQuotaSchema.UT_PeriodProcessDateIndicator, periodDateIndicator);
				result.AddToFilter(JoinCondition.And, USCQuotaSchema.UT_BeginDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
				result.AddToFilter(JoinCondition.And, USCQuotaSchema.UT_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(USCQuota);
			}
		}

		#endregion
	}
}
