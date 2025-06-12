using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class PartyResolutionComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestResolveParties()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();

			var partyAccessor = MockRepository.StrictMock<IPartyAccessor>();
			Expect.Call(partyAccessor.ClientExists("sender")).Return(false).Repeat.Times(2);
			Expect.Call(partyAccessor.ClientExists("sender1")).Return(true).Repeat.Times(3);
			Expect.Call(partyAccessor.ClientExists("recipient")).Return(false).Repeat.Times(2);
			Expect.Call(partyAccessor.ClientExists("recipient1")).Return(true);
	
			var component = MockRepository.PartialMock<PartyResolutionComponent>();
			Expect.Call(component.GetPartyAccessor()).Return(partyAccessor).Repeat.Times(5);

			MockRepository.ReplayAll();

			component.ResolveSender = false;
			component.ResolveRecipient = false;
			component.SenderID = "SND";
			component.RecipientID = "RCP";
			component.Execute(pipelineContext, message);
			Assert.AreEqual("SND", message.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("RCP", message.Context.ReadPropertyString<BTS.DestinationParty>());

			component.ResolveSender = true;
			component.ResolveRecipient = true;
			component.SenderID = "sender";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Sender 'sender' not recognized.");

			component.SenderID = null;
			message.Context.WriteProperty<BTS.SourceParty>("sender");
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Sender 'sender' not recognized.");

			message.Context.WriteProperty<BTS.SourceParty>("sender1");
			component.RecipientID = "recipient";
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Recipient 'recipient' not recognized.");

			component.RecipientID = null;
			message.Context.WriteProperty<BTS.DestinationParty>("recipient");
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ApplicationException), "Recipient 'recipient' not recognized.");

			component.RecipientID = "recipient1";
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual("sender1", message.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("recipient1", message.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual(message, result);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_PartyResolutionComponent()
		{
			var component = new PartyResolutionComponent();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("eHub Party Resolution", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("D262069D-F101-4474-AD7F-8DA128014C2F"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string senderIDProp = null;
			string recipientIDProp  = null;
			string resolveSenderProp  = null;
			string resolveRecipientProp = null;

			string senderIDValue = string.Empty;
			string recipientIDValue = string.Empty;
			bool resolveSenderValue = false;
			bool resolveRecipientValue = false;

			object senderIDPtr = "sender";
			object recipientIDPtr = "recipient";
			object resolveSenderPtr = false;
			object resolveRecipientPtr = false;

			Expect.Call(() => propertyBag.Write("SenderID", ref senderIDPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { senderIDProp = propName; senderIDValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("RecipientID", ref recipientIDPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { recipientIDProp = propName; recipientIDValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ResolveSender", ref resolveSenderPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { resolveSenderProp = propName; resolveSenderValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ResolveRecipient", ref resolveRecipientPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { resolveRecipientProp = propName; resolveRecipientValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("SenderID", out resolveSenderPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "sender_different"; }));
			Expect.Call(() => propertyBag.Read("RecipientID", out resolveSenderPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "recipient_different"; }));
			Expect.Call(() => propertyBag.Read("ResolveSender", out resolveSenderPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("ResolveRecipient", out resolveSenderPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.SenderID = "sender";
			component.RecipientID = "recipient";
			component.ResolveSender = false;
			component.ResolveRecipient = false;

			component.Save(propertyBag, true, true);

			Assert.AreEqual("SenderID", senderIDProp);
			Assert.AreEqual("RecipientID", recipientIDProp);
			Assert.AreEqual("ResolveSender", resolveSenderProp);
			Assert.AreEqual("ResolveRecipient", resolveRecipientProp);

			Assert.AreEqual("sender", senderIDValue);
			Assert.AreEqual("recipient", recipientIDValue);
			Assert.IsFalse(resolveSenderValue);
			Assert.IsFalse(resolveRecipientValue);

			component.Load(propertyBag, 0);

			Assert.AreEqual("sender_different", component.SenderID);
			Assert.AreEqual("recipient_different", component.RecipientID);
			Assert.IsTrue(component.ResolveSender);
			Assert.IsTrue(component.ResolveRecipient);

			MockRepository.VerifyAll();
		}
	}
}
