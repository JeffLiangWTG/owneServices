using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DangerousGoodsManifestMessageLookupsTest : TestCaseWithFactory
	{
		#region Port

		public void TestPortList()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL", false);
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
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
				destinations2.JB_RL_NKPortOfDischarge = "AUARD";
				destinations2.JB_E_ARV = new DateTime(2022, 12, 02);

				var origin3 = voyage.Origins.AddNew();
				origin3.FillWithValidTestData();
				origin3.JA_RL_NKPortOfLoading = "AUBNE";
				origin3.JA_E_DEP = new DateTime(2022, 12, 05);

				Factory.Save();

				var message = new DangerousGoodsManifestMessage(voyage);
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "AUSYD", "AUBNE" }, message.Lookups.Port_List.GetAllCodesZString());
			}
		}

		#endregion

		#region Principal

		public void TestPrincipalList()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage(true));

				message.Port = ZString.Empty;

				AssertEquals(0, message.Lookups.Principal_List.Count);

				message.Port = "AUSYD";
				message.Direction = ZString.Empty;
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH1" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.Port = "AUMEL";
				message.Direction = ZString.Empty;
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH1", "OH2", "OH3" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.Port = "AUMEL";
				message.Direction = Constants.PortDirection.Load;
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH2", "OH3" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.Port = "AUMEL";
				message.Direction = Constants.PortDirection.Discharge;
				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH1" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.Port = "AUMEL";
				message.Direction = "Invalid";
				AssertEquals(0, message.Lookups.Principal_List.Count);

				message.Port = "AUBNE";
				message.Direction = ZString.Empty;
				AssertEquals(0, message.Lookups.Principal_List.Count);
			}
		}

		#endregion

		#region Direction

		public void TestDirectionList()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL");

			var voyage = CreateVoyage(false);

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(voyage);

				message.Port = ZString.Empty;

				AssertEquals(0, message.Lookups.Direction_List.Count);

				message.Port = "AUSYD";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "AUMEL";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load, Constants.PortDirection.Transit, Constants.PortDirection.Discharge }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "AUBNE";
				AssertEquals(0, message.Lookups.Direction_List.Count);
			}

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");
			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(voyage);

				message.Port = ZString.Empty;

				AssertEquals(0, message.Lookups.Direction_List.Count);

				message.Port = "AUSYD";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "AUMEL";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load, Constants.PortDirection.Transit, Constants.PortDirection.Discharge }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "AUBNE";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Discharge }, message.Lookups.Direction_List.GetAllCodesZString());
			}

			voyage.Origins.RemoveAndDelete(voyage.Origins.OfType<VoyageOrigin>().First(x => x.JA_RL_NKPortOfLoading == "AUMEL"));
			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(voyage);

				message.Port = ZString.Empty;

				AssertEquals(0, message.Lookups.Direction_List.Count);

				message.Port = "AUSYD";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "AUMEL";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Transit, Constants.PortDirection.Discharge }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "AUBNE";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Discharge }, message.Lookups.Direction_List.GetAllCodesZString());
			}

			voyage.Destinations.RemoveAndDelete(voyage.Destinations.OfType<VoyageDestination>().First(x => x.JB_RL_NKPortOfDischarge == "AUMEL"));
			var aumelOrigin = voyage.Origins.AddNew();
			aumelOrigin.FillWithValidTestData();
			aumelOrigin.JA_RL_NKPortOfLoading = "AUMEL";
			aumelOrigin.JA_E_DEP = new DateTime(2022, 12, 01);
			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(voyage);

				message.Port = ZString.Empty;

				AssertEquals(0, message.Lookups.Direction_List.Count);

				message.Port = "AUSYD";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "AUMEL";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load, Constants.PortDirection.Transit }, message.Lookups.Direction_List.GetAllCodesZString());

				message.Port = "AUBNE";
				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Discharge }, message.Lookups.Direction_List.GetAllCodesZString());
			}
		}

		#endregion

		#region MessageType

		public void TestMessageTypeList()
		{
			var message = new DangerousGoodsManifestMessage(Factory.New<JobVoyage>());

			AssertEquals(3, message.Lookups.MessageType_List.Count);

			var dangerousGoodsManifestMessageTypes = new List<ZString>()
			{
				DangerousGoodsManifestMessageTypeList.Codes.Original,
				DangerousGoodsManifestMessageTypeList.Codes.Amendment,
				DangerousGoodsManifestMessageTypeList.Codes.Withdrawal
			};

			AssertContainsExactElementsInAnyOrder(dangerousGoodsManifestMessageTypes, message.Lookups.MessageType_List.GetAllCodes());

			AssertEquals(DangerousGoodsManifestMessageTypeList.Descriptions.Original, message.Lookups.MessageType_List.GetDescriptionFromCode(DangerousGoodsManifestMessageTypeList.Codes.Original));
			AssertEquals(DangerousGoodsManifestMessageTypeList.Descriptions.Amendment, message.Lookups.MessageType_List.GetDescriptionFromCode(DangerousGoodsManifestMessageTypeList.Codes.Amendment));
			AssertEquals(DangerousGoodsManifestMessageTypeList.Descriptions.Withdrawal, message.Lookups.MessageType_List.GetDescriptionFromCode(DangerousGoodsManifestMessageTypeList.Codes.Withdrawal));
		}

		#endregion

		#region Implementation

		JobVoyage CreateVoyage(ZBool isIncludeBillOfLading)
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

			if (isIncludeBillOfLading)
			{
				var billOfLading1 = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading1.JS_OH_DeliveryAgent = OrgHeader1.PK;
				billOfLading1.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "AUMEL").PK;

				var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading2.JS_OH_DeliveryAgent = OrgHeader2.PK;
				billOfLading2.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUMEL" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE").PK;

				var billOfLading3 = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading3.JS_OH_DeliveryAgent = OrgHeader3.PK;
				billOfLading3.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUMEL" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE").PK;

				var billOfLading4 = Factory.NewWithValidTestData<BillOfLading>();
				billOfLading4.JS_OH_DeliveryAgent = OrgHeader2.PK;
				billOfLading4.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUMEL" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE").PK;
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
