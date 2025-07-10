using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.MessageBuilders.CUSCAR;
using Enterprise.Customs.ZA.Manifest.Business.EDIFACT;

namespace Enterprise.Customs.ZA.Manifest.Business.MessagingProcess.Testing
{
	sealed class CustomsMessengerTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var (messengerInterface, header) = GetMessengerAndHeader();

			CombineAssertions("Pre-Requisites", () =>
			{
				AssertType<CustomsMessenger>("Interface implementation should be CustomsMessenger", messengerInterface);
			});

			CombineAssertions("Messenger Properties", () =>
			{
				AssertSame("Owner and Manifest Header should be the same Object", header, messengerInterface.Owner);
				AssertType<CUSCARMessageBuilder>("Generator should be CUSCARMessageBuilder", messengerInterface.MessageGenerator);
			});
		}

		public void TestICustomsMessenger_MessageGenerator()
		{
			var messenger = GetMessenger();
			AssertType<CUSCARMessageBuilder>(messenger.MessageGenerator);
		}

		public void TestICustomsMessenger_ShouldCreateMessage()
		{
			var messenger = GetMessenger();
			var ar = new ActionResult();

			CombineAssertions("ShouldCreateMessage", () =>
			{
				var shouldCreate = messenger.ShouldCreateMessage(ar);
				AssertEquals("No message created", true, shouldCreate);
			});
		}

		public void TestICustomsMessenger_ProcessUpdates()
		{
			var (messenger, header) = GetMessengerAndHeader();
			var bills = header.Bills.Cast<AsycudaBill>().ToList();

			var result = messenger.ProcessUpdates(new ActionResult(false));

			CombineAssertions("No change expected to messaging status when failure", () =>
			{
				AssertEquals("Header", ZString.Empty, header.AMA_MessageStatus);
				AssertEquals("Bill1", ZString.Empty, bills[0].ABL_MessageStatus);
				AssertEquals("Bill2", ZString.Empty, bills[1].ABL_MessageStatus);
				AssertEquals("Always true", true, result);
			});

			result = messenger.ProcessUpdates(new ActionResult(true));

			CombineAssertions("Only Header status should change", () =>
			{
				AssertEquals("Header", ZAMessageStatusList.Codes.AwaitingResponse, header.AMA_MessageStatus);
				AssertEquals("Bill1", ZString.Empty, bills[0].ABL_MessageStatus);
				AssertEquals("Bill2", ZString.Empty, bills[1].ABL_MessageStatus);
				AssertEquals("Always true", true, result);
			});
		}

		public void TestICustomsMessenger_ProcessUpdatesForBills()
		{
			var (messenger, header) = GetMessengerAndHeader(true);
			var bills = header.Bills.Cast<AsycudaBill>().ToList();

			var result = messenger.ProcessUpdates(new ActionResult(false));

			CombineAssertions("No change expected to messaging status when failure", () =>
			{
				AssertEquals("Header", ZString.Empty, header.AMA_MessageStatus);
				AssertEquals("Bill1", ZString.Empty, bills[0].ABL_MessageStatus);
				AssertEquals("Bill2", ZString.Empty, bills[1].ABL_MessageStatus);
				AssertEquals("Always true", true, result);
			});

			result = messenger.ProcessUpdates(new ActionResult(true));

			CombineAssertions("Only Header & first bill status should change", () =>
			{
				AssertEquals("Header", ZAMessageStatusList.Codes.AwaitingResponse, header.AMA_MessageStatus);
				AssertEquals("Bill1", ZAMessageStatusList.Codes.AwaitingResponse, bills[0].ABL_MessageStatus);
				AssertEquals("Bill2", ZString.Empty, bills[1].ABL_MessageStatus);
				AssertEquals("Always true", true, result);
			});
		}

		public void TestICustomsMessenger_Owner()
		{
			var (messengerInterface, header) = GetMessengerAndHeader();

			AssertSame("Owner is AsycudaManifestHeader", header, messengerInterface.Owner);
		}

		ICustomsMessenger GetMessenger(bool createBillMessenger = false) => GetMessengerAndHeader(createBillMessenger).messenger;

		(ICustomsMessenger messenger, AsycudaManifestHeader header) GetMessengerAndHeader(bool createBillMessenger = false)
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "ZA";
			header.AMA_ManifestType = createBillMessenger ? "ALH" : "ECL";
			header.AMA_ManifestNumber = "MAN12345";

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "B0001";
			bill1.ABL_BillIssuer = "AAA";

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "B0002";
			bill2.ABL_BillIssuer = "BBB";

			var cuscarHeader = new CusCarHeader(header);

			CustomsMessenger messengerInterface;

			if (createBillMessenger)
			{
				var singleBill = new CusCarMessagingHelper.SingleBillCusCarHeader(cuscarHeader, bill1);
				messengerInterface = CustomsMessenger.New(singleBill, singleBill, cuscarHeader, ZA.Business.MessageSubTypeCodes.Codes.Original, bill1.ABL_BillIssuer);
			}
			else
			{
				messengerInterface = CustomsMessenger.New(header, cuscarHeader, cuscarHeader, ZA.Business.MessageSubTypeCodes.Codes.Original, ZString.Empty);
			}

			return (messengerInterface, header);
		}
	}
}
