using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	[TestedType(typeof(RecipientSourceFallbackControl))]
	class RecipientSourceFallbackControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new RecipientSourceFallbackHeader();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return control.FindSingle<ZGrid>("FallbackSourcesGrid").ReadOnly;
		}
	}
}
