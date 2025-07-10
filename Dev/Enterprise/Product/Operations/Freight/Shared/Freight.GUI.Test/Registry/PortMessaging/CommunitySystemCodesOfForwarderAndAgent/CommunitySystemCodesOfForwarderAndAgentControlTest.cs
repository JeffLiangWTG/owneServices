using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(CommunitySystemCodesOfForwarderAndAgentControl))]
	sealed class CommunitySystemCodesOfForwarderAndAgentControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CommunitySystemCodesOfForwarderAndAgentCollection();
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new CommunitySystemCodesOfForwarderAndAgentControl();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CommunitySystemCodesOfForwarderAndAgentControl)control).CommunitySystemCodesOfForwarderAndAgentGrid.ReadOnly;
		}
	}
}
