using System;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class TaskManagementUnloadLineFact : InputFactWithUserDefinedProperties, ITaskManagementUnloadLineFact
	{
		public TaskManagementUnloadLineFact(
			ITaskManagementGroupingFact grouping,
			IWhsReceive docket,
			IWhsReceiveLine docketLine,
			IOrganisationFact client,
			IOrganisationFact supplier,
			IOrganisationFact consignee,
			IProductFact productFact)
		{
			Argument.NotNull(grouping, nameof(grouping));
			Argument.NotNull(docket, nameof(docket));
			Argument.NotNull(docketLine, nameof(docketLine));
			Argument.NotNull(client, nameof(client));
			Argument.NotNull(productFact, nameof(productFact));

			PK = docketLine.PK.ToGuid();
			Grouping = new FactJoin<ITaskManagementGroupingFact>(grouping);
			Client = new FactJoin<IOrganisationFact>(client);
			Supplier = new FactLeftJoin<IOrganisationFact>(supplier);
			Consignee = new FactLeftJoin<IOrganisationFact>(consignee);

			ServiceLevel = docket.WD_RS_NKServiceLevel;
			ReceiveReference = docket.WD_ExternalReference;
			CustomerReference = docket.WD_CustomerReference;
			ReceiveType = docket.WD_DocketSubType;
			ReceiveCategoryCode = docket.WD_ReceiveCategory;

			HoldCode = docketLine.WE_WHC_NKCurrentInventoryHeldCode;
			PackUQ = docketLine.WE_F3_NKPackType;
			UOMType = docketLine.PackUOM;
			Product = new FactJoin<IProductFact>(productFact);

			PalletID = docketLine.WE_PalletID;
			PartAttribute1 = docketLine.WE_PartAttrib1;
			PartAttribute2 = docketLine.WE_PartAttrib2;
			PartAttribute3 = docketLine.WE_PartAttrib3;
			ExpiryDate = docketLine.WE_ExpiryDate.ConvertToNullableDateTime();
			PackingDate = docketLine.WE_PackingDate.ConvertToNullableDateTime();
			ArrivalDate = docket.WD_ArrivalDate.ConvertToNullableDateTime();
			RequiredDate = docketLine.WE_RequiredByDate.ConvertToNullableDateTime();
		}

		public Guid PK { get; }

		public FactJoin<ITaskManagementGroupingFact> Grouping { get; }

		public FactJoin<IOrganisationFact> Client { get; }

		public FactLeftJoin<IOrganisationFact> Supplier { get; }

		public FactLeftJoin<IOrganisationFact> Consignee { get; }

		public FactJoin<IProductFact> Product { get; }

		public string PalletID { get; }

		public string PackUQ { get; }

		public string UOMType { get; }

		public string ServiceLevel { get; }

		public string ReceiveReference { get; }

		public string CustomerReference { get; }

		public string HoldCode { get; }

		public string ReceiveType { get; }

		public string ReceiveCategoryCode { get; }

		public DateTime? ArrivalDate { get; }

		public DateTime? RequiredDate { get; }

		public string PartAttribute1 { get; }

		public string PartAttribute2 { get; }

		public string PartAttribute3 { get; }

		public DateTime? PackingDate { get; }

		public DateTime? ExpiryDate { get; }
	}
}
