using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class GeneratePickActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public GeneratePickActionMethodApplicator()
			: base(Res.GetString("2b1cd58d-c03e-4e38-bc35-5b098750765a", "Generate Pick")) // text used for logging
		{
		}

		#region Generate Pick

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] dockets)
		{
			log.SetSectionProgressMax(dockets.Length);

			foreach (var pickableDocket in dockets.Cast<WhsPickableDocket>().OrderBy(d => d.WD_PickPriority))
			{
				if (pickableDocket.WD_DocketStatus != DocketStatus.Codes.Entered)
				{
					LogUnpickableOrderDueToNonEnteredStatus(pickableDocket, log);
				}
				else if (pickableDocket.WD_PickOption != WhsPickOption.Codes.Auto)
				{
					LogUnpickableOrderDueToNonAutoPickOption(pickableDocket, log);
				}
				else
				{
					var newFactory = new BusinessObjectFactory();
					var pickableDocketToProcess = newFactory.Load<WhsPickableDocket>(pickableDocket.PK) ?? pickableDocket;

#if DEBUG
					newFactory.Saving += new BusinessObjectFactory.SavingEventHandler(OnNewFactorySaving);
#endif

					var logger = new OperationalActionSectionLogger(log, pickableDocketToProcess);
					GeneratePickManager.GeneratePick(pickableDocketToProcess, logger);
				}

				log.BumpSectionProgress();
			}
		}

#if DEBUG
		public void OnNewFactorySaving(BusinessObjectFactory factory)
		{
			NewFactorySaving?.Invoke(this, new EventArgs());
		}

		public event EventHandler NewFactorySaving;
#endif

		void LogUnpickableOrderDueToNonEnteredStatus(WhsPickableDocket pickableDocket, IOperationalActionSectionLog log)
		{
			var docketLink = GetDocketIdLink(pickableDocket);

			var message = Res.GetString("4A585C59-6FD6-4F2B-9FB5-C56695538BD3", "{0} {1} is {2} ({3}).", pickableDocket.Description, "{0}", pickableDocket.WD_DocketStatusDescription, pickableDocket.WD_DocketStatus);

			log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
		}

		void LogUnpickableOrderDueToNonAutoPickOption(WhsPickableDocket pickableDocket, IOperationalActionSectionLog log)
		{
			var docketLink = GetDocketIdLink(pickableDocket);

			var message = Res.GetString("FD0838EE-9309-473A-A49F-D2C1940CDA18", "{0} {1} does not have Pick Option AUT.", pickableDocket.Description, "{0}");

			log.NotifyFormat(OperationalActionLogErrorLevel.Error, message, docketLink);
		}

		#endregion

		#region OperationalActionSectionLogger

		class OperationalActionSectionLogger : IGeneratePickLogger
		{
			public OperationalActionSectionLogger(IOperationalActionSectionLog log, WhsPickableDocket pickableDocket)
			{
				OperationalActionSectionLog = log;
				PickableDocket = pickableDocket;
			}
			readonly IOperationalActionSectionLog OperationalActionSectionLog;
			readonly WhsPickableDocket PickableDocket;

			#region IGeneratePickLogger

			void IGeneratePickLogger.LogWarning(string message)
			{
				OperationalActionSectionLog.Notify(OperationalActionLogErrorLevel.Warning, message);
			}

			void IGeneratePickLogger.LogError(WhsPick pick, string message) => Log(message + GenerateErrorMessageForPick(pick));
			void IGeneratePickLogger.LogSaveConcurrencyException(WhsPick pick, string docketID)
				=> Log(Res.GetString("12906f5f-ae07-4a2d-87aa-bd33b99d1237", "Order: {0} was modified by another user while you are running operational action. Please run the operational action on this order again.", docketID) + GenerateErrorMessageForPick(pick));

			void IGeneratePickLogger.LogError(string message) => Log(message);

			#endregion

			#region Log

			void Log(string message)
			{
				var docketLink = GetDocketIdLink(PickableDocket);

				var messageNotify = Res.GetString(
					"286E011E-801C-4B9C-BF3B-8EA1FBF02FA3",
					"Attempted Picking of {0} {1} failed -- {2}",
					PickableDocket.Description, "{0}", message // Hyperlink must be injected directly from NotifyFormat()
					);

				OperationalActionSectionLog.NotifyFormat(OperationalActionLogErrorLevel.Error, messageNotify, docketLink);
			}

			#endregion

			#region GenerateErrorMessageForPickHasError

			string GenerateErrorMessageForPick(WhsPick pick)
			{
				var errorMessage = string.Empty;
				if (pick.NotificationsIncludingChildren.Count() > 0)
				{
					const char bullet = (char)8226;
					var separator = System.Environment.NewLine + "  " + bullet + " ";
					var fatalNotifications = new ZNotificationCollector(pick, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetFatalNotifications();
					if (fatalNotifications.Count() > 0)
					{
						errorMessage = separator + string.Join(separator, fatalNotifications.GetUniqueMessageList());
					}
				}
				return errorMessage;
			}

			#endregion
		}

		#endregion
	}
}
