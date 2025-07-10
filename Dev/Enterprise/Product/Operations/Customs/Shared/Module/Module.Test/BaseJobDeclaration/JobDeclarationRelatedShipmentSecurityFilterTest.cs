using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(JobDeclarationRelatedShipmentSecurityFilter))]
	class JobDeclarationRelatedShipmentSecurityFilterTest : ModuleFilterTestCase<JobDeclarationRelatedShipmentSecurityFilter>
	{
		public void TestFilter()
		{
			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = false;
			Env.Security.MaintainShipmentCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
			Env.Security.MaintainShipmentCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;

			var filterStripBizo = new JobDeclarationFilterBusinessObject();
			var filter = (JobDeclarationRelatedShipmentSecurityFilter)filterStripBizo[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNotNull(filter);

			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = true;
			Env.Security.MaintainShipmentCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			Env.Security.MaintainShipmentCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;

			filterStripBizo = new JobDeclarationFilterBusinessObject();
			filter = (JobDeclarationRelatedShipmentSecurityFilter)filterStripBizo[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNotNull(filter);

			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = true;
			Env.Security.MaintainShipmentCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
			Env.Security.MaintainShipmentCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			filterStripBizo = new JobDeclarationFilterBusinessObject();
			filter = (JobDeclarationRelatedShipmentSecurityFilter)filterStripBizo[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNotNull(filter);

			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = true;
			Env.Security.MaintainShipmentCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
			Env.Security.MaintainShipmentCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;

			filterStripBizo = new JobDeclarationFilterBusinessObject();
			filter = (JobDeclarationRelatedShipmentSecurityFilter)filterStripBizo[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNull(filter);
		}

		public override void TestIsExpensiveQuery()
		{
			Assert(!Filter.IsExpensiveQuery);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			Assert(!Filter.Query.IsEmpty);
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(JobDeclarationRelatedShipmentSecurityFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Append(nameof(filter.ComparisonOperator)).ToArray();
		}

		protected override JobDeclarationRelatedShipmentSecurityFilter GetNewModuleFilter()
		{
			return new JobDeclarationRelatedShipmentSecurityFilter(Factory);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override ZString ExpectedDescription => DeclarationFilterConstants.RelatedShipmentSecurity;
	}
}
