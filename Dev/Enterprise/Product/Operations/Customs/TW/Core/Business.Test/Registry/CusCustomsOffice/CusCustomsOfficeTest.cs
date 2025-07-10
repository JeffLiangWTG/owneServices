using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusCustomsOffice))]
	sealed class CusCustomsOfficeTest : RegistryBusinessObjectTemplateTestCase<CusCustomsOffice>
	{
		#region Properties
		[ExpectNoExceptions]
		public void TestCustomsOfficeCode()
		{
			NUnit.Framework.Assert.That(currentElement.CustomsOfficeCode, NUnit.Framework.Is.EqualTo("CE").Using(CustomComparers.TypeComparison));
			var typeToCheck = currentElement.GetType();
			NUnit.Framework.Assert.That(typeToCheck, CustomConstraints.HasCustomAttribute<ListAttribute>(CusCustomsOffice.Schema.CustomsOfficeCode, false, attrib => attrib.ListDataSourceMember == "Lookups.CustomsOfficeList"));
		}

		[ExpectNoExceptions]
		public void TestValidationType()
		{
			var validation = currentElement.Validation;
			NUnit.Framework.Assert.That(validation, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusCustomsOfficeValidation)));
			NUnit.Framework.Assert.That(validation, NUnit.Framework.Is.TypeOf<CusCustomsOfficeValidation>());
		}

		[ExpectNoExceptions]
		public void TestLookupsType()
		{
			var lookups = currentElement.Lookups;
			NUnit.Framework.Assert.That(lookups, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Business.CusCustomsOfficeLookups)));
			NUnit.Framework.Assert.That(lookups, NUnit.Framework.Is.TypeOf<CusCustomsOfficeLookups>());
		}

		#endregion
		#region override
		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;
		protected override BusinessObject GetNewBusinessObject() => new CusCustomsOffice(new FallbackLevel(Env.CurrentCompanyPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK), Factory);
		protected override CusCustomsOffice GetBusinessObjectToClone() => (CusCustomsOffice)GetNewBusinessObject();
		protected override CusCustomsOffice GetBusinessObjectToSerialise() => (CusCustomsOffice)GetNewBusinessObject();
		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateCustomsOffice();
			currentElement = (CusCustomsOffice)GetNewBusinessObject();
			currentElement.CustomsOfficeCode = "CE";
		}

		CusCustomsOffice currentElement;
		#endregion
	}
}
