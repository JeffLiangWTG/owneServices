using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.LVS.Business;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	public class ConvertToStandAloneDeclarationHelperTest : TestCaseWithFactory
	{
		public void TestConvertToStandAloneDeclaration_ShouldSetConsignmentToInactive()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			Assert(consignment.ULB_IsActive);

			ConvertToStandAloneDeclarationHelper.ConvertToStandAloneDeclarationIndividual(Factory, new CusUSLVConsignment[] { consignment }, null, true);
			Assert(!consignment.ULB_IsActive);
		}
	}
}
