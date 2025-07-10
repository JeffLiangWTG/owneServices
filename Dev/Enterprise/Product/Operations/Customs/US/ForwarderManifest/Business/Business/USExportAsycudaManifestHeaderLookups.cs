using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaManifestHeaderLookups : AsycudaManifestHeaderLookups
	{
		public USExportAsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList TransportModeList
		{
			get
			{
				return Factory.GetCachedValue("USExportAsycudaManifestHeaderLookups.TransportTypeList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(TransportTypeList.Codes.Air, TransportTypeList.Descriptions.Air);
					result.AddPair(TransportTypeList.Codes.Sea, TransportTypeList.Descriptions.Sea);
					result.AddPair(TransportTypeList.Codes.Rail, TransportTypeList.Descriptions.Rail);
					return result;
				});
			}
		}

		protected override ICollection GetCustomsLoadingPortListCore()
		{
			var customsLoadingPorts = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			customsLoadingPorts.FilterBusinessObjectDefaults.Add(new CargoWise.EntityFramework.FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(Core.Constants.CountryCodes.UnitedStates)));
			customsLoadingPorts.FilterBusinessObjectDefaults.Add(new CargoWise.EntityFramework.FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeName, "Property", new ZString(RefCusCodeListAttributeTypes.Codes.ROLE)));
			customsLoadingPorts.FilterBusinessObjectDefaults.Add(new CargoWise.EntityFramework.FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.AttributeValue, "Property", new ZString("EXP")));
			return customsLoadingPorts;
		}

		public ZZRefCusCodeListCombinedCollection CustomsFirstArrivalPortList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (USExportAsycudaManifestHeader)Parent;
				if (parent.AMA_CustomsFirstArrivalPortIsDropEdit)
				{
					result = parent.PortOfFirstArrivalRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, ZDateTime.Today);
			}
		}

		public ZZRefCusCodeListCombinedCollection CustomsFinalDeparturePortList
		{
			get
			{
				ZZRefCusCodeListCombinedCollection result = null;
				var parent = (USExportAsycudaManifestHeader)Parent;
				if (parent.AMA_CustomsFinalDeparturePortIsDropEdit)
				{
					result = parent.PortOfFinalDepartureRefLocoMappings as ZZRefCusCodeListCombinedCollection;
				}
				return result ?? ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
			}
		}
	}
}
