using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Moq;
using WTG.Telematics.Common.Conversion;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors
{
	abstract class RimRegistrationMessageProcessorTestBase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
			baseTime = new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(10));
			devices = new List<(string id, DateTimeOffset time)>
			{
				("01020304", baseTime),
				("FFFFFFFF", baseTime),
				("04030201", baseTime),
			};
		}

		protected DateTimeOffset baseTime;
		protected List<(string id, DateTimeOffset time)> devices;

		protected Dictionary<string, GlbDevice> SetupGlbDevices(List<string> ids)
		{
			var dict = new Dictionary<string, GlbDevice>();
			for (var i = 0; i < ids.Count; i++)
			{
				var device = Factory.New<GlbDevice>();
				device.V3_HardwareIdentifier = ids[i];
				device.V3_HumanReadableIdentifier = $"WTG-00{i}";
				device.V3_HardwareKind = GlbDeviceKindCodes.WTGEmbedded;
				device.V3_IsActive = true;
				device.V3_MobileServicesIdentifier = BinaryDataConverter.HexStringToByteArray(ids[i]);
				device.V3_Model = "IVU";
				dict.Add(device.V3_HardwareIdentifier, device);
			}
			Factory.Save();
			return dict;
		}

		protected Dictionary<string, RefEquipment> SetupRefEquipment(List<string> ids)
		{
			var dict = new Dictionary<string, RefEquipment>();
			for (var i = 0; i < ids.Count; i++)
			{
				var equipment = Factory.New<RefEquipment>();
				equipment.RQ_ShortCode = $"WTG-{ids[i]}";
				dict.Add(ids[i], equipment);
			}
			Factory.Save();
			return dict;
		}

		protected void SetupRefEquipmentAndGlbDevices(List<(string id, DateTimeOffset time)> registrationTuple)
		{
			var ids = registrationTuple.Select(tuple => tuple.id).ToList();
			var glbDeviceDict = SetupGlbDevices(ids);
			var refEquipmentDict = SetupRefEquipment(ids);
			foreach (var tuple in registrationTuple)
			{
				var divot = Factory.New<GlbDeviceAssignmentDivot>();
				divot.V7_V3_Device = glbDeviceDict[tuple.id].PK;
				divot.V7_ParentID = refEquipmentDict[tuple.id].PK;
				divot.V7_StartTimeUtc = tuple.time.UtcDateTime;
				divot.V7_ParentTableCode = "RQ";
			}
			Factory.Save();
		}

		protected Mock<ILogger> loggerMock;
	}
}
