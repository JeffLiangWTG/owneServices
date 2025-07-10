using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class InnerXmlInspectionClerkObject : InnerMessageObjectBase
	{
		public new static class Constants
		{
			public const string InspectionClerk = "MUAYENEMEMURU";
		}

		public InnerXmlInspectionClerkObject(XmlElement element) : base(element)
		{
			MUAYENEMEMURU = TryGetInnerText(element, Constants.InspectionClerk);
		}

		public ZString MUAYENEMEMURU { get; }
	}
}
