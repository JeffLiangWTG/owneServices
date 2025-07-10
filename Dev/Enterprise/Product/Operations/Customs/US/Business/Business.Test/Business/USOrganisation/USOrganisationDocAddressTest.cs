using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOrganisationDocAddress))]
	sealed class USOrganisationDocAddressTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (USOrganisationDocAddress)base.GetNewBusinessObjectForDeleteTest(factory);
			return result;
		}

		#endregion
	}
}
