using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageApplicationAgent))]
	sealed class LicensingMessageApplicationAgentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var org = new TestTWCreator(Factory).CreateOrganization();
			declaration.JE_OA_DeclarantAddress = org.MainAddress.PK;
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var agent = new LicensingMessageApplicationAgent(declaration.DeclarantAddress);
			var communication = agent.Communications.Single();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(agent.Name, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison), "Agent.Name is not expected");
				NUnit.Framework.Assert.That(agent.Address.ChineseLine, NUnit.Framework.Is.EqualTo("90093台灣臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "Agent.Address.ChineseLine is not expected");
				NUnit.Framework.Assert.That(communication.ID, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison), "Agent.Communication.ID");
				NUnit.Framework.Assert.That(communication.TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison), "Agent.Communication.TypeID");
				NUnit.Framework.Assert.That(agent.ContactName, NUnit.Framework.Is.EqualTo("Contact1").Using(CustomComparers.TypeComparison), "Agent.ContactName is not expected");

				org.Contacts.RemoveAndDeleteAll();
				agent = new LicensingMessageApplicationAgent(declaration.DeclarantAddress);
				NUnit.Framework.Assert.That(agent.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty), "Agent.ContactName should be empty when not have any contact");

				org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS001", Core.Constants.CountryCodes.Taiwan);
				agent = new LicensingMessageApplicationAgent(declaration.DeclarantAddress);
				NUnit.Framework.Assert.That(agent.ID, NUnit.Framework.Is.EqualTo("PAS001").Using(CustomComparers.TypeComparison), "Agent.ID[PAS] is not expected");
				NUnit.Framework.Assert.That(agent.TypeCode, NUnit.Framework.Is.EqualTo("53").Using(CustomComparers.TypeComparison), "Agent.TypeCode[PAS] is not expected");

				org.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID001", Core.Constants.CountryCodes.Taiwan);
				agent = new LicensingMessageApplicationAgent(declaration.DeclarantAddress);
				NUnit.Framework.Assert.That(agent.ID, NUnit.Framework.Is.EqualTo("PID001").Using(CustomComparers.TypeComparison), "Agent.ID[PID] is not expected");
				NUnit.Framework.Assert.That(agent.TypeCode, NUnit.Framework.Is.EqualTo("174").Using(CustomComparers.TypeComparison), "Agent.TypeCode[PID] is not expected");

				org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT001", Core.Constants.CountryCodes.Taiwan);
				agent = new LicensingMessageApplicationAgent(declaration.DeclarantAddress);
				NUnit.Framework.Assert.That(agent.ID, NUnit.Framework.Is.EqualTo("VAT001").Using(CustomComparers.TypeComparison), "Agent.ID[VAT] is not expected");
				NUnit.Framework.Assert.That(agent.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "Agent.TypeCode[VAT] is not expected");
			});
		}
	}
}
