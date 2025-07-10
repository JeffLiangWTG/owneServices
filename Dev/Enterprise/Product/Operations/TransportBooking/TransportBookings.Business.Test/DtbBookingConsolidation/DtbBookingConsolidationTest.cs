using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration.Packing;
using Enterprise.Integration.Schedule;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingConsolidation))]
	public class DtbBookingConsolidationBizOTest : DtbTransportBusinessObjectTestCase
	{
		public void TestFindExistingTransportBookingConsolidation()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";

			var bookingParent = Factory.New<IForwardingShipment>();

			dummy.BookingParentPK = bookingParent.PK;
			dummy.BookingParentTablePrefix = "JS";

			var dummyConsol = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			dummyConsol.KB_ParentID = dummy.PK;
			dummyConsol.KB_ParentTableCode = dummy.TablePrefix;
			dummyConsol.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			var bookingParentConsol = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			bookingParentConsol.KB_ParentID = bookingParent.PK;
			bookingParentConsol.KB_ParentTableCode = "JS";
			bookingParentConsol.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			Factory.Save();

			var foundConsolidation = DtbBookingConsolidation.FindExistingTransportBookingConsolidation(Factory, dummy, DtbBookingDirection.DLV);

			AssertEquals(bookingParentConsol.PK, foundConsolidation.PK);
		}

		public void TestBookingsAreSetToReadOnlyIfConsolidationIsReadOnly()
		{
			var consolidation1 = Helper.CreateConsolidation();
			var booking1 = Factory.New<DtbBooking>();
			booking1.KM_KB_Booking = consolidation1.PK;
			consolidation1.ReadOnly = true;
			AssertEquals("Precondition", false, booking1.ReadOnly);

			consolidation1.OnConsolidationIsOverrideChanged();
			AssertEquals("Should make Child Bookings Read Only if Consolidation is Read Only.", true, booking1.ReadOnly);

			var consolidation2 = Helper.CreateConsolidation();
			var booking2 = Factory.New<DtbBooking>();
			booking2.KM_KB_Booking = consolidation2.PK;
			booking2.ReadOnly = true;
			consolidation2.OnConsolidationIsOverrideChanged();
			AssertEquals(false, consolidation2.ReadOnly);
			AssertEquals("Will override a booking.readonly = true ... if the booking truly needs to be readonly it should be included in the readonly getter.", false, booking2.ReadOnly);
		}

		public void TestPackageJob()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();

			AssertNotNull(consolidation.PackageJob);
			AssertEquals(consolidation.PackageJob, consolidation.PackageJob);
		}

		public void TestTransportBookingPartyReference()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();

			AssertEquals(false, consolidation.TransportBookingPartyReferences.Any());

			var booking1 = consolidation.Bookings.AddNew();
			var booking2 = consolidation.Bookings.AddNew();
			var entryNumX = booking1.AdditionalReferenceNumbers.AddNew();
			entryNumX.CE_EntryType = "XXX";
			entryNumX.CE_EntryNum = "123";
			var entryNumTB1 = booking1.AdditionalReferenceNumbers.AddNew();
			entryNumTB1.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			entryNumTB1.CE_EntryNum = "456";
			var entryNumTB2 = booking2.AdditionalReferenceNumbers.AddNew();
			entryNumTB2.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			entryNumTB2.CE_EntryNum = "789";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "456", "789" }, consolidation.TransportBookingPartyReferences);
		}

		public void TestJobNumber()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();

			Factory.Save(); // to save KB_JobID
			AssertEquals(consolidation.KB_JobID, consolidation.JobNumber);
			AssertEquals("Transport Booking Consolidation " + consolidation.KB_JobID, consolidation.HumanReadableName);

			var booking = consolidation.Bookings.AddNew();
			Factory.Save(); // to save KM_JobID
			AssertEquals("JobNumber should be Booking Job Number when only 1.", booking.KM_JobID, consolidation.JobNumber);
			AssertEquals("Human Readable should be Booking Job Number when only 1.", "Transport Booking Consolidation " + consolidation.KB_JobID, consolidation.HumanReadableName);

			var booking2 = consolidation.Bookings.AddNew();
			Factory.Save(); // to save KM_JobID
			AssertEquals("JobNumber should be Consolidation Job Number when more than 1.", consolidation.KB_JobID, consolidation.JobNumber);
			AssertEquals("Human Readable should be Consolidation Job Number when more than 1.", "Transport Booking Consolidation " + consolidation.KB_JobID, consolidation.HumanReadableName);

			var multiConsolidation = (DtbBookingConsolidation)GetNewBusinessObject();

			multiConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			Factory.Save(); // to save KB_JobID
			AssertEquals(multiConsolidation.KB_JobID, multiConsolidation.JobNumber);
			AssertEquals("Transport Booking Consolidation " + multiConsolidation.KB_JobID, multiConsolidation.HumanReadableName);

			booking.KM_KB_BookingConsolidationMultiJob = multiConsolidation.PK;
			Factory.Save(); // to save KM_JobID
			AssertEquals("Multi Consol to always use it's own number.", multiConsolidation.KB_JobID, multiConsolidation.JobNumber);
			AssertEquals("Multi Consol to always use it's own number.", "Transport Booking Consolidation " + multiConsolidation.KB_JobID, multiConsolidation.HumanReadableName);
		}

		public void TestCarrierServiceLevelCode()
		{
			var carrierServiceLevel = "STD";
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var booking = consolidation.Bookings.AddNew();
			var deliveryInstruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var package = consolidation.PackageJob.Packages.AddNew("BOX");
			var divot = Helper.CreatePackageDivot(deliveryInstruction, package, 1);

			booking.KM_PL_NKCarrierServiceLevel = carrierServiceLevel;
			IPackingParent packingParent = consolidation;

			AssertEquals(carrierServiceLevel, packingParent.CarrierServiceLevelCode(package));

			consolidation.Bookings.AddNew();
			AssertEquals(carrierServiceLevel, packingParent.CarrierServiceLevelCode(package));
		}

		public void TestGetCarrier()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = booking1.Instructions.AddNew();
			var packageDivot1 = instruction1.PackageDivots.AddNew();
			var package1 = Factory.New<PkgPackage>();

			packageDivot1.KD_KP_Package = package1.PK;
			IPackingParent packingParent = consolidation;

			var transportCo1 = Helper.CreateOrganisation("ABC");
			booking1.Address.OrganisationPK = transportCo1.PK;

			AssertEquals(transportCo1, packingParent.GetCarrier(package1));

			var transportCo2 = Helper.CreateOrganisation("EFG");
			var booking2 = consolidation.Bookings.AddNew();
			booking2.Address.OrganisationPK = transportCo2.PK;
			var instruction2 = booking2.Instructions.AddNew();
			var packageDivot2 = instruction2.PackageDivots.AddNew();
			var package2 = Factory.New<PkgPackage>();
			packageDivot2.KD_KP_Package = package2.PK;
			AssertEquals(transportCo2, packingParent.GetCarrier(package2));
		}

		protected override Type ExpectedLookupsType
		{
			get { return typeof(DtbBookingConsolidationLookups); }
		}

		protected override Type ExpectedValidationType
		{
			get { return typeof(DtbBookingConsolidationValidation); }
		}

		public void TestIPackingParent()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			IPackingParent packingParent = consolidation;

			Factory.Save(); // to save KB_JobID

			AssertEquals(consolidation.Factory, packingParent.Factory);
			AssertEquals(consolidation.PK, packingParent.PK);
			AssertEquals(consolidation.TablePrefix, packingParent.TablePrefix);
			AssertEquals(ControllerIDs.DtbBookingConsolidation, packingParent.ControllerID);
			AssertEquals("Bookings for Standalone Packing", packingParent.JobDescription);
			AssertEquals(consolidation.KB_JobID, packingParent.JobNo);
			AssertEquals(ZString.Empty, packingParent.ConnoteNo);
			AssertEquals(DocumentOptions.None, packingParent.DocumentOptions);
			AssertEquals(false, packingParent.IsAutoPrintAllowed);
			AssertEquals(false, packingParent.IsParentJobFinalised);
			AssertEquals(false, packingParent.IsScanEventsVisible);
			AssertNull(packingParent.CarrierBookingAgent);

			// booking with parent Dummy
			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidationForDummy = Helper.CreateConsolidation(dummy);
			var consolidationForDummyPackingParent = (IPackingParent)consolidationForDummy;

			dummy.Z0_Description = "D00000123";
			AssertEquals("Bookings for Dummy", consolidationForDummyPackingParent.JobDescription);
			AssertEquals("D00000123", consolidationForDummyPackingParent.JobNo);
		}

		public void TestIPackingParent_JobNo()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			IPackingParent packingParent = consolidation;

			Factory.Save(); // to save KB_JobID
			AssertEquals(consolidation.KB_JobID, packingParent.JobNo);

			var booking = consolidation.Bookings.AddNew();
			Factory.Save(); // to save KM_JobID
			AssertEquals("JobNo should be Booking Job Number when only 1.", booking.KM_JobID, packingParent.JobNo);

			var booking2 = consolidation.Bookings.AddNew();
			Factory.Save(); // to save KM_JobID
			AssertEquals("JobNo should be Consolidation Job Number when more than 1.", consolidation.KB_JobID, packingParent.JobNo);

			// booking with parent Dummy
			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidationForDummy = Helper.CreateConsolidation(dummy);
			var consolidationForDummyPackingParent = (IPackingParent)consolidationForDummy;

			dummy.Z0_Description = "D00000123";
			AssertEquals("D00000123", consolidationForDummyPackingParent.JobNo);
		}

		public void TestIPackingParentCustomDescription()
		{
			var consolidation = Helper.CreateConsolidation();
			IPackingParent packingParent = consolidation;
			Factory.Save(); // to save KB_JobID

			var consolidationCustomDescription = (IPackingParentCustomDescription)consolidation;
			AssertEquals("Bookings for Standalone Packing", packingParent.JobDescription);
			AssertEquals(consolidation.KB_JobID, packingParent.JobNo);
			AssertEquals("Packing Job for Multiple Bookings", consolidationCustomDescription.FullJobDescription);

			var booking = consolidation.Bookings.AddNew();
			Factory.Save(); // to save KB_JobID
			AssertEquals("Bookings for Standalone Packing", packingParent.JobDescription);
			AssertEquals(booking.KM_JobID, packingParent.JobNo);
			AssertEquals("Packing Job for Transport Booking TB00000001", consolidationCustomDescription.FullJobDescription);

			consolidation.Bookings.AddNew();
			AssertEquals("Bookings for Standalone Packing", packingParent.JobDescription);
			AssertEquals(consolidation.KB_JobID, packingParent.JobNo);
			AssertEquals("Packing Job for Multiple Bookings", consolidationCustomDescription.FullJobDescription);

			// booking with parent Dummy
			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidationForDummy = Helper.CreateConsolidation(dummy);
			var consolidationForDummyPackingParent = (IPackingParent)consolidationForDummy;
			var consolidationForDummyCustomDescription = (IPackingParentCustomDescription)consolidationForDummy;

			dummy.Z0_Description = "Parent456";
			AssertEquals("Bookings for Dummy", consolidationForDummyPackingParent.JobDescription);
			AssertEquals("Parent456", consolidationForDummyPackingParent.JobNo);
			AssertEquals("Bookings for Dummy Parent456", consolidationForDummyCustomDescription.FullJobDescription);

			consolidationForDummy.Bookings.AddNew();
			consolidationForDummy.Bookings.AddNew();
			AssertEquals("No Change. Still show parent ID.", "Bookings for Dummy", consolidationForDummyPackingParent.JobDescription);
			AssertEquals("No Change. Still show parent ID.", "Parent456", consolidationForDummyPackingParent.JobNo);
			AssertEquals("No Change. Still show parent ID.", "Bookings for Dummy Parent456", consolidationForDummyCustomDescription.FullJobDescription);
		}

		public void TestIPackingParent_CanDeletePackage()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			IPackingParent packingParent = consolidation;

			var container1 = consolidation.PackageJob.Packages.AddNew("CNT");
			container1.KP_PackageID = "123";
			AssertEquals(true, packingParent.GetPackageActionStrategy(container1).IsActionAllowed(PackageAction.Delete));

			// attempt delete of package on an instruction
			instruction.DivotsWithPackages.AddPackage(container1);
			var args1 = packingParent.GetPackageActionStrategy(container1);
			AssertEquals(false, args1.IsActionAllowed(PackageAction.Delete));
			AssertEquals(
				"\r\n" +
				"Package '1x Container 123' is assigned to one or more Instructions and cannot be deleted.\r\n" +
				"\r\n" +
				"If you want to delete this package, first un-assign it from all instructions.",
				args1.ReasonForNotAllowingAction);

			// attempt delete of package that has a child package that is on an instruction
			var container2 = consolidation.PackageJob.Packages.AddNew("CNT");
			var pallet2 = container2.Packages.AddNew("PLT");
			container2.KP_F3_NKPackType = "CNT";
			container2.KP_PackageID = "456";
			instruction.DivotsWithPackages.AddPackage(pallet2);

			var args2 = packingParent.GetPackageActionStrategy(container2);
			AssertEquals(false, args2.IsActionAllowed(PackageAction.Delete));
			AssertEquals(
				"\r\n" +
				"Package '1x Container 456' has child packages that are assigned to one or more Instructions and cannot be deleted.\r\n" +
				"\r\n" +
				"If you want to delete this package, first un-assign child packages from all instructions.",
				args2.ReasonForNotAllowingAction);

			// attempt delete of package that has a child/child package that is on an instruction (tests recursion)
			var container3 = consolidation.PackageJob.Packages.AddNew("CNT");
			var pallet3 = container3.Packages.AddNew("PLT");
			var box3 = pallet3.Packages.AddNew("BOX");
			container3.KP_F3_NKPackType = "CNT";
			container3.KP_PackageID = "789";
			instruction.DivotsWithPackages.AddPackage(box3);

			var args3 = packingParent.GetPackageActionStrategy(container3);
			AssertEquals(false, args3.IsActionAllowed(PackageAction.Delete));
			AssertEquals(
				"\r\n" +
				"Package '1x Container 789' has child packages that are assigned to one or more Instructions and cannot be deleted.\r\n" +
				"\r\n" +
				"If you want to delete this package, first un-assign child packages from all instructions.",
				args3.ReasonForNotAllowingAction);

			// remove packages from instructions and ensure delete is ok
			instruction.PackageDivots.DeleteAll();
			var args4 = packingParent.GetPackageActionStrategy(container3);
			AssertEquals(true, args4.IsActionAllowed(PackageAction.Delete));
		}

		public void TestIPackingParent_DeletePackage_WithInnerPackage()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			IPackingParent packingParent = consolidation;

			var container1 = consolidation.PackageJob.Packages.AddNew("CNT");
			var childPackage = container1.Packages.AddNew();
			instruction.DivotsWithPackages.AddPackage(childPackage);

			consolidation.PackageJob.PackageDeleteCanceled += (object sender, PackageCanceledActionEventArgs args) =>
			{
				AssertEquals("Reason for not allowing action cannot be empty.", false, args.ReasonForNotAllowingAction.IsEmpty);
			};

			// attempt delete of parent package with child package on an instruction
			AssertNoExceptionThrown("There should be no exceptions thrown", container1.Delete);
			AssertEquals("Parent Container is not deleted.", false, container1.IsDeleted);

			// remove child package from instructions and ensure delete is ok
			instruction.PackageDivots.DeleteAll();
			AssertNoExceptionThrown("There should be no exceptions thrown", container1.Delete);
			AssertEquals("Parent Container is deleted.", true, container1.IsDeleted);
		}

		public void TestIPackingParent_DeletePackage_WithInnerPackage_PackageDeleteCanceledFiredOnce()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			IPackingParent packingParent = consolidation;
			int firePackageDeleteCanceledCount = 0;

			var container1 = consolidation.PackageJob.Packages.AddNew("CNT");
			var childPackage = container1.Packages.AddNew();
			instruction.DivotsWithPackages.AddPackage(childPackage);

			consolidation.PackageJob.PackageDeleteCanceled += (object sender, PackageCanceledActionEventArgs args) =>
			{
				firePackageDeleteCanceledCount++;
			};

			// attempt delete of parent package with child package on an instruction
			container1.Delete();
			AssertEquals("PackageDeleteCanceled should be fired once only.", 1, firePackageDeleteCanceledCount);
			AssertEquals("Parent package is not deleted.", false, container1.IsDeleted);
		}

		public void TestIPackingParent_IsReadOnly()
		{
			var standAloneConsolidation = Helper.CreateConsolidation();
			var iStandAloneConsolidation = (IPackingParent)standAloneConsolidation;
			AssertEquals(false, iStandAloneConsolidation.IsReadOnly);

			standAloneConsolidation.KB_IsOverridden = true;
			AssertEquals(false, iStandAloneConsolidation.IsReadOnly);

			standAloneConsolidation.ReadOnly = true;
			AssertEquals(true, iStandAloneConsolidation.IsReadOnly);

			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidationWithParent = Helper.CreateConsolidation(dummy);
			var iConsolidationWithParent = (IPackingParent)consolidationWithParent;
			consolidationWithParent.KB_IsOverridden = true;
			AssertEquals(false, iConsolidationWithParent.IsReadOnly);

			consolidationWithParent.KB_IsOverridden = false;
			AssertEquals(true, iConsolidationWithParent.IsReadOnly);
		}

		public void TestIPackingParent_IsPackingJobReadOnly()
		{
			var standAloneConsolidation = Helper.CreateConsolidation();
			var iStandAloneConsolidation = (IPackingParent)standAloneConsolidation;
			AssertEquals(false, iStandAloneConsolidation.IsPackingJobReadOnly);

			standAloneConsolidation.KB_IsOverridden = true;
			AssertEquals(false, iStandAloneConsolidation.IsPackingJobReadOnly);

			standAloneConsolidation.ReadOnly = true;
			AssertEquals(true, iStandAloneConsolidation.IsPackingJobReadOnly);

			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidationWithParent = Helper.CreateConsolidation(dummy);
			var iConsolidationWithParent = (IPackingParent)consolidationWithParent;
			consolidationWithParent.KB_IsOverridden = true;
			AssertEquals(false, iConsolidationWithParent.IsPackingJobReadOnly);

			consolidationWithParent.KB_IsOverridden = false;
			AssertEquals(true, iConsolidationWithParent.IsPackingJobReadOnly);
		}

		public void TestPackageIDOnContainerChangedAddsEventOnSave()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_Direction = Constants.CartageDirection.Export;
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;

			var container1 = Helper.CreatePackageContainer("123");
			var container2 = Helper.CreatePackageContainer("000");
			var container3 = Helper.CreatePackageContainer("777");
			var container4 = Helper.CreatePackageContainer("");
			consolidation.PackageJob.Packages.Add(container1);
			consolidation.PackageJob.Packages.Add(container2);
			consolidation.PackageJob.Packages.Add(container3);
			consolidation.PackageJob.Packages.Add(container4);
			instruction.DivotsWithPackages.AddPackage(container4);

			var packageView = booking.Packages_PackageView.Find(container4);
			var confirmation = packageView.Confirmations[0];

			confirmation.KK_ConfirmationType = InstructionTypes.Codes.PickUp;
			confirmation.Instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			confirmation.KK_IsEmptyContainer = true;
			confirmation.KK_ReferenceNum = "ZZZZ";

			var containerIDChangeEvents = FindContainerIDEventReferences(consolidation.PK);
			AssertEquals("Precondition.", false, containerIDChangeEvents.Any());

			Factory.Save();
			containerIDChangeEvents = FindContainerIDEventReferences(consolidation.PK);
			AssertEquals("No Changes.", false, containerIDChangeEvents.Any());

			container1.KP_PackageID = "456";
			container1.KP_PackageID = "789";
			container1.KP_PackageID = "012";
			container2.KP_PackageID = "111";
			container2.KP_PackageID = "222";
			container2.KP_PackageID = "333";
			container3.KP_PackageID = "454";
			container3.KP_PackageID = "777";
			container4.KP_PackageID = "686";
			container4.KP_PackageID = "242";
			Factory.Save();
			containerIDChangeEvents = FindContainerIDEventReferences(consolidation.PK);
			AssertEquals("Container has changes.", true, containerIDChangeEvents.Any());
			AssertContainsExactElementsInAnyOrder(new[] { "|NEW=012|OLD=123|TYP=ContainerID", "|NEW=333|OLD=000|TYP=ContainerID", "|NEW=242|RFN=ZZZZ|TYP=ContainerID" }, containerIDChangeEvents);
		}

		public void TestContainerIDChangeEventsShouldNotThrowExceptionWhenContainerIsDeleted()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();

			var container1 = Helper.CreatePackageContainer("123");
			var container2 = Helper.CreatePackageContainer("000");
			consolidation.PackageJob.Packages.Add(container1);
			consolidation.PackageJob.Packages.Add(container2);

			AssertNoExceptionThrown("No Exception expected", () => Factory.Save());

			container1.KP_PackageID = "456";
			container2.KP_PackageID = "111";
			container1.Delete();

			Factory.Save();
			AssertEquals("", false, ErrorReporter.LastMessageReported.Contains(@"Developer Error: Should not be accessing a property on a deleted business object"));
			ErrorReporter.Clear();
		}

		IEnumerable<string> FindContainerIDEventReferences(ZGuid parentID)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ChangeOfIdentifier.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, parentID);

			var events = Factory.Load<StmALog>(query);
			return events.Select(e => e.SL_Reference.ToString());
		}

		public void TestIPackingParent_IsLoosePackageIDsSupported()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			IPackingParent packingParent = consolidation;
			AssertEquals(false, packingParent.IsLoosePackageIDsSupported);
		}

		public void TestIPackingParent_PackageSequenceType()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			IPackingParent packingParent = consolidation;
			AssertEquals(PackageSequenceType.Outer, packingParent.PackageSequenceType);
		}

		public void TestIPackingParent_ShouldPackTrackedPackagesViaDivot()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			IPackingParent packingParent = consolidation;
			AssertEquals(false, packingParent.ShouldPackTrackedPackagesViaDivot);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithDtbBooking);
			dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider();
			transportBookingTestCache = TransportBookingTestCache.Instance;
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
			dummyWriterDecider.Dispose();
			transportBookingTestCache.Dispose();
		}

		new TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		protected override Type ExpectedMetadataType
		{
			get { return typeof(Metadata.Business.DtbBookingConsolidation); }
		}

		IDisposable dummyWriterDecider;
		IDisposable transportBookingTestCache;

		public void TestLoadingAsWrongTypeWithoutFactorySave()
		{
			var typesToLoad = new[] { ObjectFactory.GetType<IDtbBookingConsolidation>(), ObjectFactory.GetType<IDtbConsignmentConsolidation>() };
			AssertLoadingAsWrongType(typesToLoad, factorySaveBeforeLoad: false);
		}

		public void TestLoadingAsWrongTypeWithFactorySave()
		{
			var typesToLoad = new[] { ObjectFactory.GetType<IDtbBookingConsolidation>(), ObjectFactory.GetType<IDtbConsignmentConsolidation>() };
			AssertLoadingAsWrongType(typesToLoad, factorySaveBeforeLoad: true);
		}

		public void TestSetDefaultValues_SetsJobType()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			AssertEquals(TransportConsolidationJobTypes.Codes.Booking, consolidation.KB_JobType);
		}

		public void TestBookings()
		{
			var consolidationSingleJob = (DtbBookingConsolidation)GetNewBusinessObject();
			consolidationSingleJob.Bookings.AddNew();
			AssertEquals(typeof(DtbBookingCollection), consolidationSingleJob.Bookings.GetType());
			AssertEquals(true, consolidationSingleJob.IsRegisteredEditableChildObject(consolidationSingleJob.Bookings));
		}

		public void TestBookedByAddress()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			AssertNotNull(consolidation.BookedByAddress);
			AssertEquals(DocAddressType.BookingPartyDocumentaryAddress, consolidation.BookedByAddress.DocAddressType);
		}

		public void TestParentJobDescription()
		{
			var standAloneConsolidation = Helper.CreateConsolidation();
			AssertEquals("Standalone Packing", standAloneConsolidation.ParentJobDescription);

			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "D00000123";
			var consolidationWithParent = Helper.CreateConsolidation(dummy);
			AssertEquals("Dummy D00000123", consolidationWithParent.ParentJobDescription);
		}

		public void TestBookedByOrganisationPK()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			AssertEquals(ZGuid.Empty, consolidation.BookedByOrganisationPK);

			consolidation.BookedByAddress.E2_AddressOverride = true;
			AssertEquals(ZGuid.Empty, consolidation.BookedByOrganisationPK);

			var org = Factory.New<OrgHeader>();
			consolidation.BookedByAddress.OrganisationPK = org.PK;
			AssertEquals(org.PK, consolidation.BookedByOrganisationPK);
		}

		public void TestJobIDIsPopulatedOnSave()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			AssertEquals("", consolidation.KB_JobID);

			Factory.Save();
			AssertEquals("CM00000001", consolidation.KB_JobID);

			consolidation = Helper.CreateConsolidationMultiJob();
			AssertEquals("", consolidation.KB_JobID);

			Factory.Save();
			AssertEquals("CB00000001", consolidation.KB_JobID);
		}

		public void TestDelete()
		{
			var consolidationJob = (DtbBookingConsolidation)GetNewBusinessObject();
			var transport1 = consolidationJob.Bookings.AddNew();
			var transport2 = consolidationJob.Bookings.AddNew();
			consolidationJob.Delete();
			AssertEquals(0, consolidationJob.Bookings.Count);
			AssertEquals(true, transport1.IsDeleted);
			AssertEquals(true, transport1.IsDeleted);

			var consolidationBTC = (DtbBookingConsolidation)GetNewBusinessObject();

			consolidationBTC.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var bookingForConsolidationBTC1 = consolidationBTC.Bookings.AddNew();
			var bookingForConsolidationBTC2 = consolidationBTC.Bookings.AddNew();
			consolidationBTC.Delete();
			AssertEquals(0, consolidationBTC.Bookings.Count);
			AssertEquals(false, bookingForConsolidationBTC1.IsDeleted);
			AssertEquals(false, bookingForConsolidationBTC2.IsDeleted);
		}

		public void TestIJobNumber()
		{
			var transportConsolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			transportConsolidation.KB_JobID = "FOM12341298";
			AssertEquals("((IJobNumber)transportConsolidation).JobNumber", "FOM12341298", ((IJobNumber)transportConsolidation).JobNumber);
		}

		public void TestNotificationManager()
		{
			var consolidationSingleJob = (DtbBookingConsolidation)GetNewBusinessObject();
			AssertNotNull(consolidationSingleJob.NotificationManager);
		}

		public void TestNotificationSubscriber()
		{
			var buffer = new TestNotificationBuffer();
			var consolidationSingleJob = (DtbBookingConsolidation)GetNewBusinessObject();

			consolidationSingleJob.NotificationManager.Push(buffer);
			AssertEquals(buffer, consolidationSingleJob.NotificationSubscriber);

			consolidationSingleJob.NotificationManager.Pop();
			AssertNotNull(consolidationSingleJob.NotificationSubscriber);
		}

		public void TestNotification()
		{
			var notify = new TestNotify();
			var consolidationSingleJob = (DtbBookingConsolidation)GetNewBusinessObject();
			consolidationSingleJob.NotificationManager.Push(notify);

			consolidationSingleJob.NotificationSubscriber.Notify(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "You're doing something crazy!"));
			AssertEquals("You're doing something crazy!", notify.LastNotification.Message);
		}

		class TestNotify : INotifications
		{
			void INotifications.Add(INotification notification)
			{
				Notifications.Add(notification);
				if (notification != null)
				{
					LastNotification = notification;
				}
			}

			public List<INotification> Notifications = new List<INotification>();
			public INotification LastNotification { get; private set; }
		}

		public void TestDocAddresses_WhenConsolidationIsSub()
		{
			var subConsolidation = Helper.CreateConsolidation();
			var masterConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;

			Factory.Save();

			AssertEquals("Sub Consolidation should refer to its Master Consolidation for DocAddresses.", masterConsolidation.DocAddresses, subConsolidation.DocAddresses);
		}

		public void TestDocAddresses_WhenConsolidationIsNotSub()
		{
			var consolidation = Helper.CreateConsolidation();
			AssertEquals("Precondition: Consolidation is not a sub consolidation.", false, consolidation.IsSub);

			AssertEquals(typeof(JobDocAddressDependentCollection), consolidation.DocAddresses.GetType());
			AssertEquals(true, consolidation.IsRegisteredEditableChildObject(consolidation.DocAddresses));
		}

		public void TestDocAddresses_WhenMasterConsolidationChanges()
		{
			var consolidation = Helper.CreateConsolidation();
			Factory.Save();

			AssertEquals("Precondition: KB_KB_MasterBookingConsolidation should be an empty Guid.", ZGuid.Empty, consolidation.KB_KB_MasterBookingConsolidation);
			AssertNotNull("DocAddresses should not return null.", consolidation.DocAddresses);
			AssertEquals("DocAddresses should be a child of the Consolidation.", true, consolidation.IsRegisteredEditableChildObject(consolidation.DocAddresses));

			var masterConsolidation1 = Helper.CreateConsolidation();
			masterConsolidation1.KB_IsMaster = true;
			masterConsolidation1.KB_MasterBookingVersion = 1;
			consolidation.KB_KB_MasterBookingConsolidation = masterConsolidation1.PK;
			consolidation.KB_MasterBookingVersion = 1;
			Factory.Save();

			AssertEquals("Consolidation should refer to its Master Consolidation for DocAddresses.", masterConsolidation1.DocAddresses, consolidation.DocAddresses);

			var masterConsolidation2 = Helper.CreateConsolidation();
			masterConsolidation2.KB_IsMaster = true;
			masterConsolidation2.KB_MasterBookingVersion = 1;
			consolidation.KB_KB_MasterBookingConsolidation = masterConsolidation2.PK;
			Factory.Save();

			AssertEquals("Consolidation should refer to its new Master Consolidation for DocAddresses.", masterConsolidation2.DocAddresses, consolidation.DocAddresses);

			consolidation.KB_KB_MasterBookingConsolidation = ZGuid.Empty;

			AssertNotNull("DocAddresses should not return null.", consolidation.DocAddresses);
			AssertEquals("DocAddresses should be a child of the Consolidation.", true, consolidation.IsRegisteredEditableChildObject(consolidation.DocAddresses));
			AssertNotEquals("Booking should not refer to the first Master Booking for DocAddresses.", masterConsolidation1.DocAddresses, consolidation.DocAddresses);
			AssertNotEquals("Booking should not refer to the second Master Booking for DocAddresses.", masterConsolidation2.DocAddresses, consolidation.DocAddresses);
		}

		public void TestDocAddresses()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			consolidation.BookedByAddress.E2_OA_Address = Helper.CreateOrganisation("ABCSYD").MainAddress.PK;
			AssertContainsExactElementsInAnyOrder(new JobDocAddress[] { consolidation.BookedByAddress }, consolidation.DocAddresses);
			Assert(consolidation.IsRegisteredEditableChildObject(consolidation.DocAddresses));
		}

		public void TestChangingBookingParty()
		{
			// data setup
			// debtor
			var debtor = Helper.CreateOrganisation("DEBTOR", true);

			// non debtor with IFT
			var nonDebtorWithIFT = Helper.CreateOrganisation("NONDEBTORIFT");
			var pickupParty = Helper.CreateOrganisation("PICKUP");
			var deliveryParty = Helper.CreateOrganisation("DELIVERY");
			var pickupAndDeliveryParty = Helper.CreateOrganisation("DELIVERY");
			var otherParty = Helper.CreateOrganisation("OTHER");
			var nonIFT = Helper.CreateOrganisation("NONIFT");
			CreateRelatedParty(nonDebtorWithIFT, pickupParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Pickup);
			CreateRelatedParty(nonDebtorWithIFT, deliveryParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			CreateRelatedParty(nonDebtorWithIFT, pickupAndDeliveryParty, RelatedPartyTypeList.Codes.InvoiceFreightJobsTo, RelatedPartyDirectionList.Codes.Delivery);
			CreateRelatedParty(nonDebtorWithIFT, nonIFT, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup);

			// non debtor without IFT
			var nonDebtorWithNoIFT = Helper.CreateOrganisation("NOIFT");
			CreateRelatedParty(nonDebtorWithIFT, nonIFT, RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo, RelatedPartyDirectionList.Codes.Pickup);

			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var booking = consolidation.Bookings.AddNew();
			consolidation.BookedByAddress.E2_OA_Address = debtor.MainAddress.PK;
			AssertEquals(debtor.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);

			booking.BillingPartyAddress.E2_OA_Address = ZGuid.Empty;
			consolidation.BookedByAddress.E2_OA_Address = nonDebtorWithIFT.MainAddress.PK;
			AssertEquals(pickupParty.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);

			booking.BillingPartyAddress.E2_OA_Address = ZGuid.Empty;
			consolidation.BookedByAddress.E2_OA_Address = nonDebtorWithNoIFT.MainAddress.PK;
			AssertEquals(ZGuid.Empty, booking.BillingPartyAddress.E2_OA_Address);

			nonDebtorWithIFT.OH_IsDebtor = true;
			booking.BillingPartyAddress.E2_OA_Address = ZGuid.Empty;
			consolidation.BookedByAddress.E2_OA_Address = nonDebtorWithIFT.MainAddress.PK;
			AssertEquals(pickupParty.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);

			booking.BillingPartyAddress.E2_AddressOverride = true;
			consolidation.BookedByAddress.E2_OA_Address = debtor.MainAddress.PK;
			AssertEquals(true, booking.BillingPartyAddress.E2_AddressOverride);
			AssertEquals(OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress, booking.BillingPartyAddress.E2_OA_Address);

			var billingParty = Helper.CreateOrganisation("BILLINGPARTY");
			booking.BillingPartyAddress.E2_AddressOverride = false;
			booking.BillingPartyAddress.E2_OA_Address = billingParty.MainAddress.PK;
			consolidation.BookedByAddress.E2_OA_Address = nonDebtorWithIFT.MainAddress.PK;
			AssertEquals(billingParty.MainAddress.PK, booking.BillingPartyAddress.E2_OA_Address);
		}

		static void CreateRelatedParty(OrgHeader nonDebtorWithIFT, OrgHeader relatedPartyOrg, string partyType, string direction)
		{
			var relatedParty = nonDebtorWithIFT.AllRelatedParties.AddNew();
			relatedParty.PR_OH_RelatedParty = relatedPartyOrg.PK;
			relatedParty.PR_PartyType = partyType;
			relatedParty.PR_FreightDirection = direction;
		}

		public void TestSupportedAddressTypes()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;

			var expectedAddressTypes = new List<DocAddressType>() { DocAddressType.BookingPartyDocumentaryAddress, DocAddressType.TransportCompanyDocumentaryAddress };
			AssertContainsExactElementsInAnyOrder(expectedAddressTypes, iConsolidation.SupportedAddressTypes);
		}

		public void TestCanDeleteAddress()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;

			AssertEquals(true, iConsolidation.CanDeleteAddress(consolidation.BookedByAddress));
		}

		public void TestGetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;

			AssertEquals(Env.Security.TransportJobMISCDetails, iConsolidation.GetCanOverrideCheckpoint(null));
		}

		public void TestGetDocAddressRequirement(DocAddressType addressType)
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;
			var requirement = iConsolidation.GetDocAddressRequirement(DocAddressType.BookingPartyDocumentaryAddress);

			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, requirement.DefaultDocAddressType);
			AssertEquals(ContactType.LocalTransport, requirement.DefaultContactType);
			AssertEquals(AddressType.OFC, requirement.DefaultAddressType);
			AssertEquals(true, requirement.SaveEvenIfBlank);
			AssertEquals(false, requirement.IsMandatory);
		}

		public void TestPiggyBackedDocAddressValidation()
		{
			var consolidation = (DtbBookingConsolidation)GetNewBusinessObject();
			var iConsolidation = (IDocAddresses)consolidation;
			AssertNull(iConsolidation.PiggyBackedDocAddressValidation(null));
		}
	}

	public class DtbBookingConsolidationTest : DtbBookingTestCaseWithFactory
	{
		public void TestAddress()
		{
			var transportBooking = Helper.CreateConsolidation();
			AssertNotNull(transportBooking.Address);
			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, transportBooking.Address.DocAddressType);

			var organisation = Helper.CreateOrganisation("QWESYD");
			var pickupAddress = Helper.AddAddressToOrganisation(organisation, "Pickup Addy", OrgAddressType.Pickup);
			var officeAddress = Helper.AddAddressToOrganisation(organisation, "Office Addy", OrgAddressType.Office);
			var deliveryAddress = Helper.AddAddressToOrganisation(organisation, "Delivery Addy", OrgAddressType.Delivery);

			transportBooking.Address.OrganisationPK = organisation.PK;
			AssertEquals("Should default to office address", officeAddress.PK, transportBooking.Address.E2_OA_Address);
		}

		public void TestBusinessObjectsWithRelatedEvents_MultiJobConsolHasChildBookings()
		{
			var booking1 = Factory.NewWithValidTestData<DtbBooking>();
			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			var randomBooking = Factory.NewWithValidTestData<DtbBooking>();

			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			AssertEquals("consolidationMultiJob.BusinessObjectsWithRelatedEvents None", 0, consolidationMultiJob.BusinessObjectsWithRelatedEvents.Length);

			consolidationMultiJob.Bookings.Add(booking1);
			consolidationMultiJob.Bookings.Add(booking2);

			Factory.Save();

			AssertEquals("consolidationMultiJob.BusinessObjectsWithRelatedEvents Length", 2, consolidationMultiJob.BusinessObjectsWithRelatedEvents.Length);
			AssertContainsExactElementsInAnyOrder("consolidationMultiJob BusinessObjectsWithRelatedEvents", new[] { booking1, booking2 }, consolidationMultiJob.BusinessObjectsWithRelatedEvents);
		}

		public void TestBusinessObjectsWithRelatedEvents_SingleJobConsolHasChildBookings()
		{
			var booking1 = Factory.New<DtbBooking>();
			var booking2 = Factory.New<DtbBooking>();

			var randomConsolidation = Helper.CreateConsolidation();
			var randomBooking = Helper.CreateBooking(randomConsolidation);

			var consolidationSingleJob = Helper.CreateConsolidation();
			AssertEquals("consolidationSingleJob.BusinessObjectsWithRelatedEvents None", 0, consolidationSingleJob.BusinessObjectsWithRelatedEvents.Length);

			consolidationSingleJob.Bookings.Add(booking1);
			consolidationSingleJob.Bookings.Add(booking2);

			Factory.Save();

			AssertEquals("consolidationSingleJob.BusinessObjectsWithRelatedEvents Length", 2, consolidationSingleJob.BusinessObjectsWithRelatedEvents.Length);
			AssertContainsExactElementsInAnyOrder("consolidationSingleJob BusinessObjectsWithRelatedEvents", new[] { booking1, booking2 }, consolidationSingleJob.BusinessObjectsWithRelatedEvents);
		}

		public void TestBookings()
		{
			var booking = Factory.New<DtbBooking>();
			booking.KM_Status = TransportStatuses.Codes.Delivered;

			// single job consolidation

			var consolidationSingleJob = Helper.CreateConsolidation();
			AssertEquals(true, consolidationSingleJob.IsRegisteredEditableChildObject(consolidationSingleJob.Bookings));
			AssertEquals("Status should be Available as it's empty.", TransportStatuses.Descriptions.Available, consolidationSingleJob.StatusDescription);

			consolidationSingleJob.Bookings.Add(booking);
			AssertEquals(consolidationSingleJob.PK, booking.KM_KB_Booking);
			AssertEquals(true, booking.KM_KB_BookingConsolidationMultiJob.IsEmpty);
			AssertEquals("Status should be updated when adding a Booking.", TransportStatuses.Descriptions.Delivered, consolidationSingleJob.StatusDescription);

			// multi job consolidation

			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			AssertEquals(true, consolidationMultiJob.IsRegisteredEditableChildObject(consolidationMultiJob.Bookings));
			AssertEquals("Status should be Available as it's empty.", TransportStatuses.Descriptions.Available, consolidationMultiJob.StatusDescription);

			consolidationMultiJob.Bookings.Add(booking);
			AssertEquals(consolidationMultiJob.PK, booking.KM_KB_BookingConsolidationMultiJob);
			AssertEquals(TransportStatuses.Descriptions.Delivered, consolidationMultiJob.StatusDescription);
		}

		public void TestActiveBookings()
		{
			var consolidationSingleJob = Helper.CreateConsolidation();
			AssertEquals("Should be no active bookings.", 0, consolidationSingleJob.ActiveBookings.Count);
			AssertEquals(false, consolidationSingleJob.ActiveBookings.AllowNewCore);

			var booking = Factory.New<DtbBooking>();
			booking.KM_Status = TransportStatuses.Codes.Delivered;

			consolidationSingleJob.Bookings.Add(booking);
			AssertContainsExactElementsInAnyOrder("Should contain booking as it is active.", new DtbBooking[] { booking }, consolidationSingleJob.ActiveBookings);

			booking.KM_IsActive = false;
			AssertEquals("Should not contain inactive bookings.", 0, consolidationSingleJob.ActiveBookings.Count);
		}

		public void TestParent()
		{
			var transportBookingParent = Factory.New<DummyWithDtbBooking>();

			// Transport Booking Without Parent
			var transportBooking = Factory.New<DtbBookingConsolidation>();
			AssertNull("Precondition.", transportBooking.Parent);

			// Transport Booking with IDtbBookingParent
			var transportBooking2 = Factory.New<DtbBookingConsolidation>();
			transportBooking2.KB_ParentID = transportBookingParent.PK;
			transportBooking2.KB_ParentTableCode = transportBookingParent.TablePrefix;
			AssertEquals(transportBookingParent.PK, transportBooking2.Parent.PK);
		}

		// persistent

		public void TestIsMultiBooking()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			AssertEquals("Precondition", true, consolidation.IsMultiBooking);

			consolidation.Bookings.AddNew();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation; // should not blow up as there is no change
		}

		public void TestKB_ConsolidationType_BlowsUpIfSetToCSN()
		{
			var consolidation = Helper.CreateConsolidation();
			AssertEquals("Precondition: Default Booking Consolidation Job Type should be 'BKG'", TransportConsolidationJobTypes.Codes.Booking, consolidation.KB_JobType);

			AssertNoExceptionThrown(() => consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation);
			AssertExceptionThrown(typeof(InvalidOperationException), "CSN is not a valid KB_JobType in a Booking Consolidation.",
				() => consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Consignment);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "KB_JobType cannot be changed if the Consolidation already has Bookings.")]
		public void TestKB_ConsolidationType_BlowsUpIfBookingsNotEmpty()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.Bookings.AddNew();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
		}

		public void TestKB_ParentID()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = consolidation.Bookings.AddNew();
			var booking2 = consolidation.Bookings.AddNew();
			var jobHeader1 = new JobHeader.Loader(booking1).TryCreate();
			var jobHeader2 = new JobHeader.Loader(booking2).TryCreate();
			AssertEquals("Precondition.", booking1.Job.PK, jobHeader1.PK);
			AssertEquals("Precondition.", booking2.Job.PK, jobHeader2.PK);

			DtbAgentBooking agentBooking = null;

			AssertNoExceptionThrown("Assertion shouldnt be thrown", () => agentBooking = DtbAgentBooking.NewFromBooking(booking1));

			var newJh = new JobHeader.Loader(agentBooking).TryCreate();

			AssertEquals("Precondition: No errors should have been reported yet.", string.Empty, ErrorReporter.LastMessageReported);

			AssertEquals("Should be pointing to parent job header.", booking1.Job.PK, newJh.PK);
			AssertEquals("Should be pointing to parent job header.", booking2.Job.PK, newJh.PK);

			ErrorReporter.Clear();
		}

		public void TestErrorReportedWhenBookingsParentIsSetWhenTheBookingAlreadyItsOwnJobHeader()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consol = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consol);

			var bookingJob = new JobHeader.Loader(booking).TryCreate();

			Helper.AttachConsolidationToParent(consol, parent);
			AssertEquals("Attempted to attach a booking to a IDtbBookingParent while the booking already has an attached job header.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestErrorNotReportedWhenBookingIsMovedFromOneParentToAnother()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consol = Helper.CreateConsolidation(parent);
			var booking = Helper.CreateBooking(consol);
			var parentJob = new JobHeader.Loader(booking).TryCreate();

			var otherParent = Factory.New<DummyWithDtbBooking>();
			Helper.AttachConsolidationToParent(consol, otherParent);

			AssertEquals("An error should not be reported", string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestKB_Status()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = consolidation.Bookings.AddNew();
			var booking2 = consolidation.Bookings.AddNew();

			// status should always be readonly
			AssertEquals(TransportStatuses.Codes.Available, consolidation.KB_Status);
			AssertEquals(true, consolidation.KB_StatusInfo.ReadOnly);

			booking1.KM_Status = TransportStatuses.Codes.ActionRequired;
			booking2.KM_Status = TransportStatuses.Codes.ActionRequired;
			AssertEquals(TransportStatuses.Codes.ActionRequired, consolidation.KB_Status);

			booking1.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			booking2.KM_Status = TransportStatuses.Codes.ServiceCommenced;
			AssertEquals(TransportStatuses.Codes.ServiceCommenced, consolidation.KB_Status);

			// if the bookings are partly picked up, the consolidated booking should be available
			booking1.KM_Status = TransportStatuses.Codes.PickedUp;
			booking2.KM_Status = TransportStatuses.Codes.Available;
			AssertEquals(TransportStatuses.Codes.Available, consolidation.KB_Status);

			// if the bookings are all picked up, the consolidated booking should be picked up
			booking1.KM_Status = TransportStatuses.Codes.PickedUp;
			booking2.KM_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals(TransportStatuses.Codes.PickedUp, consolidation.KB_Status);

			// if the bookings are all picked up but partly delivered, the consolidated booking should be picked up
			booking1.KM_Status = TransportStatuses.Codes.PickedUp;
			booking2.KM_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(TransportStatuses.Codes.PickedUp, consolidation.KB_Status);

			// if the bookings are all delivered, the consolidated booking should be delivered
			booking1.KM_Status = TransportStatuses.Codes.Delivered;
			booking2.KM_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(TransportStatuses.Codes.Delivered, consolidation.KB_Status);

			// if any booking is held, the consolidated booking should be held
			booking1.KM_Status = TransportStatuses.Codes.Delivered;
			booking2.KM_Status = TransportStatuses.Codes.Held;
			AssertEquals(TransportStatuses.Codes.Held, consolidation.KB_Status);
		}

		public void TestKB_IsOverridden()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(parent);
			consolidation.KB_IsOverridden = true;
			AssertEquals(true, consolidation.KB_IsOverridden);
			AssertEquals(false, consolidation.PackageJob.ReadOnly);

			consolidation.KB_IsOverridden = false;
			AssertEquals(false, consolidation.KB_IsOverridden);
			AssertEquals(true, consolidation.PackageJob.ReadOnly);

			var standaloneConsolidation = Helper.CreateConsolidation();
			AssertEquals(false, standaloneConsolidation.KB_IsOverridden);
			AssertEquals(false, standaloneConsolidation.PackageJob.ReadOnly);

			consolidation.KB_IsOverridden = true;
			AssertEquals(true, consolidation.KB_IsOverridden);
			AssertEquals("Only affects TB's with a parent", false, consolidation.PackageJob.ReadOnly);
		}

		// calculated

		public void TestDescription()
		{
			AssertEquals("Bookings for Standalone Packing", Helper.CreateConsolidation().Description);

			// booking with parent Dummy
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "D00000123";
			var consolidation = Helper.CreateConsolidation(dummy);
			AssertEquals("Bookings for Dummy D00000123", consolidation.Description);
		}

		public void TestStatusDescription()
		{
			var consolidation = Helper.CreateConsolidation();
			AssertEquals(TransportStatuses.Descriptions.Available, consolidation.StatusDescription);

			consolidation.KB_Status = TransportStatuses.Codes.Held;
			AssertEquals(TransportStatuses.Descriptions.Held, consolidation.StatusDescription);

			consolidation.KB_Status = "xXx";
			AssertEquals("", consolidation.StatusDescription);
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			var booking1 = Helper.CreateBooking();
			var consolidation = booking1.ConsolidationSingleJob;
			var booking2 = Helper.CreateBooking(consolidation);
			var note1OnBooking1 = Helper.CreateNote(booking1, "Note1.");
			var note2OnBooking1 = Helper.CreateNote(booking1, "Note2.");
			var note1OnBooking2 = Helper.CreateNote(booking2, "Note3.");

			AssertContainsExactElementsInAnyOrder(new[] { note1OnBooking1, note1OnBooking2, note2OnBooking1 }, consolidation.Notes.GetAllRelatedNotesVisibleToCurrentCompany());
		}

		public void TestIsParentHidden()
		{
			var shipment = Helper.CreateForwardingShipment("S000010", "HOUSE", "AIR", "LCL");
			var consolidationWithShipment = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			AssertEquals("Parents without the Hidden Booking Parent attribute should not be considered hidden.", false, consolidationWithShipment.IsParentHidden);

			var booking = Helper.CreateBooking();
			DtbAgentBooking.NewFromBooking(booking);
			var consolidationWithAgentBooking = booking.ConsolidationSingleJob;
			AssertEquals("Parents with the Hidden Booking Parent attribute should be considered hidden.", true, consolidationWithAgentBooking.IsParentHidden);
		}

		public void TestReadOnlyForChildren()
		{
			var consolidation = Helper.CreateConsolidation();
			var consolidationMultiJob = Helper.CreateConsolidationMultiJob();
			var package = consolidation.PackageJob.Packages.AddNew("BOX");

			var booking = Factory.New<DtbBooking>();
			consolidation.Bookings.Add(booking);
			consolidationMultiJob.Bookings.Add(booking);

			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var packageDivot = Helper.CreatePackageDivot(instruction, package, 1);
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);

			// set readonly explicitly
			var confirmationCount = instruction.Confirmations.Count;
			consolidation.Bookings.SetReadOnlyIncludingChildren(true);
			AssertEquals(true, booking.ReadOnly);
			AssertEquals(true, instruction.ReadOnly);
			AssertEquals(true, packageDivot.ReadOnly);
			AssertEquals(true, instruction.Confirmations[0].ReadOnly);
			//AssertEquals(true, confirmation.ReadOnly); // cannot do this until David fixes the DtbBookingConfirmationRelationship

			// revert readonly explicitly
			consolidation.Bookings.SetReadOnlyIncludingChildren(false);
			AssertEquals(false, booking.ReadOnly);
			AssertEquals(false, instruction.ReadOnly);
			AssertEquals(false, packageDivot.ReadOnly);
			AssertEquals(false, instruction.Confirmations[0].ReadOnly);
			//AssertEquals(true, confirmation.ReadOnly); // cannot do this until David fixes the DtbBookingConfirmationRelationship

			// change viewmode to multi-Job
			ConsolidationViewModeService.SetViewMode(Factory, ConsolidationViewMode.MultiJob);
			AssertEquals(true, booking.ReadOnly);
			AssertEquals(true, instruction.ReadOnly);
			AssertEquals(true, packageDivot.ReadOnly);
			AssertEquals("Confirmations should be editable on a Multi-Job Consolidation.", false, instruction.Confirmations[0].ReadOnly);
			//AssertEquals("Confirmations should be editable on a Multi-Job Consolidation.", false, confirmation.ReadOnly); // cannot do this until David fixes the DtbBookingConfirmationRelationship
		}

		public void TestSetDefaultValues()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			AssertEquals(TransportStatuses.Codes.Available, consolidation.KB_Status);
		}

		public void TestFetchStrategy()
		{
			var consolidation = Helper.CreateConsolidation();
			AssertNotNull(consolidation.FetchStrategy);
			AssertEquals(typeof(DtbBookingConsolidationFetchStrategy), consolidation.FetchStrategy.GetType());
		}

		public void TestSave_AddsDcfEventIfDelivered_NoParent()
		{
			var now = ZDateTime.Today;

			// create a consolidation
			var consolidation = Helper.CreateConsolidation();
			var logs = consolidation.Logs;

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = booking1.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var confirmation1 = instruction1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction2 = booking2.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var confirmation2 = instruction2.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			confirmation1.KK_Actual = now;
			Factory.Save();
			AssertEquals("Not all Bookings are delivered, CCF event should not exist.", 0, GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised).Count());

			confirmation2.KK_Actual = now.AddDays(1);
			Factory.Save();
			var dcfLogs = GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals(1, dcfLogs.Count());
			AssertEquals(now.AddDays(1), dcfLogs.ElementAt(0).SL_EventTime);
		}

		public void TestSave_AddsDcfEventIfDelivered_AndNotifiesParent()
		{
			var now = ZDateTime.Today;

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_JobDirection = "DLV";

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = booking1.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var confirmation1 = instruction1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction2 = booking2.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var confirmation2 = instruction2.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			confirmation1.KK_Actual = now;
			Factory.Save();
			AssertEquals("Not all Bookings are delivered, CCF event should not exist.", 0, GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised).Count());
			AssertEquals("Not all Bookings are delivered, Parent Job should not be notified.", 0, dummy.Logs.GetAllLogs().Count);

			confirmation2.KK_Actual = now.AddDays(1);
			Factory.Save();
			var dcfLogs = GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals(1, dcfLogs.Count());
			AssertEquals(now.AddDays(1), dcfLogs.ElementAt(0).SL_EventTime);

			var parentDCFLogs = GetLogs(dummy.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals(1, parentDCFLogs.Count());
			AssertEquals(now.AddDays(1), parentDCFLogs.ElementAt(0).SL_EventTime);
			AssertEquals("", parentDCFLogs.ElementAt(0).SL_Reference);

			confirmation1.KK_ReceivedBy = "Bob";
			Factory.Save();
			AssertEquals(2, GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised).Count());
			AssertLog(consolidation, Events.DeliveryCartageCompleteFinalised, now, "Bob");

			parentDCFLogs = GetLogs(dummy.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals(2, parentDCFLogs.Count());
			AssertEquals("Bob", parentDCFLogs.ElementAt(1).SL_Reference);
			AssertEquals(now, parentDCFLogs.ElementAt(1).SL_EventTime);
		}

		public void TestSave_AddsDcfEventIfDelivered_AndNotifiesParentOnSaved()
		{
			var now = ZDateTime.Today;

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_JobDirection = "DLV";

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = booking1.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var confirmation1 = instruction1.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction2 = booking2.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var confirmation2 = instruction2.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			confirmation1.KK_Actual = now;
			Factory.Save();
			AssertEquals("Not all Bookings are delivered, CCF event should not exist.", 0, GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised).Count());
			AssertEquals("Not all Bookings are delivered, Parent Job should not be notified.", 0, dummy.Logs.GetAllLogs().Count);

			confirmation2.KK_Actual = now.AddDays(1);

			BusinessObjectFactory.SavingEventHandler save = (f) => { throw new NotSupportedException(); };
			Factory.Saving += save;
			try
			{
				Factory.Save();
			}
			catch
			{
				// ignore
			}
			Factory.Saving -= save;

			var dcfLogs = GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals("Add the Booking Logs.", 1, dcfLogs.Count());
			AssertEquals(now.AddDays(1), dcfLogs.ElementAt(0).SL_EventTime);

			var parentDCFLogs = GetLogs(dummy.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals("But should NOT have told parent yet until save successful", 0, parentDCFLogs.Count());

			Factory.Save();

			dcfLogs = GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals("Still has the same log", 1, dcfLogs.Count());
			AssertEquals(now.AddDays(1), dcfLogs.ElementAt(0).SL_EventTime);

			parentDCFLogs = GetLogs(dummy.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals("Save was successful, so parent should now have a log", 1, parentDCFLogs.Count());
			AssertEquals(now.AddDays(1), parentDCFLogs.ElementAt(0).SL_EventTime);
			AssertEquals("", parentDCFLogs.ElementAt(0).SL_Reference);
		}

		[TestDate(2011, 04, 01)]
		public void TestSave_AddsDcfEventIfDelivered_RefiresOnDateChange()
		{
			var date = TestDateAttribute.Date;

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_JobDirection = "DLV";

			// create a delivered booking
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var confirmation = instruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			Factory.Save();

			confirmation.KK_Actual = date;

			// ensure CCF log was created and parent notified
			Factory.Save();
			var dcfLogs = GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals(1, dcfLogs.Count());
			AssertEquals(date, dcfLogs.ElementAt(0).SL_EventTime);
			var parentDCFLogs = GetLogs(dummy.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals(1, parentDCFLogs.Count());
			AssertEquals(date, parentDCFLogs.ElementAt(0).SL_EventTime);
			AssertEquals("", parentDCFLogs.ElementAt(0).SL_Reference);

			// modify the latest delivery confirmation but use the same date. ensure no CCF log was created as the dates have not changed.
			confirmation.KK_Actual = ZDate.Empty;
			confirmation.KK_Actual = date;
			Factory.Save();
			AssertEquals(1, GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised).Count());
			parentDCFLogs = GetLogs(dummy.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals(1, parentDCFLogs.Count());

			// modify the latest delivery confirmation date and ensure a second CCF log was created and parent notified
			confirmation.KK_Actual = date.AddDays(1);
			Factory.Save();
			AssertEquals(2, GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised).Count());
			parentDCFLogs = GetLogs(dummy.Logs, Events.DeliveryCartageCompleteFinalised);
			AssertEquals(2, parentDCFLogs.Count());
		}

		public void TestSave_AddsPCFEventIfPickedUp()
		{
			var now = ZDateTime.Now;

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = booking1.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var confirmation1 = instruction1.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction2 = booking2.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var confirmation2 = instruction2.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var cydInstruction = booking2.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			cydInstruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			var cydConfirmation = Helper.GetOrCreateConfirmation(cydInstruction, ConfirmationTypes.Codes.PickUp);

			confirmation1.KK_Actual = now;
			Factory.Save();
			AssertEquals("Not all Bookings are picked up, PCF event should not exist.", 0, GetLogs(consolidation.Logs, Events.PickupCartageCompleteFinalised).Count());
			var parentPCFLogs = GetLogs(dummy.Logs, Events.PickupCartageCompleteFinalised);
			AssertEquals(0, parentPCFLogs.Count());

			confirmation2.KK_Actual = now.AddDays(1);
			Factory.Save();
			AssertEquals("Container yard instruction hasn't been picked up, therefore not all Bookings are picked up, PCF event should not exist.", 0, GetLogs(consolidation.Logs, Events.PickupCartageCompleteFinalised).Count());
			parentPCFLogs = GetLogs(dummy.Logs, Events.PickupCartageCompleteFinalised);
			AssertEquals(0, parentPCFLogs.Count());

			cydConfirmation.KK_Actual = now.AddDays(2);
			Factory.Save();
			var pcfLogs = GetLogs(consolidation.Logs, Events.PickupCartageCompleteFinalised);
			AssertEquals(1, pcfLogs.Count());
			AssertEquals(now.AddDays(2), pcfLogs.ElementAt(0).SL_EventTime);

			parentPCFLogs = GetLogs(dummy.Logs, Events.PickupCartageCompleteFinalised);
			AssertEquals(1, parentPCFLogs.Count());
			AssertEquals(now.AddDays(2), parentPCFLogs.ElementAt(0).SL_EventTime);
		}

		[TestDate(2016, 06, 10)]
		public void TestSave_AddsPUPDLVEventsIfPickedUpFromCNROrCNEIsCompleted()
		{
			var now = ZDateTime.Now;
			var pickupOrganisation = Helper.CreateOrganisation("CNR");
			var ctoOrganisation = Helper.CreateOrganisation("CTO");
			var deliveryOrganisation = Helper.CreateOrganisation("CNE");

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			var instruction2 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNR, ctoOrganisation.MainAddress);
			var instruction3 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, ctoOrganisation.MainAddress);
			var confirmation1 = instruction1.Confirmations.Single();
			var confirmation2 = instruction2.Confirmations.Single();
			var confirmation3 = instruction3.Confirmations.Single();
			confirmation2.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction4 = Helper.CreateInstruction(booking2, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation.MainAddress);
			var instruction5 = Helper.CreateInstruction(booking2, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CNE, ctoOrganisation.MainAddress);
			var instruction6 = Helper.CreateInstruction(booking2, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CTO, ctoOrganisation.MainAddress);
			var confirmation4 = instruction4.Confirmations.Single();
			var confirmation5 = instruction5.Confirmations.Single();
			var confirmation6 = instruction6.Confirmations.Single();
			confirmation5.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;

			AssertEquals("Precondition", 0, GetLogs(consolidation.Logs, Events.PickedUp).Count());
			AssertEquals("Precondition", 0, GetLogs(dummy.Logs, Events.PickedUp).Count());
			confirmation1.KK_Actual = now.AddDays(1);
			Factory.Save();
			AssertEquals(0, GetLogs(consolidation.Logs, Events.PickedUp).Count());
			AssertEquals(0, GetLogs(dummy.Logs, Events.PickedUp).Count());

			confirmation2.KK_Actual = now;
			Factory.Save();
			AssertEquals("All CNR confirmations are completed.", 1, GetLogs(consolidation.Logs, Events.PickedUp).Count());
			var pickedUpLogs = GetLogs(dummy.Logs, Events.PickedUp);
			AssertEquals(1, pickedUpLogs.Count());
			AssertEquals(now.AddDays(1), pickedUpLogs.Single().SL_EventTime);

			AssertEquals("Precondition", 0, GetLogs(consolidation.Logs, Events.Delivered).Count());
			AssertEquals("Precondition", 0, GetLogs(dummy.Logs, Events.Delivered).Count());
			confirmation4.KK_Actual = now;
			Factory.Save();
			AssertEquals(0, GetLogs(consolidation.Logs, Events.Delivered).Count());
			AssertEquals(0, GetLogs(dummy.Logs, Events.Delivered).Count());

			confirmation5.KK_Actual = now;
			Factory.Save();
			AssertEquals("All CNE confirmations are completed.", 1, GetLogs(consolidation.Logs, Events.Delivered).Count());
			var deliveredLogs = GetLogs(dummy.Logs, Events.Delivered);
			AssertEquals(1, deliveredLogs.Count());
		}

		public void TestSave_AddsPUPEvents_ConfirmationActualDateIsChanged()
		{
			TestSave_AddsEvents_ConfirmationActualDateIsChanged(Events.PickedUp, InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, c => c.CheckIfPUPNeedsToBeSent);
		}

		public void TestSave_AddsDLVEvents_ConfirmationActualDateIsChanged()
		{
			TestSave_AddsEvents_ConfirmationActualDateIsChanged(Events.Delivered, InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, c => c.CheckIfDLVNeedsToBeSent);
		}

		void TestSave_AddsEvents_ConfirmationActualDateIsChanged(Event expectedEvent, string instructionType, string confirmationType, string orgType, Func<DtbBookingConsolidation, bool> getFlagValue)
		{
			var now = ZDateTime.Now;
			var organisation1 = Helper.CreateOrganisation("O1");
			var ctoOrganisation = Helper.CreateOrganisation("CTO");
			var organisation2 = Helper.CreateOrganisation("O2");

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking1, instructionType, orgType, organisation1.MainAddress);
			var instruction2 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.Multi, orgType, ctoOrganisation.MainAddress);
			var instruction3 = Helper.CreateInstruction(booking1, instructionType, OrganisationTypesList.Codes.CTO, ctoOrganisation.MainAddress);
			var confirmation1 = instruction1.Confirmations.Single();
			var confirmation2 = instruction2.Confirmations.Single();
			var confirmation3 = instruction3.Confirmations.Single();
			confirmation2.KK_ConfirmationType = confirmationType;

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction4 = Helper.CreateInstruction(booking2, instructionType, orgType, organisation2.MainAddress);
			var confirmation4 = instruction4.Confirmations.Single();

			AssertEquals("Precondition", 0, GetLogs(consolidation.Logs, expectedEvent).Count());
			AssertEquals("Precondition", 0, GetLogs(dummy.Logs, expectedEvent).Count());
			confirmation1.KK_Actual = now;
			confirmation4.KK_Actual = now;
			Factory.Save();
			AssertEquals(0, GetLogs(consolidation.Logs, expectedEvent).Count());
			AssertEquals(0, GetLogs(dummy.Logs, expectedEvent).Count());

			AssertEquals(false, getFlagValue(consolidation));
			confirmation2.KK_Actual = now;
			AssertEquals(true, getFlagValue(consolidation));
			Factory.Save();
			AssertEquals(false, getFlagValue(consolidation));
			AssertEquals($"All {orgType} confirmations are completed.", 1, GetLogs(consolidation.Logs, expectedEvent).Count());
			var dummyLogs = GetLogs(dummy.Logs, expectedEvent);
			AssertEquals(1, dummyLogs.Count());
		}

		public void TestSave_AddsPUPEvents_CNRConfirmationDeleted()
		{
			AssertSave_AddsEvents_ConfirmationDeleted(Events.PickedUp, InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, c => c.CheckIfPUPNeedsToBeSent);
		}

		public void TestSave_AddsDLVEvents_CNEConfirmationDeleted()
		{
			AssertSave_AddsEvents_ConfirmationDeleted(Events.Delivered, InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, c => c.CheckIfDLVNeedsToBeSent);
		}

		void AssertSave_AddsEvents_ConfirmationDeleted(Event expectedEvent, string instructionType, string confirmationType, string orgType, Func<DtbBookingConsolidation, bool> getFlagValue)
		{
			var now = ZDateTime.Now;
			var organisation1 = Helper.CreateOrganisation("O1");
			var ctoOrganisation = Helper.CreateOrganisation("CTO");
			var organisation2 = Helper.CreateOrganisation("O2");

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking1, instructionType, orgType, organisation1.MainAddress);
			var instruction2 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.Multi, orgType, ctoOrganisation.MainAddress);
			var instruction3 = Helper.CreateInstruction(booking1, instructionType, OrganisationTypesList.Codes.CTO, ctoOrganisation.MainAddress);
			var confirmation1 = instruction1.Confirmations.Single();
			var confirmation2 = instruction2.Confirmations.Single();
			var confirmation3 = instruction3.Confirmations.Single();
			confirmation2.KK_ConfirmationType = confirmationType;

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction4 = Helper.CreateInstruction(booking2, instructionType, orgType, organisation2.MainAddress);
			var confirmation4 = instruction4.Confirmations.Single();

			AssertEquals("Precondition", 0, GetLogs(consolidation.Logs, expectedEvent).Count());
			AssertEquals("Precondition", 0, GetLogs(dummy.Logs, expectedEvent).Count());
			confirmation1.KK_Actual = now;
			confirmation4.KK_Actual = now;
			Factory.Save();
			AssertEquals(0, GetLogs(consolidation.Logs, expectedEvent).Count());
			AssertEquals(0, GetLogs(dummy.Logs, expectedEvent).Count());

			AssertEquals(false, getFlagValue(consolidation));
			confirmation2.Delete();
			AssertEquals(true, getFlagValue(consolidation));
			Factory.Save();
			AssertEquals(false, getFlagValue(consolidation));
			AssertEquals($"All {orgType} confirmations are completed.", 1, GetLogs(consolidation.Logs, expectedEvent).Count());
			var dummyLogs = GetLogs(dummy.Logs, expectedEvent);
			AssertEquals(1, dummyLogs.Count());
		}

		public void TestSave_AddsPUPEvents_OrgTypeChangedToNonCNR()
		{
			AssertSave_AddsEvents_OrgTypeChanged(Events.PickedUp, InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR,
				OrganisationTypesList.Codes.CTO, c => c.CheckIfPUPNeedsToBeSent);
		}

		public void TestSave_AddsDLVEvents_OrgTypeChangedToNonCNE()
		{
			AssertSave_AddsEvents_OrgTypeChanged(Events.Delivered, InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery,
				OrganisationTypesList.Codes.CNE, OrganisationTypesList.Codes.CTO, c => c.CheckIfDLVNeedsToBeSent);
		}

		public void TestSave_AddsPUPEvents_OrgTypeChangedToCNR()
		{
			AssertSave_AddsEvents_OrgTypeChanged(Events.PickedUp, InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp,
				OrganisationTypesList.Codes.CTO, OrganisationTypesList.Codes.CNR, c => c.CheckIfPUPNeedsToBeSent);
		}

		public void TestSave_AddsDLVEvents_OrgTypeChangedToCNE()
		{
			AssertSave_AddsEvents_OrgTypeChanged(Events.Delivered, InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery,
				OrganisationTypesList.Codes.CTO, OrganisationTypesList.Codes.CNE, c => c.CheckIfDLVNeedsToBeSent);
		}

		void AssertSave_AddsEvents_OrgTypeChanged(Event expectedEvent, string instructionType, string confirmationType,
			string orgType, string changedOrgType, Func<DtbBookingConsolidation, bool> getFlagValue)
		{
			var now = ZDateTime.Now;
			var organisation1 = Helper.CreateOrganisation("O1");
			var ctoOrganisation = Helper.CreateOrganisation("CTO");
			var organisation2 = Helper.CreateOrganisation("O2");

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking1, instructionType, orgType, organisation1.MainAddress);
			var instruction2 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.Multi, orgType, ctoOrganisation.MainAddress);
			var instruction3 = Helper.CreateInstruction(booking1, instructionType, OrganisationTypesList.Codes.CTO, ctoOrganisation.MainAddress);
			var confirmation1 = instruction1.Confirmations.Single();
			var confirmation2 = instruction2.Confirmations.Single();
			var confirmation3 = instruction3.Confirmations.Single();
			confirmation2.KK_ConfirmationType = confirmationType;

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction4 = Helper.CreateInstruction(booking2, instructionType, orgType, organisation2.MainAddress);
			var confirmation4 = instruction4.Confirmations.Single();

			AssertEquals("Precondition", 0, GetLogs(consolidation.Logs, expectedEvent).Count());
			AssertEquals("Precondition", 0, GetLogs(dummy.Logs, expectedEvent).Count());
			confirmation1.KK_Actual = now;
			confirmation4.KK_Actual = now;
			Factory.Save();
			AssertEquals(0, GetLogs(consolidation.Logs, expectedEvent).Count());
			AssertEquals(0, GetLogs(dummy.Logs, expectedEvent).Count());

			AssertEquals(false, getFlagValue(consolidation));
			instruction2.OrganisationType = changedOrgType;
			confirmation2.KK_Actual = now;
			AssertEquals(true, getFlagValue(consolidation));
			Factory.Save();
			AssertEquals(false, getFlagValue(consolidation));
			AssertEquals($"All {orgType} confirmations are completed.", 1, GetLogs(consolidation.Logs, expectedEvent).Count());
			var dummyLogs = GetLogs(dummy.Logs, expectedEvent);
			AssertEquals(1, dummyLogs.Count());

			instruction2.OrganisationType = orgType;
			AssertEquals(true, getFlagValue(consolidation));
			Factory.Save();
			AssertEquals(1, GetLogs(consolidation.Logs, expectedEvent).Count());
		}

		public void TestSave_AddsPUPEvents_InstructionDeleted()
		{
			TestSave_AddsEvents_InstructionDeleted(Events.PickedUp, InstructionTypes.Codes.PickUp, ConfirmationTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, c => c.CheckIfPUPNeedsToBeSent);
		}

		public void TestSave_AddsDLVEvents_InstructionDeleted()
		{
			TestSave_AddsEvents_InstructionDeleted(Events.Delivered, InstructionTypes.Codes.Delivery, ConfirmationTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, c => c.CheckIfDLVNeedsToBeSent);
		}

		void TestSave_AddsEvents_InstructionDeleted(Event expectedEvent, string instructionType, string confirmationType, string orgType, Func<DtbBookingConsolidation, bool> getFlagValue)
		{
			var now = ZDateTime.Now;
			var organisation1 = Helper.CreateOrganisation("O1");
			var ctoOrganisation = Helper.CreateOrganisation("CTO");
			var organisation2 = Helper.CreateOrganisation("O2");

			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking1, instructionType, orgType, organisation1.MainAddress);
			var instruction2 = Helper.CreateInstruction(booking1, InstructionTypes.Codes.Multi, orgType, ctoOrganisation.MainAddress);
			var instruction3 = Helper.CreateInstruction(booking1, instructionType, OrganisationTypesList.Codes.CTO, ctoOrganisation.MainAddress);
			var confirmation1 = instruction1.Confirmations.Single();
			var confirmation2 = instruction2.Confirmations.Single();
			var confirmation3 = instruction3.Confirmations.Single();
			confirmation2.KK_ConfirmationType = confirmationType;

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			var instruction4 = Helper.CreateInstruction(booking2, instructionType, orgType, organisation2.MainAddress);
			var confirmation4 = instruction4.Confirmations.Single();

			AssertEquals("Precondition", 0, GetLogs(consolidation.Logs, expectedEvent).Count());
			AssertEquals("Precondition", 0, GetLogs(dummy.Logs, expectedEvent).Count());
			confirmation1.KK_Actual = now;
			confirmation4.KK_Actual = now;
			Factory.Save();
			AssertEquals(0, GetLogs(consolidation.Logs, expectedEvent).Count());
			AssertEquals(0, GetLogs(dummy.Logs, expectedEvent).Count());

			AssertEquals(false, getFlagValue(consolidation));
			instruction2.Delete();
			AssertEquals(true, getFlagValue(consolidation));
			Factory.Save();
			AssertEquals(false, getFlagValue(consolidation));
			AssertEquals($"All {orgType} confirmations are completed.", 1, GetLogs(consolidation.Logs, expectedEvent).Count());
			var dummyLogs = GetLogs(dummy.Logs, expectedEvent);
			AssertEquals(1, dummyLogs.Count());
		}

		public void TestSave_FireEvents()
		{
			var now = ZDateTime.Now;

			var consolidation = Helper.CreateConsolidation();

			var originBooking = Helper.CreateBooking(consolidation);
			var destinationBooking = Helper.CreateBooking(consolidation);
			originBooking.KM_Direction = Constants.CartageDirection.Origin;
			destinationBooking.KM_Direction = Constants.CartageDirection.Destination;
			AssertEquals("Precondition: IsDeliveryDirection is false for originBooking", false, originBooking.IsDeliveryDirection);
			AssertEquals("Precondition: IsDeliveryDirection is true for destinationBooking", true, destinationBooking.IsDeliveryDirection);

			// origin booking
			var hireInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cnrInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cfsInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var dehireInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			hireInstructionA.OrganisationType = OrganisationTypesList.Codes.CYD;
			dehireInstructionA.OrganisationType = OrganisationTypesList.Codes.CYD;

			// destination booking
			var hireInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cnrInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cfsInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var dehireInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			hireInstructionB.OrganisationType = OrganisationTypesList.Codes.CYD;
			dehireInstructionB.OrganisationType = OrganisationTypesList.Codes.CYD;

			// origin booking - hire empty container
			Helper.GetOrCreateConfirmation(hireInstructionA, ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-12);
			Factory.Save();
			AssertLogCounts(consolidation, 0, 0, 0, 0);

			// destination booking - hire empty container
			Helper.GetOrCreateConfirmation(hireInstructionB, ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-11);
			Factory.Save();
			AssertLogCounts(consolidation, 0, 0, 0, 0);

			// origin booking - pickup goods
			cnrInstructionA.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-10);
			Factory.Save();
			AssertLogCounts(consolidation, 0, 0, 0, 0);

			// destination booking - complete the pickup of goods (as IsDeliveryDirection is true, only pickup instructions need to have a status of picked up)
			cnrInstructionB.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-9);
			Factory.Save();
			AssertLogCounts(consolidation, 0, 0, 0, 0);

			// origin booking - deliver goods
			cfsInstructionA.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-8);
			Factory.Save();
			AssertLogCounts(consolidation, 0, 0, 0, 0);

			// destination booking - deliver goods
			cfsInstructionB.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-7);
			Factory.Save();
			AssertLogCounts(consolidation, 0, 0, 0, 0);

			// origin booking - complete the pickup of goods - Both bookings are picked up (as IsDeliveryDirection is false, pickup instructions and multi instructions must have a status of picked up)
			cfsInstructionA.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-6);
			Factory.Save();
			AssertLogCounts(consolidation, 1, 0, 0, 0);
			AssertLog(consolidation, Events.PickupCartageCompleteFinalised, now.AddHours(-6), "");

			// destination booking - pickup at multi
			cfsInstructionB.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-5);
			Factory.Save();
			AssertLogCounts(consolidation, 1, 0, 0, 0);

			// origin booking - complete the delivery of goods - but destination booking is not delivered
			cneInstructionA.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-4);
			Factory.Save();
			AssertLogCounts(consolidation, 1, 0, 0, 0);

			// destination booking - complete the delivery of goods - now both bookings are delivered
			cneInstructionB.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-3);
			Factory.Save();
			AssertLogCounts(consolidation, 1, 1, 0, 0);
			AssertLog(consolidation, Events.DeliveryCartageCompleteFinalised, now.AddHours(-3), "");

			// origin booking - dehire the empty container - but both bookings are not completely delivered
			Helper.GetOrCreateConfirmation(dehireInstructionA, ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-2);
			Factory.Save();
			AssertLogCounts(consolidation, 1, 1, 0, 0);

			// destination booking - dehire the empty container - now both are completely delivered
			Helper.GetOrCreateConfirmation(dehireInstructionB, ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-1);
			Factory.Save();
			AssertLogCounts(consolidation, 1, 1, 1, 1);
			AssertLog(consolidation, Events.CartageCompleteFinalised, now.AddHours(-1), "");
			AssertLog(consolidation, Events.GateIn, now.AddHours(-1), "|FAC=CY");
		}

		public void TestSave_FireEvents_AllEnteredAtOnce()
		{
			var now = ZDateTime.Now;

			var consolidation = Helper.CreateConsolidation();

			var originBooking = Helper.CreateBooking(consolidation);
			var destinationBooking = Helper.CreateBooking(consolidation);
			originBooking.KM_Direction = Constants.CartageDirection.Origin;
			destinationBooking.KM_Direction = Constants.CartageDirection.Destination;
			AssertEquals("Precondition: IsDeliveryDirection is false for originBooking", false, originBooking.IsDeliveryDirection);
			AssertEquals("Precondition: IsDeliveryDirection is true for destinationBooking", true, destinationBooking.IsDeliveryDirection);

			// origin booking
			var hireInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cnrInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cfsInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var dehireInstructionA = originBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			hireInstructionA.OrganisationType = OrganisationTypesList.Codes.CYD;
			dehireInstructionA.OrganisationType = OrganisationTypesList.Codes.CYD;

			// destination booking
			var hireInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cnrInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var cfsInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Multi);
			var cneInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			var dehireInstructionB = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			hireInstructionB.OrganisationType = OrganisationTypesList.Codes.CYD;
			dehireInstructionB.OrganisationType = OrganisationTypesList.Codes.CYD;

			// origin booking - hire empty container
			Helper.GetOrCreateConfirmation(hireInstructionA, ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-12);

			// destination booking - hire empty container
			Helper.GetOrCreateConfirmation(hireInstructionB, ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-11);

			// origin booking - pickup goods
			cnrInstructionA.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-10);

			// destination booking - complete the pickup of goods (as IsDeliveryDirection is true, only pickup instructions need to have a status of picked up)
			cnrInstructionB.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-9);

			// origin booking - deliver goods
			cfsInstructionA.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-8);

			// destination booking - deliver goods
			cfsInstructionB.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-7);

			// origin booking - complete the pickup of goods - Both bookings are picked up
			cfsInstructionA.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-6);

			// destination booking - pickup at multi
			cfsInstructionB.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp).KK_Actual = now.AddHours(-5);

			// origin booking - complete the delivery of goods - but destination booking is not delivered
			cneInstructionA.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-4);

			// destination booking - complete the delivery of goods - now both bookings are delivered
			cneInstructionB.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-3);

			// origin booking - dehire the empty container - but both bookings are not completely delivered
			Helper.GetOrCreateConfirmation(dehireInstructionA, ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-2);

			// destination booking - dehire the empty container - now both are completely delivered
			Helper.GetOrCreateConfirmation(dehireInstructionB, ConfirmationTypes.Codes.Delivery).KK_Actual = now.AddHours(-1);

			Factory.Save();
			AssertLogCounts(consolidation, 1, 1, 1, 1);
			AssertLog(consolidation, Events.CartageCompleteFinalised, now.AddHours(-1), "");
			AssertLog(consolidation, Events.GateIn, now.AddHours(-1), "|FAC=CY");
		}

		public void TestSave_FireEvents_WithoutLoadingConsolidation()
		{
			var now = ZDateTime.Now;

			var consolidation = Helper.CreateConsolidation();
			var destinationBooking = Helper.CreateBooking(consolidation);
			destinationBooking.KM_Direction = Constants.CartageDirection.Destination;

			var pickupInstruction = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = destinationBooking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			var pickupConfirmation = pickupInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.PickUp);
			var deliveryConfirmation = deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var pickupConfirmation_OtherFactory = otherFactory.Load<DtbBookingConfirmation>(pickupConfirmation.PK);
			var deliveryConfirmation_OtherFactory = otherFactory.Load<DtbBookingConfirmation>(deliveryConfirmation.PK);

			pickupConfirmation_OtherFactory.KK_Actual = now.AddHours(-4);
			deliveryConfirmation_OtherFactory.KK_Actual = now.AddHours(-2);

			otherFactory.Save();

			var consolidation_OtherFactory = otherFactory.Load<DtbBookingConsolidation>(consolidation.PK);
			AssertLogCounts(consolidation, 1, 1, 1, 0);
			AssertLog(consolidation, Events.CartageCompleteFinalised, now.AddHours(-2), "");
		}

		[TestDate(2019, 1, 1)]
		public void TestAddBookingRequestedEventToParent()
		{
			// create a consolidation
			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(dummy);

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			booking1.KM_Direction = "ORG";
			booking1.KM_JobID = "TB1";
			booking1.KM_BookingOfTransportRequestedDate = ZDateTime.Now;

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			booking2.KM_Direction = "DST";
			booking2.KM_JobID = "TB2";
			booking2.KM_BookingOfTransportRequestedDate = ZDateTime.Now;

			// create booking 3
			var booking3 = consolidation.Bookings.AddNew();
			booking3.KM_JobID = "TB3";
			booking3.KM_BookingOfTransportRequestedDate = ZDateTime.Now;

			AssertEquals("Precondition: No BKQ event in the parent.", false, dummy.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			AssertEquals("Precondition: No BKQ event in booking1.", false, booking1.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			AssertEquals("Precondition: No BKQ event in booking2.", false, booking2.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));
			AssertEquals("Precondition: No BKQ event in booking3.", false, booking3.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.BookingRequestedCode));

			Factory.Save();

			void AssertLog(IStmALogParent stmALogParent, Event eventType, ZDateTime date, ZString jobID, ZString eventReferenceType)
			{
				var onlyLog = GetLogs(stmALogParent.Logs, eventType).SingleOrDefault(l =>
					l.SL_EventTime == date
					&& l.ReferenceFreeText == jobID
					&& l.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] == eventReferenceType);
				AssertNotNull(string.Format("Could not find log of Event Type '{0}' and Date '{1}'", eventType.Description, date.ToLongTimeString()), onlyLog);
			}

			AssertLog(booking1, Events.BookingRequested, ZDateTime.Now, "TB1", "Pickup Transport");
			AssertLog(booking2, Events.BookingRequested, ZDateTime.Now, "TB2", "Delivery Transport");
			AssertLog(booking3, Events.BookingRequested, ZDateTime.Now, "TB3", "Transport");

			var parentBkqEventLogs = dummy.Logs.Find(log => log.SL_SE_NKEvent == Events.BookingRequestedCode);
			AssertEquals("There should be 3 BKQ events on parent.", 3, parentBkqEventLogs.Count());
			AssertLog(dummy, Events.BookingRequested, ZDateTime.Now, "TB1", "Pickup Transport");
			AssertLog(dummy, Events.BookingRequested, ZDateTime.Now, "TB2", "Delivery Transport");
			AssertLog(dummy, Events.BookingRequested, ZDateTime.Now, "TB3", "Transport");
		}

		public void TestAddBookingRequestedEventToParent_NoParent()
		{
			// create a consolidation
			var consolidation = Helper.CreateConsolidation();

			// create booking 1
			var booking1 = consolidation.Bookings.AddNew();
			booking1.KM_BookingOfTransportRequestedDate = ZDateTime.Now;

			// create booking 2
			var booking2 = consolidation.Bookings.AddNew();
			booking2.KM_BookingOfTransportRequestedDate = ZDateTime.Now;

			AssertNoExceptionThrown("No exception is thrown during save.", Factory.Save);
		}

		public void TestAttachEvents()
		{
			var standAloneConsolidation = Helper.CreateConsolidation();
			AssertNull("Precondition", standAloneConsolidation.Parent);

			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "D00000123";
			var pickupConsolidationWithParent = Helper.CreateConsolidation(dummy);
			pickupConsolidationWithParent.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			Factory.Save();
			AssertEquals("Only adds Parent Attach Log if there are bookings", 0, GetLogs(dummy.Logs, Events.Attached).Count());
			AssertEquals("Only adds Parent Attach Log if there are bookings", 0, GetLogs(pickupConsolidationWithParent.Logs, Events.Attached).Count());

			var pickupBooking1 = pickupConsolidationWithParent.Bookings.AddNew();
			Factory.Save();
			AssertEquals("One booking added", 1, GetLogs(pickupBooking1.Logs, Events.Attached).Count());
			AssertLog(pickupBooking1, Events.Attached, "Dummy", dummy.JobNumber);

			var pickupBooking2 = pickupConsolidationWithParent.Bookings.AddNew();
			Factory.Save();
			AssertEquals("Another booking added", 1, GetLogs(pickupBooking2.Logs, Events.Attached).Count());
			AssertLog(pickupBooking2, Events.Attached, "Dummy", dummy.JobNumber);

			var deliveryConsolidationWithParent = Helper.CreateConsolidation(dummy);
			deliveryConsolidationWithParent.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var deliveryBooking1 = deliveryConsolidationWithParent.Bookings.AddNew();
			Factory.Save();
			AssertEquals("Delivery Booking added same time as consolidation, should now have 1", 1, GetLogs(deliveryBooking1.Logs, Events.Attached).Count());
			AssertLog(deliveryBooking1, Events.Attached, "Dummy", dummy.JobNumber);

			var deliveryBooking2 = deliveryConsolidationWithParent.Bookings.AddNew();
			Factory.Save();
			AssertEquals("Another Delivery Booking added, should now have 1", 1, GetLogs(deliveryBooking2.Logs, Events.Attached).Count());
			AssertLog(deliveryBooking2, Events.Attached, "Dummy", dummy.JobNumber);
		}

		void AssertLog(IStmALogParent parent, Event eventType, ZString paramtype, ZString jobID)
		{
			var lastEventLog = GetLogs(parent.Logs, eventType).OrderBy(l => l.SL_EventTime).LastOrDefault();
			AssertNotNull("Event log should not be null", lastEventLog);
			AssertEquals("paramtype should be correct", paramtype, lastEventLog.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type));
			AssertEquals("ReferenceFreeText should be correct", jobID, lastEventLog.ReferenceFreeText);
		}

		void AssertLogCounts(DtbBookingConsolidation consolidation, ZInt pickup, ZInt delivery, ZInt complete, ZInt emptyReturned)
		{
			AssertEquals("PickupCartageCompleteFinalised Count.", pickup, GetLogs(consolidation.Logs, Events.PickupCartageCompleteFinalised).Count());
			AssertEquals("DeliveryCartageCompleteFinalised Count.", delivery, GetLogs(consolidation.Logs, Events.DeliveryCartageCompleteFinalised).Count());
			AssertEquals("CartageCompleteFinalised Count.", complete, GetLogs(consolidation.Logs, Events.CartageCompleteFinalised).Count());
			AssertEquals("GateIn Count.", emptyReturned, GetLogs(consolidation.Logs, Events.GateIn).Count(c => c.SL_Reference == "|FAC=CY"));
		}

		void AssertLog(DtbBookingConsolidation consolidation, Event eventType, ZDateTime date, ZString reference)
		{
			var onlyLog = GetLogs(consolidation.Logs, eventType).SingleOrDefault(l => l.SL_EventTime == date && l.SL_Reference == reference);
			AssertNotNull(string.Format("Could not find log of Event Type '{0}' and Date '{1}'", eventType.Description, date.ToLongTimeString()), onlyLog);
		}

		IEnumerable<StmALog> GetLogs(Logs logs, Event eventType)
		{
			return logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == eventType.Code);
		}

		public void TestDelete()
		{
			var consolidation = Helper.CreateConsolidation();
			var packageJob = Helper.CreatePackageJob(consolidation);

			AssertEquals("CanDelete should be true for a consolidation without any bookings", true, consolidation.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete is empty", "", consolidation.ReasonForNotAbleToDelete);
			consolidation.Delete();
			AssertEquals(true, consolidation.IsDeleted);
			AssertEquals(true, packageJob.IsDeleted);
		}

		public void TestDelete_ForMultiJobConsolidation()
		{
			var consolidation = Helper.CreateConsolidationMultiJob();
			consolidation.KB_JobID = "CB99999999";
			AssertEquals("Precondition: Before adding the booking, CanDelete returns true", true, consolidation.CanDelete);
			var booking = consolidation.Bookings.AddNew();
			AssertEquals("Precondition", consolidation, booking.ConsolidationMultiJob);

			AssertEquals("Should indicate CanDelete is false if a consolidation contains bookings", false, consolidation.CanDelete);
			AssertEquals("Should provide ReasonForNotAbleToDelete mentioning bookings", @"Transport Booking Consolidation ""CB99999999"" cannot be deleted. Detach existing Transport Booking(s) before deleting.", consolidation.ReasonForNotAbleToDelete);

			// CanDelete does not make Delete impossible at the code level. Keep testing describing what would happen in such a scenario.
			consolidation.Delete();
			AssertEquals(true, consolidation.IsDeleted);
			AssertEquals("Deleting a Multi-Job Consolidation should detach and *not* delete its Bookings.", false, booking.IsDeleted);
			AssertNull(booking.ConsolidationMultiJob);
		}

		public void TestRunOnSelectDtbBookingsToPrint()
		{
			DtbBookingConsolidation consolidation = Helper.CreateConsolidation();
			int selectDtbBookingsToPrintCount = 0;

			consolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>((object sender, DtbBookingsToPrintEventArgs e) =>
			{
				selectDtbBookingsToPrintCount++;
			});

			var bookingsToPrintArgs = new DtbBookingsToPrintEventArgs(new DocumentDtbBookingCollection(consolidation.Bookings));
			AssertEquals("Precondition", 0, selectDtbBookingsToPrintCount);

			consolidation.RunOnSelectDtbBookingsToPrint(this, bookingsToPrintArgs);
			AssertEquals(1, selectDtbBookingsToPrintCount);

			consolidation.RunOnSelectDtbBookingsToPrint(this, bookingsToPrintArgs);
			AssertEquals(2, selectDtbBookingsToPrintCount);
		}

		// interfaces

		public void TestDocAddresses()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.Address.E2_OA_Address = Helper.CreateOrganisation("ABCSYD").MainAddress.PK;
			AssertContainsExactElementsInAnyOrder(new JobDocAddress[] { consolidation.Address }, consolidation.DocAddresses);
			Assert(consolidation.IsRegisteredEditableChildObject(consolidation.DocAddresses));
		}

		public void TestSupportedAddressTypes()
		{
			var transportBooking = Helper.CreateConsolidation();
			var iTransportBooking = (IDocAddresses)transportBooking;

			AssertContainsExactElementsInAnyOrder(new DocAddressType[]
			{
								DocAddressType.TransportCompanyDocumentaryAddress,
								DocAddressType.BookingPartyDocumentaryAddress
			}, iTransportBooking.SupportedAddressTypes);
		}

		public void TestCanDeleteAddress()
		{
			var transportBooking = Helper.CreateConsolidation();
			var iTransportBooking = (IDocAddresses)transportBooking;

			AssertEquals("Want to always have a Transport Company", false, iTransportBooking.CanDeleteAddress(transportBooking.Address));
		}

		public void TestGetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			var transportBooking = Helper.CreateConsolidation();
			var iTransportBooking = (IDocAddresses)transportBooking;

			AssertEquals(Env.Security.TransportJobMISCDetails, iTransportBooking.GetCanOverrideCheckpoint(null));
		}

		public void TestGetDocAddressRequirement(DocAddressType addressType)
		{
			// single-job consolidation

			var consolidation = (IDocAddresses)Helper.CreateConsolidation();
			var requirement = consolidation.GetDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress);

			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, requirement.DefaultDocAddressType);
			AssertEquals(ContactType.LocalTransport, requirement.DefaultContactType);
			AssertEquals(AddressType.OFC, requirement.DefaultAddressType);
			AssertEquals(true, requirement.SaveEvenIfBlank);
			AssertEquals(false, requirement.IsMandatory);

			// multi-job consolidation

			var consolidationMultiJob = (IDocAddresses)Helper.CreateConsolidationMultiJob();
			var consolidationMultiJobRequirement = consolidationMultiJob.GetDocAddressRequirement(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertEquals(true, consolidationMultiJobRequirement.IsMandatory);
		}

		public void TestPiggyBackedDocAddressValidation()
		{
			var consolidation = (IDocAddresses)Helper.CreateConsolidation();
			AssertNull(consolidation.PiggyBackedDocAddressValidation(null));
		}

		public void TestOrgHeaderAfterChange_FiresBookingValidation()
		{
			var transportCoABC = Helper.CreateOrganisation("ABC");
			var transportCoXYZ = Helper.CreateOrganisation("XYZ");
			var consolidation = Helper.CreateConsolidationMultiJob();
			var booking = consolidation.Bookings.AddNew();
			booking.Address.OrganisationPK = transportCoABC.PK;
			AssertEquals("Precondition.", 0, booking.RowErrors.Count());

			consolidation.Address.OrganisationPK = transportCoXYZ.PK;
			AssertEquals("Changing the Consolidation TransportCo should validate the Booking.", 1, booking.RowErrors.Count());
		}

		public void TestIJobCostingPlugIn_Properties()
		{
			IJobCostingPlugIn costingPlugIn = Helper.CreateConsolidation();

			AssertEquals("ConsolExchangeRate", 0m, costingPlugIn.ConsolExchangeRate);
			AssertEquals("IsMasterCollect", false, costingPlugIn.IsMasterCollect);
			AssertEquals("JK_UniqueConsignRef", "", costingPlugIn.JK_UniqueConsignRef);
			AssertEquals("TransportMode", "", costingPlugIn.TransportMode);
			AssertEquals("ContainerMode", "", costingPlugIn.ContainerMode);
			AssertEquals("ConsolType", "", costingPlugIn.ConsolType);
			AssertEquals("Module", ApportionmentMethodModules.TransportBooking, costingPlugIn.Module);
			AssertEquals("Direction", "", costingPlugIn.Direction);
			AssertEquals("ExchangeRateForCurrency", 0m, costingPlugIn.ExchangeRateForCurrency(null, ZGuid.Empty));
			AssertEquals("CostSupporter", typeof(DtbBookingConsolidationJobCostSupporter), costingPlugIn.CostSupporter.GetType());
			AssertEquals(costingPlugIn, ((DtbBookingConsolidationJobCostSupporter)costingPlugIn.CostSupporter).ConsolidationMultiJob);

			AssertNull("ConsolCurrency", costingPlugIn.ConsolCurrency);
			AssertNull("DischargePort", costingPlugIn.DischargePort);
			AssertNull("LoadPort", costingPlugIn.LoadPort);
			AssertNull("ProfitLossContainer", costingPlugIn.ProfitLossContainer);
			AssertNull("ReceivingAgent", costingPlugIn.ReceivingAgent);
			AssertNull("ReceivingAgentAPInvoicingParty", costingPlugIn.ReceivingAgentAPInvoicingParty);
			AssertNull("ReceivingAgentARInvoicingParty", costingPlugIn.ReceivingAgentARInvoicingParty);
			AssertNull("SendingAgent", costingPlugIn.SendingAgent);
			AssertNull("SendingAgentAPInvoicingParty", costingPlugIn.SendingAgentAPInvoicingParty);
			AssertNull("SendingAgentARInvoicingParty", costingPlugIn.SendingAgentARInvoicingParty);
			AssertEquals("PrepaidCollectList", new CodeDescriptionPairList(), costingPlugIn.PrepaidCollectList);
		}

		public void TestIEDocsProvider_GetEDocsProviderSupporter()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var iBookingConsolidationEDocsProvider = (IEDocsProvider)bookingConsolidation;
			AssertNotNull(iBookingConsolidationEDocsProvider.GetEDocsProviderSupporter());
			AssertEquals(typeof(EDocsProviderSupporter), iBookingConsolidationEDocsProvider.GetEDocsProviderSupporter().GetType());
		}

		public void TestIDocumentSupportable_DocumentSupporter()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var iBookingConsolidationDocumentSupportable = (IDocumentSupportable)bookingConsolidation;
			AssertNotNull(iBookingConsolidationDocumentSupportable.DocumentSupporter);
			AssertEquals(typeof(DtbBookingConsolidationDocumentSupporter), iBookingConsolidationDocumentSupportable.DocumentSupporter.GetType());
		}

		public void TestIDocManagerSupport_DocManagerInfo()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var iBookingConsolidationDocManager = (IDocManagerSupport)bookingConsolidation;
			AssertNotNull(iBookingConsolidationDocManager.DocManagerInfo);
			AssertEquals(typeof(DtbBookingConsolidationDocManagerInfo), iBookingConsolidationDocManager.DocManagerInfo.GetType());
			AssertEquals(Constants.DocManagerCodes.DomesticTransportBookingConsolidation, iBookingConsolidationDocManager.DocManagerInfo.DocManagerCode);
		}

		public void TestIDtbBookingConsolidation_Bookings()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var iBookingConsolidation = (IDtbBookingConsolidation)bookingConsolidation;
			AssertEquals("IDtbBookingConsolidation_Bookings None", 0, iBookingConsolidation.Bookings.Length);

			var booking1 = bookingConsolidation.Bookings.AddNew();
			var booking2 = bookingConsolidation.Bookings.AddNew();
			var randomBooking = Factory.New<DtbBooking>();
			AssertContainsExactElementsInAnyOrder("IDtbBookingConsolidation_Bookings 2", new[] { booking1, booking2 }, iBookingConsolidation.Bookings);
		}

		public void TestTransportParentCommonTypeCode()
		{
			var consolidation = Helper.CreateConsolidation();
			var common = (ITransportParentCommon)consolidation;
			AssertEquals(Constants.TransportParentTypes.TransportBooking, common.TypeCode);
		}

		public void TestTransportParentCoreTypeCode()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobID = "ConsolID123";

			var core = (ITransportParentCore)consolidation;
			AssertEquals("", core.BillOfLading);
			AssertEquals("", core.ConsignmentRef);
			AssertEquals("", core.ContainerMode);
			AssertEquals("ConsolID123", core.Description);
			AssertEquals(Env.Security.RoadDistanceCalculationServiceLandTransport, core.DistanceCalculationCheckpoint);
			AssertEquals("", core.TransportMode);

			var booking1 = consolidation.Bookings.AddNew();
			booking1.KM_JobID = "Booking123";
			booking1.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "Master456");
			booking1.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "Master123");
			booking1.KM_TransportReference = "Transport123";
			AssertEquals("Master123", core.BillOfLading);
			AssertEquals("Transport123", core.ConsignmentRef);
			AssertEquals("Booking123", core.Description);

			var booking2 = consolidation.Bookings.AddNew();
			booking2.KM_JobID = "Booking456";
			booking2.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "Master789");
			booking2.ConsolidationSingleJob.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, "Master456");
			booking2.KM_TransportReference = "Transport456";
			AssertEquals("Master123", core.BillOfLading);
			AssertEquals("Transport123", core.ConsignmentRef);
			AssertEquals("ConsolID123", core.Description);
		}

		public void TestEntryTypeShouldBeUnique_ReturnsTrueForTrueRegistryValue()
		{
			var referenceNumbersCollection = TransportRegistry.Instance.AdditionalReferenceNumbers.Value;
			((TransportReferenceNumberType)referenceNumbersCollection.FindByCode(AdditionalReferenceTypes.Codes.OrderNumber)).IsUnique = true;

			using (TransportRegistry.Instance.AdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbersCollection))
			{
				var booking = Factory.NewWithValidTestData<DtbBookingConsolidation>();
				AssertEquals(true, booking.EntryTypeShouldBeUnique(AdditionalReferenceTypes.Codes.OrderNumber, "OTH", "AU"));
			}
		}

		public void TestEntryTypeShouldBeUnique_ReturnsFalseForFalseRegistryValue()
		{
			var referenceNumbersCollection = TransportRegistry.Instance.AdditionalReferenceNumbers.Value;
			((TransportReferenceNumberType)referenceNumbersCollection.FindByCode(AdditionalReferenceTypes.Codes.OrderNumber)).IsUnique = false;

			using (TransportRegistry.Instance.AdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbersCollection))
			{
				var booking = Factory.NewWithValidTestData<DtbBookingConsolidation>();
				AssertEquals(false, booking.EntryTypeShouldBeUnique(AdditionalReferenceTypes.Codes.OrderNumber, "OTH", "AU"));
			}
		}

		public void TestWhenEntryTypeShouldBeUniqueIsFalse_ShouldAllowDuplicateEntries()
		{
			var referenceNumbersCollection = TransportRegistry.Instance.AdditionalReferenceNumbers.Value;
			((TransportReferenceNumberType)referenceNumbersCollection.FindByCode(AdditionalReferenceTypes.Codes.OrderNumber)).IsUnique = false;

			using (TransportRegistry.Instance.AdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbersCollection))
			{
				var booking = Factory.NewWithValidTestData<DtbBookingConsolidation>();

				var entryNum1 = booking.AdditionalReferenceNumbers.AddNew();
				entryNum1.CE_EntryType = AdditionalReferenceTypes.Codes.OrderNumber;
				entryNum1.CE_EntryNum = "AAA";

				var entryNum2 = booking.AdditionalReferenceNumbers.AddNew();
				entryNum2.CE_EntryType = AdditionalReferenceTypes.Codes.OrderNumber;
				entryNum2.CE_EntryNum = "BBB";

				Factory.Save();

				AssertNoNotifications(entryNum2.CE_EntryTypeInfo);
			}
		}

		public void TestWhenEntryTypeShouldBeUniqueIsFalse_ShouldNotAllowDuplicateEntries()
		{
			var referenceNumbersCollection = TransportRegistry.Instance.AdditionalReferenceNumbers.Value;
			((TransportReferenceNumberType)referenceNumbersCollection.FindByCode(AdditionalReferenceTypes.Codes.OrderNumber)).IsUnique = true;

			using (TransportRegistry.Instance.AdditionalReferenceNumbers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, referenceNumbersCollection))
			{
				var booking = Factory.NewWithValidTestData<DtbBookingConsolidation>();

				var entryNum1 = booking.AdditionalReferenceNumbers.AddNew();
				entryNum1.CE_EntryType = AdditionalReferenceTypes.Codes.OrderNumber;
				entryNum1.CE_EntryNum = "AAA";

				var entryNum2 = booking.AdditionalReferenceNumbers.AddNew();
				entryNum2.CE_EntryType = AdditionalReferenceTypes.Codes.OrderNumber;
				entryNum2.CE_EntryNum = "BBB";

				Factory.Save();

				AssertHasError(entryNum2.CE_EntryTypeInfo, "The Number Type has been duplicated and must be unique.");
			}
		}

		public void TestAdditionalReferenceNumbers_CE_EntryType_SystemValue_ShouldAlwaysBeValid()
		{
			var list = new TransportReferenceNumberTypeCollection
			{
				{ "AAA", (NoResString)"A Desc" },
				{ "BBB", (NoResString)"B Desc" },
			};

			TransportRegistry.Instance.AdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var multiConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			multiConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.BookingTransportConsolidation;
			var entryNum1 = multiConsolidation.AdditionalReferenceNumbers.AddNew();
			entryNum1.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.CarrierBookingReference;
			entryNum1.CE_EntryNum = "123";

			Factory.Save();

			AssertNoNotifications("Should not have any notifications about a system value CE_EntryType", entryNum1.CE_EntryTypeInfo);
		}

		public void TestIAdditionalReferenceNumberTypeProvider()
		{
			var consolidation = Helper.CreateConsolidation();

			var additionalReferenceNumberLookups = consolidation.AdditionalReferenceNumbers.AddNew().Lookups;
			var additionalReferenceNumberTypes = additionalReferenceNumberLookups.GetType().GetProperty("AdditionalReferenceNumberTypes").GetValue(additionalReferenceNumberLookups, null);

			AssertContainsExactElementsInAnyOrder(
				"Should have correct additional reference number types",
				new[]
				{
					"ETB|External Transport Booking Number|Y",
					"TRF|Transport Reference Number|Y",
					"CIN|Commercial Invoice Number|Y",
					"CLR|Customer Reference Number|Y",
					"HSB|House Bill|Y",
					"MAB|Master Bill|N",
					"ORD|Order Number|N",
					"BPR|Booking Party Reference|Y",
					"CLN|Client|Y",
					"UCR|External (3rd Party) Unique Consignment Reference|Y",
					"CBK|Carrier Booking Reference|Y",
				},
				((CodeDescriptionPairList)additionalReferenceNumberTypes)
				.Cast<TransportReferenceNumberType>()
				.Select((number) => String.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
				.ToArray());
		}

		public void TestIRelatedJob()
		{
			var consolidation = Helper.CreateConsolidation();
			Factory.Save();
			var relatedJob = (IRelatedJob)consolidation;
			AssertEquals("PK", consolidation.PK, relatedJob.BusinessObjectPK);
			AssertEquals("ControllerID", ControllerIDs.DtbBookingConsolidation, relatedJob.ControllerID);
			AssertEquals("Transport Booking Consolidation CM00000001", relatedJob.JobDescription);
			AssertEquals(consolidation.KB_JobID, relatedJob.JobNumber);
			AssertEquals("JobStatus", TransportStatuses.Descriptions.Available, relatedJob.JobStatus);
		}

		public void TestGetBusinessObjectBaseTypeFromTablePrefix()
		{
			var prefix = DtbBookingConsolidationSchema.Constants.Prefix;
			var businessObjectType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix, false);
			AssertEquals("Should supply correct type based on the table prefix", typeof(DtbBookingConsolidation), businessObjectType);
			AssertNotEquals("Should not supply deprecated abstract type", typeof(DtbTransportConsolidation), businessObjectType);
		}

		public void TestGetTypeFromObjectFactory()
		{
			var suppliedType = typeof(IDtbTransportConsolidation);
			var expectedType = typeof(DtbBookingConsolidation);
			var actualType = ObjectFactory.GetType(suppliedType);
			AssertEquals("Should be able to get the correct type from the interface", expectedType, actualType);
		}

		public void TestCreateFromInterface()
		{
			var type = typeof(IDtbTransportConsolidation);
			var businessObject = Factory.New(ObjectFactory.GetType(type));
			var expectedType = typeof(DtbBookingConsolidation);
			AssertEquals("Should create correct business object when using Factory.New", expectedType, businessObject.GetType());
		}

		public void TestIsSub()
		{
			var consolidation = Helper.CreateConsolidation();

			AssertEquals("Precondition: KB_KB_MasterBookingConsolidation should be an empty Guid.", ZGuid.Empty, consolidation.KB_KB_MasterBookingConsolidation);
			AssertEquals("IsSub should return false.", false, consolidation.IsSub);

			consolidation.KB_KB_MasterBookingConsolidation = ZGuid.NewZGuid();
			AssertEquals("IsSub should return true.", true, consolidation.IsSub);
		}

		public void TestMasterBookingConsolidation()
		{
			var consolidation = Helper.CreateConsolidation();
			Factory.Save();

			AssertEquals("Precondition: KB_KB_MasterBookingConsolidation should be an empty Guid.", ZGuid.Empty, consolidation.KB_KB_MasterBookingConsolidation);
			AssertEquals("MasterBookingConsolidation should return null.", null, consolidation.MasterBookingConsolidation);

			var masterBookingConsolidation1 = Helper.CreateConsolidation();
			masterBookingConsolidation1.KB_IsMaster = true;
			masterBookingConsolidation1.KB_MasterBookingVersion = 1;
			consolidation.KB_KB_MasterBookingConsolidation = masterBookingConsolidation1.PK;
			consolidation.KB_MasterBookingVersion = 1;
			Factory.Save();

			AssertEquals("MasterBookingConsolidation should return the master consolidation.", masterBookingConsolidation1.PK, consolidation.MasterBookingConsolidation.PK);

			var masterBookingConsolidation2 = Helper.CreateConsolidation();
			masterBookingConsolidation2.KB_IsMaster = true;
			masterBookingConsolidation2.KB_MasterBookingVersion = 1;
			consolidation.KB_KB_MasterBookingConsolidation = masterBookingConsolidation2.PK;
			Factory.Save();

			AssertEquals("MasterBookingConsolidation should return the second master consolidation.", masterBookingConsolidation2.PK, consolidation.MasterBookingConsolidation.PK);

			consolidation.KB_KB_MasterBookingConsolidation = ZGuid.Empty;

			AssertEquals("MasterBookingConsolidation should return null.", null, consolidation.MasterBookingConsolidation);
		}

		public void TestSubBookingConsolidationUpdatesMasterBookingVersionIfNewAndNotYetSet()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = (short)1;
			Factory.Save();

			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;

			AssertEquals("Precondition: sub booking consolidation MasterBookingVersion has not yet been set (still 0)", ZShort.Zero, subConsolidation.KB_MasterBookingVersion);

			Factory.Save();

			AssertEquals("Sub booking consolidation MasterBookingVersion should have updated to 1", (short)1, subConsolidation.KB_MasterBookingVersion);
		}

		public void TestSubBookingConsolidationDoesNotUpdateMasterBookingVersionIfNewButAlreadySet()
		{
			var masterConsolidation = Helper.CreateConsolidation();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = (short)1;
			Factory.Save();

			masterConsolidation.KB_MasterBookingVersion = (short)2;
			var subConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation.KB_MasterBookingVersion = (short)2;
			Factory.Save();

			AssertEquals("Sub booking consolidation MasterBookingVersion should have stayed at 2", (short)2, subConsolidation.KB_MasterBookingVersion);
		}

		public void TestDelete_ShouldNotDelete_WhenSubConsolidationsExist()
		{
			var subConsolidation = Helper.CreateConsolidation();
			var masterConsolidation = Helper.CreateConsolidation();
			subConsolidation.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;

			Factory.Save();

			masterConsolidation.Delete();

			AssertEquals(false, masterConsolidation.IsDeleted);
		}
	}

	[TestedType(typeof(DtbBookingConsolidation))]
	sealed class DtbBookingConsolidationMasterBookingEntityTest : BaseIDtbMasterBookingEntityTest
	{
		protected override IEnumerable<SchemaColumn> ReplicatedColumns => DtbMasterBookingReplication.DtbBookingConsolidationReplicatedColumns;
		protected override IEnumerable<SchemaColumn> NonReplicatedColumns => new SchemaColumn[]
		{
			DtbBookingConsolidationSchema.PK,
			DtbBookingConsolidationSchema.KB_SystemCreateUser,
			DtbBookingConsolidationSchema.KB_SystemCreateBranch,
			DtbBookingConsolidationSchema.KB_SystemCreateDepartment,
			DtbBookingConsolidationSchema.KB_SystemCreateTimeUtc,
			DtbBookingConsolidationSchema.KB_SystemLastEditUser,
			DtbBookingConsolidationSchema.KB_SystemLastEditTimeUtc,
			DtbBookingConsolidationSchema.KB_IsMaster,
			DtbBookingConsolidationSchema.KB_MasterBookingVersion,
			DtbBookingConsolidationSchema.KB_KB_MasterBookingConsolidation,
			DtbBookingConsolidationSchema.KB_JobID,
			DtbBookingConsolidationSchema.KB_JobType,
			DtbBookingConsolidationSchema.KB_ParentID,
			DtbBookingConsolidationSchema.KB_ParentTableCode,
			DtbBookingConsolidationSchema.KB_GoodsDescription,
			DtbBookingConsolidationSchema.KB_IsOverridden,
			DtbBookingConsolidationSchema.KB_Status,
		};

		protected override ITableSchema tableSchema => DtbBookingConsolidationSchema.Instance;

		public void TestMasterBookingConsolidationUpdateToJobDirectionUpdatesMasterBookingVersion()
		{
			CoreTestMasterBookingConsolidationUpdateToReplicationFieldUpdatesMasterBookingVersion(DtbBookingConsolidationSchema.KB_JobDirection, "DST");
		}

		public void TestMasterBookingConsolidationUpdateToStatusDoesNotUpdateMasterBookingVersion()
		{
			CoreTestMasterBookingConsolidationUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(DtbBookingConsolidationSchema.KB_Status, BookingConsolidationStatuses.Codes.Incomplete);
		}

		public void TestMasterBookingUpdateToJobIDDoesNotUpdateMasterBookingVersion()
		{
			CoreTestMasterBookingConsolidationUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(DtbBookingConsolidationSchema.KB_JobID, "XXX2");
		}

		public void TestMasterBookingConsolidationUpdateToGoodsDescriptionDoesNotUpdateMasterBookingVersion()
		{
			CoreTestMasterBookingConsolidationUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(DtbBookingConsolidationSchema.KB_GoodsDescription, "Some new goods description");
		}

		public void TestMasterBookingConsolidationUpdateToIsOverriddenDoesNotUpdateMasterBookingVersion()
		{
			CoreTestMasterBookingConsolidationUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(DtbBookingConsolidationSchema.KB_IsOverridden, true);
		}

		void CoreTestMasterBookingConsolidationUpdateToReplicationFieldUpdatesMasterBookingVersion(SchemaColumn columnChanged, object updatedValue)
		{
			var bookingConsolidation = CreateBookingConsolidationForMasterBookingReplicationTests(true);

			var previousMasterBookingVersion = bookingConsolidation.KB_MasterBookingVersion;

			bookingConsolidation[columnChanged] = updatedValue;
			var propertyInfo = bookingConsolidation.FindPropertyInfo(columnChanged.Name);
			CombineAssertions("Preconditions for master booking version update", () =>
			{
				Assert("Precondition: BookingConsolidation.HasChanges is true", bookingConsolidation.HasChanges);
				Assert("Precondition: BookingConsolidation." + columnChanged.Name + " value has changed", propertyInfo.HasChanges);
			});
			Factory.Save();

			AssertGreaterThan("On a Master Booking Consolidation, update to " + columnChanged.Name + " should update master booking version", bookingConsolidation.KB_MasterBookingVersion, previousMasterBookingVersion);
		}

		void CoreTestMasterBookingConsolidationUpdateToNonReplicationFieldDoesNotUpdateMasterBookingVersion(SchemaColumn columnChanged, object updatedValue)
		{
			var bookingConsolidation = CreateBookingConsolidationForMasterBookingReplicationTests(true);

			var previousMasterBookingVersion = bookingConsolidation.KB_MasterBookingVersion;

			bookingConsolidation[columnChanged] = updatedValue;

			var propertyInfo = bookingConsolidation.FindPropertyInfo(columnChanged.Name);
			CombineAssertions("Preconditions for master booking version update", () =>
			{
				Assert("Precondition: BookingConsolidation.HasChanges is true", bookingConsolidation.HasChanges);
				Assert("Precondition: BookingConsolidation." + columnChanged.Name + " value has changed", propertyInfo.HasChanges);
			});
			Factory.Save();

			AssertEquals("On a Master Booking Consolidation, update to " + columnChanged.Name + " should not update master booking version", previousMasterBookingVersion, bookingConsolidation.KB_MasterBookingVersion);
		}

		public void TestNonMasterBookingUpdatesDoNotUpdateMasterBookingVersion()
		{
			var bookingConsolidation = CreateBookingConsolidationForMasterBookingReplicationTests(false);

			AssertEquals("Precondition: Non-master booking consolidation should have KB_MasterBookingVersion of 0", (short)0, bookingConsolidation.KB_MasterBookingVersion);

			bookingConsolidation.KB_JobID = "XXX2";
			bookingConsolidation.KB_JobDirection = "DST";
			bookingConsolidation.KB_Status = BookingConsolidationStatuses.Codes.Incomplete;
			bookingConsolidation.KB_IsOverridden = true;
			bookingConsolidation.KB_GoodsDescription = "Some new goods description";

			Assert("Precondition: Booking.HasChanges is true", bookingConsolidation.HasChanges);
			Factory.Save();

			AssertEquals("On a Non-master booking, any updates should leave master booking version at 0", (short)0, bookingConsolidation.KB_MasterBookingVersion);
		}

		DtbBookingConsolidation CreateBookingConsolidationForMasterBookingReplicationTests(bool isMaster)
		{
			var bookingConsolidation = Helper.CreateConsolidation();
			bookingConsolidation.KB_IsMaster = isMaster;
			bookingConsolidation.KB_MasterBookingVersion = (short)(isMaster ? 1 : 0);
			bookingConsolidation.KB_JobID = "XXX1";
			bookingConsolidation.KB_JobDirection = "ORG";
			bookingConsolidation.KB_Status = BookingConsolidationStatuses.Codes.Available;
			bookingConsolidation.KB_IsOverridden = false;
			bookingConsolidation.KB_GoodsDescription = string.Empty;
			bookingConsolidation.KB_JobType = "BKG";
			Factory.Save();

			return bookingConsolidation;
		}
	}

	public class DtbBookingConsolidationSingleJobNumberFountainTest : DtbBookingConsolidationNumberFountainTest
	{
		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.DtbBookingConsolidationID; }
		}

		protected override ZString ConsolidationJobType
		{
			get { return TransportConsolidationJobTypes.Codes.Booking; }
		}
	}

	public class DtbBookingConsolidationMultiJobNumberFountainTest : DtbBookingConsolidationNumberFountainTest
	{
		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.DtbBookingConsolidationMultiJobID; }
		}

		protected override ZString ConsolidationJobType
		{
			get { return TransportConsolidationJobTypes.Codes.BookingTransportConsolidation; }
		}
	}

	public abstract class DtbBookingConsolidationNumberFountainTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest
		{
			get { return typeof(DtbBookingConsolidation); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return DtbBookingConsolidationSchema.KB_JobID; }
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject bizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(bizO);

			var consolidation = (DtbBookingConsolidation)bizO;
			consolidation.KB_JobType = ConsolidationJobType;
		}

		protected abstract ZString ConsolidationJobType { get; }
	}

	[TestedType(typeof(DtbBookingConsolidation))]
	public class DtbBookingConsolidationWorkflowProviderTest : WorkflowProviderTest<DtbBookingConsolidation, DtbBookingConsolidationProcessTaskCollection>
	{
		protected override DtbBookingConsolidation GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return Helper.CreateConsolidation();
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode; }
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
