using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobEquipmentItem))]
	public class JobEquipmentItemTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
		 var refEquipmentCombinationItem  = Factory.NewWithValidTestData<JobEquipmentItem>();
			refEquipmentCombinationItem.JEI_ParentTableCode = "JEQ";
			return refEquipmentCombinationItem;
		}
	}
}
