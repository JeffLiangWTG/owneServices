using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestUnsupportDataContextsShouldNotThrowException()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertNoExceptionThrown(() => cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Shipment, null));
		}

		public void TestSupportedDataContext()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			AssertEquals("Constants.DataContext.ContainerLeg is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.ContainerLeg)));
			AssertEquals("Constants.DataContext.Container is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Container)));
			AssertEquals("Constants.DataContext.Cartage is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.Cartage)));
			AssertEquals("Constants.DataContext.CombinedCartageAdvice is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CombinedCartageAdvice)));
			AssertEquals("Constants.DataContext.CartageAdvice is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CartageAdvice)));
			AssertEquals("Constants.DataContext.TimeSlotRequest is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.TimeSlotRequest)));
			AssertEquals("Constants.DataContext.CommonContainer is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CommonContainer)));
			AssertEquals("Constants.DataContext.GenericFreightJob is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
			AssertEquals("Constants.DataContext.GenericFreightJobByContainerIfFCL is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJobByContainerIfFCL)));
			AssertEquals("Constants.DataContext.GenericFreightJobByContainer is Supported.", true, cartage.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJobByContainer)));
		}

		public void TestGetDocumentWrappers_GenericFreightJobByContainer()
		{
			var cartageSummaryMenu = Factory.New<StmMenuItem>();
			cartageSummaryMenu.SU_MenuName = "Summary Sheet";
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCNEtoCYD;
			cartage.ContainerBookedMoves.AddNew();
			var wrappers = cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByContainer, cartageSummaryMenu);
			AssertEquals(1, wrappers.Length);
			AssertEquals(cartage, wrappers[0].WrappedObject);
		}

		public void TestGetDocumentWrappers_GenericFreightJobByContainerIfFCL()
		{
			var cartageSummaryMenu = Factory.New<StmMenuItem>();
			cartageSummaryMenu.SU_MenuName = "Summary Sheet";
			// containerised
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCNEtoCYD, 1);
			var docSupporter = (CartageDocumentSupporter)cartage.DocumentSupporter;
			var wrappers = docSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByContainerIfFCL, cartageSummaryMenu);
			AssertEquals(1, wrappers.Length);
			AssertEquals(cartage, wrappers[0].WrappedObject);
			// loose
			cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirImport, 2);
			docSupporter = (CartageDocumentSupporter)cartage.DocumentSupporter;
			wrappers = docSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByContainerIfFCL, cartageSummaryMenu);
			AssertEquals(1, wrappers.Length);
			AssertEquals(cartage, wrappers[0].WrappedObject);
			AssertEquals(cartage.LooseBookedMoves[0].CartageLegs[0], docSupporter.GetLegsForTest(wrappers[0])[0]);
			AssertEquals(cartage.LooseBookedMoves[1].CartageLegs[0], docSupporter.GetLegsForTest(wrappers[0])[1]);
			var move1Legs = cartage.LooseBookedMoves[0].CartageLegs;
			var move2Legs = cartage.LooseBookedMoves[1].CartageLegs;
			var legWrappers = docSupporter.GetLegsForTest(wrappers[0]);
			AssertContainsExactElementsInAnyOrder("Should only contain the loose leg", new[] { move1Legs[0], move2Legs[0] }, legWrappers);
			// mixed
			cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLPackLooseFromSHP, 1);
			docSupporter = (CartageDocumentSupporter)cartage.DocumentSupporter;
			wrappers = docSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJobByContainerIfFCL, cartageSummaryMenu);
			AssertEquals(2, wrappers.Length);
			AssertEquals(cartage, wrappers[0].WrappedObject);
			AssertEquals(cartage, wrappers[1].WrappedObject);
			legWrappers = docSupporter.GetLegsForTest(wrappers[0]);
			AssertContainsExactElementsInAnyOrder("Should only contain the loose leg", new[] { cartage.LooseBookedMoves[0].CartageLegs[0] }, legWrappers[0]);
		}

		public void TestGetDocumentWrappers_ContainersCartageAdvice()
		{
			var cartageAdviceMenu = Factory.New<StmMenuItem>();
			cartageAdviceMenu.SU_MenuName = "something Cartage Advice something";
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			CommonContainer container2 = cartage.ContainerBookedMoves.AddNew().Container;
			DocumentWrapper[] wrappers = cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, cartageAdviceMenu);
			AssertEquals("Since there are no loose jobs it should return zero wrappers", 0, wrappers.Length);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			wrappers = cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, cartageAdviceMenu);
			AssertEquals(2, wrappers.Length);
			AssertEquals(cartage, wrappers[0].WrappedObject);
			AssertEquals(cartage, wrappers[1].WrappedObject);
		}

		public void TestGetDocumentWrappers_LooseJobsCartageAdvice()
		{
			var now = ZDateTime.Now;
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			commonCartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_DomesticLooseDelivery;
			var bookedMove1 = commonCartage.LooseBookedMoves.AddNew();
			var bookedMove2 = commonCartage.LooseBookedMoves.AddNew();
			var cartageLeg1 = bookedMove1.CartageLegs.AddNew();
			var cartageLeg2 = bookedMove2.CartageLegs.AddNew();
			var pickUpOrg1 = commonCartage.DocAddresses.AddNew();
			var pickUpOrg2 = commonCartage.DocAddresses.AddNew();
			var deliveryOrg1 = commonCartage.DocAddresses.AddNew();
			var deliveryOrg2 = commonCartage.DocAddresses.AddNew();
			cartageLeg1.JU_E2PickupAddressID = pickUpOrg1.PK;
			cartageLeg1.JU_E2DeliveryAddressID = deliveryOrg1.PK;
			cartageLeg1.JU_PlannedPickupTime = now.AddDays(1);
			cartageLeg1.JU_EstimatedDeliveryTime = now.AddDays(2);
			bookedMove1.EW_DropMode = "PSL";
			cartageLeg2.JU_E2PickupAddressID = pickUpOrg1.PK;
			cartageLeg2.JU_E2DeliveryAddressID = deliveryOrg1.PK;
			cartageLeg2.JU_PlannedPickupTime = now.AddDays(1);
			cartageLeg2.JU_EstimatedDeliveryTime = now.AddDays(2);
			bookedMove2.EW_DropMode = "PSL";
			AssertLegsAreGrouped(commonCartage, cartageLeg1, cartageLeg2);
			cartageLeg2.JU_E2PickupAddressID = pickUpOrg2.PK;
			AssertLegsAreNotGrouped(commonCartage, cartageLeg1, cartageLeg2);
			cartageLeg2.JU_E2PickupAddressID = pickUpOrg1.PK;
			cartageLeg2.JU_E2DeliveryAddressID = deliveryOrg2.PK;
			AssertLegsAreNotGrouped(commonCartage, cartageLeg1, cartageLeg2);
			cartageLeg2.JU_E2DeliveryAddressID = deliveryOrg1.PK;
			cartageLeg2.JU_PlannedPickupTime = now.AddDays(4);
			AssertLegsAreNotGrouped(commonCartage, cartageLeg1, cartageLeg2);
			cartageLeg2.JU_PlannedPickupTime = now.AddDays(1);
			cartageLeg2.JU_EstimatedDeliveryTime = now.AddDays(4);
			AssertLegsAreNotGrouped(commonCartage, cartageLeg1, cartageLeg2);
			cartageLeg2.JU_EstimatedDeliveryTime = now.AddDays(2);
			bookedMove2.EW_DropMode = "HSL";
			AssertLegsAreNotGrouped(commonCartage, cartageLeg1, cartageLeg2);
			bookedMove2.EW_DropMode = "PSL";
			AssertLegsAreGrouped(commonCartage, cartageLeg1, cartageLeg2);
		}

		void AssertLegsAreGrouped(CommonCartage commonCartage, CommonCartageLeg cartageLeg1, CommonCartageLeg cartageLeg2)
		{
			AssertLegsAreGrouped(true, commonCartage, cartageLeg1, cartageLeg2);
		}

		void AssertLegsAreNotGrouped(CommonCartage commonCartage, CommonCartageLeg cartageLeg1, CommonCartageLeg cartageLeg2)
		{
			AssertLegsAreGrouped(false, commonCartage, cartageLeg1, cartageLeg2);
		}

		void AssertLegsAreGrouped(bool isGrouped, CommonCartage commonCartage, CommonCartageLeg cartageLeg1, CommonCartageLeg cartageLeg2)
		{
			var wrapperCountExpected = isGrouped ? 1 : 2;
			var cartageAdviceMenu = Factory.New<StmMenuItem>();
			cartageAdviceMenu.SU_MenuName = "something Cartage Advice something";
			var documentSupporter = (CartageDocumentSupporter)commonCartage.DocumentSupporter;
			DocumentWrapper[] wrappers = documentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, cartageAdviceMenu);
			AssertEquals("Since cartage legs contain same information they should be grouped to one document wrapper.", wrapperCountExpected, wrappers.Length);
			if (isGrouped)
			{
				AssertContainsExactElementsInAnyOrder("Cartageleg1 should be in legcollection.", new[] { cartageLeg1, cartageLeg2 }, documentSupporter.GetLegsForTest(wrappers[0]));
			}
			else
			{
				AssertNotEquals("Since both contains different legs wrappers should be different.", wrappers[0], wrappers[1]);
				AssertContainsExactElementsInAnyOrder("Cartageleg1 should be in legcollection.", new[] { cartageLeg1 }, documentSupporter.GetLegsForTest(wrappers[0]));
				AssertContainsExactElementsInAnyOrder("Cartageleg2 should be in legcollection.", new[] { cartageLeg2 }, documentSupporter.GetLegsForTest(wrappers[1]));
			}
		}

		public void TestGetContactOrganisationForTRN()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			OrgHeader transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			transportProvider.OH_FullName = "ABC Transport";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = transportProvider.PK;
			Factory.Save();
			AssertEquals("Local Cartage Contact Org should be Client", transportProvider.OH_FullName, cartage.DocumentSupporter.GetContactOrganisation("", ContactType.LocalTransport, DocumentDirection.ARV).OrgHeader.FullName);
		}

		public void TestGetContactOrganisationForLocalClient()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			using (var job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex();
				var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
				cartage.LocalClientPK = header.PK;
				AssertEquals("Local Cartage Contact Org should be Client", header.OH_FullName, cartage.DocumentSupporter.GetContactOrganisation("", ContactType.LocalClient, DocumentDirection.ARV).OrgHeader.FullName);
			}
		}

		public void TestGetContactOrganisationForReceivables()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			using (var job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex();
				var header = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.NotEqual, ""));
				cartage.LocalClientPK = header.PK;
				AssertEquals("Receivables Contact Org should be Client", header.OH_FullName, cartage.DocumentSupporter.GetContactOrganisation("", ContactType.Receivables, DocumentDirection.ARV).OrgHeader.FullName);
			}
		}

		public void TestGetContactOrganisationForCNE()
		{
			var cartage = Factory.New<CommonCartage>();
			var jobAddress1 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "Consignee Name", "Address 1", "2000", "Sydney", "AUSYD", false);
			var jobAddress2 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageCTO, "CTO Name", "Address 1", "2000", "Sydney", "AUSYD", false);
			var jobAddress3 = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "Consignor Name", "Address 1", "2000", "Sydney", "AUSYD", false);
			var contact1a = jobAddress1.Organisation.Contacts.AddNew();
			var contact1b = jobAddress1.Organisation.Contacts.AddNew();
			var contact3 = jobAddress3.Organisation.Contacts.AddNew();
			var consigneeContact = cartage.DocumentSupporter.GetContactOrganisation("CNEMene", ContactType.Consignee, DocumentDirection.ARV);
			AssertEquals("Should find 1st JobDocAddress.", jobAddress1.OrganisationPK, consigneeContact.OrgHeader.PK);
			AssertEquals("First DocAddress Org has 2 contacts.", 2, ((OrgHeader)consigneeContact.OrgHeader).Contacts.Count);
			jobAddress1.DocAddressType = DocAddressType.LocalCartageExporter;
			jobAddress2.DocAddressType = DocAddressType.LocalCartageCTO;
			jobAddress3.DocAddressType = DocAddressType.LocalCartageImporter;
			consigneeContact = cartage.DocumentSupporter.GetContactOrganisation("CNEMene", ContactType.Consignee, DocumentDirection.ARV);
			AssertEquals("Should find 3rd JobDocAddress.", jobAddress3.OrganisationPK, consigneeContact.OrgHeader.PK);
			AssertEquals("Third DocAddress Org has 1 contact.", 1, ((OrgHeader)consigneeContact.OrgHeader).Contacts.Count);
			jobAddress3.OrganisationPK = ZGuid.Empty;
			consigneeContact = cartage.DocumentSupporter.GetContactOrganisation("CNEMene", ContactType.Consignee, DocumentDirection.ARV);
			AssertNull("Consignee DocAddress doesn't have an org, so return null.", consigneeContact);
		}

		public void TestGetDocBusinessObjectsForCombinedCartageAdvice()
		{
			StmMenuItem cartageAdviceMenu = Factory.New<StmMenuItem>();
			cartageAdviceMenu.SU_MenuName = "Combined Cartage Advice something";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			DocumentWrapper[] wrappers = cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CombinedCartageAdvice, cartageAdviceMenu);
			DocumentWrapper cartageWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CommonCartage, Factory.New<CommonCartage>());
			AssertEquals("Wrapper should be of type DocCartage", cartageWrapper.GetType(), wrappers[0].GetType());
		}

		public void TestGetDocumentWrappers_TimeSlotRequest()
		{
			StmMenuItem timeSlotMenu = Factory.New<StmMenuItem>();
			timeSlotMenu.SU_MenuName = "Time Slot something";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.ContainerBookedMoves.AddNew();
			cartage.ContainerBookedMoves.AddNew();
			cartage.ContainerBookedMoves.AddNew();
			cartage.ContainerBookedMoves.AddNew();
			Factory.Save();
			AssertEquals("Should be 4 because if none have changes, then it was run from the menu, so print all", 4, cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TimeSlotRequest, timeSlotMenu).Length);
			cartage.Containers.ElementAt(0).JC_DepartureSlotReference = "111";
			AssertEquals("Should be 1 because only 1 container SlotReference was changed", 1, cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TimeSlotRequest, timeSlotMenu).Length);
			cartage.Containers.ElementAt(1).JC_ArrivalSlotReference = "111";
			AssertEquals("Should be 2 because only 2 containers SlotReference was changed", 2, cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TimeSlotRequest, timeSlotMenu).Length);
			cartage.Containers.ElementAt(2).JC_DepartureSlotDateTime = ZDateTime.Now;
			AssertEquals("Should be 3 because only 3 containers SlotReference was changed", 3, cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TimeSlotRequest, timeSlotMenu).Length);
			cartage.Containers.ElementAt(3).JC_ArrivalSlotDateTime = ZDateTime.Now;
			AssertEquals("Should be 4 because only 4 containers SlotReference was changed", 4, cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TimeSlotRequest, timeSlotMenu).Length);
		}

		public void TestGetDocBusinessObjectsForCartageAdvice()
		{
			StmMenuItem cartageAdviceMenu = Factory.New<StmMenuItem>();
			cartageAdviceMenu.SU_MenuName = "Combined Cartage Advice something";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			DocumentWrapper[] wrappers = cartage.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CartageAdvice, cartageAdviceMenu);
			DocumentWrapper cartageWrapper = DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.CommonCartage, Factory.New<CommonCartage>());
			AssertEquals("Wrapper should be of type DocCartage", cartageWrapper.GetType(), wrappers[0].GetType());
		}

		public void TestGetDocumentWrappers_DeliveryDocketWithReceipt()
		{
			// setup Delivery docket with receipt menu item
			var menuFilter = new DocumentZQuery(StmMenuItemSchema.SU_MenuName, "Delivery Docket with Receipt");
			menuFilter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Cartage);
			var cartageMenu = Factory.LoadTop1<DocumentCommand>(menuFilter);
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var move = cartage.ContainerBookedMoves.AddNew();
			CommonContainer container = move.Container;
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();
			CommonWorkSheet workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.Add(leg1);
			workSheet.CartageLegs.Add(leg2);
			var overridenDataContext = (Constants.DataContext)Enum.Parse(typeof(Constants.DataContext), cartageMenu.Documents[0].DocConfigs[0].S3_OverrideDataContext);
			AssertEquals("DataContext should be overridden.", Constants.DataContext.GenericLocalTransportLeg, overridenDataContext);
			var docSupporter = new CartageDocumentSupporter(cartage);
			var docs = docSupporter.GetDocumentWrappers(overridenDataContext, cartageMenu);
			AssertEquals("Count of documents to be printed should be two.", 2, docs.Length);
			cartage.OnGetCartageLegsToPrint += new EventHandler<DocumentCartageLegEventArgs>(cartage_OnGetCartageLegsToPrint);
			docs = docSupporter.GetDocumentWrappers(overridenDataContext, cartageMenu);
			AssertEquals("Since only One document is selected, count of documents to be printed should be one.", 1, docs.Length);
			cartage.OnGetCartageLegsToPrint -= new EventHandler<DocumentCartageLegEventArgs>(cartage_OnGetCartageLegsToPrint);
		}

		void cartage_OnGetCartageLegsToPrint(object sender, DocumentCartageLegEventArgs e)
		{
			e.ContinueToPrint = true;
			e.DocumentCartageLegOptions.CartageLegs[0].PrintCartageLeg = true;
			e.DocumentCartageLegOptions.CartageLegs[1].PrintCartageLeg = false;
		}

		public void TestIsImport()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals(false, cartage.DocumentSupporter.IsImport);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertEquals(true, cartage.DocumentSupporter.IsImport);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(Env.Security.TransportCustomiseDocuments, cartage.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
