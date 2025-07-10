using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class SOAPLevelRefIDAndGuidObject : InnerMessageObjectBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer strings")]
		new static class Constants
		{
			public const string Guid = "Guid";
		}

		public SOAPLevelRefIDAndGuidObject(XmlElement element) : base(element)
		{
			RefID = TryGetInnerText(element, InnerMessageObjectBase.Constants.RefId);
			Guid = TryGetGuid(TryGetInnerText(element, Constants.Guid));
			Situation = TryGetInnerText(element, InnerMessageObjectBase.Constants.Situation);
		}

		public ZString RefID { get; }
		public ZGuid Guid { get; }
		public ZString Situation { get; }
	}
}
