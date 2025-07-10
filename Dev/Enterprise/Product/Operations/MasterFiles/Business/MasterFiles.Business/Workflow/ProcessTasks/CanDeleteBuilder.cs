using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[Immutable]
	class CanDeleteProvider<T>
	{
		CanDeleteProvider(IEnumerable<DeleteReason> reasons)
		{
			preventDeleteReasons = reasons.ToImmutableArray();
		}

		readonly ImmutableArray<DeleteReason> preventDeleteReasons;

		public bool CanDelete(T obj) => preventDeleteReasons.All(reason => !reason.Filter(obj));

		public MultilingualString GetReasonForNotAbleToDelete(T obj) => MultilingualString.Join(System.Environment.NewLine, preventDeleteReasons.Where(r => r.Filter(obj)).Select(s => s.Reason(obj)).ToArray());

		public class Builder
		{
			public Builder() { }

			public Builder PreventDelete(Func<T, bool> filter, Func<T, MultilingualString> reasonGetter)
			{
				reasons.Add(new DeleteReason(filter, reasonGetter));
				return this;
			}

			readonly List<DeleteReason> reasons = new List<DeleteReason>();
			public CanDeleteProvider<T> Build() => new CanDeleteProvider<T>(reasons);
		}

		[Immutable]
		public class DeleteReason
		{
			public DeleteReason(Func<T, bool> filter, Func<T, MultilingualString> reasonGetter)
			{
				this.filter = filter;
				this.reason = reasonGetter;
			}

			readonly Func<T, bool> filter;
			readonly Func<T, MultilingualString> reason;
			public Func<T, bool> Filter => filter;
			public Func<T, MultilingualString> Reason => reason;
		}
	}
}
