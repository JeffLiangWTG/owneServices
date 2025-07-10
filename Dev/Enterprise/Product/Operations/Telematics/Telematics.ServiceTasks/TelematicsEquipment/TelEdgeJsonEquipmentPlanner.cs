using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using WTG.Telematics.Common.Conversion;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	class TelEdgeJsonEquipmentPlanner : IJsonPlanner
	{
		internal TelEdgeJsonEquipmentPlanner()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Database constant")]
		public EquipmentFlatJson FlattenJson(string deviceIdentifier, string vehicleInfoJson)
		{
			var equipmentDict = new EquipmentFlatJson();
			var parsedJson = JObject.Parse(vehicleInfoJson);
			var configuration = new XElement(
				"Configuration",
				new XElement("vehicleType", parsedJson["vehicleType"].Value<string>()),
				new XElement("axleConfig", parsedJson["axleConfig"].Value<string>())
			);
			var vehicleEquipment = new TelSubEquipmentData()
			{
				id = deviceIdentifier,
				type = "RQ",
				configuration = string.Format(CultureInfo.InvariantCulture, configuration.ToString()),
			};
			equipmentDict.Add(
				new TelSubEquipmentKey()
				{
					id = vehicleEquipment.id,
					type = vehicleEquipment.type,
				},
				(new TelSubEquipmentData(), vehicleEquipment));
			GenerateAxlesGroupEquipment(vehicleEquipment, parsedJson["axleGroups"].Children().ToList(), equipmentDict);
			return equipmentDict;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Database constant")]
		void GenerateAxlesGroupEquipment(TelSubEquipmentData parentVehicleEquipment, IList<JToken> axleGroups, EquipmentFlatJson equipmentDict)
		{
			for (var i = 0; i < axleGroups.Count; i++)
			{
				var configuration = new XElement("Configuration",
					new XElement("tareMass", axleGroups[i]["tareMass"].Value<float?>()),
					new XElement("tarePressure", axleGroups[i]["tarePressure"].Value<float?>()),
					new XElement("calibrationMass", axleGroups[i]["calibrationMass"].Value<float?>()),
					new XElement("calibrationPressure", axleGroups[i]["calibrationPressure"].Value<float?>()),
					new XElement("massLimit", axleGroups[i]["massLimit"].Value<float?>())
				);
				var axleGroupEquipment = new TelSubEquipmentData()
				{
					id = BinaryDataConverter.ByteArrayToHexString(new byte[] { (byte)i }),
					type = "A",
					configuration = string.Format(CultureInfo.InvariantCulture, configuration.ToString()),
				};
				equipmentDict.Add(
					new TelSubEquipmentKey()
					{
						id = axleGroupEquipment.id,
						type = axleGroupEquipment.type
					},
					(parentVehicleEquipment, axleGroupEquipment));
				GenerateObmEquipment(axleGroupEquipment, axleGroups[i]["obms"].Children().ToList(), equipmentDict);
			}
		}

		static void GenerateObmEquipment(TelSubEquipmentData axleGroup, IList<JToken> obmList, EquipmentFlatJson equipmentDict)
		{
			foreach (var obm in obmList)
			{
				var obmEquipment = new TelSubEquipmentData()
				{
					id = BinaryDataConverter.ByteArrayToHexString(new byte[] { (byte)obm["obmId"].Value<int>() }),
					type = "O",
					configuration = string.Empty,
				};
				equipmentDict.Add(
					new TelSubEquipmentKey()
					{
						id = obmEquipment.id,
						type = obmEquipment.type
					},
					(axleGroup, obmEquipment));
			}
		}
	}
}
