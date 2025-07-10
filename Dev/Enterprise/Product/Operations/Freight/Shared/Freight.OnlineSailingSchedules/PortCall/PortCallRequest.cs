using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.OnlineSailingSchedules.PortCall
{
	public class PortCallRequest : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PortCallRequest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Parameter Names

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "URL parameter")]
		static class ParameterNames
		{
			public const string VesselName = "Vessel.VesselName";
			public const string IMO = "Vessel.ImoNumber";
			public const string CallSign = "Vessel.CallSign";
			public const string Unloco = "Port.Unloco";
			public const string CarrierCode = "Carrier.Code";
			public const string VoyageNumberIn = "VoyageNumberIn";
			public const string ETA = "Eta";
			public const string VoyageNumberOut = "VoyageNumberOut";
			public const string ETD = "Etd";
		}

		#endregion

		public string GetRequestString(out string errorMessage)
		{
			errorMessage = string.Empty;

			var carrier = Factory.Load<OrgHeader>(CarrierPK);
			var carrierCode = carrier != null ? carrier.SCACCode : ZString.Empty;
			var carrierCodePara = GetParameterString(ParameterNames.CarrierCode, carrierCode);

			if (!carrierCode.IsEmpty && carrierCode.Length != AutoRefShippingLine.Schema.RSL_StandardCarrierAlphaCodeMaxLength)
			{
				errorMessage = Res.GetString("B0441B41-044B-4BA0-AF80-37ABAA21A0DB", "SCAC must be {0} character length.", AutoRefShippingLine.Schema.RSL_StandardCarrierAlphaCodeMaxLength);
				return null;
			}

			var vessel = Factory.Load<RefVessel>(VesselPK);
			var vesselNamePara = vessel != null ? GetParameterString(ParameterNames.VesselName, vessel.RV_Name) : ZString.Empty;

			var imoPara = GetParameterString(ParameterNames.IMO, IMO);
			var callSignPara = GetParameterString(ParameterNames.CallSign, CallSign);
			var portPara = GetParameterString(ParameterNames.Unloco, Port);

			var voyagePara = GetVoyageParameter();
			var estimatedTimePara = GetEstimatedTimeParameter();

			var parameters = new[]
			{
				vesselNamePara, imoPara, callSignPara, portPara ,carrierCodePara, voyagePara, estimatedTimePara
			};

			return string.Join(ParameterSeparator, parameters.Where(x => !x.IsEmpty));
		}

		ZString GetParameterString(ZString parameterName, ZString value)
		{
			if (value.IsEmpty)
			{
				return ZString.Empty;
			}

			var replacedValue = Regex.Replace(value, @"[\*\\\""]", match => $@"\{match}");

			replacedValue = Regex.IsMatch(replacedValue, @"[\+\-<>=\(\)! ]")
				? $@"""{replacedValue}"""
				: replacedValue;

			return $"{parameterName}:{replacedValue}";
		}

		ZString GetVoyageParameter()
		{
			var parameterName = RequestType == PortCallRequestType.Load
				? ParameterNames.VoyageNumberOut
				: ParameterNames.VoyageNumberIn;

			return GetParameterString(parameterName, Voyage);
		}

		ZString GetEstimatedTimeParameter()
		{
			var parameterName = RequestType == PortCallRequestType.Load
				? ParameterNames.ETD
				: ParameterNames.ETA;

			var value = ZString.Empty;
			if (StartEstimatedTime.IsEmpty && EndEstimatedTime.IsValid)
			{
				value = $@"[* TO {EndEstimatedTime.ToISO8601ShortDateString()}]";
			}
			else if (StartEstimatedTime.IsValid && EndEstimatedTime.IsEmpty)
			{
				value = $@"[{StartEstimatedTime.ToISO8601ShortDateString()} TO *]";
			}
			else if (StartEstimatedTime.IsValid && EndEstimatedTime.IsValid)
			{
				value = $"[{StartEstimatedTime.ToISO8601ShortDateString()} TO {EndEstimatedTime.ToISO8601ShortDateString()}]";
			}

			return value.IsEmpty ? value : (ZString)$"{parameterName}:{value}";
		}

		const string ParameterSeparator = " AND ";

		#region Properties

		public ZGuid CarrierPK { get; set; }
		public ZString Voyage { get; set; }
		public ZString Port { get; set; }
		public ZGuid VesselPK { get; set; }
		public ZString IMO { get; set; }
		public ZString CallSign { get; set; }
		public ZDateTime EstimatedTime { get; set; }
		public ZDateTime StartEstimatedTime { get; set; }
		public ZDateTime EndEstimatedTime { get; set; }
		public PortCallRequestType RequestType { get; set; }

		#endregion
	}

	public enum PortCallRequestType
	{
		Load,
		Discharge
	}
}
