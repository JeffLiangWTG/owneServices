using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AsAgreedTests : TransactionedTestCase
	{
		public void TestAsAgreedDefaulted()
		{
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB = Constants.AWB.AsAgreedTypes.Codes.Collect;
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB = Constants.AWB.AsAgreedTypes.Codes.Prepaid;
			var factory = new BusinessObjectFactory();
			var consol1 = factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Expected JK_MBLAWBChargesDisplay to be CPD", ChargesApplyHelper.ChargesApplyConstants.CPD, consol1.JK_MBLAWBChargesDisplay);

			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB = Constants.AWB.AsAgreedTypes.Codes.None;
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB = Constants.AWB.AsAgreedTypes.Codes.All;
			var consol2 = factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Expected JK_MBLAWBChargesDisplay to be NAL", ChargesApplyHelper.ChargesApplyConstants.NAL, consol2.JK_MBLAWBChargesDisplay);

			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnFirstSetMAWB = Constants.AWB.AsAgreedTypes.Codes.All;
			Env.Registry.Freight.AirWaybill.PrintAsAgreedOnSecondSetMAWB = Constants.AWB.AsAgreedTypes.Codes.None;
			AssertEquals("Expected existing consol to be not changed", ChargesApplyHelper.ChargesApplyConstants.NAL, consol2.JK_MBLAWBChargesDisplay);
		}
	}
}
