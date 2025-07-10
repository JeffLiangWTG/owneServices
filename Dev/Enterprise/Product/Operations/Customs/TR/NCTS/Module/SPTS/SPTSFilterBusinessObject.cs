using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.NCTS.Module
{
	public class SPTSFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		public static class FilterConstants
		{
			public const string JobNumber = "Job #";
			public const string TransportMode = "Transport Mode";
			public const string RegistrationNumber = "Registration Number";
			public const string RegistrationDate = "Registration Date";
			public const string DepartureCustomsOffice = "Departure Customs Office";
			public const string ArrivalCustomsOffice = "Arrival Customs Office";
			public const string VoyageDate = "Voyage Date";
			public const string VoyageNo = "Voyage No";
			public const string Carrier = "Carrier";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var jobNumberFilter = result.AddTextFilter(FilterConstants.JobNumber, CusInBondHeaderSchema.BH_JobReference).WithMaxLengthOf<ModuleTextFilter>(CusInBondHeaderSchema.BH_JobReference);
			jobNumberFilter.Category = FilterCategories.TextSearch;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|JobNumber", FilterConstants.JobNumber);

			var tansportModeFilter = result.AddTextFilter(FilterConstants.TransportMode, GetTransportModeQuery, Factory.GetCachedValue<SPTSTransportModeList>()).WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_InlandTransportMode);
			tansportModeFilter.Category = FilterCategories.ModesAndTypes;
			tansportModeFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|TransportMode", FilterConstants.TransportMode);

			var registrationNumberFilter = result.AddTextFilter(FilterConstants.RegistrationNumber, GetRegistrationNumberQuery).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			registrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|RegistrationNumber", FilterConstants.RegistrationNumber);

			var registrationDateFilter = result.AddDateFilter(FilterConstants.RegistrationDate, GetRegistrationDateQuery);
			registrationDateFilter.Category = FilterCategories.Dates;
			registrationDateFilter.MaxLength = CusEntryNumSchema.CE_IssueDate.MaxLength;
			registrationDateFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|RegistrationDate", FilterConstants.RegistrationDate);

			var departureCustomsOfficeFilter = result.AddTextFilter(FilterConstants.DepartureCustomsOffice, GetDepartureCustomsOfficeQuery);
			departureCustomsOfficeFilter.Category = FilterCategories.Locations;
			departureCustomsOfficeFilter.MaxLength = CusInBondMoveHeaderSchema.BM_PortOfPresentationCode.MaxLength;
			departureCustomsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|DepartureCustomsOffice", FilterConstants.DepartureCustomsOffice);

			var arrivalCustomsOfficeFilter = result.AddTextFilter(FilterConstants.ArrivalCustomsOffice, GetArrivalCustomsOfficeQuery);
			arrivalCustomsOfficeFilter.Category = FilterCategories.Locations;
			arrivalCustomsOfficeFilter.MaxLength = CusInBondMoveHeaderSchema.BM_DestinationPortCode.MaxLength;
			arrivalCustomsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|ArrivalCustomsOffice", FilterConstants.ArrivalCustomsOffice);

			var voyageDateFilter = result.AddDateFilter(FilterConstants.VoyageDate, CusInBondHeaderSchema.BH_SailingDate);
			voyageDateFilter.Category = FilterCategories.Dates;
			voyageDateFilter.MaxLength = CusInBondHeaderSchema.BH_SailingDate.MaxLength;
			voyageDateFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|VoyageDate", FilterConstants.VoyageDate);

			var voyageNoFilter = result.AddTextFilter(FilterConstants.VoyageNo, CusInBondHeaderSchema.BH_VoyageNumber);
			voyageNoFilter.Category = FilterCategories.NumbersAndReferences;
			voyageNoFilter.MaxLength = CusInBondHeaderSchema.BH_VoyageNumber.MaxLength;
			voyageNoFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|VoyageNo", FilterConstants.VoyageNo);

			var carrierFilter = result.AddGuidFilter(FilterConstants.Carrier, ModuleIDs.Organisation, GetCarrierQuery, ShippingProviders);
			carrierFilter.Category = FilterCategories.NumbersAndReferences;
			carrierFilter.MaxLength = CusInBondMoveHeaderSchema.BM_OA_InBondCarrier.MaxLength;
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("SPTSFilter|Carrier", FilterConstants.Carrier);

			return result;
		}

		ZQuery GetTransportModeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(SPTSHeader));
			var moveHeaderQuery = GetQueryByCusInBondMoveHeader();
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_InlandTransportMode, comparisonOperator, value);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetRegistrationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(SPTSHeader));

			bool notIn = false;
			if (comparisonOperator != null)
			{
				if (comparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					notIn = true;
				}
			}

			var subQuery = new ZDBOnlySubQuery(typeof(Common.CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);

			if (!value.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			}

			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Common.CusInBondApplicationCodeList.Codes.TRSPTS);
			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, "TR");
			headerQuery.AddSubQuery(CusInBondHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);

			return headerQuery;
		}

		ZQuery GetRegistrationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(SPTSHeader));

			bool notIn = false;
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				notIn = true;
			}

			var subQuery = new ZDBOnlySubQuery(typeof(Common.CusEntryNumber), CusEntryNumSchema.CE_IssueDate, notIn);

			if (!value1.IsEmpty || !value2.IsEmpty || comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);
			}
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Common.CusInBondApplicationCodeList.Codes.TRSPTS);
			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, "TR");

			headerQuery.AddSubQuery(CusInBondHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, subQuery, JoinCondition.And);

			return headerQuery;
		}

		ZQuery GetDepartureCustomsOfficeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(SPTSHeader));
			var moveHeaderQuery = GetQueryByCusInBondMoveHeader();
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_PortOfPresentationCode, comparisonOperator, value);

			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlySubQuery GetQueryByCusInBondMoveHeader()
		{
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_SubApplicationCode, "D");
			return moveHeaderQuery;
		}

		ZQuery GetArrivalCustomsOfficeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(SPTSHeader));
			var moveHeaderQuery = GetQueryByCusInBondMoveHeader();
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_DestinationPortCode, comparisonOperator, value);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetCarrierQuery(ZGuid importerPK)
		{
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, importerPK);

			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = GetQueryByCusInBondMoveHeader();
			moveHeaderQuery.AddSubQuery(CusInBondMoveHeaderSchema.BM_OA_InBondCarrier, addressQuery, JoinCondition.And);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			return result;
		}

		public ShippingProviderCollection ShippingProviders
		{
			get { return new ShippingProviderCollection(Factory); }
		}
	}
}

