using System.Web;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public class AWBOtherChargeCodeLookupWSMethod : TrackingWebServiceMethod<AWBOtherChargeCodeLookupParameters>
	{
		#region Overrides

		protected override void ExecuteCore(AWBOtherChargeCodeLookupParameters parameters, WebServiceResponse response)
		{
			var mawb = MAWBInSession(parameters.SessionIndex);
			var chargePK = new ZGuid(parameters.ChargePK);
			if (chargePK.IsValid && mawb != null)
			{
				mawb.EH_OtherPrepaidCollect = parameters.PrepaidCollectFlag;
				ExportAWBOtherCharges charge = null;
				foreach (var otherCharge in mawb.AWBOtherCharges)
				{
					if (otherCharge.PK == chargePK)
					{
						charge = otherCharge;
						break;
					}
				}
				if (charge != null)
				{
					charge.EO_ChargeCode = parameters.CodeValue;
					string description = charge.EO_ChargeDescription;
					string entitlementCode = charge.EO_EntitlementCode;

					var updateDescriptionToken = new UpdateValueResponseToken(parameters.DescriptionControlID, description);
					updateDescriptionToken.Conditions.Add(new ResponseConditionEqualToken(parameters.CodeControlID, parameters.CodeValue));
					response.Add(updateDescriptionToken);

					var updateEntitlementCodeToken = new UpdateValueResponseToken(parameters.EntitlementCodeControlID, entitlementCode);
					updateEntitlementCodeToken.Conditions.Add(new ResponseConditionEqualToken(parameters.CodeControlID, parameters.CodeValue));
					response.Add(updateEntitlementCodeToken);
				}
			}
		}

		protected override bool AddErrorMessageToResponse()
		{
			return false;
		}

		protected override string GetMethodName()
		{
			return "LookupAWBOtherChargeCode";
		}

		protected override string GetScriptFileName()
		{
			return "LookupAWBOtherChargeCodeWSM.js";
		}

		#endregion

		#region Implementation

		protected TrackingMAWBHeader MAWBInSession(string indexer)
		{
			return !string.IsNullOrEmpty(indexer) && HttpContext.Current != null && HttpContext.Current.Session != null ? HttpContext.Current.Session[indexer] as TrackingMAWBHeader : null;
		}

		#endregion
	}
}
