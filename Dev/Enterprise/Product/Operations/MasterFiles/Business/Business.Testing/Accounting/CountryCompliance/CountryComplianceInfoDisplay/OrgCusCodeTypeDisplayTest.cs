using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay.Testing
{
	[TestedType(typeof(OrgCusCodeTypeDisplay))]
	sealed class OrgCusCodeTypeDisplayTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgCusCodeTypeDisplay(new CodeDescriptionPair("Code", (NoResString)"Desc"), false, false);
		}
	}
}
