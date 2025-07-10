using System;
using System.Globalization;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Moq;
using Newtonsoft.Json.Linq;
using WTG.Telematics.Common.Conversion;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsEquipment
{
	class TelEquipmentDeviceLinkerTests : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
			dbDeviceLinker = new TelEquipmentDeviceLinker(loggerMock.Object);
			mobileServicesIdentifier = new byte[] { 1, 2, 3, 4 };
			device = Factory.New<GlbDevice>();
			device.V3_MobileServicesIdentifier = mobileServicesIdentifier;
			device.V3_HumanReadableIdentifier = "TT00000001";
			device.V3_IsActive = true;
			device.V3_Model = "GLaDOS v3.1";
			startTime = DateTimeOffset.Now.AddDays(-100);
			Factory.Save();
		}

		Mock<ILogger> loggerMock;
		IDbDeviceLinker dbDeviceLinker;
		GlbDevice device;
		byte[] mobileServicesIdentifier;
		DateTimeOffset startTime;

		public void TestDoesNotCreateLinkIfThereIsNoPreExistingTelSubEquipment()
		{
			// Arrange
			var hardwareId = "0102030405060708090A0B0C";
			var configuration = $@"{{
	""hardwareId"": ""{hardwareId}"",
	""vehicleType"": ""Prime Mover"",
	""axleConfig"": ""12"",
}}";
			var vehicleInfoJson = JObject.Parse(configuration);
			loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

			// Act
			dbDeviceLinker.LinkDeviceToSubEquipment(Factory, device, vehicleInfoJson, startTime);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(LogType.Error, $"MSID: {BinaryDataConverter.ByteArrayToHexString(device.V3_MobileServicesIdentifier)} cannot link to non-existant sub-equipment of id {hardwareId}"));
			});
		}

		public void TestLinksGlbDeviceToPreExistingTelSubEquipment()
		{
			// Arrange
			var hardwareId = "0102030405060708090A0B0C";
			var configuration = $@"{{
	""hardwareId"": ""{hardwareId}"",
	""vehicleType"": ""Prime Mover"",
	""axleConfig"": ""12"",
}}";

			var vehicleInfoJson = JObject.Parse(configuration);
			var equipment = Factory.New<TelSubEquipment>();
			var equipmentConfiguration = new XElement(
				"Configuration",
				new XElement("vehicleType", vehicleInfoJson["vehicleType"].Value<string>()),
				new XElement("axleConfig", vehicleInfoJson["axleConfig"].Value<string>())
			);
			equipment.TSE_Configuration = string.Format(CultureInfo.InvariantCulture, equipmentConfiguration.ToString());
			equipment.TSE_Type = "RQ";
			equipment.TSE_Id = vehicleInfoJson["hardwareId"].Value<string>();
			Factory.Save();

			// Act
			dbDeviceLinker.LinkDeviceToSubEquipment(Factory, device, vehicleInfoJson, startTime);

			// Assert
			var query = new ZQuery();
			var builder = new SqlBuilder();
			builder.Append($"CONVERT(varchar(255), {AutoTelSubEquipment.Schema.TSE_Id}, 2)='{hardwareId}'");
			query.AddFilterString(builder);
			var equipmentList = Factory.Load<TelSubEquipment>(query);
			AssertEquals(1, equipmentList.Length);
			AssertEquals(equipmentList[0].TSE_Id, device.V3_HardwareIdentifier);
		}
	}
}
