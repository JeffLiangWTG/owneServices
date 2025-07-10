using System.Linq;

namespace Enterprise.Customs.PL.Business.Testing;

public static class EDIMessageExtensions
{
	public static string GetMessageText(this EnterpriseEDIMessage message)
	{
		using var reader = message.GetEM_MessageTextReader();
		var result = reader?.ReadToEnd() ?? string.Empty;
		return result;
	}

	public static string GetAssertionTextRepresentation(this EnterpriseEDIMessage message)
	{
		if (message is null)
		{
			return "NULL";
		}

		var text = message.GetMessageText();
		(string Name, string Value)[] conditions = [
			("ApplicationCode", message.EM_ApplicationCode),
			("Num", message.EM_MessageNum),
			("Type", message.EM_MessageType),
			("SubType", message.EM_MessageSubType),
			("ApplicationReference", message.EM_ApplicationReference),
			("LinkTable", message.EM_LinkTable),
			("LinkUniqueID", message.EM_LinkUniqueID.IsEmpty ? null : message.EM_LinkUniqueID.ToString()),
			("Text", text.Length > 30 ? text.Substring(0, 30) : text),
		];
		var nonEmptyConditionsString = conditions.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => $"{x.Name}={x.Value}").ToList();
		return string.Join("; ", nonEmptyConditionsString);
	}
}
