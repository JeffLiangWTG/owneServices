using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(StampDutyLedgerNumberCustomizationRegistryItemUserControl))]
	class StampDutyLedgerNumberCustomizationRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new StampDutyLedgerNumberCustomizationRegistrySetting { ExpiredYear = ZDateTime.Today.Year };

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((StampDutyLedgerNumberCustomizationRegistryItemUserControl)control).ReadOnly;
	}
}
