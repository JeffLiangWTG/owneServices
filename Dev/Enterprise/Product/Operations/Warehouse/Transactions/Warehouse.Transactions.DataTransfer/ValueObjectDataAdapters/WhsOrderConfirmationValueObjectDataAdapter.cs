using System;
using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderConfirmationValueObjectDataAdapter : WhsOrderValueObjectDataAdapter
	{
		#region Constructors

		public WhsOrderConfirmationValueObjectDataAdapter()
			: base(EventsWithSourceType.Empty)
		{
		}

		public WhsOrderConfirmationValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		#endregion

		#region Overrides

		public override void ImportFromValueObject(WhsOrder bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Job Confirmation Import is not supported");
		}

		protected override void ImportFromValueObjectCore(WhsOrder bizObj, Xsd.WhsDocket value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Job Confirmation Import is not supported");
		}

		protected override void ExportAdditionalToValueObjectCore(WhsOrder bizObj, Xsd.WhsDocket value, IValueObjectExportContext context)
		{
			base.ExportAdditionalToValueObjectCore(bizObj, value, context);
			value.Identifier.ActionType = Xsd.WhsDocketIdentifierActionType.CON;

			value.DocketDetail.ShipperCODAmount = bizObj.WD_ShipperCODAmount;
			value.DocketDetail.ShipperCODAmountSpecified = true;

			value.DocketDetail.ShipperCODType = bizObj.WD_CODPayMethod;
			value.DocketDetail.TransportInsurance = bizObj.WD_LocalCartInsuranceCost;
			value.DocketDetail.TransportInsuranceSpecified = true;

			value.DocketDetail.Weight.Value = (bizObj.WD_WeightSent > 0) ? bizObj.WD_WeightSent : bizObj.WD_TotalWeight;
			value.DocketDetail.Weight.DimensionType = bizObj.WD_TotalWeightUnit;
			value.DocketDetail.Cubic.Value = (bizObj.WD_CubicSent > 0) ? bizObj.WD_CubicSent : bizObj.WD_TotalCubic;
			value.DocketDetail.Cubic.DimensionType = bizObj.WD_TotalCubicUnit;
		}

		protected override void ExportDocketLineAdditionalInfo(WhsDocketLine line, Xsd.WhsDocketLine value, INotifications notifications)
		{
			base.ExportDocketLineAdditionalInfo(line, value, notifications);
			var orderLine = (WhsOrderLine)line;
			value.Confirmation.Quantity = orderLine.SumOfUnitsMet;

			if (orderLine.Product.IsAnyPartAttribReleaseCaptured(orderLine.Docket.Client))
			{
				ExportReleaseLines(value, orderLine);
			}
			else
			{
				ExportPickLines(value, orderLine);
			}
		}

		void ExportReleaseLines(Xsd.WhsDocketLine value, WhsOrderLine orderLine)
		{
			foreach (WhsReleaseLine releaseLine in orderLine.ReleaseLines)
			{
				var confirmationLine = value.Confirmation.Lines.AddNew();
				confirmationLine.PartAttribute1 = releaseLine.PartAttribute1;
				confirmationLine.PartAttribute2 = releaseLine.PartAttribute2;
				confirmationLine.PartAttribute3 = releaseLine.PartAttribute3;
				confirmationLine.PackingDate = releaseLine.PackingDate;
				confirmationLine.ExpiryDate = releaseLine.ExpiryDate;
				confirmationLine.Quantity = releaseLine.Quantity;
				confirmationLine.QuantityUQ = releaseLine.UnitsUQ;
			}
		}

		static void ExportPickLines(Xsd.WhsDocketLine value, WhsOrderLine orderLine)
		{
			foreach (var pickLine in orderLine.PickLines)
			{
				var confirmationLine = value.Confirmation.Lines.AddNew();
				var relatedInventoryDocketLine = pickLine.InventoryLineForAvailableInventory;
				if (!relatedInventoryDocketLine.WE_CustomAttrib1.IsEmpty)
				{
					confirmationLine.CustomAttribute1 = relatedInventoryDocketLine.WE_CustomAttrib1;
				}
				if (!relatedInventoryDocketLine.WE_CustomAttrib2.IsEmpty)
				{
					confirmationLine.CustomAttribute2 = relatedInventoryDocketLine.WE_CustomAttrib2;
				}
				if (!relatedInventoryDocketLine.WE_CustomAttrib3.IsEmpty)
				{
					confirmationLine.CustomAttribute3 = relatedInventoryDocketLine.WE_CustomAttrib3;
				}
				if (!relatedInventoryDocketLine.WE_CustomAttrib4.IsEmpty)
				{
					confirmationLine.CustomAttribute4 = relatedInventoryDocketLine.WE_CustomAttrib4;
				}
				if (!relatedInventoryDocketLine.WE_CustomAttrib5.IsEmpty)
				{
					confirmationLine.CustomAttribute5 = relatedInventoryDocketLine.WE_CustomAttrib5;
				}
				if (!relatedInventoryDocketLine.WE_CustomAttrib6.IsEmpty)
				{
					confirmationLine.CustomAttribute6 = relatedInventoryDocketLine.WE_CustomAttrib6;
				}

				if (relatedInventoryDocketLine.WE_CustomDate1.IsValid)
				{
					confirmationLine.CustomDate1 = relatedInventoryDocketLine.WE_CustomDate1;
				}
				if (relatedInventoryDocketLine.WE_CustomDate2.IsValid)
				{
					confirmationLine.CustomDate2 = relatedInventoryDocketLine.WE_CustomDate2;
				}
				if (relatedInventoryDocketLine.WE_CustomDate3.IsValid)
				{
					confirmationLine.CustomDate3 = relatedInventoryDocketLine.WE_CustomDate3;
				}
				if (relatedInventoryDocketLine.WE_CustomDate4.IsValid)
				{
					confirmationLine.CustomDate4 = relatedInventoryDocketLine.WE_CustomDate4;
				}
				if (relatedInventoryDocketLine.WE_CustomDate5.IsValid)
				{
					confirmationLine.CustomDate5 = relatedInventoryDocketLine.WE_CustomDate5;
				}

				var client = orderLine.Docket.Client;
				if (client != null)
				{
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal1) != null)
					{
						confirmationLine.CustomDecimal1 = relatedInventoryDocketLine.WE_CustomDecimal1;
						confirmationLine.CustomDecimal1Specified = true;
					}
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal2) != null)
					{
						confirmationLine.CustomDecimal2 = relatedInventoryDocketLine.WE_CustomDecimal2;
						confirmationLine.CustomDecimal2Specified = true;
					}
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal3) != null)
					{
						confirmationLine.CustomDecimal3 = relatedInventoryDocketLine.WE_CustomDecimal3;
						confirmationLine.CustomDecimal3Specified = true;
					}
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal4) != null)
					{
						confirmationLine.CustomDecimal4 = relatedInventoryDocketLine.WE_CustomDecimal4;
						confirmationLine.CustomDecimal4Specified = true;
					}
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomDecimal5) != null)
					{
						confirmationLine.CustomDecimal5 = relatedInventoryDocketLine.WE_CustomDecimal5;
						confirmationLine.CustomDecimal5Specified = true;
					}

					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag1) != null)
					{
						confirmationLine.CustomFlag1 = relatedInventoryDocketLine.WE_CustomFlag1;
						confirmationLine.CustomFlag1Specified = true;
					}
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag2) != null)
					{
						confirmationLine.CustomFlag2 = relatedInventoryDocketLine.WE_CustomFlag2;
						confirmationLine.CustomFlag2Specified = true;
					}
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag3) != null)
					{
						confirmationLine.CustomFlag3 = relatedInventoryDocketLine.WE_CustomFlag3;
						confirmationLine.CustomFlag3Specified = true;
					}
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag4) != null)
					{
						confirmationLine.CustomFlag4 = relatedInventoryDocketLine.WE_CustomFlag4;
						confirmationLine.CustomFlag4Specified = true;
					}
					if (client.CustomLabels.FindByFieldName(Constants.CustomLabels.WhsDocketLine.CustomFlag5) != null)
					{
						confirmationLine.CustomFlag5 = relatedInventoryDocketLine.WE_CustomFlag5;
						confirmationLine.CustomFlag5Specified = true;
					}
				}
				confirmationLine.PartAttribute1 = relatedInventoryDocketLine.WE_PartAttrib1;
				confirmationLine.PartAttribute2 = relatedInventoryDocketLine.WE_PartAttrib2;
				confirmationLine.PartAttribute3 = relatedInventoryDocketLine.WE_PartAttrib3;
				confirmationLine.PackingDate = relatedInventoryDocketLine.WE_PackingDate;
				confirmationLine.ExpiryDate = relatedInventoryDocketLine.WE_ExpiryDate;
				confirmationLine.Quantity = pickLine.WZ_Units;
				confirmationLine.QuantityUQ = pickLine.WZ_UnitsUQ;
			}
		}

		#endregion
	}
}
