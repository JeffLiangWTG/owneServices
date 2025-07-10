using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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
using Facilities = CargoWise.EventReference.Constants.Facilities.Code;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(SendNZReleaseOrderActionMethodApplicator))]
	internal class SendNZReleaseOrderActionMethodApplicatorTest : NZReleaseOrderActionMethodApplicatorTest
	{
		protected override BillOfLadingContainer GetContainerWithMessageSent(string uniqueConsignRef, string containerNumber, OrgHeader principal)
		{
			var container = GetContainerWithoutErrors(uniqueConsignRef, containerNumber, principal);
			container.Logs.AddNew(Events.MessageSent, Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.ImportReleaseOrder));

			return container;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SendNZReleaseOrderActionMethodApplicator(Settings);
		}

		protected override void AssertMessageHasBeenSent(BillOfLadingContainer container, bool expectedToBeSent, int numberOfEdiMessages = 1)
		{
			AssertEquals("Import Release Number populated when sent", expectedToBeSent, !container.JC_ContainerImportDORelease.IsEmpty);

			var log = container.Logs.MostRecentLogByEventTime(Events.DataExport);
			AssertEquals(string.Format("{0} event has been added", Events.DataExport.Code), expectedToBeSent, log != null);

			if (expectedToBeSent)
			{
				var ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
				AssertEquals("Message created", numberOfEdiMessages, ediMessages.Length);

				var uxml = XDocument.Parse(ediMessages[numberOfEdiMessages - 1].EM_MessageText);
				var ns = uxml.Root.GetDefaultNamespace();
				AssertEquals("Uses 2012 namespace", @"http://www.cargowise.com/Schemas/Universal/2012/11", ns.ToString());

				var pin = uxml.Descendants(ns + "ContainerImportDORelease").First().Value;
				AssertMatch("PIN is populated as 6-digit string", new Regex(@"\d{6}"), pin);
			}

			var expectedParameters = GetMessageParameters(container, Constants.EventReferenceMessageTypes.ImportReleaseOrder);
			log = container.Logs.MostRecentLogByEventTime(Events.MessageSent, StmALog.GenerateEventReference("", expectedParameters.ToArray()));
			AssertEquals(string.Format("{0} event has been added", Events.MessageSent.Code), expectedToBeSent, log != null);

			var discPort = "NZAKL";
			if (container.Booking.TransportsIncludingRelated.LastLeg != null)
			{
				discPort = container.Booking.TransportsIncludingRelated.LastLeg.JW_RL_NKDiscPort;
			}

			var expectedParameters1 = new[]
			{
						Params.Facility.AsKeyFor(Facilities.Terminal),
						Params.Location.AsKeyFor(discPort),
						Params.Department.AsKeyFor("Carrier")
			};

			log = container.Logs.MostRecentLogByEventTime(Events.ReleaseRequested, StmALog.GenerateEventReference("", expectedParameters1));
			AssertEquals(string.Format("{0} event has been added", Events.ReleaseRequested.Code), expectedToBeSent, log != null);
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
					AssertEquals("documentaryOverridePurpose", "ORG", documentaryOverridePurpose);

					Factory.Save();
					AddLog(container, Events.MessageWithdrawCancelRequest, ZDateTime.Now, "MST=Import Release Order|DEP=Terminal|LOC=NZAKL");
					this.Applicator.AllowSendWhileResponsePending = true;
					this.ApplyApplicator(new[] { container }, expectedLog);

					AssertMessageHasBeenSent(container, true, 2);

					ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
					AssertEquals("Message created", 2, ediMessages.Length);

					uxml = XDocument.Parse(ediMessages[1].EM_MessageText);
					ns = uxml.Root.GetDefaultNamespace();

					documentaryOverridePurpose = uxml.Descendants(ns + "DocumentaryOverride").First().Element(ns + "Purpose").Value;
					AssertEquals("documentaryOverridePurpose", "ORG", documentaryOverridePurpose);

					Factory.Save();
					this.Applicator.AllowSendWhileResponsePending = true;
					this.ApplyApplicator(new[] { container }, expectedLog);

					AssertMessageHasBeenSent(container, true, 3);

					ediMessages = Factory.Load<EDIMessage>(GetEDIMessageFilter());
					AssertEquals("Message created", 3, ediMessages.Length);

					uxml = XDocument.Parse(ediMessages[2].EM_MessageText);
					ns = uxml.Root.GetDefaultNamespace();

					documentaryOverridePurpose = uxml.Descendants(ns + "DocumentaryOverride").First().Element(ns + "Purpose").Value;
					AssertEquals("documentaryOverridePurpose", "AMD", documentaryOverridePurpose);
				}
			}

			void AddLog(IStmALogParent parent, Event eventType, ZDateTime eventDate, string parameters)
			{
				var paramList = new List<KeyValuePair<string, string>>();
				foreach (var paramPair in parameters.Split('|'))
				{
					var pair = paramPair.Split('=');
					paramList.Add(new KeyValuePair<string, string>(pair[0], pair[1]));
				}

				parent.Logs.AddNew(eventType, eventDate.ToOffset(), paramList.ToArray());

				Thread.Sleep(1);
				Factory.Save();
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

		new SendNZReleaseOrderActionMethodApplicator Applicator => (SendNZReleaseOrderActionMethodApplicator)base.Applicator;
	}
}
