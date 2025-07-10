using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module.Test
{
	public class WorkItemFlattenedCollectionInfoTest : TestCaseWithFactory
	{
		public void TestMandatoryMappings()
		{
			var collection = new WorkItemFlattenedCollection(Factory);
			var impl = new WorkItemFlattenedCollectionInfo(collection);
			AssertMappingIsMandatory(impl, WorkItemSchema.Constants.WKI_Summary);
		}

		void AssertMappingIsMandatory(WorkItemFlattenedCollectionInfo impl, string fieldName)
		{
			IImportPropertyInfo propertyInfo;
			propertyInfo = ((IImportCollectionInfo)impl).Properties.SingleOrDefault(x => x.MappingName == fieldName);
			AssertNotNull(propertyInfo);
			Assert($"Field {fieldName} should be mandatory", propertyInfo.IsMandatory);
		}
	}
}
