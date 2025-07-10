using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Warehouse.Integration;
using AutoJobComInvoiceLine = Enterprise.Customs.US.Business.AutoJobComInvoiceLine;

namespace Enterprise.Customs.US.InBond.Business
{
	public class MoveHeaderInventorySelectionHeader : InventorySelectionHeader
	{
		public MoveHeaderInventorySelectionHeader(CusInBondMoveHeader moveHeader)
			: base(moveHeader)
		{
		}

		public CusInBondMoveHeader MoveHeader
		{
			get { return (CusInBondMoveHeader)base.parent; }
		}

		protected override bool AllowWithdrawalOfMultipleEntryDetailsCore
		{
			get { return true; }
		}

		protected override void UpdateParentData()
		{
			if (MoveHeader.BM_OA_WarehouseAddress.IsEmpty)
			{
				var warehouseAddress = SelectionLines.GetFirstWarehouseAddress();
				if (warehouseAddress != null)
				{
					MoveHeader.BM_OA_WarehouseAddress = warehouseAddress.PK;
				}
			}
		}

		protected override void ImportSetup()
		{
			processedMoveDetailList = new List<CusInBondMoveDetail>();
			newBillDictionary = new Dictionary<ZGuid, CusInBondBill>();
			packingGroupIDDictionary = new Dictionary<ZString, PackingGroupCommodity>();
			containerDictionary = new Dictionary<ZString, CusInBondContainer>();
			SelectedLines.Sort(new Comparison<WhsInventoryWrapper>(CompareWrapper));
			foreach (WhsInventoryWrapper inventoryWrapper in SelectedLines)
			{
				var packingGroupKey = inventoryWrapper.PackingGroupKey;
				if (!packingGroupKey.IsEmpty)
				{
					PackingGroupCommodity packingGroupCommodity;
					if (!packingGroupIDDictionary.TryGetValue(packingGroupKey, out packingGroupCommodity))
					{
						packingGroupCommodity = new PackingGroupCommodity();
						packingGroupIDDictionary.Add(packingGroupKey, packingGroupCommodity);
					}
					packingGroupCommodity.ReceiveLines.Add(inventoryWrapper.Inventory.WI_WE_InDocketLine);
				}
			}
		}
		Dictionary<ZString, PackingGroupCommodity> packingGroupIDDictionary;

		int CompareWrapper(WhsInventoryWrapper x, WhsInventoryWrapper y)
		{
			var result = string.Compare(x.PackingGroupKey, y.PackingGroupKey, true);
			if (result == 0)
			{
				result = string.Compare(x.Product, y.Product, true);
				if (result == 0)
				{
					result = string.Compare(x.CustomsEntryKey, y.CustomsEntryKey, true);
					if (result == 0)
					{
						result = x.QuantityToDraw.CompareTo(y.QuantityToDraw);
					}
				}
			}
			return result;
		}

		class PackingGroupCommodity
		{
			public CusInBondCargoDesc Commodity;
			public List<ZGuid> ReceiveLines
			{
				get { return receiveLines ?? (receiveLines = new List<ZGuid>()); }
			}
			List<ZGuid> receiveLines;
		}

		protected override void ImportTearDown()
		{
			processedMoveDetailList.ForEach(x => UpdateInBondQtyAndManifestQty(x));
			processedMoveDetailList = null;
			packingGroupIDDictionary = null;
			newBillDictionary = null;
			containerDictionary = null;
		}

		void UpdateInBondQtyAndManifestQty(CusInBondMoveDetail moveDetail)
		{
			var inBondQty = moveDetail.Containers.SelectMany(x => x.Commodities.OfType<CusInBondCargoDesc>()).Sum(x => x.BY_PieceCount);
			CusInBondBill bill;
			if (!newBillDictionary.TryGetValue(moveDetail.B9_B0, out bill))
			{
				bill = moveDetail.Bill;
				if (bill != null && bill.MoveDetails.Count != 1)
				{
					bill = null;
				}
			}
			if (bill != null)
			{
				bill.B0_ManifestQty = inBondQty;
			}
			moveDetail.B9_InBoundQty = inBondQty;
		}

		protected override IWarehouseProductLine CreateProductLineFromWarehouseDataCore(WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal invoiceQuantity, ZDecimal ratio)
		{
			var part = inventoryWrapper.Part;
			var line = GetNewCommodity(inventoryWrapper, whsBondedWarehouseAttribute, invoiceQuantity);
			line.BY_OH_Supplier = inventoryWrapper.SupplierPK;
			line.BY_PartNumber = part == null ? ZString.Empty : part.OP_PartNum;
			line.BY_PartAttrib1 = whsReceiveLine.WE_PartAttrib1;
			line.BY_PartAttrib2 = whsReceiveLine.WE_PartAttrib2;
			line.BY_PartAttrib3 = whsReceiveLine.WE_PartAttrib3;
			line.BY_SerialNumber = whsReceiveLine.WE_SerialNumber;

			line.BY_InvoiceQuantity = invoiceQuantity;
			if (whsBondedWarehouseAttribute != null)
			{
				line.BY_WarehouseEntryNumber = whsBondedWarehouseAttribute.WB_EntryKey;
				line.BY_WarehouseEntryLineNo = whsBondedWarehouseAttribute.WB_EntryLineNo;
				line.BY_MonetaryValue = Round(ratio * whsBondedWarehouseAttribute.WB_ValueForDuty, 5);
			}
			if (line.IsTopLevelCommodity)
			{
				UpdateTopLevelCommodityData(line, inventoryWrapper, invoiceQuantity);
			}
			AppendZoneStatusToDescription(line, whsBondedWarehouseAttribute);
			CalculateWeight(line, whsReceiveLine, whsBondedWarehouseAttribute, ratio);
			return line;
		}

		void CalculateWeight(CusInBondCargoDesc line, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			if (line.BY_GrossWeight.IsEmpty)
			{
				var perUntWeight = ZDecimal.Zero;
				var weightUnit = line.BY_GrossWeightUnit;
				var targetWeight = ZDecimal.Zero;
				if (Core.Constants.Weight.ContainsCode(whsBondedWarehouseAttribute.WB_CustomsUnitOfQty.ToUpper()))
				{
					targetWeight = (weightUnit.IsEmpty || weightUnit == whsBondedWarehouseAttribute.WB_CustomsUnitOfQty) ? whsBondedWarehouseAttribute.WB_CustomsQty : (ZDecimal)Core.Constants.Weight.Convert(whsBondedWarehouseAttribute.WB_CustomsQty, whsBondedWarehouseAttribute.WB_CustomsUnitOfQty.ToUpper(), weightUnit);
					weightUnit = weightUnit.IsEmpty ? whsBondedWarehouseAttribute.WB_CustomsUnitOfQty.ToUpper() : weightUnit;
				}
				else if (Core.Constants.Weight.ContainsCode(whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty.ToUpper()))
				{
					targetWeight = (weightUnit.IsEmpty || weightUnit == whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty) ? whsBondedWarehouseAttribute.WB_CustomsSecondQuantity : (ZDecimal)Core.Constants.Weight.Convert(whsBondedWarehouseAttribute.WB_CustomsSecondQuantity, whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty.ToUpper(), weightUnit);
					weightUnit = weightUnit.IsEmpty ? whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty.ToUpper() : weightUnit;
				}
				else if (Core.Constants.Weight.ContainsCode(whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty.ToUpper()))
				{
					targetWeight = (weightUnit.IsEmpty || weightUnit == whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty) ? whsBondedWarehouseAttribute.WB_CustomsThirdQuantity : (ZDecimal)Core.Constants.Weight.Convert(whsBondedWarehouseAttribute.WB_CustomsThirdQuantity, whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty.ToUpper(), weightUnit);
					weightUnit = weightUnit.IsEmpty ? whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty.ToUpper() : weightUnit;
				}

				if (!targetWeight.IsEmpty)
				{
					perUntWeight = targetWeight / (whsReceiveLine.WE_TransactionQuantity.IsEmpty ? (ZDecimal)1m : whsReceiveLine.WE_TransactionQuantity);
				}

				if (!perUntWeight.IsEmpty)
				{
					line.BY_GrossWeight = Math.Ceiling(ratio * perUntWeight);
				}
				if (line.BY_GrossWeightUnit.IsEmpty)
				{
					line.BY_GrossWeightUnit = weightUnit;
				}
			}
		}

		void UpdateProcessedList(CusInBondMoveDetail moveDetail)
		{
			if (moveDetail != null)
			{
				if (!processedMoveDetailList.Contains(moveDetail))
				{
					processedMoveDetailList.Add(moveDetail);
				}
			}
		}

		void UpdateTopLevelCommodityData(CusInBondCargoDesc commodity, WhsInventoryWrapper inventoryWrapper, ZDecimal invoiceQuantity)
		{
			commodity.BY_PieceCount = new ZDecimal(invoiceQuantity / inventoryWrapper.PerPackageQty).ToZInt();
			UpdateProcessedList(commodity.MoveDetail);
		}

		CusInBondCargoDesc GetNewCommodity(WhsInventoryWrapper inventoryWrapper, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal invoiceQuantity)
		{
			CusInBondCargoDesc result = null;
			var packingGroupKey = inventoryWrapper.PackingGroupKey;
			PackingGroupCommodity packingGroupCommodity;
			var receiveLine = inventoryWrapper.ReceiveLine;
			if (receiveLine != null && !packingGroupKey.IsEmpty && packingGroupIDDictionary.TryGetValue(packingGroupKey, out packingGroupCommodity)
				&& packingGroupCommodity.ReceiveLines.Count > 1 && packingGroupCommodity.ReceiveLines.Contains(receiveLine.PK))
			{
				if (packingGroupCommodity.Commodity == null)
				{
					packingGroupCommodity.Commodity = GetContainer(whsBondedWarehouseAttribute).Commodities.AddNew();
					UpdateTopLevelCommodityData(packingGroupCommodity.Commodity, inventoryWrapper, invoiceQuantity);
				}
				var part = inventoryWrapper.Part;
				if (part != null)
				{
					var description = packingGroupCommodity.Commodity.BY_Description;
					if (description.Length < CusInBondCargoDesc.Schema.BY_DescriptionMaxLength - 1) // one for the space
					{
						var partDesc = part.OP_Desc;
						if (!partDesc.IsEmpty && !description.Contains(partDesc, StringComparison.OrdinalIgnoreCase))
						{
							var newDescription = new ZStringBuilder();
							newDescription.AppendIfNotEmpty(description);
							newDescription.AppendIfNotEmpty(partDesc.Trim());
							description = newDescription.ToStringWithDelimiterBetweenAppends(" ");
							packingGroupCommodity.Commodity.BY_Description = description.Left(CusInBondCargoDesc.Schema.BY_DescriptionMaxLength);
						}
					}
				}
				result = packingGroupCommodity.Commodity.ChildCommodities.AddNew();
			}
			return result ?? GetContainer(whsBondedWarehouseAttribute).Commodities.AddNew();
		}

		void AppendZoneStatusToDescription(CusInBondCargoDesc line, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			string description = line.BY_Description.TrimEnd(new char[] { '\r', '\n' }).Replace("\r\n\r\n", "\r\n");
			var zoneStatus = GetZoneStatus(whsBondedWarehouseAttribute);
			if (!zoneStatus.IsEmpty)
			{
				var newDescriptionWithZoneStatus = new ZStringBuilder();
				newDescriptionWithZoneStatus.AppendIfNotEmpty(description);
				newDescriptionWithZoneStatus.AppendIfNotEmpty(zoneStatus);
				line.BY_Description = ((ZString)newDescriptionWithZoneStatus.ToStringWithNewLineBetweenAppends()).Left(CusInBondCargoDesc.Schema.BY_DescriptionMaxLength);
			}
		}

		ZString GetZoneStatus(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			var zoneStatusValue = ZString.Empty;
			var zoneStatusKey = AutoJobComInvoiceLine.Schema.US_ZoneStatus.Substring(3);
			if (whsBondedWarehouseAttribute != null && whsBondedWarehouseAttribute.WB_AddInfo.Contains(zoneStatusKey, StringComparison.CurrentCultureIgnoreCase))
			{
				var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(whsBondedWarehouseAttribute.WB_AddInfo);
				addInfos.TryGetValue(zoneStatusKey, out zoneStatusValue);
			}
			return Factory.GetCachedValue<ZoneStatusCodeList>().GetDescriptionFromCode(zoneStatusValue);
		}

		protected virtual CusInBondContainer GetContainer(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			var entryNumber = whsBondedWarehouseAttribute.WB_EntryKey;
			CusInBondContainer result;
			if (!containerDictionary.TryGetValue(entryNumber, out result))
			{
				var moveDetail = GetMoveDetail();
				if (moveDetail.Containers.Count == 1)
				{
					result = moveDetail.Containers[0];
				}
				else
				{
					result = moveDetail.Containers.FirstOrDefault(x => x.IsNonContainerized);
					if (result == null)
					{
						result = moveDetail.Containers.AddNew();
						result.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
					}
				}
				containerDictionary.Add(entryNumber, result);
			}
			return result;
		}

		CusInBondMoveDetail GetMoveDetail()
		{
			CusInBondMoveDetail moveDetail = null;
			var header = MoveHeader.Header;
			if (header != null)
			{
				CusInBondBill newBill = null;
				var bill = header.Bills.OfType<CusInBondBill>().OrderBy(x => x.B0_MasterBillNumber).FirstOrDefault();
				if (bill == null)
				{
					bill = header.Bills.AddNew();
					newBill = bill;
				}
				else
				{
					moveDetail = MoveHeader.MovementDetails.FirstOrDefault(x => x.B9_B0 == bill.PK);
				}
				if (moveDetail == null)
				{
					moveDetail = MoveHeader.MovementDetails.AddNew(bill.PK);
					header.MovementDetails.Add(moveDetail);
				}
				if (newBill != null)
				{
					newBillDictionary.Add(newBill.PK, newBill);
				}
			}
			else
			{
				moveDetail = MoveHeader.MovementDetails.AddNew();
			}
			return moveDetail;
		}

		Dictionary<ZString, CusInBondContainer> containerDictionary;
		Dictionary<ZGuid, CusInBondBill> newBillDictionary;
		List<CusInBondMoveDetail> processedMoveDetailList;
	}
}
