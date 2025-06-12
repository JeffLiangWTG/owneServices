using System;
using System.Collections.Generic;
using System.Linq;
using Xtrade.Core.Objects;

namespace XT.Internal.API
{
	public class Node
	{
		public XtObject Instance { get; set; }
		public Node Parent { get; set; }
		public List<Node> Children = new List<Node>();
		public List<DirectedRelation> InRelations = new List<DirectedRelation>();
		public List<DirectedRelation> OutRelations = new List<DirectedRelation>();

		public void AddChild(Node child)
		{
			Children.Add(child);
		}

		public void AddInRelation(DirectedRelation relation)
		{
			InRelations.Add(relation);
		}

		public void AddOutRelation(DirectedRelation relation)
		{
			OutRelations.Add(relation);
		}

		public bool IsAncestor(Node other)
		{
			if (Instance.Id == other.Instance.Id) return true;
			foreach (var child in Children)
			{
				if (child.IsAncestor(other)) return true;
			}
			return false;
		}

		public IEnumerable<Node> Ancestors()
		{
			if (Parent == null) return Enumerable.Empty<Node>();
			return Parent.Ancestors().Concat(new[] { Parent });
		}

		public IEnumerable<Node> CommonAncestors(Node other)
		{
			var ancestors = Ancestors();
			var otherAncestors = other.Ancestors();
			for (var i = 0; i < new[] { ancestors.Count(), otherAncestors.Count() }.Min(); i++)
			{
				if (ancestors.ElementAt(i).Instance.Id != otherAncestors.ElementAt(i).Instance.Id)
				{
					return ancestors.Take(i);
				}
			}
			return ancestors.Count() > otherAncestors.Count()
				? otherAncestors
				: ancestors;
		}

		public IEnumerable<DirectedRelation> RecursiveRelations()
		{
			return Children.SelectMany(x => x.RecursiveRelations())
				.Concat(InRelations)
				.Concat(OutRelations)
				.Distinct(new RelationComparer());
		}

		public override string ToString()
		{
			return Instance?.ToString();
		}

		public override int GetHashCode()
		{
			return Instance.Id.GetHashCode();
		}
	}
}
