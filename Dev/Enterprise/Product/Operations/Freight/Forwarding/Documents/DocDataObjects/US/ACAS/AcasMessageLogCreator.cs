using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.US
{
	sealed class AcasMessageLogCreator : AdvancedReportMessageLogCreator
	{
		public AcasMessageLogCreator()
			: base(DocumentNames.AdvancedCargoReport, Core.Constants.CountryCodes.UnitedStates)
		{
		}

		protected override KeyValuePair<string, string>[] GetParametersForEvent(IDynamicData data, string recipient)
		{
			if (data?.Value is AirCargoAdvanceScreening acas
				&& acas.State == AcasState.AcknowledgementRequired)
			{
				var parameters = new List<KeyValuePair<string, string>>();

				foreach (var parameter in base.GetParametersForEvent(data, recipient))
				{
					parameters.Add(parameter);

					if (parameter.Key == CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType)
					{
						parameters.Add(new KeyValuePair<string, string>(
							CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageSubType,
							(NoResString)"Hold Acknowledgement")); // programmatic constant
					}
				}

				return parameters.ToArray();
			}

			return base.GetParametersForEvent(data, recipient);
		}
	}
}
