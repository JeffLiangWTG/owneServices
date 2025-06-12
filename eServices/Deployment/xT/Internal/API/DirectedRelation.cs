using System;
using System.Collections.Generic;
using System.Linq;

namespace XT.Internal.API
{
	public class DirectedRelation
	{
		public Guid Id { get; } = Guid.NewGuid();
		public Node From { get; set; }
		public Node To { get; set; }
		public bool IsExternal()
		{
			if (From.IsAncestor(To)) return false;
			if (To.IsAncestor(From)) return false;
			return From.CommonAncestors(To).Count() == 1;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}

	public class RelationComparer : IEqualityComparer<DirectedRelation>
	{
		public bool Equals(DirectedRelation x, DirectedRelation y)
		{
			return x.Id == y.Id;
		}

		public int GetHashCode(DirectedRelation obj)
		{
			return obj.GetHashCode();
		}
	}
}
