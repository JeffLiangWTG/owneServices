using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(DpsStatusUpdateSettingDataType))]
	public class DpsStatusUpdateSettingDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DpsStatusUpdateSettingDataType>
	{
		public void TestDeserialisedWithPhaseRegistryItem()
		{
			AssertNotNull(DataType.PhaseRegistryItem);
			AssertEquals("Fake Registry Item", DataType.PhaseRegistryItem.Name);
		}

		#region Implementation

		protected override DpsStatusUpdateSettingDataType GetNewDataType()
		{
			var registryItem = new PhaseSecurityRegistryItem("Fake Registry Item", null, null, null, null, null);
			return new DpsStatusUpdateSettingDataType(registryItem);
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "DpsStatusUpdateSettingRegistryItemEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var setting = new DpsStatusUpdateSetting();
			setting.Option = DpsStatusUpdateOptions.Codes.PHS;

			var setting1 = setting.JobUpdateSettings.AddNew();
			setting1.Code = "PH1";
			setting1.Description = (NoResString)"Phase 1";
			setting1.ShouldUpdate = true;

			var setting2 = setting.JobUpdateSettings.AddNew();
			setting2.Code = "PH2";
			setting2.Description = (NoResString)"Phase 2";
			setting2.ShouldUpdate = false;

			#region ByteArrayValue

			byte[] byteArrayValue = new byte[]
{
60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,68,0,112,0,115,0,83,0,116,0,97,0,116,0,117,0,115,0,85,0,112,0,100,0,97,0,116,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,79,0,112,0,116,0,105,0,111,0,110,0,62,0,80,0,72,0,83,0,60,0,47,0,79,0,112,0,116,0,105,0,111,0,110,0,62,0,60,0,74,0,111,0,98,0,85,0,112,0,100,0,97,0,116,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,62,0,60,0,74,0,111,0,98,0,80,0,104,0,97,0,115,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,80,0,72,0,49,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,83,0,104,0,111,0,117,0,108,0,100,0,85,0,112,0,100,0,97,0,116,0,101,0,62,0,89,0,60,0,47,0,83,0,104,0,111,0,117,0,108,0,100,0,85,0,112,0,100,0,97,0,116,0,101,0,62,0,60,0,47,0,74,0,111,0,98,0,80,0,104,0,97,0,115,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,74,0,111,0,98,0,80,0,104,0,97,0,115,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,67,0,111,0,100,0,101,0,62,0,80,0,72,0,50,0,60,0,47,0,67,0,111,0,100,0,101,0,62,0,60,0,83,0,104,0,111,0,117,0,108,0,100,0,85,0,112,0,100,0,97,0,116,0,101,0,62,0,78,0,60,0,47,0,83,0,104,0,111,0,117,0,108,0,100,0,85,0,112,0,100,0,97,0,116,0,101,0,62,0,60,0,47,0,74,0,111,0,98,0,80,0,104,0,97,0,115,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,47,0,74,0,111,0,98,0,85,0,112,0,100,0,97,0,116,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,47,0,68,0,112,0,115,0,83,0,116,0,97,0,116,0,117,0,115,0,85,0,112,0,100,0,97,0,116,0,101,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0
};

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(setting, byteArrayValue)
			};
		}

		#endregion
	}
}
