using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(TestCustomsManifestStatus))]
	sealed class CustomsManifestStatusConcreteTest : CustomsManifestStatusTest
	{
		public override void TestDeclareManifestWhenWeHaveErrors()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			TestCustomsManifestStatus testStatus = (TestCustomsManifestStatus)NewCustomsManifestStatus(Consol);
			Consol.HasChanges = false;
			testStatus.MessageBuilder.IsTestingErrorSituation = true;
			testStatus.DeclareManifest(sender);
			Assert("Unable to send", sender.InvalidOperationText.IndexOf("Unable to send") != -1);
		}

		protected override EDIMessage AddValidMessage(IManifestProvider manifestProvider)
		{
			TestMessage result = Factory.New<TestMessage>();
			result.EM_LinkedObject = Consol;
			result.EM_ReceiveTransmit = "TRX";
			result.EM_ApplicationCode = "xxx";
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + "'" + EDIMessage.SendersReferencePlaceHolder;
			result.EM_Status = "AWT";

			Consol.Messages.Add(result);
			result.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			return result;
		}

		protected override EDIMessage AddValidResponse(IManifestProvider manifestProvider)
		{
			TestMessage result = Factory.New<TestMessage>();
			result.EM_LinkedObject = Consol;
			result.EM_ReceiveTransmit = "RCV";
			result.EM_ApplicationCode = "xxx";
			result.EM_Status = "SNT";

			TestMessage lastOutgoing = (TestMessage)Consol.Messages.LastOutgoingMessage;
			lastOutgoing.EM_Status = "CLD";

			var entryNumber = Factory.LoadTop1<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, Consol.PK));
			if (entryNumber == null)
			{
				entryNumber = Factory.New<CusEntryNumber>();
				entryNumber.CE_EntryNum = "TTT";
				entryNumber.CE_ParentID = Consol.PK;
				entryNumber.CE_ParentTable = Consol.TableName;
			}

			Consol.Messages.Add(result);
			result.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			return result;
		}

		protected override IManifestProvider NewManifestProvider()
		{
			return Consol;
		}

		protected override ZString ExpectedCustomsEntryNumber
		{
			get { return "TTT"; }
		}

		TestConsol fConsol;
		TestConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<TestConsol>();
				}
				return fConsol;
			}
		}

		protected override CustomsManifestStatus NewCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			return new TestCustomsManifestStatus(Consol);
		}
	}
}
