using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolDocManagerInfo))]
	sealed class FreightConsolDocManagerInfoTest : ConsolDocManagerInfoTest
	{
		public void TestRelatedBusinessOrderModule()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER_1";
			buyer.Addresses.AddNew().FillWithValidTestData();
			buyer.Addresses.AddNew().FillWithValidTestData();
			buyer.Contacts.AddNew().FillWithValidTestData();
			buyer.Contacts.AddNew().FillWithValidTestData();
			order.JD_OA_BuyerAddress = buyer.Addresses[1].PK;
			order.JD_OC_BuyerContact = buyer.Contacts[1].PK;
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_ItemPrice = 2;

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.Addresses.AddNew().FillWithValidTestData();
			supplier.Addresses.AddNew().FillWithValidTestData();
			supplier.Contacts.AddNew().FillWithValidTestData();
			supplier.Contacts.AddNew().FillWithValidTestData();

			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_TransportMode = Core.Constants.TransportModes.Sea;
			booking.JSB_BookingId = "JSB00001";
			booking.JSB_LoadMode = "CY";
			booking.JSB_RL_NKLoadPort = "AUSYD";
			booking.JSB_RL_NKDischargePort = "CNCAN";
			booking.JSB_OH_BookingParty = bookingParty.PK;
			booking.JSB_BookedOnDate = ZDate.Today;
			booking.JSB_Status = "PLC";
			booking.JSB_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			booking.SupplierAddress.OrganisationPK = supplier.PK;
			booking.SupplierAddress.E2_OA_Address = supplier.Addresses[1].PK;
			booking.SupplierAddress.ContactPK = supplier.Contacts[1].PK;

			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.FillWithValidTestData();
			bookingLine.JSL_JSB_Booking = booking.PK;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			bookingLine.JSL_BookedQuantity = 6;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var etd = ZDateTime.Today;
			var eta = ZDateTime.Today.AddDays(2);

			var transport = consol.Transports[0];
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;

			var container1 = consol.Containers.AddNew();
			container1.FillWithValidTestData();
			container1.JC_JSB_SupplierBooking = booking.PK;

			var loadListHeader = Factory.NewWithValidTestData<CYContainerLoadList>();
			loadListHeader.CLH_JSB_Booking = booking.PK;
			loadListHeader.CLH_LoadListId = "CLL00001";
			loadListHeader.CLH_OH_LoadListParty = bookingParty.PK;
			loadListHeader.CLH_Status = "SHP";

			var loadListLine = loadListHeader.LoadListLines.AddNew();
			loadListLine.CLL_JSL_BookingLine = bookingLine.PK;
			loadListLine.CLL_JC_Container = container1.PK;

			loadListLine.CLL_Volume = 2.1;
			loadListLine.CLL_VolumeUnit = "M3";
			loadListLine.CLL_Weight = 4.3;
			loadListLine.CLL_WeightUnit = "KG";
			loadListLine.CLL_LoadSequence = 5;
			loadListLine.CLL_Packages = 3;
			loadListLine.CLL_F3_NKPackagesUnit = "PKG";
			loadListLine.CLL_RH_NKCommodityCode = "GEN";
			loadListLine.CLL_HarmonizedCode = "HAR";
			loadListLine.CLL_ReferenceNumber = "RN0001";
			loadListLine.CLL_PackedQuantity = 6;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = booking.JSB_TransportMode;
			shipment.JS_PackingMode = booking.JSB_TransportMode == Core.Constants.TransportModes.Air ? Core.Constants.ContainerModes.ULD : Core.Constants.ContainerModes.FCL;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = booking.SupplierAddress.OrganisationPK;
			shipment.ConsignorDocumentaryAddress.ContactPK = booking.SupplierAddress.ContactPK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = booking.SupplierAddress.E2_OA_Address;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = order.Buyer.PK;
			shipment.ConsigneeDocumentaryAddress.ContactPK = order.JD_OC_BuyerContact;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = order.JD_OA_BuyerAddress;
			shipment.JS_RL_NKOrigin = booking.JSB_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = booking.JSB_RL_NKDischargePort;
			shipment.JS_E_DEP = consol.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_ETD ?? ZDateTime.Empty;
			shipment.JS_E_ARV = consol.MostInterestingTransportForBinding.Cast<Transport>().FirstOrDefault()?.JW_ETA ?? ZDateTime.Empty;
			shipment.JS_INCO = booking.JSB_IncoTerm;

			var packLine = shipment.OuterPackLines.AddNew();
			loadListLine.CLL_JL_PackLine = packLine.PK;
			packLine.JL_JC = loadListLine.CLL_JC_Container;
			packLine.JL_ActualVolume = loadListLine.CLL_Volume;
			packLine.JL_ActualVolumeUQ = loadListLine.CLL_VolumeUnit;
			packLine.JL_ActualWeight = loadListLine.CLL_Weight;
			packLine.JL_ActualWeightUQ = loadListLine.CLL_WeightUnit;
			packLine.JL_ContainerPackingOrder = loadListLine.CLL_LoadSequence;
			packLine.JL_PackageCount = loadListLine.CLL_Packages;
			packLine.JL_F3_NKPackType = loadListLine.CLL_F3_NKPackagesUnit;
			packLine.JL_RH_NKCommodityCode = loadListLine.CLL_RH_NKCommodityCode;
			packLine.JL_HarmonisedCode = loadListLine.CLL_HarmonizedCode;
			packLine.JL_RefNumber = loadListLine.CLL_ReferenceNumber;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { container1, loadListHeader, shipment, order, booking }, ((IDocManagerSupport)consol).DocManagerInfo.RelatedObjects);
		}

		public void TestRelatedObjectsForCAeManifest()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
			var caeManifest = Factory.New<Enterprise.Integration.Customs.CA.ICusCAeMHMaster>();
			Assert(!relatedObjects.Contains((BusinessObject)caeManifest));
			caeManifest.BP_ParentID = consol.PK;
			caeManifest.BP_ParentTableCode = consol.TablePrefix;
			relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
			Assert(!relatedObjects.Contains((BusinessObject)caeManifest));

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CABLO";
			relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
			Assert(relatedObjects.Contains((BusinessObject)caeManifest));
			consol.JK_RL_NKDischargePort = "CNBJS";
			relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
			Assert(!relatedObjects.Contains((BusinessObject)caeManifest));
		}

		public new void TestAllRelatedObjectsRetrieved()
		{
			BusinessObject[] relatedObjects = new ForwardingConsolDocManagerInfo((ForwardingConsol)GetPopulatedParentBusinessObject()).RelatedObjects;
			AssertContainsExactElementsInAnyOrder("Expected related objects", new BusinessObject[] { Shipment1, Shipment2, Container1, Container2, Invoice }, relatedObjects);
		}

		public void TestAllRelatedObjectsRetrievedForGbAirConsolWithCcsukAwbs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment1 = consol.Shipments.AddNew();
				var shipment2 = consol.Shipments.AddNew();
				var mawbType = ObjectFactory.GetType<Enterprise.Integration.Customs.GB.CCSUK.ICusMAWB>();
				var mawb = Factory.New(mawbType);
				mawb[CusMAWBSchema.CM_JK] = consol.PK;
				var hawb1 = Factory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusHAWB>();
				hawb1.CS_CM = mawb.PK;
				hawb1.CS_JS = shipment1.PK;
				var hawb2 = Factory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusHAWB>();
				hawb2.CS_CM = mawb.PK;
				hawb2.CS_JS = shipment2.PK;
				var ediMessageShouldNotBeFoundInRelatedObjects = EDIMessageTestFactory.New(Factory);
				var hawb1BO = hawb1 as BusinessObject;
				ediMessageShouldNotBeFoundInRelatedObjects.EM_LinkedObject = hawb1BO;
				var relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
				var hawb2BO = hawb2 as BusinessObject;
				AssertContainsExactElementsInAnyOrder("Expected related objects. If you see that the two HAWBs are missing then check that you're not initialising the mawb's CHildBills collection too early. e.g. check that nothing in the instantiation of the mawb touches ChildBills, as this is too early (the CS_CMs have not yet been set)",
														new BusinessObject[] { shipment1, shipment2, hawb1BO, hawb2BO, mawb }, relatedObjects);
			}
		}

		public void TestRelatedObjectsForEuNcts()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var nctsHeader = Factory.New<Enterprise.Integration.Customs.EU.NCTS.ICusInBondHeader>();
			nctsHeader.BH_ParentID = consol.PK;
			nctsHeader.BH_ParentTableCode = consol.TablePrefix;
			var relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
			Assert(relatedObjects.Contains((BusinessObject)nctsHeader));
		}

		public void TestRelatedObjectsForAsycudaManifestHeader()
		{
			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var manifestHeader = Factory.New<Enterprise.Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
				manifestHeader.AMA_ParentId = consol.PK;
				manifestHeader.AMA_ParentTableCode = consol.TablePrefix;
				var relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
				Assert(relatedObjects.Contains((BusinessObject)manifestHeader));
			}
		}

		public void TestShouldRecordDocumentCore()
		{
			StmMenuItem testMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			testMenuItem.SU_MenuName = Core.Constants.MenuNameConstantsForPrinting.ConsolJobProfitDocument;
			ForwardingConsol consol = (ForwardingConsol)GetPopulatedParentBusinessObject();
			Assert("Shouldn't record the document", !consol.DocManagerInfo.ShouldRecordDocument(testMenuItem));
			Assert("Should record the document", consol.DocManagerInfo.ShouldRecordDocument(Factory.NewWithValidTestData<StmMenuItem>()));
			AssertNoExceptionThrown(() => consol.DocManagerInfo.ShouldRecordDocument(null));
		}

		public void TestAllRelatedObjectsRetrievedForCusExitDetail()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();

			var exitHeader = Factory.New<Enterprise.Integration.Customs.EU.ICusExitControlHeader>();
			exitHeader.CEH_ParentID = consol.PK;
			exitHeader.CEH_ParentTableCode = consol.TablePrefix;
			exitHeader.CEH_ReferenceNumber = "ref";
			var exitDetail = Factory.New<Enterprise.Integration.Customs.EU.ICusExitDetail>();
			exitDetail.CED_CEH = exitHeader.PK;
			Factory.Save();

			var relatedObjects = new ForwardingConsolDocManagerInfo(consol).RelatedObjects;
			AssertCollectionContains(exitDetail, relatedObjects);
		}

		public void TestAllRelatedObjectsRetrievedForCusExitHeader()
		{
			var consol = Factory.New<ForwardingConsol>();

			var exitHeader = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitHeader>();
			exitHeader.CXH_ParentID = consol.PK;
			exitHeader.CXH_ParentTableCode = consol.TablePrefix;
			((BusinessObject)exitHeader).FillWithValidTestData();

			var exitConsignment = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitConsignment>();
			exitConsignment.CXC_CXH_Header = exitHeader.PK;
			((BusinessObject)exitConsignment).FillWithValidTestData();

			var exitReport = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitReport>();
			exitReport.CER_CXH_Header = exitHeader.PK;
			((BusinessObject)exitReport).FillWithValidTestData();

			Factory.Save();

			var relatedObjects = consol.DocManagerInfo.RelatedObjects;
			AssertCollectionContains(exitHeader, relatedObjects);
			AssertCollectionContains(exitReport, relatedObjects);
			AssertCollectionContains(exitConsignment, relatedObjects);
		}

		public void TestAllRelatedObjectsRetrievedForCusExitHeader_MultipleUnsavedCusExitHeader_NoException()
		{
			var consol = Factory.New<ForwardingConsol>();

			var exitHeader = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitHeader>();
			exitHeader.CXH_ParentID = consol.PK;
			exitHeader.CXH_ParentTableCode = consol.TablePrefix;
			((BusinessObject)exitHeader).FillWithValidTestData();

			var exitHeader2 = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitHeader>();
			exitHeader2.CXH_ParentID = consol.PK;
			exitHeader2.CXH_ParentTableCode = consol.TablePrefix;
			((BusinessObject)exitHeader).FillWithValidTestData();

			var exitConsignment = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitConsignment>();
			exitConsignment.CXC_CXH_Header = exitHeader.PK;
			((BusinessObject)exitConsignment).FillWithValidTestData();

			var exitReport = Factory.New<Enterprise.Integration.Customs.EUExitControl.ICusExitReport>();
			exitReport.CER_CXH_Header = exitHeader.PK;
			((BusinessObject)exitReport).FillWithValidTestData();

			AssertNoExceptionThrown(() => _ = consol.DocManagerInfo.RelatedObjects);
		}

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<ForwardingConsol>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Shipment1 = consol.Shipments.AddNew();
			Shipment2 = consol.Shipments.AddNew();

			Container1 = consol.Containers.AddNew();
			Container2 = consol.Containers.AddNew();

			ZString consolID = "C00000001";
			consol.JK_UniqueConsignRef = consolID;

			Debtor = Factory.New<OrgHeader>();
			Debtor.OH_IsDebtor = ZBool.True;
			Debtor.OH_Code = "DEBTOR";

			Invoice = Factory.New<AccTransactionHeader>();
			Invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			Invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			Invoice.AH_OH = Debtor.PK;
			Invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			Invoice.AH_ConsolidatedInvoiceRef = consolID;

			NonConsolInvoice = Factory.New<AccTransactionHeader>();
			NonConsolInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			NonConsolInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			NonConsolInvoice.AH_OH = Debtor.PK;
			NonConsolInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			PayablesInvoice = Factory.New<AccTransactionHeader>();
			PayablesInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			PayablesInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			PayablesInvoice.AH_OH = Debtor.PK;

			return consol;
		}

		OrgHeader Debtor;
		CommonShipment Shipment1;
		CommonShipment Shipment2;
		CommonContainer Container1;
		CommonContainer Container2;
		AccTransactionHeader Invoice;
		AccTransactionHeader NonConsolInvoice;
		AccTransactionHeader PayablesInvoice;

		#endregion
	}
}
