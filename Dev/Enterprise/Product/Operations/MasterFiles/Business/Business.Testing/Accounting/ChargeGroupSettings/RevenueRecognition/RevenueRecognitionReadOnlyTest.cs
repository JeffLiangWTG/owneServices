using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class RevenueRecognitionReadOnlyTest : JobConfigurationSelectorReadOnlyTest
	{
		public void TestBrokerReadOnly()
		{
			BizObj.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
			BizObj.JobType = "BRK";
			AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo.IsEmpty", true, BizObj.BrokerCode.IsEmpty);

			BizObj.JobType = "SHP";
			AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo.IsEmpty", true, BizObj.BrokerCode.IsEmpty);

			BizObj.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			AssertEquals("BrokerInfo.ReadOnly", false, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo.IsEmpty", false, BizObj.BrokerCode.IsEmpty);

			BizObj.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo.IsEmpty", true, BizObj.BrokerCode.IsEmpty);

			BizObj.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			AssertEquals("BrokerInfo.ReadOnly", false, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo.IsEmpty", false, BizObj.BrokerCode.IsEmpty);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				BizObj.JobType = "!@#";
				AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
				AssertEquals("BrokerInfo.IsEmpty", true, BizObj.BrokerCode.IsEmpty);

				BizObj.JobType = "";
				AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
				AssertEquals("BrokerInfo.IsEmpty", true, BizObj.BrokerCode.IsEmpty);
			}

			BizObj.JobType = "AWB";
			AssertEquals("BrokerInfo.ReadOnly", true, BizObj.BrokerCodeInfo.ReadOnly);
			AssertEquals("BrokerInfo.IsEmpty", true, BizObj.BrokerCode.IsEmpty);
		}

		public void TestOffsetReadOnly()
		{
			BizObj.Offset = 10;
			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AssertEquals("OffsetInfo.ReadOnly", true, BizObj.OffsetInfo.ReadOnly);
			AssertEquals("OffsetInfo value", 0, BizObj.Offset);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AssertEquals("OffsetInfo.ReadOnly", false, BizObj.OffsetInfo.ReadOnly);
			AssertEquals("OffsetInfo value", 10, BizObj.Offset);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AssertEquals("OffsetInfo.ReadOnly", false, BizObj.OffsetInfo.ReadOnly);
			AssertEquals("OffsetInfo value", 10, BizObj.Offset);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			AssertEquals("OffsetInfo.ReadOnly", false, BizObj.OffsetInfo.ReadOnly);
			AssertEquals("OffsetInfo value", 10, BizObj.Offset);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			AssertEquals("OffsetInfo.ReadOnly", false, BizObj.OffsetInfo.ReadOnly);
			AssertEquals("OffsetInfo value", 10, BizObj.Offset);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AssertEquals("OffsetInfo.ReadOnly", false, BizObj.OffsetInfo.ReadOnly);
			AssertEquals("OffsetInfo value", 10, BizObj.Offset);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AssertEquals("OffsetInfo.ReadOnly", false, BizObj.OffsetInfo.ReadOnly);
			AssertEquals("OffsetInfo value", 10, BizObj.Offset);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AssertEquals("OffsetInfo.ReadOnly", false, BizObj.OffsetInfo.ReadOnly);
			AssertEquals("OffsetInfo value", 10, BizObj.Offset);
		}

		public void TestOffsetTypeReadOnly()
		{
			BizObj.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Periods;
			ZString expectedValue = JobConfigurationSelectorHelper.OffsetTypeCodes.Periods;
			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			AssertEquals("OffsetTypeInfo.ReadOnly", true, BizObj.OffsetTypeInfo.ReadOnly);
			AssertEquals("OffsetTypeInfo value", "", BizObj.OffsetType);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AssertEquals("OffsetTypeInfo.ReadOnly", false, BizObj.OffsetTypeInfo.ReadOnly);
			AssertEquals("OffsetTypeInfo value", expectedValue, BizObj.OffsetType);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate;
			AssertEquals("OffsetTypeInfo.ReadOnly", false, BizObj.OffsetTypeInfo.ReadOnly);
			AssertEquals("OffsetTypeInfo value", expectedValue, BizObj.OffsetType);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			AssertEquals("OffsetTypeInfo.ReadOnly", false, BizObj.OffsetTypeInfo.ReadOnly);
			AssertEquals("OffsetTypeInfo value", expectedValue, BizObj.OffsetType);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			AssertEquals("OffsetTypeInfo.ReadOnly", false, BizObj.OffsetTypeInfo.ReadOnly);
			AssertEquals("OffsetTypeInfo value", expectedValue, BizObj.OffsetType);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AssertEquals("OffsetTypeInfo.ReadOnly", false, BizObj.OffsetTypeInfo.ReadOnly);
			AssertEquals("OffsetTypeInfo value", expectedValue, BizObj.OffsetType);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			AssertEquals("OffsetTypeInfo.ReadOnly", false, BizObj.OffsetTypeInfo.ReadOnly);
			AssertEquals("OffsetTypeInfo value", expectedValue, BizObj.OffsetType);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AssertEquals("OffsetTypeInfo.ReadOnly", false, BizObj.OffsetTypeInfo.ReadOnly);
			AssertEquals("OffsetTypeInfo value", expectedValue, BizObj.OffsetType);
		}

		#region Implementation

		protected new IRevenueRecognition BizObj
		{
			get { return (IRevenueRecognition)base.BizObj; }
			set { base.BizObj = value; }
		}
		#endregion
	}
}
