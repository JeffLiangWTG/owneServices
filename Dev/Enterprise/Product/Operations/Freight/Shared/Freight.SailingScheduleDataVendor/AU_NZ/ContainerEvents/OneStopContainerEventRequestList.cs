using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class OneStopContainerEventRequestList : ReadOnlyCollection<OneStopContainerEventRequest>, IService
	{
		protected OneStopContainerEventRequestList()
			: base(new List<OneStopContainerEventRequest>())
		{
		}

		public static OneStopContainerEventRequestList New()
		{
			OneStopContainerEventRequestList result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden();
			}
			else
			{
				result = new OneStopContainerEventRequestList();
			}
			return result;
		}

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();
		protected delegate OneStopContainerEventRequestList NewDelegate();

		public OneStopContainerEventRequest FindOrCreateRequest(CommonContainer container)
		{
			OneStopContainerEventRequest result = null;
			containerPKsToLines.TryGetValue(container.PK, out result);
			if (result == null)
			{
				result = OneStopContainerEventRequest.New(container);
				List.Add(result);
				containerPKsToLines[container.PK] = result;
			}
			return result;
		}

		public void Remove(OneStopContainerEventRequest request)
		{
			containerPKsToLines.Remove(request.Container.PK);
			List.Remove(request);
		}

		public void RemoveUnwantedRequests()
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				OneStopContainerEventRequest request = this[i];
				if (!request.IsEventRequestRequired())
				{
					Remove(request);
				}
			}
		}

		static string RequestCountry
		{
			get
			{
				if (FreightDataRegistry.OneStopAUContainerIntegrationIsEnabled && FreightDataRegistry.OneStopNZContainerIntegrationIsEnabled)
				{
					return "ANY";
				}

				if (FreightDataRegistry.OneStopAUContainerIntegrationIsEnabled)
				{
					return "AU";
				}

				if (FreightDataRegistry.OneStopNZContainerIntegrationIsEnabled)
				{
					return "NZ";
				}

				return string.Empty;
			}
		}

		public IEnumerable<EDIMessage> CreateMessages()
		{
			var result = new List<EDIMessage>();
			void WriteMessageToResult(string eventType, string country, OneStopContainerEventRequest request)
			{
				var m = WriteMessage(eventType, country, request);
				if (m != null)
				{
					result.Add(m);
				}
			}
			if (Count > 0)
			{
				foreach (OneStopContainerEventRequest request in List)
				{
					ZString portOfLoading = request.PortOfLoading;
					ZString portOfDischarge = request.PortOfDischarge;

					if (portOfLoading.StartsWith(Constants.CountryCodes.Australia) || portOfLoading.StartsWith(Constants.CountryCodes.NewZealand))
					{
						ZString loadingCountry = portOfLoading.Substring(0, 2).ToUpper();

						if (RequestCountry == "ANY" || RequestCountry == loadingCountry)
						{
							WriteMessageToResult("GATEIN", loadingCountry, request);
							WriteMessageToResult("LOAD", loadingCountry, request);
							WriteMessageToResult((NoResString)"EXPORT PREADVICE", loadingCountry, request);
						}
					}

					if (portOfDischarge.StartsWith(Constants.CountryCodes.Australia) || portOfDischarge.StartsWith(Constants.CountryCodes.NewZealand))
					{
						ZString dischargeCountry = portOfDischarge.Substring(0, 2).ToUpper();

						if (RequestCountry == "ANY" || RequestCountry == dischargeCountry)
						{
							WriteMessageToResult("GATEOUT", dischargeCountry, request);
							WriteMessageToResult("DISCHARGE", dischargeCountry, request);
							WriteMessageToResult((NoResString)"IMPORT PREADVICE", dischargeCountry, request);
							WriteMessageToResult("DEHIRE", dischargeCountry, request);
							WriteMessageToResult("STORAGESTART", dischargeCountry, request);
							WriteMessageToResult("IMPAVAILABLE", dischargeCountry, request);
						}
					}
				}
			}
			return result;
		}

		EDIMessage WriteMessage(string eventType, string country, OneStopContainerEventRequest request)
		{
			string message = request.ToCsvLine(eventType, country).ToString();

			if (!string.IsNullOrEmpty(message))
			{
				var ediMessage = request.Container.ComTracMessages.AddNew();
				ediMessage.EM_MessageText = string.Concat(message, System.Environment.NewLine);
				ediMessage.EM_ApplicationReference = OneStopContainerEventRequest.GetPKString(request.Container);
				return ediMessage;
			}
			return null;
		}

		#region Implementation

		readonly Dictionary<ZGuid, OneStopContainerEventRequest> containerPKsToLines = new Dictionary<ZGuid, OneStopContainerEventRequest>();

		protected List<OneStopContainerEventRequest> List
		{
			get { return (List<OneStopContainerEventRequest>)base.Items; }
		}

		#endregion
	}
}
