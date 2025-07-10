using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class GlbExternalPasswordLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestPasswordTypesList()
		{
			var externalPassword = Factory.New<GlbExternalPassword>();
			var list1 = externalPassword.Lookups.PasswordTypeList;
			var list2 = externalPassword.Lookups.PasswordTypeList;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list1.GetDescriptionFromCode("TVA"), NUnit.Framework.Is.EqualTo(PasswordTypesList.Descriptions.TVA));
				NUnit.Framework.Assert.That(list1.GetDescriptionFromCode("UVC"), NUnit.Framework.Is.EqualTo(PasswordTypesList.Descriptions.UVC));
				NUnit.Framework.Assert.That(list1.Count, NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(list2, NUnit.Framework.Is.SameAs(list1));
			});
		}
	}
}
