using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusCustomsOfficeLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCustomsOfficeList()
		{
			new TestTWCreator(Factory).CreateCustomsOffice();
			var list = (ZZRefCusCodeListCombinedCollection)parent.Lookups.CustomsOfficeList;
			list.Load();
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "BA"), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "CE"), NUnit.Framework.Is.True);
		}

		protected override void SetUp()
		{
			base.SetUp();
			parent = new CusCustomsOffice(new FallbackLevel(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		}

		CusCustomsOffice parent;
	}
}
