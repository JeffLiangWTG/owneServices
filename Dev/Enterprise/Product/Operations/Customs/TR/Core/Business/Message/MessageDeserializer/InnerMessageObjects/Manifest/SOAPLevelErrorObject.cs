using System.Xml;

namespace Enterprise.Customs.TR.Business
{
	public class SOAPLevelErrorObject : InnerMessageObjectBase
	{
		static new class Constants
		{
			public const string Error = nameof(Error);
			public const string Message = nameof(Message);
		}

		public static (string, string)[] ElementErrorPath => new (string, string)[]
		{
			(TRMessageConstants.Xml.SummaryDeclarationResponseElementName, TRMessageConstants.Xml.CustomsBizTalkNamespace),
			(Constants.Error, string.Empty),
			(Constants.Message, string.Empty)
		};

		public static (string, string)[] RootErrorPath => new (string, string)[]
		{
			(TRMessageConstants.Xml.SOAPRootElementName, TRMessageConstants.Xml.BizTalk2003AnyNamespace),
			(Constants.Error, string.Empty),
			(Constants.Message, string.Empty)
		};

		public SOAPLevelErrorObject(XmlElement node) : base(node)
		{
			ErrorMessage = node.InnerText;
		}

		public string ErrorMessage { get; }
	}
}
