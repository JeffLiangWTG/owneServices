using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using WiseRates.Api.Model;
using static Enterprise.Core.Constants;
using DTOConsts = WiseRates.Api.Model.Constants;

namespace Enterprise.Rating.GUI
{
	public static class TransportLegConverter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Its a format")]
		const string TransitTimeFormatString = @"d\d\ h\h";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Its a format")]
		const string DeadlineDateFormat = "yyyy-MM-dd HH:mm";

		public static List<TransportLeg> Convert(BusinessObjectFactory factory, OrgHeader carrier, IEnumerable<ScheduleDetail> scheduleDetails, ILogger logger)
		{
			var result = new List<TransportLeg>();
			var vessels = new HashSet<string>();

			ZByte legOrder = 1;
			TimeSpan maxTransitTime = scheduleDetails.Max(d => d.TransitTime);

			foreach (var item in scheduleDetails)
			{
				var leg = new TransportLeg();

				leg.JW_Status = TransportStatus.Planned;
				leg.JW_TransportMode = TransportModes.Sea;

				leg.JW_IsLinked = true;
				leg.JW_LegOrder = legOrder;
				leg.JW_LegNotes = PopulateLegNote(factory, item);

				leg.JW_RL_NKLoadPort = item.Origin;
				leg.JW_RL_NKDiscPort = item.Destination;
				leg.JW_VoyageFlight = item.VoyageNumber;

				var vesselName = item.VesselName?.Trim() ?? string.Empty;

				if (!string.IsNullOrEmpty(vesselName) && !vessels.Contains(vesselName))
				{
					vesselName = GetOrCreateVessel(vesselName, item.IMONumber, logger);

					if (!string.IsNullOrEmpty(vesselName))
					{
						vessels.Add(vesselName);
					}
				}

				leg.JW_Vessel = vesselName;
				if (string.IsNullOrEmpty(leg.JW_Vessel))
				{
					leg.JW_IsLinked = false;
				}

				leg.JW_ETA = item.ArrivalDate;
				leg.JW_ETD = item.DepartureDate;

				leg.JW_OA_CarrierAddress = carrier?.Addresses.MainAddress.PK ?? ZGuid.Empty;
				if (item.VGMCutOff.HasValue)
				{
					leg.JW_VGMCutOff = item.VGMCutOff.Value;
				}

				if (item.CTOCutOff.HasValue)
				{
					leg.JW_TerminalCutOff = item.CTOCutOff.Value;
				}

				if (item.DocsDue.HasValue)
				{
					leg.JW_DocumentaryCutOff = item.DocsDue.Value;
				}

				result.Add(leg);
				legOrder++;
			}

			return result;
		}

		static string PopulateLegNote(BusinessObjectFactory factory, ScheduleDetail detail)
		{
			string LoadCountryFlagName(string flagCode)
			{
				var country = RefCountry.LoadFromCountryCode(factory, flagCode);
				if (country != null)
				{
					return country.RN_Desc;
				}

				return string.Empty;
			}

			var collectionToPopulate = new Dictionary<string, string>();
			if (detail.TransitTime != default)
			{
				collectionToPopulate[Res.GetString("1df0190f-955c-413a-88cb-ca42fcfc065a", "Transit Time")] = detail.TransitTime.ToString(TransitTimeFormatString);
			}

			if (!string.IsNullOrEmpty(detail.TradeLane))
			{
				collectionToPopulate[Res.GetString("d32d3bbd-b9ae-49e5-9471-993b5d3936a7", "Trade Lane")] = detail.TradeLane;
			}

			if (!string.IsNullOrEmpty(detail.ServiceCode))
			{
				collectionToPopulate[Res.GetString("bdcbb608-ae4b-4cf0-a3c4-f8011052a641", "Service Code")] = detail.ServiceCode;
			}

			if (!string.IsNullOrEmpty(detail.ServiceName))
			{
				collectionToPopulate[Res.GetString("5e19c6ac-ef7e-48db-a3c9-d7db50c9975a", "Service Name")] = detail.ServiceName;
			}

			if (!string.IsNullOrEmpty(detail.FlagCode))
			{
				collectionToPopulate[Res.GetString("6e0c473e-6c85-4710-ae1e-e6af54a87f27", "Flag Code")] = detail.FlagCode;

				var flagName = LoadCountryFlagName(detail.FlagCode);

				if (!string.IsNullOrEmpty(flagName))
				{
					collectionToPopulate[Res.GetString("554aec32-5d48-4ed7-9b73-9cac0f764e6c", "Flag Name")] = flagName;
				}
			}

			if (detail.DateInfos?.Any() == true)
			{
				var specialDateKeys = new string[] { DTOConsts.RouteOriginDetails.VGMCutOffKey, DTOConsts.RouteOriginDetails.CTOCutOffKey, DTOConsts.RouteOriginDetails.DocsDueKey };
				foreach (var item in detail.DateInfos.Where(d => !specialDateKeys.Contains(d.Code)).OrderBy(d => d.Date))
				{
					collectionToPopulate[item.Name] = item.Date.ToString(DeadlineDateFormat);
				}
			}

			if (collectionToPopulate.Any())
			{
				var firstColumnWidth = -1 * collectionToPopulate.Select(c => c.Key.Length).Max();

				var builder = new StringBuilder();
				foreach (var item in collectionToPopulate)
				{
					builder.AppendLine($"{string.Format($"{{0,{firstColumnWidth}}}", item.Key)} {item.Value}"); // Just a template, no content.
				}

				return builder.ToString();
			}

			return string.Empty;
		}

		static string GetOrCreateVessel(string vesselName, string imo, ILogger logger)
		{
			var imoNumber = imo ?? string.Empty;

			var localFactory = new BusinessObjectFactory(Db.Connection);
			var vessel = RefVessel.LookupVesselByName(vesselName, localFactory, true).FirstOrDefault();
			if (vessel != null)
			{
				return vessel.RV_Code;
			}

			var confirmationCaption = Res.GetString("39a7733c-48a5-4a17-88a4-4ead8cbc01a2", "Creating a new Vessel");
			var confirmationMsg = Res.GetString("da16c0ec-3437-4b11-8544-005cf3846e32",
				@"During the operation the Vessel: '{0}' IMO: '{1}' could not be found. Do you want to create a new Vessel?

Click 'Yes' to proceed with creating a new Vessel

Click 'No' to proceed without creating any Vessel", vesselName, imoNumber);

			if (DialogResult.Yes != Globals.Message.Show(confirmationMsg, confirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
			{
				return string.Empty;
			}

			var newVesselCreated =
				WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
				(
					Res.GetString("a04d9b6b-6295-4d80-b321-2aa5afaad335", "Vessel"),
					(securityCore) => securityCore.VesselsModify,
					false,
					confirmationCaption,
					() =>
					{
						var newVessel = localFactory.New<RefVessel>();

						newVessel.RV_Code = vesselName;
						newVessel.RV_LloydsNumber = imoNumber;

						localFactory.Save();

						logger.Log(LogType.Information, FormattableString.Invariant($"A new vessel with Name '{vesselName}' and IMO '{imo}' has been created during autorating"));

						return true;
					}
				);

			if (newVesselCreated)
			{
				return vesselName;
			}
			else
			{
				return string.Empty;
			}
		}
	}
}
