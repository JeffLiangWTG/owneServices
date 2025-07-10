using System;
using CargoWise.ComponentModel;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class OGAFC01Test : OGABIRDUpdateTest
	{
		public override void TestUpdate()
		{
			foreach (var ogaRecord in GetPopulatedOGARecords())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EnableENS = true;

				declaration.Invoices.AddNew();

				var invoiceLine = declaration.InvoiceLines.AddNew();
				var ogaLine = CreateOGALine(invoiceLine);

				PrepareJob(invoiceLine, ogaLine, ogaRecord);

				var notifications = new NotificationCollection();
				ogaRecord.Update(ogaLine, notifications);

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				var builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true);
				var message = builder.PopulateMessage();

				AssertNotContains(ogaRecord.Serialise(), message.EM_MessageText);
			}
		}

		protected override IBIRDOGALineRecord[] GetPopulatedOGARecords()
		{
			var fcc01 = new OGAFC01();
			fcc01.ImportConditionNumber = FCCImportConditionNumberList.Codes._01;
			fcc01.ImportConditionNumberQuantityApproval = "Y";
			fcc01.FCCIdentifier = "980w-9802347890";
			fcc01.FCCLineNumber = 1;
			fcc01.TradeName = "Radio Radio";
			fcc01.ModelTypeNumber = "84RU-324";

			return new IBIRDOGALineRecord[] { fcc01 };
		}

		protected override IOGALine CreateOGALine(JobComInvoiceLine invoiceLine) => invoiceLine.FCCs.AddNew();

		protected override void PrepareJob(JobComInvoiceLine invoiceLine, IOGALine ogaLine, IBIRDOGALineRecord ogaRecord)
		{
			base.PrepareJob(invoiceLine, ogaLine, ogaRecord);
			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(OGAFC01);
	}
}
