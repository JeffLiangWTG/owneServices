using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupLink))]
	sealed class GlbGroupLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var typeList = new CodeDescriptionPairList();
			typeList.AddPair(MembershipTypeList.Codes.UDF, MembershipTypeList.Descriptions.UDF);
			typeList.AddPair(MembershipTypeList.Codes.STF, MembershipTypeList.Descriptions.STF);
			DataRegistry.Instance.RawRegistry.FindByName("StaffMembershipTypeList").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, typeList);

			var groupLink = Factory.New<GlbGroupLink>();
			AssertEquals("Default staff membership type should be", MembershipTypeList.Codes.UDF, groupLink.GK_MembershipType);

			typeList.RemoveAt(typeList.IndexOfCode(MembershipTypeList.Codes.UDF));
			DataRegistry.Instance.RawRegistry.FindByName("StaffMembershipTypeList").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, typeList);
			groupLink = new BusinessObjectFactory().New<GlbGroupLink>();
			AssertEquals("Default staff membership type should be", MembershipTypeList.Codes.STF, groupLink.GK_MembershipType);

			typeList.RemoveAt(typeList.IndexOfCode(MembershipTypeList.Codes.STF));
			DataRegistry.Instance.RawRegistry.FindByName("StaffMembershipTypeList").SetValue(Guid.Empty, Guid.Empty, Guid.Empty, typeList);
			groupLink = new BusinessObjectFactory().New<GlbGroupLink>();
			AssertEquals("Default staff membership type should be", ZString.Empty, groupLink.GK_MembershipType);
		}

		public void TestLogging_ConcurrentEdit()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Tester";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "PPP";
			group.GG_Desc = "Group PPP";

			Factory.Save();
			AssertEquals(0, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(0, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			staff.GS_City = "Sydney";
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GS = staff.PK;
			link2.GK_GG = group.PK;
			Factory.Save();
			AssertEquals("Attached log is added to group", true, group.Logs.Find(log => log.SL_Reference == "Attached - (TST) Tester").Any());
			AssertEquals("Attached log is added to staff", true, staff.Logs.Find(log => log.SL_Reference == "Attached - (PPP) Group PPP").Any());
			AssertEquals(1, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(2, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			staff.GS_City = "Melbourne";
			link2.Delete();
			Factory.Save();
			AssertEquals("Detached log is added to group", true, group.Logs.Find(log => log.SL_Reference == "Detached - (TST) Tester").Any());
			AssertEquals("Detached log is added to staff", true, staff.Logs.Find(log => log.SL_Reference == "Detached - (PPP) Group PPP").Any());
			AssertEquals(2, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(4, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
		}

		public void TestLogging_ConcurrentEditWithDifferentFactories()
		{
			// To simulate behavior in WI00753781
			var staffFactory = new BusinessObjectFactory();
			var staff = staffFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Tester";
			staffFactory.Save();

			var groupFactory = new BusinessObjectFactory();
			var group = groupFactory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "PPP";
			group.GG_Desc = "Group PPP";
			groupFactory.Save();

			var staffAttachLogReference = "Attached - (PPP) Group PPP";
			var staffDetachLogReference = "Detached - (PPP) Group PPP";
			var groupAttachLogReference = "Attached - (TST) Tester";
			var groupDetachLogReference = "Detached - (TST) Tester";

			AssertEquals(0, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(0, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			var linkInGroup = groupFactory.New<GlbGroupLink>();
			linkInGroup.GK_GS = staff.PK;
			linkInGroup.GK_GG = group.PK;
			groupFactory.Save();
			var linkInStaff = staffFactory.LoadTop1<GlbGroupLink>(new ZQuery(GlbGroupLinkSchema.PK, linkInGroup.PK));

			AssertEquals("Attached log is added to group", 1, group.Logs.Find(log => log.SL_Reference == groupAttachLogReference).Count());
			AssertEquals("Detach log is added to group", 0, group.Logs.Find(log => log.SL_Reference == groupDetachLogReference).Count());
			AssertEquals("Attached log is added to staff", 1, staff.Logs.Find(log => log.SL_Reference == staffAttachLogReference).Count());
			AssertEquals("Detach log is added to staff", 0, staff.Logs.Find(log => log.SL_Reference == staffDetachLogReference).Count());

			linkInGroup.Delete();
			linkInStaff.Delete();
			groupFactory.Save();
			staffFactory.Save();

			AssertEquals("Attached log is added to group", 1, group.Logs.Find(log => log.SL_Reference == groupAttachLogReference).Count());
			AssertEquals("Detach log is added to group", 1, group.Logs.Find(log => log.SL_Reference == groupDetachLogReference).Count());
			AssertEquals("Attached log is added to staff", 1, staff.Logs.Find(log => log.SL_Reference == staffAttachLogReference).Count());
			AssertEquals("Detach log is added to staff", 1, staff.Logs.Find(log => log.SL_Reference == staffDetachLogReference).Count());

			var linkInStaff2 = staffFactory.New<GlbGroupLink>();
			linkInStaff2.GK_GS = staff.PK;
			linkInStaff2.GK_GG = group.PK;
			staffFactory.Save();

			AssertEquals("Attached log is added to staff", 2, staff.Logs.Find(log => log.SL_Reference == staffAttachLogReference).Count());
			AssertEquals("Detached log is added to staff", 1, staff.Logs.Find(log => log.SL_Reference == staffDetachLogReference).Count());
		}

		public void TestLogging_ConcurrentEditWithSavingErrors()
		{
			var staffFactory = new BusinessObjectFactory();
			var staff = staffFactory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Tester";
			staffFactory.Save();

			var groupFactory = new BusinessObjectFactory();
			var group = groupFactory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "PPP";
			group.GG_Desc = "Group PPP";
			groupFactory.Save();

			AssertEquals(0, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(0, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			var linkInGroup = groupFactory.New<GlbGroupLink>();
			linkInGroup.GK_GS = staff.PK;
			linkInGroup.GK_GG = group.PK;
			var linkInStaff = staffFactory.New<GlbGroupLink>();
			linkInStaff.GK_GS = staff.PK;
			linkInStaff.GK_GG = group.PK;

			groupFactory.Save();
			AssertExceptionThrown<ZSaveException>(() => staffFactory.Save());

			var staffAttachLogReference = "Attached - (PPP) Group PPP";
			var staffDetachLogReference = "Detached - (PPP) Group PPP";
			var groupAttachLogReference = "Attached - (TST) Tester";
			var groupDetachLogReference = "Detached - (TST) Tester";

			AssertEquals("Attached log is added to group", 1, group.Logs.Find(log => log.SL_Reference == groupAttachLogReference).Count());
			AssertEquals("Detach log is added to group", 0, group.Logs.Find(log => log.SL_Reference == groupDetachLogReference).Count());
			AssertEquals("Attached log is added to staff", 1, staff.Logs.Find(log => log.SL_Reference == staffAttachLogReference).Count());
			AssertEquals("Detach log is added to staff", 0, staff.Logs.Find(log => log.SL_Reference == staffDetachLogReference).Count());

			linkInStaff.Delete();
			staffFactory.Save();

			AssertEquals("Attached log is added to group", 1, group.Logs.Find(log => log.SL_Reference == groupAttachLogReference).Count());
			AssertEquals("Detach log is added to group", 0, group.Logs.Find(log => log.SL_Reference == groupDetachLogReference).Count());
			AssertEquals("Attached log is added to staff", 1, staff.Logs.Find(log => log.SL_Reference == staffAttachLogReference).Count());
			AssertEquals("Detach log is added to staff", 0, staff.Logs.Find(log => log.SL_Reference == staffDetachLogReference).Count());

			var failStaffFactory = new BusinessObjectFactoryForTestConcurrency();
			var linkInStaff2 = failStaffFactory.LoadTop1<GlbGroupLinkForTestConcurrency>(new ZQuery(GlbGroupLinkSchema.PK, linkInGroup.PK));
			AssertEquals(linkInStaff2.Group.PK, group.PK);
			var staffInMock = failStaffFactory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staff.PK));
			linkInGroup.Delete();
			linkInStaff2.Delete();
			groupFactory.Save();
			AssertExceptionThrown<ZSaveConcurrencyException>(() => failStaffFactory.Save());

			AssertEquals("Attached log is added to group", 1, group.Logs.Find(log => log.SL_Reference == groupAttachLogReference).Count());
			AssertEquals("Detach log is added to group", 1, group.Logs.Find(log => log.SL_Reference == groupDetachLogReference).Count());
			AssertEquals("Attached log is added to staff", 1, staffInMock.Logs.Find(log => log.SL_Reference == staffAttachLogReference).Count());
			AssertEquals("Detach log is added to staff", 1, staffInMock.Logs.Find(log => log.SL_Reference == staffDetachLogReference).Count());
			((IErrorReporter)ExceptionReporter.Instance).Clear();
		}

		public void TestLogging()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Tester";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "PPP";
			group.GG_Desc = "Group PPP";

			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GS = staff.PK;
			link1.GK_GG = group.PK;
			link1.Delete();
			Factory.Save();
			AssertEquals("Attached log is not added to group because it is deleted before save", false, group.Logs.Find(log => log.SL_Reference == "Attached - (TST) Tester").Any());
			AssertEquals("Attached log is not added to staff because it is deleted before save", false, staff.Logs.Find(log => log.SL_Reference == "Attached - (PPP) Group PPP").Any());
			AssertEquals("Detached log is not added to group because it is deleted before save", false, group.Logs.Find(log => log.SL_Reference == "Detached - (TST) Tester").Any());
			AssertEquals("Detached log is not added to staff because it is deleted before save", false, staff.Logs.Find(log => log.SL_Reference == "Detached - (PPP) Group PPP").Any());
			AssertEquals(0, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(0, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GS = staff.PK;
			link2.GK_GG = group.PK;

			Factory.Save();
			AssertEquals("Attached log is added to group", true, group.Logs.Find(log => log.SL_Reference == "Attached - (TST) Tester").Any());
			AssertEquals("Attached log is added to staff", true, staff.Logs.Find(log => log.SL_Reference == "Attached - (PPP) Group PPP").Any());
			AssertEquals(1, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(1, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			link2.Delete();
			Factory.Save();
			AssertEquals("Detached log is added to group", true, group.Logs.Find(log => log.SL_Reference == "Detached - (TST) Tester").Any());
			AssertEquals("Detached log is added to staff", true, staff.Logs.Find(log => log.SL_Reference == "Detached - (PPP) Group PPP").Any());
			AssertEquals(2, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(2, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			var link3 = Factory.New<GlbGroupLink>();
			link3.GK_GS = staff.PK;
			link3.GK_GG = group.PK;
			Factory.Save();
			AssertEquals("Attached log count", 2, group.Logs.Find(log => log.SL_Reference == "Attached - (TST) Tester").Count());
			AssertEquals("Attached log count", 2, staff.Logs.Find(log => log.SL_Reference == "Attached - (PPP) Group PPP").Count());
			AssertEquals("Detached log count", 1, group.Logs.Find(log => log.SL_Reference == "Detached - (TST) Tester").Count());
			AssertEquals("Detached log count", 1, staff.Logs.Find(log => log.SL_Reference == "Detached - (PPP) Group PPP").Count());
			AssertEquals(3, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(3, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			link3.Delete();
			var link4 = Factory.New<GlbGroupLink>();
			link4.GK_GS = staff.PK;
			link4.GK_GG = group.PK;
			link4.Delete();
			var link5 = Factory.New<GlbGroupLink>();
			link5.GK_GS = staff.PK;
			link5.GK_GG = group.PK;
			Factory.Save();
			AssertEquals("Attached log count should be unchanged", 2, group.Logs.Find(log => log.SL_Reference == "Attached - (TST) Tester").Count());
			AssertEquals("Attached log count should be unchanged", 2, staff.Logs.Find(log => log.SL_Reference == "Attached - (PPP) Group PPP").Count());
			AssertEquals("Detached log count should be unchanged", 1, group.Logs.Find(log => log.SL_Reference == "Detached - (TST) Tester").Count());
			AssertEquals("Detached log count should be unchanged", 1, staff.Logs.Find(log => log.SL_Reference == "Detached - (PPP) Group PPP").Count());
			AssertEquals(3, group.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());
			AssertEquals(3, staff.Logs.Find(log => log.SL_SE_NKEvent == "EDT").Count());

			var allGroup = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, GlbGroup.AllStaffGroupCode);
			var linkAllGroup = Factory.New<GlbGroupLink>();
			linkAllGroup.GK_GS = staff.PK;
			linkAllGroup.GK_GG = allGroup.PK;
			AssertEquals("Attached log is not added to group because it is all staff group", false, allGroup.Logs.Find(log => log.SL_Reference == "Attached - (TST) Tester").Any());
			AssertEquals("Attached log is not added to staff because it is all staff group", false, staff.Logs.Find(log => log.SL_Reference == "Attached - (ALL) " + allGroup.GG_Desc).Any());

			linkAllGroup.Delete();

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TS2";
			staff2.GS_FullName = "Tester 2";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "SSS";
			group2.GG_Desc = "Group SSS";

			Factory.Save();

			staff2.GS_City = "Sydney";
			var link6 = Factory.New<GlbGroupLink>();
			link6.GK_GS = staff2.PK;
			link6.GK_GG = group2.PK;

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedStaff2 = anotherFactory.Load<GlbStaff>(staff2.PK);
			loadedStaff2.GS_City = "Melbourne";
			anotherFactory.Save();

			try
			{
				Factory.Save();
			}
			catch { }

			AssertEquals("Save should fail because of concurrency error and link is not in database", false, link6.IsInDatabase);
			AssertEquals("Attached log is not added to group if save fails", false, group2.Logs.Find(log => log.SL_Reference == "Attached - (TS2) Tester 2").Any());
			AssertEquals("Attached log is not added to staff if save fails", false, staff2.Logs.Find(log => log.SL_Reference == "Attached - (SSS) Group SSS").Any());
			AssertEquals("Detached log is not added to group if save fails", false, group2.Logs.Find(log => log.SL_Reference == "Detached - (TS2) Tester 2").Any());
			AssertEquals("Detached log is not added to staff if save fails", false, staff2.Logs.Find(log => log.SL_Reference == "Detached - (SSS) Group SSS").Any());

			staff2.Reload();
			Factory.Save();
			AssertEquals("Attached log is added to group", true, group2.Logs.Find(log => log.SL_Reference == "Attached - (TS2) Tester 2").Any());
			AssertEquals("Attached log is added to staff", true, staff2.Logs.Find(log => log.SL_Reference == "Attached - (SSS) Group SSS").Any());
			AssertEquals("Detached log should not be added", false, group2.Logs.Find(log => log.SL_Reference == "Detached - (TS2) Tester 2").Any());
			AssertEquals("Detached log should not be added", false, staff2.Logs.Find(log => log.SL_Reference == "Detached - (SSS) Group SSS").Any());
		}

		public void TestLogging_WithAtcAndDtcEvents()
		{
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "DE", "Davey");
			var group = MasterFilesTestHelper.CreateGroup(Factory, "BOOP");
			Factory.Save();
			MasterFilesTestHelper.AssertNoEventRaised(staff, Events.AttachedCode);
			MasterFilesTestHelper.AssertNoEventRaised(group, Events.AttachedCode);

			var link = Factory.New<GlbGroupLink>();
			link.GK_GS = staff.PK;
			link.GK_GG = group.PK;
			Factory.Save();

			MasterFilesTestHelper.AssertEventRaised(staff, Events.AttachedCode, "|GRP=BOOP");
			MasterFilesTestHelper.AssertEventRaised(group, Events.AttachedCode, "|STF=DE");
			MasterFilesTestHelper.AssertNoEventRaised(staff, Events.DetachedCode);
			MasterFilesTestHelper.AssertNoEventRaised(group, Events.DetachedCode);

			link.Delete();
			Factory.Save();

			MasterFilesTestHelper.AssertEventRaised(staff, Events.DetachedCode, "|GRP=BOOP");
			MasterFilesTestHelper.AssertEventRaised(group, Events.DetachedCode, "|STF=DE");
		}

		public void TestOnSavingSalesTeam()
		{
			var groupLink = Factory.New<GlbGroupLink>();
			var staff = Factory.New<GlbStaff>();
			var group = Factory.New<GlbGroup>();

			staff.GS_IsActive = false;
			staff.GS_IsSalesRep = true;
			group.GG_IsSales = true;
			groupLink.GK_GS = staff.PK;
			groupLink.GK_GG = group.PK;
			Factory.Save();

			Assert("The groupLink should NOT be deleted since the group is sales", !groupLink.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedLink = newFactory.Load<GlbGroupLink>(groupLink.PK);
			var reloadedGroup = newFactory.Load<GlbGroup>(group.PK);
			reloadedGroup.GG_IsSales = false;
			reloadedLink.GK_MembershipType = MembershipTypeList.Codes.STF;
			newFactory.Save();

			Assert("The groupLink should be deleted since the staff is inactive and the group is non-sales", reloadedLink.IsDeleted);
		}

		[TestDate]
		public void TestGroupMemberChangeSignalsGroupNeedToBeADSynced()
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_FullName = "Tester";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "PPP";
			group.GG_Desc = "Group PPP";
			Factory.Save();

			// Add group from staff
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			staff.Groups.Add(group);
			Factory.Save();
			var expectedLastEditedDate = TestDateAttribute.Date;
			Assertion.AssertEquals("Added", expectedLastEditedDate, group.GG_SystemLastEditTimeUtc);

			// Remove group from staff
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			staff.Groups.Remove(group);
			Factory.Save();
			expectedLastEditedDate = TestDateAttribute.Date;
			Assertion.AssertEquals("Removed", expectedLastEditedDate, group.GG_SystemLastEditTimeUtc);

			// Add staff from group
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			group.Staff.Add(staff);
			Factory.Save();
			expectedLastEditedDate = TestDateAttribute.Date;
			Assertion.AssertEquals("Added", expectedLastEditedDate, group.GG_SystemLastEditTimeUtc);

			// Remove staff from group
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			group.Staff.Remove(staff);
			Factory.Save();
			expectedLastEditedDate = TestDateAttribute.Date;
			Assertion.AssertEquals("Added", expectedLastEditedDate, group.GG_SystemLastEditTimeUtc);
		}

		public void TestAttachedLogs_WhenUniversalCopyStaff_ShouldBeRecorded()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "GP1";
			group1.GG_Desc = "Group1";
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "GP2";
			group2.GG_Desc = "Group2";

			var glbGroupLink1 = Factory.New<GlbGroupLink>();
			glbGroupLink1.GK_GG = group1.PK;
			glbGroupLink1.GK_GS = staff.PK;

			var glbGroupLink2 = Factory.New<GlbGroupLink>();
			glbGroupLink2.GK_GG = group2.PK;
			glbGroupLink2.GK_GS = staff.PK;

			Factory.Save();

			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms))
			{
				var templateXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<CopyTemplateTree xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" N=""GlbStaff"" ConfigurationSource=""SLT""
  NominatedRecordPk=""00000000-0000-0000-0000-000000000000"" TableName=""GlbStaff"" Active=""true"">
  <E N=""GlbStaff"">
    <C N=""GlbGroupLinks"" ItemPropertyName=""GK_GS"" ItemTableName=""GlbGroupLink"" Do=""All""
      IsSplitCollection=""false"" Order=""0"">
      <E N=""GlbGroupLink"">
        <P N=""GK_MembershipType"" Do=""Copy"" />
        <P N=""GK_SkillLevel"" Do=""Copy"" />
        <R N=""GlbGroup"" RelatedPropertyName=""GK_GG"" RelatedEntityTableName=""GlbGroup""
          Do=""LinkCopied"">
          <E N=""GlbGroup"">
            <P N=""GG_ActiveDirectoryObjectGuid"" Do=""Copy"" />
            <P N=""GG_Code"" Do=""Copy"" />
            <P N=""GG_Desc"" Do=""Copy"" />
            <P N=""GG_IsSales"" Do=""Copy"" />
          </E>
        </R>
      </E>
    </C>
  </E>
</CopyTemplateTree>";
				sw.Write(templateXML, new UnicodeEncoding());
				sw.Flush();
				ms.Seek(0, SeekOrigin.Begin);

				var copyTemplate = CopyTemplateTree.Deserialize(ms);
				var copyManager = new BusinessObjectCopyManager();
				var copyStaff = (GlbStaff)copyManager.Copy(staff, copyTemplate).Object;

				Factory.Save();

				AssertNotNull("Attached log", copyStaff.Logs.GetAllLogs().ToList<StmALog>().Find(log => log.SL_Reference == "Attached - (GP1) Group1"));
				AssertNotNull("Attached log", copyStaff.Logs.GetAllLogs().ToList<StmALog>().Find(log => log.SL_Reference == "Attached - (GP2) Group2"));
			}
		}

		class GlbGroupLinkForTestConcurrency : GlbGroupLink
		{
			public GlbGroupLinkForTestConcurrency(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override bool IsInDatabase => true;
		}

		class BusinessObjectFactoryForTestConcurrency : BusinessObjectFactory
		{
			protected override IChangedTableNames SaveInTransactionCore()
			{
				base.SaveInTransactionCore();
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(null, null, null), this);
			}
		}
	}
}
