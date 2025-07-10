using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using JobMessageTypeList = Enterprise.Customs.NZ.Business.JobMessageTypeList;

namespace Enterprise.Customs.NZ.Module.Declaration
{
	public class ECIWriteOffManifestingFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddNumberFilter("Manifest Number", CusEntryHeaderSchema.CH_BGMReference)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|ManifestNumber", "Manifest Number");
			filters.AddNumberFilter("ECI Entry Number", GetEntryNumberQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|ECIEntryNumber", "ECI Entry Number");
			filters.AddNumberFilter("Master Bill", GetMasterBillQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobDeclarationSchema.JE_MasterBill)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|MasterBill", "Master Bill");
			filters.AddNumberFilter("Flight Number", GetVoyageFlightQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobDeclarationSchema.JE_VoyageFlightNo)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|FlightNumber", "Flight Number");

			filters.AddTextFilter("Entry Type", GetEntryTypeQuery, MessageTypeList)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|EntryType", "Entry Type");
			filters.AddTextFilter("Entry Status", CusEntryHeaderSchema.CH_EntryStatus, EntryStatusList)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|EntryStatus", "Entry Status");
			filters.AddTextFilter("Message Mode", GetMessageModeQuery, MessageModeList)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|MessageMode", "Message Mode");

			filters.AddDateFilter("Local Transfer Date", GetBarrierDateFilter)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|LocalTransferDate", "Local Transfer Date");

			filters.AddGuidFilter("Carrier", ModuleIDs.Organisation, GetCarrierQuery, CarrierList)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|Carrier", "Carrier");
			var localTransferPortFilter = filters.AddNkFilter("Local Transfer Port", GetBarrierPortFilter, ModuleIDs.RefUNLOCO, LocalPortList);
			localTransferPortFilter.Category = FilterCategories.Locations;
			localTransferPortFilter.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|LocalTransferPort", "Local Transfer Port");

			ModuleTextFilter eCIFilter = filters.AddTextFilter("ECI Message Type", CusEntryHeaderSchema.CH_MessageType);
			eCIFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			eCIFilter.Property = CusEntryHeader.EntryHeaderTypes.NZ.ECIWriteOffManifest;
			eCIFilter.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.ECIWriteOffManifestingFilterBusinessObject|ECIMessageType", "ECI Message Type");

			return filters;
		}

		ZString EntryType
		{
			get { return ((ModuleTextFilter)this["Entry Type"]).Property; }
		}

		#region Dates

		ZQuery GetBarrierDateFilter(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			ZDBOnlySubQuery decSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.JobDeclaration), CusEntryHeaderSchema.CH_JE);

			ZQuery decFilter = new ZQuery();
			if (EntryType == JobMessageTypeList.Codes.Import)
			{
				AddDateRange(decFilter, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_DateOfArrival, date1.Date, date2.Date);
			}
			else if (EntryType == JobMessageTypeList.Codes.Export)
			{
				AddDateRange(decFilter, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_ExportDate, date1.Date, date2.Date);
			}
			else
			{
				ZQuery importFilter = new ZQuery(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
				AddDateRange(importFilter, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_DateOfArrival, date1.Date, date2.Date);

				ZQuery exportFilter = new ZQuery(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);
				AddDateRange(exportFilter, comparisonOperator, JoinCondition.And, JobDeclarationSchema.JE_ExportDate, date1.Date, date2.Date);

				decFilter.AddToFilter(new ZQuery(importFilter, JoinCondition.Or, exportFilter));
			}

			decSubQuery.AddToFilter(decFilter);
			query.AddSubQuery(decSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Locations

		ZQuery GetBarrierPortFilter(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			ZDBOnlySubQuery decSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.JobDeclaration), CusEntryHeaderSchema.CH_JE);

			ZQuery decFilter = new ZQuery();
			if (EntryType == JobMessageTypeList.Codes.Import)
			{
				decFilter.AddToFilter(JobDeclarationSchema.JE_RL_NKPortOfArrival, value);
			}
			else if (EntryType == JobMessageTypeList.Codes.Export)
			{
				decFilter.AddToFilter(JobDeclarationSchema.JE_RL_NKPortOfLoading, value);
			}
			else
			{
				ZQuery importFilter = new ZQuery(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
				importFilter.AddToFilter(JobDeclarationSchema.JE_RL_NKPortOfArrival, value);

				ZQuery exportFilter = new ZQuery(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);
				exportFilter.AddToFilter(JobDeclarationSchema.JE_RL_NKPortOfLoading, value);

				decFilter.AddToFilter(new ZQuery(importFilter, JoinCondition.Or, exportFilter));
			}

			decSubQuery.AddToFilter(decFilter);
			query.AddSubQuery(decSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Numbers

		ZQuery GetEntryNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, @operator, value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetEntryTypeQuery(ZString value)
		{
			return GetDeclarationSimpleQuery(JobDeclarationSchema.JE_MessageType, value);
		}

		ZQuery GetMessageModeQuery(ZString value)
		{
			return GetDeclarationSimpleQuery(JobDeclarationSchema.JE_ApplicationCode, value);
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetDeclarationSimpleQuery(JobDeclarationSchema.JE_MasterBill, @operator, value);
		}

		ZQuery GetVoyageFlightQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetDeclarationSimpleQuery(JobDeclarationSchema.JE_VoyageFlightNo, @operator, value);
		}

		ZQuery GetCarrierQuery(ZGuid value)
		{
			return GetDeclarationSimpleQuery(JobDeclarationSchema.JE_OH_ShippingLine, value);
		}

		ZQuery GetDeclarationSimpleQuery(SchemaColumn decSchemaColumn, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			ZDBOnlySubQuery decSubQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.JobDeclaration), CusEntryHeaderSchema.CH_JE);
			decSubQuery.AddToFilter_PossiblyCommaSeparated(decSchemaColumn, @operator, value);
			query.AddSubQuery(decSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetDeclarationSimpleQuery(SchemaColumn decSchemaColumn, object value)
		{
			return GetDeclarationSimpleQuery(decSchemaColumn, SQLComparisonOperator.Equal, value);
		}

		#endregion

		#region Lookups

		#region MessageTypeList

		CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<JobMessageTypeList>();

		#endregion

		public CodeDescriptionPairList MessageModeList
		{
			get { return Factory.GetCachedValue<JobApplicationCodeList>(); }
		}

		#region EntryStatusList

		CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<LowValueManifestStatusList>();

		#endregion

		#region LocalPortList

		RefUNLOCOCollection LocalPortList
		{
			get
			{
				if (fLocalPortList == null)
				{
					fLocalPortList = new RefUNLOCOCollection(Factory, new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				}
				return fLocalPortList;
			}
		}
		RefUNLOCOCollection fLocalPortList;

		#endregion

		#region CarrierList

		ShippingProviderCollection CarrierList
		{
			get
			{
				if (fCarrierList == null)
				{
					fCarrierList = new ShippingProviderCollection(Factory);
				}
				return fCarrierList;
			}
		}
		ShippingProviderCollection fCarrierList;

		#endregion

		#endregion
	}
}
