using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class Party : JobDocAddress
	{
		public Party(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			OverrideRequirement = new JobDocAddressRequirement();
			Requirement.GetRegistrationNumberResult = GetPartyIdResult;
		}

		#region Load

		internal static Party LoadOrCreate(BusinessObject parent, string partyType)
		{
			var filter = new ZQuery(JobDocAddressSchema.E2_ParentID, parent.PK);
			filter.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, parent.TablePrefix);
			filter.AddToFilter(JobDocAddressSchema.E2_AddressType, partyType);
			filter.FetchOnlyFromLocalCache = !parent.IsInDatabase;

			return parent.Factory.LoadTop1<Party>(filter) ?? Create(parent, partyType);
		}

		internal static Party Create(BusinessObject parent, string partyType)
		{
			var party = parent.Factory.New<Party>();
			using (party.SuspendSettingHasChanges())
			using (party.GetValidationSuspender())
			{
				party.E2_ParentID = parent.PK;
				party.E2_ParentTableCode = parent.TablePrefix;
				party.E2_AddressType = partyType;
			}

			return party;
		}

		#endregion

		#region Properties

		#region ABIRoutingCode

		public ZString ABIRoutingCode
		{
			get
			{
				var result = ZString.Empty;
				if (HasRealOrganisation && HasRealAddress
					&& E2_GovRegNumType == PartyIdTypes.Codes.FilerCode
					&& (E2_AddressType == PartyTypes.Codes.CustomsBroker || E2_AddressType == PartyTypes.Codes.Importer))
				{
					result = Organisation.CustomsCodes.GetCustomsRegNo(PartyIdTypes.Codes.ABIRoutingCode, Constants.CountryCodes.UnitedStates, E2_OA_Address);
				}
				return result;
			}
		}

		#endregion

		#region E2_AddressType

		[List(nameof(Lookups) + "." + nameof(PartyLookups.PartyTypes))]
		public override ZString E2_AddressType
		{
			get { return base.E2_AddressType; }
			set
			{
				var hasChanges = base.E2_AddressType != value;
				base.E2_AddressType = value;
				if (hasChanges && !IsCopying)
				{
					MarkAsNeedingValidation();
					Shipment.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region E2_GovRegNumType

		[List(nameof(Lookups) + "." + nameof(PartyLookups.PartyIdTypes))]
		public override ZString E2_GovRegNumType
		{
			get { return base.E2_GovRegNumType; }
			set { base.E2_GovRegNumType = value; }
		}

		#endregion

		#region OrganisationPK

		[List(nameof(Lookups) + "." + nameof(PartyLookups.Organizations))]
		public new ZGuid OrganisationPK
		{
			get { return base.OrganisationPK; }
			set { base.OrganisationPK = value; }
		}

		#endregion

		#region Shipment

		public Shipment Shipment
		{
			get { return Factory.Load<Shipment>(E2_ParentID); }
		}

		#endregion

		#endregion

		#region Overrides

		protected override CodeDescriptionPairList SupportedDocAddressTypesList
		{
			get { return Factory.GetCachedValue<PartyTypes>(); }
		}

		protected override Type GetParentType(string prefix) => null;

		#region Validation

		public new PartyValidation Validation
		{
			get { return (PartyValidation)base.Validation; }
		}

		protected override JobDocAddressValidation GetNewValidation()
		{
			return new PartyValidation(this);
		}

		#endregion

		#region Lookups

		public new PartyLookups Lookups
		{
			get { return (PartyLookups)base.Lookups; }
		}

		protected override JobDocAddressLookups GetNewLookups()
		{
			return new PartyLookups(this);
		}

		#endregion

		#endregion

		#region Implementation

		RegistrationNumberResult GetPartyIdResult(JobDocAddress party)
		{
			return new RegistrationNumberResult(party.Factory, true, () => GetPartyId(party));
		}

		RegistrationNumber GetPartyId(JobDocAddress party)
		{
			var ace = new ZString[] { PartyIdTypes.Codes.ACE, PartyIdTypes.Codes.FAST };
			var firms = new ZString[] { PartyIdTypes.Codes.FIRMS };
			var filerCode = new ZString[] { PartyIdTypes.Codes.FilerCode };
			var other = new ZString[]
			{
				PartyIdTypes.Codes.SCAC,
				PartyIdTypes.Codes.DUNS,
				PartyIdTypes.Codes.EmployerIdentificationNumber,
				PartyIdTypes.Codes.SocialSecurityNumber,
				PartyIdTypes.Codes.CustomsAssignedNumber
			};

			ZString[] codes;
			switch (E2_AddressType)
			{
				case PartyTypes.Codes.NotifyParty:
					codes = firms.Concat(filerCode).Concat(ace).Concat(other).ToArray();
					break;
				case PartyTypes.Codes.CustomsBroker:
				case PartyTypes.Codes.Importer:
					codes = filerCode.Concat(ace).Concat(firms).Concat(other).ToArray();
					break;
				default:
					codes = ace.Concat(firms).Concat(filerCode).Concat(other).ToArray();
					break;
			}

			var cusCode = party.Organisation.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Constants.CountryCodes.UnitedStates, codes);
			var result = new RegistrationNumber();
			if (cusCode != null)
			{
				result.NumberType = cusCode.OK_CodeType;
				result.Number = cusCode.OK_CustomsRegNo;
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			E2_GovRegNumType = ZString.Empty;
		}

		#endregion
	}
}
