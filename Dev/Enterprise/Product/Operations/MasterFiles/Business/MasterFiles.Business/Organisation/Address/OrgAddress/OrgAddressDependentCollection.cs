using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[UniversalCopyClearCollectionOnCopy]
	public sealed class OrgAddressDependentCollection : DependentBusinessObjectCollection<OrgAddress, OrgHeader>
	{
		public OrgAddressDependentCollection(OrgHeader parent)
			: base(parent)
		{
			Argument.NotNull(parent, "Parent");
			this.ParentOrg = parent;
		}

		public OrgAddressDependentCollection(OrgHeader parent, ZQuery filter)
			: base(parent, filter)
		{
			Argument.NotNull(parent, "Parent");
			this.ParentOrg = parent;
		}

		public OrgAddressDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgAddressDependentCollection(OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory, false)
		{
			Argument.NotNull(parent, "Parent");
			this.ParentOrg = parent;
		}

		readonly OrgHeader ParentOrg;

		#region Remove / Delete

		public event EventHandler OnTriedToRemoveMainAddress;

		public override void Remove(BusinessObject elementToRemove)
		{
			if (elementToRemove.IsDeleted)
			{
				return;     // HACK stupid datarefreshbus crap
			}

			OrgAddress addressToRemove = (OrgAddress)elementToRemove;
			if ((!MainAddressDeleted && addressToRemove != MainAddressDontCreate) || RemoveMainAddressIsAllowed || IsDeletingForDataRefresh)
			{
				base.Remove(elementToRemove);
			}
			else
			{
				FireOnTriedToRemoveMainAddressEvent();
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			OrgAddress addressToDelete = (OrgAddress)elementToDelete;
			if (!addressToDelete.IsDeleted)
			{
				if ((!MainAddressDeleted && addressToDelete != MainAddressDontCreate) || RemoveMainAddressIsAllowed)
				{
					base.RemoveAndDelete(elementToDelete);
				}
			}
			else
			{
				FireOnTriedToRemoveMainAddressEvent();
			}
		}

		internal void RemoveAndDeleteAllIncludingMainAddress()
		{
			RemoveMainAddressIsAllowed = true;
			try
			{
				RemoveAndDeleteMainAddress();
				base.RemoveAndDeleteAll();
			}
			finally
			{
				RemoveMainAddressIsAllowed = false;
			}
		}

		void RemoveAndDeleteMainAddress()
		{
			if (Contains(MainAddress))
			{
				RemoveAndDelete(MainAddress);
			}
			MainAddressDeleted = true;
		}

		ZBool RemoveMainAddressIsAllowed;
		bool MainAddressDeleted;

		#endregion

		#region Add

		public OrgAddress AddNew(OrgAddressType addressType, ZBool isDefault)
		{
			OrgAddress result = null;

			if (isDefault)
			{
				foreach (OrgAddress address in ParentOrg.Addresses)
				{
					if (address.AddressCapability.GetCapabilityEnabled(addressType) && address.AddressCapability.GetIsMainAddress(addressType))
					{
						result = address;
					}
				}
			}
			if (result == null)
			{
				result = AddNew();
				result.AddressCapability.SetCapabilityEnabled(addressType.Code);
				if (isDefault)
				{
					result.AddressCapability.SetIsMainAddress(addressType.Code);
				}
				else
				{
					result.AddressCapability.SetIsNotMainAddress(addressType.Code);
				}
			}

			return result;
		}

		public OrgAddress AddNewMainAddress()
		{
			if (Factory.ServiceContainer.GetService<BusinessObjectUniversalCopyFactoryService>() != null)
			{
				return null;
			}

			ReportDuplicateMainAddressIfExists();

			OrgAddress address = AddNew();
			address.SettingDefaults = true;
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
			address.SettingDefaults = false;

			address.AddressCapability.ClearHasChanges();
			return address;
		}

		#endregion

		#region Duplicate Main Address / AddressNotOnFile

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting message")]
		void ReportDuplicateMainAddressIfExists()
		{
			if (OrganisationRegistry.Instance.EnableAddressNotOnFileLogging.Value)
			{
				var mainAddressFromDb = LoadMainAddressDirectlyFromDatabase();

				var shouldReport = LastMainAddress is not null
					|| LastMainAddressDontCreate is not null
					|| LastMainAddressCached && (LastMainAddressCachedValue?.IsDeleted ?? true)
					|| mainAddressFromDb is not null;

				if (shouldReport)
				{
					var messageBuilder = new StringBuilder("Duplicate main address should not be created again when it already exists.");
					messageBuilder.AppendLine();
					messageBuilder.AppendLine("-- Additional Information --");
					messageBuilder.AppendLine("LastMainAddress:");
					messageBuilder.AppendLine(LastMainAddress is null ? ValueIsNull : GetAddressInformationForLog(LastMainAddress));
					messageBuilder.AppendLine("LastMainAddressDontCreate:");
					messageBuilder.AppendLine(LastMainAddressDontCreate is null ? ValueIsNull : GetAddressInformationForLog(LastMainAddressDontCreate));
					messageBuilder.AppendLine("LastMainAddressCached:");
					messageBuilder.AppendLine(LastMainAddressCached.ToString());
					messageBuilder.AppendLine("LastMainAddressCachedValue:");
					messageBuilder.AppendLine(LastMainAddressCachedValue is null ? ValueIsNull : GetAddressInformationForLog(LastMainAddressCachedValue));
					messageBuilder.AppendLine("mainAddressFromDb:");
					messageBuilder.AppendLine(mainAddressFromDb is null ? ValueIsNull : GetAddressInformationForLog(mainAddressFromDb));
					messageBuilder.AppendLine("OrgAddressDependentCollection:");
					messageBuilder.AppendLine(string.Join(System.Environment.NewLine, this.ToArray<OrgAddress>().OrderBy(x => x.OA_Code).Select((x, index) => $"[{index}]: {GetAddressInformationForLog(x)}")));
					messageBuilder.AppendLine("OrgAddressesFromDb:");
					messageBuilder.AppendLine(string.Join(System.Environment.NewLine, LoadAddressesFromDatabase().OrderBy(x => x.OA_Code).Select((x, index) => $"[{index}]: {GetAddressInformationForLog(x)}")));
					messageBuilder.AppendLine("IsParentOrgDeleted:");
					messageBuilder.AppendLine(ParentOrg is null ? ValueIsNull : ParentOrg.IsDeleted.ToString());

					ErrorReporter.ReportDeveloperExceptionOnce("DuplicateMainAddress", messageBuilder.ToString(), new InvalidOperationException("Duplicate Main Address Created"));
				}
			}
		}

		OrgAddress LoadMainAddressDirectlyFromDatabase()
		{
			if (ParentOrg is null || !ParentOrg.IsInDatabase)
			{
				return null;
			}

			return LoadMainAddressDirectlyFromDatabase(Factory, ParentOrg.PK);
		}

		public static OrgAddress LoadMainAddressDirectlyFromDatabase(BusinessObjectFactory factory, ZGuid orgPk)
		{
			var capabilitySubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			capabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgAddressType.Office.Code);
			capabilitySubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True);

			var addressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPk);
			addressQuery.AddSubQuery(capabilitySubQuery, JoinCondition.And);

			return factory.LoadTop1<OrgAddress>(addressQuery);
		}

		OrgAddress[] LoadAddressesFromDatabase()
		{
			if (ParentOrg is null || !ParentOrg.IsInDatabase)
			{
				return Array.Empty<OrgAddress>();
			}

			var addressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, ParentOrg.PK);

			return Factory.Load<OrgAddress>(addressQuery);
		}

		string GetAddressInformationForLog(OrgAddress address)
		{
			var isActive = address.OA_IsActive ? "A" : "N";
			var isDeleted = address.IsDeleted ? "D" : "N";
			var isInDatabase = address.IsInDatabase ? "I" : "N";
			var addressCapabilityCount = address.AddressCapability.Count;
			var isMainAddress = address.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Office) ? "M" : "N";
			var isOfficeEnabled = address.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office) ? "E" : "N";
			var officeCapability = address.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Office);
			var isOfficeExist = officeCapability != null ? "E" : "N";
			var isOfficeDeleted = officeCapability?.IsDeleted == true ? "D" : "N";
			var capabilitiesCollectionCount = address.CapabilitiesCollection.Count;
			var isMainAddressFromCapabilities = address.CapabilitiesCollection.Any(v => v.PZ_AddressType == OrgConstants.AddressType.Office && v.PZ_IsMainAddress) ? "M" : "N";
			var isOfficeEnabledFromCapabilities = address.CapabilitiesCollection.Any(v => v.PZ_AddressType == OrgConstants.AddressType.Office) ? "E" : "N";
			var isCapabilitiesCollectionCached = address.Factory.IsCached(OrgAddressCapabilitySchema.Constants.TableName, new ZQuery(OrgAddressCapabilitySchema.PZ_OA, address.PK)) ? "E" : "N";
			return $"[{isActive}/{isDeleted}/{isInDatabase}/{addressCapabilityCount}/{isMainAddress}/{isOfficeEnabled}/{isOfficeExist}/{isOfficeDeleted}/{capabilitiesCollectionCount}/{isMainAddressFromCapabilities}/{isOfficeEnabledFromCapabilities}/{isCapabilitiesCollectionCached}] {address.OA_Code}";
		}

		OrgAddress LastMainAddress { get; set; }

		OrgAddress LastMainAddressDontCreate { get; set; }

		OrgAddress LastMainAddressCachedValue { get; set; }

		bool LastMainAddressCached { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "For Error reporting")]
		const string ValueIsNull = "NullForLog";

		#endregion Duplicate Main Address / AddressNotOnFile

		#region Addresses Types / Categories

		public OrgAddress BestAddressForPort(ZString port, params string[] addressTypes)
		{
			PortBasedOrgAddressRetriever retriever = new PortBasedOrgAddressRetriever(ParentOrg, port, addressTypes);
			return retriever.BestAddress;
		}

		public OrgAddress DefaultAddressOfType(OrgAddressType addressType, bool useCommonLanguage = false, string preferredLanguage = null, bool activeOnly = true)
			=> GetGlobalDefaultAddressOfType(addressType, preferredLanguage, activeOnly)
				?? new OrgAddressDefaultFinder(this.ToArray<OrgAddress>(), useCommonLanguage, preferredLanguage).DefaultAddressOfType(addressType, activeOnly);

		OrgAddress GetGlobalDefaultAddressOfType(OrgAddressType addressType, string preferredLanguage, bool activeOnly)
		{
			OrgAddress result = null;

			if (!ParentOrg.IsDeleted && ParentOrg.OH_IsGlobalAccount)
			{
				if (!string.IsNullOrEmpty(preferredLanguage) && preferredLanguage != Core.Constants.Languages.English)
				{
					result = GetGlobalDefaultAddressOfTypeAndLanguage(addressType, preferredLanguage, activeOnly);
				}

				if (result == null)
				{
					result = GetGlobalDefaultAddressOfTypeAndLanguage(addressType, Core.Constants.Languages.English, activeOnly);
				}

				if (result == null)
				{
					result = GetGlobalDefaultAddressOfTypeAndLanguage(addressType, string.Empty, activeOnly);
				}
			}

			return result;
		}

		OrgAddress GetGlobalDefaultAddressOfTypeAndLanguage(OrgAddressType addressType, string preferredLanguage, bool activeOnly)
		{
			foreach (OrgAddress address in this)
			{
				if (!address.IsDeleted &&
					(!activeOnly || address.OA_IsActive) &&
					address.AddressCapability.GetCapabilityEnabled(addressType) && address.AddressCapability.GetIsMainAddress(addressType) &&
					address.RelatedCountry != null && GlbCompany.CurrentCompany?.Country != null &&
					address.RelatedCountry.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode &&
					(string.IsNullOrEmpty(preferredLanguage) || address.OA_Language == preferredLanguage))
				{
					return address;
				}
			}

			return null;
		}

		public OrgAddressList RetrieveAddressesWithAddressType(ZString type, bool activeOnly = true)
		{
			return AddressesOfType(type, activeOnly);
		}

		public OrgAddressList AddressesOfType(OrgAddressType type, bool activeOnly = true)
		{
			return AddressesOfType(type.Code, activeOnly);
		}

		public OrgAddressList AddressesOfType(ZString type, bool activeOnly = true, bool currentCompanyOnly = true)
		{
			OrgAddressList list = new OrgAddressList();
			foreach (OrgAddress address in this)
			{
				if (!address.IsDeleted
					&& (!activeOnly || address.OA_IsActive)
					&& address.AddressCapability.GetCapabilityEnabled(type)
					&& (!ParentOrg.OH_IsGlobalAccount || !currentCompanyOnly || ParentOrg.OH_IsGlobalAccount && address.RelatedCountry != null && address.RelatedCountry.Code == GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					list.Add(address);
				}
			}
			return list;
		}

		public OrgAddressList AddressesOfCategory(AddressCategory category, bool activeAddressesOnly = true)
		{
			OrgAddressList list = new OrgAddressList();
			foreach (OrgAddress address in this)
			{
				if (!address.IsDeleted && (!activeAddressesOnly || address.OA_IsActive))
				{
					foreach (OrgAddressCapabilityWrapper oAC in address.AddressCapability)
					{
						if (oAC.Enabled)
						{
							OrgAddressType type = OrgAddressType.Find(oAC.AddressCapabilityType);
							if (type != null && type.Category == category)
							{
								list.Add(address);
							}
						}
					}
				}
			}
			return list;
		}

		public void SwapMainAddress(OrgAddress oldMain, OrgAddress newMain)
		{
			bool changed = false;
			if (oldMain != newMain)
			{
				foreach (OrgAddressCapabilityWrapper oAC in oldMain.AddressCapability)
				{
					if (oldMain.AddressCapability.GetCapabilityEnabled(OrgConstants.AddressType.Office) && oldMain.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Office))
					{
						oAC.SetMainSilently(false);
						changed = true;
					}
				}
				if (changed)
				{
					MainAddressCache = null;
					if (ParentOrg != null)
					{
						if (ParentOrg.MainAddressCollection.Contains(oldMain))
						{
							ParentOrg.MainAddressCollection.Remove(oldMain);
						}
						ParentOrg.MainAddressCollection.Add(newMain);

						if (!ParentOrg.OH_RL_NKClosestPort.IsEmpty && !newMain.OA_RL_NKRelatedPortCode.IsEmpty && ParentOrg.OH_RL_NKClosestPort != newMain.OA_RL_NKRelatedPortCode)
						{
							ParentOrg.OH_RL_NKClosestPort = newMain.OA_RL_NKRelatedPortCode;
						}
					}
				}
				oldMain.MarkAsNeedingValidation();
			}
		}

		public ZBool ContainsAddressType(OrgAddressType type)
		{
			return ContainsAddressType(type.Code);
		}

		public ZBool ContainsAddressType(ZString type)
		{
			return AddressesOfType(type).Count > 0;
		}

		public ZBool CanBeSetAsDefaultAddress(ZString type)
		{
			ZBool result = type != OrgConstants.AddressType.Miscellaneous
				&& IsUniqueAddressType(type)
				&& !IsOtherDependentTypeDefaulted(OrgAddressType.Find(type));
			return result;
		}

		internal OrgAddress MainAddressDontCreate
		{
			get
			{
				LastMainAddressDontCreate = DefaultAddressOfType(OrgAddressType.Office, activeOnly: false);
				return LastMainAddressDontCreate;
			}
		}

		public OrgAddress CustomsAddress
		{
			get { return DefaultAddressOfType(OrgAddressType.CustomsAddressOfRecord, activeOnly: true); }
		}

		public OrgAddress MainAddress
		{
			get
			{
				LastMainAddress = null;
				LastMainAddressDontCreate = null;
				LastMainAddressCachedValue = null;
				LastMainAddressCached = MainAddressCache is not null;

				if (MainAddressCache == null)
				{
					MainAddressCache = new CachedProperty<OrgAddress>(Factory, new GetValueDelegate<OrgAddress>(GetMainAddressCache));
				}
				else
				{
					LastMainAddressCachedValue = MainAddressCache.Value;
				}

				LastMainAddress = MainAddressCache != null ? MainAddressCache.Value : null;
				return LastMainAddress;
			}
		}
		CachedProperty<OrgAddress> MainAddressCache;

		OrgAddress GetMainAddress(string preferredLanguage)
		{
			if (string.IsNullOrEmpty(preferredLanguage) || preferredLanguage == Res.DefaultLanguage)
			{
				return MainAddress;
			}
			else
			{
				return DefaultAddressOfType(OrgAddressType.Office, false, preferredLanguage);
			}
		}

		OrgAddress GetMainAddressCache()
		{
			OrgAddress address = MainAddressDontCreate;
			// If no address, create an OFC address so we always have a MainAddress, unless we are cycled
			if (address == null && !inGetMainAddressCache)
			{
				inGetMainAddressCache = true;
				try
				{
					address = Master.CanAddNewAddress ? AddNewMainAddress() : NullAddress;
				}
				finally
				{
					inGetMainAddressCache = false;
				}
			}
			return address;
		}

		bool inGetMainAddressCache;

		OrgAddress NullAddress
		{
			get { return (OrgAddress)Factory.GetNull(typeof(OrgAddress)); }
		}

		public ZBool HasMailingAddress
		{
			get { return AddressesOfCategory(AddressCategory.Mailing).Count > 0; }
		}

		public ZBool HasPhysicalAddress
		{
			get { return AddressesOfCategory(AddressCategory.Physical).Count > 0; }
		}

		public OrgAddress GetAddressWithMainAddressFallback(ZString relatedPort, OrgAddressType addressType)
			=> this.Cast<OrgAddress>().FirstOrDefault(orgAddress => orgAddress.OA_IsActive
																&& orgAddress.AddressCapability.GetCapabilityEnabled(addressType)
																&& orgAddress.OA_RL_NKRelatedPortCode == relatedPort)
				?? this.Cast<OrgAddress>().FirstOrDefault(orgAddress => orgAddress.OA_IsActive
																&& orgAddress.AddressCapability.GetCapabilityEnabled(addressType)
																&& orgAddress.IsMainAddressOfType(addressType));

		#endregion

		#region Security

		protected override bool AllowNewCore
		{
			get { return ParentOrg == null || ParentOrg.SecurityProvider.HasModifyAddressListSecurity; }
		}

		protected override bool AllowRemoveCore
		{
			get { return ParentOrg == null || ParentOrg.SecurityProvider.HasModifyAddressListSecurity; }
		}

		#endregion

		#region Document Addresses

		public OrgAddress AddressForDocument(IStmMenuItem menuItem, ZString notifyMode, bool useCommonLanguage, string preferredLanguage = null)
		{
			OrgAddress result = GetMainAddress(preferredLanguage);
			if (menuItem != null)
			{
				switch (menuItem.SU_AddressCategory)
				{
					case OrgAddressCategory.Codes.Commercial:
						result = AddressForCommercialDoc(menuItem.SU_ContactType, useCommonLanguage, preferredLanguage);
						break;

					case OrgAddressCategory.Codes.Receivables:
						result = AddressForReceivableDoc(notifyMode, useCommonLanguage, preferredLanguage);
						break;

					case OrgAddressCategory.Codes.Payables:
						result = AddressForPayableDoc(notifyMode, useCommonLanguage, preferredLanguage);
						break;

					case OrgAddressCategory.Codes.Sales:
						result = AddressForSalesDoc(notifyMode, useCommonLanguage, preferredLanguage);
						break;

					case OrgAddressCategory.Codes.Office:
						result = AddressForClientFacingDoc(notifyMode, useCommonLanguage, preferredLanguage);
						break;

					case OrgAddressCategory.Codes.Transport:
						result = AddressForTransportDoc(useCommonLanguage, preferredLanguage);
						break;

					default:
						break;
				}
			}
			return result;
		}

		/// <summary>
		/// Return default address for commercial documents.
		/// Fallback rules are:
		/// PIC/DLV -> PAD -> MainAddress (default OFC)
		/// </summary>
		/// <param name="ContactType"></param>
		/// <returns></returns>
		OrgAddress AddressForCommercialDoc(ZString contact, bool useCommonLanguage, string preferredLanguage)
		{
			OrgAddress result;
			if (contact == ContactType.Consignee.Code)
			{
				result = DefaultAddressOfType(OrgAddressType.Delivery, useCommonLanguage, preferredLanguage);
			}
			else if (contact == ContactType.Consignor.Code)
			{
				result = DefaultAddressOfType(OrgAddressType.Pickup, useCommonLanguage, preferredLanguage);
			}
			else
			{
				result = null;
			}
			if (result == null)
			{
				result = DefaultAddressOfType(OrgAddressType.PickupAndDelivery, useCommonLanguage, preferredLanguage);
			}
			if (result == null)
			{
				result = GetMainAddress(preferredLanguage);
			}
			return result;
		}

		/// <summary>
		/// Return default address for receivables documents.
		/// Fallback rules are:
		/// for printed docs - ARM -> PST -> MainAddress (default OFC)
		/// for fax/email    - ARM -> MainAddress (default OFC)
		/// </summary>
		/// <returns></returns>
		OrgAddress AddressForReceivableDoc(ZString notifyMode, bool useCommonLanguage, string preferredLanguage)
		{
			OrgAddress result = null;

			result = DefaultAddressOfType(OrgAddressType.Receivables, useCommonLanguage, preferredLanguage);

			if (result == null)
			{
				result = FallbackForOfficeAndPostalAddress(notifyMode, useCommonLanguage, preferredLanguage);
			}
			return result;
		}

		/// <summary>
		/// Return default address for payables documents.
		/// Fallback rules are:
		/// for printed docs - APM -> PST -> MainAddress (default OFC)
		/// for fax/email    - APM -> MainAddress (default OFC)
		/// </summary>
		/// <returns></returns>
		OrgAddress AddressForPayableDoc(ZString notifyMode, bool useCommonLanguage, string preferredLanguage)
			=> DefaultAddressOfType(OrgAddressType.Payables, useCommonLanguage, preferredLanguage)
				?? FallbackForOfficeAndPostalAddress(notifyMode, useCommonLanguage, preferredLanguage);

		/// <summary>
		/// Return default address for sales documents.
		/// Fallback rules are:
		/// for printed docs - SQM -> PST -> MainAddress (default OFC)
		/// for fax/email    - SQM -> MainAddress (default OFC)
		/// </summary>
		/// <returns></returns>
		OrgAddress AddressForSalesDoc(ZString notifyMode, bool useCommonLanguage, string preferredLanguage)
			=> DefaultAddressOfType(OrgAddressType.Sales, useCommonLanguage, preferredLanguage)
				?? FallbackForOfficeAndPostalAddress(notifyMode, useCommonLanguage, preferredLanguage);

		/// <summary>
		/// Return default address for forwarder client facing documents.
		/// Fallback rules are:
		/// MainAddress (default OFC)
		/// </summary>
		/// <returns></returns>
		OrgAddress AddressForClientFacingDoc(ZString notifyMode, bool useCommonLanguage, string preferredLanguage)
		{
			return FallbackForOfficeAndPostalAddress(notifyMode, useCommonLanguage, preferredLanguage);
		}

		#region Local Transport Documents

		/// <summary>
		/// Return default address for local transport and warehousing documents.
		/// Fallback rules are:
		/// PAD -> PIC -> DLV -> MainAddress (default OFC)
		/// </summary>
		/// <returns></returns>
		OrgAddress AddressForTransportDoc(bool useCommonLanguage, string preferredLanguage)
			=> DefaultAddressOfType(OrgAddressType.PickupAndDelivery, useCommonLanguage, preferredLanguage)
				?? DefaultAddressOfType(OrgAddressType.Pickup, useCommonLanguage, preferredLanguage)
				?? DefaultAddressOfType(OrgAddressType.Delivery, useCommonLanguage, preferredLanguage)
				?? GetMainAddress(preferredLanguage);

		#endregion

		#endregion

		#region Implementation

		OrgAddress FallbackForOfficeAndPostalAddress(ZString notifyMode, bool useCommonLanguage, string preferredLanguage)
		{
			OrgAddress result = null;

			if (notifyMode == Constants.ContactNotifyModes.Print)
			{
				result = DefaultAddressOfType(OrgAddressType.Postal, useCommonLanguage, preferredLanguage);
			}

			if (result == null)
			{
				result = GetMainAddress(preferredLanguage);
			}

			return result;
		}

		ZBool IsUniqueAddressType(ZString type)
		{
			return AddressesOfType(type).Count == 1;
		}

		void FireOnTriedToRemoveMainAddressEvent()
		{
			if (OnTriedToRemoveMainAddress != null)
			{
				OnTriedToRemoveMainAddress(this, EventArgs.Empty);
			}
		}

		ZBool IsOtherDependentTypeDefaulted(OrgAddressType type)
		{
			ZBool result = false;

			if (type == OrgAddressType.Pickup || type == OrgAddressType.Delivery)
			{
				OrgAddressList pADAddresses = AddressesOfType(OrgAddressType.PickupAndDelivery);
				result = HasDefaultAddress(pADAddresses);
			}
			else if (type == OrgAddressType.PickupAndDelivery)
			{
				OrgAddressList pICAddresses = AddressesOfType(OrgAddressType.Pickup);
				OrgAddressList dLVAddresses = AddressesOfType(OrgAddressType.Delivery);

				result = HasDefaultAddress(pICAddresses) || HasDefaultAddress(dLVAddresses);
			}

			return result;
		}

		ZBool HasDefaultAddress(OrgAddressList list)
		{
			bool result = false;
			foreach (OrgAddress address in list)
			{
				foreach (OrgAddressCapabilityWrapper oAC in address.AddressCapability)
				{
					if (oAC.Main)
					{
						result = true;
					}
				}
			}
			return result;
		}

		#endregion
	}
}
