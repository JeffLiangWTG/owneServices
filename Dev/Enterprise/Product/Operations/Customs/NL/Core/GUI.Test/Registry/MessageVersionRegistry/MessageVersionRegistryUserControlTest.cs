using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(MessageVersionRegistryUserControl))]
sealed class MessageVersionRegistryUserControlTest : RegistryZUserControlTestCase
{
	protected override IBusiness GetNewBusinessEntity() => new MessageVersionRegistryCollection();

	protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => control.ReadOnly || businessEntity.IsReadOnly;
}
