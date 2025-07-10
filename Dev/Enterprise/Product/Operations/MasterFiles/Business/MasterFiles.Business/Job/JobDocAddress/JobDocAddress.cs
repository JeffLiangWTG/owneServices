using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.DocumentEngineIntegration.DocumentParsing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.ManifestBase;
using Semaphore = CargoWise.EntityFramework.Semaphore;

namespace Enterprise.MasterFiles.Business
{
	[ActionFieldFollow(true)]
	[CodeProperty(JobDocAddress.Schema.E2_CompanyName), DescriptionProperty(JobDocAddress.Schema.E2_CompanyName)]
	[UniversalCopyWithExtendedEntities]

	public class JobDocAddress : AutoJobDocAddress, IJobDocAddress, IDocAddress, IScreeningPartyProvider, IZAddress, ISupportWebAddressValidation, ILocation, IManifestBillAddress, IDataVersionLoggingSupported
	{
		#region Schema

		public new class Schema : AutoJobDocAddress.Schema
		{
			public const string AddressCaption = "AddressCaption";
			public const string E2_PassportDetails = "E2_PassportDetails";
			public const string E2_PassportCountryOfIssue = "E2_PassportCountryOfIssue";
			public const string E2_PassportDateOfBirth = "E2_PassportDateOfBirth";
			public const string E2_PassportID = "E2_PassportID";

			public const string E2_Fax_Formatted = "E2_Fax_Formatted";
			public const string E2_Fax_FormattedLocalNumberIfLoggedInSameCountry = "E2_Fax_FormattedLocalNumberIfLoggedInSameCountry";
			public const string E2_Fax_IsManuallyVerified = "E2_Fax_IsManuallyVerified";
			public const string E2_Mobile_Formatted = "E2_Mobile_Formatted";
			public const string E2_Mobile_FormattedLocalNumberIfLoggedInSameCountry = "E2_Mobile_FormattedLocalNumberIfLoggedInSameCountry";
			public const string E2_Mobile_IsManuallyVerified = "E2_Mobile_IsManuallyVerified";
			public const string E2_Phone_Formatted = "E2_Phone_Formatted";
			public const string E2_Phone_FormattedLocalNumberIfLoggedInSameCountry = "E2_Phone_FormattedLocalNumberIfLoggedInSameCountry";
			public const string E2_Phone_IsManuallyVerified = "E2_Phone_IsManuallyVerified";

			public const int E2_PassportCountryOfIssueMaxLength = 2;
			public const int E2_PassportIDMaxLength = 9;
			public const int E2_CompanyNameTruncatedLength = 50;
		}

		#endregion

		#region IAmsBillAddress implementation
		ZPropertyInfo IManifestBillAddress.OA_AddressInfo
		{
			get { return E2_OA_AddressInfo; }
		}
		#endregion

		#region Spike

		#region SynchroniseWithParent

		public bool SynchroniseWithParentWithDetection(JobDocAddress parentDocAddress, bool detectChages, bool force = true)
		{
			try
			{
				detectChangeEnabled = detectChages;
				syncChangeDetected = false;
				SynchroniseWithParent(parentDocAddress, force);
			}
			finally
			{
				detectChangeEnabled = false;
			}
			return syncChangeDetected;
		}
		bool syncChangeDetected { get; set; }
		bool detectChangeEnabled { get; set; }

		public void SynchroniseWithParent(JobDocAddress parentDocAddress, bool force = true)
		{
			if (parentDocAddress != null
				&& !(IsDeleted || parentDocAddress.IsDeleted)
				&& parentDocAddress.PK != PK
				&& (parentDocAddress != this.ParentDocAddress
					|| (force && !IsSynchronisedWithParentDocAddress)))
			{
				if (HookedEvents)
				{
					UnHookChangeEventsInParentDocAddress();
				}
				this.ParentDocAddress = parentDocAddress;
				if (force)
				{
					SetActualFieldValuesFromParent();
				}
				else
				{
					IsSynchronisedWithParentDocAddress = false;
				}

				if (!detectChangeEnabled)
				{
					HookChangeEventsInParentDocAddress();
				}
			}
		}

		public bool HasLinkedDocAddress
		{
			get { return HookedEvents; }
		}

		public void DeSynchroniseWithParent()
		{
			if (!IsSettingHasChangesSuspended && ParentDocAddress != null)
			{
				this.HasChanges = true;
			}

			if (HookedEvents)
			{
				UnHookChangeEventsInParentDocAddress();
			}

			this.ParentDocAddress = null;
			IsSynchronisedWithParentDocAddress = false;
		}

		JobDocAddress ParentDocAddress
		{
			get { return (fParentDocAddress == null || fParentDocAddress.IsDeleted) ? null : fParentDocAddress; }
			set { fParentDocAddress = value; }
		}
		JobDocAddress fParentDocAddress;

		#endregion

		#region HookChangeEventsInParentDocAddress

		void HookChangeEventsInParentDocAddress()
		{
			if (ParentDocAddress != null)
			{
				ParentDocAddress.E2_AddressOverrideInfo.ValueChanged += new EventHandler(ParentDocAddressE2_AddressOverrideInfo_ValueChanged);

				ParentDocAddress.OrganisationPKInfo.ValueChanged += new EventHandler(ParentDocAddressOrganisationPKInfo_ValueChanged);
				ParentDocAddress.E2_OA_AddressInfo.ValueChanged += new EventHandler(ParentDocAddressE2_OA_AddressInfo_ValueChanged);

				ParentDocAddress.E2_CompanyNameInfo.ValueChanged += new EventHandler(ParentDocAddressE2_CompanyNameInfo_ValueChanged);
				ParentDocAddress.E2_AdditionalAddressInformationInfo.ValueChanged += new EventHandler(ParentDocAddressE2_AdditionalAddressInformationInfo_ValueChanged);
				ParentDocAddress.E2_Address1Info.ValueChanged += new EventHandler(ParentDocAddressE2_Address1Info_ValueChanged);
				ParentDocAddress.E2_Address2Info.ValueChanged += new EventHandler(ParentDocAddressE2_Address2Info_ValueChanged);
				ParentDocAddress.E2_CityInfo.ValueChanged += new EventHandler(ParentDocAddressE2_CityInfo_ValueChanged);
				ParentDocAddress.E2_StateInfo.ValueChanged += new EventHandler(ParentDocAddressE2_StateInfo_ValueChanged);
				ParentDocAddress.E2_PostcodeInfo.ValueChanged += new EventHandler(ParentDocAddressE2_PostcodeInfo_ValueChanged);
				ParentDocAddress.E2_RN_NKCountryCodeInfo.ValueChanged += new EventHandler(ParentDocAddressE2_RN_NKCountryCodeInfo_ValueChanged);
				ParentDocAddress.E2_ContactInfo.ValueChanged += new EventHandler(ParentDocAddressE2_ContactInfo_ValueChanged);
				ParentDocAddress.E2_PhoneInfo.ValueChanged += new EventHandler(ParentDocAddressE2_PhoneInfo_ValueChanged);
				ParentDocAddress.E2_FaxInfo.ValueChanged += new EventHandler(ParentDocAddressE2_FaxInfo_ValueChanged);
				ParentDocAddress.E2_EmailInfo.ValueChanged += new EventHandler(ParentDocAddressE2_EmailInfo_ValueChanged);
				ParentDocAddress.E2_MobileInfo.ValueChanged += new EventHandler(ParentDocAddressE2_MobileInfo_ValueChanged);

				HookedEvents = true;
			}
		}

		#endregion

		#region UnHookChangeEventsInParentDocAddress

		void UnHookChangeEventsInParentDocAddress()
		{
			if (ParentDocAddress != null)
			{
				ParentDocAddress.E2_AddressOverrideInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_AddressOverrideInfo_ValueChanged);

				ParentDocAddress.OrganisationPKInfo.ValueChanged -= new EventHandler(ParentDocAddressOrganisationPKInfo_ValueChanged);
				ParentDocAddress.E2_OA_AddressInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_OA_AddressInfo_ValueChanged);

				ParentDocAddress.E2_CompanyNameInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_CompanyNameInfo_ValueChanged);
				ParentDocAddress.E2_AdditionalAddressInformationInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_AdditionalAddressInformationInfo_ValueChanged);
				ParentDocAddress.E2_Address1Info.ValueChanged -= new EventHandler(ParentDocAddressE2_Address1Info_ValueChanged);
				ParentDocAddress.E2_Address2Info.ValueChanged -= new EventHandler(ParentDocAddressE2_Address2Info_ValueChanged);
				ParentDocAddress.E2_CityInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_CityInfo_ValueChanged);
				ParentDocAddress.E2_StateInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_StateInfo_ValueChanged);
				ParentDocAddress.E2_PostcodeInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_PostcodeInfo_ValueChanged);
				ParentDocAddress.E2_RN_NKCountryCodeInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_RN_NKCountryCodeInfo_ValueChanged);
				ParentDocAddress.E2_ContactInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_ContactInfo_ValueChanged);
				ParentDocAddress.E2_PhoneInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_PhoneInfo_ValueChanged);
				ParentDocAddress.E2_FaxInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_FaxInfo_ValueChanged);
				ParentDocAddress.E2_EmailInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_EmailInfo_ValueChanged);
				ParentDocAddress.E2_MobileInfo.ValueChanged -= new EventHandler(ParentDocAddressE2_MobileInfo_ValueChanged);

				ParentDocAddress = null;
				IsSynchronisedWithParentDocAddress = false;
			}

			HookedEvents = false;
		}

		#endregion

		void CheckSynchronisationWithParentDocAddress()
		{
			if (ParentDocAddress != null && !IsSynchronisingWithParentDocAddress)
			{
				if (HasExactSameValuesAsDocAddressParent && !HookedEvents)
				{
					HookChangeEventsInParentDocAddress();
				}
				else if (HookedEvents)
				{
					UnHookChangeEventsInParentDocAddress();
				}
			}
		}

		bool IsSynchronisingWithParentDocAddress;
		bool IsSynchronisedWithParentDocAddress;
		bool HookedEvents;

		#region Events

		#region FieldUpdateSynchroniser

		class FieldUpdateSynchroniser : IDisposable
		{
			public FieldUpdateSynchroniser(JobDocAddress docAddress)
			{
				this.DocAddress = docAddress;
				docAddress.IsSynchronisingWithParentDocAddress = true;
			}

			readonly JobDocAddress DocAddress;

			public void Dispose()
			{
				DocAddress.IsSynchronisingWithParentDocAddress = false;
			}
		}

		#endregion

		void ParentDocAddressE2_AddressOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ParentDocAddress != null)
			{
				using (FieldUpdateSynchroniser synchroniser = new FieldUpdateSynchroniser(this))
				{
					E2_AddressOverride = ParentDocAddress.E2_AddressOverride;
				}
			}
		}

		void ParentDocAddressOrganisationPKInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ParentDocAddress != null)
			{
				using (FieldUpdateSynchroniser synchroniser = new FieldUpdateSynchroniser(this))
				{
					OrganisationPK = ParentDocAddress.OrganisationPK;
				}
			}
		}

		void UpdateDocAddressFromParent(ZPropertyInfo info)
		{
			if (ParentDocAddress != null && info != null)
			{
				using (FieldUpdateSynchroniser synchroniser = new FieldUpdateSynchroniser(this))
				{
					info.Value = (IZType)ParentDocAddress[info.Name];
				}
			}
		}

		void ParentDocAddressE2_OA_AddressInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_OA_AddressInfo);
		}

		void ParentDocAddressE2_CompanyNameInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_CompanyNameInfo);
		}

		void ParentDocAddressE2_AdditionalAddressInformationInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_AdditionalAddressInformationInfo);
		}

		void ParentDocAddressE2_Address1Info_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_Address1Info);
		}

		void ParentDocAddressE2_Address2Info_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_Address2Info);
		}

		void ParentDocAddressE2_CityInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_CityInfo);
		}

		void ParentDocAddressE2_StateInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_StateInfo);
		}

		void ParentDocAddressE2_PostcodeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_PostcodeInfo);
		}

		void ParentDocAddressE2_RN_NKCountryCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_RN_NKCountryCodeInfo);
		}

		void ParentDocAddressE2_ContactInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_ContactInfo);
		}

		void ParentDocAddressE2_PhoneInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_PhoneInfo);
		}

		void ParentDocAddressE2_FaxInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_FaxInfo);
		}

		void ParentDocAddressE2_EmailInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_EmailInfo);
		}

		void ParentDocAddressE2_MobileInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateDocAddressFromParent(E2_MobileInfo);
		}

		public void SetActualFieldValuesFromParent(JobDocAddress parentDocAddress)
		{
			JobDocAddress savedParentDocAddress = this.ParentDocAddress;
			ParentDocAddress = parentDocAddress;
			try
			{
				SetActualFieldValuesFromParent();
			}
			finally
			{
				this.ParentDocAddress = savedParentDocAddress;
			}
		}

		public void SetActualFieldValuesFromParent()
		{
			if (ParentDocAddress != null)
			{
				using (FieldUpdateSynchroniser synchroniser = new FieldUpdateSynchroniser(this))
				{
					using (SuspendRefreshingParent())
					{
						if (detectChangeEnabled)
						{
							syncChangeDetected = E2_AddressOverride != ParentDocAddress.E2_AddressOverride;
							if (!syncChangeDetected)
							{
								if (E2_AddressOverride)
								{
									syncChangeDetected = (!E2_CompanyName.EqualsIgnoringCase(ParentDocAddress.E2_CompanyName) ||
										!E2_AdditionalAddressInformation.EqualsIgnoringCase(ParentDocAddress.E2_AdditionalAddressInformation) ||
										!E2_Address1.EqualsIgnoringCase(ParentDocAddress.E2_Address1) ||
										!E2_Address2.EqualsIgnoringCase(ParentDocAddress.E2_Address2) ||
										!E2_City.EqualsIgnoringCase(ParentDocAddress.E2_City) ||
										!E2_State.EqualsIgnoringCase(ParentDocAddress.E2_State) ||
										!E2_Postcode.EqualsIgnoringCase(ParentDocAddress.E2_Postcode) ||
										!E2_RN_NKCountryCode.EqualsIgnoringCase(ParentDocAddress.E2_RN_NKCountryCode) ||
										!E2_Contact.EqualsIgnoringCase(ParentDocAddress.E2_Contact) ||
										!E2_Phone.EqualsIgnoringCase(ParentDocAddress.E2_Phone) ||
										!E2_Fax.EqualsIgnoringCase(ParentDocAddress.E2_Fax) ||
										!E2_Email.EqualsIgnoringCase(ParentDocAddress.E2_Email) ||
										!E2_Mobile.EqualsIgnoringCase(ParentDocAddress.E2_Mobile));
								}
								else
								{
									syncChangeDetected = E2_OA_Address != ParentDocAddress.E2_OA_Address;
								}
							}
							if (syncChangeDetected)
							{
								return;
							}
						}
						else
						{
							E2_AddressOverride = ParentDocAddress.E2_AddressOverride;
							if (!E2_AddressOverride)
							{
								if (E2_OA_Address != ParentDocAddress.E2_OA_Address)
								{
									OrganisationPK = ZGuid.Empty;
									E2_OA_Address = ParentDocAddress.E2_OA_Address;
								}
								E2_Contact = ParentDocAddress.E2_Contact;
							}
							else
							{
								E2_CompanyName = ParentDocAddress.E2_CompanyName;
								E2_AdditionalAddressInformation = ParentDocAddress.E2_AdditionalAddressInformation;
								E2_Address1 = ParentDocAddress.E2_Address1;
								E2_Address2 = ParentDocAddress.E2_Address2;
								E2_City = ParentDocAddress.E2_City;
								E2_State = ParentDocAddress.E2_State;
								E2_Postcode = ParentDocAddress.E2_Postcode;

								E2_RN_NKCountryCode = ParentDocAddress.E2_RN_NKCountryCode;

								E2_Contact = ParentDocAddress.E2_Contact;
								E2_Phone = ParentDocAddress.E2_Phone;
								E2_Fax = ParentDocAddress.E2_Fax;
								E2_Email = ParentDocAddress.E2_Email;
								E2_Mobile = ParentDocAddress.E2_Mobile;
							}
						}
					}
					RefreshParent();
				}
				IsSynchronisedWithParentDocAddress = true;
			}
		}

		#endregion

		bool HasExactSameValuesAsDocAddressParent
		{
			get
			{
				bool result = false;
				if (ParentDocAddress != null && E2_AddressOverride == ParentDocAddress.E2_AddressOverride)
				{
					if (!E2_AddressOverride)
					{
						result = (E2_OA_Address == ParentDocAddress.E2_OA_Address);
					}
					else
					{
						result =
							E2_AdditionalAddressInformation == ParentDocAddress.E2_AdditionalAddressInformation &&
							E2_Address1 == ParentDocAddress.E2_Address1 &&
							E2_Address2 == ParentDocAddress.E2_Address2 &&
							E2_City == ParentDocAddress.E2_City &&
							E2_State == ParentDocAddress.E2_State &&
							E2_Postcode == ParentDocAddress.E2_Postcode &&
							E2_RN_NKCountryCode == ParentDocAddress.E2_RN_NKCountryCode &&

							E2_CompanyName == ParentDocAddress.E2_CompanyName &&

							E2_Contact == ParentDocAddress.E2_Contact &&
							E2_Phone == ParentDocAddress.E2_Phone &&
							E2_Fax == ParentDocAddress.E2_Fax &&
							E2_Email == ParentDocAddress.E2_Email &&
							E2_Mobile == ParentDocAddress.E2_Mobile &&
							E2_GovRegNum == ParentDocAddress.E2_GovRegNum &&
							E2_GovRegNumType == ParentDocAddress.E2_GovRegNumType;
					}
					//result &= (E2_AddressType == ParentDocAddress.E2_AddressType);
				}

				return result;
			}
		}

		#endregion

		public JobDocAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(ResetParentScreeningStatus);
			phoneNumberPropertyHelperThunk = new Lazy<PhoneNumberPropertyHelper>(() => new PhoneNumberPropertyHelper(() => DefaultCountryCodeForPhoneNumbers));
			this.GetDefaultCountryCodeIfEmpty = delegate
			{ return Env.CurrentCompany.Country.Code; };
		}

		public string DocAddressCollectionInfo
		{
			get
			{
				var result = string.Empty;
				var docAddresses = Parent?.DocAddresses;
				result += (NoResString)"\r\nDocAddresses.Master.PK: " + docAddresses?.Master?.PK;
				result += "\r\nDocAddresses.HasChanges: " + docAddresses?.HasChanges;
				result += (NoResString)"\r\nDocAddresses Contents: " + string.Join((NoResString)", ", docAddresses?.Select(d => d.PK) ?? Array.Empty<ZGuid>());

				return result;
			}
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(JobDocAddress jobDocAddress) : base(jobDocAddress) { }

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(typeof(OrgAddress), BusinessObject.E2_OA_Address);
				Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				AddJobDocAddressNumberFetchHintIfRequired(Factory, BusinessObject);
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				AddJobDocAddressNumberFetchHintIfRequired(Factory, BusinessObject);
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
			}

			protected override void FetchForDeleteCore()
			{
				base.FetchForDeleteCore();
				Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(JobDocumentExclusionSchema.JDE_E2_Address, BusinessObject.PK);
				AddJobDocAddressNumberFetchHintIfRequired(Factory, BusinessObject);
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				foreach (var column in columns)
				{
					if (column.ColumnName == JobDocAddress.Schema.E2_Fax_IsManuallyVerified
						|| column.ColumnName == JobDocAddress.Schema.E2_Mobile_IsManuallyVerified
						|| column.ColumnName == JobDocAddress.Schema.E2_Phone_IsManuallyVerified)
					{
						Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
					}
				}
			}

			void AddJobDocAddressNumberFetchHintIfRequired(BusinessObjectFactory factory, JobDocAddress parent)
			{
				if (parent.SupportsDocAddressNumbers)
				{
					factory.AddFetchHint(JobDocAddressNumberSchema.E2N_E2, parent.PK);
				}
			}

			new JobDocAddress BusinessObject
			{
				get { return (JobDocAddress)base.BusinessObject; }
			}
		}

		#endregion

		BusinessObject ParentAsBusinessObject
		{
			get { return Parent as BusinessObject; }
		}

		internal Type ParentType;

		#region Static Instantiation Members

		#region New

		public static JobDocAddress New(BusinessObject parent)
		{
			return JobDocAddress.New(parent, DocAddressType.None);
		}

		public static JobDocAddress New(BusinessObject parent, DocAddressType docAddressType)
		{
			JobDocAddress result = parent.Factory.New<JobDocAddress>();
			result.ParentType = parent.GetType();
			result.InitialiseForeignKey(parent, docAddressType);
			result.RegisterAsChildOfParent(parent);

			return result;
		}

		#endregion

		#region Load

		public static JobDocAddress Load(BusinessObject parent, DocAddressType docAddressType)
		{
			return Load(parent, docAddressType, false);
		}

		public static JobDocAddress Load(BusinessObject parent, DocAddressType docAddressType, bool fetchOnlyFromLocalCache)
		{
			return LoadCore(parent, docAddressType, fetchOnlyFromLocalCache);
		}

		static JobDocAddress LoadCore(BusinessObject parent, DocAddressType docAddressType, bool fetchOnlyFromLocalCache)
		{
			ZQuery filter = new ZQuery(JobDocAddressSchema.E2_ParentID, parent.PK);
			filter.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(parent.Factory, docAddressType));
			filter.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;

			JobDocAddress result = (JobDocAddress)parent.Factory.LoadTop1(typeof(JobDocAddress), filter);

			if (result != null)
			{
				result.RegisterAsChildOfParent(parent);
			}

			return result;
		}

		#endregion

		#region GetOrCreateDocAddressFromParent

		public static JobDocAddress GetOrCreateDocAddressFromParent(BusinessObject parent, DocAddressType docAddressType)
			=> JobDocAddress.Load(parent, docAddressType, !parent.IsInDatabase)
				?? JobDocAddress.New(parent, docAddressType);

		#endregion

		#region CreateNonPersistantDocAddress

		public static JobDocAddress GetOrCreateNonPersistantDocAddress(BusinessObject parentBO, DocAddressType docAddressType, ZGuid orgAddressPK)
		{
			JobDocAddress result = JobDocAddress.Load(parentBO, docAddressType, true);

			if (result == null)
			{
				result = JobDocAddress.New(parentBO, docAddressType);
				result.MakeNonPersistent();
			}

			using (result.SuspendSettingHasChanges())
			{
				result.E2_OA_Address = orgAddressPK;
			}

			return result;
		}

		#endregion

		#region GetFilter

		public static ZQuery GetFilter(string addressType, string parrentTableCode, int addressSequece)
		{
			var query = new ZQuery(JobDocAddressSchema.E2_AddressType, addressType);
			query.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, parrentTableCode);
			query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, addressSequece);

			return query;
		}

		#endregion

		#endregion

		#region Parent and ForeignKey Details

		public IDocAddresses Parent
		{
			get
			{
				IDocAddresses result = null;
				if (!E2_ParentID.IsEmpty && !E2_ParentTableCode.IsEmpty)
				{
					var typeOfParent = GetParentType(E2_ParentTableCode);
					if (typeOfParent != null)
					{
						result = Factory.Load(typeOfParent, E2_ParentID) as IDocAddresses;
					}
				}

				return result;
			}
		}

		void InitialiseForeignKey(BusinessObject bizO, DocAddressType docAddressType)
		{
			using (SuspendSettingHasChanges())
			using (GetValidationSuspender())
			{
				E2_ParentID = bizO.PK;
				E2_ParentTableCode = bizO.TablePrefix;
				DocAddressType = docAddressType;
			}
		}

		public override ZGuid E2_ParentID
		{
			get { return base.E2_ParentID; }
			set
			{
				base.E2_ParentID = value;
				if (MarkParentAsNeedingValidation)
				{
					ParentAsBusinessObject?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_ParentTableCode
		{
			get { return base.E2_ParentTableCode; }
			set
			{
				base.E2_ParentTableCode = value;
				if (MarkParentAsNeedingValidation)
				{
					ParentAsBusinessObject?.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		public bool MarkParentAsNeedingValidation;

		#region Setup And Register Child

		void RegisterAsChildOfParent(BusinessObject parent)
		{
			if (parent != null && !parent.GetType().IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				parent.RegisterEditableChildObject(this);
			}
		}

		public override bool IsSavedByFactory
		{
			get { return fIsPersistent && (IsInDatabase ? base.IsSavedByFactory : ShouldBePersisted); }
		}

		bool ShouldBePersisted
		{
			get
			{
				if (persistentEvenIfEmpty)
				{
					return true;
				}
				else if (IsEmpty)
				{
					return false;
				}
				else
				{
					return HasChanges;
				}
			}
		}

		public bool IsPersistent
		{
			get { return fIsPersistent; }
		}
		bool fIsPersistent = true;
		bool persistentEvenIfEmpty;

		public void MakeNonPersistent()
		{
			fIsPersistent = false;
			persistentEvenIfEmpty = false;
		}

		public void MakePersistentIfNotEmpty()
		{
			fIsPersistent = true;
			persistentEvenIfEmpty = false;
		}

		public void MakePersistentEvenIfEmpty()
		{
			fIsPersistent = true;
			persistentEvenIfEmpty = true;
		}

		protected override void RunPreSaveValidationCore()
		{
			if (fIsPersistent && !IsRowDeletedOrDetachedOrNull)
			{
				base.RunPreSaveValidationCore();
			}
		}

		public override bool HasChanges
		{
			get
			{
				if (fIsPersistent)
				{
					return base.HasChanges;
				}
				else
				{
					return false;
				}
			}
			set
			{
				base.HasChanges = value;
			}
		}

		protected virtual Type GetParentType(string prefix)
		{
			if (ParentType == null)
			{
				if (PrefixToTypeHash == null)
				{
					PrefixToTypeHash = new Hashtable();
					PrefixToTypeHash["JP"] = ObjectFactory.GetType<Enterprise.Integration.Freight.IJobDocsAndCartage>();
					PrefixToTypeHash["JE"] = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					PrefixToTypeHash["BM"] = ObjectFactory.GetType<Enterprise.Integration.Customs.ICusInBondMoveHeader>();
					PrefixToTypeHash["JS"] = ObjectFactory.GetType<Enterprise.Integration.Freight.ICommonShipment>();
					PrefixToTypeHash["JJ"] = ObjectFactory.GetType<Freight.LocalCartage.Integration.ICommonCartage>();
					PrefixToTypeHash["JU"] = ObjectFactory.GetType<Freight.LocalCartage.Integration.ICommonCartageLeg>();
					PrefixToTypeHash["JK"] = ObjectFactory.GetType<Forwarding.IForwardingConsol>();
					PrefixToTypeHash["WD"] = ObjectFactory.GetType<Warehouse.Integration.IWhsDocket>();
					PrefixToTypeHash["JZ"] = ObjectFactory.GetType<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
				}

				string actualPrefix = (!PrefixToTypeHash.Contains(prefix)) ? "JS" : prefix;
				ParentType = (Type)PrefixToTypeHash[actualPrefix];
			}
			return ParentType;
		}

		protected internal Hashtable PrefixToTypeHash;

		#endregion

		#region E2_AddressOverride

		[ReadOnlyMember(nameof(E2_AddressOverrideReadOnly))]
		public override ZBool E2_AddressOverride
		{
			get
			{
				if (this.IsRowDeletedOrDetachedOrNull)
				{
					return ZBool.False;
				}
				return base.E2_AddressOverride;
			}
			set
			{
				if (this.IsDeleted)
				{ return; }

				if (value != base.E2_AddressOverride)
				{
					if (value)
					{
						SavedContactOnOverride = E2_Contact;
						SetOverrideContactDetailsFromOrgContact();
						SetOverrideAddressDetailsFromOrgAddress();
						SetOverrideGovernmentRegistrationNumberFromHost();
						using (ShouldActivateMISCOrganisation())
						{
							EnsureMISCOrganisationIsActive(Organisation);
						}
					}

					ZString originalCompanyName = base.E2_CompanyName;
					ZGuid originalOH = OrganisationPK;
					base.E2_AddressOverride = value;
					if (base.E2_AddressOverride)
					{
						PopulateOverrideTickedThroughRequirement(originalOH);
					}
					else
					{
						PopulateOverrideUntickedThroughRequirement(originalCompanyName);
					}

					if (value)
					{
						SavedOAOnOverride = base.E2_OA_Address;
						SavedOHOnOverride = OrganisationPK;
						E2_OA_Address = MiscAddressPK;
						E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
						SetNonPersistentPropertyValue(OrganisationPKInfo, ref fOrganisationPK, ZGuid.Empty);
						((ILightValidationInternals)this).IsValid = false;
						if (string.IsNullOrEmpty(E2_RN_NKCountryCode))
						{
							E2_RN_NKCountryCode = GetDefaultCountryCodeIfEmpty();
						}
					}
					else
					{
						ClearAddressOverrides();
						SetNonPersistentPropertyValue(OrganisationPKInfo, ref fOrganisationPK, SavedOHOnOverride);
						E2_OA_Address = SavedOAOnOverride;
						E2_ValidationStatus = AddressValidationStatus.NotRequired;
						E2_Contact = SavedContactOnOverride;
						((ILightValidationInternals)this).IsValid = true;
					}
					OrganisationPKIncludesMiscOrgInfo.RefreshBinding(originalOH);
				}

				CheckSynchronisationWithParentDocAddress();
				RefreshParent();

				// Refresh the bindings of the wrapped numbers so they are displayed correctly on the interface. Cannot test in this class.
				PhoneNumber.Refresh();
				MobilePhoneNumber.Refresh();
				FaxNumber.Refresh();
			}
		}

		public Func<string> GetDefaultCountryCodeIfEmpty;

		bool E2_AddressOverrideReadOnly
		{
			get { return ReadOnlyStrategy?.OrganisationPKReadOnly ?? false; }
		}

		public override ZPropertyInfo E2_AddressOverrideInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_AddressOverrideInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public bool CanOverride => Requirement?.CanOverride ?? true;

		ZGuid MiscAddressPK
		{
			get
			{
				if (miscAddressPK.IsEmpty)
				{
					miscAddressPK = OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress;
				}
				return miscAddressPK;
			}
		}

		void EnsureMISCOrganisationIsActive(IOrgHeader orgHeader)
		{
			if (orgHeader != null && !orgHeader.OH_IsActive)
			{
				orgHeader.OH_IsActive = true;
			}
		}

		[ThreadStatic]
		static ZGuid miscAddressPK;

		protected ZGuid SavedOAOnOverride;
		ZString SavedContactOnOverride;
		protected ZGuid SavedOHOnOverride;
		bool isSettingAddressOverride;

		void SetOverrideContactDetailsFromOrgContact()
		{
			if (HasRealAddress)
			{
				var contact = Contact;
				base.E2_Phone = (contact != null && !contact.OC_Phone.IsEmpty) ? contact.OC_Phone : Address.OA_Phone;
				base.E2_Fax = (contact != null && !contact.OC_Fax.IsEmpty) ? contact.OC_Fax : Address.OA_Fax;
				base.E2_Email = (contact != null && !contact.OC_Email.IsEmpty) ? contact.OC_Email : Address.OA_Email;
				base.E2_Mobile = (contact != null && !contact.OC_Mobile.IsEmpty) ? contact.OC_Mobile : Address.OA_Mobile;

				if (base.E2_Contact.IsEmpty)
				{
					base.E2_Contact = (contact == null) ? GetContactNameFromParent() : contact.OC_ContactName;
				}
			}
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "JobDocAddress GovRegNum will truncate to 35 chars, and Primary CusCode type for country should be less than 35 anyway")]
		void SetOverrideAddressDetailsFromOrgAddress()
		{
			if (HasRealAddress)
			{
				E2_CompanyName = GetCompanyName();
				CopyAddressFieldsWhenOverride();
				E2_RN_NKCountryCode = GetCountryCodeFromOriginalAddress();
				if (Address.Header != null && Address.Header.PrimaryRegistrationNumber != null)
				{
					E2_GovRegNum = Address.Header.PrimaryRegistrationNumber.Number;
					E2_GovRegNumType = Address.Header.PrimaryRegistrationNumber.NumberType;
				}
			}
		}

		void CopyAddressFieldsWhenOverride()
		{
			if (!ShouldClearAddressFieldsWhenOverride)
			{
				IsUpdatingCityTown = true;
				IsValidatingAddress = true;
				E2_AdditionalAddressInformation = Address.OA_AdditionalAddressInformation;
				E2_Address1 = Address.OA_Address1;
				E2_Address2 = Address.OA_Address2;
				E2_City = Address.OA_City;
				E2_State = Address.OA_State;
				E2_Postcode = Address.OA_PostCode;
				IsUpdatingCityTown = false;
				IsValidatingAddress = false;
			}
		}
		public bool ShouldClearAddressFieldsWhenOverride { get; set; }

		void SetOverrideGovernmentRegistrationNumberFromHost()
		{
			if (HasRealAddress)
			{
				RegistrationNumber registrationNumber = GetRegistrationNumberFromRequirement();
				E2_GovRegNum = registrationNumber.Number;
				E2_GovRegNumType = registrationNumber.NumberType;
			}
		}

		RegistrationNumber GetRegistrationNumberFromRequirement()
		{
			var result = cachedRegistrationNumber ?? GetRegistrationNumberFromRequirementCore();

			if (cachedRegistrationNumber == null && CacheRegistrationNumberFromRequirementSemaphore.IsSuspended)
			{
				cachedRegistrationNumber = result;
			}

			return result;
		}

		RegistrationNumber GetRegistrationNumberFromRequirementCore()
		{
			var result = new RegistrationNumber();

			var requirement = Requirement;
			if (requirement != null && requirement.GetRegistrationNumberResult != null)
			{
				var regNumberResult = requirement.GetRegistrationNumberResult(this);
				if (regNumberResult != null)
				{
					result.Number = regNumberResult.RegistrationNumber;
					result.NumberType = regNumberResult.RegistrationNumberType;
				}
			}
			else
			{
				var primaryRegistrationNumber = Address.Header?.PrimaryRegistrationNumber;
				if (primaryRegistrationNumber != null)
				{
					result.Number = primaryRegistrationNumber.Number;
					result.NumberType = primaryRegistrationNumber.NumberType;
				}
			}

			return result;
		}

		internal IDisposable CacheRegistrationNumberFromRequirement() =>
			new DisposableList(new IDisposable[] { new SemaphoreManager(CacheRegistrationNumberFromRequirementSemaphore), new DisposableAction(ClearRegistrationNumberCache) });

		Semaphore CacheRegistrationNumberFromRequirementSemaphore
		{
			get { return cacheRegistrationNumberFromRequirementSemaphore ?? (cacheRegistrationNumberFromRequirementSemaphore = new Semaphore()); }
		}

		void ClearRegistrationNumberCache()
		{
			if (!CacheRegistrationNumberFromRequirementSemaphore.IsSuspended)
			{
				cachedRegistrationNumber = null;
			}
		}

		Semaphore cacheRegistrationNumberFromRequirementSemaphore;
		RegistrationNumber? cachedRegistrationNumber;

		IDisposable ShouldActivateMISCOrganisation()
		{
			isSettingAddressOverride = true;
			return new MISCAddressSetter(this);
		}

		class MISCAddressSetter : IDisposable
		{
			readonly JobDocAddress jobDocAddress;

			public MISCAddressSetter(JobDocAddress jobDocAddress)
			{
				this.jobDocAddress = jobDocAddress;
			}

			public void Dispose()
			{
				jobDocAddress.isSettingAddressOverride = false;
			}
		}

		#endregion

		#region Property Overrides

		public override ZPropertyInfo E2_ScreeningStatusInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_ScreeningStatusInfo); }
		}

		[ActionField(FieldType = ActionFieldType.Hidden)]
		[UniversalCopyExtraMetadata(IsMandatory = true)]
		public override ZString E2_AddressType
		{
			get { return base.E2_AddressType; }
			set
			{
				base.E2_AddressType = value;
				if (MarkParentAsNeedingValidation)
				{
					ParentAsBusinessObject?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZPropertyInfo E2_ParentIDInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_ParentIDInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_ParentTableCodeInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_ParentTableCodeInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_AdditionalAddressInformationInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_AdditionalAddressInformationInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_AddressMapInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_AddressMapInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_AddressTypeInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_AddressTypeInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_IsResidentialInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_IsResidentialInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public ZPropertyInfo ResidentialCommercialAddressTypeInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(ResidentialCommercialAddressType));
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_SystemLastEditTimeUtcInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_SystemLastEditTimeUtcInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_SystemCreateTimeUtcInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_SystemCreateTimeUtcInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_SystemCreateUserInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_SystemCreateUserInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public override ZPropertyInfo E2_SystemLastEditUserInfo
		{
			get
			{
				ZPropertyInfo result = base.E2_SystemLastEditUserInfo;
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		[List("Lookups.Address_List")]
		[OperationActionReadOnlyMember(nameof(E2_OA_Address_OperationActionReadOnly))]
		public override ZGuid E2_OA_Address
		{
			get { return base.E2_OA_Address; }
			set
			{
				if (base.E2_OA_Address != value)
				{
					RaiseOrgAddressBeforeChange();

					// ensure DocAddress respects parent's suspension of HasChanges
					IDisposable hasChangesSuspender = (ParentAsBusinessObject != null && ParentAsBusinessObject.IsSettingHasChangesSuspended)
						? SuspendSettingHasChanges() : null;
					try
					{
						if (value != MiscAddressPK)
						{
							if (E2_AddressOverride)
							{
								ClearAddressOverrides();
								base.E2_AddressOverride = false;
								base.E2_ValidationStatus = AddressValidationStatus.NotRequired;
							}
							else
							{
								E2_AdditionalAddressInformation = ZString.Empty;
							}
						}

						ZGuid originalAddress = base.E2_OA_Address;
						if (fOrganisationPK.IsDefault && OrganisationPK.IsValid && !E2_AddressOverride && value == ZGuid.Invalid)
						{
							fOrganisationPK = OrganisationPK;
						}

						base.E2_OA_Address = value;

						if (ShouldAlwaysUpdateSecondary)
						{
							PopulateAddressThroughRequirement(originalAddress, true);
						}
					}
					finally
					{
						if (hasChangesSuspender != null)
						{
							hasChangesSuspender.Dispose();
						}
					}

					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
					if (MarkParentAsNeedingValidation)
					{
						ParentAsBusinessObject?.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool E2_OA_Address_ReadOnly
		{
			get { return E2_AddressOverride || Organisation == null || fE2_OA_Address_ReadOnly; }
			set { fE2_OA_Address_ReadOnly = value; }
		}
		bool fE2_OA_Address_ReadOnly;

		public bool E2_OA_Address_OperationActionReadOnly => E2_OA_Address_ReadOnly && Organisation == null && E2_OA_Address.IsEmpty && IsOrganisationPKReadOnly;

		/// <summary>
		/// Use this property if only we want to show the full company name.
		/// Otherwise use 'E2_CompanyNameTruncated' as in most of places we only want to show first 50 characters of a company name
		/// </summary>
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_CompanyName
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? GetCompanyName() : base.E2_CompanyName; }
			set
			{
				if (base.E2_CompanyName != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_CompanyNameInfo);
					ZString originalValue = base.E2_CompanyName;
					base.E2_CompanyName = value;
					PopulateCompanyNameThroughRequirement(originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
				}
			}
		}

		/// <summary>
		/// Use this property to diplay first 50 characters for company name.
		/// In most cases the system is best suited to show 50 characters for company name.
		/// Use the property 'E2_CompanyName' if only we want to show the full company name.
		/// </summary>
		public ZString E2_CompanyNameTruncated
		{
			get
			{
				return E2_CompanyName.Substring(0, JobDocAddress.Schema.E2_CompanyNameTruncatedLength);
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		[List("Lookups.ResidentialCommercialAddressType_List")]
		public ZString ResidentialCommercialAddressType
		{
			get
			{
				return residentialCommercialAddressType ?? (residentialCommercialAddressType = base.E2_IsResidential ? ResidentialCommercialAddressTypeList.Codes.Residential : ResidentialCommercialAddressTypeList.Codes.Commercial);
			}
			set
			{
				if (residentialCommercialAddressType != value)
				{
					residentialCommercialAddressType = value;
					base.E2_IsResidential = (residentialCommercialAddressType == ResidentialCommercialAddressTypeList.Codes.Residential);
					ResidentialCommercialAddressTypeInfo.RefreshBinding();
				}
			}
		}

		string residentialCommercialAddressType;

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_AdditionalAddressInformation
		{
			get
			{
				return ShouldApplyOrgAddressAdditionalInformation ? Address.PrimaryOrgAddressAdditionalInfoDetail : base.E2_AdditionalAddressInformation;
			}
			set
			{
				if (base.E2_AdditionalAddressInformation != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_AdditionalAddressInformationInfo);
					ZString originalValue = base.E2_AdditionalAddressInformation;
					base.E2_AdditionalAddressInformation = value;
					PopulateAddressFieldThroughRequirement(E2_AdditionalAddressInformationInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
					ResetValidationStatus(E2_AdditionalAddressInformationInfo);

					SetUnrestrictedAdditionalAddressInformation(value);
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_Address1
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_Address1 : base.E2_Address1; }
			set
			{
				if (base.E2_Address1 != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_Address1Info);
					ZString originalValue = base.E2_Address1;
					base.E2_Address1 = value;
					PopulateAddressFieldThroughRequirement(E2_Address1Info, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
					ResetValidationStatus(E2_Address1Info);
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_Address2
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_Address2 : base.E2_Address2; }
			set
			{
				if (base.E2_Address2 != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_Address2Info);
					ZString originalValue = base.E2_Address2;
					base.E2_Address2 = value;
					PopulateAddressFieldThroughRequirement(E2_Address2Info, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
					ResetValidationStatus(E2_Address2Info);
				}
			}
		}

		[MaxLength(70)]
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public virtual ZString E2_Address1AndE2_Address2
		{
			get => E2_Address1 + E2_Address2;
			set
			{
				ZString dummyValue = default;
				SetNonPersistentPropertyValue(E2_Address1AndE2_Address2Info, ref dummyValue, value);
				E2_Address1 = value.SubstringSafe(0, E2_Address1Info.MaxLength);
				E2_Address2 = value.SubstringSafe(E2_Address1Info.MaxLength, E2_Address2Info.MaxLength);
				Validation.ValidateE2_Address1AndE2_Address2();
			}
		}
		public ZPropertyInfo E2_Address1AndE2_Address2Info => GetInfoWithUpdatedHumanReadableName(GetZPropertyInfo(nameof(E2_Address1AndE2_Address2)));

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_City
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_City : base.E2_City; }
			set
			{
				if (base.E2_City != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_CityInfo);
					ZString originalValue = base.E2_City;
					base.E2_City = value;
					PopulateAddressFieldThroughRequirement(E2_CityInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();

					ResetValidationStatus(E2_CityInfo);
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		[List("Lookups.State_List")]
		public override ZString E2_State
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_State : base.E2_State; }
			set
			{
				if (base.E2_State != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_StateInfo);
					ZString originalValue = base.E2_State;
					base.E2_State = value;
					PopulateAddressFieldThroughRequirement(E2_StateInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
					ResetValidationStatus(E2_StateInfo);
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_GovRegNum
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? GetRegistrationNumberFromRequirement().Number : base.E2_GovRegNum; }
			set
			{
				if (base.E2_GovRegNum != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_GovRegNumInfo);
					ZString originalValue = base.E2_GovRegNum;
					base.E2_GovRegNum = value.Left(E2_GovRegNumInfo.MaxLength);
					PopulateAddressFieldThroughRequirement(E2_GovRegNumInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
					if (IsPassportIDGovRegNumType && !IsPassportDataUpdateInProgress)
					{
						E2_PassportIDInfo.RefreshBinding();
						E2_PassportCountryOfIssueInfo.RefreshBinding();
						E2_PassportDateOfBirthInfo.RefreshBinding();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		[List("Lookups.GovRegNumTypes")]
		public override ZString E2_GovRegNumType
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? GetRegistrationNumberFromRequirement().NumberType : base.E2_GovRegNumType; }
			set
			{
				if (base.E2_GovRegNumType != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_GovRegNumTypeInfo);
					ZString originalValue = base.E2_GovRegNumType;
					base.E2_GovRegNumType = value;
					PopulateAddressFieldThroughRequirement(E2_GovRegNumTypeInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_Postcode
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_PostCode : base.E2_Postcode; }
			set
			{
				if (base.E2_Postcode != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_PostcodeInfo);
					ZString originalValue = base.E2_Postcode;
					base.E2_Postcode = value;
					PopulateAddressFieldThroughRequirement(E2_PostcodeInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();

					ResetValidationStatus(E2_PostcodeInfo);
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		[List("Lookups.Countries")]
		public override ZString E2_RN_NKCountryCode
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? GetCountryCodeFromOriginalAddress() : base.E2_RN_NKCountryCode; }
			set
			{
				var oldCode = this.E2_RN_NKCountryCode;

				if (base.E2_RN_NKCountryCode != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_RN_NKCountryCodeInfo);
					ZString originalValue = base.E2_RN_NKCountryCode;
					base.E2_RN_NKCountryCode = value;
					PopulateAddressFieldThroughRequirement(E2_RN_NKCountryCodeInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
					RaiseCountryCodeChanged(oldCode);

					if (!IsValidationSuspended)
					{
						Validation.ValidateE2_State(); // Run country-dependant validation
						E2_StateInfo.RefreshBinding();
					}
					ResetValidationStatus(E2_RN_NKCountryCodeInfo);
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_AddressMap
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_AddressMap : base.E2_AddressMap; }
			set
			{
				if (base.E2_AddressMap != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_AddressMapInfo);
					var originalValue = base.E2_AddressMap;
					base.E2_AddressMap = value;
					PopulateAddressFieldThroughRequirement(E2_AddressMapInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
				}
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public ZDecimal E2_Latitude
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_Latitude : E2_LatitudeCore; }
			set
			{
				if (E2_LatitudeCore != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_LatitudeInfo);
					var originalValue = E2_LatitudeCore;
					E2_LatitudeCore = value;
					PopulateAddressFieldThroughRequirement(E2_LatitudeInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
				}
			}
		}

		public ZPropertyInfo E2_LatitudeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetInfoWithUpdatedHumanReadableName(GetZPropertyInfo(nameof(E2_Latitude))); }
		}

		protected ZDecimal E2_LatitudeCore
		{
			get => (E2_GeoLocation.IsEmpty) ? 0m : (decimal)E2_GeoLocation.Latitude.GetValueOrDefault();
			set
			{
				if (E2_GeoLocation.IsEmpty)
				{
					if (value == 0)
					{
						return;
					}
					else
					{
						SetPropertyValue(E2_GeoLocationInfo, ZGeography.CreatePoint(0, (double)value));
					}
				}
				else
				{
					SetPropertyValue(E2_GeoLocationInfo, (E2_GeoLocation.Longitude == 0 && value == 0) ? ZGeography.Empty : ZGeography.CreatePoint(E2_GeoLocation.Longitude.GetValueOrDefault(), (double)value));
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateE2_GeoLocation();
				}
				E2_LatitudeInfo.RefreshBinding();
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public ZDecimal E2_Longitude
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_Longitude : E2_LongitudeCore; }
			set
			{
				if (E2_LongitudeCore != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_LongitudeInfo);
					var originalValue = E2_LongitudeCore;
					E2_LongitudeCore = value;
					PopulateAddressFieldThroughRequirement(E2_LongitudeInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
				}
			}
		}

		public ZPropertyInfo E2_LongitudeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetInfoWithUpdatedHumanReadableName(GetZPropertyInfo(nameof(E2_Longitude))); }
		}

		public ZDecimal E2_LongitudeCore
		{
			get
			{
				return E2_GeoLocation.IsEmpty ? 0m : (decimal)E2_GeoLocation.Longitude.GetValueOrDefault();
			}
			set
			{
				if (E2_GeoLocation.IsEmpty)
				{
					if (value == 0)
					{
						return;
					}
					else
					{
						SetPropertyValue(E2_GeoLocationInfo, ZGeography.CreatePoint((double)value, 0));
					}
				}
				else
				{
					SetPropertyValue(E2_GeoLocationInfo, (E2_GeoLocation.Latitude == 0 && value == 0) ? ZGeography.Empty : ZGeography.CreatePoint((double)value, E2_GeoLocation.Latitude.GetValueOrDefault()));
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateE2_GeoLocation();
				}
				E2_LongitudeInfo.RefreshBinding();
			}
		}

		public override ZGeography E2_GeoLocation
		{
			get
			{
				return (!E2_AddressOverride && HasRealAddress) ? (
					(Address.OA_Latitude == 0 && Address.OA_Longitude == 0) ? ZGeography.Empty : ZGeography.CreatePoint((double)Address.OA_Longitude, (double)Address.OA_Latitude))
					: base.E2_GeoLocation;
			}
			set
			{
				if (base.E2_GeoLocation != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_GeoLocationInfo);
					var originalValue = base.E2_GeoLocation;
					base.E2_GeoLocation = value;
					PopulateAddressFieldThroughRequirement(E2_GeoLocationInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
				}
			}
		}

		public override ZPropertyInfo E2_GeoLocationInfo => GetInfoWithUpdatedHumanReadableName(base.E2_GeoLocationInfo);

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZBool E2_SuppressAddressValidationError
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_SuppressAddressValidationError : base.E2_SuppressAddressValidationError; }
			set
			{
				if (base.E2_SuppressAddressValidationError != value)
				{
					RaiseAnyAddressFieldBeforeChange(E2_SuppressAddressValidationErrorInfo);
					var originalValue = base.E2_SuppressAddressValidationError;
					base.E2_SuppressAddressValidationError = value;
					PopulateAddressFieldThroughRequirement(E2_SuppressAddressValidationErrorInfo, originalValue);
					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
					RaiseDocAddressChanged();
				}
			}
		}

		public bool HasRealOrganisation => (Organisation?.OH_Code.ToString() ?? "MISC") != "MISC";

		public bool HasRealAddress
		{
			get { return E2_OA_Address.IsValid && E2_OA_Address != MiscAddressPK && Address != null; }
		}

		public override ZString E2_Contact
		{
			get { return (!E2_AddressOverride && base.E2_Contact.IsEmpty && Contact != null) ? Contact.OC_ContactName : base.E2_Contact; }
			set
			{
				ZGuid result = ZGuid.Empty;

				if (!IsSettingContactPK && !E2_AddressOverride && HasRealOrganisation && !value.IsEmpty)
				{
					ZQuery query = new ZQuery(OrgContactSchema.OC_OH, Organisation.PK);
					query.AddToFilter(OrgContactSchema.OC_ContactName, value);
					OrgContact orgC = Factory.LoadTop1<OrgContact>(query);

					if (orgC != null)
					{
						result = orgC.PK;
					}
				}

				//If Org found, set E2_Contact via ContactPK
				if (!result.IsEmpty)
				{
					ContactPK = result;
				}
				else
				{
					if (!IsSettingContactPK)
					{
						fContactPK = ZGuid.Empty;
					}
					ZString originalValue = base.E2_Contact;
					base.E2_Contact = value;
					PopulateContactFieldThroughRequirement(E2_ContactInfo, originalValue);

					CheckSynchronisationWithParentDocAddress();
					RefreshParent();
				}
			}
		}

		protected bool E2_Contact_ReadOnly
		{
			get { return !E2_AddressOverride && Organisation == null; }
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		[EmailAddress]
		public override ZString E2_Email
		{
			get
			{
				ZString result = (!E2_AddressOverride && HasRealAddress) ? Address.OA_Email : base.E2_Email;
				if (Contact != null && !Contact.OC_Email.IsEmpty)
				{
					result = Contact.OC_Email;
				}

				return result;
			}
			set
			{
				ZString originalValue = base.E2_Email;
				base.E2_Email = value;
				PopulateContactFieldThroughRequirement(E2_EmailInfo, originalValue);
				CheckSynchronisationWithParentDocAddress();
				RefreshParent();
			}
		}

		[ChildEditable(true)]
		public GenCustomAddOnRuleAckCollection AddOnRuleAcks
		{
			get
			{
				if (addOnRuleAcks == null)
				{
					addOnRuleAcks = new GenCustomAddOnRuleAckCollection(this);
					RegisterEditableChildObject(addOnRuleAcks);
				}

				return addOnRuleAcks;
			}
		}
		GenCustomAddOnRuleAckCollection addOnRuleAcks;

		#region PhoneNumbers

		PhoneNumberPropertyHelper PhoneNumberPropertyHelper
		{
			get { return phoneNumberPropertyHelperThunk.Value; }
		}

		readonly Lazy<PhoneNumberPropertyHelper> phoneNumberPropertyHelperThunk;

		internal ZString DefaultCountryCodeForPhoneNumbers
		{
			get { return E2_RN_NKCountryCode; }
		}

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public PhoneNumber MobilePhoneNumber
		{
			get
			{
				if (mobilePhoneNumber == null)
				{
					mobilePhoneNumber = new PhoneNumber(E2_Mobile_FormattedInfo, null, E2_Mobile_FormattedLocalNumberIfLoggedInSameCountryInfo, E2_Mobile_IsManuallyVerifiedInfo);
				}
				return mobilePhoneNumber;
			}
		}
		PhoneNumber mobilePhoneNumber;

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public PhoneNumber PhoneNumber
		{
			get
			{
				if (phoneNumber == null)
				{
					phoneNumber = new PhoneNumber(E2_Phone_FormattedInfo, null, E2_Phone_FormattedLocalNumberIfLoggedInSameCountryInfo, E2_Phone_IsManuallyVerifiedInfo);
				}
				return phoneNumber;
			}
		}
		PhoneNumber phoneNumber;

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public PhoneNumber FaxNumber
		{
			get
			{
				if (faxNumber == null)
				{
					faxNumber = new PhoneNumber(E2_Fax_FormattedInfo, null, E2_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo, E2_Fax_IsManuallyVerifiedInfo);
				}
				return faxNumber;
			}
		}
		PhoneNumber faxNumber;

		#endregion

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_Mobile
		{
			get
			{
				ZString result;

				if (Contact != null && !Contact.OC_Mobile.IsEmpty)
				{
					result = Contact.OC_Mobile;
				}
				else if (!E2_AddressOverride && HasRealAddress)
				{
					result = Address.OA_Mobile;
				}
				else
				{
					result = base.E2_Mobile;
				}

				return result;
			}
			set
			{
				ZString originalValue = base.E2_Mobile;
				base.E2_Mobile = value;
				PopulateContactFieldThroughRequirement(E2_MobileInfo, originalValue);

				CheckSynchronisationWithParentDocAddress();
				RefreshParent();
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public ZString E2_Mobile_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(E2_MobileInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(E2_MobileInfo, E2_Mobile_FormattedInfo, value, Validation.ValidateE2_Mobile_Formatted, E2_Mobile_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo E2_Mobile_FormattedInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Mobile_Formatted);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		public ZString E2_Mobile_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(E2_MobileInfo); }
		}

		public ZPropertyInfo E2_Mobile_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Mobile_FormattedLocalNumberIfLoggedInSameCountry);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public ZBool E2_Mobile_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, JobDocAddressSchema.Constants.E2_Mobile, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(E2_Mobile_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, JobDocAddressSchema.Constants.E2_Mobile, Validation.ValidateE2_Mobile_Formatted, MobilePhoneNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo E2_Mobile_IsManuallyVerifiedInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Mobile_IsManuallyVerified);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_Fax
		{
			get
			{
				ZString result = (!E2_AddressOverride && HasRealAddress) ? Address.OA_Fax : base.E2_Fax;
				if (Contact != null && !Contact.OC_Fax.IsEmpty)
				{
					result = Contact.OC_Fax;
				}

				return result;
			}
			set
			{
				ZString originalValue = base.E2_Fax;
				base.E2_Fax = value;
				PopulateContactFieldThroughRequirement(E2_FaxInfo, originalValue);
				CheckSynchronisationWithParentDocAddress();
				RefreshParent();
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public ZString E2_Fax_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(E2_FaxInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(E2_FaxInfo, E2_Fax_FormattedInfo, value, Validation.ValidateE2_Fax_Formatted, E2_Fax_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo E2_Fax_FormattedInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Fax_Formatted);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		public ZString E2_Fax_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(E2_FaxInfo); }
		}

		public ZPropertyInfo E2_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Fax_FormattedLocalNumberIfLoggedInSameCountry);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public ZBool E2_Fax_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, JobDocAddressSchema.Constants.E2_Fax, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(E2_Fax_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, JobDocAddressSchema.Constants.E2_Fax, Validation.ValidateE2_Fax_Formatted, FaxNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo E2_Fax_IsManuallyVerifiedInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Fax_IsManuallyVerified);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#region E2_Phone

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public override ZString E2_Phone
		{
			get
			{
				ZString result = base.E2_Phone;

				if (!E2_AddressOverride)
				{
					if (Contact != null && !Contact.OC_Phone.IsEmpty)
					{
						result = Contact.OC_Phone;
					}
					else if (HasRealAddress)
					{
						result = Address.OA_Phone;
					}
				}

				return result;
			}
			set
			{
				ZString originalValue = base.E2_Phone;
				base.E2_Phone = value;
				PopulateContactFieldThroughRequirement(E2_PhoneInfo, originalValue);
				CheckSynchronisationWithParentDocAddress();
				RefreshParent();
			}
		}

		[BusinessObjectTestExclude] // The test for MaxLength fails due to the formatted Result being trimmed before calling base
		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public ZString E2_Phone_Formatted
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumber(E2_PhoneInfo); }
			set { PhoneNumberPropertyHelper.SetPhoneNumber(E2_PhoneInfo, E2_Phone_FormattedInfo, value, Validation.ValidateE2_Phone_Formatted, E2_Phone_IsManuallyVerifiedInfo); }
		}

		public ZPropertyInfo E2_Phone_FormattedInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Phone_Formatted);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		public ZString E2_Phone_FormattedLocalNumberIfLoggedInSameCountry
		{
			get { return PhoneNumberPropertyHelper.GetPhoneNumberInLocalIfLoggedInSameCountry(E2_PhoneInfo); }
		}

		public ZPropertyInfo E2_Phone_FormattedLocalNumberIfLoggedInSameCountryInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Phone_FormattedLocalNumberIfLoggedInSameCountry);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		[ReadOnlyMember(nameof(AddressNotOverridden))]
		public ZBool E2_Phone_IsManuallyVerified
		{
			get { return PhoneNumberFormatterAndValidator.IsManuallyVerified(this, JobDocAddressSchema.Constants.E2_Phone, AddOnRuleAcks); }
			set { PhoneNumberFormatterAndValidator.SetIsManuallyVerified(E2_Phone_IsManuallyVerifiedInfo, value, AddOnRuleAcks, this, JobDocAddressSchema.Constants.E2_Phone, Validation.ValidateE2_Phone_Formatted, PhoneNumber.FormattedForBindingInfo); }
		}

		public ZPropertyInfo E2_Phone_IsManuallyVerifiedInfo
		{
			get
			{
				var info = GetZPropertyInfo(Schema.E2_Phone_IsManuallyVerified);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		public override ZString E2_ValidationStatus
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.ValidationStatus : base.E2_ValidationStatus; }
			set
			{
				if (base.E2_ValidationStatus != value)
				{
					E2_AddressOverride = value != AddressValidationStatus.NotRequired;
					base.E2_ValidationStatus = value;
					RaiseAddressValidationStatusChanged();
				}

				isManuallyVerifiedByUser = value == AddressValidationStatus.ManuallyVerified;
			}
		}

		#endregion

		#region New Properties

		#region AddressCaption

		public ZString AddressCaption
		{
			get
			{
				ZString addressCaption = "";
				var iDocAddress = Parent as IDocAddressesCaption;
				if (iDocAddress != null)
				{
					addressCaption = iDocAddress.GetAddressCaption(this);
				}
				if (addressCaption.IsEmpty)
				{
					addressCaption = SupportedDocAddressTypesList.GetDescriptionFromCode(E2_AddressType)
						?? DocAddressTypes.GetDescription(Factory, DocAddressType);
				}
				return addressCaption;
			}
		}

		public ZPropertyInfo AddressCaptionInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.AddressCaption);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		public ZString ContactDataFieldType
		{
			get { return (E2_AddressOverride) ? nameof(FieldType.Text) : nameof(FieldType.TextDropEdit); }
		}

		public ZString OrganisationDataFieldType
		{
			get { return (E2_AddressOverride) ? nameof(FieldType.Text) : nameof(FieldType.OrganisationGuid); }
		}

		[DefaultValue(false)]
		public bool ShouldAlwaysUpdateSecondary
		{ get; set; }

		#region Passport Data

		public ZBool IsPassportIDGovRegNumType
		{
			get { return E2_GovRegNumType == OrgCusCode.CodeTypes.PassportID; }
		}

		#region E2_PassportDetails

		public ZString E2_PassportDetails
		{
			get { return Res.GetString("4f833b93-6500-42bb-b40c-1b6f1142371b", "ID:{0} CO:{1} DOB:{2}", E2_PassportID, E2_PassportCountryOfIssue, E2_PassportDateOfBirth.ToString(PassportDateOfBirthFormat).ToUpper()); }
		}

		public ZPropertyInfo E2_PassportDetailsInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.E2_PassportDetails);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		#region E2_PassportID

		[ReadOnlyMember(nameof(PassportDataNotOverridable))]
		[BusinessObjectTestExclude]
		[MaxLength(Schema.E2_PassportIDMaxLength)]
		public ZString E2_PassportID
		{
			get { return (ZString)GetPassportData(PassportDataFieldType.PassportID); }
			set
			{
				ZString oldValue = E2_PassportID;
				if (IsPassportIDGovRegNumType)
				{
					value = value.TrimEnd(' ');
					CheckMaximumLength(E2_PassportIDInfo, value);
					SetPassportData(value, E2_PassportCountryOfIssue, E2_PassportDateOfBirth);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateE2_PassportID();
				}
				E2_PassportIDInfo.RefreshBinding(oldValue);
				E2_PassportDetailsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo E2_PassportIDInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.E2_PassportID);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		#region E2_PassportCountryOfIssue

		[ReadOnlyMember(nameof(PassportDataNotOverridable))]
		[List("Lookups.Countries")]
		[BusinessObjectTestExclude]
		[MaxLength(Schema.E2_PassportCountryOfIssueMaxLength)]
		public ZString E2_PassportCountryOfIssue
		{
			get { return (ZString)GetPassportData(PassportDataFieldType.CountryOfIssue); }
			set
			{
				ZString oldValue = E2_PassportCountryOfIssue;
				if (IsPassportIDGovRegNumType)
				{
					value = value.TrimEnd(' ');
					CheckMaximumLength(E2_PassportCountryOfIssueInfo, value);
					SetPassportData(E2_PassportID, value, E2_PassportDateOfBirth);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateE2_PassportCountryOfIssue();
				}
				E2_PassportCountryOfIssueInfo.RefreshBinding(oldValue);
				E2_PassportDetailsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo E2_PassportCountryOfIssueInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.E2_PassportCountryOfIssue);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		#region E2_PassportDateOfBirth

		[ReadOnlyMember(nameof(PassportDataNotOverridable))]
		[BusinessObjectTestExclude]
		public ZDateTime E2_PassportDateOfBirth
		{
			get { return (ZDateTime)GetPassportData(PassportDataFieldType.DateOfBirth); }
			set
			{
				ZDateTime oldValue = E2_PassportDateOfBirth;
				if (IsPassportIDGovRegNumType)
				{
					SetPassportData(E2_PassportID, E2_PassportCountryOfIssue, value);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateE2_PassportDateOfBirth();
				}
				E2_PassportDateOfBirthInfo.RefreshBinding(oldValue);
				E2_PassportDetailsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo E2_PassportDateOfBirthInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(Schema.E2_PassportDateOfBirth);
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		bool PassportDataNotOverridable
		{
			get { return !IsPassportDataOverridable; }
		}

		public bool IsPassportDataOverridable
		{
			get { return IsPassportIDGovRegNumType && E2_AddressOverride; }
		}

		enum PassportDataFieldType { PassportID, CountryOfIssue, DateOfBirth }

		IZType GetPassportData(PassportDataFieldType fieldType)
		{
			switch (fieldType)
			{
				case PassportDataFieldType.PassportID:
					return IsPassportIDGovRegNumType ? E2_GovRegNum.SubstringSafe(0, Schema.E2_PassportIDMaxLength).TrimEnd() : ZString.Empty;
				case PassportDataFieldType.CountryOfIssue:
					return IsPassportIDGovRegNumType ? E2_GovRegNum.SubstringSafe(Schema.E2_PassportIDMaxLength, Schema.E2_PassportCountryOfIssueMaxLength).TrimEnd() : ZString.Empty;
				default:
					ZDateTime result = ZDateTime.Empty;
					if (IsPassportIDGovRegNumType)
					{
						ZDateTime.TryParseExact(E2_GovRegNum.SubstringSafe(Schema.E2_PassportIDMaxLength + Schema.E2_PassportCountryOfIssueMaxLength, PassportDateOfBirthFormat.Length), out result, PassportDateOfBirthFormat);
					}
					return result;
			}
		}

		bool IsPassportDataUpdateInProgress;
		void SetPassportData(ZString passportID, ZString countryOfIssue, ZDateTime dateOfBirth)
		{
			if (!IsPassportDataUpdateInProgress)
			{
				try
				{
					IsPassportDataUpdateInProgress = true;
					E2_GovRegNum = GetPassportDataStringFormat(passportID, countryOfIssue, dateOfBirth);
				}
				finally
				{
					IsPassportDataUpdateInProgress = false;
				}
			}
		}
		const string PassportDateOfBirthFormat = "ddMMMyyyy";

		public static string GetPassportDataStringFormat(ZString passportID, ZString countryOfIssue, ZDateTime dateOfBirth)
		{
			return passportID.PadRight(Schema.E2_PassportIDMaxLength).Left(Schema.E2_PassportIDMaxLength) +
						countryOfIssue.PadRight(Schema.E2_PassportCountryOfIssueMaxLength).Left(Schema.E2_PassportCountryOfIssueMaxLength) +
						dateOfBirth.ToString(PassportDateOfBirthFormat);
		}

		#endregion

		public string Apartment
		{
			get { return AddressValidationService.GetApartment(this); }
		}

		public string Street
		{
			get { return AddressValidationService.GetStreet(this); }
		}

		public string StreetNumber
		{
			get { return AddressValidationService.GetStreetNumber(this); }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var isCloningViaProperties = !args.PerformRowCopyWithoutTriggeringValidationAndSetter;
			var e2_AddressOverride = E2_AddressOverride;

			if (isCloningViaProperties && e2_AddressOverride)
			{
				args.AddExcludedColumns(new[] { JobDocAddressSchema.Constants.E2_OA_Address }); // when E2_AddressOverride = true, setting E2_OA_Address was calling ClearAddressOverrides() if the AddressPK was not MiscAddressPK
			}

			var result = (JobDocAddress)base.CloneInternal(args);
			result.OverrideRequirement = OverrideRequirement == null ? null : OverrideRequirement.Clone();

			if (e2_AddressOverride && SupportsDocAddressNumbers)
			{
				foreach (var docAddressNumber in DocAddressNumbers)
				{
					result.DocAddressNumbers.Add((JobDocAddressNumber)docAddressNumber.Clone());
				}
			}

			return result;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());

			result.Add(JobDocAddressSchema.Constants.E2_ParentID);
			result.Add(JobDocAddressSchema.Constants.E2_ParentTableCode);

			return result;
		}
		#endregion

		#region E2_Address fields Human Readable Names

		public override ZPropertyInfo E2_GovRegNumTypeInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_GovRegNumTypeInfo); }
		}

		public override ZPropertyInfo E2_AddressSequenceInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_AddressSequenceInfo); }
		}

		protected bool E2_AddressSequence_ReadOnly
		{
			get { return true; }
		}

		public override ZPropertyInfo E2_Address1Info
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_Address1Info); }
		}

		public override ZPropertyInfo E2_Address2Info
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_Address2Info); }
		}

		public override ZPropertyInfo E2_CityInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_CityInfo); }
		}

		public override ZPropertyInfo E2_CompanyNameInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_CompanyNameInfo); }
		}

		public override ZPropertyInfo E2_EmailInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_EmailInfo); }
		}

		public override ZPropertyInfo E2_MobileInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_MobileInfo); }
		}

		public override ZPropertyInfo E2_FaxInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_FaxInfo); }
		}

		public override ZPropertyInfo E2_PhoneInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_PhoneInfo); }
		}

		public override ZPropertyInfo E2_PostcodeInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_PostcodeInfo); }
		}

		public override ZPropertyInfo E2_RN_NKCountryCodeInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_RN_NKCountryCodeInfo); }
		}

		public override ZPropertyInfo E2_StateInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_StateInfo); }
		}

		public override ZPropertyInfo E2_ValidationStatusInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_ValidationStatusInfo); }
		}

		public override ZPropertyInfo E2_GovRegNumInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_GovRegNumInfo); }
		}

		public override ZPropertyInfo E2_ContactInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_ContactInfo); }
		}

		public override ZPropertyInfo E2_OA_AddressInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(base.E2_OA_AddressInfo); }
		}

		public override ZPropertyInfo E2_SuppressAddressValidationErrorInfo =>
			GetInfoWithUpdatedHumanReadableName(base.E2_SuppressAddressValidationErrorInfo);

		class ZPropertyInfoStringWithHumanReadableNameHook : ZPropertyInfoString, IWrappedPropertyInfo
		{
			public ZPropertyInfoStringWithHumanReadableNameHook(JobDocAddress jobDocAddress, ZPropertyInfo innerInfo)
				: base(jobDocAddress, innerInfo.Name)
			{
				this.jobDocAddress = jobDocAddress;
				this.innerInfo = innerInfo;
			}

			readonly JobDocAddress jobDocAddress;
			readonly ZPropertyInfo innerInfo;

			#region IWrappedPropertyInfo

			public ZPropertyInfo InnerInfo => innerInfo;

			#endregion

			protected override ZString GetHumanReadableNameCore()
			{
				using (((IBusinessObjectInternals)jobDocAddress).SuppressReportRowDeletedError())
				{
					return jobDocAddress.AddressDescription + ": " + innerInfo.Description;
				}
			}
		}

		protected ZPropertyInfo GetInfoWithUpdatedHumanReadableName(ZPropertyInfo info)
		{
			return new ZPropertyInfoStringWithHumanReadableNameHook(this, info);
		}

		protected bool AddressNotOverridden
		{
			get { return !E2_AddressOverride; }
		}

		#endregion

		#region Lists

		#region StateListHasMembers

		[BusinessObjectTestExclude]
		public ZBool StateListHasMembers
		{
			get { return Lookups.State_List.Count > 0; }
		}

		public ZPropertyInfo StateListHasMembersInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(StateListHasMembers));
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		#endregion

		#region SupportedDocAddressTypesList

		protected virtual CodeDescriptionPairList SupportedDocAddressTypesList
		{
			get
			{
				if (fSupportedDocAddressTypesList == null)
				{
					fSupportedDocAddressTypesList = Factory.GetCachedValue<DocAddressTypes>();
				}

				return fSupportedDocAddressTypesList;
			}
		}

		CodeDescriptionPairList fSupportedDocAddressTypesList;

		#endregion

		#region JobDocAddressNumbers

		[ChildEditable(false)]
		public JobDocAddressNumberCollection DocAddressNumbers
		{
			get
			{
				if (fDocAddressNumbers == null)
				{
					fDocAddressNumbers = new JobDocAddressNumberCollection(this);
					fDocAddressNumbers.Load();
					RegisterEditableChildObject(fDocAddressNumbers);
				}
				return fDocAddressNumbers;
			}
		}
		JobDocAddressNumberCollection fDocAddressNumbers;

		public virtual bool SupportsDocAddressNumbers => false;

		#endregion

		#endregion

		#region DocAddress Specific Properties

		#region DocAddressManager

		public JobDocAddressManager DocAddressManager
		{
			get
			{
				if (fDocAddressManager == null)
				{
					fDocAddressManager = new JobDocAddressManager();
				}
				return fDocAddressManager;
			}
			set { fDocAddressManager = value; }
		}
		JobDocAddressManager fDocAddressManager;

		#endregion

		#region DocAddressType

		public DocAddressType DocAddressType
		{
			get { return (fDocAddressType ?? (fDocAddressType = DocAddressTypes.GetDocAddressTypeFromCode(Factory, E2_AddressType))).Value; }
			set
			{
				var updateSequence = DocAddressType != value && Parent != null;

				fDocAddressType = value;
				E2_AddressType = (DocAddressType == DocAddressType.None) ? DocAddressTypes.None : DocAddressTypes.GetCode(Factory, DocAddressType);

				if (updateSequence)
				{
					var docAddresses = Parent.DocAddresses.FindDocAddressesByType(DocAddressType, this);
					var max = docAddresses.Length > 0 ? docAddresses[docAddresses.Length - 1].E2_AddressSequence + 1 : 0;
					E2_AddressSequence = ZByte.ParseSafe(max.ToString(), 0);
				}
			}
		}

		DocAddressType? fDocAddressType;

		#endregion

		#region DefaultAddressType

		public AddressType DefaultAddressType
		{
			get { return fDefaultAddressType; }
			set { fDefaultAddressType = value; }
		}

		AddressType fDefaultAddressType = AddressType.OFC;

		#endregion

		#region DefaultContactType

		public ContactType DefaultContactType
		{
			get { return fDefaultContactType; }
			set { fDefaultContactType = value; }
		}

		public ContactType FallbackContactType
		{
			get { return fFallbackContactType; }
			set { fFallbackContactType = value; }
		}

		ContactType fDefaultContactType = ContactType.NoContactType;
		ContactType fFallbackContactType = ContactType.NoContactType;

		#endregion

		#region PreventChangingContact

		public ZBool PreventChangingContact
		{
			get { return fPreventChangingContact; }
			set
			{
				fPreventChangingContact = value;
				E2_ContactInfo.RefreshBinding();
			}
		}

		ZBool fPreventChangingContact = false;

		#endregion

		#region FireEventWhenChanged

		public ZBool BypassFireEventBeforeChange = false;

		#endregion

		#region Events

		public event EventHandler DocAddressChanged;
		public event EventHandler OrgAddressBeforeChange;
		public event EventHandler AnyAddressFieldBeforeChange;
		public event EventHandler OrgHeaderAfterChange;

		public event EventHandler<ZPropertyValueChangedEventArgs> CountryCodeChanged;

		void RaiseDocAddressChanged()
		{
			Parent?.DocAddressChanged(this);

			if (DocAddressChanged != null)
			{
				DocAddressChanged(this, EventArgs.Empty);
			}
		}

		void RaiseOrgAddressBeforeChange()
		{
			Parent?.OrgAddressBeforeChange(this);

			if (OrgAddressBeforeChange != null)
			{
				OrgAddressBeforeChange(this, EventArgs.Empty);
			}
		}

		void RaiseAnyAddressFieldBeforeChange(ZPropertyInfo info)
		{
			if (!BypassFireEventBeforeChange)
			{
				Parent?.AnyAddressFieldBeforeChange(this);

				if (AnyAddressFieldBeforeChange != null)
				{
					AnyAddressFieldBeforeChange(info, EventArgs.Empty);
				}
			}
		}

		void RaiseOrgHeaderAfterChange()
		{
			Parent?.OrgHeaderAfterChange(this);

			if (OrgHeaderAfterChange != null)
			{
				OrgHeaderAfterChange(this, EventArgs.Empty);
			}
		}

		[SuppressMessage(
			"Microsoft.Design",
			"CA1030:UseEventsWhereAppropriate",
			Justification = "This method supposed to be called OnCountryCodeChanged(...) to avoid this CA warning, but leaving it as is for consistency with other RaiseXXX(...) methods!")]
		public void RaiseCountryCodeChanged(ZString oldCode)
		{
			CountryCodeChanged?.Invoke(this, new ZPropertyValueChangedEventArgs(E2_RN_NKCountryCodeInfo, oldCode));
		}

		#endregion

		#endregion

		#region Requirement

		public JobDocAddressRequirement Requirement
		{
			get { return OverrideRequirement ?? (Parent?.GetDocAddressRequirement(DocAddressType)); }
		}

		public JobDocAddressRequirement OverrideRequirement { get; set; }

		#endregion

		#region Defaulting / Clearing Fields

		[DefaultValue("")]
		public string DefaultContactAllocationType { get; set; }

		void SetDefaultContactFromOrg()
		{
			fContactPK = ZGuid.Empty;
			if (!DefaultContactAllocationType.IsNullOrEmpty())
			{
				var contact = Contact;
				if (contact != null)
				{
					E2_Contact = contact.OC_ContactName;
				}
			}
		}

		DefaultContactFinder ContactFinder
		{
			get
			{
				if (Organisation == null)
				{
					fContactFinder = null;
				}
				else if (fContactFinder == null || fContactFinderOrganisation != Organisation)
				{
					fContactFinderOrganisation = Organisation;
					fContactFinder = new DefaultContactFinder(Organisation);
				}

				return fContactFinder;
			}
		}

		DefaultContactFinder fContactFinder;
		OrgHeader fContactFinderOrganisation;

		public void UpdateDefaultAddress(RefUNLOCO lOCOForDefaultAddress)
		{
			UNLOCOForDefaultAddress = lOCOForDefaultAddress;
			var effectiveRelatedPortCode = Address?.EffectiveRelatedPortCode;
			if (effectiveRelatedPortCode != null && effectiveRelatedPortCode != lOCOForDefaultAddress)
			{
				SetDefaultAddressFromOrg(false, false);
			}
		}

		public void SetDefaultAddressFromOrg()
		{
			SetDefaultAddressFromOrg(true, false);
		}

#if DEBUG
		internal
#endif
		void SetDefaultAddressFromOrg(bool setDefaultIfBlank, bool isSettingEmptyOrg)
		{
			OrgAddress result = null;

			if (fOrganisationPK.IsEmpty && HasRealAddress && !isSettingEmptyOrg)
			{
				fOrganisationPK = Address.OA_OH;
			}

			if (!fOrganisationPK.IsEmpty && fOrganisationPK.IsValid)
			{
				result = GetDefaultAddress(fOrganisationPK, setDefaultIfBlank);
			}
			else
			{
				E2_OA_Address = ZGuid.Empty;
			}

			if (result != null)
			{
				E2_OA_Address = result.PK;
			}
		}

		public OrgAddress GetDefaultAddress(ZGuid orgHeaderPK, bool setDefaultIfBlank)
		{
			OrgAddress address = null;

			OrgHeader org = Factory.Load<OrgHeader>(orgHeaderPK);
			if (org != null)
			{
				if (UNLOCOForDefaultAddress != null)
				{
					address = GetAddressWithCorrectLoco(org);
				}

				if (address == null && setDefaultIfBlank)
				{
					address = org.GetAddressWithFallback(DefaultAddressType);
				}
			}

			return address;
		}

		OrgAddress GetAddressWithCorrectLoco(OrgHeader org)
		{
			OrgAddress result = null;

			//Make sure main address exists prior to looping through the addresses below otherwise if main address is missing,
			//it will be created when getting EffectiveRelatedPortCode thus causing the ErrorReporter to send the error message (in ActiveBusinessObjectCollectionIndex)
			if (org.Addresses.MainAddress == null)
			{
				org.Addresses.AddNewMainAddress();
			}

			foreach (OrgAddress address in org.AddressesActive)
			{
				if (address.AddressCapability.GetCapabilityEnabled(DefaultAddressType.ToString())
					&& address.AddressCapability.GetIsMainAddress(DefaultAddressType.ToString())
					&& address.EffectiveRelatedPortCode == UNLOCOForDefaultAddress)
				{
					result = address;
					break;
				}
			}

			if (result == null)
			{
				foreach (OrgAddress address in org.AddressesActive)
				{
					if (address.AddressCapability.GetCapabilityEnabled(DefaultAddressType.ToString())
						&& address.EffectiveRelatedPortCode == UNLOCOForDefaultAddress)
					{
						result = address;
						break;
					}
				}
			}

			return result;
		}

		RefUNLOCO UNLOCOForDefaultAddress;

		protected virtual void ClearAddressOverrides()
		{
			bool saveBypassFireEventValue = BypassFireEventBeforeChange;
			try
			{
				BypassFireEventBeforeChange = true;

				E2_CompanyName = "";

				E2_RN_NKCountryCode = "";
				E2_AdditionalAddressInformation = "";
				E2_Address1 = "";
				E2_Address2 = "";
				E2_City = "";
				E2_State = "";
				E2_Postcode = "";

				E2_Contact = "";
				E2_Phone = "";
				E2_Fax = "";
				E2_Email = "";
				E2_Mobile = "";

				E2_GovRegNum = "";
			}
			finally
			{
				BypassFireEventBeforeChange = saveBypassFireEventValue;
			}
		}

		#endregion

		#region Organisation

		public OrgHeader Organisation
		{
			get
			{
				OrgHeader result = null;

				if (!IsDeleted)
				{
					if (E2_AddressOverride || isSettingAddressOverride)
					{
						result = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MISC");
					}
					else if (HasRealAddress || fOrganisationPK.IsValid)
					{
						result = Factory.Load<OrgHeader>(OrganisationPK);
					}
				}

				return result;
			}
		}

		[RelatedBusinessObject("Organisation")]
		[ActionField(CollectionType = typeof(OrganisationsFindBoxCollection))]
		[ReadOnlyMember(nameof(IsOrganisationPKReadOnly))]
		public ZGuid OrganisationPK
		{
			get { return !IsRowDeletedOrDetachedOrNull && HasRealAddress ? Address.OA_OH : fOrganisationPK; }
			set
			{
				if (!IsRowDeletedOrNull && OrganisationPK != value)
				{
					ClearAddressOverrides();
					ZGuid originalValue = OrganisationPK;
					SetNonPersistentPropertyValue(OrganisationPKInfo, ref fOrganisationPK, value);

					SetDefaultAddressFromOrg(true, value.IsEmpty);
					PopulateOrganisationThroughRequirement(originalValue);
					SetDefaultContactFromOrg();

					if (!IsValidationSuspended)
					{
						Validation.ValidateOrganisationPK();
						Validation.ValidateOrganisationNameOrPK();
					}

					OrganisationPKInfo.RefreshBinding(originalValue);
					OrganisationPKIncludesMiscOrgInfo.RefreshBinding(originalValue);

					RefreshParent();
					RaiseOrgHeaderAfterChange();
				}
			}
		}

		bool IsOrganisationPKReadOnly
		{
			get { return (E2_AddressOverride || (ReadOnlyStrategy?.OrganisationPKReadOnly ?? false)) && CanOverride; }
		}

		public ZPropertyInfo OrganisationPKInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(OrganisationPK));
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		public ZString AddressFull
		{
			get
			{
				var result = new ZStringBuilder();

				if (!E2_Address1.IsEmpty)
				{
					result.Append(E2_Address1).Append(", ");
				}

				if (!E2_Address2.IsEmpty)
				{
					result.Append(E2_Address2).Append(", ");
				}

				if (!E2_RN_NKCountryCode.IsEmpty)
				{
					result.Append(E2_RN_NKCountryCode).Append(", ");
				}

				if (!E2_City.IsEmpty)
				{
					result.Append(E2_City).Append(", ");
				}

				if (!E2_Postcode.IsEmpty)
				{
					result.Append(E2_Postcode).Append(", ");
				}

				if (!E2_State.IsEmpty)
				{
					result.Append(E2_State).Append(", ");
				}

				return result.ToString().Trim().TrimEnd(',');
			}
		}

		public ZString CountryDescription => Country?.Description ?? ZString.Empty;

		#endregion

		#region OrganisationPKIncludesMiscOrg

		public ZGuid OrganisationPKIncludesMiscOrg
		{
			get { return Organisation != null ? Organisation.PK : ZGuid.Empty; }
		}

		public ZPropertyInfo OrganisationPKIncludesMiscOrgInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(OrganisationPKIncludesMiscOrg));
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		ZGuid fOrganisationPK;

		#endregion

		#region OrganisationNameOrPK

		[RelatedBusinessObject("Organisation")]
		[List("Lookups.OrgHeader_List")]
		[BusinessObjectTestExclude]
		public ZString OrganisationNameOrPK
		{
			get
			{
				ZString result = "";

				if (E2_AddressOverride)
				{
					result = E2_CompanyName;
				}
				else if (Organisation != null)
				{
					result = Organisation.PK.ToString();
				}
				else
				{
					result = ZGuid.Empty.ToString();
				}

				return result;
			}
			set
			{
				if (E2_AddressOverride)
				{
					E2_CompanyName = value;
				}
				else
				{
					try
					{
						OrganisationPK = new Guid(value);
					}
					catch (FormatException)
					{
					}
				}
				OrganisationNameOrPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OrganisationNameOrPKInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationNameOrPK), OrganisationPKInfo.HumanReadableName); }
		}

		protected int OrganisationNameOrPK_MaxLength
		{
			get { return E2_AddressOverride ? JobDocAddressSchema.E2_CompanyName.MaxLength : 36; }
		} // where 36 = AnyGuid.ToString().Length

		#endregion

		#region Contact

		public OrgContact Contact
		{
			get
			{
				var pk = ContactPK;
				return hasSystemDefaultContact ? null : Factory.Load<OrgContact>(pk);
			}
		}

		[RelatedBusinessObject("Contact")]
		public ZGuid ContactPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;

				if (HasRealAddress)
				{
					result = fContactPK;
					if (!fContactPK.IsValid)
					{
						var contact = GetContactFromParent();
						if (contact != null)
						{
							result = contact.PK;
							fContactPK = result;
							if (Requirement != null && Requirement.ValidateContact != null)
							{
								Validation.ValidateE2_Contact();
							}
							hasSystemDefaultContact = contact.IsSystemDefaultContact;
						}
					}
				}

				return result;
			}
			set
			{
				IsSettingContactPK = true;
				hasSystemDefaultContact = false;

				if (value.IsValid && !OrganisationPK.IsValid && !E2_AddressOverride)
				{
					SetDefaultOrgFromContact(value);
				}

				fContactPK = value;
				E2_Contact = (Contact != null) ? Contact.OC_ContactName : ZString.Empty;

				if (!IsValidationSuspended)
				{
					Validation.ValidateContactPK();
				}
				IsSettingContactPK = false;

				E2_ContactInfo.RefreshBinding();
				RefreshParent();
				ContactPKInfo.RefreshBinding();
			}
		}

		bool IsSettingContactPK;

		public ZPropertyInfo ContactPKInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(ContactPK));
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		void SetDefaultOrgFromContact(ZGuid newContactPK)
		{
			if (!OrganisationPK.IsValid && !E2_AddressOverride)
			{
				OrgContact orgC = (OrgContact)Factory.Load(typeof(OrgContact), newContactPK);
				if (orgC != null)
				{
					OrganisationPK = orgC.ParentOrg.PK;
				}
			}
		}

		ZGuid fContactPK;
		bool hasSystemDefaultContact;

		#endregion

		#region RealAddress

		public OrgAddress RealAddress => HasRealAddress ? Address : null;

		#endregion

		#region AddressSummary

		[BusinessObjectTestExclude]
		public ZString AddressSummary
		{
			get
			{
				StringBuilder result = new StringBuilder();

				if (!E2_Address1.IsEmpty)
				{
					result.Append(E2_Address1.ToUpper() + System.Environment.NewLine);
				}

				if (!E2_Address2.IsEmpty)
				{
					result.Append(E2_Address2.ToUpper() + System.Environment.NewLine);
				}

				if (!E2_City.IsEmpty)
				{
					result.Append(E2_City.ToUpper() + " ");
				}

				if (!E2_State.IsEmpty)
				{
					result.Append(E2_State.ToUpper() + " ");
				}

				if (!E2_Postcode.IsEmpty)
				{
					result.Append(E2_Postcode.ToUpper() + " ");
				}

				if (Country != null)
				{
					result.Append(Country.Description.ToUpper() + " ");
				}

				return (result.Length == 0) ? Res.GetString("73ca6a26-2b6e-44e7-89da-a71bda7e634b", "**No Address Selected**") : result.ToString().Trim();
			}
		}

		#region AddressSummaryWithCompanyName

		[BusinessObjectTestExclude]
		public ZString AddressSummaryWithCompanyName
		{
			get { return E2_CompanyNameTruncated + System.Environment.NewLine + AddressSummary; }
		}

		public ZPropertyInfo AddressSummaryWithCompanyNameInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(AddressSummaryWithCompanyName));
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		public ZPropertyInfo AddressSummaryInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(AddressSummary));
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		#region AddressSummaryWithCode

		[BusinessObjectTestExclude]
		public ZString AddressSummaryWithCode
		{
			get
			{
				var result = new ZStringBuilder();
				if (!E2_AddressOverride && Address != null)
				{
					result.AppendIfNotEmpty(Address.OA_Code);
				}
				result.AppendIfNotEmpty(AddressSummary);
				return result.ToStringWithNewLineBetweenAppends().Trim();
			}
		}

		public ZPropertyInfo AddressSummaryWithCodeInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(AddressSummaryWithCode));
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		#region AddressDescription

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public ZString AddressDescription
		{
			get
			{
				ZString result = E2_AddressType;

				if (result.IsEmpty)
				{
					result = DocAddressTypes.Unspecified;
				}
				else if (SupportedDocAddressTypesList.ContainsCode(E2_AddressType))
				{
					var iDocAddress = Parent as IDocAddressesCaption;
					if (iDocAddress != null)
					{
						result = iDocAddress.GetAddressCaption(this);
					}
					else
					{
						result = SupportedDocAddressTypesList.GetDescriptionFromCode(E2_AddressType);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo AddressDescriptionInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(AddressDescription));
				return GetInfoWithUpdatedHumanReadableName(info);
			}
		}

		#endregion

		#region Country

		public override RefCountry Country
		{
			get
			{
				return (this.IsRowDeletedOrDetachedOrNull || E2_RN_NKCountryCode.IsEmpty) ? null : base.Country;
			}
		}

		[Obsolete("Please use Country property. This will be removed once references in client-defined macros (Documents, Workflows, Notes, etc) are transformed.")]
		[MacroIgnore]
		public RefCountry CountryCode => Country;

		#endregion

		#region PopulateThroughRequirement

		#region PopulateOrganisationThroughRequirement

		void PopulateOrganisationThroughRequirement(ZGuid originalValue)
		{
			IDocAddresses parent = Parent;
			if (Requirement != null && parent != null)
			{
				foreach (DocAddressType secondaryType in Requirement.SupportedDocAddressTypes)
				{
					JobDocAddress secondaryDocAddress = parent.DocAddresses.FindByDocAddressType(secondaryType);
					if (secondaryDocAddress != null)
					{
						if ((!secondaryDocAddress.E2_AddressOverride && secondaryDocAddress.OrganisationPK == originalValue)
							|| ShouldAlwaysUpdateSecondary)
						{
							secondaryDocAddress.OrganisationPK = fOrganisationPK;
						}
					}
				}
			}
		}

		#endregion

		#region PopulateAddressThroughRequirement

		void PopulateAddressThroughRequirement(ZGuid originalValue, bool ignoreOverride)
		{
			IDocAddresses parent = Parent;
			if (Requirement != null && parent != null)
			{
				foreach (DocAddressType secondaryType in Requirement.SupportedDocAddressTypes)
				{
					JobDocAddress secondaryDocAddress = parent.DocAddresses.FindByDocAddressType(secondaryType);
					if (secondaryDocAddress != null)
					{
						if (((!secondaryDocAddress.E2_AddressOverride || ignoreOverride) && secondaryDocAddress.E2_OA_Address == originalValue))
						{
							secondaryDocAddress.E2_OA_Address = E2_OA_Address;
						}
					}
				}
			}
		}

		#endregion

		#region PopulateCompanyNameThroughRequirement

		void PopulateCompanyNameThroughRequirement(ZString originalValue)
		{
			IDocAddresses parent = Parent;
			if (Requirement != null && parent != null)
			{
				foreach (DocAddressType secondaryType in Requirement.SupportedDocAddressTypes)
				{
					JobDocAddress secondaryDocAddress = parent.DocAddresses.FindByDocAddressType(secondaryType);
					if (secondaryDocAddress != null && secondaryDocAddress.E2_AddressOverride && secondaryDocAddress.E2_CompanyName == originalValue)
					{
						secondaryDocAddress.E2_CompanyName = E2_CompanyName;
					}
				}
			}
		}

		#endregion

		#region PopulateOverrideTickedThroughRequirement

		void PopulateOverrideTickedThroughRequirement(ZGuid originalOH)
		{
			IDocAddresses parent = Parent;
			if (Requirement != null && parent != null)
			{
				foreach (DocAddressType secondaryType in Requirement.SupportedDocAddressTypes)
				{
					JobDocAddress secondaryDocAddress = parent.DocAddresses.FindByDocAddressType(secondaryType);
					if (secondaryDocAddress != null)
					{
						if ((!secondaryDocAddress.E2_AddressOverride && secondaryDocAddress.OrganisationPK == originalOH)
							|| ShouldAlwaysUpdateSecondary)
						{
							secondaryDocAddress.E2_AddressOverride = E2_AddressOverride;

							secondaryDocAddress.E2_CompanyName = E2_CompanyName;
							foreach (ZString field in AddressFields)
							{
								secondaryDocAddress[field] = this[field];
							}

							foreach (ZString field in ContactFields)
							{
								secondaryDocAddress[field] = this[field];
							}
						}
					}
				}
			}
		}

		#endregion

		#region PopulateOverrideUntickedThroughRequirement

		void PopulateOverrideUntickedThroughRequirement(ZString originalCompanyName)
		{
			IDocAddresses parent = Parent;
			if (Requirement != null && parent != null)
			{
				foreach (DocAddressType secondaryType in Requirement.SupportedDocAddressTypes)
				{
					JobDocAddress secondaryDocAddress = parent.DocAddresses.FindByDocAddressType(secondaryType);
					if (secondaryDocAddress != null
						&& secondaryDocAddress.E2_AddressOverride
						&& secondaryDocAddress.E2_CompanyName == originalCompanyName)
					{
						secondaryDocAddress.E2_AddressOverride = E2_AddressOverride;
					}
				}
			}
		}

		#endregion

		#region PopulateAddressFieldThroughRequirement

		void PopulateAddressFieldThroughRequirement(ZPropertyInfo addressInfo, IZType originalValue)
		{
			var parent = Parent;

			if (Requirement != null && parent != null)
			{
				foreach (var secondaryType in Requirement.SupportedDocAddressTypes)
				{
					var secondaryDocAddress = parent.DocAddresses.FindByDocAddressType(secondaryType);
					if (secondaryDocAddress != null
						&& secondaryDocAddress.E2_AddressOverride
						&& OriginalAddressMatchesDocAddress(addressInfo, originalValue, secondaryDocAddress))
					{
						secondaryDocAddress[addressInfo.Name] = addressInfo.Value;
					}
				}
			}
		}

		bool OriginalAddressMatchesDocAddress(ZPropertyInfo addressFieldChanging, IZType originalValue, JobDocAddress secondaryDocAddress)
		{
			var originalAddress = new StringBuilder();
			var secondaryAddress = new StringBuilder();
			var skipChangingField = addressFieldChanging == null || originalValue == null;

			foreach (var addressField in AddressFields)
			{
				secondaryAddress.Append(secondaryDocAddress[addressField]);
				originalAddress.Append(!skipChangingField && addressField == addressFieldChanging.Name ? originalValue : this[addressField]);
			}

			return originalAddress.ToString() == secondaryAddress.ToString();
		}

		List<ZString> AddressFields
		{
			get
			{
				List<ZString> result = new List<ZString>();
				result.Add(E2_AdditionalAddressInformationInfo.Name);
				result.Add(E2_Address1Info.Name);
				result.Add(E2_Address2Info.Name);
				result.Add(E2_CityInfo.Name);
				result.Add(E2_PostcodeInfo.Name);
				result.Add(E2_RN_NKCountryCodeInfo.Name);
				result.Add(E2_StateInfo.Name);
				return result;
			}
		}

		#endregion

		#region PopulateContactFieldThroughRequirement

		void PopulateContactFieldThroughRequirement(ZPropertyInfo contactInfo, ZString originalValue)
		{
			IDocAddresses parent = Parent;

			if (Requirement != null && parent != null)
			{
				foreach (DocAddressType secondaryType in Requirement.SupportedDocAddressTypes)
				{
					JobDocAddress secondaryDocContact = parent.DocAddresses.FindByDocAddressType(secondaryType);
					if (secondaryDocContact != null
						&& (secondaryDocContact.E2_AddressOverride
						&& OriginalContactMatchesDocContact(contactInfo, originalValue, secondaryDocContact)) || ShouldAlwaysUpdateSecondary)
					{
						secondaryDocContact[contactInfo.Name] = contactInfo.Value;
					}
				}
			}
		}

		bool OriginalContactMatchesDocContact(ZPropertyInfo contactFieldChanging, ZString originalValue, JobDocAddress secondaryDocContact)
		{
			StringBuilder originalContact = new StringBuilder();
			StringBuilder secondaryContact = new StringBuilder();

			foreach (ZString contactField in ContactFields)
			{
				secondaryContact.Append(secondaryDocContact[contactField]);
				originalContact.Append(contactField == contactFieldChanging.Name ? originalValue : this[contactField]);
			}

			return originalContact.ToString() == secondaryContact.ToString();
		}

		List<ZString> ContactFields
		{
			get
			{
				List<ZString> result = new List<ZString>();
				result.Add(E2_ContactInfo.Name);
				result.Add(E2_EmailInfo.Name);
				result.Add(E2_FaxInfo.Name);
				result.Add(E2_MobileInfo.Name);
				result.Add(E2_PhoneInfo.Name);
				return result;
			}
		}

		#endregion

		#endregion

		#region Additional Methods

		internal ZString GetCountryCodeFromOriginalAddress()
		{
			ZString result = "";
			if (HasRealAddress)
			{
				if (!Address.OA_RN_NKCountryCode.IsEmpty)
				{
					result = Address.OA_RN_NKCountryCode;
				}
				else if (!Address.OA_RL_NKRelatedPortCode.IsEmpty)
				{
					RefUNLOCO uNLOCO = (RefUNLOCO)Factory.LoadTop1(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, Address.OA_RL_NKRelatedPortCode));
					result = (uNLOCO != null) ? uNLOCO.RL_RN_NKCountryCode : Address.OA_RL_NKRelatedPortCode.SubstringSafe(0, 2);
				}
				else if (HasRealOrganisation && Organisation.UNLOCO != null && Organisation.UNLOCO.Country != null)
				{
					result = Organisation.UNLOCO.RL_RN_NKCountryCode;
				}
				else if (HasRealOrganisation && !Organisation.OH_RL_NKClosestPort.IsEmpty)
				{
					result = Organisation.OH_RL_NKClosestPort.SubstringSafe(0, 2);
				}
			}

			return result;
		}

		internal OrgContact GetContactFromParent()
		{
			OrgContact result = null;

			if (!E2_AddressOverride && HasRealOrganisation)
			{
				if (!base.E2_Contact.IsEmpty)
				{
					ZQuery query = new ZQuery(OrgContactSchema.OC_OH, Organisation.PK);
					query.AddToFilter(OrgContactSchema.OC_ContactName, base.E2_Contact);
					result = Factory.LoadTop1<OrgContact>(query);
				}

				if (result == null && !DefaultContactAllocationType.IsNullOrEmpty() && ContactFinder != null)
				{
					result = ContactFinder.DefaultContactAllocationType(DefaultContactAllocationType);
				}

				if (result == null && DefaultContactType != ContactType.NoContactType && ContactFinder != null)
				{
					result = ContactFinder.DefaultContact(DefaultContactType);
					if (result == null && FallbackContactType != ContactType.NoContactType)
					{
						result = ContactFinder.DefaultContact(FallbackContactType);
					}
				}
			}

			return result;
		}

		internal ZString GetContactNameFromParent()
		{
			ZString result = "";
			if (!E2_AddressOverride)
			{
				OrgContact orgC = GetContactFromParent();
				result = (orgC != null) ? orgC.OC_ContactName : ZString.Empty;
			}

			return result;
		}

		internal ZString GetCompanyName()
		{
			ZString result = "";
			if (HasRealAddress)
			{
				result = Address.OA_CompanyNameOverride;
				if (result.IsEmpty && HasRealOrganisation)
				{
					result = Organisation.OH_FullName;
				}
			}

			return result;
		}

		[BusinessObjectTestExclude]
		public ZString AddressAsASingleLine
		{
			get { return new AddressFormatter(Factory, this, GlbCompany.CurrentCompany, false).PostalAddressAsASingleLine(); }
		}

		public ZString AddressAsASingleLineWithoutCompanyName
		{
			get { return new AddressFormatter(Factory, this, GlbCompany.CurrentCompany, false).PostalAddressAsASingleLineWithoutCompanyName(); }
		}

		public ZPropertyInfo AddressAsASingleLineInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(nameof(AddressAsASingleLine));
				return GetInfoWithUpdatedHumanReadableName(result);
			}
		}

		#endregion

		#region Cartage Equipment

		public ZString FCLCartageEquipmentNeeded
		{
			get { return Address != null ? Address.OA_FCLEquipmentNeeded : (ZString)OrganisationsDataRegistry.Instance.RequiredCartageEquipmentFCL.Value; }
		}

		public ZString LCLCartageEquipmentNeeded
		{
			get { return Address != null ? Address.OA_LCLEquipmentNeeded : (ZString)OrganisationsDataRegistry.Instance.RequiredCartageEquipmentLCL.Value; }
		}

		public ZString AirCartageEquipmentNeeded
		{
			get { return Address != null ? Address.OA_AIREquipmentNeeded : (ZString)OrganisationsDataRegistry.Instance.RequiredCartageEquipmentAIR.Value; }
		}

		#endregion

		#region Does this Address Exist for Business Purposes?

		public bool IsValidAddress
		{
			get
			{
				bool hasErrors;

				if (E2_AddressOverride)
				{
					hasErrors =
						E2_AddressTypeInfo.HasErrors() ||
						E2_CompanyNameInfo.HasErrors() ||
						E2_Address1Info.HasErrors() ||
						E2_CityInfo.HasErrors() ||
						E2_StateInfo.HasErrors();
				}
				else
				{
					hasErrors = !HasRealAddress || E2_AddressTypeInfo.HasErrors();
				}

				return !hasErrors;
			}
		}

		#endregion

		#region IsTheSameAddressAs (another JobDocAddress)

		public bool IsTheSameAddressAs(IDocAddress docAddressForComparison)
		{
			bool result;
			if (E2_AddressOverride)
			{
				result = E2_CompanyName == docAddressForComparison.E2_CompanyName
					&& E2_AdditionalAddressInformation == docAddressForComparison.E2_AdditionalAddressInformation
					&& (E2_Address1 == docAddressForComparison.E2_Address1
					|| (E2_Address2 == docAddressForComparison.E2_Address2 && !E2_Address2.IsEmpty))
					&& E2_City == docAddressForComparison.E2_City
					&& E2_State == docAddressForComparison.E2_State;
			}
			else
			{
				result = E2_OA_Address == docAddressForComparison.E2_OA_Address;
			}

			return result;
		}

		public bool IsTheSameDocAddressAndContactAs(JobDocAddress docAddressForComparison)
		{
			bool result;

			if (docAddressForComparison == null)
			{
				return false;
			}

			if (E2_AddressOverride || E2_OA_Address.IsEmpty || docAddressForComparison.E2_AddressOverride || docAddressForComparison.E2_OA_Address.IsEmpty)
			{
				result = (E2_CompanyName == docAddressForComparison.E2_CompanyName)
				&& (E2_AdditionalAddressInformation == docAddressForComparison.E2_AdditionalAddressInformation)
				&& (E2_Address1 == docAddressForComparison.E2_Address1)
				&& (E2_Address2 == docAddressForComparison.E2_Address2)
				&& (E2_City == docAddressForComparison.E2_City)
				&& (E2_State == docAddressForComparison.E2_State);

				result &= (E2_Postcode == docAddressForComparison.E2_Postcode)
					&& (E2_RN_NKCountryCode == docAddressForComparison.E2_RN_NKCountryCode)
					&& (E2_Contact == docAddressForComparison.E2_Contact)
					&& (E2_Phone == docAddressForComparison.E2_Phone)
					&& (E2_Fax == docAddressForComparison.E2_Fax)
					&& (E2_Email == docAddressForComparison.E2_Email);
			}
			else
			{
				result = E2_OA_Address == docAddressForComparison.E2_OA_Address;
			}
			return result;
		}

		bool IsStringEmptyOrContainedInStringB(ZString stringA, ZString stringB)
		{
			return stringA.IsEmpty || stringB.Contains(stringA, StringComparison.CurrentCulture);
		}

		public bool IsMatchedAgainst(IDocAddress docAddressForComparison)
		{
			bool result;
			if (E2_AddressOverride)
			{
				result = IsStringEmptyOrContainedInStringB(docAddressForComparison.E2_CompanyName, E2_CompanyName)
					&& IsStringEmptyOrContainedInStringB(docAddressForComparison.E2_AdditionalAddressInformation, E2_AdditionalAddressInformation)
					&& IsStringEmptyOrContainedInStringB(docAddressForComparison.E2_Address1, E2_Address1) || E2_Address2.Contains(docAddressForComparison.E2_Address2)
					&& IsStringEmptyOrContainedInStringB(docAddressForComparison.E2_Address2, E2_Address2) || E2_Address2.Contains(docAddressForComparison.E2_Address1)
					&& IsStringEmptyOrContainedInStringB(docAddressForComparison.E2_City, E2_City)
					&& IsStringEmptyOrContainedInStringB(docAddressForComparison.E2_State, E2_State);

				var docAddress = docAddressForComparison as JobDocAddress;
				if (docAddress != null)
				{
					result &= IsStringEmptyOrContainedInStringB(docAddress.E2_Postcode, E2_Postcode)
							&& IsStringEmptyOrContainedInStringB(docAddress.E2_RN_NKCountryCode, E2_Contact)
							&& IsStringEmptyOrContainedInStringB(docAddress.E2_Contact, E2_Contact)
							&& IsStringEmptyOrContainedInStringB(docAddress.E2_Phone, E2_Contact)
							&& IsStringEmptyOrContainedInStringB(docAddress.E2_Fax, E2_Contact)
							&& IsStringEmptyOrContainedInStringB(docAddress.E2_Email, E2_Contact);
				}
			}
			else
			{
				result = E2_OA_Address == docAddressForComparison.E2_OA_Address;
			}

			return result;
		}

		#endregion

		#region IEquatable<IJobDocAddress> Members

		public bool Equals(IJobDocAddress other)
		{
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			var docAddress = other as JobDocAddress;
			if (docAddress != null)
			{
				return IsTheSameDocAddressAndContactAs(docAddress);
			}

			return false;
		}

		#endregion

		#region IsEmpty

		public virtual bool IsEmpty
		{
			get { return !E2_OA_Address.IsValid && !E2_AddressOverride; }
		}

		public bool IsOverridenButEmpty
		{
			get
			{
				return E2_AddressOverride
					&& E2_CompanyName.IsEmpty
					&& E2_AdditionalAddressInformation.IsEmpty
					&& E2_Address1.IsEmpty
					&& E2_Address2.IsEmpty
					&& E2_City.IsEmpty
					&& E2_Postcode.IsEmpty
					&& E2_State.IsEmpty
					&& E2_Contact.IsEmpty
					&& E2_Phone.IsEmpty
					&& E2_Email.IsEmpty
					&& E2_Fax.IsEmpty
					&& E2_GovRegNum.IsEmpty
					;
			}
		}
		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get => base.ReadOnly || (ReadOnlyStrategy?.ReadOnly ?? false);
			set => base.ReadOnly = value;
		}

		public IJobDocAddressReadOnlyStrategy ReadOnlyStrategy { get; set; }

		#endregion

		#region Saving & Deleting

		public override void OnSaving()
		{
			InvalidateScreeningStatuses();

			this.AddAddressValidationEventLog(E2_ValidationStatusInfo.OriginalValue.ToString(), Factory);

			if (!IsDeleted)
			{
				if (IsEmpty && !ShouldBePersisted)
				{
					Delete();
				}
			}
			base.OnSaving();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (SupportsDocAddressNumbers)
			{
				if (E2_AddressOverride)
				{
					DeleteEmptyJobDocAddressNumbers();
				}
				else
				{
					DocAddressNumbers.RemoveAndDeleteAll();
				}
			}
		}

		void DeleteEmptyJobDocAddressNumbers()
		{
			var docAddressNumbers = DocAddressNumbers;
			var emptyDocAddressNumbers = docAddressNumbers.Cast<JobDocAddressNumber>().Where(x => !x.IsDeleted && x.E2N_Number.IsEmpty).ToArray();
			emptyDocAddressNumbers.ForEach(x => docAddressNumbers.RemoveAndDelete(x));
		}

		void ResetParentScreeningStatus(object sender, HasChangesChangedEventArgs e)
		{
			if (!IsDeleted && HasChanges)
			{
				ResetParentScreeningStatusCore();
			}
		}

		void ResetParentScreeningStatusCore()
		{
			IShouldUpdateScreeningStatus parentProvider = null;
			IScreeningStatusProvider screeningStatusProvider = null;
			try
			{
				parentProvider = Parent as IShouldUpdateScreeningStatus;
				screeningStatusProvider = Parent as IScreeningStatusProvider;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//ignore errors
			}

			if (parentProvider != null && ShouldUpdateScreenStatus(screeningStatusProvider))
			{
				parentProvider.ShouldUpdateScreeningStatus = true;
			}
		}

		bool ShouldUpdateScreenStatus(IScreeningStatusProvider screeningStatusProvider)
		{
			return !(screeningStatusProvider != null
				&& screeningStatusProvider.ScreeningStatus == ScreeningStatusesList.Codes.JobCleared
				&& !E2_AddressOverride
				&& (!E2_OA_Address.IsValid
					|| IsInDatabase && !E2_OA_AddressInfo.HasChanges
					|| Organisation == null
					|| (string)Organisation.OH_ScreeningStatus == ScreeningStatusesList.Codes.Clear
					|| (string)Organisation.OH_ScreeningStatus == ScreeningStatusesList.Codes.PermanentClear
					|| (string)Organisation.OH_ScreeningStatus == ScreeningStatusesList.Codes.JobCleared));
		}

		public void InvalidateScreeningStatuses()
		{
			if (E2_ScreeningStatus != ScreeningStatusesList.Codes.PermanentClear &&
				(E2_AddressOverride || Organisation == null) &&
				(!IsInDatabase || E2_Address1Info.HasChanges || E2_Address2Info.HasChanges || E2_CityInfo.HasChanges ||
				E2_CompanyNameInfo.HasChanges || E2_PostcodeInfo.HasChanges ||
				E2_RN_NKCountryCodeInfo.HasChanges || E2_StateInfo.HasChanges))
			{
				ScreeningLogCollection.InvalidateByLocalDataChanges();
				E2_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			}
		}

		public override void Delete()
		{
			if (!IsRowDeletedOrDetachedOrNull)
			{
				Parent?.OnBeforeDocAddressDeleted(this);

				if (IsInDatabase)
				{
					ResetParentScreeningStatusCore();
				}
			}

			if (HookedEvents)
			{
				UnHookChangeEventsInParentDocAddress();
			}

			if (OnDeleting != null)
			{
				OnDeleting(this, EventArgs.Empty);
			}

			if (SupportsDocAddressNumbers)
			{
				DocAddressNumbers.RemoveAndDeleteAll();
			}

			AddOnRuleAcks.DeleteAll();
			DeleteJobDocumentExclusion();

			base.Delete();
		}

		void DeleteJobDocumentExclusion()
		{
			Factory.Load<JobDocumentExclusion>(new ZQuery(JobDocumentExclusionSchema.JDE_E2_Address, PK)).ForEach(b => b.Delete());
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new JobDocAddressUniqueIndexFailureHandler(this); }
		}

		public event EventHandler OnDeleting;

		#endregion

		#region Implementation

		/// <summary>
		/// If parent's validation needs to be marked as invalid, these can be hooked up.
		/// </summary>
		public event EventHandler OnRelationshipFieldsChanged
		{
			add
			{
				E2_ParentIDInfo.ValueChanged += value;
				E2_ParentTableCodeInfo.ValueChanged += value;
				E2_AddressTypeInfo.ValueChanged += value;
				E2_OA_AddressInfo.ValueChanged += value;
			}
			remove
			{
				E2_ParentIDInfo.ValueChanged -= value;
				E2_ParentTableCodeInfo.ValueChanged -= value;
				E2_AddressTypeInfo.ValueChanged -= value;
				E2_OA_AddressInfo.ValueChanged -= value;
			}
		}

		void RefreshParent()
		{
			if (refreshingParentSuspended == 0)
			{
				// with this, parents using + binding (eg. Shipment+DocAddress) won't get the updates in the GUI.
				BusinessObject bizObj = Parent as BusinessObject;
				if (bizObj != null)
				{
					using (SuspendRefreshingParent())
					{
						bizObj.RefreshBinding();
					}
				}
			}
		}

		public IDisposable SuspendRefreshingParent()
		{
			refreshingParentSuspended++;
			return new DisposableAction(delegate
			{ refreshingParentSuspended--; });
		}

		public bool IsRefreshingParentSuspended
		{
			get { return refreshingParentSuspended > 0; }
		}
		int refreshingParentSuspended;

		protected override IZType GetCurrentValueToCompareToForChanges(ZPropertyInfo info)
		{
			return info.PersistentValue;
		}

		public bool IgnoreValidationStatusError { get; set; }
		#endregion

		#region Validation

		protected override JobDocAddressValidation GetNewValidation()
		{
			var result = base.GetNewValidation();
			var parent = this.Parent;
			if (parent != null)
			{
				var piggyBackValidation = parent.PiggyBackedDocAddressValidation(this);
				if (piggyBackValidation != null)
				{
					IValidationInternals resultInternals = result;
					resultInternals.Add(piggyBackValidation);
				}

				if (AdditionalValidation != null)
				{
					IValidationInternals resultInternals = result;
					resultInternals.Add(AdditionalValidation);
				}
			}
			return result;
		}

		public ZValidation AdditionalValidation
		{ get; set; }

		#endregion

		#region Document Delivery Details

		public DocDeliveryContact GetDocumentDeliveryContact(IStmMenuItem menuItem)
		{
			DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);

			deliveryContact.Initialise(menuItem, null);

			deliveryContact.OrgHeaderPK = HasRealAddress ? Address.Header.PK : ZGuid.Empty;

			deliveryContact.Name = Contact != null ? Contact.OC_ContactName : E2_Contact;
			deliveryContact.Email = Contact != null ? Contact.OC_Email : E2_Email;
			deliveryContact.Fax = Contact != null ? Contact.OC_Fax : E2_Fax;
			deliveryContact.Phone = Contact != null ? Contact.OC_Phone : E2_Phone;

			if (Contact != null)
			{
				deliveryContact.DeliveryMethod = Contact.OC_NotifyMode;
			}
			else if (!E2_Email.IsEmpty)
			{
				deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Email;
			}
			else if (!E2_Fax.IsEmpty)
			{
				deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Fax;
			}
			else
			{
				deliveryContact.DeliveryMethod = Constants.ContactNotifyModes.Print;
			}

			deliveryContact.AdditionalAddress = HasRealAddress ? Address.OA_AdditionalAddressInformation : E2_AdditionalAddressInformation;
			deliveryContact.Address1 = HasRealAddress ? Address.OA_Address1 : E2_Address1;
			deliveryContact.Address2 = HasRealAddress ? Address.OA_Address2 : E2_Address2;
			deliveryContact.City = HasRealAddress ? Address.OA_City : E2_City;
			deliveryContact.PostCode = HasRealAddress ? Address.OA_PostCode : E2_Postcode;
			deliveryContact.State = HasRealAddress ? Address.OA_State : E2_State;
			deliveryContact.CompanyName = HasRealAddress ? Address.Header.OH_FullNameTruncated : E2_CompanyNameTruncated;

			return deliveryContact;
		}

		#endregion

		#region IDocAddress Members

		ZString IDocAddress.E2_PortCode
		{
			get { return Address != null && !E2_AddressOverride ? ((IDocAddress)Address).E2_PortCode : null; }
		}

		ZString IDocAddress.CountryCode
		{
			get { return E2_RN_NKCountryCode; }
		}

		ZString IDocAddress.E2_RN_NKCountryCode
		{
			get { return (!E2_AddressOverride && HasRealAddress) ? Address.OA_RN_NKCountryCode : base.E2_RN_NKCountryCode; }
		}

		ZString IDocAddress.ParentDescription
		{
			get { return ParentAsBusinessObject != null ? ParentAsBusinessObject.HumanReadableName : ZString.Empty; }
		}

		IOrgHeader IDocAddress.Organisation
		{
			get { return Organisation; }
		}

		#endregion

		#region Screening Log Collection

		[ChildEditable]
		[ChildEditableTestExclude]
		public IStmEntityScreeningLogCollection ScreeningLogCollection
		{
			get
			{
				if (!E2_AddressOverride && Organisation != null)
				{
					return Organisation.ScreeningLogCollection;
				}
				else
				{
					if (screeningLogCollection == null)
					{
						screeningLogCollection = (IStmEntityScreeningLogCollection)Activator.CreateInstance(ObjectFactory.GetType<IStmEntityScreeningLogCollection>(), this);
						RegisterEditableChildObject((IBusiness)screeningLogCollection);
					}
					return screeningLogCollection;
				}
			}
		}
		IStmEntityScreeningLogCollection screeningLogCollection;

		#endregion

		#region IScreeningPartyProvider Members

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties
		{
			get { return Array.Empty<ScreeningParty>(); }
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			if (!IsDeleted)
			{
				if (E2_AddressOverride)
				{
					return base.E2_ScreeningStatus;
				}
				else if (Organisation != null)
				{
					return Organisation.OH_ScreeningStatus;
				}
			}

			return ScreeningStatusesList.Codes.Clear;
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return (this as IScreeningPartyProvider).GetWorstScreeningStatus();
		}

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get { return E2_ScreeningStatus; }
			set { E2_ScreeningStatus = value; }
		}

		public override ZString E2_ScreeningStatus
		{
			get
			{
				if (E2_AddressOverride || this.Organisation == null)
				{
					return base.E2_ScreeningStatus;
				}
				else
				{
					return this.Organisation.OH_ScreeningStatus;
				}
			}
			set
			{
				base.E2_ScreeningStatus = value;
			}
		}

		#endregion

		#region IZAddress members

		[ActionField(ReadOnly = false)]
		ZAddressList IZAddress.AddressList
		{
			get
			{
				var addressList = Lookups.Address_List;
				AddSelectedInactiveAddress(addressList);
				return addressList;
			}
		}

		void AddSelectedInactiveAddress(ZAddressList list)
		{
			if (Address != null && !Address.IsDeleted && !Address.OA_IsActive &&
				list.List.Cast<ZAddressItem>().All(item => item.PK != Address.PK))
			{
				ZString headerInactive = Res.GetString("086f6383-e432-4eae-adfd-6d986140089e", "Inactive");
				ZString addressCode = headerInactive + ": " + Address.OA_Code;
				ZString addressDescription = headerInactive + ": " + ((IOrgAddress)Address).AddressDetailedOnSingleLine;
				list.AddAddress(Address.PK, addressCode, addressDescription, new AddressCapabilityItem { Capability = headerInactive, IsDefault = false });
				MarkAsNeedingValidation();
			}
		}

		ZGuid IZAddress.OrgPK
		{
			get { return OrganisationPK; }
		}

		bool IZAddress.IsDeleted
		{
			get { return IsDeleted; }
		}

		#endregion

		#region ISupportWebAddressValidation

		public ZString AddressRecordGUID
		{
			get { return PK.ToString(); }
		}

		public ZString AddressSourceTable
		{
			get { return !string.IsNullOrEmpty(E2_ParentTableCode) ? E2_ParentTableCode.ToString() : JobDocAddressSchema.Constants.Prefix; }
		}

		[List("AdditionalAddressInfoList")]
		[DocumentFieldExcludeFromMap]
		public ZString UnrestrictedAdditionalAddressInformation
		{
			get
			{
				if (ShouldApplyOrgAddressAdditionalInformation)
				{
					return Address.OA_AdditionalAddressInformation;
				}
				else
				{
					if (!E2_AdditionalAddressInformation.IsEmpty && unrestrictedAdditionalAddressInformation.IsEmpty)
					{
						SetUnrestrictedAdditionalAddressInformation(E2_AdditionalAddressInformation);
					}

					return unrestrictedAdditionalAddressInformation;
				}
			}
			set
			{
				SetUnrestrictedAdditionalAddressInformation(value.TrimEndSpaceTab());
			}
		}

		protected bool UnrestrictedAdditionalAddressInformation_ReadOnly => !E2_AddressOverride && (Organisation == null || !HasRealAddress || !OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.Value);

		public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo => ShouldApplyOrgAddressAdditionalInformation ? Address.UnrestrictedAdditionalAddressInformationInfo : E2_AdditionalAddressInformationInfo;

		bool ShouldApplyOrgAddressAdditionalInformation => !E2_AddressOverride && HasRealAddress && (base.E2_AdditionalAddressInformation.IsEmpty || !OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.Value);

		void SetUnrestrictedAdditionalAddressInformation(ZString value)
		{
			if (unrestrictedAdditionalAddressInformation != value)
			{
				unrestrictedAdditionalAddressInformation = value;
				if (!HasChanges)
				{
					HasChanges = value != E2_AdditionalAddressInformation;
				}
				if (value.Length <= E2_AdditionalAddressInformationInfo.MaxLength)
				{
					E2_AdditionalAddressInformation = value;
				}

				Validation.ValidateE2_AdditionalAddressInformation();
			}
		}

		ZString unrestrictedAdditionalAddressInformation;

		public CodeDescriptionPairList AdditionalAddressInfoList => Address != null ? Address.AdditionalAddressInfoList : new CodeDescriptionPairList();

		public string GetTranslatedAdditionalAddressInSpecificLanguageWithFallback(string language)
		{
			string overrideAdditionalInfo = null;
			if (!E2_AddressOverride && !E2_AdditionalAddressInformation.IsEmpty && Address != null)
			{
				var translatedOne = Address.AdditionalInfos.FirstOrDefault(u => u.OAI_AdditionalInfo.EqualsIgnoringCase(E2_AdditionalAddressInformation))?.TranslatedInfos.FirstOrDefault(u => u.OTI_Language == language);

				if (translatedOne != null)
				{
					overrideAdditionalInfo = translatedOne.OTI_AdditionalInfo;
				}
				else
				{
					overrideAdditionalInfo = E2_AdditionalAddressInformation;
				}
			}

			return overrideAdditionalInfo;
		}

		public ZString AddressCode { get; set; }

		public ZString AdditionalAddressInformation
		{
			get { return E2_AdditionalAddressInformation; }
			set { E2_AdditionalAddressInformation = value; }
		}

		public ZString Address1
		{
			get { return E2_Address1; }
			set { E2_Address1 = value; }
		}

		public ZPropertyInfo Address1Info
		{
			get { return E2_Address1Info; }
		}

		public int Address1_MaxLength
		{
			get { return Schema.E2_Address1MaxLength; }
		}

		public ZString Address2
		{
			get { return E2_Address2; }
			set { E2_Address2 = value; }
		}

		public ZPropertyInfo Address2Info
		{
			get { return E2_Address2Info; }
		}

		public int Address2_MaxLength
		{
			get { return Schema.E2_Address2MaxLength; }
		}

		public ZString City
		{
			get { return E2_City; }
			set { E2_City = value; }
		}

		public ZPropertyInfo CityInfo
		{
			get { return E2_CityInfo; }
		}

		public int City_MaxLength
		{
			get { return Schema.E2_CityMaxLength; }
		}

		public ZString State
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Lookups.State_List.GetDescriptionFromCode(E2_State)))
				{
					return E2_State;
				}
				return Lookups.State_List.GetDescriptionFromCode(E2_State);
			}
			set
			{
				var code = (ZString)Lookups.State_List.GetCodeFromDescription(value);
				E2_State = string.IsNullOrEmpty(code) ? value : code;
			}
		}

		public int State_MaxLength
		{
			get { return AutoJobDocAddress.Schema.E2_StateMaxLength; }
		}

		public ZString StateCode
		{
			get { return E2_State; }
			set
			{
				E2_State = value;
				StateCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StateCodeInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(GetZPropertyInfo(nameof(StateCode))); }
		}

		public int StateCode_MaxLength
		{
			get { return AutoJobDocAddress.Schema.E2_StateMaxLength; }
		}

		public CodeDescriptionPairList StateCodeList
		{
			get { return Lookups.State_List; }
		}

		public ZString Postcode
		{
			get { return E2_Postcode; }
			set { E2_Postcode = value; }
		}

		public ZPropertyInfo PostcodeInfo
		{
			get { return E2_PostcodeInfo; }
		}

		public int Postcode_MaxLength
		{
			get { return Schema.E2_PostcodeMaxLength; }
		}

		public int PostcodeMaxLength
		{
			get { return AutoJobDocAddress.Schema.E2_PostcodeMaxLength; }
		}

		ZString ISupportWebAddressValidation.CountryCodeISO2
		{
			get { return E2_RN_NKCountryCode; }
			set { E2_RN_NKCountryCode = value; }
		}

		public int CountryCodeISO2_MaxLength => Schema.E2_RN_NKCountryCodeMaxLength;

		public RefCountryCollection CountryCodeList
		{
			get { return Lookups.Countries; }
		}

		//JobDocAddress doesnt support multiple language for now
		[BusinessObjectTestExclude]
		public ZString Language
		{
			get { return Constants.Languages.English; }
			set { }
		}

		public ZPropertyInfo LanguageInfo
		{
			get { return GetInfoWithUpdatedHumanReadableName(GetZPropertyInfo(nameof(Language))); }
		}

		//JobDocAddress doesnt support multiple language for now
		public int Language_MaxLength { get { return 0; } }

		//JobDocAddress doesnt support multiple language for now
		public CodeDescriptionPairList LanguageList { get { return null; } }

		public ZString CompanyName
		{
			get { return E2_CompanyName; }
			set { E2_CompanyName = value; }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return E2_CompanyNameInfo; }
		}

		public int CompanyName_MaxLength
		{
			get { return Schema.E2_CompanyNameMaxLength; }
		}

		public ZString DisplayText
		{
			get { return Address1; }
			set { }
		}

		public ZGuid EntityPK
		{
			get { return PK; }
		}

		public bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return false;
		}

		public ZString ValidationStatus
		{
			get
			{
				return E2_ValidationStatus;
			}
			set
			{
				ZString originalValue = E2_ValidationStatus;
				E2_ValidationStatus = value;
				PopulateAddressFieldThroughRequirement(E2_ValidationStatusInfo, originalValue);
				RaiseDocAddressChanged();
			}
		}

		public ZString AddressMap
		{
			get { return E2_AddressMap; }
			set { E2_AddressMap = value; }
		}

		public ZString AddressValidationRuleForCountry
		{
			get
			{
				var currentCountry = RefCountry.LoadFromCountryCode(Factory, E2_RN_NKCountryCode);
				return currentCountry != null ? currentCountry.RN_ValidationStatus : new ZString(ExternalAddressValidationRulesList.Codes.NotAvailable);
			}
		}

		public ZString Addressee
		{
			get { return E2_CompanyName; }
		}

		public ZGeography GeoLocation
		{
			get => E2_GeoLocation;
			set => E2_GeoLocation = value;
		}

		public ZString ClosestPort { get; set; }

		public AddressValidationSection ValidationSection
		{
			get
			{
				AddressValidationSection addressValidationSection;

				if (IsInAdminPanel)
				{
					addressValidationSection = AddressValidationSection.AdminPanel;
				}
				else if (E2_AddressOverride)
				{
					addressValidationSection = AddressValidationSection.OverrideAddress;
				}
				else
				{
					addressValidationSection = AddressValidationSection.OrganizationAddress;
				}

				return addressValidationSection;
			}
		}

		public bool IsInAdminPanel { get; set; }

		bool isManuallyVerifiedByUser;

		public void ResetValidationStatus(ZPropertyInfo propertyInfo)
		{
			if (isManuallyVerifiedByUser)
			{
				return;
			}

			if (E2_AddressOverride)
			{
				if (Env.Registry.EnableAddressValidationWebService)
				{
					if (ValidationStatus != AddressValidationStatus.CountryNotAvailable && Country != null && OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(Country.PK.ToGuid(), ValidationSection))
					{
						E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
						E2_AddressMap = string.Empty;
						var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
						if (!string.IsNullOrEmpty(registrationKey.SystemId))
						{
							RaiseWebServices(propertyInfo);
						}
					}
					else if (propertyInfo.Name == nameof(E2_RN_NKCountryCode))
					{
						E2_ValidationStatus = AddressValidationStatus.ToBeVerified;
						E2_AddressMap = string.Empty;
					}
				}
			}
		}

		public async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, WTG.AddressCleansing.Common.CleanseAction cleanseAction = WTG.AddressCleansing.Common.CleanseAction.ValidateAndSuggest)
		{
			return await AddressValidationService.ValidateAddressAsync(this, cancellationToken, false, cleanseAction);
		}

		public bool IsErrorSuppressed => E2_SuppressAddressValidationError;

		public event EventHandler AddressValidationStatusChanged;
		public event EventHandler TriggerWebAddressValidation;
		public event EventHandler TriggerWebGetCityTown;

		public void ClearWebAddressValidationHandler()
		{
			if (TriggerWebAddressValidation != null)
			{
				foreach (EventHandler item in TriggerWebAddressValidation.GetInvocationList())
				{
					TriggerWebAddressValidation -= item;
				}
			}
		}

		public void ClearWebGetCityTownHandler()
		{
			if (TriggerWebGetCityTown != null)
			{
				foreach (EventHandler item in TriggerWebGetCityTown.GetInvocationList())
				{
					TriggerWebGetCityTown -= item;
				}
			}
		}

		void RaiseAddressValidationStatusChanged()
		{
			if (AddressValidationStatusChanged != null)
			{
				AddressValidationStatusChanged(this, EventArgs.Empty);
			}
		}

		void RaiseWebServices(ZPropertyInfo propertyInfo)
		{
			if (!string.IsNullOrEmpty(E2_RN_NKCountryCode))
			{
				if (propertyInfo == E2_CityInfo || propertyInfo == E2_StateInfo || propertyInfo == E2_PostcodeInfo)
				{
					RaiseTriggerWebGetCityTown(propertyInfo);
				}
				else
				{
					RaiseTriggerWebAddressValidation(propertyInfo);
				}
			}
		}

		void RaiseTriggerWebAddressValidation(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebAddressValidation != null)
			{
				TriggerWebAddressValidation(this, new InfoEventArgs(propertyInfo));
			}
		}

		void RaiseTriggerWebGetCityTown(ZPropertyInfo propertyInfo)
		{
			if (TriggerWebGetCityTown != null)
			{
				TriggerWebGetCityTown(this, new InfoEventArgs(propertyInfo));
			}
		}

		public bool NeedValidation
		{
			get
			{
				if (Country != null)
				{
					if (!E2_Address1.IsEmpty && !E2_Postcode.IsEmpty && !E2_City.IsEmpty && !E2_State.IsEmpty && !E2_RN_NKCountryCode.IsEmpty)
					{
						if (!IsInDatabase || (E2_Address1Info.HasChanges || E2_Address2Info.HasChanges || E2_PostcodeInfo.HasChanges ||
												E2_CityInfo.HasChanges || E2_StateInfo.HasChanges || E2_RN_NKCountryCodeInfo.HasChanges))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public void PreValidationForAddressValidationService()
		{
			Validation.ValidateE2_Address1();
			Validation.ValidateE2_City();
			Validation.ValidateE2_RN_NKCountryCode();
			ValidatePostcodeAndStateForAddress();
		}

		public void ValidatePostcodeAndStateForAddress()
		{
			Validation.ValidateE2_Postcode();
			Validation.ValidateE2_State();
		}

		public bool IsUpdatingCityTown { get; set; }
		public bool IsValidatingAddress { get; set; }
		public bool IsExactPointFound { get; set; }
		public bool IsValidatedByBackgroundService { get; set; }
		public bool IsTSAKnownAddress => false;
		public bool IsMIDAddress => false;
		public bool IsJobDocAddress => true;

		#endregion

		#region ILocation Members

		ZString ILocation.Code
		{
			get { return ZString.Empty; }
		}

		public async Task<WTG.AddressCleansing.Common.CandidateCityTown[]> GetCityTownAsync(CancellationTokenSource cancellationToken)
		{
			return await AddressValidationService.GetCityTownAsync(this, cancellationToken);
		}

		ZString ILocation.Description
		{
			get { return ZString.Empty; }
		}

		ZBool ILocation.IsActive
		{
			get { return true; }
		}

		RefCityTown ILocation.CityTown
		{
			get { return this.GetCityTown(E2_City, Factory); }
		}

		RefCountryStates ILocation.State
		{
			get { return !E2_AddressOverride && HasRealAddress ? Address.RelatedState : CountryStates; }
		}

		RefCountryStates CountryStates
		{
			get
			{
				if (!State.IsEmpty && Country != null)
				{
					var stateQuery = new ZQuery(RefCountryStatesSchema.RW_Code, E2_State);
					stateQuery.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, Country.RN_Code);

					return Factory.LoadTop1<RefCountryStates>(stateQuery);
				}

				return null;
			}
		}

		RefUNLOCO ILocation.UNLOCO
		{
			get { return !E2_AddressOverride && HasRealAddress ? Address.RelatedPortCode : null; }
		}

		IATACityCode ILocation.IATACityCode => ((ILocation)this).UNLOCO?.IATACityCode;

		RefZoneHeader[] ILocation.Zones => ((ILocation)((ILocation)this).UNLOCO)?.Zones ?? Array.Empty<RefZoneHeader>();

		bool ILocationReference.IsLocalInRelationTo(ZString code)
		{
			return false;
		}

		#endregion

		#region OnConcurrencyExceptionAfterMergeCore

		protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			base.OnConcurrencyExceptionAfterMergeCore(propertyRecords);
			if (!IsRowDeletedOrDetachedOrNull && !E2_AddressOverride && E2_ValidationStatus != AddressValidationStatus.NotRequired)
			{
				E2_ValidationStatus = AddressValidationStatus.NotRequired;
			}
		}

		#endregion

		#region DuplicatedRecords

		public List<JobDocAddress> AllDuplicatedRecords
		{
			get
			{
				var allDuplicatedRecords = Factory.Load<JobDocAddress>(QueryForSearchDuplicates).ToList();
				return allDuplicatedRecords;
			}
		}

		ZDBOnlyQuery QueryForSearchDuplicates
		{
			get
			{
				var queryForSearchDuplicates = new ZDBOnlyQuery(typeof(JobDocAddress));
				queryForSearchDuplicates.AddToFilter(JobDocAddressSchema.E2_Address1, Address1);
				queryForSearchDuplicates.AddToFilter(JobDocAddressSchema.E2_Address2, Address2);
				queryForSearchDuplicates.AddToFilter(JobDocAddressSchema.E2_RN_NKCountryCode, E2_RN_NKCountryCode);
				queryForSearchDuplicates.AddToFilter(JobDocAddressSchema.E2_City, City);
				queryForSearchDuplicates.AddToFilter(JobDocAddressSchema.E2_Postcode, Postcode);
				queryForSearchDuplicates.AddToFilter(JobDocAddressSchema.E2_State, StateCode);
				queryForSearchDuplicates.AddToFilter(JobDocAddressSchema.E2_ValidationStatus, StatusList);
				return queryForSearchDuplicates;
			}
		}

		List<string> StatusList
		{
			get
			{
				if (statusList == null)
				{
					statusList = new List<string>
					{
						AddressValidationStatus.Invalid,
						AddressValidationStatus.ToBeVerified,
						AddressValidationStatus.CountryNotAvailable,
						AddressValidationStatus.ManuallyVerified,
						AddressValidationStatus.Unverifiable,
						AddressValidationStatus.VerifiedToStreet
					};
				}
				return statusList;
			}
		}

		List<string> statusList;
		#endregion

		#region Test Helpers
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			E2_ValidationStatus = AddressValidationStatus.Verified;
			E2_AddressOverride = true;
			E2_ParentID = new ZGuid("12345678-90AB-CDEF-FEDC-BA0987654321");
			E2_ParentTableCode = "Z0";
		}

#endif
		#endregion

		#region Logging

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				return Res.GetString("6537da60-7f90-4721-9db9-0c10d64eb428", "Job Doc Address");
			}
		}

		#endregion

		#region E2_AddressSequence

		public static void ReinitializeAddressSequenceNumber(IEnumerable<JobDocAddress> docAddresses)
		{
			var addressGroups = docAddresses.Where(x => !x.IsRowDeletedOrDetachedOrNull).GroupBy(x => x.E2_AddressType);
			foreach (var addressGroup in addressGroups)
			{
				var orderedAddresses = addressGroup.OrderBy(x => x.E2_AddressSequence).ToList();
				if (orderedAddresses[0].E2_AddressSequence != 0)
				{
					orderedAddresses[0].E2_AddressSequence = 0;
				}
			}
		}

		#endregion

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => IsDataVersionsAutoLogged;
		protected virtual bool IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion
	}
}
