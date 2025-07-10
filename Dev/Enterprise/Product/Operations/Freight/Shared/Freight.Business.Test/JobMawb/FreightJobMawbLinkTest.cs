using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightJobMawbLinkTest : TestCaseWithFactory
	{
		public void TestLoadUnusedByAirlineAndService_AlwaysReturnsSameResult()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			ZString parentPrefix = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			AssertEquals("Found unused JobMawb", mawb1, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", ZGuid.NewZGuid(), parentPrefix));

			JobMawb mawb2 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "11111111", "OTH");

			Factory.Save();

			var parentId = ZGuid.NewZGuid();
			AssertEquals("Found unused JobMawb", mawb2, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", parentId, parentPrefix));

			FreightJobMawbLink.DoDeallocateMAWB(mawb2.PK, parentId, null);

			AssertEquals("Found same unused JobMawb", mawb2, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", ZGuid.NewZGuid(), parentPrefix));
		}

		public void TestLoadUnusedByAirlineAndService_OtherBranch()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, OtherBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			ZString parentPrefix = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));

			using (FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Found unused JobMawb", mawb1, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
			}
		}

		public void TestLoadUnusedByAirlineAndService_ExceptionTypeIsZCannotSaveException()
		{
			var mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, OtherBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			var parentPrefix = JobConsolSchema.Constants.Prefix;

			Factory.Save();

			AssertExceptionThrown(typeof(ZCannotSaveException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
		}

		public void TestLoadUnusedByAirlineAndService()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb2 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000002", OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb3 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000011", OrgCarrierServiceLevel.AllCode);
			JobMawb mawb4 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000012", OrgCarrierServiceLevel.AllCode);

			ZString parentPrefix = JobConsolSchema.Constants.Prefix;

			Factory.Save();
			var parentId = ZGuid.NewZGuid();
			AssertEquals("Found unused JobMawb", mawb1, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", parentId, parentPrefix));
			AssertEquals("Found unused JobMawb", mawb3, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", ZGuid.NewZGuid(), parentPrefix));

			AssertEquals("Found unused JobMawb", mawb2, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
			AssertEquals("Found unused JobMawb", mawb4, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", ZGuid.NewZGuid(), parentPrefix));

			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", ZGuid.NewZGuid(), parentPrefix));

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			JobMawb mawb1OnOtherFactory = Factory.Load<JobMawb>(mawb1.PK);
			JobMawb mawb2OnOtherFactory = Factory.Load<JobMawb>(mawb2.PK);

			FreightJobMawbLink.DoDeallocateMAWB(mawb1.PK, parentId, null);

			AssertEquals("Found unused JobMawb", mawb1OnOtherFactory, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", ZGuid.NewZGuid(), parentPrefix));
		}

		public void TestLoadUnusedByAirlineAndService_Parent()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			ZString parentPrefix = JobConsolSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Found unused JobMawb", mawb1, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", ZGuid.NewZGuid(), parentPrefix));

			JobMawb mawb2 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "11111111", "OTH");
			Factory.Save();

			var parentId = ZGuid.NewZGuid();
			AssertEquals("Found unused JobMawb", mawb2, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", parentId, parentPrefix));

			FreightJobMawbLink.DoDeallocateMAWB(mawb2.PK, parentId, null);

			AssertEquals("Found unused JobMawb", mawb2, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "OTH", ZGuid.NewZGuid(), parentPrefix));
		}

		public void TestLoadUnusedByAirlineAndService_ServiceLevelFallBack()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb2 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000011", OrgCarrierServiceLevel.StandardCode);
			Factory.Save();

			ZString parentPrefix = JobConsolSchema.Constants.Prefix;

			AssertEquals("Found unused JobMawb with exact Srvc Lvl", mawb1, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "", "CRP", ZGuid.NewZGuid(), parentPrefix));

			JobMawb mawb3 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000022", OrgCarrierServiceLevel.AllCode);
			Factory.Save();

			AssertEquals("Found unused JobMawb with exact Srvc Lvl", mawb2, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "STD", ZGuid.NewZGuid(), parentPrefix));
			AssertEquals("Found unused JobMawb with ALL Srvc Lvl", mawb3, FreightJobMawbLink.AllocateUnusedMAWB("176", "", "CRP", ZGuid.NewZGuid(), parentPrefix));
		}

		public void TestLoadFromParentPK()
		{
			ZGuid dummyPK = ZGuid.NewZGuid();

			AssertNull("Found JobMawb", FreightJobMawbLink.LoadFromParentPK(JobConsolSchema.Constants.Prefix, dummyPK));

			JobMawb mawb = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);

			mawb.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			mawb.JM_ParentID = dummyPK;

			AssertEquals("Found JobMawb", mawb, FreightJobMawbLink.LoadFromParentPK(JobConsolSchema.Constants.Prefix, dummyPK));
			AssertNull("Did not find JobMawb", FreightJobMawbLink.LoadFromParentPK(JobConsolSchema.Constants.Prefix, ZGuid.NewZGuid()));
			AssertEquals("Found JobMawb", mawb, FreightJobMawbLink.LoadFromParentPK(mawb.JM_ParentTableCode, mawb.JM_ParentID));
		}

		public void TestLoadExistingByAirlineAndMawbNo()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb2 = AddJobMAWB(GlbCompany.CurrentCompany, OtherBranch, "176", "10000011", OrgCarrierServiceLevel.AllCode);
			JobMawb mawb3 = AddJobMAWB(OtherCompanyBranch.Company, OtherCompanyBranch, "081", "10000022", OrgCarrierServiceLevel.StandardCode);

			AssertEquals("Found mawb1", mawb1, FreightJobMawbLink.LoadExistingByMAWB("176", "10000001"));
			AssertNull("Did not find non existing mawb", FreightJobMawbLink.LoadExistingByMAWB("555", "10000001"));
			AssertNull("Should not find mawb2 (different branch)", FreightJobMawbLink.LoadExistingByMAWB("176", "10000011"));
			AssertNull("Should not find mawb3 (different company)", FreightJobMawbLink.LoadExistingByMAWB("081", "10000022"));

			FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals("Should find mawb2", mawb2, FreightJobMawbLink.LoadExistingByMAWB("176", "10000011"));
			AssertNull("Still should not find mawb3 (different company)", FreightJobMawbLink.LoadExistingByMAWB("081", "10000022"));
		}

		public void TestLoadFromBookingReference()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			ZString parentPrefix = JobConsolSchema.Constants.Prefix;
			Factory.Save();

			var parentId = ZGuid.NewZGuid();
			AssertEquals(mawb1, FreightJobMawbLink.AllocateUnusedMAWB("176", "17610000001", "", parentId, parentPrefix));

			mawb1.JM_IsPrinted = true;

			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "17610000001", "", ZGuid.NewZGuid(), parentPrefix));

			mawb1.JM_IsPrinted = false;
			mawb1.JM_ParentTableCode = "XX";
			mawb1.JM_ParentID = ZGuid.NewZGuid();

			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "17610000001", "", ZGuid.NewZGuid(), parentPrefix));

			FreightJobMawbLink.DoDeallocateMAWB(mawb1.PK, parentId, null);

			AssertEquals(mawb1, FreightJobMawbLink.AllocateUnusedMAWB("176", "17610000001", "", ZGuid.NewZGuid(), parentPrefix));
		}

		public void TestLoadFromBookingReference_Numbers()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb2 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000002", OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb3 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000003", OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb4 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000004", OrgCarrierServiceLevel.StandardCode);
			Factory.Save();

			ZGuid dummyPK = ZGuid.NewZGuid();
			ZString parentPrefix = JobConsolSchema.Constants.Prefix;

			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB(ZString.Empty, ZString.Empty, ZString.Empty, dummyPK, parentPrefix));
			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", ZString.Empty, ZString.Empty, dummyPK, parentPrefix));
			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "   ", ZString.Empty, dummyPK, parentPrefix));
			AssertExceptionThrown(typeof(MAWBAllocationException), () => FreightJobMawbLink.AllocateUnusedMAWB("176", "10000001", ZString.Empty, dummyPK, parentPrefix));

			AssertEquals(mawb1, FreightJobMawbLink.AllocateUnusedMAWB("176", "17610000001", ZString.Empty, dummyPK, parentPrefix));
			AssertEquals(mawb2, FreightJobMawbLink.AllocateUnusedMAWB("176", "176-10000002", ZString.Empty, ZGuid.NewZGuid(), parentPrefix));
			AssertEquals(mawb3, FreightJobMawbLink.AllocateUnusedMAWB("176", "176 - 10000003", ZString.Empty, ZGuid.NewZGuid(), parentPrefix));
			AssertEquals(mawb4, FreightJobMawbLink.AllocateUnusedMAWB("176", "XXX10000004", ZString.Empty, ZGuid.NewZGuid(), parentPrefix));
		}

		public void TestNoJobMawbsAvailable()
		{
			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000011", OrgCarrierServiceLevel.AllCode);
			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "081", "10000044", OrgCarrierServiceLevel.StandardCode);
			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "081", "10000055", OrgCarrierServiceLevel.AllCode);
			AddJobMAWB(GlbCompany.CurrentCompany, OtherBranch, "176", "10000071", OrgCarrierServiceLevel.StandardCode);
			AddJobMAWB(GlbCompany.CurrentCompany, OtherBranch, "176", "10000141", OrgCarrierServiceLevel.StandardCode);
			AddJobMAWB(OtherCompanyBranch.Company, OtherCompanyBranch, "176", "10000066", OrgCarrierServiceLevel.StandardCode);

			Factory.Save();

			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "STD" }));

			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000023", OrgCarrierServiceLevel.StandardCode);

			Factory.Save();

			AssertEquals(2, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "STD" }));
			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "ALL" }));
			AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "OTH" }));
			AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("083", new ZString[] { "STD" }));
			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("081", new ZString[] { "STD" }));
			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("081", new ZString[] { "ALL" }));

			FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(4, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "STD" }));
			AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "OTH" }));
			AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("083", new ZString[] { "STD" }));
		}

		public void TestNoJobMawbsAvailable_ServiceLevelFallBack()
		{
			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000011", OrgCarrierServiceLevel.AllCode);
			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "081", "10000044", OrgCarrierServiceLevel.StandardCode);
			AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "081", "10000055", OrgCarrierServiceLevel.AllCode);
			AddJobMAWB(GlbCompany.CurrentCompany, OtherBranch, "176", "10000071", OrgCarrierServiceLevel.StandardCode);
			AddJobMAWB(GlbCompany.CurrentCompany, OtherBranch, "176", "10000141", OrgCarrierServiceLevel.StandardCode);
			AddJobMAWB(OtherCompanyBranch.Company, OtherCompanyBranch, "176", "10000066", OrgCarrierServiceLevel.StandardCode);

			Factory.Save();

			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "STD" }));
			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "ALL" }));
			AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "CRP" }));
			AssertEquals(2, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "STD", "ALL" }));
			AssertEquals(2, FreightJobMawbLink.NoJobMawbsAvailable("176", null));

			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("081", new ZString[] { "STD" }));
			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("081", new ZString[] { "ALL" }));
			AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("081", new ZString[] { "CRP" }));
			AssertEquals(2, FreightJobMawbLink.NoJobMawbsAvailable("081", new ZString[] { "CRP", "STD", "ALL" }));
			AssertEquals(2, FreightJobMawbLink.NoJobMawbsAvailable("081", null));

			using (OtherBranch.SetAsTemporaryContext())
			{
				AssertEquals(2, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "STD" }));
				AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "ALL" }));
				AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("081", null));
				AssertEquals(0, FreightJobMawbLink.NoJobMawbsAvailable("081", new ZString[] { "STD" }));
			}

			FreightDataRegistry.Instance.AllowMatchingOfMAWBsFromOtherBranches.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertEquals(3, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "STD" }));
			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("176", new ZString[] { "ALL" }));
			AssertEquals(2, FreightJobMawbLink.NoJobMawbsAvailable("081", null));
			AssertEquals(1, FreightJobMawbLink.NoJobMawbsAvailable("081", new ZString[] { "STD" }));
		}

		public void TestLoadUnusedMultipleTimesDoesntCache()
		{
			JobMawb mawb1 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb2 = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000012", OrgCarrierServiceLevel.StandardCode);
			ZString parentPrefix = JobConsolSchema.Constants.Prefix;
			Factory.Save();

			BusinessObjectFactory myFactory = new BusinessObjectFactory();
			FreightJobMawbLink myLink = new FreightJobMawbLink(myFactory);
			JobMawb resultMawb = myLink.AllocateUnusedMAWB("176", "", OrgCarrierServiceLevel.StandardCode, ZGuid.NewZGuid(), parentPrefix);
			AssertEquals(mawb1.JM_MAWB, resultMawb.JM_MAWB);

			resultMawb.JM_ParentID = ZGuid.NewZGuid();
			Factory.Save();
			AssertEquals(mawb2.JM_MAWB, myLink.AllocateUnusedMAWB("176", "", OrgCarrierServiceLevel.StandardCode, ZGuid.NewZGuid(), parentPrefix).JM_MAWB);
		}

		public void TestLoadByAirlineAndMawbNo()
		{
			JobMawb mawb = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			mawb.JM_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-5);
			Factory.Save();

			AssertEquals(mawb, FreightJobMawbLink.LoadByAirlineAndMawbNo("176", "10000001", ZDateTime.Empty));
			AssertEquals(mawb, FreightJobMawbLink.LoadByAirlineAndMawbNo("176", "10000001", ZDateTime.Now.AddMonths(-6)));
			AssertEquals(null, FreightJobMawbLink.LoadByAirlineAndMawbNo("176", "10000001", ZDateTime.Now.AddMonths(-4)));

			mawb.JM_SystemCreateTimeUtc = ZDateTime.MinSmallDateTimeValue;
			Factory.Save();

			AssertEquals(mawb, FreightJobMawbLink.LoadByAirlineAndMawbNo("176", "10000001", DateTime.MinValue));
			AssertEquals(mawb, FreightJobMawbLink.LoadByAirlineAndMawbNo("176", "10000001", DateTime.MaxValue));
			AssertEquals(mawb, FreightJobMawbLink.LoadByAirlineAndMawbNo("176", "10000001", ZDateTime.Invalid));
		}

		public void TestLoadUnallocatedPrintedMawb()
		{
			var mawb = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			Factory.Save();

			var parent = new MAWBAllocationParentForTest(Factory)
			{
				IsAir = true,
				MawbPortOfLoading = "AUSYD",
				IsNeutralMaster = true,
				IsValidForNeutralMaster = true,
			};

			var mawbAllocation = new MAWBAllocation(parent);
			AssertNull("mawb should be null", mawbAllocation.AllocatedMawb);

			parent.MasterBillAirlinePrefix = "176";
			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("10000001", parent.MasterBillMAWB);
			AssertEquals(mawb, mawbAllocation.AllocatedMawb);

			var notesParentPK = ZGuid.NewZGuid();
			var notesParentTableName = "SomeTableName";

			var notesParent = new Mock<IStmNoteParent>();

			notesParent.Setup(m => m.NotesParentPK).Returns(notesParentPK);
			notesParent.Setup(m => m.NotesParentTableName).Returns(notesParentTableName);

			AssertNull(FreightJobMawbLink.LoadUnallocatedPrintedMawb(null));
			AssertNull(FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			mawb.JM_IsPrinted = true;
			AssertNull(FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			FreightJobMawbLink.DoDeallocateMAWB(mawb.PK, parent.PK, notesParent.Object);
			AssertEquals(mawb, FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			mawb.JM_ParentID = ZGuid.NewZGuid();
			mawb.JM_ParentTableCode = "JS";
			AssertNull(FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			mawb.JM_ParentID = ZGuid.Empty;
			mawb.JM_ParentTableCode = ZString.Empty;
			mawb.JM_IsPaper = true;
			AssertNull(FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			mawb.JM_IsPaper = false;
			mawb.JM_OH_AllocatedTo = ZGuid.NewZGuid();
			AssertNull(FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			mawb.JM_OH_AllocatedTo = ZGuid.Empty;
			AssertEquals(mawb, FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			notesParent.VerifyAll();
		}

		[TestDate(2011, 7, 25, 21, 10, 52)]
		public void TestCreatePrintedMawbUnallocationNote()
		{
			var mawb = AddJobMAWB(GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, "176", "10000001", OrgCarrierServiceLevel.StandardCode);
			mawb.JM_ParentID = ZGuid.NewZGuid();
			mawb.JM_ParentTableCode = "JS";
			Factory.Save();

			var notesParentPK = ZGuid.NewZGuid();
			var notesParentTableName = "SomeTableName";

			var notesQuery = new ZQuery(StmNoteSchema.ST_ParentID, notesParentPK);
			notesQuery.AddToFilter(StmNoteSchema.ST_Description, FreightJobMawbLink.PrintedMawbUnallocationInfo);
			notesQuery.AddToFilter(StmNoteSchema.ST_Table, notesParentTableName);

			var notesParent = new Mock<IStmNoteParent>();

			notesParent.Setup(m => m.NotesParentPK).Returns(notesParentPK);
			notesParent.Setup(m => m.NotesParentTableName).Returns(notesParentTableName);

			FreightJobMawbLink.DoDeallocateMAWB(ZGuid.Empty, ZGuid.Empty, null);
			AssertEquals(0, Factory.Load<HiddenStmNote>(notesQuery).Length);

			FreightJobMawbLink.DoDeallocateMAWB(mawb.PK, ZGuid.Empty, null);
			AssertEquals(0, Factory.Load<HiddenStmNote>(notesQuery).Length);

			FreightJobMawbLink.DoDeallocateMAWB(ZGuid.Empty, ZGuid.Empty, notesParent.Object);
			AssertEquals(0, Factory.Load<HiddenStmNote>(notesQuery).Length);

			mawb.JM_IsPrinted = true;
			FreightJobMawbLink.DoDeallocateMAWB(mawb.PK, mawb.JM_ParentID, notesParent.Object);

			var notes = Factory.Load<HiddenStmNote>(notesQuery);
			AssertEquals(1, notes.Length);
			var pk = new ZGuid(notes[0].ST_NoteData.ToAscii());
			AssertEquals(mawb.PK, pk);
			AssertEquals("MAWB 176-10000001 has been unallocated by CargoWise Support on 25-Jul-11 21:10:52", notes[0].ST_NoteText);

			notesParent.VerifyAll();
		}

		#region Implementation

		FreightJobMawbLink FreightJobMawbLink;
		GlbBranch OtherBranch;
		GlbBranch OtherCompanyBranch;

		protected override void SetUp()
		{
			base.SetUp();

			ZQuery query = new ZQuery();
			query.AddToFilter(GlbBranchSchema.GB_GC, GlbBranch.CurrentBranch.GB_GC);
			query.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);

			OtherBranch = Factory.LoadTop1<GlbBranch>(query);
			OtherCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.GB_GC));

			FreightJobMawbLink = new FreightJobMawbLink(Factory);
		}

		JobMawb AddJobMAWB(GlbCompany company, GlbBranch branch, string airlineCode, string mawbNo, string serviceLevel)
		{
			JobMawb mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = airlineCode;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_ServiceLevel = serviceLevel;

			if (company != null)
			{
				mawb.JM_GC_Company = company.PK;
			}

			if (branch != null)
			{
				mawb.JM_GB = branch.PK;
			}

			return mawb;
		}

		#endregion
	}
}
