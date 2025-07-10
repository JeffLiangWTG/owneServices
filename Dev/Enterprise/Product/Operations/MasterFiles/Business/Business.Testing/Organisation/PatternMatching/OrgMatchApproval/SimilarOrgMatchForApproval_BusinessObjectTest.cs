using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SimilarOrgMatchForApproval))]
	sealed class SimilarOrgMatchForApproval_BusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DummyBusinessObject dummyParent = Factory.New<DummyBusinessObject>();
			OrgMatchApproval.Loader loader = new OrgMatchApproval.Loader(Factory);
			DummyOrgMatchApproval dummyMatchApproval = (DummyOrgMatchApproval)loader.LoadOrCreate(dummyParent.PK, OrgMatchApprovalType.DummyType);

			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgPatternMatch orgPatternMatch = Factory.New<OrgPatternMatch>();
			orgPatternMatch.OS_OH = organisation.PK;
			return SimilarOrgMatchForApproval.New(dummyMatchApproval, orgPatternMatch);
		}
	}
}
