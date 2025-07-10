using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationGlbStaff : IGlbStaff, IDeduplicationGlowObject, IPerson, IRawNameProvider
	{
		[Newtonsoft.Json.JsonConstructor]
		DeduplicationGlbStaff() { }

		public DeduplicationGlbStaff(GlbStaff staff, DeduplicationGlbPerson parentGlowGlbPerson)
		{
			GS_PK = staff.PK.ToGuid();
			if (!staff.GS_GB_HomeBranch.IsEmpty)
			{
				GS_GB_HomeBranch = staff.GS_GB_HomeBranch.ToGuid();
			}

			GS_Code = staff.GS_Code;
			GS_IsActive = staff.GS_IsActive;
			GS_FullName = staff.GS_FullNameInternal;
			GS_FriendlyName = staff.GS_FriendlyName;
			GS_FullNameInMotherLanguage = staff.GS_FullNameInMotherLanguage;
			GS_GivenName = staff.GS_GivenName;
			GS_MiddleName = staff.GS_MiddleName;
			GS_PreferredSurname = staff.GS_PreferredSurname;
			GS_Surname = staff.GS_Surname;
			GS_Birthdate = staff.GS_BirthdateInternal.IsValid ? staff.GS_BirthdateInternal.ToDateTime() : default(DateTime?);
			GS_HomePhone = staff.GS_HomePhoneInternal;
			GS_WorkPhone = staff.GS_WorkPhone;
			GS_MobilePhone = staff.GS_MobilePhoneInternal;
			GS_Pager = staff.GS_Pager;
			GS_FaxNum = staff.GS_FaxNumInternal;
			GS_EmailAddress = staff.GS_EmailAddressInternal;
			GS_UserAddress1 = staff.GS_UserAddress1Internal;
			GS_UserAddress2 = staff.GS_UserAddress2Internal;
			GS_City = staff.GS_CityInternal;
			GS_Postcode = staff.GS_PostcodeInternal;
			GS_State = staff.GS_StateInternal;
			GS_GeoLocation = staff.GS_GeoLocation.ToString();
			GS_PER = staff.GS_PER.IsValid ? staff.GS_PER.ToGuid() : Guid.Empty;
			GS_RN_NKCountryCode = staff.GS_RN_NKCountryCodeInternal;
			IsInDatabase = staff.IsInDatabase;
			GlbPerson = parentGlowGlbPerson;
			RawName = staff.GS_FullNameInternal;

			try
			{
				CertificateOrAccreditations = staff.Certificates
				.Cast<GenRegCertAccredMaintList>()
				.Where(certificate => certificate != null)
				.Select(certificate => new DeduplicationCertifcateOrAccreditation<IGlbStaff>(certificate))
				.ToArray();
			}
			catch (InvalidOperationException ex)
			{
				var devMessage = Res.GetString("5a4beed3-3c10-4cd4-87e2-3bcf2e2619d7", "{0} staff: {1}", ex.Message, staff.PK);
				ExceptionReporter.Instance.ReportDeveloperException("f33bb83a-d278-475e-acc7-d53e28059a5c", devMessage, ex);
			}
		}

		public Guid PK => GS_PK;

		public string TablePrefix => GlbStaffSchema.Constants.Prefix;

		public bool IsInDatabase { get; }

		public Type BizoType => typeof(GlbStaff);

		public string CountryCode => GS_RN_NKCountryCode;

		public Guid GS_PK { get; set; }

		public Guid? GS_ActiveDirectoryObjectGuid { get; set; }
		public bool GS_SavePersonalDataToActiveDirectory { get; set; }
		public string GS_ActivityTrackingStatus { get; set; }
		public string GS_AddressMap { get; set; }
		public DateTime? GS_Birthdate { get; set; }
		public string GS_BrokerID { get; set; }
		public string GS_BrokerPassword { get; set; }
		public string GS_BrokerPasswordStatus { get; set; }
		public string GS_BrokerWorkingPassword { get; set; }
		public bool GS_CanLogin { get; set; }
		public bool GS_ChangePasswordAtNextLogin { get; set; }
		public string GS_City { get; set; }
		public string GS_Code { get; set; }
		public string GS_CommissionBasis { get; set; }
		public DateTime? GS_DepartureDate { get; set; }
		public string GS_DomainName { get; set; }
		public DateTime? GS_DueBack { get; set; }
		public bool GS_EftWages { get; set; }
		public string GS_EmailAddress { get; set; }
		public string GS_EmergencyContactName { get; set; }
		public string GS_EmergencyContactRelationship { get; set; }
		public string GS_EmergencyHomePhone { get; set; }
		public string GS_EmergencyWorkPhone { get; set; }
		public string GS_EmploymentBasis { get; set; }
		public DateTime? GS_EmploymentDate { get; set; }
		public string GS_EnterpriseCertificationID { get; set; }
		public string GS_FaxNum { get; set; }
		public string GS_FriendlyName { get; set; }
		public string GS_FullNameInMotherLanguage { get; set; }
		public string GS_GivenName { get; set; }
		public string GS_MiddleName { get; set; }
		public string GS_PreferredSurname { get; set; }
		public string GS_Surname { get; set; }
		public string GS_FullName { get; set; }
		public Guid? GS_GB_HomeBranch { get; set; }
		public Guid? GS_GB_LastLogonBranch { get; set; }
		public Guid? GS_GC_PreferredPaymentCompany { get; set; }
		public Guid? GS_GE_HomeDepartment { get; set; }
		public Guid? GS_GE_LastLogonDepartment { get; set; }
		public string GS_Gender { get; set; }
		public string GS_GenderCustomTerm { get; set; }
		public string GS_GeoLocation { get; set; }
		public string GS_HomePhone { get; set; }
		public bool GS_IsActive { get; set; }
		public bool GS_IsActivityLogged { get; set; }
		public bool GS_IsController { get; set; }
		public bool GS_IsDeveloper { get; set; }
		public bool GS_IsDevice { get; set; }
		public bool GS_IsInTrainingMode { get; set; }
		public bool GS_IsOperational { get; set; }
		public bool GS_IsResource { get; set; }
		public bool GS_IsSalesRep { get; set; }
		public bool GS_IsSystemAccount { get; set; }
		public bool GS_IsTwoFactorAuthenticationEnabled { get; set; }
		public bool GS_IsValid { get; set; }
		public DateTime? GS_LastActivityDate { get; set; }
		public DateTime? GS_LastPasswordAttemptDateTime_Utc { get; set; }
		public DateTime? GS_LastPasswordChangeDate { get; set; }
		public string GS_LoginName { get; set; }
		public string GS_MobilePhone { get; set; }
		public string GS_NameSuffix { get; set; }
		public string GS_NameTitle { get; set; }
		public string GS_NextOfKin { get; set; }
		public string GS_NextOfKinHomePhone { get; set; }
		public string GS_NextOfKinRelationship { get; set; }
		public string GS_NextOfKinWorkPhone { get; set; }
		public DateTime? GS_NextReviewDate { get; set; }
		public string GS_OutOnTask { get; set; }
		public string GS_Pager { get; set; }
		public string GS_Passport { get; set; }
		public bool GS_PasswordNeverChanges { get; set; }
		public Guid? GS_PER { get; set; }
		public string GS_PersonalEDIMailBox { get; set; }
		public string GS_Postcode { get; set; }
		public bool GS_PublishEmailAddress { get; set; }
		public bool GS_PublishFaxNum { get; set; }
		public bool GS_PublishHomePhone { get; set; }
		public bool GS_PublishMobilePhone { get; set; }
		public bool GS_PublishWorkExtension { get; set; }
		public bool GS_PublishWorkPhone { get; set; }
		public string GS_ResourceType { get; set; }
		public string GS_RN_NKCountryCode { get; set; }
		public string GS_RN_NKNationalityCode { get; set; }
		public string GS_SecurityCardNumber { get; set; }
		public string GS_State { get; set; }
		public DateTime? GS_SystemCreateTimeUtc { get; set; }
		public string GS_SystemCreateUser { get; set; }
		public string GS_SystemCreateBranch { get; set; }
		public string GS_SystemCreateDepartment { get; set; }
		public DateTime? GS_SystemLastEditTimeUtc { get; set; }
		public string GS_SystemLastEditUser { get; set; }
		public string GS_Title { get; set; }
		public string GS_UserAddress1 { get; set; }
		public string GS_UserAddress2 { get; set; }
		public string GS_ValidationStatus { get; set; }
		public string GS_WagesBankAccount { get; set; }
		public string GS_WagesBankBsb { get; set; }
		public string GS_WagesBankName { get; set; }
		public string GS_WagesBankSwift { get; set; }
		public string GS_WorkExtension { get; set; }
		public string GS_WorkingLanguage { get; set; }
		public string GS_WorkPhone { get; set; }
		public bool GS_IsDriver { get; set; }
		public string GS_EmergencyContactEmail { get; set; }
		public DateTime? GS_LastDayOfWork { get; set; }
		public string GS_NextOfKinEmail { get; set; }
		public DateTime? GS_ProbationEndDate { get; set; }
		public DateTime? GS_ResidencyExpiry { get; set; }
		public string GS_ResidencyStatus { get; set; }
		public bool GS_IsRobot { get; set; }
		public string GS_ExternalId { get; set; }
		public IRefCountryInfo Country { get; set; }
		public IRefCountryInfo Nationality { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }
		public IGlbBranch HomeBranch { get; set; }
		public IGlbBranch LastLogonBranch { get; set; }
		public IGlbCompany PreferredPaymentCompany { get; set; }
		public IGlbDepartment HomeDepartment { get; set; }
		public IGlbDepartment LastLogonDepartment { get; set; }
		public IGlbPerson GlbPerson { get; set; }

		public ICollection<IGlbGroupLink> GlbGroupLinks { get; }

		public ICollection<IGlbResourceCapabilityPivot> GlbResourceCapabilityPivots { get; }

		public ICollection<IGlbSecurity> GlbSecurities { get; }

		public ICollection<IGlbStaffHoliday> GlbStaffHolidays { get; }

		public ICollection<IGlbStaffRemuneration> GlbStaffRemunerations { get; }

		public ICollection<IConversationParticipant<IGlbStaff>> ConversationParticipants { get; }

		public ICollection<IGlbDeviceAssignmentDivot<IGlbStaff>> GlbDeviceAssignmentDivots { get; }

		public ICollection<ICertificateOrAccreditation<IGlbStaff>> CertificateOrAccreditations { get; }

		public ICollection<ILog<IGlbStaff>> Logs { get; }

		public ICollection<IAcknowledgement<IGlbStaff>> Acknowledgements { get; }

		public ICollection<IAddOnValue<IGlbStaff>> AddOnValues { get; }

		public ICollection<IGlbBeneficiaryBranchDepartment> GlbBeneficiaryBranchDepartments { get; }

		public ICollection<IGlbEmploymentHistory> GlbEmploymentHistories { get; }

		public ICollection<IGlbEmploymentLocation> GlbEmploymentLocations { get; }

		public ICollection<IGlbEmploymentTeam> GlbEmploymentTeams { get; }

		public ICollection<IGlbEmployingBranchDepartment> GlbEmployingBranchDepartments { get; }

		public ICollection<IGlbStaffClassification> GlbStaffClassifications { get; }

		public ICollection<IGlbStaffCostCentre> GlbStaffCostCentres { get; }

		public ICollection<IGlbStaffManager> DirectReports { get; }

		public ICollection<IGlbStaffManager> Managers { get; }

		public ICollection<IGlbStaffReview> GlbStaffReviews { get; }

		public ICollection<IGlbStaffWorkingBasis> GlbStaffWorkingBases { get; }

		public ICollection<IGlbWorkPattern> GlbWorkPatterns { get; }

		public ICollection<IHRMRemHistory> HRMRemHistories { get; }

		public ICollection<IHRMStaffHistory> HRMStaffHistories { get; }

		public ICollection<IReviewAllocation> ReviewAllocations { get; }

		public ICollection<IWorkTime<IGlbStaff>> WorkTimes { get; }

		public (string ColName, string Value) AddressFull
		{
			get
			{
				var result = new StringBuilder();

				if (!string.IsNullOrEmpty(GS_UserAddress1))
				{
					result.Append(GS_UserAddress1);
				}

				if (!string.IsNullOrEmpty(GS_UserAddress2))
				{
					result.Append(GS_UserAddress2);
				}

				if (!string.IsNullOrEmpty(GS_City))
				{
					result.Append(" " + GS_City);
				}

				if (!string.IsNullOrEmpty(GS_State))
				{
					result.Append(" " + GS_State);
				}

				if (!string.IsNullOrEmpty(GS_Postcode))
				{
					result.Append(" " + GS_Postcode);
				}

				if (!string.IsNullOrEmpty(CountryCode))
				{
					result.Append(" " + CountryCode);
				}

				return ("AddressFull", result.ToString().Trim());
			}
		}

		public IList<(string ColName, string Value)> Address => new List<(string ColName, string Value)> {
			(nameof(GS_UserAddress1), GS_UserAddress1),
			(nameof(GS_UserAddress2), GS_UserAddress2)
		};

		public (string ColName, DateTime? Value) BirthDate => (nameof(GS_Birthdate), GS_Birthdate);
		public IList<(string ColName, string Value)> EmailAddresses => new List<(string ColName, string Value)>
		{
			(nameof(GS_EmailAddress), GS_EmailAddress)
		};
		public (string ColName, string Value) FullName => (nameof(GS_FullName), GS_FullName);
		public IList<(string ColName, string Value)> PhoneNumbers => new List<(string ColName, string Value)>
		{
			(nameof(GS_FaxNum), GS_FaxNum),
			(nameof(GS_HomePhone), GS_HomePhone),
			(nameof(GS_MobilePhone), GS_MobilePhone),
			(nameof(GS_Pager), GS_Pager),
			(nameof(GS_WorkPhone), GS_WorkPhone)
		};
		public Type GlowType => typeof(IGlbStaff);

		public string RawName { get; }

		public bool IsSelf { get; set; }
		public bool IsManaged1 { get; set; }
		public bool IsManaged2 { get; set; }
		public bool IsManaged3 { get; set; }
		public bool IsNew { get; set; }

		public ICollection<IGlbPasswordHistory<IGlbStaff>> GlbPasswordHistories { get; }

		public ICollection<IGlbStaffTimezone> GlbStaffTimezones { get; }

		public ICollection<IWorkflowAuditLog<IGlbStaff>> WorkflowAuditLogs { get; }

		public ICollection<IGlbHolidaySourceHistory> GlbHolidaySourceHistories { get; }

		public ICollection<IHrlStaffPolicy> HrlStaffPolicies { get; }

		public ICollection<IHrlBalanceTransaction> HrlBalanceTransactions { get; }

		public ICollection<IReviewProcessEndpoint> ReviewProcessEndpoints { get; }

		public ICollection<IGlbStaffOneOffEntitlement> GlbStaffOneOffEntitlements { get; }

		public ICollection<IReviewProcessNode> ReviewProcessNodes { get; }

		public ICollection<IIndirectManager> IndirectManagers { get; }

		public ICollection<IHrlBenefitOnDemand> HrlBenefitOnDemands { get; }

		public ICollection<IProcessHeader<IGlbStaff>> ProcessHeaders => Array.Empty<IProcessHeader<IGlbStaff>>();

		public ICollection<IHrlConsolidatedBalanceTransactionView> HrlConsolidatedBalanceTransactionViews { get; }
	}
}
