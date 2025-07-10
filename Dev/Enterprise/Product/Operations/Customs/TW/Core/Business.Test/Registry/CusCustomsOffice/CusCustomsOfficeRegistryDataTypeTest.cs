using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusCustomsOfficeRegistryDataType))]
	sealed class CusCustomsOfficeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CusCustomsOfficeRegistryDataType>
	{
		protected override CusCustomsOfficeRegistryDataType GetNewDataType() => new CusCustomsOfficeRegistryDataType();
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var item1 = new CusCustomsOffice(new FallbackLevel(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item1.CustomsOfficeCode = "CE";
			var item2 = new CusCustomsOffice(new FallbackLevel(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK), factory);
			item2.CustomsOfficeCode = "BA";
			return new[] { new ValidSampleAndBinaryValueInDB(item1, new CusCustomsOfficeRegistryDataType().Serialise(item1)), new ValidSampleAndBinaryValueInDB(item2, new CusCustomsOfficeRegistryDataType().Serialise(item2)) };
		}

		protected override string ExpectedEditorName => "CusCustomsOfficeRegistryItemEditor";
		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();
			new TestTWCreator(factory).CreateCustomsOffice();
		}

		BusinessObjectFactory factory;
	}
}
