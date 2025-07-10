using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business
{
	[TestedType(typeof(ManualRegistrationNoEntry))]
	public class ManualRegistrationNoEntryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ManualRegistrationNoEntry("2020IMP5555", ZDateTime.BrettsBirthday);
		}

		public void TestRegistrationNumberMaxLength()
		{
			var manualRegistrationNoEntry = new ManualRegistrationNoEntry("2020IMP5555", ZDateTime.BrettsBirthday);
			AssertEquals(18, manualRegistrationNoEntry.RegistrationNumberInfo.MaxLength);
			AssertEquals("2020IMP5555", manualRegistrationNoEntry.RegistrationNumber);
			AssertEquals(ZDateTime.BrettsBirthday, manualRegistrationNoEntry.RegistrationDate);
		}
	}
}

