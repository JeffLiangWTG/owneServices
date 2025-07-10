using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class FinalisePicksActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public FinalisePicksActionMethodApplicator()
			: base(Res.GetString("07b1a252-993f-418d-a54f-b2d8461b6ea7", "Finalize Picks")) // text used for logging
		{
		}

		const string OutputTextFormat = "{0} {1} - {2}"; // eg. Pick 123 - Was Finalised ok.
		const string OutputTextFormat_Error = "{0} {1} - {2}\r\n{3}";

		#region Finalise Picks

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] picks)
		{
			log.SetSectionProgressMax(picks.Length);

			foreach (var pick in picks.Cast<WhsPick>())
			{
				log.BumpSectionProgress();
				AddFetchHints(pick);
				TryToFinalise(pick, log);
			}
		}

		void AddFetchHints(WhsPick pick)
		{
			pick.Factory.AddFetchHint(WhsOrderStatusViewSchema.Instance, new ZQuery(WhsOrderStatusViewSchema.PK, pick.Orders.Select(o => o.PK)));
		}

		void TryToFinalise(WhsPick pick, IOperationalActionSectionLog log)
		{
			var pickLink = GetPickNoLink(pick);
			var otherFactory = new BusinessObjectFactory(); // using separate factory for each order to minimise amount of reloaded / cached objects in a factory.
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			if (pickInOtherFactory.IsCancelled)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pickInOtherFactory.HumanReadableName, pickLink, Res.GetString("ddbddb57-91af-46ef-86f5-bfdab319651e", "is canceled and cannot be finalized."));
			}
			else if (pickInOtherFactory.IsFinalised)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pickInOtherFactory.HumanReadableName, pickLink, Res.GetString("ca52a92b-a319-4f97-8fad-422a5c4c9e43", "is already finalized."));
			}
			else if (pickInOtherFactory.Orders.Count == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pickInOtherFactory.HumanReadableName, pickLink, Res.GetString("02928247-d4c2-4b0a-83b6-4daa026a5769", "has no Orders attached and cannot be finalized."));
			}
			else if (pickInOtherFactory.Orders.Cast<WhsPickableDocket>().Any(o => !o.IsFinalised) && !WarehouseDataRegistry.Instance.AutoFinalizeOrders.Value)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pickInOtherFactory.HumanReadableName, pickLink, Res.GetString("7a55b0d2-4c14-4d4d-b7bd-db15acca0519", "has non-finalized Orders. Finalize the Orders before trying to finalize the Pick."));
			}
			else
			{
				FinalisePick(pickInOtherFactory, log, pickLink);

				if (pickInOtherFactory.IsFinalised)
				{
					FinalizeAnotherOrderAndPickForTest();
					ModifyOrderAndPickWithFactoryForTest(otherFactory);

					SaveAndHandleErrors(() =>
					{
						try
						{
							otherFactory.Save();
						}
						catch (Exception ex) when (ex is ZSaveConcurrencyException || ex is ZConcurrencyCheckFailureException)
						{
							if (ex.IsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectTriggerException())
							{
								LogWarning(log, pickLink, pickInOtherFactory, WhsExceptionHandler.WhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectMsg_ForServiceTask);
							}
							else if (ex is ZSaveConcurrencyException)
							{
								var message = Res.GetString("15851632-5D4C-450E-BE4F-1DCE97E3DF9F", "While you were editing your data, another user modified it. Your changes cannot be saved because they may conflict with the other user's changes. Please try running this operational action again.");
								LogWarning(log, pickLink, pickInOtherFactory, message);
							}
						}
					}, log, OutputTextFormat, pickInOtherFactory.HumanReadableName, pickLink);
				}
			}
		}

		static void LogWarning(IOperationalActionSectionLog log, LogControllerLink pickLink, WhsPick pick, string message)
		{
			log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
				OutputTextFormat,
				pick.HumanReadableName,
				pickLink,
				message);
		}

		void FinalisePick(WhsPick pick, IOperationalActionSectionLog log, LogControllerLink pickLink)
		{
			if (WarehouseDataRegistry.Instance.AutoFinalizeOrders.Value)
			{
				pick.FinaliseAllOrders();
			}

			pick.IsAlterPick = true; // necessary to force validation of release lines
			pick.FinalisePick();
			if (pick.IsFinalised)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, pick.HumanReadableName, pickLink, Res.GetString("e96d16bd-9f0d-4b12-9146-00563893ce43", "was successfully finalized."));
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat_Error, pick.HumanReadableName, pickLink, Res.GetString("13abd964-f74d-4a27-be8e-bb73aae79538", "could not be auto-finalized. It must be manually finalized."), new WhsPickFinalisationErrorReportingHelper(pick).ReportPickFinalisationErrorMessage().Trim());
			}
		}

		#endregion

		partial void FinalizeAnotherOrderAndPickForTest();
		partial void ModifyOrderAndPickWithFactoryForTest(BusinessObjectFactory factory);
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class FinalisePicksActionMethodApplicator
	{
		partial void FinalizeAnotherOrderAndPickForTest()
		{
			FinalizeOrderAndPickForTest?.Invoke();
		}
		public Action FinalizeOrderAndPickForTest;

		partial void ModifyOrderAndPickWithFactoryForTest(BusinessObjectFactory factory)
		{
			SetupOrderAndPickWithFactoryForTest?.Invoke(factory);
		}
		public Action<BusinessObjectFactory> SetupOrderAndPickWithFactoryForTest;
	}
}

#endif
#endregion
