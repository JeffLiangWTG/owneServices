using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.LocalCartage.GUI
{
	static class PortTransportFormExceptionHandler
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTriggerID = "TriggerLikelyConcurrencyError: Attempted to insert duplicate suffix on cartage job leg(s).";

		public static string JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTriggerMessageForUser => Res.GetString("38fe5977-dc40-4378-9601-aa3f2a78ec17", "Attempted to insert duplicate suffix on cartage job leg(s). Try to close and re-open the form.");

		public static bool HandleSaveExceptionForTriggers(Exception ex)
		{
			if (ex is ZSaveException && ex.InnerException?.InnerException?.Message == JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTriggerID)
			{
				Globals.Message.ShowError(JobContainerLegsEnsureSplitDeliverySuffixIsNotDuplicatedForCartageJobTriggerMessageForUser);
				return true;
			}

			return false;
		}
	}
}
