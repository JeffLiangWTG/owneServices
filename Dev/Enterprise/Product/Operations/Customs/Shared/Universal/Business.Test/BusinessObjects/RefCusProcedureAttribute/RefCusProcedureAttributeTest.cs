using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProcedureAttribute))]
	internal class RefCusProcedureAttributeTest : EnterpriseBusinessObjectTestCase
	{
		RefCusProcedure Procedure => procedure ?? (procedure = Factory.NewWithValidTestData<RefCusProcedure>());
		RefCusProcedure procedure;
		protected override BusinessObject GetNewBusinessObject()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			return Procedure.Attributes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var cusProcedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "01", "", "", "", "IMP");
			factory.Save();
			return helper.CreateRefCusProcedureAttribute(cusProcedure.PK, "", "");
		}
	}
}
