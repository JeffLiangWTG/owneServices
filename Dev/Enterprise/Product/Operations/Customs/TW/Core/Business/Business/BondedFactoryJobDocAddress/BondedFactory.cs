using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class BondedFactory : JobDocAddress
	{
		public BondedFactory(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
			DefaultAddressType = ZArchitecture.Business.AddressType.NoDefault;
		}

		public JobDeclaration Declaration => Factory.Load<JobDeclaration>(E2_ParentID);

		[List(nameof(Lookups) + "." + nameof(BondedFactoryLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.TW.Business.BondedFactory|OrganisationPK", Caption = "Organization", FullDescription = "The VAT number of the bonded warehouse.")]
		public new ZGuid OrganisationPK
		{
			get { return base.OrganisationPK; }
			set
			{
				base.OrganisationPK = value;
				E2_OA_AddressInfo.RefreshBinding();
				SetDefaultAddress();
			}
		}

		void SetDefaultAddress()
		{
			var addresses = Organisation?.AddressesActive;
			var addressesWithCCPAndTaiwain = addresses?.OfType<OrgAddress>().
												Where(x => x.CustomsCodes.Any(y => y.OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID &&
												y.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Taiwan));

			E2_OA_Address = addressesWithCCPAndTaiwain?.FirstOrDefault()?.PK ?? Organisation?.MainAddress.PK ?? ZGuid.Empty;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.BondedFactory|BondedID", Caption = "Bonded ID", FullDescription = "The Bonded ID of the bonded warehouse issued by customs.")]
		public ZString BondedID => GetOrgCusCode(Address)?.OK_CustomsRegNo ?? ZString.Empty;

		public ZPropertyInfo BondedIDInfo => GetZPropertyInfo(nameof(BondedID));

		[ResourceStringData("Enterprise.Customs.TW.Business.BondedFactory|BondedIDTypeCode", Caption = "Type Code")]
		[List(nameof(Lookups) + "." + nameof(BondedFactoryLookups.CustomsCodesList))]
		public ZString BondedIDTypeCode => GetOrgCusCode(Address)?.OK_CodeType ?? ZString.Empty;

		public ZPropertyInfo BondedIDTypeCodeInfo => GetZPropertyInfo(nameof(BondedIDTypeCode));

		[ResourceStringData("Enterprise.Customs.TW.Business.BondedFactory|BondedIDTypeDescription", Caption = "Description")]
		public ZString BondedIDTypeDescription => Lookups.CustomsCodesList.GetDescriptionFromCode(BondedIDTypeCode);

		public ZPropertyInfo BondedIDTypeDescriptionInfo => GetZPropertyInfo(nameof(BondedIDTypeDescription));

		OrgCusCode GetOrgCusCode(OrgAddress address)
		{
			OrgCusCode cacheValue = null;
			if (address != null && address.PK.IsValid)
			{
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "BondedFactoryOrgCusCode{0}", address.PK.ToString());
				cacheValue = Factory.GetCachedValue(cacheKey, () =>
				{
					return SharedHelper.GetCustomsControlID(Address);
				});
			}
			return cacheValue;
		}

		#region Type safety 
		public new BondedFactoryLookups Lookups => (BondedFactoryLookups)base.Lookups;

		protected override JobDocAddressLookups GetNewLookups() => new BondedFactoryLookups(this);

		protected override JobDocAddressValidation GetNewValidation() => new BondedFactoryValidation(this);
		#endregion
	}
}
