using System;
using CargoWise.Types;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class TaskManagementLoadPackageFact : InputFactWithUserDefinedProperties, ITaskManagementLoadPackageFact
	{
		public TaskManagementLoadPackageFact(
			ITaskManagementGroupingFact grouping,
			ITaskManagementLocationFact dockDoorLocation,
			IOrganisationFact client,
			IOrganisationFact transportCompany,
			IDocAddressFact consigneeAddress,
			ZGuid pk,
			string carrierServiceLevel,
			bool hasDangerousGoods,
			bool isOnAHandlingUnit,
			string outerPackageType,
			string outerUomType)
		{
			Argument.NotNull(grouping, nameof(grouping));
			Argument.NotNull(dockDoorLocation, nameof(dockDoorLocation));
			Argument.NotNull(transportCompany, nameof(transportCompany));
			Argument.NotNull(consigneeAddress, nameof(consigneeAddress));
			Argument.NotNull(transportCompany, nameof(transportCompany));

			PK = pk.ToGuid();
			Grouping = new FactJoin<ITaskManagementGroupingFact>(grouping);
			DockDoorLocation = new FactJoin<ITaskManagementLocationFact>(dockDoorLocation);
			Client = new FactJoin<IOrganisationFact>(client);
			TransportCompany = new FactJoin<IOrganisationFact>(transportCompany);
			ConsigneeAddress = new FactJoin<IDocAddressFact>(consigneeAddress);
			CarrierServiceLevel = Argument.NotNull(carrierServiceLevel, nameof(carrierServiceLevel));
			OuterPackageType = Argument.NotNull(outerPackageType, nameof(outerPackageType));
			OuterUOMType = Argument.NotNull(outerUomType, nameof(outerUomType));
			HasDangerousGoods = hasDangerousGoods;
			IsOnAHandlingUnit = isOnAHandlingUnit;
		}

		public Guid PK { get; }

		public FactJoin<ITaskManagementGroupingFact> Grouping { get; }

		public FactJoin<ITaskManagementLocationFact> DockDoorLocation { get; }

		public FactJoin<IOrganisationFact> Client { get; }

		public FactJoin<IOrganisationFact> TransportCompany { get; }

		public FactJoin<IDocAddressFact> ConsigneeAddress { get; }

		public string CarrierServiceLevel { get; }

		public string OuterPackageType { get; }

		public string OuterUOMType { get; }

		public bool HasDangerousGoods { get; }

		public bool IsOnAHandlingUnit { get; }
	}
}
