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
	internal class DangerousGoodsManifestMessageValidationTest : TestCaseWithFactory
	{
		public void TestValidatePort()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage());

				message.Port = ZString.Empty;
				message.Validation.ValidatePort();
				AssertHasError(message.PortInfo, "Please enter a Port.");

				message.Port = "AUSYD";
				AssertNoNotifications(message.PortInfo);

				message.Port = "DONT";
				AssertHasError(message.PortInfo, "Enter a valid Port.");
			}
		}

		public void TestValidatePrincipal()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL", false);

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage());

				message.Port = "AUSYD";

				AssertContainsExactElementsInAnyOrder(new List<ZString> { "OH1" }, message.Lookups.Principal_List.Select(x => x.OH_Code));

				message.PrincipalPK = ZGuid.Empty;

				AssertHasError(message.PrincipalPKInfo, "Please enter a Principal.");

				message.PrincipalPK = OrgHeader1.PK;

				AssertNoNotifications(message.PrincipalPKInfo);

				message.PrincipalPK = ZGuid.Invalid;

				AssertHasError(message.PrincipalPKInfo, "Enter a valid Principal.");
			}
		}

		public void TestValidateDirection()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL", false);
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage());

				message.Port = "AUSYD";
				message.Direction = Constants.PortDirection.Discharge;

				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Load }, message.Lookups.Direction_List.GetAllCodesZString());
				AssertHasError(message.DirectionInfo, "Enter a valid Direction.");

				message.Direction = ZString.Empty;

				AssertHasError(message.DirectionInfo, "Please enter a Direction.");

				message.Port = "AUBNE";
				message.Direction = Constants.PortDirection.Discharge;

				AssertContainsExactElementsInAnyOrder(new List<ZString> { Constants.PortDirection.Discharge }, message.Lookups.Direction_List.GetAllCodesZString());
				AssertNoNotifications(message.DirectionInfo);
			}
		}

		public void TestValidateMessageType()
		{
			var message = new DangerousGoodsManifestMessage(Factory.New<JobVoyage>());

			message.MessageType = "ABC";
			AssertHasError(message.MessageTypeInfo, "Enter a valid Message Type.");

			message.MessageType = ZString.Empty;
			AssertHasError(message.MessageTypeInfo, "Please enter a Message Type.");

			message.MessageType = MessagePurposes.Codes.Original;
			AssertNoNotifications(message.MessageTypeInfo);
		}

		#region Implementation

		JobVoyage CreateVoyage()
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

			var billOfLading1 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading1.JS_OH_DeliveryAgent = OrgHeader1.PK;
			billOfLading1.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUSYD" && x.JX_JB_RL_NKPortOfDischarge == "AUMEL").PK;

			var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading2.JS_OH_DeliveryAgent = OrgHeader2.PK;
			billOfLading2.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "AUMEL" && x.JX_JB_RL_NKPortOfDischarge == "AUBNE").PK;

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
		}

		OrgHeader OrgHeader1;
		OrgHeader OrgHeader2;

		#endregion
	}
}
