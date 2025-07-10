using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecurityProfileUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrgSecurityProfileUpdater() : base()
		{
		}

		public OrgSecurityProfileUpdater(OrgHeader org) : base()
		{
			Org = org ?? throw new ArgumentNullException(nameof(org));
		}

		public OrgSecurityProfileUpdater(IEnumerable<ZGuid> orgPKs) : base()
		{
			OrgPKs = orgPKs ?? throw new ArgumentNullException(nameof(orgPKs));
		}

		#region Properties

		[List("ProfileNames")]
		public ZString ProfileName
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return profilename; }
			set
			{
				SetNonPersistentPropertyValue(ProfileNameInfo, ref profilename, value);

				if (!IsValidationSuspended)
				{
					ValidateProfileName();
				}
			}
		}

		ZString profilename;

		public ZPropertyInfo ProfileNameInfo => GetZPropertyInfo(nameof(ProfileName));

		public CodeDescriptionPairList ProfileNames
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (var profile in OrganisationRegistry.Instance.WebSecurityDefaultValues.Value.OfType<OrgSecurityProfile>())
				{
					if (profile.Default)
					{
						result.AddPair(profile.Name, Res.GetString("1b838cc4-d2a7-406d-a646-0c1b0a370c4c", "(Default)"));
					}
					else
					{
						result.AddPair(profile.Name, string.Empty);
					}
				}
				return result;
			}
		}

		public ZBool ShouldApplyGranted
		{
			get { return shouldApplyGranted; }
			set
			{
				SetNonPersistentPropertyValue(ShouldApplyGrantedInfo, ref shouldApplyGranted, value);
				if (!IsValidationSuspended)
				{
					ValidateShouldApplyGranted();
				}
			}
		}

		ZBool shouldApplyGranted;

		public ZPropertyInfo ShouldApplyGrantedInfo => GetZPropertyInfo(nameof(ShouldApplyGranted));

		public ZBool ShouldApplyIsCustomerManaged
		{
			get { return shouldApplyIsCustomerManaged; }
			set
			{
				SetNonPersistentPropertyValue(ShouldApplyIsCustomerManagedInfo, ref shouldApplyIsCustomerManaged, value);
				if (!IsValidationSuspended)
				{
					ValidateShouldApplyIsCustomerManaged();
				}
			}
		}

		ZBool shouldApplyIsCustomerManaged;

		public ZPropertyInfo ShouldApplyIsCustomerManagedInfo => GetZPropertyInfo(nameof(ShouldApplyIsCustomerManaged));

		public ZBool ShouldMaintainExistingContacts
		{
			get { return shouldMaintainExistingContacts; }
			set
			{
				SetNonPersistentPropertyValue(ShouldMaintainExistingContactsInfo, ref shouldMaintainExistingContacts, value);
				if (!IsValidationSuspended)
				{
					ValidateShouldMaintainExistingContacts();
				}
			}
		}

		ZBool shouldMaintainExistingContacts;

		public ZPropertyInfo ShouldMaintainExistingContactsInfo => GetZPropertyInfo(nameof(ShouldMaintainExistingContacts));

		public ZBool ShouldEraseExistingContacts
		{
			get { return shouldEraseExistingContacts; }
			set
			{
				SetNonPersistentPropertyValue(ShouldEraseExistingContactsInfo, ref shouldEraseExistingContacts, value);
				if (!IsValidationSuspended)
				{
					ValidateShouldMaintainExistingContacts();
				}
			}
		}

		ZBool shouldEraseExistingContacts;

		public ZPropertyInfo ShouldEraseExistingContactsInfo => GetZPropertyInfo(nameof(ShouldEraseExistingContacts));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateProfileName();
			ValidateShouldApplyGranted();
			ValidateShouldApplyIsCustomerManaged();
			ValidateShouldMaintainExistingContacts();
		}

		void ValidateProfileName()
		{
			ProfileNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ProfileNameInfo);
			ListValidation.ErrorIfInvalidCode(ProfileNameInfo, ResString.GetMultilingualString("fd4603ad-3210-4df8-a085-2bc86449dea3", "Profile Name"));
		}

		void ValidateShouldApplyGranted() => ValidateOptions(ShouldApplyGrantedInfo);

		void ValidateShouldApplyIsCustomerManaged() => ValidateOptions(ShouldApplyIsCustomerManagedInfo);

		void ValidateOptions(ZPropertyInfo propertyInfo)
		{
			propertyInfo.ClearAllNotifications();
			if (!ShouldApplyGranted && !ShouldApplyIsCustomerManaged)
			{
				propertyInfo.AddError(ResString.GetMultilingualString("f1b9279e-a2ce-4529-bfa3-cdad133c480a", "At least one option has to be selected"));
			}
		}

		void ValidateShouldMaintainExistingContacts()
		{
			ShouldMaintainExistingContactsInfo.ClearAllNotifications();
			ShouldEraseExistingContactsInfo.ClearAllNotifications();
			if (ShouldMaintainExistingContacts && ShouldEraseExistingContacts)
			{
				ShouldMaintainExistingContactsInfo.AddError(ResString.GetMultilingualString("ca8265f3-d132-4c25-9442-42acc43477d8", "Only one option can be selected at once."));
			}
		}

		OrgSecurityProfile SelectedOrgSecurityProfile => OrganisationRegistry.Instance.WebSecurityDefaultValues.Value.OfType<OrgSecurityProfile>().FirstOrDefault(x => x.Name == ProfileName);

		#endregion Properties

		readonly OrgHeader Org;
		readonly IEnumerable<ZGuid> OrgPKs;

		#region Actions

		public int Update()
		{
			var profile = SelectedOrgSecurityProfile;
			var processedCount = 0;

			if (profile != null)
			{
				if (Org != null)
				{
					Org.SecurityRights.SetOrgSecurities(profile, ShouldApplyGranted, ShouldApplyIsCustomerManaged, ShouldMaintainExistingContacts, ShouldEraseExistingContacts);
					processedCount++;
				}
				else if (OrgPKs != null)
				{
					processedCount = UpdateCore(new ZQuery(OrgHeaderSchema.PK, OrgPKs.ToArray()));
				}
				else
				{
					processedCount = UpdateCore(new ZQuery(OrgHeaderSchema.OH_IsActive, true));
				}
			}

			return processedCount;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044")]
		ZInt UpdateCore(ZQuery filter)
		{
			var profile = SelectedOrgSecurityProfile;
			var processingCount = 0;
			var processedCount = 0;
			var totalCount = 0;
			IsCancelled = false;

			if (profile != null)
			{
				var inititalFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var reader = new FilteredBusinessObjectReader<OrgHeader>(filter, inititalFactory);
				reader.BatchSize = 1;
				reader.SaveBeforeLoadNextEnabled = true;

				totalCount = reader.Factory.GetDatabaseCount(typeof(OrgHeader), filter);

				if (totalCount == 0)
				{
					return 0;
				}

				foreach (OrgHeader org in reader)
				{
					if (IsCancelled)
					{
						break;
					}

					processingCount++;
					OrgHeaderProcessing?.Invoke(this, new OrgHeaderProcessedEventArgs(Res.GetString("fd738207-306a-4c79-b98b-a6630e0d6cd7", "Processing... {0}/{1}",
						processingCount, totalCount), (int)(processingCount * 100m / totalCount)));
					org.SecurityRights.SetOrgSecurities(profile, ShouldApplyGranted, ShouldApplyIsCustomerManaged, ShouldMaintainExistingContacts, ShouldEraseExistingContacts);

					processedCount++;
				}
			}

			return processedCount;
		}

		public void Cancel() => IsCancelled = true;

		bool IsCancelled;

		public event EventHandler<OrgHeaderProcessedEventArgs> OrgHeaderProcessing;

		public class OrgHeaderProcessedEventArgs : EventArgs
		{
			public string Status { get; private set; }
			public int PercentComplete { get; private set; }

			public OrgHeaderProcessedEventArgs(string status, int percentComplete)
			{
				Status = status;
				PercentComplete = percentComplete;
			}
		}

		#endregion Actions
	}
}
