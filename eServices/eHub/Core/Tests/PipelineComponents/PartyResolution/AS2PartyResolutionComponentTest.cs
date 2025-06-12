using System;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class AS2PartyResolutionComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestResolveParties()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();

			var partyAccessor = MockRepository.StrictMock<IPartyAccessor>();
			Expect.Call(partyAccessor.ClientExists("SND")).Return(true);
			Expect.Call(partyAccessor.ClientExists("RCP")).Return(true);
			Expect.Call(partyAccessor.ClientExists("SenderID")).Return(true);
			Expect.Call(partyAccessor.ClientExists("RecipientID")).Return(true);

			Expect.Call(partyAccessor.GetClientIDFromAS2Code("AS2SenderCode")).Return("SenderID");
			Expect.Call(partyAccessor.GetClientIDFromAS2Code("AS2RecipientCode")).Return("RecipientID");

			var component = MockRepository.PartialMock<AS2PartyResolutionComponent>();
			Expect.Call(component.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();

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
			message.Context.WriteProperty<EdiIntAS.AS2From>("AS2SenderCode");
			message.Context.WriteProperty<EdiIntAS.AS2To>("AS2RecipientCode");
			component.Execute(pipelineContext, message);
			Assert.AreEqual("SenderID", message.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("RecipientID", message.Context.ReadPropertyString<BTS.DestinationParty>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_PartyResolutionComponent()
		{
			var component = new AS2PartyResolutionComponent();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("eHub AS2 Party Resolution", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("11AABAC0-EAE8-494E-B44F-3CB5C0FB79D4"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string senderIDProp = null;
			string recipientIDProp = null;
			string resolveSenderProp = null;
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
