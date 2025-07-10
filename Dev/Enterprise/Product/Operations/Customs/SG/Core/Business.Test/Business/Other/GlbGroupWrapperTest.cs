using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(GlbGroupForPluginWrapper))]
	public class GlbGroupWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var group = Factory.New<GlbGroup>();
			return new GlbGroupForPluginWrapper(group);
		}

		public void TestAccessPassword()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var password = Factory.NewWithValidTestData<GlbExternalPassword_SGA>();
			password.GP_GG = group1.PK;
			Factory.Save();
			var groupWrapper1 = new GlbGroupForPluginWrapper(group1);
			AssertEquals(password.PK, groupWrapper1.AccessPassword.PK);
			var groupWrapper2 = new GlbGroupForPluginWrapper(group2);
			AssertNotNull(groupWrapper2.AccessPassword);
			Assert(!groupWrapper2.AccessPassword.IsInDatabase);
			Assert(!groupWrapper2.AccessPassword.HasChanges);
		}
	}
}
