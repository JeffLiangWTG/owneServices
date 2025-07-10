using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Module
{
	public class PalletTransactionModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PalletTransaction; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.PalletTransaction);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PalletTransactionFilterControl(GridCollection, (PalletTransactionFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PalletTransactionFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new PkgPalletTransactionCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Packing; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PalletTransaction; }
		}

		protected override void ExportIntoAndOpenExcel()
		{
			base.ExportIntoAndOpenExcel();
			MarkExportedItemsAsProcessed(CollectionForExport.Cast<PkgPalletTransaction>(), Factory);
		}

#if DEBUG
		internal
#endif
		static void MarkExportedItemsAsProcessed(IEnumerable<PkgPalletTransaction> records, BusinessObjectFactory factory)
		{
			var message = Res.GetString("d74f1f42-b606-443b-91cb-cf78586dd04f", "Do you want to mark the {0} exported pallet transactions as Exported, so that they can be excluded from future imports?", records.Count());
			if (records.Any() && Globals.Message.Show(message, Res.GetString("92fe1919-07fe-48c8-b189-2a8c108610a5", "Pallet Transactions"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				foreach (var item in records)
				{
					item.KTR_Status = PalletTransactionStatusList.Codes.Processed;
				}

				factory.Save();
			}
		}
	}
}
