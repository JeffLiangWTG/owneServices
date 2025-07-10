using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommunicationsModeSubstitutorTest : TestCaseWithFactory
	{
		[TestDate(2012, 08, 13, 16, 32, 17, 431)]
		public void TestSubstituteIsSafe()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Filename = "TransportBooking_(*JobNumber*)_(*DateTime*).xml";

			CommunicationsModeSubstitutor substitutor = new CommunicationsModeSubstitutor() { JobNumber = "S00001001/E" };

			IEDICommunicationsMode substitutedMode = substitutor.Substitute(mode);
			Assert("original filename is not valid", !MakeFilenameSafe.IsSafe(mode.EK_Filename));
			Assert("substituted filename is valid", MakeFilenameSafe.IsSafe(substitutedMode.EK_Filename));
			AssertEquals("Invalid characters should be replaced by '_'", "TransportBooking_S00001001_E_201208131632174310.xml", substitutedMode.EK_Filename);
		}
	}
}
