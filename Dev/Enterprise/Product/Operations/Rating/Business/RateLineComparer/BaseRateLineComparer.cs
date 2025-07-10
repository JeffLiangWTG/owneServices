using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	internal abstract class BaseRateLineComparer : IComparer<FastLine>, IEquatable<BaseRateLineComparer>
	{
		public abstract int Compare(FastLine line1, FastLine line2);

		public virtual bool Equals(BaseRateLineComparer other)
		{
			return GetType() == other.GetType();
		}

		public string GetReason(FastLine overriddenLine, FastLine overriddenBy)
		{
			var reason = GetReasonCore(overriddenLine, overriddenBy);

			if (!string.IsNullOrWhiteSpace(reason))
			{
				reason = (NoResString)" due to " + reason; // log message, subject to change, more for support people as of now
			}
			else
			{
				reason = (NoResString)" by " + GetName() + (NoResString)" comparer"; // log message, subject to change, more for support people as of now
			}

			return ZString.Format((NoResString)"overridden by {0}{1}", overriddenBy.DisplayInfo(), reason); // log message, subject to change, more for support people as of now
		}

		protected virtual string GetReasonCore(FastLine overriddenLine, FastLine overriddenBy)
		{
			return string.Empty;
		}

		protected abstract string GetName();
	}
}
