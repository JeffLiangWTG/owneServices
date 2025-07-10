using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class RelatedJobsHelperTest : TestCaseWithFactory
	{
		public void TestGetRelatedBillOfLading()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.FillWithValidTestData();
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = new DateTime(2022, 11, 29);

			var origin2 = voyage.Origins.AddNew();
			origin2.FillWithValidTestData();
			origin2.JA_RL_NKPortOfLoading = "AUMEL";
			origin2.JA_E_DEP = new DateTime(2022, 12, 01);

			var destinations1 = voyage.Destinations.AddNew();
			destinations1.FillWithValidTestData();
			destinations1.JB_RL_NKPortOfDischarge = "AUMEL";
			destinations1.JB_E_ARV = new DateTime(2022, 11, 30);

			var destinations2 = voyage.Destinations.AddNew();
			destinations2.FillWithValidTestData();
			destinations2.JB_RL_NKPortOfDischarge = "AUBNE";
			destinations2.JB_E_ARV = new DateTime(2022, 12, 02);

			voyage.GenerateSailings();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "OH1";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "OH2";

			var billOfLading1 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading1.JS_OH_DeliveryAgent = orgHeader1.PK;
			billOfLading1.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "AUMEL").PK;

			var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading2.JS_OH_DeliveryAgent = orgHeader2.PK;
			billOfLading2.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUMEL" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE").PK;

			Factory.Save();

			var filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUSYD", ZString.Empty);
			var billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(1, billOfLadings.Length);
			AssertEquals(billOfLading1, billOfLadings.First());

			filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUSYD", Constants.PortDirection.Load);
			billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(1, billOfLadings.Length);
			AssertEquals(billOfLading1, billOfLadings.First());

			filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUSYD", Constants.PortDirection.Discharge);
			billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(0, billOfLadings.Length);

			filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUMEL", ZString.Empty);
			billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(2, billOfLadings.Length);
			Assert(billOfLadings.Contains(billOfLading1));
			Assert(billOfLadings.Contains(billOfLading2));

			filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUMEL", Constants.PortDirection.Load);
			billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(1, billOfLadings.Length);
			AssertEquals(billOfLading2, billOfLadings.First());

			filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUMEL", Constants.PortDirection.Discharge);
			billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(1, billOfLadings.Length);
			AssertEquals(billOfLading1, billOfLadings.First());
		}

		public void TestGetRelatedBillOfLading_Transit()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.FillWithValidTestData();
			origin1.JA_RL_NKPortOfLoading = "NZAKL";
			origin1.JA_E_DEP = new DateTime(2025, 03, 09);

			var destinations1 = voyage.Destinations.AddNew();
			destinations1.FillWithValidTestData();
			destinations1.JB_RL_NKPortOfDischarge = "AUMEL";
			destinations1.JB_E_ARV = new DateTime(2025, 03, 23);

			var destinations2 = voyage.Destinations.AddNew();
			destinations2.FillWithValidTestData();
			destinations2.JB_RL_NKPortOfDischarge = "AUSYD";
			destinations2.JB_E_ARV = new DateTime(2025, 03, 26);

			var destinations3 = voyage.Destinations.AddNew();
			destinations3.FillWithValidTestData();
			destinations3.JB_RL_NKPortOfDischarge = "AUBNE";
			destinations3.JB_E_ARV = new DateTime(2025, 03, 29);

			voyage.GenerateSailings();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "OH1";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "OH2";

			var billOfLading1 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading1.JS_OH_DeliveryAgent = orgHeader1.PK;
			billOfLading1.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "NZAKL" && x.JX_JB_RL_NKPortOfDischarge == "AUMEL").PK;

			var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading2.JS_OH_DeliveryAgent = orgHeader2.PK;
			billOfLading2.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "NZAKL" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE").PK;

			Factory.Save();

			var filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUMEL", Constants.PortDirection.Transit);
			var billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(1, billOfLadings.Length);
			AssertEquals(billOfLading2, billOfLadings.First());

			filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUBNE", Constants.PortDirection.Transit);
			billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(0, billOfLadings.Length);
		}

		public void TestGetRelatedBillOfLading_LoadAndDischarge()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.FillWithValidTestData();
			origin1.JA_RL_NKPortOfLoading = "AUEML";
			origin1.JA_E_DEP = new DateTime(2025, 03, 09);

			var origin2 = voyage.Origins.AddNew();
			origin2.FillWithValidTestData();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_E_DEP = new DateTime(2025, 03, 10);

			var destinations1 = voyage.Destinations.AddNew();
			destinations1.FillWithValidTestData();
			destinations1.JB_RL_NKPortOfDischarge = "AUSYD";
			destinations1.JB_E_ARV = new DateTime(2025, 03, 10);

			var destinations2 = voyage.Destinations.AddNew();
			destinations2.FillWithValidTestData();
			destinations2.JB_RL_NKPortOfDischarge = "AUBNE";
			destinations2.JB_E_ARV = new DateTime(2025, 03, 26);

			voyage.GenerateSailings();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OH1";

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_OH_DeliveryAgent = orgHeader.PK;
			billOfLading.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUEML" && x.JX_JB_RL_NKPortOfDischarge == "AUSYD").PK;

			var transport = billOfLading.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "AUBNE";
			transport.JW_IsLinked = true;
			transport.JW_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE").PK;

			Factory.Save();

			var filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUSYD", Constants.PortDirection.Load);
			var billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(1, billOfLadings.Length);
			AssertEquals(billOfLading, billOfLadings.First());

			filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUSYD", Constants.PortDirection.Transit);
			billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(0, billOfLadings.Length);

			filter = RelatedJobsHelper.GetRelatedBillOfLadingFilter(Factory, voyage, "AUSYD", Constants.PortDirection.Discharge);
			billOfLadings = Factory.Load<BillOfLading>(filter);

			AssertEquals(1, billOfLadings.Length);
			AssertEquals(billOfLading, billOfLadings.First());
		}
	}
}
