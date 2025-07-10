using System.Collections.Generic;
using System.Xml;

namespace Enterprise.Customs.PL.Business;

public interface IXmlReadingStrategy
{
	IReadOnlyCollection<XmlQualifiedName> SupportedXmlNodes { get; }
}
