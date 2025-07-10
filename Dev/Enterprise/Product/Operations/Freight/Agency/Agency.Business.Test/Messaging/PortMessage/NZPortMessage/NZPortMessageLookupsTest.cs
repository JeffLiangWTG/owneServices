using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	class NZPortMessageLookupsTest : BaseAgencyTest
	{
		#region Port

		public void TestPortList()
		{
			var portCollection = new PortManifestPortCollection();

			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZAKL");
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZLYT", false);
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZNPE");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var origin1 = voyage.Origins.AddNew();
				origin1.FillWithValidTestData();
				origin1.JA_RL_NKPortOfLoading = "NZAKL";
				origin1.JA_E_DEP = new DateTime(2022, 11, 29);

				var origin2 = voyage.Origins.AddNew();
				origin2.FillWithValidTestData();
				origin2.JA_RL_NKPortOfLoading = "NZLYT";
				origin2.JA_E_DEP = new DateTime(2022, 12, 01);

				var destinations1 = voyage.Destinations.AddNew();
				destinations1.FillWithValidTestData();
				destinations1.JB_RL_NKPortOfDischarge = "NZLYT";
				destinations1.JB_E_ARV = new DateTime(2022, 11, 30);

				var destinations2 = voyage.Destinations.AddNew();
				destinations2.FillWithValidTestData();
				destinations2.JB_RL_NKPortOfDischarge = "NZPOE";
				destinations1.JB_E_ARV = new DateTime(2022, 12, 02);

				voyage.GenerateSailings();

				var origin3 = voyage.Origins.AddNew();
				origin3.FillWithValidTestData();
				origin3.JA_RL_NKPortOfLoading = "NZNPE";
				origin3.JA_E_DEP = new DateTime(2022, 12, 05);

				var nznpeSailings = new JobSailingCollection(Factory);

				foreach (JobSailing sailing in voyage.Sailings)
				{
					if (sailing.JX_JA_RL_NKPortOfLoading == "NZNPE")
					{
						nznpeSailings.Add(sailing);
					}
				}

				voyage.Sailings.RemoveRange(nznpeSailings);

				Factory.Save();

				AssertNull(voyage.Sailings.OfType<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "NZNPE"));

				var message = new NZPortMessage(voyage);

				AssertContainsExactElementsInAnyOrder(new List<ZString> { "NZAKL", "NZNPE" }, message.Lookups.Port_List.GetAllCodesZString());
			}
		}

		#endregion

		#region Principal

		public void TestPrincipalList()
		{
			var portCollection = new PortManifestPortCollection();

			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZAKL");
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZLYT");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new NZPortMessage(CreateVoyage(true));

				message.Port = ZString.Empty;

				AssertEquals(0, message.Lookups.Principal_List.Count);

				message.Port = "NZAKL";
				message.Direction = ZString.Empty;
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH1" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.Port = "NZLYT";
				message.Direction = ZString.Empty;
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH1", "OH2", "OH3" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.Port = "NZLYT";
				message.Direction = Constants.PortDirection.Load;
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH2", "OH3" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.Port = "NZLYT";
				message.Direction = Constants.PortDirection.Discharge;
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH1" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.Port = "NZLYT";
				message.Direction = "Invalid";
				AssertEquals(0, message.Lookups.Principal_List.Count);

				message.Port = "NZNPE";
				message.Direction = ZString.Empty;
				AssertEquals(0, message.Lookups.Principal_List.Count);
			}
		}

		#endregion

		#region Direction

		public void TestDirectionList()
		{
			var portCollection = new PortManifestPortCollection();

			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZAKL");
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZLYT");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new NZPortMessage(CreateVoyage(false));

				message.Port = ZString.Empty;

				AssertEquals(0, message.Lookups.Direction_List.Count);

				message.Port = "NZAKL";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "NZLYT";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load, Constants.PortDirection.Discharge }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "NZNPE";
				AssertEquals(0, message.Lookups.Direction_List.Count);
			}
		}

		#endregion

		#region Implementation

		JobVoyage CreateVoyage(ZBool isIncludeBillOfLading)
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();

			var origin1 = voyage.Origins.AddNew();
			origin1.FillWithValidTestData();
			origin1.JA_RL_NKPortOfLoading = "NZAKL";
			origin1.JA_E_DEP = new DateTime(2022, 11, 29);

			var origin2 = voyage.Origins.AddNew();
			origin2.FillWithValidTestData();
			origin2.JA_RL_NKPortOfLoading = "NZLYT";
			origin2.JA_E_DEP = new DateTime(2022, 12, 01);

			var destinations1 = voyage.Destinations.AddNew();
			destinations1.FillWithValidTestData();
			destinations1.JB_RL_NKPortOfDischarge = "NZLYT";
			destinations1.JB_E_ARV = new DateTime(2022, 11, 30);

			var destinations2 = voyage.Destinations.AddNew();
			destinations2.FillWithValidTestData();
			destinations2.JB_RL_NKPortOfDischarge = "NZNPE";
			destinations1.JB_E_ARV = new DateTime(2022, 12, 02);

			voyage.GenerateSailings();

			if (isIncludeBillOfLading)
			{
				var billOfLading1 = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading1.JS_OH_DeliveryAgent = OrgHeader1.PK;
				billOfLading1.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "NZAKL" && x.JX_JB_RL_NKPortOfDischarge == "NZLYT").PK;

				var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading2.JS_OH_DeliveryAgent = OrgHeader2.PK;
				billOfLading2.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "NZLYT" && x.JX_JB_RL_NKPortOfDischarge == "NZNPE").PK;

				var billOfLading3 = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading3.JS_OH_DeliveryAgent = OrgHeader3.PK;
				billOfLading3.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "NZLYT" && x.JX_JB_RL_NKPortOfDischarge == "NZNPE").PK;

				var billOfLading4 = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading4.JS_OH_DeliveryAgent = OrgHeader2.PK;
				billOfLading4.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "NZLYT" && x.JX_JB_RL_NKPortOfDischarge == "NZNPE").PK;
			}

			Factory.Save();

			return voyage;
		}

		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader1.OH_Code = "OH1";

			OrgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader2.OH_Code = "OH2";

			OrgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader3.OH_Code = "OH3";
		}

		OrgHeader OrgHeader1;
		OrgHeader OrgHeader2;
		OrgHeader OrgHeader3;

		#endregion
	}
}
