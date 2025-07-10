using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
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
	public class PrintFinalMasterMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		public PrintFinalMasterMethodApplicator(AWBPrintSettings settings, BusinessObjectFactory factory)
			: base(Res.GetString("304431d2-189f-442e-8e8f-53396de84017", "Print Final Master Options"), factory)
		{
			onErrorMessageError = settings.OnErrorMessageError;
		}

		readonly ZString onErrorMessageError;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("7e1a4db2-dd5e-4030-b8b8-49a513dfd013", "No consols selected."));
			}
			else
			{
				RunPrintFinalMaster(log, targets);
			}
		}

		void RunPrintFinalMaster(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			BulkConsolAWBActions actions = new BulkConsolAWBActions(Factory);
#if DEBUG
			if (Globals.IsTest)
			{
				SetBulkActionsSettingsForTest(actions);
			}
#endif
			if (ZFormModaliser.ShowDialogAndDispose(new PrintFinalMasterActionMethodForm(actions, onErrorMessageError)) == DialogResult.OK)
			{
				var consols = targets.Cast<ForwardingConsol>().ToList();
				var nonAirConsols = new List<ForwardingConsol>();
				var consolAWBActions = new List<ConsolAWBActions>();

				ConsolAWBFetchHelper.AddFetchHintsForConsols(consols);
				foreach (ForwardingConsol consol in consols)
				{
					if (!consol.IsAir)
					{
						nonAirConsols.Add(consol);
					}
					else
					{
						var consolActions = CreateAndDefaultConsolActions(consol, actions);

						if (consolActions.SendFHL)
						{
							consolActions.ValidateShipmentsToSend();
						}

						consolAWBActions.Add(consolActions);
					}
				}

				if (onErrorMessageError == AWBPrintSettings.Codes.Skip)
				{
					log.SetSectionProgressMax(consols.Count);

					foreach (ForwardingConsol consol in nonAirConsols)
					{
						string messageFormat = Res.GetString("3807ddbf-d453-4494-a026-ddba19f2620a", "{0:G} is not an air consol and was skipped.");
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, messageFormat, consol.Hyperlink());
						log.BumpSectionProgress();
					}

					ProcessConsols(log, consolAWBActions, actions);

					actions.SaveSettings();
				}
				else if (onErrorMessageError == AWBPrintSettings.Codes.Abort)
				{
					if (nonAirConsols.Count > 0)
					{
						string messageFormat = Res.GetString("35f406a7-70b7-4f4a-a07c-1672728026e0", "{0:G} is not an air consol.");
						nonAirConsols.ForEach(c => log.NotifyFormat(OperationalActionLogErrorLevel.Error, messageFormat, c.Hyperlink()));
					}

					if (consolAWBActions.Count == consols.Count)
					{
						bool errorsExist = false;

						foreach (var consolActions in consolAWBActions)
						{
							bool consolHasPrintErrors = !ValidateAndLogAWBType(log, consolActions);
							bool consolHasMessagingErrors = !ValidateAndLogMessaging(log, consolActions);
							bool consolHasErrors = consolHasPrintErrors || consolHasMessagingErrors;

							if (consolHasErrors || ConsolHeaderHasErrors(consolActions))
							{
								errorsExist = true;
								string messageFormat = Res.GetString("a44e8e03-ec1d-491c-bc12-a2d5064102ce", "{0:G} has errors and/or message errors.");
								log.NotifyFormat(OperationalActionLogErrorLevel.Error, messageFormat, consolActions.Consol.Hyperlink());
							}
						}

						if (!errorsExist)
						{
							log.SetSectionProgressMax(consols.Count);
							ProcessConsols(log, consolAWBActions, actions);
						}
						else
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Error,
								Res.GetString("1572a2b2-a62f-46e7-80ba-9e64e98ea74a", "Operation failed. See errors above."));
						}
					}

					actions.SaveSettings();
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error,
					Res.GetString("8da7ceb3-815f-43c1-8f38-e7894601ab9d", "Operation was canceled."));
			}
		}

		static bool ConsolHeaderHasErrors(ConsolAWBActions action)
		{
			var result = false;

			if (!action.SendFWB)
			{
				action.ValidateConsolExportAWBHeader();
				result = action.SendFWBInfo.HasErrors() || action.SendFHLInfo.HasErrors();
			}

			return result;
		}

		ConsolAWBActions CreateAndDefaultConsolActions(ForwardingConsol consol, BulkConsolAWBActions bulkActions)
		{
			ConsolAWBActions consolActions = new ConsolAWBActions(consol, AWBActions.ActionsModeType.All);
			consolActions.PrintMasterAirWaybill = bulkActions.PrintMasterAirWaybill;
			consolActions.MAWBPrinter = bulkActions.MAWBPrinter;
			consolActions.SendFWB = bulkActions.SendFWB;
			consolActions.SendFHL = bulkActions.SendFHL;
			consolActions.IncludeSecurityDeclaration = bulkActions.IncludeSecurityDeclaration;
			consolActions.CarrierAWB = bulkActions.CarrierAWB;
			consolActions.NeutralAWB = bulkActions.NeutralAWB;
			consolActions.LaserAWB = bulkActions.LaserAWB;
			consolActions.PrintBarcodeLabel = ZBool.False;
			consolActions.PrintHAWBBarcodeLabels = ZBool.False;
			consolActions.PrintConsignmentSecurityDeclaration = bulkActions.PrintConsignmentSecurityDeclaration;
			return consolActions;
		}

		void LogNotifications(IOperationalActionSectionLog log, ZPropertyInfo info)
		{
			foreach (INotification notification in info.Notifications)
			{
				if (notification.Type == CargoWise.ComponentModel.NotificationType.Error)
				{
					OperationalActionLogErrorLevel errorLevel = onErrorMessageError == AWBPrintSettings.Codes.Skip ? OperationalActionLogErrorLevel.Warning : OperationalActionLogErrorLevel.Error;
					log.NotifyFormat(errorLevel, notification.Message);
				}
			}
		}

		void ProcessConsols(IOperationalActionSectionLog log, IEnumerable<ConsolAWBActions> consolAWBActions, BulkConsolAWBActions bulkActions)
		{
			int consolsProcessed = 0;
			var consolActionsToProcess = new List<ConsolAWBActions>();

			foreach (ConsolAWBActions actions in consolAWBActions)
			{
				bool consolSkipped = ExtraProcessingForSkipping(log, actions.Consol, bulkActions, actions);

				if (!consolSkipped)
				{
					consolActionsToProcess.Add(actions);
				}

				log.BumpSectionProgress();
			}

			foreach (ConsolAWBActions consolActions in consolActionsToProcess)
			{
				consolActions.PerformAllActions();
				string messageFormat = Res.GetString("ace758b7-8a6a-498f-9a95-5cb1b3264d5b", "{0:G} processed successfully.");
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, messageFormat, consolActions.Consol.Hyperlink());

				if (consolActions.PrintMasterAirWaybill)
				{
					consolActions.Consol.UpdateAWBPrinted();
				}

				consolsProcessed++;
			}

			if (consolsProcessed == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error,
					Res.GetString("625ea804-977f-4036-b947-0075e765cfcc", "All selected consols were skipped. See above for details."));
			}
		}

		bool ExtraProcessingForSkipping(IOperationalActionSectionLog log, ForwardingConsol consol, BulkConsolAWBActions bulkActions, ConsolAWBActions consolActions)
		{
			if (onErrorMessageError == AWBPrintSettings.Codes.Skip)
			{
				if (!ValidateAndLogAWBType(log, consolActions))
				{
					string messageFormat = Res.GetString("af6349d7-e8b7-49cd-ac2a-4376018aac32", "{0:G} was skipped for the above reasons.");
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, messageFormat, consol.Hyperlink());
					return true;
				}

				if (!ValidateAndLogMessaging(log, consolActions))
				{
					if (bulkActions.AllowPrintWithMessageErrors && bulkActions.PrintMasterAirWaybill)
					{
						consolActions.SendFWB = ZBool.False;
						consolActions.SendFHL = ZBool.False;
						string messageFormat = Res.GetString("c1618ee8-b9bc-4025-a128-97eebe6cd819", "{0:G} has skipped electronic messaging for the above reasons.");
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, messageFormat, consol.Hyperlink());
					}
					else
					{
						string messageFormat = Res.GetString("6236816f-23bc-48aa-9ea5-01b2b3477097", "{0:G} was skipped for the above reasons.");
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, messageFormat, consol.Hyperlink());
						return true;
					}
				}
				else if (!bulkActions.AllowPrintWithMessageErrors && ConsolHeaderHasErrors(consolActions))
				{
					string messageFormat = Res.GetString("c78af29d-ff74-4417-a7dd-fe2ee09576ae", "{0:G} has errors/message errors and was skipped.");
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, messageFormat, consol.Hyperlink());
					return true;
				}
			}

			return false;
		}

		bool ValidateAndLogAWBType(IOperationalActionSectionLog log, ConsolAWBActions consolActions)
		{
			if (consolActions.PrintMasterAirWaybill)
			{
				consolActions.ValidateNeutralAWB();
				consolActions.ValidateCarrierAWB();
				consolActions.ValidateLaserAWB();
				LogNotifications(log, consolActions.NeutralAWBInfo);
				LogNotifications(log, consolActions.CarrierAWBInfo);
				LogNotifications(log, consolActions.LaserAWBInfo);
				return !consolActions.NeutralAWBInfo.HasErrors() && !consolActions.CarrierAWBInfo.HasErrors() && !consolActions.LaserAWBInfo.HasErrors();
			}

			return true;
		}

		bool ValidateAndLogMessaging(IOperationalActionSectionLog log, ConsolAWBActions consolActions)
		{
			var result = true;

			if (consolActions.SendFWB)
			{
				LogNotifications(log, consolActions.SendFWBInfo);
				LogNotifications(log, consolActions.SendFHLInfo);
				LogNotifications(log, consolActions.IncludeSecurityDeclarationInfo);
				result = !consolActions.SendFWBInfo.HasErrors() && !consolActions.SendFHLInfo.HasErrors() && !consolActions.IncludeSecurityDeclarationInfo.HasErrors();
			}

			LogNotifications(log, consolActions.PrintConsignmentSecurityDeclarationInfo);
			return result && !consolActions.PrintConsignmentSecurityDeclarationInfo.HasErrors();
		}

		#region SetBulkActionsSettingsForTest
#if DEBUG

		void SetBulkActionsSettingsForTest(BulkConsolAWBActions actions)
		{
			actions.PrintMasterAirWaybill = PrintMasterAirWaybill;
			actions.NeutralAWB = NeutralAWB;
			actions.CarrierAWB = CarrierAWB;
			actions.LaserAWB = LaserAWB;
			actions.MAWBPrinter = MAWBPrinter;
			actions.SendFWB = SendFWB;
			actions.SendFHL = SendFHL;
			actions.AllowPrintWithMessageErrors = AllowPrintWithMessageErrors;
			actions.PrintConsignmentSecurityDeclaration = PrintConsignmentSecurityDeclaration;
		}

		internal ZBool PrintMasterAirWaybill { get; set; }
		internal ZBool NeutralAWB { get; set; }
		internal ZBool CarrierAWB { get; set; }
		internal ZBool LaserAWB { get; set; }
		internal ZGuid MAWBPrinter { get; set; }
		internal ZBool SendFWB { get; set; }
		internal ZBool SendFHL { get; set; }
		internal ZBool AllowPrintWithMessageErrors { get; set; }
		internal ZBool PrintConsignmentSecurityDeclaration { get; set; }

#endif
		#endregion
	}
}
