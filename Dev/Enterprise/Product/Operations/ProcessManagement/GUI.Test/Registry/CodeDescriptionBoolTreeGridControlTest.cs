using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(CodeDescriptionBoolTreeGridControl))]
	public class CodeDescriptionBoolTreeGridControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CodeDescriptionBoolTreeNodeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CodeDescriptionBoolTreeGridControl)control).CodeDescriptionBoolGridReadOnly;
		}
	}
}
