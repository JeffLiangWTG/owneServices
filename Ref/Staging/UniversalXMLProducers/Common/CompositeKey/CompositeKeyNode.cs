using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey
{
	public class CompositeKeyNode : ICompositeKeyNode
	{
		public CompositeKeyNode(string value, string description, DateTime startDate, DateTime endDate, CompositeKeyNodeType nodeType, int level, ICompositeKeyNode parent, List<(string Language, string Description)> language = null)
		{
			Argument.NotNullOrEmpty(value, nameof(value));
			Argument.NotNullOrEmpty(description, nameof(description));

			Value = value;
			Description = description;
			StartDate = startDate;
			EndDate = endDate;
			NodeType = nodeType;
			Level = level;
			Parent = parent;
			Children = new List<ICompositeKeyNode>();
			Language = language ?? new List<(string language, string description)>();

			if (parent != null)
			{
				parent.Children.Add(this);
			}

			ZZ5Value = value;
		}

		public string Value { get; }
		public string ZZ5Value { get; set; }
		public string Description { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public CompositeKeyNodeType NodeType { get; }
		public int Level { get; }
		public string CompositeKey { get; set; }
		public ICompositeKeyNode Parent { get; set; }
		public ICollection<ICompositeKeyNode> Children { get; }
		public ICollection<(string language, string description)> Language { get; }
	}
}
