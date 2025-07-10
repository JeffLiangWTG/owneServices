using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRServiceTemplateLine))]
	class MNRServiceTemplateLineTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<MNRServiceTemplateLine>();
		}

		#region TestServiceTemplate
		public void TestServiceTemplate()
		{
			var serviceTemplate = Factory.NewWithValidTestData<MNRServiceTemplate>();
			var serviceTemplateLine = (MNRServiceTemplateLine)GetNewBusinessObject();
			serviceTemplateLine.MSL_MST_MNRServiceTemplate = serviceTemplate.PK;
			AssertEquals(serviceTemplate, serviceTemplateLine.ServiceTemplate);
		}
		#endregion
	}
}
