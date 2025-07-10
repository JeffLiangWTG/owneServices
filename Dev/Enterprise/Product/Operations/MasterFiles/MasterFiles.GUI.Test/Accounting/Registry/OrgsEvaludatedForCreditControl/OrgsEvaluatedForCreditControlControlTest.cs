using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgsEvaluatedForCreditControlControl))]
	sealed class OrgsEvaluatedForCreditControlControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OrgsEvaluatedForCreditControlCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((OrgsEvaluatedForCreditControlControl)control).OrgsEvaluatedForCreditControlGrid.ReadOnly;
		}
	}
}
