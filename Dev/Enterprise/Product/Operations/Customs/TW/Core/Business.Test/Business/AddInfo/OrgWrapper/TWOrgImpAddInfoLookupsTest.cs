using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWOrgImpAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestExamModeList()
		{
			NUnit.Framework.Assert.That(lookups.ExamModeList.CodesAsString, NUnit.Framework.Is.EqualTo("2, 3, 4, 6, 7, 8, 9, A"));
		}

		[ExpectNoExceptions]
		public void TestIMPPaymentMethodList()
		{
			NUnit.Framework.Assert.That(lookups.IMPPaymentMethodList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2, 3, 4, 5, 6, 7, 8"));
		}

		[ExpectNoExceptions]
		public void TestEXPPaymentMethodList()
		{
			NUnit.Framework.Assert.That(lookups.EXPPaymentMethodList.CodesAsString, NUnit.Framework.Is.EqualTo("1, 2"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var orgHeader = Factory.New<OrgHeader>();
			lookups = TWOrgImpAddInfo.Get(orgHeader).Lookups;
		}

		TWOrgImpAddInfoLookups lookups;
	}
}
