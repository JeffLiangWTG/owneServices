using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class DiffGramGuidObject : InnerMessageObjectBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Option strings")]
		public new static class Constants
		{
			public const string Tip = "Tip";
			public const string OptionTime = "Optime";
		}

		public DiffGramGuidObject(XmlElement element) : base(element)
		{
			Guid = TryGetGuid(TryGetInnerText(element, InnerMessageObjectBase.Constants.Guid));
			RefId = TryGetInnerText(element, InnerMessageObjectBase.Constants.RefId);
			Situation = TryGetInnerText(element, InnerMessageObjectBase.Constants.Situation);
			Tip = TryGetInt(TryGetInnerText(element, Constants.Tip));
			OptionTime = TryGetDateTime(TryGetInnerText(element, Constants.OptionTime));
		}

		public ZGuid Guid { get; }
		public ZString RefId { get; }
		public ZString Situation { get; }
		public ZInt Tip { get; }
		public ZDateTime OptionTime { get; }
	}
}
