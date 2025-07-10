using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Registry.Testing
{
	[TestedType(typeof(OverspeedAlertRegistryDataType))]
	class OverspeedAlertRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OverspeedAlertRegistryDataType>
	{
		protected override OverspeedAlertRegistryDataType GetNewDataType()
		{
			return new OverspeedAlertRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "OverspeedAlertRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			OverspeedAlertConfiguration copy = new OverspeedAlertConfiguration(null, null);

			byte[] byteArrayValue = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,79,0,118,0,101,0,114,
				0,115,0,112,0,101,0,101,0,100,0,65,0,108,0,101,0,114,0,116,0,67,0,111,0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0,60,0,79,0,118,0,101,0,114,0,115,0,112,0,101,0,101,0,100,0,65,0,108,0,101,0,114,0,116,0,84,0,121,0,
				112,0,101,0,62,0,68,0,69,0,70,0,60,0,47,0,79,0,118,0,101,0,114,0,115,0,112,0,101,0,101,0,100,0,65,0,108,0,101,0,114,0,116,0,84,0,121,0,112,0,101,0,62,0,60,0,68,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,73,0,110,0,77,0,105,0,110,0,117,0,116,
				0,101,0,115,0,62,0,54,0,48,0,60,0,47,0,68,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,73,0,110,0,77,0,105,0,110,0,117,0,116,0,101,0,115,0,62,0,60,0,47,0,79,0,118,0,101,0,114,0,115,0,112,0,101,0,101,0,100,0,65,0,108,0,101,0,114,0,116,0,67,0,111,
				0,110,0,102,0,105,0,103,0,117,0,114,0,97,0,116,0,105,0,111,0,110,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(copy, byteArrayValue)
			};
		}
	}
}
