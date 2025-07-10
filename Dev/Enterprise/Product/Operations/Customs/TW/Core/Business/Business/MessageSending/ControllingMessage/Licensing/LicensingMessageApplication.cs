using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.Business;
using Enterprise.Customs.TW.Business.MessageSending;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageApplication : IApplication,
		IApplicationAdditionalInformation,
		IApplicationWine,
		IAppointment,
		IAuthorizedInformation
	{
		protected CusTWControllingMessageHeader Header { get; }
		JobDeclaration Declaration { get; }
		IAdditionalSupportingDocument AddtionalDoc { get; }

		public LicensingMessageApplication(CusTWControllingMessageHeader header, IAdditionalSupportingDocument addtionalDoc)
		{
			Header = Argument.NotNull(header, nameof(header));
			Declaration = header.Declaration;
			AddtionalDoc = addtionalDoc;
		}

		ZString IApplication.FunctionalReferenceID => null;

		ZString IApplication.ID => null;

		ZString IApplication.PurposeCode => Header.TW1_Purpose;

		ZString IApplication.TypeCode => Header.TW1_BusinessType;

		IEnumerable<IAdditionalDocument> IApplication.AdditionalDocuments => new AdditionalDocumentWrapper().GetDocuments(AddtionalDoc);

		IApplicationAdditionalInformation IApplication.AdditionalInformation => this;

		IPartyDetails IApplication.Agent => Declaration?.DeclarantAddress is OrgAddress declarant ? new LicensingMessageApplicationAgent(declarant) : null;

		ZString IApplication.BankAccount => Declaration?.JE_OtherBankAccount ?? ZString.Empty;

		ZString IApplication.ContactOffice => Header.TW1_ProcessingUnit;

		IPayment IApplication.Payment => new PaymentWrapper(Header.TW1_PaymentMethod, Header.BulkPaymentID);

		ZString IApplication.ResponsibleGovernmentAgency => null;

		IAppointment IApplication.Appointment => this;

		ZString IApplication.ApprovalAuthenticationInformation => Header.TW1_InspectionRegistrationNumber;

		IAuthorizedInformation IApplication.AuthorizedInformation => this;

		IPartyDetails IApplication.Declarer => GetApplicant();

		IEnumerable<ZInt> IApplication.ItemGroupReferenceSequenceNumerics => null;

		IEnumerable<ILabel> IApplication.Labels => Label.GetLabels(Header.ProductLabelRanges);

		IPartyDetails IApplication.LocalManufacturer => Header.LocalProcessorAddress is TWJobDocAddress localProcessorAddress ? new LicensingMessageApplicationLocalManufacturer(localProcessorAddress) : null;

		IApplicationWine IApplication.Wine => this;

		IPartyDetails IApplication.Applicant => GetApplicant();

		IPartyDetails GetApplicant() => applicant ??= new LicensingMessageApplicationApplicant(Header.ApplicantDocumentaryAddress, Declaration);
		LicensingMessageApplicationApplicant applicant;

		#region IAuthorizedInformation members
		const string AuthorizedTypeCode1 = "1";
		const string AuthorizedTypeCode2 = "2";
		const string AuthorizedTypeCode3 = "3";

		JobRequiredDocument PowerOfAttorneyCustomsDoc
		{
			get
			{
				if (powerOfAttorneyCustomsDoc == null)
				{
					powerOfAttorneyCustomsDoc = AuthorizedInformationHeader?.RequiredDocuments.Cast<JobRequiredDocument>().FirstOrDefault(x => x.EQ_ValidToDate >= ZDateTime.Today && x.EQ_DocType == Core.Constants.RefDocTypes.PowerOfAttorneyCustoms);
				}
				return powerOfAttorneyCustomsDoc;
			}
		}
		JobRequiredDocument powerOfAttorneyCustomsDoc;

		OrgHeader AuthorizedInformationHeader => Declaration.IsImport ? Header.Importer : Header.Supplier;

		ZString AuthorizedType => Declaration.Factory.GetValue(ref authorizedTypeCached, () =>
		{
			var authorizedInformationHeaderPK = AuthorizedInformationHeader?.PK;
			var declarantPK = Declaration.DeclarantAddress?.Header?.PK;
			ZString result;
			if (authorizedInformationHeaderPK.HasValue && authorizedInformationHeaderPK == declarantPK)
			{
				result = AuthorizedTypeCode3;
			}
			else
			{
				result = PowerOfAttorneyCustomsDoc != null ? AuthorizedTypeCode1 : AuthorizedTypeCode2;
			}
			return result;
		});
		CachedProperty<ZString> authorizedTypeCached;

		ZString IAuthorizedInformation.AuthorizedTypeCode => AuthorizedType;

		IAdditionalDocument IAuthorizedInformation.AdditionalDocument => AuthorizedType == AuthorizedTypeCode1 ? new AdditionalDocumentWrapper(PowerOfAttorneyCustomsDoc.EQ_DocNumber) : null;
		#endregion

		#region IAppointment members
		ZDateTime IAppointment.ReservationDate => Header.TW1_AppointmentDate;

		ZString IAppointment.ReservationPeriodCode => Header.TW1_AppointmentPeriod;
		#endregion

		#region IApplicationWine members
		IEnumerable<IAdditionalDocument> IApplicationWine.AdditionalDocuments => Header.EthanolPermitNumbers.Select(x => new AdditionalDocumentWrapper(x.CSI_ReferenceNumber, ZInt.Zero));

		ZString IApplicationWine.GovernmentProcedurePreviousCode => Header.TW1_PreWineInspectionStatus;

		IPreviousDocument IApplicationWine.PreviousDocument => new PreviousDocumentWrapper(Header.TW1_PrePermitNumber);
		#endregion

		#region IApplicationAdditionalInformation

		ZString IApplicationAdditionalInformation.StatementDescription => Header.TW1_RequestDescription;

		ZString IApplicationAdditionalInformation.DeductionSample => Header.TW1_SamplingReductionReason;

		ZString IApplicationAdditionalInformation.ElectronicReceipt => Header.TW1_ElectronicReceipt ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		ZString IApplicationAdditionalInformation.ProvedPaper => GetProvedPaperCore();

		protected virtual ZString GetProvedPaperCore() => Header.TW1_ProofOfPaper ? YesNoList.Codes.Yes : ZString.Empty;

		ZString IApplicationAdditionalInformation.ReturnSample => GetReturnSampleCore();

		protected virtual ZString GetReturnSampleCore() => Header.TW1_ApplyForSampleReturn ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		ZString IApplicationAdditionalInformation.AddressChineseLine => Header.TW1_SampleReturnAddress;

		ZString IApplicationAdditionalInformation.BulkApplicationID => Header.BulkApplicationID;

		ZString IApplicationAdditionalInformation.BulkPortCode => Header.TW1_PortOfBulkCommodity;

		#endregion
	}
}
