using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	public class DummyCustomFieldsDescriptor : CustomFieldsDescriptor<DummyBusinessObject>
	{
		public DummyCustomFieldsDescriptor(IEnumerable<CustomFieldInfo> customFieldsInfos)
		{
			this.customFieldsInfos = customFieldsInfos;
		}

		readonly IEnumerable<CustomFieldInfo> customFieldsInfos;

		protected override IEnumerable<CustomFieldInfo> GetCustomFieldsInfos()
		{
			return customFieldsInfos;
		}
	}
}
