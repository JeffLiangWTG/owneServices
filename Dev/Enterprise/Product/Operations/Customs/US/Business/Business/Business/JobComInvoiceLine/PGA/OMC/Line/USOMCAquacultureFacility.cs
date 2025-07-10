using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class USOMCAquacultureFacility : CusAddInfo<USOMCAquacultureFacilityAddInfo>
	{
		public USOMCAquacultureFacility(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<USOMCAquacultureFacilityAddInfo>.Schema
		{
			public const string US_OA_AquacultureFacility = USOMCAquacultureFacilityAddInfoSchema.Constants.US_OA_AquacultureFacility;
			public const string AquacultureFacilityOrgPK = "AquacultureFacilityOrgPK";
		}

		#endregion

		public new OMCHeader Parent
		{
			get { return (OMCHeader)base.Parent; }
		}

		#region AddInfo Properties

		[List(nameof(US_OA_AquacultureFacility_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_AquacultureFacility
		{
			get { return AddInfo.US_OA_AquacultureFacility; }
			set { AddInfo.US_OA_AquacultureFacility = value; }
		}

		public ZPropertyInfo US_OA_AquacultureFacilityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_AquacultureFacility, x => AddInfo.US_OA_AquacultureFacilityInfo); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_AquacultureFacility_ZAddress
		{
			get
			{
				if (aquacultureFacilityAddress_ZAddress == null)
				{
					aquacultureFacilityAddress_ZAddress = GetNewUS_AquacultureFacilityAddress_ZAddress();
					aquacultureFacilityAddress_ZAddress.IsOrgVisible = true;
					aquacultureFacilityAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return aquacultureFacilityAddress_ZAddress;
			}
		}
		ZAddress aquacultureFacilityAddress_ZAddress;

		protected ZAddress GetNewUS_AquacultureFacilityAddress_ZAddress()
		{
			return new ZAddress(US_OA_AquacultureFacilityInfo);
		}

		public OrgAddress AquacultureFacilityAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_AquacultureFacility); }
		}

		internal OrgHeaderWrapper AquacultureFacilityWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (AquacultureFacilityAddress != null)
				{
					result = OrgHeaderWrapper.New(AquacultureFacilityAddress);
				}
				return result;
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USOMCAquacultureFacilityAddInfoLookups.Organizations))]
		public ZGuid AquacultureFacilityOrgPK
		{
			get { return US_OA_AquacultureFacility_ZAddress.OrgPK; }
			set { US_OA_AquacultureFacility_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo AquacultureFacilityOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AquacultureFacilityOrgPK, x => US_OA_AquacultureFacility_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USOMCAquacultureFacilityAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USOMCAquacultureFacilityAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		USOMCAquacultureFacilityAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USOMCAquacultureFacilityAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USOMCAquacultureFacilityAddInfo fAddInfo;

		public void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "OMC Aquaculture Facility"; }
		}

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (USOMCAquacultureFacility)base.CloneInternal(args);
			return result;
		}

		#endregion
	}
}
