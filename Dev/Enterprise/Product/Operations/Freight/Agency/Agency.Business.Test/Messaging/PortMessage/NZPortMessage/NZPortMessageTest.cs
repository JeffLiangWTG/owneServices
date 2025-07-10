using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(NZPortMessage))]
	sealed class NZPortMessageNonPersistentBusinessObject : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZPortMessage(Factory.New<JobVoyage>());
		}

		#endregion
	}

	class NZPortMessageTest : TestCaseWithFactory
	{
		#region CTO

		public void TestCTOGetter()
		{
			var voyage = Factory.New<JobVoyage>();
			var originCTO = Factory.NewWithValidTestData<OrgHeader>();
			var destinationCTO = Factory.NewWithValidTestData<OrgHeader>();

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "UAIEV";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			var message = new NZPortMessage(voyage);

			message.Port = "UAIEV";
			message.Direction = Constants.PortDirection.Load;
			AssertEquals("Load port CTO", ZGuid.Empty, message.CTO);

			message.Port = "NZAKL";
			message.Direction = Constants.PortDirection.Discharge;
			AssertEquals("Load port CTO", ZGuid.Empty, message.CTO);

			origin.JA_OA_DepartureCTOAddress = originCTO.MainAddress.PK;
			destination.JB_OA_ArrivalCTOAddress = destinationCTO.MainAddress.PK;

			message.Port = "UAIEV";
			message.Direction = Constants.PortDirection.Load;
			AssertEquals("Load port CTO", originCTO.PK, message.CTO);

			message.Port = "NZAKL";
			message.Direction = Constants.PortDirection.Discharge;
			AssertEquals("Load port CTO", destinationCTO.PK, message.CTO);
		}

		#endregion

		#region Port

		public void TestPortSetter_ShouldUpdatePrincipalAndDirection()
		{
			var portCollection = new PortManifestPortCollection();

			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZAKL");
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZLYT");
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZNPE");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new NZPortMessage(CreateVoyage());
				message.Port = ZString.Empty;
				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;

				message.Port = "NZAKL";
				CombineAssertions(() =>
				{
					AssertEquals(1, message.Lookups.Direction_List.Count);
					AssertEquals(1, message.Lookups.Principal_List.Count);

					AssertEquals(Constants.PortDirection.Load, message.Direction);
					AssertEquals(OrgHeader1.PK, message.PrincipalPK);
				});

				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;
				message.Port = "NZLYT";
				CombineAssertions(() =>
				{
					AssertEquals(2, message.Lookups.Direction_List.Count);
					AssertEquals(2, message.Lookups.Principal_List.Count);

					AssertEquals(ZString.Empty, message.Direction);
					AssertEquals(ZGuid.Empty, message.PrincipalPK);
				});

				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;
				message.Port = "NZNPE";
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
			var portCollection = new PortManifestPortCollection();

			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZAKL");
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZLYT");
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZNPE");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var message = new NZPortMessage(CreateVoyage());
				message.Port = ZString.Empty;
				message.Direction = ZString.Empty;
				message.PrincipalPK = ZGuid.Empty;

				message.Port = "NZAKL";
				CombineAssertions(() =>
				{
					AssertEquals(Constants.PortDirection.Load, message.Direction);
					AssertEquals(OrgHeader1.PK, message.PrincipalPK);
				});

				message.Port = "NZLYT";
				CombineAssertions(() =>
				{
					Assert(message.Lookups.Direction_List.GetAllCodes().Contains(Constants.PortDirection.Load));
					Assert(!message.Lookups.Principal_List.OfType<OrgHeader>().Any(x => x.OH_Code == "OH1"));

					AssertEquals(Constants.PortDirection.Load, message.Direction);
					AssertEquals(OrgHeader2.PK, message.PrincipalPK);
				});

				message.Port = "NZNPE";
				CombineAssertions(() =>
				{
					Assert(!message.Lookups.Direction_List.GetAllCodes().Contains(Constants.PortDirection.Load));
					Assert(message.Lookups.Principal_List.OfType<OrgHeader>().Any(x => x.OH_Code == "OH2"));

					AssertEquals(Constants.PortDirection.Discharge, message.Direction);
					AssertEquals(OrgHeader2.PK, message.PrincipalPK);
				});

				message.Port = "NZLYT";
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

		#region LastMessageSent

		[TestDate(2015, 01, 01, 0, 0, 0)]
		public void TestGetLastMessageSent()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "UAIEV";

			var message = new NZPortMessage(voyage);
			message.Port = "UAIEV";

			// LOAD DIRECTION
			message.Direction = Constants.PortDirection.Load;

			//		No message
			AssertEquals(ZString.Empty, message.LastMessageSent);

			//		Original Message
			AddEvent(voyage, "NZAKL", Constants.EventReferenceMessageTypes.LoadManifest);
			AssertEquals(ZString.Empty, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", "Some other type");
			AssertEquals(ZString.Empty, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", Constants.EventReferenceMessageTypes.LoadManifest);
			AssertEquals(PortMessageTypeList.Codes.Original, message.LastMessageSent);

			//		Replacement Message
			AddEvent(voyage, "NZAKL", Constants.EventReferenceMessageTypes.LoadManifestReplacement);
			AssertEquals(PortMessageTypeList.Codes.Original, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", "Some other type");
			AssertEquals(PortMessageTypeList.Codes.Original, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", Constants.EventReferenceMessageTypes.LoadManifestReplacement);
			AssertEquals(PortMessageTypeList.Codes.Replace, message.LastMessageSent);

			//		Cancellation Message
			AddEvent(voyage, "NZAKL", Constants.EventReferenceMessageTypes.LoadManifestCancellation);
			AssertEquals(PortMessageTypeList.Codes.Replace, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", "Some other type");
			AssertEquals(PortMessageTypeList.Codes.Replace, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", Constants.EventReferenceMessageTypes.LoadManifestCancellation);
			AssertEquals(PortMessageTypeList.Codes.Cancellation, message.LastMessageSent);

			// DISCHARGE DIRECTION
			message.Direction = Constants.PortDirection.Discharge;

			//		No message
			AssertEquals(ZString.Empty, message.LastMessageSent);

			//		Original Message
			AddEvent(voyage, "NZAKL", Constants.EventReferenceMessageTypes.DischargeManifest);
			AssertEquals(ZString.Empty, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", "Some other type");
			AssertEquals(ZString.Empty, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", Constants.EventReferenceMessageTypes.DischargeManifest);
			AssertEquals(PortMessageTypeList.Codes.Original, message.LastMessageSent);

			//		Replacement Message
			AddEvent(voyage, "NZAKL", Constants.EventReferenceMessageTypes.DischargeManifestReplacement);
			AssertEquals(PortMessageTypeList.Codes.Original, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", "Some other type");
			AssertEquals(PortMessageTypeList.Codes.Original, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", Constants.EventReferenceMessageTypes.DischargeManifestReplacement);
			AssertEquals(PortMessageTypeList.Codes.Replace, message.LastMessageSent);

			//		Cancellation Message
			AddEvent(voyage, "NZAKL", Constants.EventReferenceMessageTypes.DischargeManifestCancellation);
			AssertEquals(PortMessageTypeList.Codes.Replace, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", "Some other type");
			AssertEquals(PortMessageTypeList.Codes.Replace, message.LastMessageSent);

			AddEvent(voyage, "UAIEV", Constants.EventReferenceMessageTypes.DischargeManifestCancellation);
			AssertEquals(PortMessageTypeList.Codes.Cancellation, message.LastMessageSent);
		}

		void AddEvent(JobVoyage voyage, ZString location, ZString messageType)
		{
			var evnt = messageType == Constants.EventReferenceMessageTypes.DischargeManifestCancellation || messageType == Constants.EventReferenceMessageTypes.LoadManifestCancellation
				? Events.MessageWithdrawCancelRequest
				: Events.MessageSent;

			voyage.Logs.AddNew(evnt, ZDateTimeOffset.Now.AddSeconds(secondsCounter++), false, Params.Location.AsKeyFor(location), Params.MessageType.AsKeyFor(messageType));
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int secondsCounter = 0;

		#region Implementation

		JobVoyage CreateVoyage()
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

			var billOfLading1 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading1.JS_OH_DeliveryAgent = OrgHeader1.PK;
			billOfLading1.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "NZAKL" && x.JX_JB_RL_NKPortOfDischarge == "NZLYT").PK;

			var billOfLading2 = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading2.JS_OH_DeliveryAgent = OrgHeader2.PK;
			billOfLading2.JS_JX = voyage.Sailings.OfType<JobSailing>().First(x => x.JX_JA_RL_NKPortOfLoading == "NZLYT" && x.JX_JB_RL_NKPortOfDischarge == "NZNPE").PK;

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
