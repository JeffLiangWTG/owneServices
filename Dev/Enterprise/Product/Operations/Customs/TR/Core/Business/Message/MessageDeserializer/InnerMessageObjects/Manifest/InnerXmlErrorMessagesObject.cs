using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business
{
	public class InnerXmlErrorMessagesObject : InnerMessageObjectBase
	{
		public new static class Constants
		{
			public const string ErrorDescription = "HataAciklamasi";
		}

		public static (string ElementName, string NamespaceUrl)[] MatchPath =>
			matchPath ?? (matchPath = new (string, string)[] { (Constants.ErrorDescription, TRMessageConstants.Xml.CustomsNamespace) });

		[ThreadStatic]
		static (string, string)[] matchPath;

		public InnerXmlErrorMessagesObject(XmlDocument innerDocument) : base(innerDocument)
		{
			ErrorMessages = innerDocument.GetElementsByPath(MatchPath).Select(element => new ZString(element.InnerText)).ToArray();
		}

		public IReadOnlyCollection<ZString> ErrorMessages { get; }
	}
}
