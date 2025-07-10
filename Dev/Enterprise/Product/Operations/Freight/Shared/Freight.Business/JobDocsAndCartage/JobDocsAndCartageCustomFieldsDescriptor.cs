using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobDocsAndCartageCustomFieldsDescriptor : CustomFieldsDescriptor<JobDocsAndCartage>
	{
		protected override IEnumerable<CustomFieldInfo> GetCustomFieldsInfos()
		{
			yield return GetCustomFieldInfo(FreightDataRegistry.Instance.ShipmentCustomText1.Value, JobDocsAndCartageSchema.JP_CustomAttrib1);
			yield return GetCustomFieldInfo(FreightDataRegistry.Instance.ShipmentCustomText2.Value, JobDocsAndCartageSchema.JP_CustomAttrib2);

			yield return GetCustomFieldInfo(FreightDataRegistry.Instance.ShipmentCustomDate1.Value, JobDocsAndCartageSchema.JP_CustomDate1);
			yield return GetCustomFieldInfo(FreightDataRegistry.Instance.ShipmentCustomDate2.Value, JobDocsAndCartageSchema.JP_CustomDate2);

			yield return GetCustomFieldInfo(FreightDataRegistry.Instance.ShipmentCustomDecimalNo1.Value, JobDocsAndCartageSchema.JP_CustomDecimal1);
			yield return GetCustomFieldInfo(FreightDataRegistry.Instance.ShipmentCustomDecimalNo2.Value, JobDocsAndCartageSchema.JP_CustomDecimal2);

			yield return GetCustomFieldInfo(FreightDataRegistry.Instance.ShipmentCustomFlag1.Value, JobDocsAndCartageSchema.JP_CustomFlag1);
			yield return GetCustomFieldInfo(FreightDataRegistry.Instance.ShipmentCustomFlag2.Value, JobDocsAndCartageSchema.JP_CustomFlag2);
		}

		CustomFieldInfo GetCustomFieldInfo(ICaptionAndHint captionAndHint, SchemaColumn schemaColumn)
		{
			return CustomFieldInfo.New(captionAndHint.Caption,
				captionAndHint.Hint,
				schemaColumn);
		}
	}
}
