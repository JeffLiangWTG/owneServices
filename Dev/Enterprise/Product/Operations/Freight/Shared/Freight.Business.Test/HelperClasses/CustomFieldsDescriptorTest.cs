using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestsSubclassesOf(typeof(CustomFieldsDescriptor<>))]
	public abstract class CustomFieldsDescriptorTest<T> : TestCaseWithFactory
			where T : BusinessObject
	{
		public void TestGetCustomFieldsInfosDoNotReturnNull()
		{
			var descriptor = GetNewCustomFieldsDescriptor();
			AssertNotNull("GetCustomFieldsInfos should return empty enumerable instead of null", descriptor.CustomFieldsInfos);
		}

		public void TestAllCustomFieldInfosHaveSchemaColumnAndZDataType()
		{
			var descriptor = GetNewCustomFieldsDescriptor();

			foreach (var customFieldInfo in descriptor.CustomFieldsInfos)
			{
				AssertEquals("SchemaColumnName cannot be empty", false, string.IsNullOrWhiteSpace(customFieldInfo.SchemaColumnName));
				AssertNotNull("ZDataType cannot be null", customFieldInfo.ZDataType);
			}
		}

		public void TestNoDuplicateCustomFieldInfos()
		{
			var descriptor = GetNewCustomFieldsDescriptor();
			var customFieldInfos = descriptor.CustomFieldsInfos.ToArray();
			var groupedByColumnName = customFieldInfos.GroupBy(info => info.SchemaColumnName)
				.Where(group => group.Count() > 1)
				.Select(group => group.Key);

			AssertEquals(string.Format("The following CustomFieldInfos have duplicate SchemaColumnNames {0}", string.Join(",", groupedByColumnName)),
				false, groupedByColumnName.Any());
		}

		public void TestZDataTypeMatchesPropertyType()
		{
			var descriptor = GetNewCustomFieldsDescriptor();
			var parent = GetNewCustomFieldsDescriptorParent();

			foreach (var customFieldInfo in descriptor.CustomFieldsInfos)
			{
				var propertyInfo = parent.ZPropertyInfoHash[customFieldInfo.SchemaColumnName];

				AssertNotNull(string.Format("propertyInfo for {0} not found", customFieldInfo.SchemaColumnName), propertyInfo);
				Assert(string.Format("ZDataType must match property type on {0}", customFieldInfo.SchemaColumnName),
					customFieldInfo.ZDataType == propertyInfo.PropertyType);
			}
		}

		public void TestParentType()
		{
			var descriptor = GetNewCustomFieldsDescriptor();
			var parent = GetNewCustomFieldsDescriptorParent();

			AssertNotNull("ParentType cannot be null", descriptor.ParentType);
			AssertEquals("ParentType must match type of GetNewCustomFieldsDescriptorParent", descriptor.ParentType, parent.GetType());
		}

		protected abstract CustomFieldsDescriptor<T> GetNewCustomFieldsDescriptor();
		protected abstract T GetNewCustomFieldsDescriptorParent();
	}
}
