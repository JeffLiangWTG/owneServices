using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class CustomFieldsDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateCustomFields()
		{
			var template = CustomFieldProcessTaskTemplateTestHelper.GetTemplate(Factory);
			CustomFieldProcessTaskTemplateTestHelper.AddCustomField(template, "String1", AddOnColumnDataType.Codes.String);
			CustomFieldProcessTaskTemplateTestHelper.AddCustomField(template, "Bool2", AddOnColumnDataType.Codes.Boolean);
			CustomFieldProcessTaskTemplateTestHelper.AddCustomField(template, "Decimal3", AddOnColumnDataType.Codes.Decimal);
			var dummy = CustomFieldProcessTaskTemplateTestHelper.GetDummy(Factory);

			Factory.Save();

			dummy.SetUserDefinedValue("String1", new ZString("This is a test string"));
			var dataObject = new DummyDataObjectWithCustomizedFields();
			var dataWritingManager = new DataWritingManager(new ActionInfo(null, dummy));
			var writer = new CustomFieldsDataObjectWriter<DummyWithCustomFields, DummyDataObjectWithCustomizedFields>(dataWritingManager, dataObject);

			writer.GetDataObject(dummy);

			var customFields = dataObject.CustomizedFieldCollection.Select(x => $"{x.DataType}: {x.Key} - {x.Value}");
			AssertContainsExactElementsInAnyOrder(new[] { "String: String1 - This is a test string", "Boolean: Bool2 - false", "Decimal: Decimal3 - 0" }, customFields);
		}
	}
}
