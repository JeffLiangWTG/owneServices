using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;

public class InnerXmlRegistrationNumberObject : InnerMessageObjectBase
{
	public new static class Constants
	{
		public const string IsSucceed = "BasariliMi";
		public const string RegistrationNumber = "TescilNo";
		public const string RegistrationTime = "TescilTarihi";
		public const string NumberOfItems = "KalemSayisi";
	}

	public static bool IsMatch(XmlDocument document) =>
		document.GetElementByPath(new (string, string)[] { (Constants.RegistrationNumber, TRMessageConstants.Xml.CustomsNamespace) }) is XmlElement registrationNumberElement
		&& !string.IsNullOrEmpty(registrationNumberElement.InnerText);

	public InnerXmlRegistrationNumberObject(XmlDocument document) : base(document)
	{
		var resultInfoElement = document[InnerMessageObjectBase.Constants.ResultInfo];
		RegistrationNumber = TryGetInnerText(resultInfoElement, Constants.RegistrationNumber);
		RegistrationTime = TryGetDateTime(TryGetInnerText(resultInfoElement, Constants.RegistrationTime));
		NumberOfItems = TryGetInt(TryGetInnerText(resultInfoElement, Constants.NumberOfItems));

		if (document.GetElementByPath(InnerXmlGroupageAnswerObject.MatchPath) is XmlElement groupageAnswerNode)
		{
			GroupageAnswerObject = new InnerXmlGroupageAnswerObject(groupageAnswerNode);
		}
	}

	public InnerXmlGroupageAnswerObject GroupageAnswerObject { get; }

	public ZString RegistrationNumber { get; }

	public ZDateTime RegistrationTime { get; }

	public ZInt NumberOfItems { get; }
}
