using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public USExportAsycudaBillLookups(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		public CodeDescriptionPairList InlandTransportMode => Factory.GetCachedValue("USExportAsycudaBillLookups.PriorTransportationModeList", () => new PriorTransportationModeList());
		public CodeDescriptionPairList BillOfLadingType => Factory.GetCachedValue("USExportAsycudaBillLookups.BillOfLadingTypeList", () => new BillOfLadingTypeList());

		public override IBusinessObjectCollection BillIssuers
		{
			get { return usCarrierList ?? (usCarrierList = new USCarrierCombinedCollection(Factory)); }
		}
		USCarrierCombinedCollection usCarrierList;

		public CodeDescriptionPairList ManifestUQList
		{
			get
			{
				var date = ZDateTime.Today;
				return Factory.GetCachedValue("ManifestUQList" + date, () =>
				{
					var list = new CodeDescriptionPairList();
					var codes = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.USExportManifestUOM, date);
					list.AddRange(codes);
					list.Sort();
					return list;
				});
			}
		}

		public ZZRefCusCodeListCombinedCollection CustomsLoadPortList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (USExportAsycudaBill)Parent;
				if (parent.ABL_CustomsLoadPortIsDropEdit)
				{
					result = parent.PortOfLadingRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			}
		}

		public ZZRefCusCodeListCombinedCollection CustomsOriginPortList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (USExportAsycudaBill)Parent;
				if (parent.ABL_CustomsOriginPortIsDropEdit)
				{
					result = parent.PortOfOriginRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			}
		}

		public ZZRefCusCodeListCombinedCollection CustomsPortOfUnladingList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (USExportAsycudaBill)Parent;
				if (parent.ABL_CustomsDischargePortIsDropEdit)
				{
					result = parent.PortOfUnladingRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			}
		}

		public ZZRefCusCodeListCombinedCollection CustomsFinalDestinationPortList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (USExportAsycudaBill)Parent;
				if (parent.ABL_CustomsFinalDestinationPortIsDropEdit)
				{
					result = parent.PortOfFinalDestinationRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			}
		}
	}
}
