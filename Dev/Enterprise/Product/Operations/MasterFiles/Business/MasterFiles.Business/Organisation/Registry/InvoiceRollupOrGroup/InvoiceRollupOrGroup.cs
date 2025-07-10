using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class InvoiceRollupOrGroup : RegistryBusinessObjectTemplate, IInvoiceRollupOrGroup, IDocRollupOrGroupForBestMatcher
	{
		#region Schema

		public abstract class Schema
		{
			public const string GroupOrSubTotal = "GroupOrSubTotal";
			public const string GroupOrSubtotalStyle = "GroupOrSubtotalStyle";
			public const string InvoiceLineDisplayOption = "InvoiceLineDisplayOption";
			public const string InvoicePostingStyle = "InvoicePostingStyle";
			public const string JobType = "JobType";
			public const string ServiceDirection = "ServiceDirection";
			public const string TransportMode = "TransportMode";
			public const string ServiceLevel = "ServiceLevel";
			public const string InvoicePostingCurrency = "InvoicePostingCurrency";
		}

		#endregion

		public InvoiceRollupOrGroup()
		{
		}

		public InvoiceRollupOrGroup(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceRollupOrGroup(fallbackLevel, factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateInvoicePostingStyle();
			ValidateInvoiceLineDisplayOption();
			ValidateJobType();
			ValidateServiceDirection();
			ValidateTransportMode();
			ValidateGroupOrSubtotalStyle();
			ValidateGroupOrSubTotal();
			ValidateInvoicePostingCurrency();
		}

		#region Bound Properties

		#region InvoicePostingStyle

		[List("InvoicePostingOptionsList")]
		[MaxLength(3)]
		public ZString InvoicePostingStyle
		{
			get { return fInvoicePostingStyle; }
			set
			{
				SetNonPersistentPropertyValue(InvoicePostingStyleInfo, ref fInvoicePostingStyle, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoicePostingStyle();
				}
			}
		}

		ZString fInvoicePostingStyle;

		protected bool InvoicePostingStyle_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.InvoicePostingStyle_ReadOnly; }
		}

		public ZPropertyInfo InvoicePostingStyleInfo
		{
			get { return GetZPropertyInfo(Schema.InvoicePostingStyle); }
		}

		public void ValidateInvoicePostingStyle()
		{
			InvoicePostingStyleInfo.ClearAllNotifications();

			InvoiceRollupOrGroupHelper.ValidateInvoicePostingStyle();
		}

		#endregion

		#region InvoiceLineDisplayOption

		[List("InvoiceLineDisplayOptionsList")]
		[MaxLength(3)]
		public ZString InvoiceLineDisplayOption
		{
			get { return fInvoiceLineDisplayOption; }
			set
			{
				SetNonPersistentPropertyValue(InvoiceLineDisplayOptionInfo, ref fInvoiceLineDisplayOption, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoiceLineDisplayOption();
				}
			}
		}

		ZString fInvoiceLineDisplayOption;

		protected bool InvoiceLineDisplayOption_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.InvoiceLineDisplayOption_ReadOnly; }
		}

		public ZPropertyInfo InvoiceLineDisplayOptionInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceLineDisplayOption); }
		}

		public void ValidateInvoiceLineDisplayOption()
		{
			InvoiceLineDisplayOptionInfo.ClearAllNotifications();

			InvoiceRollupOrGroupHelper.ValidateInvoiceLineDisplayOption();
		}

		#endregion

		#region JobType

		[List("JobTypeList")]
		[MaxLength(3)]
		public ZString JobType
		{
			get { return fJobType; }
			set
			{
				SetNonPersistentPropertyValue(JobTypeInfo, ref fJobType, value);

				InvoiceRollupOrGroupHelper.OnSettingJobType(value);

				if (!IsValidationSuspended)
				{
					ValidateJobType();
				}
			}
		}

		ZString fJobType;

		public ZPropertyInfo JobTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JobType); }
		}

		public void ValidateJobType()
		{
			JobTypeInfo.ClearAllNotifications();

			InvoiceRollupOrGroupHelper.ValidateJobType();

			CheckThatDefaultRecordExist(JobTypeInfo);
		}

		#endregion

		#region ServiceDirection

		[List("ServiceDirectionList")]
		[MaxLength(3)]
		public ZString ServiceDirection
		{
			get { return fServiceDirection; }
			set
			{
				SetNonPersistentPropertyValue(ServiceDirectionInfo, ref fServiceDirection, value);
				if (!IsValidationSuspended)
				{
					ValidateServiceDirection();
				}
			}
		}

		ZString fServiceDirection;

		public ZPropertyInfo ServiceDirectionInfo
		{
			get { return GetZPropertyInfo(Schema.ServiceDirection); }
		}

		public void ValidateServiceDirection()
		{
			ServiceDirectionInfo.ClearAllNotifications();

			InvoiceRollupOrGroupHelper.ValidateServiceDirection();
		}

		protected bool ServiceDirection_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.ServiceDirection_ReadOnly; }
		}

		#endregion

		#region TransportMode

		[List("TransportModeList")]
		[MaxLength(3)]
		public ZString TransportMode
		{
			get { return fTransportMode; }
			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref fTransportMode, value);
				if (!IsValidationSuspended)
				{
					ValidateTransportMode();
				}
			}
		}

		ZString fTransportMode;

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		public void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();

			InvoiceRollupOrGroupHelper.ValidateTransportMode();
		}

		protected bool TransportMode_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.TransportMode_ReadOnly; }
		}

		#endregion

		#region ServiceLevel

		[List("ServiceLevelList")]
		[MaxLength(3)]
		public ZString ServiceLevel
		{
			get => fServiceLevel;
			set
			{
				SetNonPersistentPropertyValue(ServiceLevelInfo, ref fServiceLevel, value);
				if (!IsValidationSuspended)
				{
					ValidateServiceLevel();
				}
			}
		}

		ZString fServiceLevel;

		public ZPropertyInfo ServiceLevelInfo => GetZPropertyInfo(Schema.ServiceLevel);

		public void ValidateServiceLevel()
		{
			ServiceLevelInfo.ClearAllNotifications();
			InvoiceRollupOrGroupHelper.ValidateServiceLevel();
		}

		protected bool ServiceLevel_ReadOnly => InvoiceRollupOrGroupHelper.ServiceLevel_ReadOnly;

		#endregion

		#region GroupOrSubtotalStyle

		[List("GroupOrSubTotalStyleList")]
		[MaxLength(3)]
		public ZString GroupOrSubtotalStyle
		{
			get { return fGroupOrSubtotalStyle; }
			set
			{
				SetNonPersistentPropertyValue(GroupOrSubtotalStyleInfo, ref fGroupOrSubtotalStyle, value);
				if (!IsValidationSuspended)
				{
					ValidateGroupOrSubtotalStyle();
				}
			}
		}

		ZString fGroupOrSubtotalStyle;

		public ZPropertyInfo GroupOrSubtotalStyleInfo
		{
			get { return GetZPropertyInfo(Schema.GroupOrSubtotalStyle); }
		}

		protected bool GroupOrSubtotalStyle_ReadOnly
		{
			get { return InvoiceRollupOrGroupHelper.GroupOrSubtotalStyle_ReadOnly; }
		}

		public void ValidateGroupOrSubtotalStyle()
		{
			GroupOrSubtotalStyleInfo.ClearAllNotifications();

			InvoiceRollupOrGroupHelper.ValidateGroupOrSubtotalStyle();
		}

		#endregion

		#region GroupOrSubTotal

		[List("GroupOrSubTotalList")]
		[MaxLength(3)]
		public ZString GroupOrSubTotal
		{
			get { return fGroupOrSubTotal; }
			set
			{
				if (fGroupOrSubTotal != value)
				{
					SetNonPersistentPropertyValue(GroupOrSubTotalInfo, ref fGroupOrSubTotal, value);

					if (!IsValidationSuspended)
					{
						ValidateGroupOrSubTotal();
					}
				}
			}
		}
		ZString fGroupOrSubTotal;

		public ZPropertyInfo GroupOrSubTotalInfo
		{
			get { return GetZPropertyInfo(Schema.GroupOrSubTotal); }
		}

		public void ValidateGroupOrSubTotal()
		{
			GroupOrSubTotalInfo.ClearAllNotifications();

			InvoiceRollupOrGroupHelper.ValidateGroupOrSubTotal();
		}

		#endregion

		#region InvoicePostingCurrency

		[List("InvoicePostingCurrencies")]
		[MaxLength(3)]
		public ZString InvoicePostingCurrency
		{
			get { return fInvoicePostingCurrency; }
			set
			{
				SetNonPersistentPropertyValue(InvoicePostingCurrencyInfo, ref fInvoicePostingCurrency, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoicePostingCurrency();
				}
			}
		}

		ZString fInvoicePostingCurrency;

		public ZPropertyInfo InvoicePostingCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.InvoicePostingCurrency); }
		}

		public void ValidateInvoicePostingCurrency()
		{
			InvoicePostingCurrencyInfo.ClearAllNotifications();
			InvoiceRollupOrGroupHelper.ValidateInvoicePostingCurrency();
		}

		#endregion

		string IOrgInvoiceType.DuplicateRowErrorMessage
		{
			get { return InvoiceRollupOrGroupHelper.DuplicateRowErrorMessage; }
		}

		#region Implementation

		void CheckThatDefaultRecordExist(ZPropertyInfo propertyInfo)
		{
			if (ParentCollection != null)
			{
				bool isDefaultExist = false;
				foreach (InvoiceRollupOrGroup rollupOrGroup in ParentCollection)
				{
					if (rollupOrGroup.JobType == OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code &&
						rollupOrGroup.ServiceDirection == OrgConstants.ServiceDirection.Code.All && rollupOrGroup.TransportMode == OrgConstants.ModesForGroupOrSubTotal.Codes.All)
					{
						isDefaultExist = true;
						break;
					}
				}
				if (!isDefaultExist)
				{
					string errorMessage = Res.GetString("24072288-74AF-400d-90F6-AD8E4589F5B8", "You must always have a row with Job = All, Direction = All, Mode = All.");
					propertyInfo.AddError(errorMessage);
				}
			}
		}

		public BusinessObjectCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (InvoiceRollupOrGroupCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList InvoicePostingOptionsList
		{
			get { return InvoiceRollupOrGroupHelper.InvoicePostingOptionsList; }
		}

		public CodeDescriptionPairList InvoiceLineDisplayOptionsList
		{
			get { return InvoiceRollupOrGroupHelper.InvoiceLineDisplayOptionsList; }
		}

		public CodeDescriptionPairList JobTypeList
		{
			get { return InvoiceRollupOrGroupHelper.JobTypeList; }
		}

		public CodeDescriptionPairList TransportModeList
		{
			get { return InvoiceRollupOrGroupHelper.TransportModeList; }
		}

		public CodeDescriptionPairList ServiceDirectionList
		{
			get { return InvoiceRollupOrGroupHelper.ServiceDirectionList; }
		}

		public CodeDescriptionPairList GroupOrSubTotalList
		{
			get { return InvoiceRollupOrGroupHelper.GroupOrSubTotalList; }
		}

		public CodeDescriptionPairList GroupOrSubTotalStyleList
		{
			get { return InvoiceRollupOrGroupHelper.GroupOrSubTotalStyleList; }
		}

		public RefCurrencyCollection InvoicePostingCurrencies
		{
			get { return InvoiceRollupOrGroupHelper.InvoicePostingCurrencies; }
		}

		public CodeDescriptionPairList ServiceLevelList => InvoiceRollupOrGroupHelper.ServiceLevelList;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.GroupOrSubTotal, GroupOrSubTotal.ToString());
			writer.WriteElementString(Schema.GroupOrSubtotalStyle, GroupOrSubtotalStyle.ToString());
			writer.WriteElementString(Schema.InvoiceLineDisplayOption, InvoiceLineDisplayOption.ToString());
			writer.WriteElementString(Schema.InvoicePostingStyle, InvoicePostingStyle.ToString());
			writer.WriteElementString(Schema.JobType, JobType.ToString());
			writer.WriteElementString(Schema.ServiceDirection, ServiceDirection.ToString());
			writer.WriteElementString(Schema.TransportMode, TransportMode.ToString());
			writer.WriteElementString(Schema.ServiceLevel, ServiceLevel.ToString());
			writer.WriteElementString(Schema.InvoicePostingCurrency, InvoicePostingCurrency.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			GroupOrSubTotal = new ZString(reader.ReadElementString(Schema.GroupOrSubTotal));
			GroupOrSubtotalStyle = new ZString(reader.ReadElementString(Schema.GroupOrSubtotalStyle));
			InvoiceLineDisplayOption = new ZString(reader.ReadElementString(Schema.InvoiceLineDisplayOption));
			InvoicePostingStyle = new ZString(reader.ReadElementString(Schema.InvoicePostingStyle));
			JobType = new ZString(reader.ReadElementString(Schema.JobType));
			ServiceDirection = new ZString(reader.ReadElementString(Schema.ServiceDirection));
			TransportMode = new ZString(reader.ReadElementString(Schema.TransportMode));
			ServiceLevel = new ZString(reader.ReadElementString(Schema.ServiceLevel));
			InvoicePostingCurrency = new ZString(reader.ReadElementString(Schema.InvoicePostingCurrency));
		}

		#endregion

		#region IRollupOrGroupForBestMatch

		string IDocRollupOrGroupForBestMatcher.JobType => JobType;
		string IDocRollupOrGroupForBestMatcher.ServiceDirection => ServiceDirection;
		string IDocRollupOrGroupForBestMatcher.TransportMode => TransportMode;

		bool IDocRollupOrGroupForBestMatcher.HasServiceDirection => true;

		#endregion

		InvoiceRollupOrGroupHelper InvoiceRollupOrGroupHelper
		{
			get { return invoiceRollupOrGroupHelper_innerValue ?? (invoiceRollupOrGroupHelper_innerValue = new InvoiceRollupOrGroupHelper(this)); }
		}
		InvoiceRollupOrGroupHelper invoiceRollupOrGroupHelper_innerValue;
	}
}
