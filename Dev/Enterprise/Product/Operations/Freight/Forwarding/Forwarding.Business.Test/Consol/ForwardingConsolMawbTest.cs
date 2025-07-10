using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.Test.Consol
{
	internal class ForwardingConsolMawbTest : BaseFreightTest
	{
		public void TestOneConsolChangeMawbPrefix()
		{
			var mawb1 = Factory.New<JobMawb>();
			mawb1.JM_Airline3DigitPrefix = "001";
			mawb1.JM_MAWB = "11111111";
			mawb1.JM_GB = GlbBranch.CurrentBranch.PK;
			var mawb2 = Factory.New<JobMawb>();
			mawb2.JM_Airline3DigitPrefix = "002";
			mawb2.JM_MAWB = "11111111";
			mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var consolA = Factory.New<ForwardingConsol>();
			consolA.JK_TransportMode = Constants.TransportModes.Air;
			consolA.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			consolA.JK_RL_NKDischargePort = "SGSIN";

			consolA.MasterBillAirlinePrefix = mawb1.JM_Airline3DigitPrefix;
			Factory.Save();
			AssertMawbAllocateToConsol(mawb1, consolA);
			AssertMawbAllocateToConsol(mawb2, null);

			consolA.MasterBillAirlinePrefix = mawb2.JM_Airline3DigitPrefix;
			Factory.Save();
			AssertMawbAllocateToConsol(mawb1, null);
			AssertMawbAllocateToConsol(mawb2, consolA);
		}

		public void TestTwoConsolsSwitchMawbPrefixV2()
		{
			var mawbFactory = new BusinessObjectFactory();
			var mawb1 = mawbFactory.New<JobMawb>();
			mawb1.JM_Airline3DigitPrefix = "001";
			mawb1.JM_MAWB = "11111111";
			mawb1.JM_GB = GlbBranch.CurrentBranch.PK;

			var mawb2 = mawbFactory.New<JobMawb>();
			mawb2.JM_Airline3DigitPrefix = "002";
			mawb2.JM_MAWB = "11111111";
			mawb2.JM_GB = GlbBranch.CurrentBranch.PK;
			mawbFactory.Save();

			var consolFactory = new BusinessObjectFactory();
			var consolA = consolFactory.New<ForwardingConsol>();
			consolA.JK_TransportMode = Constants.TransportModes.Air;
			consolA.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			consolA.JK_RL_NKDischargePort = "SGSIN";
			consolA.MasterBillAirlinePrefix = mawb1.JM_Airline3DigitPrefix;

			var consolB = consolFactory.New<ForwardingConsol>();
			consolB.JK_TransportMode = Constants.TransportModes.Air;
			consolB.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.HomePort.RL_Code;
			consolB.JK_RL_NKDischargePort = "SGSIN";
			consolB.MasterBillAirlinePrefix = mawb2.JM_Airline3DigitPrefix;
			consolFactory.Save();

			AssertMawbAllocateToConsol(mawb1, consolA);
			AssertMawbAllocateToConsol(mawb2, consolB);

			consolA.MasterBillAirlinePrefix = mawb2.JM_Airline3DigitPrefix;
			var ex = AssertExceptionThrown<MAWBAllocationException>("Should throw no mawb stock exception", () => consolFactory.Save());

			AssertEquals(ex.Message, "No Master Bill Numbers in stock to allocate to this job. MAWB stock can be added via Maintain > Reference Files > MAWB Stock.");
			consolA.Reload();

			consolB.MasterBillAirlinePrefix = string.Empty;
			consolFactory.Save();
			AssertMawbAllocateToConsol(mawb2, null);

			consolA.MasterBillAirlinePrefix = mawb2.JM_Airline3DigitPrefix;
			consolFactory.Save();
			AssertMawbAllocateToConsol(mawb2, consolA);
			AssertMawbAllocateToConsol(mawb1, null);

			consolB.MasterBillAirlinePrefix = mawb1.JM_Airline3DigitPrefix;
			consolFactory.Save();
			AssertMawbAllocateToConsol(mawb1, consolB);
		}

		static void AssertMawbAllocateToConsol(JobMawb mawb, ForwardingConsol consol)
		{
			mawb.Reload();
			if (consol != null)
			{
				consol.Reload();
				AssertEquals($"{nameof(consol)} should be allocated with {nameof(mawb)}", mawb.JM_Airline3DigitPrefix + mawb.JM_MAWB, consol.JK_MasterBillNum);
				AssertEquals(consol.PK, mawb.JM_ParentID);
			}
			else
			{
				AssertEquals($"{nameof(mawb)} should be deallocated", ZGuid.Empty, mawb.JM_ParentID);
			}
		}
	}
}
