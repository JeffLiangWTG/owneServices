using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Common;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationHRJobApplicant : IHRJobApplicant, IDeduplicationGlowObject, IPerson, IRawNameProvider
	{
		[Newtonsoft.Json.JsonConstructor]
		DeduplicationHRJobApplicant() { }

		public DeduplicationHRJobApplicant(Integration.Recruiter.IHRJobApplicant applicant, DeduplicationGlbPerson parentGlowGlbPerson)
		{
			HA_PK = applicant.PK.ToGuid();

			HA_FullName = applicant.HA_FullName;
			HA_Birthdate = applicant.HA_Birthdate.IsValid ? applicant.HA_Birthdate.ToDateTime() : default(DateTime?);
			HA_HomePhone = applicant.HA_HomePhone;
			HA_WorkPhone = applicant.HA_WorkPhone;
			HA_MobilePhone = applicant.HA_MobilePhone;
			HA_FaxNum = applicant.HA_FaxNum;
			HA_EmailAddress = applicant.HA_EmailAddress;
			HA_UserAddress1 = applicant.HA_UserAddress1;
			HA_UserAddress2 = applicant.HA_UserAddress2;
			HA_City = applicant.HA_City;
			HA_State = applicant.HA_State;
			HA_Postcode = applicant.HA_Postcode;
			HA_PER = applicant.HA_PER.IsValid ? applicant.HA_PER.ToGuid() : Guid.Empty;
			IsInDatabase = applicant.IsInDatabase;
			GlbPerson = parentGlowGlbPerson;
			RawName = applicant.HA_FullName;

			try
			{
				CertificateOrAccreditations = applicant.CertificatesBizoCollection
				.Cast<GenRegCertAccredMaintList>()
				.Where(certificate => certificate != null)
				.Select(certificate => new DeduplicationCertifcateOrAccreditation<IHRJobApplicant>(certificate))
				.ToArray();
			}
			catch (InvalidOperationException ex)
			{
				var devMessage = Res.GetString("75d9c8bb-2421-4ed2-8f2d-9eedc0152fd2", "{0} job applicant: {1}", ex.Message, applicant.PK);
				ExceptionReporter.Instance.ReportDeveloperException("8cf067df-bf13-490a-a959-58cd00ee654d", devMessage, ex);
			}
		}

		public Guid PK => HA_PK;

		public string TablePrefix => HRJobApplicantSchema.Constants.Prefix;

		public bool IsInDatabase { get; }

		public Type BizoType => typeof(Integration.Recruiter.IHRJobApplicant);

		public Type BizoTyp2 = typeof(IHRJobApplicant);

		public string CountryCode => HA_RN_NKCountry;

		public Guid HA_PK { get; set; }

		public string HA_Availability { get; set; }
		public DateTime? HA_Birthdate { get; set; }
		public string HA_City { get; set; }
		public string HA_CurrentStatus { get; set; }
		public decimal HA_CurrentWage { get; set; }
		public string HA_DriversLicenseNumber { get; set; }
		public string HA_EmailAddress { get; set; }
		public string HA_FaxNum { get; set; }
		public string HA_FullName { get; set; }
		public string HA_Gender { get; set; }
		public string HA_GeoLocation { get; set; }
		public string HA_HomePhone { get; set; }
		public string HA_MobilePhone { get; set; }
		public string HA_NameSuffix { get; set; }
		public string HA_OtherIdentityDocument { get; set; }
		public string HA_Passport { get; set; }
		public Guid HA_PER { get; set; }
		public string HA_Postcode { get; set; }
		public string HA_RN_NKCountry { get; set; }
		public string HA_RN_NKNationalityCodeISO { get; set; }
		public string HA_RX_NKCurrentWageCurrency { get; set; }
		public string HA_RX_NKWageExpectationCurrency { get; set; }
		public string HA_State { get; set; }
		public DateTime? HA_SystemCreateTimeUtc { get; set; }
		public string HA_SystemCreateUser { get; set; }
		public DateTime? HA_SystemLastEditTimeUtc { get; set; }
		public string HA_SystemLastEditUser { get; set; }
		public string HA_Title { get; set; }
		public string HA_UserAddress1 { get; set; }
		public string HA_UserAddress2 { get; set; }
		public string HA_ValidationStatus { get; set; }
		public decimal HA_WageExpectation { get; set; }
		public string HA_WorkExtension { get; set; }
		public string HA_WorkPermitStatus { get; set; }
		public string HA_WorkPhone { get; set; }
		public IRefCountryInfo Country { get; set; }
		public IRefCountryInfo NationalityCodeISO { get; set; }
		public IRefCurrencyInfo CurrentWageCurrency { get; set; }
		public IRefCurrencyInfo WageExpectationCurrency { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }
		public IGlbPerson GlbPerson { get; set; }

		public ICollection<IConversationParticipant<IHRJobApplicant>> ConversationParticipants { get; }

		public ICollection<ICertificateOrAccreditation<IHRJobApplicant>> CertificateOrAccreditations { get; }

		public ICollection<ILog<IHRJobApplicant>> Logs { get; }

		public ICollection<IAcknowledgement<IHRJobApplicant>> Acknowledgements { get; }

		public ICollection<IHRJobApplication> HRJobApplications { get; }

		public void UpdateEmailAddress(ZString newEmailAddress)
		{
			HA_EmailAddress = newEmailAddress;
		}

		public (string ColName, string Value) AddressFull
		{
			get
			{
				var result = new StringBuilder();

				if (!string.IsNullOrEmpty(HA_UserAddress1))
				{
					result.Append(HA_UserAddress1);
				}

				if (!string.IsNullOrEmpty(HA_UserAddress2))
				{
					result.Append(HA_UserAddress2);
				}

				if (!string.IsNullOrEmpty(HA_City))
				{
					result.Append(" " + HA_City);
				}

				if (!string.IsNullOrEmpty(HA_State))
				{
					result.Append(" " + HA_State);
				}

				if (!string.IsNullOrEmpty(HA_Postcode))
				{
					result.Append(" " + HA_Postcode);
				}

				if (!string.IsNullOrEmpty(CountryCode))
				{
					result.Append(" " + CountryCode);
				}

				return ("AddressFull", result.ToString().Trim());
			}
		}

		public IList<(string ColName, string Value)> Address => new List<(string ColName, string Value)> {
			(nameof(HA_UserAddress1), HA_UserAddress1),
			(nameof(HA_UserAddress2), HA_UserAddress2)
		};

		public (string ColName, DateTime? Value) BirthDate => (nameof(HA_Birthdate), HA_Birthdate);
		public IList<(string ColName, string Value)> EmailAddresses => new List<(string ColName, string Value)>
		{
			(nameof(HA_EmailAddress), HA_EmailAddress)
		};
		public (string ColName, string Value) FullName => (nameof(HA_FullName), HA_FullName);
		public IList<(string ColName, string Value)> PhoneNumbers => new List<(string ColName, string Value)>
		{
			(nameof(HA_FaxNum), HA_FaxNum),
			(nameof(HA_HomePhone), HA_HomePhone),
			(nameof(HA_MobilePhone), HA_MobilePhone),
			(nameof(HA_WorkPhone), HA_WorkPhone)
		};
		public Type GlowType => typeof(IHRJobApplicant);

		public string RawName { get; }

		public ICollection<ITalActivityResult> TalActivityResults { get; }

		public ICollection<ITalEducationHistory> TalEducationHistories { get; }

		public ICollection<ITalExtracurricular> TalExtracurriculars { get; }

		public ICollection<ITalWorkHistory> TalWorkHistories { get; }

		public ICollection<ITalWorkReference> TalWorkReferences { get; }
	}
}
