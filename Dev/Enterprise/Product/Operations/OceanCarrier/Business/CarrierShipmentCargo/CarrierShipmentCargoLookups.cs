using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierShipmentCargoLookups : AutoCarrierShipmentCargoLookups
	{
		public CarrierShipmentCargoLookups(AutoCarrierShipmentCargo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList SealParty_List => GetCachedValue_SealParty_List(Factory);

		public static CodeDescriptionPairList GetCachedValue_SealParty_List(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CommonContainer.Lookups.SealParty_List",
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

		public CodeDescriptionPairList GrossWeightVerificationTypeList
		{
			get
			{
				return Factory.GetCachedValue("CommonContainer.Lookups.GrossWeightVerificationTypeList",
					delegate
					{
						var result = new CodeDescriptionPairList
						{
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotVerified),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotRequired),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method1Container),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method2Packages),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.WeightAtTerminal),
							new CodeDescriptionPair(Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod,
								Constants.ContainerGrossWeightVerificationTypes.Descriptions.RationalMethod)
						};

						return result;
					});
			}
		}
	}
}
