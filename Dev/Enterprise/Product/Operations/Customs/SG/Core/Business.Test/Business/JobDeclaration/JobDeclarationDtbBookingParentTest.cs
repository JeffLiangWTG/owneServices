using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	class JobDeclarationDtbBookingParentTest : BaseJobDeclarationDtbBookingParentTest
	{
		public void TestTansportBookingTransportModeForExport()
		{
			CombineAssertions(() =>
			{
				AssertTransportBookingTransportModeForExport(TransportModeCodeList.Codes.TransportMode_4_Air, Constants.TransportModes.Air);
				AssertTransportBookingTransportModeForExport(TransportModeCodeList.Codes.TransportMode_1_SEA, Constants.TransportModes.Sea);
				AssertTransportBookingTransportModeForExport(TransportModeCodeList.Codes.TransportMode_3_Road, Constants.TransportModes.Road);
				AssertTransportBookingTransportModeForExport(TransportModeCodeList.Codes.TransportMode_2_Rail, Constants.TransportModes.Rail);
				AssertTransportBookingTransportModeForExport(TransportModeCodeList.Codes.TransportMode_5_Mail, Constants.TransportModes.Mail);
				AssertTransportBookingTransportModeForExport("D!3", string.Empty);
				AssertTransportBookingTransportModeForExport(TransportModeCodeList.Codes.TransportMode_7_Pipeline, string.Empty);
			});
		}

		void AssertTransportBookingTransportModeForExport(string transportMode, string expectedMode)
		{
			var declaration = (JobDeclaration)GetNewParent();
			SetExport(declaration);
			declaration.SG_OutwardTransportMode = transportMode;
			AssertEquals($"Converting '{transportMode}' for Export", expectedMode, declaration.TransportBookingTransportMode);
		}

		protected override BaseJobDeclaration GetNewParent()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
