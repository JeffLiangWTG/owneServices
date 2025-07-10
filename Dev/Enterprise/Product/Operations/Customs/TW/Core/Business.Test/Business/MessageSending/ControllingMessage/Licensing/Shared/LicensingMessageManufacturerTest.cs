using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageManufacturer))]
	sealed class LicensingMessageManufacturerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var org = new TestTWCreator(Factory).CreateOrganization();
			var localProcessorAddress = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew().LocalProcessorAddress;
			localProcessorAddress.E2_OA_Address = org.MainAddress.PK;
			localProcessorAddress.E2_AddressOverride = true;
			localProcessorAddress.IDCode = "96944490";
			localProcessorAddress.E2_Contact = "Contact1";
			IPartyDetails localProcessorAddressWrapper = new LicensingMessageManufacturer(localProcessorAddress);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.ID, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison), "ID");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Name, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.").Using(CustomComparers.TypeComparison), "Name");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.ChineseName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison), "ChineseName");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Address.CountrySubDivisionID, NUnit.Framework.Is.EqualTo("TPE").Using(CustomComparers.TypeComparison), "CountrySubDivisionID");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Address.CountrySubDivisionName, NUnit.Framework.Is.EqualTo("TAIPEI").Using(CustomComparers.TypeComparison), "CountrySubDivisionName");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Address.Line, NUnit.Framework.Is.EqualTo("1500 HAPPY RD ORANGE DISTRICT APPLE CITY 12345 TAIWAN").Using(CustomComparers.TypeComparison), "Line");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Address.ChineseLine, NUnit.Framework.Is.EqualTo("90093臺北巿臺北加工出口區園東街6號").Using(CustomComparers.TypeComparison), "ChineseLine");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Communications.FirstOrDefault(c => c.TypeID == MessageConstants.CommunicationTypeIDs.TE).ID, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison), "Phone");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Communications.FirstOrDefault(c => c.TypeID == MessageConstants.CommunicationTypeIDs.MA).ID, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison), "Email");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Communications.FirstOrDefault(c => c.TypeID == MessageConstants.CommunicationTypeIDs.FX).ID, NUnit.Framework.Is.EqualTo("13925579322").Using(CustomComparers.TypeComparison), "Fax");
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.ContactName, NUnit.Framework.Is.EqualTo("Contact1").Using(CustomComparers.TypeComparison), "ContactName");
			});
		}
	}
}
