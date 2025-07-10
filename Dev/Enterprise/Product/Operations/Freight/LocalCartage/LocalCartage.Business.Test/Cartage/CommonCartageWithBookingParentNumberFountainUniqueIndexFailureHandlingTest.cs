using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonCartageWithBookingParentNumberFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest
		{
			get
			{
				return typeof(CommonCartage);
			}
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get
			{
				return JobCartageSchema.JJ_ConsignmentID;
			}
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get
			{
				return Env.NumberFountains.JobCartageNumber;
			}
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var result = base.AdditionalInsertValues;
				result.Add(JobCartageSchema.Constants.JJ_GB, string.Format("'{0}'", GlbBranch.CurrentBranch.PK));
				return result;
			}
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);
			var parentBooking = Factory.New<IDtbBooking>();
			var parentBookingConsolidation = Factory.New<IDtbBookingConsolidation>();
			parentBooking.KM_KB_Booking = parentBookingConsolidation.PK;
			var commonCartageBizObj = (CommonCartage)testBizO;
			commonCartageBizObj.JJ_ParentID = parentBooking.PK;
			commonCartageBizObj.JJ_ParentTableCode = "KM";
		}
	}
}
