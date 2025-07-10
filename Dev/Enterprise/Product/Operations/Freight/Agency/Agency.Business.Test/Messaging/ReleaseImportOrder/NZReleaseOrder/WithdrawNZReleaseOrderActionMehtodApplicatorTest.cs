using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(WithdrawNZReleaseOrderActionMethodApplicator))]
	internal class WithdrawNZReleaseOrderActionMethodApplicatorTest : NZReleaseOrderActionMethodApplicatorTest
	{
		protected override BillOfLadingContainer GetContainerWithMessageSent(string uniqueConsignRef, string containerNumber, OrgHeader principal)
		{
			var container = GetContainerWithoutErrors(uniqueConsignRef, containerNumber, principal);
			container.Logs.AddNew(Events.MessageWithdrawCancelRequest, Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.ImportReleaseOrderWithdrawal));

			return container;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WithdrawNZReleaseOrderActionMethodApplicator(Settings);
		}

		protected override void AssertMessageHasBeenSent(BillOfLadingContainer container, bool expectedToBeSent, int numberOfEdimessage = 1)
		{
			var log = container.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertEquals(string.Format("{0} event has been added", Events.DataExport.Code), expectedToBeSent, log != null);

			if (expectedToBeSent)
			{
				var ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
				AssertEquals("Message created", numberOfEdimessage, ediMessages.Length);

				var uxml = XDocument.Parse(ediMessages[numberOfEdimessage - 1].EM_MessageText);
				var ns = uxml.Root.GetDefaultNamespace();
				AssertEquals("Uses 2012 namespace", @"http://www.cargowise.com/Schemas/Universal/2012/11", ns.ToString());
			}

			var expectedParameters = GetMessageParameters(container, Constants.EventReferenceMessageTypes.ImportReleaseOrderWithdrawal);
			log = container.Logs.MostRecentLogByEventTime(Events.MessageWithdrawCancelRequest, StmALog.GenerateEventReference("", expectedParameters));
			AssertEquals(string.Format("{0} event has been added", Events.MessageWithdrawCancelRequest.Code), expectedToBeSent, log != null);
		}

		public void TestAssertUXMLMessage_Purpose()
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, NewZelandBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var importReleaseOrderPorts = new PortMessagingPortCollection();
				AddNewPortMessagingPort(importReleaseOrderPorts, "NZAKL", PrincipalNZ.PK, true).OfType<PortMessagingPort>().First(x => x.Port == "NZAKL").SenderID = "DIJIAAUTOMAN";

				using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, importReleaseOrderPorts))
				using (AgencyRegistry.Instance.ShippingPortsMessagingEHubID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetShippingPortsMessagingEHubIDCollection()))
				{
					var container = GetContainerWithoutErrors("BOOKING_1", "CONTAINER_1", PrincipalNZ, "AUSYD", "NZAKL", Constants.CountryCodes.NewZealand);

					var expectedLog =
						"INFO: Processing Container 'CONTAINER_1'" + "\r\n" +
						"INFO: Universal Shipment queued for sending." + "\r\n" +
						"INFO: " + "\r\n" +
						"INFO: Delivery Succeeded." + "\r\n";

					this.ApplyApplicator(new[] { container }, expectedLog);

					AssertMessageHasBeenSent(container, true);

					var ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
					AssertEquals("Message created", 1, ediMessages.Length);

					var uxml = XDocument.Parse(ediMessages[0].EM_MessageText);
					var ns = uxml.Root.GetDefaultNamespace();

					var documentaryOverridePurpose = uxml.Descendants(ns + "DocumentaryOverride").First().Element(ns + "Purpose").Value;
					AssertEquals("documentaryOverridePurpose", "WTH", documentaryOverridePurpose);
				}
			}
		}

		#region Implementation

		PortMessagingPortCollection AddNewPortMessagingPort(PortMessagingPortCollection importReleaseOrderPorts, string port, ZGuid principalPK, bool enabled, string senderID = "Sender")
		{
			var portMessagingPort = importReleaseOrderPorts.AddNew();
			portMessagingPort.Port = port;
			portMessagingPort.PrincipalPK = principalPK;
			portMessagingPort.SenderID = $"{senderID}_{importReleaseOrderPorts.Count}";
			portMessagingPort.Enabled = enabled;

			return importReleaseOrderPorts;
		}

		GlbBranch NewZelandBranch
		{
			get
			{
				var orgProxy = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", Constants.CountryCodes.NewZealand);
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_OH_OrgProxy = orgProxy.PK;
				branch.GB_RL_NKHomePort = "NZAKL";
				branch.Company.GC_RN_NKCountryCode = "NZ";

				Factory.Save();

				return branch;
			}
		}

		OrgHeader PrincipalNZ
		{
			get
			{
				return principalNZ ?? (principalNZ = CreateNewPrincipal(Constants.CountryCodes.NewZealand));
			}
		}
		OrgHeader principalNZ;

		OrgHeader CreateNewPrincipal(ZString countryCode)
		{
			var principal = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierPrincipalCode, "AAA", countryCode);
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			Factory.Save();

			return principal;
		}

		#endregion
	}
}
