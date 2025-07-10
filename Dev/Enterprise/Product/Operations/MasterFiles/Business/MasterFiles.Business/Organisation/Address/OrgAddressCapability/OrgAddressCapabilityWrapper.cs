using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgAddressCapabilityWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgAddressCapabilityWrapper(OrgAddress parentAddress)
			: base(parentAddress.Factory)
		{
			if (parentAddress == null)
			{
				throw new ArgumentNullException(nameof(parentAddress), "OrgAddress parentAddress");
			}

			this.parentAddress = parentAddress;
		}

		readonly OrgAddress parentAddress;

		#region Schema

		public abstract class Schema
		{
			public const string Enabled = "Enabled";
			public const string AddressTypeDescription = "AddressTypeDescription";
			public const string Main = "Main";
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMain();
			ValidateEnabled();
		}

		void ValidateEnabled()
		{
			if (!IsValidationSuspended)
			{
				EnabledInfo.ClearAllNotifications();
				if (parentAddress.OA_IsActive && parentAddress.AddressCapability.IsEmpty)
				{
					EnabledInfo.AddError(Res.GetString("865a581b-10a7-4824-b574-9b8db8b2f215", "All addresses should have at least one capability selected."));
				}

				if (Enabled && Capability != null && Capability.PZ_AddressType == OrgConstants.AddressType.CustomsAddressOfRecord)
				{
					OrgHeader parentOrganisation = parentAddress.Header;
					if (parentOrganisation.Addresses.AddressesOfType(OrgConstants.AddressType.CustomsAddressOfRecord).Count > 1)
					{
						EnabledInfo.AddError(Res.GetString("10D5ED0C-B848-4D64-9BC7-1BA94657DE1A", "You have selected more than one Customs Address Of Record."));
					}
				}
			}
		}

		internal void ValidateMain()
		{
			if (!IsValidationSuspended)
			{
				MainInfo.ClearAllNotifications();
				var parentOrganisation = parentAddress.Header;
				if (Main && parentOrganisation != null)
				{
					var shouldCheckChildren = Enabled && !parentAddress.IsUniqueMainAddressForGlobalOrgOfType(AddressCapabilityType);
					if (shouldCheckChildren)
					{
						var cachedExistingMainAddresses = Factory.GetCachedValue(AddressCapabilityType + parentOrganisation.PK, () =>
						{
							return (from address in parentOrganisation.Addresses.Cast<OrgAddress>()
									where address.AddressCapability.GetIsMainAddress(AddressCapabilityType)
									select address).ToList();
						}, CacheStalenessPolicy.StaleWhenDataTableChanges(OrgAddressCapabilitySchema.Constants.TableName, Factory));

						var otherMatchedMainAddresses = cachedExistingMainAddresses.Where(address => address.PK != parentAddress.PK).Select(address => address.OA_Language == parentAddress.OA_Language);
						if (otherMatchedMainAddresses.Any())
						{
							if (otherMatchedMainAddresses.Any(languageMatches => languageMatches))
							{
								MainInfo.AddError(MainAddressOfThisTypeAlreadyExists);
							}
							else
							{
								MainInfo.AddWarning(MainAddressOfThisTypeAlreadyExistsDiffLanguage);
							}
						}
					}
				}
			}
		}

		public void ResetValidationCache(string code)
		{
			if (parentAddress.Header != null)
			{
				Factory.ClearCachedValue<List<bool>>(code + parentAddress.Header.PK);
			}
		}

		internal static string MainAddressOfThisTypeAlreadyExists
		{
			get { return Res.GetString("4f4639ca-106f-4d48-8ff8-69be923fb4f3", "Main address of this type already exists for this organization."); }
		}

		internal static string MainAddressOfThisTypeAlreadyExistsDiffLanguage
		{
			get { return Res.GetString("86619962-0ddc-425c-92b4-9c3ed38c5efe", "A different language Main address of this type already exists for this organization."); }
		}

		internal static string CannotUntickDefaultOfficeAddressCaption
		{
			get { return Res.GetString("2367735e-f951-48da-b530-57b54af66192", "Cannot Change Default Office Address"); }
		}

		internal static string NoAccessToUntickDefaultMessage
		{
			get { return Res.GetString("d2f97f70-bb55-4435-893b-3ccba59c7f21", "You do not have security rights to change the Main Address of this organization."); }
		}

		internal static string CannotUntickDefaultOfficeAddressText
		{
			get { return Res.GetString("8ad0f906-5912-4dc5-aa43-74031c33dcbf", "You cannot make the Main Office Address non-default.\r\nTo change the Main Office Address, you should select the Default checkbox on another address."); }
		}

		#endregion

		public override void Delete()
		{
			base.Delete();
			if (Enabled)
			{
				Capability.Delete();
			}
		}

		#region Capability

		public OrgAddressCapability Capability
		{
			get
			{
				if (Enabled)
				{
					using (SuspendSettingHasChanges())
					{
						CreateOrLoadCapability();
					}
				}
				return fCapability;
			}
		}

		OrgAddressCapability fCapability;

		void CreateOrLoadCapability()
		{
			if (fCapability == null || fCapability.IsDeleted)
			{
				fCapability = parentAddress.GetAddressCapability(AddressCapabilityType);
				if (fCapability == null || fCapability.IsDeleted)
				{
					refreshValuesSuspended = true;

					fCapability = Factory.New<OrgAddressCapability>();
					using (fCapability.SuspendSettingHasChanges())
					{
						fCapability.PZ_OA = parentAddress.PK;
						fCapability.PZ_AddressType = AddressCapabilityType;
					}
					RegisterEditableChildObject(fCapability);
					refreshValuesSuspended = false;

					RefreshValues(fCapability);
				}

				if (fCapability != null && !fCapability.IsDeleted)
				{
					fCapability.UpdatedByDataRefresh -= (obj, arg) => RefreshValues(fCapability);
					fCapability.UpdatedByDataRefresh += (obj, arg) => RefreshValues(fCapability);
				}
			}
		}

		#endregion

		#region Properties

		#region Enabled

		public void SetEnabledSilently(ZBool value)
		{
			enabled = value;
			if (enabled)
			{
				CreateOrLoadCapability();
			}
			else
			{
				main = ZBool.False;
				Capability.Delete();
			}
		}

		public ZBool Enabled
		{
			get { return enabled; }
			set
			{
				if (enabled != value)
				{
					SetEnabledSilently(value);
					if (!parentAddress.SettingDefaults)
					{
						if (parentAddress.Header != null && Enabled)
						{
							SetMainSilently(parentAddress.Header.Addresses.CanBeSetAsDefaultAddress(AddressCapabilityType));
						}
					}
					ValidateCapabilitiesEnabled();
				}
				EnabledInfo.RefreshBinding();
			}
		}

		void ValidateCapabilitiesEnabled()
		{
			foreach (OrgAddressCapabilityWrapper capability in parentAddress.AddressCapability)
			{
				capability.ValidateEnabled();
			}
		}

		internal bool Enabled_ReadOnly => CanNotBeModified;
		ZBool enabled;

		bool CanNotBeModified
		{
			get
			{
				if (IsDeleted || IsTheOnlyMainOfficeAddress() || IsCustomsAddressWithoutSecurityOrHeaderIsNull || IsEUCustomsAddressWithoutSecurityOrHeaderIsNull)
				{
					return true;
				}

				var security = parentAddress.Header?.SecurityProvider;
				var isARAP = AddressCapabilityType == OrgAddressType.Receivables || AddressCapabilityType == OrgAddressType.Payables;
				return security != null && (isARAP ? !security.HasModifyAddressCapabilitiesARAP : !security.HasModifyAddressCapabilitiesNonARAP);
			}
		}

		internal bool IsCustomsAddressWithoutSecurityOrHeaderIsNull
		{
			get
			{
				return AddressCapabilityType == OrgConstants.AddressType.CustomsAddressOfRecord &&
							(parentAddress.Header == null || !parentAddress.Header.SecurityProvider.HasModifyCustomsAddressSecurity);
			}
		}

		internal bool IsEUCustomsAddressWithoutSecurityOrHeaderIsNull
		{
			get
			{
				return AddressCapabilityType == OrgConstants.AddressType.EUCustomsAddress &&
							(parentAddress.Header == null || !parentAddress.Header.SecurityProvider.HasModifyEUCustomsAddressSecurity);
			}
		}

		public ZPropertyInfo EnabledInfo
		{
			get { return GetZPropertyInfo(Schema.Enabled); }
		}

		#endregion

		#region Address Type

		public ZString AddressCapabilityType
		{
			get { return fAddressCapabilityType; }
			set
			{
				fAddressCapabilityType = value;
				fCapability = parentAddress.GetAddressCapability(AddressCapabilityType);

				if (fCapability != null && !fCapability.IsDeleted)
				{
					SetEnabledSilently(true);
					SetMain(fCapability.PZ_IsMainAddress);
					fCapability.UpdatedByDataRefresh -= (obj, arg) => RefreshValues(fCapability);
					fCapability.UpdatedByDataRefresh += (obj, arg) => RefreshValues(fCapability);
				}
			}
		}

		ZString fAddressCapabilityType;

		public ZString AddressTypeDescription
		{
			get { return OrgCodeLists.AddressType_List(Factory).GetDescriptionFromCode(AddressCapabilityType); }
		}

		public ZPropertyInfo AddressTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.AddressTypeDescription); }
		}

		#endregion

		#region Main

		public void SetMainSilently(ZBool value)
		{
			OrgAddress oldMain = null;
			if (!parentAddress.SettingDefaults && parentAddress.Header != null)
			{
				oldMain = parentAddress.Header.MainAddress;
			}

			SetMain(value);
			if (!parentAddress.SettingDefaults && parentAddress.Header != null)
			{
				if ((!parentAddress.Header.OH_IsGlobalAccount ||
					(parentAddress.RelatedCountry != null && GlbCompany.CurrentCompany.Country != null &&
					parentAddress.Header.OH_IsGlobalAccount && parentAddress.GetBaseOA_RL_NKRelatedPortCode().Left(2) == oldMain.GetBaseOA_RL_NKRelatedPortCode().Left(2))) &&
					parentAddress.OA_Language == oldMain.OA_Language)
				{
					if (oldMain != null && parentAddress.PK != oldMain.PK && parentAddress.Header != null && IsDefaultOfficeAddress)
					{
						if (parentAddress.GetBaseOA_RL_NKRelatedPortCode().IsEmpty)
						{
							parentAddress.OA_RL_NKRelatedPortCode = parentAddress.Header.OH_RL_NKClosestPort;
						}

						parentAddress.Header.Addresses.SwapMainAddress(oldMain, parentAddress);
						oldMain.MarkAsNeedingValidation();
					}
				}
			}
		}

		public ZBool Main
		{
			get { return main; }
			set
			{
				if (IsTheOnlyMainOfficeAddress() && !value && parentAddress.Header != null)
				{
					parentAddress.Header.OnShowMessage(CannotUntickDefaultOfficeAddressCaption, NoAccessToUntickDefaultMessage);
				}
				else
				{
					if (value && !Enabled)
					{
						Enabled = ZBool.True;
					}
					SetMainSilently(value);
					if (!IsValidationSuspended)
					{
						ValidateMain();
						if (parentAddress.Header != null)
						{
							foreach (OrgAddress address in parentAddress.Header.Addresses)
							{
								address.Validation.ValidateOA_IsActive();
								address.AddressCapability.MarkAsNeedingValidationIncludingChildren();
							}
						}
					}
				}
				MainInfo.RefreshBinding();
			}
		}

		internal bool Main_ReadOnly => CanNotBeModified || !Env.Security.OrgAddressCapabilitiesMain.IsAllowed;

		ZBool main;

		public void SetMain(ZBool value)
		{
			main = value;
			if (Enabled)
			{
				Capability.PZ_IsMainAddress = value;
			}
		}

		public ZPropertyInfo MainInfo
		{
			get { return GetZPropertyInfo(Schema.Main); }
		}

		#endregion

		public bool IsDefaultOfficeAddress
		{
			get { return AddressCapabilityType == OrgAddressType.Office.Code && Enabled && Main; }
		}

		bool IsTheOnlyMainOfficeAddress()
		{
			bool result = IsDefaultOfficeAddress;

			if (result && parentAddress.Header != null)
			{
				foreach (OrgAddress otherAddress in parentAddress.Header.Addresses)
				{
					OrgAddressCapabilityWrapper otherOfficeAddressCapability = otherAddress.AddressCapability.GetAddressCapabilityOnCode(OrgAddressType.Office.Code);
					if (
						otherAddress != parentAddress
						&& otherOfficeAddressCapability != null
						&& otherOfficeAddressCapability.IsDefaultOfficeAddress)
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		internal void RefreshValues(OrgAddressCapability newCapability)
		{
			if (!refreshValuesSuspended)
			{
				bool newEnabled = newCapability != null;
				bool newMain = newCapability != null && newCapability.PZ_IsMainAddress;

				if ((fCapability != null && fCapability != newCapability) || enabled != newEnabled || main != newMain)
				{
					fCapability = newCapability;
					enabled = newEnabled;
					main = newMain;

					EnabledInfo.RefreshBinding();
					MainInfo.RefreshBinding();
				}
			}
		}
		bool refreshValuesSuspended;

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}
	}
}
