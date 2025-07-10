using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RecentActivityDateFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateProperty1()
		{
			var filter = new RecentActivityDateFilter("Dummy", OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var validation = new RecentActivityDateFilterValidation(filter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Empty;

			filter.TypeProperty = "";
			filter.Property1 = ZDateTime.Empty;
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, false);

			filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			filter.Property1 = ZDateTime.Empty;
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, true);

			filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			filter.Property1 = new ZDateTime(2014, 1, 1);
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, false);

			filter.Property2 = new ZDateTime(2014, 1, 1);
			filter.Property1 = ZDateTime.Empty;
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, false);

			filter.PropertySearch = ModuleDateFilter.Past;
			filter.Property2 = ZDateTime.Empty;
			filter.Property1 = ZDateTime.Empty;
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, false);
		}

		public void TestValidateProperty2()
		{
			var filter = new RecentActivityDateFilter("Dummy", OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var validation = new RecentActivityDateFilterValidation(filter);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;

			filter.TypeProperty = "";
			filter.Property2 = ZDateTime.Empty;
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, false);

			filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			filter.Property2 = ZDateTime.Empty;
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, true);

			filter.TypeProperty = SalesRelationActivityFilterHelper.AnySalesRelationTypeCode;
			filter.Property2 = new ZDateTime(2014, 1, 1);
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, false);

			filter.Property1 = new ZDateTime(2014, 1, 1);
			filter.Property2 = ZDateTime.Empty;
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, false);

			filter.PropertySearch = ModuleDateFilter.Future;
			filter.Property2 = ZDateTime.Empty;
			filter.Property1 = ZDateTime.Empty;
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, false);
		}

		public void TestValidateTypeProperty()
		{
			var filter = new RecentActivityDateFilter("Dummy", OrgOpportunitySchema.Constants.Prefix, OrgOpportunitySchema.P8_SystemLastEditTimeUtc, typeof(OrgOpportunity));
			var validation = new RecentActivityDateFilterValidation(filter);

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
