using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	class FinalisedDateFromFinaliseEvent : IFinalisedDateProvider
	{
		public FinalisedDateFromFinaliseEvent(ZDateTimeOffset finalisedTimeOffset)
		{
			if (!finalisedTimeOffset.IsValid)
			{
				throw new ArgumentException("Must provide a valid finalised Time.", nameof(finalisedTimeOffset));
			}

			FinalisedTimeOffset = finalisedTimeOffset;
		}

		ZDateTimeOffset FinalisedTimeOffset { get; }

		ZDateTimeOffset IFinalisedDateProvider.GetFinalisationTimeOffset() => FinalisedTimeOffset;
	}
}
