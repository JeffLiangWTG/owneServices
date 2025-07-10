using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class CO2eManualRequestHelper
	{
		public static CO2eProcessResult SendRequestManually(ICO2eCalculationSupporter cO2ECalculationSupporter, INotifications notifications, IAddressesValidationManager addressValidationManager = default, ICO2eRecalculationChecker recalculationChecker = null)
		{
			Argument.NotNull(cO2ECalculationSupporter, nameof(cO2ECalculationSupporter));
			if (!ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				return CO2eProcessResult.EhubFail;
			}

			var supporterBO = cO2ECalculationSupporter as BusinessObject;
			if (supporterBO == null)
			{
				ErrorReporter.ReportOnce("CO2e Calculation Supporter is not a Business Object.");
				return CO2eProcessResult.EhubFail;
			}

			var factory = new BusinessObjectFactory() { NameForDebugging = "CO2e Calculation Request Sender" };
			var supporterBOInNewFactory = factory.ImportFromAnotherFactory(supporterBO);
			using (factory.AddDisposableService())
			{
				var processor = new SendCO2eCalculationRequestProcessor(supporterBOInNewFactory, new ManualCO2eCalculationActionInfo(supporterBOInNewFactory), addressValidationManager: addressValidationManager, skipValidateInputs: true, recalculationChecker: recalculationChecker);
				var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
				processor.Process(notifications, replaceThisTokenEventuallyQuestionMarkExclamationMark);

				if (!processor.HasSupportersToSend)
				{
					return CO2eProcessResult.NotRequired;
				}

				try
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, () => { }, true, true);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					notifications.AddError(Res.GetString("f4ba4897-54aa-49cf-84a3-6e733b555983", "Sending greenhouse gas emissions calculation request failed due to an error. Please try again."));
					return CO2eProcessResult.EhubFail;
				}
			}

			return CO2eProcessResult.EhubSuccess;
		}
	}
}
