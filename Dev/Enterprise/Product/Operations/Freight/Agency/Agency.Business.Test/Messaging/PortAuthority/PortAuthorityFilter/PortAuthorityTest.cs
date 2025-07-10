
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortAuthority))]
	internal sealed class PortAuthorityFilterBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PortAuthority(Factory.New<JobVoyage>());
		}

		#endregion
	}

	internal sealed class PortAuthorityFilterTest : PortMessageTest
	{
		public void TestSerialisation()
		{
			const string xml =
				"<PortAuthorityFilter>" +
					"<Version>2.0</Version>" +
					"<SenderId>SenderID</SenderId>" +
					"<RecipientId>RecipientID</RecipientId>" +
					"<EmailAddress>bob@freadnet.org</EmailAddress>" +
				"</PortAuthorityFilter>" +
				"";

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				PortAuthority filter = new PortAuthority(Voyage);
				filter.SenderId = "SenderID";
				filter.RecipientId = "RecipientID";
				filter.Version = PortAuthorityVersionList.Codes.V20;
				filter.EmailAddress = "bob@freadnet.org";

				xmlWriter.Formatting = Formatting.None;
				xmlWriter.WriteStartElement("PortAuthorityFilter");
				((IXmlSerializable)filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				xmlWriter.Flush();
				AssertMultilineASCIIEquals("", xml.Replace("><", ">\r\n<"), writer.ToString().Replace("><", ">\r\n<"));
			}

			using (StringReader reader = new StringReader(xml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				PortAuthority filter = new PortAuthority(Voyage);
				xmlReader.MoveToContent();
				((IXmlSerializable)filter).ReadXml(xmlReader);

				AssertEquals("SenderId", "SenderID", filter.SenderId);
				AssertEquals("RecipientId", "RecipientID", filter.RecipientId);
				AssertEquals("Version", PortAuthorityVersionList.Codes.V20, filter.Version);
				AssertEquals("Email Address", "bob@freadnet.org", filter.EmailAddress);
			}
		}

		public void TestPersist3rdPartyDetails()
		{
			{
				PortAuthority filter = new PortAuthority(Voyage);
				filter.SenderId = "SID";
				filter.RecipientId = "RID";
				filter.Version = PortAuthorityVersionList.Codes.V11;
				filter.EmailAddress = "mik@freadnet.org";
				filter.Persist3rdPartySettings();
			}

			{
				PortAuthority filter = new PortAuthority(Voyage);
				filter.Restore3rdPartySettings();

				AssertEquals("SenderId", "SID", filter.SenderId);
				AssertEquals("RecipientId", "RID", filter.RecipientId);
				AssertEquals("Version", PortAuthorityVersionList.Codes.V11, filter.Version);
				AssertEquals("Email Address", "mik@freadnet.org", filter.EmailAddress);
			}
		}

		public void TestDirectionUpdatesMessageType()
		{
			SetPortAuthoritySettings("AUBNE");

			VoyageOrigin origin1 = Voyage.Origins.AddNew();
			origin1.JA_RL_NKPortOfLoading = "AUMEL";
			VoyageOrigin origin2 = Voyage.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUBNE";

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_RL_NKPortOfDischarge = "AUBNE";
			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";

			PortAuthorityMessage message = Factory.New<PortAuthorityMessage>();
			message.EM_LinkUniqueID = destination1.PK;
			message.EM_LinkTable = destination1.TableName;

			Message.Port = "AUBNE";
			Message.Direction = Constants.PortDirection.Load;
			AssertEquals("no message sent for load yet, default to original", PortMessageTypeList.Codes.Original, Message.MessageType);

			Message.Direction = Constants.PortDirection.Discharge;
			AssertEquals("message sent for discharge, default to replace", PortMessageTypeList.Codes.Replace, Message.MessageType);

			Message.Direction = Constants.PortDirection.Load;
			AssertEquals("no message sent for load yet, default to original", PortMessageTypeList.Codes.Original, Message.MessageType);

			Message.Direction = "";
			AssertEquals("no endpoint, clear", "", Message.MessageType);
		}

		public void TestGetEndPoint()
		{
			JobSailing sailing1 = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			JobSailing sailing2 = FindOrCreateSailing(Voyage, AlternateHomePort, OverseasPort2);

			Message.Port = HomePort;
			Message.Direction = Constants.PortDirection.Load;
			AssertSame(sailing1.Origin, Message.GetEndPoint());

			Message.Port = OverseasPort2;
			Message.Direction = Constants.PortDirection.Discharge;
			AssertSame(sailing2.Destination, Message.GetEndPoint());
		}

		public void TestMessageFunction()
		{
			FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			Factory.Save();

			Message.Port = HomePort;
			Message.Direction = Constants.PortDirection.Load;

			Message.MessageType = PortMessageTypeList.Codes.Cancellation;
			AssertEquals(PortAuthorityMessageFunction.Cancelation, Message.ExtractData().MessageFunction);

			Message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertEquals(PortAuthorityMessageFunction.Replace, Message.ExtractData().MessageFunction);

			Message.MessageType = PortMessageTypeList.Codes.Original;
			AssertEquals(PortAuthorityMessageFunction.Original, Message.ExtractData().MessageFunction);
		}

		public void TestExtractDataWithoutValidationErrors()
		{
			ZDateTime now = ZDateTime.Now;

			OrgHeader principal = NewPrincipal();
			SetAcosCode(principal, "principal");

			OrgHeader consignor = NewConsignor();
			OrgHeader consignee = NewConsignee();

			OrgHeader cto = Factory.NewWithValidTestData<OrgHeader>();
			SetAcosCode(cto, "Blat");

			OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsShippingLine = true;
			shippingLine.OH_IsShippingProvider = true;

			var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();

			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			Voyage.JV_VoyageFlight = "x42";
			Voyage.JV_OH_Line = shippingLine.PK;

			JobSailing sailing = FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			sailing.Origin.JA_E_DEP = now.AddDays(1);
			sailing.Destination.JB_E_ARV = now.AddDays(2);
			sailing.Destination.JB_OA_ArrivalCTOAddress = cto.MainAddress.PK;
			Factory.Save();

			AgencyShipment shipment = NewShipment(sailing, principal, false, true);
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.JS_GoodsDescription = "Random Goods";
			shipment.JS_HouseBill = "BLAT";
			shipment.JS_OA_BookedShippingLineAddress = shippingLine.MainAddress.PK;
			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.RunPreSaveValidation();
			AssertNoErrors("precondition:", shipment);
			AssertNoMessageErrors("precondition:", shipment);

			Factory.Save();

			FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			Message.Port = HomePort;
			Message.Direction = Constants.PortDirection.Load;

			IPortAuthorityMessagingData data = Message.ExtractData();
			AssertEquals(HomePort, data.Load);
			AssertEquals("", data.Discharge);

			AssertNotNull("should have found the consignment.", FindConsignment(data, "BLAT"));
			AssertEquals("SenderNotifications", 0, Message.Issues.Count);
		}

		public void TestExtractDataWithValidationErrors()
		{
			OrgHeader principal = NewPrincipal();
			JobSailing sailing = FindOrCreateSailing(Voyage, OverseasPort, HomePort);
			Factory.Save();

			AgencyShipment shipment = NewShipment(sailing, principal, false, true);
			shipment.JS_UniqueConsignRef = "S00000000";

			Factory.Save();

			FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			Message.Port = HomePort;
			Message.Direction = Constants.PortDirection.Discharge;

			Message.ExtractData();
			AssertEquals("Issues", 1, Message.Issues.Count);
			AssertEquals("S00000000 has errors.", Message.Issues[0].Text);
			AssertEquals(shipment.PK, Message.Issues[0].TargetPK);
		}

		public void TestDefaultPrincipal()
		{
			OrgHeader principal = NewPrincipal();
			JobSailing sailing = FindOrCreateSailing(Voyage, OverseasPort, HomePort);
			Factory.Save();

			AgencyShipment shipment = NewShipment(sailing, principal, false, true);
			shipment.JS_UniqueConsignRef = "S00000000";

			Factory.Save();

			FindOrCreateSailing(Voyage, HomePort, OverseasPort);
			Message.Port = HomePort;
			Message.Direction = Constants.PortDirection.Discharge;

			AssertEquals(principal.PK, Message.PrincipalPK);
		}

		public void TestNotify()
		{
			ZGuid pk = ZGuid.NewZGuid();

			AssertEquals("Should be no sender notification", 0, Message.Issues.Count);

			Listener.Notify(pk, "JS", "Blat", "Icarus");
			AssertEquals("Should have 1 sender notification", 1, Message.Issues.Count);
			AssertEquals(pk, Message.Issues[0].TargetPK);
			AssertEquals("JS", Message.Issues[0].TargetCode);
			AssertEquals("Blat", Message.Issues[0].Text);
			AssertEquals("Icarus", Message.Issues[0].Detail);
		}

		#region Implementation

		IPortAuthorityConsignmentData FindConsignment(IPortAuthorityMessagingData data, string bol)
		{
			foreach (IPortAuthorityConsignmentData consignment in data.Consignments)
			{
				if (consignment.BillOfLading == bol)
				{
					return consignment;
				}
			}

			return null;
		}

		IPortAuthorityIssueListener Listener
		{
			get { return Message; }
		}

		PortAuthority Message
		{
			get { return message ?? (message = new PortAuthority(Voyage)); }
		}
		PortAuthority message;

		JobVoyage Voyage
		{
			get { return voyage ?? (voyage = Factory.New<JobVoyage>()); }
		}
		JobVoyage voyage;

		#endregion
	}
}
