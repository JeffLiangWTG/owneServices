using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EmptyOrgMatchApproval))]
	sealed class EmptyOrgMatchApprovalTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentFax()
		{
			EmptyOrgMatchApproval matchApproval = Factory.New<EmptyOrgMatchApproval>();
			AssertEquals("", matchApproval.Fax);
		}

		public void TestParentStreet2()
		{
			EmptyOrgMatchApproval matchApproval = Factory.New<EmptyOrgMatchApproval>();
			AssertEquals("", matchApproval.Street2);
		}

		public void TestParentNull()
		{
			EmptyOrgMatchApproval matchApproval = Factory.New<EmptyOrgMatchApproval>();
			AssertEquals("Parent should be null", null, matchApproval.Parent);
		}

		public void TestMatchType()
		{
			EmptyOrgMatchApproval matchApproval = Factory.New<EmptyOrgMatchApproval>();
			AssertEquals("Match type should be correct", OrgMatchApprovalType.Empty.Code, matchApproval.MatchType.Code);
		}

		[ExpectNoExceptions]
		public void TestCopyToOrganisation_WorksWithoutError()
		{
			OrgHeader newOrganisation = Factory.New<OrgHeader>();
			EmptyOrgMatchApproval matchApproval = Factory.New<EmptyOrgMatchApproval>();
			matchApproval.CopyDetailsToOrganisation(newOrganisation);
		}

		[ExpectExceptionMessage(typeof(NotSupportedException), "Setting P2_ParentID is not supported")]
		public void TestSettingP2_ParentIDIsNotSupported()
		{
			EmptyOrgMatchApproval matchApproval = (EmptyOrgMatchApproval)Factory.New(typeof(OrgMatchApproval));
			matchApproval.P2_ParentID = ZGuid.NewZGuid();
		}
	}
}
