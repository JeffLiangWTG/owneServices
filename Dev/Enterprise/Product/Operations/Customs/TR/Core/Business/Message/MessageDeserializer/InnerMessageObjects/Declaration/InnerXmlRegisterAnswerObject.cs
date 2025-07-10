using System.Xml;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using static InnerXmlControlAnswerObject;

public class InnerXmlRegisterAnswerObject : InnerMessageObjectBase
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Localised strings")]
	public new static class Constants
	{
		public const string Result = "sonuc";
		public const string IsSucceed = "BasariliMi";
		public const string RegistrationNo = "Beyanname_no";
		public const string RegistrationDate = "Tescil_tarihi";
		public const string MessageTypeByRegistration = "Tescil";
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
	static bool IsMatch(XmlDocument document) => document.GetElementByPath(new (string, string)[] { (ConstantsTagName.Type, TRMessageConstants.Xml.TempuriNamespace) }) is XmlElement typeElement
		&& typeElement.InnerText == Constants.MessageTypeByRegistration;

	public InnerXmlRegisterAnswerObject(XmlDocument document) : base(document)
	{
		if (IsMatch(document))
		{
			var resultInfoElement = document[Constants.Result];
			RegistrationDate = TryGetDateTime(TryGetInnerText(resultInfoElement, Constants.RegistrationDate));
			RegistrationNumber = TryGetInnerText(resultInfoElement, Constants.RegistrationNo);
		}
		ControlAnswerObject = new InnerXmlControlAnswerObject(document);
	}

	public InnerXmlControlAnswerObject ControlAnswerObject { get; }

	public ZString RegistrationNumber { get; }

	public ZDateTime RegistrationDate { get; }
}
