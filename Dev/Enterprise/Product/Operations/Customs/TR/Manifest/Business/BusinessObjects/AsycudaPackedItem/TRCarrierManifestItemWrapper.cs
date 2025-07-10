using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class TRCarrierManifestItemWrapper : DocumentWrapper
	{
		public TRCarrierManifestItemWrapper(AsycudaBill bill)
			: base(bill, bill.Factory)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;
		public TRCarrierManifestItemWrapper(AsycudaPack pack)
			: base(pack, pack.Factory)
		{
			this.pack = pack;
		}
		readonly AsycudaPack pack;

		public TRCarrierManifestItemWrapper(AsycudaPackedItem packedItem)
			: base(packedItem, packedItem.Factory)
		{
			this.packedItem = packedItem;
		}
		readonly AsycudaPackedItem packedItem;

		public ZString BillSequenceNumber => ShowBillLevelInfo ? new ZString(BillWrapper.ABL_SequenceNumber.ToString()) : ZString.Empty;

		public ZString BillNumber => ShowBillLevelInfo ? BillWrapper.ABL_BillNumber : ZString.Empty;

		public ZString ShipperAndConsigneeNames => ShowBillLevelInfo ? new ZString(BillWrapper.ABL_ShipperName + "\r\n" + BillWrapper.ABL_ConsigneeName) : ZString.Empty;

		public ZString PackLineNo => ShowPackLevelInfo ? new ZString(PackWrapper.APA_LineNo.ToString()) : ZString.Empty;

		public ZString PackQty => ShowPackLevelInfo ? new ZString(PackWrapper.APA_PackQty.ToString()) : ZString.Empty;

		public ZString PackUQ => ShowPackLevelInfo ? (ZString)"BI" : ZString.Empty;

		public ZString PackContainerNumber => ShowPackLevelInfo && ContainerWrapper != null ? ContainerWrapper.ACN_ContainerNumber : ZString.Empty;

		public ZString ItemTariff => IsPackedItem ? packedItem.API_Tariff : ZString.Empty;

		public ZString ItemGoodsDescription => IsPackedItem ? packedItem.API_GoodsDescription : ZString.Empty;

		public ZString ItemGrossWeight => string.Format(DefaultCulture.Instance.NumberFormat, "{0:#,0.00}", (IsPackedItem && packedItem.API_GrossWeight > 0) ? new ZDecimal(NumericUtil.ToStringWithDecimalPlaces(Core.Constants.Weight.ConvertSafe(packedItem.API_GrossWeight, packedItem.API_GrossWeightUQ, Core.Constants.Weight.Kilograms), 2)) : 0);

		public ZString PackMarksAndNumbers => IsPackNoItem ? pack.APA_MarksAndNumbers : ZString.Empty;

		bool IsBillNoPack => bill != null;

		bool IsPackNoItem => pack != null;

		bool IsPackedItem => packedItem != null;

		bool ShowBillLevelInfo => BillWrapper != null && (IsBillNoPack || (IsPackNoItem && BillWrapper?.Packs?.FirstOrDefault()?.PK == pack.PK) ||
			(IsPackedItem && BillWrapper?.Packs?.FirstOrDefault().PK == PackWrapper?.PK && PackWrapper?.PackedItems?.Cast<ManifestBase.AsycudaPackPackedItemPivot>().FirstOrDefault()?.APP_API_Item == packedItem.PK));

		bool ShowPackLevelInfo => PackWrapper != null && (IsPackNoItem || (IsPackedItem && PackWrapper?.PackedItems?.Cast<ManifestBase.AsycudaPackPackedItemPivot>().FirstOrDefault()?.APP_API_Item == packedItem.PK));

		public AsycudaBill BillWrapper => IsBillNoPack ? bill : IsPackNoItem ? pack.Bill : IsPackedItem ? packedItem.Pack?.Bill : null;

		AsycudaPack PackWrapper => IsPackNoItem ? pack : IsPackedItem ? packedItem.Pack : null;

		AsycudaContainer ContainerWrapper => PackWrapper?.Container;

		#region Related Declarations For Exports
		public ZString CusReferenceNumber
		{
			get
			{
				var result = ZString.Empty;
				if (ShowBillLevelInfo)
				{
					bool newLine = false;
					foreach (var declaration in BillWrapper?.RelatedDeclarationForExports)
					{
						if (declaration != null)
						{
							result += (newLine ? "\r\n" : "") + declaration.CSI_ReferenceNumber;
							newLine = true;
						}
					}
				}
				return result;
			}
		}
		public ZString CusPartial
		{
			get
			{
				var result = ZString.Empty;
				if (ShowBillLevelInfo)
				{
					bool newLine = false;
					foreach (var declaration in BillWrapper?.RelatedDeclarationForExports)
					{
						if (declaration != null)
						{
							result += newLine ? "\r\n" : "";
							result += String.Equals(declaration.CSI_SubType,
													Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList.Descriptions.Yes,
													StringComparison.OrdinalIgnoreCase) ?
										TurkishConstants.AnswerYesShorten : TurkishConstants.AnswerNoShorten;
							newLine = true;
						}
					}
				}
				return result;
			}
		}
		public ZString CusProcedure
		{
			get
			{
				var result = ZString.Empty;
				if (ShowBillLevelInfo)
				{
					bool newLine = false;
					foreach (var declaration in BillWrapper?.RelatedDeclarationForExports)
					{
						if (declaration != null)
						{
							result += (newLine ? "\r\n" : "") + declaration.CSI_Procedure;
							newLine = true;
						}
					}
				}
				return result;
			}
		}

		public ZString CusBoxQuantity
		{
			get
			{
				var result = ZString.Empty;
				if (ShowBillLevelInfo)
				{
					bool newLine = false;
					foreach (var declaration in BillWrapper?.RelatedDeclarationForExports)
					{
						if (declaration != null)
						{
							result += (newLine ? "\r\n" : "") + declaration.CSI_Quantity.ToStringTrimZeros();
							newLine = true;
						}
					}
				}
				return result;
			}
		}

		public ZString CusGrossWeight
		{
			get
			{
				var result = ZString.Empty;
				if (ShowBillLevelInfo)
				{
					bool newLine = false;
					foreach (var declaration in BillWrapper?.RelatedDeclarationForExports)
					{
						if (declaration != null)
						{
							result += (newLine ? "\r\n" : "") + declaration.CSI_Quantity2.ToStringTrimZeros();
							newLine = true;
						}
					}
				}
				return result;
			}
		}
		#endregion
	}
}
