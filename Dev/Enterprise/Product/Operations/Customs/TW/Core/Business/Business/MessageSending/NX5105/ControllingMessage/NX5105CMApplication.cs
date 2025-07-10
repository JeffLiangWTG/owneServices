using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageSending;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CMApplication : IApplication
	{
		public NX5105CMApplication(CusTWControllingMessageHeader header, IEnumerable<CusEntryLine> entryLines)
		{
			this.header = Argument.NotNull(header, nameof(header));
			declaration = Argument.NotNull(header?.EntryInstruction?.JobDeclaration, nameof(header.EntryInstruction.JobDeclaration));
			this.entryLines = Argument.NotNull(entryLines, nameof(entryLines));
		}

		#region IApplication
		ZString IApplication.BankAccount => declaration.JE_OtherBankAccount;

		ZString IApplication.ContactOffice => header.TW1_ProcessingUnit;

		IPayment IApplication.Payment => new PaymentWrapper(header.TW1_PaymentMethod);

		ZString IApplication.ResponsibleGovernmentAgency => header.TW1_ControllingAgency;

		#region IAppointment
		IAppointment IApplication.Appointment => new Appointment(header.TW1_AppointmentDate, header.TW1_AppointmentPeriod);

		internal class Appointment : IAppointment
		{
			public Appointment(ZDateTime reservationDate, ZString reservationPeriodCode)
			{
				ReservationDate = reservationDate;
				ReservationPeriodCode = reservationPeriodCode;
			}

			public ZDateTime ReservationDate { get; }

			public ZString ReservationPeriodCode { get; }
		}
		#endregion

		ZString IApplication.ApprovalAuthenticationInformation => header.TW1_InspectionRegistrationNumber;

		#region IAuthorizedInformation
		IAuthorizedInformation IApplication.AuthorizedInformation => GetAuthorizedInformation();

		IAuthorizedInformation GetAuthorizedInformation()
		{
			AuthorizedInformation result = null;
			if (declaration.Importer?.RequiredDocuments.Any() ?? false)
			{
				var doc = declaration.Importer.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.CountryCodes.Taiwan);
				result = new AuthorizedInformation(doc == null ? "2" : "1", doc?.EQ_DocNumber ?? ZString.Empty);
			}

			return result;
		}

		internal class AuthorizedInformation : IAuthorizedInformation
		{
			public AuthorizedInformation(ZString typeCode, ZString id)
			{
				AuthorizedTypeCode = typeCode;
				if (!id.IsEmpty)
				{
					AdditionalDocument = new AdditionalDocumentWrapper(id);
				}
			}

			public ZString AuthorizedTypeCode { get; }

			public IAdditionalDocument AdditionalDocument { get; }
		}

		#endregion

		IPartyDetails IApplication.Declarer => declaration.Importer == null ? null : new NX5105CMApplicationDeclarerWrapper(declaration.Importer.MainAddress);

		IEnumerable<ZInt> IApplication.ItemGroupReferenceSequenceNumerics => entryLines.Select(x => x.CL_LineNumber.ToZInt());

		#region Labels
		IEnumerable<ILabel> IApplication.Labels => Label.GetLabels(header.ProductLabelRanges);

		IPartyDetails IApplication.LocalManufacturer => header.LocalProcessorAddress?.Address == null ? null : new NX5105CMApplicationLocalManufacturerWrapper(header.LocalProcessorAddress);
		#endregion

		IApplicationWine IApplication.Wine => new NX5105CMApplicationWine(header);

		ZString IApplication.FunctionalReferenceID => SharedHelper.GetFunctionalReferenceIDPlaceHolderWithPK(header.PK);

		ZString IApplication.ID => header.PermitNumber;

		ZString IApplication.PurposeCode => header.TW1_Purpose;

		ZString IApplication.TypeCode => header.TW1_BusinessType;

		IEnumerable<IAdditionalDocument> IApplication.AdditionalDocuments => null;

		IApplicationAdditionalInformation IApplication.AdditionalInformation => new NX5105CMApplicationAdditionalInformation(header);

		IPartyDetails IApplication.Applicant => null;

		IPartyDetails IApplication.Agent => declaration.DeclarantAddress == null ? null : new NX5105CMApplicationAgentWrapper(declaration.DeclarantAddress);

		#endregion

		readonly CusTWControllingMessageHeader header;

		readonly JobDeclaration declaration;

		readonly IEnumerable<CusEntryLine> entryLines;
	}
}
