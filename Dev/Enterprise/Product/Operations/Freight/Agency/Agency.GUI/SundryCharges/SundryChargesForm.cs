using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class SundryChargesForm : ZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public SundryChargesForm()
		{
			InitializeComponent();
		}

		public SundryChargesForm(SundryCharges charges)
			: base(charges)
		{
			InitializeComponent();

			PlugIns.AddJobInvoicing(charges.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);

			ZFormPostingButtonsStrategy.SetupPosting(this, this.postingButtons);
		}

		#region Form Overrides

		protected override ZTabControl TopLevelTabControl
		{
			get { return mainTabControl; }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			SundryCharges sundry = (SundryCharges)CurrentDataItem;
			ContinueWithSave result;

			if (sundry.BillToParty == null)
			{
				// continue, will be stopped by validation anyway.
				result = base.ValidateAndSave();
			}
			else
			{
				AgencySundryMutex mutex = AgencySundryMutex.New(sundry.BillToParty);

				mutex.Lock();
				if (mutex.HasLock)
				{
					try
					{
						sundry.ForceResetOverlappingJobs();
						result = base.ValidateAndSave();
					}
					finally
					{
						mutex.Unlock();
					}
				}
				else
				{
					result = ContinueWithSave.No;
					Globals.Message.ShowError(mutex.GetFriendlyMessage());
				}
			}

			return result;
		}

		public override string FormCaption
		{
			get
			{
				SundryCharges sundry = (SundryCharges)CurrentDataItem;

				if (sundry == null || sundry.D4_JobNumber.IsEmpty)
				{
					return Res.GetString("SundryChargesForm|Caption", "Sundry Charges");
				}
				else
				{
					return Res.GetString("SundryChargesForm|CaptionDetail", "Sundry Charges: {0}", sundry.D4_JobNumber);
				}
			}
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion
	}
}


