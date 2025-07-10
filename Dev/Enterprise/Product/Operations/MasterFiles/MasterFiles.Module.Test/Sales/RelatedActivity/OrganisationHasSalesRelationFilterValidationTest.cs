using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrganisationHasSalesRelationFilterValidationTest : HasSalesRelationFilterValidationTest
	{
		public void TestValidateTypeProperty()
		{
			var filter = new OrganisationHasSalesRelationFilter(ZString.Empty, ZString.Format("moo"), OrgHeaderSchema.PK);
			var validation = new OrganisationHasSalesRelationFilterValidation(filter);

			filter.TypeProperty = "";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, false);

			filter.TypeProperty = "XXX";
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, true);

			filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			validation.ValidateTypeProperty();
			AssertListValidationInvalidCodeError(filter.TypePropertyInfo, false);
		}
	}
}
