using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI
{
	[TestedType(typeof(RecruiterTestTypeControl))]
	class RecruiterTestTypeControl_Test : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new RecruiterTestTypeCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((RecruiterTestTypeControl)control).RecruiterTestTypeGrid.ReadOnly;
		}
	}
}
