using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[TestedType(typeof(CBPEDIInterchange))]
	sealed class EDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEI_ReceiveTransmit()
		{
			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			interchange.EI_Status = EDIInterchange.Status.Queued;

			interchange.EI_ReceiveTransmit = ZString.Empty;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			AssertEquals("EI_SessionGUID will never be empty for eHub messages.", true, !interchange.EI_SessionGUID.IsEmpty);
			AssertNotNull("EI_SessionGUID will never be null for eHub messages.", interchange.EI_SessionGUID);

			Factory.Save();
			interchange = new BusinessObjectFactory().Load<CBPEDIInterchange>(interchange.PK);
			AssertNoExceptionThrown("No need to load stuff from factory if the property condition not match criteria", () => interchange.EI_Status = EDIInterchange.Status.Sent);
			AssertEquals(EDIInterchange.Status.Sent, interchange.EI_Status);
		}

		public void TestEI_Status()
		{
			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_Status = "BLA";
			AssertEquals("BLA", interchange.EI_Status);

			interchange.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals(Guid.Empty, interchange.EI_SessionGUID);

			interchange.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals(Guid.Empty, interchange.EI_SessionGUID);

			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			AssertEquals("EI_SessionGUID will never be empty for eHub messages.", true, !interchange.EI_SessionGUID.IsEmpty);
			AssertNotNull("EI_SessionGUID will never be null for eHub messages.", interchange.EI_SessionGUID);
		}

		public void TestEI_InterchangeText()
		{
			var interchange = Factory.New<CBPEDIInterchange>();
			AssertEquals("".PadRight(160), interchange.EI_InterchangeText);
			interchange.EI_HeaderText = "HELLO";
			AssertEquals("HELLO".PadRight(160), interchange.EI_InterchangeText);
			interchange.EI_FooterText = "WORLD";
			var headerText = "HELLO".PadRight(80);
			var footerText = "WORLD".PadRight(80);
			AssertEquals(headerText + footerText, interchange.EI_InterchangeText);
			interchange.EI_BodyText = "".PadRight(79, 'X');
			AssertEquals(headerText + "".PadRight(79, 'X') + " " + footerText, interchange.EI_InterchangeText);
			interchange.EI_BodyText = "".PadRight(80, 'X');
			AssertEquals(headerText + "".PadRight(80, 'X') + footerText, interchange.EI_InterchangeText);
			interchange.EI_BodyText = "".PadRight(81, 'X');
			AssertEquals(headerText + "".PadRight(81, 'X') + " ".PadRight(79) + footerText, interchange.EI_InterchangeText);
		}

		public void TestEI_InterchangeTextShort()
		{
			var interchange = Factory.New<CBPEDIInterchange>();
			AssertEquals("".PadRight(160), interchange.EI_InterchangeTextShort);
			interchange.EI_HeaderText = "HELLO";
			AssertEquals("HELLO".PadRight(160), interchange.EI_InterchangeTextShort);
			interchange.EI_FooterText = "WORLD";
			var headerText = "HELLO".PadRight(80);
			var footerText = "WORLD".PadRight(80);
			AssertEquals(headerText + footerText, interchange.EI_InterchangeTextShort);
			interchange.EI_BodyText = "".PadRight(79, 'X');
			AssertEquals(headerText + "".PadRight(79, 'X') + " " + footerText, interchange.EI_InterchangeTextShort);
			interchange.EI_BodyText = "".PadRight(80, 'X');
			AssertEquals(headerText + "".PadRight(80, 'X') + footerText, interchange.EI_InterchangeTextShort);
			interchange.EI_BodyText = "".PadRight(81, 'X');
			AssertEquals(headerText + "".PadRight(81, 'X') + " ".PadRight(79) + footerText, interchange.EI_InterchangeTextShort);
		}

		public void TestEI_InterchangeTextDetail()
		{
			var interchange = Factory.New<CBPEDIInterchange>();
			AssertEquals("".PadRight(160), interchange.EI_InterchangeTextDetail);
			interchange.EI_HeaderText = "HELLO";
			AssertEquals("HELLO".PadRight(160), interchange.EI_InterchangeTextDetail);
			interchange.EI_FooterText = "WORLD";
			var headerText = "HELLO".PadRight(80);
			var footerText = "WORLD".PadRight(80);
			AssertEquals(headerText + footerText, interchange.EI_InterchangeTextDetail);
			interchange.EI_BodyText = "".PadRight(79, 'X');
			AssertEquals(headerText + "".PadRight(79, 'X') + " " + footerText, interchange.EI_InterchangeTextDetail);
			interchange.EI_BodyText = "".PadRight(80, 'X');
			AssertEquals(headerText + "".PadRight(80, 'X') + footerText, interchange.EI_InterchangeTextDetail);
			interchange.EI_BodyText = "".PadRight(81, 'X');
			AssertEquals(headerText + "".PadRight(81, 'X') + " ".PadRight(79) + footerText, interchange.EI_InterchangeTextDetail);
		}

		public void TestSavingRecordsInterchangeNumberInFooter()
		{
			var interchange = CreateInterchange(Factory);
			interchange.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			interchange.EI_InterchangeType = ApplicationIdentifierCodeList.Codes.EntrySummary;

			CBPEDIInterchange interchange2 = CreateInterchange(Factory);
			interchange2.EI_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsExport;
			interchange2.EI_InterchangeType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			Factory.Save();
			Assert(!interchange.EI_HeaderText.Contains(interchange.EI_InterchangeNum.PadLeft(11, '0')));
			Assert(!interchange.EI_FooterText.Contains(interchange.EI_InterchangeNum.PadLeft(11, '0')));
			AssertEquals("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", interchange2.EI_HeaderText);
			AssertEquals("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", interchange2.EI_FooterText);
		}

		CBPEDIInterchange CreateInterchange(BusinessObjectFactory factory)
		{
			var interchange = factory.New<CBPEDIInterchange>();
			interchange.EI_From = "HEY";
			interchange.EI_To = "YOU";
			interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Transmit;
			interchange.EI_HeaderText = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			interchange.EI_FooterText = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return interchange;
		}
	}
}
