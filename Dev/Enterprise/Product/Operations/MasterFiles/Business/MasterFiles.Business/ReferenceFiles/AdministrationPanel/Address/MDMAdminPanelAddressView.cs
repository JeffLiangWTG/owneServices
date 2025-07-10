using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class MDMAdminPanelAddressView : AutoMDMAdminPanelAddressView
	{
		public MDMAdminPanelAddressView(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public BusinessObject AddressEntity
		{
			get
			{
				var type = MDM_AddressType == nameof(OrgAddress) ? typeof(OrgAddress) : typeof(JobDocAddress);
				return FactoryForAddressEntity.Load(type, PK) ?? FactoryForAddressEntity.GetNull<OrgAddress>();
			}
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => false;

		public void SaveAddressEntity()
		{
			FactoryForAddressEntity.Save();
		}

		BusinessObjectFactory FactoryForAddressEntity => factoryForAddressEntity ?? (factoryForAddressEntity = new BusinessObjectFactory());
		BusinessObjectFactory factoryForAddressEntity;

		[List("OrganizationList")]
		public override ZGuid MDM_ParentID
		{
			get => base.MDM_ParentID;
			set => base.MDM_ParentID = value;
		}

		public ZString MDM_ValidationStatusDescripition
		{
			get
			{
				if (AddressValidationStatusList.AddressValidationStatuses.ContainsCode(MDM_ValidationStatus))
				{
					return AddressValidationStatusList
						.AddressValidationStatuses[MDM_ValidationStatus, System.StringComparison.CurrentCultureIgnoreCase].Description;
				}
				return Res.GetString("8d83bb27-05c5-4567-90d4-6c33d62de8e6", "Unknown Validation Status");
			}
		}

		public ZString MDM_AddressTypeDescription
		{
			get
			{
				var list = DocAddressTypes.GetAddressTypesCodeDescriptionPairList(Factory);
				if (list.ContainsCode(MDM_AddressType))
				{
					return list[MDM_AddressType, System.StringComparison.CurrentCultureIgnoreCase].Description;
				}
				return Res.GetString("5756f5ce-365f-4059-8260-c44c6c3c4b28", "Unknown Address Type");
			}
		}

		public OrgHeaderCollection OrganizationList
		{
			get
			{
				return new OrgHeaderCollection(Factory);
			}
		}

		public bool HasAddressInfoChanges { get; set; }

		public AddressAutoVerifyState AutoVerifyState { get; set; }
	}

	public enum AddressAutoVerifyState
	{
		Waiting = 0,
		Verifying = 1,
		Skipped = 2,
		Verified = 3
	}
}
