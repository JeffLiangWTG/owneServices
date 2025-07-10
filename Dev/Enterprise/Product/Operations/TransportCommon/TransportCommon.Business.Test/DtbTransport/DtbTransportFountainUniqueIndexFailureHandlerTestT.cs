using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportFountainUniqueIndexFailureHandlerTest<T> : NumberFountainUniqueIndexFailureHandlingTest
			where T : DtbTransport
	{
		protected override Type BizOTypeToTest
		{
			get { return typeof(T); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return DtbBookingSchema.KM_JobID; }
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			var transport = (T)testBizO;
			transport.KM_KB_Booking = TransportConsolidation.PK;
			transport.KM_GB_Branch = GlbBranch.CurrentBranch.PK;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var result = base.AdditionalInsertValues;
				result.Add(DtbBookingSchema.Constants.KM_JobType, string.Format("'{0}'", TransportConsolidation.KB_JobType));
				result.Add(DtbBookingSchema.Constants.KM_KB_Booking, string.Format("'{0}'", TransportConsolidation.PK));
				result.Add(DtbBookingSchema.Constants.KM_GB_Branch, string.Format("'{0}'", GlbBranch.CurrentBranch.PK));
				return result;
			}
		}

		DtbTransportConsolidation TransportConsolidation
		{
			get { return transportConsolidation ?? (transportConsolidation = GetNewConsolidation()); }
		}

		protected abstract DtbTransportConsolidation GetNewConsolidation();

		DtbTransportConsolidation transportConsolidation;
	}
}
