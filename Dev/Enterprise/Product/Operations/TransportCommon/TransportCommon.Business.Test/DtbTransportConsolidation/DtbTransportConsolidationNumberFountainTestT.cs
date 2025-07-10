using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportConsolidationNumberFountainTest<T> : NumberFountainUniqueIndexFailureHandlingTest
			where T : DtbTransportConsolidation
	{
		#region Implementation

		protected override Type BizOTypeToTest
		{
			get { return typeof(T); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return DtbBookingConsolidationSchema.KB_JobID; }
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject bizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(bizO);

			var consolidation = (T)bizO;
			consolidation.KB_JobType = ConsolidationJobType;
		}

		protected abstract ZString ConsolidationJobType { get; }

		#endregion
	}
}
