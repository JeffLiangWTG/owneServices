using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal class InterchangeSenderTest : TestCaseWithFactory
	{
		public void TestCreateInstance()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new InterchangeSender(null));
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_From = "codeX";
			ICMMOrganisationData cmmOrgData = new InterchangeSender(interchange);
			AssertEquals("codeX", cmmOrgData.Code);
			AssertEquals(CMMOrganisationType.MutuallyDefined, cmmOrgData.CodeType);
		}
	}
}
