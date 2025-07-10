using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module
{
	/// <summary>
	/// Module FilterBusinessObject for NZClassification.
	/// Override validation and filter SQL generation here.
	/// </summary>
	public class CusClassificationFilterBusinessObject : Customs.Module.CusClassificationFilterBusinessObject
	{
		public CusClassificationFilterBusinessObject()
		{
		}

		#region Filters

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "result is not needed")]
		protected override void AddCustomFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Other Info", OtherInfoQuery, OtherInfoList)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.CusClassificationFilterBusinessObject|OtherInfo", "Other Info");
			filters.AddTextFilter("Permit Code", PermitCodeQuery, PermitCodeList)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.CusClassificationFilterBusinessObject|PermitCode", "Permit Code");
			filters.AddTextFilter("Prohibited Code", ProhibitedCodeQuery, ProhibitedCodeList)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.CusClassificationFilterBusinessObject|ProhibitedCode", "Prohibited Code");

			var concessionCodeFilter = GetAddInfoNkExactFilter("Concession Code", ModuleIDs.Customs.NZ.Concession, ConcessionCollection, Business.NZAddInfo.Schema.ZN_ConcessionCode.Substring(3));
			concessionCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.CusClassificationFilterBusinessObject|ConcessionCode", "Concession Code");
			filters.AddFilter(concessionCodeFilter);
		}

		#endregion

		#region Lists

		public CodeDescriptionPairList PermitCodeList
		{
			get
			{
				return fPermitCodeList ??= new Business.PermitCodeList();
			}
		}

		CodeDescriptionPairList fPermitCodeList;

		public CodeDescriptionPairList ProhibitedCodeList
		{
			get
			{
				return fProhibitedCodeList ??= new Business.ProhibitedCodeList();
			}
		}

		CodeDescriptionPairList fProhibitedCodeList;

		public CodeDescriptionPairList OtherInfoList
		{
			get
			{
				return fOtherInfoList ??= new Business.LineOtherInfoList();
			}
		}

		CodeDescriptionPairList fOtherInfoList;

		#endregion

		#region ConcessionCollection

		BusinessObjectCollection ConcessionCollection
		{
			get
			{
				return fConcessionCollection ??= new NonDependentNZCConcessionCollection(Factory);
			}
		}

		BusinessObjectCollection fConcessionCollection;

		#endregion

		#region Queries

		ZQuery OtherInfoQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				AddInfoQuery(query, value, CusClassification.Schema.CC_OtherInfos.Substring(3));
			}
			return query;
		}

		ZQuery PermitCodeQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				AddInfoQuery(query, value, CusClassification.Schema.CC_PermitCodes.Substring(3));
			}
			return query;
		}

		ZQuery ProhibitedCodeQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				AddInfoQuery(query, value, CusClassification.Schema.CC_ProhibitedCodes.Substring(3));
			}
			return query;
		}

		#endregion

		#region Tariff Formatter

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return new NZTariffFormatter();
		}

		#endregion
	}
}
