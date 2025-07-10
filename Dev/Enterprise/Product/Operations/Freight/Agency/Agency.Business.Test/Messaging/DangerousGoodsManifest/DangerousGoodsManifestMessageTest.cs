using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(DangerousGoodsManifestMessage))]
	internal sealed class DangerousGoodsManifestMessageBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DangerousGoodsManifestMessage(Factory.New<JobVoyage>());
		}

		#endregion
	}

	internal class DangerousGoodsManifestMessageTest : TestCaseWithFactory
	{
		#region Port

		public void TestPortSetter_ShouldUpdatePrincipalAndDirection()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage());
				message.Port = ZString.Empty;
				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;

				message.Port = "AUSYD";
				CombineAssertions(() =>
				{
					AssertEquals(1, message.Lookups.Direction_List.Count);
					AssertEquals(1, message.Lookups.Principal_List.Count);

					AssertEquals(Constants.PortDirection.Load, message.Direction);
					AssertEquals(OrgHeader1.PK, message.PrincipalPK);
				});

				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;
				message.Port = "AUMEL";
				CombineAssertions(() =>
				{
					AssertEquals(3, message.Lookups.Direction_List.Count);
					AssertEquals(2, message.Lookups.Principal_List.Count);

					AssertEquals(ZString.Empty, message.Direction);
					AssertEquals(ZGuid.Empty, message.PrincipalPK);
				});

				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;
				message.Port = "AUBNE";
				CombineAssertions(() =>
				{
					AssertEquals(1, message.Lookups.Direction_List.Count);
					AssertEquals(1, message.Lookups.Principal_List.Count);

					AssertEquals(Constants.PortDirection.Discharge, message.Direction);
					AssertEquals(OrgHeader2.PK, message.PrincipalPK);
				});

				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;
				message.Port = ZString.Empty;
				CombineAssertions(() =>
				{
					AssertEquals(0, message.Lookups.Direction_List.Count);
					AssertEquals(0, message.Lookups.Principal_List.Count);

					AssertEquals(ZString.Empty, message.Direction);
					AssertEquals(ZGuid.Empty, message.PrincipalPK);
				});
			}
		}

		public void TestPortSetter_ShouldUpdatePrincipalAndDirection_BasedOnExistingValues()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage());
				message.Port = ZString.Empty;
				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;

				message.Port = "AUSYD";
				CombineAssertions(() =>
				{
					AssertEquals(Constants.PortDirection.Load, message.Direction);
					AssertEquals(OrgHeader1.PK, message.PrincipalPK);
				});

				message.Port = "AUMEL";
				CombineAssertions(() =>
				{
					Assert(message.Lookups.Direction_List.GetAllCodes().Contains(Constants.PortDirection.Load));
					Assert(!message.Lookups.Principal_List.OfType<OrgHeader>().Any(x => x.OH_Code == "OH1"));

					AssertEquals(Constants.PortDirection.Load, message.Direction);
					AssertEquals(OrgHeader2.PK, message.PrincipalPK);
				});

				message.Port = "AUBNE";
				CombineAssertions(() =>
				{
					Assert(!message.Lookups.Direction_List.GetAllCodes().Contains(Constants.PortDirection.Load));
					Assert(message.Lookups.Principal_List.OfType<OrgHeader>().Any(x => x.OH_Code == "OH2"));

					AssertEquals(Constants.PortDirection.Discharge, message.Direction);
					AssertEquals(OrgHeader2.PK, message.PrincipalPK);
				});

				message.Port = "AUMEL";
				CombineAssertions(() =>
				{
					Assert(message.Lookups.Direction_List.GetAllCodes().Contains(Constants.PortDirection.Discharge));
					Assert(!message.Lookups.Principal_List.OfType<OrgHeader>().Any(x => x.OH_Code == "OH2"));

					AssertEquals(Constants.PortDirection.Discharge, message.Direction);
					AssertEquals(OrgHeader1.PK, message.PrincipalPK);
				});

				message.Port = ZString.Empty;
				CombineAssertions(() =>
				{
					Assert(!message.Lookups.Direction_List.GetAllCodes().Contains(Constants.PortDirection.Discharge));
					Assert(!message.Lookups.Principal_List.OfType<OrgHeader>().Any(x => x.OH_Code == "OH2"));

					AssertEquals(ZString.Empty, message.Direction);
					AssertEquals(ZGuid.Empty, message.PrincipalPK);
				});
			}
		}

		#endregion

		#region Direction

		public void TestDirectionSetter_ShouldUpdatePrincipal()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage());
				message.Port = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;
				message.Direction = ZString.Empty;

				AssertEquals(0, message.Lookups.Principal_List.Count);

				message.Port = "NOTIN";
				AssertEquals(ZGuid.Empty, message.PrincipalPK);

				message.Direction = "InValid";
				AssertEquals(ZGuid.Empty, message.PrincipalPK);

				message.Port = "AUMEL";
				message.Direction = Constants.PortDirection.Load;
				CombineAssertions(() =>
				{
					AssertEquals(1, message.Lookups.Principal_List.Count);
					AssertEquals(OrgHeader2.PK, message.PrincipalPK);
				});

				message.Direction = Constants.PortDirection.Discharge;
				CombineAssertions(() =>
				{
					AssertEquals(1, message.Lookups.Principal_List.Count);
					AssertEquals(OrgHeader1.PK, message.PrincipalPK);
				});

				message.Direction = "InValid";
				AssertEquals(ZGuid.Empty, message.PrincipalPK);
			}
		}

		public void TestDirectionSetter_ShouldUpdatePrincipal_BasedOnExistingValues()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage());
				message.Port = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;
				message.Direction = ZString.Empty;

				AssertEquals(0, message.Lookups.Principal_List.Count);

				message.Port = "NOTIN";
				AssertEquals(ZGuid.Empty, message.PrincipalPK);

				message.Direction = "InValid";
				AssertEquals(ZGuid.Empty, message.PrincipalPK);

				message.Port = "AUSYD";
				CombineAssertions(() =>
				{
					AssertEquals(Constants.PortDirection.Load, message.Direction);
					AssertEquals(OrgHeader1.PK, message.PrincipalPK);
				});

				message.Port = "AUMEL";
				CombineAssertions(() =>
				{
					AssertEquals(Constants.PortDirection.Load, message.Direction);
					AssertEquals(1, message.Lookups.Principal_List.Count);
					AssertEquals(OrgHeader2.PK, message.PrincipalPK);
				});

				message.Direction = Constants.PortDirection.Discharge;
				CombineAssertions(() =>
				{
					AssertEquals(1, message.Lookups.Principal_List.Count);
					AssertEquals(OrgHeader1.PK, message.PrincipalPK);
				});

				message.Port = "AUBNE";
				CombineAssertions(() =>
				{
					AssertEquals(Constants.PortDirection.Discharge, message.Direction);
					AssertEquals(1, message.Lookups.Principal_List.Count);
					AssertEquals(OrgHeader2.PK, message.PrincipalPK);
				});

				message.Direction = "InValid";
				AssertEquals(ZGuid.Empty, message.PrincipalPK);
			}
		}

		#endregion

		#region SenderID

		public void TestSenderID()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new DangerousGoodsManifestMessage(CreateVoyage());
				message.PrincipalPK = ZGuid.Empty;
				message.Direction = ZString.Empty;

				message.Port = "NOTIN";
				AssertEquals(ZString.Empty, message.GetSenderID());

				message.Direction = "InValid";
				AssertEquals(ZString.Empty, message.GetSenderID());

				message.Port = "AUMEL";
				message.Direction = Constants.PortDirection.Load;
				AssertEquals("SenderID_2", message.GetSenderID());
			}
		}

		#endregion

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
