using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(FallbackControl))]
sealed class FallbackControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity() => new FallbackConfiguration();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((FallbackControl)control).ReadOnly;

	protected override string CountryCode => Core.Constants.CountryCodes.Netherlands;
}
