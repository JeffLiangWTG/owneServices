using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	[CodeProperty(JobDocAddress.Schema.E2_CompanyName), DescriptionProperty(USOrganisationDocAddress.Schema.E2_ShortAddress)]
	public class USOrganisationDocAddress : JobDocAddress, IObsoleteValidation
	{
		public USOrganisationDocAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : JobDocAddress.Schema
		{
			public const string E2_ShortAddress = "E2_ShortAddress";
		}

		#endregion

		#region New Properties

		#region Invoice

		public JobComInvoiceHeader Invoice
		{
			get { return Factory.Load<JobComInvoiceHeader>(E2_ParentID); }
		}

		#endregion

		#region ShortAddress

		public ZString E2_ShortAddress
		{
			get
			{
				var orgAddress = this.Address;
				return orgAddress != null && !orgAddress.OA_Code.IsEmpty ? orgAddress.OA_Code : E2_Address1;
			}
		}

		#endregion

		#endregion

		#region Overrides

		#region Validation

		public new USOrganisationDocAddressValidation Validation
		{
			get { return (USOrganisationDocAddressValidation)base.Validation; }
		}

		protected override JobDocAddressValidation GetNewValidation()
		{
			return new USOrganisationDocAddressValidation(this);
		}

		#endregion

		#region E2_AddressOverride

		public override ZBool E2_AddressOverride
		{
			get => base.E2_AddressOverride;
			set
			{
				base.E2_AddressOverride = value;
				if (Invoice != null && !Invoice.IsDeleted)
				{
					Invoice.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region E2_OA_Address

		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set
			{
				base.E2_OA_Address = value;
				if (Invoice != null && !Invoice.IsDeleted)
				{
					Invoice.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region E2_Contact

		public override ZString E2_Contact
		{
			get => base.E2_Contact;
			set
			{
				base.E2_Contact = value;
				if (Invoice != null && !Invoice.IsDeleted)
				{
					Invoice.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region E2_ParentID

		public override ZGuid E2_ParentID
		{
			get => base.E2_ParentID;
			set
			{
				base.E2_ParentID = value;
				if (Invoice != null && !Invoice.IsDeleted)
				{
					Invoice.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region E2_ParentTableCode

		public override ZString E2_ParentTableCode
		{
			get => base.E2_ParentTableCode;
			set
			{
				base.E2_ParentTableCode = value;
				if (Invoice != null && !Invoice.IsDeleted)
				{
					Invoice.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region E2_Phone

		public override ZString E2_Phone
		{
			get
			{
				if (Country != null)
				{
					return PhoneNumberCalculator.GetUnformattedPhoneNumber(base.E2_Phone, Country.Code != Core.Constants.CountryCodes.UnitedStates);
				}
				else
				{
					return base.E2_Phone;
				}
			}
			set
			{
				base.E2_Phone = value;
				if (Invoice != null && !Invoice.IsDeleted)
				{
					Invoice.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region E2_AddressType

		public override ZString E2_AddressType
		{
			get => base.E2_AddressType;
			set
			{
				base.E2_AddressType = value;
				if (Invoice != null && !Invoice.IsDeleted)
				{
					Invoice.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region E2_RN_NKCountryCode

		public override ZString E2_RN_NKCountryCode
		{
			get => base.E2_RN_NKCountryCode;
			set
			{
				base.E2_RN_NKCountryCode = value;
				if (Invoice != null && !Invoice.IsDeleted)
				{
					Invoice.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#endregion
	}
}
