namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.Customs.NZ.Business.MAFeBACCa;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Customs.NZ.Business.TariffValidation;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;

	public class NZDocsMAFCoverSheet : AutoNZDocsMAFCoverSheet
	{
		public NZDocsMAFCoverSheet(MAFMessagingBO mafMessaging)
			: this(mafMessaging.PlugInSupport.Master)
		{
			SetDefaults(mafMessaging);
		}

		public NZDocsMAFCoverSheet(BusinessObject parent)
			: base(parent.Factory)
		{
			Parent = parent;
			phoneNumberPropertyHelperThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers));
		}

		public ZString DefaultCountryCodeForPhoneNumbers { get; set; }

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper => phoneNumberPropertyHelperThunk.Value;

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperThunk;

		#region D0_AgentPhoneNumber_Formatted

		public ZString D0_AgentPhoneNumber_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(D0_AgentPhoneNumberInfo); }
		}

		public ZPropertyInfo D0_AgentPhoneNumber_FormattedInfo
		{
			get { return GetZPropertyInfo(nameof(D0_AgentPhoneNumber_Formatted)); }
		}

		#endregion

		#region D0_AgentFaxNumber_Formatted

		public ZString D0_AgentFaxNumber_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(D0_AgentFaxNumberInfo); }
		}

		public ZPropertyInfo D0_AgentFaxNumber_FormattedInfo
		{
			get { return GetZPropertyInfo(nameof(D0_AgentFaxNumber_Formatted)); }
		}

		#endregion

		#region Set Default Values

		void SetDefaults(IMAFMessagingRequest data)
		{
			var metaData = data.MetaData;
			var source = metaData.Source;

			SetValueAdjustingLength(D0_ToMAFQuarantineServiceInfo, GetOfficeDescriptionWithFax(metaData.ProcessingOffice, !source.FlightNumber.IsEmpty));
			SetValueAdjustingLength(D0_AgentCompanyNameInfo, GlbCompany.CurrentCompany.GC_Name);
			SetValueAdjustingLength(D0_AgentContactNameInfo, GlbStaff.CurrentUser.GS_FullName);
			SetValueAdjustingLength(D0_AgentEmailInfo, GlbStaff.CurrentUser.GS_EmailAddress);
			SetValueAdjustingLength(D0_AgentFaxNumberInfo, GlbStaff.CurrentUser.GS_FaxNum);
			SetValueAdjustingLength(D0_AgentPhoneNumberInfo, GlbStaff.CurrentUser.GS_WorkPhone);
			SetValueAdjustingLength(D0_ClientReferenceInfo, source.ClientReferenceCoverSheet);
			D0_DateOfArrival = source.VoyageArrivalDate.IsValid ? source.VoyageArrivalDate : source.FlightArrivalDate;
			D0_DateSigned = ZDateTime.Today;
			SetValueAdjustingLength(D0_EntryNumberInfo, source.CustomsEntryNumber.ToString("#"));

			var exporter = metaData.Exporter;
			if (exporter != null)
			{
				SetValueAdjustingLength(D0_ExporterNameInfo, exporter.OrganisationName);
			}

			var importer = metaData.Importer;
			if (importer != null)
			{
				SetValueAdjustingLength(D0_ImporterNameInfo, importer.OrganisationName);
			}

			SetValueAdjustingLength(D0_ShippingOrAirLineInfo, source.ShippingCompany);
			SetValueAdjustingLength(D0_VesselInfo, source.ShipName);
			SetValueAdjustingLength(D0_VoyageOrFlightInfo, source.VoyageNumber.IsEmpty ? source.FlightNumber : source.VoyageNumber);
			SetCommaSeperatedValues(D0_HouseBillInfo, source.SubBillOfLadingNumbers);
			SetCommaSeperatedValues(D0_MasterBillInfo, source.BillOfLadingNumbers);
			SetValueAdjustingLength(D0_RN_NKCountryOfOriginInfo, source.OriginCountry);
			SetCommaSeperatedValues(D0_RL_NKPortOfDischargeInfo, source.DischargePorts);
			SetCommaSeperatedValues(D0_RL_NKDestinationInfo, source.Destinations);

			SetSuppliedDocumentation(data);

			D0_TransitionalFacility = GetFormattedAddress(source.TransitionalFacility);
			D0_TreatmentSupplier = GetFormattedAddress(source.TreatmentProvider);

			SetPaymentDetails(metaData);

			D0_SignatoryCompanyName = D0_AgentCompanyName;
			D0_SignatoryName = D0_AgentContactName;
			D0_SignatoryPhoneNumber = D0_AgentPhoneNumber;
			D0_TotalPages = D0_TotalPages == 0 ? new ZInt(1) : D0_TotalPages;

			foreach (var container in source.Containers)
			{
				var csContainer = Containers.AddNew();
				csContainer.D2_ContainerNumber = container.ContainerNumber;
				csContainer.D2_IsFCL = container.IsFullContainer;
				csContainer.D2_IsLCL = !container.IsFullContainer;
			}

			var tariffFormatter = new NZTariffFormatter();
			var tariffCodesRequiringPermits = new List<ZString>();
			foreach (var commodity in source.Commodities.Where(c => c.RequirePermits))
			{
				var csCommodity = Commodities.AddNew();
				SetDescriptionAdjustingLength(csCommodity.D1_CommodityOrSpeciesInfo, commodity.GoodsDescription);
				SetQuantityWithUnit(csCommodity.D1_QuantityWithUnitInfo, commodity.Quantity);
				SetQuantityWithUnit(csCommodity.D1_MeasureWithUnitInfo, commodity.Measure);
				tariffCodesRequiringPermits.AddRange(commodity.TariffCodes);
			}

			var formattedTarrifs = from tariff in tariffCodesRequiringPermits.Distinct()
								   select tariffFormatter.Format(tariff);

			SetCommaSeperatedValues(D0_EDITariffCodesInfo, formattedTarrifs);
		}

		ZString GetOfficeDescriptionWithFax(ZString code, bool isAir)
		{
			if (code == MAFProcessingOfficeList.Codes.Auckland)
			{
				return isAir ? MAFOfficesList_DescriptionsOnly.Codes.AucklandAirCargo : MAFOfficesList_DescriptionsOnly.Codes.Auckland;
			}
			return Lookups.MAFOfficesList.GetCodeFromDescription(Constants.CountryCodes.NewZealand + code);
		}

		void SetSuppliedDocumentation(IMAFMessagingRequest data)
		{
			var docList = new DocumentTypeList();
			var otherDocuments = new List<ZString>();
			foreach (var file in data.Files)
			{
				if (file.DocumentType == DocumentTypeList.Codes.BillofLading)
				{
					D0_SuppliedBillOfLading = true;
				}
				else if (file.DocumentType == DocumentTypeList.Codes.QuarantineDeclaration)
				{
					D0_SuppliedQuarantineDeclaration = true;
				}
				else if (file.DocumentType == DocumentTypeList.Codes.Other)
				{
					otherDocuments.Add(Path.GetFileNameWithoutExtension(file.FileName));
				}
				else if (DocumentTypeList.IsRelevantInvoice(file.DocumentType))
				{
					D0_SuppliedRelevantInvoices = true;
				}
				else if (DocumentTypeList.IsImportPermit(file.DocumentType))
				{
					D0_SuppliedImportPermit = true;
				}
				else if (DocumentTypeList.IsCertificate(file.DocumentType))
				{
					D0_SuppliedCertificates = true;
				}
				else
				{
					otherDocuments.Add(docList.GetDescriptionFromCode(file.DocumentType));
				}
			}

			SetCommaSeperatedValues(D0_SuppliedOtherDocumentationInfo, otherDocuments);
		}

		void SetPaymentDetails(IMAFMessagingMetaData metaData)
		{
			D0_PayBeCash = metaData.AlternativePaymentMethod == MAFPaymentMethodList.Codes.Cash;
			D0_PayBeCheque = false;
			D0_PayByAccount = !D0_PayBeCash;

			if (D0_PayByAccount)
			{
				var details = metaData.AccountDetails;
				if (!details.AccountNumber.IsEmpty)
				{
					SetValueAdjustingLength(D0_QENumberInfo, details.AccountNumber);
					SetValueAdjustingLength(D0_AccountHolderInfo, details.AccountHolderName);
				}
				else
				{
					SetValueAdjustingLength(D0_AccountHolderInfo, GlbCompany.CurrentCompany.GC_Name);
				}
			}
		}

		#region Implementation

		ZString GetFormattedAddress(IMAFOrganisation address)
		{
			var result = ZString.Empty;
			if (address != null)
			{
				var formatter = new AddressFormatter(Factory, address.OrganisationName, address.AddressLine1, address.AddressLine2,
													 address.City, string.Empty, address.PostalCode, (NoResString)address.Country, false);
				result = formatter.PostalAddress().Replace("\n", ", ");
			}
			return result;
		}

		static void SetDescriptionAdjustingLength(ZPropertyInfo destinationInfo, ZString description)
		{
			if (description.Length > destinationInfo.MaxLength)
			{
				description = description.Left(destinationInfo.MaxLength - 3) + "...";
			}
			destinationInfo.Value = description;
		}

		static void SetQuantityWithUnit(ZPropertyInfo destinationInfo, IMAFMeasurement quantity)
		{
			SetValueAdjustingLength(destinationInfo, quantity.MeasurementValue.ToStringTrimZeros() + " " + quantity.MeasurementUQ);
		}

		static void SetValueAdjustingLength(ZPropertyInfo destinationInfo, ZString value)
		{
			destinationInfo.Value = value.Left(destinationInfo.MaxLength);
		}

		static void SetCommaSeperatedValues(ZPropertyInfo destinationInfo, IEnumerable<ZString> values)
		{
			ZString result = new ZStringBuilder(values).ToStringWithDelimiterBetweenAppends(", ");
			if (result.Length > destinationInfo.MaxLength)
			{
				result = "MULTIPLE";
			}
			destinationInfo.Value = result.Left(destinationInfo.MaxLength);
		}

		#endregion

		#endregion

		public NZDocsMAFCSCommodityCollection Commodities
		{
			get { return fCommodities ?? (fCommodities = new NZDocsMAFCSCommodityCollection(Factory)); }
		}
		NZDocsMAFCSCommodityCollection fCommodities;

		public NZDocsMAFCSContainerCollection Containers
		{
			get { return fContainers ?? (fContainers = new NZDocsMAFCSContainerCollection(Factory)); }
		}
		NZDocsMAFCSContainerCollection fContainers;

		public BusinessObject Parent { get; private set; }
	}
}
