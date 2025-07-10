using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GLLocalNumberFormatRegistryControl))]
	sealed class GLLocalNumberFormatRegistryControl_ComplianceReportsSetupCategoryControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			GLLocalNumberFormatCollection collection = new GLLocalNumberFormatCollection();
			return collection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((GLLocalNumberFormatRegistryControl)control).GLLocalNumberFormatGrid.ReadOnly;
		}
	}
}
