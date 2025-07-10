using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentExclusion))]
	public class JobDocumentExclusionTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			var exclusion = Factory.NewWithValidTestData<JobDocumentExclusion>();
			exclusion.JDE_OD_Document = orgDocument.PK;
			orgDocument.OD_OC = orgContact.PK;
			return exclusion;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		#endregion
	}
}
