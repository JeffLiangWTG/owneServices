using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Freight.Integration.CFS;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using BusinessContext = Enterprise.Integration.Accounting.BusinessContext;
using Constants = Enterprise.Core.Constants;
using OrgCodes = Enterprise.MasterFiles.Business.AccountingMasterFilesConstants.OrganisationTypeCodes;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartage))]
	public class CommonCartageBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonCartage);
			}
		}

		// CartageInternalType tests which test CartageType.GetCartageType()
		public void TestCartageInternalTypeMatchesJobTypeFirst()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType1 = new DummyCartageType(dummyParent);
			cartageType1.SetCartageJobType("AAA");
			var cartageType2 = new DummyCartageType(dummyParent);
			cartageType2.SetCartageJobType("BBB");
			dummyParent.SetCartageTypes(cartageType1, cartageType2);
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.SetParent(dummyCartageParent);
			cartage1.JJ_E3_NKJobType = "AAA";
			AssertEquals("Should match cartage on CartageJobType 'AAA'", cartageType1, cartage1.CartageInternalType);
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.SetParent(dummyCartageParent);
			cartage2.JJ_E3_NKJobType = "BBB";
			AssertEquals("Should match cartage on CartageJobType 'BBB'", cartageType2, cartage2.CartageInternalType);
		}

		public void TestCartageInternalTypeMatchesDirectionIfNoMatchOnJobType()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType1 = new DummyCartageType(dummyParent, new ZString[] { "EXP", "ORG" });
			cartageType1.SetCartageJobType("AAA");
			var cartageType2 = new DummyCartageType(dummyParent, new ZString[] { "IMP", "DST" });
			cartageType2.SetCartageJobType("BBB");
			dummyParent.SetCartageTypes(cartageType1, cartageType2);
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.SetParent(dummyCartageParent);
			cartage1.JJ_Direction = "EXP";
			AssertEquals("Should match cartage on CartageJobType 'EXP'", cartageType1, cartage1.CartageInternalType);
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.SetParent(dummyCartageParent);
			cartage2.JJ_Direction = "DST";
			AssertEquals("Should match cartage on CartageDirection 'DST'", cartageType2, cartage2.CartageInternalType);
		}

		public void TestCartageInternalTypeMatchesSingleCartageType()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = new DummyCartageType(dummyParent, new ZString[] { "EXP", "ORG" });
			cartageType.SetCartageJobType("AAA");
			dummyParent.SetCartageType(cartageType);
			var cartage = Factory.New<CommonCartage>();
			cartage.SetParent(dummyCartageParent);
			AssertEquals("Should match single CartageType", cartageType, cartage.CartageInternalType);
		}

		public void TestCartageInternalTypeReturnsNullIfNoOtherMatch()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType1 = new DummyCartageType(dummyParent, new ZString[] { "EXP", "ORG" });
			cartageType1.SetCartageJobType("AAA");
			var cartageType2 = new DummyCartageType(dummyParent, new ZString[] { "IMP", "DST" });
			cartageType2.SetCartageJobType("BBB");
			dummyParent.SetCartageTypes(cartageType1, cartageType2);
			var cartage = Factory.New<CommonCartage>();
			cartage.SetParent(dummyCartageParent);
			cartage.JJ_E3_NKJobType = "ZZZ";
			cartage.JJ_Direction = "XXX";
			AssertNull("Should return null for no match", cartage.CartageInternalType);
		}

		public void TestJJ_Direction()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(Constants.CartageDirection.Import, cartage.JJ_Direction);
			cartage.JJ_E3_NKJobType = "EALL";
			AssertEquals(Constants.CartageDirection.Export, cartage.JJ_Direction);
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(Constants.CartageDirection.Local, cartage.JJ_Direction);
		}

		public void TestJJ_ContainerMode()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
			cartage.JJ_E3_NKJobType = "EALL";
			AssertEquals(Constants.CartageContainerMode.Loose, cartage.JJ_ContainerMode);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			AssertEquals(Constants.CartageContainerMode.Mixed, cartage.JJ_ContainerMode);
		}

		public void TestJJ_ContainerMode_ChangeToContainerModeRemovesAllLooseBookedMoves()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			var legContainer = containerMove.CartageLegs.AddNew();
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { looseMove, containerMove }, cartage.BookedMovesCollection);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			AssertContainsExactElementsInAnyOrder(new[] { containerMove }, cartage.BookedMovesCollection);
			AssertContainsExactElementsInAnyOrder(new[] { legContainer }, cartage.CartageLegs);
		}

		public void TestJJ_ContainerMode_ChangeToLooseModeRemovesAllContainerBookedMoves()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var legLoose = looseMove.CartageLegs.AddNew();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { looseMove, containerMove }, cartage.BookedMovesCollection);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			AssertContainsExactElementsInAnyOrder(new[] { looseMove }, cartage.BookedMovesCollection);
			AssertContainsExactElementsInAnyOrder(new[] { legLoose }, cartage.CartageLegs);
		}

		public void TestJJ_ContainerMode_RemovesLegsUnsupportedByContainerMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var legLoose = looseMove.CartageLegs.AddNew();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			var legContainer = containerMove.CartageLegs.AddNew();
			AssertContainsExactElementsInAnyOrder("Precondition - legs", new[] { legLoose, legContainer }, cartage.CartageLegs);
			AssertContainsExactElementsInAnyOrder("Precondition - moves", new[] { containerMove, looseMove }, cartage.BookedMovesCollection);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			AssertContainsExactElementsInAnyOrder(new[] { legLoose }, cartage.CartageLegs);
			AssertContainsExactElementsInAnyOrder(new[] { looseMove }, cartage.BookedMovesCollection);
			containerMove = cartage.ContainerBookedMoves.AddNew();
			legContainer = containerMove.CartageLegs.AddNew();
			AssertContainsExactElementsInAnyOrder("Precondition - legs", new[] { legLoose, legContainer }, cartage.CartageLegs);
			AssertContainsExactElementsInAnyOrder("Precondition - moves", new[] { containerMove, looseMove }, cartage.BookedMovesCollection);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			AssertContainsExactElementsInAnyOrder(new[] { legContainer }, cartage.CartageLegs);
			AssertContainsExactElementsInAnyOrder(new[] { containerMove }, cartage.BookedMovesCollection);
		}

		public void TestJJ_ContainerMode_RemovesLegsUnsupportedByContainerModeDoesNotThrowIfMoveAlreadyDeleted()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			var containerLeg = containerMove.CartageLegs.AddNew();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			var looseLeg1 = looseMove.CartageLegs.AddNew();
			var looseLeg2 = looseMove.CartageLegs.AddNew();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			AssertCollectionContains(containerLeg, cartage.CartageLegs);
			AssertCollectionNotContains(looseLeg1, cartage.CartageLegs);
			AssertCollectionNotContains(looseLeg2, cartage.CartageLegs);
		}

		public void TestJJ_ShippingTransportMode()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(Constants.TransportModes.Sea, cartage.JJ_ShippingTransportMode);
			cartage.JJ_E3_NKJobType = "EALL";
			AssertEquals(Constants.TransportModes.Air, cartage.JJ_ShippingTransportMode);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Rail;
			AssertEquals(Constants.TransportModes.Rail, cartage.JJ_ShippingTransportMode);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestJJ_ConsignmentID_UpdatesJobsJobNumb_WhenChanges()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "Test ID";
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company1.Branches.AddNew();
			branch1.GB_RL_NKHomePort = "PLGDN";
			branch1.GB_Code = "GDN";
			var job1 = new JobHeader.Loader(cartage).TryCreate(branch1);
			AssertEquals(job1.JH_JobNum, cartage.JJ_ConsignmentID);
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = company2.Branches.AddNew();
			branch2.GB_RL_NKHomePort = "HKHGK";
			branch2.GB_Code = "HGK";
			var job2 = new JobHeader.Loader(cartage).TryCreate(branch2);
			AssertEquals(job2.JH_JobNum, cartage.JJ_ConsignmentID);
			AssertNotEquals(job1.PK, job2.PK);
			var cartageTest = Factory.New<CommonCartage>();
			cartageTest.JJ_ConsignmentID = "Test ID";
			var jobTest = new JobHeader.Loader(cartageTest).TryLoadOrCreate();
			AssertEquals(jobTest.JH_JobNum, "Test ID");
			cartage.JJ_ConsignmentID = "New ID";
			Factory.Save();
			AssertEquals("New ID", cartage.JJ_ConsignmentID);
			AssertEquals("Job Num should have been updated.", "New ID", job1.JH_JobNum);
			AssertEquals("Job Num should have been updated.", "New ID", job2.JH_JobNum);
			AssertEquals("Job links to different cartage should not get updated.", "Test ID", jobTest.JH_JobNum);
		}

		public void TestJJ_ConsignmentID_LogServiceCommenced()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "Test ID";
			((IConsignmentService)cartage).LogServicesCommenced();
			var log = cartage.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == AutoEvents.ServiceCommencedCode);
			AssertNotNull(log);
			AssertEquals("Test ID", log.ReferenceFreeText);
			AssertEquals("Transport", log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			cartage.JJ_ConsignmentID = "NewID";
			((IConsignmentService)cartage).LogServicesCommenced();
			var newLog = cartage.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_SE_NKEvent == AutoEvents.ServiceCommencedCode && l.PK != log.PK);
			AssertEquals("NewID", newLog.ReferenceFreeText);
			AssertEquals("Transport", newLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
		}

		public void TestJJ_ConsignmentID_TooLongForJobNum()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = (ZString)"TooLongForJH_JobNum".PadRight(cartage.JJ_ConsignmentIDInfo.MaxLength, '1');
			var testJob = new JobHeader.Loader(cartage).TryLoadOrCreate();
			AssertNotNull(testJob);
			AssertEquals("JH_JobNum Truncated", cartage.JJ_ConsignmentID.SubstringSafe(0, testJob.JH_JobNumInfo.MaxLength), testJob.JH_JobNum);
		}

		public void TestJJ_ConsignmentID_PopulatedOnSaveEvenWhenHasNonTBParent()
		{
			var dummyCartageParent = new DummyCartageParent(Factory);
			var cartage = Factory.New<CommonCartage>();
			cartage.SetParent(dummyCartageParent);
			Factory.Save();
			AssertNotNullOrEmpty("Consignment ID of cartage with non-TB parent should be populated", cartage.JJ_ConsignmentID);
		}

		public void TestIsAir()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, cartage.IsAir);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			AssertEquals(true, cartage.IsAir);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Rail;
			AssertEquals(false, cartage.IsAir);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Road;
			AssertEquals(false, cartage.IsAir);
		}

		public void TestIsSea()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Sea;
			AssertEquals(true, cartage.IsSea);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			AssertEquals(false, cartage.IsSea);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Rail;
			AssertEquals(false, cartage.IsSea);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Road;
			AssertEquals(false, cartage.IsSea);
		}

		public void TestIsRoad()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, cartage.IsRoad);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			AssertEquals(false, cartage.IsRoad);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Rail;
			AssertEquals(false, cartage.IsRoad);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Road;
			AssertEquals(true, cartage.IsRoad);
		}

		public void TestIsRail()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, cartage.IsRail);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			AssertEquals(false, cartage.IsRail);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Rail;
			AssertEquals(true, cartage.IsRail);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Road;
			AssertEquals(false, cartage.IsRail);
		}

		public void TestJobDirection()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Import, cartage.JobDirection);
			cartage.JJ_Direction = Constants.CartageDirection.Destination;
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Domestic, cartage.JobDirection);
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Export, cartage.JobDirection);
			cartage.JJ_Direction = Constants.CartageDirection.Origin;
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Domestic, cartage.JobDirection);
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Domestic, cartage.JobDirection);
			cartage.JJ_Direction = Constants.CartageDirection.LineHaul;
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Domestic, cartage.JobDirection);
			cartage.JJ_Direction = "RND";
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Unknown, cartage.JobDirection);
		}

		public void TestIsImportOrDestination()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(true, cartage.IsImportOrDestination);
			cartage.JJ_Direction = Constants.CartageDirection.Destination;
			AssertEquals(true, cartage.IsImportOrDestination);
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			AssertEquals(false, cartage.IsImportOrDestination);
			cartage.JJ_Direction = Constants.CartageDirection.LineHaul;
			AssertEquals(false, cartage.IsImportOrDestination);
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(false, cartage.IsImportOrDestination);
		}

		public void TestIsExportOrOrigin()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			AssertEquals(true, cartage.IsExportOrOrigin);
			cartage.JJ_Direction = Constants.CartageDirection.Origin;
			AssertEquals(true, cartage.IsExportOrOrigin);
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(false, cartage.IsExportOrOrigin);
			cartage.JJ_Direction = Constants.CartageDirection.LineHaul;
			AssertEquals(false, cartage.IsExportOrOrigin);
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(false, cartage.IsExportOrOrigin);
		}

		public void TestIsImport()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(true, cartage.IsImport);
			cartage.JJ_Direction = Constants.CartageDirection.Destination;
			AssertEquals(false, cartage.IsImport);
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			AssertEquals(false, cartage.IsImport);
			cartage.JJ_Direction = Constants.CartageDirection.LineHaul;
			AssertEquals(false, cartage.IsImport);
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(false, cartage.IsImport);
		}

		public void TestIsExport()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			AssertEquals(true, cartage.IsExport);
			cartage.JJ_Direction = Constants.CartageDirection.Origin;
			AssertEquals(false, cartage.IsExport);
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(false, cartage.IsExport);
			cartage.JJ_Direction = Constants.CartageDirection.LineHaul;
			AssertEquals(false, cartage.IsExport);
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(false, cartage.IsExport);
		}

		public void TestIsDomestic()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(true, cartage.IsDomestic);
			cartage.JJ_Direction = Constants.CartageDirection.Origin;
			AssertEquals(true, cartage.IsDomestic);
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(false, cartage.IsDomestic);
			cartage.JJ_Direction = Constants.CartageDirection.LineHaul;
			AssertEquals(true, cartage.IsDomestic);
			cartage.JJ_Direction = Constants.CartageDirection.Destination;
			AssertEquals(true, cartage.IsDomestic);
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			AssertEquals(false, cartage.IsDomestic);
		}

		public void TestIsOrigin()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_Direction = Constants.CartageDirection.Origin;
			AssertEquals(true, cartage.IsOrigin);
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			AssertEquals(false, cartage.IsOrigin);
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(false, cartage.IsOrigin);
			cartage.JJ_Direction = Constants.CartageDirection.LineHaul;
			AssertEquals(false, cartage.IsOrigin);
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(false, cartage.IsOrigin);
		}

		public void TestIsDestination()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_Direction = Constants.CartageDirection.Destination;
			AssertEquals(true, cartage.IsDestination);
			cartage.JJ_Direction = Constants.CartageDirection.Export;
			AssertEquals(false, cartage.IsDestination);
			cartage.JJ_Direction = Constants.CartageDirection.Import;
			AssertEquals(false, cartage.IsDestination);
			cartage.JJ_Direction = Constants.CartageDirection.LineHaul;
			AssertEquals(false, cartage.IsDestination);
			cartage.JJ_Direction = Constants.CartageDirection.Local;
			AssertEquals(false, cartage.IsDestination);
		}

		public void TestIsMixed()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			AssertEquals(false, cartage.IsMixed);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			AssertEquals(false, cartage.IsMixed);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			AssertEquals(true, cartage.IsMixed);
		}

		public void TestIsContainerised()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			AssertEquals(false, cartage.IsContainerised);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			AssertEquals(true, cartage.IsContainerised);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			AssertEquals(true, cartage.IsContainerised);
		}

		public void TestIsLoose()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			AssertEquals(true, cartage.IsLoose);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			AssertEquals(false, cartage.IsLoose);
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			AssertEquals(true, cartage.IsLoose);
		}

		public void TestBookingParty()
		{
			var tbBookingParty = Factory.New<OrgHeader>();
			var parentBooking = Factory.New<IDtbBooking>();
			var parentBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			parentBooking.KM_KB_Booking = parentBookingConsolidation.PK;
			var cartage = Factory.New<CommonCartage>();
			AssertNull("Precondition.", cartage.BookingParty);
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			cartage.JJ_ParentID = parentBooking.PK;
			AssertEquals("Should return OrgProxy as there is a Booking, but no booking party, must have been created internally?", GlbCompany.CurrentCompany.OrgProxy.PK, cartage.BookingParty.PK);
			var iParentBookingConsolidation = (IDocAddresses)parentBookingConsolidation;
			iParentBookingConsolidation.DocAddresses.AddNew(tbBookingParty.MainAddress, DocAddressType.BookingPartyDocumentaryAddress);
			AssertEquals("Should return Booking Consols Booking Party.", tbBookingParty.PK, cartage.BookingParty.PK);
			var cartageBookingParty = Factory.New<OrgHeader>();
			cartage.DocAddresses.AddNew(cartageBookingParty.MainAddress, DocAddressType.BookingPartyDocumentaryAddress);
			AssertEquals("Should return overridden Cartage Booking Party.", cartageBookingParty.PK, cartage.BookingParty.PK);
		}

		public void TestShouldReorderAddresses()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(false, cartage.ShouldReorderAddresses);
			cartage.MarkAsNeededAddressReorder();
			AssertEquals(true, cartage.ShouldReorderAddresses);
			cartage.MarkAsHavingAddressesReordered();
			AssertEquals(false, cartage.ShouldReorderAddresses);
			var address = cartage.DocAddresses.AddNew();
			address.E2_AddressOverride = true;
			address.E2_Address1 = "hello";
			AssertEquals(true, cartage.ShouldReorderAddresses);
		}

		public void TestMakeAddressesPersistentButDeleteEmpty()
		{
			var cartage = Factory.New<CommonCartage>();
			var addressEmpty = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			var addressFull = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			addressFull.E2_AddressOverride = true;
			addressFull.E2_Address1 = "hello";
			cartage.MakeAddressesPersistentButDeleteEmpty();
			AssertEquals(true, addressEmpty.IsDeleted);
			AssertEquals(false, addressFull.IsDeleted);
		}

		public void TestResetMainAddressesForBinding()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var addressCTO = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCTO);
			var addressCFS = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			leg.JU_E2PickupAddressID = addressCTO.PK;
			leg.JU_E2DeliveryAddressID = addressCFS.PK;
			AssertEquals(addressCTO, cartage.FirstDocAddress);
			AssertEquals(addressCFS, cartage.SecondDocAddress);
			leg.JU_E2PickupAddressID = addressCFS.PK;
			leg.JU_E2DeliveryAddressID = addressCTO.PK;
			AssertEquals("Precondition - Doesn't change because strategy behaviour is not running", addressCTO, cartage.FirstDocAddress);
			AssertEquals("Precondition - Doesn't change because strategy behaviour is not running", addressCFS, cartage.SecondDocAddress);
			cartage.ResetMainAddressesForBinding();
			AssertEquals("Order Changed, and has updated FirstDocAddress", addressCFS, cartage.FirstDocAddress);
			AssertEquals("Order Changed, and has updated SecondDocAddress", addressCTO, cartage.SecondDocAddress);
		}

		public void TestLatestDeliveryDateIsPassedUpToCartageParent()
		{
			var year = ZDateTime.Now.Year;
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = new CompletedDummyCartageType(dummyParent);
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			var now = ZDateTime.Now;
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			leg1.JU_PickupTimeIn = new ZDateTime(year, 1, 1);
			leg1.JU_PickupTimeOut = new ZDateTime(year, 1, 1);
			leg1.JU_DeliverTimeIn = new ZDateTime(year, 2, 2);
			leg1.JU_DeliverTimeOut = new ZDateTime(year, 2, 2);
			var leg2 = move2.CartageLegs.AddNew();
			leg2.JU_PickupTimeIn = new ZDateTime(year, 1, 1);
			leg2.JU_PickupTimeOut = new ZDateTime(year, 1, 1);
			leg2.JU_DeliverTimeIn = new ZDateTime(year, 1, 1);
			leg2.JU_DeliverTimeOut = new ZDateTime(year, 1, 1);
			var importer = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			leg1.JU_E2PickupAddressID = importer.PK;
			cartage.DeliveryCompleted(leg1, DocAddressType.LocalCartageImporter, now);
			AssertEquals(DocAddressType.LocalCartageImporter, cartageType.addressType);
			AssertEquals(new ZDateTime(year, 2, 2), cartageType.timeOut);
			AssertEquals(1, cartageType.hit);
			leg2.JU_E2PickupAddressID = importer.PK;
			cartage.DeliveryCompleted(leg2, DocAddressType.LocalCartageImporter, now.AddDays(1));
			AssertEquals(DocAddressType.LocalCartageImporter, cartageType.addressType);
			AssertEquals(new ZDateTime(year, 2, 2), cartageType.timeOut);
			AssertEquals(2, cartageType.hit);
		}

		public void TestLatestPickupDateIsPassedUpToCartageParent()
		{
			var year = ZDateTime.Now.Year;
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = new CompletedDummyCartageType(dummyParent);
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			var now = ZDateTime.Now;
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			var leg1 = move1.CartageLegs.AddNew();
			leg1.JU_PickupTimeIn = new ZDateTime(year, 2, 2);
			leg1.JU_PickupTimeOut = new ZDateTime(year, 2, 2);
			leg1.JU_DeliverTimeIn = new ZDateTime(year, 1, 1);
			leg1.JU_DeliverTimeOut = new ZDateTime(year, 1, 1);
			var leg2 = move2.CartageLegs.AddNew();
			leg2.JU_PickupTimeIn = new ZDateTime(year, 1, 1);
			leg2.JU_PickupTimeOut = new ZDateTime(year, 1, 1);
			leg2.JU_DeliverTimeIn = new ZDateTime(year, 1, 1);
			leg2.JU_DeliverTimeOut = new ZDateTime(year, 1, 1);
			var importer = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			leg1.JU_E2PickupAddressID = importer.PK;
			cartage.PickupCompleted(leg1, DocAddressType.LocalCartageImporter, now);
			AssertEquals(DocAddressType.LocalCartageImporter, cartageType.addressType);
			AssertEquals(new ZDateTime(year, 2, 2), cartageType.timeOut);
			AssertEquals(1, cartageType.hit);
			leg2.JU_E2PickupAddressID = importer.PK;
			cartage.PickupCompleted(leg2, DocAddressType.LocalCartageImporter, now.AddDays(1));
			AssertEquals(DocAddressType.LocalCartageImporter, cartageType.addressType);
			AssertEquals(new ZDateTime(year, 2, 2), cartageType.timeOut);
			AssertEquals(2, cartageType.hit);
		}

		public void TestIsCancelled()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			cartage.IsRoot = true;
			var containerBookedMove = cartage.ContainerBookedMoves.First();
			AssertEquals("Precondition: Cartage should not be readonly", false, cartage.ReadOnly);
			AssertEquals("Precondition: Child Container should not be readonly", false, containerBookedMove.ReadOnly);
			cartage.JJ_IsCancelled = true;
			AssertEquals("Cartage is cancelled, so Cartage should be readonly", true, cartage.ReadOnly);
			AssertEquals("Cartage is cancelled, so Container should be readonly", true, containerBookedMove.ReadOnly);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cartage_newFactory = newFactory.Load<CommonCartage>(cartage.PK);
			cartage_newFactory.IsRoot = true;
			var containerBookedMove_newFactory = cartage_newFactory.ContainerBookedMoves.First();
			AssertEquals("Cartage is cancelled, so Cartage should be readonly when loaded in another factory", true, cartage_newFactory.ReadOnly);
			AssertEquals("Cartage is cancelled, so Container should be readonly when loaded in another factory", true, containerBookedMove_newFactory.ReadOnly);
			cartage.JJ_IsCancelled = false;
			AssertEquals("Cartage reactivated, so Cartage should not be readonly", false, cartage.ReadOnly);
			AssertEquals("Cartage reactivated, so Container should not be readonly", false, containerBookedMove.ReadOnly);
			Factory.Save();
			newFactory = new BusinessObjectFactory();
			cartage_newFactory = newFactory.Load<CommonCartage>(cartage.PK);
			cartage_newFactory.IsRoot = true;
			containerBookedMove_newFactory = cartage_newFactory.ContainerBookedMoves.First();
			AssertEquals("Cartage reactivated, so Cartage should not be readonly when loaded in another factory", false, cartage_newFactory.ReadOnly);
			AssertEquals("Cartage reactivated, so Container should not be readonly when loaded in another factory", false, containerBookedMove_newFactory.ReadOnly);
		}

		[TestDate(1991, 7, 3, 1, 0, 0)]
		public void TestIsCancelled_ServiceCancelledEventAdded()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			cartage.JJ_ConsignmentID = "T87654321";
			AssertEquals("Precondition: Should not have log entries", 0, cartage.Logs.DatabaseCount);
			cartage.JJ_IsCancelled = true;
			AssertEquals("Should have log entry for service cancelled", 1, cartage.Logs.GetAllLogs().Count);
			var logEntry = cartage.Logs.MostRecentLog;
			CombineAssertions("Should log service cancelled", () =>
			{
				AssertEquals("Should be of type ServiceCancelled", AutoEvents.ServiceCancelledCode, logEntry.SL_SE_NKEvent);
				AssertEquals("Should not be an estimate", false, logEntry.IsEstimate);
				AssertEquals("Should have event time", ZDateTimeOffset.Now, logEntry.EventTimeOffset);
				AssertEquals("Should have log reference", cartage.JJ_ConsignmentID, logEntry.ReferenceFreeText);
				AssertEquals("Should have event reference parameter type of transport", Constants.EventReferenceParameterTypes.Transport, logEntry.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			});
		}

		public void TestIsCancelledOrIsActivated_BookingStatusChanged()
		{
			var booking = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>());
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			cartage.JJ_ParentTableCode = booking.TablePrefix;
			cartage.JJ_ParentID = booking.PK;
			((IDtbBooking)booking).KM_IsActive = true;
			AssertEquals("Precondition: Booking has status service commenced", BookingStatuses.Codes.ServiceCommenced, ((IDtbBooking)booking).KM_Status);
			Factory.Save();

			cartage.JJ_IsCancelled = true;
			Factory.Save();
			AssertEquals("Booking should have status updated to available after first cancellation", BookingStatuses.Codes.Available, ((IDtbBooking)booking).KM_Status);

			cartage.JJ_IsCancelled = false;
			Factory.Save();
			AssertEquals("Booking should have status updated to service commenced after first activation", BookingStatuses.Codes.ServiceCommenced, ((IDtbBooking)booking).KM_Status);

			cartage.JJ_IsCancelled = true;
			Factory.Save();
			AssertEquals("Booking should have status updated to available after second cancellation", BookingStatuses.Codes.Available, ((IDtbBooking)booking).KM_Status);

			cartage.JJ_IsCancelled = false;
			Factory.Save();
			AssertEquals("Booking should have status updated to service commenced after second activation", BookingStatuses.Codes.ServiceCommenced, ((IDtbBooking)booking).KM_Status);
		}

		[TestDate(1991, 7, 3, 1, 0, 0)]
		public void TestIsActivated_LogServicesCommenced()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			cartage.JJ_ConsignmentID = "T87654321";
			cartage.JJ_IsCancelled = true;
			AssertEquals("Precondition: Should have log entry for service cancelled", 1, cartage.Logs.GetAllLogs().Count);
			cartage.JJ_IsCancelled = false;
			AssertEquals("Should have log entry for services commenced", 2, cartage.Logs.GetAllLogs().Count);
			var logEntry = cartage.Logs.MostRecentLogByEventTime(AutoEvents.ServiceCommenced);
			CombineAssertions("Should log services commenced", () =>
			{
				AssertEquals("Should be of type ServiceCommenced", AutoEvents.ServiceCommencedCode, logEntry.SL_SE_NKEvent);
				AssertEquals("Should not be an estimate", false, logEntry.IsEstimate);
				AssertEquals("Should have event time", ZDateTimeOffset.Now, logEntry.EventTimeOffset);
				AssertEquals("Should have log reference", cartage.JJ_ConsignmentID, logEntry.ReferenceFreeText);
				AssertEquals("Should have event reference parameter type of transport", Constants.EventReferenceParameterTypes.Transport, logEntry.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
			});
		}

		public void TestIsActivated_LogServicesCommenced_DoesNotCreateRedundantEntries()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			AssertEquals("Precondition: JJ_IsCancelled starts off false", false, cartage.JJ_IsCancelled);
			cartage.JJ_IsCancelled = false;
			AssertEquals("Should not create a redundant entry", 0, cartage.Logs.GetAllLogs().Count);
		}

		public void TestDefaultDropMode()
		{
			CommonCartageType iSMY = Factory.New<CommonCartageType>();
			iSMY.E3_JobType = "ISM1";
			CommonCartageOrg cTO = iSMY.CommonCartageOrganisations.AddNew();
			cTO.E5_OrgType = "CTO";
			CommonCartageOrg cFS = iSMY.CommonCartageOrganisations.AddNew();
			cFS.E5_OrgType = "CFS";
			CommonCartageOrg cNE = iSMY.CommonCartageOrganisations.AddNew();
			cNE.E5_OrgType = "CNE";
			CommonCartageOrg cYD = iSMY.CommonCartageOrganisations.AddNew();
			cYD.E5_OrgType = "CYD";
			CommonCartageLegType containerMove = iSMY.ContainerizedBookedMoveTypes.AddNew();
			containerMove.E4_ContainerMode = "CNT";
			containerMove.E4_E5_FromOrg = cTO.PK;
			containerMove.E4_E5_WaitPointOrg = cFS.PK;
			containerMove.E4_E5_ToOrg = cYD.PK;
			containerMove.E4_EquipmentGroup = Constants.FCLEquipmentNeeded.SideLoader;
			CommonCartageLegType containerLeg1 = iSMY.ContainerizedCartageLegTypes.AddNew();
			containerLeg1.E4_ContainerMode = "CNT";
			containerLeg1.E4_E5_FromOrg = cTO.PK;
			containerLeg1.E4_E5_ToOrg = cFS.PK;
			CommonCartageLegType containerLeg2 = iSMY.ContainerizedCartageLegTypes.AddNew();
			containerLeg2.E4_ContainerMode = "CNT";
			containerLeg2.E4_E5_FromOrg = cFS.PK;
			containerLeg2.E4_E5_ToOrg = cYD.PK;
			CommonCartageLegType looseMove = iSMY.LooseBookedMoveTypes.AddNew();
			looseMove.E4_ContainerMode = "LSE";
			looseMove.E4_E5_FromOrg = cFS.PK;
			looseMove.E4_E5_ToOrg = cNE.PK;
			looseMove.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.HandHaulier;
			CommonCartageLegType looseLeg = iSMY.LooseCartageLegTypes.AddNew();
			looseLeg.E4_ContainerMode = "LSE";
			looseLeg.E4_E5_FromOrg = cFS.PK;
			looseLeg.E4_E5_ToOrg = cNE.PK;
			//Fallback: Cartage Type
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "ISM1";
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			//Fallback: Cartage
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			//Address Fallback:
			OrgHeader ctoOrgHeader = Factory.New<OrgHeader>();
			ctoOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			ctoOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			ctoOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			OrgHeader cfsOrgHeader = Factory.New<OrgHeader>();
			cfsOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			cfsOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cfsOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			OrgHeader cydOrgHeader = Factory.New<OrgHeader>();
			cydOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			cydOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cydOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			OrgHeader cneOrgHeader = Factory.New<OrgHeader>();
			cneOrgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.Trailer;
			cneOrgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Haulier;
			cneOrgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			OrgHeader cneOrgHeader2 = Factory.New<OrgHeader>();
			cneOrgHeader2.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.Trailer;
			cneOrgHeader2.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			cneOrgHeader2.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			cartage.FirstDocAddress.E2_OA_Address = ctoOrgHeader.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cfsOrgHeader.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = cydOrgHeader.MainAddress.PK;
			cartage.FourthDocAddress.E2_OA_Address = cneOrgHeader.MainAddress.PK;
			AssertEquals("Change to address drop mode", Constants.LCLAIREquipmentNeeded.Haulier, cartage.JJ_DropMode);
			cartage.FourthDocAddress.E2_OA_Address = cneOrgHeader2.MainAddress.PK;
			AssertEquals("Change to address drop mode", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
		}

		public void TestRegisteredEditable()
		{
			var cartage = Factory.New<CommonCartage>();
			Assert(!cartage.IsRoot);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			move.CartageLegs.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			move2.CartageLegs.AddNew();
			Assert("Precondition: LooseBookedMoves is not yet a registered editable child object", !cartage.IsRegisteredEditableChildObject(cartage.LooseBookedMoves));
			Assert("Precondition: ContainerBookedMoves is not yet a registered editable child object", !cartage.IsRegisteredEditableChildObject(cartage.ContainerBookedMoves));
			Assert("Precondition: ContainerLegs is not yet a registered editable child object", !cartage.IsRegisteredEditableChildObject(cartage.CartageLegs));
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			Assert(!cartage_NewFactory.IsRoot);
			Assert("Precondition: LooseBookedMoves is not yet a registered editable child object", !cartage_NewFactory.IsRegisteredEditableChildObject(cartage_NewFactory.LooseBookedMoves));
			Assert("Precondition: ContainerBookedMoves is not yet a registered editable child object", !cartage_NewFactory.IsRegisteredEditableChildObject(cartage_NewFactory.ContainerBookedMoves));
			Assert("Precondition: ContainerLegs is not yet a registered editable child object", !cartage_NewFactory.IsRegisteredEditableChildObject(cartage_NewFactory.CartageLegs));
			foreach (var leg in cartage_NewFactory.CartageLegs)
			{
				Assert("Precondition: All child legs should be marked as IsRoot true", leg.IsRoot);
			}

			newFactory = new BusinessObjectFactory();
			cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			cartage_NewFactory.IsRoot = true;
			Assert("LooseBookedMoves should now be a registered editable child object", cartage_NewFactory.IsRegisteredEditableChildObject(cartage_NewFactory.LooseBookedMoves));
			Assert("ContainerBookedMoves should now be a registered editable child object", cartage_NewFactory.IsRegisteredEditableChildObject(cartage_NewFactory.ContainerBookedMoves));
			Assert("ContainerLegs should now be a registered editable child object", cartage_NewFactory.IsRegisteredEditableChildObject(cartage_NewFactory.CartageLegs));
			foreach (var leg in cartage_NewFactory.CartageLegs)
			{
				Assert("All child legs should be marked as IsRoot false", !leg.IsRoot);
			}
		}

		public void TestRegisteredEditableForBookedMoves()
		{
			var cartage = Factory.New<CommonCartage>();
			Assert(!cartage.IsRoot);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			Assert(move.IsRegisteredEditableChildObject(container));
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			var container_NewFactory = newFactory.Load<CommonContainer>(container.PK);
			Assert(!cartage_NewFactory.IsRoot);
			var move_NewFactory = cartage_NewFactory.ContainerBookedMoves.Single();
			Assert(!move_NewFactory.IsRegisteredEditableChildObject(container_NewFactory));
			newFactory = new BusinessObjectFactory();
			cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			cartage_NewFactory.IsRoot = true;
			move_NewFactory = cartage_NewFactory.ContainerBookedMoves.Single();
			container_NewFactory = move_NewFactory.Container;
			Assert(cartage_NewFactory.IsRoot);
			Assert(move_NewFactory.IsRegisteredEditableChildObject(container_NewFactory));
		}

		public void TestRegisterEditableChildObject()
		{
			var cartage = Factory.New<CommonCartage>();
			Assert(!cartage.IsRoot);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			move.CartageLegs.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			move2.CartageLegs.AddNew();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cartage_NewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			cartage_NewFactory.RegisterEditableChildObject(cartage_NewFactory.CartageLegs);
			foreach (var leg in cartage_NewFactory.CartageLegs)
			{
				Assert("After CartageLegs is registered as an editable child object, all child legs should be have IsRoot false", !leg.IsRoot);
			}

			cartage_NewFactory.UnRegisterEditableChildObject(cartage_NewFactory.CartageLegs);
			foreach (var leg in cartage_NewFactory.CartageLegs)
			{
				Assert("After CartageLegs is unregistered as an editable child object, all child legs should be marked have IsRoot true again", leg.IsRoot);
			}
		}

		public void TestIsRoot()
		{
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLCTOtoCNE, 1);
			Assert(!cartage.IsRoot);
			Assert(!cartage.IsRegisteredEditableChildObject(cartage.LooseBookedMoves));
			Assert(!cartage.IsRegisteredEditableChildObject(cartage.ContainerBookedMoves));
			cartage.IsRoot = true;
			Assert(cartage.IsRoot);
			Assert(cartage.IsRegisteredEditableChildObject(cartage.LooseBookedMoves));
			Assert(cartage.IsRegisteredEditableChildObject(cartage.ContainerBookedMoves));
			cartage.IsRoot = false;
			Assert(!cartage.IsRoot);
			Assert(!cartage.IsRegisteredEditableChildObject(cartage.LooseBookedMoves));
			Assert(!cartage.IsRegisteredEditableChildObject(cartage.ContainerBookedMoves));
		}

		public void TestAllowScheduleCreation()
		{
			var mappings = new[]
			{
				new { Code = "XS1", Mode = Core.Constants.TransportModes.Sea, Checkpoint = Env.Security.SailingScheduleCreateFromJob },
				new { Code = "XA2", Mode = Core.Constants.TransportModes.Air, Checkpoint = Env.Security.FlightScheduleCreateFromJob },
				new { Code = "XR3", Mode = Core.Constants.TransportModes.Road, Checkpoint = Env.Security.TruckScheduleCreateFromJob },
				new { Code = "XL4", Mode = Core.Constants.TransportModes.Rail, Checkpoint = Env.Security.RailScheduleCreateFromJob },
			};

			foreach (var map in mappings)
			{
				map.Checkpoint.IsAllowed = false;
				CommonCartageType type = Factory.New<CommonCartageType>();
				type.E3_JobType = map.Code;
				type.E3_ShippingTransportMode = map.Mode;
				type.E3_GE = GlbDepartment.CurrentDepartment.PK;
				type.E3_Description = "Description";
			}

			Factory.Save();
			CommonCartage cartage = Factory.New<CommonCartage>();
			foreach (var map in mappings)
			{
				cartage.JJ_E3_NKJobType = map.Code;
				map.Checkpoint.IsAllowed = true;
				AssertEquals(map.Mode + ": allowed", true, ((ISailingManaged)cartage).AllowScheduleCreation);
				map.Checkpoint.IsAllowed = false;
				AssertEquals(map.Mode + ": allowed", false, ((ISailingManaged)cartage).AllowScheduleCreation);
			}
		}

		public void TestISailingManaged_TransportMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			var iCartage = (ISailingManaged)cartage;
			AssertEquals(Constants.TransportModes.Air, iCartage.TransportMode);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Rail;
			AssertEquals(Constants.TransportModes.Rail, iCartage.TransportMode);
		}

		public void TestSailingManagerChecksAllowScheduleDatesChanging()
		{
			Env.Security.SailingScheduleEdit.IsAllowed = false;
			var today = ZDateTime.Today;
			var etd1 = today;
			var eta1 = today.AddDays(25);
			var etd2 = today.AddDays(2);
			var eta2 = today.AddDays(30);
			var etd3 = today.AddDays(3);
			var eta3 = today.AddDays(32);
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "TestVessel";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "GH67";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = etd1;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = eta1;
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_JX_Sailing = sailing.PK;
			Factory.Save();
			AssertEquals(cartage.JJ_JX_Sailing, sailing.PK);
			AssertEquals(etd1, sailing.JX_JA_E_DEP);
			AssertEquals(eta1, sailing.JX_JB_E_ARV);
			cartage.E_DEP = etd2;
			cartage.E_ARV = eta2;
			AssertEquals(cartage.JJ_JX_Sailing, sailing.PK);
			AssertEquals(etd1, sailing.JX_JA_E_DEP);
			AssertEquals(eta1, sailing.JX_JB_E_ARV);
			Env.Security.SailingScheduleEdit.IsAllowed = true;
			cartage.E_DEP = etd3;
			cartage.E_ARV = eta3;
			AssertEquals(cartage.JJ_JX_Sailing, sailing.PK);
			AssertEquals(etd3, sailing.JX_JA_E_DEP);
			AssertEquals(eta3, sailing.JX_JB_E_ARV);
		}

		public void TestISailingParentFindBox_TransportMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			var iCartage = (ISailingParentFindBox)cartage;
			AssertEquals(Constants.TransportModes.Air, iCartage.TransportMode);
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Rail;
			AssertEquals(Constants.TransportModes.Rail, iCartage.TransportMode);
		}

		public void TestEventsFor_JJ_A_JCL()
		{
			var year = ZDateTime.Now.Year;
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_A_JCL = new ZDateTime(year, 1, 1);
			AssertEquals("Correct event", false, cartage.Logs.MostRecentLogByEventTime(Events.DeliveryCartageCompleteFinalised).SL_IsEstimate);
		}

		public void TestEventOnFactorySaved()
		{
			bool eventFired = false;
			var cartage = Factory.New<CommonCartage>();
			cartage.CartageFactorySaved += (s, e) =>
			{
				eventFired = true;
			};
			cartage.Factory.Save();
			AssertEquals("Event was fired.", true, eventFired);
		}

		public void TestCartageRating()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			AssertNull(cartage.GetFirstAdapter());
			var move1 = cartage.ContainerBookedMoves.AddNew();
			move1.CreateDefaultLegs();
			var move2 = cartage.ContainerBookedMoves.AddNew();
			move2.CreateDefaultLegs();
			var container1 = move1.Container;
			var container2 = move2.Container;
			move1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move1.EW_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			move1.EW_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;
			move2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move2.EW_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			move2.EW_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;
			AssertEquals(2, cartage.GetRatingAdapters().Count);
			CommonCartageLeg leg = cartage.GetBookedMoves(container1)[0].CartageLegs.AddNew();
			AssertEquals(2, cartage.GetRatingAdapters().Count);
			leg.JU_AdditionalService = Constants.CartageAdditional.AdditionalService;
			AssertEquals(3, cartage.GetRatingAdapters().Count);
		}

		public void TestCartageRating_GetCorrectAdapter_BasedOnCartageContainerMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			var move1 = cartage.ContainerBookedMoves.AddNew();
			move1.CreateDefaultLegs();
			var container1 = move1.Container;
			move1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move1.EW_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			move1.EW_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;
			var leg1 = cartage.GetBookedMoves(container1)[0].CartageLegs.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			move2.CreateDefaultLegs();
			move2.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move2.EW_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			move2.EW_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;
			var leg2 = move2.CartageLegs.AddNew();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var adapters = cartage.GetRatingAdapters();
			AssertEquals(2, adapters.Count);
			AssertEquals(1, adapters.Cast<IAutoRating>().Count(a => a.FreightMode == FreightMode.FRO)); // FCL
			AssertEquals(1, adapters.Cast<IAutoRating>().Count(a => a.FreightMode == FreightMode.LRO)); // LTL Loose
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			adapters = cartage.GetRatingAdapters();
			AssertEquals(FreightMode.LRO, adapters.Cast<IAutoRating>().Single().FreightMode); // LTL (Loose)
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			move1 = cartage.ContainerBookedMoves.AddNew();
			move1.CreateDefaultLegs();
			container1 = move1.Container;
			move1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			move1.EW_E2WaitPointAddressID = cartage.SecondDocAddress.PK;
			move1.EW_E2DeliveryAddressID = cartage.ThirdDocAddress.PK;
			leg1 = cartage.GetBookedMoves(container1)[0].CartageLegs.AddNew();
			adapters = cartage.GetRatingAdapters();
			AssertEquals(1, adapters.Count);
			AssertEquals(FreightMode.FRO, adapters.Cast<IAutoRating>().Single().FreightMode); // FCL
		}

		public void TestICDArchive_ContainerNumbersList()
		{
			var cartage = Factory.New<CommonCartage>();
			var archiveInfo = ((ICDArchive)cartage).CDArchiveInfo;
			AssertEquals(0, archiveInfo.ContainerNumbersList.Length);
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "", "" }, archiveInfo.ContainerNumbersList);
			container1.JC_ContainerNum = "C1";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "C1", "" }, archiveInfo.ContainerNumbersList);
			container2.JC_ContainerNum = "C2";
			AssertContainsExactElementsInAnyOrder(new ZString[] { "C1", "C2" }, archiveInfo.ContainerNumbersList);
		}

		public void TestTEUCount()
		{
			var cartage = Factory.New<CommonCartage>();
			var containerNumbersList = ((ICDArchive)cartage).CDArchiveInfo.ContainerNumbersList;
			AssertEquals(0m, cartage.InvoicingSupporter.TEUCount);
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			AssertEquals(2m, cartage.InvoicingSupporter.TEUCount); // Default value for JC_Calc_TEUCount is one
			container1.JC_ContainerCount = 2;
			AssertEquals(3m, cartage.InvoicingSupporter.TEUCount);
			container2.JC_ContainerCount = 3;
			AssertEquals(5m, cartage.InvoicingSupporter.TEUCount);
		}

		public void TestJobTypeList()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_AirExport, null);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_AirExport));
			Assert(!cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_AirImport));
			cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_AirImport, null);
			cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_AirImport));
			Assert(!cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_AirExport));
			cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_DomesticContainerizedDelivery, null);
			cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_DomesticContainerizedDelivery));
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_DomesticLooseDelivery));
			Assert(!cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup));
			cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup, null);
			cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup));
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_DomesticLoosePickup));
			Assert(!cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_DomesticContainerizedDelivery));
			cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_EmptyCFStoCYD, null);
			cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_EmptyCFStoCYD));
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_FCLCTOtoCNE));
			Assert(!cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_DomesticContainerizedDelivery));
			Assert(!cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_FCLExportPack));
			cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLExportToSHP, null);
			cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_FCLExportToSHP));
			Assert(cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_EmptyCYDtoCFS));
			Assert(!cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_EmptyCFStoCYD));
			Assert(!cartage.JobTypeList.ContainsCode(Core.Constants.CartageJobType.NEW_FCLCTOtoCNE));
		}

		public void TestJobType()
		{
			var cartage = Factory.New<CommonCartage>(); //ISFC
			AssertEquals(Constants.CartageDirection.Import, cartage.JJ_Direction);
			AssertEquals(Constants.TransportModes.Sea, cartage.JJ_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
			var jobType = Factory.New<CommonCartageType>();
			jobType.E3_JobType = "ESFX";
			cartage.JJ_E3_NKJobType = "ESFX";
			AssertEquals(Constants.CartageDirection.Export, cartage.JJ_Direction);
			AssertEquals(Constants.TransportModes.Sea, cartage.JJ_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
			jobType.E3_JobType = "IRLX";
			cartage.JJ_E3_NKJobType = "IRLX";
			AssertEquals(Constants.CartageDirection.Import, cartage.JJ_Direction);
			AssertEquals(Constants.TransportModes.Road, cartage.JJ_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Loose, cartage.JJ_ContainerMode);
			jobType.E3_JobType = "OAMX";
			cartage.JJ_E3_NKJobType = "OAMX";
			AssertEquals(Constants.CartageDirection.Origin, cartage.JJ_Direction);
			AssertEquals(Constants.TransportModes.Air, cartage.JJ_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Mixed, cartage.JJ_ContainerMode);
			jobType.E3_JobType = "LACX";
			cartage.JJ_E3_NKJobType = "LACX";
			AssertEquals(Constants.CartageDirection.Local, cartage.JJ_Direction);
			AssertEquals(Constants.TransportModes.Air, cartage.JJ_ShippingTransportMode);
			AssertEquals(Constants.CartageContainerMode.Containerized, cartage.JJ_ContainerMode);
		}

		public void TestSetDropMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.Haulier;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			var address = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			cartage.SetCartageDropModeWithOutSettingMoves(DropMode.Address);
			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			cartage.SetCartageDropModeWithOutSettingMoves(DropMode.CartageType);
			AssertEquals(Constants.LCLAIREquipmentNeeded.Haulier, cartage.JJ_DropMode);
			cartage.SetCartageDropModeWithOutSettingMoves();
			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			((ISupportDataImporting)cartage).IsImportingData = true;
			cartage.SetCartageDropModeWithOutSettingMoves(DropMode.CartageType);
			AssertEquals("Cannot default drop mode when importing", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
		}

		public void TestJJ_DropMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLPackLooseFromSHP;
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			var move3 = cartage.ContainerBookedMoves.AddNew();
			var container = move3.Container;
			cartage.JJ_DropMode = Core.Constants.FCLEquipmentNeeded.SideLoader;
			Assert("Should not set because it's not a valid drop mode for loose", move1.EW_DropMode.IsEmpty);
			Assert("Should not set because it's not a valid drop mode for loose", move2.EW_DropMode.IsEmpty);
			AssertEquals(Core.Constants.FCLEquipmentNeeded.SideLoader, move3.EW_DropMode);
			move1.EW_DropMode = Core.Constants.LCLAIREquipmentNeeded.HandHaulier;
			cartage.JJ_DropMode = Core.Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			AssertEquals(Core.Constants.LCLAIREquipmentNeeded.HandUnloadLoad, move1.EW_DropMode);
			AssertEquals(Core.Constants.LCLAIREquipmentNeeded.HandUnloadLoad, move2.EW_DropMode);
			AssertEquals("Cartage one isn't valid, there are not others to set, so leave as is", Core.Constants.FCLEquipmentNeeded.SideLoader, move3.EW_DropMode);
		}

		public void TestJJ_DropModePopupBehavesCorrectly()
		{
			var notify = new TestNotify();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.LooseBookedMoves.DeleteAll();
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.Premise;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			var address = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cartage.SetNotificationSubscriber(notify);
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			cartage.JJ_DropMode = "";
			AssertNull("Setting Drop Mode to empty does not trigger popup", notify.LastEventYesNoArgs);
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			AssertNull("Invalid Drop Mode does not trigger popup", notify.LastEventYesNoArgs);
			move1.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			move2.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			AssertNull("Drop Mode the same as all booked moves does not trigger popup", notify.LastEventYesNoArgs);
			((ISupportDataImporting)cartage).IsImportingData = true;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			AssertNull("When importing Data popup is not triggered", notify.LastEventYesNoArgs);
			((ISupportDataImporting)cartage).IsImportingData = false;
			move2.EW_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			AssertEquals("At least one move had a different drop mode, so popup was triggered", "Populate Booked Movement Drop Mode", notify.LastEventYesNoArgs.Caption);
			AssertEquals("Would you like to override ALL Booked Movement Drop Modes with 'HUL'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Drop Mode should not be set because Answer was no", Constants.LCLAIREquipmentNeeded.Premise, move1.EW_DropMode);
			AssertEquals("Drop Mode was already the same", Constants.LCLAIREquipmentNeeded.HandUnloadLoad, move2.EW_DropMode);
			notify.ResponseToDialogs = true;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			AssertEquals("Another popup is triggered", 2, notify.Events.Count);
			AssertEquals("Drop Mode should be set because Answer was yes", Constants.LCLAIREquipmentNeeded.Haulier, move1.EW_DropMode);
			AssertEquals("Drop Mode should be set because Answer was yes", Constants.LCLAIREquipmentNeeded.Haulier, move2.EW_DropMode);
			notify.ClearLastEventArgs();
			orgHeader.MainAddress.OA_LCLEquipmentNeeded = "";
			cartage.LooseBookedMoves.DeleteAll();
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			AssertNull("Setting Drop Mode with no moves will not trigger popup", notify.LastEventYesNoArgs);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			AssertNull("Setting Drop Mode with no moves will not trigger popup", notify.LastEventYesNoArgs);
			// all tests from here are for container mode == Mixed - should not have container moves and loose moves without setting container mode to Mixed
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var containerMove1 = cartage.ContainerBookedMoves.AddNew();
			var containerMove2 = cartage.ContainerBookedMoves.AddNew();
			var container1 = containerMove1.Container;
			var container2 = containerMove2.Container;
			containerMove1.EW_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			containerMove2.EW_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			AssertNull("Drop Mode the same as all booked moves does not trigger popup", notify.LastEventYesNoArgs);
			containerMove2.EW_DropMode = "";
			cartage.JJ_DropMode = "";
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			AssertEquals("At least one Containerized move with different drop mode, will trigger popup", "Populate Booked Movement Drop Mode", notify.LastEventYesNoArgs.Caption);
			AssertEquals("Would you like to override ALL Booked Movement Drop Modes with 'SDL'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Drop Mode should be set because Answer was yes", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("Drop Mode should be set because Answer was yes", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			notify.ClearLastEventArgs();
			cartage.LooseBookedMoves.DeleteAll();
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			AssertNull("Setting Drop Mode with no moves will not trigger popup", notify.LastEventYesNoArgs);
			var looseMove = cartage.LooseBookedMoves.AddNew();
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			AssertEquals("Populate Booked Movement Drop Mode", notify.LastEventYesNoArgs.Caption);
			AssertEquals("Would you like to override ALL Booked Movement Drop Modes with 'HUL'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Drop Mode should be set because Answer was yes", Constants.LCLAIREquipmentNeeded.HandUnloadLoad, looseMove.EW_DropMode);
			AssertEquals("Container drop mode should not have changed", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("Container drop mode should not have changed", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			AssertEquals("Would you like to override ALL Booked Movement Drop Modes with 'HSL'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Drop Mode should be set because Answer was yes", Constants.LCLAIREquipmentNeeded.Haulier, looseMove.EW_DropMode);
			AssertEquals("Container drop mode should not have changed", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("Container drop mode should not have changed", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			AssertEquals("Would you like to override ALL Booked Movement Drop Modes with 'LOF'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Loose move drop mode should not have changed", Constants.LCLAIREquipmentNeeded.Haulier, looseMove.EW_DropMode);
			AssertEquals("Drop Mode should be set because Answer was yes", Constants.FCLEquipmentNeeded.LiftOffOn, containerMove1.EW_DropMode);
			AssertEquals("Drop Mode should be set because Answer was yes", Constants.FCLEquipmentNeeded.LiftOffOn, containerMove2.EW_DropMode);
			cartage.JJ_DropMode = "ASK";
			AssertEquals("Would you like to override ALL Booked Movement Drop Modes with 'ASK'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Drop mode should be set because Answer was yes", "ASK", looseMove.EW_DropMode);
			AssertEquals("Drop Mode should not change", "ASK", containerMove1.EW_DropMode);
			AssertEquals("Drop Mode should not change", "ASK", containerMove2.EW_DropMode);
			notify.ClearLastEventArgs();
			cartage.Containers.DeleteAll();
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			AssertNull("Setting Drop Mode with no containerized moves will not trigger popup", notify.LastEventYesNoArgs);
			cartage.ContainerBookedMoves.AddNew();
			cartage.LooseBookedMoves.DeleteAll();
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			AssertNull("Setting Drop Mode with no loose moves will not trigger popup", notify.LastEventYesNoArgs);
		}

		public void TestDropModeDefaultsFromCartageParentOnCreationOfCartageFromCartageParent()
		{
			var cartage = Factory.New<CartageForTest>();
			var parent = new DummyCartageParent(Factory);
			var type = new DummyCartageType(parent);
			type.SetDropMode(Constants.LCLAIREquipmentNeeded.Premise);
			cartage.SetParent(parent);
			parent.SetCartageType(type);
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Haulier;
			var address = cartage.DocAddresses.AddNew();
			address.DocAddressType = DocAddressType.LocalCartageExporter;
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_DropMode = "";
			type.SetDropMode(Constants.FCLEquipmentNeeded.LiftOffOn);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			AssertEquals("Straight defaulting should not pick the Parent Drop Mode if not a valid drop mode", Constants.LCLAIREquipmentNeeded.Haulier, cartage.JJ_DropMode);
		}

		public void TestAdditionalReferenceNumbers()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(0, cartage.AdditionalReferenceNumbers.Count);
			cartage.AdditionalReferenceNumbers.AddNew();
			AssertEquals(1, cartage.AdditionalReferenceNumbers.Count);
		}

		public void TestTransportBookingPartyReference()
		{
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var parentBooking = Factory.New<IDtbBooking>();
			parentBooking.KM_KB_Booking = consolidation.PK;
			parentBooking.KM_JobID = "XYZ";
			Factory.Save();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentID = parentBooking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			AssertEquals("", cartage.TransportBookingPartyReference);

			var reference = cartage.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryNum = "ABC";
			AssertEquals("", cartage.TransportBookingPartyReference);

			cartage.JJ_OrderReferenceNumber = "TB001";
			AssertEquals("Use own internal TB order reference if there is one and there is no ETB reference - OLD WAY - Brenton may change this", "TB001", cartage.TransportBookingPartyReference);

			reference.CE_EntryType = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;
			AssertEquals("Use Additional Reference over own TB, we send events directly to sending tb.", "ABC", cartage.TransportBookingPartyReference);

			cartage.CurrentlyPublishingAnEventToTheOrgProxy = true;
			AssertEquals("Use own internal TB JobID if there is one and CurrentlyPublishingAnEventToTheOrgProxy is true, even if an ETB reference exists.", "XYZ", cartage.TransportBookingPartyReference);
		}

		public void TestClientReference()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals("", cartage.ClientReference);
			var reference = cartage.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryNum = "ABC";
			AssertEquals("", cartage.ClientReference);
			cartage.JJ_OrderReferenceNumber = "TB001";
			AssertEquals("Use order ref if there is no Additional Transport Reference. Order Ref is populated with External TB #, Internal TB # or Order #.", "TB001", cartage.ClientReference);
			reference.CE_EntryType = AdditionalReferenceTypes.Codes.TransportReference;
			AssertEquals("Use Additional Reference TRF (internal TB #) over JJ_OrderRef (likely to be external TB #). Client Ref should be the internal TB #.", "ABC", cartage.ClientReference);
		}

		public void TestParentViewAndParentJobProperties_StandAloneCartage()
		{
			var standAloneCartage = Factory.New<CommonCartage>();
			AssertNull("Standalone cartage should have null parent record", standAloneCartage.ParentView);
			AssertEquals("Standalone cartage should have empty string for Parent Job Number", ZString.Empty, standAloneCartage.ParentJobNumber);
			AssertEquals("Standalone cartage should have empty string for Parent Job Type", ZString.Empty, standAloneCartage.ParentJobType);
			AssertNull("Standalone cartage should have a null ParentJob", standAloneCartage.ParentJob);
		}

		public void TestParentViewAndParentJobProperties_CartageWithForwardingShipmentParent()
		{
			(var cartageWithShipmentParent, var _, var shipmentParent, var _) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			var shipmentParentBO = (CommonShipment)shipmentParent;
			AssertEquals("Cartage should have non-null parent record with PK equal to the parent PK", shipmentParentBO.PK, cartageWithShipmentParent.ParentView.PK);
			AssertEquals("Cartage Parent Job Number should match shipment parent's Job Number (JS_UniqueConsignRef)", shipmentParentBO.JS_UniqueConsignRef, cartageWithShipmentParent.ParentJobNumber);
			AssertEquals("Cartage Parent Job Type should be 'SHP' - for Forwarding Shipment", "SHP", cartageWithShipmentParent.ParentJobType);
			Assert("Cartage Parent Job should be ForwardingShipment", cartageWithShipmentParent.ParentJob is IForwardingShipment);
		}

		public void TestParentViewAndParentJobProperties_CartageWithCFSShipmentParent()
		{
			var cartage = Factory.New<CommonCartage>();
			var cfsShipment = (BusinessObject)Factory.New<ICFSShipment>();
			cartage.JJ_ParentID = cfsShipment.PK;
			cartage.JJ_ParentTableCode = cfsShipment.TablePrefix;
			Factory.Save();
			AssertEquals("Cartage should have non-null parent record with PK equal to the parent PK", cfsShipment.PK, cartage.ParentView.PK);
			AssertEquals("Cartage Parent Job Number should match shipment parent's Job Number (JS_UniqueConsignRef)", cfsShipment[JobShipmentSchema.JS_UniqueConsignRef], cartage.ParentJobNumber);
			AssertEquals("Cartage Parent Job Type should be 'CFS' - for CFS Shipment", "CFS", cartage.ParentJobType);
			Assert("Cartage Parent Job should be ICFSShipment", cartage.ParentJob is ICFSShipment);
		}

		public void TestParentViewAndParentJobProperties_CartageWithCFSLoadListConsolParent()
		{
			var cartage = Factory.New<CommonCartage>();
			var cfsLoadListConsol = (BusinessObject)Factory.New<ICFSLoadListConsol>();
			cartage.JJ_ParentID = cfsLoadListConsol.PK;
			cartage.JJ_ParentTableCode = cfsLoadListConsol.TablePrefix;
			Factory.Save();
			AssertEquals("Cartage should have non-null parent record with PK equal to the parent PK", cfsLoadListConsol.PK, cartage.ParentView.PK);
			AssertEquals("Cartage Parent Job Number should match cfs load list consol parent's Job Number (JK_UniqueConsignRef)", cfsLoadListConsol[JobConsolSchema.JK_UniqueConsignRef], cartage.ParentJobNumber);
			AssertEquals("Cartage Parent Job Type should be 'CFC' - for CFS Load List Consol", "CFC", cartage.ParentJobType);
			Assert("Cartage Parent Job should be ICFSLoadListConsol", cartage.ParentJob is ICFSLoadListConsol);
		}

		public void TestParentViewAndParentJobProperties_CartageWithWhsOrderParent()
		{
			var cartage = Factory.New<CommonCartage>();
			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();
			whsOrder[WhsDocketSchema.WD_DocketID] = "BX0000001";
			whsOrder[WhsDocketSchema.WD_DocketSubType] = "ORD";
			whsOrder[WhsDocketSchema.WD_TotalUnits] = 1;
			whsOrder[WhsDocketSchema.WD_TotalWeight] = 1;
			whsOrder[WhsDocketSchema.WD_TotalWeightUnit] = "KG";
			whsOrder[WhsDocketSchema.WD_TotalCubic] = 1;
			whsOrder[WhsDocketSchema.WD_TotalCubicUnit] = "M3";
			cartage.JJ_ParentID = whsOrder.PK;
			cartage.JJ_ParentTableCode = whsOrder.TablePrefix;
			Factory.Save();
			AssertEquals("Cartage should have non-null parent record with PK equal to the parent PK", whsOrder.PK, cartage.ParentView.PK);
			AssertEquals("Cartage Parent Job Number should match cfs load list consol parent's Job Number (JK_UniqueConsignRef)", whsOrder[WhsDocketSchema.WD_DocketID], cartage.ParentJobNumber);
			AssertEquals("Cartage Parent Job Type should be 'WHS' - for Warehouse Order", "WHO", cartage.ParentJobType);
			Assert("Cartage Parent Job should be IWhsOrder", cartage.ParentJob is IWhsOrder);
		}

		public void TestParentViewAndParentJobProperties_CartageWithJobDeclarationParent()
		{
			var cartage = Factory.New<CommonCartage>();
			var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			Factory.Save();
			AssertEquals("Cartage should have non-null parent record with PK equal to the parent PK", declaration.PK, cartage.ParentView.PK);
			AssertEquals("Cartage Parent Job Number should match shipment parent's Job Number (JS_UniqueConsignRef)", declaration[JobDeclarationSchema.JE_DeclarationReference], cartage.ParentJobNumber);
			AssertEquals("Cartage Parent Job Type should be 'CUS' - for Customs Declaration", "CUS", cartage.ParentJobType);
			Assert("Cartage Parent Job should be IBaseJobDeclaration", cartage.ParentJob is IBaseJobDeclaration);
		}

		public void TestParentViewAndParentJobProperties_CartageWithDtbBookingParent()
		{
			var cartage = Factory.New<CommonCartage>();
			var dtbBooking = (BusinessObject)Factory.New<IDtbBooking>();
			dtbBooking.FillWithValidTestData();
			var dtbBookingConsolidation = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
			dtbBookingConsolidation.FillWithValidTestData();
			dtbBooking[DtbBookingSchema.KM_KB_Booking] = dtbBookingConsolidation.PK;
			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = dtbBooking.TablePrefix;
			Factory.Save();
			AssertEquals("Cartage should have non-null parent record with PK equal to the parent PK", dtbBooking.PK, cartage.ParentView.PK);
			AssertEquals("Cartage Parent Job Number should match cfs load list consol parent's Job Number (KM_JobID)", dtbBooking[DtbBookingSchema.KM_JobID], cartage.ParentJobNumber);
			AssertEquals("Cartage Parent Job Type should be 'TBK' - for Warehouse Order", "TBK", cartage.ParentJobType);
			Assert("Cartage Parent Job should be IDtbBooking", cartage.ParentJob is IDtbBooking);
		}

		public void TestAddressChangeDefaultsDropMode()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.MainAddress.OA_LCLEquipmentNeeded = ""; // default was Premise
			var cnrAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			var notify = new TestNotify();
			cartage.SetNotificationSubscriber(notify);
			cnrAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			AssertNull("If Address has empty drop mode, popup is not triggered", notify.LastEventYesNoArgs);
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			cnrAddress.E2_OA_Address = orgHeader2.MainAddress.PK;
			AssertNull("If Address has same drop mode as job, popup is not triggered", notify.LastEventYesNoArgs);
			((ISupportDataImporting)cartage).IsImportingData = true;
			cartage.JJ_DropMode = "";
			orgHeader1.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cnrAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			AssertNull("When Importing Data, popup should not be triggered", notify.LastEventYesNoArgs);
			((ISupportDataImporting)cartage).IsImportingData = false;
			cnrAddress.E2_OA_Address = ZGuid.Empty;
			cnrAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			AssertEquals("If Job Drop Mode was empty, defaulting is done without asking", Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			AssertEquals(1, notify.Events.Count);
			AssertEquals("Changing drop mode on Job through address triggers Drop Mode Popup", "Populate Booked Movement Drop Mode", notify.LastEventYesNoArgs.Caption);
			cnrAddress.E2_OA_Address = orgHeader2.MainAddress.PK;
			AssertEquals("Populate Cartage Drop Mode", notify.LastEventYesNoArgs.Caption);
			AssertEquals("Would you like to set the Cartage Drop Mode with the Consignor Drop Mode 'PSL'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Drop Mode should be same because Answer was no", Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			notify.ResponseToDialogs = true;
			orgHeader1.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			cnrAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			AssertEquals("Drop Mode should have changed because Answer was yes", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			notify.ClearLastEventArgs();
			var containerYard = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageYard);
			orgHeader1.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			containerYard.E2_OA_Address = orgHeader1.MainAddress.PK;
			AssertNull("Changing an address that is not Consignee or Consignor should not trigger popup", notify.LastEventYesNoArgs);
			AssertEquals("Drop Mode should not have changed", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			cartage.JJ_DropMode = "";
			orgHeader2.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			containerYard.E2_OA_Address = orgHeader2.MainAddress.PK;
			AssertNull("Changing an address that is not Consignee or Consignor should not trigger popup", notify.LastEventYesNoArgs);
			AssertEquals("Drop Mode should not have changed", "", cartage.JJ_DropMode);
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCYDtoCFS;
			var cfs = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS);
			orgHeader1.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			cnrAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			AssertNull("Changing an address that is not CFS when there is no Consignee or Consignor should not trigger popup", notify.LastEventYesNoArgs);
			AssertEquals("Drop Mode should not have changed", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			cfs.E2_OA_Address = orgHeader1.MainAddress.PK;
			AssertNotNull("Popup was triggered", notify.LastEventYesNoArgs);
			AssertEquals("Drop Mode should have changed because Answer was yes", Constants.FCLEquipmentNeeded.LiftOffOn, cartage.JJ_DropMode);
			orgHeader1.MainAddress.OA_AIREquipmentNeeded = "";
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			notify.ClearLastEventArgs();
			orgHeader1.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Haulier;
			cnrAddress.E2_OA_Address = ZGuid.Empty;
			cnrAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			AssertNotNull("Popup was triggered", notify.LastEventYesNoArgs);
			AssertEquals("Drop Mode should have changed because Answer was yes", Constants.LCLAIREquipmentNeeded.Haulier, cartage.JJ_DropMode);
		}

		public void TestAddressChangeCanDefaultFromCFS()
		{
			var notify = new TestNotify();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLPackLooseFromSHP;
			cartage.LooseBookedMoves.DeleteAll();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			orgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			var cfs = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS);
			cfs.E2_OA_Address = orgHeader.MainAddress.PK;
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.EW_E2DeliveryAddressID = cfs.PK;
			containerMove.EW_DropMode = "";
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			cartage.SetNotificationSubscriber(notify);
			notify.ResponseToDialogs = false;
			cfs.E2_OA_Address = ZGuid.Empty;
			cfs.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("Changing the CFS should trigger popup when at least one move has CFS as requested address", "Would you like to set the Cartage Drop Mode with the CFS Drop Mode 'LOF'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Should not have changed drop mode", Constants.FCLEquipmentNeeded.SideLoader, cartage.JJ_DropMode);
			notify.ResponseToDialogs = true;
			cfs.E2_OA_Address = ZGuid.Empty;
			cfs.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("Changing the CFS should trigger popup when at least one move has CFS as requested address", "Would you like to override ALL Booked Movement Drop Modes with 'LOF'?", notify.LastEventYesNoArgs.Message);
			AssertEquals("Should have drop mode changed", Constants.FCLEquipmentNeeded.LiftOffOn, cartage.JJ_DropMode);
			AssertEquals("Should have drop mode changed on move", Constants.FCLEquipmentNeeded.LiftOffOn, containerMove.EW_DropMode);
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.EW_E2DeliveryAddressID = cfs.PK;
			notify.ResponseToDialogs = false;
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			cfs.E2_OA_Address = ZGuid.Empty;
			cfs.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("Changing the CFS should trigger popup when at least one move has CFS as requested address", "Would you like to set the Cartage Drop Mode with the CFS Drop Mode 'LOF'?", notify.LastEventYesNoArgs.Message);
			cartage.Containers.DeleteAll();
			cfs.E2_OA_Address = ZGuid.Empty;
			cfs.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("Changing the CFS should trigger popup when at least one move has CFS as requested address", "Would you like to set the Cartage Drop Mode with the CFS Drop Mode 'PSL'?", notify.LastEventYesNoArgs.Message);
			var consignor = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			cartage.LooseBookedMoves.DeleteAll();
			notify.ClearLastEventArgs();
			consignor.E2_OA_Address = orgHeader.MainAddress.PK;
			AssertEquals("When no moves are present on mixed job, defaults to loose drop mode", "Would you like to set the Cartage Drop Mode with the Consignor Drop Mode 'PSL'?", notify.LastEventYesNoArgs.Message);
		}

		public void TestAddressChangeDoesNotDefaultDropModeIfJobTypeIsInError()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			var address = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.EW_E2PickupAddressID = address.PK;
			cartage.JJ_E3_NKJobType = "XXXX";
			((IDocAddresses)cartage).DocAddressChanged(address);
			AssertEquals("Did not change cartage Drop Mode", Constants.FCLEquipmentNeeded.SideLoader, cartage.JJ_DropMode);
		}

		public void TestAddressChangeDoesNotDefaultDropModeIfJobTypeIsChanging()
		{
			var notify = new TestNotify();
			notify.ResponseToDialogs = true;
			var parent = new DummyCartageParent(Factory);
			var cartage = Factory.New<CartageForTest>();
			var dummyHeader = Factory.New<OrgHeader>();
			dummyHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = dummyHeader.MainAddress.PK;
			docAddress.DocAddressType = DocAddressType.LocalCartageExporter;
			var addressDictionary = new Dictionary<ZString, JobDocAddress>();
			addressDictionary.Add("CNR", docAddress);
			var type = new DummyCartageType(parent, Constants.CartageJobType.NEW_FCLExportToSHP, addressDictionary);
			cartage.SetParent(parent);
			parent.SetCartageType(type);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			var address = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.SideLoader;
			cartage.SetNotificationSubscriber(notify);
			var collection = new JobDocAddressCollection(Factory);
			cartage.FindAndAddAddressIfRequired(collection, 2);
			AssertNull("No popup was triggered", notify.LastEventYesNoArgs);
			AssertEquals("Drop mode did not change", Constants.FCLEquipmentNeeded.SideLoader, cartage.JJ_DropMode);
		}

		public void TestJobTypeChangeDefaultsDropMode_ForAir()
		{
			var notify = new TestNotify();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.Premise;
			var orgHeader = Factory.New<OrgHeader>();
			var address = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			orgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			orgHeader.MainAddress.OA_AIREquipmentNeeded = Constants.LCLAIREquipmentNeeded.Haulier;
			cartage.SetNotificationSubscriber(notify);
			cartage.LooseBookedMoves.DeleteAll();
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			move1.EW_E2DeliveryAddressID = address.PK;
			move1.EW_DropMode = "";
			move2.EW_E2DeliveryAddressID = address.PK;
			move2.EW_DropMode = "";
			notify.ResponseToDialogs = true;
			cartage.JJ_DropMode = "";
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			AssertNull("If Job Drop Mode was empty Job Type drop mode is defaulted without asking", notify.LastDropModeEventArgs);
			AssertEquals("Straight defaulting the Drop Mode triggers Drop Mode Popup", 1, notify.Events.Count);
			AssertEquals(Constants.LCLAIREquipmentNeeded.Haulier, cartage.JJ_DropMode);
			AssertEquals("Answer was yes to change drop mode on all Booked moves", Constants.LCLAIREquipmentNeeded.Haulier, move1.EW_DropMode);
			AssertEquals("Answer was yes to change drop mode on all Booked moves", Constants.LCLAIREquipmentNeeded.Haulier, move2.EW_DropMode);
			notify.ClearLastEventArgs();
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.CartageType;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			AssertEquals("Another popup is triggered", 2, notify.Events.Count);
			AssertNull("Changing drop mode through here does not trigger Drop Mode popup", notify.LastEventYesNoArgs);
			AssertEquals("Answer was to change to the Job Type Drop Mode, so Drop Mode is changed", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.Premise, move1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.Premise, move2.EW_DropMode);
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.Address;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			AssertEquals("Another popup is triggered", 3, notify.Events.Count);
			AssertEquals("Correct button text", "Set Loose moves with Consignor Drop Mode of 'HSL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertNull("Changing drop mode through here does not trigger Drop Mode popup", notify.LastEventYesNoArgs);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed", Constants.LCLAIREquipmentNeeded.Haulier, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.Haulier, move1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.Haulier, move2.EW_DropMode);
		}

		public void TestJobTypeChangeDefaultsDropMode_ForLoose()
		{
			var notify = new TestNotify();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.Premise;
			var orgHeader = Factory.New<OrgHeader>();
			var address = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			orgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cartage.SetNotificationSubscriber(notify);
			cartage.JJ_E3_NKJobType = "";
			AssertNull("Setting Job Type to empty does not trigger popup", notify.LastDropModeEventArgs);
			cartage.JJ_E3_NKJobType = "XXX";
			AssertNull("Setting Job Type to an invalid job type does not trigger popup", notify.LastDropModeEventArgs);
			((ISupportDataImporting)cartage).IsImportingData = true;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertNull("When importing data it should not trigger popup", notify.LastDropModeEventArgs);
			((ISupportDataImporting)cartage).IsImportingData = false;
			cartage.LooseBookedMoves.DeleteAll();
			var move1 = cartage.LooseBookedMoves.AddNew();
			var move2 = cartage.LooseBookedMoves.AddNew();
			move1.EW_E2DeliveryAddressID = address.PK;
			move1.EW_DropMode = "";
			move2.EW_E2DeliveryAddressID = address.PK;
			move2.EW_DropMode = "";
			notify.ResponseToDialogs = true;
			cartage.JJ_DropMode = "";
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertNull("If Job Drop Mode was empty Job Type drop mode is defaulted without asking", notify.LastDropModeEventArgs);
			AssertEquals("Straight defaulting the Drop Mode triggers Drop Mode Popup", 1, notify.Events.Count);
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			AssertEquals("Answer was yes to change drop mode on all Booked moves", Constants.LCLAIREquipmentNeeded.HandHaulier, move1.EW_DropMode);
			AssertEquals("Answer was yes to change drop mode on all Booked moves", Constants.LCLAIREquipmentNeeded.HandHaulier, move2.EW_DropMode);
			notify.ClearLastEventArgs();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			AssertNull("If Job Type and Address Drop Modes are empty, no popup is triggered", notify.LastDropModeEventArgs);
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			notify.ResponseToDropModeDialog = DropMode.None;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("Popup is triggered", "The Address and/or Job Type Drop Modes are different from the current Drop Mode. Would you like to keep the current Cartage Drop Mode of 'HWL' or default to a new Drop Mode?", notify.LastDropModeEventArgs.Message);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Loose moves with Job Type Drop Mode of 'PSL'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals(2, notify.Events.Count);
			AssertEquals("No answer was specified so Drop Mode remains the same", Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			notify.ResponseToDropModeDialog = DropMode.CartageType;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("Another popup is triggered", 3, notify.Events.Count);
			AssertNull("Changing drop mode through here does not trigger Drop Mode popup", notify.LastEventYesNoArgs);
			AssertEquals("Answer was to change to the Job Type Drop Mode, so Drop Mode is changed", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.Premise, move1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.Premise, move2.EW_DropMode);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLImport;
			notify.ResponseToDropModeDialog = DropMode.Address;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_LCLExport;
			AssertEquals("Another popup is triggered", 4, notify.Events.Count);
			AssertEquals("Correct button text", "Set Loose moves with Consignor Drop Mode of 'HWL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertNull("Changing drop mode through here does not trigger Drop Mode popup", notify.LastEventYesNoArgs);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed", Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.HandHaulier, move1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.HandHaulier, move2.EW_DropMode);
		}

		public void TestJobTypeChangeDefaultsDropMode_ForContainerised()
		{
			var notify = new TestNotify();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			cartage.CartageType.ContainerizedBooking.E4_EquipmentGroup = Constants.FCLEquipmentNeeded.LiftOffOn;
			var orgHeader = Factory.New<OrgHeader>();
			var address = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			address.E2_OA_Address = orgHeader.MainAddress.PK;
			orgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;
			var containerMove1 = cartage.ContainerBookedMoves.AddNew();
			var containerMove2 = cartage.ContainerBookedMoves.AddNew();
			containerMove1.EW_E2PickupAddressID = address.PK;
			containerMove1.EW_DropMode = "";
			containerMove2.EW_E2PickupAddressID = address.PK;
			containerMove2.EW_DropMode = "";
			cartage.SetNotificationSubscriber(notify);
			notify.ResponseToDialogs = true;
			cartage.JJ_DropMode = "";
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertNull("If Job Drop Mode was empty Job Type drop mode is defaulted without asking", notify.LastDropModeEventArgs);
			AssertEquals("Straight defaulting the Drop Mode triggers Drop Mode Popup", 1, notify.Events.Count);
			AssertEquals(Constants.FCLEquipmentNeeded.SideLoader, cartage.JJ_DropMode);
			AssertEquals("Answer was yes to change drop mode on all Booked moves", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("Answer was yes to change drop mode on all Booked moves", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			notify.ClearLastEventArgs();
			notify.ResponseToDropModeDialog = DropMode.CartageType;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportPack;
			var cfsAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS);
			cfsAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("Popup is triggered", "The Address and/or Job Type Drop Modes are different from the current Drop Mode. Would you like to keep the current Cartage Drop Mode of 'SDL' or default to a new Drop Mode?", notify.LastDropModeEventArgs.Message);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Container moves with Job Type Drop Mode of 'LOF'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals(2, notify.Events.Count);
			AssertEquals("Answer was to change to the Job Type Drop Mode, so Drop Mode is changed", Constants.FCLEquipmentNeeded.LiftOffOn, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.LiftOffOn, containerMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.LiftOffOn, containerMove2.EW_DropMode);
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.Address;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("Another popup is triggered", 3, notify.Events.Count);
			AssertEquals("Correct button text", "Set Container moves with Consignor Drop Mode of 'SDL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertNull("Changing drop mode through here does not trigger Drop Mode popup", notify.LastEventYesNoArgs);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed", Constants.FCLEquipmentNeeded.SideLoader, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			cartage.Containers.DeleteAll();
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.CartageType;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("Another popup is triggered", 4, notify.Events.Count);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Cartage Drop Mode with the Job Type Drop Mode 'LOF'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Job Type Drop Mode, so Drop Mode is changed", Constants.FCLEquipmentNeeded.LiftOffOn, cartage.JJ_DropMode);
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.Address;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			AssertEquals("Another popup is triggered", 5, notify.Events.Count);
			AssertEquals("Correct button text", "Set Cartage Drop Mode with the Consignor Drop Mode 'SDL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed", Constants.FCLEquipmentNeeded.SideLoader, cartage.JJ_DropMode);
			orgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.Trailer;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCYDtoCFS;
			AssertEquals("Another popup is triggered", 6, notify.Events.Count);
			AssertEquals("Correct button text", "Set Cartage Drop Mode with the CFS Drop Mode 'TRL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed", Constants.FCLEquipmentNeeded.Trailer, cartage.JJ_DropMode);
		}

		public void TestJobTypeChangeDefaultsDropMode_ForMixed()
		{
			var notify = new TestNotify();
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			cartage.LooseBookedMoves.DeleteAll();
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.HandHaulier;
			cartage.CartageType.ContainerizedBooking.E4_EquipmentGroup = Constants.FCLEquipmentNeeded.LiftOffOn;
			var orgHeader = Factory.New<OrgHeader>();
			var cneAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageImporter);
			cneAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			var cfsAddress = cartage.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageCFS);
			cfsAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			orgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;
			orgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			var containerMove1 = cartage.ContainerBookedMoves.AddNew();
			containerMove1.EW_E2DeliveryAddressID = cfsAddress.PK;
			containerMove1.EW_DropMode = "";
			var containerMove2 = cartage.ContainerBookedMoves.AddNew();
			containerMove2.EW_E2DeliveryAddressID = cfsAddress.PK;
			containerMove2.EW_DropMode = "";
			var looseMove1 = cartage.LooseBookedMoves.AddNew();
			looseMove1.EW_E2DeliveryAddressID = cneAddress.PK;
			looseMove1.EW_DropMode = "";
			var looseMove2 = cartage.LooseBookedMoves.AddNew();
			looseMove2.EW_E2DeliveryAddressID = cneAddress.PK;
			looseMove2.EW_DropMode = "";
			cartage.SetNotificationSubscriber(notify);
			notify.ResponseToDialogs = true;
			cartage.JJ_DropMode = "";
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertNull("If Job Drop Mode was empty Job Type drop mode is defaulted without asking", notify.LastDropModeEventArgs);
			AssertEquals("Straight defaulting the Drop Mode triggers Drop Mode Popup", 1, notify.Events.Count);
			AssertEquals(Constants.LCLAIREquipmentNeeded.HandUnloadLoad, cartage.JJ_DropMode);
			AssertEquals("Answer was yes to change drop mode on all Booked moves", Constants.LCLAIREquipmentNeeded.HandUnloadLoad, looseMove1.EW_DropMode);
			AssertEquals("Answer was yes to change drop mode on all Booked moves", Constants.LCLAIREquipmentNeeded.HandUnloadLoad, looseMove2.EW_DropMode);
			AssertEquals("Containers are unaffected", "", containerMove1.EW_DropMode);
			AssertEquals("Containers are unaffected", "", containerMove2.EW_DropMode);
			notify.ClearLastEventArgs();
			notify.ResponseToDropModeDialog = DropMode.CartageType;
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("Popup is triggered", "The Address and/or Job Type Drop Modes are different from the current Drop Mode. Would you like to keep the current Cartage Drop Mode of 'HUL' or default to a new Drop Mode?", notify.LastDropModeEventArgs.Message);
			AssertEquals("Correct button text", "Set Container moves with CFS Drop Mode of 'SDL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Container moves with Job Type Drop Mode of 'LOF'\r\nSet Loose moves with Job Type Drop Mode of 'HWL'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals(2, notify.Events.Count);
			AssertEquals("Answer was to change to the Job Type Drop Mode, so Drop Mode is changed", Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.HandHaulier, looseMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.HandHaulier, looseMove2.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.LiftOffOn, containerMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.LiftOffOn, containerMove2.EW_DropMode);
			cartage.JJ_E3_NKJobType = "";
			orgHeader.MainAddress.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.Premise;
			notify.ResponseToDropModeDialog = DropMode.Address;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("Another popup is triggered", 3, notify.Events.Count);
			AssertEquals("Correct button text", "Set Container moves with CFS Drop Mode of 'SDL'\r\nSet Loose moves with Consignee Drop Mode of 'PSL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertNull("Changing drop mode through here does not trigger Drop Mode popup", notify.LastEventYesNoArgs);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.Premise, looseMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.Premise, looseMove2.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			cartage.CartageType.ContainerizedBooking.E4_EquipmentGroup = Constants.FCLEquipmentNeeded.SideLoader;
			looseMove1.EW_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			looseMove2.EW_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			looseMove1.EW_DropMode = "";
			looseMove2.EW_DropMode = "";
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.CartageType;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("Another popup is triggered", 4, notify.Events.Count);
			AssertEquals("Correct button text", "Set Loose moves with Consignee Drop Mode of 'PSL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Loose moves with Job Type Drop Mode of 'HWL'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Job Type Drop Mode, so Drop Mode is changed", Constants.LCLAIREquipmentNeeded.HandHaulier, cartage.JJ_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.HandHaulier, looseMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.LCLAIREquipmentNeeded.HandHaulier, looseMove2.EW_DropMode);
			AssertEquals("Containers unchanged", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("Containers unchanged", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.Premise;
			cartage.JJ_E3_NKJobType = "";
			looseMove1.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			looseMove1.EW_E2DeliveryAddressID = ZGuid.Empty;
			looseMove2.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			looseMove2.EW_E2DeliveryAddressID = ZGuid.Empty;
			containerMove1.EW_E2DeliveryAddressID = ZGuid.Empty;
			containerMove2.EW_E2DeliveryAddressID = ZGuid.Empty;
			notify.ResponseToDropModeDialog = DropMode.Address;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("Another popup is triggered", 5, notify.Events.Count);
			AssertEquals("Correct button text", "Set Cartage Drop Mode with the Consignee Drop Mode 'PSL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Cartage Drop Mode with the Job Type Drop Mode 'PSL'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Premise, looseMove1.EW_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Premise, looseMove2.EW_DropMode);
			AssertEquals("Containers unchanged", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("Containers unchanged", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = "";
			containerMove1.EW_E2DeliveryAddressID = cfsAddress.PK;
			containerMove1.EW_DropMode = "";
			containerMove2.EW_E2DeliveryAddressID = cfsAddress.PK;
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.CartageType;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("Another popup is triggered", 6, notify.Events.Count);
			AssertEquals("Correct button text", "Set Container moves with CFS Drop Mode of 'SDL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Container moves with Job Type Drop Mode of 'SDL'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Job Type Drop Mode, but loose drop mode was empty so dropmode remains the same", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Premise, looseMove1.EW_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Premise, looseMove2.EW_DropMode);
			AssertEquals("Containers reflect change", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("Containers reflect change", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			looseMove1.EW_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			looseMove2.EW_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			looseMove1.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			looseMove2.EW_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			orgHeader.MainAddress.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.Trailer;
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.Address;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("Another popup is triggered", 7, notify.Events.Count);
			AssertEquals("Correct button text", "Set Container moves with CFS Drop Mode of 'TRL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", null, notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed to loose drop mode", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Premise, looseMove1.EW_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Premise, looseMove2.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.Trailer, containerMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.Trailer, containerMove2.EW_DropMode);
			looseMove1.EW_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			looseMove2.EW_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Haulier;
			cartage.CartageType.LooseBooking.E4_EquipmentGroup = Constants.LCLAIREquipmentNeeded.Haulier;
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.CartageType;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("Another popup is triggered", 8, notify.Events.Count);
			AssertEquals("Correct button text", "Set Cartage Drop Mode with the Consignee Drop Mode 'PSL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Container moves with Job Type Drop Mode of 'SDL'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Job Type Drop Mode, so Drop Mode is changed to loose drop mode on Job Type", Constants.LCLAIREquipmentNeeded.Haulier, cartage.JJ_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Haulier, looseMove1.EW_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Haulier, looseMove2.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.SideLoader, containerMove1.EW_DropMode);
			AssertEquals("All moves reflect the change", Constants.FCLEquipmentNeeded.SideLoader, containerMove2.EW_DropMode);
			containerMove1.EW_E2DeliveryAddressID = ZGuid.Empty;
			containerMove1.EW_DropMode = "";
			containerMove2.EW_E2DeliveryAddressID = ZGuid.Empty;
			containerMove2.EW_DropMode = Constants.FCLEquipmentNeeded.Trailer;
			cartage.JJ_E3_NKJobType = "";
			notify.ResponseToDropModeDialog = DropMode.Address;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("Another popup is triggered", 9, notify.Events.Count);
			AssertEquals("Correct button text", "Set Cartage Drop Mode with the Consignee Drop Mode 'PSL'", notify.LastDropModeEventArgs.AddressButtonText);
			AssertEquals("Correct button text", "Set Container moves with Job Type Drop Mode of 'SDL'", notify.LastDropModeEventArgs.JobTypeButtonText);
			AssertEquals("Answer was to change to the Address Drop Mode, so Drop Mode is changed to loose drop mode on Job Type", Constants.LCLAIREquipmentNeeded.Premise, cartage.JJ_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Haulier, looseMove1.EW_DropMode);
			AssertEquals("Loose moves unchanged", Constants.LCLAIREquipmentNeeded.Haulier, looseMove2.EW_DropMode);
			AssertEquals("Containers unchanged", "", containerMove1.EW_DropMode);
			AssertEquals("Containers unchanged", Constants.FCLEquipmentNeeded.Trailer, containerMove2.EW_DropMode);
		}

		public void TestCheckTotalsDiffer()
		{
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.UpdateCartageTotalsPackQuantityVariation += new CancelEventHandler(cartage_UpdateCartageTotalsPackQuantityVariation);
			cartage.LooseBookedMoves.AddNew().EW_BookedWeight = 20;
			cartage.LooseBookedMoves.AddNew().EW_BookedVolume = 30;
			cartage.LooseBookedMoves.AddNew().EW_BookedPackCount = 5;
			cartage.CheckTotalsDiffer();
			AssertEquals(true, UpdateCartageTotalsPackQuantityVariationFired);
			cartage.JJ_Weight = 20;
			cartage.JJ_Volume = 30;
			cartage.JJ_OuterPacks = 5;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.CheckTotalsDiffer();
			AssertEquals(false, UpdateCartageTotalsPackQuantityVariationFired);
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_EmptyCYDtoCFS;
			cartage.JJ_Weight = 0;
			cartage.JJ_Volume = 0;
			cartage.JJ_OuterPacks = 0;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.CheckTotalsDiffer();
			AssertEquals(false, UpdateCartageTotalsPackQuantityVariationFired);
			DummyCartageParent parent = new DummyCartageParent(Factory);
			DummyCartageType cartageType = new DummyCartageType(parent);
			cartage.SetParent(parent);
			parent.SetCartageType(cartageType);
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			PackLine line = Factory.New<PackLine>();
			line.JL_FreightMode = FreightConstants.DeliveryPackType;
			container.PackLines.Add(line);
			line.JL_PackageCount = 2;
			line.JL_ActualWeight = 23;
			line.JL_ActualVolume = 12;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.CheckTotalsDiffer();
			AssertEquals(true, UpdateCartageTotalsPackQuantityVariationFired);
			cartage.JJ_Weight = 23;
			cartage.JJ_Volume = 12;
			cartage.JJ_OuterPacks = 2;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.CheckTotalsDiffer();
			AssertEquals(false, UpdateCartageTotalsPackQuantityVariationFired);
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.JJ_WeightUQ = "+1"; // invalid
			cartage.CheckTotalsDiffer();
			AssertEquals(true, UpdateCartageTotalsPackQuantityVariationFired);
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.JJ_WeightUQ = "KG";
			cartage.JJ_VolumeUQ = "+1"; // invalid
			cartage.CheckTotalsDiffer();
			AssertEquals(true, UpdateCartageTotalsPackQuantityVariationFired);
		}

		public void TestCheckTotalsDiffer_StandaloneContainerised()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_FCLCFStoCTO;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.UpdateCartageTotalsPackQuantityVariation += new CancelEventHandler(cartage_UpdateCartageTotalsPackQuantityVariation);
			CommonContainer container1 = cartage.ContainerBookedMoves.AddNew().Container;
			CommonContainer container2 = cartage.ContainerBookedMoves.AddNew().Container;
			container1.JC_GrossWeight = 10000m;
			container2.JC_GrossWeight = 11000m;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.CheckTotalsDiffer();
			AssertEquals(false, UpdateCartageTotalsPackQuantityVariationFired);
			cartage.JJ_Weight = 100;
			cartage.JJ_Volume = 120;
			cartage.JJ_OuterPacks = 140;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.CheckTotalsDiffer();
			AssertEquals(false, UpdateCartageTotalsPackQuantityVariationFired);
			AssertEquals(100m, cartage.JJ_Weight);
			AssertEquals(120m, cartage.JJ_Volume);
			AssertEquals((ZInt)140, cartage.JJ_OuterPacks);
		}

		public void TestCheckTotalsDiffer_WithParentContainerised()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.SetParent(dummyParent);
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_FCLCFStoCTO;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.UpdateCartageTotalsPackQuantityVariation += new CancelEventHandler(cartage_UpdateCartageTotalsPackQuantityVariation);
			CommonContainer container1 = cartage.ContainerBookedMoves.AddNew().Container;
			CommonContainer container2 = cartage.ContainerBookedMoves.AddNew().Container;
			var pack1 = Factory.New<PackLine>();
			pack1.JL_FreightMode = FreightConstants.DeliveryPackType;
			pack1.JL_PackageCount = 1;
			pack1.JL_F3_NKPackType = Constants.PkgUnit.Box;
			pack1.JL_ActualWeight = 10000m;
			pack1.JL_ActualWeightUQ = "KG";
			pack1.JL_ActualVolume = 10000m;
			pack1.JL_ActualVolumeUQ = "M3";
			container1.PackLines.Add(pack1);
			var pack2 = Factory.New<PackLine>();
			pack2.JL_FreightMode = FreightConstants.DeliveryPackType;
			pack2.JL_PackageCount = 1;
			pack2.JL_F3_NKPackType = Constants.PkgUnit.Box;
			pack2.JL_ActualWeight = 11000m;
			pack2.JL_ActualWeightUQ = "KG";
			pack2.JL_ActualVolume = 11000m;
			pack2.JL_ActualVolumeUQ = "M3";
			container2.PackLines.Add(pack2);
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.CheckTotalsDiffer();
			AssertEquals(true, UpdateCartageTotalsPackQuantityVariationFired);
			cartage.JJ_Weight = 21000m;
			cartage.JJ_Volume = 21000m;
			cartage.JJ_OuterPacks = 2;
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.CheckTotalsDiffer();
			AssertEquals(false, UpdateCartageTotalsPackQuantityVariationFired);
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.JJ_WeightUQ = "+1"; // invalid
			cartage.CheckTotalsDiffer();
			AssertEquals(true, UpdateCartageTotalsPackQuantityVariationFired);
			UpdateCartageTotalsPackQuantityVariationFired = false;
			cartage.JJ_WeightUQ = "KG";
			cartage.JJ_VolumeUQ = "+1"; // invalid
			cartage.CheckTotalsDiffer();
			AssertEquals(true, UpdateCartageTotalsPackQuantityVariationFired);
		}

		void cartage_UpdateCartageTotalsPackQuantityVariation(object sender, CancelEventArgs e)
		{
			UpdateCartageTotalsPackQuantityVariationFired = true;
		}

		bool UpdateCartageTotalsPackQuantityVariationFired;

		public void TestUpdateCartageFromLooseBookedMovesOrContainers()
		{
			//Containerised
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLCTOtoCNE, 1);
			var line = Factory.New<PackLine>();
			line.JL_FreightMode = FreightConstants.DeliveryPackType;
			line.JL_PackageCount = 2;
			line.JL_F3_NKPackType = Constants.PkgUnit.Bag;
			line.JL_ActualWeight = 23m;
			line.JL_ActualWeightUQ = Constants.Weight.Pounds;
			line.JL_ActualVolume = 12m;
			line.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			cartage.Containers.First().PackLines.Add(line);
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertCartageTotals(cartage, 2, Constants.PkgUnit.Bag, 23m, Constants.Weight.Pounds, 12m, Constants.Volume.CubicFeet);
			//Loose
			var cartageLoose = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_AirImport, 2);
			Helper.SetupBookedMove(cartageLoose.LooseBookedMoves[0], 10, Constants.PkgUnit.Bag, 11m, Constants.Weight.Pounds, 12m, Constants.Volume.CubicFeet);
			Helper.SetupBookedMove(cartageLoose.LooseBookedMoves[1], 23, Constants.PkgUnit.Bag, 24m, Constants.Weight.Pounds, 25m, Constants.Volume.CubicFeet);
			cartageLoose.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertCartageTotals(cartageLoose, 33, Constants.PkgUnit.Bag, 35m, Constants.Weight.Pounds, 37m, Constants.Volume.CubicFeet);
			Helper.SetupBookedMove(cartageLoose.LooseBookedMoves[1], 21, Constants.PkgUnit.Pallet, 14m, Constants.Weight.Kilograms, 15m, Constants.Volume.CubicMetres);
			cartageLoose.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertCartageTotals(cartageLoose, 31, Constants.PkgUnit.Piece, 41.865, Constants.Weight.Pounds, 541.72m, Constants.Volume.CubicFeet);
		}

		void AssertCartageTotals(CommonCartage cartage, ZInt packs, ZString packType, ZDecimal weight, ZString weightUnit, ZDecimal volume, ZString volumeUnit)
		{
			AssertEquals(packs, cartage.JJ_OuterPacks);
			AssertEquals(packType, cartage.JJ_F3_NKPackType);
			AssertEquals(weight, cartage.JJ_Weight);
			AssertEquals(weightUnit, cartage.JJ_WeightUQ);
			AssertEquals(volume, cartage.JJ_Volume);
			AssertEquals(volumeUnit, cartage.JJ_VolumeUQ);
		}

		public void TestLooseBookedMovesIsNotReadOnly()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			AssertEquals(false, cartage.LooseBookedMoves.ReadOnly);
			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			AssertEquals(cartage.PK, ((IJobHeaderParent)cartage).PK);
			AssertEquals(cartage.TableName, ((IJobHeaderParent)cartage).TableName);
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			AssertEquals(true, cartage.HasParent);
			AssertEquals(false, cartage.LooseBookedMoves.ReadOnly);
		}

		public void TestJobTypeChangingEventIsFired()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			jobTypeChangingEventIsFired = false;
			cartage.JJ_E3_NKJobType = "IALC";
			cartage.OnJobTypeChanging += new CancelEventHandler(cartage_OnJobTypeChanging);
			cartage.JJ_E3_NKJobType = "ESEF";
			AssertEquals(true, jobTypeChangingEventIsFired);
			jobTypeChangingEventIsFired = false;
			cartage.JJ_E3_NKJobType = "IALC";
			AssertEquals(true, jobTypeChangingEventIsFired);
			jobTypeChangingEventIsCancelled = true;
			cartage.JJ_E3_NKJobType = "ESEF";
			AssertEquals("IALC", cartage.JJ_E3_NKJobType);
		}

		void cartage_OnJobTypeChanging(object sender, CancelEventArgs e)
		{
			jobTypeChangingEventIsFired = true;
			e.Cancel = jobTypeChangingEventIsCancelled;
		}

		bool jobTypeChangingEventIsFired;
		bool jobTypeChangingEventIsCancelled;

		public void TestShouldUseParentJob()
		{
			BusinessObject declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			Factory.Save();
			CommonCartage cartage = Factory.New<CommonCartage>();
			Assert(!cartage.ShouldUseParentJob);
			AssertEquals(cartage.PK, ((IJobHeaderParent)cartage).PK);
			AssertEquals(cartage.TableName, ((IJobHeaderParent)cartage).TableName);
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			Assert(!cartage.ShouldUseParentJob);
			AssertEquals(cartage.PK, ((IJobHeaderParent)cartage).PK);
			AssertEquals(cartage.TableName, ((IJobHeaderParent)cartage).TableName);
			cartage.JJ_Status = "v1";
			Assert(cartage.ShouldUseParentJob);
			AssertEquals(declaration.PK, ((IJobHeaderParent)cartage).PK);
			AssertEquals(declaration.TableName, ((IJobHeaderParent)cartage).TableName);
			cartage.JJ_Status = "v2";
			Assert(!cartage.ShouldUseParentJob);
			AssertEquals(cartage.PK, ((IJobHeaderParent)cartage).PK);
			AssertEquals(cartage.TableName, ((IJobHeaderParent)cartage).TableName);
		}

		public void TestContainersType()
		{
			var ref20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull("Precondition", ref20GP);
			var cartage = Factory.New<CartageForTest>();
			AssertEquals("", cartage.ContainersType);
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_RC = ref20GP.PK;
			AssertEquals("20GP", cartage.ContainersType);
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			AssertEquals("Many", cartage.ContainersType);
			container2.JC_RC = ref20GP.PK;
			AssertEquals("20GP", cartage.ContainersType);
			var ref40GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
			container2.JC_RC = ref40GP.PK;
			AssertEquals("Many", cartage.ContainersType);
		}

		public void TestPickupCompleted()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			CompletedDummyCartageType cartageType = new CompletedDummyCartageType(dummyParent);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			ZDateTime now = ZDateTime.Now;
			CommonBookedCtgMove move1 = cartage.ContainerBookedMoves.AddNew();
			CommonBookedCtgMove move2 = cartage.ContainerBookedMoves.AddNew();
			CommonContainer cnt1 = move1.Container;
			CommonContainer cnt2 = move2.Container;
			cnt1.JC_ContainerNum = "C1";
			cnt2.JC_ContainerNum = "C2";
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move2.CartageLegs.AddNew();
			JobDocAddress importer = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageImporter);
			leg1.JU_E2PickupAddressID = importer.PK;
			cartage.PickupCompleted(leg1, DocAddressType.LocalCartageImporter, now);
			AssertEquals("ContainerPickupCompleted", cartageType.method);
			AssertEquals("C1", cartageType.containerNo);
			AssertEquals(DocAddressType.LocalCartageImporter, cartageType.addressType);
			AssertEquals(now, cartageType.timeOut);
			AssertEquals(1, cartageType.hit);
			leg2.JU_E2PickupAddressID = importer.PK;
			cartage.PickupCompleted(leg2, DocAddressType.LocalCartageImporter, now.AddDays(1));
			AssertEquals("ContainerPickupCompleted", cartageType.method);
			AssertEquals("C2", cartageType.containerNo);
			AssertEquals(DocAddressType.LocalCartageImporter, cartageType.addressType);
			AssertEquals(now.AddDays(1), cartageType.timeOut);
			AssertEquals(2, cartageType.hit);
		}

		public void TestPickupCompleted_LegIsNull()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			CompletedDummyCartageType cartageType = new CompletedDummyCartageType(dummyParent);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);

			var move = cartage.ContainerBookedMoves.AddNew();

			AssertNoExceptionThrown(() =>
			{
				cartage.PickupCompleted(null, DocAddressType.LocalCartageImporter, ZDateTime.Now);
			});

			AssertEquals("PickupCompleted should not have been called", "", cartageType.method);
		}

		public void TestPickupCompleted_ContainerIsNotNullAndCartageTypeIsNull()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			dummyParent.CartageTypesReturnsEmptyArray = true;
			ICartageParent dummyCartageParent = dummyParent;
			CompletedDummyCartageType cartageType = new CompletedDummyCartageType(dummyParent);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);

			var move = cartage.ContainerBookedMoves.AddNew();
			var cnt = move.Container;
			cnt.JC_ContainerNum = "C1";

			var leg = move.CartageLegs.AddNew();

			AssertNoExceptionThrown(() =>
			{
				cartage.PickupCompleted(leg, DocAddressType.LocalCartageImporter, ZDateTime.Now);
			});

			AssertEquals("PickupCompleted should not have been called", "", cartageType.method);
		}

		public void TestPickupCompleted_ContainerIsNullAndCartageTypeIsNull()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			dummyParent.CartageTypesReturnsEmptyArray = true;
			ICartageParent dummyCartageParent = dummyParent;
			CompletedDummyCartageType cartageType = new CompletedDummyCartageType(dummyParent);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);

			var leg = Factory.New<CommonCartageLeg>();

			AssertNoExceptionThrown(() =>
			{
				cartage.PickupCompleted(leg, DocAddressType.LocalCartageImporter, ZDateTime.Now);
			});

			AssertEquals("PickupCompleted should not have been called", "", cartageType.method);
		}

		public void TestDeliveryCompleted_LegIsNull()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			CompletedDummyCartageType cartageType = new CompletedDummyCartageType(dummyParent);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);

			var move = cartage.ContainerBookedMoves.AddNew();

			AssertNoExceptionThrown(() =>
			{
				cartage.DeliveryCompleted(null, DocAddressType.LocalCartageImporter, ZDateTime.Now);
			});

			AssertEquals("DeliveryCompleted should not have been called", "", cartageType.method);
		}

		public void TestDeliveryCompleted_ContainerIsNotNullAndCartageTypeIsNull()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			dummyParent.CartageTypesReturnsEmptyArray = true;
			ICartageParent dummyCartageParent = dummyParent;
			CompletedDummyCartageType cartageType = new CompletedDummyCartageType(dummyParent);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);

			var move = cartage.ContainerBookedMoves.AddNew();
			var cnt = move.Container;
			cnt.JC_ContainerNum = "C1";

			var leg = move.CartageLegs.AddNew();

			AssertNoExceptionThrown(() =>
			{
				cartage.DeliveryCompleted(leg, DocAddressType.LocalCartageImporter, ZDateTime.Now);
			});

			AssertEquals("DeliveryCompleted should not have been called", "", cartageType.method);
		}

		public void TestDeliveryCompleted_ContainerIsNullAndCartageTypeIsNull()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			dummyParent.CartageTypesReturnsEmptyArray = true;
			ICartageParent dummyCartageParent = dummyParent;
			CompletedDummyCartageType cartageType = new CompletedDummyCartageType(dummyParent);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);

			var leg = Factory.New<CommonCartageLeg>();

			AssertNoExceptionThrown(() =>
			{
				cartage.DeliveryCompleted(leg, DocAddressType.LocalCartageImporter, ZDateTime.Now);
			});

			AssertEquals("DeliveryCompleted should not have been called", "", cartageType.method);
		}

		public void TestGetDocAddressRequirement()
		{
			var cartage = Factory.New<CommonCartage>();
			var iCartage = (IDocAddresses)cartage;
			AssertNotNull(iCartage.GetDocAddressRequirement(DocAddressType.LocalCartageCFS));
			AssertNotNull(iCartage.GetDocAddressRequirement(DocAddressType.LocalCartageCTO));
			AssertNotNull(iCartage.GetDocAddressRequirement(DocAddressType.LocalCartageExporter));
			AssertNotNull(iCartage.GetDocAddressRequirement(DocAddressType.LocalCartageImporter));
			AssertNotNull(iCartage.GetDocAddressRequirement(DocAddressType.LocalCartageYard));
			AssertNotNull(iCartage.GetDocAddressRequirement(DocAddressType.LocalCartageService));
			AssertNotNull(iCartage.GetDocAddressRequirement(DocAddressType.LocalCartageWarehouse));
			AssertNotNull(iCartage.GetDocAddressRequirement(DocAddressType.LocalCartageMSC));
			AssertNull(iCartage.GetDocAddressRequirement(DocAddressType.BuyingParty));
		}

		public void TestAddJobDocAddress()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Org1";
			OrgAddress org1Address1 = org1.MainAddress;
			org1Address1.OA_Code = "org1Address1";
			OrgAddress org1Address2 = org1.Addresses.AddNew();
			org1Address2.OA_Code = "org1Address2";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Org2";
			OrgAddress org2Address1 = org2.MainAddress;
			org2Address1.OA_Code = "org2Address1";
			OrgAddress org2Address2 = org2.Addresses.AddNew();
			org2Address2.OA_Code = "org2Address2";
			OrgHeader org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "Org3";
			OrgAddress org3Address1 = org3.MainAddress;
			org3Address1.OA_Code = "org3Address1";
			OrgAddress org3Address2 = org3.Addresses.AddNew();
			org3Address2.OA_Code = "org3Address2";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLExport;
			cartage.FirstDocAddress.E2_OA_Address = org1Address1.PK;
			cartage.SecondDocAddress.E2_OA_Address = org2Address1.PK;
			AssertEquals(2, cartage.DocAddresses.Count);
			AssertEquals(2, GetPersistentJobDocAddresses(cartage).Count);
			JobDocAddress docAddress3 = cartage.AddJobDocAddress(org3Address1.PK);
			AssertNotNull(docAddress3);
			AssertEquals("Still 3, org3Address1 (ORG3) is not part of Local Transport", 3, GetPersistentJobDocAddresses(cartage).Count);
			cartage.FirstDocAddress.E2_OA_Address = org3Address1.PK;
			AssertEquals(3, GetPersistentJobDocAddresses(cartage).Count);
			docAddress3 = cartage.AddJobDocAddress(org3Address2.PK);
			AssertNotNull(docAddress3);
			AssertEquals("Now Org is a part of cartage, so allow address 4 to be added", 4, GetPersistentJobDocAddresses(cartage).Count);
		}

		public void TestOnBeforeDocAddressDeleted()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var move = cartage.BookedMovesCollection[0];
			var cnr = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageExporter, "CNR", "CNR Address", "2000", "Sydney", "AUSYD", true);
			var cne = Helper.CreateJobDocAddress(cartage, DocAddressType.LocalCartageImporter, "CNE", "CNE Address", "2000", "Sydney", "AUSYD", true);
			move.EW_E2PickupAddressID = cnr.PK;
			move.EW_E2WaitPointAddressID = cne.PK;
			move.EW_E2DeliveryAddressID = cne.PK;
			AssertNotNull(move.EW_E2PickupAddressID);
			AssertNotNull(move.EW_E2WaitPointAddressID);
			AssertNotNull(move.EW_E2DeliveryAddressID);
			var leg = move.CartageLegs.AddNew();
			leg.JU_E2PickupAddressID = cnr.PK;
			leg.JU_E2WaitPointAddressID = cne.PK;
			leg.JU_E2DeliveryAddressID = cne.PK;
			AssertNotNull(leg.JU_E2PickupAddressID);
			AssertNotNull(leg.JU_E2WaitPointAddressID);
			AssertNotNull(leg.JU_E2DeliveryAddressID);
			cnr.Delete();
			AssertEquals(ZGuid.Empty, move.EW_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg.JU_E2PickupAddressID);
			AssertNotNull(move.EW_E2WaitPointAddressID);
			AssertNotNull(move.EW_E2DeliveryAddressID);
			AssertNotNull(leg.JU_E2WaitPointAddressID);
			AssertNotNull(leg.JU_E2DeliveryAddressID);
			cne.Delete();
			AssertEquals(ZGuid.Empty, move.EW_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2DeliveryAddressID);
			AssertEquals(ZGuid.Empty, leg.JU_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg.JU_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, leg.JU_E2DeliveryAddressID);
		}

		public void TestDontRemoveJobDocAddressIfNotInUse()
		{
			OrgHeader org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "Org1";
			OrgAddress org1Address1 = org1.MainAddress;
			org1Address1.OA_Code = "org1Address1";
			OrgAddress org1Address2 = org1.Addresses.AddNew();
			org1Address2.OA_Code = "org1Address2";
			OrgHeader org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "Org2";
			OrgAddress org2Address1 = org2.MainAddress;
			org2Address1.OA_Code = "org2Address1";
			OrgAddress org2Address2 = org2.Addresses.AddNew();
			org2Address2.OA_Code = "org2Address2";
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLExport;
			cartage.FirstDocAddress.E2_OA_Address = org1Address1.PK;
			cartage.SecondDocAddress.E2_OA_Address = org2Address1.PK;
			AssertEquals(2, cartage.DocAddresses.Count);
			AssertEquals(2, GetPersistentJobDocAddresses(cartage).Count);
			JobDocAddress org1DocAddress2 = cartage.AddJobDocAddress(org1Address2.PK);
			AssertEquals("Address 2 is from org 1, so should be added", 3, GetPersistentJobDocAddresses(cartage).Count);
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();
			move1.EW_E2PickupAddressID = cartage.FirstDocAddress.PK;
			CommonCartageLeg leg1 = move1.CartageLegs.AddNew();
			leg1.JU_E2PickupAddressID = cartage.FirstDocAddress.PK;
			CommonBookedCtgMove move2 = cartage.LooseBookedMoves.AddNew();
			move2.EW_E2PickupAddressID = org1DocAddress2.PK;
			CommonCartageLeg leg2 = move1.CartageLegs.AddNew();
			leg2.JU_E2PickupAddressID = org1DocAddress2.PK;
			AssertEquals(3, GetPersistentJobDocAddresses(cartage).Count);
			leg1.JU_E2PickupAddressID = ZGuid.Empty;
			AssertEquals("Should still exist", 3, GetPersistentJobDocAddresses(cartage).Count);
			leg2.JU_E2PickupAddressID = ZGuid.Empty;
			AssertEquals("Should still exist", 3, GetPersistentJobDocAddresses(cartage).Count);
			move1.EW_E2PickupAddressID = ZGuid.Empty;
			AssertEquals("Should still exist, is on front screen", 3, GetPersistentJobDocAddresses(cartage).Count);
			move2.EW_E2PickupAddressID = ZGuid.Empty;
			AssertEquals("Should still exist", 3, GetPersistentJobDocAddresses(cartage).Count);
		}

		public void TestEnsureAddressRetentionWhenChangingJobType()
		{
			var cto = Helper.CreateOrgHeader("CTO", "CTO");
			var cfs = Helper.CreateOrgHeader("CFS", "CFS");
			var cyd = Helper.CreateOrgHeader("CYD", "CYD");
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(0, cartage.DocAddresses.Count);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportUnpack;
			cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = cfs.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = cyd.MainAddress.PK;
			AssertEquals(3, cartage.DocAddresses.Count);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			AssertEquals("CNE added", 4, cartage.DocAddresses.Count);
			AssertEquals(cto.MainAddress.PK, cartage.FirstDocAddress.E2_OA_Address);
			AssertEquals(true, cartage.SecondDocAddress.E2_OA_Address.IsEmpty);
			AssertEquals(cyd.MainAddress.PK, cartage.ThirdDocAddress.E2_OA_Address);
			AssertNull(cartage.FourthDocAddress);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCTOtoCNE;
			AssertEquals("The same addresses", 4, cartage.DocAddresses.Count);
			AssertEquals(cto.MainAddress.PK, cartage.FirstDocAddress.E2_OA_Address);
			AssertEquals(true, cartage.SecondDocAddress.E2_OA_Address.IsEmpty);
			AssertNull(cartage.ThirdDocAddress);
			AssertNull(cartage.FourthDocAddress);
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var cartage_OtherFactory = otherFactory.Load<CommonCartage>(cartage.PK);
			AssertEquals("The same addresses", 4, cartage_OtherFactory.DocAddresses.Count);
			AssertEquals(cto.MainAddress.PK, cartage_OtherFactory.FirstDocAddress.E2_OA_Address);
			AssertEquals(true, cartage_OtherFactory.SecondDocAddress.E2_OA_Address.IsEmpty);
			AssertNull(cartage_OtherFactory.ThirdDocAddress);
			AssertNull(cartage_OtherFactory.FourthDocAddress);
			cartage_OtherFactory.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals("The same addresses", 4, cartage_OtherFactory.DocAddresses.Count);
			AssertEquals(cto.MainAddress.PK, cartage_OtherFactory.FirstDocAddress.E2_OA_Address);
			AssertEquals(cfs.MainAddress.PK, cartage_OtherFactory.SecondDocAddress.E2_OA_Address);
			AssertEquals(cyd.MainAddress.PK, cartage_OtherFactory.ThirdDocAddress.E2_OA_Address);
			AssertEquals(true, cartage_OtherFactory.FourthDocAddress.E2_OA_Address.IsEmpty);
		}

		public void TestMainAddressesBuiltFromLegs()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			var cto = Helper.CreateOrgHeader("CTO", "CTO");
			var cfs = Helper.CreateOrgHeader("CFS", "CFS");
			var cne = Helper.CreateOrgHeader("CNE", "CNE");
			var cyd = Helper.CreateOrgHeader("CYD", "CYD");
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			AssertEquals(0, cartage.DocAddresses.Count);
			AssertNull(cartage.FirstDocAddress);
			AssertNull(cartage.SecondDocAddress);
			AssertNull(cartage.ThirdDocAddress);
			AssertNull(cartage.FourthDocAddress);
			var docCTO = cartage.DocAddresses.AddNew(cto.MainAddress, DocAddressType.LocalCartageCTO);
			var docCFS = cartage.DocAddresses.AddNew(cfs.MainAddress, DocAddressType.LocalCartageCFS);
			var docCNE = cartage.DocAddresses.AddNew(cne.MainAddress, DocAddressType.LocalCartageImporter);
			var docCYD = cartage.DocAddresses.AddNew(cyd.MainAddress, DocAddressType.LocalCartageYard);
			AssertNull(cartage.FirstDocAddress);
			AssertNull(cartage.SecondDocAddress);
			AssertNull(cartage.ThirdDocAddress);
			AssertNull(cartage.FourthDocAddress);
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			var move = cartage.GetBookedMoves(container).First();
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			leg1.JU_E2PickupAddressID = docCTO.PK;
			leg1.JU_E2WaitPointAddressID = docCFS.PK;
			leg1.JU_E2DeliveryAddressID = docCNE.PK;
			leg2.JU_E2PickupAddressID = docCNE.PK;
			leg2.JU_E2DeliveryAddressID = docCYD.PK;
			AssertEquals(4, cartage.DocAddresses.Count);
			AssertEquals(cto.MainAddress.PK, cartage.FirstDocAddress.E2_OA_Address);
			AssertEquals(cfs.MainAddress.PK, cartage.SecondDocAddress.E2_OA_Address);
			AssertEquals(cne.MainAddress.PK, cartage.ThirdDocAddress.E2_OA_Address);
			AssertEquals(cyd.MainAddress.PK, cartage.FourthDocAddress.E2_OA_Address);
		}

		public void TestLocalClientPropertyDoesNotThrowException()
		{
			var cartage = Factory.New<CommonCartage>();
			using (JobHeader newJob = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				var newOrg = Factory.NewWithValidTestData<OrgHeader>();
				cartage.LocalClientPK = newOrg.PK;
				AssertNotNull("cartage.LocalClient", cartage.LocalClient);
				newJob.Delete();
			}

			AssertNoExceptionThrown(() =>
			{
				var poke = cartage.LocalClient;
			});
		}

		public void TestDefaultLocalClientAddressToARAddressWithMultipleLanguages()
		{
			GlbCompany.CurrentCompany.OrgProxy.OH_Language = Constants.Languages.ChineseTraditional;
			var cartage = Factory.New<CommonCartage>();
			using (JobHeader newJob = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				var newOrg = Factory.NewWithValidTestData<OrgHeader>();
				newOrg.OH_Language = Constants.Languages.ChineseTraditional;
				var officeAddress = newOrg.Addresses.AddNew();
				officeAddress.OA_Language = Constants.Languages.English;
				officeAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office);
				officeAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office);
				cartage.LocalClientPK = newOrg.PK;
				AssertEquals(officeAddress.PK, cartage.LocalClientAddressPK);
				var receivablesAddress = newOrg.Addresses.AddNew();
				receivablesAddress.OA_Language = Constants.Languages.ChineseTraditional;
				receivablesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
				receivablesAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
				cartage.LocalClientPK = ZGuid.Empty;
				cartage.LocalClientPK = newOrg.PK;
				AssertEquals(receivablesAddress.PK, cartage.LocalClientAddressPK);
				var receivablesAddress2 = newOrg.Addresses.AddNew();
				receivablesAddress2.OA_Language = Constants.Languages.English;
				receivablesAddress2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
				receivablesAddress2.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
				cartage.LocalClientPK = ZGuid.Empty;
				cartage.LocalClientPK = newOrg.PK;
				AssertEquals(receivablesAddress.PK, cartage.LocalClientAddressPK);
			}
		}

		public void TestJobCompletion()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			CommonBookedCtgMove move = cartage.ContainerBookedMoves.AddNew();
			CommonContainer container = move.Container;
			move.CartageLegs.DeleteAll();
			CommonCartageLeg leg1 = move.CartageLegs.AddNew();
			CommonCartageLeg leg2 = move.CartageLegs.AddNew();
			Assert("Precondition: Completed Date should not be set.", cartage.JJ_A_JCL.IsEmpty);
			var leg1PickupTime = ZDateTime.Now.AddHours(-3);
			var leg1DeliverTime = ZDateTime.Now.AddHours(-2);
			var leg2PickupTime = ZDateTime.Now.AddHours(-1);
			var leg2DeliverTime = ZDateTime.Now;
			leg1.JU_PickupTimeIn = leg1PickupTime.AddMinutes(-2);
			leg1.JU_PickupTimeOut = leg1PickupTime;
			leg1.JU_DeliverTimeIn = leg1DeliverTime.AddMinutes(-2);
			leg1.JU_DeliverTimeOut = leg1DeliverTime;
			Assert("Completed Date should not be set after first leg completed.", cartage.JJ_A_JCL.IsEmpty);
			leg2.JU_AdditionalService = Core.Constants.CartageAdditional.Futile;
			AssertEquals("Completed Date should be set to JU_DeliverTimeOut of first leg when first leg is completed and other leg is Futile.", leg1DeliverTime, cartage.JJ_A_JCL);
			leg2.JU_AdditionalService = "";
			Assert("Completed Date should be cleared when first leg is completed and other leg is no longer Futile.", cartage.JJ_A_JCL.IsEmpty);
			leg2.JU_AdditionalService = Core.Constants.CartageAdditional.Futile;
			AssertEquals("Completed Date should be set to JU_DeliverTimeOut of first leg when first leg is completed and other leg changes back to Futile.", leg1DeliverTime, cartage.JJ_A_JCL);
			ZDateTime manualEntry = ZDateTime.Now.AddMinutes(-1);
			cartage.JJ_A_JCL = manualEntry; //override
			leg2.JU_AdditionalService = "";
			AssertEquals("Completed Date should remain as the manually set time (when not equal to the last leg time)", manualEntry, cartage.JJ_A_JCL);
			leg2.JU_PickupTimeIn = leg2PickupTime.AddMinutes(-2);
			leg2.JU_PickupTimeOut = leg2PickupTime;
			leg2.JU_DeliverTimeIn = leg2DeliverTime.AddMinutes(-2);
			leg2.JU_DeliverTimeOut = leg2DeliverTime;
			AssertEquals("Completed Date should remain as the manually set time (when not equal to the last leg time)", manualEntry, cartage.JJ_A_JCL);
			cartage.JJ_A_JCL = ZDateTime.Empty;
			leg2.JU_DeliverTimeOut = ZDateTime.Empty;
			leg2.JU_DeliverTimeOut = leg2DeliverTime;
			AssertEquals("Completed Date should be set to JU_DeliverTimeOut of second leg on both the first and second legs being completed (when not set).", leg2DeliverTime, cartage.JJ_A_JCL);
			cartage.JJ_A_JCL = leg2DeliverTime; //override ignored
			move.CartageLegs.AddNew();
			Assert("Completed Date should be cleared (when equal to the last leg time) on addition of a new leg", cartage.JJ_A_JCL.IsEmpty);
			cartage.JJ_A_JCL = manualEntry; //override
			move.CartageLegs.AddNew();
			AssertEquals("Completed Date should remain as the manually set time (when not equal to the last leg time) on addition of a new leg", manualEntry, cartage.JJ_A_JCL);
		}

		public void TestIJobInvoicingPlugIn_OnJobCreated()
		{
			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			var cartage = Helper.CreateInternalCartage(shipment);
			Assert("Precondition", cartage.Job.JH_JH_ParentJob.IsEmpty);
			Assert("Precondition", cartage.Job.JH_OA_LocalChargesAddr.IsEmpty);
			shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S2";
			new JobHeader.Loader(shipment).TryLoadOrCreate().JH_GE = GlbDepartment.CurrentDepartment.PK;
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			shipment.Job.JH_OA_LocalChargesAddr = address1.PK;
			cartage = Helper.CreateInternalCartage(shipment);
			AssertEquals(shipment.Job.PK, cartage.Job.JH_JH_ParentJob);
			AssertAddressAndNotChangedIfLoadedInNewFactory(Factory, shipment, cartage, address1, address1);
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.Header.OH_Code = "OH2";
			shipment.Job.JH_OA_LocalChargesAddr = address2.PK;
			AssertAddressAndNotChangedIfLoadedInNewFactory(Factory, shipment, cartage, address2, address1);
			var address3 = Factory.NewWithValidTestData<OrgAddress>();
			address3.Header.OH_Code = "OH3";
			cartage.Job.JH_OA_LocalChargesAddr = address3.PK;
			AssertAddressAndNotChangedIfLoadedInNewFactory(Factory, shipment, cartage, address2, address3);
		}

		void AssertAddressAndNotChangedIfLoadedInNewFactory(BusinessObjectFactory factory, CommonShipment shipment, CommonCartage cartage, OrgAddress shipmentAddress, OrgAddress cartageAddress)
		{
			AssertEquals(shipmentAddress.PK, shipment.Job.JH_OA_LocalChargesAddr);
			AssertEquals(cartageAddress.PK, cartage.Job.JH_OA_LocalChargesAddr);
			factory.Save();
			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = (CommonShipment)newFactory.Load<IForwardingShipment>(shipment.PK);
			var cartageInNewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			new JobHeader.Loader(cartageInNewFactory).TryLoadOrCreate();
			AssertEquals(shipmentAddress.PK, shipmentInNewFactory.Job.JH_OA_LocalChargesAddr);
			AssertEquals(cartageAddress.PK, cartageInNewFactory.Job.JH_OA_LocalChargesAddr);
		}

		[ExpectNoExceptions]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestIJobInvoicingPlugIn_OnJobCreated_DeletingAllJobHeaders()
		{
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = company2.Branches.AddNew();
			branch2.GB_Code = "DEF";
			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			var cartage = Helper.CreateInternalCartage(shipment);
			var job2 = new JobHeader.Loader(cartage).TryCreate(branch2); // create cartage branch first, so it has no shipment job header to link to.
			var shipJob1 = new JobHeader.Loader(shipment).TryCreate(GlbBranch.CurrentBranch);
			var shipJob2 = new JobHeader.Loader(shipment).TryCreate(branch2);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cartageInNewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			AssertExceptionThrown<InvalidOperationException>(() => cartageInNewFactory.Delete()); // There will have an exception when job.Delete becausejob cannot be deleted.
		}

		public void TestHouseBillNumber()
		{
			IJobInvoicingPlugIn cartage = Factory.New<CommonCartage>();
			((CommonCartage)cartage).JJ_WaybillNumber = "W0005660";
			AssertEquals("W0005660", cartage.InvoicingSupporter.HouseBillNumber);
		}

		public void TestCommonCartageInvoicingSupporter_OverriddenDepartmentPK()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ShippingTransportMode = "";
			cartage.JJ_Direction = "";
			cartage.JJ_ContainerMode = "";
			var supporter = cartage.InvoicingSupporter;
			AssertEquals(Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, (ZString)"TOT").PK, supporter.OverriddenDepartmentPK);
			var jobType = Factory.LoadTop1<CommonCartageType>(new ZQuery());
			jobType.E3_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			cartage.JJ_E3_NKJobType = jobType.E3_JobType;
			AssertEquals(jobType.E3_GE, supporter.OverriddenDepartmentPK);
			cartage.JJ_E3_NKJobType = "";
			AssertDepartment(cartage, "Cartage Domestic Delivery", "TDD", "", Constants.CartageDirection.Destination, "");
			AssertDepartment(cartage, "Cartage Domestic Pickup", "TDP", "", Constants.CartageDirection.Origin, "");
			AssertDepartment(cartage, "Cartage Export Air", "TEA", Constants.TransportModes.Air, Constants.CartageDirection.Export, "");
			AssertDepartment(cartage, "Cartage Export Sea", "TES", Constants.TransportModes.Sea, Constants.CartageDirection.Export, "");
			AssertDepartment(cartage, "Cartage FCL", "TFC", "", "", Constants.CartageContainerMode.Containerized);
			AssertDepartment(cartage, "Cartage FCL Rail", "TFL", Constants.TransportModes.Rail, "", Constants.CartageContainerMode.Containerized);
			AssertDepartment(cartage, "Cartage Import Air", "TIA", Constants.TransportModes.Air, Constants.CartageDirection.Import, "");
			AssertDepartment(cartage, "Cartage Import Sea", "TIS", Constants.TransportModes.Sea, Constants.CartageDirection.Import, "");
			AssertDepartment(cartage, "Cartage Air", "TLA", Constants.TransportModes.Air, "", "");
			AssertDepartment(cartage, "Cartage LCL Sea", "TLC", Constants.TransportModes.Sea, "", Constants.CartageContainerMode.Loose);
			AssertDepartment(cartage, "Cartage LCL Rail", "TLL", Constants.TransportModes.Rail, "", Constants.CartageContainerMode.Loose);
			AssertDepartment(cartage, "Cartage Other", "TOT", "", "", "");
		}

		void AssertDepartment(CommonCartage cartage, ZString departmentDescription, ZString departmentCode, ZString transportMode, ZString direction, ZString containerMode)
		{
			var supporter = cartage.InvoicingSupporter;
			cartage.JJ_ShippingTransportMode = transportMode;
			cartage.JJ_Direction = direction;
			cartage.JJ_ContainerMode = containerMode;
			AssertEquals(departmentDescription, Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode).PK, supporter.OverriddenDepartmentPK);
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TDD()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(string.Empty, Constants.CartageDirection.Destination, string.Empty, "TDD");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TDP()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(string.Empty, Constants.CartageDirection.Origin, string.Empty, "TDP");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TEA()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(Constants.TransportModes.Air, Constants.CartageDirection.Export, string.Empty, "TEA");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TES()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(Constants.TransportModes.Sea, Constants.CartageDirection.Export, string.Empty, "TES");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TFC()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(string.Empty, string.Empty, Constants.CartageContainerMode.Containerized, "TFC");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TFL()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(Constants.TransportModes.Rail, string.Empty, Constants.CartageContainerMode.Containerized, "TFL");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TIA()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(Constants.TransportModes.Air, Constants.CartageDirection.Import, string.Empty, "TIA");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TIS()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(Constants.TransportModes.Sea, Constants.CartageDirection.Import, string.Empty, "TIS");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TLA()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(Constants.TransportModes.Air, string.Empty, string.Empty, "TLA");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TLC()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(Constants.TransportModes.Sea, string.Empty, Constants.CartageContainerMode.Loose, "TLC");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TLL()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(Constants.TransportModes.Rail, string.Empty, Constants.CartageContainerMode.Loose, "TLL");
		}

		public void TestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled_TOT()
		{
			CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(string.Empty, string.Empty, string.Empty, "TOT");
		}

		void CoreTestCommonCartageInvoicingSupport_OverriddenDepartmentPK_WhenLocalCartageDepartmentsAreDisabled(ZString transportMode, ZString direction, ZString containerMode, ZString usualDepartmentCode)
		{
			var cartageOtherDepartment = Factory.LoadFromUniqueKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, (ZString)"TOT");
			cartageOtherDepartment.GE_IsActive = true;

			var allSystemLocalCartageDepartmentsQuery = new ZQuery(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "T"), new ZQuery(GlbDepartmentSchema.GE_SystemCode, true));
			var allSystemLocalCartageDepartments = Factory.Load<GlbDepartment>(allSystemLocalCartageDepartmentsQuery).OrderByDescending(d => d.GE_Code);
			foreach (var department in allSystemLocalCartageDepartments)
			{
				department.GE_IsActive = true;
			}
			var usualCartageDepartment = allSystemLocalCartageDepartments.Single(d => d.GE_Code == usualDepartmentCode);

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			cartage.JJ_ShippingTransportMode = transportMode;
			cartage.JJ_Direction = direction;
			cartage.JJ_ContainerMode = containerMode;

			CombineAssertions("Department selection should fall back correctly depending on which departments are marked as inactive", () =>
			{
				AssertEquals(FormattableString.Invariant($"When {usualDepartmentCode} department is active, should choose {usualDepartmentCode} department"), usualCartageDepartment.PK, cartage.InvoicingSupporter.OverriddenDepartmentPK);

				usualCartageDepartment.GE_IsActive = false;

				foreach (var currentFallbackDepartment in allSystemLocalCartageDepartments)
				{
					if (currentFallbackDepartment.GE_Code == usualDepartmentCode)
					{
						continue;
					}

					AssertEquals(
						FormattableString.Invariant($"When {usualDepartmentCode} is inactive, should choose first system Local Cartage department in order of GE_Code descending, currently {currentFallbackDepartment.GE_Code}"),
						currentFallbackDepartment.PK,
						cartage.InvoicingSupporter.OverriddenDepartmentPK);

					currentFallbackDepartment.GE_IsActive = false;
				}
				AssertEquals(FormattableString.Invariant($"When all system local cartage departments are inactive, it should choose {usualDepartmentCode} department anyway"), usualCartageDepartment.PK, cartage.InvoicingSupporter.OverriddenDepartmentPK);
			});
		}

		public void TestCommonCartageInvoicingSupporter_Consignor()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = "";
			var supporter = cartage.InvoicingSupporter;
			AssertNull(supporter.Consignor);
			var consignorNoRelated = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var cnrAIRTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			var cnrFCLTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			var cnrLCLTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			var cneAIRTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			var cneFCLTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			var cneLCLTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			consignor.SetRelatedParty(cnrAIRTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			consignor.SetRelatedParty(cnrFCLTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignor.SetRelatedParty(cnrLCLTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignee.SetRelatedParty(cneAIRTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			consignee.SetRelatedParty(cneFCLTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignee.SetRelatedParty(cneLCLTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			Factory.Save();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirExport;
				AssertEquals("No Addresses set", null, supporter.Consignor);
				cartage.FirstDocAddress.OrganisationPK = consignorNoRelated.PK;
				AssertEquals("No related Party", consignorNoRelated, supporter.Consignor);
				cartage.FirstDocAddress.E2_OA_Address = ZGuid.Empty;
				cartage.LocalClientAddressPK = ZGuid.Empty;
				cartage.FirstDocAddress.OrganisationPK = consignor.PK;
				AssertEquals("CNR AIR", cnrAIRTransportBillTo, supporter.Consignor);
				cartage.JJ_E3_NKJobType = "";
				AssertEquals("CNR AIR", cnrAIRTransportBillTo, supporter.Consignor);
			}

			cartage = Factory.New<CommonCartage>();
			supporter = cartage.InvoicingSupporter;
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLSHPtoCTO;
				cartage.FirstDocAddress.OrganisationPK = consignor.PK;
				AssertEquals("CNR FCL", cnrFCLTransportBillTo, supporter.Consignor);
				cartage.JJ_E3_NKJobType = "";
				AssertEquals("CNR FCL", cnrFCLTransportBillTo, supporter.Consignor);
			}

			cartage = Factory.New<CommonCartage>();
			supporter = cartage.InvoicingSupporter;
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLExport;
				cartage.FirstDocAddress.OrganisationPK = consignor.PK;
				AssertEquals("CNR LCL", cnrLCLTransportBillTo, supporter.Consignor);
				cartage.JJ_E3_NKJobType = "";
				AssertEquals("CNR LCL", cnrLCLTransportBillTo, supporter.Consignor);
			}

			cartage = Factory.New<CommonCartage>();
			supporter = cartage.InvoicingSupporter;
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
				cartage.SecondDocAddress.OrganisationPK = consignee.PK;
				AssertEquals("CNE AIR", cneAIRTransportBillTo, supporter.Consignor);
				cartage.JJ_E3_NKJobType = "";
				AssertEquals("CNE AIR", cneAIRTransportBillTo, supporter.Consignor);
			}

			cartage = Factory.New<CommonCartage>();
			supporter = cartage.InvoicingSupporter;
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLCTOtoCNE;
				cartage.SecondDocAddress.OrganisationPK = consignee.PK;
				AssertEquals("CNE FCL", cneFCLTransportBillTo, supporter.Consignor);
				cartage.JJ_E3_NKJobType = "";
				AssertEquals("CNE FCL", cneFCLTransportBillTo, supporter.Consignor);
			}

			cartage = Factory.New<CommonCartage>();
			supporter = cartage.InvoicingSupporter;
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLImport;
				cartage.SecondDocAddress.OrganisationPK = consignee.PK;
				AssertEquals("CNE LCL", cneLCLTransportBillTo, supporter.Consignor);
				cartage.JJ_E3_NKJobType = "";
				AssertEquals("CNE LCL", cneLCLTransportBillTo, supporter.Consignor);
			}
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent cartage = Factory.New<CommonCartage>();
			Assert(cartage.AllowInvoiceDeletion);
		}

		public void TestOnJobCreatingEvent()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JobCreating += (s, e) =>
			{
				Assert("Event was raised", true);
			};
			((IJobHeaderParent)cartage).OnJobCreating(Factory.NewJobForTesting<JobHeader>());
		}

		public void TestOnJobDeletingEvent()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JobDeleting += (s, e) =>
			{
				Assert("Event was raised", true);
			};
			((IJobHeaderParent)cartage).OnJobDeleting(Factory.NewJobForTesting<JobHeader>());
		}

		public void TestJobCreate_LinkToParentJob_TransportBooking()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentID = booking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			var jobOnShipment = new JobHeader.Loader((IJobHeaderParent)shipment).TryLoadOrCreate();
			AssertEquals("Pre-condtion: JobHeader ParentID:", shipment.PK, jobOnShipment.JH_ParentID);
			AssertNull(cartage.Job);
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			AssertNotNull(cartage.Job);
			AssertEquals("JH_JH_ParentJob should link to parent job.", jobOnShipment.PK, cartage.Job.JH_JH_ParentJob);
		}

		public void TestIRelatedJob()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			IRelatedJob cartageAsRelatedJob = cartage;
			cartage.JJ_ConsignmentID = "test ID";
			AssertEquals("test ID", cartageAsRelatedJob.JobNumber);
			AssertEquals(cartage.HumanReadableName, cartageAsRelatedJob.JobDescription);
			AssertEquals("Active", cartageAsRelatedJob.JobStatus);
			AssertEquals(ControllerIDs.Cartage, cartageAsRelatedJob.ControllerID);
		}

		public void TestCancelledIRelatedJob()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			IRelatedJob cartageAsRelatedJob = cartage;
			cartage.JJ_ConsignmentID = "test ID";
			cartage.JJ_IsCancelled = true;
			AssertEquals("test ID", cartageAsRelatedJob.JobNumber);
			AssertEquals(cartage.HumanReadableName, cartageAsRelatedJob.JobDescription);
			AssertEquals("Canceled", cartageAsRelatedJob.JobStatus);
			AssertEquals(ControllerIDs.Cartage, cartageAsRelatedJob.ControllerID);
		}

		public void TestRelatedJobs()
		{
			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentID = booking.PK;
			cartage.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			var relatedJobs = cartage.RelatedJobs;
			AssertContainsExactElementsInAnyOrder(new[] { (BusinessObject)booking, shipment }, cartage.RelatedJobs);
		}

		public void TestBusinessObjectsWithRelatedEvents()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			using (JobHeader job2 = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				AssertEquals("Related Logs has JobHeader", true, ((IList)cartage.BusinessObjectsWithRelatedEvents).Contains(job));
			}
		}

		public void TestInvoicePrintingLogsIncludedInCommonCartageRelatedLogs()
		{
			var cne = Factory.New<OrgHeader>();
			cne.OH_Code = "LCLCNE";
			cne.OH_IsConsignee = true;
			cne.OH_FullName = "Local Consignee ";
			cne.MainAddress.OA_Address1 = "Test Address Line";
			cne.OH_RL_NKClosestPort = "AUSYD";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLCTOtoCNE, 1);
			cartage.LocalClientPK = cne.PK;
			Factory.Save();
			var invoiceHelper = new InvoiceCreationTestHelper(Factory);
			var header = invoiceHelper.SetupTransaction(GlbBranch.CurrentBranch, "ANYNUMBER", cne, cartage.JJ_ConsignmentID, ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice);
			var printLog = header.Logs.AddNew(Events.DocumentSent, "FAKE Invoice Printed");
			var cartageLogs = new StmALogCollectionView(cartage);
			Assert("Failed to get invoice printed log", cartageLogs.Contains(printLog.PK));
		}

		public void TestInvoicePrintingParentLogsIncludedInCommonCartageRelatedLogs()
		{
			var cartage = Factory.New<CommonCartage>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			cartage.JJ_ConsignmentID = "XX1234";
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = cartage.PK;
			job.JH_ParentTableCode = "JJ";
			job.JH_JobNum = "S1/E";
			Factory.Save();
			var cne = Factory.New<OrgHeader>();
			cne.OH_Code = "LCLCNE";
			cne.OH_IsConsignee = true;
			cne.OH_FullName = "Local Consignee ";
			cne.MainAddress.OA_Address1 = "Test Address Line";
			cne.OH_RL_NKClosestPort = "AUSYD";
			cartage.LocalClientPK = cne.PK;
			Factory.Save();
			var invoiceHelper = new InvoiceCreationTestHelper(Factory);
			var header = invoiceHelper.SetupTransaction(GlbBranch.CurrentBranch, "ANYNUMBER", cne, cartage.JJ_ConsignmentID, ZArchitecture.Core.LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, job);
			var printLog = header.Logs.AddNew(Events.DocumentSent, "FAKE Invoice Printed");
			var cartageLogs = new StmALogCollectionView(cartage);
			Assert("Failed to get invoice printed log", cartageLogs.Contains(printLog.PK));
		}

		public void TestPopulateClient()
		{
			OrgHeader consignorNoRelated = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader cnrAIRTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader cnrFCLTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader cnrLCLTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader cneAIRTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader cneFCLTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader cneLCLTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			consignor.SetRelatedParty(cnrAIRTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Air, ZString.Empty);
			consignor.SetRelatedParty(cnrFCLTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignor.SetRelatedParty(cnrLCLTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			consignee.SetRelatedParty(cneAIRTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			consignee.SetRelatedParty(cneFCLTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.FCL);
			consignee.SetRelatedParty(cneLCLTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Sea, Constants.ContainerModes.LCL);
			Factory.Save();
			CommonCartage cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirExport;
				AssertEquals("No Addresses set", ZGuid.Empty, cartage.LocalClientAddressPK);
				cartage.FirstDocAddress.OrganisationPK = consignorNoRelated.PK;
				AssertEquals("No related Party", consignorNoRelated.MainAddress.PK, cartage.LocalClientAddressPK);
				cartage.FirstDocAddress.E2_OA_Address = ZGuid.Empty;
				cartage.LocalClientAddressPK = ZGuid.Empty;
				cartage.FirstDocAddress.OrganisationPK = consignor.PK;
				AssertEquals("CNR AIR", cnrAIRTransportBillTo.MainAddress.PK, cartage.LocalClientAddressPK);
			}

			cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLSHPtoCTO;
				cartage.FirstDocAddress.OrganisationPK = consignor.PK;
				AssertEquals("CNR FCL", cnrFCLTransportBillTo.MainAddress.PK, cartage.LocalClientAddressPK);
			}

			cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLExport;
				cartage.FirstDocAddress.OrganisationPK = consignor.PK;
				AssertEquals("CNR LCL", cnrLCLTransportBillTo.MainAddress.PK, cartage.LocalClientAddressPK);
			}

			cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
				cartage.SecondDocAddress.OrganisationPK = consignee.PK;
				AssertEquals("CNE AIR", cneAIRTransportBillTo.MainAddress.PK, cartage.LocalClientAddressPK);
			}

			cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLCTOtoCNE;
				cartage.SecondDocAddress.OrganisationPK = consignee.PK;
				AssertEquals("CNE FCL", cneFCLTransportBillTo.MainAddress.PK, cartage.LocalClientAddressPK);
			}

			cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_LCLImport;
				cartage.SecondDocAddress.OrganisationPK = consignee.PK;
				AssertEquals("CNE LCL", cneLCLTransportBillTo.MainAddress.PK, cartage.LocalClientAddressPK);
			}
		}

		public void TestPopulateClient_EnsureARMAddress()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeReceivablesAddress = consignee.Addresses.AddNew();
			consigneeReceivablesAddress.OA_Language = Constants.Languages.ChineseTraditional;
			consigneeReceivablesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
			consigneeReceivablesAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
			var cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
				cartage.SecondDocAddress.OrganisationPK = consignee.PK;
				AssertEquals("Consignee Receivables Address", consigneeReceivablesAddress.PK, cartage.LocalClientAddressPK);
			}

			var cneAIRTransportBillTo = Factory.NewWithValidTestData<OrgHeader>();
			var cneAIRReceivablesAddress = cneAIRTransportBillTo.Addresses.AddNew();
			cneAIRReceivablesAddress.OA_Language = Constants.Languages.ChineseTraditional;
			cneAIRReceivablesAddress.AddressCapability.SetCapabilityEnabled(OrgAddressType.Receivables);
			cneAIRReceivablesAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Receivables);
			consignee.SetRelatedParty(cneAIRTransportBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.Air, ZString.Empty);
			cartage = Factory.New<CommonCartage>();
			using (JobHeader job = new JobHeader.Loader(cartage).TryLoadOrCreateWithMutex())
			{
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
				cartage.SecondDocAddress.OrganisationPK = consignee.PK;
				AssertEquals("CNE AIR", cneAIRReceivablesAddress.PK, cartage.LocalClientAddressPK);
			}
		}

		public void TestTotalLooseBookedWeight()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			move1.EW_BookedWeight = 20;
			move1.EW_WeightUQ = "XX";
			var move2 = cartage.LooseBookedMoves.AddNew();
			move2.EW_BookedWeight = 1;
			move2.EW_WeightUQ = "T";
			var move3 = cartage.LooseBookedMoves.AddNew();
			move3.EW_BookedWeight = 30;
			move3.EW_WeightUQ = "KG";
			AssertEquals(1050m, cartage.TotalLooseBookedWeight);
			AssertEquals("KG", cartage.TotalLooseBookedWeightUnit);
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertEquals(1050m, cartage.JJ_Weight);
			AssertEquals("KG", cartage.JJ_WeightUQ);
		}

		public void TestTotalLooseBookedWeightReScales()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			move1.EW_BookedWeight = 900000;
			move1.EW_WeightUQ = "KG";
			var move2 = cartage.LooseBookedMoves.AddNew();
			move2.EW_BookedWeight = 50;
			move2.EW_WeightUQ = "T";
			var move3 = cartage.LooseBookedMoves.AddNew();
			move3.EW_BookedWeight = 50000;
			move3.EW_WeightUQ = "XX";
			AssertEquals(1000000m, cartage.TotalLooseBookedWeight);
			AssertEquals("KG", cartage.TotalLooseBookedWeightUnit);
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertEquals(1000m, cartage.JJ_Weight);
			AssertEquals("T", cartage.JJ_WeightUQ);
		}

		public void TestTotalLooseBookedWeight_ShouldNotRaiseException_WhenWeightUnitIsInvalid()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			var move1 = cartage.LooseBookedMoves[0];
			move1.EW_BookedWeight = 900000;
			move1.EW_WeightUQ = "22";
			Assert("Pre-condition: '22' is not a valid weight unit.", !FreightUtilities.IsValidWeightUnit("22"));
			AssertEquals("TotalLooseBookedWeight returns 0.", 0m, cartage.TotalLooseBookedWeight);
			AssertEquals("TotalLooseBookedWeightUnit remains invalid value.", "22", cartage.TotalLooseBookedWeightUnit);
		}

		public void TestTotalLooseBookedVolume()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			move1.EW_BookedVolume = 20;
			move1.EW_VolumeUQ = "XX";
			var move2 = cartage.LooseBookedMoves.AddNew();
			move2.EW_BookedVolume = 1;
			move2.EW_VolumeUQ = "ML";
			var move3 = cartage.LooseBookedMoves.AddNew();
			move3.EW_BookedVolume = 30;
			move3.EW_VolumeUQ = "M3";
			AssertEquals(1050m, cartage.TotalLooseBookedVolume);
			AssertEquals("M3", cartage.TotalLooseBookedVolumeUnit);
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertEquals(1050m, cartage.JJ_Volume);
			AssertEquals("M3", cartage.JJ_VolumeUQ);
		}

		public void TestTotalLooseBookedVolumeReScales()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			var move1 = cartage.LooseBookedMoves.AddNew();
			move1.EW_BookedVolume = 900000;
			move1.EW_VolumeUQ = "M3";
			var move2 = cartage.LooseBookedMoves.AddNew();
			move2.EW_BookedVolume = 50;
			move2.EW_VolumeUQ = "ML";
			var move3 = cartage.LooseBookedMoves.AddNew();
			move3.EW_BookedVolume = 50000;
			move3.EW_VolumeUQ = "XX";
			AssertEquals(1000000m, cartage.TotalLooseBookedVolume);
			AssertEquals("M3", cartage.TotalLooseBookedVolumeUnit);
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertEquals(1000m, cartage.JJ_Volume);
			AssertEquals("ML", cartage.JJ_VolumeUQ);
		}

		public void TestTotalLooseBookedVolume_ShouldNotRaiseException_WhenVolumeUnitIsInvalid()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			var move1 = cartage.LooseBookedMoves[0];
			move1.EW_BookedVolume = 900000;
			move1.EW_VolumeUQ = "CB";
			Assert("Pre-condition: 'CB' is not a valid volume unit.", !FreightUtilities.IsValidVolumeUnit("CB"));
			AssertEquals("TotalLooseBookedVolume returns 0.", 0m, cartage.TotalLooseBookedVolume);
			AssertEquals("TotalLooseBookedVolumeUnit remains invalid value.", "CB", cartage.TotalLooseBookedVolumeUnit);
		}

		public void TestTotalContainerWeight()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var containerBookedMovesComparer = new ContainerBookedMovesComparer();
			cartage.ContainerBookedMoves.ApplySort(containerBookedMovesComparer);
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.DeliveryPackType;
			line1.JL_ActualWeight = 20;
			line1.JL_ActualWeightUQ = "KG"; // container already fixes invalid weight units, no way to test here if container has invalid weight unit
			line1.JL_JS = shipment.PK;
			container1.PackLines.Add(line1);
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			var line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.DeliveryPackType;
			line2.JL_ActualWeight = 1;
			line2.JL_ActualWeightUQ = "T";
			line2.JL_JS = shipment.PK;
			container2.PackLines.Add(line2);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(1020m, cartage.TotalContainerWeight);
				AssertEquals("KG", cartage.TotalContainerWeightUnit);
			});
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			CombineAssertions(() =>
			{
				AssertEquals(1020m, cartage.JJ_Weight);
				AssertEquals("KG", cartage.JJ_WeightUQ);
			});
		}

		public void TestTotalContainerWeightReScales()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var containerBookedMovesComparer = new ContainerBookedMovesComparer();
			cartage.ContainerBookedMoves.ApplySort(containerBookedMovesComparer);
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.DeliveryPackType;
			line1.JL_ActualWeight = 900000;
			line1.JL_ActualWeightUQ = "KG";
			line1.JL_JS = shipment.PK;
			container1.PackLines.Add(line1);
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			var line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.DeliveryPackType;
			line2.JL_ActualWeight = 100000;
			line2.JL_ActualWeightUQ = "KG";
			line2.JL_JS = shipment.PK;
			container2.PackLines.Add(line2);
			Factory.Save();
			CombineAssertions("Precondition", () =>
			{
				AssertEquals("This number is large enough it needs rescaling", 1000000m, cartage.TotalContainerWeight);
				AssertEquals("This is a small unit that'll need to be replaced with a larger unit", "KG", cartage.TotalContainerWeightUnit);
			});
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			CombineAssertions(() =>
			{
				AssertEquals("Rescaling sets correct value for JJ_Weight", 1000m, cartage.JJ_Weight);
				AssertEquals("Rescaling sets correct unit for JJ_VolumeUQ", "T", cartage.JJ_WeightUQ);
			});
		}

		public void TestTotalContainerVolume()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var containerBookedMovesComparer = new ContainerBookedMovesComparer();
			cartage.ContainerBookedMoves.ApplySort(containerBookedMovesComparer);
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.DeliveryPackType;
			line1.JL_ActualVolume = 20;
			line1.JL_ActualVolumeUQ = "M3"; // container already fixes invalid volume units, no way to test here if container has invalid weight unit
			line1.JL_JS = shipment.PK;
			container1.PackLines.Add(line1);
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			var line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.DeliveryPackType;
			line2.JL_ActualVolume = 1;
			line2.JL_ActualVolumeUQ = "ML";
			line2.JL_JS = shipment.PK;
			container2.PackLines.Add(line2);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(1020m, cartage.TotalContainerVolume);
				AssertEquals("M3", cartage.TotalContainerVolumeUnit);
			});
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			AssertEquals(1020m, cartage.JJ_Volume);
			AssertEquals("M3", cartage.JJ_VolumeUQ);
		}

		public void TestTotalContainerVolumeReScales()
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup;
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var containerBookedMovesComparer = new ContainerBookedMovesComparer();
			cartage.ContainerBookedMoves.ApplySort(containerBookedMovesComparer);
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var line1 = Factory.New<PackLine>();
			line1.JL_FreightMode = FreightConstants.DeliveryPackType;
			line1.JL_ActualVolume = 900000;
			line1.JL_ActualVolumeUQ = "M3";
			line1.JL_JS = shipment.PK;
			container1.PackLines.Add(line1);
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			var line2 = Factory.New<PackLine>();
			line2.JL_FreightMode = FreightConstants.DeliveryPackType;
			line2.JL_ActualVolume = 100000;
			line2.JL_ActualVolumeUQ = "M3";
			line2.JL_JS = shipment.PK;
			container2.PackLines.Add(line2);
			Factory.Save();
			CombineAssertions("Precondition", () =>
			{
				AssertEquals("This number is large enough it needs rescaling", 1000000m, cartage.TotalContainerVolume);
				AssertEquals("This is a small unit that'll need to be replaced with a larger unit", "M3", cartage.TotalContainerVolumeUnit);
			});
			cartage.UpdateCartageFromLooseBookedMovesOrContainers();
			CombineAssertions(() =>
			{
				AssertEquals("Rescaling sets correct value for JJ_Volume", 1000m, cartage.JJ_Volume);
				AssertEquals("Rescaling sets correct unit for JJ_VolumeUQ", "ML", cartage.JJ_VolumeUQ);
			});
		}

		public void TestCanDeleteAddress()
		{
			var cartage = Factory.New<CommonCartage>();
			var consignor = Factory.New<OrgHeader>();
			var docAddress1 = cartage.DocAddresses.AddNew();
			docAddress1.DocAddressType = DocAddressType.LocalCartageImporter;
			docAddress1.OrganisationPK = consignor.PK;
			AssertNotNull(docAddress1.Organisation);
			AssertEquals(false, ((IDocAddresses)cartage).CanDeleteAddress(docAddress1));
			var docAddress2 = cartage.DocAddresses.AddNew();
			docAddress2.DocAddressType = DocAddressType.LocalCartageExporter;
			AssertNull(docAddress2.Organisation);
			AssertEquals(true, ((IDocAddresses)cartage).CanDeleteAddress(docAddress2));
		}

		public void TestNoteTypes()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertContainsExactElementsInAnyOrder(new[] { PredefinedNoteTypes.Instance.AutoRatingAuditLog, PredefinedNoteTypes.Instance.ClientVisibleJobNotes, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation, PredefinedNoteTypes.Instance.DeliveryInstructionsNote, PredefinedNoteTypes.Instance.PickupInstructionsNote, PredefinedNoteTypes.Instance.DetailedGoodsDescription, PredefinedNoteTypes.Instance.FaxEmailTransmissionLog, PredefinedNoteTypes.Instance.HandlingInstructions, PredefinedNoteTypes.Instance.InternalWorkNotes, PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes, PredefinedNoteTypes.Instance.UnmatchedOrgDetails }, cartage.NoteTypes);
		}

		public void TestBusinessObjectsWithRelatedNotes()
		{
			var dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			var cartageType = new CompletedDummyCartageType(dummyParent);
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			StmNote note1 = dummyParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "ParentNote");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			StmNote note2 = org1.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Org1Note");
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			StmNote note3 = org1.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Org2Note");
			cartage.FirstDocAddress.E2_OA_Address = org1.MainAddress.PK;
			cartage.SecondDocAddress.E2_OA_Address = org1.MainAddress.PK;
			cartage.ThirdDocAddress.E2_OA_Address = org2.MainAddress.PK;
			AssertEquals(3, cartage.BusinessObjectsWithRelatedNotes.Length);
			AssertContainsExactElementsInAnyOrder(cartage.BusinessObjectsWithRelatedNotes, new BusinessObject[] { org1, org2, dummyParent });
		}

		public void TestBusinessObjectsWithRelatedNotes_QuotedBooking()
		{
			var quotedBooking = GetNewQuoteBooking(Constants.TransportCodes.Air);
			var quotedBookingWithNotes = (IStmNoteParent)quotedBooking;
			var note = quotedBookingWithNotes.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "ParentNote");
			Factory.Save();
			var cartage = Factory.New<CartageForTest>();
			cartage.SetParent((ICartageParent)quotedBooking);
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			AssertNoExceptionThrown(() => new StmNoteCollectionView(cartage));
		}

		IQuotedBooking GetNewQuoteBooking(ZString transportMode)
		{
			var quotedBooking = (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().InvokeMember("New", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new object[] { Enterprise.Freight.Integration.QuoteBookingType.BookingWithQuote, Factory });
			var quotedBookingJob = new JobHeader.Loader((IJobHeaderParent)quotedBooking).TryCreate();
			quotedBookingJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			quotedBookingJob.JH_GB = GlbBranch.CurrentBranch.PK;
			var shipment = (CommonShipment)quotedBooking.ForwardingShipment;
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			return quotedBooking;
		}

		public void TestNoteContextsForRelatedNotes_Module()
		{
			var cartage = Factory.New<CartageForTest>();
			var noteContexts = cartage.GetNoteContextsForRelatedNotes();
			AssertEquals("Cartage should load notes for ALL and 'Transport' module only.", StmNoteContextModule.T | StmNoteContextModule.A, noteContexts.Module);
		}

		public void TestNoteContextsForRelatedNotes_Direction()
		{
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Export, true, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Origin, true, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Import, false, true);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Destination, false, true);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.LineHaul, false, false);
			CreateAndAssertNoteContextDirection(Constants.CartageDirection.Local, false, false);
			CreateAndAssertNoteContextDirection("", false, false);
		}

		void CreateAndAssertNoteContextDirection(ZString direction, bool hasExport, bool hasImport)
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_Direction = direction;
			var noteContexts = cartage.GetNoteContextsForRelatedNotes();
			AssertEquals("Direction: All", true, noteContexts.Direction.HasFlag(StmNoteContextDirection.A));
			AssertEquals("Direction: Import", hasImport, noteContexts.Direction.HasFlag(StmNoteContextDirection.I));
			AssertEquals("Direction: Export", hasExport, noteContexts.Direction.HasFlag(StmNoteContextDirection.E));
			AssertEquals("Direction: Import and Export", hasImport || hasExport, noteContexts.Direction.HasFlag(StmNoteContextDirection.B));
			AssertEquals("Direction: Domestic", true, noteContexts.Direction.HasFlag(StmNoteContextDirection.D));
			AssertEquals("Direction: Cross Trade", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.X));
			AssertEquals("Direction: All Forwarding", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.F));
			AssertEquals("Direction: Other / Warehouse Out", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.O));
			AssertEquals("Direction: Warehouse In", false, noteContexts.Direction.HasFlag(StmNoteContextDirection.R));
		}

		public void TestNoteContextsForRelatedNotes_FreightMode()
		{
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Air, Constants.CartageContainerMode.Loose, true, false, false, false, true);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Air, Constants.CartageContainerMode.Containerized, true, false, false, true, false);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Air, Constants.CartageContainerMode.Mixed, true, false, false, true, true);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Sea, Constants.CartageContainerMode.Loose, false, true, false, false, true);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Sea, Constants.CartageContainerMode.Containerized, false, true, false, true, false);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Sea, Constants.CartageContainerMode.Mixed, false, true, false, true, true);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Road, Constants.CartageContainerMode.Loose, false, false, false, false, true);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Road, Constants.CartageContainerMode.Containerized, false, false, false, true, false);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Road, Constants.CartageContainerMode.Mixed, false, false, false, true, true);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Rail, Constants.CartageContainerMode.Loose, false, false, true, false, true);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Rail, Constants.CartageContainerMode.Containerized, false, false, true, true, false);
			CreateAndAssertNoteContextFreightMode(Constants.TransportModes.Rail, Constants.CartageContainerMode.Mixed, false, false, true, true, true);
			CreateAndAssertNoteContextFreightMode("", Constants.CartageContainerMode.Loose, false, false, false, false, true);
			CreateAndAssertNoteContextFreightMode("", Constants.CartageContainerMode.Containerized, false, false, false, true, false);
			CreateAndAssertNoteContextFreightMode("", Constants.CartageContainerMode.Mixed, false, false, false, true, true);
		}

		void CreateAndAssertNoteContextFreightMode(ZString transportMode, ZString containerMode, bool hasAir, bool hasSea, bool hasRail, bool hasContainers, bool hasLoose)
		{
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_ShippingTransportMode = transportMode;
			cartage.JJ_ContainerMode = containerMode;
			var noteContexts = cartage.GetNoteContextsForRelatedNotes();
			AssertEquals("FreightMode: All", true, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.A));
			AssertEquals("FreightMode: Sea", hasSea, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.S));
			AssertEquals("FreightMode: FCL", hasContainers, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.F));
			AssertEquals("FreightMode: LCL", hasLoose, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.L));
			AssertEquals("FreightMode: Air", hasAir, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.I));
			AssertEquals("FreightMode: Road", true, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.R));
			AssertEquals("FreightMode: Rail", hasRail, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.W));
			AssertEquals("FreightMode: Air and Sea", hasSea || hasAir, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.B));
			AssertEquals("FreightMode: Warehouse Orders", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.O));
			AssertEquals("FreightMode: Warehouse Transfers", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.T));
			AssertEquals("FreightMode: Warehouse Adjustments", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.D));
			AssertEquals("FreightMode: Warehouse Periodic Billing", false, noteContexts.FreightMode.HasFlag(StmNoteContextFreightMode.P));
		}

		public void TestTemplateCopy()
		{
			var cfs = Helper.CreateOrgHeader("CFSSYD", "Freight Station").MainAddress.PK;
			var cne = Helper.CreateOrgHeader("CNESYD", "CNE Address").MainAddress.PK;
			var client = Helper.CreateOrgHeader("CLNSYD", "Client Address").MainAddress.PK;
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirExport, 3);
			cartage.FirstDocAddress.E2_OA_Address = cfs;
			cartage.SecondDocAddress.E2_OA_Address = cne;
			cartage.JJ_RS_NKServiceLevel = "SVC";
			cartage.JJ_DropMode = "DRP";
			var job = new JobHeader.Loader(cartage).TryCreate();
			job.JH_OA_LocalChargesAddr = client;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			AssertEquals(Constants.CartageJobType.NEW_AirExport, copy.JJ_E3_NKJobType);
			AssertEquals(cfs, copy.FirstDocAddress.E2_OA_Address);
			AssertEquals(cne, copy.SecondDocAddress.E2_OA_Address);
			AssertEquals("SVC", copy.JJ_RS_NKServiceLevel);
			AssertEquals("DRP", copy.JJ_DropMode);
			AssertEquals(client, copy.LocalClientAddressPK);
			AssertEquals(3, copy.LooseBookedMoves.Count);
		}

		public void TestTemplateCopy_Containers()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCFStoCYD, 3);
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			AssertEquals(Constants.CartageJobType.NEW_EmptyCFStoCYD, copy.JJ_E3_NKJobType);
			AssertEquals(3, copy.Containers.Count());
		}

		public void TestTemplateCopy_CloneContainerDetails()
		{
			var ref20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			AssertNotNull("Precondition", ref20GP);
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_EmptyCFStoCYD, 1);
			var container = cartage.Containers.Single();
			container.JC_ContainerNum = "A";
			container.JC_RC = ref20GP.PK;
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			AssertEquals("Precondition", Constants.CartageJobType.NEW_EmptyCFStoCYD, copy.JJ_E3_NKJobType);
			var clonedContainer = copy.Containers.Single();
			AssertNotEquals(container, clonedContainer);
			AssertEquals("", clonedContainer.JC_ContainerNum);
			AssertEquals("", clonedContainer.JC_SealNum);
			AssertEquals("Only this should be copied from client job.", ref20GP.PK, clonedContainer.JC_RC);
		}

		public void TestCopyCartageFromCustomJobType_SeaExportContainerised()
		{
			var customJobType = Factory.New<CommonCartageType>();
			var jobType = "ESF9";
			customJobType.E3_JobType = jobType;
			customJobType.E3_Description = "Custom JobType";
			customJobType.E3_GE = GlbDepartment.CurrentDepartment.PK;
			customJobType.Direction = Constants.FreightShipmentDirection.Code.Export;
			customJobType.E3_ShippingTransportMode = Constants.TransportModes.Sea;
			customJobType.ContainerMode = Constants.ContainerModes.FCL;
			var cTO = customJobType.CommonCartageOrganisations.AddNew();
			cTO.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CTO;
			var cFS = customJobType.CommonCartageOrganisations.AddNew();
			cFS.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CFS;
			var cNE = customJobType.CommonCartageOrganisations.AddNew();
			cNE.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CNE;
			var cYD = customJobType.CommonCartageOrganisations.AddNew();
			cYD.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CYD;
			var containerLeg1 = customJobType.ContainerizedCartageLegTypes.AddNew();
			containerLeg1.E4_ContainerMode = Constants.ContainerModes.Containerised;
			containerLeg1.E4_E5_FromOrg = cTO.PK;
			containerLeg1.E4_E5_ToOrg = cFS.PK;
			var containerLeg2 = customJobType.ContainerizedCartageLegTypes.AddNew();
			containerLeg2.E4_ContainerMode = Constants.ContainerModes.Containerised;
			containerLeg2.E4_E5_FromOrg = cFS.PK;
			containerLeg2.E4_E5_ToOrg = cYD.PK;
			Factory.Save();
			var cneSyd = Helper.CreateOrgHeader("CNESYD", "CNE Address").MainAddress.PK;
			var cnrSyd = Helper.CreateOrgHeader("CNRSYD", "CNR Address").MainAddress.PK;
			var ctoSyd = Helper.CreateOrgHeader("CTOSYD", "CTO Address").MainAddress.PK;
			var client = Helper.CreateOrgHeader("CLNSYD", "Client Address").MainAddress.PK;
			var cartage = Helper.CreateCartage(jobType, 3);
			cartage.FirstDocAddress.E2_OA_Address = cneSyd;
			cartage.SecondDocAddress.E2_OA_Address = cnrSyd;
			cartage.ThirdDocAddress.E2_OA_Address = ctoSyd;
			cartage.JJ_DropMode = Constants.FCLEquipmentNeeded.LiftOffOn;
			cartage.JJ_ContainerMode = Constants.ContainerModes.Containerised;
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Sea;
			cartage.JJ_Direction = Constants.FreightShipmentDirection.Code.Export;
			var job = new JobHeader.Loader(cartage).TryCreate();
			job.JH_OA_LocalChargesAddr = client;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			AssertEquals(jobType, copy.JJ_E3_NKJobType);
			AssertEquals(Constants.ContainerModes.Containerised, copy.JJ_ContainerMode);
			AssertEquals(Constants.FCLEquipmentNeeded.LiftOffOn, copy.JJ_DropMode);
			AssertEquals(Constants.TransportModes.Sea, copy.JJ_ShippingTransportMode);
			AssertEquals(Constants.FreightShipmentDirection.Code.Export, copy.JJ_Direction);
		}

		public void TestCopyCartageFromCustomJobType_AirDomestic()
		{
			var customJobType = Factory.New<CommonCartageType>();
			var jobType = "DAL1";
			customJobType.E3_JobType = jobType;
			customJobType.E3_Description = "Custom JobType AIR";
			customJobType.E3_GE = GlbDepartment.CurrentDepartment.PK;
			customJobType.Direction = Constants.FreightShipmentDirection.Code.Domestic;
			customJobType.E3_ShippingTransportMode = Constants.TransportModes.Air;
			customJobType.ContainerMode = Constants.ContainerModes.Loose;
			var cFS = customJobType.CommonCartageOrganisations.AddNew();
			cFS.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CFS;
			var cNE = customJobType.CommonCartageOrganisations.AddNew();
			cNE.E5_OrgType = LocalCartageJobOrgTypeList.Codes.CNE;
			var containerLeg1 = customJobType.LooseCartageLegTypes.AddNew();
			containerLeg1.E4_ContainerMode = Constants.ContainerModes.Loose;
			containerLeg1.E4_E5_FromOrg = cFS.PK;
			containerLeg1.E4_E5_ToOrg = cNE.PK;
			Factory.Save();
			var cfsSyd = Helper.CreateOrgHeader("CFSSYD", "CFS Address").MainAddress.PK;
			var cneSyd = Helper.CreateOrgHeader("CNESYD", "CNE Address").MainAddress.PK;
			var client = Helper.CreateOrgHeader("CLNSYD", "Client Address").MainAddress.PK;
			var cartage = Helper.CreateCartage(jobType, 3);
			cartage.FirstDocAddress.E2_OA_Address = cfsSyd;
			cartage.SecondDocAddress.E2_OA_Address = cneSyd;
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			cartage.JJ_ContainerMode = Constants.ContainerModes.Loose;
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			cartage.JJ_Direction = Constants.FreightShipmentDirection.Code.Domestic;
			var job = new JobHeader.Loader(cartage).TryCreate();
			job.JH_OA_LocalChargesAddr = client;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			AssertEquals(jobType, copy.JJ_E3_NKJobType);
			AssertEquals(Constants.ContainerModes.Loose, copy.JJ_ContainerMode);
			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, copy.JJ_DropMode);
			AssertEquals(Constants.TransportModes.Air, copy.JJ_ShippingTransportMode);
			AssertEquals(Constants.FreightShipmentDirection.Code.Domestic, copy.JJ_Direction);
		}

		public void TestCopyCartageFromCustomJobType_AirDomestic_NoJobType()
		{
			var client = Helper.CreateOrgHeader("CLNSYD", "Client Address").MainAddress.PK;
			var cartage = Helper.CreateCartage("", 3);
			cartage.JJ_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			cartage.JJ_ContainerMode = Constants.ContainerModes.Loose;
			cartage.JJ_ShippingTransportMode = Constants.TransportModes.Air;
			cartage.JJ_Direction = Constants.FreightShipmentDirection.Code.Domestic;
			var job = new JobHeader.Loader(cartage).TryCreate();
			job.JH_OA_LocalChargesAddr = client;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			AssertEquals("", copy.JJ_E3_NKJobType);
			AssertEquals(Constants.ContainerModes.Loose, copy.JJ_ContainerMode);
			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, copy.JJ_DropMode);
			AssertEquals(Constants.TransportModes.Air, copy.JJ_ShippingTransportMode);
			AssertEquals(Constants.FreightShipmentDirection.Code.Domestic, copy.JJ_Direction);
		}

		public void TestTemplateCopy_CloneCartageLegs_WithCorrectDisplayOrder()
		{
			var cto = Helper.CreateOrgHeader("CTOMEL", "CTO");
			var cne = Helper.CreateOrgHeader("CNEHOB", "CNE");
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var move = cartage.ContainerBookedMoves.AddNew();
			var cartageLegWithDisplayOrder2 = move.CartageLegs.AddNew();
			cartageLegWithDisplayOrder2.JU_E2PickupAddressID = cne.MainAddress.PK;
			cartageLegWithDisplayOrder2.JU_E2DeliveryAddressID = cyd.MainAddress.PK;
			cartageLegWithDisplayOrder2.JU_DisplayOrder = 2;
			var cartageLegWithDisplayOrder1 = move.CartageLegs.AddNew();
			cartageLegWithDisplayOrder1.JU_E2PickupAddressID = cto.MainAddress.PK;
			cartageLegWithDisplayOrder1.JU_E2DeliveryAddressID = cne.MainAddress.PK;
			cartageLegWithDisplayOrder1.JU_DisplayOrder = 1;
			AssertEquals("Precondition: Wrong number of cartage legs", 2, move.CartageLegs.Count);
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			var clonedMove = copy.ContainerBookedMoves.Single();
			AssertEquals("Wrong number of cartage legs for copy", 2, clonedMove.CartageLegs.Count);
			var clonedLegWithDisplayOrder2 = clonedMove.CartageLegs.Single(leg => leg.JU_DisplayOrder == 2);
			var clonedLegWithDisplayOrder1 = clonedMove.CartageLegs.Single(leg => leg.JU_DisplayOrder == 1);
			CombineAssertions(delegate
			{
				AssertEquals("Display order is incorrect: Pickup address for leg with display order of 2 does not match", cartageLegWithDisplayOrder2.PickupAddressCode, clonedLegWithDisplayOrder2.PickupAddressCode);
				AssertEquals("Display order is incorrect: Delivery address for leg with display order of 2 does not match", cartageLegWithDisplayOrder2.DeliveryAddressCode, clonedLegWithDisplayOrder2.DeliveryAddressCode);
				AssertEquals("Display order is incorrect: Pickup address for leg with display order of 1 does not match", cartageLegWithDisplayOrder1.PickupAddressCode, clonedLegWithDisplayOrder1.PickupAddressCode);
				AssertEquals("Display order is incorrect: Delivery address for leg with display order of 1 does not match", cartageLegWithDisplayOrder1.DeliveryAddressCode, clonedLegWithDisplayOrder1.DeliveryAddressCode);
				AssertEquals("Cartage Legs are not ordered in display order", 1, clonedMove.CartageLegs[0].JU_DisplayOrder);
				AssertEquals("Cartage Legs are not ordered in display order", 2, clonedMove.CartageLegs[1].JU_DisplayOrder);
			});
		}

		public void TestTemplateCopy_CloneContainerBookedMoves_WithCorrectDisplayOrder()
		{
			var cto = Helper.CreateOrgHeader("CTOMEL", "CTO");
			var cne = Helper.CreateOrgHeader("CNEHOB", "CNE");
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var moveWithDisplayOrder2 = cartage.ContainerBookedMoves.AddNew();
			moveWithDisplayOrder2.EW_DisplayOrder = 2;
			moveWithDisplayOrder2.EW_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			var moveWithDisplayOrder1 = cartage.ContainerBookedMoves.AddNew();
			moveWithDisplayOrder1.EW_DisplayOrder = 1;
			moveWithDisplayOrder1.EW_DropMode = Constants.LCLAIREquipmentNeeded.HandHaulier;
			AssertEquals("Precondition: Wrong number of container booked moves", 2, cartage.ContainerBookedMoves.Count);
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			AssertEquals("Wrong number of container booked moves for copy", 2, copy.ContainerBookedMoves.Count);
			var clonedMoveWithDisplayOrder2 = copy.ContainerBookedMoves.Single(move => move.EW_DisplayOrder == 2);
			var clonedMoveWithDisplayOrder1 = copy.ContainerBookedMoves.Single(move => move.EW_DisplayOrder == 1);
			CombineAssertions(delegate
			{
				AssertEquals("Display order is incorrect: Drop mode for move with display order of 2 does not match", moveWithDisplayOrder2.EW_DropMode, clonedMoveWithDisplayOrder2.EW_DropMode);
				AssertEquals("Display order is incorrect: Drop mode for move with display order of 1 does not match", moveWithDisplayOrder1.EW_DropMode, clonedMoveWithDisplayOrder1.EW_DropMode);
			});
		}

		public void TestTemplateCopy_CartageJobCartageLegs_HaveCorrectDisplayOrder()
		{
			var cto = Helper.CreateOrgHeader("CTOMEL", "CTO");
			var cne = Helper.CreateOrgHeader("CNEHOB", "CNE");
			var cyd = Helper.CreateOrgHeader("CYDSYD", "CYD");
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var moveWithDisplayOrder2 = cartage.ContainerBookedMoves.AddNew();
			moveWithDisplayOrder2.EW_DisplayOrder = 2;
			moveWithDisplayOrder2.EW_DropMode = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;
			var move2leg2 = moveWithDisplayOrder2.CartageLegs.AddNew();
			move2leg2.JU_E2PickupAddressID = cne.MainAddress.PK;
			move2leg2.JU_E2DeliveryAddressID = cyd.MainAddress.PK;
			move2leg2.JU_DisplayOrder = 2;
			var move2leg1 = moveWithDisplayOrder2.CartageLegs.AddNew();
			move2leg1.JU_E2PickupAddressID = cto.MainAddress.PK;
			move2leg1.JU_E2DeliveryAddressID = cne.MainAddress.PK;
			move2leg1.JU_DisplayOrder = 1;
			var moveWithDisplayOrder1 = cartage.ContainerBookedMoves.AddNew();
			moveWithDisplayOrder1.EW_DisplayOrder = 1;
			moveWithDisplayOrder1.EW_DropMode = Constants.LCLAIREquipmentNeeded.HandHaulier;
			var move1leg2 = moveWithDisplayOrder1.CartageLegs.AddNew();
			move1leg2.JU_E2PickupAddressID = cne.MainAddress.PK;
			move1leg2.JU_E2DeliveryAddressID = cyd.MainAddress.PK;
			move1leg2.JU_DisplayOrder = 2;
			var move1leg1 = moveWithDisplayOrder1.CartageLegs.AddNew();
			move1leg1.JU_E2PickupAddressID = cto.MainAddress.PK;
			move1leg1.JU_E2DeliveryAddressID = cne.MainAddress.PK;
			move1leg1.JU_DisplayOrder = 1;
			AssertEquals("Precondition: Wrong number of cartage legs for the cartage job", 4, cartage.CartageLegs.Count);
			Factory.Save();
			var copy = (CommonCartage)((ITemplateCopyable)cartage).TemplateCopy();
			AssertEquals("Wrong number of cartage legs for the cartage job", 4, copy.CartageLegs.Count);
			var clonedMove2Leg2 = copy.CartageLegs.Single(leg => leg.JU_DisplayOrder == 2 && leg.BookedCtgMove.EW_DisplayOrder == 2);
			var clonedMove2Leg1 = copy.CartageLegs.Single(leg => leg.JU_DisplayOrder == 1 && leg.BookedCtgMove.EW_DisplayOrder == 2);
			var clonedMove1Leg2 = copy.CartageLegs.Single(leg => leg.JU_DisplayOrder == 2 && leg.BookedCtgMove.EW_DisplayOrder == 1);
			var clonedMove1Leg1 = copy.CartageLegs.Single(leg => leg.JU_DisplayOrder == 1 && leg.BookedCtgMove.EW_DisplayOrder == 1);
			CombineAssertions(delegate
			{
				AssertEquals("Display order of either cartage leg or of container booked move is incorrect: Pickup address for leg with display order of 2 and move display order of 2 does not match", move2leg2.PickupAddressCode, clonedMove2Leg2.PickupAddressCode);
				AssertEquals("Display order of either cartage leg or of container booked move is incorrect: Delivery address for leg with display order of 2 and move display order of 2 does not match", move2leg2.DeliveryAddressCode, clonedMove2Leg2.DeliveryAddressCode);
				AssertEquals("Display order of either cartage leg or of container booked move is incorrect: Pickup address for leg with display order of 1 and move display order of 2 does not match", move2leg1.PickupAddressCode, clonedMove2Leg1.PickupAddressCode);
				AssertEquals("Display order of either cartage leg or of container booked move is incorrect: Delivery address for leg with display order of 1 and move display order of 2 does not match", move2leg1.DeliveryAddressCode, clonedMove2Leg1.DeliveryAddressCode);
				AssertEquals("Display order of either cartage leg or of container booked move is incorrect: Pickup address for leg with display order of 2 and move display order of 1 does not match", move1leg2.PickupAddressCode, clonedMove1Leg2.PickupAddressCode);
				AssertEquals("Display order of either cartage leg or of container booked move is incorrect: Delivery address for leg with display order of 2 and move display order of 1 does not match", move1leg2.DeliveryAddressCode, clonedMove1Leg2.DeliveryAddressCode);
				AssertEquals("Display order of either cartage leg or of container booked move is incorrect: Pickup address for leg with display order of 1 and move display order of 1 does not match", move1leg1.PickupAddressCode, clonedMove1Leg1.PickupAddressCode);
				AssertEquals("Display order of either cartage leg or of container booked move is incorrect: Delivery address for leg with display order of 1 and move display order of 1 does not match", move1leg1.DeliveryAddressCode, clonedMove1Leg1.DeliveryAddressCode);
				AssertEquals("Display order of container booked move is incorrect: Drop mode for leg with display order of 2 and move display order of 2 does not match", move2leg2.BookedCtgMove.EW_DropMode, clonedMove2Leg2.BookedCtgMove.EW_DropMode);
				AssertEquals("Display order of container booked move is incorrect: Drop mode for leg with display order of 1 and move display order of 2 does not match", move2leg1.BookedCtgMove.EW_DropMode, clonedMove2Leg1.BookedCtgMove.EW_DropMode);
				AssertEquals("Display order of container booked move is incorrect: Drop mode for leg with display order of 2 and move display order of 1 does not match", move1leg2.BookedCtgMove.EW_DropMode, clonedMove1Leg2.BookedCtgMove.EW_DropMode);
				AssertEquals("Display order of container booked move is incorrect: Drop mode for leg with display order of 1 and move display order of 1 does not match", move1leg1.BookedCtgMove.EW_DropMode, clonedMove1Leg1.BookedCtgMove.EW_DropMode);
			});
		}

		ZBool IsOKToPrintCreditOnHoldDocument;
		ZInt CreditOnHoldRaisedCount;
		public void TestICreditControlledDocumentDelivery_GetDocumentLogin()
		{
			var command = Factory.New<DocumentCommand>();
			var cartage = Factory.New<CommonCartage>();
			var creditOnHoldBusinessDocument = (ICreditControlledDocumentDelivery)cartage;
			creditOnHoldBusinessDocument.GetDocumentLogin += (s, e) =>
			{
				CreditOnHoldRaisedCount++;
				e.IsAllowedToProceed = IsOKToPrintCreditOnHoldDocument;
			};
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			cartage.LocalClientPK = localClient.PK;
			RunDocumentLogin(cartage, localClient, command);
			cartage.LocalClientPK = ZGuid.Empty;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			var consignorDocAddress = cartage.DocAddresses.FindOrCreateWithDocAddressType(consignor.MainAddress.PK, DocAddressType.LocalCartageExporter);
			RunDocumentLogin(cartage, consignor, command);
			consignorDocAddress.Delete();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consigneeDocAddress = cartage.DocAddresses.FindOrCreateWithDocAddressType(consignee.MainAddress.PK, DocAddressType.LocalCartageImporter);
			RunDocumentLogin(cartage, consignee, command);
			consigneeDocAddress.Delete();
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			IsOKToPrintCreditOnHoldDocument = true;
			DocumentSupporterDataState result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should NOT have raised Credit On Hold Event, the menu is credit controlled, but no clients are specified for the transport job.", 2, CreditOnHoldRaisedCount);
			AssertEquals("Should have continued to print document, even though we didn't allow it, we were never asked.", true, result.IsValid);
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			IsOKToPrintCreditOnHoldDocument = false;
			result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should NOT have raised Credit On Hold Event, the menu is credit controlled, but no clients are specified for the transport job.", 2, CreditOnHoldRaisedCount);
			AssertEquals("Should have continued to print document, even though we didn't allow it, we were never asked.", true, result.IsValid);
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			IsOKToPrintCreditOnHoldDocument = true;
			result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should NOT have raised Credit On Hold Event, the menu is not credit controlled, but no clients are specified for the transport job.", 2, CreditOnHoldRaisedCount);
			AssertEquals("Should have continued to print document, even though we didn't allow it, we were never asked.", true, result.IsValid);
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			IsOKToPrintCreditOnHoldDocument = false;
			result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should NOT have raised Credit On Hold Event, the menu is not credit controlled, but no clients are specified for the transport job.", 2, CreditOnHoldRaisedCount);
			AssertEquals("Should have continued to print document, even though we didn't allow it, we were never asked.", true, result.IsValid);
		}

		void RunDocumentLogin(CommonCartage cartage, OrgHeader organisation, DocumentCommand command)
		{
			IsOKToPrintCreditOnHoldDocument = false;
			CreditOnHoldRaisedCount = 0;
			organisation.OH_IsDebtor = true;
			organisation.MiscServ.OM_AROnCreditHold = true;
			Factory.Save();
			organisation.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			IsOKToPrintCreditOnHoldDocument = true;
			DocumentSupporterDataState result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should have raised Credit On Hold Event.", 1, CreditOnHoldRaisedCount);
			AssertEquals("Should have continued to print document.", true, result.IsValid);
			IsOKToPrintCreditOnHoldDocument = false;
			result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should have raised Credit On Hold Event.", 2, CreditOnHoldRaisedCount);
			AssertEquals("Should NOT have continued to print document, because we have not allowed it to.", false, result.IsValid);
			organisation.MiscServ.OM_AROnCreditHold = false;
			Factory.Save();
			organisation.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should NOT have raised Credit On Hold Event, because the client credit is OK.", 2, CreditOnHoldRaisedCount);
			AssertEquals("Should have continued to print document, even though we didn't allow it, we were never asked.", true, result.IsValid);
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			organisation.MiscServ.OM_AROnCreditHold = false;
			Factory.Save();
			organisation.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should NOT have raised Credit On Hold Event, because the client credit is OK and the menu is not credit controlled.", 2, CreditOnHoldRaisedCount);
			AssertEquals("Should have continued to print document, even though we didn't allow it, we were never asked.", true, result.IsValid);
			command.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			organisation.MiscServ.OM_AROnCreditHold = true;
			Factory.Save();
			organisation.CreditChecker.ClearCreditLimitAndOutstandingBalanceCache();
			result = cartage.DocumentSupporter.GetDataStateBeforeRun(command);
			AssertEquals("Should NOT have raised Credit On Hold Event, the menu is credit controlled, but the clients credit is OK.", 2, CreditOnHoldRaisedCount);
			AssertEquals("Should have continued to print document, even though we didn't allow it, we were never asked.", true, result.IsValid);
		}

		public void TestICreditControlledDocumentDelivery_IsDPSFreightMovementRestricted()
		{
			var cartage = Factory.New<CommonCartage>();
			var creditOnHoldDocument = (ICreditControlledDocumentDelivery)cartage;
			AssertEquals(false, creditOnHoldDocument.IsDPSFreightMovementRestricted);
		}

		public void TestICreditControlledDocumentDelivery_DescriptionOfOrganisationBeingCheckedForCredit()
		{
			var cartage = Factory.New<CommonCartage>();
			var creditOnHoldDocument = (ICreditControlledDocumentDelivery)cartage;
			AssertContains("Consignee, Consignor or Local Client for Billing", creditOnHoldDocument.DescriptionOfOrganisationBeingCheckedForCredit);
		}

		public void TestIRelatedJobNumber_JobNumber()
		{
			var cartage = Factory.New<CommonCartage>();
			var creditOnHoldDocument = (ICreditControlledDocumentDelivery)cartage;
			AssertNotNull(creditOnHoldDocument.JobNumber);
			AssertEquals(1, creditOnHoldDocument.JobNumber.Length);
			AssertEquals("", creditOnHoldDocument.JobNumber[0]);
			cartage.JJ_ConsignmentID = "T12345678";
			AssertNotNull(creditOnHoldDocument.JobNumber);
			AssertEquals(1, creditOnHoldDocument.JobNumber.Length);
			AssertEquals("T12345678", creditOnHoldDocument.JobNumber[0]);
			cartage.JJ_ConsignmentID = "T87654321";
			AssertNotNull(creditOnHoldDocument.JobNumber);
			AssertEquals(1, creditOnHoldDocument.JobNumber.Length);
			AssertEquals("T87654321", creditOnHoldDocument.JobNumber[0]);
		}

		public void TestGSTOverridesOnExportJobs()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLExportPack;
			AssertEquals("Origin", GlbBranch.CurrentBranch.HomePort.Code, cartage.InvoicingSupporter.Origin.RL_Code);
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			AssertEquals("Destination", GlbBranch.CurrentBranch.HomePort.Code, cartage2.InvoicingSupporter.Destination.RL_Code);
		}

		public void TestGSTOverridesOnDomesticJobsOriginType()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertNotNull(cartage.CartageType);
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_DomesticContainerizedDelivery;
			AssertEquals("Is Domestic Job", true, cartage.IsDomestic());
			AssertEquals("Origin", GlbBranch.CurrentBranch.HomePort.Code, cartage.InvoicingSupporter.Origin.RL_Code);
			AssertEquals("Destination", GlbBranch.CurrentBranch.HomePort.Code, cartage.InvoicingSupporter.Destination.RL_Code);
		}

		public void TestGSTOverridesOnDomesticJobsDestinationType()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertNotNull(cartage.CartageType);
			cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_DomesticContainerizedPickup;
			AssertEquals("Is Domestic Job", true, cartage.IsDomestic());
			AssertEquals("Origin", GlbBranch.CurrentBranch.HomePort.Code, cartage.InvoicingSupporter.Origin.RL_Code);
			AssertEquals("Destination", GlbBranch.CurrentBranch.HomePort.Code, cartage.InvoicingSupporter.Destination.RL_Code);
		}

		public void TestTotalDemurrage()
		{
			var year = ZDateTime.Now.Year;
			var dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			var cartageType = new CompletedDummyCartageType(dummyParent);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "hi";
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			AssertEquals(0, dummyParent.Demurrage.Ticks);
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			leg1.JU_CartagePickupDemurrage = new ZDateTime(year, 1, 1, 1, 0, 0);
			AssertEquals(0, dummyParent.Demurrage.Ticks);
			Factory.Save();
			AssertEquals(36000000000, dummyParent.Demurrage.Ticks);
		}

		public void TestTotalDemurrage_LegRemoved()
		{
			var year = ZDateTime.Now.Year;
			var dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			var cartageType = new CompletedDummyCartageType(dummyParent);
			var cartage = Factory.New<CommonCartage>();
			cartage.SetParent(dummyParent);
			cartage.JJ_ConsignmentID = "hi";
			dummyParent.SetCartageType(cartageType);
			AssertEquals(0, dummyParent.Demurrage.Ticks);
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			var leg1 = move.CartageLegs.AddNew();
			var leg2 = move.CartageLegs.AddNew();
			leg1.JU_CartagePickupDemurrage = new ZDateTime(year, 1, 1, 1, 0, 0);
			leg2.JU_CartagePickupDemurrage = new ZDateTime(year, 1, 1, 0, 1, 0);
			AssertEquals(0, dummyParent.Demurrage.Ticks);
			Factory.Save();
			AssertEquals(36600000000, dummyParent.Demurrage.Ticks);
			leg1.Delete();
			Factory.Save();
			AssertEquals(600000000, dummyParent.Demurrage.Ticks);
		}

		public void TestIAdditionalReferenceNumberSupporter_AdditionalReferenceNumbers()
		{
			var cartage = Factory.New<CommonCartage>();
			var iCartage = (IAdditionalReferenceNumberSupporter)cartage;
			AssertEquals(cartage.AdditionalReferenceNumbers, iCartage.AdditionalReferenceNumbers);
		}

		public void TestIAdditionalReferenceNumberSupporter_IncludeSpecialCustomsInstructionsItems()
		{
			var cartage = Factory.New<CommonCartage>();
			var iCartage = (IAdditionalReferenceNumberSupporter)cartage;
			AssertEquals(false, iCartage.IncludeSpecialCustomsInstructionsItems);
		}

		public void TestIAdditionalReferenceNumberTypeProvider_GetAdditionalReferenceNumberTypeList()
		{
			var expected = new CodeDescriptionPairList();
			foreach (CodeDescriptionPair regType in WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetCodeDescriptionPairList())
			{
				var customType = new CustomsNumberTypeCodeDescription(regType.Code, regType.MultilingualDescription, false);
				expected.Add(customType);
			}

			var cartage = Factory.New<CommonCartage>();
			var iCartage = (IAdditionalReferenceNumberTypeProvider)cartage;
			AssertContainsExactElementsInAnyOrder(expected, iCartage.GetAdditionalReferenceNumberTypeList("", ""));
		}

		public void TestRaiseWorkSheetLegLinkAdded()
		{
			var dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			var cartageType = new CompletedDummyCartageType(dummyParent);
			var cartage = Factory.New<CommonCartage>();
			// no exeption if no event has been hooked
			cartage.RaiseWorkSheetLegLinkAdded(new WorkSheetLegLinkEventArgs(null));
			// event called if it has been hooked
			bool didWorkSheetLegLinkAddedCall = false;
			cartage.OnWorkSheetLegLinkAdded += new EventHandler<WorkSheetLegLinkEventArgs>(delegate(object sender, WorkSheetLegLinkEventArgs e)
			{
				didWorkSheetLegLinkAddedCall = true;
			});
			cartage.RaiseWorkSheetLegLinkAdded(new WorkSheetLegLinkEventArgs(null));
			AssertEquals("Event sould be called", true, didWorkSheetLegLinkAddedCall);
		}

		public void TestScheduleDates()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.SydLaxSailing);
			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.Transports.AddNew();
			consol.Transports[0].JW_JX = sailingHelper.SydLaxSailing.PK;
			var internalCartage = Helper.CreateInternalCartage(shipment);
			Assert(!internalCartage.LCLReceivalCommences.IsEmpty);
			Assert(!internalCartage.LCLCutOff.IsEmpty);
			Assert(!internalCartage.FCLReceivalCommences.IsEmpty);
			Assert(!internalCartage.FCLCutOff.IsEmpty);
			Assert(!internalCartage.FCLAvailabilityDate.IsEmpty);
			Assert(!internalCartage.FCLStorageDate.IsEmpty);
			Assert(!internalCartage.LCLAvailabilityDate.IsEmpty);
			Assert(!internalCartage.LCLStorageDate.IsEmpty);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_CTOReceivalCommences, internalCartage.FCLReceivalCommences);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JA_CTOCutOff, internalCartage.FCLCutOff);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotReceivalCommences, internalCartage.LCLReceivalCommences);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotCutOff, internalCartage.LCLCutOff);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JB_CTOAvailabilityDate, internalCartage.FCLAvailabilityDate);
			AssertEquals(sailingHelper.SydLaxSailing.JX_JB_CTOStorageDate, internalCartage.FCLStorageDate);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotAvailabilityDate, internalCartage.LCLAvailabilityDate);
			AssertEquals(sailingHelper.SydLaxSailing.JX_DepotStorageDate, internalCartage.LCLStorageDate);
			var standaloneCartage = Helper.CreateCartage(Constants.CartageJobType.NEW_AirExport, 1);
			Assert(standaloneCartage.LCLReceivalCommences.IsEmpty);
			Assert(standaloneCartage.LCLCutOff.IsEmpty);
			Assert(standaloneCartage.FCLReceivalCommences.IsEmpty);
			Assert(standaloneCartage.FCLCutOff.IsEmpty);
			Assert(standaloneCartage.FCLAvailabilityDate.IsEmpty);
			Assert(standaloneCartage.FCLStorageDate.IsEmpty);
			Assert(standaloneCartage.LCLAvailabilityDate.IsEmpty);
			Assert(standaloneCartage.LCLStorageDate.IsEmpty);
			sailingHelper.SetupFCLLCLDates(sailingHelper.MelSydFlightLeg);
			standaloneCartage.JJ_JX_Sailing = sailingHelper.MelSydFlightLeg.PK;
			Assert(!standaloneCartage.LCLReceivalCommences.IsEmpty);
			Assert(!standaloneCartage.LCLCutOff.IsEmpty);
			Assert(!standaloneCartage.FCLReceivalCommences.IsEmpty);
			Assert(!standaloneCartage.FCLCutOff.IsEmpty);
			Assert(!standaloneCartage.FCLAvailabilityDate.IsEmpty);
			Assert(!standaloneCartage.FCLStorageDate.IsEmpty);
			Assert(!standaloneCartage.LCLAvailabilityDate.IsEmpty);
			Assert(!standaloneCartage.LCLStorageDate.IsEmpty);
			AssertEquals(sailingHelper.MelSydFlightLeg.JX_JA_CTOReceivalCommences, standaloneCartage.FCLReceivalCommences);
			AssertEquals(sailingHelper.MelSydFlightLeg.JX_JA_CTOCutOff, standaloneCartage.FCLCutOff);
			AssertEquals(sailingHelper.MelSydFlightLeg.JX_DepotReceivalCommences, standaloneCartage.LCLReceivalCommences);
			AssertEquals(sailingHelper.MelSydFlightLeg.JX_DepotCutOff, standaloneCartage.LCLCutOff);
			AssertEquals(sailingHelper.MelSydFlightLeg.JX_JB_CTOAvailabilityDate, standaloneCartage.FCLAvailabilityDate);
			AssertEquals(sailingHelper.MelSydFlightLeg.JX_JB_CTOStorageDate, standaloneCartage.FCLStorageDate);
			AssertEquals(sailingHelper.MelSydFlightLeg.JX_DepotAvailabilityDate, standaloneCartage.LCLAvailabilityDate);
			AssertEquals(sailingHelper.MelSydFlightLeg.JX_DepotStorageDate, standaloneCartage.LCLStorageDate);
		}

		public void TestEstimatedAndActualScheduleDatesArePopulatedForInternalCartage()
		{
			var date1 = ZDateTime.Now;
			var date2 = date1.AddDays(1);
			var date3 = date1.AddDays(2);
			var date4 = date1.AddDays(3);
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.SydLaxSailing);
			var shipment = (CommonShipment)Factory.New<IForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.Transports.AddNew();
			var transport = consol.Transports[0];
			transport.JW_JX = sailingHelper.SydLaxSailing.PK;
			transport.JW_ETD = date1;
			transport.JW_ETA = date2;
			transport.JW_ATD = date3;
			transport.JW_ATA = date4;
			var cartage = Helper.CreateInternalCartage(shipment);
			AssertEquals("Est. Departure", date1, cartage.E_DEP);
			AssertEquals("Est. Arrival", date2, cartage.E_ARV);
			AssertEquals("Act. Departure", date3, cartage.A_DEP);
			AssertEquals("Act. Arrival", date4, cartage.A_ARV);
			AssertEquals("Sch. Departure", date1, cartage.E_DEP);
			AssertEquals("Sch. Arrival", date2, cartage.E_ARV);
		}

		public void TestOnGetCartageLegsToPrintEvent()
		{
			var cartage = Factory.New<CommonCartage>();
			bool eventFired = false;
			cartage.OnGetCartageLegsToPrint += delegate
			{
				eventFired = true;
			};
			cartage.RaiseOnGetCartageLegsToPrint(null);
			AssertEquals("OnGetCartageLegsToPrint event should be fired.", true, eventFired);
			cartage.OnGetCartageLegsToPrint -= delegate
			{
				eventFired = true;
			};
		}

		public void TestDocumentMenuItemVisibilityDependingOnImportExportState()
		{
			var cartage = Factory.New<CommonCartage>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			cartage.JJ_ConsignmentID = "S1/E";
			cartage.JJ_ParentID = shipment.PK;
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
			Factory.Save();
			var documentCommands = new DocumentCommandCollection(cartage);
			documentCommands.Load();
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Export, cartage.JobDirection);
			foreach (var documentCommand in documentCommands.Cast<DocumentCommand>().Where(o => o.SU_DocumentDirection == "ARV"))
			{
				Assert(!documentCommand.IsApplicable);
			}

			foreach (var documentCommand in documentCommands.Cast<DocumentCommand>().Where(o => o.SU_DocumentDirection == "DEP"))
			{
				Assert(documentCommand.IsApplicable);
			}

			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirImport;
			Factory.Save();
			documentCommands = new DocumentCommandCollection(cartage);
			documentCommands.Load();
			AssertEquals(Enterprise.MasterFiles.Business.Directions.Import, cartage.JobDirection);
			foreach (var documentCommand in documentCommands.Cast<DocumentCommand>().Where(o => o.SU_DocumentDirection == "ARV"))
			{
				Assert(documentCommand.IsApplicable);
			}

			foreach (var documentCommand in documentCommands.Cast<DocumentCommand>().Where(o => o.SU_DocumentDirection == "DEP"))
			{
				Assert(!documentCommand.IsApplicable);
			}
		}

		[ExpectNoExceptions]
		public void TestShouldNotAccessPropertyOnDeletedBusinessObject()
		{
			var cartage = Factory.New<CommonCartage>();
			var declaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			cartage.JJ_ParentID = declaration.PK;
			cartage.JJ_ParentTableCode = declaration.TablePrefix;
			cartage.Delete();
			var notes = cartage.BusinessObjectsWithRelatedNotes;
		}

		public void TestShouldCreateAutoLogIfOnlyChildrenHaveChanges()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(false, ((IAutoAdminLogTarget)cartage).ShouldCreateAutoLogIfOnlyChildrenHaveChanges);
			cartage.SetAutoLogOverride();
			AssertEquals(true, ((IAutoAdminLogTarget)cartage).ShouldCreateAutoLogIfOnlyChildrenHaveChanges);
		}

		public void TestShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges()
		{
			var cartage = Factory.New<CommonCartage>();
			AssertEquals(false, ((IUpdateAuditFields)cartage).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);
			cartage.SetAutoLogOverride();
			AssertEquals(true, ((IUpdateAuditFields)cartage).ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges);
		}

		public void TestOrganisationsForCreditChecks()
		{
			var cartage = Factory.New<CommonCartage>();
			Factory.Save();
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(0, ((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Length);
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			var job = new JobHeader.Loader(cartage).TryCreateWithoutMutexForTestOnly();
			cartage.LocalClientPK = localClient.PK;
			var exporter1 = Factory.NewWithValidTestData<OrgHeader>();
			var docAddressWrapper = cartage.DocAddresses.AddNew();
			docAddressWrapper.DocAddressType = DocAddressType.LocalCartageExporter;
			docAddressWrapper.OrganisationPK = exporter1.PK;
			var exporter2 = Factory.NewWithValidTestData<OrgHeader>();
			docAddressWrapper = cartage.DocAddresses.AddNew();
			docAddressWrapper.DocAddressType = DocAddressType.LocalCartageExporter;
			docAddressWrapper.OrganisationPK = exporter2.PK;
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			docAddressWrapper = cartage.DocAddresses.AddNew();
			docAddressWrapper.DocAddressType = DocAddressType.LocalCartageImporter;
			docAddressWrapper.OrganisationPK = importer1.PK;
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			docAddressWrapper = cartage.DocAddresses.AddNew();
			docAddressWrapper.DocAddressType = DocAddressType.LocalCartageImporter;
			docAddressWrapper.OrganisationPK = importer2.PK;
			var contractor = Factory.NewWithValidTestData<OrgHeader>();
			docAddressWrapper = cartage.DocAddresses.AddNew();
			docAddressWrapper.DocAddressType = DocAddressType.Contractor;
			docAddressWrapper.OrganisationPK = contractor.PK;
			Factory.Save();
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(5, ((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(localClient));
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(exporter1));
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(exporter2));
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(importer1));
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(importer2));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
			Action<string> setupOrganizationsEvaluatedForCreditControlRegistry = organizationType =>
			{
				var orgCreditControlCollection = new OrgsEvaluatedForCreditControlCollection();
				var orgCreditControl1 = orgCreditControlCollection.AddNew();
				orgCreditControl1.JobType = cartage.InvoicingSupporter.ConsumerType.Code;
				orgCreditControl1.OrganizationType = organizationType;
				AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControl.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, orgCreditControlCollection);
				Factory.ClearCachedValue<OrgsEvaluatedForCreditControlCollection>(AccountingMasterFilesRegistry.OrganizationsEvaluatedForCreditControlCacheKey());
			};
			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.LocalClient);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(4, ((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Length);
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(exporter1));
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(exporter2));
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(importer1));
			Assert(((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Contains(importer2));
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
			setupOrganizationsEvaluatedForCreditControlRegistry(OrgCodes.All);
			AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly = 0;
			AssertEquals(0, ((ICreditControlledDocumentDelivery)cartage).OrganisationsForCreditChecks.Length);
			AssertLessThanOrEqualTo("OrganizationsEvaluatedForCreditControl registry should be accessed once (at most), and then cached in factory, as reading the default value has poor performance.", AccountingMasterFilesRegistry.Instance.OrganizationsEvaluatedForCreditControlAccessCounter_ForTestOnly, 1);
		}

		public void TestGetWorkflowInformationProvider()
		{
			var cartage = Factory.New<CommonCartage>();
			var workflowInformationProvider = (cartage as IWorkflowProvider).GetWorkflowInformationProvider();
			AssertNotNull(workflowInformationProvider);
			AssertEquals("Origin", "", workflowInformationProvider.Origin);
			AssertEquals("Destination", "", workflowInformationProvider.Destination);
			AssertEquals("Business Context", TrackingConstants.BusinessContext.Cartage, workflowInformationProvider.BusinessContext);
			AssertContainsExactElementsInAnyOrder("Companies", new[] { GlbBranch.CurrentBranch.PK }, workflowInformationProvider.Companies);
		}

		[TestedType(typeof(CommonCartage))]
		class CustomFieldsTest : TestICustomFieldProvider
		{
		}

		public void TestICustomFieldProvider_GetCustomBusinessObject()
		{
			if (!string.IsNullOrEmpty(WorkflowDescriptorCode))
			{
				var template = Helper.CreateWorkflowTemplate(WorkflowDescriptorCode);
				Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
				Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
				Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
				Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
				Factory.Save();
				var cartage = Factory.New<CommonCartage>();
				var provider = (ICustomFieldProvider)cartage;
				var customBizo = provider.GetCustomBusinessObject();
				var dynamicBizo = (IDynamicBusinessObject)customBizo;
				AssertNotNull(dynamicBizo.GetProperty("__STRINGFIELD__prop__ZString"));
				AssertNotNull(dynamicBizo.GetProperty("__INTFIELD__prop__ZInt"));
				AssertNotNull(dynamicBizo.GetProperty("__DATETIMEFIELD__prop__ZDateTime"));
				AssertNotNull(dynamicBizo.GetProperty("__BOOLFIELD__prop__ZBool"));
			}
			else
			{
				Assert("Doesn't Support Workflow Custom Fields", true);
			}
		}

		protected virtual string WorkflowDescriptorCode
		{
			get
			{
				return JobInvoicingConsumerTypes.LocalCartage.Code;
			}
		}

		public void TestJobHeaderIsNotDeactivatedWhenFactoryHasInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(true);
		}

		public void TestJobHeaderIsDeactivatedWhenFactoryDoesNotHaveInvoicingPlugInGUIBusinessContext()
		{
			AssertJobHeaderDeactivation(false);
		}

		void AssertJobHeaderDeactivation(bool hasInvoicingPlugInGUIContext)
		{
			var cartage = Factory.New<CommonCartage>();
			var jobLoader = new JobHeader.Loader(cartage);
			var job = jobLoader.TryCreate();
			Factory.Save();
			cartage.IsCancelled = true;
			if (hasInvoicingPlugInGUIContext)
			{
				Factory.SetContext(BusinessContext.InvoicingPlugInGUI);
			}

			var assertionMessage1 = string.Format("cartage {0} have InvoicingPluginGUI Business Context", hasInvoicingPlugInGUIContext ? "should" : "should not");
			var assertionMessage2 = string.Format("Job {0} be deactivated by OnSaving method", hasInvoicingPlugInGUIContext ? "should not" : "should");
			Assert("Deactivating cartage, IsCancelled flag should be set to true", cartage.IsCancelled);
			Assert("Deactivating cartage, IsCancelledInfo should have changes", cartage.IsCancelledHasChanged);
			AssertEquals(assertionMessage1, hasInvoicingPlugInGUIContext, cartage.HasContext(BusinessContext.InvoicingPlugInGUI));
			Assert("Job is not yet deactivated", !job.IsCancelled);
			Factory.Save();
			AssertEquals(assertionMessage2, !hasInvoicingPlugInGUIContext, job.IsCancelled);
		}

		public void TestSavingNewCartageWithParentWillGenerateCreatedEventsOnParent()
		{
			var (cartage, cartageType, cartageParent, orgProxy) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory, overrideLocalTransportProviderOrgName: "A Local Transport Provider Org");
			var mostRecentLog = ((IStmALogParent)cartageParent).Logs.MostRecentLog;
			AssertEquals("Most recent log should be ICC - Internal Cartage Job Created", Events.InternalCartageJobCreatedCode, mostRecentLog.SL_SE_NKEvent);
			AssertEquals("Most recent log should have reference in correct format and with correct values for parameters JOB (JobID), NAM (Transport Provider Org Name) and TYP (Direction)", "|JOB=" + cartage.JJ_ConsignmentID + "|NAM=A Local Transport Provider Org|TYP=EXP", mostRecentLog.SL_Reference);
		}

		public void TestScheduleRelatedProperties_PortTransportRelatedToShipment_AreReadOnly()
		{
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartage = Factory.New<CommonCartage>();
			cartage.SetParent(dummyCartageParent);
			CombineAssertions("When a port transport is made from a shipment, schedule related properties should be readonly", () =>
			{
				AssertEquals("Should make VoyageFlight readonly", true, cartage.VoyageFlightInfo.ReadOnly);
				AssertEquals("Should make Vessel readonly", true, cartage.VesselInfo.ReadOnly);
				AssertEquals("Should make PortOfLoading readonly", true, cartage.PortOfLoadingInfo.ReadOnly);
				AssertEquals("Should make PortOfDischarge readonly", true, cartage.PortOfDischargeInfo.ReadOnly);
				AssertEquals("Should make E_DEP readonly", true, cartage.E_DEPInfo.ReadOnly);
				AssertEquals("Should make E_ARV readonly", true, cartage.E_ARVInfo.ReadOnly);
				AssertEquals("Should make FCLAvailabilityDate readonly", true, cartage.FCLAvailabilityDateInfo.ReadOnly);
				AssertEquals("Should make FCLStorageDate readonly", true, cartage.FCLStorageDateInfo.ReadOnly);
				AssertEquals("Should make FCLCutOff readonly", true, cartage.FCLCutOffInfo.ReadOnly);
				AssertEquals("Should make FCLReceivalCommences readonly", true, cartage.FCLReceivalCommencesInfo.ReadOnly);
				AssertEquals("Should make LCLAvailabilityDate readonly", true, cartage.LCLAvailabilityDateInfo.ReadOnly);
				AssertEquals("Should make LCLStorageDate readonly", true, cartage.LCLStorageDateInfo.ReadOnly);
				AssertEquals("Should make LCLCutOff readonly", true, cartage.LCLCutOffInfo.ReadOnly);
				AssertEquals("Should make LCLReceivalCommences readonly", true, cartage.LCLReceivalCommencesInfo.ReadOnly);
			});
		}

		public void TestScheduleRelatedProperties_PortTransportRelatedToTransportBooking_AreReadOnly()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.MelSydFlightLeg);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_JX_Sailing = sailingHelper.MelSydFlightLeg.PK;
			var dtbBooking = (BusinessObject)Factory.New<IDtbBooking>();
			dtbBooking.FillWithValidTestData();
			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = dtbBooking.TablePrefix;
			Factory.Save();
			AssertEquals("Precondition: HasParent is false", false, cartage.HasParent);
			AssertEquals("Precondition: ParentJob exists", dtbBooking, cartage.ParentJob);
			AssertEquals("ParentJob should not have an external TB reference", false, ((ITransportAdditionalReferenceNumbers)cartage.ParentJob).AdditionalReferenceNumbers.ToArray().Select(n => ((ICusEntryNumber)n).CE_EntryType).Contains(AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber));
			AssertNotNull("Precondition: SailingStandalone is not null", cartage.SailingStandalone);
			CombineAssertions("When a port transport is made from a transport booking, schedule related properties should be readonly", () =>
			{
				AssertEquals("Should make VoyageFlight readonly", true, cartage.VoyageFlightInfo.ReadOnly);
				AssertEquals("Should make Vessel readonly", true, cartage.VesselInfo.ReadOnly);
				AssertEquals("Should make PortOfLoading readonly", true, cartage.PortOfLoadingInfo.ReadOnly);
				AssertEquals("Should make PortOfDischarge readonly", true, cartage.PortOfDischargeInfo.ReadOnly);
				AssertEquals("Should make E_DEP readonly", true, cartage.E_DEPInfo.ReadOnly);
				AssertEquals("Should make E_ARV readonly", true, cartage.E_ARVInfo.ReadOnly);
				AssertEquals("Should make FCLAvailabilityDate readonly", true, cartage.FCLAvailabilityDateInfo.ReadOnly);
				AssertEquals("Should make FCLStorageDate readonly", true, cartage.FCLStorageDateInfo.ReadOnly);
				AssertEquals("Should make FCLCutOff readonly", true, cartage.FCLCutOffInfo.ReadOnly);
				AssertEquals("Should make FCLReceivalCommences readonly", true, cartage.FCLReceivalCommencesInfo.ReadOnly);
				AssertEquals("Should make LCLAvailabilityDate readonly", true, cartage.LCLAvailabilityDateInfo.ReadOnly);
				AssertEquals("Should make LCLStorageDate readonly", true, cartage.LCLStorageDateInfo.ReadOnly);
				AssertEquals("Should make LCLCutOff readonly", true, cartage.LCLCutOffInfo.ReadOnly);
				AssertEquals("Should make LCLReceivalCommences readonly", true, cartage.LCLReceivalCommencesInfo.ReadOnly);
			});
		}

		public void TestScheduleRelatedProperties_PortTransportRelatedToTransportBookingFromExternalSource_AreNotReadOnly()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.MelSydFlightLeg);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_JX_Sailing = sailingHelper.MelSydFlightLeg.PK;
			var dtbBooking = (BusinessObject)Factory.New<IDtbBooking>();
			dtbBooking.FillWithValidTestData();

			var referenceNum = ((ITransportAdditionalReferenceNumbers)dtbBooking).AdditionalReferenceNumbers.AddNew();
			referenceNum.CE_EntryType = AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber;

			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = dtbBooking.TablePrefix;
			Factory.Save();
			AssertEquals("Precondition: HasParent is false", false, cartage.HasParent);
			AssertEquals("Precondition: ParentJob exists", dtbBooking, cartage.ParentJob);
			AssertEquals("ParentJob should have an external TB reference", true, ((ITransportAdditionalReferenceNumbers)cartage.ParentJob).AdditionalReferenceNumbers.ToArray().Select(n => ((ICusEntryNumber)n).CE_EntryType).Contains(AdditionalReferenceTypes.Codes.ExternalTransportBookingNumber));
			AssertNotNull("Precondition: SailingStandalone is not null", cartage.SailingStandalone);
			CombineAssertions("When a port transport is made from a transport booking, schedule related properties should be readonly", () =>
			{
				AssertEquals("Should not make VoyageFlight readonly", false, cartage.VoyageFlightInfo.ReadOnly);
				AssertEquals("Should not make Vessel readonly", false, cartage.VesselInfo.ReadOnly);
				AssertEquals("Should not make PortOfLoading readonly", false, cartage.PortOfLoadingInfo.ReadOnly);
				AssertEquals("Should not make PortOfDischarge readonly", false, cartage.PortOfDischargeInfo.ReadOnly);
				AssertEquals("Should not make E_DEP readonly", false, cartage.E_DEPInfo.ReadOnly);
				AssertEquals("Should not make E_ARV readonly", false, cartage.E_ARVInfo.ReadOnly);
				AssertEquals("Should not make FCLAvailabilityDate readonly", false, cartage.FCLAvailabilityDateInfo.ReadOnly);
				AssertEquals("Should not make FCLStorageDate readonly", false, cartage.FCLStorageDateInfo.ReadOnly);
				AssertEquals("Should not make FCLCutOff readonly", false, cartage.FCLCutOffInfo.ReadOnly);
				AssertEquals("Should not make FCLReceivalCommences readonly", false, cartage.FCLReceivalCommencesInfo.ReadOnly);
				AssertEquals("Should not make LCLAvailabilityDate readonly", false, cartage.LCLAvailabilityDateInfo.ReadOnly);
				AssertEquals("Should not make LCLStorageDate readonly", false, cartage.LCLStorageDateInfo.ReadOnly);
				AssertEquals("Should not make LCLCutOff readonly", false, cartage.LCLCutOffInfo.ReadOnly);
				AssertEquals("Should not make LCLReceivalCommences readonly", false, cartage.LCLReceivalCommencesInfo.ReadOnly);
			});
		}

		public void TestScheduleRelatedProperties_SailingStandaloneNull()
		{
			var cartage = Factory.New<CommonCartage>();
			Factory.Save();
			AssertEquals("Precondition: HasParent is false", false, cartage.HasParent);
			AssertNull("Precondition: ParentJob is null", cartage.ParentJob);
			AssertNull("Precondition: SailingStandalone is null", cartage.SailingStandalone);
			CombineAssertions("Regression testing of existing behaviour", () =>
			{
				AssertEquals(false, cartage.VoyageFlightInfo.ReadOnly);
				AssertEquals(false, cartage.VesselInfo.ReadOnly);
				AssertEquals(false, cartage.PortOfLoadingInfo.ReadOnly);
				AssertEquals(false, cartage.PortOfDischargeInfo.ReadOnly);
				AssertEquals(false, cartage.E_DEPInfo.ReadOnly);
				AssertEquals(false, cartage.E_ARVInfo.ReadOnly);
				AssertEquals(true, cartage.FCLAvailabilityDateInfo.ReadOnly);
				AssertEquals(true, cartage.FCLStorageDateInfo.ReadOnly);
				AssertEquals(true, cartage.FCLCutOffInfo.ReadOnly);
				AssertEquals(true, cartage.FCLReceivalCommencesInfo.ReadOnly);
				AssertEquals(true, cartage.LCLAvailabilityDateInfo.ReadOnly);
				AssertEquals(true, cartage.LCLStorageDateInfo.ReadOnly);
				AssertEquals(true, cartage.LCLCutOffInfo.ReadOnly);
				AssertEquals(true, cartage.LCLReceivalCommencesInfo.ReadOnly);
			});
		}

		public void TestNonScheduleRelatedProperties_PortTransportRelatedToTransportBooking()
		{
			var sailingHelper = new SailingsForTestClasses(Factory);
			sailingHelper.SetupFCLLCLDates(sailingHelper.MelSydFlightLeg);
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_JX_Sailing = sailingHelper.MelSydFlightLeg.PK;
			var dtbBooking = (BusinessObject)Factory.New<IDtbBooking>();
			dtbBooking.FillWithValidTestData();
			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = dtbBooking.TablePrefix;
			Factory.Save();
			AssertEquals("Precondition: HasParent is false", false, cartage.HasParent);
			AssertNotNull("Precondition: SailingStandalone is not null", cartage.SailingStandalone);
			CombineAssertions("Regression testing of existing behaviour", () =>
			{
				AssertEquals(false, cartage.JJ_E3_NKJobTypeInfo.ReadOnly);
				AssertEquals("JJ_ConsignmentID is always readonly", true, cartage.JJ_ConsignmentIDInfo.ReadOnly);
				AssertEquals(false, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals(false, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals(false, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
				AssertEquals("GrossWeight is always readonly", true, cartage.GrossWeightInfo.ReadOnly);
				AssertEquals(false, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals(false, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals(false, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals(false, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals(false, cartage.LocalClientAddressPKInfo.ReadOnly);
				// A_DEP and A_ARV being read only is inconsistent with the ReadOnlyMember attribute for them
				AssertEquals(true, cartage.A_DEPInfo.ReadOnly);
				AssertEquals(true, cartage.A_ARVInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_ParentIsForwardingShipment_AreReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();
			var parent = (BusinessObject)Factory.New<IForwardingShipment>();
			parent.FillWithValidTestData();
			cartage.JJ_ParentID = parent.PK;
			cartage.JJ_ParentTableCode = parent.TablePrefix;

			Factory.Save();

			CombineAssertions("When a port transport is made from a forwarding shipment, waybill and job total properties should be readonly", () =>
			{
				AssertEquals("Should make JJ_WaybillNumber readonly", true, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should make JJ_Weight readonly", true, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should make JJ_WeightUQ readonly", true, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should make JJ_Volume readonly", true, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should make JJ_VolumeUQ readonly", true, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should make JJ_OuterPacks readonly", true, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should make JJ_F3_NKPackType readonly", true, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_ParentIsCFSShipment_AreReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();
			var parent = (BusinessObject)Factory.New<ICFSShipment>();
			parent.FillWithValidTestData();
			cartage.JJ_ParentID = parent.PK;
			cartage.JJ_ParentTableCode = parent.TablePrefix;

			Factory.Save();

			CombineAssertions("When a port transport is made from a CFS shipment, waybill and job total properties should be readonly", () =>
			{
				AssertEquals("Should make JJ_WaybillNumber readonly", true, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should make JJ_Weight readonly", true, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should make JJ_WeightUQ readonly", true, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should make JJ_Volume readonly", true, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should make JJ_VolumeUQ readonly", true, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should make JJ_OuterPacks readonly", true, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should make JJ_F3_NKPackType readonly", true, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_ParentIsWhsOrder_AreReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();
			var parent = (BusinessObject)Factory.New<IWhsOrder>();
			parent.FillWithValidTestData();
			cartage.JJ_ParentID = parent.PK;
			cartage.JJ_ParentTableCode = parent.TablePrefix;

			Factory.Save();

			CombineAssertions("When a port transport is made from a warehouse order, waybill and job total properties should be readonly", () =>
			{
				AssertEquals("Should make JJ_WaybillNumber readonly", true, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should make JJ_Weight readonly", true, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should make JJ_WeightUQ readonly", true, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should make JJ_Volume readonly", true, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should make JJ_VolumeUQ readonly", true, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should make JJ_OuterPacks readonly", true, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should make JJ_F3_NKPackType readonly", true, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_ParentIsJobDeclaration_AreReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();
			var parent = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			parent.FillWithValidTestData();
			cartage.JJ_ParentID = parent.PK;
			cartage.JJ_ParentTableCode = parent.TablePrefix;

			Factory.Save();

			CombineAssertions("When a port transport is made from a job declaration, waybill and job total properties should be readonly", () =>
			{
				AssertEquals("Should make JJ_WaybillNumber readonly", true, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should make JJ_Weight readonly", true, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should make JJ_WeightUQ readonly", true, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should make JJ_Volume readonly", true, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should make JJ_VolumeUQ readonly", true, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should make JJ_OuterPacks readonly", true, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should make JJ_F3_NKPackType readonly", true, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_ParentIsCFSLoadListConsol_AreReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();
			var parent = (BusinessObject)Factory.New<ICFSLoadListConsol>();
			parent.FillWithValidTestData();
			cartage.JJ_ParentID = parent.PK;
			cartage.JJ_ParentTableCode = parent.TablePrefix;

			Factory.Save();

			CombineAssertions("When a port transport is made from a CFS load list consol, waybill and job total properties should be readonly", () =>
			{
				AssertEquals("Should make JJ_WaybillNumber readonly", true, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should make JJ_Weight readonly", true, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should make JJ_WeightUQ readonly", true, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should make JJ_Volume readonly", true, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should make JJ_VolumeUQ readonly", true, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should make JJ_OuterPacks readonly", true, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should make JJ_F3_NKPackType readonly", true, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_ParentIsTransportBookingFromForwardingShipment_AreReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();
			var dtbBooking = Factory.New<IDtbBooking>();
			var consol = Factory.New<IDtbBookingConsolidation>();
			dtbBooking.KM_KB_Booking = consol.PK;

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment.FillWithValidTestData();
			consol.KB_ParentID = shipment.PK;
			consol.KB_ParentTableCode = shipment.TablePrefix;

			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = ((BusinessObject)dtbBooking).TablePrefix;

			Factory.Save();

			CombineAssertions("When a port transport is made from a transport booking from a forwarding shipment, waybill and job total properties should be readonly", () =>
			{
				AssertEquals("Should make JJ_WaybillNumber readonly", true, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should make JJ_Weight readonly", true, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should make JJ_WeightUQ readonly", true, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should make JJ_Volume readonly", true, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should make JJ_VolumeUQ readonly", true, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should make JJ_OuterPacks readonly", true, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should make JJ_F3_NKPackType readonly", true, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_ParentIsTransportBookingWithNoParent_AreNotReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();
			var dtbBooking = Factory.New<IDtbBooking>();
			var consol = Factory.New<IDtbBookingConsolidation>();
			dtbBooking.KM_KB_Booking = consol.PK;

			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = ((BusinessObject)dtbBooking).TablePrefix;

			Factory.Save();

			CombineAssertions("When a port transport is made from a transport booking with no parent, waybill and job total properties should not be readonly", () =>
			{
				AssertEquals("Should not make JJ_WaybillNumber readonly", false, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should not make JJ_Weight readonly", false, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should not make JJ_WeightUQ readonly", false, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should not make JJ_Volume readonly", false, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should not make JJ_VolumeUQ readonly", false, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should not make JJ_OuterPacks readonly", false, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should not make JJ_F3_NKPackType readonly", false, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_ParentIsTransportBookingWithAgencyBookingParent_AreNotReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();
			var dtbBooking = Factory.New<IDtbBooking>();
			var consol = Factory.New<IDtbBookingConsolidation>();
			dtbBooking.KM_KB_Booking = consol.PK;

			var agentBookingParent = (BusinessObject)Factory.New<IDtbAgentBooking>();
			agentBookingParent.FillWithValidTestData();
			consol.KB_ParentID = agentBookingParent.PK;
			consol.KB_ParentTableCode = agentBookingParent.TablePrefix;

			cartage.JJ_ParentID = dtbBooking.PK;
			cartage.JJ_ParentTableCode = ((BusinessObject)dtbBooking).TablePrefix;

			Factory.Save();

			CombineAssertions("When a port transport is made from a transport booking with an agent booking parent, waybill and job total properties should not be readonly", () =>
			{
				AssertEquals("Should not make JJ_WaybillNumber readonly", false, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should not make JJ_Weight readonly", false, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should not make JJ_WeightUQ readonly", false, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should not make JJ_Volume readonly", false, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should not make JJ_VolumeUQ readonly", false, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should not make JJ_OuterPacks readonly", false, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should not make JJ_F3_NKPackType readonly", false, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestWaybillAndJobTotalProperties_HasNoParent_AreNotReadOnly()
		{
			var cartage = Factory.New<CommonCartage>();

			cartage.JJ_ParentID = Guid.Empty;
			cartage.JJ_ParentTableCode = null;

			Factory.Save();

			CombineAssertions("When a port transport has no parent, waybill and job total properties should not be readonly", () =>
			{
				AssertEquals("Should not make JJ_WaybillNumber readonly", false, cartage.JJ_WaybillNumberInfo.ReadOnly);
				AssertEquals("Should not make JJ_Weight readonly", false, cartage.JJ_WeightInfo.ReadOnly);
				AssertEquals("Should not make JJ_WeightUQ readonly", false, cartage.JJ_WeightUQInfo.ReadOnly);
				AssertEquals("Should not make JJ_Volume readonly", false, cartage.JJ_VolumeInfo.ReadOnly);
				AssertEquals("Should not make JJ_VolumeUQ readonly", false, cartage.JJ_VolumeUQInfo.ReadOnly);
				AssertEquals("Should not make JJ_OuterPacks readonly", false, cartage.JJ_OuterPacksInfo.ReadOnly);
				AssertEquals("Should not make JJ_F3_NKPackType readonly", false, cartage.JJ_F3_NKPackTypeInfo.ReadOnly);
			});
		}

		public void TestJH_GS_NKRepSalesSecurity()
		{
			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			salesRep.GS_Code = "AAA";
			var salesRep2 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep2.GS_Code = "BBB";
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportUnpack, 1);
			var job = new JobHeader.Loader(cartage).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GS_NKRepSales = salesRep.GS_Code;
			Factory.Save();

			Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.TransportJobInvoicing, SecurityCore.AllowOverrideSalesRep).IsAllowed = false;
			cartage.Job.JH_GS_NKRepSales = salesRep2.GS_Code;
			AssertHasError("Precondition: security error message when setting the Sales Rep", cartage.Job.JH_GS_NKRepSalesInfo,
				"You do not have sufficient security rights to modify this field. You must reset the value to its previous value AAA");

			Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.TransportJobInvoicing, SecurityCore.AllowOverrideSalesRep).IsAllowed = true;
			cartage = new BusinessObjectFactory().Load<CommonCartage>(cartage.PK);
			cartage.Job.JH_GS_NKRepSales = salesRep2.GS_Code;
			AssertNoErrors("Should not have security error message", cartage.Job.JH_GS_NKRepSalesInfo);
		}

		List<JobDocAddress> GetPersistentJobDocAddresses(CommonCartage cartage)
		{
			var result = new List<JobDocAddress>();
			foreach (JobDocAddress docAddress in cartage.DocAddresses)
			{
				if (docAddress.DocAddressType != DocAddressType.None && docAddress.DocAddressType != DocAddressType.NonPersistent)
				{
					result.Add(docAddress);
				}
			}

			return result;
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;

		class TestNotify : INotifications, INotificationSubscriberQueryUser
		{
			void INotifications.Add(INotification notification)
			{
				Notifications.Add(notification);
			}

			void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
			{
				Events.Add(e);
				var queryUserArgs = e as QueryUserYesNoEventArgs;
				if (queryUserArgs != null)
				{
					LastEventYesNoArgs = queryUserArgs;
					queryUserArgs.Response = ResponseToDialogs;
				}

				var queryUserDropModeArgs = e as QueryUserCartageTypeDropModeEventArgs;
				if (queryUserDropModeArgs != null)
				{
					LastDropModeEventArgs = queryUserDropModeArgs;
					queryUserDropModeArgs.Response = ResponseToDropModeDialog;
				}
			}

			public void ClearLastEventArgs()
			{
				LastEventYesNoArgs = null;
			}

			public List<IQueryUserEventArgs> Events = new List<IQueryUserEventArgs>();
			public List<INotification> Notifications = new List<INotification>();
			public bool ResponseToDialogs { get; set; }

			public DropMode ResponseToDropModeDialog { get; set; }

			public QueryUserCartageTypeDropModeEventArgs LastDropModeEventArgs { get; private set; }

			public QueryUserYesNoEventArgs LastEventYesNoArgs { get; private set; }
		}
	}
}
