using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class PGAPG04Test : TestCaseWithFactory
	{
		public void TestUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();

			var pgaLine = invoiceLine.LaceyActLines.AddNew();
			var constituentElement = pgaLine.PG04ConstituentElements.AddNew();

			var record = new PGAPG04();
			record.NameOfTheConstituentElement = "PINE";
			record.PercentOfConstituentElement = 0.999m;
			record.QuantityOfConstituentElement = 39398.99m;
			record.UnitOfMeasure = "KG";

			var notifications = new NotificationBuffer();
			((IBIRDPGAConstituentRecord)record).Update(constituentElement, notifications);

			Assert(!notifications.HasWarnings);
			Assert(!notifications.HasErrors);

			AssertEquals("PINE", constituentElement.US_PGANameOfTheConstituentElement);
			AssertEquals(0.999m, constituentElement.US_PGAPercentOfConstituentElement);
			AssertEquals(39398.99m, constituentElement.US_PGAQuantityOfConstituentElement);
			AssertEquals("KG", constituentElement.US_PGAUnitOfMeasure);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true);
			var messageTextToCompare = builder.PopulateMessage().EM_MessageText.Replace(MQEDIMessage.USEntryNumberPlaceHolder, declaration.ImportEntryNumber);
			AssertContains(record.Serialise(), messageTextToCompare);
		}
	}
}
