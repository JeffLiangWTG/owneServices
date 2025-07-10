using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Business.Testing
{
	sealed class MAWBAllocationTest : BaseFreightTest
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown("parent cannot be null", typeof(ArgumentNullException), () => new MAWBAllocation(null));
			AssertNoExceptionThrown(() => new MAWBAllocation(new MAWBAllocationParentForTest(Factory)));
		}

		public void TestIsLoadMawbFromParentMawbDetailsAllowed()
		{
			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory)
			{
				IsAir = false,
				IsNeutralMaster = false
			};

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);
			AssertEquals(false, mawbAllocation.IsLoadMawbFromParentMawbDetailsAllowed);

			parent.IsNeutralMaster = true;
			AssertEquals(false, mawbAllocation.IsLoadMawbFromParentMawbDetailsAllowed);

			parent.IsAir = true;
			AssertEquals(true, mawbAllocation.IsLoadMawbFromParentMawbDetailsAllowed);
		}

		public void TestLock_LoadMawbFromParentMawbDetails()
		{
			JobMawb mawb = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory)
			{
				MawbPortOfLoading = "AUBNE",
			};

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);
			AssertEquals("prerequisite", true, mawbAllocation.IsLoadMawbFromParentMawbDetailsAllowed);

			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertNull(mawbAllocation.AllocatedMawb);

			parent.MasterBillAirlinePrefix = "123";
			parent.MasterBillMAWB = "10000011";

			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals(mawb, mawbAllocation.AllocatedMawb);

			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals("should not remove allocation", mawb, mawbAllocation.AllocatedMawb);

			parent.MasterBillAirlinePrefix = "081";

			mawbAllocation.MarkForDeallocation();
			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertNull("should deallocate", mawbAllocation.AllocatedMawb);
			AssertEquals("should not clear mawb number on parent", "10000011", parent.MasterBillMAWB);

			parent.MasterBillAirlinePrefix = "123";

			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals("should reallocate", mawb, mawbAllocation.AllocatedMawb);

			parent.IsNeutralMaster = false;

			AssertEquals("prerequisite", false, mawbAllocation.IsLoadMawbFromParentMawbDetailsAllowed);
		}

		public void TestLoadMawbFromParentMawbDetails_MawbWasDeleted()
		{
			JobMawb mawb = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");
			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory);
			parent.MasterBillMAWB = "10000011";
			parent.MasterBillAirlinePrefix = "123";

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);
			AssertNull("Not loaded should be null", mawbAllocation.AllocatedMawb);

			Assert(mawbAllocation.LoadMawbFromParentMawbDetails());
			AssertEquals("Should now be loaded", mawb, mawbAllocation.AllocatedMawb);

			mawb.Delete();

			Assert("MAWB was deleted", !mawbAllocation.LoadMawbFromParentMawbDetails());
			AssertNull("Deleted MAWB was unallocated", mawbAllocation.AllocatedMawb);
		}

		public void TestJobMawb()
		{
			JobMawb mawb1 = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("123", "10000022", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory);
			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);

			AssertEquals("Parent", parent, mawbAllocation.Parent);
			AssertNull("mawb should be null", mawbAllocation.AllocatedMawb);

			mawb1.JM_ParentID = parent.PK;
			mawb1.JM_ParentTableCode = parent.Prefix;

			Factory.Save();

			AssertEquals("JobMawb should have been loaded", mawb1, mawbAllocation.AllocatedMawb);
			AssertEquals("JobMawb should have been loaded", parent.PK, mawbAllocation.AllocatedMawb.JM_ParentID);
			AssertEquals("JobMawb should have been loaded", parent.Prefix, mawbAllocation.AllocatedMawb.JM_ParentTableCode);

			parent.MasterBillMAWB = "10000022";
			parent.MasterBillAirlinePrefix = "123";
			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals("JobMawb should have been loaded", mawb2, mawbAllocation.AllocatedMawb); //Useless test actually
		}

		public void TestJobMawb_UnallocatedPrinted()
		{
			JobMawb mawb1 = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("123", "10000022", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory);
			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);

			AssertNull("mawb should be null", mawbAllocation.AllocatedMawb);

			parent.MasterBillMAWB = "10000011";
			parent.MasterBillAirlinePrefix = "123";

			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals("mawb1 should have been loaded", mawb1, mawbAllocation.AllocatedMawb);
			mawb1.JM_IsPrinted = true;
			Factory.Save();

			parent.MasterBillMAWB = "10000022";
			parent.MasterBillAirlinePrefix = "123";
			mawbAllocation.MarkForDeallocation();
			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals("mawb2 should have been loaded", mawb2, mawbAllocation.AllocatedMawb);
			AssertEquals("mawb1 should have been unloaded", ZGuid.Empty, mawb1.JM_ParentID);
			AssertEquals("mawb1 should have been unloaded", ZString.Empty, mawb1.JM_ParentTableCode);
			AssertEquals("mawb1 marked as not printed", true, mawb1.JM_IsPrinted);

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			JobMawb mawb1OnOtherFactory = otherFactory.Load<JobMawb>(mawb1.PK);

			AssertEquals("mawb1 will not remain as printed - should reset the printed status of a MAWB", true, mawb1.JM_IsPrinted);
			AssertEquals("mawb1 will not remain as printed - should reset the printed status of a MAWB", true, mawb1OnOtherFactory.JM_IsPrinted);
		}

		public void TestCreateUnallocationInfoForPrintedMawb()
		{
			JobMawb mawb1 = AddMawb("081", "00000011", GlbBranch.CurrentBranch, OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb2 = AddMawb("001", "99999999", GlbBranch.CurrentBranch, OrgCarrierServiceLevel.StandardCode);

			Factory.Save();

			var notesParent = new Mock<IStmNoteParent>();

			var parent = new MAWBAllocationParentForTest(Factory)
			{
				IsAir = true,
				MawbPortOfLoading = "AUSYD",
				IsNeutralMaster = true,
				IsValidForNeutralMaster = true,
				NotesParent = notesParent.Object,
			};

			var noteParentPK = ZGuid.NewZGuid();
			notesParent.Setup(m => m.NotesParentPK).Returns(noteParentPK);
			notesParent.Setup(m => m.NotesParentTableName).Returns("SomeTableName");

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);

			parent.MasterBillAirlinePrefix = "081";

			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals(mawb1, mawbAllocation.AllocatedMawb);

			parent.MasterBillAirlinePrefix = "001";

			mawbAllocation.RefreshAllocatedMawbInDatabase();
			mawbAllocation.MarkForReallocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals(mawb2, mawbAllocation.AllocatedMawb);
			AssertEquals(null, mawbAllocation.FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			mawb2.JM_IsPrinted = true;
			Factory.Save();
			parent.MasterBillAirlinePrefix = "081";
			AssertEquals(false, mawbAllocation.IsAllocationOfMawbAllowed);
			mawbAllocation.MarkForDeallocation();
			mawbAllocation.PerformMAWBAllocation();

			AssertEquals(mawb2, mawbAllocation.FreightJobMawbLink.LoadUnallocatedPrintedMawb(notesParent.Object));

			notesParent.VerifyAll();
		}

		public void TestReallocatedPrintedMawb()
		{
			JobMawb mawb1 = AddMawb("081", "00000011", GlbBranch.CurrentBranch, OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb2 = AddMawb("081", "00000022", GlbBranch.CurrentBranch, OrgCarrierServiceLevel.StandardCode);

			Factory.Save();

			var parent = new MAWBAllocationParentForTest(Factory)
			{
				IsAir = true,
				MawbPortOfLoading = HomePort,
				IsNeutralMaster = true,
				IsValidForNeutralMaster = true,
			};

			var mawbAllocation = new MAWBAllocation(parent);

			AssertNull("mawb should be null", mawbAllocation.AllocatedMawb);

			parent.MasterBillAirlinePrefix = "081";

			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("00000011", parent.MasterBillMAWB);
			AssertEquals(mawb1, mawbAllocation.AllocatedMawb);

			var noteParent = new Mock<IStmNoteParent>();

			ZGuid noteParentPK = ZGuid.NewZGuid();
			noteParent.Setup(m => m.NotesParentPK).Returns(noteParentPK);
			noteParent.Setup(m => m.NotesParentTableName).Returns("SomeTableName");
			parent.NotesParent = noteParent.Object;
			mawb2.JM_IsPrinted = true;
			mawbAllocation.FreightJobMawbLink.CreateOrUpdatePrintedMawbUnallocationNote(mawb2, noteParent.Object);

			bool onReallocatingPrintedMawbCalled = false;
			mawbAllocation.OnReallocatingPrintedMawb += (s, e) =>
			{
				onReallocatingPrintedMawbCalled = true;
				AssertEquals(s, mawbAllocation);
				AssertEquals(e.Mawb, mawb2);
				AssertEquals(false, e.Cancel);
			};

			mawbAllocation.MarkForReallocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("00000022", parent.MasterBillMAWB);
			AssertEquals(mawb2, mawbAllocation.AllocatedMawb);

			AssertEquals(true, onReallocatingPrintedMawbCalled);

			noteParent.VerifyAll();
		}

		public void TestLoadTwiceUnallocatedPrintedMAWB()
		{
			JobMawb mawb1 = AddMawb("081", "00000011", GlbBranch.CurrentBranch, OrgCarrierServiceLevel.StandardCode);
			JobMawb mawb2 = AddMawb("081", "00000022", GlbBranch.CurrentBranch, OrgCarrierServiceLevel.StandardCode);

			Factory.Save();

			var parent = new MAWBAllocationParentForTest(Factory)
			{
				IsAir = true,
				MawbPortOfLoading = HomePort,
				IsNeutralMaster = true,
				IsValidForNeutralMaster = true,
			};

			var mawbAllocation = new MAWBAllocation(parent);

			AssertNull("mawb should be null", mawbAllocation.AllocatedMawb);

			parent.MasterBillAirlinePrefix = "081";

			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("00000011", parent.MasterBillMAWB);
			AssertEquals(mawb1, mawbAllocation.AllocatedMawb);

			var noteParent = new Mock<IStmNoteParent>();

			var noteParentPK = ZGuid.NewZGuid();
			noteParent.Setup(m => m.NotesParentPK).Returns(noteParentPK);
			noteParent.Setup(m => m.NotesParentTableName).Returns("SomeTableName");

			parent.NotesParent = noteParent.Object;
			mawb2.JM_IsPrinted = true;
			mawbAllocation.FreightJobMawbLink.CreateOrUpdatePrintedMawbUnallocationNote(mawb2, noteParent.Object);

			bool onReallocatingPrintedMawbCalled = false;
			mawbAllocation.OnReallocatingPrintedMawb += (s, e) =>
			{
				onReallocatingPrintedMawbCalled = true;
				AssertEquals(s, mawbAllocation);
				AssertEquals(e.Mawb, mawb2);
				AssertEquals(false, e.Cancel);
			};

			mawbAllocation.MarkForAllocation();
			Assert(onReallocatingPrintedMawbCalled);
			onReallocatingPrintedMawbCalled = false;
			mawbAllocation.MarkForReallocation();
			Assert("Should call ReallocatingPrintedMawbCalled in MarkForAllocation", onReallocatingPrintedMawbCalled);

			noteParent.VerifyAll();
		}

		public void TestFreightJobMawbLink()
		{
			MAWBAllocation mawbAllocation = new MAWBAllocation(new MAWBAllocationParentForTest(Factory));
			AssertNotNull(mawbAllocation.FreightJobMawbLink);
		}

		public void TestLoadJobMawb()
		{
			JobMawb mawb = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory);
			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);

			AssertNull(mawbAllocation.AllocatedMawb);

			mawb.JM_ParentID = parent.PK;
			mawb.JM_ParentTableCode = parent.Prefix;

			AssertEquals(mawb, mawbAllocation.AllocatedMawb);
		}

		public void TestNeutralReadOnly()
		{
			JobMawb mawb = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory)
			{
				IsValidForNeutralMaster = true,
				MawbPortOfLoading = "AUBNE"
			};

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);

			AssertEquals(false, mawbAllocation.NeutralReadOnly);

			parent.MasterBillMAWB = "10000011";
			parent.MasterBillAirlinePrefix = "123";
			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals(false, mawbAllocation.NeutralReadOnly);

			mawb.JM_IsPrinted = true;

			AssertEquals("Neutral Read Only should be true, mawb is printed", true, mawbAllocation.NeutralReadOnly);

			mawb.JM_IsPrinted = false;
			parent.MawbPortOfLoading = "USLAX";

			AssertEquals("Neutral Read Only should be true, not export", true, mawbAllocation.NeutralReadOnly);

			parent.MawbPortOfLoading = "AUBNE";
			parent.IsValidForNeutralMaster = false;

			AssertEquals("Neutral Read Only should be true, Agent type not direct", true, mawbAllocation.NeutralReadOnly);

			parent.IsValidForNeutralMaster = true;

			AssertEquals("Neutral Read Only should be false", false, mawbAllocation.NeutralReadOnly);
		}

		public void TestIsMAWBPrinted()
		{
			JobMawb mawb = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory);
			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);

			AssertEquals("IsMAWBPrinted should be false", false, mawbAllocation.IsMAWBPrinted);

			parent.MasterBillMAWB = "10000011";
			parent.MasterBillAirlinePrefix = "123";
			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals("IsMAWBPrinted should be false", false, mawbAllocation.IsMAWBPrinted);

			mawb.JM_IsPrinted = true;

			AssertEquals("IsMAWBPrinted should be true, mawb is printed", true, mawbAllocation.IsMAWBPrinted);
		}

		public void TestAllocationOfMAWBAllowed()
		{
			JobMawb mawb = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory)
			{
				IsAir = false,
				MawbPortOfLoading = "USCHI",
				IsNeutralMaster = false,
				IsValidForNeutralMaster = false
			};

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);

			AssertEquals("AllocationOfMAWBAllowed should be false", false, mawbAllocation.IsAllocationOfMawbAllowed);

			parent.IsAir = true;

			AssertEquals("AllocationOfMAWBAllowed should be false", false, mawbAllocation.IsAllocationOfMawbAllowed);

			parent.MawbPortOfLoading = "AUBNE";
			AssertEquals("AllocationOfMAWBAllowed should be false", false, mawbAllocation.IsAllocationOfMawbAllowed);

			parent.IsNeutralMaster = true;
			AssertEquals("AllocationOfMAWBAllowed should be false", false, mawbAllocation.IsAllocationOfMawbAllowed);

			parent.MasterBillAirlinePrefix = "123";
			parent.IsValidForNeutralMaster = true;
			AssertEquals("AllocationOfMAWBAllowed should be true", true, mawbAllocation.IsAllocationOfMawbAllowed);

			parent.MasterBillMAWB = "10000011";
			mawbAllocation.LoadMawbFromParentMawbDetails();

			AssertEquals("AllocationOfMAWBAllowed should be true", true, mawbAllocation.IsAllocationOfMawbAllowed);

			mawb.JM_IsPrinted = true;
			AssertEquals("AllocationOfMAWBAllowed should be false", false, mawbAllocation.IsAllocationOfMawbAllowed);
		}

		public void TestTryAllocateMAWB()
		{
			JobMawb mawb1 = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("123", "10000022", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var parent = new Mock<IMAWBAllocationParent>();

			parent.Setup(m => m.Factory).Returns(Factory);
			parent.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			parent.Setup(m => m.Prefix).Returns("JS");
			parent.Setup(m => m.IsAir).Returns(true);
			parent.Setup(m => m.MawbPortOfLoading).Returns("AUBNE");
			parent.Setup(m => m.IsNeutralMaster).Returns(true);
			parent.Setup(m => m.IsValidForNeutralMaster).Returns(true);
			parent.Setup(m => m.MasterBillAirlinePrefix).Returns("123");
			parent.Setup(m => m.AWBServiceLevel).Returns("STD");
			parent.Object.MasterBillAirlinePrefix = "123";
			parent.Object.MasterBillMAWB = "10000011";

			var mawbAllocation = new MAWBAllocation(parent.Object);

			AssertEquals("AllocationOfMAWBAllowed should be true", true, mawbAllocation.IsAllocationOfMawbAllowed);
			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb1", mawb1, mawbAllocation.AllocatedMawb);

			parent.Setup(m => m.Factory).Returns(Factory);
			parent.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			parent.Setup(m => m.IsAir).Returns(true);
			parent.Setup(m => m.MawbPortOfLoading).Returns("AUBNE");
			parent.Setup(m => m.IsNeutralMaster).Returns(true);
			parent.Setup(m => m.IsValidForNeutralMaster).Returns(true);
			parent.Setup(m => m.MasterBillAirlinePrefix).Returns("123");
			parent.Setup(m => m.MawbBookingReference).Returns("123-10000022");
			parent.Setup(m => m.AWBServiceLevel).Returns("STD");
			parent.Object.MasterBillAirlinePrefix = "123";
			parent.Object.MasterBillMAWB = "10000022";

			AssertEquals("AllocationOfMAWBAllowed should be true", true, mawbAllocation.IsAllocationOfMawbAllowed);
			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb2", mawb2, mawbAllocation.AllocatedMawb);
		}

		public void TestTryAllocateMAWB_ServiceLevelMatch()
		{
			JobMawb mawb1 = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("123", "10000022", GlbBranch.CurrentBranch, "ALL");
			JobMawb mawb3 = AddMawb("081", "10000033", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory)
			{
				MasterBillAirlinePrefix = "123",
				MawbPortOfLoading = "AUBNE"
			};

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);
			AssertEquals("AllocationOfMAWBAllowed should be true", true, mawbAllocation.IsAllocationOfMawbAllowed);

			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb1", mawb1, mawbAllocation.AllocatedMawb);

			parent.AWBServiceLevel = "XYZ";
			mawbAllocation.RefreshAllocatedMawbInDatabase();
			mawbAllocation.MarkForReallocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb2", mawb2, mawbAllocation.AllocatedMawb);

			parent.MasterBillAirlinePrefix = "081";
			mawbAllocation.RefreshAllocatedMawbInDatabase();
			mawbAllocation.MarkForReallocation();
			AssertExceptionThrown(typeof(MAWBAllocationException), () => mawbAllocation.PerformMAWBAllocation());

			parent.AWBServiceLevel = "STD";
			mawbAllocation.RefreshAllocatedMawbInDatabase();
			mawbAllocation.MarkForReallocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb3", mawb3, mawbAllocation.AllocatedMawb);
		}

		public void TestTryAllocateMAWB_BranchMatches()
		{
			JobMawb mawb1 = AddMawb("123", "10000011", OtherCompanyBranch, "STD");
			JobMawb mawb2 = AddMawb("123", "10000022", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory)
			{
				MasterBillAirlinePrefix = "123",
				MawbPortOfLoading = "AUBNE"
			};

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);
			AssertEquals("AllocationOfMAWBAllowed should be true", true, mawbAllocation.IsAllocationOfMawbAllowed);

			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb2", mawb2, mawbAllocation.AllocatedMawb);
		}

		public void TestTryAllocateMAWB_ShouldAllocateMAWBsOrderByCreateDateThenMAWBNumber()
		{
			var mawb1 = AddMawb("123", "99000011", GlbBranch.CurrentBranch, "STD");
			var mawb2 = AddMawb("123", "10000022", GlbBranch.CurrentBranch, "STD");
			var mawb3 = AddMawb("123", "10000023", GlbBranch.CurrentBranch, "STD");
			mawb1.JM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-5);
			mawb2.JM_SystemCreateTimeUtc = ZDateTime.Now;
			mawb3.JM_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.Save();

			var parent = new MAWBAllocationParentForTest(Factory)
			{
				MasterBillAirlinePrefix = "123",
				MawbPortOfLoading = HomePort
			};

			var mawbAllocation = new MAWBAllocation(parent);
			AssertEquals("AllocationOfMAWBAllowed should be true", true, mawbAllocation.IsAllocationOfMawbAllowed);

			mawbAllocation.MarkForAllocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb1", mawb1, mawbAllocation.AllocatedMawb);
			mawb1.JM_ServiceLevel = "XYZ";
			Factory.Save();

			mawbAllocation.MarkForReallocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb2", mawb2, mawbAllocation.AllocatedMawb);
			mawb2.JM_ServiceLevel = "XYZ";
			Factory.Save();

			mawbAllocation.MarkForReallocation();
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals("Mawb is mawb3", mawb3, mawbAllocation.AllocatedMawb);
		}

		public void TestForceAllocate()
		{
			JobMawb mawb1 = AddMawb("001", "10000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("001", "10000022", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory);
			parent.MawbPortOfLoading = "NZAKL";
			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);
			AssertEquals("prerequisite to check that the allocation is actually forced", false, mawbAllocation.IsAllocationOfMawbAllowed);
			AssertEquals(null, mawbAllocation.AllocatedMawb);

			mawbAllocation.SetShouldAllocate(true);
			mawbAllocation.SetNewJobMAWBId(mawb1.PK);
			mawbAllocation.PerformMAWBAllocation();

			AssertEquals(mawb1, mawbAllocation.AllocatedMawb);
			AssertEquals("001", parent.MasterBillAirlinePrefix);
			AssertEquals("10000011", parent.MasterBillMAWB);

			mawbAllocation.RefreshAllocatedMawbInDatabase();
			mawbAllocation.MarkForReallocation();
			mawbAllocation.SetNewJobMAWBId(mawb2.PK);
			mawbAllocation.PerformMAWBAllocation();
			AssertEquals(mawb2, mawbAllocation.AllocatedMawb);
			AssertEquals("001", parent.MasterBillAirlinePrefix);
			AssertEquals("10000022", parent.MasterBillMAWB);
		}

		public void TestLoadFromParentPK_PrioritizeFactoryOverDatabase()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new UniversalObjectFactory(Factory);

			var parentId = ZGuid.NewZGuid();
			ZString prefix = "JS";

			var mawbInDb = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD", factory1);
			mawbInDb.JM_ParentID = parentId;
			mawbInDb.JM_ParentTableCode = prefix;
			factory1.Save();

			var mawbInMemory = factory2.BOFactory.New<JobMawb>();
			mawbInMemory.JM_Airline3DigitPrefix = "123";
			mawbInMemory.JM_MAWB = "10000011";
			mawbInMemory.JM_ParentID = parentId;
			mawbInMemory.JM_ParentTableCode = prefix;

			var freightJobMawbLink = new FreightJobMawbLink(factory2.BOFactory);
			JobMawb loadedMawb = freightJobMawbLink.LoadFromParentPK(prefix, parentId);
			JobMawb loadedMawbdatabase = freightJobMawbLink.LoadBOInTheDBFromParentPK(prefix, parentId);

			AssertEquals("Loaded MAWB should be the unsaved in-memory instance", mawbInMemory.PK, loadedMawb.PK);
			AssertEquals("Loaded MAWB should be the database instance", mawbInDb.PK, loadedMawbdatabase.PK);
		}

		public void TestAllocatedMawbInDatabaseUpdatesAfterSavingMAWB()
		{
			JobMawb mawb = AddMawb("123", "10000011", GlbBranch.CurrentBranch, "STD");

			MAWBAllocationParentForTest parent = new MAWBAllocationParentForTest(Factory)
			{
				MasterBillAirlinePrefix = "123",
				MasterBillMAWB = "10000011",
				MawbPortOfLoading = "AUBNE"
			};
			mawb.JM_ParentID = parent.PK;
			mawb.JM_ParentTableCode = parent.Prefix;

			MAWBAllocation mawbAllocation = new MAWBAllocation(parent);
			AssertNull("AllocatedMawbInDatabase should be null", mawbAllocation.AllocatedMawbInDatabase);

			Factory.Save();
			AssertNotNull("AllocatedMawbInDatabase should not be null after saving", mawbAllocation.AllocatedMawbInDatabase);
			AssertEquals("AllocatedMawbInDatabase should match the saved MAWB", mawb, mawbAllocation.AllocatedMawbInDatabase);
		}

		#region Implementation

		JobMawb AddMawb(string prefix, string mawbNo, GlbBranch branch, string serviceLevel, BusinessObjectFactory factory = null)
		{
			factory = factory ?? Factory;

			JobMawb mawb = factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;

			return mawb;
		}

		GlbBranch OtherCompanyBranch
		{
			get { return otherCompanyBranch ?? (otherCompanyBranch = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew()); }
		}
		GlbBranch otherCompanyBranch;

		#endregion
	}
}
