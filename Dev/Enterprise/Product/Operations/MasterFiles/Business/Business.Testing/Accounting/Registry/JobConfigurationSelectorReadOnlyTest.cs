using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class JobConfigurationSelectorReadOnlyTest : TestCaseWithFactory
	{
		public void TestDirectionReadOnly()
		{
			BizObj.DirectionCode = "ALL";
			BizObj.JobType = "BRK";
			AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", true, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "SHP";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "CLL";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "CSH";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "FCN";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "GCN";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "AGS";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", false, BizObj.DirectionCode.IsEmpty);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				BizObj.JobType = "!@#";
				AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
				AssertEquals("DirectionInfo.IsEmpty", true, BizObj.DirectionCode.IsEmpty);
			}

			BizObj.JobType = "";
			AssertEquals("DirectionInfo.ReadOnly", false, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", false, BizObj.DirectionCode.IsEmpty);

			BizObj.JobType = "AWB";
			AssertEquals("DirectionInfo.ReadOnly", true, BizObj.DirectionCodeInfo.ReadOnly);
			AssertEquals("DirectionInfo.IsEmpty", true, BizObj.DirectionCode.IsEmpty);
		}

		public void TestModeReadOnly()
		{
			BizObj.Mode = "AIR";
			BizObj.JobType = "BRK";
			AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", true, BizObj.Mode.IsEmpty);

			BizObj.JobType = "QSH";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "SHP";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "CLL";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "CSH";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "FCN";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "GCN";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", false, BizObj.Mode.IsEmpty);

			BizObj.JobType = "";
			AssertEquals("ModeInfo.ReadOnly", false, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", false, BizObj.Mode.IsEmpty);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				BizObj.JobType = "!@#";
				AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
				AssertEquals("ModeInfo.IsEmpty", true, BizObj.Mode.IsEmpty);
			}

			BizObj.JobType = "AWB";
			AssertEquals("ModeInfo.ReadOnly", true, BizObj.ModeInfo.ReadOnly);
			AssertEquals("ModeInfo.IsEmpty", true, BizObj.Mode.IsEmpty);
		}

		#region Implementation

		protected abstract IJobConfigurationSelector GetNewBizObj { get; }

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBizObj;
		}

		protected IJobConfigurationSelector BizObj { get; set; }

		#endregion
	}
}
