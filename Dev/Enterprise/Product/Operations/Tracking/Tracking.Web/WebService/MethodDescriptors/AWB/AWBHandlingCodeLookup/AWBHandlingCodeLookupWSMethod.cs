using System.Web;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public class AWBHandlingCodeLookupWSMethod : TrackingWebServiceMethod<AWBHandlingCodeLookupParameters>
	{
		#region Overrides

		protected override void ExecuteCore(AWBHandlingCodeLookupParameters parameters, WebServiceResponse response)
		{
			string description = SpecialHandlingCodeDescriptionList.ContainsCode(parameters.CodeValue) ? SpecialHandlingCodeDescriptionList[parameters.CodeValue].Description : string.Empty;
			if (!string.IsNullOrEmpty(description))
			{
				var updateToken = new UpdateValueResponseToken(parameters.DescriptionControlID, description);
				updateToken.Conditions.Add(new ResponseConditionEqualToken(parameters.CodeControlID, parameters.CodeValue));
				response.Add(updateToken);
			}
		}

		protected override bool AddErrorMessageToResponse()
		{
			return false;
		}

		protected override string GetMethodName()
		{
			return "LookupAWBHandlingCode";
		}

		protected override string GetScriptFileName()
		{
			return "LookupAWBHandlingCodeWSM.js";
		}

		#endregion

		#region Implementation

		static readonly object locker = new object();
		const string AWBSpecialHandlingListKey = "AWBSpecialHandlingList";

		public AWBSpecialHandlingCodeDescriptionPairList SpecialHandlingCodeDescriptionList
		{
			get
			{
				var list = HttpRuntime.Cache[AWBSpecialHandlingListKey] as AWBSpecialHandlingCodeDescriptionPairList;
				if (list == null)
				{
					lock (locker)
					{
						list = HttpRuntime.Cache[AWBSpecialHandlingListKey] as AWBSpecialHandlingCodeDescriptionPairList;
						if (list == null)
						{
							list = new AWBSpecialHandlingCodeDescriptionPairList();
							HttpRuntime.Cache.Insert(AWBSpecialHandlingListKey, list);
						}
					}
				}

				return list;
			}
		}

		#endregion
	}
}
