using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	[AsycudaCustomsCountries(Core.Constants.CountryCodes.Namibia)]
	public class UpdateWarehouseTest : TestCaseWithFactory
	{
		public void TestOnSavingExWarehouse()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclarationForTesting>();
			mockDec.Protected().Setup<bool>("GetIsWHSUniversalXMLActive").Returns(false);
			var dec = mockDec.Object;
			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			Assert("Precondition - False", !dec.UpdateJobDeclarationReferenceOnWarehouseSideCalled);

			Factory.Save();
			Assert("Function called OnSaving", dec.UpdateJobDeclarationReferenceOnWarehouseSideCalled);

			dec.UpdateJobDeclarationReferenceOnWarehouseSideCalled = false;
			Factory.Save();
			Assert("Not called second time", !dec.UpdateJobDeclarationReferenceOnWarehouseSideCalled);
		}

		public void TestOnSavingInWarehouse()
		{
			BaseJobDeclarationForTesting dec = Factory.New<BaseJobDeclarationForTesting>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("Precondition - False", !dec.UpdateJobDeclarationReferenceOnWarehouseSideCalled);

			Factory.Save();
			Assert("Function NOT called OnSaving", !dec.UpdateJobDeclarationReferenceOnWarehouseSideCalled);
		}

		public void TestOnSavingWarehouseByExternalAgent()
		{
			BaseJobDeclarationForTesting dec = Factory.New<BaseJobDeclarationForTesting>();
			dec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			Assert("Precondition - False", !dec.UpdateJobDeclarationReferenceOnWarehouseSideCalled);

			Factory.Save();
			Assert("Function NOT called OnSaving", !dec.UpdateJobDeclarationReferenceOnWarehouseSideCalled);
		}

		public void TestAutoUpdateBondedWarehouseOnSaved()
		{
			var dec = Factory.New<BaseJobDeclarationForTesting>();
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = dec.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			Assert("Precondition - False", !dec.Factory_SavedCalled);

			dec.DoMerge();
			Factory.Save();
			Assert("Function NOT called when not enable AutoUpdateBondedWarehouse", !dec.Factory_SavedCalled);

			dec.SetAutoUpdateBondedWarehouseEnabled = true;
			dec.DoMerge();
			Factory.Save();
			Assert("Function called when enable AutoUpdateBondedWarehouse", dec.Factory_SavedCalled);
		}

		public void TestAutoUpdateBondedWarehouse_UpdateSucceed()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia)) // Declaration.IsAutoUpdateBondedWarehouseEnabled = true for NA
			{
				var inwardEntry = helper.GetNewEntryHeader("IMP", "B00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT1230", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m, helper.InwardCusProcedure.ZZ6_Group, Core.Constants.CurrencyCodes.Namibia);
				inwardEntry.CH_BGMReference = "Inward";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardMessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				inwardDeclaration.MessageInitiator = inwardMessageInitiator;
				inwardDeclaration.DoMerge();
				Factory.Save();

				Assert("Precondition: IsAutoUpdateBondedWarehouseEnabled", inwardDeclaration.IsAutoUpdateBondedWarehouseEnabled);
				Assert("Precondition: CH_HasManualWhsUpdate", !inwardEntry.CH_HasManualWhsUpdate);
				AssertEquals("Precondition: CH_WarehouseTransactionStatus", "", inwardEntry.CH_WarehouseTransactionStatus);

				JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(inwardEntry, false);
				JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSInwardAndSaveIfNeeded(inwardEntry, true, true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1230-1", 100m);
				AssertEquals("Manual update warehouse: status", WarehouseTransactionStatusList.Codes.InwardCreated, inwardEntry.CH_WarehouseTransactionStatus);
				Assert("Precondition: MessageInitiator", !inwardMessageInitiator.SuccessfulSendOccured);
				Assert("Precondition: CH_HasManualWhsUpdate", inwardEntry.CH_HasManualWhsUpdate);

				var invoiceLine = inwardDeclaration.InvoiceLines[0];
				invoiceLine.JI_BondedWhsQuantity = 50m;
				inwardDeclaration.DoMerge();
				Factory.Save();

				Assert("Auto update warehouse occur", inwardMessageInitiator.SuccessfulSendOccured);
				AssertEquals("Auto update warehouse notice", "Entry (IP - IP DESC - ENT1230): Stock Levels have been updated. (WHS Receipt: W00000002)\r\n", inwardMessageInitiator.SuccessfulSendText);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1230-1", 50m);

				inwardMessageInitiator.SuccessfulSendOccured = false;
				inwardEntry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
				Factory.Save();
				invoiceLine.JI_BondedWhsQuantity = 100m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				Assert("Auto update warehouse stop when disable integration", !inwardMessageInitiator.SuccessfulSendOccured);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1230-1", 50m);

				var outwardEntry = helper.GetNewEntryHeader("IMP", "B00001231", helper.OutwardCusProcedure.ZZ6_ProcedureCode, "ENT1230", helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 10m, helper.OutwardCusProcedure.ZZ6_Group, Core.Constants.CurrencyCodes.Namibia);
				outwardEntry.CH_BGMReference = "Outward";
				outwardEntry.EntryNumber = "entryNumber";
				var outwardDeclaration = outwardEntry.Declaration;
				var outwardMessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				outwardDeclaration.MessageInitiator = outwardMessageInitiator;
				outwardDeclaration.DoMerge();
				Factory.Save();

				Assert("Precondition: IsAutoUpdateBondedWarehouseEnabled", outwardDeclaration.IsAutoUpdateBondedWarehouseEnabled);
				Assert("Preconditio: CH_HasManualWhsUpdate", !outwardEntry.CH_HasManualWhsUpdate);
				AssertEquals("Precondition: CH_WarehouseTransactionStatus", "", outwardEntry.CH_WarehouseTransactionStatus);

				JobDeclarationWarehouseExtensions.PublishShipmentForWHSOutward(outwardEntry, true);
				JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(outwardEntry, true, true);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1230-1", 40m);
				AssertEquals("Manual update warehouse: status ", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardEntry.CH_WarehouseTransactionStatus);
				Assert("Precondition: MessageInitiator", !outwardMessageInitiator.SuccessfulSendOccured);
				Assert("Precondition: CH_HasManualWhsUpdate", outwardEntry.CH_HasManualWhsUpdate);

				var outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
				outwardInvoiceLine.JI_BondedWhsQuantity = 20m;
				outwardDeclaration.DoMerge();
				Factory.Save();

				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1230-1", 30m);
				Assert("Auto update warehouse notice", outwardMessageInitiator.SuccessfulSendOccured);
				AssertEquals("Auto update warehouse notice", "Entry (OP - OP DESC - entryNumber): Stock Release has been updated. (WHS Order: W00000005)\r\n", outwardMessageInitiator.SuccessfulSendText);
			}
		}

		public void TestAutoUpdateBondedWarehouse_WhsRequiredFieldsCheck()
		{
			var helper = new WhsDataTestHelper(Factory);
			using (helper.WhsHelper.UseAllocationEngineMock())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia)) // Declaration.IsAutoUpdateBondedWarehouseEnabled = true for NA
			{
				var inwardEntry = helper.GetNewEntryHeader("IMP", "B00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT1230", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m, helper.InwardCusProcedure.ZZ6_Group, Core.Constants.CurrencyCodes.Namibia);
				inwardEntry.CH_BGMReference = "inward";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardMessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				inwardDeclaration.MessageInitiator = inwardMessageInitiator;
				inwardDeclaration.DoMerge();
				Factory.Save();

				CombineAssertions(() =>
				{
					JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(inwardEntry, false);
					JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSInwardAndSaveIfNeeded(inwardEntry, true, true);
					WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("ENT1230-1", 100m);
					AssertEquals("Precondition: Manual update warehouse: status", WarehouseTransactionStatusList.Codes.InwardCreated, inwardEntry.CH_WarehouseTransactionStatus);
					AssertEquals("Precondition: Manual update warehouse: flag", true, inwardEntry.CH_HasManualWhsUpdate);
					AssertEquals("Precondition: MessageInitiator", false, inwardMessageInitiator.SuccessfulSendOccured);

					var invoiceLine = inwardDeclaration.InvoiceLines[0];
					invoiceLine.JI_BondedWhsQuantity = 0m;
					inwardDeclaration.DoMerge();
					Factory.Save();
					AssertEquals("Auto update inward warehouse fail", true, inwardMessageInitiator.SuccessfulSendOccured);
					AssertEquals("Auto update inward warehouse failure notice", "Entry (IP - IP DESC - ENT1230): An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.\r\n", inwardMessageInitiator.SuccessfulSendText);

					inwardMessageInitiator.SuccessfulSendOccured = false;
					invoiceLine.JI_BondedWhsQuantity = 50m;
					inwardDeclaration.DoMerge();
					Factory.Save();
					AssertEquals("Auto update inward warehouse", true, inwardMessageInitiator.SuccessfulSendOccured);
					AssertEquals("Auto update inward warehouse notice", "Entry (IP - IP DESC - ENT1230): Stock Levels have been updated. (WHS Receipt: W00000002)\r\n", inwardMessageInitiator.SuccessfulSendText);

					var outwardEntry = helper.GetNewEntryHeader("IMP", "B00001231", helper.OutwardCusProcedure.ZZ6_ProcedureCode, "ENT1230", helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 10m, helper.OutwardCusProcedure.ZZ6_Group, Core.Constants.CurrencyCodes.Namibia);
					outwardEntry.CH_BGMReference = "outwrd";
					outwardEntry.EntryNumber = "entryNumber";
					var outwardDeclaration = outwardEntry.Declaration;
					var outwardMessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					outwardDeclaration.MessageInitiator = outwardMessageInitiator;
					outwardDeclaration.DoMerge();
					Factory.Save();

					JobDeclarationWarehouseExtensions.PublishShipmentForWHSOutward(outwardEntry, true);
					JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(outwardEntry, true, true);
					AssertEquals("Precondition: Manual update outward warehouse: status ", WarehouseTransactionStatusList.Codes.OutwardCreated, outwardEntry.CH_WarehouseTransactionStatus);
					AssertEquals("Precondition: Manual update outward warehouse: flag", true, outwardEntry.CH_HasManualWhsUpdate);
					AssertEquals("Precondition: outward MessageInitiator", false, outwardMessageInitiator.SuccessfulSendOccured);

					var outwardInvoiceLine = outwardDeclaration.InvoiceLines[0];
					outwardInvoiceLine.JI_BondedWhsQuantity = 0m;
					outwardDeclaration.DoMerge();
					Factory.Save();
					AssertEquals("Auto update outward warehouse faile", true, outwardMessageInitiator.SuccessfulSendOccured);
					AssertEquals("Auto update outward warehouse failure notice", "Entry (OP - OP DESC - entryNumber): An Invoice Line marked for Inventory Management must have a Countable Quantity and a Countable Unit of Quantity; not all Invoice Lines marked for Inventory Management have an Countable Quantity and a Countable Unit of Quantity specified.\r\n", outwardMessageInitiator.SuccessfulSendText);

					outwardMessageInitiator.SuccessfulSendOccured = false;
					outwardInvoiceLine.JI_BondedWhsQuantity = 20m;
					outwardDeclaration.DoMerge();
					Factory.Save();
					AssertEquals("Auto update outward warehouse", true, outwardMessageInitiator.SuccessfulSendOccured);
					AssertEquals("Auto update outward warehouse notice", "Entry (OP - OP DESC - entryNumber): Stock Release has been updated. (WHS Order: W00000005)\r\n", outwardMessageInitiator.SuccessfulSendText);
				});
			}
		}

		public class BaseJobDeclarationForTesting : BaseJobDeclaration
		{
			public BaseJobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void UpdateJobDeclarationReferenceOnWarehouseSide()
			{
				UpdateJobDeclarationReferenceOnWarehouseSideCalled = true;
			}

			public bool UpdateJobDeclarationReferenceOnWarehouseSideCalled;

			protected override void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				Factory_SavedCalled = true;
			}

			public bool Factory_SavedCalled;

			protected override bool IsAutoUpdateBondedWarehouseEnabledCore => SetAutoUpdateBondedWarehouseEnabled;
			public bool SetAutoUpdateBondedWarehouseEnabled;
		}
	}
}
