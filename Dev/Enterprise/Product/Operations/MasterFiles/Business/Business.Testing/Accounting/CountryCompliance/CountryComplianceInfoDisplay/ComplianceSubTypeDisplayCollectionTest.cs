using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(ComplianceSubTypeDisplayCollection))]
	sealed class ComplianceSubTypeDisplayCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComplianceSubTypeDisplayCollection>
	{
		protected override ComplianceSubTypeDisplayCollection GetCollectionToTest() => new ComplianceSubTypeDisplayCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceSubTypeDisplay("AU",
				new ComplianceSubType("XX",
					() => (NoResString)"Desc",
					() => "Local Desc",
					() => "Internal Note"
					)
				);
		}
	}
}
