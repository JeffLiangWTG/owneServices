using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Packing.Business
{
	public class PkgPackageContainerLookups : AutoPkgPackageContainerLookups
	{
		public PkgPackageContainerLookups(AutoPkgPackageContainer parent)
			: base(parent) { }

		#region ContainerModes

		public CodeDescriptionPairList ContainerModes
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.ContainerMode); }
		}

		#endregion

		#region ContainerStatuses

		public ReadOnlyCodeDescriptionPairList ContainerStatuses
		{
			get { return FreightDataRegistry.Instance.ContainerStatusList.Value; }
		}

		#endregion

		#region ContainerQualities

		public ReadOnlyCodeDescriptionPairList ContainerQualities
		{
			get { return FreightDataRegistry.Instance.ContainerQualityList.Value; }
		}

		#endregion

		#region AirVentFlowRateUnits

		public CodeDescriptionPairList AirVentFlowRateUnits
		{
			get { return Factory.GetCachedValue("PkgPackageContainerLookups|AirVentFlowRateUnits", () => GetAirVentFlowRateUnits()); }
		}

		static CodeDescriptionPairList GetAirVentFlowRateUnits()
		{
			var airVentFlowRateUnits = new CodeDescriptionPairList();
			airVentFlowRateUnits.AddPair("2L", Res.GetString("6dc605c5-99e1-432f-80b1-de9ad2fb514b", "Cubic feet per minute"));
			airVentFlowRateUnits.AddPair("MQH", Res.GetString("c89b2406-25d6-4b9a-b0cd-ec922f61c123", "Cubic meters per hour"));
			airVentFlowRateUnits.AddPair("P1", Res.GetString("da335b72-5f0d-4100-8871-03a6dbf2324b", "Percent"));
			return airVentFlowRateUnits;
		}

		#endregion

		#region SealPartyList

		public CodeDescriptionPairList SealParty_List => GetCachedValue_SealParty_List(Factory);

		public static CodeDescriptionPairList GetCachedValue_SealParty_List(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PkgPackageContainer.Lookups.SealParty_List",
				delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Constants.ContainerSealParties.Codes.CarrierShippingLine, Constants.ContainerSealParties.Descriptions.CarrierShippingLine);
					result.AddPair(Constants.ContainerSealParties.Codes.ConsignorShipper, Constants.ContainerSealParties.Descriptions.ConsignorShipper);
					result.AddPair(Constants.ContainerSealParties.Codes.Customs, Constants.ContainerSealParties.Descriptions.Customs);
					result.AddPair(Constants.ContainerSealParties.Codes.Quarantine, Constants.ContainerSealParties.Descriptions.Quarantine);
					result.AddPair(Constants.ContainerSealParties.Codes.Terminal, Constants.ContainerSealParties.Descriptions.Terminal);
					return result;
				});
		}

		#endregion
	}
}
