using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefZoneHeaderForm))]
	sealed class RefZoneHeaderFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestFormCanAttachWithoutSecurity()
		{
			using (var form = new RefZoneHeaderForm(Factory.New<RefZoneHeader>()))
			{
				Assert(form is ICanAttachWithoutSecurity);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new RefZoneHeaderForm(Factory.New<RefZoneHeader>());
		}

		public void TestOriginDestinationGateway()
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Description = "Some Description";

			using (RefZoneHeaderForm form = new RefZoneHeaderForm(zone))
			{
				form.Show();
				Application.DoEvents();

				var carrierGuidFindBoxField = typeof(RefZoneHeaderForm).GetField("CarrierGuidFindBox", BindingFlags.NonPublic | BindingFlags.Instance);
				var carrierGuidFindBox = (ZGuidFindBox)carrierGuidFindBoxField.GetValue(form);

				zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.OriginGateway.Code;
				AssertEquals("Gateway", carrierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Gateway", zone.FZ_OH_RelatedPartyInfo.HumanReadableName);

				zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.WiseRatesOcean.Code;
				AssertEquals("Carrier/Customer", carrierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Carrier/Customer", zone.FZ_OH_RelatedPartyInfo.HumanReadableName);

				zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.DestinationGateway.Code;
				AssertEquals("Gateway", carrierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Gateway", zone.FZ_OH_RelatedPartyInfo.HumanReadableName);
			}

			using (RefZoneHeaderForm form = new RefZoneHeaderForm(zone))
			{
				form.Show();
				Application.DoEvents();

				var carrierGuidFindBoxField = typeof(RefZoneHeaderForm).GetField("CarrierGuidFindBox", BindingFlags.NonPublic | BindingFlags.Instance);
				var carrierGuidFindBox = (ZGuidFindBox)carrierGuidFindBoxField.GetValue(form);

				AssertEquals("Gateway", carrierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Gateway", zone.FZ_OH_RelatedPartyInfo.HumanReadableName);
			}
		}

		public void TestHVLVGateway()
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_Description = "Some Description";

			using (RefZoneHeaderForm form = new RefZoneHeaderForm(zone))
			{
				form.Show();
				Application.DoEvents();

				var carrierGuidFindBoxField = typeof(RefZoneHeaderForm).GetField("CarrierGuidFindBox", BindingFlags.NonPublic | BindingFlags.Instance);
				var carrierGuidFindBox = (ZGuidFindBox)carrierGuidFindBoxField.GetValue(form);

				zone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.HVLVGateway.Code;
				AssertEquals("Gateway", carrierGuidFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Gateway", zone.FZ_OH_RelatedPartyInfo.HumanReadableName);
			}
		}

		public void TestValidateAndSaveShowWarningMessageBox()
		{
			var zone1 = CreateZone(zoneCode: "ZON1", zoneType: "RAT", zoneMode: "AIR", locationCodes: new[] { "AUSYD", "AUMEL", "AUBNE" });
			var zone2 = CreateZone(zoneCode: "ZON2", zoneType: "RAT", zoneMode: "AIR", locationCodes: new[] { "NZAKL", "NZROT", "NZWLG" });
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (RatingDataRegistry.Instance.AllowSameUNLOCOInRatingInternationalZones.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new RefZoneHeaderForm(zone2))
			{
				form.Show();
				Application.DoEvents();

				zone2.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL"));
				form.FireSaveButton();
			}

			var lastMessage = UnitTestUserNotification.Instance.LastMessage;
			Assert(lastMessage.WasWarning);
			AssertContains(
				message: "Should have message saying location is included in different zones",
				expected: "'AUMEL' is included in overlapping Rating International Zones:",
				actualContainingExpected: lastMessage.Text);
		}

		RefZoneHeader CreateZone(string zoneCode, string zoneType, string zoneMode, params string[] locationCodes)
		{
			var zone = Factory.New<RefZoneHeader>();
			zone.FZ_Code = zoneCode;
			zone.FZ_Description = $"{zoneCode} Description";
			zone.FZ_ZoneType = zoneType;
			zone.FZ_ZoneMode = zoneMode;

			var locations = locationCodes.Select(code => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code));
			zone.UNLOCOs.AddRange(locations);

			return zone;
		}
	}
}
