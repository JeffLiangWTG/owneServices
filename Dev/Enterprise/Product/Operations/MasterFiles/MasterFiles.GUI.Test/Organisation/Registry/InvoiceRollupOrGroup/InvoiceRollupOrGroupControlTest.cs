using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(InvoiceRollupOrGroupControl))]
	sealed class InvoiceRollupOrGroupControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			InvoiceRollupOrGroupCollection testCollection = new InvoiceRollupOrGroupCollection();
			testCollection.AddNew();
			return testCollection;
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((InvoiceRollupOrGroupControl)control).GroupChargesGrid.ReadOnly &&
				((InvoiceRollupOrGroupControl)control).JobTypeDropEdit.ReadOnly &&
				((InvoiceRollupOrGroupControl)control).DirectionDropEdit.ReadOnly &&
				((InvoiceRollupOrGroupControl)control).ModeDropEdit.ReadOnly &&
				((InvoiceRollupOrGroupControl)control).DisplayDropEdit.ReadOnly &&
				((InvoiceRollupOrGroupControl)control).StyleDropEdit.ReadOnly &&
				((InvoiceRollupOrGroupControl)control).InvoiceDropEdit.ReadOnly &&
				((InvoiceRollupOrGroupControl)control).PostingDropEdit.ReadOnly;
		}
	}
}
