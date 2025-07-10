using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Business.Utilities.Testing
{
	sealed class TDTSegmentSplitHelperTest : TestCaseWithFactory
	{
		public void TestIsTDTSegementSplitApplicable()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				Assert(TDTSegmentSplitHelper.IsTDTSegementSplitApplicable(dec));
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				Assert(!TDTSegmentSplitHelper.IsTDTSegementSplitApplicable(dec));
			}

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				Assert(TDTSegmentSplitHelper.IsTDTSegementSplitApplicable(dec));
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				Assert(!TDTSegmentSplitHelper.IsTDTSegementSplitApplicable(dec));
			}
		}
	}
}
