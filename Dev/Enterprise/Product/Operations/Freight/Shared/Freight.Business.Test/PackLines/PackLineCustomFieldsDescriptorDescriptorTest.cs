using System.Linq;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PackLineCustomFieldsDescriptor))]
	sealed class PackLineCustomFieldsDescriptorDescriptorTest : CustomFieldsDescriptorTest<PackLine>
	{
		public void TestPakLineCustomFields()
		{
			var customFieldsDescriptor = GetNewCustomFieldsDescriptor();

			AssertEquals("all fields should be inactive because the caption hasn't been set in the Freight registry",
				false, customFieldsDescriptor.CustomFieldsInfos.Any(info => info.IsActive));

			Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "String1";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption = "String2";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption = "String3";
			Env.Registry.Freight.PackLine.PackLineCustomAttribute4Caption = "String4";

			Env.Registry.Freight.PackLine.PackLineCustomDate1Caption = "Date1";
			Env.Registry.Freight.PackLine.PackLineCustomDate2Caption = "Date2";

			Env.Registry.Freight.PackLine.PackLineCustomDecimal1Caption = "Decimal1";
			Env.Registry.Freight.PackLine.PackLineCustomDecimal2Caption = "Decimal2";

			Env.Registry.Freight.PackLine.PackLineCustomFlag1Caption = "Flag1";
			Env.Registry.Freight.PackLine.PackLineCustomFlag2Caption = "Flag2";

			AssertEquals("all fields should be active because the caption have been set in the Freight registry",
				true, customFieldsDescriptor.CustomFieldsInfos.All(info => info.IsActive));

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"String1",
				"String2",
				"String3",
				"String4",
				"Date1",
				"Date2",
				"Decimal1",
				"Decimal2",
				"Flag1",
				"Flag2"
			},
			customFieldsDescriptor.CustomFieldsInfos.Select(info => info.Caption));
		}

		protected override CustomFieldsDescriptor<PackLine> GetNewCustomFieldsDescriptor()
		{
			return new PackLineCustomFieldsDescriptor();
		}

		protected override PackLine GetNewCustomFieldsDescriptorParent()
		{
			return Factory.New<PackLine>();
		}
	}
}
