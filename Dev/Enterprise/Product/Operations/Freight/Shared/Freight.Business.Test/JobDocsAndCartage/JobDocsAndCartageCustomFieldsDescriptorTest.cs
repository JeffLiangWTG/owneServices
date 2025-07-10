using System;
using System.Linq;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobDocsAndCartageCustomFieldsDescriptor))]
	sealed class JobDocsAndCartageCustomFieldsDescriptorTest : CustomFieldsDescriptorTest<JobDocsAndCartage>
	{
		public void TestJobDocsAndCartageCustomFields()
		{
			var customFieldsDescriptor = GetNewCustomFieldsDescriptor();

			AssertEquals("all fields should be inactive because the caption hasn't been set in the Freight registry",
				false, customFieldsDescriptor.CustomFieldsInfos.Any(info => info.IsActive));

			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("String1", "String1 hint"));
			FreightDataRegistry.Instance.ShipmentCustomText2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("String2", "String2 hint"));
			FreightDataRegistry.Instance.ShipmentCustomDate1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date1", "Date1 hint"));
			FreightDataRegistry.Instance.ShipmentCustomDate2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date2", "Date2 hint"));
			FreightDataRegistry.Instance.ShipmentCustomDecimalNo1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal1", "Decimal1 hint"));
			FreightDataRegistry.Instance.ShipmentCustomDecimalNo2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal2", "Decimal2 hint"));
			FreightDataRegistry.Instance.ShipmentCustomFlag1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Flag1", "Flag1 hint"));
			FreightDataRegistry.Instance.ShipmentCustomFlag2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Flag2", "Flag2 hint"));

			AssertEquals("all fields should be active because the caption have been set in the Freight registry",
				true, customFieldsDescriptor.CustomFieldsInfos.All(info => info.IsActive));

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"String1",
				"String2",
				"Date1",
				"Date2",
				"Decimal1",
				"Decimal2",
				"Flag1",
				"Flag2"
			},
			customFieldsDescriptor.CustomFieldsInfos.Select(info => info.Caption));
		}

		protected override CustomFieldsDescriptor<JobDocsAndCartage> GetNewCustomFieldsDescriptor()
		{
			return new JobDocsAndCartageCustomFieldsDescriptor();
		}

		protected override JobDocsAndCartage GetNewCustomFieldsDescriptorParent()
		{
			return JobDocsAndCartage.New(new MockJobDocsAndCartageParent(Factory));
		}
	}
}
