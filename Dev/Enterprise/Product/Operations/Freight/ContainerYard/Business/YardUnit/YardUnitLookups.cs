using CargoWise.Integration;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class YardUnitLookups : AutoYardUnitLookups
	{
		public YardUnitLookups(AutoYardUnit parent)
			: base(parent)
		{
		}

		#region Locations

		public WhsLocationCollection Locations
		{
			get
			{
				return new WhsLocationCollection(Factory);
			}
		}

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses
		{
			get
			{
				return new WhsWarehouseCollectionWithSecurityCheck(Factory);
			}
		}

		#endregion

		#region Qualities

		public ReadOnlyCodeDescriptionPairList Qualities
		{
			get { return AgencyRegistry.Instance.ContainerCleanCodes.Value; }
		}

		#endregion

		#region ContainerStatuses

		public ICodeDescriptionPairList ContainerStatuses
		{
			get { return FreightDataRegistry.Instance.ContainerStatusList.Value; }
		}

		#endregion

		#region SealParty_List

		public CodeDescriptionPairList SealParty_List =>
			Factory.GetCachedValue("YardUnit.Lookups.SealParty_List",
				delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, Core.Constants.ContainerSealParties.Descriptions.CarrierShippingLine);
					result.AddPair(Core.Constants.ContainerSealParties.Codes.ConsignorShipper, Core.Constants.ContainerSealParties.Descriptions.ConsignorShipper);
					result.AddPair(Core.Constants.ContainerSealParties.Codes.Customs, Core.Constants.ContainerSealParties.Descriptions.Customs);
					result.AddPair(Core.Constants.ContainerSealParties.Codes.Quarantine, Core.Constants.ContainerSealParties.Descriptions.Quarantine);
					result.AddPair(Core.Constants.ContainerSealParties.Codes.Terminal, Core.Constants.ContainerSealParties.Descriptions.Terminal);
					return result;
				});

		#endregion
	}
}
