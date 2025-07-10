using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public class InvoiceAttacher : ZRecordAttacher
	{
		public InvoiceAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList)
			: base(destinationCollection, findBoxList, ModuleIDs.CommercialInvoice)
		{
		}

		protected override void ShowCore(IZForm formToShowModalTo)
		{
			if (DestinationCollection != null && !((IBusinessObjectCollectionInternals)DestinationCollection).MastersAreDeleted)
			{
				if (DestinationCollection.ReadOnly)
				{
					Globals.Message.ShowInformation(Res.GetString("7054b9d3-4e08-4504-bf1a-93856fe2c828", "Sorry, Commercial Invoices can be viewed but not attached."), Res.GetString("26e2c24e-8899-4fa1-8c8d-875d58443c24", "Cannot attach..."));
				}
				else
				{
					if (ShouldShow((ZForm)formToShowModalTo))
					{
						base.ShowCore(formToShowModalTo);
					}
				}
			}
		}

		protected internal bool ShouldShow(ZForm formToShowModalTo)
		{
			return !NeedToSave ||
					(ShowConfirmationForSaveBeforeAttach() == DialogResult.Yes &&
					formToShowModalTo.FireSaveButton() == ContinueWithSave.Yes);
		}

		protected bool NeedToSave
		{
			get { return DestinationCollection.HasChanges; }
		}

		DialogResult ShowConfirmationForSaveBeforeAttach()
		{
			string caption = Res.GetString("aebd8f91-9ad1-416b-83e5-2a2668f6bae5", "Save Confirmation");
			string message = Res.GetString("6660c53b-2c34-4bf8-b390-327b3dcd9280", "The form must be saved before an Invoice can be attached. Do you wish to save the form?");
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);
		}

		protected internal bool AttachCoreInternal(BusinessObject bizO, List<BusinessObject> listToBulkAdd) => AttachCore(bizO, listToBulkAdd);

		protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
		{
			var pk = bizO.PK;
			bool result = base.AttachCore(bizO, listToBulkAdd);
			if (result)
			{
				var invoiceHeader = DestinationCollection.Factory.Load<BaseJobComInvoiceHeader>(pk);
				if (invoiceHeader != null)
				{
					invoiceHeader.RefreshDefaultsWhenInvoiceAttachedToDeclarationRequired = true;
					if (temporarySetupCachedConvertion == null)
					{
						temporarySetupCachedConvertion = UnitConverter.TemporarySetupCachedConvertion(DestinationCollection.Factory);
					}
				}
			}
			return result;
		}
		protected internal void AttachItemsCoreInternal(IBusinessObjectCollection destinationCollection, IEnumerable<BusinessObject> list) => AttachItemsCore(destinationCollection, list);

		protected override void AttachItemsCore(IBusinessObjectCollection destinationCollection, IEnumerable<BusinessObject> list)
		{
			base.AttachItemsCore(destinationCollection, list);
			ReCalculateGoodsDescription_ByEnableAutoTariffDescriptionPopulation_EnableCustomsDeclaration();

			void ReCalculateGoodsDescription_ByEnableAutoTariffDescriptionPopulation_EnableCustomsDeclaration()
			{
				list.Cast<BaseJobComInvoiceHeader>().SelectMany(header => header.InvoiceLines).Cast<BaseJobComInvoiceLine>().ForEach(
				line =>
				{
					line.ReCalculateTariffDescriptionWhenAttachedToDec();
				});
			}
		}

		protected override bool CheckAttaching(List<BusinessObject> businessObjectsToAttach)
		{
			var invoicesToSkip = new List<BaseJobComInvoiceHeader>();
			foreach (BaseJobComInvoiceHeader invoice in businessObjectsToAttach.ToArray())
			{
				if (((ICustomsFileParent)invoice).IsLocked)
				{
					invoicesToSkip.Add(invoice);
				}
			}

			if (invoicesToSkip.Count > 0)
			{
				string caption = Res.GetString("4EE1630B-52A0-49E8-A3EB-943D1E37E482", "Attaching Commercial Invoices...");
				string message = Res.GetString("B44C4EA2-D492-46EB-89AF-60248AEB636C",
					@"The following commercial invoices are locked and cannot be attached to the declaration until they have been Unlocked:
{0}",
					string.Join(System.Environment.NewLine, invoicesToSkip.Select(x => x.HumanReadableName)));

				if (Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel) == DialogResult.Cancel)
				{
					return false;
				}

				foreach (var consol in invoicesToSkip)
				{
					businessObjectsToAttach.Remove(consol);
				}
			}

			return businessObjectsToAttach.Count > 0;
		}

		IDisposable temporarySetupCachedConvertion;

		protected override void OnAttached()
		{
			base.OnAttached();
			if (temporarySetupCachedConvertion != null)
			{
				temporarySetupCachedConvertion.Dispose();
				temporarySetupCachedConvertion = null;
			}
		}
	}
}
