using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EntityStaffRestriction))]
	public class EntityStaffRestrictionTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var staffRestriction = Factory.NewWithValidTestData<EntityStaffRestriction>();
			staffRestriction.ESR_ParentTableCode = CrmOpportunitySchema.PK.ColumnPrefix;
			Factory.Save();
			staffRestriction.Delete();
		}
	}
}
