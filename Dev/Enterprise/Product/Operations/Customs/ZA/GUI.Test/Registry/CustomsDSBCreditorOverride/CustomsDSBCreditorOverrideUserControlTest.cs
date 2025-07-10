using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.DataRegistry.GUI.Testing
{
	[TestedType(typeof(CustomsDSBCreditorOverrideUserControl))]
	sealed class CustomsDSBCreditorOverrideUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CustomsDSBCreditorOverrideCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CustomsDSBCreditorOverrideUserControl)control).MainGrid.ReadOnly;
		}
	}
}
