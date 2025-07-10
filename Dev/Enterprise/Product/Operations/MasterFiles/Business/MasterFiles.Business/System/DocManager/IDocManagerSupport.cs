using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IDocManagerSupport : IDocManagerSupportBase
	{
		DocManagerInfo DocManagerInfo { get; }
	}

	public interface IDocManagerSupportCore : IDocManagerSupportBase
	{
		IDocManagerInfoCore DocManagerInfo { get; }
	}

	public interface IDocManagerSupportProvider
	{
		IEnumerable<IDocManagerSupport> DocManagerSupports { get; }
	}

	public static class IDocManagerSupportExtensions
	{
		public static DocManagerInfo DocManagerInfo(this IDocManagerSupportBase docManagerParent)
		{
			return ((IDocManagerSupport)docManagerParent).DocManagerInfo;
		}

		public static IDocManagerInfoCore DocManagerInfoCore(this IDocManagerSupportBase docManagerParent)
		{
			switch (docManagerParent)
			{
				case IDocManagerSupportCore docManagerSupportCore:
					return docManagerSupportCore.DocManagerInfo;
				case IDocManagerSupport docManagerSupport:
					return docManagerSupport.DocManagerInfo;
				default:
					throw new InvalidCastException(FormattableString.Invariant($"docManagerParent of type {docManagerParent.GetType().FullName} cannot be cast to {nameof(IDocManagerSupport)} or {nameof(IDocManagerInfoCore)}."));
			}
		}
	}
}
