using System.Xml;

namespace Enterprise.Customs.TR.Business
{
	public class SOAPLevelExceptionObject : InnerMessageObjectBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Not for front end use")]
		static new class Constants
		{
			public const string FaultString = "faultstring";
			public const string Fault = nameof(Fault);
		}

		public static (string, string)[] ExceptionPath => new (string, string)[]
		{
			(Constants.Fault, TRMessageConstants.Xml.SOAPNamespace),
		};

		public SOAPLevelExceptionObject(XmlElement node) : base(node)
		{
			FaultString = TryGetInnerText(node, Constants.FaultString);
		}

		public string FaultString { get; }
	}
}
