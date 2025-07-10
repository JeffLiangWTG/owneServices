using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CancelPickApplicator : WhsOperationalActionMethodApplicator
	{
		public CancelPickApplicator(IFactoryService factoryService)
			: base(Res.GetString("95620816-b0f6-4c64-be5b-96bd7c166dca", "Cancel Picks")) // text used for logging
		{
			FactoryService = Argument.NotNull(factoryService, nameof(factoryService));
		}
		IFactoryService FactoryService { get; }

		const string OutputTextFormat = "{0} {1} - {2}";

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] picks)
		{
			log.SetSectionProgressMax(picks.Length);
			foreach (var selectedPick in picks.Cast<WhsPick>())
			{
				var pick = LoadPickInNewFactory(selectedPick);
				if (pick != null)
				{
					log.BumpSectionProgress();
					TryToCancelPickAndSave(pick, log);
				}
			}
		}

		WhsPick LoadPickInNewFactory(WhsPick selectedPick)
		{
			var factory = FactoryService.GetFactory<Func<BusinessObjectFactory>>().Invoke();
			var pick = factory.Load<WhsPick>(selectedPick.PK);
			return pick;
		}

		#region Cancel Pick

		void TryToCancelPickAndSave(WhsPick pick, IOperationalActionSectionLog log)
		{
			var pickLink = GetPickNoLink(pick);
			var errorMessage = pick.GetIsCancellableErrorMessage();
			if (string.IsNullOrEmpty(errorMessage))
			{
				if (pick.IsCartonising || pick.WP_IsCartonised)
				{
					LogWarning(log, pick, Res.GetString("66b761c5-9095-407b-879d-f61b13cf2bc4", $"Pick cannot be canceled as the Pick has already had Package Labels allocated, or is in the process of having Package Labels allocated in another instance."));
				}
				else
				{
					CancelPickAndSave(pick, log, pickLink);
				}
			}
			else
			{
				LogWarning(log, pick, errorMessage);
			}
		}

		void CancelPickAndSave(WhsPick pick, IOperationalActionSectionLog log, LogControllerLink pickLink)
		{
			var errorMessage = string.Empty;
			SaveAndHandleErrors(() =>
			{
				var canceled = pick.CancelCancellablePick();
				errorMessage = pick.GetErrors().ToMessageListString();
				if (string.IsNullOrEmpty(errorMessage))
				{
					if (canceled)
					{
						pick.Factory.Save();
						LogInformational(log, pick, Res.GetString("b4a4bfb6-a0b5-4f18-bae0-2ecda8b976aa", "was successfully canceled."));
					}
					else
					{
						LogWarning(log, pick, Res.GetString("215cf0ea-279d-47d3-bea9-50f18451a935", "could not be canceled. try to cancel manually."));
					}
				}
			}, log, OutputTextFormat, pick.HumanReadableName, pickLink, reattempts: 0); // should not reattempt save if it failed because retrying deletion will result in a misleading concurrency error.

			if (!string.IsNullOrEmpty(errorMessage))
			{
				LogWarning(log, pick, Res.GetString("e7ef5472-2530-4420-a032-d4556a5cf1db", "had the following errors when trying to cancel Pick:\r\n{0}", errorMessage));
			}
		}

		#endregion

		#region Validation

		public CancelPickApplicatorValdidation Validation => new CancelPickApplicatorValdidation(this);

		#endregion

		#region Log Helper

		static void LogWarning(IOperationalActionSectionLog log, WhsPick pick, string message)
		{
			var pickLink = GetPickNoLink(pick);
			log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pick.HumanReadableName, pickLink, message);
		}

		static void LogInformational(IOperationalActionSectionLog log, WhsPick pick, string message)
		{
			var pickLink = GetPickNoLink(pick);
			log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, pick.HumanReadableName, pickLink, message);
		}

		#endregion
	}

	#region CancelPickApplicatorValidation

	/// <summary>
	/// This class is unnecessary and only exists to satisfy the Z-test for IObsoleteValidation.
	/// </summary>
	public class CancelPickApplicatorValdidation : ZValidation
	{
		public CancelPickApplicatorValdidation(CancelPickApplicator parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType => typeof(CancelPickApplicator);

		public override void ValidateAll()
		{
		}
	}

	#endregion
}
