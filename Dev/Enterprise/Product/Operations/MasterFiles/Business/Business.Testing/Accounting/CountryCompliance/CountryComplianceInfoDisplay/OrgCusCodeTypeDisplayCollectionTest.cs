using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(OrgCusCodeTypeDisplayCollection))]
	sealed class OrgCusCodeTypeDisplayCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgCusCodeTypeDisplayCollection>
	{
		protected override OrgCusCodeTypeDisplayCollection GetCollectionToTest() => new OrgCusCodeTypeDisplayCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgCusCodeTypeDisplay(new CodeDescriptionPair("Code", (NoResString)"Desc"), false, false);
		}
	}
}
