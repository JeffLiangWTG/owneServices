using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module.Test
{
	public class ProjectFlattenedCollectionInfoTest : TestCaseWithFactory
	{
		public void TestMandatoryMappings()
		{
			var collection = new ProjectFlattenedCollection(Factory);
			var impl = new ProjectFlattenedCollectionInfo(collection);
			AssertMappingIsMandatory(impl, WorkProjectSchema.Constants.WKP_Summary);
		}

		void AssertMappingIsMandatory(ProjectFlattenedCollectionInfo impl, string fieldName)
		{
			IImportPropertyInfo propertyInfo;
			propertyInfo = ((IImportCollectionInfo)impl).Properties.SingleOrDefault(x => x.MappingName == fieldName);
			AssertNotNull(propertyInfo);
			Assert($"Field {fieldName} should be mandatory", propertyInfo.IsMandatory);
		}
	}
}
