using System;
using System.Linq;
using System.Xml;
using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Types;
using XMLTools;

namespace Enterprise.Customs.PL.Business.Testing;

public readonly record struct ExpectedMessage : IEquatable<EnterpriseEDIMessage>
{
	public ExpectedMessage(EnterpriseEDIMessage templateMessage)
	{
		if (templateMessage != null)
		{
			ApplicationCode = templateMessage.EM_ApplicationCode;
			Num = templateMessage.EM_MessageNum;
			Type = templateMessage.EM_MessageType;
			SubType = templateMessage.EM_MessageSubType;
			ApplicationReference = templateMessage.EM_ApplicationReference;
			LinkTable = templateMessage.EM_LinkTable;
			LinkUniqueID = templateMessage.EM_LinkUniqueID;
			BranchGuid = templateMessage.EM_GB;
			ReceiveTransmit = templateMessage.EM_ReceiveTransmit;
			Status = templateMessage.EM_Status;
			IsActive = templateMessage.EM_IsActive;
			IsTestMessage = templateMessage.EM_IsTestMessage;
			Text = templateMessage.EM_MessageText;
		}
	}

	public string ApplicationCode { get; init; }
	public string Num { get; init; }
	public string Type { get; init; }
	public string SubType { get; init; }
	public string ApplicationReference { get; init; }
	public string LinkTable { get; init; }
	public ZGuid LinkUniqueID { get; init; }
	public ZGuid BranchGuid { get; init; }
	public string ReceiveTransmit { get; init; }
	public string Status { get; init; }
	public bool? IsActive { get; init; }
	public bool? IsTestMessage { get; init; }
	public string Text { get; init; }
	public XmlQualifiedName RootNode { get; init; }

	public bool Equals(EnterpriseEDIMessage message)
	{
		string actualMessageText = null;
		if (message is null)
		{
			return false;
		}
		if (ApplicationCode is not null && ApplicationCode != message.EM_ApplicationCode)
		{
			return false;
		}
		if (Num is not null && Num != message.EM_MessageNum)
		{
			return false;
		}
		if (Type is not null && Type != message.EM_MessageType)
		{
			return false;
		}
		if (SubType is not null && SubType != message.EM_MessageSubType)
		{
			return false;
		}
		if (ApplicationReference is not null && ApplicationReference != message.EM_ApplicationReference)
		{
			return false;
		}
		if (LinkTable is not null && LinkTable != message.EM_LinkTable)
		{
			return false;
		}
		if (!LinkUniqueID.IsEmpty && LinkUniqueID != message.EM_LinkUniqueID)
		{
			return false;
		}
		if (!BranchGuid.IsEmpty && BranchGuid != message.EM_GB)
		{
			return false;
		}
		if (ReceiveTransmit is not null && ReceiveTransmit != message.EM_ReceiveTransmit)
		{
			return false;
		}
		if (Status is not null && Status != message.EM_Status)
		{
			return false;
		}
		if (IsActive.HasValue && IsActive.Value != message.EM_IsActive)
		{
			return false;
		}
		if (IsTestMessage.HasValue && IsTestMessage.Value != message.EM_IsTestMessage)
		{
			return false;
		}

		if (Text is not null)
		{
			try
			{
				var xmlComparer = new XmlComparer();
				var comparisonRes = xmlComparer.CompareXml(Text, GetActualMessageText(), getDiff: true);
				if (!comparisonRes.result)
				{
					return false;
				}
			}
			catch
			{
				if (Text != GetActualMessageText())
				{
					return false;
				}
			}
		}

		if (RootNode != null &&
			(XmlHelper.GetRootXmlNodeName(GetActualMessageText()) is not { } messageRootNode ||
			 ((RootNode.Namespace is not null && RootNode != messageRootNode) ||
			  (RootNode.Namespace is null && RootNode.Name != messageRootNode.Name))))
		{
			return false;
		}

		return true;

		string GetActualMessageText() => actualMessageText ??= message.GetMessageText();
	}

	public static bool operator ==(ExpectedMessage expectedMessage, EnterpriseEDIMessage enterpriseEDIMessage)
		=> expectedMessage.Equals(enterpriseEDIMessage);

	public static bool operator !=(ExpectedMessage expectedMessage, EnterpriseEDIMessage enterpriseEDIMessage)
		=> !(expectedMessage == enterpriseEDIMessage);

	public static bool operator ==(EnterpriseEDIMessage enterpriseEDIMessage, ExpectedMessage expectedMessage)
		=> expectedMessage.Equals(enterpriseEDIMessage);

	public static bool operator !=(EnterpriseEDIMessage enterpriseEDIMessage, ExpectedMessage expectedMessage)
		=> !(expectedMessage == enterpriseEDIMessage);

	public override string ToString()
	{
		(string Name, string Value)[] conditions = [
			(nameof(ApplicationCode), ApplicationCode),
			(nameof(Num), Num),
			(nameof(Type), Type),
			(nameof(SubType), SubType),
			(nameof(ApplicationReference), ApplicationReference),
			(nameof(LinkTable), LinkTable),
			(nameof(LinkUniqueID), LinkUniqueID.IsEmpty ? null : LinkUniqueID.ToString()),
			(nameof(RootNode), RootNode?.ToString()),
			(nameof(Text), Text is null ? null : Text.Length > 30 ? Text.Substring(0, 30) : Text),
		];
		var nonEmptyConditionsString = conditions.Where(x => x.Value is not null).Select(x => $"{x.Name}={x.Value}");
		return string.Join("; ", nonEmptyConditionsString);
	}
}
