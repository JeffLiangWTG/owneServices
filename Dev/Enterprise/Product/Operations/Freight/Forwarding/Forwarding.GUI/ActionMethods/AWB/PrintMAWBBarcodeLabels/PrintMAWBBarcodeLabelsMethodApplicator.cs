using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.GUI.AWB;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class PrintMAWBBarcodeLabelsMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		public PrintMAWBBarcodeLabelsMethodApplicator(AWBPrintSettings settings, BusinessObjectFactory factory)
			: base(Res.GetString("7b7fd604-0adc-43f8-992a-ab408c29a207", "Print MAWB Barcode Labels Options"), factory)
		{
			onError = settings.OnErrorMessageError;
		}

		readonly ZString onError;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("b3d4f82e-4927-4c1c-8d62-6ba7b601640e", "No consols selected."));
			}
			else
			{
				PrintMAWBBarcodeLabels(log, targets);
			}
		}

		void PrintMAWBBarcodeLabels(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			List<ForwardingConsol> consols = targets.Cast<ForwardingConsol>().ToList();
			BulkMAWBLabelActions actions = new BulkMAWBLabelActions(Factory);
#if DEBUG
			if (Globals.IsTest)
			{
				SetBulkMAWBLabelActionSettingsForTest(actions);
			}
#endif
			if (ZFormModaliser.ShowDialogAndDispose(new PrintMAWBBarcodeLabelsActionMethodForm(actions)) == DialogResult.OK)
			{
				ValidateConsols(consols, actions.PackagesFromAWB);

				List<ForwardingConsol> nonAirConsols = new List<ForwardingConsol>();
				List<ForwardingConsol> consolsWithoutLabels = new List<ForwardingConsol>();
				List<ForwardingConsol> consolsWithErrors = new List<ForwardingConsol>();
				List<ForwardingConsol> consolsToProcess = new List<ForwardingConsol>();

				foreach (ForwardingConsol consol in consols)
				{
					if (!consol.IsAir)
					{
						nonAirConsols.Add(consol);
					}
					else if (consol.JK_TotalShipmentQuantity < 1)
					{
						consolsWithoutLabels.Add(consol);
					}
					else if (consol.HasErrors)
					{
						consolsWithErrors.Add(consol);
					}
					else
					{
						consolsToProcess.Add(consol);
					}
				}

				if (onError == AWBPrintSettings.Codes.Skip)
				{
					log.SetSectionProgressMax(consols.Count);

					foreach (ForwardingConsol consol in nonAirConsols)
					{
						string messageFormat = Res.GetString("ba4cc6cf-69c6-409c-8c95-e6d1fb37078a", "{0:G} is not an air consol and was skipped.");
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, messageFormat, consol.Hyperlink());
						log.BumpSectionProgress();
					}

					foreach (ForwardingConsol consol in consolsWithoutLabels)
					{
						string messageFormat = Res.GetString("2888b0c8-0623-40b1-9311-691bde8b6275", "{0:G} has no labels and was skipped.");
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, messageFormat, consol.Hyperlink());
						log.BumpSectionProgress();
					}

					foreach (ForwardingConsol consol in consolsWithErrors)
					{
						string messageFormat = Res.GetString("730d0ca4-7e0e-4ff5-ab53-3f7e94158583", "{0:G} has errors and was skipped.");
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, messageFormat, consol.Hyperlink());
						log.BumpSectionProgress();
					}

					if (consolsToProcess.Count > 0)
					{
						ProcessConsols(log, actions, consolsToProcess);
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Error,
							Res.GetString("ecf58bdc-90d8-4f1c-b319-c0f8172cd0e1", "All selected consols were skipped. See above for details."));
					}
				}
				else if (onError == AWBPrintSettings.Codes.Abort)
				{
					if (nonAirConsols.Count > 0)
					{
						string messageFormat = Res.GetString("d786afd8-0c14-49c4-bb40-21d408e0b7a1", "{0:G} is not an air consol.");
						nonAirConsols.ForEach(c => log.NotifyFormat(OperationalActionLogErrorLevel.Error, messageFormat, c.Hyperlink()));
					}

					if (consolsWithoutLabels.Count > 0)
					{
						string messageFormat = Res.GetString("287da1ab-a42d-4684-b266-8615a488ffa5", "{0:G} has no barcode labels.");
						consolsWithoutLabels.ForEach(c => log.NotifyFormat(OperationalActionLogErrorLevel.Error, messageFormat, c.Hyperlink()));
					}

					if (consolsWithErrors.Count > 0)
					{
						string messageFormat = Res.GetString("29baad9f-90d4-4dd6-b2e4-c052c7f336c9", "{0:G} has errors.");
						consolsWithErrors.ForEach(c => log.NotifyFormat(OperationalActionLogErrorLevel.Error, messageFormat, c.Hyperlink()));
					}

					if (consolsToProcess.Count == consols.Count)
					{
						log.SetSectionProgressMax(consolsToProcess.Count);
						ProcessConsols(log, actions, consolsToProcess);
					}
				}

				actions.SaveSettings();
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error,
					Res.GetString("9b619f82-a991-45e7-996b-6198451ae9e3", "Operation was canceled."));
			}
		}

		void ValidateConsols(List<ForwardingConsol> consols, bool validateAWBs)
		{
			foreach (ForwardingConsol consol in consols)
			{
				if (validateAWBs)
				{
					consol.PopulateAWB();
				}
				consol.MarkAsNeedingValidationIncludingChildren();
				consol.RunPreSaveValidation();
			}
		}

		void ProcessConsols(IOperationalActionSectionLog log, BulkMAWBLabelActions bulkActions, List<ForwardingConsol> consols)
		{
			foreach (ForwardingConsol consol in consols)
			{
				ConsolAWBActions consolActions = CreateAndDefaultConsolActions(consol, bulkActions);
				consolActions.PerformAllActions();
				string messageFormat = Res.GetString("3650b366-9aeb-4cda-8b80-21b1e01f907e", "{0:G} processed successfully.");
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, messageFormat, consol.Hyperlink());

				log.BumpSectionProgress();
			}
		}

		ConsolAWBActions CreateAndDefaultConsolActions(ForwardingConsol consol, BulkMAWBLabelActions bulkActions)
		{
			ConsolAWBActions consolActions = new ConsolAWBActions(consol, AWBActions.ActionsModeType.All);
			consolActions.PrintMasterAirWaybill = ZBool.False;
			consolActions.SendFWB = ZBool.False;
			consolActions.SendFHL = ZBool.False;
			consolActions.PrintBarcodeLabel = ZBool.True;
			consolActions.PrintHAWBBarcodeLabels = ZBool.False;
			consolActions.LabelRangeFrom = 1;
			consolActions.LabelRangeTo = (ZInt)consol.JK_TotalShipmentQuantity;
			consolActions.AWBLabelCopies = bulkActions.NumberOfCopies;
			consolActions.PrintOptionalInformation = bulkActions.PrintOptionalInformation;
			consolActions.FiveInchLabel = bulkActions.FiveInchLabel;
			consolActions.SixInchLabel = bulkActions.SixInchLabel;
			consolActions.LabelPrinter = bulkActions.AWBLabelPrinter;
			consolActions.AWBPackagesLabel = bulkActions.PackagesFromAWB;
			consolActions.ParentPackagesLabel = bulkActions.PackagesFromConsol;
			return consolActions;
		}

		#region SetBulkMAWBLabelActionsSettingsForTest
#if DEBUG

		void SetBulkMAWBLabelActionSettingsForTest(BulkMAWBLabelActions actions)
		{
			actions.PrintOptionalInformation = PrintOptionalInformation;
			actions.FiveInchLabel = FiveInchLabel;
			actions.SixInchLabel = SixInchLabel;
			actions.PackagesFromAWB = PackagesFromAWB;
			actions.PackagesFromConsol = PackagesFromConsol;
			actions.AWBLabelPrinter = AWBLabelPrinter;
			actions.NumberOfCopies = NumberOfCopies;
		}

		internal ZBool PrintOptionalInformation { get; set; }
		internal ZBool FiveInchLabel { get; set; }
		internal ZBool SixInchLabel { get; set; }
		internal ZBool PackagesFromAWB { get; set; }
		internal ZBool PackagesFromConsol { get; set; }
		internal ZGuid AWBLabelPrinter { get; set; }
		internal ZInt NumberOfCopies { get; set; }

#endif
		#endregion
	}
}
