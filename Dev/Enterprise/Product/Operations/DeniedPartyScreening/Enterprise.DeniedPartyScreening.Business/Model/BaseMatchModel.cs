using System.Collections.Generic;

namespace Enterprise.DeniedPartyScreening.Business
{
	public abstract class BaseMatchModel<T>
	{
		public abstract List<T> TotalScreenedDeniedItems { get; }
	}
}
