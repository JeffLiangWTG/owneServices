using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class MSNEventProcessHelper
	{
		public static void AddMSNEvent(ForwardingConsol consol, string documentName)
		{
			if (consol != null)
			{
				var factory = new BusinessObjectFactory();
				consol = factory.Load<ForwardingConsol>(consol.PK);

				var scac = OCBEventParameterHelper.GetSCAC(consol);
				consol.Logs.CreateOrRecreateEventLog(Events.MessageSent, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, GetMSNParametersForEvent(documentName, scac));

				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Parameters")]
		static KeyValuePair<string, string>[] GetMSNParametersForEvent(string typeValue, string company)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, "Carrier"));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, typeValue));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Company, company));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Action, "Email"));

			return result.ToArray();
		}
	}
}
