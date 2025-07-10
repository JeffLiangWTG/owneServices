using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusCustomsOfficeRegistryItem))]
	sealed class CusCustomsOfficeRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CusCustomsOffice>
	{
		protected override StronglyTypedRegistryItem<CusCustomsOffice, CusCustomsOffice> GetNewRegistryItem() => new CusCustomsOfficeRegistryItem("", null, null, null, RegistryStorageFlags.BranchDepartment);
		protected override CusCustomsOffice ValidValue
		{
			get
			{
				var result = new CusCustomsOffice(new FallbackLevel(Guid.Empty, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
				result.CustomsOfficeCode = "CE";
				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateCustomsOffice();
		}
	}
}
