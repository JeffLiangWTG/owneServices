using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVConsignmentsToStandAloneDeclarationsForm : ZChildForm
	{
		public HVLVConsignmentsToStandAloneDeclarationsForm(HVLVConsignmentHeader header) : base(header)
		{
			InitializeComponent();
		}

		#region Buttons

		void ButtonConvert_Click(object sender, EventArgs e)
		{
			var convertToDeclarations = true;
			var consignments = GridConsignments.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>().Where(c => c.Convert).Select(selectedConsignment => selectedConsignment.Consignment);

			if (!consignments.Any() || ((HVLVConsignmentHeader)BusinessEntity).ConsignmentsConvertToStandAloneDeclaration.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("22aa8db6-43bb-452a-8e06-ee216f36677d", "No consignments selected to convert."));
				return;
			}

			convertToDeclarations = ConvertFreeTextAddressesIfNeeded(consignments);

			if (convertToDeclarations)
			{
				var sameConsigneeFoundAmongSelectedConsignments = consignments.Any(c => c.ConsignmentsBelongToSameConsigneeExcludingParent.Any(x => consignments.Any(y => y.PK == x.PK)));
				if (sameConsigneeFoundAmongSelectedConsignments)
				{
					convertToDeclarations = PromptMergeConsignments(consignments);
				}
			}

			if (convertToDeclarations)
			{
				ConvertStandAloneDeclarations(consignments);
			}
		}

		void ButtonCancel_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ButtonSelectOrDeselectAll_Click(object sender, EventArgs e)
		{
			var consignments = GridConsignments.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>();

			if (!consignments.Any())
			{
				Globals.Message.ShowError(Res.GetString("214761c0-206c-459e-a16d-3f428f614e6a", "There are no consignments to convert."));
			}
			else
			{
				var shouldSelectAll = consignments.ToList().Count != consignments.Where(c => c.Convert).ToList().Count;
				consignments.ForEach(c => c.Convert = shouldSelectAll);
			}
		}

		void ButtonSelectHeld_Click(object sender, EventArgs e)
		{
			var consignments = GridConsignments.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>();

			if (!consignments.Any())
			{
				Globals.Message.ShowError(Res.GetString("214761c0-206c-459e-a16d-3f428f614e6a", "There are no consignments to convert."));
			}
			else
			{
				foreach (var consignment in consignments)
				{
					var shouldSelectHeld = consignment.Consignment.ReleaseStatusForCurrentDirection == HVLVReleaseStatus.Held;
					consignment.Convert = shouldSelectHeld;
				}
			}
		}

		void ButtonSelectNoneReported_Click(object sender, EventArgs e)
		{
			var consignments = GridConsignments.List.Cast<HVLVConsignmentForStandAloneDeclarationConversionWrapper>();

			if (!consignments.Any())
			{
				Globals.Message.ShowError(Res.GetString("214761c0-206c-459e-a16d-3f428f614e6a", "There are no consignments to convert."));
			}
			else
			{
				foreach (var consignment in consignments)
				{
					var shouldSelectNone = consignment.Consignment.ReleaseStatusForCurrentDirection == HVLVReleaseStatus.None;
					consignment.Convert = shouldSelectNone;
				}
			}
		}

		bool ConvertFreeTextAddressesIfNeeded(IEnumerable<HVLVConsignment> consignments)
		{
			var shouldContinue = true;

			if (consignments.Any(c => !c.ConsigneeIsOrganisation || !c.ShipperIsOrganisation)
				&& ConvertToStandAloneDeclarationHelper.PromptToMatchOrCreateConsigneeAndShipperOrgsIfNotExist())
			{
				shouldContinue = false;
				var newFactoryInCaseUserCancelsAddressConversion = new CargoWise.EntityFramework.BusinessObjectFactory();
				var conversion = new FreeTextAddressConversion<HVLVConsignment>(newFactoryInCaseUserCancelsAddressConversion);
				conversion.AddToList(consignments);

				using (var freeTextAddressConversionForm = new FreeTextAddressConversionForm<HVLVConsignment>(conversion))
				{
					ZFormModaliser.ShowDialogWithoutDispose(freeTextAddressConversionForm);

					if (freeTextAddressConversionForm.LastSaveSucceeded)
					{
						Globals.Message.Show(Res.GetString("322e5024-ff6b-4694-807e-7cb464baf792", "New organization(s) have been linked to the selected consignments. Please click 'Convert' again to create Stand Alone Declarations."));
					}
				}
			}

			return shouldContinue;
		}

		bool PromptMergeConsignments(IEnumerable<HVLVConsignment> consignments)
		{
			var mergeConsignmentDeclarationsPrompt = ResString.GetMultilingualString("6d71fc04-9554-4501-899d-de56b35fffce", "Same Consignee details are found for the selected Consignment(s), would you like to merge and create one stand alone declaration for the same Consignee?");
			var mergeConsignmentDeclarationsCaption = ResString.GetMultilingualString("96eaf127-0dcb-4f80-b1a0-ef8a61661b9c", "Merge Declaration");
			var promptResult = Globals.Message.Show(mergeConsignmentDeclarationsPrompt, mergeConsignmentDeclarationsCaption, MessageBoxButtons.YesNo, DialogResult.No);

			if (promptResult == DialogResult.Yes)
			{
				MergeAndConvertToStandAloneDeclarations(consignments);
				return false;
			}

			return true;
		}

		void ConvertStandAloneDeclarations(IEnumerable<HVLVConsignment> consignments, bool needMerge = false)
		{
			var tokenSource = new CancellationTokenSource();
			var progressForm = new HVLVCancelableMinimisableProgressForm(this, tokenSource.Cancel);
			progressForm.ShowModalTo(this);

			var helper = new ConvertToStandAloneDeclarationHelper();
			if (helper.ConvertToStandAloneDeclarations(consignments, ProgressUpdateCallBack(progressForm), tokenSource))
			{
				FireSaveButton();
				Globals.Message.Show(Res.GetString("e71beae7-3922-476a-bae3-294911dbc272", "Consignments have been converted to Stand Alone Declarations."));
			}

			progressForm.Close();
			Close();
		}

		Action<string, string, int> ProgressUpdateCallBack(HVLVCancelableMinimisableProgressForm progressForm)
		{
			return (string caption, string message, int progress) =>
			{
				if (progressForm.WindowState != FormWindowState.Minimized)
				{
					if (!string.IsNullOrEmpty(caption))
					{
						progressForm.Text = caption;
					}

					if (!string.IsNullOrEmpty(message))
					{
						progressForm.SetStatusAndPercentComplete(message, progress);
					}
				}
			};
		}

		void MergeAndConvertToStandAloneDeclarations(IEnumerable<HVLVConsignment> consignments)
		{
			var helper = new ConvertToStandAloneDeclarationHelper();
			helper.MergeAndConvertToStandAloneDeclarations(consignments);

			FireSaveButton();

			Close();
			Globals.Message.Show(Res.GetString("a21cb904-76ac-4412-b7d6-2da40e2d32eb", "Consignments have been converted to Stand Alone Declarations."));
		}

		#endregion

		public override string FormVerb => string.Empty;
	}
}
