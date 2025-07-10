using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DetentionAdviceDeliveryRegistryDataType))]
	internal class DetentionAdviceDeliveryRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DetentionAdviceDeliveryRegistryDataType>
	{
		public void TestDefaultValue()
		{
			DetentionAdviceDeliveryRegistryDataType dataType = new DetentionAdviceDeliveryRegistryDataType();
			AssertEquals("Mode", DetentionAdviceDeliveryMode.Codes.Notify, dataType.DefaultValue.Mode);
			AssertEquals("SendToNotificationGroup", true, dataType.DefaultValue.SendToNotificationGroup);
			AssertEquals("Printer", ZGuid.Empty, dataType.DefaultValue.Printer);
			AssertEquals("NotificationGroup", Constants.Groups.AllPK, dataType.DefaultValue.NotificationGroup);
		}

		#region Implementation
		protected override DetentionAdviceDeliveryRegistryDataType GetNewDataType()
		{
			return new DetentionAdviceDeliveryRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "DetentionAdviceDeliveryRegistryItemEditor";
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			DetentionAdviceDelivery advice = new DetentionAdviceDelivery(new BusinessObjectFactory());
			byte[] byteArrayValue = new byte[] { 255, 254, 60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102, 0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 68, 0, 101, 0, 116, 0, 101, 0, 110, 0, 116, 0, 105, 0, 111, 0, 110, 0, 65, 0, 100, 0, 118, 0, 105, 0, 99, 0, 101, 0, 68, 0, 101, 0, 108, 0, 105, 0, 118, 0, 101, 0, 114, 0, 121, 0, 62, 0, 60, 0, 77, 0, 111, 0, 100, 0, 101, 0, 62, 0, 78, 0, 79, 0, 84, 0, 60, 0, 47, 0, 77, 0, 111, 0, 100, 0, 101, 0, 62, 0, 60, 0, 83, 0, 101, 0, 110, 0, 100, 0, 84, 0, 111, 0, 78, 0, 111, 0, 116, 0, 105, 0, 102, 0, 105, 0, 99, 0, 97, 0, 116, 0, 105, 0, 111, 0, 110, 0, 71, 0, 114, 0, 111, 0, 117, 0, 112, 0, 62, 0, 78, 0, 60, 0, 47, 0, 83, 0, 101, 0, 110, 0, 100, 0, 84, 0, 111, 0, 78, 0, 111, 0, 116, 0, 105, 0, 102, 0, 105, 0, 99, 0, 97, 0, 116, 0, 105, 0, 111, 0, 110, 0, 71, 0, 114, 0, 111, 0, 117, 0, 112, 0, 62, 0, 60, 0, 80, 0, 114, 0, 105, 0, 110, 0, 116, 0, 101, 0, 114, 0, 62, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 45, 0, 48, 0, 48, 0, 48, 0, 48, 0, 45, 0, 48, 0, 48, 0, 48, 0, 48, 0, 45, 0, 48, 0, 48, 0, 48, 0, 48, 0, 45, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 48, 0, 60, 0, 47, 0, 80, 0, 114, 0, 105, 0, 110, 0, 116, 0, 101, 0, 114, 0, 62, 0, 60, 0, 78, 0, 111, 0, 116, 0, 105, 0, 102, 0, 105, 0, 99, 0, 97, 0, 116, 0, 105, 0, 111, 0, 110, 0, 71, 0, 114, 0, 111, 0, 117, 0, 112, 0, 62, 0, 57, 0, 52, 0, 55, 0, 53, 0, 53, 0, 101, 0, 55, 0, 49, 0, 45, 0, 97, 0, 56, 0, 55, 0, 97, 0, 45, 0, 52, 0, 48, 0, 51, 0, 52, 0, 45, 0, 56, 0, 100, 0, 102, 0, 97, 0, 45, 0, 55, 0, 56, 0, 53, 0, 55, 0, 55, 0, 51, 0, 97, 0, 52, 0, 57, 0, 54, 0, 48, 0, 55, 0, 60, 0, 47, 0, 78, 0, 111, 0, 116, 0, 105, 0, 102, 0, 105, 0, 99, 0, 97, 0, 116, 0, 105, 0, 111, 0, 110, 0, 71, 0, 114, 0, 111, 0, 117, 0, 112, 0, 62, 0, 60, 0, 47, 0, 68, 0, 101, 0, 116, 0, 101, 0, 110, 0, 116, 0, 105, 0, 111, 0, 110, 0, 65, 0, 100, 0, 118, 0, 105, 0, 99, 0, 101, 0, 68, 0, 101, 0, 108, 0, 105, 0, 118, 0, 101, 0, 114, 0, 121, 0, 62, 0 };
			return new ValidSampleAndBinaryValueInDB[] { new ValidSampleAndBinaryValueInDB(advice, byteArrayValue), };
		}
		#endregion
	}
}
