using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(JiraCustomFieldMappingControl))]
	class JiraCustomFieldMappingControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return ProcessMgmtTestHelper.GetDummyJiraCustomFieldMap();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.FindSingle<ZGrid>().ReadOnly;
		}
	}
}
