using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitDocumentSupporter))]
	public class WhsItemReceiveTransportationUnitDocumentSupporterTest : DocumentSupporterTest
	{
		#region TestShowShowReasonForNotPrinting

		public void TestShowShowReasonForNotPrinting()
		{
			var unit = Helper.CreateReceiveTransportationUnit("WRHID", Warehouse.PK, Warehouse.Rows[0].Locations[0].PK);
			var docSupporter = ((IDocumentSupportable)unit).DocumentSupporter;
			AssertEquals("ShowReasonForNotPrinting", false, docSupporter.ShowReasonForNotPrinting(Core.Constants.DataContext.None, null));
		}

		#endregion

		#region TestGenerateBarcodeLabel

		public void TestGenerateBarcodeLabel()
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
				var unit = Helper.CreateReceiveTransportationUnit("WRHID", Warehouse.PK, Warehouse.Rows[0].Locations[0].PK);
				var docSupporter = ((IDocumentSupportable)unit).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, Factory.New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packages = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertEquals("Number should generate as: Enterprise Code + Server Code + 00000001", "ENTSVR00000001", packages.FirstOrDefault()["RefNumber"]);
			}
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;
			AssertEquals(Env.Security.WhsItemReceiveTransportationUnitCustomizeDocuments, docSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGenericNewPackageID

		public void TestGenericNewPackageID_WithNotCancelledSVREvent()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var warehouse = Helper.CreateWarehouse("TWH", "A", 1, 1);
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				rtu.WRH_ReferenceNumber = "WRH00000001";
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;

				var svrLog = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=Document|RES=Package Label (Generate New ID)|QTY=2");
				Factory.Save();

				AssertEquals("Precondition:", "AAA", svrLog.SL_GS_NKUser);
				AssertEquals("Precondition:", false, svrLog.IsCancelled);

				var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 2, wrappers.Length);
				var packageCollection1 = (BusinessObjectCollection)wrappers[0]["Packages"];
				var packageCollection2 = (BusinessObjectCollection)wrappers[1]["Packages"];
				AssertEquals("Should generate a new package id", "WRH00000001-001", packageCollection1.FirstOrDefault()["RefNumber"]);
				AssertEquals("Should generate a new package id", "WRH00000001-002", packageCollection2.FirstOrDefault()["RefNumber"]);
				AssertEquals("The log should be cancelled", true, svrLog.SL_IsCancelled);
			}
		}

		public void TestGenericNewPackageID_WithNotCancelledSVREvent_MultipleUsers()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var warehouse = Helper.CreateWarehouse("TWH", "A", 1, 1);
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				rtu.WRH_ReferenceNumber = "WRH00000001";
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;

				var svrLog1 = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=Document|RES=Package Label (Generate New ID)|QTY=2");
				svrLog1.SL_GS_NKUser = "AAA";
				var svrLog2 = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=Document|RES=Package Label (Generate New ID)|QTY=5");
				svrLog2.SL_GS_NKUser = "BBB";
				Factory.Save();

				AssertEquals("Precondition:", "AAA", GlbStaff.CurrentUser.GS_Code);
				AssertEquals("Precondition:", "AAA", svrLog1.SL_GS_NKUser);
				AssertEquals("Precondition:", false, svrLog1.IsCancelled);
				AssertEquals("Precondition:", "BBB", svrLog2.SL_GS_NKUser);
				AssertEquals("Precondition:", false, svrLog2.IsCancelled);

				var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 2, wrappers.Length);
				var packageCollection1 = (BusinessObjectCollection)wrappers[0]["Packages"];
				var packageCollection2 = (BusinessObjectCollection)wrappers[1]["Packages"];
				AssertEquals("Should generate a new package id", "WRH00000001-001", packageCollection1.FirstOrDefault()["RefNumber"]);
				AssertEquals("Should generate a new package id", "WRH00000001-002", packageCollection2.FirstOrDefault()["RefNumber"]);
				AssertEquals("The log should be cancelled", true, svrLog1.SL_IsCancelled);
				AssertEquals("The log should not be cancelled", false, svrLog2.SL_IsCancelled);
			}
		}

		public void TestGenericNewPackageIDs1Doc_WithNotCancelledSVREvent()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var warehouse = Helper.CreateWarehouse("TWH", "A", 1, 1);
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				rtu.WRH_ReferenceNumber = "WRH00000001";
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;

				var svrLog = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=Document|RES=Package Label (Generate New ID)|QTY=2");
				Factory.Save();

				AssertEquals("Precondition:", "AAA", svrLog.SL_GS_NKUser);
				AssertEquals("Precondition:", false, svrLog.IsCancelled);

				var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs1Doc, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packageCollection = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertArrayEqualsByElements(new string[] { "WRH00000001-001", "WRH00000001-002" }, packageCollection.Select(p => p["RefNumber"]).ToArray());
				AssertEquals("The log should be cancelled", true, svrLog.SL_IsCancelled);
			}
		}

		public void TestGenericNewPackageIDs1Doc_WithNotCancelledSVREvent_MultipleUsers()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var warehouse = Helper.CreateWarehouse("TWH", "A", 1, 1);
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				rtu.WRH_ReferenceNumber = "WRH00000001";
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;

				var svrLog1 = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=Document|RES=Package Label (Generate New ID)|QTY=2");
				svrLog1.SL_GS_NKUser = "AAA";
				var svrLog2 = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=Document|RES=Package Label (Generate New ID)|QTY=5");
				svrLog2.SL_GS_NKUser = "BBB";
				Factory.Save();

				AssertEquals("Precondition:", "AAA", GlbStaff.CurrentUser.GS_Code);
				AssertEquals("Precondition:", "AAA", svrLog1.SL_GS_NKUser);
				AssertEquals("Precondition:", false, svrLog1.IsCancelled);
				AssertEquals("Precondition:", "BBB", svrLog2.SL_GS_NKUser);
				AssertEquals("Precondition:", false, svrLog2.IsCancelled);

				var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageIDs1Doc, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packageCollection = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertArrayEqualsByElements(new string[] { "WRH00000001-001", "WRH00000001-002" }, packageCollection.Select(p => p["RefNumber"]).ToArray());
				AssertEquals("The log should be cancelled", true, svrLog1.SL_IsCancelled);
				AssertEquals("The log should not be cancelled", false, svrLog2.SL_IsCancelled);
			}
		}

		public void TestGenericNewPackageID_WithCancelledSVREvent()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var warehouse = Helper.CreateWarehouse("TWH", "A", 1, 1);
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				rtu.WRH_ReferenceNumber = "WRH00000001";
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;

				var svrLog = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=Document|RES=Package Label (Generate New ID)|QTY=2");
				svrLog.Cancel();
				Factory.Save();

				AssertEquals("Precondition:", "AAA", svrLog.SL_GS_NKUser);
				AssertEquals("Precondition:", true, svrLog.IsCancelled);

				var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packageCollection1 = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertEquals("Should generate a new package id", "WRH00000001-001", packageCollection1.FirstOrDefault()["RefNumber"]);
			}
		}

		public void TestGenericNewPackageID_NoSVREvent()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var warehouse = Helper.CreateWarehouse("TWH", "A", 1, 1);
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				rtu.WRH_ReferenceNumber = "WRH00000001";
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;
				Factory.Save();

				AssertNull("Precondition: No SVR event", rtu.GetLogs().MostRecentLogByEventTime(Events.ServiceRequested));

				var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packageCollection1 = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertEquals("Should generate a new package id", "WRH00000001-001", packageCollection1.FirstOrDefault()["RefNumber"]);
			}
		}

		public void TestGenericNewPackageID_IncorrectSVREvent()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var warehouse = Helper.CreateWarehouse("TWH", "A", 1, 1);
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				rtu.WRH_ReferenceNumber = "WRH00000001";
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;
				Factory.Save();

				var svrLog = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=XXXX|RES=Package Label (Generate New ID)|QTY=5");
				Factory.Save();

				AssertEquals("Precondition:", "AAA", svrLog.SL_GS_NKUser);
				AssertEquals("Precondition:", false, svrLog.IsCancelled);

				var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packageCollection1 = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertEquals("Should generate a new package id", "WRH00000001-001", packageCollection1.FirstOrDefault()["RefNumber"]);
				AssertEquals("The log should not be cancelled", false, svrLog.SL_IsCancelled);
			}
		}

		public void TestGenericNewPackageID_SVREventWithIncorrectQty()
		{
			var staff = Helper.CreateGlbStaff("AAA", "AAA");
			var warehouse = Helper.CreateWarehouse("TWH", "A", 1, 1);
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				rtu.WRH_ReferenceNumber = "WRH00000001";
				rtu.WRH_WL_StagingLocation = warehouse.DefaultInboundDockDoorLocation.PK;
				Factory.Save();

				var svrLog = rtu.Logs.AddNew(Events.ServiceRequested, "TYP=Document|RES=Package Label (Generate New ID)|QTY=XXX");
				Factory.Save();

				AssertEquals("Precondition:", "AAA", svrLog.SL_GS_NKUser);
				AssertEquals("Precondition:", false, svrLog.IsCancelled);

				var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;

				var wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericNewPackageID, new BusinessObjectFactory().New<StmMenuItem>());

				AssertEquals("Should of generated one Label.", 1, wrappers.Length);
				var packageCollection1 = (BusinessObjectCollection)wrappers[0]["Packages"];
				AssertEquals("Should generate a new package id", "WRH00000001-001", packageCollection1.FirstOrDefault()["RefNumber"]);
				AssertEquals("The log should be cancelled", true, svrLog.SL_IsCancelled);
			}
		}

		#endregion

		#region TestGetContactOrganisation

		public void TestGetContactOrganisation()
		{
			var warehouseOrgHeader = Warehouse.WarehouseAddress.Header;
			var transportCompany = Factory.New<OrgHeader>();
			var unit = Helper.CreateReceiveTransportationUnit("WRHID", Warehouse.PK, Warehouse.Rows[0].Locations[0].PK);
			Helper.CreateJobDocAddressFromAddress(unit, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			AssertNull(new WhsItemReceiveTransportationUnitDocumentSupporter(unit).GetContactOrganisation("", null, DocumentDirection.ANY));
			AssertEquals(warehouseOrgHeader, new WhsItemReceiveTransportationUnitDocumentSupporter(unit).GetContactOrganisation("", ContactType.TransitWarehouse, DocumentDirection.ANY).OrgHeader);

			AssertEquals(transportCompany, new WhsItemReceiveTransportationUnitDocumentSupporter(unit).GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY).OrgHeader);

			unit.TransportCompany.E2_AddressOverride = true;
			AssertNull(new WhsItemReceiveTransportationUnitDocumentSupporter(unit).GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ANY));
		}

		#endregion

		#region TestDocumentSupporter_SupportedChildBusinessContexts

		public void TestDocumentSupporter_SupportedChildBusinessContexts()
		{
			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;
			AssertContainsExactElementsInAnyOrder(new BusinessContext[] { BusinessContext.TransitRcvConsignmnt, BusinessContext.TransitReceiveASN }, docSupporter.SupportedChildBusinessContexts);
		}

		#endregion

		#region TestGetChildCollection

		public void TestGetChildCollection_TransitRcvConsignmnt()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);

			var rcnMenuItem = Factory.New<StmMenuItem>();
			rcnMenuItem.SU_MenuName = "RTU Menu";

			var rtu = Helper.CreateReceiveTransportationUnit("WRHID", Warehouse.PK, Warehouse.Rows[0].Locations[0].PK);
			var rcn1 = Helper.CreateReceiveConsignment("RC1", data.Whs1.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC2", data.Whs1.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RC3", data.Whs1.PK);
			var p1 = Helper.CreatePackageState(rcn1, 1, "PKG", "", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var p2 = Helper.CreatePackageState(rcn1, 2, "PKG", "", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var p3 = Helper.CreatePackageState(rcn2, 3, "PLT", "", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var p4 = Helper.CreatePackageState(rcn2, 1, "PLT", "", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var p5 = Helper.CreatePackageState(rcn3, 1, "PLT", "", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;
			var childCollection_3RCNs = docSupporter.GetChildCollection(rcnMenuItem, BusinessContext.TransitRcvConsignmnt, null);
			AssertContainsExactElementsInAnyOrder(new[] { rcn1, rcn2, rcn3 }, childCollection_3RCNs);
		}

		public void TestGetChildCollection_TransitReceiveASN()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);

			var rcnMenuItem = Factory.New<StmMenuItem>();
			rcnMenuItem.SU_MenuName = "RTU Menu";

			var rtu = Helper.CreateReceiveTransportationUnit("WRHID", Warehouse.PK, Warehouse.Rows[0].Locations[0].PK);
			var asn1 = Helper.CreateReceiveASN("ASN1", data.Whs1.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", data.Whs1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn2.PK);

			var docSupporter = ((IDocumentSupportable)rtu).DocumentSupporter;
			var childCollection_2ASNs = docSupporter.GetChildCollection(rcnMenuItem, BusinessContext.TransitReceiveASN, null);
			AssertContainsExactElementsInAnyOrder(new[] { asn1, asn2 }, childCollection_2ASNs);
		}

		#endregion

		#region Implementation

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			base.DoSetupForDocument(command, documentSupportableBO);

			var unit = (WhsItemReceiveTransportationUnit)documentSupportableBO;
			unit.WRH_WW_Warehouse = Warehouse.PK;
			unit.WRH_ReferenceNumber = "WRHID00000001";
			unit.WRH_WL_StagingLocation = Warehouse.Rows[0].Locations[0].PK;
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
			return Factory.New<WhsItemReceiveTransportationUnit>();
		}

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;

		#endregion
	}
}
