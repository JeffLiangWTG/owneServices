using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class StowPlanInterchangeProviderTest : InterchangeProviderTestCase
	{
		[ExpectNoExceptions]
		public void TestNoExceptionForMoreThanOneMessages()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var coll = new NonDependentEDIMessageCollection(Factory);
			var message = coll.AddNew();
			message.EM_MessageType = "XXX";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.StowPlan;
			message.EM_MessageText = "MSG TEXT";
			message = coll.AddNew();
			message.EM_MessageType = "XXX";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.StowPlan;
			message.EM_MessageText = "MSG TEXT";
			using (var stowProvider = new StowPlanInterchangeProvider(coll))
			{
				AssertNotNull(stowProvider.Interchanges);
			}
		}

		public void TestMessageAreDiscardedWhenSCACIsMissing()
		{
			var coll = new NonDependentEDIMessageCollection(Factory);
			var message1 = Factory.New<StowPlanMessage>();
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_MessageType = "XXX";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.StowPlan;
			message1.EM_MessageText = "MSG TEXT";
			coll.Add(message1);
			var message2 = Factory.New<StowPlanMessage>();
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_MessageType = "XXX";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.StowPlan;
			message2.EM_MessageText = "MSG TEXT";
			coll.Add(message2);
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, ZString.Empty, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			using (var stowProvider = new StowPlanInterchangeProvider(coll))
			{
				AssertEquals(0, stowProvider.Interchanges.Length);
				var notes = message1.Notes.FindByDescription("Processing Log");
				AssertEquals(1, notes.Length);
				AssertEquals(StowPlanInterchangeProvider.GetMissingCustomsInterchangeSenderIDMessage(message1.EM_MessageNum, orgProxy.OH_Code), notes[0].ST_NoteDataAsText);
				AssertEquals(EDIMessage.Status.Discarded, message1.EM_Status);
				notes = message2.Notes.FindByDescription("Processing Log");
				AssertEquals(1, notes.Length);
				AssertEquals(StowPlanInterchangeProvider.GetMissingCustomsInterchangeSenderIDMessage(message2.EM_MessageNum, orgProxy.OH_Code), notes[0].ST_NoteDataAsText);
				AssertEquals(EDIMessage.Status.Discarded, message2.EM_Status);
			}
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new StowPlanInterchangeProvider(collection);
		}

		[TestDate(2012, 10, 26, 16, 35, 00)]
		public override void TestMessagesPopulateNewInterchange()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "1";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var coll = new NonDependentEDIMessageCollection(Factory);
			var message = coll.AddNew();
			message.EM_MessageType = "XXX";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.StowPlan;
			message.EM_MessageText = "MSG TEXT";
			using (var stowProvider = new StowPlanInterchangeProvider(coll))
			{
				var interchanges = stowProvider.Interchanges;
				AssertEquals(EDIMessage.Status.Sent, message.EM_Status);
				AssertEquals(1, interchanges.Length);
				var interchange = interchanges[0];
				AssertEquals(EDIMessage.ApplicationCodes.StowPlan, interchange.EI_ApplicationCode);
				AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals(EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
				AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("SCAC", interchange.EI_From);
				AssertEquals("USC", interchange.EI_To);
				AssertCollectionContains(message, interchange.ContainedMessages);
				AssertEquals("MSG TEXT", interchange.EI_BodyText);
				AssertEquals("UNB+UNOA:2+SCAC+USC+121026:1635+<<INTERCHANGENUMBERPLACEHOLDER>>'", interchange.EI_HeaderText);
				AssertEquals("UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'", interchange.EI_FooterText);
			}
		}
	}
}
