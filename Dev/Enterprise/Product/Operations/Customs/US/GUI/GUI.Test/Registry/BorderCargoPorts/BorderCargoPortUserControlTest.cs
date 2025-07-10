using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(BorderCargoPortUserControl))]
	sealed class BorderCargoPortUserControlTest : RegistryZUserControlTestCase
	{
		public void TestUserControlHasSpecificColumns()
		{
			using (var control = new BorderCargoPortUserControl())
			{
				var mainGrid = control.MainGrid;
				AssertEquals(3, mainGrid.ColumnStyles.Count);
				AssertContains("PortCode", mainGrid.ColumnStyles[0].ToString());
				AssertContains("CRProcess", mainGrid.ColumnStyles[1].ToString());
				AssertContains("Location", mainGrid.ColumnStyles[2].ToString());
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new BorderCargoPortCollection(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((BorderCargoPortUserControl)control).MainGrid.ReadOnly;
	}
}
