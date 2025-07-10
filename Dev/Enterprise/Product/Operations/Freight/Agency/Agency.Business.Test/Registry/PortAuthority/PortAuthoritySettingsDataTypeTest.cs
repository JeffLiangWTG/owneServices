using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortAuthoritySettingsDataType))]
	internal class PortAuthoritySettingsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PortAuthoritySettingsDataType>
	{
		#region Implementation
		protected override string ExpectedEditorName
		{
			get
			{
				return "PortAuthoritySettingsRegistryItemEditor";
			}
		}

		protected override PortAuthoritySettingsDataType GetNewDataType()
		{
			return new PortAuthoritySettingsDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			PortAuthorityPortCollection ports = new PortAuthorityPortCollection();
			PortAuthorityPort port = ports.AddNew();
			port.Port = "AUBNE";
			port.Version = PortAuthorityVersionList.Codes.V11;
			port.ProductionEmail = "bob@fread.net";
			port.ProductionID = "RecipientID";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ports);
			PortAuthoritySettings settings = new PortAuthoritySettings();
			PortAuthoritySetting setting = settings.Settings[0];
			setting.Status = PortAuthoritySettingStatus.Codes.Production;
			setting.SenderID = "SenderID";
			byte[] blob = new byte[] { 255, 254, 60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102, 0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 80, 0, 111, 0, 114, 0, 116, 0, 65, 0, 117, 0, 116, 0, 104, 0, 111, 0, 114, 0, 105, 0, 116, 0, 121, 0, 83, 0, 101, 0, 116, 0, 116, 0, 105, 0, 110, 0, 103, 0, 115, 0, 62, 0, 60, 0, 80, 0, 111, 0, 114, 0, 116, 0, 65, 0, 117, 0, 116, 0, 104, 0, 111, 0, 114, 0, 105, 0, 116, 0, 121, 0, 83, 0, 101, 0, 116, 0, 116, 0, 105, 0, 110, 0, 103, 0, 62, 0, 60, 0, 80, 0, 111, 0, 114, 0, 116, 0, 62, 0, 65, 0, 85, 0, 66, 0, 78, 0, 69, 0, 60, 0, 47, 0, 80, 0, 111, 0, 114, 0, 116, 0, 62, 0, 60, 0, 83, 0, 116, 0, 97, 0, 116, 0, 117, 0, 115, 0, 62, 0, 76, 0, 73, 0, 86, 0, 60, 0, 47, 0, 83, 0, 116, 0, 97, 0, 116, 0, 117, 0, 115, 0, 62, 0, 60, 0, 83, 0, 101, 0, 110, 0, 100, 0, 101, 0, 114, 0, 73, 0, 68, 0, 62, 0, 83, 0, 101, 0, 110, 0, 100, 0, 101, 0, 114, 0, 73, 0, 68, 0, 60, 0, 47, 0, 83, 0, 101, 0, 110, 0, 100, 0, 101, 0, 114, 0, 73, 0, 68, 0, 62, 0, 60, 0, 47, 0, 80, 0, 111, 0, 114, 0, 116, 0, 65, 0, 117, 0, 116, 0, 104, 0, 111, 0, 114, 0, 105, 0, 116, 0, 121, 0, 83, 0, 101, 0, 116, 0, 116, 0, 105, 0, 110, 0, 103, 0, 62, 0, 60, 0, 47, 0, 80, 0, 111, 0, 114, 0, 116, 0, 65, 0, 117, 0, 116, 0, 104, 0, 111, 0, 114, 0, 105, 0, 116, 0, 121, 0, 83, 0, 101, 0, 116, 0, 116, 0, 105, 0, 110, 0, 103, 0, 115, 0, 62, 0 };
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(settings, blob), };
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			PortAuthoritySettings lhsSettings = lhs as PortAuthoritySettings;
			PortAuthoritySettings rhsSettings = rhs as PortAuthoritySettings;
			if (lhsSettings == null || rhsSettings == null)
			{
				base.AssertValuesEqual(message, lhs, rhs);
			}
			else
			{
				base.AssertValuesEqual("base:" + message, lhs, rhs);
				lhsSettings.Settings.Sort(PortAuthoritySetting.Schema.Port, ListSortDirection.Ascending);
				rhsSettings.Settings.Sort(PortAuthoritySetting.Schema.Port, ListSortDirection.Ascending);
				base.AssertValuesEqual("settings:" + message, lhsSettings.Settings, rhsSettings.Settings);
			}
		}
		#endregion
	}
}
