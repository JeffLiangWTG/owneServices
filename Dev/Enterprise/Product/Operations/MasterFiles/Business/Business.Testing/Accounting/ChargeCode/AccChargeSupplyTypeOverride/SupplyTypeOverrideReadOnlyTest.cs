namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SupplyTypeOverrideReadOnlyTest : SupplyTypeConfigurationReadOnlyTest
	{
		public new void TestDirectionReadOnly()
		{
			BizObj.DirectionCode = "EXP";
			BizObj.JobType = "BRK";
			AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "ALL", BizObj.DirectionCode);

			BizObj.DirectionCode = "EXP";
			BizObj.JobType = "SHP";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "EXP", BizObj.DirectionCode);

			BizObj.JobType = "CLL";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "EXP", BizObj.DirectionCode);

			BizObj.JobType = "CSH";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "EXP", BizObj.DirectionCode);

			BizObj.JobType = "FCN";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "EXP", BizObj.DirectionCode);

			BizObj.JobType = "GCN";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "EXP", BizObj.DirectionCode);

			BizObj.JobType = "AGS";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "EXP", BizObj.DirectionCode);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				BizObj.JobType = "!@#";
				AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
				AssertEquals("DirectionInfo.IsEmpty", "ALL", BizObj.DirectionCode);
			}

			BizObj.DirectionCode = "EXP";
			BizObj.JobType = "";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "EXP", BizObj.DirectionCode);

			BizObj.JobType = "AWB";
			AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", "ALL", BizObj.DirectionCode);
		}

		public new void TestModeReadOnly()
		{
			BizObj.Mode = "AIR";
			BizObj.JobType = "BRK";
			AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", "ALL", BizObj.Mode);

			BizObj.Mode = "AIR";
			BizObj.JobType = "SHP";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", "AIR", BizObj.Mode);

			BizObj.JobType = "CLL";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", "AIR", BizObj.Mode);

			BizObj.JobType = "CSH";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", "AIR", BizObj.Mode);

			BizObj.JobType = "FCN";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", "AIR", BizObj.Mode);

			BizObj.JobType = "GCN";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", "AIR", BizObj.Mode);

			BizObj.JobType = "";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", "AIR", BizObj.Mode);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				BizObj.JobType = "!@#";
				AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
				AssertEquals("ModeInfo.IsEmpty", "ALL", BizObj.Mode);
			}

			BizObj.Mode = "AIR";
			BizObj.JobType = "AWB";
			AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", "ALL", BizObj.Mode);
		}

		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return Factory.NewWithValidTestData<AccChargeSupplyTypeOverride>();
			}
		}
	}
}
