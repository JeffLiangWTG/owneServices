using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.LandTransport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingDocManagerInfo))]
	public sealed class DtbBookingDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var template = Helper.CreateTransportBookingTemplate("IFCL", "FCL Import", Constants.CartageDirection.Import);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CTO, InstructionTypes.Codes.PickUp);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CNE, InstructionTypes.Codes.Delivery);
			Helper.AddInstructionToTemplate(template, OrganisationTypesList.Codes.CYD, InstructionTypes.Codes.Delivery);

			parent = Factory.New<DummyWithDtbBooking>();
			var consolidationBooking = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consolidationBooking);
			var booking2 = Helper.CreateBooking(consolidationBooking);
			var outOfScopeBooking = Helper.CreateBooking();
			booking.KM_KT_NKBookingTemplate = template.KT_Code;

			transportCo = Helper.CreateOrganisation("TRA");
			cto = Helper.CreateOrganisation("CTO");
			cne = Helper.CreateOrganisation("CNE");
			cyd = Helper.CreateOrganisation("CYD");

			booking.Address.E2_OA_Address = transportCo.MainAddress.PK;
			booking.Instructions[0].Address.E2_OA_Address = cto.MainAddress.PK;
			booking.Instructions[1].Address.E2_OA_Address = cne.MainAddress.PK;
			booking.Instructions[2].Address.E2_OA_Address = cyd.MainAddress.PK;

			return booking;
		}

		public void TestGetRelatedeDocs()
		{
			string docType = "MSC", bookingFile = "bookingFile", consignmentFile = "ConsignmentFile", landTransportConsignmentFile = "LandTransportConsignmentFile", actionFile = "ActionFile";

			var booking = Helper.CreateBooking();
			Helper.CreateConsignmentConsol(booking);
			var consignment = booking.ConsignmentConsol.Bookings.AddNew();

			var landTransportConsignment = Factory.New<IDtbConsignment>();
			landTransportConsignment.LTC_KM_Booking = booking.PK;
			var landTransportConsignmentBusinessObject = (BusinessObject)landTransportConsignment;
			landTransportConsignmentBusinessObject.FillWithValidTestData();

			var addressQuery = new ZQuery(DtbConsignmentAddressSchema.LTS_LTC_Consignment, landTransportConsignment.PK);
			var landTransportAddress = Factory.Load<IDtbConsignmentAddress>(addressQuery).First();

			var landTransportAction = Factory.New<IDtbConsignmentAction>();
			landTransportAction.LTA_LTS_ConsignmentAddress = landTransportAddress.PK;
			var landTransportActionBusinessObject = (BusinessObject)landTransportAction;
			landTransportActionBusinessObject.FillWithValidTestData();

			var bookingDocManagerInfo = ((IDocManagerSupport)booking).DocManagerInfo;
			bookingDocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, bookingFile, docType, true);

			var consignmentDocManagerInfo = ((IDocManagerSupport)consignment).DocManagerInfo;
			consignmentDocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, consignmentFile, docType, true);

			var landTransportConsignmentDocManagerInfo = ((IDocManagerSupport)landTransportConsignment).DocManagerInfo;
			landTransportConsignmentDocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, landTransportConsignmentFile, docType, true);

			var landTransportActionDocManagerInfo = ((IDocManagerSupport)landTransportAction).DocManagerInfo;
			landTransportActionDocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, actionFile, docType, true);

			Factory.Save();

			var relatedObjects = new DtbBookingDocManagerInfo(booking).RelatedObjects;

			AssertContainsExactElementsInAnyOrder(new[] { landTransportConsignmentBusinessObject, landTransportActionBusinessObject }, relatedObjects);

			var relatedLandTransportConsignment = relatedObjects.OfType<IDtbConsignment>().First();
			var relatedLandTransportAction = relatedObjects.OfType<IDtbConsignmentAction>().First();
			AssertDocumentTypeAndFileName(((IDocManagerSupport)booking).DocManagerInfo, 1, docType, bookingFile);
			AssertDocumentTypeAndFileName(((IDocManagerSupport)relatedLandTransportConsignment).DocManagerInfo, 1, docType, landTransportConsignmentFile);
			AssertDocumentTypeAndFileName(((IDocManagerSupport)relatedLandTransportAction).DocManagerInfo, 1, docType, actionFile);
		}

		public void TestGetRelatedeDocsForMaster()
		{
			var docType = "MSC";

			var masterBooking = Helper.CreateBooking();
			var subBooking1 = Helper.CreateBooking();
			var subBooking2 = Helper.CreateBooking();
			var subBooking3 = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking3.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			var subBooking1DocManagerInfo = ((IDocManagerSupport)subBooking1).DocManagerInfo;
			subBooking1DocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "Sub1", docType, true);

			var subBooking2DocManagerInfo = ((IDocManagerSupport)subBooking2).DocManagerInfo;
			subBooking2DocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "Sub2", docType, true);

			var subBooking3DocManagerInfo = ((IDocManagerSupport)subBooking3).DocManagerInfo;
			subBooking3DocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "Sub3", docType, true);

			Factory.Save();

			var relatedObjects = new DtbBookingDocManagerInfo(masterBooking).RelatedObjects;

			AssertEquals("Master booking should have its subs as related objects", 3, relatedObjects.Length);

			AssertContainsExactElementsInAnyOrder(new[] { subBooking1, subBooking2, subBooking3 }, relatedObjects);

			var relatedObject1DocManagerInfo = ((IDocManagerSupport)relatedObjects[0]).DocManagerInfo;
			var relatedObject2DocManagerInfo = ((IDocManagerSupport)relatedObjects[1]).DocManagerInfo;
			var relatedObject3DocManagerInfo = ((IDocManagerSupport)relatedObjects[2]).DocManagerInfo;

			CombineAssertions(() =>
			{
				AssertDocumentTypeAndFileName(relatedObject1DocManagerInfo, 1, docType, "Sub1");
				AssertDocumentTypeAndFileName(relatedObject2DocManagerInfo, 1, docType, "Sub2");
				AssertDocumentTypeAndFileName(relatedObject3DocManagerInfo, 1, docType, "Sub3");
			});
		}

		public void TestGetRelatedeDocsForSub()
		{
			var docType = "MSC";

			var masterBooking = Helper.CreateBooking();
			var subBooking = Helper.CreateBooking();

			masterBooking.KM_IsMaster = true;
			subBooking.KM_KM_MasterBooking = masterBooking.PK;

			Factory.Save();

			var masterBookingDocManagerInfo = ((IDocManagerSupport)masterBooking).DocManagerInfo;
			masterBookingDocManagerInfo.AddFileOrDocument(new byte[] { 0x1, 0x2 }, "Master1", docType, true);

			Factory.Save();

			var relatedObjects = new DtbBookingDocManagerInfo(subBooking).RelatedObjects;

			AssertEquals("Sub booking should have its master booking as a related object", 1, relatedObjects.Length);

			AssertContainsExactElementsInAnyOrder(new[] { masterBooking }, relatedObjects);

			var relatedMaster = relatedObjects.OfType<DtbBooking>().First();

			var relatedMasterDocManagerInfo = ((IDocManagerSupport)relatedMaster).DocManagerInfo;

			AssertDocumentTypeAndFileName(relatedMasterDocManagerInfo, 1, docType, "Master1");
		}

		void AssertDocumentTypeAndFileName(DocManagerInfo docManagerInfo, int totalDoc, string docType, string fileName)
		{
			AssertEquals("Total file count should be: " + totalDoc.ToString(), totalDoc, docManagerInfo.AllEDocs.Count);
			AssertEquals("DocType should be: " + docType, docType, docManagerInfo.AllEDocs[0].DocType);
			AssertEquals("FileName should be: " + fileName, fileName, docManagerInfo.AllEDocs[0].FileName);
		}

		public void TestGetRelatedObjectsShouldNotThrow()
		{
			var booking = Helper.CreateBooking();
			Helper.CreateConsignmentConsol(booking);
			booking.ConsignmentConsol.Bookings.AddNew();

			var bookingDocManagerInfo = new DtbBookingDocManagerInfo(booking);
			AssertNoExceptionThrown("Should not throw an unable to cast object Exception", () => _ = bookingDocManagerInfo.RelatedObjects);
		}

		public void TestGetRelatedObjectsShouldNotReportError_WhenParentWithWorkflowIsNull()
		{
			var cfsShipment = (BusinessObject)Factory.New<ICFSShipment>();
			var consolidationBooking = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidationBooking);
			consolidationBooking.KB_ParentID = cfsShipment.PK;
			consolidationBooking.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			AssertEquals("Precondition: booking.ConsolidationSingleJob.Parent.ParentWithWorkflow should be null, as CFSShipment does not inherit from IDtbBookingParent", null, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow);
			AssertEquals("Precondition: Should not have any errors", 0, ErrorReporter.TotalErrorCount);

			var bookingDocManagerInfo = new DtbBookingDocManagerInfo(booking);

			var relatedObjects = bookingDocManagerInfo.RelatedObjects;
			AssertEquals("relatedObjects should be empty", 0, relatedObjects.Length);
			AssertEquals("Should not report any errors", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestGetRelatedObjects()
		{
			var booking = (DtbBooking)GetPopulatedParentBusinessObject();

			var portTransport2 = Factory.New<ICommonCartage>();
			portTransport2.JJ_ParentID = booking.PK;
			portTransport2.JJ_ParentTableCode = booking.TablePrefix;
			portTransport2.JJ_ConsignmentID = "2";

			var portTransport1 = Factory.New<ICommonCartage>();
			portTransport1.JJ_ParentID = booking.PK;
			portTransport1.JJ_ParentTableCode = booking.TablePrefix;
			portTransport1.JJ_ConsignmentID = "1";

			var bookingDocManagerInfo = new DtbBookingDocManagerInfo(booking);
			var relatedObjects = bookingDocManagerInfo.RelatedObjects;
			AssertContainsExactElementsInAnyOrder("RelatedObjects should get return the correct objects", (b) => b.HumanReadableName, new[] { parent, transportCo, cto, cne, cyd, (BusinessObject)portTransport1, (BusinessObject)portTransport2 }, bookingDocManagerInfo.RelatedObjects);

			var relatedPortTransports = bookingDocManagerInfo.RelatedObjects.OfType<ICommonCartage>().ToArray();
			AssertEquals("Should have ordered PortTransports by ConsignmentID.", portTransport1, relatedPortTransports[0]);
			AssertEquals("Should have ordered PortTransports by ConsignmentID.", portTransport2, relatedPortTransports[1]);
		}

		public void TestReadOnly()
		{
			var booking = (DtbBooking)GetPopulatedParentBusinessObject();
			var bookingDocManagerInfo = new DtbBookingDocManagerInfo(booking);

			AssertEquals("ReadOnly should always false.", false, bookingDocManagerInfo.ReadOnly);

			booking.ConsolidationSingleJob.KB_IsOverridden = false;
			AssertEquals(true, booking.ReadOnly);
			AssertEquals("ReadOnly should always false.", false, bookingDocManagerInfo.ReadOnly);

			booking.ConsolidationSingleJob.KB_IsOverridden = true;
			AssertEquals(false, booking.ReadOnly);
			AssertEquals("ReadOnly should always false.", false, bookingDocManagerInfo.ReadOnly);
		}

		DummyWithDtbBooking parent;
		OrgHeader transportCo;
		OrgHeader cto;
		OrgHeader cne;
		OrgHeader cyd;

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
			base.TearDown();
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<DtbBooking>();
		}
	}
}
