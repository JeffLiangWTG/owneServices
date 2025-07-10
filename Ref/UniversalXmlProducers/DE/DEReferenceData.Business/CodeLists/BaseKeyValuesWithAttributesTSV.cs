using System.Collections.Generic;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public class BaseKeyValuesWithAttributesTSV : BaseKeyValuesTSV, IKeyValuesWithAttributes
	{
		public BaseKeyValuesWithAttributesTSV(string[] record, IList<KeyValueAttribute> attributes)
			: base(record)
		{
			Attributes = attributes;
		}
		
		public IList<KeyValueAttribute> Attributes { get; }
	}
}
