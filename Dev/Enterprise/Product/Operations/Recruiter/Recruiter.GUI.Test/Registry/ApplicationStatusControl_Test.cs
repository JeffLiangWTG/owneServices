using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI
{
	[TestedType(typeof(ApplicationStatusControl))]
	class ApplicationStatusControl_Test : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ApplicationStatusCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ApplicationStatusControl)control).StatusesGrid.ReadOnly && !((ApplicationStatusControl)control).notificationEmailTemplateControl1.Enabled;
		}
	}
}
