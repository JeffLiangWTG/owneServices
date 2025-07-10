using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbResourceCapabilityPivot))]
	sealed class GlbResourceCapabilityPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSkillLevel_ShouldTransformByteValue()
		{
			var pivot = Factory.New<GlbResourceCapabilityPivot>();
			pivot.G5_SkillLevel = 1;
			AssertEquals("1", pivot.SkillLevel);

			pivot.SkillLevel = "2";
			AssertEquals((byte)2, pivot.G5_SkillLevel);

			pivot.SkillLevel = "ABC";
			AssertEquals((byte)0, pivot.G5_SkillLevel);
		}

		public void TestSkillLevelDescription()
		{
			var pivot = Factory.New<GlbResourceCapabilityPivot>();
			pivot.G5_SkillLevel = 0;
			AssertEquals("", pivot.SkillLevelDescription);

			pivot.G5_SkillLevel = 1;
			AssertEquals("Achieved", pivot.SkillLevelDescription);

			pivot.G5_SkillLevel = 2;
			AssertEquals("Poor", pivot.SkillLevelDescription);

			pivot.G5_SkillLevel = 3;
			AssertEquals("Average", pivot.SkillLevelDescription);

			pivot.G5_SkillLevel = 4;
			AssertEquals("Excellent", pivot.SkillLevelDescription);

			pivot.G5_SkillLevel = 5;
			AssertEquals("", pivot.SkillLevelDescription);
		}

		public void TestLogging_WithAtcAndDtcEvents()
		{
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "DE", "Davey");
			var capability = MasterFilesTestHelper.CreateCapability(Factory, "BOP", "Boop");
			Factory.Save();
			MasterFilesTestHelper.AssertNoEventRaised(staff, Events.AttachedCode);
			MasterFilesTestHelper.AssertNoEventRaised(capability, Events.AttachedCode);

			var link = Factory.New<GlbResourceCapabilityPivot>();
			link.G5_GS_Resource = staff.PK;
			link.G5_G4_Capability = capability.PK;
			Factory.Save();

			MasterFilesTestHelper.AssertEventRaised(staff, Events.AttachedCode, "|CAP=BOP");
			MasterFilesTestHelper.AssertEventRaised(capability, Events.AttachedCode, "|STF=DE");
			MasterFilesTestHelper.AssertNoEventRaised(staff, Events.DetachedCode);
			MasterFilesTestHelper.AssertNoEventRaised(capability, Events.DetachedCode);

			link.Delete();
			Factory.Save();

			MasterFilesTestHelper.AssertEventRaised(staff, Events.DetachedCode, "|CAP=BOP");
			MasterFilesTestHelper.AssertEventRaised(capability, Events.DetachedCode, "|STF=DE");
		}
	}
}
