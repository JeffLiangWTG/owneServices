using System;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class TaskManagementTransferLineFact : InputFactWithUserDefinedProperties, ITaskManagementTransferLineFact
	{
		public TaskManagementTransferLineFact(
			ITaskManagementGroupingFact grouping,
			IWhsTransfer transfer,
			IWhsTransferLine transferLine,
			IOrganisationFact client,
			IProductFact productFact,
			ITaskManagementLocationFact fromLocation,
			ITaskManagementLocationFact toLocation,
			bool hasAwaitingPicks)
		{
			Argument.NotNull(grouping, nameof(grouping));
			Argument.NotNull(transfer, nameof(transfer));
			Argument.NotNull(transferLine, nameof(transferLine));
			Argument.NotNull(client, nameof(client));
			Argument.NotNull(fromLocation, nameof(fromLocation));
			Argument.NotNull(productFact, nameof(productFact));

			PK = transferLine.PK.ToGuid();
			Grouping = new FactJoin<ITaskManagementGroupingFact>(grouping);
			Client = new FactJoin<IOrganisationFact>(client);
			Product = new FactJoin<IProductFact>(productFact);
			FromLocation = new FactJoin<ITaskManagementLocationFact>(fromLocation);
			ToLocation = new FactLeftJoin<ITaskManagementLocationFact>(toLocation);

			DocketID = transfer.WD_DocketID;
			Reference = transfer.WD_ExternalReference;
			TransferType = transfer.DocketSubType;

			HoldCode = transferLine.WE_WHC_NKOriginalInventoryHeldCode;
			PackUQ = transferLine.WE_F3_NKPackType;
			UOMType = transferLine.PackUOM;

			FromPalletID = transferLine.WE_TransferFromPalletId;
			ToPalletID = transferLine.WE_PalletID;
			PartAttribute1 = transferLine.WE_PartAttrib1;
			PartAttribute2 = transferLine.WE_PartAttrib2;
			PartAttribute3 = transferLine.WE_PartAttrib3;
			ExpiryDate = transferLine.WE_ExpiryDate.ConvertToNullableDateTime();
			PackingDate = transferLine.WE_PackingDate.ConvertToNullableDateTime();
			ArrivalDate = transferLine.WE_AdjustmentArrivalDate.ConvertToNullableDateTime();

			HasAwaitingPicks = hasAwaitingPicks;
		}

		public Guid PK { get; }

		public FactJoin<ITaskManagementGroupingFact> Grouping { get; }

		public FactJoin<IOrganisationFact> Client { get; }

		public FactJoin<IProductFact> Product { get; }

		public FactJoin<ITaskManagementLocationFact> FromLocation { get; }

		public FactLeftJoin<ITaskManagementLocationFact> ToLocation { get; }

		public string FromPalletID { get; }

		public string ToPalletID { get; }

		public string PackUQ { get; }

		public string UOMType { get; }

		public string DocketID { get; }

		public string Reference { get; }

		public string HoldCode { get; }

		public string TransferType { get; }

		public DateTime? ArrivalDate { get; }

		public string PartAttribute1 { get; }

		public string PartAttribute2 { get; }

		public string PartAttribute3 { get; }

		public DateTime? PackingDate { get; }

		public DateTime? ExpiryDate { get; }

		public bool HasAwaitingPicks { get; }
	}
}
