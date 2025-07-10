using System;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class OrderLineFact : InputFactWithUserDefinedProperties, IOrderLineFact
	{
		public OrderLineFact(
			IWhsPickableDocketLine pickableDocketLine,
			IWhsPickableDocket pickableDocket,
			IOrganisationFact client,
			IAllocationProductFact product,
			IOrganisationFact consignee,
			ZGuid orderedInventoryPk,
			IOrganisationFact transportCompany,
			IOrganisationFact distributionCentre)
		{
			Argument.NotNull(pickableDocketLine, nameof(pickableDocketLine));
			Argument.NotNull(pickableDocket, nameof(pickableDocket));
			Argument.NotNull(client, nameof(client));
			Argument.NotNull(product, nameof(product));

			PK = pickableDocketLine.PK.ToGuid();
			OrderedInventoryPK = orderedInventoryPk.ToGuid();

			Client = new FactJoin<IOrganisationFact>(client);
			Product = new FactJoin<IAllocationProductFact>(product);
			Consignee = new FactLeftJoin<IOrganisationFact>(consignee);

			TransportCompany = new FactLeftJoin<IOrganisationFact>(transportCompany);
			DistributionCentre = new FactLeftJoin<IOrganisationFact>(distributionCentre);

			Quantity = pickableDocketLine.QuantityNotMet;
			OrderType = pickableDocket.WD_DocketSubType;
			IsWorkOrder = pickableDocket.WD_DocketType.EqualsIgnoringCase("WOR") || pickableDocket.WD_DocketType.EqualsIgnoringCase("DWO");
			IsHeldInventoryOrder = !pickableDocketLine.WE_WHC_NKOrderedHeldCode.IsEmpty;
			IsCustomsTransaction = pickableDocket.IsCustomsTransaction;
			OrderNumber = pickableDocket.WD_ExternalReference;
			CustomerReference = pickableDocket.WD_CustomerReference;
			RequiredDate = pickableDocket.WD_RequiredDate.ToDateTime();
			ServiceLevel = pickableDocket.WD_RS_NKServiceLevel;
			CarrierServiceLevel = pickableDocket.WD_PL_NKCarrierServiceLevel;
			PickPriority = pickableDocket.WD_PickPriority;
			TransportZone = pickableDocket.TransportZoneName;
			SalesChannelCode = pickableDocket.SalesChannelCode;
			PickGroupsCode = pickableDocketLine.WE_PickGroup.ToString();
		}

		public Guid PK { get; }

		public Guid OrderedInventoryPK { get; }

		public decimal Quantity { get; set; }

		public FactJoin<IOrganisationFact> Client { get; }

		public FactJoin<IAllocationProductFact> Product { get; }

		public FactLeftJoin<IOrganisationFact> Consignee { get; }

		public FactLeftJoin<IOrganisationFact> TransportCompany { get; }

		public FactLeftJoin<IOrganisationFact> DistributionCentre { get; }

		public string OrderType { get; }

		public bool IsWorkOrder { get; }

		public bool IsHeldInventoryOrder { get; }

		public bool IsCustomsTransaction { get; }

		public string OrderNumber { get; }

		public string CustomerReference { get; }

		public DateTime RequiredDate { get; }

		public string TransportZone { get; }

		public string ServiceLevel { get; }

		public string CarrierServiceLevel { get; }

		public string SalesChannelCode { get; }

		public string PickGroupsCode { get; }

		public int PickPriority { get; }

		public Guid? ExpiryDateFilterKey { get; set; }
		public DateTime? ExpiryDateFilter { get; set; }
	}
}
