using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.GUI.Testing
{
	[TestedType(typeof(ASNRefreshOptionsControl))]
	sealed class ASNRefreshOptionsControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ASNRefreshOptionsConfigCollection(null, Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var configControl = (ASNRefreshOptionsControl)control;
			var grids = configControl.Controls.OfType<DataGrid>();

			return grids.All(c => c.ReadOnly);
		}
	}
}
