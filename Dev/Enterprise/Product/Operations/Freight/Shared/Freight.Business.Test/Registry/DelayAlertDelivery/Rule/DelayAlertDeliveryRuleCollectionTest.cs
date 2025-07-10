using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DelayAlertDeliveryRuleCollection))]
	sealed class DelayAlertDeliveryRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DelayAlertDeliveryRuleCollection>
	{
		public void TestIsEnabledForAny()
		{
			const string Sea = Constants.TransportModes.Sea;

			DelayAlertDeliveryRuleCollection collection = new DelayAlertDeliveryRuleCollection();

			AssertEquals(false, collection.IsEnabledFor(Sea));

			collection.Add(new DelayAlertDeliveryRule() { TransportMode = Sea });
			AssertEquals(true, collection.IsEnabledForAny());
		}

		public void TestIsEnabledFor()
		{
			const string Air = Constants.TransportModes.Air;
			const string Sea = Constants.TransportModes.Sea;

			DelayAlertDeliveryRuleCollection collection = new DelayAlertDeliveryRuleCollection();

			AssertEquals(false, collection.IsEnabledFor(Sea));

			collection.Add(new DelayAlertDeliveryRule() { TransportMode = Sea });
			AssertEquals(false, collection.IsEnabledFor(Air));
			AssertEquals(true, collection.IsEnabledFor(Sea));
		}

		public void TestShouldDeliverDelayalert()
		{
			const string Sea = Constants.TransportModes.Sea;
			const string Air = Constants.TransportModes.Air;
			const string Shipping = DelayAlertDeliveryModules.Codes.ShippingManager;
			const string Forwarding = DelayAlertDeliveryModules.Codes.Forwarding;
			const string Imp = DelayAlertDeliveryDirections.Codes.Import;
			const string Exp = DelayAlertDeliveryDirections.Codes.Export;

			DelayAlertDeliveryRuleCollection collection = new DelayAlertDeliveryRuleCollection();

			AssertEquals(false, collection.ShouldDeliverDelayAlert(Sea, Shipping, Exp));

			collection.Add(new DelayAlertDeliveryRule() { Direction = Exp });
			AssertEquals(true, collection.ShouldDeliverDelayAlert(Sea, Shipping, Exp));
			AssertEquals(false, collection.ShouldDeliverDelayAlert(Sea, Shipping, Imp));

			collection.Add(new DelayAlertDeliveryRule() { Module = Shipping, Direction = Imp });
			AssertEquals(true, collection.ShouldDeliverDelayAlert(Sea, Shipping, Imp));
			AssertEquals(false, collection.ShouldDeliverDelayAlert(Sea, Forwarding, Imp));

			collection.Add(new DelayAlertDeliveryRule() { TransportMode = Air, Module = Forwarding, Direction = Imp });
			AssertEquals(true, collection.ShouldDeliverDelayAlert(Air, Forwarding, Imp));
			AssertEquals(false, collection.ShouldDeliverDelayAlert(Sea, Forwarding, Imp));
		}

		public void TestSerialisation()
		{
			const string xml =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
				"<ArrayOfDelayAlertDeliveryRule xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
					"<DelayAlertDeliveryRule>" +
						"<TransportMode>SEA</TransportMode>" +
						"<Module>SHP</Module>" +
						"<Direction>EXP</Direction>" +
						"<Deliver />" +
					"</DelayAlertDeliveryRule>" +
					"<DelayAlertDeliveryRule>" +
						"<TransportMode>ALL</TransportMode>" +
						"<Module>ALL</Module>" +
						"<Direction>ALL</Direction>" +
						"<Deliver />" +
					"</DelayAlertDeliveryRule>" +
				"</ArrayOfDelayAlertDeliveryRule>";

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(DelayAlertDeliveryRuleCollection));

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				DelayAlertDeliveryRuleCollection collection = new DelayAlertDeliveryRuleCollection();

				collection.Add(new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.Sea,
					Module = DelayAlertDeliveryModules.Codes.ShippingManager,
					Direction = DelayAlertDeliveryDirections.Codes.Export,
				});

				collection.Add(new DelayAlertDeliveryRule()
				{
					TransportMode = Constants.TransportModes.All,
					Module = DelayAlertDeliveryModules.Codes.All,
					Direction = DelayAlertDeliveryDirections.Codes.All,
				});

				serializer.Serialize(xmlWriter, collection);

				xmlWriter.Flush();

				AssertXMLEquals("", xml.Replace("><", ">\r\n<"), writer.ToString().Replace("><", ">\r\n<"));
			}

			using (StringReader reader = new StringReader(xml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				DelayAlertDeliveryRuleCollection collection = (DelayAlertDeliveryRuleCollection)serializer.Deserialize(xmlReader);

				CombineAssertions(delegate
				{
					AssertEquals("[0].TransportMode", Constants.TransportModes.Sea, collection[0].TransportMode);
					AssertEquals("[0].Module", DelayAlertDeliveryModules.Codes.ShippingManager, collection[0].Module);
					AssertEquals("[0].Direction", DelayAlertDeliveryDirections.Codes.Export, collection[0].Direction);

					AssertEquals("[1].TransportMode", Constants.TransportModes.All, collection[1].TransportMode);
					AssertEquals("[1].Module", DelayAlertDeliveryModules.Codes.All, collection[1].Module);
					AssertEquals("[1].Direction", DelayAlertDeliveryDirections.Codes.All, collection[1].Direction);
				});
			}
		}

		#region Implementation

		protected override DelayAlertDeliveryRuleCollection GetCollectionToTest()
		{
			return new DelayAlertDeliveryRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DelayAlertDeliveryRule();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
