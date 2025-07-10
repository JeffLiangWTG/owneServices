using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchConsignmentDocumentSupporter))]
	class WhsItemDispatchConsignmentDocumentSupporterTest : DocumentSupporterTest
	{
		#region TestGetContactOrganisation

		public void TestGetContactOrganisation()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouseOrgHeader = Warehouse.WarehouseAddress.Header;
			var consignor = Factory.New<OrgHeader>();
			var consignor2 = Factory.New<OrgHeader>();
			var bookingParty = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var billingParty = Factory.NewWithValidTestData<OrgHeader>();

			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", Warehouse.PK, consignor: consignor);
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", Warehouse.PK, consignor: consignor);
			var receiveConsignment3 = helper.CreateReceiveConsignment("RCN3", Warehouse.PK, consignor: consignor2);
			var packageState1ForConsignment1 = receiveConsignment1.PackageStates.AddNew();
			var packageState2ForConsignment1 = receiveConsignment1.PackageStates.AddNew();
			var packageState1ForConsignment2 = receiveConsignment2.PackageStates.AddNew();
			var packageState2ForConsignment2 = receiveConsignment2.PackageStates.AddNew();
			var packageState1ForConsignment3 = receiveConsignment3.PackageStates.AddNew();

			var consignmentWithMultiRCNs_SameConsignor = helper.CreateDispatchConsignment("DCN1", Warehouse.PK, bookedByParty: bookingParty, consignee: consignee);
			var consignmentWithMultiRCNs_DifferentConsignors = helper.CreateDispatchConsignment("DCN1", Warehouse.PK, bookedByParty: bookingParty, consignee: consignee);
			var consignmentWithSingleReceiveConsignment = helper.CreateDispatchConsignment("DCN2", Warehouse.PK, bookedByParty: bookingParty, consignee: consignee);

			var jobHeader = new JobHeader.Loader(consignmentWithMultiRCNs_SameConsignor).TryLoadOrCreate();
			jobHeader.JH_OA_LocalChargesAddr = billingParty.MainAddress.PK;

			packageState1ForConsignment1.WPS_WDC_TransitDispatchConsignment = consignmentWithMultiRCNs_SameConsignor.PK;
			packageState1ForConsignment2.WPS_WDC_TransitDispatchConsignment = consignmentWithMultiRCNs_SameConsignor.PK;
			packageState2ForConsignment2.WPS_WDC_TransitDispatchConsignment = consignmentWithMultiRCNs_DifferentConsignors.PK;
			packageState1ForConsignment3.WPS_WDC_TransitDispatchConsignment = consignmentWithMultiRCNs_DifferentConsignors.PK;
			packageState2ForConsignment1.WPS_WDC_TransitDispatchConsignment = consignmentWithSingleReceiveConsignment.PK;

			AssertNull(new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", null, DocumentDirection.ANY));
			AssertEquals(warehouseOrgHeader, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.TransitWarehouse, DocumentDirection.ANY).OrgHeader);
			AssertNull(new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_DifferentConsignors).GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY));
			AssertEquals(consignor, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY).OrgHeader);
			AssertEquals(consignee, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader);
			AssertEquals(billingParty, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.LocalClient, DocumentDirection.ANY).OrgHeader);

			// booking party
			AssertEquals(bookingParty, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.ImportSeaFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.ImportAirFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithMultiRCNs_SameConsignor).GetContactOrganisation("", ContactType.ExportAirFreightAgent, DocumentDirection.ANY).OrgHeader);

			AssertEquals(consignor, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithSingleReceiveConsignment).GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY).OrgHeader);

			var billingParty2 = Factory.New<OrgHeader>();
			var consignmentWithoutJobHeader = helper.CreateDispatchConsignment("RCN3", Warehouse.PK, bookedByParty: bookingParty, consignee: consignee);
			helper.CreateJobDocAddressFromAddress(consignmentWithoutJobHeader, DocAddressTypes.Codes.ClientRequestedBillingParty, billingParty2.MainAddress);
			AssertEquals(billingParty2, new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithoutJobHeader).GetContactOrganisation("", ContactType.LocalClient, DocumentDirection.ANY).OrgHeader);

			// JobDocAddress is overridden
			consignmentWithSingleReceiveConsignment.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertNull(new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithSingleReceiveConsignment).GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY));

			consignmentWithSingleReceiveConsignment.BookingPartyDocAddress.E2_AddressOverride = true;
			AssertNull(new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithSingleReceiveConsignment).GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY));

			consignmentWithoutJobHeader.ClientRequestedBillToPartyDocAddress.E2_AddressOverride = true;
			AssertNull(new WhsItemDispatchConsignmentDocumentSupporter(consignmentWithoutJobHeader).GetContactOrganisation("", ContactType.LocalClient, DocumentDirection.ANY));
		}

		#endregion

		#region TestShowShowReasonForNotPrinting

		public void TestShowShowReasonForNotPrinting()
		{
			var dispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			var docSupporter = ((IDocumentSupportable)dispatchConsignment).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			var dispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			var docSupporter = ((IDocumentSupportable)dispatchConsignment).DocumentSupporter;
			AssertEquals(Env.Security.WhsItemDispatchConsignmentCustomizeDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGenericNewPackageID

		public void TestGenericNewPackageID()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var numberCustomisation = Helper.GetEnterpriseAndServerCodeNumberCustomisation();
			using (branch.SetAsTemporaryContext()) // required to set registration key
			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, numberCustomisation))
			{
				var consignment = Helper.CreateDispatchConsignment("WdCID", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packages = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertEquals("Number should generate as: Enterprise Code + Server Code + 00000001", "ENTSVR00000001", packages.FirstOrDefault()["RefNumber"]);
			}
		}

		#endregion

		#region TestGenericNewPackageIDsAndExistingIDs

		public void TestGenericNewPackageIDsAndExistingIDs()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var numberCustomisation = Helper.GetEnterpriseAndServerCodeNumberCustomisation();
			using (branch.SetAsTemporaryContext()) // required to set registration key
			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, numberCustomisation))
			{
				var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var dcn = helper.CreateDispatchConsignment("WDCID", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)dcn).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				var packline1 = Helper.CreatePackageState(rcn, 1, "PLT", "", "BKD", dispatchConsignment: dcn);
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
				var packline2 = Helper.CreatePackageState(rcn, 2, "PLT", "", "BKD", dispatchConsignment: dcn);
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID2", "BKD", dispatchConsignment: dcn);
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID3", "BKD", dispatchConsignment: dcn);

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 3, 3);

				packline1.Package.KP_PackageQty = 3;
				packline2.Package.KP_PackageQty = 3;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 6, 3);

				packline1.Package.KP_PackageQty = 2;
				packline2.Package.KP_PackageQty = 2;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 6, 3);

				packline1.Package.KP_PackageQty = 4;
				packline2.Package.KP_PackageQty = 5;
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID4", "BKD", dispatchConsignment: dcn);
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID5", "BKD", dispatchConsignment: dcn);

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 9, 5);
			}
		}

		void AssertPackageWrappers(DocumentWrapper[] wrappers, int newIDCount, int existingIDCount)
		{
			CombineAssertions(() =>
			{
				var total = newIDCount + existingIDCount;
				AssertEquals($"Should of generated {total} Labels.", total, wrappers.Length);
				for (var i = 0; i < newIDCount; i++)
				{
					ZString expectedRefNumber = "ENTSVR" + (i + 1).ToString().PadLeft(8, '0');
					AssertNotNull($"Number should generate as: {expectedRefNumber}", wrappers.FirstOrDefault(w => (ZString)(((BusinessObjectCollection)w["Packages"]).FirstOrDefault()["RefNumber"]) == expectedRefNumber));
				}
				for (var i = 0; i < existingIDCount; i++)
				{
					var expectedRefNumber = $"AlreadyHaveAnID{i + 1}";
					AssertNotNull($"Non loose package ID should generate as: {expectedRefNumber}", wrappers.FirstOrDefault(w => (ZString)(((BusinessObjectCollection)w["Packages"]).FirstOrDefault()["RefNumber"]) == expectedRefNumber));
				}
			});
		}

		#endregion

		#region TestGenericNewAndExistingPackageIDs1Doc

		public void TestGenericNewAndExistingPackageIDs1Doc()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var numberCustomisation = Helper.GetEnterpriseAndServerCodeNumberCustomisation();
			using (branch.SetAsTemporaryContext()) // required to set registration key
			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, numberCustomisation))
			{
				var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var dcn = helper.CreateDispatchConsignment("WDCID", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)dcn).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				var packline1 = Helper.CreatePackageState(rcn, 1, "PLT", "", "BKD", dispatchConsignment: dcn);
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
				var packline2 = Helper.CreatePackageState(rcn, 2, "PLT", "", "BKD", dispatchConsignment: dcn);
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID2", "BKD", dispatchConsignment: dcn);
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID3", "BKD", dispatchConsignment: dcn);

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 3, 3);

				packline1.Package.KP_PackageQty = 3;
				packline2.Package.KP_PackageQty = 3;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 6, 3);

				packline1.Package.KP_PackageQty = 2;
				packline2.Package.KP_PackageQty = 2;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 6, 3);

				packline1.Package.KP_PackageQty = 4;
				packline2.Package.KP_PackageQty = 5;
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID4", "BKD", dispatchConsignment: dcn);
				Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID5", "BKD", dispatchConsignment: dcn);

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 9, 5);
			}
		}

		void AssertPackagesOfOneWrapper(DocumentWrapper[] wrappers, int newIDCount, int existingIDCount)
		{
			AssertEquals("Should of generated one Label.", 1, wrappers.Length);
			var packageCollection = (BusinessObjectCollection)wrappers[0]["Packages"];
			var idStrings = new List<string>();
			for (var i = 0; i < existingIDCount; i++)
			{
				idStrings.Add($"AlreadyHaveAnID{i + 1}");
			}
			for (var i = 0; i < newIDCount; i++)
			{
				idStrings.Add("ENTSVR" + (i + 1).ToString().PadLeft(8, '0'));
			}
			AssertContainsExactElementsInAnyOrder(idStrings.ToArray(), packageCollection.Select(p => p["RefNumber"].ToString()).ToArray());
		}

		#endregion

		#region TestFixSequence

		public void TestFixSequence_DocumentNumber()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			var numberCustomisation = Helper.GetEnterpriseAndServerCodeNumberCustomisation();
			using (branch.SetAsTemporaryContext()) // required to set registration key
			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, numberCustomisation))
			{
				var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var dcn = helper.CreateDispatchConsignment("WDCID", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)dcn).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);

				var package1 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
				package1.Package.KP_Sequence = 3;
				var package2 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID2", "BKD", dispatchConsignment: dcn);
				package2.Package.KP_Sequence = 3;
				var package3 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID3", "BKD", dispatchConsignment: dcn);
				package3.Package.KP_Sequence = 6;
				var package4 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID4", "BKD", dispatchConsignment: dcn);
				package4.Package.KP_Sequence = 9;

				var looseID1 = dcn.PackageJob.LoosePackageIDs.AddNew();
				looseID1.KPH_PackageID = "LP1";
				var looseID2 = dcn.PackageJob.LoosePackageIDs.AddNew();
				looseID2.KPH_PackageID = "LP2";
				Factory.Save();

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);

				var fixedSequence = wrappers.Select(wrapper => ((IPackageOverrider)wrapper).DocumentNumber);
				var expectedSequence = new ZInt[] { 1, 2, 3, 4, 5, 6, };
				AssertContainsExactElementsInExactOrder(expectedSequence, fixedSequence);
			}
		}

		public void TestFixSequence_Sequence_GenericNewAndExistingPackageIDs1Doc()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();

			var numberCustomisation = Helper.GetEnterpriseAndServerCodeNumberCustomisation();
			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, numberCustomisation))
			{
				var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var dcn = helper.CreateDispatchConsignment("WDCID", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)dcn).DocumentSupporter;

				var package1 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
				package1.Package.KP_Sequence = 1;
				var package2 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID2", "BKD", dispatchConsignment: dcn);
				package2.Package.KP_Sequence = 3;
				var package3 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID3", "BKD", dispatchConsignment: dcn);
				package3.Package.KP_Sequence = 2;
				var package4 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID4", "BKD", dispatchConsignment: dcn);
				package4.Package.KP_Sequence = 4;

				var looseID1 = dcn.PackageJob.LoosePackageIDs.AddNew();
				looseID1.KPH_PackageID = "LP1";

				var looseID2 = dcn.PackageJob.LoosePackageIDs.AddNew();
				looseID2.KPH_PackageID = "LP2";

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);

				var packageCollection = (BusinessObjectCollection)wrappers[0]["Packages"];
				var fixedSequence = packageCollection.Select(p => p["RefNumber"].ToString()).ToArray();
				var expectedSequence = new string[] { "AlreadyHaveAnID1", "AlreadyHaveAnID3", "AlreadyHaveAnID2", "AlreadyHaveAnID4", "LP1", "LP2" };
				AssertContainsExactElementsInExactOrder(expectedSequence, fixedSequence);
			}
		}

		public void TestFixSequence_Sequence_GenericNewAndExistingPackageIDs1()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();

			var numberCustomisation = Helper.GetEnterpriseAndServerCodeNumberCustomisation();
			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, numberCustomisation))
			{
				var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var dcn = helper.CreateDispatchConsignment("WDCID", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)dcn).DocumentSupporter;

				var package1 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
				package1.Package.KP_Sequence = 1;
				var package2 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID2", "BKD", dispatchConsignment: dcn);
				package2.Package.KP_Sequence = 3;
				var package3 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID3", "BKD", dispatchConsignment: dcn);
				package3.Package.KP_Sequence = 2;
				var package4 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID4", "BKD", dispatchConsignment: dcn);
				package4.Package.KP_Sequence = 4;

				var looseID1 = dcn.PackageJob.LoosePackageIDs.AddNew();
				looseID1.KPH_PackageID = "LP1";

				var looseID2 = dcn.PackageJob.LoosePackageIDs.AddNew();
				looseID2.KPH_PackageID = "LP2";

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);

				var fixedSequence = wrappers.Select(wrapper => ((BusinessObjectCollection)wrapper["Packages"]).First()["RefNumber"].ToString());
				var expectedSequence = new string[] { "AlreadyHaveAnID1", "AlreadyHaveAnID3", "AlreadyHaveAnID2", "AlreadyHaveAnID4", "LP1", "LP2" };
				AssertContainsExactElementsInExactOrder(expectedSequence, fixedSequence);
			}
		}

		#endregion

		#region Implementation

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			base.DoSetupForDocument(command, documentSupportableBO);

			var consignment = (WhsItemDispatchConsignment)documentSupportableBO;
			consignment.WDC_WW_Warehouse = Warehouse.PK;
			consignment.WDC_ConsignmentID = "WDCID00000001";
			consignment.WDC_JobID = "WDCID00000001";
		}

		WhsWarehouse Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					var warehouseFactory = new BusinessObjectFactory();
					var warehouseHelper = new WhsTransitTestHelper(warehouseFactory);
					var warehouseInWarehouseFactory = warehouseHelper.CreateWarehouse("TWH", "A", 1, 1);
					warehouseFactory.Save();
					warehouse = Factory.Load<WhsWarehouse>(warehouseInWarehouseFactory.PK);
				}
				return warehouse;
			}
		}
		WhsWarehouse warehouse;

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<WhsItemDispatchConsignment>();
		}

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;

		#endregion
	}
}
