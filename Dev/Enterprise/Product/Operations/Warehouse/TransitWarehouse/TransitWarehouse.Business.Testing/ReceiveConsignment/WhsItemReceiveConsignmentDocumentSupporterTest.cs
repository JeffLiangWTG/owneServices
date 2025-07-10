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
	[TestedType(typeof(WhsItemReceiveConsignmentDocumentSupporter))]
	class WhsItemReceiveConsignmentDocumentSupporterTest : DocumentSupporterTest
	{
		#region TestGetContactOrganisation

		public void TestGetContactOrganisation()
		{
			var warehouseOrgHeader = Warehouse.WarehouseAddress.Header;
			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var bookingParty = Factory.New<OrgHeader>();
			var billingParty = Factory.NewWithValidTestData<OrgHeader>();
			var consignment = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK, bookingParty, consignor, consignee);

			var jobHeader = new JobHeader.Loader(consignment).TryLoadOrCreate();
			jobHeader.JH_OA_LocalChargesAddr = billingParty.MainAddress.PK;

			AssertNull(new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", null, DocumentDirection.ANY));

			AssertEquals(warehouseOrgHeader, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.TransitWarehouse, DocumentDirection.ANY).OrgHeader);
			AssertEquals(consignor, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY).OrgHeader);
			AssertEquals(consignee, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY).OrgHeader);
			AssertEquals(billingParty, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.LocalClient, DocumentDirection.ANY).OrgHeader);

			// booking party
			AssertEquals(bookingParty, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.ImportFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.ExportAirFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.ImportSeaFreightAgent, DocumentDirection.ANY).OrgHeader);
			AssertEquals(bookingParty, new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.ImportAirFreightAgent, DocumentDirection.ANY).OrgHeader);

			var billingParty2 = Factory.New<OrgHeader>();
			var consignmentWithoutJobHeader = Helper.CreateReceiveConsignment("RCN1", Warehouse.PK, bookingParty, consignor, consignee);
			Helper.CreateJobDocAddressFromAddress(consignmentWithoutJobHeader, DocAddressTypes.Codes.ClientRequestedBillingParty, billingParty2.MainAddress);
			AssertEquals(billingParty2, new WhsItemReceiveConsignmentDocumentSupporter(consignmentWithoutJobHeader).GetContactOrganisation("", ContactType.LocalClient, DocumentDirection.ANY).OrgHeader);

			// JobDocAddress is overridden
			consignment.ConsignorDocAddress.E2_AddressOverride = true;
			AssertNull(new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.Consignor, DocumentDirection.ANY));

			consignment.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertNull(new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY));

			consignment.BookingPartyDocAddress.E2_AddressOverride = true;
			AssertNull(new WhsItemReceiveConsignmentDocumentSupporter(consignment).GetContactOrganisation("", ContactType.ExportFreightAgent, DocumentDirection.ANY));

			consignmentWithoutJobHeader.ClientRequestedBillToPartyDocAddress.E2_AddressOverride = true;
			AssertNull(new WhsItemReceiveConsignmentDocumentSupporter(consignmentWithoutJobHeader).GetContactOrganisation("", ContactType.LocalClient, DocumentDirection.ANY));
		}

		#endregion

		#region TestShowShowReasonForNotPrinting

		public void TestShowShowReasonForNotPrinting()
		{
			var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
			var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packages = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertEquals("Number should generate as: Enterprise Code + Server Code + 00000001", "ENTSVR00000001", packages.FirstOrDefault()["RefNumber"]);
			}
		}

		#endregion

		#region TestGenericNewPackageIDs

		public void TestGenericNewPackageIDs()
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				var packline1 = Helper.CreatePackageState(consignment, 1, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID1", "BKD");
				var packline2 = Helper.CreatePackageState(consignment, 2, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID2", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID3", "BKD");

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 3, 0);

				packline1.Package.KP_PackageQty = 3;
				packline2.Package.KP_PackageQty = 3;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 6, 0);

				packline1.Package.KP_PackageQty = 2;
				packline2.Package.KP_PackageQty = 2;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 6, 0);

				packline1.Package.KP_PackageQty = 4;
				packline2.Package.KP_PackageQty = 5;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 9, 0);
			}
		}

		public void TestGenericNewPackageIDs_NewFactory_PackageHeaderCurrentPacakgeJobNotNull()
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				var packline1 = Helper.CreatePackageState(consignment, 1, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID1", "BKD");
				var packline2 = Helper.CreatePackageState(consignment, 2, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID2", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID3", "BKD");

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 3, 0);

				var newFactory = new BusinessObjectFactory();

				var rcn_InNewFactory = newFactory.Load<WhsItemReceiveConsignment>(consignment.PK);
				var docSupporter_InNewFactory = ((IDocumentSupportable)rcn_InNewFactory).DocumentSupporter;
				wrappers = docSupporter_InNewFactory.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 3, 0);
				Assert(rcn_InNewFactory.PackageJob.LoosePackageIDs.All(p => p.CurrentPackageJob != null));
			}
		}

		#endregion

		#region TestGenericNewPackageIDs1Doc

		public void TestGenericNewPackageIDs1Doc()
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs1Doc, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				var packline1 = Helper.CreatePackageState(consignment, 1, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID1", "BKD");
				var packline2 = Helper.CreatePackageState(consignment, 2, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID2", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID3", "BKD");

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 3, 0);

				packline1.Package.KP_PackageQty = 3;
				packline2.Package.KP_PackageQty = 3;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 6, 0);

				packline1.Package.KP_PackageQty = 2;
				packline2.Package.KP_PackageQty = 2;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 6, 0);

				packline1.Package.KP_PackageQty = 4;
				packline2.Package.KP_PackageQty = 5;

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 9, 0);
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				var packline1 = Helper.CreatePackageState(consignment, 1, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID1", "BKD");
				var packline2 = Helper.CreatePackageState(consignment, 2, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID2", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID3", "BKD");

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
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID4", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID5", "BKD");

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertPackageWrappers(wrappers, 9, 5);
			}
		}

		public void TestGenericNewPackageIDsAndExistingIDsWithOVP()
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID1", "BKD");
				var rtu = helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, Warehouse.DefaultLocation.PK);
				var ovpPackage = helper.CreateOverpackPackage("OVP1", consignment, rtu, rcn: consignment);
				var innerPackage1 = Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID2", "BKD");
				helper.PackPackageIntoHandlingUnit(ovpPackage, innerPackage1, ZDateTimeOffset.Now, "XXX");
				var innerPackage2 = Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID3", "BKD");
				helper.PackPackageIntoHandlingUnit(ovpPackage, innerPackage2, ZDateTimeOffset.Now, "XXX");

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);
				AssertNotNull(wrappers.FirstOrDefault(w => (ZString)(((BusinessObjectCollection)w["Packages"]).FirstOrDefault()["RefNumber"]) == "AlreadyHaveAnID1"));
				AssertNotNull(wrappers.FirstOrDefault(w => (ZString)(((BusinessObjectCollection)w["Packages"]).FirstOrDefault()["RefNumber"]) == "OVP1"));
			}
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				var packline1 = Helper.CreatePackageState(consignment, 1, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID1", "BKD");
				var packline2 = Helper.CreatePackageState(consignment, 2, "PLT", "", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID2", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID3", "BKD");

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
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID4", "BKD");
				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID5", "BKD");

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				AssertPackagesOfOneWrapper(wrappers, 9, 5);
			}
		}

		public void TestGenericNewAndExistingPackageIDs1DocWithOVP()
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				AssertEquals("Should of generated 0 Labels.", 0, wrappers.Length);

				Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID1", "BKD");
				var rtu = helper.CreateReceiveTransportationUnit("RTU1", Warehouse.PK, Warehouse.DefaultLocation.PK);
				var ovpPackage = helper.CreateOverpackPackage("OVP1", consignment, rtu, rcn: consignment);
				var innerPackage1 = Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID2", "BKD");
				helper.PackPackageIntoHandlingUnit(ovpPackage, innerPackage1, ZDateTimeOffset.Now, "XXX");
				var innerPackage2 = Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID3", "BKD");
				helper.PackPackageIntoHandlingUnit(ovpPackage, innerPackage2, ZDateTimeOffset.Now, "XXX");

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs1Doc, testStmMenu);
				var refNumbers = (wrappers.First()["Packages"] as BusinessObjectCollection).Select(p => p["RefNumber"]).ToArray();
				AssertArrayEqualsByElements(new string[] { "AlreadyHaveAnID1", "OVP1" }, refNumbers);
			}
		}

		#endregion

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
			AssertArrayEqualsByElements(idStrings.ToArray(), packageCollection.Select(p => p["RefNumber"]).ToArray());
		}

		#region TestFixSequence

		public void TestFixSequence()
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
				var consignment = Helper.CreateReceiveConsignment("WRCID", "STD", Warehouse.PK);
				var docSupporter = ((IDocumentSupportable)consignment).DocumentSupporter;

				var testStmMenu = new BusinessObjectFactory().New<StmMenuItem>();
				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);

				var package1 = Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID1", "BKD");
				package1.Package.KP_Sequence = 3;
				var package2 = Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID2", "BKD");
				package2.Package.KP_Sequence = 3;
				var package3 = Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID3", "BKD");
				package3.Package.KP_Sequence = 6;
				var package4 = Helper.CreatePackageState(consignment, 1, "PLT", "AlreadyHaveAnID4", "BKD");
				package4.Package.KP_Sequence = 9;

				var looseID1 = consignment.PackageJob.LoosePackageIDs.AddNew();
				looseID1.KPH_PackageID = "LP1";
				var looseID2 = consignment.PackageJob.LoosePackageIDs.AddNew();
				looseID2.KPH_PackageID = "LP2";
				Factory.Save();

				wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewAndExistingPackageIDs, testStmMenu);

				var fixedSequence = wrappers.Select(wrapper => ((IPackageOverrider)wrapper).DocumentNumber);
				var expectedSequence = new ZInt[] { 1, 2, 3, 4, 5, 6 };
				AssertContainsExactElementsInExactOrder(expectedSequence, fixedSequence);
			}
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			var docSupporter = ((IDocumentSupportable)receiveConsignment).DocumentSupporter;
			AssertEquals(Env.Security.WhsItemReceiveConsignmentCustomizeDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region Implementation

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			base.DoSetupForDocument(command, documentSupportableBO);

			var consignment = (WhsItemReceiveConsignment)documentSupportableBO;
			consignment.WRC_WW_IntendedWarehouse = Warehouse.PK;
			consignment.WRC_ConsignmentID = "WRCID00000001";
			consignment.WRC_JobID = "WRCID00000001";
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(consignment);
			packageJob.Packages.AddNew();
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
			return Factory.New<WhsItemReceiveConsignment>();
		}

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;

		#endregion
	}
}
