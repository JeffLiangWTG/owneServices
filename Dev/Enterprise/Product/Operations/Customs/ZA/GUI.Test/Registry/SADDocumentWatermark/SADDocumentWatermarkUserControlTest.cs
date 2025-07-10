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
	[TestedType(typeof(SADDocumentWatermarkUserControl))]
	sealed class SADDocumentWatermarkUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new SADDocumentWatermarkCollection(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((SADDocumentWatermarkUserControl)control).MainGrid.ReadOnly;
		}
	}
}
