using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestsSubclassesOf(typeof(MessageHeaderProvider))]
public abstract class MessageHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T> where T : MessageHeaderProvider
{
	public void TestMessageType() => AssertEquals(MessageType, Provider.MessageType);

	public void TestMessageSender()
	{
		var senderIDCollection = new SenderInfoCollection
			{
				new SenderInfo()
				{
					OrganizationPK = GlbCompany.CurrentCompany.OrgProxy.PK,
					SenderID = "NL001234567.01.05",
					DefaultSenderID = true,
				},
			};

		using (NLCustomsRegistry.Instance.SenderIDs.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, senderIDCollection))
		{
			AssertEquals("NL001234567.01.05", Provider.MessageSender);
		}
	}

	public void TestMessageRecipient()
	{
		var messageVersionRegistryCollection = new MessageVersionRegistryCollection
			{
				new MessageVersionRegistry { DomainCode = MessageVersionRegistry.NCTSP5DomainCode, TargetSystemName = "NCTS.NL" }
			};
		using (NLCustomsRegistry.Instance.CustomsMessageVersion.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, messageVersionRegistryCollection))
		{
			AssertEquals("NCTS.NL", Provider.MessageRecipient);
		}
	}

	[TestDate(2024, 05, 13, 11, 38, 00)]
	public void TestPreparationDateTime()
	{
		AssertEquals(new DateTime(2024, 05, 13, 11, 38, 00), Provider.PreparationDateTime);
	}

	public void TestMessageIdentification()
	{
		AssertEquals("<<MSGNO PLACEHOLDER>>", Provider.MessageIdentification);
	}

	public void TestCorrelationIdentifier() => AssertNull(Provider.CorrelationIdentifier);  // Null for now, it is not mapped for the CC141C-message

	protected abstract string MessageType { get; }

	protected abstract string MovementType { get; }

	protected virtual bool HasSendingActionParameter => false;

	protected override T GetProvider() => provider;

	protected virtual void CreateProvider()
	{
		// Create instance of provider to test
		if (HasSendingActionParameter)
		{
			switch (MovementType)
			{
				case NctsMovementType.Codes.Arrival:
					action = new MessageSendingAction(nctsHeader.ArrivalMovementHeader);
					break;
				default:
					action = new MessageSendingAction(nctsHeader.MovementHeader);
					break;
			}
			provider = (T)Activator.CreateInstance(typeof(T), action);
		}
		else
		{
			provider = (T)Activator.CreateInstance(typeof(T), nctsHeader);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(MovementType);
		CreateProvider();
	}

	protected T provider;
	protected NctsHeader nctsHeader;
	protected MessageSendingAction action;
}
