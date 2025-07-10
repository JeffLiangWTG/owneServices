using System;
using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CurrentCompanyFilterHandler))]
	sealed class CurrentCompanyFilterHandlerTest : BaseJobDeclarationIndexFilterHandlerTestCase<CurrentCompanyFilterHandler>
	{
		public void TestIsApplicable_ReturnsFalse_WhenShouldAddCompanyRelatedFiltersIsFalse()
		{
			using (Env.Instance.TemporaryServiceTaskContext("TAG", canRunInAnyBranch: true))
			{
				CombineAssertions(() =>
				{
					foreach (var applicableType in FilterHandlerToTest.ApplicableTypes)
					{
						var applicableObject = (FilterStripBusinessObject)Activator.CreateInstance(applicableType);
						var filterHandler = GetNewIndexFilterHandler(applicableObject);
						Assert("CurrentCompanyFilterHandler should not be applicable when ShouldAddCompanyRelatedFilters is false", !filterHandler.IsApplicable());
					}
				});
			}
		}

		#region Overrides of IndexFilterHandlerBaseTestCase<CurrentCompanyFilterHandler>

		protected override IReadOnlyCollection<string> ExpectedQueries =>
			new[]
			{
				"(CompanyCode eq 'EDI')",
			};

		#endregion
	}
}
