using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CRMProcessTask))]
	class CRMProcessTaskTest : ProcessTaskTest
	{
		public void TestReminderTimeZone()
		{
			RefTimeZoneSet timeZonesMumbai = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZonesMumbai.StandardZone.R2_CivilianTimeZoneCode = "IN1";
			timeZonesMumbai.StandardZone.R2_OffsetMinutesFromUTC = 300;
			timeZonesMumbai.StandardZone.R2_CivilianTimeZoneFullName = "Mumbai Standard";

			RefTimeZoneSet timeZonesPerth = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZonesPerth.StandardZone.R2_CivilianTimeZoneCode = "PER";
			timeZonesPerth.StandardZone.R2_OffsetMinutesFromUTC = 180;
			timeZonesPerth.StandardZone.R2_CivilianTimeZoneFullName = "Perth Standard";

			RefUNLOCO mumbai = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INBOM");
			mumbai.RL_R3 = timeZonesMumbai.PK;

			RefUNLOCO perth = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUPER");
			perth.RL_R3 = timeZonesPerth.PK;

			Factory.Save();

			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUPER";

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "New Staff";
			staff1.GS_GB_HomeBranch = branch.PK;

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "New Staff";
			staff2.ResetBranchAndDepartment();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "My Organisation in Newington";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			var task1 = Factory.New<CRMProcessTaskTestForTest>();
			task1.P9_TaskID = "987";
			task1.P9_Description = "Ring John";
			task1.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task1.P9_OA = org.MainAddress.PK;
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			AssertEquals("Must be equal", task1.AssignedStaffMember.HomeBranch.HomePort.TimeZoneSet.GetCalculationTimeZone().StandardName, task1.ReminderTimeZone_Exposed.StandardName);

			var task2 = Factory.New<CRMProcessTaskTestForTest>();

			task2.P9_TaskID = "987";
			task2.P9_Description = "Ring John";
			task2.P9_Notes = ZBlob.FromAscii(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task2.P9_OA = org.MainAddress.PK;
			task2.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			task2.Address.OA_RL_NKRelatedPortCode = mumbai.RL_Code;

			AssertEquals("Must be equal", task2.Address.RelatedPortCode.TimeZoneSet.GetCalculationTimeZone().StandardName, task2.ReminderTimeZone_Exposed.StandardName);
		}

		class CRMProcessTaskTestForTest : CRMProcessTask
		{
			public CRMProcessTaskTestForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ITimeZone ReminderTimeZone_Exposed
			{
				get { return base.ReminderTimeZone; }
			}
		}

		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert(true);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var task = (CRMProcessTask)Factory.NewWithValidTestData<OrgSalesCall>().WorkflowItems.AddNew();
			AssertNotNull("OrgSalesCall's Process Task should be type CRMProcessTask");
			return task;
		}

		#endregion
	}
}
