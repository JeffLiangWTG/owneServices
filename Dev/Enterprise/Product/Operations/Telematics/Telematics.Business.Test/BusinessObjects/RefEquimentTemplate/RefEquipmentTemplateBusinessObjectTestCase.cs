using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(RefEquipmentTemplate))]
	class RefEquipmentTemplateBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var o = (RefEquipmentTemplate)base.GetNewBusinessObjectForDeleteTest(factory);
			o.RET_Description = "descr";
			return o;
		}
	}
}
