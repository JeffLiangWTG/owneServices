using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(SuggestedOrganisation))]
	public class SuggestedOrganisationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new SuggestedOrganisation(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new SuggestedOrganisation((NoResString)"Consignee", null));
			AssertExceptionThrown<ArgumentNullException>(() => new SuggestedOrganisation(null, Factory.New<OrgHeader>()));
			AssertNoExceptionThrown(() => new SuggestedOrganisation((NoResString)"Consignee", Factory.New<OrgHeader>()));
		}

		public void TestOrgHeader()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var suggestion = new SuggestedOrganisation((NoResString)"Consginee", orgHeader);
			AssertEquals(orgHeader, suggestion.OrgHeader);
		}

		public void TestOrganisationType()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var suggestion = new SuggestedOrganisation((NoResString)"Consginee", orgHeader);
			AssertEquals("Consginee", suggestion.OrganisationType);
		}

		public void TestOrganisationCode()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "JNC";
			var suggestion = new SuggestedOrganisation((NoResString)"Consginee", orgHeader);
			AssertEquals("JNC", suggestion.OrganisationCode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgHeader = Factory.New<OrgHeader>();
			return new SuggestedOrganisation((NoResString)"Consginee", orgHeader);
		}

		#endregion
	}
}
