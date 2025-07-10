using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.TransportConsignment.Business
{
	[Immutable]
	public class GroupedPackTypeCounts
	{
		public GroupedPackTypeCounts(ZGuid key, IEnumerable<PackTypeCount> packTypeCounts)
		{
			if (!key.IsValid)
			{
				throw new InvalidOperationException("Use 'Empty' if you want to create an Empty GroupedPackTypeCounts.");
			}

			this.key = key;
			this.packTypeCounts = Argument.NotNull(packTypeCounts, "packTypeCounts").ToImmutableArray();
		}

		GroupedPackTypeCounts()
		{
			packTypeCounts = ImmutableArray<PackTypeCount>.Empty;
		}

		#region Properties

		#region IsEmpty

		public bool IsEmpty
		{
			get { return Key.IsEmpty; }
		}

		#endregion

		#region Key

		public ZGuid Key
		{
			get { return key; }
		}

		readonly ZGuid key;

		#endregion

		#region PackTypeCounts

		public IEnumerable<PackTypeCount> PackTypeCounts
		{
			get { return packTypeCounts; }
		}

		readonly ImmutableArray<PackTypeCount> packTypeCounts;

		#endregion

		#endregion

		#region Empty

		public static readonly GroupedPackTypeCounts Empty = new GroupedPackTypeCounts();

		#endregion
	}
}
