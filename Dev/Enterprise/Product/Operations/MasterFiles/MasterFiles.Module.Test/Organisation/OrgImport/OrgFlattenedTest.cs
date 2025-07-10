using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport.Autos.Testing
{
	[TestedType(typeof(OrgFlattened))]
	sealed class OrgFlattenedTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOC_EmailMaxLengthConsistentWithSchema()
		{
			AssertEquals(OrgContactSchema.OC_Email.MaxLength, new OrgFlattened().OC_EmailInfo.MaxLength);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgFlattened();
		}

		#endregion
	}
}
