using CargoWise.EntityFramework;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.GUI
{
	[TestedType(typeof(CertificationCodeMappingControl))]
	class CertificationCodeMappingControl_Test : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CertificationCodeMappingCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CertificationCodeMappingControl)control).CertMappingsGrid.ReadOnly;
		}
	}
}
