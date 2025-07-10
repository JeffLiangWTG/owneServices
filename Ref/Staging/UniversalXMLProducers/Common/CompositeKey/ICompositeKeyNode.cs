using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey
{
	public interface ICompositeKeyNode
	{
		string Value { get; }
		string ZZ5Value { get; set; }
		string Description { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		CompositeKeyNodeType NodeType { get; }
		int Level { get; }
		string CompositeKey { get; set; }
		ICompositeKeyNode Parent { get; set; }
		ICollection<ICompositeKeyNode> Children { get; }
		ICollection<(string language, string description)> Language { get; }
	}
}
