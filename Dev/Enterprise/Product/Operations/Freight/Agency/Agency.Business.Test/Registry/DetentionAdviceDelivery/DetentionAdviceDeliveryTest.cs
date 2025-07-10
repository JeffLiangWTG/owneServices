using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DetentionAdviceDelivery))]
	internal class DetentionAdviceDeliveryTest : RegistryBusinessObjectTemplateTestCase<DetentionAdviceDelivery>
	{
		public void TestSerialisation()
		{
			const string expected = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" + "<DetentionAdviceDelivery>\r\n" + "  <Mode>PRN</Mode>\r\n" + "  <SendToNotificationGroup>N</SendToNotificationGroup>\r\n" + "  <Printer>37b6a7e5-7f06-4798-9f80-d2f06452f0d6</Printer>\r\n" + "  <NotificationGroup>00000000-0000-0000-0000-000000000000</NotificationGroup>\r\n" + "</DetentionAdviceDelivery>\r\n" + "";
			{
				DetentionAdviceDelivery delivery = new DetentionAdviceDelivery(Factory);
				delivery.Mode = DetentionAdviceDeliveryMode.Codes.Print;
				delivery.SendToNotificationGroup = false;
				delivery.Printer = new ZGuid("{37b6a7e5-7f06-4798-9f80-d2f06452f0d6}");
				delivery.NotificationGroup = ZGuid.Empty;
				using (TextWriter writer = new StringWriter())
				{
					ZXmlSerializer serializer = ZXmlSerializer.New(typeof(DetentionAdviceDelivery));
					serializer.Serialize(writer, delivery);
					AssertMultilineASCIIEquals("", expected, writer.ToString());
				}
			}

			{
				using (TextReader reader = new StringReader(expected))
				{
					ZXmlSerializer serializer = ZXmlSerializer.New(typeof(DetentionAdviceDelivery));
					DetentionAdviceDelivery delivery = (DetentionAdviceDelivery)serializer.Deserialize(reader);
					AssertEquals("Mode", DetentionAdviceDeliveryMode.Codes.Print, delivery.Mode);
					AssertEquals("SendToNotificationGroup", false, delivery.SendToNotificationGroup);
					AssertEquals("Printer", new ZGuid("{37B6A7E5-7F06-4798-9F80-D2F06452F0D6}"), delivery.Printer);
					AssertEquals("NotificationGroup", ZGuid.Empty, delivery.NotificationGroup);
				}
			}
		}

		public void TestPrinterReadOnlyness()
		{
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Auto;
			AssertEquals(false, Delivery.PrinterInfo.ReadOnly);
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Notify;
			AssertEquals(true, Delivery.PrinterInfo.ReadOnly);
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Print;
			AssertEquals(false, Delivery.PrinterInfo.ReadOnly);
		}

		public void TestNotificationGroupReadOnlyness()
		{
			Delivery.SendToNotificationGroup = false;
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Auto;
			AssertEquals(true, Delivery.NotificationGroupInfo.ReadOnly);
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Notify;
			AssertEquals(false, Delivery.NotificationGroupInfo.ReadOnly);
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Print;
			AssertEquals(true, Delivery.NotificationGroupInfo.ReadOnly);
			Delivery.SendToNotificationGroup = true;
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Auto;
			AssertEquals(false, Delivery.NotificationGroupInfo.ReadOnly);
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Notify;
			AssertEquals(false, Delivery.NotificationGroupInfo.ReadOnly);
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Print;
			AssertEquals(false, Delivery.NotificationGroupInfo.ReadOnly);
		}

		public void TestModeValidation()
		{
			const string error1 = "Enter a valid selection.";
			const string error2 = "Please enter a value.";
			Delivery.Mode = "XXX";
			AssertHasError(Delivery.ModeInfo, error1);
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Notify;
			AssertNoNotifications(Delivery.ModeInfo);
			Delivery.Mode = "";
			AssertHasError(Delivery.ModeInfo, error2);
		}

		public void TestPrinterValidation()
		{
			const string error1 = "Enter a valid selection.";
			const string error2 = "Please enter a value.";
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Blaticus";
			printer.SQ_AllowPrinting = true;
			Factory.Save();
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Print;
			Delivery.Printer = ZGuid.NewZGuid();
			AssertHasError(Delivery.PrinterInfo, error1);
			Delivery.Printer = printer.PK;
			AssertNoNotifications(Delivery.PrinterInfo);
			Delivery.Printer = ZGuid.Empty;
			AssertHasError(Delivery.PrinterInfo, error2);
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Notify;
			Delivery.ValidatePrinter();
			AssertNoNotifications(Delivery.PrinterInfo);
		}

		public void TestNotificationGroupValidation()
		{
			const string error1 = "Enter a valid selection.";
			const string error2 = "Please enter a value.";
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "GGG";
			Factory.Save();
			Delivery.Mode = DetentionAdviceDeliveryMode.Codes.Print;
			Delivery.SendToNotificationGroup = true;
			Delivery.NotificationGroup = ZGuid.NewZGuid();
			AssertHasError(Delivery.NotificationGroupInfo, error1);
			Delivery.NotificationGroup = group.PK;
			AssertNoNotifications(Delivery.NotificationGroupInfo);
			Delivery.NotificationGroup = ZGuid.Empty;
			AssertHasError(Delivery.NotificationGroupInfo, error2);
			Delivery.SendToNotificationGroup = false;
			delivery.ValidateNotificationGroup();
			AssertNoNotifications(Delivery.NotificationGroupInfo);
		}

		#region Implementation
		DetentionAdviceDelivery Delivery
		{
			get
			{
				return delivery ?? (delivery = new DetentionAdviceDelivery(Factory));
			}
		}

		DetentionAdviceDelivery delivery;
		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override DetentionAdviceDelivery GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override DetentionAdviceDelivery GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		DetentionAdviceDelivery NewPopulatedBusinessObject()
		{
			DetentionAdviceDelivery result = new DetentionAdviceDelivery(Factory);
			result.Mode = DetentionAdviceDeliveryMode.Codes.Notify;
			result.SendToNotificationGroup = false;
			result.Printer = ZGuid.NewZGuid();
			result.NotificationGroup = ZGuid.NewZGuid();
			return result;
		}
		#endregion
	}
}
