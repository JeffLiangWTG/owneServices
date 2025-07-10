using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;
using WTG.ProductionRules.Core;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class InventoryFact : InputFactWithUserDefinedProperties, IInventoryFact
	{
		public InventoryFact(
			IWhsReceive docket,
			IWhsReceiveLine docketLine,
			IOrganisationFact client,
			IOrganisationFact supplier,
			IOrganisationFact consignee,
			IOrgSupplierPart part,
			IPutawayProductFact productFact,
			IEquipmentFact equipment = null,
			bool isTsaKnownClient = false,
			bool isTsaPolicyRequired = false,
			bool hasPutawayTransfer = false)
			: this(
				  (ILineToPutawayInfo)docketLine,
				  docketLine,
				  client,
				  part,
				  productFact,
				  equipment,
				  isTsaKnownClient,
				  isTsaPolicyRequired,
				  hasPutawayTransfer)
		{
			Argument.NotNull(docket, nameof(docket));

			ServiceLevel = docket.WD_RS_NKServiceLevel;
			ReceiveReference = docket.WD_ExternalReference;
			CustomerReference = docket.WD_CustomerReference;
			ReceiveType = docket.WD_DocketSubType;
			ReceiveCategoryCode = docket.WD_ReceiveCategory;
			Supplier = new FactLeftJoin<IOrganisationFact>(supplier);
			Consignee = new FactLeftJoin<IOrganisationFact>(consignee);

			ArrivalDate = docket.WD_ArrivalDate.ConvertToNullableDateTime();
			RequiredDate = docketLine.WE_RequiredByDate.ConvertToNullableDateTime();
		}

		public InventoryFact(
			IWhsVASOrder vasOrder,
			ILineToPutawayInfo lineToPutawayInfo,
			IWhsDocketLine docketLine,
			IOrganisationFact client,
			IOrgSupplierPart part,
			IPutawayProductFact productFact,
			IEquipmentFact equipment = null,
			bool isTsaKnownClient = false,
			bool isTsaPolicyRequired = false,
			string vasServiceAreaName = "",
			string vasServiceAreaTypeCode = "")
				: this(
					lineToPutawayInfo,
					docketLine,
					client,
					part,
					productFact,
					equipment,
					isTsaKnownClient,
					isTsaPolicyRequired,
					false,
					vasServiceAreaName,
					vasServiceAreaTypeCode)
		{
			Argument.NotNull(vasOrder, nameof(vasOrder));

			ReceiveType = "VAS";
			ReceiveCategoryCode = string.Empty;

			ServiceLevel = string.Empty;
			ReceiveReference = vasOrder.WVO_CustomerReferenceNo;
			CustomerReference = vasOrder.WVO_CustomerReferenceNo;

			Supplier = new FactLeftJoin<IOrganisationFact>(null);
			Consignee = new FactLeftJoin<IOrganisationFact>(null);
			ArrivalDate = null;
			RequiredDate = null;
		}

		InventoryFact(
			ILineToPutawayInfo lineToPutawayInfo,
			IWhsDocketLine docketLine,
			IOrganisationFact client,
			IOrgSupplierPart part,
			IPutawayProductFact productFact,
			IEquipmentFact equipment,
			bool isTsaKnownClient,
			bool isTsaPolicyRequired,
			bool hasPutawayTransfer = false,
			string vasServiceAreaName = "",
			string vasServiceAreaTypeCode = "")
		{
			Argument.NotNull(lineToPutawayInfo, nameof(lineToPutawayInfo));
			Argument.NotNull(docketLine, nameof(docketLine));
			Argument.NotNull(client, nameof(client));
			Argument.NotNull(productFact, nameof(productFact));
			Argument.NotNull(part, nameof(part));

			PK = lineToPutawayInfo.PK.ToGuid();
			Client = new FactJoin<IOrganisationFact>(client);
			HoldCode = lineToPutawayInfo.InventoryHeldCode;
			Quantity = lineToPutawayInfo.QuantityToPutaway;
			PackUnits = lineToPutawayInfo.PackQuantity;
			PackUQ = lineToPutawayInfo.PackType;
			UOMType = docketLine.PackUOM;

			PartPK = lineToPutawayInfo.ProductPK.ToGuid();
			Product = new FactJoin<IPutawayProductFact>(productFact);
			Equipment = new FactLeftJoin<IEquipmentFact>(equipment);

			PalletID = lineToPutawayInfo.PalletID;

			PartAttribute1 = docketLine.WE_PartAttrib1;
			PartAttribute2 = docketLine.WE_PartAttrib2;
			PartAttribute3 = docketLine.WE_PartAttrib3;
			ExpiryDate = docketLine.WE_ExpiryDate.ConvertToNullableDateTime();
			PackingDate = docketLine.WE_PackingDate.ConvertToNullableDateTime();

			ProductWeight = part.OP_Weight;
			ProductWeightUQ = part.OP_WeightUQ;
			ProductVolume = part.OP_Cubic;
			ProductVolumeUQ = part.OP_CubicUQ;

			IsTSAKnownClient = isTsaKnownClient;
			TSAPolicyRequired = isTsaPolicyRequired;

			HasPutawayTransfer = hasPutawayTransfer;
			VASServiceAreaName = vasServiceAreaName;
			VASServiceAreaTypeCode = vasServiceAreaTypeCode;
		}

		public Guid PK { get; }

		public Guid PartPK { get; }

		public FactJoin<IOrganisationFact> Client { get; }

		public FactJoin<IPutawayProductFact> Product { get; }

		public FactLeftJoin<IOrganisationFact> Consignee { get; }

		public FactLeftJoin<IOrganisationFact> Supplier { get; }

		public FactLeftJoin<IEquipmentFact> Equipment { get; }

		public string PalletID { get; }

		public decimal Quantity { get; set; }

		public decimal PackUnits { get; }

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

		public bool IsTSAKnownClient { get; }

		public bool TSAPolicyRequired { get; }

		public string PartAttribute1 { get; }

		public string PartAttribute2 { get; }

		public string PartAttribute3 { get; }

		public DateTime? PackingDate { get; }

		public DateTime? ExpiryDate { get; }

		public bool HasPutawayTransfer { get; }

		public decimal ProductWeight { get; }

		public string ProductWeightUQ { get; }

		public decimal ProductVolume { get; }

		public string ProductVolumeUQ { get; }

		public string VASServiceAreaName { get; }

		public string VASServiceAreaTypeCode { get; }
	}
}
