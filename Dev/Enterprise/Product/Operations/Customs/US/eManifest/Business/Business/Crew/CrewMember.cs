using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using GenRegCertAccredMaintList = Enterprise.MasterFiles.Business.GenRegCertAccredMaintList;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class CrewMember : CusInBondPerson, ICertificatesValidationProvider, ITopLevelBizOProviderForJobDocAddress
	{
		public CrewMember(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region CP_BH_Header

		[RelatedBusinessObject("Trip")]
		public override ZGuid CP_BH_Header
		{
			get { return base.CP_BH_Header; }
			set
			{
				base.CP_BH_Header = value;
				var trip = Trip;
				if (trip != null && USAddress.E2_OA_Address.IsEmpty && !trip.BH_OH_Carrier.IsEmpty && trip.Carrier != null && trip.Carrier.MainAddress != null)
				{
					USAddress.E2_OA_Address = trip.Carrier.MainAddress.PK;
				}
			}
		}

		public Trip Trip
		{
			get { return Factory.Load<Trip>(CP_BH_Header); }
		}

		#endregion

		#region CP_HasHazmatEndorsment

		public override ZBool CP_HasHazmatEndorsment
		{
			get { return CP_Type != CrewTypes.Codes.Passenger && (HasHazmatEndorsementNumber || base.CP_HasHazmatEndorsment); }
			set { base.CP_HasHazmatEndorsment = value; }
		}

		protected bool CP_HasHazmatEndorsment_ReadOnly
		{
			get { return CP_Type == CrewTypes.Codes.Passenger || HasHazmatEndorsementNumber; }
		}

		bool HasHazmatEndorsementNumber
		{
			get { return !string.IsNullOrEmpty(GetTravelDocumentNumber(TravelDocumentTypes.Codes.HazmatEndorsement)); }
		}

		#endregion

		#region CP_Type

		[List(nameof(Lookups) + "." + nameof(CrewMemberLookups.CrewTypes))]
		public override ZString CP_Type
		{
			get { return base.CP_Type; }
			set
			{
				var hasChanges = base.CP_Type != value;
				base.CP_Type = value;
				if (hasChanges && !IsCopying)
				{
					USAddress.SetReadOnlyIncludingChildren(CP_Type == CrewTypes.Codes.Passenger);
					Trip.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region USAddress

		public JobDocAddress USAddress
		{
			get
			{
				if (usAddress == null || usAddress.IsDeleted)
				{
					usAddress = JobDocAddress.GetOrCreateDocAddressFromParent(this, DocAddressType.Location);
					usAddress.SetReadOnlyIncludingChildren(CP_Type == CrewTypes.Codes.Passenger);
					var requirement = usAddress.OverrideRequirement = new JobDocAddressRequirement();
					requirement.ValidateOrganisationPK = Validation.ValidateUSAddressOrganisationPK;
					requirement.ValidateCountry = Validation.ValidateUSAddressCountry;
				}
				return usAddress;
			}
		}
		JobDocAddress usAddress;

		#endregion

		#region Travel Documents

		public ZString DriversLicense
		{
			get
			{
				return CP_Type == CrewTypes.Codes.Passenger ? string.Empty
						: GetTravelDocumentNumber(TravelDocumentTypes.Codes.CommercialDriversLicense)
						  ?? GetTravelDocumentNumber(TravelDocumentTypes.Codes.EnhancedDriversLicense)
						  ?? GetTravelDocumentNumber(TravelDocumentTypes.Codes.DrivingLicenseNational);
			}
		}

		public ZString HazmatEndorsement
		{
			get
			{
				return CP_Type == CrewTypes.Codes.Passenger ? string.Empty
						: GetTravelDocumentNumber(TravelDocumentTypes.Codes.HazmatEndorsement)
						  ?? (CP_HasHazmatEndorsment ? Yes : No);
			}
		}

		internal bool IsRegistered
		{
			get
			{
				var ids = new CrewACEIdTypes();
				return Certificates.Any(cert => ids.ContainsCode(cert.XZ_Type));
			}
		}

		string GetTravelDocumentNumber(string type)
		{
			return (from cert in Certificates
					where cert.XZ_Type == type
					select (string)cert.XZ_RefNumber).FirstOrDefault();
		}

		public const string Yes = "YES";
		public const string No = "NO";

		#endregion

		#endregion

		#region Overrides

		public override void Delete()
		{
			USAddress.Delete();
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (usAddress != null && CP_Type == CrewTypes.Codes.Passenger)
			{
				usAddress.E2_AddressOverride = false;
				usAddress.OrganisationPK = ZGuid.Empty;
			}
		}

		#region Implementation of ICertificatesProvider

		public override ICodeDescriptionPairList GetCertificateTypeList()
		{
			return new TravelDocumentTypes().GetAdditionalCertificateTypes();
		}

		public override ICodeDescriptionPairList GetActiveCertificateTypeList()
		{
			//Not a registry defined list
			return ((ICertificatesProvider)this).GetCertificateTypeList();
		}

		GenRegCertAccredMaintListValidation ICertificatesValidationProvider.GetValidation(GenRegCertAccredMaintList parent)
		{
			return new TravelDocumentsValidation(parent);
		}

		#endregion

		#region Validation

		public new CrewMemberValidation Validation
		{
			get { return (CrewMemberValidation)base.Validation; }
		}

		protected override CusInBondPersonValidation GetNewValidation()
		{
			return new CrewMemberValidation(this);
		}

		internal bool ValidateAllHasBeenRun { get; set; }

		#endregion

		#region Lookups

		public new CrewMemberLookups Lookups
		{
			get { return (CrewMemberLookups)base.Lookups; }
		}

		protected override CusInBondPersonLookups GetNewLookups()
		{
			return new CrewMemberLookups(this);
		}

		#endregion

		#endregion

		public BusinessObject GetTopBusinessObject()
		{
			var inBondHeader = Factory.Load<Trip>(CP_BH_Header);
			return Factory.Load(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(inBondHeader.BH_ParentTableCode), inBondHeader.BH_ParentID);
		}
	}
}
