using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI
{
	[TestedType(typeof(OnlineApplicationDocTypesControl))]
	class OnlineApplicationDocTypesControl_Test : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new OnlineApplicationDocTypeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			OnlineApplicationDocTypesControl onlineApplicationDocTypesControl = (OnlineApplicationDocTypesControl)control;
			return onlineApplicationDocTypesControl.DocTypesGrid.ReadOnly;
		}
	}
}
