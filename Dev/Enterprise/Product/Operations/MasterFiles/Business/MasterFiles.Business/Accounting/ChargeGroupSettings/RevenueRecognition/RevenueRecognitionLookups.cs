using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RevenueRecognitionLookups : JobConfigurationSelectorLookups
	{
		public RevenueRecognitionLookups(IRevenueRecognition parent)
			: base(parent)
		{
			this.Parent = parent;
		}
		new readonly IRevenueRecognition Parent;

		#region Broker List

		public CodeDescriptionPairList BrokerList
		{
			get
			{
				if (fBrokerList == null)
				{
					fBrokerList = new CodeDescriptionPairList();
					fBrokerList.AddPair(BrokerCodes.All, BrokerDescriptions.All);
					fBrokerList.AddPair(BrokerCodes.Internal, BrokerDescriptions.Internal);
					fBrokerList.AddPair(BrokerCodes.External, BrokerDescriptions.External);
				}

				return fBrokerList;
			}
		}
		CodeDescriptionPairList fBrokerList;

		public static class BrokerCodes
		{
			public const string All = "ALL";
			public const string Internal = "INT";
			public const string External = "EXT";
		}

		public static class BrokerDescriptions
		{
			public static string All
			{
				get { return Res.GetString("7a81f33b-16bc-4533-9926-5a8f53448746", "All Brokers"); }
			}
			public static string Internal
			{
				get { return Res.GetString("963d3882-57da-4f7f-a923-588b7e254baa", "Broker is OrgProxy of Login Company/Branches"); }
			}
			public static string External
			{
				get { return Res.GetString("89d20551-69e5-4c1c-9201-9d5cec4044f7", "Broker is not OrgProxy of Login Company/Branches"); }
			}
		}

		#endregion

		#region Recognition Date Option List

		public CodeDescriptionPairList RecognitionDateOptionList
		{
			get
			{
				if (Parent.JobType == String.Empty)
				{
					return CompleteRecognitionDateOptionList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.Shipment.Code)
				{
					return RecognitionDateOptionPermittedForShipmentsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code)
				{
					return RecognitionDateOptionPermittedForBookingsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code
					|| Parent.JobType == JobInvoicingConsumerTypes.GatewayConsol.Code)
				{
					return RecognitionDateOptionPermittedForConsolsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.Brokerage.Code)
				{
					return RecognitionDateOptionPermittedForDeclarationsList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.AgencyBillOfLading.Code
					|| Parent.JobType == JobInvoicingConsumerTypes.AgencyBooking.Code)
				{
					return RecognitionDateOptionPermittedForShippingManagerList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.TransportBookingConsignment.Code)
				{
					return RecognitionDateOptionPermittedForTransportConsignmentList;
				}
				if (Parent.JobType == JobInvoicingConsumerTypes.TransportConsignment.Code)
				{
					return RecognitionDateOptionPermittedForTransportConsignmentList;
				}

				return RecognitionDateOptionPermittedForOthersList;
			}
		}

		public static class RecognitionDateOptionCodes
		{
			public const string OldJob = "JOB";
			public const string ActualArrivalDate = AccountingMasterFilesConstants.SignificantDateCodes.ActualArrivalDate;
			public const string ActualDepartureDate = AccountingMasterFilesConstants.SignificantDateCodes.ActualDepartureDate;
			public const string EstimatedArrivalDate = AccountingMasterFilesConstants.SignificantDateCodes.EstimatedArrivalDate;
			public const string EstimatedDepartureDate = AccountingMasterFilesConstants.SignificantDateCodes.EstimatedDepartureDate;
			public const string CustomsClearanceDate = AccountingMasterFilesConstants.SignificantDateCodes.CustomsClearanceDate;
			public const string PickupDate = AccountingMasterFilesConstants.SignificantDateCodes.PickupDate;
			public const string DeliveryDate = AccountingMasterFilesConstants.SignificantDateCodes.DeliveryDate;
			public const string Immediate = "IMM";
			public const string JobClosure = "JCL";
			public const string JobOpenDate = "JOP";
			public const string PostDateOfFirstARTransaction = "FAR";
			public const string AWBIssueDate = AccountingMasterFilesConstants.SignificantDateCodes.AWBIssueDate;
			public const string VesselArrivalDate = AccountingMasterFilesConstants.SignificantDateCodes.VesselArrivalDate;
			public const string VesselDepartureDate = AccountingMasterFilesConstants.SignificantDateCodes.VesselDepartureDate;
		}

		public static class RecognitionDateOptionDescriptions
		{
			public static MultilingualString ActualArrivalDate
			{
				get { return ResString.GetMultilingualString("1608a50c-87b0-48e2-ae36-67cf9a7af33b", "Actual/Estimated Arrival Date"); }
			}
			public static MultilingualString ActualDepartureDate
			{
				get { return ResString.GetMultilingualString("2bc915fd-02cb-43ed-b0d7-2c366d1ab286", "Actual/Estimated Departure Date"); }
			}
			public static MultilingualString EstimatedArrivalDate
			{
				get { return ResString.GetMultilingualString("269a4dd8-d317-4dbe-9735-8ea68d18f6c2", "Estimated Arrival Date"); }
			}
			public static MultilingualString EstimatedDepartureDate
			{
				get { return ResString.GetMultilingualString("c6f55f7c-fd51-48d8-a665-e84b12ea5fb9", "Estimated Departure Date"); }
			}
			public static MultilingualString CustomsClearanceDate
			{
				get { return ResString.GetMultilingualString("a0b97d1c-9396-4c84-8ded-9bb6c6e51ce5", "Customs Clearance Date"); }
			}
			public static MultilingualString PickupDate
			{
				get { return ResString.GetMultilingualString("e96b8aae-fa53-4ed0-9e45-8b5f999720cc", "Pickup Date"); }
			}
			public static MultilingualString DeliveryDate
			{
				get { return ResString.GetMultilingualString("3de27616-b2c7-436e-b4ec-510b0990fe30", "Delivery Date"); }
			}
			public static MultilingualString Immediate
			{
				get { return ResString.GetMultilingualString("3fc51407-5280-47c5-b772-852af9cbcc40", "Immediate"); }
			}
			public static MultilingualString JobClosure
			{
				get { return ResString.GetMultilingualString("ce7d76a6-1908-4683-bd37-6513c0d44b20", "Job Closure"); }
			}
			public static MultilingualString JobOpenDate
			{
				get { return ResString.GetMultilingualString("8244ab3d-449b-48e1-83f8-c224f2aeaa70", "Job Open Date"); }
			}
			public static MultilingualString PostDateOfFirstARTransaction
			{
				get { return ResString.GetMultilingualString("9fdb0439-a577-42de-b28a-c21c751266e4", "Post Date of First AR Transaction"); }
			}
			public static MultilingualString AWBIssueDate
			{
				get { return ResString.GetMultilingualString("113a9147-5979-4459-9f08-a887a6a70e88", "AWB Issue Date"); }
			}
			public static MultilingualString VesselArrivalDate
			{
				get { return ResString.GetMultilingualString("8a7c7b24-22d7-4241-934d-296e181fa509", "Vessel Arrival Date"); }
			}
			public static MultilingualString VesselDepartureDate
			{
				get { return ResString.GetMultilingualString("6257fec4-5afd-48be-9601-b85a89802db8", "Vessel Departure Date"); }
			}
		}

		public static CodeDescriptionPairList CompleteRecognitionDateOptionList
		{
			get
			{
				if (fCompleteRecognitionDateOptionList == null)
				{
					fCompleteRecognitionDateOptionList = new CodeDescriptionPairList();
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.ActualArrivalDate, RecognitionDateOptionDescriptions.ActualArrivalDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.ActualDepartureDate, RecognitionDateOptionDescriptions.ActualDepartureDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.EstimatedArrivalDate, RecognitionDateOptionDescriptions.EstimatedArrivalDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.EstimatedDepartureDate, RecognitionDateOptionDescriptions.EstimatedDepartureDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.PickupDate, RecognitionDateOptionDescriptions.PickupDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.DeliveryDate, RecognitionDateOptionDescriptions.DeliveryDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.Immediate, RecognitionDateOptionDescriptions.Immediate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.JobClosure, RecognitionDateOptionDescriptions.JobClosure);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.JobOpenDate, RecognitionDateOptionDescriptions.JobOpenDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.CustomsClearanceDate, RecognitionDateOptionDescriptions.CustomsClearanceDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.AWBIssueDate, RecognitionDateOptionDescriptions.AWBIssueDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.VesselArrivalDate, RecognitionDateOptionDescriptions.VesselArrivalDate);
					fCompleteRecognitionDateOptionList.AddPair(RecognitionDateOptionCodes.VesselDepartureDate, RecognitionDateOptionDescriptions.VesselDepartureDate);
				}
				return fCompleteRecognitionDateOptionList;
			}
		}
		[ThreadStatic]
		static CodeDescriptionPairList fCompleteRecognitionDateOptionList;

		public CodeDescriptionPairList RecognitionDateOptionPermittedForShipmentsList
		{
			get
			{
				if (fRecognitionDateOptionPermittedForShipmentsList == null)
				{
					fRecognitionDateOptionPermittedForShipmentsList = new CodeDescriptionPairList();
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.ActualArrivalDate, RecognitionDateOptionDescriptions.ActualArrivalDate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.ActualDepartureDate, RecognitionDateOptionDescriptions.ActualDepartureDate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.EstimatedArrivalDate, RecognitionDateOptionDescriptions.EstimatedArrivalDate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.EstimatedDepartureDate, RecognitionDateOptionDescriptions.EstimatedDepartureDate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.PickupDate, RecognitionDateOptionDescriptions.PickupDate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.DeliveryDate, RecognitionDateOptionDescriptions.DeliveryDate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.Immediate, RecognitionDateOptionDescriptions.Immediate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.JobClosure, RecognitionDateOptionDescriptions.JobClosure);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.JobOpenDate, RecognitionDateOptionDescriptions.JobOpenDate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.CustomsClearanceDate, RecognitionDateOptionDescriptions.CustomsClearanceDate);
					fRecognitionDateOptionPermittedForShipmentsList.AddPair(RecognitionDateOptionCodes.AWBIssueDate, RecognitionDateOptionDescriptions.AWBIssueDate);
				}
				return fRecognitionDateOptionPermittedForShipmentsList;
			}
		}
		CodeDescriptionPairList fRecognitionDateOptionPermittedForShipmentsList;

		public CodeDescriptionPairList RecognitionDateOptionPermittedForBookingsList
		{
			get
			{
				if (recognitionDateOptionPermittedForBookingsList == null)
				{
					recognitionDateOptionPermittedForBookingsList = new CodeDescriptionPairList();
					recognitionDateOptionPermittedForBookingsList.AddPair(RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
				}
				return recognitionDateOptionPermittedForBookingsList;
			}
		}
		CodeDescriptionPairList recognitionDateOptionPermittedForBookingsList;

		public CodeDescriptionPairList RecognitionDateOptionPermittedForConsolsList
		{
			get
			{
				if (fRecognitionDateOptionPermittedForConsolsList == null)
				{
					fRecognitionDateOptionPermittedForConsolsList = new CodeDescriptionPairList();
					fRecognitionDateOptionPermittedForConsolsList.AddPair(RecognitionDateOptionCodes.ActualArrivalDate, RecognitionDateOptionDescriptions.ActualArrivalDate);
					fRecognitionDateOptionPermittedForConsolsList.AddPair(RecognitionDateOptionCodes.ActualDepartureDate, RecognitionDateOptionDescriptions.ActualDepartureDate);
					fRecognitionDateOptionPermittedForConsolsList.AddPair(RecognitionDateOptionCodes.EstimatedArrivalDate, RecognitionDateOptionDescriptions.EstimatedArrivalDate);
					fRecognitionDateOptionPermittedForConsolsList.AddPair(RecognitionDateOptionCodes.EstimatedDepartureDate, RecognitionDateOptionDescriptions.EstimatedDepartureDate);
					fRecognitionDateOptionPermittedForConsolsList.AddPair(RecognitionDateOptionCodes.Immediate, RecognitionDateOptionDescriptions.Immediate);
					fRecognitionDateOptionPermittedForConsolsList.AddPair(RecognitionDateOptionCodes.JobClosure, RecognitionDateOptionDescriptions.JobClosure);
					fRecognitionDateOptionPermittedForConsolsList.AddPair(RecognitionDateOptionCodes.JobOpenDate, RecognitionDateOptionDescriptions.JobOpenDate);
					fRecognitionDateOptionPermittedForConsolsList.AddPair(RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
				}
				return fRecognitionDateOptionPermittedForConsolsList;
			}
		}
		CodeDescriptionPairList fRecognitionDateOptionPermittedForConsolsList;

		public CodeDescriptionPairList RecognitionDateOptionPermittedForDeclarationsList
		{
			get
			{
				if (fRecognitionDateOptionPermittedForDeclarationsList == null)
				{
					fRecognitionDateOptionPermittedForDeclarationsList = new CodeDescriptionPairList();
					fRecognitionDateOptionPermittedForDeclarationsList.AddPair(RecognitionDateOptionCodes.Immediate, RecognitionDateOptionDescriptions.Immediate);
					fRecognitionDateOptionPermittedForDeclarationsList.AddPair(RecognitionDateOptionCodes.JobClosure, RecognitionDateOptionDescriptions.JobClosure);
					fRecognitionDateOptionPermittedForDeclarationsList.AddPair(RecognitionDateOptionCodes.JobOpenDate, RecognitionDateOptionDescriptions.JobOpenDate);
					fRecognitionDateOptionPermittedForDeclarationsList.AddPair(RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fRecognitionDateOptionPermittedForDeclarationsList.AddPair(RecognitionDateOptionCodes.CustomsClearanceDate, RecognitionDateOptionDescriptions.CustomsClearanceDate);
				}
				return fRecognitionDateOptionPermittedForDeclarationsList;
			}
		}
		CodeDescriptionPairList fRecognitionDateOptionPermittedForDeclarationsList;

		public CodeDescriptionPairList RecognitionDateOptionPermittedForTransportConsignmentList
		{
			get
			{
				if (fRecognitionDateOptionPermittedForTransportConsignmentList == null)
				{
					fRecognitionDateOptionPermittedForTransportConsignmentList = new CodeDescriptionPairList();
					fRecognitionDateOptionPermittedForTransportConsignmentList.AddPair(RecognitionDateOptionCodes.Immediate, RecognitionDateOptionDescriptions.Immediate);
					fRecognitionDateOptionPermittedForTransportConsignmentList.AddPair(RecognitionDateOptionCodes.JobClosure, RecognitionDateOptionDescriptions.JobClosure);
					fRecognitionDateOptionPermittedForTransportConsignmentList.AddPair(RecognitionDateOptionCodes.JobOpenDate, RecognitionDateOptionDescriptions.JobOpenDate);
					fRecognitionDateOptionPermittedForTransportConsignmentList.AddPair(RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fRecognitionDateOptionPermittedForTransportConsignmentList.AddPair(RecognitionDateOptionCodes.PickupDate, RecognitionDateOptionDescriptions.PickupDate);
					fRecognitionDateOptionPermittedForTransportConsignmentList.AddPair(RecognitionDateOptionCodes.DeliveryDate, RecognitionDateOptionDescriptions.DeliveryDate);
				}
				return fRecognitionDateOptionPermittedForTransportConsignmentList;
			}
		}
		CodeDescriptionPairList fRecognitionDateOptionPermittedForTransportConsignmentList;

		public CodeDescriptionPairList RecognitionDateOptionPermittedForOthersList
		{
			get
			{
				if (fRecognitionDateOptionPermittedForOthersList == null)
				{
					fRecognitionDateOptionPermittedForOthersList = new CodeDescriptionPairList();
					fRecognitionDateOptionPermittedForOthersList.AddPair(RecognitionDateOptionCodes.Immediate, RecognitionDateOptionDescriptions.Immediate);
					fRecognitionDateOptionPermittedForOthersList.AddPair(RecognitionDateOptionCodes.JobClosure, RecognitionDateOptionDescriptions.JobClosure);
					fRecognitionDateOptionPermittedForOthersList.AddPair(RecognitionDateOptionCodes.JobOpenDate, RecognitionDateOptionDescriptions.JobOpenDate);
					fRecognitionDateOptionPermittedForOthersList.AddPair(RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
				}
				return fRecognitionDateOptionPermittedForOthersList;
			}
		}
		CodeDescriptionPairList fRecognitionDateOptionPermittedForOthersList;

		public CodeDescriptionPairList RecognitionDateOptionPermittedForShippingManagerList
		{
			get
			{
				if (fRecognitionDateOptionPermittedForShippingManagerList == null)
				{
					fRecognitionDateOptionPermittedForShippingManagerList = new CodeDescriptionPairList();
					fRecognitionDateOptionPermittedForShippingManagerList.AddPair(RecognitionDateOptionCodes.Immediate, RecognitionDateOptionDescriptions.Immediate);
					fRecognitionDateOptionPermittedForShippingManagerList.AddPair(RecognitionDateOptionCodes.JobClosure, RecognitionDateOptionDescriptions.JobClosure);
					fRecognitionDateOptionPermittedForShippingManagerList.AddPair(RecognitionDateOptionCodes.JobOpenDate, RecognitionDateOptionDescriptions.JobOpenDate);
					fRecognitionDateOptionPermittedForShippingManagerList.AddPair(RecognitionDateOptionCodes.PostDateOfFirstARTransaction, RecognitionDateOptionDescriptions.PostDateOfFirstARTransaction);
					fRecognitionDateOptionPermittedForShippingManagerList.AddPair(RecognitionDateOptionCodes.VesselArrivalDate, RecognitionDateOptionDescriptions.VesselArrivalDate);
					fRecognitionDateOptionPermittedForShippingManagerList.AddPair(RecognitionDateOptionCodes.VesselDepartureDate, RecognitionDateOptionDescriptions.VesselDepartureDate);
				}
				return fRecognitionDateOptionPermittedForShippingManagerList;
			}
		}
		CodeDescriptionPairList fRecognitionDateOptionPermittedForShippingManagerList;

		#endregion

		#region Offset Type List

		public CodeDescriptionPairList OffsetTypeList
		{
			get
			{
				helper = helper ?? new JobConfigurationSelectorHelper();
				return helper.OffsetTypeList;
			}
		}
		JobConfigurationSelectorHelper helper;

		#endregion

	}
}
