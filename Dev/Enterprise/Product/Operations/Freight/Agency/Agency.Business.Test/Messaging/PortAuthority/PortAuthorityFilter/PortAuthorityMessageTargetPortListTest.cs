using System;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal sealed class PortAuthorityMessageTargetPortListTest : BaseAgencyTest
	{
		public void TestLoad()
		{
			SetPortAuthoritySettings("AUBNE", "AUSYD", "AUCNS", "AUMEL", "AUDRW");

			var portAuthoritySettings = AgencyRegistry.Instance.PortAuthoritySettings.Value;
			portAuthoritySettings.Settings.FindPortSetting("AUDRW").Status = PortAuthoritySettingStatus.Codes.Disabled;

			using (AgencyRegistry.Instance.PortAuthoritySettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portAuthoritySettings))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUFRE";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUCNS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUSYD";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUDRW";

				var list = new PortAuthorityMessageTargetPortList(voyage, portAuthoritySettings.Settings);
				list.Load();

				AssertEquals("AUBNE, AUCNS, AUSYD", list.CodesAsString);
				AssertEquals("Is AUCNS origin port", true, list["AUBNE"].Directions.HasFlag(PortDirections.Load));
				AssertEquals("Is AUBNE origin port", true, list["AUCNS"].Directions.HasFlag(PortDirections.Load));
				AssertEquals("Is AUSYD origin port", false, list["AUSYD"].Directions.HasFlag(PortDirections.Load));
				AssertEquals("Is AUCNS destination port", true, list["AUBNE"].Directions.HasFlag(PortDirections.Discharge));
				AssertEquals("Is AUBNE destination port", false, list["AUCNS"].Directions.HasFlag(PortDirections.Discharge));
				AssertEquals("Is AUSYD destination port", true, list["AUSYD"].Directions.HasFlag(PortDirections.Discharge));
			}
		}
	}
}
