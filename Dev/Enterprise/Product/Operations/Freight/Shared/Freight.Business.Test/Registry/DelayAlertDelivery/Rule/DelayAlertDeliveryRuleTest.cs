using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DelayAlertDeliveryRule))]
	sealed class DelayAlertDeliveryRuleTest : RegistryBusinessObjectTemplateTestCase<DelayAlertDeliveryRule>
	{
		public void TestValidateTransportMode()
		{
			const string error1 = "Enter a valid Transport Mode.";
			const string error2 = "Please enter a Transport Mode.";

			DelayAlertDeliveryRule delivery = new DelayAlertDeliveryRule();
			delivery.TransportMode = Constants.TransportModes.Sea;
			AssertNoNotifications(delivery.TransportModeInfo);

			delivery.TransportMode = "XXX";
			AssertHasError(delivery.TransportModeInfo, error1);

			delivery.TransportMode = Constants.TransportModes.All;
			AssertNoNotifications(delivery.TransportModeInfo);

			delivery.TransportMode = "";
			AssertHasError(delivery.TransportModeInfo, error2);
		}

		public void TestValidateModule()
		{
			const string error1 = "Enter a valid Module.";
			const string error2 = "Please enter a Module.";

			DelayAlertDeliveryRule delivery = new DelayAlertDeliveryRule();
			delivery.Module = DelayAlertDeliveryModules.Codes.ShippingManager;
			AssertNoNotifications(delivery.ModuleInfo);

			delivery.Module = "XXX";
			AssertHasError(delivery.ModuleInfo, error1);

			delivery.Module = DelayAlertDeliveryModules.Codes.All;
			AssertNoNotifications(delivery.ModuleInfo);

			delivery.Module = "";
			AssertHasError(delivery.ModuleInfo, error2);
		}

		public void TestValidateDirection()
		{
			const string error1 = "Enter a valid Direction.";
			const string error2 = "Please enter a Direction.";

			DelayAlertDeliveryRule delivery = new DelayAlertDeliveryRule();
			delivery.Direction = DelayAlertDeliveryDirections.Codes.Export;
			AssertNoNotifications(delivery.DirectionInfo);

			delivery.Direction = "XXX";
			AssertHasError(delivery.DirectionInfo, error1);

			delivery.Direction = DelayAlertDeliveryDirections.Codes.All;
			AssertNoNotifications(delivery.DirectionInfo);

			delivery.Direction = "";
			AssertHasError(delivery.DirectionInfo, error2);
		}

		public void TestRateMatch()
		{
			DelayAlertDeliveryRule[] rules =
			{
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.All,
					Module = DelayAlertDeliveryModules.Codes.All,
					Direction = DelayAlertDeliveryDirections.Codes.All,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.Sea,
					Module = DelayAlertDeliveryModules.Codes.All,
					Direction = DelayAlertDeliveryDirections.Codes.All,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.All,
					Module = DelayAlertDeliveryModules.Codes.ShippingManager,
					Direction = DelayAlertDeliveryDirections.Codes.All,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.All,
					Module = DelayAlertDeliveryModules.Codes.All,
					Direction = DelayAlertDeliveryDirections.Codes.Import,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.Air,
					Module = DelayAlertDeliveryModules.Codes.All,
					Direction = DelayAlertDeliveryDirections.Codes.All,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.All,
					Module = DelayAlertDeliveryModules.Codes.Forwarding,
					Direction = DelayAlertDeliveryDirections.Codes.All,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.All,
					Module = DelayAlertDeliveryModules.Codes.All,
					Direction = DelayAlertDeliveryDirections.Codes.Export,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.Sea,
					Module = DelayAlertDeliveryModules.Codes.ShippingManager,
					Direction = DelayAlertDeliveryDirections.Codes.All,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.All,
					Module = DelayAlertDeliveryModules.Codes.Forwarding,
					Direction = DelayAlertDeliveryDirections.Codes.Export,
				},
				new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.Air,
					Module = DelayAlertDeliveryModules.Codes.Forwarding,
					Direction = DelayAlertDeliveryDirections.Codes.Import,
				},
			};

			const string expected1 =
				"(ALL, ALL, ALL)\r\n" +
				"(SEA, ALL, ALL)\r\n" +
				"(ALL, SHP, ALL)\r\n" +
				"(ALL, ALL, IMP)\r\n" +
				"(SEA, SHP, ALL)\r\n" +
				"";

			AssertMultilineASCIIEquals("", expected1, Render(Filter(rules, Constants.TransportModes.Sea, DelayAlertDeliveryModules.Codes.ShippingManager, DelayAlertDeliveryDirections.Codes.Import)));

			const string expected2 =
				"(ALL, ALL, ALL)\r\n" +
				"(AIR, ALL, ALL)\r\n" +
				"(ALL, FOR, ALL)\r\n" +
				"(ALL, ALL, EXP)\r\n" +
				"(ALL, FOR, EXP)\r\n" +
				"";

			AssertMultilineASCIIEquals("", expected2, Render(Filter(rules, Constants.TransportModes.Air, DelayAlertDeliveryModules.Codes.Forwarding, DelayAlertDeliveryDirections.Codes.Export)));

			const string expected3 =
				"(ALL, ALL, ALL)\r\n" +
				"(ALL, ALL, IMP)\r\n" +
				"(AIR, ALL, ALL)\r\n" +
				"(ALL, FOR, ALL)\r\n" +
				"(AIR, FOR, IMP)\r\n" +
				"";

			AssertMultilineASCIIEquals("", expected3, Render(Filter(rules, Constants.TransportModes.Air, DelayAlertDeliveryModules.Codes.Forwarding, DelayAlertDeliveryDirections.Codes.Import)));
		}

		public void TestDefaultValues()
		{
			CombineAssertions(delegate
			{
				DelayAlertDeliveryRule delivery = new DelayAlertDeliveryRule();
				AssertEquals("TransportMode", Constants.TransportModes.All, delivery.TransportMode);
				AssertEquals("Module", DelayAlertDeliveryModules.Codes.All, delivery.Module);
				AssertEquals("Direction", DelayAlertDeliveryDirections.Codes.All, delivery.Direction);
			});
		}

		public void TestSerialisation()
		{
			const string xml =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<DelayAlertDeliveryRule>" +
					"<TransportMode>SEA</TransportMode>" +
					"<Module>SHP</Module>" +
					"<Direction>EXP</Direction>" +
					"<Deliver />" +
				"</DelayAlertDeliveryRule>" +
				"";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(DelayAlertDeliveryRule));

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				DelayAlertDeliveryRule delivery = new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.Sea,
					Module = DelayAlertDeliveryModules.Codes.ShippingManager,
					Direction = DelayAlertDeliveryDirections.Codes.Export,
				};

				serializer.Serialize(xmlWriter, delivery);

				xmlWriter.Flush();

				AssertMultilineASCIIEquals("", xml.Replace("><", ">\r\n<"), writer.ToString().Replace("><", ">\r\n<"));
			}

			using (StringReader reader = new StringReader(xml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				DelayAlertDeliveryRule delivery = (DelayAlertDeliveryRule)serializer.Deserialize(xmlReader);

				CombineAssertions(delegate
				{
					AssertEquals("TransportMode", Constants.TransportModes.Sea, delivery.TransportMode);
					AssertEquals("Module", DelayAlertDeliveryModules.Codes.ShippingManager, delivery.Module);
					AssertEquals("Direction", DelayAlertDeliveryDirections.Codes.Export, delivery.Direction);
				});
			}
		}

		#region Implementation

		protected override DelayAlertDeliveryRule GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override DelayAlertDeliveryRule GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		DelayAlertDeliveryRule NewPopulatedBusinessObject()
		{
			DelayAlertDeliveryRule result = new DelayAlertDeliveryRule();
			result.TransportMode = Constants.TransportModes.Sea;
			result.Direction = DelayAlertDeliveryDirections.Codes.Export;
			result.Module = DelayAlertDeliveryModules.Codes.ShippingManager;
			return result;
		}

		DelayAlertDeliveryRule[] Filter(IEnumerable<DelayAlertDeliveryRule> rules, string transportMode, string module, string direction)
		{
			List<DelayAlertDeliveryRule> list = new List<DelayAlertDeliveryRule>();
			List<int> keyList = new List<int>();

			foreach (DelayAlertDeliveryRule rule in rules)
			{
				if (rule.Matches(transportMode, module, direction))
				{
					list.Add(rule);
				}
			}

			return list.ToArray();
		}

		string Render(DelayAlertDeliveryRule[] rules)
		{
			StringBuilder builder = new StringBuilder();

			foreach (DelayAlertDeliveryRule rule in rules)
			{
				builder.Append('(');
				builder.Append(rule.TransportMode);
				builder.Append(", ");
				builder.Append(rule.Module);
				builder.Append(", ");
				builder.Append(rule.Direction);
				builder.Append(')');
				builder.AppendLine();
			}

			return builder.ToString();
		}

		#endregion
	}
}
