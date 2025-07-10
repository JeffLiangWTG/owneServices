using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(ZZRefCusRulingCombined.Schema.ZZX_RulingNumber)]
	public class ZZRefCusRulingCombined : AutoZZRefCusRulingCombined, ICodeDescription, IStmALogParent, ITemplateCopyable
	{
		public ZZRefCusRulingCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoZZRefCusRulingCombined.Schema
		{
			public const string RulingTypeDescription = "RulingTypeDescription";
		}

		public static readonly TypeDecider TypeDecider = new ZZRefCusRulingCombinedTypeDecider();

		#region New Properties

		public ZString RulingTypeDescription
		{
			get
			{
				return Lookups.RulingTypeList.GetDescriptionFromCode(ZZX_RulingType);
			}
		}

		public OrgAddress AppliesToAddress
		{
			get
			{
				return Factory.Load<OrgAddress>(ZZX_OA_AppliesTo);
			}
		}

		public ZString AppliesToAddressCountry
		{
			get
			{
				return AppliesToAddress == null ? ZString.Empty : AppliesToAddress.OA_RN_NKCountryCode;
			}
		}

		[ReadOnly(true)]
		public ZBool IsSystem
		{
			get { return ZZX_SystemCreateTimeUtc.IsEmpty; }
		}

		#endregion

		#region Collection

		[ChildEditable]
		public CusRulingConfigCombinedCollection Configurations
		{
			get
			{
				if (configurations == null)
				{
					configurations = GetConfigurationsCore();
					configurations.Load();
					RegisterEditableChildObject(configurations);
					configurations.SetReadOnlyIncludingChildren(ConfigurationsReadOnly);
				}
				return configurations;
			}
		}
		CusRulingConfigCombinedCollection configurations;

		protected virtual ZBool ConfigurationsReadOnly => ZBool.False;

		protected virtual CusRulingConfigCombinedCollection GetConfigurationsCore()
		{
			return new CusRulingConfigCombinedCollection(this);
		}

		#endregion

		#region Override Properties

		[ResourceStringData("013EF290-C68C-4809-B77F-20EFC395E409", Caption = "Ruling Type")]
		[List("Lookups.RulingTypeList")]
		public override ZString ZZX_RulingType
		{
			get { return base.ZZX_RulingType; }
			set
			{
				var oldValue = ZZX_RulingType;
				base.ZZX_RulingType = value;
				if (!IsCopying && oldValue != ZZX_RulingType)
				{
					Configurations.MarkAsNeedingValidation();
					Configurations.SetReadOnlyIncludingChildren(ConfigurationsReadOnly);
				}
			}
		}

		[ResourceStringData("D40F0926-8207-4E59-88AE-FE87EBA4D784", Caption = "Ruling Number")]
		public override ZString ZZX_RulingNumber { get => base.ZZX_RulingNumber; set => base.ZZX_RulingNumber = value; }

		[List("ZZX_OA_AppliesTo_ZAddress.OrgAddress_List")]
		public override ZGuid ZZX_OA_AppliesTo
		{
			get { return base.ZZX_OA_AppliesTo; }
			set { base.ZZX_OA_AppliesTo = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress ZZX_OA_AppliesTo_ZAddress
		{
			get
			{
				if (fZZX_OA_AppliesTo_ZAddress == null)
				{
					fZZX_OA_AppliesTo_ZAddress = new ZAddress(ZZX_OA_AppliesToInfo);
					fZZX_OA_AppliesTo_ZAddress.IsOrgVisible = true;
					fZZX_OA_AppliesTo_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(x => ((OrgHeader)x)?.MainAddress?.PK ?? ZGuid.Empty);
				}
				return fZZX_OA_AppliesTo_ZAddress;
			}
		}
		ZAddress fZZX_OA_AppliesTo_ZAddress;

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public override bool ReadOnly
		{
			get { return IsSystem || base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				if (humanReadableShortcutNameCached == null)
				{
					humanReadableShortcutNameCached = new CachedProperty<ZString>(Factory, delegate
					{
						var result = HumanReadableShortcutNameCorePrefix;
						var org = AppliesToAddress?.Header;
						if (org != null)
						{
							result += " " + Res.GetString("52732181-68CF-4272-88DC-4A08B40C12D2", "- {0}", org.OH_Code);
						}
						return result;
					});
				}
				return humanReadableShortcutNameCached.Value;
			}
		}
		CachedProperty<ZString> humanReadableShortcutNameCached;

		protected virtual ZString HumanReadableShortcutNameCorePrefix => Res.GetString("2790D52C-ECAD-4167-9835-DBBC4D59377D", "Ruling - {0}", ZZX_RulingNumber);

		#endregion

		#region Override Method

		public override void Delete()
		{
			Configurations.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(ZZRefCusRulingCombined cusRuling)
				: base(cusRuling)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusRulingConfigCombinedSchema.ZZY_ZZX_CusRuling, BusinessObject.PK);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZZX_StartDate = ZDate.Today;
			ZZX_EndDate = ZDateTime.MaxSmallDateTimeValue.Date;
			ZZX_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ZZX_SystemCreateTimeUtc = ZDateTime.UtcNow;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (ZZRefCusRulingCombined)base.CloneInternal(args);
			using (result.GetValidationSuspender())
			{
				foreach (CusRulingConfigCombined config in Configurations)
				{
					result.Configurations.Add((CusRulingConfigCombined)config.Clone());
				}
			}
			return result;
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !IsSystem; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return IsSystem ? CannotDeleteSystemGenerated : (NoResString)string.Empty; }
		}

		internal static MultilingualString CannotDeleteSystemGenerated
		{
			get { return ResString.GetMultilingualString("2E3E32A4-A847-4900-AA77-6666692A9AEA", "You cannot delete a system-generated record."); }
		}

		#endregion

		#region IStmALogParent Members

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		string IStmALogParent.LogsParentTableName
		{
			get { return ZZRefCusRulingCombined.Schema.TableName; }
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			var result = (ZZRefCusRulingCombined)Clone();
			return result;
		}

		#endregion

#if DEBUG

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ZZX_Description = "11111 Des";
			ZZX_RulingNumber = "11111";
			ZZX_RulingType = RefCusRulingTypeList.Codes._2;
			ZZX_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

#endif
	}
}
