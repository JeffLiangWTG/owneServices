using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DummyCustomFieldsDescriptor))]
	sealed class DummyCustomFieldsDescriptorTest : CustomFieldsDescriptorTest<DummyBusinessObject>
	{
		protected override CustomFieldsDescriptor<DummyBusinessObject> GetNewCustomFieldsDescriptor()
		{
			var customFieldInfos = new List<CustomFieldInfo>
			{
				CustomFieldInfo.New(true, "string column caption", "hint", DummyBizoSchema.Z0_Description),
				CustomFieldInfo.New(true,"decimal column caption", "hint", DummyBizoSchema.Z0_Decimal),
				CustomFieldInfo.New(true, "date column caption", "hint", DummyBizoSchema.Z0_Date),
				CustomFieldInfo.New(true,"bool column caption", "hint", DummyBizoSchema.Z0_Bool),
				CustomFieldInfo.New(false,"inactive string column caption", "hint", DummyBizoSchema.Z0_Code),
				CustomFieldInfo.New(true, "int column caption", "hint" , DummyBizoSchema.Z0_Number)
			};

			return new DummyCustomFieldsDescriptor(customFieldInfos);
		}

		protected override DummyBusinessObject GetNewCustomFieldsDescriptorParent()
		{
			return Factory.New<DummyBusinessObject>();
		}
	}
}
