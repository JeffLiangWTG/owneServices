namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Freight.Forwarding.Business;

	class MAFValidator : IMAFValidator
	{
		public MAFValidator(IMAFMessagingRequest messagingRequest)
		{
			Argument.NotNull(messagingRequest, "request");
			this.messagingRequest = messagingRequest;
		}

		#region Implementation of IMAFValidator

		public ZString GetWarningMessage()
		{
			var additionalValidator = messagingRequest.MetaData.Source as IMAFValidator;
			return additionalValidator != null ? additionalValidator.GetWarningMessage() : ZString.Empty;
		}

		public ZString GetErrorMessage()
		{
			ValidateAll();
			var errorBuilder = new ZStringBuilder(errors.Select(error => error.FormattedMessage));
			var additionalValidator = messagingRequest.MetaData.Source as IMAFValidator;
			if (additionalValidator != null)
			{
				var message = additionalValidator.GetErrorMessage();
				if (!message.IsEmpty)
				{
					errorBuilder.Prepend(message);
				}
			}
			return errorBuilder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region Implementation

		#region ValidateAll

		void ValidateAll()
		{
			errors = new List<Error>();

			ValidateMessagingRequest(messagingRequest);
			ValidateEBACCARequest(messagingRequest.MetaData);

			errors.Sort();
		}

		void ValidateMessagingRequest(IMAFMessagingRequest request)
		{
			CheckMandatoryField(request.SenderName, "Company Name in your Company Registration");
			CheckMandatoryField(request.SenderAddress, "POP3 Email Address in the Registry");
		}

		void ValidateEBACCARequest(IMAFMessagingMetaData ebaccaData)
		{
			var isDeclaration = ebaccaData.Source.Master is JobDeclaration;
			var consol = ebaccaData.Source.Master as ForwardingConsol;
			ValidateBroker(ebaccaData.Source.Broker);
			ValidateImporter(ebaccaData.Importer, isDeclaration ? "an Importer" : "a Receiving Agent");
			ValidateExporter(ebaccaData.Exporter, isDeclaration ? "Supplier" : "Sending Agent");
			ValidateDetails(ebaccaData);
			ValidateShipmentIdentifiers(ebaccaData);
			ValidateConsignmentDescription(ebaccaData);
			ValidateCommodities(ebaccaData.Source.Commodities);
			ValidateReferences(ebaccaData);
			if (consol != null)
			{
				ValidateGoodsLocation(consol);
			}
			else
			{
				ValidateTransitionalFacility(ebaccaData, isDeclaration ? "Depot or Delivery Address with ATF Code" : "Arrival CFS with ATF Code");
			}

			ValidateTreatmentProvider(ebaccaData.Source.TreatmentProvider);
			ValidatePaymentDetails(ebaccaData);
		}

		void ValidateBroker(IMAFOrganisation broker)
		{
			if (broker == null)
			{
				Fail("You must setup Brokerage Details to send an eBACCa/IPI.");
				return;
			}
			CheckMandatoryField(broker.OrganisationCode, "NZ Customs Brokerage ID in the Registry for this Company");
			ValidateMAFOrganisation(broker, "Branch");
			CheckMandatoryField(broker.ContactName, "Brokerage Contact Name");

			if (broker.ContactEmail.IsEmpty && broker.ContactFax.IsEmpty)
			{
				Fail("You must have at least one Email Address or Fax Number setup for the Currently Logged in User/Branch.");
			}
		}

		void ValidateImporter(IMAFOrganisation importerData, string description)
		{
			if (importerData == null)
			{
				Fail(string.Format("You must have {0} when sending an eBACCa/IPI.", description));
				return;
			}
			ValidateMAFOrganisation(importerData, description);
		}

		void ValidateExporter(IMAFOrganisation exporterData, string description)
		{
			if (exporterData == null)
			{
				Fail(string.Format("You must have a {0} when sending an eBACCa/IPI.", description));
				return;
			}
			ValidateMAFOrganisation(exporterData, description);
		}

		void ValidateDetails(IMAFMessagingMetaData ebaccaData)
		{
			CheckMandatoryField(ebaccaData.ProcessingOffice, "MPI Processing Office");
			CheckMandatoryField(ebaccaData.ConsignmentType, "MPI Consignment Type");
			CheckMandatoryField(ebaccaData.Source.OriginCountry, "Port of Origin");
			CheckMandatoryField(ebaccaData.Source.DischargePorts, "Port of Discharge");
			CheckMandatoryField(ebaccaData.Source.Destinations, "Port of Destination");

			if (!string.IsNullOrEmpty(ebaccaData.Source.FlightNumber))
			{
				CheckMandatoryField(ebaccaData.Source.FlightNumber, "Flight Number");
				CheckMandatoryField(ebaccaData.Source.FlightArrivalDate, "Actual Time of Arrival");
			}
			else if (!string.IsNullOrEmpty(ebaccaData.Source.ShipName))
			{
				CheckMandatoryField(ebaccaData.Source.ShipName, "Vessel Name");
				CheckMandatoryField(ebaccaData.Source.VoyageNumber, "Voyage Number");
				CheckMandatoryField(ebaccaData.Source.ShippingCompany, "Shipping Line");
				CheckMandatoryField(ebaccaData.CargoType, "Cargo Type for a Sea eBACCa/IPI");
			}
			else
			{
				Fail("You must supply either a Flight number for Air shipments or a Vessel Name for Sea shipments.");
			}
		}

		void ValidateShipmentIdentifiers(IMAFMessagingMetaData ebaccaData)
		{
			if (!ebaccaData.Source.BillOfLadingNumbers.Any() && !ebaccaData.Source.SubBillOfLadingNumbers.Any())
			{
				Fail("You must have at least one Master Bill or House Bill number.");
			}

			for (var index = 0; index < ebaccaData.Source.BillOfLadingNumbers.Count(); index++)
			{
				if (string.IsNullOrEmpty(ebaccaData.Source.BillOfLadingNumbers.ElementAt(index)))
				{
					Fail("Master Bill " + (index + 1) + ": You cannot have a Blank Master Bill on a packing row.");
				}
			}
			var maxCount = ebaccaData.Source.SubBillOfLadingNumbers.Count();
			for (var index = 0; index < maxCount; index++)
			{
				if (string.IsNullOrEmpty(ebaccaData.Source.SubBillOfLadingNumbers.ElementAt(index)))
				{
					Fail("House Bill " + (index + 1) + ": You cannot have a Blank House Bill on a packing row.");
				}
			}

			foreach (var containerData in ebaccaData.Source.Containers)
			{
				CheckMandatoryField(containerData.ContainerNumber, "Container Number on All Containers");
				CheckMandatoryField(containerData.ContainerType, "MPI Container Type for Container No [" + containerData.ContainerNumber + "]");
			}
		}

		void ValidateConsignmentDescription(IMAFMessagingMetaData ebaccaData)
		{
			CheckMandatoryField(ebaccaData.Source.ConsignmentDescription, "Goods Description");
			CheckMandatoryField(ebaccaData.MeasurementValue, "MPI Total Quantity");
			CheckMandatoryField(ebaccaData.MeasurementUQ, "MPI Total Quantity Unit");
		}

		void ValidateCommodities(IEnumerable<IMAFCommodity> commodities)
		{
			if (!commodities.Any())
			{
				Fail("You must have at least one Commodity (Goods Type or Merged Line).");
			}
			else
			{
				foreach (var commodity in commodities)
				{
					try
					{
						currentLine = commodity.MergedLineNumber;
						CheckMandatoryField(commodity.GoodsType, "MPI Goods Type");
						CheckMandatoryField(commodity.GoodsDescription, "Goods Description");
						CheckMandatoryField(commodity.IsNew, "setting for 'Is New'");
						if (!commodity.GoodsMeasurements.Any())
						{
							Fail("You must have at least one Measurement for each commodity line.");
						}
						else
						{
							foreach (var measurement in commodity.GoodsMeasurements)
							{
								CheckMandatoryField(measurement.MeasurementUQ, "Measurement Unit");
							}
						}
					}
					finally
					{
						currentLine = 0;
					}
				}
			}
		}

		void ValidateReferences(IMAFMessagingMetaData ebaccaData)
		{
			if (!string.IsNullOrEmpty(ebaccaData.Source.ShipName) && ebaccaData.CargoType == CargoTypeList.Codes.Fcl)
			{
				CheckMandatoryField(ebaccaData.Source.CustomsEntryNumber, "Entry Number for an FCL eBACCa/IPI");
			}
		}

		void ValidateTransitionalFacility(IMAFMessagingMetaData ebaccaData, string description)
		{
			var transitionalFacility = ebaccaData.Source.TransitionalFacility;
			if (transitionalFacility != null)
			{
				ValidateMAFOrganisation(transitionalFacility, "Transitional Facility");
			}
			else
			{
				if (!string.IsNullOrEmpty(ebaccaData.Source.ShipName) && ebaccaData.Source.Containers.Any())
				{
					Fail(string.Format("You must specify a Transitional Facility ({0}) for all Seafreight eBACCa's with Containers.", description));
				}
			}
		}

		void ValidateGoodsLocation(ForwardingConsol consol)
		{
			CheckMandatoryField(consol.JK_OA_UnpackDepotAddress_ZAddress.GetCCPOrATFCode(), "Goods Location, (entered in the Consol > Arrival tab > CFS Address).\r\nThe organisation/address entered must have a valid CCP or ATF code configured");
		}

		void ValidateTreatmentProvider(IMAFOrganisation treatmentProvider)
		{
			if (treatmentProvider != null)
			{
				ValidateMAFOrganisation(treatmentProvider, "Treatment Provider");
			}
		}

		void ValidatePaymentDetails(IMAFMessagingMetaData ebaccaData)
		{
			if (ebaccaData.AlternativePaymentMethod.IsEmpty)
			{
				CheckMandatoryField(ebaccaData.AccountDetails.AccountHolderName, "MPI Account Holder if your Payment Method is 'Account'");
				CheckMandatoryField(ebaccaData.AccountDetails.AccountNumber, "MPI Account Number if your Payment Method is 'Account'");
			}
		}

		void ValidateMAFOrganisation(IMAFOrganisation organisationData, string description)
		{
			CheckMandatoryField(organisationData.OrganisationName, description + " Company Name");
			CheckMandatoryField(organisationData.AddressLine1, description + " Address Line");
			CheckMandatoryField(organisationData.City, description + " City");
			CheckMandatoryField(organisationData.Country, description + " Country/Region");
		}

		#endregion

		#region CheckMandatoryField

		void CheckMandatoryField(IZType value, ZString description)
		{
			if (value.IsEmpty)
			{
				FailBuildingReason(description);
			}
		}

		void CheckMandatoryField(IEnumerable<ZString> values, ZString description)
		{
			if (values == null || !values.Any())
			{
				FailBuildingReason(description);
			}
			else
			{
				foreach (var value in values)
				{
					CheckMandatoryField(value, description);
				}
			}
		}

		void CheckMandatoryField<T>(T? value, ZString description) where T : struct
		{
			if (!value.HasValue)
			{
				FailBuildingReason(description);
			}
		}

		void FailBuildingReason(ZString missingFieldDescription)
		{
			Fail("You must have a" + ("AEIOU".Contains(missingFieldDescription.Left(1).ToUpper()) ? "n " : " ") + missingFieldDescription + ".");
		}

		void Fail(string reason)
		{
			errors.Add(new Error(currentLine, reason));
		}

		#endregion

		#region Error

		class Error : IComparable<Error>
		{
			public Error(int lineNo, string message)
			{
				this.message = message;
				this.lineNo = lineNo;
			}

			int IComparable<Error>.CompareTo(Error other)
			{
				var result = lineNo.CompareTo(other.lineNo);
				if (result == 0)
				{
					result = message.CompareTo(other.message);
				}
				return result;
			}

			public string FormattedMessage
			{
				get { return (lineNo > 0 ? "Merged Line " + lineNo + " - " : "") + message; }
			}

			readonly int lineNo;
			readonly string message;
		}

		#endregion

		int currentLine;
		List<Error> errors;
		readonly IMAFMessagingRequest messagingRequest;

		#endregion
	}
}
