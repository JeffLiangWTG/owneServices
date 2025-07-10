using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	[TestedType(typeof(ProtestInvoicingSupporter))]
	sealed class ProtestInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestIJobInvoicingPlugIn_DefaultChargeGroup()
		{
			IJobInvoicingPlugIn protest = new Protest(Factory.New<JobDeclaration>());
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, protest.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestConsignee()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			var protestInvoicingPlugIn = (IJobInvoicingPlugIn)protest;
			protest.Protestant.OrganisationPK = Factory.New<OrgHeader>().PK;
			AssertEquals("Consignee should be the same as the protestant", protest.Protestant.Organisation, protestInvoicingPlugIn.InvoicingSupporter.Consignee);
		}

		public void TestServiceDirection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			IJobInvoicingPlugIn protest = new Protest(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			Assert(protest.InvoicingSupporter is IServiceDirection);
			AssertEquals(JobMessageTypeList.Codes.FTZ, ((IServiceDirection)protest.InvoicingSupporter).ServiceDirection);
		}

		public void TestCreateAccountingJobOnSavingOfOperationsJob()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.SuspendAddingWorkflow = true;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TST";
			var pro = new Protest(dec);
			pro.Protestant.OrganisationPK = orgHeader.PK;
			IJobInvoicingPlugIn protest = pro;
			AssertEquals(true, protest.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
			Factory.Save();
			AssertEquals(false, protest.InvoicingSupporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		public void TestGetOperationsSignificantDate()
		{
			var protest = new Protest(Factory.New<JobDeclaration>());
			var jobInvoicingPlugIn = (IJobInvoicingPlugIn)protest;

			AssertEquals(ZDateTime.Empty, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals(ZDateTime.Empty, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDateByDirection(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ""));

			var log = protest.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2008, 9, 11));

			AssertEquals(log.SL_EventTime, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals(log.SL_EventTime, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDateByDirection(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ""));

			log = protest.Logs.AddNew(Events.CustomsCleared, new ZDateTimeOffset(2009, 9, 11));

			AssertEquals(log.SL_EventTime, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDate(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals(log.SL_EventTime, jobInvoicingPlugIn.InvoicingSupporter.GetOperationsSignificantDateByDirection(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate, ""));
		}

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new Protest(jobDeclaration);
		}

		protected override ZString TestingCountry => Core.Constants.CountryCodes.UnitedStates;
	}
}
