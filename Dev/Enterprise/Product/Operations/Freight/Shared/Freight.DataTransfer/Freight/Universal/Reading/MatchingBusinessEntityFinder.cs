using System;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class MatchingBusinessEntityFinder<T> : IMatchingBusinessEntityFinder<T> where T : BusinessObject
	{
		public MatchingBusinessEntityFinder(Func<T> businessObjectProvider)
		{
			this.businessObjectProvider = businessObjectProvider;
		}

		public MatchingBusinessEntityFinder(T businessObject)
			: this(() => businessObject)
		{
		}

		readonly Func<T> businessObjectProvider;

		T IMatchingBusinessEntityFinder<T>.GetBestMatch()
		{
			return businessObjectProvider != null
				? businessObjectProvider()
				: null;
		}
	}
}
