using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgAddressActiveStatusAndInfoModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateActiveStatus()
		{
			var filter = new OrgAddressWithActiveStatusModuleTextFilter("TEST")
			{
				Property = string.Empty,
				ActiveStatus = string.Empty
			};

			AssertNoErrors(filter.ActiveStatusInfo);

			filter.ActiveStatus = "INVALID";
			AssertHasErrors(filter.ActiveStatusInfo);

			filter.Property = "123";
			filter.ActiveStatus = string.Empty;
			AssertHasError(filter.ActiveStatusInfo, "Can't filter address without active status");

			var allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys.ToList();
			allLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					filter.ActiveStatus = OrgAddressWithActiveStatusModuleTextFilter.StatusActive;
					AssertNoErrors(filter.ActiveStatusInfo);
				}
			});
		}
	}
}
