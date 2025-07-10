using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Customs.Business
{
	public class DeclarationInventorySelectionHeader : InventorySelectionHeader
	{
		public DeclarationInventorySelectionHeader(BaseJobDeclaration declaration)
			: base(declaration)
		{
			IsGroupByCartonSupported = true;
		}

		public ZString UpdateOutwardLinesWithInventoryDetails(IEnumerable<BaseJobComInvoiceLine> invoiceLines)
		{
			var errorMessage = ZString.Empty;
			var whsWarehouse = WhsWarehouse;
			if (whsWarehouse == null)
			{
				errorMessage = NoMatchingWarehouseFound;
			}
			else
			{
				var validInvoiceLines = invoiceLines.Where(IsValidForOutwardLineUpdate).OrderByDescending(GetBondedWhsQuantity).ToList();
				if (validInvoiceLines.Count == 0)
				{
					errorMessage = NoValidInvoiceLine;
				}
				else
				{
					UpdateOutwardLinesWithInventoryDetails(validInvoiceLines, whsWarehouse.PK);
				}
			}
			return errorMessage;
		}

		public BaseJobDeclaration Declaration => (BaseJobDeclaration)parent;

		#region Implementation

		bool IsValidForOutwardLineUpdate(BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.ClearRowNotifications();

			var result = false;
			if (HasInwardEntryNumber(invoiceLine) && !HasInwardEntryLineNumber(invoiceLine))
			{
				invoiceLine.AddRowMessageError(InwardEntryLineNumberMustBeSuppliedIfTheInwardEntryNumberHasBeenSpecified);
			}
			else if (!HasInwardEntryNumber(invoiceLine) && HasInwardEntryLineNumber(invoiceLine))
			{
				invoiceLine.AddRowMessageError(InwardEntryNumberMustBeSuppliedIfTheInwardEntryLineNumberHasBeenSpecified);
			}
			else
			{
				result = HasPart(invoiceLine) && GetBondedWhsQuantity(invoiceLine) > 0
						|| HasBondedEntryKey(invoiceLine)
						|| SupportsVINLookup && HasVIN(invoiceLine);
			}

			return result;
		}

		protected ZDecimal GetBondedWhsQuantity(BaseJobComInvoiceLine invoiceLine)
		{
			if (Declaration.IsAllocatedQuantityRequiredForBondedWarehouse)
			{
				return invoiceLine.ComponentInventoryCollection.Cast<JobComInvLineComponentInventory>().Sum(c => c.JIV_QuantityToDraw);
			}
			else if (Declaration.IsBondedWhsQuantityRequiredForBondedWarehouse)
			{
				return invoiceLine.JI_BondedWhsQuantity;
			}
			else
			{
				return invoiceLine.JI_InvoiceQuantity;
			}
		}

		protected void SetBondedWhsQuantity(BaseJobComInvoiceLine invoiceLine, ZDecimal quantity)
		{
			if (Declaration.IsBondedWhsQuantityRequiredForBondedWarehouse)
			{
				invoiceLine.JI_BondedWhsQuantity = quantity;
			}
			invoiceLine.JI_InvoiceQuantity = quantity;
		}

		protected void SetBondedWhsUnitQty(BaseJobComInvoiceLine invoiceLine, ZString uq)
		{
			if (Declaration.IsBondedWhsQuantityRequiredForBondedWarehouse)
			{
				invoiceLine.JI_BondedWhsUnitQty = uq;
			}
			invoiceLine.JI_InvoiceUQ = uq;
		}

		protected virtual ZString GetInwardEntryNumber(BaseJobComInvoiceLine invoiceLine) => invoiceLine.JI_PreviousEntryNumber;

		protected virtual ZInt GetInwardEntryLineNumber(BaseJobComInvoiceLine invoiceLine) => invoiceLine.JI_PreviousEntryLineNumber;

		protected bool HasBondedEntryKey(BaseJobComInvoiceLine invoiceLine) => HasInwardEntryNumber(invoiceLine) && HasInwardEntryLineNumber(invoiceLine);

		protected bool HasInwardEntryNumber(BaseJobComInvoiceLine invoiceLine) => !GetInwardEntryNumber(invoiceLine).IsEmpty;

		protected bool HasInwardEntryLineNumber(BaseJobComInvoiceLine invoiceLine) => !GetInwardEntryLineNumber(invoiceLine).IsEmpty;

		protected ZString GetBondedEntryKey(BaseJobComInvoiceLine invoiceLine) => ZString.Format("{0}-{1}", GetInwardEntryNumber(invoiceLine), GetInwardEntryLineNumber(invoiceLine));

		protected virtual ZBool SupportsVINLookup => ZBool.False;

		protected virtual ZString GetVIN(BaseJobComInvoiceLine invoiceLine) => ZString.Empty;

		protected bool HasVIN(BaseJobComInvoiceLine invoiceLine) => !GetVIN(invoiceLine).IsEmpty;

		protected virtual ZString GetVINAddInfoExpression(BaseJobComInvoiceLine invoiceLine) => ZString.Empty;

		protected ZString GetVIN(IWhsInventoryView inventory) => GetVIN(GetWhsBondedWarehouseAttribute(inventory));

		protected virtual ZString GetVIN(IWhsBondedWarehouseAttribute attr) => ZString.Empty;

		protected OrgSupplierPart GetPart(ZGuid pk) => Factory.Load<OrgSupplierPart>(pk);

		protected ZBool HasPart(BaseJobComInvoiceLine invoiceLine) => invoiceLine.Part != null;

		void UpdateOutwardLinesWithInventoryDetails(List<BaseJobComInvoiceLine> invoiceLines, ZGuid warehousePK)
		{
			var invoiceLinesGroupedByCurrency = invoiceLines.GroupBy(GetInvoiceLineCurrencyCode);
			foreach (var invoiceLinesWithCurrency in invoiceLinesGroupedByCurrency)
			{
				var invoiceLinesToUpdate = invoiceLinesWithCurrency.ToList();
				var productDictionary = GetMatchedInventoryDictionary(invoiceLinesToUpdate, warehousePK);
				var invoices = new List<BaseJobComInvoiceHeader>();
				UpdateOutwardLinesWithInventoryDetailsUsingMatchingOrder(productDictionary, invoiceLinesToUpdate, invoices);
				var localCurrencyCode = invoiceLinesWithCurrency.Key;
				invoices.ForEach(x =>
				{
					x.JZ_InvoiceAmount = x.JZ_Calc_LinesEntered;
					x.JZ_RX_NKInvoice_Currency = localCurrencyCode;
				});
			}
		}

		protected virtual ZString GetInvoiceLineCurrencyCode(BaseJobComInvoiceLine baseJobComInvoiceLine) => baseJobComInvoiceLine.Declaration.LocalCurrencyCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		void UpdateOutwardLinesWithInventoryDetailsUsingMatchingOrder(IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> productDictionary, List<BaseJobComInvoiceLine> invoiceLines, List<BaseJobComInvoiceHeader> invoices)
		{
			var invoiceLinesWithBondedEntryKeyDetails = new List<BaseJobComInvoiceLine>();
			var invoiceLinesWithVINDetails = new List<BaseJobComInvoiceLine>();
			var invoiceLinesWithPartOnly = new List<BaseJobComInvoiceLine>();
			foreach (var invoiceLine in invoiceLines)
			{
				List<BaseJobComInvoiceLine> list;
				if (HasBondedEntryKey(invoiceLine))
				{
					list = invoiceLinesWithBondedEntryKeyDetails;
				}
				else if (SupportsVINLookup && HasVIN(invoiceLine))
				{
					list = invoiceLinesWithVINDetails;
				}
				else
				{
					list = invoiceLinesWithPartOnly;
				}
				list.Add(invoiceLine);
			}

			var usedProductDictionary = new Dictionary<ZGuid, SortedList<string, IWhsInventoryView>>();

			UpdateOutwardLinesWithInventoryDetails(productDictionary, invoiceLinesWithBondedEntryKeyDetails.OrderBy(InvoiceLineThatHasPartShouldBeHandledLast).ToList(), invoices, GetInventoryWithBondedEntryKey, usedProductDictionary, true
				, GetInventoriesWithBondedEntryKey
				, NotifyNoMatchWithBondedEntryKey
				, NotifyNoMatchWithBondedEntryKey
				, UpdateInvoiceLineNotHavingPart);

			if (SupportsVINLookup)
			{
				UpdateOutwardLinesWithInventoryDetails(productDictionary, invoiceLinesWithVINDetails.OrderBy(InvoiceLineThatHasPartShouldBeHandledLast).ToList(), invoices, GetInventoryWithVIN, usedProductDictionary, true
					, GetInventoriesWithVIN
					, NotifyNoMatchWithVIN
					, NotifyNoMatchWithVIN
					, UpdateInvoiceLineNotHavingPart);
			}

			UpdateOutwardLinesWithInventoryDetails(productDictionary, invoiceLinesWithPartOnly, invoices, GetInventoryMeetingQuantityRequirement, usedProductDictionary, true
				, notifyNoMatchInventories: NotifyNoMatchWithSpecifiedProduct
				, notifyNoMatchInventory: NotifyNoMatchWithSpecifiedProduct);

			UpdateOutwardLinesWithInventoryDetails(productDictionary, invoiceLinesWithPartOnly, invoices, GetInventoryWithAnyMatch, usedProductDictionary, true
				, notifyNoMatchInventories: NotifyNoMatchWithSpecifiedProduct
				, notifyNoMatchInventory: NotifyNoMatchWithSpecifiedProduct);
		}

		int InvoiceLineThatHasPartShouldBeHandledLast(BaseJobComInvoiceLine invoiceLine) => !HasPart(invoiceLine) ? 0 : 1;

		void NotifyNoMatchWithBondedEntryKey(BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.AddRowMessageError(HasPart(invoiceLine)
				? BondedEntryKeyAndProductCodeCombinationCannotBeMatchedInTheWarehouseInventory
				: BondedEntryKeyCannotBeMatchedInTheWarehouseInventory);
		}

		void NotifyNoMatchWithVIN(BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.AddRowMessageError(HasPart(invoiceLine)
				? ProductCodeAndVINCombinationCannotBeMatchedInTheWarehouseInventory
				: VINCannotBeMatchedInTheWarehouseInventory);
		}

		static void NotifyNoMatchWithSpecifiedProduct(BaseJobComInvoiceLine invoiceLine)
		{
			invoiceLine.AddRowMessageError(NoAvailableWarehouseInventoryCanBeFoundForTheRequestedProductCode);
		}

		void UpdateInvoiceLineNotHavingPart(BaseJobComInvoiceLine invoiceLine, IWhsInventoryView inventory)
		{
			invoiceLine.JI_PartNo = GetPart(inventory.WI_OP)?.OP_PartNum ?? ZString.Empty;
			invoiceLine.JI_OP = inventory.WI_OP;
			invoiceLine.JI_PartAttrib1 = inventory.WI_PartAttrib1;
			invoiceLine.JI_PartAttrib2 = inventory.WI_PartAttrib2;
			invoiceLine.JI_PartAttrib3 = inventory.WI_PartAttrib3;
			invoiceLine.JI_SerialNumber = inventory.WI_SerialNumber;
		}

		SortedList<string, IWhsInventoryView> GetInventoriesWithVIN(IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> productDictionary, BaseJobComInvoiceLine invoiceLine)
		{
			var vin = GetVIN(invoiceLine);
			return GetInventoriesNotMatchingPartPK(productDictionary
				, inventory => GetVIN(inventory) == vin
				, inventory => GetVIN(inventory));
		}

		SortedList<string, IWhsInventoryView> GetInventoriesWithBondedEntryKey(IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> productDictionary, BaseJobComInvoiceLine invoiceLine)
		{
			var bondedEntryKey = GetBondedEntryKey(invoiceLine);
			return GetInventoriesNotMatchingPartPK(productDictionary
				, inventory => inventory.WI_BondedEntryKey == bondedEntryKey
				, inventory => Invariant($"{inventory.WI_OP}_{inventory.WI_PartAttrib1}_{inventory.WI_PartAttrib2}_{inventory.WI_PartAttrib3}_{inventory.WI_SerialNumber}"));
		}

		SortedList<string, IWhsInventoryView> GetInventoriesNotMatchingPartPK(IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> productDictionary
			, Func<IWhsInventoryView, bool> matchInventory
			, Func<IWhsInventoryView, string> getKey)
		{
			SortedList<string, IWhsInventoryView> result = null;

			var partDetails = GetDistinctPartFor(productDictionary,
				inventory => matchInventory?.Invoke(inventory) ?? false,
				inventory => getKey?.Invoke(inventory) ?? ZString.Empty);
			if (partDetails.Count == 1)
			{
				result = partDetails.First().Value;
			}

			return result;
		}

		Dictionary<string, SortedList<string, IWhsInventoryView>> GetDistinctPartFor(IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> productDictionary
			, Func<IWhsInventoryView, bool> matchInventory
			, Func<IWhsInventoryView, string> getKey)
		{
			var result = new Dictionary<string, SortedList<string, IWhsInventoryView>>();
			foreach (var productData in productDictionary)
			{
				foreach (var inventory in productData.Value.Select(x => x.Value).Where(x => matchInventory?.Invoke(x) ?? false))
				{
					var key = getKey?.Invoke(inventory);
					if (key != null)
					{
						if (!result.TryGetValue(key, out var list))
						{
							list = new SortedList<string, IWhsInventoryView>();
							result.Add(key, list);
						}

						list.Add(GetSortKey(inventory), inventory);
					}
				}
			}

			return result;
		}

		IWhsInventoryView GetInventoryWithBondedEntryKey(SortedList<string, IWhsInventoryView> inventories, BaseJobComInvoiceLine invoiceLine)
		{
			var bondedEntryKey = GetBondedEntryKey(invoiceLine);
			return GetInventoryMatchingPartAttributes(inventories, invoiceLine).FirstOrDefault(x => x.WI_BondedEntryKey == bondedEntryKey);
		}

		IWhsInventoryView GetInventoryWithVIN(SortedList<string, IWhsInventoryView> inventories, BaseJobComInvoiceLine invoiceLine)
		{
			var vin = GetVIN(invoiceLine);
			return GetInventoryMatchingPartAttributes(inventories, invoiceLine).FirstOrDefault(x => GetVIN(x) == vin);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected SortedList<string, IWhsInventoryView> GetInventoriesMatchingPartPK(IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> productDictionary, BaseJobComInvoiceLine invoiceLine)
		{
			var partPK = invoiceLine.JI_OP;
			return partPK.IsValid && productDictionary.TryGetValue(partPK, out var result) ? result : null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected void UpdateOutwardLinesWithInventoryDetails(IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> productDictionary
			, List<BaseJobComInvoiceLine> invoiceLines, List<BaseJobComInvoiceHeader> invoices
			, Func<SortedList<string, IWhsInventoryView>, BaseJobComInvoiceLine, IWhsInventoryView> getInventory
			, Dictionary<ZGuid, SortedList<string, IWhsInventoryView>> usedProductDictionary
			, bool reuseInventory = true
			, Func<IDictionary<ZGuid, SortedList<string, IWhsInventoryView>>, BaseJobComInvoiceLine, SortedList<string, IWhsInventoryView>> getInventoriesNotMatchingPartPK = null
			, Action<BaseJobComInvoiceLine> notifyNoMatchInventories = null
			, Action<BaseJobComInvoiceLine> notifyNoMatchInventory = null
			, Action<BaseJobComInvoiceLine, IWhsInventoryView> updateInvoiceLineNotHavingPartWhenMatched = null)
		{
			foreach (var invoiceLine in invoiceLines.ToArray())
			{
				var inventories = GetInventories(productDictionary, invoiceLine, getInventoriesNotMatchingPartPK, updateInvoiceLineNotHavingPartWhenMatched);

				if (inventories != null)
				{
					var inventory = getInventory(inventories, invoiceLine);
					if (inventory != null)
					{
						UpdateOutwardLineWithInventoryDetail(invoiceLine, inventory);
						var invoice = invoiceLine.InvoiceHeader;
						if (!invoices.Contains(invoice))
						{
							invoices.Add(invoice);
						}
						invoiceLines.Remove(invoiceLine);
						if (reuseInventory)
						{
							var partPK = inventory.WI_OP;
							if (!usedProductDictionary.TryGetValue(partPK, out var usedList))
							{
								usedList = new SortedList<string, IWhsInventoryView>();
								usedProductDictionary.Add(partPK, usedList);
							}

							var key = GetSortKey(inventory);
							if (!usedList.ContainsKey(key))
							{
								usedList.Add(key, inventory);
							}
							inventories.Remove(key);
						}
					}
					else if (!reuseInventory)
					{
						notifyNoMatchInventory?.Invoke(invoiceLine);
					}
				}
				else
				{
					notifyNoMatchInventories?.Invoke(invoiceLine);
					if (reuseInventory)
					{
						invoiceLines.Remove(invoiceLine);
					}
				}
			}

			if (reuseInventory && invoiceLines.Count > 0 && usedProductDictionary.Count > 0)
			{
				UpdateOutwardLinesWithInventoryDetails(usedProductDictionary, invoiceLines, invoices, getInventory, usedProductDictionary, false);
			}
		}

		SortedList<string, IWhsInventoryView> GetInventories(IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> productDictionary
			, BaseJobComInvoiceLine invoiceLine
			, Func<IDictionary<ZGuid, SortedList<string, IWhsInventoryView>>, BaseJobComInvoiceLine, SortedList<string, IWhsInventoryView>> getInventoriesNotMatchingPartPK
			, Action<BaseJobComInvoiceLine, IWhsInventoryView> updateInvoiceLineNotHavingPartWhenMatched)
		{
			SortedList<string, IWhsInventoryView> inventories;
			if (HasPart(invoiceLine))
			{
				inventories = GetInventoriesMatchingPartPK(productDictionary, invoiceLine);
			}
			else
			{
				inventories = getInventoriesNotMatchingPartPK?.Invoke(productDictionary, invoiceLine);
				var inventory = inventories?.Select(x => x.Value).FirstOrDefault();
				if (inventory != null)
				{
					updateInvoiceLineNotHavingPartWhenMatched?.Invoke(invoiceLine, inventory);
				}
			}

			return inventories;
		}

		void UpdateOutwardLineWithInventoryDetail(BaseJobComInvoiceLine invoiceLine, IWhsInventoryView inventory)
		{
			if (GetBondedWhsQuantity(invoiceLine).IsEmpty)
			{
				SetBondedWhsQuantity(invoiceLine, inventory.WI_AvailableToPickQuantity);
			}
			var whsReceiveLine = GetWhsDocketLine(inventory);
			var ratio = whsReceiveLine == null || whsReceiveLine.WE_TransactionQuantity.IsEmpty ? 1m : GetBondedWhsQuantity(invoiceLine) / whsReceiveLine.WE_TransactionQuantity;

			var whsBondedWarehouseAttribute = GetWhsBondedWarehouseAttribute(whsReceiveLine);
			using (Declaration.IsBondedWhsQuantityRequiredForBondedWarehouse ? new DisposableObject() : invoiceLine.GetNewLinePriceCalculationFieldSettingSupporter(LinePriceCalculationFieldSettingType.Quantity))
			{
				UpdateOutwardLineWithInventoryDetailCore(invoiceLine, inventory, whsReceiveLine, whsBondedWarehouseAttribute, ratio);
			}
		}

		protected IWhsDocketLine GetWhsDocketLine(IWhsInventoryView inventory) => Factory.Load<IWhsDocketLine>(inventory.WI_WE_InDocketLine);

		protected IWhsDocketLine GetWhsDocketLineByAllocationKey(string allocationKey) => Factory.LoadTop1<IWhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_AllocationKey, allocationKey));

		protected IWhsBondedWarehouseAttribute GetWhsBondedWarehouseAttribute(IWhsInventoryView inventory)
		{
			var whsReceiveLine = GetWhsDocketLine(inventory);
			return GetWhsBondedWarehouseAttribute(whsReceiveLine);
		}

		protected IWhsBondedWarehouseAttribute GetWhsBondedWarehouseAttribute(IWhsDocketLine docketLine)
		{
			IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute = null;

			if (docketLine != null)
			{
				var attributeQuery = new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, docketLine.PK);
				attributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
				whsBondedWarehouseAttribute = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(attributeQuery);
			}

			return whsBondedWarehouseAttribute;
		}

		protected virtual void UpdateOutwardLineWithInventoryDetailCore(BaseJobComInvoiceLine invoiceLine, IWhsInventoryView inventory, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal ratio)
		{
			if (whsBondedWarehouseAttribute != null)
			{
				ZDecimal linePrice = ratio * whsBondedWarehouseAttribute.WB_ValueForDuty;
				invoiceLine.JI_LinePrice = linePrice.Round(JobComInvoiceLineSchema.JI_LinePrice.Scale);
				var packType = whsReceiveLine.WE_F3_NKPackType;
				if (invoiceLine.JI_InvoiceUQ != packType)
				{
					invoiceLine.JI_InvoiceUQ = packType;
				}
				if (Declaration.IsBondedWhsQuantityRequiredForBondedWarehouse)
				{
					if (invoiceLine.JI_BondedWhsUnitQty != packType)
					{
						invoiceLine.JI_BondedWhsUnitQty = packType;
					}
					invoiceLine.JI_InvoiceQuantity = invoiceLine.JI_BondedWhsQuantity;
				}

				if (!invoiceLine.JI_CustomsQuantity_ReadOnly && !whsBondedWarehouseAttribute.WB_CustomsUnitOfQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsUnitOfQty == invoiceLine.JI_CustomsUnitQty)
				{
					ZDecimal customsQty = ratio * whsBondedWarehouseAttribute.WB_CustomsQty;
					invoiceLine.JI_CustomsQuantity = customsQty.Round(JobComInvoiceLineSchema.JI_CustomsQuantity.Scale);
				}

				if (!invoiceLine.JI_CustomsSecondUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty == invoiceLine.JI_CustomsSecondUnitQty)
				{
					ZDecimal customs2ndQty = ratio * whsBondedWarehouseAttribute.WB_CustomsSecondQuantity;
					invoiceLine.JI_CustomsSecondQuantity = customs2ndQty.Round(JobComInvoiceLineSchema.JI_CustomsSecondQuantity.Scale);
				}

				if (!invoiceLine.JI_CustomsThirdUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty == invoiceLine.JI_CustomsThirdUnitQty)
				{
					ZDecimal customs3rdQty = ratio * whsBondedWarehouseAttribute.WB_CustomsThirdQuantity;
					invoiceLine.JI_CustomsThirdQuantity = customs3rdQty.Round(JobComInvoiceLineSchema.JI_CustomsThirdQuantity.Scale);
				}

				var countryOfOrigin = whsBondedWarehouseAttribute.WB_RN_NKCountryOfOrigin;
				if (!countryOfOrigin.IsEmpty && invoiceLine.JI_CountryOfOrigin != countryOfOrigin)
				{
					invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
				}

				var primaryPreference = whsBondedWarehouseAttribute.WB_PrimaryPreference;
				if (!primaryPreference.IsEmpty && invoiceLine.JI_PrimaryPreference != primaryPreference)
				{
					invoiceLine.JI_PrimaryPreference = primaryPreference;
				}
			}

			if (invoiceLine.Declaration?.SupportInwardProcessing ?? false)
			{
				PopulateLinePriceFromComponents(invoiceLine);
			}
		}

		protected IWhsInventoryView GetInventoryMeetingQuantityRequirement(SortedList<string, IWhsInventoryView> inventories, BaseJobComInvoiceLine invoiceLine)
		{
			var matchedInventories = GetInventoryMatchingPartAttributes(inventories, invoiceLine);
			var expectedInvoiceQuantity = GetBondedWhsQuantity(invoiceLine);
			return matchedInventories.Length > 0 ? matchedInventories.FirstOrDefault(x => x.WI_AvailableToPickQuantity >= expectedInvoiceQuantity) : null;
		}

		protected IWhsInventoryView GetInventoryWithAnyMatch(SortedList<string, IWhsInventoryView> inventories, BaseJobComInvoiceLine invoiceLine)
		{
			return GetInventoryMatchingPartAttributes(inventories, invoiceLine).FirstOrDefault(x => x.WI_AvailableToPickQuantity > 0);
		}

		protected static IWhsInventoryView[] GetInventoryMatchingPartAttributes(SortedList<string, IWhsInventoryView> inventories, BaseJobComInvoiceLine invoiceLine)
		{
			var expectedPartAttrib1 = invoiceLine.JI_PartAttrib1;
			var expectedPartAttrib2 = invoiceLine.JI_PartAttrib2;
			var expectedPartAttrib3 = invoiceLine.JI_PartAttrib3;
			var expectedSerialNumber = invoiceLine.JI_SerialNumber;
			var matchedInventories = inventories.Select(x => x.Value).Where(x => x.WI_PartAttrib1 == expectedPartAttrib1 && x.WI_PartAttrib2 == expectedPartAttrib2 && x.WI_PartAttrib3 == expectedPartAttrib3 && x.WI_SerialNumber == expectedSerialNumber);
			return matchedInventories.ToArray();
		}

		IDictionary<ZGuid, SortedList<string, IWhsInventoryView>> GetMatchedInventoryDictionary(List<BaseJobComInvoiceLine> invoiceLines, ZGuid warehousePK)
		{
			var inventoryDictionary = new Dictionary<ZGuid, SortedList<string, IWhsInventoryView>>();
			foreach (var inventory in Factory.Load<IWhsInventoryView>(GetInventoryQuery(invoiceLines, warehousePK)))
			{
				var partPK = inventory.WI_OP;
				if (!inventoryDictionary.TryGetValue(partPK, out var inventories))
				{
					inventories = new SortedList<string, IWhsInventoryView>();
					inventoryDictionary.Add(partPK, inventories);
				}
				inventories.Add(GetSortKey(inventory), inventory);
			}
			return inventoryDictionary;
		}

		static ZString GetSortKey(IWhsInventoryView inventory)
		{
			var arrivalDate = inventory.WI_ArrivalDate;
			var ticks = arrivalDate.IsValid ? arrivalDate.Ticks : ZDateTime.Today.Ticks;

			return Invariant($"{ticks}_{inventory.WI_TotalUnits}_{inventory.WI_BondedEntryKey}_{inventory.PK.ToStringKey()}");
		}

		protected ZDBOnlyQuery GetInventoryQuery(List<BaseJobComInvoiceLine> invoiceLines, ZGuid warehousePK)
		{
			var query = new ZDBOnlyQuery(typeof(IWhsInventoryView));
			query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, parent.ClientPK);

			var bracketedQuery = new ZQuery();
			var partPKsQuery = new ZQuery(WhsInventoryViewSchema.WI_OP, GetPartPKs(invoiceLines));
			var bondedEntryKeysQuery = new ZQuery(WhsInventoryViewSchema.WI_BondedEntryKey, GetBondedEntryKeys(invoiceLines));

			bracketedQuery.AddToFilter(partPKsQuery);
			bracketedQuery.AddToFilter(bondedEntryKeysQuery, JoinCondition.Or);

			if (SupportsVINLookup)
			{
				var warehouseAttributeSubQuery = new ZDBOnlySubQuery(typeof(IWhsBondedWarehouseAttribute), WhsBondedWarehouseAttributeSchema.WB_ParentID);
				warehouseAttributeSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
				warehouseAttributeSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_AddInfo, SQLComparisonOperator.Contains, GetVINAddInfoExpressions(invoiceLines));
				var vinsQuery = new ZDBOnlyQuery(typeof(IWhsInventoryView));
				vinsQuery.AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, warehouseAttributeSubQuery, JoinCondition.And);
				bracketedQuery.AddToFilter(vinsQuery, JoinCondition.Or);
			}

			query.AddToFilter(bracketedQuery);
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, ZDecimal.Zero);

			var clientSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocket), WhsDocketSchema.PK);
			clientSubQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, parent.ClientPK);
			query.AddSubQuery(WhsInventoryViewSchema.WI_WD, clientSubQuery, JoinCondition.And);

			var warehouseSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocket), WhsDocketSchema.PK);
			warehouseSubQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehousePK);

			var receiveLineSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocketLine), WhsDocketLineSchema.PK);
			receiveLineSubQuery.AddSubQuery(WhsDocketLineSchema.WE_WD, warehouseSubQuery, JoinCondition.Or);

			var locationWarehouseSubQuery = new ZDBOnlySubQuery(typeof(IWhsRow), WhsRowSchema.PK);
			locationWarehouseSubQuery.AddToFilter(WhsRowSchema.WR_WW_Whs, warehousePK);

			var locationSubQuery = new ZDBOnlySubQuery(typeof(IWhsLocation), WhsLocationViewSchema.PK);
			locationSubQuery.AddSubQuery(WhsLocationViewSchema.WLV_WR, locationWarehouseSubQuery, JoinCondition.And);
			receiveLineSubQuery.AddSubQuery(WhsDocketLineSchema.WE_WL, locationSubQuery, JoinCondition.Or);
			query.AddSubQuery(WhsInventoryViewSchema.PK, receiveLineSubQuery, JoinCondition.And);
			query.OrderBy = WhsInventoryViewSchema.WI_ArrivalDate.Name;

			return query;
		}

		protected ZString[] GetBondedEntryKeys(IEnumerable<BaseJobComInvoiceLine> invoiceLines)
		{
			return invoiceLines.Where(HasBondedEntryKey)
				.Select(GetBondedEntryKey)
				.Distinct()
				.ToArray();
		}

		protected ZString[] GetVINAddInfoExpressions(IEnumerable<BaseJobComInvoiceLine> invoiceLines)
		{
			return invoiceLines.Where(HasVIN)
				.Select(GetVINAddInfoExpression)
				.Distinct()
				.ToArray();
		}

		protected ZGuid[] GetPartPKs(List<BaseJobComInvoiceLine> invoiceLines)
		{
			return invoiceLines
				.Where(x => !x.JI_OP.IsEmpty)
				.Select(x => x.JI_OP).Distinct().ToArray();
		}

		protected override void UpdateParentData()
		{
			if (Declaration.WarehouseDocAddress.IsEmpty)
			{
				var warehouseAddress = SelectionLines.GetFirstWarehouseAddress();
				if (warehouseAddress != null)
				{
					Declaration.WarehouseDocAddress.E2_OA_Address = warehouseAddress.PK;
				}
			}
		}

		protected virtual BaseJobComInvoiceHeader GetFirstOrCreateNewInvoiceHeader(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute) =>
			Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.Count > 0
				? Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0]
				: Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();

		protected sealed override WarehouseExtensions.IWarehouseProductLine CreateProductLineFromWarehouseDataCore(WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal invoiceQuantity, ZDecimal ratio)
		{
			var invoiceHeader = GetFirstOrCreateNewInvoiceHeader(whsBondedWarehouseAttribute);

			var part = inventoryWrapper.Part;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			SetHeaderData(invoiceLine, whsBondedWarehouseAttribute);
			invoiceLine.JI_PartNo = part == null ? ZString.Empty : part.OP_PartNum;
			SetPartNoDependentData(invoiceLine, whsBondedWarehouseAttribute);
			invoiceLine.JI_PartAttrib1 = whsReceiveLine.WE_PartAttrib1;
			invoiceLine.JI_PartAttrib2 = whsReceiveLine.WE_PartAttrib2;
			invoiceLine.JI_PartAttrib3 = whsReceiveLine.WE_PartAttrib3;
			invoiceLine.JI_SerialNumber = whsReceiveLine.WE_SerialNumber;

			SetDefaultCustomsProcedureCode(invoiceLine);

			if (whsBondedWarehouseAttribute != null)
			{
				ZDecimal linePrice = ratio * whsBondedWarehouseAttribute.WB_ValueForDuty;
				invoiceLine.JI_LinePrice = linePrice.Round(JobComInvoiceLineSchema.JI_LinePrice.Scale);
				if (!whsBondedWarehouseAttribute.WB_BondedWhsQty.IsEmpty && whsReceiveLine.WE_TransactionQuantity.IsEmpty)
				{
					SetBondedWhsUnitQty(invoiceLine, whsBondedWarehouseAttribute.WB_BondedWhsUnitOfQty);
				}
				else
				{
					SetBondedWhsUnitQty(invoiceLine, whsReceiveLine.WE_F3_NKPackType);
				}

				SetBondedWhsQuantity(invoiceLine, invoiceQuantity);
				if (!invoiceLine.JI_CustomsQuantity_ReadOnly && !whsBondedWarehouseAttribute.WB_CustomsUnitOfQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsUnitOfQty == invoiceLine.JI_CustomsUnitQty)
				{
					ZDecimal customsQty = ratio * whsBondedWarehouseAttribute.WB_CustomsQty;
					invoiceLine.JI_CustomsQuantity = customsQty.Round(JobComInvoiceLineSchema.JI_CustomsQuantity.Scale);
				}

				if (!invoiceLine.JI_CustomsSecondUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsSecondUnitQty == invoiceLine.JI_CustomsSecondUnitQty)
				{
					ZDecimal customs2ndQty = ratio * whsBondedWarehouseAttribute.WB_CustomsSecondQuantity;
					invoiceLine.JI_CustomsSecondQuantity = customs2ndQty.Round(JobComInvoiceLineSchema.JI_CustomsSecondQuantity.Scale);
				}

				if (!invoiceLine.JI_CustomsThirdUnitQty.IsEmpty && !whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty.IsEmpty && whsBondedWarehouseAttribute.WB_CustomsThirdUnitQty == invoiceLine.JI_CustomsThirdUnitQty)
				{
					ZDecimal customs3rdQty = ratio * whsBondedWarehouseAttribute.WB_CustomsThirdQuantity;
					invoiceLine.JI_CustomsThirdQuantity = customs3rdQty.Round(JobComInvoiceLineSchema.JI_CustomsThirdQuantity.Scale);
				}

				var countryOfOrigin = whsBondedWarehouseAttribute.WB_RN_NKCountryOfOrigin;
				if (!countryOfOrigin.IsEmpty && invoiceLine.JI_CountryOfOrigin != countryOfOrigin)
				{
					invoiceLine.JI_CountryOfOrigin = countryOfOrigin;
				}
			}
			var addInfos = PopulateAddInfoData(whsBondedWarehouseAttribute);
			PopulateCountrySpecificInvoiceLineData(invoiceLine, inventoryWrapper, whsReceiveLine, whsBondedWarehouseAttribute, addInfos, invoiceQuantity, ratio);

			if (invoiceLine.Declaration?.SupportInwardProcessing ?? false)
			{
				PopulateComponentInventory(inventoryWrapper, invoiceLine);
				PopulateLinePriceFromComponents(invoiceLine);
			}

			invoiceHeader.JZ_InvoiceAmount = invoiceHeader.JZ_Calc_LinesEntered;
			if (invoiceHeader.JZ_RX_NKInvoice_Currency.IsEmpty)
			{
				invoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.Branch.Company.GC_RX_NKLocalCurrency;
			}
			if (invoiceHeader.JZ_IncoTerm.IsEmpty)
			{
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			}
			SetBondedWhsOrderAndOrderLineNumber(invoiceLine, inventoryWrapper);

			return invoiceLine;
		}

		void SetBondedWhsOrderAndOrderLineNumber(BaseJobComInvoiceLine invoiceLine, WhsInventoryWrapper inventoryWrapper)
		{
			if (inventoryWrapper.relatedOrderLineWrapper is WhsOrderLineWrapper orderLineWrapper)
			{
				invoiceLine.JI_BondedWHSOrderNumber = orderLineWrapper.Order.WD_DocketID;
				invoiceLine.JI_BondedWHSOrderLineNumber = orderLineWrapper.OrderLine.WE_LineNo;
			}
		}

		protected virtual void SetHeaderData(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
		}

		protected virtual void SetPartNoDependentData(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
		}

		protected virtual void SetDefaultCustomsProcedureCode(BaseJobComInvoiceLine invoiceLine)
		{
		}

		protected virtual void PopulateCountrySpecificInvoiceLineData(BaseJobComInvoiceLine invoiceLine, WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, Dictionary<ZString, ZString> addInfos, ZDecimal invoiceQuantity, ZDecimal ratio)
		{
			if (whsBondedWarehouseAttribute != null)
			{
				PopulatePrimaryPreference(invoiceLine, whsBondedWarehouseAttribute);

				var previousEntryNumber = whsBondedWarehouseAttribute.WB_EntryKey;
				if (!previousEntryNumber.IsEmpty && invoiceLine.JI_PreviousEntryNumber != previousEntryNumber)
				{
					invoiceLine.JI_PreviousEntryNumber = previousEntryNumber;
				}

				var previousEntryLineNumber = whsBondedWarehouseAttribute.WB_EntryLineNo;
				if (!previousEntryLineNumber.IsEmpty && invoiceLine.JI_PreviousEntryLineNumber != previousEntryLineNumber)
				{
					invoiceLine.JI_PreviousEntryLineNumber = previousEntryLineNumber;
				}

				PopulateCustomsThirdQuantityAndUnitIfNeeded(invoiceLine, addInfos, ratio);
			}
		}

		protected virtual void PopulateComponentInventory(WhsInventoryWrapper inventoryWrapper, BaseJobComInvoiceLine invoiceLine)
		{
			var inventory = invoiceLine.ComponentInventoryCollection.AddNew();
			inventory.JIV_ClusterKey = Declaration.JE_ClusterKey;
			inventory.JIV_AllocationKey = inventoryWrapper.AllocationKey;
			inventory.JIV_QuantityToDraw = inventoryWrapper.QuantityToDraw;
		}

		protected virtual void PopulatePrimaryPreference(BaseJobComInvoiceLine invoiceLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			var primaryPreference = whsBondedWarehouseAttribute.WB_PrimaryPreference;
			if (!primaryPreference.IsEmpty && invoiceLine.JI_PrimaryPreference != primaryPreference)
			{
				invoiceLine.JI_PrimaryPreference = primaryPreference;
			}
		}

		protected void PopulateCustomsThirdQuantityAndUnitIfNeeded(BaseJobComInvoiceLine invoiceLine, Dictionary<ZString, ZString> addInfos, ZDecimal ratio)
		{
			if (!invoiceLine.JI_CustomsThirdUnitQty.IsEmpty
				&& addInfos.TryGetValue(BondedWarehousingHelper.Constants.CustomsThirdQuantityUnit, out var customsThirdQuantityUnit)
				&& invoiceLine.JI_CustomsThirdUnitQty == customsThirdQuantityUnit
				&& addInfos.TryGetValue(BondedWarehousingHelper.Constants.CustomsThirdQuantity, out var customsThirdQuantityValue)
				&& !customsThirdQuantityValue.IsEmpty
				&& ZDecimal.TryParse(customsThirdQuantityValue, out var customsThirdQuantity))
			{
				ZDecimal customs3rdQty = customsThirdQuantity * ratio;
				invoiceLine.JI_CustomsThirdQuantity = customs3rdQty.Round(5);
			}
		}

		public IEnumerable<(IWhsDocketLine, decimal ratio)> GetLowestLevelComponents(IWhsDocketLine inventory)
		{
			var result = new List<(IWhsDocketLine docketLine, decimal ratio)>();

			var pivots = Factory.Load<IWhsBOMInventoryPivot>(new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, inventory.PK));
			var orderLines = pivots.Select(x => new { x.ComponentLine, x.WIP_ComponentQuantity });

			foreach (var orderLine in orderLines)
			{
				var receiptQty = orderLine.WIP_ComponentQuantity;
				var pickLines = Factory.Load<IWhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine.ComponentLine.PK)).ToArray();

				foreach (var pickLine in pickLines)
				{
					var inventoryLine = pickLine.InventoryLine;
					var ratio = receiptQty / inventoryLine.WE_TransactionQuantity;

					var lowerComponents = GetLowestLevelComponents(inventoryLine);
					if (lowerComponents.Any())
					{
						result.AddRange(lowerComponents.Select<(IWhsDocketLine inv, decimal ratio), (IWhsDocketLine, decimal)>(x => (x.inv, x.ratio * ratio)));
					}
					else
					{
						result.Add((inventoryLine, ratio));
					}
				}
			}

			return result;
		}

		protected void PopulateLinePriceFromComponents(BaseJobComInvoiceLine invoiceLine)
		{
			var valueForDuty = ZDecimal.Zero;

			var componentInventories = invoiceLine.ComponentInventoryCollection.Cast<JobComInvLineComponentInventory>().Where(i => !i.JIV_AllocationKey.IsEmpty).ToArray();
			foreach (var componentInventory in componentInventories)
			{
				var inventoryLine = GetWhsDocketLineByAllocationKey(componentInventory.JIV_AllocationKey);
				valueForDuty += GetValueForDutyFromComponents(inventoryLine, componentInventory.JIV_QuantityToDraw);
			}

			if (!valueForDuty.IsEmpty)
			{
				invoiceLine.JI_LinePrice = valueForDuty;
			}
		}

		protected ZDecimal GetValueForDutyFromComponents(IWhsDocketLine inventoryLine, ZDecimal quantityToDraw)
		{
			var totalValueForDuty = ZDecimal.Zero;
			var mppRatio = quantityToDraw / inventoryLine.WE_TransactionQuantity;

			var components = GetLowestLevelComponents(inventoryLine);
			if (components.Any())
			{
				foreach (var (component, ratio) in components)
				{
					var valueForDutyPerUnit = component.CustomsData.WB_ValueForDuty / component.WE_TransactionQuantity;
					var unitCount = Utilities.Round(component.WE_TransactionQuantity * ratio * mppRatio, 0);
					totalValueForDuty += unitCount * valueForDutyPerUnit;
				}
			}
			else
			{
				totalValueForDuty = inventoryLine.CustomsData.WB_ValueForDuty * mppRatio;
			}

			return totalValueForDuty;
		}

		#endregion

		#region Messages

		static ZString NoMatchingWarehouseFound => Res.GetString("{A3FC75C1-E883-4ECB-A091-49EACF3322F8}", "No matching Warehouse found.");

		ZString NoValidInvoiceLine => SupportsVINLookup
				? Res.GetString("7A63A5E1-8476-44FE-9204-A03566EFEF41", "At least one Invoice Line with valid Previous Entry Details or VIN or Part and {0} is required.", BondedWhsQuantityHumanReadable)
				: Res.GetString("257B1DB1-BC23-4577-A71D-4F913C345BF0", "At least one Invoice Line with valid Previous Entry Details or Part and {0} is required.", BondedWhsQuantityHumanReadable);

		ZString BondedWhsQuantityHumanReadable => Declaration.IsAllocatedQuantityRequiredForBondedWarehouse
			? Res.GetString("0D592557-25E7-4743-AD11-7A6685932FF3", "Allocated Quantity")
			: Declaration.IsBondedWhsQuantityRequiredForBondedWarehouse
			? Res.GetString("CC653070-5505-432F-8677-383BB28900D8", "Countable Quantity")
			: Res.GetString("8CC7B213-3690-4DC2-8401-2EC8E0D72AD8", "Invoice Quantity");

		ZString InwardEntryLineNumberMustBeSuppliedIfTheInwardEntryNumberHasBeenSpecified => Res.GetString("B6626DCC-679B-4CAA-8A0E-B3D5AEFE5DFE"
			, "{0} must be supplied if the {1} has been specified", InwardEntryLineNumberHumanReadable, InwardEntryNumberHumanReadable);

		ZString InwardEntryNumberMustBeSuppliedIfTheInwardEntryLineNumberHasBeenSpecified => Res.GetString("1CF4096A-3850-4F34-A6DD-2BCB0B339328"
			, "{0} must be supplied if the {1} has been specified", InwardEntryNumberHumanReadable, InwardEntryLineNumberHumanReadable);

		ZString BondedEntryKeyAndProductCodeCombinationCannotBeMatchedInTheWarehouseInventory => Res.GetString("65606D06-4C5A-4ED2-84A9-23FC2188D708"
			, "{0}, {1} and Product Code combination cannot be matched in the Warehouse Inventory", InwardEntryNumberHumanReadable, InwardEntryLineNumberHumanReadable);

		ZString BondedEntryKeyCannotBeMatchedInTheWarehouseInventory => Res.GetString("D44685BD-54B4-486D-99B8-6AD3F0D3F0B3"
			, "{0}, {1} combination cannot be matched in the Warehouse Inventory", InwardEntryNumberHumanReadable, InwardEntryLineNumberHumanReadable);

		static ZString ProductCodeAndVINCombinationCannotBeMatchedInTheWarehouseInventory => Res.GetString("F7ACF7FA-0503-437A-A128-A377F7985C51"
			, "Product Code and VIN combination cannot be matched in the Warehouse Inventory");

		static ZString VINCannotBeMatchedInTheWarehouseInventory => Res.GetString("1A88FD93-251E-458E-A4AF-060EFA26F81A"
			, "VIN cannot be matched in the Warehouse Inventory");

		static ZString NoAvailableWarehouseInventoryCanBeFoundForTheRequestedProductCode => Res.GetString("87C3077A-6B50-4956-B486-5BF9773BB90B"
			, "No available warehouse inventory can be found for the requested product code");

		protected virtual ZString InwardEntryNumberHumanReadable => Res.GetString("6E32706A-FB73-4FDC-9B90-B099FF9B93B6", "Previous Entry Number");

		protected virtual ZString InwardEntryLineNumberHumanReadable => Res.GetString("B247C8E2-42FE-4C23-92ED-94D99CBAD0EB", "Previous Entry Line Number");

		#endregion
	}
}
