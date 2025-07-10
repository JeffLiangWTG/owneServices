using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.Business
{
	public static class DpsWorkflowTrackingEvent
	{
		public static void AddNew(BusinessObject bizo, string currentStatus, string newStatus)
		{
			AddNew(bizo, currentStatus, newStatus, null, Core.Constants.ScreeningType.Manual, null);
		}

		public static void AddNew(BusinessObject bizo, string currentStatus, string newStatus, string eventReference, string screenedType, string screenedScore)
		{
			if (!ComplianceRiskHelper.CheckIfComplianceRiskEnabled(bizo))
			{
				var logParameters = new Dictionary<string, string>
				{
					[Params.Codes.New] = newStatus,
					[Params.Codes.Old] = currentStatus
				};

				if (!string.IsNullOrEmpty(screenedType))
				{
					if (screenedType == Core.Constants.ScreeningType.Silent && !string.IsNullOrEmpty(screenedScore))
					{
						logParameters.Add(Params.Codes.Score, screenedScore);
					}

					logParameters.Add(Params.Codes.Type, screenedType);
				}

				logParameters.Add(Params.Codes.Company, Environment.Env.CurrentCompanyPK.ToString());
				var eventLog = bizo.GetLogs().AddNew(AutoEvents.DeniedPartyStatusUpdated, eventReference, logParameters.ToArray());

				if (screenedType == Core.Constants.ScreeningType.Resynchronize)
				{
					eventLog.SL_GS_NKUser = User.ServiceUserCode;
				}
			}
		}

		public static void AddNewWithScreeningLog(BusinessObject bizo, string currentStatus, string newStatus, string clearReason)
		{
			var screeningLogCollection = (IStmEntityScreeningLogCollection)Activator.CreateInstance(ObjectFactory.GetType<IStmEntityScreeningLogCollection>(), bizo);
			screeningLogCollection.AddMarkAsJobClearLog(clearReason);

			AddNew(bizo, currentStatus, newStatus);

			try
			{
				bizo.Factory.Save();
			}
			catch (Exception ex) when (ex is ZSaveConcurrencyException || ex is ZCannotSaveException || ex is ZSaveException)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}
	}
}
