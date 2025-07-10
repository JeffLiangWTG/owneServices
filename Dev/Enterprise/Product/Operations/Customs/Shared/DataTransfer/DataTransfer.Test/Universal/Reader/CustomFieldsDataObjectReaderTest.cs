using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class CustomFieldsDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestPopulateCustomFields()
		{
			var dataObject = new DummyDataObjectWithCustomizedFields();
			dataObject.CustomizedFieldCollection = new List<CustomizedField>
			{
				CustomizedField.New("String1", new ZString("Test String")),
				CustomizedField.New("Bool2", new ZBool(true)),
				CustomizedField.New("Decimal3", new ZDecimal(12.36m))
			};

			var dummy = CustomFieldProcessTaskTemplateTestHelper.GetDummy(Factory.BOFactory);

			var reader = new CustomFieldsDataObjectReader<DummyWithCustomFields>(dataObject, new TestErrorLogger(), Factory);
			var usedFeilds = new ValueTuple<string, DataType?>[] { new ValueTuple<string, DataType>("Bool2", DataType.Boolean) };
			reader.PopulateCustomFields(dummy, usedFeilds);

			var customFields = dummy.GetUserDefinedValues().Select(x => $"{x.PropertyName} - {x.Value}");
			AssertContainsExactElementsInAnyOrder(new[] { "String1 - Test String", "Decimal3 - 12.36" }, customFields);
		}
	}
}
