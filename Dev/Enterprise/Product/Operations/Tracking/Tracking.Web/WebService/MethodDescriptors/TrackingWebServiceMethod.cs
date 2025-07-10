using System;
using System.Collections.Generic;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.ServerServices
{
	public abstract class TrackingWebServiceMethod<ParametersType> : WebServiceMethod<ParametersType>
		where ParametersType : WebServiceParameters
	{
		#region Overrides

		protected override string GetScriptLocation()
		{
			return "Enterprise.Tracking.Web.WebService.JavaScripts";
		}

		protected override Type GetScriptType()
		{
			return typeof(TrackingWebService);
		}

		protected override void AddServiceScripts(ZPage page, Dictionary<string, ZWebResource> serviceScripts)
		{
			base.AddServiceScripts(page, serviceScripts);
			ZWebResource script = GetTrackingScript(page);
			if (script != null)
			{
				serviceScripts.Add(TrackingScriptKey, script);
			}
		}

		protected override ServiceReference GetWebServiceReference()
		{
			return TrackingWebServices.Instance.WebServiceReference;
		}

		#endregion

		#region Implementation

		protected static bool HasNewDecimalValue(string newValue, ZDecimal oldValue, out ZDecimal result)
		{
			result = ZDecimal.ParseSafe(newValue, ZDecimal.Zero);

			return result != oldValue;
		}

		protected static bool HasNewIntValue(string newValue, ZInt oldValue, out ZInt result)
		{
			result = ZInt.ParseSafe(newValue, ZInt.Zero);

			return result != oldValue;
		}

		protected static string GetFormatUpdateValue(object value, bool emptyStringIfZero)
		{
			if (emptyStringIfZero && value is ZDecimal numericValue && numericValue.IsEmpty)
			{
				return string.Empty;
			}

			return value?.ToString() ?? string.Empty;
		}

		protected static void AddUpdateToken(WebServiceResponse response, string modifiedControlID, string controlID, object initialValue, object updatedValue, bool emptyStringIfZero = false)
		{
			if (controlID != modifiedControlID && !string.IsNullOrEmpty(controlID) && !object.Equals(initialValue, updatedValue))
			{
				var updateToken = new UpdateValueResponseToken(controlID, GetFormatUpdateValue(updatedValue, emptyStringIfZero));
				updateToken.Conditions.Add(new ResponseConditionEqualToken(controlID, initialValue));
				response.Add(updateToken);
			}
		}

		protected virtual ZWebResource GetTrackingScript(ZPage page)
		{
			return new ZWebResource(GetScriptType(), "TrackingWebServiceMethod.js", page, "Enterprise.Tracking.Web.WebService.JavaScripts");
		}

		protected string TrackingScriptKey
		{
			get { return GetTrackingScriptKey(); }
		}

		protected virtual string GetTrackingScriptKey()
		{
			return "TrackingWebServiceMethodScriptKey";
		}

		#endregion
	}
}
