using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PortAuthorityFilterValidationTest : BaseAgencyTest
	{
		public void TestCorrectValidation()
		{
			AssertType(typeof(PortAuthorityValidation), Message.Validation);
		}

		public void TestValidateMessageType()
		{
			Message.MessageType = "";
			AssertHasError(Message.MessageTypeInfo, "Please enter a Message Type.");

			Message.MessageType = PortMessageTypeList.Codes.Original;
			AssertNoNotifications(Message.MessageTypeInfo);

			Message.MessageType = "Crp";
			AssertHasError(Message.MessageTypeInfo, "Enter a valid Message Type.");
		}

		public void TestValidateDeliverTo3rdParty()
		{
			Message.DeliverTo3rdParty = true;
			AssertHasWarningContaining(Message.DeliverTo3rdPartyInfo, "This option is in the process of being removed. Please contact your account manager or support for alternative solutions");

			Message.DeliverTo3rdParty = false;
			AssertNoWarningContaining(Message.DeliverTo3rdPartyInfo, "This option is in the process of being removed. Please contact your account manager or support for alternative solutions");
		}

		public void TestValidateMessageTypeAgainstSentMessages()
		{
			const string nothingToCancel = "No messages have been sent for this port/direction, so there is nothing to cancel.";
			const string alreadyCanceled = "A cancellation has already been sent for this port/direction.";
			const string nothingToReplace = "No messages have been sent for this port/direction, so there is nothing to replace.";
			const string replaceCancelation = "The last message sent for this port/direction was a cancellation, so there is nothing to replace.";
			const string messageAlreadySent = "An uncanceled message has already been sent for this port/direction.";

			SetPortAuthoritySettings("AUBNE");
			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			Message.Port = "AUBNE";
			Message.Direction = Constants.PortDirection.Load;

			Message.MessageType = PortMessageTypeList.Codes.Original;
			AssertNoWarnings(Message.MessageTypeInfo);

			Message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertHasWarning(Message.MessageTypeInfo, nothingToReplace);

			Message.MessageType = PortMessageTypeList.Codes.Cancellation;
			AssertHasWarning(Message.MessageTypeInfo, nothingToCancel);

			PortAuthorityMessage message = (PortAuthorityMessage)Voyage.Origins[0].Messages.AddNew(typeof(PortAuthorityMessage));
			message.EM_MessageSubType = PortMessageTypeList.Codes.Original;

			Message.MessageType = PortMessageTypeList.Codes.Original;
			AssertHasWarning(Message.MessageTypeInfo, messageAlreadySent);

			Message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertNoWarnings(Message.MessageTypeInfo);

			Message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertNoWarnings(Message.MessageTypeInfo);

			message.EM_MessageSubType = PortMessageTypeList.Codes.Cancellation;

			Message.MessageType = PortMessageTypeList.Codes.Original;
			AssertNoWarnings(Message.MessageTypeInfo);

			Message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertHasWarning(Message.MessageTypeInfo, replaceCancelation);

			Message.MessageType = PortMessageTypeList.Codes.Cancellation;
			AssertHasWarning(Message.MessageTypeInfo, alreadyCanceled);
		}

		public void TestValidatePrincipalPK()
		{
			var principal = GlbBranch.CurrentBranch.OrgProxy;
			principal.OH_IsShippingProvider = true;
			principal.OH_IsShippingLine = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			principal.Factory.Save();

			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_JX = Voyage.Sailings[0].PK;
			billOfLading.JS_OH_DeliveryAgent = GlbBranch.CurrentBranch.OrgProxy.PK;

			var billOfLading2 = Factory.New<BillOfLading>();
			billOfLading2.JS_JX = Voyage.Sailings[0].PK;
			billOfLading2.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			var otherGuid = ZGuid.NewZGuid();

			var portCollection = new PortAuthorityPortCollection();
			var port1 = portCollection.AddNew();
			port1.Port = "AUBNE";
			port1.ProductionEmail = "prod1@freadnet.org";
			port1.ProductionID = "prod1";
			port1.TestingEmail = "test1@freadnet.org";
			port1.TestingID = "test1";
			AgencyRegistry.Instance.PortAuthorityPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);

			var settings = new PortAuthoritySettings();
			settings.Settings.RemoveAll();
			var setting1 = settings.Settings.AddNew();
			setting1.Port = "AUBNE";
			setting1.Status = PortAuthoritySettingStatus.Codes.Production;
			setting1.PrincipalPK = principal.PK;
			setting1.SenderID = "Sender ID";
			AgencyRegistry.Instance.PortAuthoritySettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			Message.Port = "AUBNE";
			Message.Direction = Constants.PortDirection.Load;
			Message.PrincipalPK = principal.PK;
			Message.Validation.ValidatePrincipalPK();
			AssertEquals(principal.PK, Message.PrincipalPK);
			AssertNoError(Message.PrincipalPKInfo, "Enter a valid Principal.");
			AssertNoError(Message.PrincipalPKInfo, "Principal is not configured for sending Port Authority message to AUBNE");

			Message.PrincipalPK = otherGuid;
			Message.Validation.ValidatePrincipalPK();
			AssertHasError(Message.PrincipalPKInfo, "Enter a valid Principal.");
			AssertNoError(Message.PrincipalPKInfo, "Principal is not configured for sending Port Authority message to AUBNE");

			Message.PrincipalPK = billOfLading2.JS_OH_DeliveryAgent;
			Message.Validation.ValidatePrincipalPK();
			AssertNoError(Message.PrincipalPKInfo, "Enter a valid Principal.");
			AssertHasError(Message.PrincipalPKInfo, "Principal is not configured for sending Port Authority message to AUBNE");

			Message.PrincipalPK = ZGuid.Empty;
			Message.Validation.ValidatePrincipalPK();
			AssertHasError(Message.PrincipalPKInfo, "Please enter a Principal.");
			AssertNoError(Message.PrincipalPKInfo, "Enter a valid Principal.");
			AssertNoError(Message.PrincipalPKInfo, "Principal is not configured for sending Port Authority message to AUBNE");
		}

		public void TestValidateMessageType_CheckEndPointsFromSimilarVoyages()
		{
			SetPortAuthoritySettings("AUBNE");
			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

			Message.Port = "AUBNE";
			Message.Direction = Constants.PortDirection.Load;

			Message.MessageType = PortMessageTypeList.Codes.Original;
			AssertNoWarnings(Message.MessageTypeInfo);

			var relatedVoyage = Voyage.Clone() as JobVoyage;
			var siblingOrigin = relatedVoyage.Origins.GetOriginFromLoading("AUBNE");

			var message = siblingOrigin.Messages.AddNew(typeof(PortAuthorityMessage));
			message.EM_MessageSubType = PortMessageTypeList.Codes.Original;

			string expectedWarning = relatedVoyage.HumanReadableName + ": " + "An uncanceled message has already been sent for this port/direction.";

			Message.Validation.ValidateMessageType();
			AssertHasWarning(Message.MessageTypeInfo, expectedWarning);
		}

		#region Implementation

		#region Message

		PortAuthority Message
		{
			get { return message ?? (message = new PortAuthority(Voyage)); }
		}
		PortAuthority message;

		#endregion

		#region Voyage

		JobVoyage Voyage
		{
			get { return voyage ?? (voyage = Factory.New<JobVoyage>()); }
		}
		JobVoyage voyage;

		#endregion

		#endregion
	}
}
