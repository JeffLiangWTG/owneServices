using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(GENRALMessage))]
	sealed class GENRALMessageTest : SARSEDIMessageAbstractTest
	{
		public void TestDefaultValues()
		{
			var testMessage = Factory.New<GENRALMessage>();
			AssertEquals("GEN", testMessage.EM_MessageType);
		}

		public void TestLocalProfileName()
		{
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.AgentCode, "20507309", Core.Constants.CountryCodes.SouthAfrica);
			testAgent.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "WTG", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var testMessage = Factory.New<GENRALMessage>();
			testMessage.EM_MessageOwner = "20507309WTG";
			testMessage.EM_MessageSubType = GenralMessageSubTypeList.Codes.PEN;

			AssertEquals("20507309", testMessage.AgentCode);
			AssertEquals("WTG", testMessage.CustomsDualProfileCode);
			AssertEquals(testAgent.OH_Code, testMessage.LocalProfileName);
		}
	}

	public class GENRALMessageForTest : GENRALMessage
	{
		internal string MessageNumForTesting { get; set; }

		public GENRALMessageForTest(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return MessageNumForTesting ?? "0002";
		}

		public new ZDateTime EM_MessageDateTime
		{
			get { return base.EM_MessageDateTime; }
			set { EM_SystemCreateTimeUtc = value; }
		}
	}
}
