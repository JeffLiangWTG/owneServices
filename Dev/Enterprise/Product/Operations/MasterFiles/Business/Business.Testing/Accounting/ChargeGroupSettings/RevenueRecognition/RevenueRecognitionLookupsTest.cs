using System;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class RevenueRecognitionLookupsTest : JobConfigurationSelectorLookupsTest
	{
		public void TestRecognitionDateOptionList()
		{
			// RecognitionDateOptionList depends on JobType. This test only checks list for the empty job type.
			BizObj.JobType = ZString.Empty;

			AssertEquals("RecognitionDateOptionList.Count", 14, BizObj.RecognitionDateOptionList.Count);
			AssertEquals("RecognitionDateOptionList should contain 'Immediate'", true, BizObj.RecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate));
			AssertEquals("RecognitionDateOptionList should contain 'Actual Departure Date'", true, BizObj.RecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));
			AssertEquals("RecognitionDateOptionList should contain 'Customs Clearance Date'", true, BizObj.RecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals("RecognitionDateOptionList should contain 'Post Date of First AR Transaction'", true, BizObj.RecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			AssertEquals("RecognitionDateOptionList should contain 'AWB Issue Date'", true, BizObj.RecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate));
			AssertEquals("RecognitionDateOptionList should contain 'Vessel Arrival Date'", true, BizObj.RecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselArrivalDate));
			AssertEquals("RecognitionDateOptionList should contain 'Vessel Departure Date'", true, BizObj.RecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselDepartureDate));
		}

		public void TestCompleteRecognitionDateOptionList()
		{
			AssertEquals("RecognitionDateOptionList.Count", 14, CompleteRecognitionDateOptionList.Count);
			AssertEquals("RecognitionDateOptionList should contain 'Immediate'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate));
			AssertEquals("RecognitionDateOptionList should contain 'Actual Departure Date'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));
			AssertEquals("RecognitionDateOptionList should contain 'Estimated Departure Date'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedDepartureDate));
			AssertEquals("RecognitionDateOptionList should contain 'Estimated Arrival Date'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate));
			AssertEquals("RecognitionDateOptionList should contain 'Customs Clearance Date'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals("RecognitionDateOptionList should contain 'Post Date of First AR Transaction'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			AssertEquals("RecognitionDateOptionList should contain 'AWB Issue Date'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate));
			AssertEquals("RecognitionDateOptionList should contain 'Vessel Arrival Date'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselArrivalDate));
			AssertEquals("RecognitionDateOptionList should contain 'Vessel Departure Date'", true, CompleteRecognitionDateOptionList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselDepartureDate));
		}

		public void TestRecognitionDateOptionPermittedForTransportConsignmentList()
		{
			AssertEquals("RecognitionDateOptionList.Count", 6, RecognitionDateOptionPermittedForTransportConsignmentList.Count);
			AssertEquals("RecognitionDateOptionList should contain 'Immediate'", true, RecognitionDateOptionPermittedForTransportConsignmentList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate));
			AssertEquals("RecognitionDateOptionList should contain 'Job Closure'", true, RecognitionDateOptionPermittedForTransportConsignmentList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure));
			AssertEquals("RecognitionDateOptionList should contain 'Job Open Date'", true, RecognitionDateOptionPermittedForTransportConsignmentList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
			AssertEquals("RecognitionDateOptionList should not contain 'Customs Clearance Date'", false, RecognitionDateOptionPermittedForTransportConsignmentList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals("RecognitionDateOptionList should contain 'Post Date of First AR Transaction'", true, RecognitionDateOptionPermittedForTransportConsignmentList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			AssertEquals("RecognitionDateOptionList should contain 'Pickup Date'", true, RecognitionDateOptionPermittedForTransportConsignmentList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate));
			AssertEquals("RecognitionDateOptionList should contain 'Delivery Date'", true, RecognitionDateOptionPermittedForTransportConsignmentList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate));
		}

		public void TestRecognitionDateOptionPermittedForOthersList()
		{
			AssertEquals("RecognitionDateOptionList.Count", 4, RecognitionDateOptionPermittedForOthersList.Count);
			AssertEquals("RecognitionDateOptionList should contain 'Immediate'", true, RecognitionDateOptionPermittedForOthersList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate));
			AssertEquals("RecognitionDateOptionList should contain 'Job Closure'", true, RecognitionDateOptionPermittedForOthersList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure));
			AssertEquals("RecognitionDateOptionList should contain 'Job Open Date'", true, RecognitionDateOptionPermittedForOthersList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
			AssertEquals("RecognitionDateOptionList should not contain 'Customs Clearance Date'", false, RecognitionDateOptionPermittedForOthersList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals("RecognitionDateOptionList should contain 'Post Date of First AR Transaction'", true, RecognitionDateOptionPermittedForOthersList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
		}

		public void TestRecognitionDateOptionPermittedForShipmentsList()
		{
			AssertEquals("RecognitionDateOptionList.Count", 12, RecognitionDateOptionPermittedForShipmentsList.Count);
			AssertEquals("RecognitionDateOptionList should contain 'Immediate'", true, RecognitionDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate));
			AssertEquals("RecognitionDateOptionList should contain 'Actual Departure Date'", true, RecognitionDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualDepartureDate));
			AssertEquals("RecognitionDateOptionList should contain 'Estimated Departure Date'", true, RecognitionDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedDepartureDate));
			AssertEquals("RecognitionDateOptionList should contain 'Estimated Arrival Date'", true, RecognitionDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.EstimatedArrivalDate));
			AssertEquals("RecognitionDateOptionList should contain 'Customs Clearance Date'", true, RecognitionDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
			AssertEquals("RecognitionDateOptionList should contain 'Post Date of First AR Transaction'", true, RecognitionDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			AssertEquals("RecognitionDateOptionList should contain 'AWB Issue Date'", true, RecognitionDateOptionPermittedForShipmentsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.AWBIssueDate));
		}

		public void TestRecognitionDateOptionPermittedForDeclarationsList()
		{
			AssertionWithHtml.CombineAssertions(delegate
			{
				AssertEquals("RecognitionDateOptionList.Count", 5, RecognitionDateOptionPermittedForDeclarationsList.Count);
				AssertEquals("RecognitionDateOptionList should contain 'Immediate'", true, RecognitionDateOptionPermittedForDeclarationsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate));
				AssertEquals("RecognitionDateOptionList should contain 'Job Closure'", true, RecognitionDateOptionPermittedForDeclarationsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure));
				AssertEquals("RecognitionDateOptionList should contain 'Job Open Date'", true, RecognitionDateOptionPermittedForDeclarationsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
				AssertEquals("RecognitionDateOptionList should contain 'Customs Clearance Date'", true, RecognitionDateOptionPermittedForDeclarationsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate));
				AssertEquals("RecognitionDateOptionList should contain 'Post Date of First AR Transaction'", true, RecognitionDateOptionPermittedForDeclarationsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
			});
		}

		public void TestRecognitionDateOptionPermittedForShippingManagerList()
		{
			AssertionWithHtml.CombineAssertions(delegate
			{
				AssertEquals("RecognitionDateOptionList.Count", 6, RecognitionDateOptionPermittedForShippingManagerList.Count);
				AssertEquals("RecognitionDateOptionList should contain 'Immediate'", true, RecognitionDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate));
				AssertEquals("RecognitionDateOptionList should contain 'Job Closure'", true, RecognitionDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure));
				AssertEquals("RecognitionDateOptionList should contain 'Job Open Date'", true, RecognitionDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate));
				AssertEquals("RecognitionDateOptionList should contain 'Post Date of First AR Transaction'", true, RecognitionDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
				AssertEquals("RecognitionDateOptionList should contain 'Vessel Arrival Date'", true, RecognitionDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselArrivalDate));
				AssertEquals("RecognitionDateOptionList should contain 'Vessel Departure Date'", true, RecognitionDateOptionPermittedForShippingManagerList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.VesselDepartureDate));
			});
		}

		public void TestRecognitionDateOptionPermittedForBookingsList()
		{
			AssertEquals("RecognitionDateOptionList.Count", 1, RecognitionDateOptionPermittedForBookingsList.Count);
			AssertEquals("RecognitionDateOptionList should contain 'Post Date of First AR Transaction'", true, RecognitionDateOptionPermittedForBookingsList.ContainsCode(RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction));
		}

		public void TestOffestTypeList()
		{
			AssertEquals("OffestTypeList.Count", 2, BizObj.OffsetTypeList.Count);
			AssertEquals("The OffestTypeList should contain 'DAY'", true, BizObj.OffsetTypeList.ContainsCode(JobConfigurationSelectorHelper.OffsetTypeCodes.Days));
			AssertEquals("The OffestTypeList should contain 'PER'", true, BizObj.OffsetTypeList.ContainsCode(JobConfigurationSelectorHelper.OffsetTypeCodes.Periods));
		}

		public void TestCorrectRecognitionDateOptionListUsed()
		{
			BizObj.JobType = String.Empty;
			AssertListsAreSame(BizObj.RecognitionDateOptionList, CompleteRecognitionDateOptionList);

			BizObj.JobType = "ALL";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForOthersList);

			BizObj.JobType = "SHP";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForShipmentsList);

			BizObj.JobType = "FCN";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForConsolsList);

			BizObj.JobType = "GCN";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForConsolsList);

			BizObj.JobType = "BRK";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForDeclarationsList);

			BizObj.JobType = "AGS";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForShippingManagerList);

			BizObj.JobType = "QSH";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForBookingsList);

			BizObj.JobType = "AGB";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForShippingManagerList);

			BizObj.JobType = "TCW";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForTransportConsignmentList);

			BizObj.JobType = "LTC";
			AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForTransportConsignmentList);

			foreach (ICodeDescription jobType in JobTypeList)
			{
				if (jobType.Code != "ALL" && jobType.Code != "SHP" && jobType.Code != "BRK" && jobType.Code != "FCN" && jobType.Code != "GCN" && jobType.Code != "AGS" && jobType.Code != "AGB" && jobType.Code != "QSH" && jobType.Code != "TCW" && jobType.Code != "LTC")
				{
					BizObj.JobType = jobType.Code;
					AssertListsAreSame(BizObj.RecognitionDateOptionList, RecognitionDateOptionPermittedForOthersList);
				}
			}
		}

		public void TestBrokerList()
		{
			AssertEquals("BrokerList.Count", 3, BizObj.BrokerList.Count);
			AssertEquals("The BrokerList should contain 'All'", true, BizObj.BrokerList.ContainsCode(RevenueRecognitionLookups.BrokerCodes.All));
			AssertEquals("The BrokerList should contain 'Internal'", true, BizObj.BrokerList.ContainsCode(RevenueRecognitionLookups.BrokerCodes.Internal));
			AssertEquals("The BrokerList should contain 'External'", true, BizObj.BrokerList.ContainsCode(RevenueRecognitionLookups.BrokerCodes.External));
		}

		#region Implementation

		protected new IRevenueRecognition BizObj
		{
			get { return (IRevenueRecognition)base.BizObj; }
			set { base.BizObj = value; }
		}

		CodeDescriptionPairList fCompleteRecognitionDateOptionList;
		CodeDescriptionPairList CompleteRecognitionDateOptionList
		{
			get { return fCompleteRecognitionDateOptionList ?? (fCompleteRecognitionDateOptionList = RevenueRecognitionLookups.CompleteRecognitionDateOptionList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForTransportConsignmentList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForTransportConsignmentList
		{
			get { return fRecognitionDateOptionPermittedForTransportConsignmentList ?? (fRecognitionDateOptionPermittedForTransportConsignmentList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForTransportConsignmentList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForOthersList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForOthersList
		{
			get { return fRecognitionDateOptionPermittedForOthersList ?? (fRecognitionDateOptionPermittedForOthersList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForOthersList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForConsolsList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForConsolsList
		{
			get { return fRecognitionDateOptionPermittedForConsolsList ?? (fRecognitionDateOptionPermittedForConsolsList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForConsolsList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForShipmentsList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForShipmentsList
		{
			get { return fRecognitionDateOptionPermittedForShipmentsList ?? (fRecognitionDateOptionPermittedForShipmentsList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForShipmentsList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForDeclarationsList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForDeclarationsList
		{
			get { return fRecognitionDateOptionPermittedForDeclarationsList ?? (fRecognitionDateOptionPermittedForDeclarationsList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForDeclarationsList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForShippingManagerList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForShippingManagerList
		{
			get { return fRecognitionDateOptionPermittedForShippingManagerList ?? (fRecognitionDateOptionPermittedForShippingManagerList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForShippingManagerList); }
		}

		CodeDescriptionPairList recognitionDateOptionPermittedForBookingsList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForBookingsList
		{
			get { return recognitionDateOptionPermittedForBookingsList ?? (recognitionDateOptionPermittedForBookingsList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForBookingsList); }
		}

		#endregion

	}
}
