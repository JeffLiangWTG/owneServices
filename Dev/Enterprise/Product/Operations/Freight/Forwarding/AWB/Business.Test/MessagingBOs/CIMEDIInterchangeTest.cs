using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(CIMEDIInterchange))]
	sealed class CIMEDIInterchangeTest : EnterpriseBusinessObjectTestCaseWithListChecking<CIMEDIInterchange>
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestSetDefaultValues()
		{
			CIMEDIInterchange interchange = Factory.New<CIMEDIInterchange>();
			AssertEquals(ZBool.True, interchange.EI_IsActive);
			AssertEquals(EDIInterchange.ApplicationCodes.CIM, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchange.ApplicationCodes.CIM, interchange.EI_InterchangeType);
			AssertEquals(((char)4).ToString(), interchange.EI_FooterText);
			AssertEquals(GlbBranch.CurrentBranch.PK, interchange.EI_GB);
		}

		public void TestGetERouterToCode()
		{
			AssertEquals("EDI CCN", CIMEDIInterchange.GetERouterToCode());
		}
	}
}
