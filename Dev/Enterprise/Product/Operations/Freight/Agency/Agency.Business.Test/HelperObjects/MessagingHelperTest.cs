using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class MessagingHelperTest : BaseAgencyTest
	{
		public void TestGetPortAuthoritySubject()
		{
			var repository = new MockRepository(MockBehavior.Strict);
			var endPoint = repository.Create<ISailingEndPoint>();
			endPoint.Setup(m => m.Vessel).Returns("Vessel");
			endPoint.Setup(m => m.Voyage).Returns("Voyage");

			AssertEquals("Manifest from: SenderID for: Vessel Voyage", MessagingHelper.GetPortAuthoritySubject(endPoint.Object, "SenderID"));
		}

		public void TestGetEIDOMessagingDetail()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();
			EIDOMessagingHeader disabled = new EIDOMessagingHeader();
			EIDOMessagingHeader enabled = new EIDOMessagingHeader();
			enabled.Email = "bob@freadnet.org";
			EIDOMessagingIdentity identity = enabled.Identities.AddNew();
			identity.PrincipalPK = principal.PK;
			identity.SenderID = "sender";
			identity.RecipientID = "recipient";
			identity.Password = "password";
			AgencyRegistry.Instance.EIDOMessagingDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, disabled);
			try
			{
				MessagingHelper.GetEIDOMessagingDetail();
				Fail("should have thrown a MessageProcessingException");
			}
			catch (MessageProcessingException)
			{
			}

			AgencyRegistry.Instance.EIDOMessagingDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled);
			EIDOMessagingHeader value = MessagingHelper.GetEIDOMessagingDetail();
			AssertEquals(1, value.Identities.Count);
			AssertEquals("bob@freadnet.org", value.Email);
		}

		public void TestGetEIDOIdentityFromMessage()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_OH_DeliveryAgent = principal.PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			Factory.Save();
			EIDOMessagingHeader header = new EIDOMessagingHeader();
			header.Email = "bob@freadnet.org";
			EIDOMessage message = Factory.New<EIDOMessage>();
			try
			{
				MessagingHelper.GetEIDOIdentityFromMessage(header, message);
				Fail("should have thrown a MessageProcessingException");
			}
			catch (MessageProcessingException ex)
			{
				AssertEquals("Unable to identify the correct principal.", ex.Message);
			}

			message.EM_LinkUniqueID = container.PK;
			message.EM_LinkTable = JobContainerSchema.Constants.TableName;
			try
			{
				MessagingHelper.GetEIDOIdentityFromMessage(header, message);
				Fail("should have thrown a MessageProcessingException");
			}
			catch (MessageProcessingException ex)
			{
				AssertEquals("E-IDO messaging has not been enabled in the registry for Principal.\r\nTo fix this, go to the registry option Liner & Agency -> E-IDO Messaging and enter the details.", ex.Message);
			}

			EIDOMessagingIdentity identity = header.Identities.AddNew();
			identity.PrincipalPK = principal.PK;
			identity.SenderID = "sender";
			identity.RecipientID = "recipient";
			identity.Password = "password";
			EIDOMessagingIdentity foundIdentity = MessagingHelper.GetEIDOIdentityFromMessage(header, message);
			AssertEquals(identity, foundIdentity);
		}

		public void TestHash()
		{
			// Do not change these existing values, the hash generation needs to be consistent.
			// If this argorithm changes then port authority messaging will start to use the wrong number fountains on existing clients.
			AssertEquals("GLOBE STAR", "B03509F0", MessagingHelper.Hash("GLOBE STAR"));
			AssertEquals("Port Of Brisbane", "457E8BDD", MessagingHelper.Hash("Port Of Brisbane"));
			AssertEquals("Port 747", "6DA0C613", MessagingHelper.Hash("Port 747"));
		}

		public void TestGetEndPointFromMessage()
		{
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			AssertEquals(sailing.Origin, MessagingHelper.GetEndPointFromMessage(sailing.Origin.Messages.AddNew()));
			AssertEquals(sailing.Destination, MessagingHelper.GetEndPointFromMessage(sailing.Destination.Messages.AddNew()));
		}

		public void TestGetPortAuthoritySettingFromEndPoint_Success()
		{
			JobSailing sailing = FindOrCreateSailing(Voyage, "AUSYD", "AUPKL");
			SetPortAuthoritySettings("AUSYD", "AUPKL");
			AssertEquals("SenderID1", MessagingHelper.GetPortAuthoritySettingFromEndPoint(sailing.Origin, string.Empty).SenderID);
			AssertEquals(ZGuid.Empty, MessagingHelper.GetPortAuthoritySettingFromEndPoint(sailing.Origin, string.Empty).PrincipalPK);
			AssertEquals("SenderID2", MessagingHelper.GetPortAuthoritySettingFromEndPoint(sailing.Destination, string.Empty).SenderID);
			AssertEquals(ZGuid.Empty, MessagingHelper.GetPortAuthoritySettingFromEndPoint(sailing.Origin, string.Empty).PrincipalPK);
		}

		[ExpectException(typeof(MessageProcessingException))]
		public void TestGetPortAuthoritySettingFromEndPoint_MissingPortAuthoritySetting()
		{
			SetPortAuthoritySettings();
			JobVoyage voyage = Factory.New<JobVoyage>();
			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			MessagingHelper.GetPortAuthoritySettingFromEndPoint(sailing.Origin, string.Empty);
		}

		#region Implementation
		JobVoyage Voyage
		{
			get
			{
				return voyage ?? (voyage = Factory.New<JobVoyage>());
			}
		}

		JobVoyage voyage;
		#endregion
	}
}
