using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageApplicationLocalManufacturer))]
	sealed class LicensingMessageApplicationLocalManufacturerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var org = new TestTWCreator(Factory).CreateOrganization();
			var localProcessorAddress = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew().LocalProcessorAddress;
			localProcessorAddress.E2_OA_Address = org.MainAddress.PK;
			localProcessorAddress.E2_AddressOverride = true;
			IPartyDetails localProcessorAddressWrapper = new LicensingMessageApplicationLocalManufacturer(localProcessorAddress);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Communications.Count(), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(localProcessorAddressWrapper.Communications.FirstOrDefault().ID, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison));
			});
		}
	}
}
