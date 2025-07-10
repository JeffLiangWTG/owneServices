using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI.CO2e;
using Enterprise.Freight.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class CalculateGreenhouseGasEmissionsApplicator : OperationalActionMethodApplicator, INotifications
	{
		public CalculateGreenhouseGasEmissionsApplicator() : base("CalculateGreenhouseGasEmissionsApplicator")
		{
		}

		#region INotifications Members

		public void Add(INotification notification)
		{ }

		#endregion

		#region Simple Properties

		public bool MultipleBatch_HasExceededLimit { get; private set; }

		#endregion

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (MultipleBatch_HasExceededLimit)
			{
				return;
			}

			if (targets.Length > 25)
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("7A650AEB-D0A8-49C9-9C6E-A08365F478DA", "Please reduce the number of selected jobs, the maximum allowed for this action is 25."));
				Globals.Message.ShowError(Res.GetString("7A650AEB-D0A8-49C9-9C6E-A08365F478DA", "Please reduce the number of selected jobs, the maximum allowed for this action is 25."), Res.GetString("892CDC40-F010-4056-86A6-D7C7E37D16D7", "Request failed"));
				MultipleBatch_HasExceededLimit = true;
				return;
			}

			log.SetSectionProgressMax(targets.Length);

			foreach (var shipment in targets as ForwardingShipment[])
			{
				var supporter = shipment as ICO2eCalculationSupporter;

				var reasons = supporter.ValidateInputsWithAdditionalSupporter();
				if (reasons.Any())
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("27E38762-2323-4D97-B845-5FF2E251C41E", "{0}: {1}", shipment.JS_UniqueConsignRef, string.Join("\n", reasons.Prepend(Res.GetString("ADC6E8D9-47F5-44CE-98BE-DBAA3CF37818", "Error: The greenhouse gas emissions calculation cannot be requested because the following mandatory input is missing or invalid:")))));
					log.BumpSectionProgress();
					continue;
				}

				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("35D699F9-1CF1-439D-B4F3-2F471233BB74", "{0}: The greenhouse gas emissions calculation has been requested.", shipment.JS_UniqueConsignRef));
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("B40D795E-B4B2-4731-B070-FABA1F7DEE18", "{0}: Processing", shipment.JS_UniqueConsignRef));
				var result = new CO2eRequestProcessor(progressFormManager: new DummyProgressForm()).SendRequest(shipment, this);

				if (result.IsSuccess)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Success, Res.GetString("94114432-8F44-472D-9E34-20F8FA91BD30", "{0}: Updated", shipment.JS_UniqueConsignRef));
				}
				else if (result.Type == CO2eResultType.ApiFail)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("11674B3B-B064-4A00-BD4E-6AD96D90622B", "{0}: Error requesting greenhouse gas emissions calculation service.", shipment.JS_UniqueConsignRef));
				}
				else if (result.Type == CO2eResultType.ServiceUnavailable)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("252c640e-5724-4b8e-a35a-d964c3ed30d7", "{0}: Issue communicating with server. Please try again later.", shipment.JS_UniqueConsignRef));
				}
				else if (result.Type == CO2eResultType.NotRequired)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, Res.GetString("929cfb87-08e7-427a-84c0-c8fd90373336", "{0}: Greenhouse Gas calculation not required because the CO2e value is current.", shipment.JS_UniqueConsignRef));
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("F28A46C9-1829-4097-A429-B1A12AB434B9", "{0}: Failed", shipment.JS_UniqueConsignRef));
				}

				log.BumpSectionProgress();
			}

			log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("BA0FD0AE-C302-44F9-ABC4-C2605E845294", "Greenhouse gas emissions calculation request is completed, please check above log for details."));
		}

		protected override void InitialiseBeforeAllBatchesRunCore()
		{
			base.InitialiseBeforeAllBatchesRunCore();
			MultipleBatch_HasExceededLimit = false;
		}
	}
}
