using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	public class WhsBondedTransactionProcessor
	{
		#region Constructors

		public WhsBondedTransactionProcessor(BusinessObjectFactory factory, IWhsBondedWarehouseTransaction bondedTransaction)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			if (bondedTransaction == null)
			{
				throw new ArgumentNullException(nameof(bondedTransaction));
			}

			this.factory = factory;
			this.bondedTransaction = bondedTransaction;
		}

		#endregion

		#region Properties

		protected BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		protected IWhsBondedWarehouseTransaction BondedTransaction
		{
			get { return bondedTransaction; }
		}

		#endregion

		#region Process

		public void Process()
		{
			ValidateMandatoryData();
			CheckAndCorrectProductOwnerRelationship();

			BuildDistinctEntryKeyList();
			BuildExistingTransactions();

			adjustmentsCollection = new WhsAdjustmentCollectionBuilderForBonded(Factory);
			receiveCollection = new WhsReceiveCollectionBuilderForBonded(Factory);

			// build adjustments to remove all stock (bringing balances to zero) for all entry numbers / line numbers in InWarehouseTransaction
			BuildAdjustmentsToRemoveExistingStock();

			// build receive to create stock specified in InWarehouseTransaction
			BuildReceiveToCreateBondedTransactionStock();

			FinaliseAllDockets();
		}

		#endregion

		#region PreprocessSetup

		void SortAdjustmentLines()
		{
			foreach (WhsDocket docket in adjustmentsCollection.Dockets)
			{
				docket.Lines.ApplySort(WhsDocketLineSchema.WE_TransactionQuantity.Name, System.ComponentModel.ListSortDirection.Descending);
			}
		}

		void SetupNotificationSubscriberForBatchProcessor()
		{
			SetupDocketCollectionNotificationSubsriberForBatchProcessor(adjustmentsCollection.Dockets);
			SetupDocketCollectionNotificationSubsriberForBatchProcessor(receiveCollection.Dockets);
		}

		void SetupDocketCollectionNotificationSubsriberForBatchProcessor(IEnumerable<WhsDocket> dockets)
		{
			foreach (var docket in dockets)
			{
				docket.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
			}
		}

		#endregion

		#region Notification

		protected void NotifyUserWithFinaliseErrors(WhsDocket docket)
		{
			string body;
			string processType = (docket.WD_DocketType == DocketType.Codes.Receive) ? (NoResString)"Receive" : (NoResString)"Adjustments"; // Exception messages

			if (docket.HasErrors)
			{
				// User error	
				var errors = new ZNotificationCollector(docket, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().ToUniqueMessageListString();
				body = processType + (NoResString)" (Client='" + docket.Client.OH_Code + (NoResString)"', ExternalReference='" + docket.WD_ExternalReference + (NoResString)"') could not be Finalized due to the following error(s)\r\n" +
					 errors + (NoResString)"\r\nPlease load this record in Warehouse " + processType + (NoResString)" screen, fix the error(s) and Finalize."; // Exception messages
			}
			else
			{
				body = processType + (NoResString)" (Client='" + docket.Client.OH_Code + (NoResString)"', ExternalReference='" + docket.WD_ExternalReference + (NoResString)"') was not Finalized, Please load this record in Warehouse Receive screen and Finalize.\r\n"; // Exception messages
			}

			throw new CannotUpdateStockException(body);
		}

		#endregion

		#region Finalisation

		void FinaliseAllDockets()
		{
			SetupNotificationSubscriberForBatchProcessor();
			SortAdjustmentLines();

			adjustmentsCollection.FinaliseAllDockets();

			PutawayReceive();
			receiveCollection.FinaliseAllDockets();

			CheckIfAllDocketsAreFinalised();
		}

		#endregion

		#region Validations

		void CheckIfItISTooLateToAdjust(ZString entryKey, ZShort entryLineNo)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(WhsOrderLine));
			filter.AddToFilter(WhsDocketLineSchema.WE_BondedEntryKey, WhsBondedWarehouseAttribute.BuildKey(entryKey, entryLineNo));
			filter.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, CodeLists.DocketType.Codes.Order);

			ZDBOnlySubQuery docketFilter = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK);
			docketFilter.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Entered);

			filter.AddSubQuery(WhsDocketLineSchema.WE_WD, docketFilter, JoinCondition.And);
			if (Factory.LoadTop1(typeof(WhsOrderLine), filter) != null)
			{
				ThrowCannotUpdateStockException(entryKey, entryLineNo);
			}
		}

		void CheckIfCollectionDocketsAreFinalised(IEnumerable<WhsDocket> dockets)
		{
			foreach (var docket in dockets)
			{
				if (!docket.IsFinalised)
				{
					NotifyUserWithFinaliseErrors(docket);
				}
			}
		}

		void CheckIfAllDocketsAreFinalised()
		{
			CheckIfCollectionDocketsAreFinalised(adjustmentsCollection.Dockets);
			CheckIfCollectionDocketsAreFinalised(receiveCollection.Dockets);
		}

		void CheckAndCorrectProductOwnerRelationship()
		{
			OrgPartRelationCollection productRelationshipList;
			OrgPartRelation productClientOwnerRelationship;

			foreach (IWhsBondedWarehouseTransactionLine line in BondedTransaction.Lines)
			{
				var product = (OrgSupplierPart)line.Product;

				// Should ensure part/product has an owner relationship with the client
				productRelationshipList = product.RelatedOrganisations;
				if (productRelationshipList.FindByOrganisationPKAndRelationship(BondedTransaction.Client.PK, OrgPartRelation.RelationshipTypes.Owner) == null)
				{
					productClientOwnerRelationship = productRelationshipList.AddNew();
					productClientOwnerRelationship.OU_OH = BondedTransaction.Client.PK;
					productClientOwnerRelationship.OU_OP = product.PK;
					productClientOwnerRelationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
				}
			}
		}

		void ValidateMandatoryData()
		{
			var message = new ZStringBuilder();

			var transactionClient = ValidateMandatoryDataClientMissing(message);
			ValidateMandatoryDataAdditionalReferenceReferenceType(message);

			if ((BondedTransaction.Lines == null) || (BondedTransaction.Lines.Count <= 0))
			{
				message.Append((NoResString)"Missing data for field: Lines"); // Exception messages
			}
			else
			{
				foreach (IWhsBondedWarehouseTransactionLine line in BondedTransaction.Lines)
				{
					var warehouse = GetTransactionLineWarehouse(line);
					ValidateFieldWarehouse(warehouse, message);

					if (line.Product == null)
					{
						message.Append((NoResString)"Missing data for field: Product"); // Exception messages
					}

					if (line.EntryKey.IsEmpty)
					{
						message.Append((NoResString)"Missing data for field: EntryKey"); // Exception messages
					}

					if (line.EntryLineNumber.IsEmpty)
					{
						message.Append((NoResString)"Missing data for field: EntryLineNumber"); // Exception messages
					}

					if (line.OriginalEntryKey.IsEmpty && line.Quantity.IsEmpty)
					{
						message.Append((NoResString)"Incorrect data for field: Quantity, is zero"); // Exception messages
					}

					var product = (OrgSupplierPart)line.Product;
					if (transactionClient != null && product != null)
					{
						bool attributeUsedByProduct = transactionClient.PartAttributeManager.IsPartAttributeUsedByProduct(product, 1);
						bool attributeIsMandatory = transactionClient.PartAttributeManager.IsPartAttributeMandatory(1);
						ValidateFieldBondID1(line, attributeUsedByProduct, attributeIsMandatory, message);

						attributeUsedByProduct = transactionClient.PartAttributeManager.IsPartAttributeUsedByProduct(product, 2);
						attributeIsMandatory = transactionClient.PartAttributeManager.IsPartAttributeMandatory(2);
						ValidateFieldBondID2(line, attributeUsedByProduct, attributeIsMandatory, message);

						attributeUsedByProduct = transactionClient.PartAttributeManager.IsPartAttributeUsedByProduct(product, 3);
						attributeIsMandatory = transactionClient.PartAttributeManager.IsPartAttributeMandatory(3);
						ValidateFieldBondID3(line, attributeUsedByProduct, attributeIsMandatory, message);

						ValidateFieldBondID4(line, transactionClient.PartAttributeManager.IsSerialNumberUsedByProduct(product), message);
					}
				}
			}

			if (!message.IsEmpty)
			{
				throw new MissingDataException(message.ToStringWithNewLineBetweenAppends());
			}
		}

		OrgHeader ValidateMandatoryDataClientMissing(ZStringBuilder message)
		{
			var transactionClient = (OrgHeader)BondedTransaction.Client;
			if (transactionClient == null)
			{
				message.Append((NoResString)"Missing data for field: Client"); // Exception messages
			}

			return transactionClient;
		}

		void ValidateFieldBondID3(IWhsBondedWarehouseTransactionLine line, bool attributeUsedByProduct, bool attributeIsMandatory, ZStringBuilder message)
		{
			if (attributeUsedByProduct && attributeIsMandatory && line.PartAttrib3.IsEmpty)
			{
				message.Append((NoResString)"Missing data for field: BondID3, Product(" + line.Product.OP_PartNum + (NoResString)") requires this attribute detail "); // Exception messages
			}
			else if (!attributeUsedByProduct && !line.PartAttrib3.IsEmpty)
			{
				message.Append((NoResString)"Incorrect data for field: BondID3 should be empty as Product(" + line.Product.OP_PartNum + (NoResString)") does not require this attribute detail"); // Exception messages
			}
		}

		void ValidateFieldBondID2(IWhsBondedWarehouseTransactionLine line, bool attributeUsedByProduct, bool attributeIsMandatory, ZStringBuilder message)
		{
			if (attributeUsedByProduct && attributeIsMandatory && line.PartAttrib2.IsEmpty)
			{
				message.Append((NoResString)"Missing data for field: BondID2, Product(" + line.Product.OP_PartNum + (NoResString)") requires this attribute detail "); // Exception messages
			}
			else if (!attributeUsedByProduct && !line.PartAttrib2.IsEmpty)
			{
				message.Append((NoResString)"Incorrect data for field: BondID2 should be empty as Product(" + line.Product.OP_PartNum + (NoResString)") does not require this attribute detail"); // Exception messages
			}
		}

		void ValidateFieldBondID1(IWhsBondedWarehouseTransactionLine line, bool attributeUsedByProduct, bool attributeIsMandatory, ZStringBuilder message)
		{
			if (attributeUsedByProduct && attributeIsMandatory && line.PartAttrib1.IsEmpty)
			{
				message.Append((NoResString)"Missing data for field: BondID1, Product(" + line.Product.OP_PartNum + (NoResString)") requires this attribute detail"); // Exception messages
			}
			else if (!attributeUsedByProduct && !line.PartAttrib1.IsEmpty)
			{
				message.Append((NoResString)"Incorrect data for field: BondID1 should be empty as Product(" + line.Product.OP_PartNum + (NoResString)") does not require this attribute detail"); // Exception messages
			}
		}

		void ValidateFieldBondID4(IWhsBondedWarehouseTransactionLine line, bool attributeUsedByProduct, ZStringBuilder message)
		{
			if (attributeUsedByProduct && line.SerialNumber.IsEmpty)
			{
				message.Append((NoResString)"Missing data for field: BondID4, Product(" + line.Product.OP_PartNum + (NoResString)") requires this attribute detail"); // Exception messages
			}
			else if (!attributeUsedByProduct && !line.SerialNumber.IsEmpty)
			{
				message.Append((NoResString)"Incorrect data for field: BondID4 should be empty as Product(" + line.Product.OP_PartNum + (NoResString)") does not require this attribute detail"); // Exception messages
			}
		}

		void ValidateFieldWarehouse(WhsWarehouse warehouse, ZStringBuilder message)
		{
			if (warehouse == null)
			{
				message.Append((NoResString)"Missing data for field: Warehouse"); // Exception messages
			}
			else if (!warehouse.IsWarehouseBondEnabled)
			{
				message.Append((NoResString)"The warehouse must be a Bonded Warehouse. You can set this in Config -> Warehouse -> Warehouses"); // Exception messages
			}
		}

		void ValidateMandatoryDataAdditionalReferenceReferenceType(ZStringBuilder message)
		{
			ICodeDescriptionPairListWithDefaultCode docketReferences = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;
			foreach (AdditionalReference referenceToValidate in BondedTransaction.AdditionalReferences)
			{
				if (!docketReferences.ContainsCode(referenceToValidate.Type))
				{
					message.Append((NoResString)"Incorrect data for field: Additional Reference, incorrect reference type: " + referenceToValidate.Type); // Exception messages
				}
			}
		}

		#endregion

		#region Exceptions

		void ThrowCannotUpdateStockException(ZString entryKey, ZShort entryLineNo)
		{
			string errorMessage = (NoResString)"Stock Correction failed on Entry Key:" + entryKey + (NoResString)" and Entry Line Number:" + entryLineNo.ToString() +
(NoResString)" as ExWarehouse transactions have already taken place for this Entry Number. Users need to manually correct stock in the Warehouse -> Adjustment screen."; // Exception messages
			throw new CannotUpdateStockException(errorMessage);
		}

		#endregion

		#region Implementation

		#region Warehouse

		WhsWarehouse GetTransactionLineWarehouse(IWhsBondedWarehouseTransactionLine transactionLine)
		{
			var address = (transactionLine.Warehouse != null) ? (OrgAddress)transactionLine.Warehouse : (OrgAddress)BondedTransaction.Warehouse;
			var result = (address != null) ? FindWarehouse(address) : null;
			if (result == null && address != null)
			{
				result = CreateVirtualWarehouse(Factory, address);
			}

			return result;
		}

		WhsWarehouse FindWarehouse(OrgAddress warehouseAddress) => Factory.LoadTop1<WhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, warehouseAddress.PK));

		public static WhsWarehouse CreateVirtualWarehouse(BusinessObjectFactory factory, OrgAddress address)
		{
			var warehouse = factory.New<WhsWarehouse>();
			var locationType = factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, WhsLocationType.SystemWideDefaultLocationTypeCode));
			warehouse.WW_WLT_DefaultLocationType = locationType.PK;
			warehouse.SetUpBondedWarehouse(address);
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			return warehouse;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Property is used in internal exception message")]
		void BuildReceiveToCreateBondedTransactionStock()
		{
			foreach (IWhsBondedWarehouseTransactionLine line in BondedTransaction.Lines)
			{
				var warehouse = GetTransactionLineWarehouse(line);
				var defaultLocationInBondedArea = warehouse.DefaultLocationInBondedArea;
				if (defaultLocationInBondedArea != null)
				{
					var receiveLine = (WhsReceiveLine)receiveCollection.AddLine(warehouse,
						 (OrgHeader)BondedTransaction.Client, BuildExternalReferencePrefix(line.EntryKey, "INW"), // May be a some code abbreviature.
						 ReceiveType.Codes.Customs, (OrgSupplierPart)line.Product, line.Quantity, "",
						 "", "", WhsBondedWarehouseAttribute.BuildKey(line.EntryKey, line.EntryLineNumber), // String is empty or contains only symbols.
						 ZDate.Empty, ZDate.Empty, line.PartAttrib1, line.PartAttrib2, line.PartAttrib3, line.SerialNumber);

					receiveLine.WE_WL = defaultLocationInBondedArea.PK;
					ExtractDataForReceiveLine(line, receiveLine);
				}
				else
				{
					string message = (NoResString)"The Bonded Warehouse has not been updated. This warehouse (" + warehouse.WW_WarehouseName + (NoResString)@") does not have a default location in a Bonded Pick Area defined for stock receipt. The system is supposed to create one automatically, but in this case it was not able to do so. To get around this problem you can create the location manually and then run the 'Update Bonded Warehouse' menu item again (from the Customs Declaration form). To create the location manually, please create a new Bonded Area and configure a location's Pick Area to this bonded area. See WiseLearning Units 1WDF013, 1WDF012 and 1WDF014 for instructions."; // Exception messages
					throw new CannotUpdateStockException(message);
				}
			}
			ExtractAdditionalTransactionData(receiveCollection.Dockets);
			ExtractAdditionalReferences(receiveCollection.Dockets);
		}

		void PutawayReceive()
		{
			var docketsByWarehouse = receiveCollection.Dockets.GroupBy(d => d.WD_WW_Whs);

			foreach (var receives in docketsByWarehouse)
			{
				var receive = receives.Single();
				ReceiveAllocationHelper.AllocateLocations(receive, receive.Warehouse);
			}
		}

		void BuildAdjustmentsToRemoveExistingStock()
		{
			foreach (WhsDocketLine docketLine in existingTransactions)
			{
				var docket = docketLine.Docket;
				var entry = WhsBondedWarehouseAttribute.BreakUpKey(docketLine.WE_BondedEntryKey);
				var adjustmentLine = (WhsAdjustmentLine)adjustmentsCollection.AddLine(docket.Warehouse, docket.Client, BuildExternalReferencePrefix(entry.EntryKey, "AMD"), AdjustmentType.Codes.Customs,
					docketLine.SupplierPart, -docketLine.WE_TransactionQuantity, docketLine.Location?.WLV_LocationString ?? ZString.Empty, "AMD", "AMD", docketLine);
				ExtractDataForAdjustmentLine(docketLine, adjustmentLine);
			}
		}

		void ExtractAdditionalTransactionData(IEnumerable<WhsReceive> dockets)
		{
			foreach (var docket in dockets)
			{
				docket.WD_ArrivalDate = docket.Warehouse.GetWarehouseBranchLocalDateTimeOffset(BondedTransaction.Date);
				if (BondedTransaction.TransportCompany != null)
				{
					docket.TransportCoPK = BondedTransaction.TransportCompany.PK;
				}
			}
		}

		void ExtractAdditionalReferences(IEnumerable<WhsDocket> dockets)
		{
			WhsDocketReference inReference;
			foreach (var tempReference in BondedTransaction.AdditionalReferences)
			{
				foreach (var docket in dockets)
				{
					inReference = docket.References.AddNew();
					inReference.WX_RefType = tempReference.Type;
					inReference.WX_Reference = tempReference.Value.Left(25);
					inReference.WX_WD = docket.PK;
				}
			}
		}

		ZString BuildExternalReferencePrefix(ZString entryKey, ZString postfix)
		{
			return entryKey + "-" + postfix;
		}

		void BuildExistingTransactions()
		{
			existingTransactions = new WhsDocketLineCollectionND(Factory, new ZQuery());

			foreach (object key in distinctEntryKeys.Values)
			{
				ZString entryKey = (ZString)key;
				ZQuery query = new ZQuery(WhsDocketLineSchema.WE_BondedEntryKey, entryKey);
				existingTransactions.AddRange(Factory.Load<WhsDocketLine>(query));
			}
		}

		void BuildDistinctEntryKeyList()
		{
			distinctEntryKeys = new Hashtable();

			foreach (IWhsBondedWarehouseTransactionLine line in BondedTransaction.Lines)
			{
				CheckIfItISTooLateToAdjust(line.EntryKey, line.EntryLineNumber);
				ZString key = WhsBondedWarehouseAttribute.BuildKey(line.EntryKey, line.EntryLineNumber);
				if (!distinctEntryKeys.ContainsKey(key))
				{
					distinctEntryKeys.Add(key, key);
				}
			}
		}

		#region DataExtraction

		void ExtractDataForAdjustmentLine(WhsDocketLine fromLine, WhsAdjustmentLine toLine)
		{
			toLine.WE_F3_NKPackType = fromLine.WE_F3_NKPackType;

			toLine.CustomsData.WB_AddInfo = fromLine.CustomsData.WB_AddInfo;
			toLine.CustomsData.WB_BondedWhsQty = fromLine.CustomsData.WB_BondedWhsQty;
			toLine.CustomsData.WB_BondedWhsUnitOfQty = fromLine.CustomsData.WB_BondedWhsUnitOfQty;
			toLine.CustomsData.WB_CustomsQty = fromLine.CustomsData.WB_CustomsQty;
			toLine.CustomsData.WB_CustomsUnitOfQty = fromLine.CustomsData.WB_CustomsUnitOfQty;
			toLine.CustomsData.WB_CustomsSecondQuantity = fromLine.CustomsData.WB_CustomsSecondQuantity;
			toLine.CustomsData.WB_CustomsSecondUnitQty = fromLine.CustomsData.WB_CustomsSecondUnitQty;
			toLine.CustomsData.WB_CustomsThirdQuantity = fromLine.CustomsData.WB_CustomsThirdQuantity;
			toLine.CustomsData.WB_CustomsThirdUnitQty = fromLine.CustomsData.WB_CustomsThirdUnitQty;
			toLine.CustomsData.WB_EntryDate = fromLine.CustomsData.WB_EntryDate;
			toLine.CustomsData.WB_EntryKey = fromLine.CustomsData.WB_EntryKey;
			toLine.CustomsData.WB_EntryLineNo = fromLine.CustomsData.WB_EntryLineNo;
			toLine.CustomsData.WB_ParentTableCode = fromLine.CustomsData.WB_ParentTableCode;
			toLine.CustomsData.WB_RN_NKCountryOfOrigin = fromLine.CustomsData.WB_RN_NKCountryOfOrigin;
			toLine.CustomsData.WB_ValueForDuty = fromLine.CustomsData.WB_ValueForDuty;
			toLine.CustomsData.WB_TILV = fromLine.CustomsData.WB_TILV;
			toLine.CustomsData.WB_RX_NKTILVCurrency = fromLine.CustomsData.WB_RX_NKTILVCurrency;
			toLine.CustomsData.WB_DeclarationReference = fromLine.CustomsData.WB_DeclarationReference;
		}

		protected virtual void ExtractDataForReceiveLine(IWhsBondedWarehouseTransactionLine fromLine, WhsDocketLine toLine)
		{
			toLine.WE_F3_NKPackType = fromLine.QuantityUnit;
			WhsReceiveLine receiveLine = toLine as WhsReceiveLine;
			if (receiveLine != null && receiveLine.Inventory.Count > 0)
			{
				receiveLine.Inventory[0].WI_F3_NKPackType = fromLine.QuantityUnit;
			}
			toLine.CustomsData.WB_EntryKey = fromLine.EntryKey;
			toLine.CustomsData.WB_EntryLineNo = fromLine.EntryLineNumber;
			toLine.CustomsData.WB_ParentID = toLine.PK;
			toLine.CustomsData.WB_ParentTableCode = WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode;
			toLine.CustomsData.WB_EntryDate = fromLine.EntryDate;
			toLine.CustomsData.WB_ValueForDuty = fromLine.ValueForDuty;
			toLine.CustomsData.WB_TILV = fromLine.TILV.Amount;
			if (fromLine.TILV.Currency != null)
			{
				toLine.CustomsData.WB_RX_NKTILVCurrency = fromLine.TILV.Currency.Code;
			}
			toLine.CustomsData.WB_AddInfo = fromLine.AddInfo;
			toLine.CustomsData.WB_BondedWhsQty = fromLine.BondedWarehouseQuantity;
			toLine.CustomsData.WB_BondedWhsUnitOfQty = fromLine.BondedWarehouseQuantityUnit;
			toLine.CustomsData.WB_CustomsQty = fromLine.CustomsQuantity;
			toLine.CustomsData.WB_CustomsUnitOfQty = fromLine.CustomsQuantityUnit;
			toLine.CustomsData.WB_CustomsSecondQuantity = fromLine.CustomsSecondQuantity;
			toLine.CustomsData.WB_CustomsSecondUnitQty = fromLine.CustomsSecondQuantityUnit;
			toLine.CustomsData.WB_CustomsThirdQuantity = fromLine.CustomsThirdQuantity;
			toLine.CustomsData.WB_CustomsThirdUnitQty = fromLine.CustomsThirdQuantityUnit;
			toLine.CustomsData.WB_DeclarationReference = BondedTransaction.Reference;
			if (fromLine.CountryOfOrigin != null)
			{
				toLine.CustomsData.WB_RN_NKCountryOfOrigin = fromLine.CountryOfOrigin.RN_Code;
			}
		}

		#endregion

		Hashtable distinctEntryKeys;
		readonly BusinessObjectFactory factory;
		readonly IWhsBondedWarehouseTransaction bondedTransaction;
		WhsDocketLineCollectionND existingTransactions;
		WhsReceiveCollectionBuilderForBonded receiveCollection;
		WhsAdjustmentCollectionBuilderForBonded adjustmentsCollection;

		#endregion
	}
}
