using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocAddressCreatorHostLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestAddressTypeCode

		public void TestAddressTypeCode()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			AssertEquals(true, host.Lookups.AddressTypes.ContainsOnly("CFS", "CTO"));
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageCFS), host.Lookups.AddressTypes.GetDescriptionFromCode("CFS"));
			AssertEquals(DocAddressTypes.GetDescription(Factory, DocAddressType.LocalCartageCTO), host.Lookups.AddressTypes.GetDescriptionFromCode("CTO"));
		}

		#endregion

		#region TestOrganisations

		public void TestOrganisations()
		{
			var host = DocAddressCreatorHelper.CreateDocAddressCreatorHost();
			AssertNotNull(host.Lookups.Organisations);
			AssertEquals(typeof(OrgHeaderCollection), host.Lookups.Organisations.GetType());
		}

		#endregion

		#region DocAddressCreatorHelper

		DocAddressCreatorHelper DocAddressCreatorHelper
		{
			get { return docAddressCreatorHelper ?? (docAddressCreatorHelper = new DocAddressCreatorHelper(Factory)); }
		}
		DocAddressCreatorHelper docAddressCreatorHelper;

		#endregion
	}
}
