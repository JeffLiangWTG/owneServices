using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.CarbonEmissions.Business;

public class EmissionsCalculationLogger
{
	readonly List<string> logLines = [];

	public void LogHeader(ICO2eCalculationSupporter supporter, decimal previousCO2e, bool isManualCalculation)
	{
		var jobName = (supporter as IBusiness)?.HumanReadableName ?? ZString.Empty;
		var companyName = GlbCompany.CurrentCompany?.CompanyName ?? ZString.Empty;

		var headers = new List<string>
		{
			(NoResString)$"{ZDateTime.Now:dd-MMM-yy HH:mm} ----- {jobName} ----- {companyName}",
			Res.GetString("F5BD6BB9-3C1B-4747-AAF5-18438FEEA816", "Previous CO2e value: {0} kg", previousCO2e),
			Res.GetString("0FBAD1DE-AB2B-4CE7-A69C-0A3F8F7B9881", "New CO2e value: {0} kg", supporter.GetTotalCO2e()),
			isManualCalculation
				? Res.GetString("22A4A2BD-3AB2-44E7-B617-C79FEA5C219C", "Calculated manually")
				: Res.GetString("913B69AC-CD73-43D8-B520-F7EEC30AB38F", "Calculated automatically")
		};

		foreach (var header in headers.AsEnumerable().Reverse())
		{
			LogTop(header);
		}
	}

	public void LogJobLevelParameters(ICO2eLegBasedSupporter supporter)
	{
		Log("\r\n");
		Log(Res.GetString("C00E011A-9146-4DF4-8D7A-702B3FC67817", "Job level input parameters:"));
		Log(Res.GetString("664D43D9-E5C8-4D42-B004-CA182493DA3F", "Transport Mode: {0}", supporter.TransportMode));
		Log(Res.GetString("25A0A2AB-146F-4228-A63A-BD7A52656AF7", "Container Mode: {0}", supporter.ContainerMode));
		Log(Res.GetString("73E2631F-F713-4B18-A0DC-711E99BFBE2B", "Total Weight: {0} {1}", Utilities.Round(supporter.Weight, 3), supporter.UnitOfWeight));

		if (supporter.RequireTEU)
		{
			Log(Res.GetString("3CCC7730-45D8-4A23-A477-37119F68DCFC", "Number of TEUs: {0}", Utilities.Round(supporter.NumberOfTEU, 6)));
			Log(Res.GetString("0551F6CC-CD0D-4552-8A42-A6BFBE37BBB9", "Tonnes per TEU: {0} t", Utilities.Round(supporter.TonnesPerTEU, 6)));
		}

		Log(Res.GetString("B4D58185-51C7-4872-AF48-E9C7525F241E", "Temperature Controlled: {0}", (NoResString)(supporter.RequiresTemperatureControl ? "Y" : "N")));
	}

	public void LogEmptyContainerParameters(ICO2eLegBasedSupporter supporter, WeightData dto)
	{
		Log("\r\n");

		var dtoJobId = dto.ContainerJobID ?? ZString.Empty;
		var provider = supporter.EmptyContainers?.Select(x => x.Container).OfType<ICO2eEmptyContainerProvider>().FirstOrDefault(c => c.ContainerJobID == dtoJobId);

		Log(Res.GetString("1551c311-e8ed-4fa8-a8aa-822c2949107b", "Empty Container Pickup/Return:"));
		Log(Res.GetString("73212897-e0af-4232-ad50-3fb5b7301b51", "Consol number: {0}", provider?.ConsolNumber ?? "-"));
		Log(Res.GetString("b29d870f-3664-4381-9391-6d4d3addaa2b", "Container number: {0}", provider?.ContainerNumber ?? "-"));
		Log(Res.GetString("9283b52b-7f8a-426d-93c4-ea0655cd2aad", "No. of TEUs: {0}", provider != null ? provider.NumberOfTEU : "-"));
		Log(Res.GetString("dbe29f1b-0b83-4fca-b515-60bc66e687e9", "Tare Weight: {0} kg", provider != null ? Utilities.Round(provider.TotalWeight, 3) : "-"));

		if (dto.EmptyPickup != null)
		{
			Log(Res.GetString("e20a89d2-70de-4a6e-9537-e430692abc51", "Pickup Transport Mode: {0}", GetDescription(dto.EmptyPickup.TransportMode)));
			Log(Res.GetString("f0f9cc3e-16c2-426c-a757-fd30c263ca1c", "Pickup Origin: {0}", FormatLocation(dto.EmptyPickup.From)));
			Log(Res.GetString("8245977f-c039-45ba-9804-68e7bc734c9b", "Pickup Destination: {0}", FormatLocation(dto.EmptyPickup.To)));
			Log(Res.GetString("df4173ee-fb4b-4d4e-8efa-aa123b1c7a2e", "Pickup CO2e: {0} kg", dto.EmptyPickup.GreenhouseGasEmission?.CO2e ?? 0m));
			Log(Res.GetString("49fea0b0-3d21-469b-bd82-df31fc4159c4", "Pickup Distance: {0} km", dto.EmptyPickup.GreenhouseGasEmission?.CO2eDistanceInKm ?? 0m));
		}

		if (dto.EmptyReturn != null)
		{
			Log(Res.GetString("e221a799-e4cd-4315-b47b-a65b622c85ae", "Return Transport Mode: {0}", GetDescription(dto.EmptyReturn.TransportMode)));
			Log(Res.GetString("bf30bb22-6439-4bc6-8bd8-1eb8634eab59", "Return Origin: {0}", FormatLocation(dto.EmptyReturn.From)));
			Log(Res.GetString("3deba151-8084-4815-ad93-c55196f133f6", "Return Destination: {0}", FormatLocation(dto.EmptyReturn.To)));
			Log(Res.GetString("3d8934d4-8dec-46c5-9615-3615fd503de1", "Return CO2e: {0} kg", dto.EmptyReturn.GreenhouseGasEmission?.CO2e ?? 0m));
			Log(Res.GetString("acd2b801-4387-4f6c-a82a-304f16f7779d", "Return Distance: {0} km", dto.EmptyReturn.GreenhouseGasEmission?.CO2eDistanceInKm ?? 0m));
		}
	}

	public void SaveToNote(IStmNoteParent noteParent)
	{
		if (noteParent is null)
		{
			return;
		}

		var description = PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description;
		var note = noteParent.Notes
			.FindByDescription(description)
			.SingleOrDefault(x => !x.IsNull);

		if (note == null)
		{
			note = noteParent.Notes.AddNew();
			note.ST_Description = description;
		}

		if (note.ST_NoteText != ZString.Empty)
		{
			Log(Separator);
			Log(note.ST_NoteText);
		}

		note.ST_NoteText = TrimToFit(GetLog(), note.NoteTextMaxLength);

		Clear();
	}

	ZString TrimToFit(string text, int maxLength, string trimMessage = "...trimmed to fit")
	{
		return maxLength > 0 && text.Length > maxLength ? text.Substring(0, maxLength - trimMessage.Length) + trimMessage : text;
	}

	public void LogTop(string message)
	{
		logLines.Insert(0, message);
	}

	public void Log(string message)
	{
		logLines.Add(message);
	}

	public string GetLog()
	{
		return string.Join("\r\n", logLines);
	}

	public void Clear()
	{
		logLines.Clear();
	}

	const string Separator = "------------------------------------------------------------------------------------";

	static string FormatLocation(OrganizationAddress addr)
	{
		if (addr == null)
		{
			return "-";
		}

		var parts = new[]
		{
			addr.GeoLocation != null ? $"{addr.GeoLocation.Latitude} {addr.GeoLocation.Longitude}" : string.Empty,
			addr.Port?.Code.ToString() ?? string.Empty,
			!string.IsNullOrWhiteSpace(addr.City?.ToString()) ? $"{addr.City}, {addr.Country?.Code}" : string.Empty,
			!string.IsNullOrWhiteSpace(addr.Postcode?.ToString()) ? $"{addr.Postcode}, {addr.Country?.Code}" : string.Empty
		}.Where(p => !string.IsNullOrWhiteSpace(p));

		return parts.Any() ? string.Join(" // ", parts) : "-";
	}

	static string GetDescription(CodeDescriptionPair pair)
	{
		var code = pair?.Code ?? ZString.Empty;
		return code.IsEmpty ? "-" : new TransportModeConverter().ToEnumValue(code.ToUpper())?.ToString() ?? "-";
	}
}
