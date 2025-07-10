using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public class EmissionsCalculationLoggerTest : TestCaseWithFactory
{
	[TestDate(2025, 01, 01, 1, 0, 0)]
	public void TestLogHeader()
	{
		var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
		orgProxy.OH_IsConsignor = true;

		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
		company.GC_OH_OrgProxy = orgProxy.PK;
		company.GC_Name = "Your Mars Company";

		var branch = Factory.NewWithValidTestData<GlbBranch>();
		branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
		branch.GB_GC = company.PK;
		branch.GB_BranchName = "Mars - District 1";

		Factory.Save();

		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000123";
			((ICO2eProvider)shipment).SetTotalCO2e(200.9876543m);

			var logger = new EmissionsCalculationLogger();
			logger.LogHeader((ICO2eCalculationSupporter)shipment, 100.1234567m, true);

			AssertMultilineASCIIEquals(@"01-Jan-25 01:00 ----- Shipment S00000123 ----- Your Mars Company
Previous CO2e value: 100.1234567 kg
New CO2e value: 200.9876543 kg
Calculated manually
", logger.GetLog());

			logger.Clear();
			logger.LogHeader((ICO2eCalculationSupporter)shipment, 100.1234567m, false);
			AssertMultilineASCIIEquals(@"01-Jan-25 01:00 ----- Shipment S00000123 ----- Your Mars Company
Previous CO2e value: 100.1234567 kg
New CO2e value: 200.9876543 kg
Calculated automatically
", logger.GetLog());
		}
	}

	public void TestLogJobLevelParameters()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
		shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
		shipment.JS_PackingMode = Core.Constants.ContainerModes.Loose;
		shipment.JS_ActualWeight = 10.05m;
		shipment.JS_UnitOfWeight = Core.Constants.Weight.Tonnes;

		var logger = new EmissionsCalculationLogger();
		logger.LogJobLevelParameters((ICO2eLegBasedSupporter)shipment);

		AssertContains(@"Job level input parameters:
Transport Mode: AIR
Container Mode: LSE
Total Weight: 10.05 T
Temperature Controlled: N", logger.GetLog());
	}

	public void TestLogEmptyContainerParameters()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
		var weightData = new WeightData
		{
			ContainerJobID = "CJOB123456",
			EmptyPickup = new EmptyContainerAddress
			{
				TransportMode = new CodeDescriptionPair { Code = "Roa", Description = "Road" },
				From = new OrganizationAddress(),
				To = new OrganizationAddress(),
				GreenhouseGasEmission = new GreenhouseGasEmission { CO2e = 1.234m, CO2eDistanceInKm = 12.34m }
			},
			EmptyReturn = new EmptyContainerAddress
			{
				TransportMode = new CodeDescriptionPair { Code = "Iwt" },
				From = new OrganizationAddress(),
				To = new OrganizationAddress(),
				GreenhouseGasEmission = new GreenhouseGasEmission { CO2e = 2.345m, CO2eDistanceInKm = 23.45m }
			}
		};

		var logger = new EmissionsCalculationLogger();

		logger.LogEmptyContainerParameters((ICO2eLegBasedSupporter)shipment, weightData);

		var logText = logger.GetLog();

		AssertContains("Empty Container Pickup/Return:", logText);
		AssertContains("Container number: -", logText);
		AssertContains("Pickup Transport Mode: Road", logText);
		AssertContains("Return Transport Mode: InlandWaterway", logText);
		AssertContains("Pickup CO2e: 1.234 kg", logText);
		AssertContains("Return CO2e: 2.345 kg", logText);
		AssertContains("Pickup Distance: 12.34 km", logText);
		AssertContains("Return Distance: 23.45 km", logText);
	}

	public void TestSaveToNote()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as IStmNoteParent;

		var logger = new EmissionsCalculationLogger();
		logger.Log("something");
		logger.SaveToNote(shipment);

		Factory.Save();

		var emissionsCalculationNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).FirstOrDefault();
		AssertNotNull("Note exists", emissionsCalculationNote);
		Assert("Note is readonly", emissionsCalculationNote.ReadOnly);
		AssertEquals("Note content", "something", emissionsCalculationNote.ST_NoteText);
	}

	public void TestSaveToNote_ConsecutiveSave()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as IStmNoteParent;

		var logger = new EmissionsCalculationLogger();
		logger.Log("something1");
		logger.SaveToNote(shipment);
		logger.Log("something2");
		logger.SaveToNote(shipment);

		var emissionsCalculationNotes = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description);
		AssertEquals(1, emissionsCalculationNotes.Length);

		var emissionsCalculationeNote = emissionsCalculationNotes.FirstOrDefault();
		AssertMultilineASCIIEquals("Note content", @"something2
------------------------------------------------------------------------------------
something1", emissionsCalculationeNote.ST_NoteText);
	}

	public void TestSaveToNote_ExceedMaxLength()
	{
		var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as IStmNoteParent;

		var logger = new EmissionsCalculationLogger();
		logger.Log(string.Concat(Enumerable.Repeat("X", 5000)));
		logger.SaveToNote(shipment);

		var emissionsCalculationNote = shipment.Notes.FindByDescription(PredefinedNoteTypes.Instance.EmissionsCalculationLog.Description).FirstOrDefault();
		emissionsCalculationNote.NoteTextMaxLength = 7000;

		logger.Log(string.Concat(Enumerable.Repeat("Y", 5000)));
		logger.SaveToNote(shipment);
		Assert(emissionsCalculationNote.ST_NoteText.EndsWith("...trimmed to fit"));
		AssertEquals(7000, emissionsCalculationNote.ST_NoteText.Length);
	}

	public void TestFormatLocation()
	{
		var method = typeof(EmissionsCalculationLogger).GetMethod("FormatLocation", BindingFlags.Static | BindingFlags.NonPublic);
		AssertNotNull("FormatLocation exists", method);

		AssertEquals("-", (string)method.Invoke(null, new object[] { null }));

		var addrEmpty = new OrganizationAddress { AddressType = "TST" };
		AssertEquals("-", (string)method.Invoke(null, new object[] { addrEmpty }));

		var addrCity = new OrganizationAddress
		{
			AddressType = "TST",
			City = "Brisbane",
			Country = new Country { Code = "AU" }
		};

		AssertEquals("Brisbane, AU", (string)method.Invoke(null, new object[] { addrCity }));

		var addrPort = new OrganizationAddress
		{
			AddressType = "TST",
			City = "Brisbane",
			Port = new UNLOCO { Code = "AUBNE" },
			Country = new Country { Code = "AU" }
		};

		AssertEquals("AUBNE // Brisbane, AU", (string)method.Invoke(null, new object[] { addrPort }));

		var addrFull = new OrganizationAddress
		{
			AddressType = "TST",
			City = "Sydney",
			Postcode = "2000",
			Country = new Country { Code = "AU" },
			Port = new UNLOCO { Code = "AUSYD" },
			GeoLocation = new GeoLocation { Latitude = -33.87m, Longitude = 151.21m }
		};

		AssertEquals("-33.87 151.21 // AUSYD // Sydney, AU // 2000, AU", (string)method.Invoke(null, new object[] { addrFull }));
	}

	public void TestGetDescription()
	{
		var method = typeof(EmissionsCalculationLogger).GetMethod("GetDescription", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		AssertNotNull("GetDescription exists", method);

		var cases = new Dictionary<string, string>
		{
			{ "SEA", "Sea" },
			{ "AIR", "Air" },
			{ "ROA", "Road" },
			{ "RAI", "Rail" },
			{ "IWT", "InlandWaterway" },
			{ "XYZ", "-" },
		};

		foreach (var key in cases)
		{
			var pair = new CodeDescriptionPair { Code = key.Key };
			var result = (string)method.Invoke(null, new object[] { pair });

			AssertEquals($"Description for {key.Key}", key.Value, result);
		}
	}
}
