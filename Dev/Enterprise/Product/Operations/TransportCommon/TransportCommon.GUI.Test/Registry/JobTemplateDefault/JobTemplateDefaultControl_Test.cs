using CargoWise.EntityFramework;
using Enterprise.Registry.GUI;
using Enterprise.TransportCommon.Registry;
using NUnit.Framework;

namespace Enterprise.TransportCommon.GUI.Registry
{
	[TestedType(typeof(JobTemplateDefaultControl))]
	class JobTemplateDefaultControl_Test : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new JobTemplateDefaultCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((JobTemplateDefaultControl)control).JobTemplateDefaultGrid.ReadOnly;
		}
	}
}
