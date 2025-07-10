using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ACEFDAJobDocAddress : JobDocAddress, IPGAContactDetails, IAddressDetails
	{
		public ACEFDAJobDocAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			OverrideRequirement = new ACEFDAJobDocAddressRequirement(this);
		}

		public bool IsFSVPImporter => E2_AddressType == DocAddressTypes.Codes.FSVPImporter;

		#region AddressDescription

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public new ZString AddressDescription
		{
			get
			{
				var result = E2_AddressType;

				if (result.IsEmpty || !SupportedDocAddressTypesList.ContainsCode(E2_AddressType))
				{
					result = DocAddressTypes.Unspecified;
				}
				else
				{
					result = Lookups.AddressTypeList.GetDescriptionFromCode(E2_AddressType);
				}

				return result;
			}
		}

		#endregion

		#region Lookups

		public new ACEFDAJobDocAddressLookups Lookups
		{
			get { return (ACEFDAJobDocAddressLookups)base.Lookups; }
		}

		protected override JobDocAddressLookups GetNewLookups()
		{
			return new ACEFDAJobDocAddressLookups(this);
		}

		#endregion

		#region Overrides

		[List(nameof(Lookups) + "." + nameof(ACEFDAJobDocAddressLookups.AddressTypeList))]
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
					if (IsFSVPImporter && E2_AddressOverride)
					{
						Validation.ValidateE2_GovRegNumType();
					}
				}
			}
		}

		public override ZBool E2_AddressOverride
		{
			get { return base.E2_AddressOverride; }
			set
			{
				var hasChanges = base.E2_AddressOverride != value;
				base.E2_AddressOverride = value;
				if (hasChanges && !IsCopying & IsFSVPImporter && E2_AddressOverride)
				{
					Validation.ValidateE2_GovRegNumType();
				}
			}
		}

		protected override CodeDescriptionPairList SupportedDocAddressTypesList
		{
			get { return Lookups.AddressTypeList; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			E2_GovRegNumType = ZString.Empty;
		}

		#endregion

		#region IPGAContactDetails

		ZString IPGAContactDetails.Name => ((IAddressDetails)this).ContactName;

		ZString IPGAContactDetails.PhoneNumber => ((IAddressDetails)this).Phone.GetLocalPhoneNumber(Core.Constants.CountryCodes.UnitedStates);

		ZString IPGAContactDetails.EmailAddress => ((IAddressDetails)this).Email;

		ZString IPGAContactDetails.Fax => ((IAddressDetails)this).Fax;

		IAddressDetails IPGAContactDetails.CompanyAddress => this;

		#endregion

		#region IAddressDetails

		ZString IAddressDetails.CompanyName => CompanyName;

		ZString IAddressDetails.ContactName => E2_Contact;

		ZString IAddressDetails.Phone => PhoneNumber.FormattedForBinding;

		ZString IAddressDetails.Fax => E2_Fax;

		ZString IAddressDetails.Email => E2_Email;

		ZString IAddressDetails.AddressLine1 => Address1;

		ZString IAddressDetails.AddressLine2 => Address2;

		ZString IAddressDetails.City => City;

		ZString IAddressDetails.State => E2_State;

		ZString IAddressDetails.PostCode => Postcode;

		ZString IAddressDetails.Country => E2_RN_NKCountryCode;

		#endregion
	}
}
