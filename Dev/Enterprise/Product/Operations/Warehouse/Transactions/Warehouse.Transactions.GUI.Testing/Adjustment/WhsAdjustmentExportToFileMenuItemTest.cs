using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class WhsAdjustmentExportToFileMenuItemTest : WhsDocketExportToFileMenuItemTest<WhsAdjustment, Xsd.WhsDocket>
	{
		#region Overrides

		protected override WhsAdjustment GetNewDocket()
		{
			var adjustment = Helper.CreateWhsAdjustment(Client, Warehouse);
			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Adjustment is not finalized", true, adjustment.IsFinalised);
			return adjustment;
		}

		protected override WhsValueObjectDataAdapter<WhsAdjustment, Xsd.WhsDocket> GetNewAdapter()
		{
			return new WhsAdjustmentValueObjectDataAdapter();
		}

		protected override System.Windows.Forms.MenuItem GetNewMenuItem()
		{
			return new WhsAdjustmentExportToFileMenuItem(Docket, (WhsAdjustmentValueObjectDataAdapter)Adapter);
		}

		protected override Type GetExpectedExportDirectorType()
		{
			return typeof(WhsAdjustmentXmlExportToFileDirector);
		}

		#endregion
	}
}
