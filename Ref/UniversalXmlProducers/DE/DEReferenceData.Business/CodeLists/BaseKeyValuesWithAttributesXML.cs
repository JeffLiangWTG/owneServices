using System.Collections.Generic;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public class BaseKeyValuesWithAttributesXML : BaseKeyValuesXML, IKeyValuesWithAttributes
	{
		public BaseKeyValuesWithAttributesXML(XElement entry, string codeName, IList<KeyValueAttribute> attributes) : base(entry, codeName)
		{
			Attributes = attributes;
		}
		public IList<KeyValueAttribute> Attributes { get; }
	}
}
