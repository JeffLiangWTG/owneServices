using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Business.CommonGoodsItemsIntegration;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class BaseOnlyEntryHeaderTest : CusEntryHeaderTest
	{
		public void TestPackagesCountWhenInvoiceLineIsEmpty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);

				var cusEntryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals(0, cusEntryHeader.PackagesCount);

				invoiceLine.Delete();
				AssertEquals(0, cusEntryHeader.PackagesCount);
			}
		}

		public void TestConfirmedChargesAndCollectionType()
		{
			var dec = GetNewDeclaration();
			var ceh = dec.CustomsEntryHeaders.AddNew();
			AssertType("CEH needs to have its own type of ConfirmedCharges collection", typeof(ConfirmedCusEntryHeaderChargesCollection<CusEntryHeaderCharges>), ceh.ConfirmedCharges);
			AssertType("CEH.ConfirmedCharges.AddNew() gives correct charge type", typeof(CusEntryHeaderCharges), ceh.ConfirmedCharges.AddNew());
			AssertType("CEH.ConfirmedCharges.AddOrUpdate(string) gives correct charge type", typeof(CusEntryHeaderCharges), ceh.ConfirmedCharges.AddOrUpdate("x"));
			AssertType("CEH.ConfirmedCharges[int] gives correct charge type", typeof(CusEntryHeaderCharges), ceh.ConfirmedCharges[0]);
			AssertType("CEH.ConfirmedCharges[string] gives correct charge type", typeof(CusEntryHeaderCharges), ceh.ConfirmedCharges["x"]);
		}

		public void TestCH_DataModel_SetOnSaving()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_JE = declaration.PK;
			AssertEquals("Not set", ZString.Empty, entry.CH_DataModel);
			entry.OnSaving();
			AssertEquals("set", "ER", entry.CH_DataModel);
		}

		public void TestCH_DataModel_SetOnFactorySave()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals("Not set", ZString.Empty, entry.CH_DataModel);
			Factory.Save();
			AssertEquals("set", "ER", entry.CH_DataModel);
		}

		public void TestCH_DataModel_ReportErrorWhenUpdated() =>
			DataModelTestHelper.RunDataModelTest_ReportErrorWhenUpdated<CusEntryHeader>(Factory);

		public void TestCH_DataModel_CanSaveTwice() =>
			DataModelTestHelper.RunDataModelTest_CanSaveTwice<CusEntryHeader>(Factory);

		public void TestIEDIMessageCollectionOwner()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var ediMessageCollectionOwner = entryHeader as IEDIMessageCollectionOwner;

			AssertNotNull("Failed to cast EntryHeader", ediMessageCollectionOwner);
			AssertSame("Message Owner", entryHeader, ediMessageCollectionOwner.MessageOwner);
			AssertSame("Messages Collection", entryHeader.Messages, ediMessageCollectionOwner.Messages);
		}

		public void TestGetInventoryAutomationAction()
		{
			var helper = new WhsDataTestHelper(Factory);
			var data = helper.CreateChangeOfRegimeEntryData();
			var entry = data.Entry;
			AssertEquals("ChangeOfRegime", InventoryAutomationAction.ChangeOfRegime, entry.GetInventoryAutomationAction());
			var instruction = data.Instruction;
			instruction.CEI_OH_Owner = helper.Owner.PK;
			AssertEquals("ChangeOfOwnership", InventoryAutomationAction.ChangeOfOwnership, entry.GetInventoryAutomationAction());
			var invoiceLine = data.InvoiceLine;
			var outwardCusProcedure = helper.OutwardCusProcedure;
			instruction.CEI_Style = outwardCusProcedure.ZZ6_ProcedureCode;
			invoiceLine.JI_Procedure = outwardCusProcedure.ZZ6_ProcedureCode + outwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertEquals("Outward", InventoryAutomationAction.Outward, entry.GetInventoryAutomationAction());
			var inwardCusProcedure = helper.InwardCusProcedure;
			instruction.CEI_Style = inwardCusProcedure.ZZ6_ProcedureCode;
			invoiceLine.JI_Procedure = inwardCusProcedure.ZZ6_ProcedureCode + inwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertEquals("Inward", InventoryAutomationAction.Inward, entry.GetInventoryAutomationAction());
		}

		public void TestCharges()
		{
			AssertCharges("Charges : Only include C1_Source = CW1 ot NULL", false);
		}

		public void TestConfirmedCharges()
		{
			AssertCharges("ConfirmedCharges : Only include C1_Source = CUS", true);
		}

		public void TestTotalAmountPayable()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryline1 = entry.MergedLines.AddNew();
			var fee1 = entryline1.Fees.AddNew();
			fee1.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			fee1.CF_ChargeAmount = 2.3m;
			var fee2 = entryline1.ConfirmedFees.AddNew();
			fee2.CF_ChargeAmount = 10.3m;
			var entryline2 = entry.MergedLines.AddNew();
			var fee3 = entryline2.ConfirmedFees.AddNew();
			fee3.CF_ChargeAmount = 7.3m;
			AssertEquals(17.6m, entry.TotalAmountPayable);
		}

		public void TestTotalAmountPayable_NoCUSData()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryline1 = entry.MergedLines.AddNew();
			var fee1 = entryline1.Fees.AddNew();
			fee1.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			fee1.CF_ChargeAmount = 2.3m;
			var entryline2 = entry.MergedLines.AddNew();
			var fee2 = entryline2.Fees.AddNew();
			fee2.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			fee2.CF_ChargeAmount = 7.3m;
			AssertEquals(9.6m, entry.TotalAmountPayable);
		}

		void AssertCharges(ZString message, bool isConfirmedCharges)
		{
			var entry = Factory.New<CusEntryHeader>();
			var charge1 = Factory.New<CusEntryHeaderCharges>();
			charge1.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			charge1.C1_CH = entry.PK;
			var charge2 = Factory.New<CusEntryHeaderCharges>();
			charge2.C1_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			charge2.C1_CH = entry.PK;
			var charge3 = Factory.New<CusEntryHeaderCharges>();
			charge3.C1_IsLandedCostOnly = true;
			charge3.C1_CH = entry.PK;

			CombineAssertions(() =>
			{
				var actualChargesCount = isConfirmedCharges ? entry.ConfirmedCharges.Count : entry.Charges.Count;
				var actualCharges = isConfirmedCharges ? entry.ConfirmedCharges.Select(x => x.PK) : entry.Charges.Select(x => x.PK);
				AssertEquals("Count: " + message, isConfirmedCharges ? 1 : 2, actualChargesCount);
				AssertContainsExactElementsInAnyOrder("Charges" + message, isConfirmedCharges ? new[] { charge1.PK } : new[] { charge2.PK, charge3.PK }, actualCharges);
			});
		}

		public void TestGetContainerOrEquipmentToEntryLineMapping_OnlyEquipmentIsRequired()
		{
			var declaration = Factory.New<JobDeclarationForTestingEquipmentsOnly>();
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();
			var package5 = declaration.Packages.AddNew();
			var package6 = declaration.Packages.AddNew();

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "C3";
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "E1";
			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "E2";
			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "E3";
			package4.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;
			package5.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;
			package6.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.PackagesPivot.AddPivotFor(package1);
			invoiceLine1.PackagesPivot.AddPivotFor(package2);
			invoiceLine1.PackagesPivot.AddPivotFor(package3);
			invoiceLine1.PackagesPivot.AddPivotFor(package4);
			invoiceLine1.PackagesPivot.AddPivotFor(package5);
			invoiceLine1.PackagesPivot.AddPivotFor(package6);
			invoiceLine1.JI_CL = entryLine1.PK;

			CombineAssertions(() =>
			{
				(IDictionary<BaseCusContainer, IReadOnlyCollection<CusEntryLine>> containers, IDictionary<CusEquipment, IReadOnlyCollection<CusEntryLine>> equipments) entry1Mapping = entry1.GetContainerOrEquipmentToEntryLineMapping();

				var entry1MappingContainers = entry1Mapping.containers;
				var entry1MappingEquipments = entry1Mapping.equipments;
				AssertEquals("entry1Mapping.containers.Count", 0, entry1MappingContainers.Count);
				AssertEquals("entry1Mapping.equipments.Count", 3, entry1MappingEquipments.Count);

				AssertContainsExactElementsInAnyOrder("entry1Mapping - container1", new[] { entryLine1 }, entry1MappingEquipments[equipment1]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - container2", new[] { entryLine1 }, entry1MappingEquipments[equipment2]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - container3", new[] { entryLine1 }, entry1MappingEquipments[equipment3]);
			});
		}

		public void TestGetContainerOrEquipmentToEntryLineMapping_OnlyContainerIsRequired()
		{
			var declaration = Factory.New<JobDeclarationForTestingContainersOnly>();
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();
			var package5 = declaration.Packages.AddNew();
			var package6 = declaration.Packages.AddNew();

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "C3";
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "E1";
			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "E2";
			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "E3";
			package4.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;
			package5.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;
			package6.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.PackagesPivot.AddPivotFor(package1);
			invoiceLine1.PackagesPivot.AddPivotFor(package2);
			invoiceLine1.PackagesPivot.AddPivotFor(package3);
			invoiceLine1.PackagesPivot.AddPivotFor(package4);
			invoiceLine1.PackagesPivot.AddPivotFor(package5);
			invoiceLine1.PackagesPivot.AddPivotFor(package6);
			invoiceLine1.JI_CL = entryLine1.PK;

			CombineAssertions(() =>
			{
				(IDictionary<BaseCusContainer, IReadOnlyCollection<CusEntryLine>> containers, IDictionary<CusEquipment, IReadOnlyCollection<CusEntryLine>> equipments) entry1Mapping = entry1.GetContainerOrEquipmentToEntryLineMapping();

				var entry1MappingContainers = entry1Mapping.containers;
				var entry1MappingEquipments = entry1Mapping.equipments;
				AssertEquals("entry1Mapping.containers.Count", 3, entry1MappingContainers.Count);
				AssertEquals("entry1Mapping.equipments.Count", 0, entry1MappingEquipments.Count);

				AssertContainsExactElementsInAnyOrder("entry1Mapping - container1", new[] { entryLine1 }, entry1MappingContainers[container1]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - container2", new[] { entryLine1 }, entry1MappingContainers[container2]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - container3", new[] { entryLine1 }, entry1MappingContainers[container3]);
			});
		}

		public void TestGetContainerOrEquipmentToEntryLineMapping_NoContainerOrEquipmentIsRequired()
		{
			var declaration = Factory.New<JobDeclarationForTestingNonContainersAndEquipments>();
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();
			var package5 = declaration.Packages.AddNew();
			var package6 = declaration.Packages.AddNew();

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "C3";
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "E1";
			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "E2";
			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "E3";
			package4.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;
			package5.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;
			package6.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.PackagesPivot.AddPivotFor(package1);
			invoiceLine1.PackagesPivot.AddPivotFor(package2);
			invoiceLine1.PackagesPivot.AddPivotFor(package3);
			invoiceLine1.PackagesPivot.AddPivotFor(package4);
			invoiceLine1.PackagesPivot.AddPivotFor(package5);
			invoiceLine1.PackagesPivot.AddPivotFor(package6);
			invoiceLine1.JI_CL = entryLine1.PK;

			CombineAssertions(() =>
			{
				(IDictionary<BaseCusContainer, IReadOnlyCollection<CusEntryLine>> containers, IDictionary<CusEquipment, IReadOnlyCollection<CusEntryLine>> equipments) entry1Mapping = entry1.GetContainerOrEquipmentToEntryLineMapping();

				var entry1MappingContainers = entry1Mapping.containers;
				var entry1MappingEquipments = entry1Mapping.equipments;
				AssertEquals("entry1Mapping.containers.Count", 0, entry1MappingContainers.Count);
				AssertEquals("entry1Mapping.equipments.Count", 0, entry1MappingEquipments.Count);
			});
		}

		public void TestGetContainerOrEquipmentToEntryLineMapping()
		{
			var declaration = Factory.New<JobDeclarationForTestingEquipments>();
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			var package4 = declaration.Packages.AddNew();
			var package5 = declaration.Packages.AddNew();
			var package6 = declaration.Packages.AddNew();

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "C1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "C2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "C3";

			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
			package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;

			var equipment1 = declaration.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "E1";
			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "E2";
			var equipment3 = declaration.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "E3";

			package4.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;
			package5.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;
			package6.CW_ContainerNoOrEquipmentNo = equipment3.CEQ_IdentificationNumber;

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			var entry1Line1 = entry1.MergedLines.AddNew();
			var entry1Line2 = entry1.MergedLines.AddNew();
			var entry2Line1 = entry2.MergedLines.AddNew();
			var entry3Line1 = entry3.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.PackagesPivot.AddPivotFor(package1);
			invoiceLine1.PackagesPivot.AddPivotFor(package3);
			invoiceLine1.PackagesPivot.AddPivotFor(package4);
			invoiceLine1.PackagesPivot.AddPivotFor(package6);
			invoiceLine1.JI_CL = entry1Line1.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.PackagesPivot.AddPivotFor(package2);
			invoiceLine2.PackagesPivot.AddPivotFor(package3);
			invoiceLine2.PackagesPivot.AddPivotFor(package5);
			invoiceLine2.PackagesPivot.AddPivotFor(package6);
			invoiceLine2.JI_CL = entry1Line2.PK;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.PackagesPivot.AddPivotFor(package3);
			invoiceLine3.PackagesPivot.AddPivotFor(package6);
			invoiceLine3.JI_CL = entry2Line1.PK;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CL = entry3Line1.PK;

			CombineAssertions(() =>
			{
				(IDictionary<BaseCusContainer, IReadOnlyCollection<CusEntryLine>> containers, IDictionary<CusEquipment, IReadOnlyCollection<CusEntryLine>> equipments) entry1Mapping = entry1.GetContainerOrEquipmentToEntryLineMapping();
				Assert("Should be cached", entry1Mapping == entry1.GetContainerOrEquipmentToEntryLineMapping());
				var entry1MappingContainers = entry1Mapping.containers;
				var entry1MappingEquipments = entry1Mapping.equipments;
				AssertEquals("entry1Mapping.containers.Count", 3, entry1MappingContainers.Count);
				AssertEquals("entry1Mapping.equipments.Count", 3, entry1MappingEquipments.Count);

				AssertContainsExactElementsInAnyOrder("entry1Mapping - container1", new[] { entry1Line1 }, entry1MappingContainers[container1]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - container2", new[] { entry1Line2 }, entry1MappingContainers[container2]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - container3", new[] { entry1Line1, entry1Line2 }, entry1MappingContainers[container3]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - equipment1", new[] { entry1Line1 }, entry1MappingEquipments[equipment1]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - equipment2", new[] { entry1Line2 }, entry1MappingEquipments[equipment2]);
				AssertContainsExactElementsInAnyOrder("entry1Mapping - equipment3", new[] { entry1Line1, entry1Line2 }, entry1MappingEquipments[equipment3]);

				var entry2Mapping = entry2.GetContainerOrEquipmentToEntryLineMapping();
				AssertEquals("entry2Mapping.containers.Count", 1, entry2Mapping.containers.Count);
				AssertEquals("entry2Mapping.equipments.Count", 1, entry2Mapping.equipments.Count);
				AssertContainsExactElementsInAnyOrder("entry2Mapping - container3", new[] { entry2Line1 }, entry2Mapping.containers[container3]);
				AssertContainsExactElementsInAnyOrder("entry2Mapping - equipment3", new[] { entry2Line1 }, entry2Mapping.equipments[equipment3]);

				var entry3Mapping = entry3.GetContainerOrEquipmentToEntryLineMapping();
				AssertEquals("entry3Mapping.containers.Count", 0, entry3Mapping.containers.Count);
				AssertEquals("entry3Mapping.equipments.Count", 0, entry3Mapping.equipments.Count);
			});
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert($"Covered by {nameof(FetchStrategies.Testing.CusEntryHeaderFetchStrategyTest)}.", true);
		}

		public void TestPivotToContainerIsDeleted()
		{
			var declarationMock = Factory.New<DummyBaseJobDeclaration_ForTest>();
			declarationMock.SupportContainerEntryHeaderPivotReturns = true;
			var declaration = declarationMock;
			var container = declaration.CusContainers.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entry.PK;
			pivot.CCE_CO_Container = container.PK;
			Factory.Save();

			entry.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals("pivot.IsDeleted", true, pivot.IsDeleted);
		}

		public void TestPivotsToContainersIsNotLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var pivot = Factory.New<CusContainerEntryHeaderPivot>();
			pivot.CCE_CH_EntryHeader = entry.PK;
			pivot.CCE_CO_Container = container.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDecMock = newFactory.LoadMoq<BaseJobDeclaration>(declaration.PK);
			loadedDecMock.Setup(m => m.SupportContainerEntryHeaderPivot).Returns(false);

			declaration = loadedDecMock.Object;
			entry = declaration.CustomsEntryHeaders.Single();
			entry.LoadChildEditableObjects();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusContainerEntryHeaderPivot.Schema.TableName));
			AssertEquals(0, entry.PivotsToContainers.Count);
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusContainerEntryHeaderPivot.Schema.TableName));

			newFactory = new BusinessObjectFactory();

			var loadedDecMock2 = newFactory.LoadMoq<BaseJobDeclaration>(declaration.PK);
			loadedDecMock2.Setup(m => m.SupportContainerEntryHeaderPivot).Returns(true);

			declaration = loadedDecMock2.Object;
			entry = declaration.CustomsEntryHeaders.Single();
			entry.LoadChildEditableObjects();
			AssertEquals("db hits", 1, newFactory.GetTableHitCount(CusContainerEntryHeaderPivot.Schema.TableName));
			AssertEquals(1, entry.PivotsToContainers.Count);
		}

		public void TestIWarehouseIntegrationSupporter_SupportModificationState()
		{
			AssertEquals(false, ((IWarehouseIntegrationSupporter)Factory.New<CusEntryHeader>()).SupportModificationState);
		}

		public void TestInvoiceEffectiveUCR()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line = invoice1.JobComInvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line = invoice2.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			invoice1Line.JI_CL = entryLine1.PK;
			var entryLine2 = entry.MergedLines.AddNew();
			invoice2Line.JI_CL = entryLine2.PK;
			CombineAssertions(() =>
			{
				AssertEquals("InvoiceEffectiveUCR", ZString.Empty, entry.InvoiceEffectiveUCR);
				declaration.JE_UCR = "DECUCR";
				AssertEquals("InvoiceEffectiveUCR should come from Declaration", "DECUCR", entry.InvoiceEffectiveUCR);
				invoice2.JZ_UCR = "INVUCR";
				AssertEquals("InvoiceEffectiveUCR should be empty as not all invoice have same EffectiveUCR", ZString.Empty, entry.InvoiceEffectiveUCR);
				invoice1.JZ_UCR = "INVUCR";
				AssertEquals("InvoiceEffectiveUCR should be same as all invoice.EffectiveUCR", "INVUCR", entry.InvoiceEffectiveUCR);
			});
		}

		public void TestHasWHSStatus()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var list = new WarehouseTransactionStatusList();
			foreach (var code in new[]
			{
				"", "%",
				WarehouseTransactionStatusList.Codes.AutomationIsDisabled,
				WarehouseTransactionStatusList.Codes.Multiple,
				WarehouseTransactionStatusList.Codes.InwardCanceled,
				WarehouseTransactionStatusList.Codes.OutwardCanceled,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCanceled,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCanceled
			})
			{
				list.RemoveCode(code);
				entry.CH_WarehouseTransactionStatus = code;
				AssertEquals("HasWHSTransaction", code == "%" || code == WarehouseTransactionStatusList.Codes.Multiple, entry.HasWHSTransaction);
				AssertEquals("HasWHSInwardTransaction", false, entry.HasWHSInwardTransaction);
				AssertEquals("HasWHSOutwardTransaction", false, entry.HasWHSOutwardTransaction);
				AssertEquals("HasWHSChangeOfOwnershipTransaction", false, entry.HasWHSChangeOfOwnershipTransaction);
				AssertEquals("HasWHSChangeOfRegimeTransaction", false, entry.HasWHSChangeOfRegimeTransaction);
			}
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.InwardCreated,
				WarehouseTransactionStatusList.Codes.InwardCreatedPending,
				WarehouseTransactionStatusList.Codes.InwardCreationHeld,
				WarehouseTransactionStatusList.Codes.InwardUpdated,
				WarehouseTransactionStatusList.Codes.InwardUpdatedPending
			})
			{
				list.RemoveCode(code);
				entry.CH_WarehouseTransactionStatus = code;
				AssertEquals("HasWHSTransaction", true, entry.HasWHSTransaction);
				AssertEquals("HasWHSInwardTransaction", true, entry.HasWHSInwardTransaction);
				AssertEquals("HasWHSOutwardTransaction", false, entry.HasWHSOutwardTransaction);
				AssertEquals("HasWHSChangeOfOwnershipTransaction", false, entry.HasWHSChangeOfOwnershipTransaction);
				AssertEquals("HasWHSChangeOfRegimeTransaction", false, entry.HasWHSChangeOfRegimeTransaction);
			}
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal,
				WarehouseTransactionStatusList.Codes.OutwardCreated,
				WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
				WarehouseTransactionStatusList.Codes.OutwardHolding,
				WarehouseTransactionStatusList.Codes.OutwardUpdated,
				WarehouseTransactionStatusList.Codes.OutwardUpdatedPending
			})
			{
				list.RemoveCode(code);
				entry.CH_WarehouseTransactionStatus = code;
				AssertEquals("HasWHSTransaction", true, entry.HasWHSTransaction);
				AssertEquals("HasWHSInwardTransaction", false, entry.HasWHSInwardTransaction);
				AssertEquals("HasWHSOutwardTransaction", true, entry.HasWHSOutwardTransaction);
				AssertEquals("HasWHSChangeOfOwnershipTransaction", false, entry.HasWHSChangeOfOwnershipTransaction);
				AssertEquals("HasWHSChangeOfRegimeTransaction", false, entry.HasWHSChangeOfRegimeTransaction);
			}
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreated,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipCreatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipHolding,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdated,
				WarehouseTransactionStatusList.Codes.ChangeOfOwnershipUpdatedPending
			})
			{
				list.RemoveCode(code);
				entry.CH_WarehouseTransactionStatus = code;
				AssertEquals("HasWHSTransaction", true, entry.HasWHSTransaction);
				AssertEquals("HasWHSInwardTransaction", false, entry.HasWHSInwardTransaction);
				AssertEquals("HasWHSOutwardTransaction", false, entry.HasWHSOutwardTransaction);
				AssertEquals("HasWHSChangeOfOwnershipTransaction", true, entry.HasWHSChangeOfOwnershipTransaction);
				AssertEquals("HasWHSChangeOfRegimeTransaction", false, entry.HasWHSChangeOfRegimeTransaction);
			}
			foreach (var code in new[]
			{
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreated,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeCreatedPending,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeHolding,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdated,
				WarehouseTransactionStatusList.Codes.ChangeOfRegimeUpdatedPending
			})
			{
				list.RemoveCode(code);
				entry.CH_WarehouseTransactionStatus = code;
				AssertEquals("HasWHSTransaction", true, entry.HasWHSTransaction);
				AssertEquals("HasWHSInwardTransaction", false, entry.HasWHSInwardTransaction);
				AssertEquals("HasWHSOutwardTransaction", false, entry.HasWHSOutwardTransaction);
				AssertEquals("HasWHSChangeOfOwnershipTransaction", false, entry.HasWHSChangeOfOwnershipTransaction);
				AssertEquals("HasWHSChangeOfRegimeTransaction", true, entry.HasWHSChangeOfRegimeTransaction);
			}
			AssertEquals("Should be empty", 0, list.Count);
		}

		public void TestIsBondedWarehousingDisabled()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("IsBondedWarehousingDisabled", false, entry.IsBondedWarehousingDisabled);
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			AssertEquals("IsBondedWarehousingDisabled", true, entry.IsBondedWarehousingDisabled);
		}

		public void TestBondedWarehouseMenuItemText()
		{
			var declaration = Factory.New<BaseJobDeclarationWithEntryInstructions>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AB";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("BondedWarehouseMenuItemText", "", entry.EntryHeaderDescriptiveMenuItemText);
			entry.CH_CEI_Instruction = entryInstruction.PK;
			AssertEquals("BondedWarehouseMenuItemText", "AB", entry.EntryHeaderDescriptiveMenuItemText);
			entry.EntryNumber = "BYE";
			AssertEquals("BondedWarehouseMenuItemText", "AB - BYE", entry.EntryHeaderDescriptiveMenuItemText);
			entryInstruction.CEI_Description = "HELLO";
			AssertEquals("BondedWarehouseMenuItemText", "AB - HELLO - BYE", entry.EntryHeaderDescriptiveMenuItemText);
		}

		public void TestPublishEventForDisablingIntegration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertNull(entry.Logs.MostRecentLogByEventTime(Events.WarehouseIntegrationDisabled));
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
			AssertNotNull(entry.Logs.MostRecentLogByEventTime(Events.WarehouseIntegrationDisabled));
		}

		public void TestIsChangeOfRegimeWarehousing()
		{
			var helper = new WhsDataTestHelper(Factory);
			var data = helper.CreateChangeOfRegimeEntryData();
			var entry = data.Entry;
			AssertEquals("IsChangeOfRegimeWarehousing", true, entry.IsChangeOfRegimeWarehousing);

			var instruction = data.Instruction;
			instruction.CEI_OH_Owner = helper.Owner.PK;
			AssertEquals("IsChangeOfRegimeWarehousing", false, entry.IsChangeOfRegimeWarehousing);
		}

		public void TestIsChangeOfRegimeWarehousingEnabled()
		{
			var helper = new WhsDataTestHelper(Factory);
			var data = helper.CreateChangeOfRegimeEntryData();
			var entry = data.Entry;
			AssertEquals("IsChangeOfRegimeWarehousingEnabled", true, entry.IsChangeOfRegimeWarehousingEnabled);

			var instruction = data.Instruction;
			instruction.CEI_OH_Owner = helper.Owner.PK;
			AssertEquals("IsChangeOfRegimeWarehousingEnabled", false, entry.IsChangeOfRegimeWarehousingEnabled);
		}

		[TestDate(2021, 4, 9)]
		public void TestIsIntoTemporaryImportEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = true;

				var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TMPIMP, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CombineAssertions(() =>
					{
						procedure.ZZ6_IntoTemporaryImport = YesNoList.Codes.Yes;
						AssertEquals("ZZ6_IntoTemporaryImport", true, entry.IsIntoTemporaryImportEnabled);

						procedure.ZZ6_IntoTemporaryImport = YesNoList.Codes.No;
						AssertEquals("ZZ6_IntoTemporaryImport not set", false, entry.IsIntoTemporaryImportEnabled);
					});
				}
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestIsOutOfTemporaryImportEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryImports = true;

				var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TMPIMP, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CombineAssertions(() =>
					{
						procedure.ZZ6_OutOfTemporaryImport = YesNoList.Codes.Yes;
						AssertEquals("ZZ6_OutOfTemporaryImport", true, entry.IsOutOfTemporaryImportEnabled);

						procedure.ZZ6_OutOfTemporaryImport = YesNoList.Codes.No;
						AssertEquals("ZZ6_OutOfTemporaryImport not set", false, entry.IsOutOfTemporaryImportEnabled);
					});
				}
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestIsIntoTemporaryExportEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = true;

				var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TMPEXP, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CombineAssertions(() =>
					{
						procedure.ZZ6_IntoTemporaryExport = YesNoList.Codes.Yes;
						AssertEquals("ZZ6_IntoTemporaryExport", true, entry.IsIntoTemporaryExportEnabled);

						procedure.ZZ6_IntoTemporaryExport = YesNoList.Codes.No;
						AssertEquals("ZZ6_IntoTemporaryExport not set", false, entry.IsIntoTemporaryExportEnabled);
					});
				}
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestIsOutOfTemporaryExportEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.WarehouseClient.CompanyData.OB_CusInventoryForTemporaryExports = true;

				var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.TMPEXP, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CombineAssertions(() =>
					{
						procedure.ZZ6_OutOfTemporaryExport = YesNoList.Codes.Yes;
						AssertEquals("ZZ6_OutOfTemporaryExport", true, entry.IsOutOfTemporaryExportEnabled);

						procedure.ZZ6_OutOfTemporaryExport = YesNoList.Codes.No;
						AssertEquals("ZZ6_OutOfTemporaryExport not set", false, entry.IsOutOfTemporaryExportEnabled);
					});
				}
			}
		}

		public void TestIsInProcessOfMerging()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			CombineAssertions(() =>
			{
				declaration.IsMergeInProgress = true;
				AssertEquals("CusEntryHeader.IsInProcessOfMerging should be true", true, entryHeader.IsInProcessOfMerging);

				declaration.IsMergeInProgress = false;
				AssertEquals("CusEntryHeader.IsInProcessOfMerging should be false", false, entryHeader.IsInProcessOfMerging);
			});
		}

		[TestDate(2021, 4, 9)]
		public void TestIsIntoInwardProcessingEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;

				var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.IWDPROC, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CombineAssertions(() =>
					{
						procedure.ZZ6_IntoInwardProcessing = YesNoList.Codes.Yes;
						AssertEquals("ZZ6_IntoInwardProcessing", true, entry.IsIntoInwardProcessingEnabled);

						procedure.ZZ6_IntoInwardProcessing = YesNoList.Codes.No;
						AssertEquals("ZZ6_IntoInwardProcessing not set", false, entry.IsIntoInwardProcessingEnabled);
					});
				}
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestIsOutOfInwardProcessingEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.WarehouseClient.CompanyData.OB_CusInventoryForInwardProcessing = true;

				var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.IWDPROC, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CombineAssertions(() =>
					{
						procedure.ZZ6_OutOfInwardProcessing = YesNoList.Codes.Yes;
						AssertEquals("ZZ6_OutOfInwardProcessing", true, entry.IsOutOfInwardProcessingEnabled);

						procedure.ZZ6_OutOfInwardProcessing = YesNoList.Codes.No;
						AssertEquals("ZZ6_OutOfInwardProcessing not set", false, entry.IsOutOfInwardProcessingEnabled);
					});
				}
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestIsIntoOutwardProcessingEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = true;

				var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.OWDPROC, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CombineAssertions(() =>
					{
						procedure.ZZ6_IntoOutwardProcessing = YesNoList.Codes.Yes;
						AssertEquals("ZZ6_IntoOutwardProcessing", false, entry.IsIntoOutwardProcessingEnabled);

						procedure.ZZ6_IntoOutwardProcessing = YesNoList.Codes.No;
						AssertEquals("ZZ6_IntoOutwardProcessing not set", false, entry.IsIntoOutwardProcessingEnabled);
					});
				}
			}
		}

		[TestDate(2021, 4, 9)]
		public void TestIsOutOfOutwardProcessingEnabled()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "AB";
				procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_OH_Importer = Factory.New<OrgHeader>().PK;
				dec.JE_OH_Supplier = Factory.New<OrgHeader>().PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Export;
				dec.WarehouseClient.CompanyData.OB_CusInventoryForOutwardProcessing = true;

				var entryInstruction = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = dec.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var invoiceHeader = dec.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.OWDPROC, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.Today, true))
				using (DataRegistry.Business.CustomsDataRegistry.Instance.EnableByProductFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					CombineAssertions(() =>
					{
						procedure.ZZ6_IntoOutwardProcessing = YesNoList.Codes.Yes;
						AssertEquals("ZZ6_IntoOutwardProcessing", false, entry.IsIntoOutwardProcessingEnabled);

						procedure.ZZ6_IntoOutwardProcessing = YesNoList.Codes.No;
						AssertEquals("ZZ6_IntoOutwardProcessing not set", false, entry.IsIntoOutwardProcessingEnabled);
					});
				}
			}
		}

		public void TestGetMessageErrorOfRequiredFieldsForBondedWarehousing_SupplierIsWarehouseClient()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "BOB1";
			supplier.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_RL_NKClosestPort = "ZZABC";
			warehouse.MainAddress.OA_Address1 = "ADD 1";
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ProcedureCode = "AB";
			procedure1.ZZ6_PreviousProcedureCode = "10";
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ProcedureCode = "CD";
			procedure2.ZZ6_PreviousProcedureCode = "10";
			procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure2.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			var declarationMockProtected = declarationMock.Protected();
			declarationMockProtected.Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(false);
			declarationMockProtected.Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);

			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected()
				.Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns(true);

			declarationMockProtected.Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
			declarationMock.Setup(x => x.TermNameForBondedWarehouse).Returns("Bob's System");

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.SetSupportsBondedWarehousingForTesting(true);

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AB";
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			warehouse.OH_RL_NKClosestPort = "ZAJNB";
			var entryMock = Factory.NewMoq<CusEntryHeader>();
			entryMock.Protected().Setup<bool>("IsInwardBondedWarehousingEnabledCore").Returns(true);
			var entry = entryMock.Object;
			entry.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entry);
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryNumber = "B32342";
			var entryLine = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "AB10";

			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			const string messageError = "A Supplier marked as a Warehouse Client is needed when Bob's System is enabled.";
			AssertContains(messageError, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));

			supplier.OH_IsWarehouseClient = true;
			AssertNotContains(messageError, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
		}

		public void TestCH_HighestLineNumberWhenAllLinesAreDeleted()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = Factory.New<CusEntryHeaderWithStatus>();
			entry.MergedLines.AddNew().CL_LineNumber = 1;
			entry.CH_JE = declaration.PK;
			entry.CH_Status = "CLR";
			AssertEquals((short)1, entry.CH_HighestLineNumber);

			entry.MergedLines.RemoveAndDeleteAll();
			entry.CH_Status = "XXX";
			entry.CH_Status = "WDN";
			AssertEquals((short)1, entry.CH_HighestLineNumber);
		}

		class CusEntryHeaderWithStatus : CusEntryHeader
		{
			public CusEntryHeaderWithStatus(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsStatusClear(string status)
			{
				return status == "WDN" || status == "CLR";
			}
		}

		public void TestGetMessageErrorOfRequiredFieldsForBondedWarehousing_ImporterIsWarehouseClient()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_RL_NKClosestPort = "ZZABC";
			warehouse.MainAddress.OA_Address1 = "ADD 1";
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ProcedureCode = "AB";
			procedure1.ZZ6_PreviousProcedureCode = "10";
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ProcedureCode = "CD";
			procedure2.ZZ6_PreviousProcedureCode = "10";
			procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure2.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;

			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			var declarationMockProtected = declarationMock.Protected();
			declarationMockProtected.Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(false);
			declarationMockProtected.Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);

			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected()
				.Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns(true);

			declarationMockProtected.Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
			declarationMock.Setup(x => x.TermNameForBondedWarehouse).Returns("Bob's System");

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SetSupportsBondedWarehousingForTesting(true);

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AB";
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			warehouse.OH_RL_NKClosestPort = "ZAJNB";
			var entryMock = Factory.NewMoq<CusEntryHeader>();
			entryMock.Protected().Setup<bool>("IsInwardBondedWarehousingEnabledCore").Returns(true);
			var entry = entryMock.Object;
			entry.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entry);
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryNumber = "B32342";
			var entryLine = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "AB10";

			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			const string messageError = "An Importer marked as a Warehouse Client is needed when Bob's System is enabled.";
			AssertContains(messageError, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));

			importer.OH_IsWarehouseClient = true;
			AssertNotContains(messageError, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));

			importer.OH_IsWarehouseClient = false;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertNotContains(messageError, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
		}

		public void TestGetMessageErrorOfRequiredFieldsForBondedWarehousing_WarehouseAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var importer = Factory.New<OrgHeader>();
				importer.OH_Code = "BOB1";
				importer.OH_IsWarehouseClient = true;
				importer.CompanyData.OB_IMUsedBondedWhs = true;
				var warehouse = Factory.New<OrgHeader>();
				warehouse.OH_RL_NKClosestPort = "ZZABC";
				warehouse.MainAddress.OA_Address1 = "ADD 1";
				var procedure1 = Factory.New<RefCusProcedure>();
				procedure1.ZZ6_ProcedureCode = "AB";
				procedure1.ZZ6_PreviousProcedureCode = "10";
				procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
				procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
				var procedure2 = Factory.New<RefCusProcedure>();
				procedure2.ZZ6_ProcedureCode = "CD";
				procedure2.ZZ6_PreviousProcedureCode = "10";
				procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
				procedure2.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = importer.PK;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "AB";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = entryInstruction.PK;
				entry.EntryNumber = "B32342";
				var entryLine = entry.MergedLines.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				invoiceLine.JI_CL = entryLine.PK;
				invoiceLine.JI_Procedure = "AB10";
				AssertXMLEquals(CusEntryHeader.BondedWarehouseIsRequiredForInwardBondedWarehousing("BOB1", entry.EntryHeaderDescriptiveMenuItemText), entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
				AssertNull(entry.GetIntoWarehouseAddress());
				AssertNull(entry.GetOutOfWarehouseAddress());
				entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
				AssertXMLEquals(CusEntryHeader.BondedWarehouseAddressShouldBeInsideDeclarationCountry("BOB1", entry.EntryHeaderDescriptiveMenuItemText, "South Africa"), entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
				AssertEquals(warehouse.MainAddress, entry.GetIntoWarehouseAddress());
				AssertNull(entry.GetOutOfWarehouseAddress());
				warehouse.OH_RL_NKClosestPort = "ZAJNB";
				AssertEquals("", entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
				entryInstruction.CEI_Style = "CD";
				AssertXMLEquals(CusEntryHeader.BondedWarehouseIsRequiredForOutwardBondedWarehousing("BOB1", entry.EntryHeaderDescriptiveMenuItemText), entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
				AssertEquals(warehouse.MainAddress, entry.GetIntoWarehouseAddress());
				AssertNull(entry.GetOutOfWarehouseAddress());
				entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
				AssertEquals("", entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
				AssertEquals(warehouse.MainAddress, entry.GetIntoWarehouseAddress());
				AssertEquals(warehouse.MainAddress, entry.GetOutOfWarehouseAddress());
				warehouse.OH_RL_NKClosestPort = "ZZABC";
				AssertXMLEquals(CusEntryHeader.BondedWarehouseAddressShouldBeInsideDeclarationCountry("BOB1", entry.EntryHeaderDescriptiveMenuItemText, "South Africa"), entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
			}
		}

		public void TestGetMessageErrorOfRequiredFieldsForBondedWarehousing_WarehouseAddress_Supplier()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "BOB1";
			supplier.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_RL_NKClosestPort = "ZZABC";
			warehouse.MainAddress.OA_Address1 = "ADD 1";
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ProcedureCode = "AB";
			procedure1.ZZ6_PreviousProcedureCode = "10";
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ProcedureCode = "CD";
			procedure2.ZZ6_PreviousProcedureCode = "10";
			procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure2.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;

			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			var declarationMockProtected = declarationMock.Protected();
			declarationMockProtected.Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(false);
			declarationMockProtected.Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);

			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected()
				.Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns(true);

			declarationMockProtected.Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);

			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.SetSupportsBondedWarehousingForTesting(true);

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AB";
			var entryMock = Factory.NewMoq<CusEntryHeader>();
			entryMock.Protected().Setup<bool>("IsInwardBondedWarehousingEnabledCore").Returns(true);
			var entry = entryMock.Object;
			entry.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entry);
			entry.CH_CEI_Instruction = entryInstruction.PK;
			entry.EntryNumber = "B32342";
			var entryLine = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Procedure = "AB10";
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			warehouse.OH_RL_NKClosestPort = "ZAJNB";

			AssertNotContains("no BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage", BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage(declaration.TermNameForBondedWarehouse), entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "O236";
			supplier2.MainAddress.OA_Address1 = "1";
			invoice.JZ_OH_Supplier = supplier2.PK;
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertContains("BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage", BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage(declaration.TermNameForBondedWarehouse), entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));

			AssertNotContains("no SupplierDocumentaryAddressIsRequiredForBondedWarehousing", declaration.SupplierDocumentaryAddressIsRequiredForBondedWarehousing, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
			declaration.SupplierDocumentaryAddress.Delete();
			AssertContains("SupplierDocumentaryAddressIsRequiredForBondedWarehousing", declaration.SupplierDocumentaryAddressIsRequiredForBondedWarehousing, entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(false, false, false));
		}

		public void TestHasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "O234";
			importer.MainAddress.OA_Address1 = "1";
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			var declarationMock = Factory.NewMoq<BaseJobDeclarationWithEntryInstructions>();
			var declarationMockProtected = declarationMock.Protected();
			declarationMockProtected.Setup<bool>("SupportMultipleWarehouseEntryCore").Returns(true);

			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns<BaseJobComInvoiceLine>((targetInvoiceLine) =>
				{
					return targetInvoiceLine.Declaration.IsExWarehouse ? targetInvoiceLine.UseBondedWarehouseAutomation : targetInvoiceLine.IsGoingIntoBondedWarehouse;
				});

			declarationMockProtected.Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);

			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var helper = new WhsDataTestHelper(Factory);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_CEI = instruction.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_CEI = instruction.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertCollectionContains(entry, declaration.CustomsEntryHeaders);
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine1.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			invoiceLine2.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine1.JI_BondedWhsQuantity = 1m;
			invoiceLine1.JI_BondedWhsUnitQty = "NO";
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine1.JI_BondedWhsUnitQty = ZString.Empty;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine1.JI_BondedWhsUnitQty = "NO";
			invoiceLine1.JI_BondedWhsQuantity = 0m;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine1.JI_BondedWhsQuantity = 1m;
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			instruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine2.JI_BondedWhsQuantity = 1m;
			invoiceLine2.JI_BondedWhsUnitQty = "NO";
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine2.JI_BondedWhsUnitQty = "";
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine2.JI_BondedWhsUnitQty = "NO";
			invoiceLine2.JI_BondedWhsQuantity = 0m;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
			invoiceLine2.JI_BondedWhsQuantity = 1m;
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
		}

		public void TestHasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantity()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "O234";
			importer.MainAddress.OA_Address1 = "1";
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.GetIsWHSUniversalXMLActiveReturns = true;
			declaration.IsInvoiceQuantityRequiredForBondedWarehouseReturns = true;

			Mock<BondedWarehousingHelper> helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns<BaseJobComInvoiceLine>((targetInvoiceLine) =>
				{
					return targetInvoiceLine.Declaration.IsExWarehouse ? targetInvoiceLine.UseBondedWarehouseAutomation : targetInvoiceLine.IsGoingIntoBondedWarehouse;
				});

			declaration.GetNewBondedWarehousingHelperReturns = helperMock.Object;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertCollectionContains(entry, declaration.CustomsEntryHeaders);
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine2.SetUseBondedWarehouseAutomationForTesting(true);
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_InvoiceUQ = "NO";
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine1.JI_InvoiceUQ = ZString.Empty;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine1.JI_InvoiceUQ = "NO";
			invoiceLine1.JI_InvoiceQuantity = 0m;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine1.JI_InvoiceQuantity = 1m;
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_InvoiceUQ = "NO";
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine2.JI_InvoiceUQ = "";
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_InvoiceQuantity = 0m;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
			invoiceLine2.JI_InvoiceQuantity = 1m;
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
		}

		public void TestHasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "O234";
			importer.MainAddress.OA_Address1 = "1";
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.GetIsWHSUniversalXMLActiveReturns = true;

			Mock<BondedWarehousingHelper> helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected().Setup<bool>("HasBondedWarehouseEntryDetailsCore",
				ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns(true);

			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns<BaseJobComInvoiceLine>((targetInvoiceLine) =>
				{
					return targetInvoiceLine.Declaration.IsExWarehouse ? targetInvoiceLine.UseBondedWarehouseAutomation : targetInvoiceLine.IsGoingIntoBondedWarehouse;
				});

			declaration.GetNewBondedWarehousingHelperReturns = helperMock.Object;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertCollectionContains(entry, declaration.CustomsEntryHeaders);
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails);
			invoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine2.SetUseBondedWarehouseAutomationForTesting(true);
			Factory.InvalidateCachedProperties();
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails);
			helperMock.Protected().Setup<bool>("HasBondedWarehouseEntryDetailsCore",
				ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns<BaseJobComInvoiceLine, bool, bool, bool>((targetInvoiceLine, bool1, bool2, bool3) =>
				{
					return invoiceLine1 == targetInvoiceLine;
				});

			Factory.InvalidateCachedProperties();
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails);

			helperMock.Protected().Setup<bool>("HasBondedWarehouseEntryDetailsCore",
				ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns(true);

			Factory.InvalidateCachedProperties();
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails);
		}

		public void TestHasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct_NonAssembledProduct()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var consignor = CreateOrgHeader("ORGAUSYD", "AUSYD");
				var consignee = CreateOrgHeader("ORGUSCHI", "USCHI");

				var product = CreateOrgSupplierPart("PART1", 100m, "HG", consignor, consignee);

				(var declaration, var entry) = SetupDeclaration(consignor, consignee, product);

				var helperMock = SetupBondedWarehousingHelperMockForBondedWarehouseEntryDetails(declaration);
				declaration.GetNewBondedWarehousingHelperReturns = helperMock.Object;

				AssertCollectionContains("PreReq", entry, declaration.CustomsEntryHeaders);
				AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails", true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);
			}
		}

		public void TestHasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct_AssembledProduct()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var consignor = CreateOrgHeader("ORGAUSYD", "AUSYD");
				var consignee = CreateOrgHeader("ORGUSCHI", "USCHI");

				var product = CreateOrgSupplierPart("PART1", 100m, "HG", consignor, consignee);

				var componentProduct = Factory.New<OrgSupplierPart>();
				product.OP_PartNum = "12345";
				var component = product.BillOfMaterials.AddNew();
				component.OE_OP_Component = componentProduct.PK;

				(var declaration, var entry) = SetupDeclaration(consignor, consignee, product);

				var helperMock = SetupBondedWarehousingHelperMockForBondedWarehouseEntryDetails(declaration);
				declaration.GetNewBondedWarehousingHelperReturns = helperMock.Object;

				AssertCollectionContains("PreReq", entry, declaration.CustomsEntryHeaders);
				AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);
			}
		}

		OrgHeader CreateOrgHeader(string code, string closestPort)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			orgHeader.OH_Code = code;
			orgHeader.OH_IsConsignor = true;
			orgHeader.OH_RL_NKClosestPort = closestPort;
			return orgHeader;
		}

		OrgSupplierPart CreateOrgSupplierPart(string partNum, decimal weight, string weightUQ, OrgHeader consignor, OrgHeader consignee)
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = partNum;
			product.OP_Weight = weight;
			product.OP_WeightUQ = weightUQ;
			product.RelatedOrganisations.AddSupplier(consignor);
			product.RelatedOrganisations.AddOwner(consignee);
			return product;
		}

		(BaseJobDeclarationForTesting, CusEntryHeader) SetupDeclaration(OrgHeader consignor, OrgHeader consignee, OrgSupplierPart product)
		{
			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.GetIsWHSUniversalXMLActiveReturns = true;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_PartNo = product.OP_PartNum;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return (declaration, entry);
		}

		Mock<BondedWarehousingHelper> SetupBondedWarehousingHelperMockForBondedWarehouseEntryDetails(BaseJobDeclarationForTesting declaration)
		{
			var helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected().Setup<bool>("HasBondedWarehouseEntryDetailsCore",
				ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns(false);

			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns<BaseJobComInvoiceLine>((x) => true);

			return helperMock;
		}

		public void TestHasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct_NoProduct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "O234";
			importer.MainAddress.OA_Address1 = "1";
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			var declaration = Factory.New<BaseJobDeclarationForTesting>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.GetIsWHSUniversalXMLActiveReturns = true;

			Mock<BondedWarehousingHelper> helperMock = new Mock<BondedWarehousingHelper>(declaration);
			helperMock.Protected().Setup<bool>("HasBondedWarehouseEntryDetailsCore",
				ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns(false);

			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns<BaseJobComInvoiceLine>((targetInvoiceLine) =>
				{
					return targetInvoiceLine.Declaration.IsExWarehouse ? targetInvoiceLine.UseBondedWarehouseAutomation : targetInvoiceLine.IsGoingIntoBondedWarehouse;
				});

			declaration.GetNewBondedWarehousingHelperReturns = helperMock.Object;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.Delete();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertCollectionContains(entry, declaration.CustomsEntryHeaders);
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);
			invoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine2.SetUseBondedWarehouseAutomationForTesting(true);
			Factory.InvalidateCachedProperties();
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);

			helperMock.Protected().Setup<bool>("HasBondedWarehouseEntryDetailsCore",
				ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns<BaseJobComInvoiceLine, bool, bool, bool>((targetInvoiceLine, bool1, bool2, bool3) =>
				{
					return invoiceLine1 == targetInvoiceLine;
				});

			Factory.InvalidateCachedProperties();
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);

			helperMock.Protected().Setup<bool>("HasBondedWarehouseEntryDetailsCore",
				ItExpr.IsAny<BaseJobComInvoiceLine>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>())
				.Returns(true);

			Factory.InvalidateCachedProperties();
			AssertEquals(false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);
		}

		public void TestHasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct()
		{
			var declarationMock = Factory.NewMoq<DummyDeclarationWithIntegrationSupport>();
			var dec = declarationMock.Object;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			dec.SupportMultipleWarehouseEntryCoreExposed = false;

			var declarationMockProtected = declarationMock.Protected();
			var helperMock = new Mock<BondedWarehousingHelper>(dec);
			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>())
				.Returns<BaseJobComInvoiceLine>((targetInvoiceLine) =>
				{
					return targetInvoiceLine.Declaration.IsExWarehouse ? targetInvoiceLine.UseBondedWarehouseAutomation : targetInvoiceLine.IsGoingIntoBondedWarehouse;
				});

			declarationMockProtected.Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);

			dec.JE_SystemCreateTimeUtc = ZDateTime.Today;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var importerPK = dec.JE_OH_Importer;
			var entry = dec.CustomsEntryHeaders.AddNew();
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			var invoice = dec.Invoices.AddNew();
			var line1 = invoice.JobComInvoiceLines.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			line1.JI_CL = entryLine1.PK;
			line1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertCollectionContains(entry, dec.CustomsEntryHeaders);
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			dec.JE_OH_Importer = ZGuid.Empty;
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			dec.JE_OH_Importer = importerPK;
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line1.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line1.JI_PartNo = "PART1";
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~1";
			part.OP_StockKeepingUnit = "NO";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importerPK, OrgPartRelation.RelationshipTypes.Owner);

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			classification.CC_LookupCode = "~~1L";
			classification.CC_TariffNum = "0000000000";

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;

			line1.JI_PartNo = part.OP_PartNum;
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			var line2 = invoice.JobComInvoiceLines.AddNew();
			var entryLine2 = entry.MergedLines.AddNew();
			line2.JI_CL = entryLine2.PK;
			line2.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertCollectionContains(entry, dec.CustomsEntryHeaders);
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line2.JI_PartNo = part.OP_PartNum;
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			dec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line1.JI_PartNo = ZString.Empty;
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line1.SetUseBondedWarehouseAutomationForTesting(true);
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line1.JI_PartNo = part.OP_PartNum;
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line2.JI_PartNo = "";
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line2.SetUseBondedWarehouseAutomationForTesting(true);
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);

			line2.JI_PartNo = part.OP_PartNum;
			AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct", false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAProduct);
		}

		#region TestEntryNumberType
		public void TestEntryNumberType()
		{
			//IMP
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "IMS213";
			Factory.InvalidateCachedProperties();
			AssertEquals("Entry Type", Customs.Business.JobMessageTypeList.Codes.Import, entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("Entry Number", "IMS213", entryHeader.CusEntryNumber.CE_EntryNum);

			//Based on JE_MessageType
			entryHeader.CusEntryNumber.Delete();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.EntryNumber = "IMS213";
			Factory.InvalidateCachedProperties();
			AssertEquals("Entry Type", Customs.Business.JobMessageTypeList.Codes.Export, entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("Entry Number", "IMS213", entryHeader.CusEntryNumber.CE_EntryNum);

			//Based on CH_MessageType
			entryHeader.CusEntryNumber.Delete();
			entryHeader.CH_MessageType = "I33";
			entryHeader.EntryNumber = "IMS213";
			Factory.InvalidateCachedProperties();
			AssertEquals("Entry Type", "I33", entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("Entry Number", "IMS213", entryHeader.CusEntryNumber.CE_EntryNum);
		}

		#endregion

		#region TestCusEntryNumber

		public void TestCusEntryNumber()
		{
			var decl = BaseJobDeclaration.New(Factory);
			var entryHeader = decl.CustomsEntryHeaders.AddNew();

			AssertNull(entryHeader.CusEntryNumber);

			entryHeader.EntryNumber = "1M15353189291";
			AssertNotNull("CusEntryNumber should be not null", entryHeader.CusEntryNumber);
			AssertEquals("1M15353189291", entryHeader.CusEntryNumber.CE_EntryNum);

			entryHeader.EntryNumber = "";
			AssertNull("CusEntryNumber should be null because corresponding record was deleted", entryHeader.CusEntryNumber);

			var number = CusEntryNumber.New(entryHeader, decl.JE_MessageType, decl.CountryCode);
			number.CE_EntryNum = "EX123";
			AssertEquals("CusEntryNum should have reference to EntryHeader", entryHeader.PK, number.CE_ParentID);

			number.CE_RN_NKCountryCode = ZString.Empty;
			AssertNull("CusEntryNumber should be null because the new Record could not be loaded", entryHeader.CusEntryNumber);

			number.CE_RN_NKCountryCode = decl.Country.Code;
			AssertNotNull("CusEntryNumber should be not null because proper country code allows to load it", entryHeader.CusEntryNumber);
			AssertEquals("EntryNumber should be one from the Number.CE_EntryNum", "EX123", entryHeader.EntryNumber);

			var number2 = CusEntryNumber.New(entryHeader, decl.JE_MessageType, decl.CountryCode);
			number2.CE_EntryNum = "EX4343";
			number2.CE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
			number.CE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-1);
			AssertEquals("EX4343", entryHeader.EntryNumber);
			number.CE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
			number2.CE_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-1);
			AssertEquals("EX123", entryHeader.EntryNumber);
		}
		#endregion

		public void TestUpdateEQ_ReceivedDateWhenCusEntryHeaderEDocsIsAdded()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var requiredDocument = declaration.DocsAndCartage.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = "EPR";
			requiredDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Assert("PreCondition", entry is IDocsAndCartageParent);

			var docManagerInfo = ((IDocManagerSupport)entry).DocManagerInfo;
			docManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			var storageMain = docManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(entry, "CEH");

			_ = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "EPR", false);
			Factory.Save();
			docManagerInfo.MasterFactory.Save();

			requiredDocument.Reload();
			AssertNotEquals(ZDateTimeOffset.Empty, requiredDocument.EQ_DateReceived);
		}

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var list = declaration.Lookups.MessageTypeList;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			IAdditionalReferenceNumberTypeProvider provider = entry;
			AssertEquals(list, provider.GetAdditionalReferenceNumberTypeList(ZString.Empty, ZString.Empty));
			entry.CH_JE = ZGuid.Invalid;
			AssertNull(entry.Declaration);
			AssertNull(provider.GetAdditionalReferenceNumberTypeList(ZString.Empty, ZString.Empty));
			AssertNull(provider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.CustomsPermitClearanceNumber, Core.Constants.CountryCodes.UnitedStates));

			var additionalReferenceNumberTypes = CusEntryNumLookups.GetAdditionalReferenceNumberTypes(Factory, Core.Constants.CountryCodes.UnitedStates, false);
			AssertEquals(additionalReferenceNumberTypes, provider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, Core.Constants.CountryCodes.UnitedStates));
		}

		public void TestStatusChangedToHeldSinceLoading()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var header = Factory.New<StatusChangedToHeldSinceLoadingCoreCusEntryHeader>();
			header.CH_JE = declaration.PK;
			var incomingMessage = header.Messages.AddNew();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			Factory.Save();
			var logsFound = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceivedCode));
			AssertEquals("Should not be able to find CustomsImpedimentReceived event - logsFound.Length", 0, logsFound.Length);

			header.StatusChangedToHeld = true;
			Factory.Save();
			logsFound = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceivedCode));
			AssertEquals("Should be able to find CustomsImpedimentReceived event - logsFound.Length", 1, logsFound.Length);
		}

		public void TestEntryChargeTypeListIsCached()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var list = header1.EntryChargeTypeList;
			AssertType<ZZEntryChargeTypeList>(list);
			var header2 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(list, header2.EntryChargeTypeList);
		}

		class StatusChangedToHeldSinceLoadingCoreCusEntryHeader : CusEntryHeader
		{
			public StatusChangedToHeldSinceLoadingCoreCusEntryHeader(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool StatusChangedToHeldSinceLoadingCore()
			{
				return StatusChangedToHeld;
			}

			public bool StatusChangedToHeld;
		}

		public void TestCH_MessageTypeDescription()
		{
			var header = Factory.New<CusEntryHeader>();
			header.CH_MessageType = ZString.Empty;
			AssertEquals(ZString.Empty, header.CH_MessageTypeDescription);
			foreach (ICodeDescription pair in new JobMessageTypeList())
			{
				header.CH_MessageType = pair.Code;
				AssertEquals(pair.Description, header.CH_MessageTypeDescription);
			}
			header.CH_MessageType = "!ZD";
			AssertEquals(ZString.Empty, header.CH_MessageTypeDescription);
		}

		public void TestThrowAwayEntryNumber()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "0123121";
			CusEntryNumber entryNumber = entryHeader.CusEntryNumber;
			entryHeader.ThrowAwayEntryNumber();
			AssertNotEquals(entryNumber, entryHeader.CusEntryNumber);
			AssertEquals(true, entryNumber.IsDeleted);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			IRegistryAccessingSupporter supporter = entry;

			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), supporter.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);

			entry.CH_JE = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), supporter.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), supporter.RegistryBranchPK);
		}

		public void TestCustomsClearedEvent()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryMock = Factory.NewMoq<CusEntryHeader>();
			entryMock
				.Protected()
				.Setup<bool>("IsStatusChangingToCleared", ItExpr.IsAny<ZString>(), ItExpr.IsAny<ZString>())
				.Returns(true);
			entryMock
				.Protected()
				.Setup<bool>("IsCustomsClearedEventSupported")
				.Returns(false);
			CusEntryHeader entryHeader = entryMock.Object;
			entryHeader.CH_JE = declaration.PK;
			Factory.Save();
			AssertNull(entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			entryMock
				.Protected()
				.Setup<bool>("IsCustomsClearedEventSupported")
				.Returns(true);
			entryHeader.HasChanges = true;
			Factory.Save();
			var log = entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull(log);
			AssertEquals(1, entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsClearedCode)).Length);

			entryHeader.HasChanges = true;
			Factory.Save();
			AssertEquals(1, entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsClearedCode)).Length);
			AssertEquals(log, entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsCleared));

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_IsEstimate = true;
			}
			entryHeader.HasChanges = true;
			Factory.Save();
			AssertEquals(2, entryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsClearedCode)).Length);
			var log2 = entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsCleared);
			AssertNotNull(log2);
			AssertNotEquals(log, log2);
			entryMock.VerifyAll();
		}

		public void TestCustomsImpedimentReceivedEvent()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			var entryMock = Factory.NewMoq<CusEntryHeader>();
			entryMock
				.Protected()
				.Setup<bool>("StatusChangedToHeldSinceLoadingCore")
				.Returns(true);
			entryMock
				.Protected()
				.Setup<bool>("IsCustomsImpedimentReceivedEventSupported")
				.Returns(false);
			CusEntryHeader entryHeader = entryMock.Object;
			entryHeader.CH_JE = declaration.PK;
			Factory.Save();
			entryHeader.Logs.GetAllLogs().Load();
			AssertNull(entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsImpedimentReceived));
			entryHeader.HasChanges = true;
			Factory.Save();
			AssertNull(entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsImpedimentReceived));
			entryMock
				.Protected()
				.Setup<bool>("IsCustomsImpedimentReceivedEventSupported")
				.Returns(true);
			entryHeader.HasChanges = true;
			Factory.Save();
			AssertNotNull(entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsImpedimentReceived));
			entryMock.VerifyAll();
		}

		public void TestChangingCH_CustomsMessageRemarksAffectsHasChangesAfterNoteAlreadyExists()
		{
			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("Precondition: entryHeader.HasChanges", false, entryHeader.HasChanges);
			entryHeader.CH_CustomsMessageRemarks = "123";
			AssertEquals("entryHeader.HasChanges", true, entryHeader.HasChanges);
			Factory.Save();

			AssertEquals("Precondition: entryHeader.HasChanges", false, entryHeader.HasChanges);
			entryHeader.CH_CustomsMessageRemarks = "";
			AssertEquals("entryHeader.HasChanges", true, entryHeader.HasChanges);
			Factory.Save();

			AssertEquals("Precondition: entryHeader.HasChanges", false, entryHeader.HasChanges);
			entryHeader.CH_CustomsMessageRemarks = "456";
			AssertEquals("entryHeader.HasChanges", true, entryHeader.HasChanges);
			Factory.Save();

			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			CusEntryHeader reloadedEntryHeader = secondFactory.Load<CusEntryHeader>(entryHeader.PK);

			AssertEquals("Precondition: entryHeader.HasChanges", false, reloadedEntryHeader.HasChanges);
			reloadedEntryHeader.CH_CustomsMessageRemarks = "123";
			AssertEquals("entryHeader.HasChanges", true, reloadedEntryHeader.HasChanges);
			Factory.Save();
		}

		public void TestIsEntryNumberGeneratedButNotSavedYet()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("not generated yet now", false, entry.IsEntryNumberGeneratedButNotSavedYet);

			entry.EntryNumber = "A";
			AssertEquals("not saved yet now", true, entry.IsEntryNumberGeneratedButNotSavedYet);

			Factory.Save();
			AssertEquals("IsSaved now", false, entry.IsEntryNumberGeneratedButNotSavedYet);
		}

		public void TestInvoiceHeaders()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice1 = declaration.Invoices.AddNew();
			_ = invoice1.JobComInvoiceLines.AddNew();
			_ = invoice1.JobComInvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			_ = invoice2.JobComInvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Two invoice headers", 2, declaration.CustomsEntryHeaders[0].InvoiceHeaders.Length);

			invoice1.JZ_ValuationDateOverride = ZDateTime.BrettsBirthday;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("One invoice headers", 1, declaration.CustomsEntryHeaders[0].InvoiceHeaders.Length);
			AssertEquals("One invoice headers", 1, declaration.CustomsEntryHeaders[1].InvoiceHeaders.Length);
		}

		public void TestHasTransactionsWithCustoms()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertNoExceptionThrown(delegate
			{ declaration.CustomsEntryHeaders.AddNew().IsActive = false; });

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			EDIMessage outgoingMessage = entry.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("IsWaitingFor response", true, entry.IsWaitingForResponse);
			AssertEquals("HasTransactionWithCustoms", true, entry.HasTransactionsWithCustoms);
			AssertEquals("HasBeenLodgedAtCustoms", false, entry.HasBeenLodgedAtCustoms);

			entry.EntryNumber = "A";
			AssertEquals("HasBeenLodgedAtCustoms", true, entry.HasBeenLodgedAtCustoms);
		}

		public void TestHasNonAmendableChanges()
		{
			var mockDeclaration = Factory.New<DummyBaseJobDeclaration_ForTest>();
			mockDeclaration.IsCustomsHeaderAmendmentATotalReplacementReturns = true;

			CusEntryHeader entry = mockDeclaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "A";
			AssertEquals("HasNonAmendableChanges", false, entry.HasNonAmendableChanges);
		}

		public void TestResetTotalsAndCachedValues()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_TotalPaid = 10000m;

			var mock = Factory.NewMoq<CusEntryHeaderCharges>();
			mock
				.Protected()
				.Setup<bool>("ShouldResetDataOnMergingCore")
				.Returns(false);
			var charge = mock.Object;

			entryHeader.Charges.Add(charge);
			charge.C1_ChargeAmount = 500m;
			charge.C1_ChargeType = "TTT";
			charge.C1_IsLandedCostOnly = true;

			AssertEquals("Should not clear amount", false, charge.ShouldResetDataOnMerging);

			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("Total paid is cleared", 0m, entryHeader.CH_TotalPaid);
			AssertEquals("Charge amount is not cleared. AU has a charge amount which brokers manually enters after contacting Customs and Re-merging should not clear those amounts as system wont calculate this back", 500m, charge.C1_ChargeAmount);
			AssertEquals("charge.C1_IsLandedCostOnly should be reset to false as it is going to be set properly at the end of merge", false, charge.C1_IsLandedCostOnly);

			mock.VerifyAll();
			mock.Reset();
			entryHeader.ResetTotalsAndCachedValues();
			AssertEquals("Total paid is cleared", 0m, entryHeader.CH_TotalPaid);
			AssertEquals("Charge amount is cleared", 0m, charge.C1_ChargeAmount);
			AssertEquals("charge.C1_IsLandedCostOnly", false, charge.C1_IsLandedCostOnly);
		}

		public void TestCH_StatusWhenSavingFails()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			TestEntry entry = Factory.New<TestEntry>();
			entry.CH_JE = declaration.PK;

			entry.CH_Status = "AAA";
			entry.ShouldThrowExceptionOnSaving = true;

			try
			{
				Factory.Save();
			}
			catch
			{
				AssertEquals("CH_Status should have been reverted as saving fails and messages that might have been generated are going to deleted", "", entry.CH_Status);
			}

			entry.ShouldThrowExceptionOnSaving = false;
			entry.CH_Status = "AAA";
			Factory.Save();

			entry.CH_Status = "BBB";
			entry.ShouldThrowExceptionOnSaving = true;

			try
			{
				Factory.Save();
			}
			catch
			{
				AssertEquals("CH_Status should have been reverted as saving fails and messages that might have been generated are going to deleted", "AAA", entry.CH_Status);
			}
		}

		// WI00006580
		public void TestAccessingDeclarationOnDeletedObject()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CusEntryHeader reloadedEntry = newFactory.Load<CusEntryHeader>(entry.PK);
			AssertNotNull(reloadedEntry.Declaration);
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			Factory.Save();
			AssertNotNull(reloadedEntry.Declaration);
		}

		class TestEntry : CusEntryHeader
		{
			public TestEntry(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ShouldThrowExceptionOnSaving;
			public override void OnSaving()
			{
				base.OnSaving();
				if (ShouldThrowExceptionOnSaving)
				{
					throw new Exception("Testing");
				}
			}

			public EntryChargeTypeList EntryChargeTypeListExposed;
			protected override EntryChargeTypeList GetEntryChargeTypeList()
			{
				return EntryChargeTypeListExposed ?? base.GetEntryChargeTypeList();
			}
		}

		public void TestICustomsChargesProvider()
		{
			var dty = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "DTY");
			var lct = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "LCT");
			var wet = helper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "WET");
			helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dty.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "LCT", lct.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "WET", wet.PK);
			Factory.Save();

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;

			TestEntry entry = Factory.New<TestEntry>();
			entry.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entry);

			entry.Charges.AddNew(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.DutyAmount, 20m);
			entry.Charges.AddNew(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.LCTAmount, 30m);
			entry.Charges.AddNew(Enterprise.Registry.Business.Customs.AU.EntryChargeTypeList.Codes.WetAmount, 40m);

			entry.EntryNumber = "123456";
			entry.EntryChargeTypeListExposed = new Registry.Business.Customs.AU.EntryChargeTypeList();

			IAccInvoiceDataProvider provider = entry;

			ICustomsCharges[] charges = provider.CustomsCharges;
			AssertEquals(1, charges.Length);
			AssertEquals(3, charges[0].GetCustomsCharges(null).Length);
			AssertEquals("Amount", 20m, charges[0].GetCustomsCharges(null)[0].Amount);
			AssertEquals("Amount", 30m, charges[0].GetCustomsCharges(null)[1].Amount);
			AssertEquals("Amount", 40m, charges[0].GetCustomsCharges(null)[2].Amount);

			AssertEquals(declaration, provider.CustomsJob);
			AssertEquals("InvoiceDate", ZDateTime.Today, provider.InvoiceDate);
			AssertEquals("123456", provider.UniqueNumber);
			AssertEquals("", provider.ReasonForUnbillability);
			AssertEquals(true, provider.IsBillable);
		}

		public void TestIsAutoBillingDueDateFromPaymentTerms()
		{
			CustomsDataRegistry.Instance.AutoBillingDueDateFromPaymentTerms.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var dec = Factory.New<BaseJobDeclaration>();
			var provider = dec.CustomsEntryHeaders.AddNew() as IAccInvoiceDataProvider;
			AssertEquals("The provider's IsAutoBillingDueDateFromPaymentTerms should reflect the explicitly set configuration value.", true, provider.IsAutoBillingDueDateFromPaymentTerms);
		}

		public void TestIAccInvoiceDataProvider_MatchCustomsChargesToClear()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;

			TestEntry entry = Factory.New<TestEntry>();
			entry.CH_JE = declaration.PK;

			declaration.CustomsEntryHeaders.Add(entry);

			entry.CH_BGMReference = "BGM12345";
			entry.EntryNumber = "EN123456";

			var provider = entry as IAccInvoiceDataProvider;

			using (Registry.Business.RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("NOT EntryReferenceInChargeDescSupported", () =>
				{
					AssertEquals("Pre-Req", false, ((ICustomsChargeEntry)entry).EntryReferenceInChargeDescSupported);
					AssertEquals("No Matches", false, provider.MatchCustomsChargesToClear("INV12345", "Random Description"));
					AssertEquals("Empty InvoiceNum", true, provider.MatchCustomsChargesToClear("", "Random Description"));
					AssertEquals("Match UniqueNum", true, provider.MatchCustomsChargesToClear("EN123456 InvoiceNum", "Random Description"));
				});
			}

			using (Registry.Business.RatingDataRegistry.Instance.IncludeEntryHeaderReferenceInCustomsDisbursementCharges.SetTemporaryValue(entry.RegistryCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Pre-Req", true, ((ICustomsChargeEntry)entry).EntryReferenceInChargeDescSupported);
				AssertEquals("No Matches", false, provider.MatchCustomsChargesToClear("INV12345", "Random Description"));
				AssertEquals("Empty InvoiceNum", false, provider.MatchCustomsChargesToClear("", "Random Description"));

				AssertEquals("Ref in First line", true, provider.MatchCustomsChargesToClear("", "ABC BGM12345\r\nDEF Ref00004"));
				AssertEquals("Ref in 2nd line", false, provider.MatchCustomsChargesToClear("", "ABC Ref00004\r\nDEF BGM12345"));
			}
		}

		public void TestHeaderCDArchiveInfo()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			var helper = new TestHelper();
			declaration.JE_OH_Importer = helper.Buyer.PK;
			declaration.JE_OH_Supplier = helper.Supplier.PK;

			var invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();
			var invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OH_Buyer = invoiceImporter.PK;
			invoice.JZ_OH_Supplier = invoiceSupplier.PK;

			var customsContainer = declaration.CusContainers.AddNew();
			customsContainer.CO_ContainerNumber = "123";

			var customsContainer2 = declaration.CusContainers.AddNew();
			customsContainer2.CO_ContainerNumber = "APLU123001";

			declaration.JE_RL_NKFinalDestination = "52000";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			declaration.JE_VoyageFlightNo = "001Y";
			declaration.JE_HouseBill = "HouseBill1";
			declaration.JE_DeclarationReference = "B0001645";
			declaration.JE_MasterBill = "MasterBill1";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_VesselName = "BUNGA DELIMA";

			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_OwnerRef = "Test Reference";
			declaration.DocsAndCartage.JP_OrderItemsAsString = "Order1, Order2";

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = "AAA";
			entry1.EntryNumber = "10000012";

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = "BBB";
			entry2.EntryNumber = "10000042";

			var job = new JobHeader.Loader(declaration).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoiceConsignee = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceConsignee.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoiceConsignee.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			invoiceConsignee.AH_OH = declaration.Importer.PK;
			invoiceConsignee.AH_TransactionNum = "00001001";
			invoiceConsignee.AH_RX_NKTransactionCurrency = declaration.LocalCurrencyCode;
			invoiceConsignee.AH_GB = GlbBranch.CurrentBranch.PK;
			invoiceConsignee.AH_ConsolidatedInvoiceRef = declaration.JE_DeclarationReference;
			invoiceConsignee.AH_TransactionReference = "00001001";
			invoiceConsignee.AH_JH = job.PK;

			var entryHeaderInfo = ((ICDArchive)entry1).CDArchiveInfo;
			AssertEquals("Consignee", helper.Buyer.OH_Code, entryHeaderInfo.ConsigneeCode);
			AssertEquals("Consignor", helper.Supplier.OH_Code, entryHeaderInfo.ConsignorCode);
			AssertEquals("Containers", "123, APLU123001", entryHeaderInfo.ContainerNumbers);
			AssertEquals("Destination", "52000", entryHeaderInfo.Destination);
			AssertEquals("EntryNumbers", "10000012", entryHeaderInfo.EntryNumber);
			AssertEquals("ETA", ZDateTime.Today, entryHeaderInfo.ETA);
			AssertEquals("ETD", ZDateTime.Today.AddDays(-1), entryHeaderInfo.ETD);
			AssertEquals("HouseBill", "HouseBill1", entryHeaderInfo.HouseBill);
			AssertEquals("InvoiceNumbers", "B0001645", entryHeaderInfo.InvoiceNumbers);
			AssertEquals("JobNumber", "B0001645", entryHeaderInfo.JobNumber);
			AssertEquals("MasterBill", "MasterBill1", entryHeaderInfo.MasterBill);
			AssertEquals("OrderNumbers", "Order1, Order2, Test Reference", entryHeaderInfo.OrderNumbers);
			AssertEquals("Origin", "NZAKL", entryHeaderInfo.Origin);
			AssertEquals("Vessel", "BUNGA DELIMA", entryHeaderInfo.Vessel);
			AssertEquals("VoyageFlight", "001Y", entryHeaderInfo.VoyageFlight);
		}

		public void TestIsExport()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(false, entry1.IsExport);

			declaration.JE_MessageType = "EXP";
			AssertEquals(true, entry1.IsExport);
		}

		public void TestICustomsDocumentGeneratorSupporter()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Assert(entry is Integration.Customs.ICustomsDocumentGeneratorSupporter);
			var supporter = (Integration.Customs.ICustomsDocumentGeneratorSupporter)entry;
			AssertExceptionThrown(typeof(InvalidOperationException), () => supporter.GetDocumentName("~"));
			AssertEquals(false, supporter.GenerateCustomsDocument("~"));
			AssertEquals(ZString.Empty, supporter.GetReasonForUnableToGenerateCustomsDocument());
		}

		public void TestTotalInvoiceLinesGrossWeightInKG()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.InvoiceLines.AddNew();
			invoice1Line1.JI_Weight = 15000m;
			invoice1Line1.JI_WeightUQ = Core.Constants.Weight.Grams;
			var invoice1Line2 = invoice1.InvoiceLines.AddNew();
			invoice1Line2.JI_Weight = 1.545m;
			invoice1Line2.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			var invoice1Line3 = invoice1.InvoiceLines.AddNew();
			invoice1Line3.JI_Weight = 200000m;
			invoice1Line3.JI_WeightUQ = Core.Constants.Weight.Milligrams;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entry1Line1 = entry1.MergedLines.AddNew();
			invoice1Line1.JI_CL = entry1Line1.PK;
			var entry1Line2 = entry1.MergedLines.AddNew();
			invoice1Line2.JI_CL = entry1Line2.PK;
			var entry1Line3 = entry1.MergedLines.AddNew();
			invoice1Line3.JI_CL = entry1Line3.PK;

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entry2Line1 = entry2.MergedLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoice2.InvoiceLines.AddNew();
			invoice2Line1.JI_Weight = 300m;
			invoice2Line1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoice2Line1.JI_CL = entry2Line1.PK;
			CombineAssertions(() =>
			{
				AssertEquals("entry1.TotalInvoiceLinesGrossWeightInKG", 1560.2m, entry1.TotalInvoiceLinesGrossWeightInKG);
				AssertEquals("entry2.TotalInvoiceLinesGrossWeightInKG", 300m, entry2.TotalInvoiceLinesGrossWeightInKG);
			});
		}

		public void TestPackageCount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				_ = invoiceHeader.InvoiceLines.AddNew();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);

				var cusEntryHeader = declaration.CustomsEntryHeaders[0];
				declaration.JE_TotalNoOfPacks = 10;
				AssertEquals(10, cusEntryHeader.PackagesCount);
				AssertEquals(10, ((IAllowPermitProcessing)cusEntryHeader).PackageCount);
			}
		}

		public void TestIsAllEntryLinesVatSuspended()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			_ = GenerateProcedure("No", YesNoList.Codes.No, YesNoList.Codes.No, "No description", false);
			_ = GenerateProcedure("Ye", YesNoList.Codes.No, YesNoList.Codes.Yes, "Yes description", true);
			Factory.Save();

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			var invoiceline = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceline.JI_Procedure = "Ye";
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = true", invoiceline.HasAnyProcedureWithSuspendedVat);

			var invoiceline2 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceline2.JI_Procedure = "";
			Assert("invoiceline HasAnyProcedureWithSuspendedVat = false", !invoiceline2.HasAnyProcedureWithSuspendedVat);

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			CusEntryHeader entryheader = declaration.CustomsEntryHeaders[0];
			Assert(!entryheader.IsAllEntryLinesNotVatSuspended);
			Assert(!entryheader.IsAllEntryLinesVatSuspended);

			invoiceline2.JI_Procedure = "Ye";
			Assert(invoiceline.HasAnyProcedureWithSuspendedVat);
			Assert(invoiceline2.HasAnyProcedureWithSuspendedVat);
			Assert(!entryheader.IsAllEntryLinesNotVatSuspended);
			Assert(entryheader.IsAllEntryLinesVatSuspended);

			invoiceline.JI_Procedure = "";
			invoiceline2.JI_Procedure = "";
			Assert(!invoiceline.HasAnyProcedureWithSuspendedVat);
			Assert(!invoiceline2.HasAnyProcedureWithSuspendedVat);
			Assert(entryheader.IsAllEntryLinesNotVatSuspended);
			Assert(!entryheader.IsAllEntryLinesVatSuspended);
		}

		RefCusProcedure GenerateProcedure(string procedureCode, string isGuaranteeConsumed, string isGuaranteeReleased, string description, ZBool isCalculeVAT)
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = procedureCode;
			procedure.ZZ6_IsGuaranteeConsumed = isGuaranteeConsumed;
			procedure.ZZ6_IsGuaranteeReleased = isGuaranteeReleased;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Description = description;
			procedure.ZZ6_CalculateVAT = isCalculeVAT;
			return procedure;
		}

		public void TestShouldCalculatePackagesCountBasedOnLinesPackagesPivot()
		{
			var entryHeader = Factory.New<CusEntryHeaderForPackagesCountForTesting>();
			AssertEquals("ShouldCalculatePackagesCountBasedOnLinesPackagesPivot", false, entryHeader.ShouldCalculatePackagesCountBasedOnLinesPackagesPivotExposed);
		}

		public void TestPackagesCountWhenNoParentDeclaration()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("When entry has no parent declaration, PackagesCount", 0, entryHeader.PackagesCount);
		}

		public void TestPackagesCountWhenShouldNotUsePackagesPivotAndThereAreNoInvoices()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("When there are no invoices and job only has one entry, PackagesCount", 10, entryHeader.PackagesCount);

			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("When there are no invoices and job has more than one entry, PackagesCount", 0, entryHeader.PackagesCount);
		}

		public void TestPackagesCountWhenShouldNotUsePackagesPivotAndFirstInvoiceLineIsGoingIntoBondedWarehouse()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = mergedLine.PK;
			invoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);

			var invoice2 = Factory.New<BaseJobComInvoiceHeaderForTesting>();
			invoice2.PackagesBondReturns = 11;
			declaration.Invoices.Add(invoice2);
			invoice2.InvoiceLines.AddNew().JI_CL = mergedLine.PK;

			var invoice3 = Factory.New<BaseJobComInvoiceHeaderForTesting>();
			invoice3.PackagesBondReturns = 9;
			declaration.Invoices.Add(invoice3);
			invoice3.InvoiceLines.AddNew().JI_CL = mergedLine.PK;
			AssertEquals("When first invoice line is going into bonded warehouse, PackagesCount", 20, entryHeader.PackagesCount);
		}

		public void TestPackagesCountWhenShouldNotUsePackagesPivotAndFirstInvoiceLineIsNotGoingIntoBondedWarehouse()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = mergedLine.PK;

			var invoice2 = Factory.New<BaseJobComInvoiceHeaderForTesting>();
			invoice2.PackagesFreeStoreReturns = 21;
			declaration.Invoices.Add(invoice2);
			invoice2.InvoiceLines.AddNew().JI_CL = mergedLine.PK;

			var invoice3 = Factory.New<BaseJobComInvoiceHeaderForTesting>();
			invoice3.PackagesFreeStoreReturns = 9;
			declaration.Invoices.Add(invoice3);
			invoice3.InvoiceLines.AddNew().JI_CL = mergedLine.PK;
			AssertEquals("When first invoice line is not going into bonded warehouse, PackagesCount", 30, entryHeader.PackagesCount);
		}

		public void TestPackagesCountWhenShouldUsePackagesPivot()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var packingGroup = declaration.Bills.AddNew().PackingGroups.AddNew();
			var package1 = declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = packingGroup.PK;
			package1.CW_PackQty = 100;
			var package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = packingGroup.PK;
			package2.CW_PackQty = 200;
			var package3 = declaration.Packages.AddNew();
			package3.CW_CR_HouseContainer = packingGroup.PK;
			package3.CW_PackQty = 50;
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var packageCtInvoiceLine1 = invoiceLine1.PackagesPivot.AddNew();
			packageCtInvoiceLine1.CHC_CW = package1.PK;
			packageCtInvoiceLine1.CHC_NumberOfPacks = 75;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var packageCtInvoiceLine2 = invoiceLine2.PackagesPivot.AddNew();
			packageCtInvoiceLine2.CHC_CW = package2.PK;
			packageCtInvoiceLine2.CHC_NumberOfPacks = 190;
			var entryHeader = Factory.NewMoq<CusEntryHeader>();
			entryHeader
				.Protected()
				.Setup<bool>("ShouldCalculatePackagesCountBasedOnLinesPackagesPivot")
				.Returns(true);
			declaration.CustomsEntryHeaders.Add(entryHeader.Object);
			var entryLine = entryHeader.Object.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			AssertEquals("PackagesCount", 265, entryHeader.Object.PackagesCount);
			entryHeader.VerifyAll();
		}

		GlbBranch MakeNewBranchInEritrea()
		{
			var foreignCompany = Factory.NewWithValidTestData<GlbCompany>();
			foreignCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var branchInAnotherCompany = foreignCompany.Branches.AddNew();
			branchInAnotherCompany.GB_Code = "DJC";
			Factory.Save();
			return branchInAnotherCompany;
		}

		public void TestLoadForBGMReference()
		{
			string bGMReference = "ABC123";
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = bGMReference;
			Factory.Save();
			CusEntryHeader retrievedHeader = CusEntryHeader.LoadForBGMReference(Factory, bGMReference);
			AssertEquals("Initial fetch", entryHeader.PK, retrievedHeader.PK);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_GB = MakeNewBranchInEritrea().PK;
			Factory.Save();
			CusEntryHeader retrievedHeaderThatShouldNotExist = CusEntryHeader.LoadForBGMReference(Factory, bGMReference);
			AssertNull(retrievedHeaderThatShouldNotExist);
		}

		public void TestLoadForBGMReferenceAndNoEntryNumber()
		{
			string bGMReference = "ABC123";
			string entryNumber = "ENTNUM";
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = bGMReference;
			Factory.Save();
			CusEntryHeader retrievedHeader = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, bGMReference, entryNumber);
			AssertEquals("Initial fetch", entryHeader.PK, retrievedHeader.PK);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_GB = MakeNewBranchInEritrea().PK;
			Factory.Save();
			CusEntryHeader retrievedHeaderThatShouldNotExist = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, bGMReference, entryNumber);
			AssertNull(retrievedHeaderThatShouldNotExist);
		}

		public void TestLoadForBGMReferenceAndMatchingEntryNumber()
		{
			string bGMReference = "ABC123";
			string entryNumber = "ENTNUM";
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = bGMReference;
			entryHeader.EntryNumber = entryNumber;
			Factory.Save();
			CusEntryHeader retrievedHeader = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, bGMReference, entryNumber);
			AssertEquals("Initial fetch", entryHeader.PK, retrievedHeader.PK);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_GB = MakeNewBranchInEritrea().PK;
			Factory.Save();
			CusEntryHeader retrievedHeaderThatShouldNotExist = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, bGMReference, entryNumber);
			AssertNull(retrievedHeaderThatShouldNotExist);
		}

		public void TestLoadForBGMReferenceAndNonMatchingEntryNumber()
		{
			string bGMReference = "ABC123";
			string matchingEntryNumber = "ENTNUM";
			string nonMatchingEntryNumber = "NoMatch";
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = bGMReference;
			entryHeader.EntryNumber = matchingEntryNumber;
			Factory.Save();
			CusEntryHeader retrievedHeader = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, bGMReference, nonMatchingEntryNumber);
			AssertNull(retrievedHeader);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_GB = MakeNewBranchInEritrea().PK;
			Factory.Save();
			CusEntryHeader retrievedHeaderThatShouldNotExist = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, bGMReference, nonMatchingEntryNumber);
			AssertNull(retrievedHeaderThatShouldNotExist);
		}

		public void TestLoadForBGMReferenceAndNoEntryNumberOnMessage()
		{
			string bGMReference = "ABC123";
			string entryNumber = "ENTNUM";
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = bGMReference;
			entryHeader.EntryNumber = entryNumber;
			Factory.Save();
			CusEntryHeader retrievedHeader = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, bGMReference, "");
			AssertEquals("Initial fetch", entryHeader.PK, retrievedHeader.PK);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_GB = MakeNewBranchInEritrea().PK;
			Factory.Save();
			CusEntryHeader retrievedHeaderThatShouldNotExist = CusEntryHeader.LoadForBGMReferenceAndEntryNumber(Factory, bGMReference, "");
			AssertNull(retrievedHeaderThatShouldNotExist);
		}

		public void TestICommonGoodsItemsIntegratorProvider_CommonGoodsItemsIntegrator()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<CommonGoodsItemsIntegrator>(entryHeader.CommonGoodsItemsIntegrator);
		}

		public void TestCH_EntryReleaseDate()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var resStringData = entry.CH_EntryReleaseDateInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Release Date", resStringData.Caption);
		}

		public void TestEntryNumber_Caption()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var resStringData = entry.EntryNumberInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Entry Number", resStringData.Caption);
		}

		public void TestCheckChargeTypeAndPaymentMethodUniqueness()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeType = "AA";
			charge1.C1_MethodOfPayment = "BB";
			var charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeType = "BB";
			charge2.C1_MethodOfPayment = "CC";
			CombineAssertions(() =>
			{
				var chargesAABB = entryHeader.Charges.GetChargesByTypeAndPaymentMethod("AA", "BB");
				var chargesBBCC = entryHeader.Charges.GetChargesByTypeAndPaymentMethod("BB", "CC");
				var chargesCCDD = entryHeader.Charges.GetChargesByTypeAndPaymentMethod("CC", "DD");
				AssertEquals(1, chargesAABB.Count());
				AssertEquals(1, chargesAABB.Count());
				AssertEquals(0, chargesCCDD.Count());
			}

			);
		}

		public void TestITypeDeciderContext()
		{
			var nzCompany = Factory.New<GlbCompany>();
			nzCompany.GC_Code = "CNZ";
			nzCompany.GC_Name = "NZ Company";
			nzCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			var nzBranch = nzCompany.Branches.AddNew();
			nzBranch.GB_Code = "BNZ";

			CombineAssertions(() =>
			{
				AssertEquals("From CurrentCompany", "ER", (Factory.New<CusEntryHeader>() as ITypeDeciderContext).Country);

				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.NZ.IJobDeclaration>();
				declaration.JE_GB = nzBranch.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				AssertEquals("From Declaration", "NZ", (entryHeader as ITypeDeciderContext).Country);
			});
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
		}
		UniversalReferenceTestDataHelper helper;

		sealed class BaseJobDeclarationForTesting : BaseJobDeclaration
		{
			public BaseJobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool GetIsWHSUniversalXMLActiveReturns { get; set; }
			public bool IsInvoiceQuantityRequiredForBondedWarehouseReturns { get; set; }
			public BondedWarehousingHelper GetNewBondedWarehousingHelperReturns { get; set; }

			protected override bool GetIsWHSUniversalXMLActive() => GetIsWHSUniversalXMLActiveReturns;
			protected internal override bool IsInvoiceQuantityRequiredForBondedWarehouse => IsInvoiceQuantityRequiredForBondedWarehouseReturns;
			protected override BondedWarehousingHelper GetNewBondedWarehousingHelper() => GetNewBondedWarehousingHelperReturns;
			protected override EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);
		}

		sealed class BaseJobComInvoiceHeaderForTesting : BaseJobComInvoiceHeader
		{
			public BaseJobComInvoiceHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public int PackagesFreeStoreReturns { get; set; }
			public int PackagesBondReturns { get; set; }

			public override int PackagesFreeStore => PackagesFreeStoreReturns;
			public override int PackagesBond => PackagesBondReturns;
		}

		sealed class CusEntryHeaderForPackagesCountForTesting : CusEntryHeader
		{
			public CusEntryHeaderForPackagesCountForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool ShouldCalculatePackagesCountBasedOnLinesPackagesPivotExposed => ShouldCalculatePackagesCountBasedOnLinesPackagesPivot;
		}
	}

	[TestedType(typeof(CusEntryHeader))]
	class CusEntryHeaderClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren()
		{
			var entryHeader = (CusEntryHeader)ClusterKeyEntityToTest;
			var entryCharge = entryHeader.Charges.AddNew();
			entryCharge.C1_ChargeAmount = 1;
			var payInfo = entryHeader.EntryPayInfos.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			return new IClusterKeyWorker[] { entryCharge, payInfo, entryLine };
		}

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var dec = (BaseJobDeclaration)NewParentObject();
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_JE = dec.PK;
			return entryHeader;
		}

		protected override EnterpriseBusinessObject NewParentObject() => Factory.New<BaseJobDeclaration>();
	}

	class CusEntryHeaderForEntryStatusLoggingTest : TestCaseWithFactory
	{
		[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
		public class CusEntryHeaderForEntryStatusLogging : CusEntryHeader
		{
			public CusEntryHeaderForEntryStatusLogging(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool ShouldLogEntryStatus { get { return true; } }
		}

		public void TestEntryStatusLogging()
		{
			CombineAssertions("Entry Status Logging - Disabled", () =>
			{
				CusEntryHeader header = Factory.NewWithValidTestData<CusEntryHeader>();
				var logs = header.Logs;
				header.CH_EntryStatus = "1";
				Factory.Save();
				AssertEquals("Not yet logged", null, header.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus));
				header.CH_EntryStatus = "2";
				Factory.Save();
				AssertEquals("Still not logged", null, header.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus));
			});

			CombineAssertions("Entry Status Logging - Enabled", () =>
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var header = Factory.New<CusEntryHeaderForEntryStatusLogging>();
				header.CH_JE = declaration.PK;
				var logs = header.Logs;
				header.CH_EntryStatus = "1";
				Factory.Save();
				AssertEquals("Not yet logged", null, header.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus));
				header.CH_EntryStatus = "2";
				Factory.Save();
				AssertNotEquals("Has been logged", null, header.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus));
			});
		}

		public void TestLoggingHighestEntryLineNumberChangedWhenStatusChanges()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "InvoiceLineDescription";
			invoiceLine.JI_Tariff = "0000.00.00";

			LineMerger merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			Assert("CH_HighestLineNumber is 0", entryHeader.CH_HighestLineNumber == 0);
			entryHeader.CH_Status = "CLR";
			Assert("CH_HighestLineNumber is 1", entryHeader.CH_HighestLineNumber == 1);

			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
			AssertEquals("Log Entry created", true, logEntries.Any(le => le.referenceFreeText.HasValue && le.referenceFreeText.Value.Contains("Highest Line Number changed from 0 to 1")));
		}

		public void TestLoggingHighestEntryLineNumberNotChangedWhenStatusChanges()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			Assert("CH_HighestLineNumber is 0", entryHeader.CH_HighestLineNumber == 0);
			entryHeader.CH_Status = "CLR";
			Assert("CH_HighestLineNumber is 0", entryHeader.CH_HighestLineNumber == 0);

			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.referenceFreeText.HasValue && le.referenceFreeText.Value.Contains("Highest Line Number changed from 0 to 0")));
		}
	}

	class CusEntryConcurrencyTest : TestCaseWithFactory
	{
		public void TestConcurrencyErrorsStopSave()
		{
			Factory.RefreshEnabled = false;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeaderFactory1 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			CusEntryHeader entryHeaderFactory2 = factory2.Load<CusEntryHeader>(entryHeaderFactory1.PK);
			entryHeaderFactory1.CH_AddInfo = "Hello";
			Factory.Save();
			entryHeaderFactory2.CH_AddInfo = "Goodbye";
			try
			{
				factory2.Save();
				Fail("Should throw exception");
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				ErrorReporter.Clear();
				// deal with merge
			}
			try
			{
				factory2.Save();
				Fail("Should throw exception again");
			}
			catch (ZSaveConcurrencyException)
			{
				Assert(true);
				ErrorReporter.Clear();
			}
		}

		public void TestConcurrencyErrorsOnStatusStopSave()
		{
			Factory.RefreshEnabled = false;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeaderFactory1 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			CusEntryHeader entryHeaderFactory2 = factory2.Load<CusEntryHeader>(entryHeaderFactory1.PK);
			entryHeaderFactory1.CH_Status = "XXX";
			Factory.Save();
			entryHeaderFactory2.CH_Status = "YYY";
			try
			{
				factory2.Save();
				Fail("Should throw exception");
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				ErrorReporter.Clear();
			}
			Assert(true);
		}

		public void TestConcurrencyErrorsOnEntryStatusStopSave()
		{
			Factory.RefreshEnabled = false;
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			CusEntryHeader entryHeaderFactory1 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			CusEntryHeader entryHeaderFactory2 = factory2.Load<CusEntryHeader>(entryHeaderFactory1.PK);
			entryHeaderFactory1.CH_EntryStatus = "XXX";
			Factory.Save();
			entryHeaderFactory2.CH_EntryStatus = "YYY";
			try
			{
				factory2.Save();
				Fail("Should throw exception");
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				ErrorReporter.Clear();
			}
			try
			{
				factory2.Save();
				Fail("Should throw exception again");
			}
			catch (ZSaveConcurrencyException)
			{
				Assert(true);
				ErrorReporter.Clear();
			}
		}
	}

	class CusEntryHeaderSubmittedDateTest : TestCaseWithFactory
	{
		[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
		public class CusEntryHeaderForSubmittedDateTest : CusEntryHeader
		{
			public CusEntryHeaderForSubmittedDateTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool ShouldPopulateJE_EntrySubmittedDate
			{
				get { return CanPopulateEntrySubmittedDateInDeclaration; }
			}

			public bool CanPopulateEntrySubmittedDateInDeclaration { get; set; }
		}

		public void TestShouldPopulateEntrySubmittedDateInDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = Factory.New<CusEntryHeaderForSubmittedDateTest>();
			entry.CH_JE = declaration.PK;
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;

			var submittedDateTime = ZDateTime.Now.AddMinutes(-15);

			entry.CanPopulateEntrySubmittedDateInDeclaration = false;
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);

			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			entry.CanPopulateEntrySubmittedDateInDeclaration = true;
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate is set to ZDateTime.Now", true, declaration.JE_EntrySubmittedDate == submittedDateTime);
		}

		public void TestPopulateEntrySubmittedDateIfRequired()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = Factory.New<CusEntryHeader>();

			Assert("CusEntryHeader IsFormalEntry", entry.IsFormalEntry);

			// without declaration
			AssertEquals("CH_EntrySubmittedDate.IsEmpty", true, entry.CH_EntrySubmittedDate.IsEmpty);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);
			entry.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);

			// with declaration
			entry.CH_JE = declaration.PK;
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			entry.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is set to ZDateTime.Now", true, declaration.JE_EntrySubmittedDate == entry.CH_EntrySubmittedDate);

			// with already set declaration
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime = ZDateTime.Now.AddMinutes(-30);
			declaration.JE_EntrySubmittedDate = declarationTime;
			entry.PopulateEntrySubmittedDateIfRequired();
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate > ZDateTime.Now.AddSeconds(-10));
			AssertEquals("JE_EntrySubmittedDate is unchanged", true, declaration.JE_EntrySubmittedDate == declarationTime);
		}

		public void TestPopulateEntrySubmittedDateIfRequiredWithParam()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = Factory.New<CusEntryHeader>();

			Assert("CusEntryHeader IsFormalEntry", entry.IsFormalEntry);

			var submittedDateTime = ZDateTime.Now.AddMinutes(-15);

			// without declaration
			AssertEquals("CH_EntrySubmittedDate.IsEmpty", true, entry.CH_EntrySubmittedDate.IsEmpty);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate == submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate.IsEmpty", true, declaration.JE_EntrySubmittedDate.IsEmpty);

			// with declaration
			entry.CH_JE = declaration.PK;
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate == submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate is set to CH_EntrySubmittedDate", true, declaration.JE_EntrySubmittedDate == entry.CH_EntrySubmittedDate);

			// with already set declaration
			entry.CH_EntrySubmittedDate = ZDateTime.Empty;
			var declarationTime = ZDateTime.Now.AddMinutes(-30);
			declaration.JE_EntrySubmittedDate = declarationTime;
			entry.PopulateEntrySubmittedDateIfRequired(submittedDateTime);
			AssertEquals("CH_EntrySubmittedDate is set to ZDateTime.Now", true, entry.CH_EntrySubmittedDate == submittedDateTime);
			AssertEquals("JE_EntrySubmittedDate is unchanged", true, declaration.JE_EntrySubmittedDate == declarationTime);
		}

		public void TestDocManagerSupports()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			AssertContainsExactElementsInExactOrder("Should return its parent declaration as DocManagerSupports.", new IDocManagerSupport[] { declaration }, ((IDocManagerSupportProvider)entry).DocManagerSupports);

			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>();
			declaration.JE_JS = shipment.PK;

			AssertContainsExactElementsInExactOrder("Should return its parent shipment and declaration as DocManagerSupports.", new IDocManagerSupport[] { (IDocManagerSupport)shipment, declaration }, ((IDocManagerSupportProvider)entry).DocManagerSupports);
		}
	}

	// Manually mocking the BaseJobDeclaration class, due to inconsistent encapsulation making Moq mocking impossible.
	public class DummyBaseJobDeclaration_ForTest : BaseJobDeclaration
	{
		public DummyBaseJobDeclaration_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties to return
		public bool SupportContainerEntryHeaderPivotReturns { get; set; }
		public bool IsCustomsHeaderAmendmentATotalReplacementReturns { get; set; }
		#endregion

		#region Overrides
		protected internal override bool SupportContainerEntryHeaderPivot => SupportContainerEntryHeaderPivotReturns;

		protected internal override bool IsCustomsHeaderAmendmentATotalReplacement => IsCustomsHeaderAmendmentATotalReplacementReturns;
		#endregion
	}

	class CusEntryHeaderSupportsBondedWarehousingTest : TestCaseWithFactory
	{
		[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
		public class CusEntryHeaderSupportsBondedWarehousingForTest : CusEntryHeader
		{
			public CusEntryHeaderSupportsBondedWarehousingForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool ShouldPopulateJE_EntrySubmittedDate
			{
				get { return CanPopulateEntrySubmittedDateInDeclaration; }
			}

			public bool CanPopulateEntrySubmittedDateInDeclaration { get; set; }
		}

		public void TestSupportsBondedWarehousing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "AA", "BB", ZString.Empty, "OP DESC1", "EXW", outOfWarehouse: true);
			var procedure2 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "CC", "DD", ZString.Empty, "OP DESC2", "EXW");
			procedure2.ZZ6_OutOfInwardProcessing = "Y";
			var procedure3 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "EE", "FF", ZString.Empty, "OP DESC3", "EXW");
			procedure3.ZZ6_OutofOutwardProcessing = "Y";
			_ = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "GG", "HH", ZString.Empty, "OP DESC4", "IMP", intoWarehouse: true);
			var procedure5 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "II", "JJ", ZString.Empty, "OP DESC5", "IMP");
			procedure5.ZZ6_IntoInwardProcessing = "Y";
			var procedure6 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "KK", "LL", ZString.Empty, "OP DESC6", "IMP");
			procedure6.ZZ6_IntoOutwardProcessing = "Y";

			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "MM", "NN", ZString.Empty, "OP DESC7", "EXW");
			Factory.Save();

			var whshelper = new WhsDataTestHelper(Factory);
			whshelper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			whshelper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;

			var declaration = Factory.New<DummyBaseJobDeclarationWithEntryInstructions>();
			declaration.SupportMultipleWarehouseEntryCoreReturns = true;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse = whshelper.Warehouse.MainAddress.PK;
			entryInstruction.CEI_OA_Warehouse2 = whshelper.Warehouse2.MainAddress.PK;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			invoiceLine.JI_Procedure = "AABB";
			Assert(entryHeader.SupportsBondedWarehousing);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.Delete();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_Procedure = "GGHH";
			Assert(entryHeader.SupportsBondedWarehousing);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.Delete();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_Procedure = "CCDD";
			Assert(!entryHeader.SupportsBondedWarehousing);
			invoiceLine.JI_Procedure = "IIJJ";
			Assert(!entryHeader.SupportsBondedWarehousing);

			whshelper.Warehouse.CompanyData.OB_CusInventoryForInwardProcessing = true;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.Delete();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_Procedure = "CCDD";
			Assert(entryHeader.SupportsBondedWarehousing);

			whshelper.Warehouse2.CompanyData.OB_CusInventoryForInwardProcessing = true;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.Delete();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_Procedure = "IIJJ";
			Assert(entryHeader.SupportsBondedWarehousing);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.Delete();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_Procedure = "EEFF";
			Assert(!entryHeader.SupportsBondedWarehousing);
			invoiceLine.JI_Procedure = "KKLL";
			Assert(!entryHeader.SupportsBondedWarehousing);

			whshelper.Warehouse.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.Delete();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_Procedure = "EEFF";
			Assert(!entryHeader.SupportsBondedWarehousing);

			whshelper.Warehouse2.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.Delete();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_Procedure = "KKLL";
			Assert(!entryHeader.SupportsBondedWarehousing);

			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			invoiceLine.Delete();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_Procedure = "MMNN";
			Assert(!entryHeader.SupportsBondedWarehousing);
		}

		sealed class DummyBaseJobDeclarationWithEntryInstructions : BaseJobDeclarationWithEntryInstructions
		{
			public DummyBaseJobDeclarationWithEntryInstructions(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool SupportMultipleWarehouseEntryCoreReturns { get; set; }

			protected internal override bool SupportMultipleWarehouseEntryCore => SupportMultipleWarehouseEntryCoreReturns;
		}
	}
}
