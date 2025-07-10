using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSBuildConsolHelperFreightTest : BaseFreightTest
	{
		public void TestAddBookingToCFSLoadList()
		{
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_IsForwarder = true;
			org2.OH_FullName = "OTHER COMPANY";
			org2.MainAddress.OA_Address1 = "ALEXANDRIA";
			org2.OH_RL_NKClosestPort = ExportSailing.JX_JA_RL_NKPortOfLoading;

			IQuotedBooking quotedBooking = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember("New",
					System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
					null, null, new object[] { Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			CommonShipment shipment = (CommonShipment)quotedBooking.ForwardingShipment;
			shipment.JS_RL_NKDestination = ExportSailing.JX_JB_RL_NKPortOfDischarge;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_GoodsDescription = "BOOKS";
			shipment.ConsignorPK = org2.PK;
			shipment.JS_OA_ExportReceivingDepot = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			shipment.JS_IsCFSRegistered = true;
			CommonContainer container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TRLU5555555";
			Factory.Save();

			AssertEquals("Expecting shipment to be a booking.", true, shipment.JS_IsBooking);
			AssertEquals("Expecting shipment to be cfs.", true, shipment.JS_IsCFSRegistered);
			AssertEquals("Not expecting shipment to be forward registered.", false, shipment.JS_IsForwardRegistered);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			CFSLoadListConsol consol = factory2.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = ExportSailing.PK;
			consol.JK_OH_Forwarder = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			CFSShipment shipment2 = factory2.Load<CFSShipment>(shipment.PK);
			consol.Shipments.Add(shipment2);

			AssertEquals("Expecting shipment to be cfs registered.", true, shipment2.JS_IsCFSRegistered);
			AssertEquals("Expecting shipment to be forward registered.", true, shipment2.JS_IsForwardRegistered);
			AssertEquals("Expecting consol to have 1 container.", 1, consol.Containers.Count);
			AssertEquals("Expecting consol container to be TRLU5555555.", "TRLU5555555", consol.Containers[0].JC_ContainerNum);
		}

		public void TestAddShipmentToCFSLoadList()
		{
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_IsForwarder = true;
			org2.OH_FullName = "OTHER COMPANY";
			org2.MainAddress.OA_Address1 = "ALEXANDRIA";
			org2.OH_RL_NKClosestPort = ExportSailing.JX_JA_RL_NKPortOfLoading;

			IQuotedBooking quotedBooking = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember("New",
					System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
					null, null, new object[] { Enterprise.Freight.Integration.QuoteBookingType.QuickBooking, Factory });

			CommonShipment shipment = (CommonShipment)quotedBooking.ForwardingShipment;
			shipment.JS_RL_NKDestination = ExportSailing.JX_JB_RL_NKPortOfDischarge;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_GoodsDescription = "BOOKS";
			shipment.ConsignorPK = org2.PK;
			shipment.JS_OA_ExportReceivingDepot = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			CommonContainer container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = "TRLU5555555";
			Factory.Save();
			AssertEquals("Expecting shipment to not be cfs.", false, shipment.JS_IsCFSRegistered);

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(shipment, null);
			Factory.Save();
			AssertEquals("Expecting shipment to be a booking.", true, shipment.JS_IsBooking);
			AssertEquals("Expecting shipment to be cfs.", true, shipment.JS_IsCFSRegistered);
			AssertEquals("Not expecting shipment to be forward registered.", true, shipment.JS_IsForwardRegistered);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			CFSLoadListConsol consol = factory2.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = ExportSailing.PK;
			consol.JK_OH_Forwarder = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			CFSShipment shipment2 = factory2.Load<CFSShipment>(shipment.PK);
			consol.Shipments.Add(shipment2);

			AssertEquals("Expecting shipment to be cfs registered.", true, shipment2.JS_IsCFSRegistered);
			AssertEquals("Expecting shipment to be forward registered.", true, shipment2.JS_IsForwardRegistered);
		}

		public void TestAddBookingToCFSLoadList_ShouldNotAddBookingContainer_WhenBookingContainerAlreadyExistsInConsolContainers()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsForwarder = true;
			org.OH_FullName = "OTHER COMPANY";
			org.MainAddress.OA_Address1 = "ALEXANDRIA";
			org.OH_RL_NKClosestPort = ExportSailing.JX_JA_RL_NKPortOfLoading;

			var quotedBooking = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember("New",
					System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
					null, null, new object[] { Integration.QuoteBookingType.QuickBooking, Factory });

			var shipment = (CommonShipment)quotedBooking.ForwardingShipment;
			shipment.JS_RL_NKDestination = ExportSailing.JX_JB_RL_NKPortOfDischarge;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;
			shipment.JS_GoodsDescription = "BOOKS";
			shipment.ConsignorPK = org.PK;
			shipment.JS_OA_ExportReceivingDepot = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			shipment.JS_IsCFSRegistered = true;

			var refCon = Factory.New<RefContainer>();
			refCon.RC_TareWeight = 2000;
			var containerNumber = "IRSU5676476";

			var container = (CommonContainer)quotedBooking.QuotedBookingContainers.AddNew();
			container.JC_ContainerNum = containerNumber;
			container.JC_RC = refCon.PK;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var consol = anotherFactory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = ExportSailing.PK;
			consol.JK_OH_Forwarder = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = containerNumber;
			consolContainer.JC_RC = refCon.PK;

			var reloadedShipment = anotherFactory.Load<CFSShipment>(shipment.PK);
			consol.Shipments.Add(reloadedShipment);

			AssertNoExceptionThrown("Should save without any exception.", () => anotherFactory.Save());
			AssertEquals("Expecting consol to have 1 container.", 1, consol.Containers.Count);
			AssertEquals("Expecting consol existing container to have 1 packing line.", 1, consol.Containers[0].PackLines.Count);
		}

		#region Implementation

		ZGuid OriginalBranchOrg;
		protected override void SetUp()
		{
			OriginalBranchOrg = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = OriginalBranchOrg;
		}

		#endregion
	}
}
