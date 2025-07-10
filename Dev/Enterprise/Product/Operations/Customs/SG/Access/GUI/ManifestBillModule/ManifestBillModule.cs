using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class ManifestBillModule : ASYCUDA.Module.ASYCUDAManifestBillModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.ASYCUDA.SGAccess.ManifestBill;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.ASYCUDA.SGAccess.ManifestBill);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ManifestBillFilterStripControl(GridCollection, (ManifestBillFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ManifestBillModuleCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ManifestBillFilterStrip();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var sendManifestsToCustomsMenuName = ResString.GetMultilingualString("F935DD3F-1DE9-4544-A762-E7350BAE82EF",
				"Send Import Manifests to Customs");

			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			var menuItem = new ZMenuItem(sendManifestsToCustomsMenuName, OnSendManifestsToCustoms_Click);
			menuItem.Caption = sendManifestsToCustomsMenuName;
			result.Add(menuItem);
			return result.ToArray();
		}

		void OnSendManifestsToCustoms_Click(object sender, EventArgs e)
		{
			var selectedBillsThatCanBeSend = GetSelectedBillsThatCanBeSend();
			if (selectedBillsThatCanBeSend.Length == 0)
			{
				Globals.Message.ShowError(SelectAtLeastOneBillNotSentToCustoms);
			}
			else
			{
				using (var mutexManager = new MutexManager(selectedBillsThatCanBeSend, (x) => x.SendToManifestMutex))
				{
					if (mutexManager.HasAquiredLockForAllBills || (mutexManager.SucessfulLockedBills.Any() && Globals.Message.Show(Res.GetString("{65CC206A-688C-43A9-B911-259A27A36458}", "One or more of the bills selected is currently being sent by another user. Do you want to send the rest?"), Res.GetString("{54EE5B03-AFAB-4204-8344-FF248D4F4CCC}", "BILL IS BEING SENT"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes))
					{
						ZFormModaliser.ShowDialogAndDispose(new MultiManifestBillSenderDialog(new MultiManifestBillSender(mutexManager.SucessfulLockedBills.Cast<AsycudaBill>())));
					}
					else if (!mutexManager.SucessfulLockedBills.Any())
					{
						Globals.Message.Show(string.Format(CultureInfo.CurrentCulture, "Messages are currently being generated and sent by {0}, please try again later.", mutexManager.GetMutexLockByInfo()), "Try later", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					}
				}
			}
		}

		AsycudaBill[] GetSelectedBillsThatCanBeSend()
		{
			Grid.SelectedElements.OfType<AsycudaBill>().ForEach(x =>
			{
				x.ReloadSafe();
				x.Header?.ReloadSafe();
			});
			return Grid.SelectedElements.OfType<AsycudaBill>().Where(x => !ASYCUDA.Business.MessageStatusCodeList.HasBeenSentCustoms(x.ABL_MessageStatus) && x.RegistrationNumber.IsEmpty && (x.Header?.IsImport ?? false)).ToArray();
		}

		internal static string SelectAtLeastOneBillNotSentToCustoms => Res.GetString("{24EF1C71-065B-4DF5-9E45-0EDE3485E208}", "Please select at least one Import Bill which hasn't been sent to Customs.");
	}
}
