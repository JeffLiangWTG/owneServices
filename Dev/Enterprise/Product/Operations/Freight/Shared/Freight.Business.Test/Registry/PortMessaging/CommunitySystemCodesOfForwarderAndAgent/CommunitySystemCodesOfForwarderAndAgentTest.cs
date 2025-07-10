using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CommunitySystemCodesOfForwarderAndAgent))]
	sealed class CommunitySystemCodesOfForwarderAndAgentTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidatePort()
		{
			BizObj.RunPreSaveValidation();
			BizObj.Port = "";
			AssertHasErrors(BizObj.PortInfo);
			AssertHasErrorContaining(BizObj.PortInfo, "Port Code is required.");

			BizObj.Port = "XX";
			AssertHasErrors(BizObj.PortInfo);
			AssertHasErrorContaining(BizObj.PortInfo, "Enter a valid selection.");

			BizObj.Port = "AUSYD";
			AssertHasErrors(BizObj.PortInfo);
			AssertHasErrorContaining(BizObj.PortInfo, "Please select a port in France or Territories.");

			BizObj.Port = "FRPAR";
			AssertNoErrors(BizObj.PortInfo);
		}

		public void TestValidatePCS()
		{
			var locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_RL_NKLocoPort = "FRPAS";
			locoMap.RY_SystemUsage = "PCS";
			locoMap.RY_LocalPortCode = "MGI";
			Factory.Save();

			Assert(BizObj.PCSInfo.ReadOnly);

			BizObj.RunPreSaveValidation();
			BizObj.Port = "AUSYD";
			AssertEquals("", BizObj.PCS);
			AssertHasErrors(BizObj.PCSInfo);
			AssertHasErrorContaining(BizObj.PCSInfo, "PCS Code is required (UNLOCO > Usage Code PCS).");

			BizObj.Port = "FRPAS";
			AssertEquals("MGI", BizObj.PCS);
			AssertNoErrors(BizObj.PCSInfo);
		}

		public void TestValidateForwarderCode()
		{
			BizObj.RunPreSaveValidation();
			BizObj.ForwarderCode = "";
			AssertHasErrors(BizObj.ForwarderCodeInfo);
			AssertHasErrorContaining(BizObj.ForwarderCodeInfo, "Forwarder Code is required.");

			BizObj.ForwarderCode = "F001";
			AssertNoErrors(BizObj.ForwarderCodeInfo);
		}

		public void TestValidateAgentCode()
		{
			BizObj.RunPreSaveValidation();
			BizObj.AgentCode = "";
			AssertHasErrors(BizObj.AgentCodeInfo);
			AssertHasErrorContaining(BizObj.AgentCodeInfo, "Agent Code is required.");

			BizObj.AgentCode = "A001";
			AssertNoErrors(BizObj.AgentCodeInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new CommunitySystemCodesOfForwarderAndAgent();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new CommunitySystemCodesOfForwarderAndAgent BizObj
		{
			get { return (CommunitySystemCodesOfForwarderAndAgent)base.BizObj; }
		}

		#endregion
	}
}
