using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	#region RequiredConstants

	public static class RequiredConstants
	{
		public static MultilingualString RequiredFrom
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|RequiredFrom", "Required From"); }
		}

		public static MultilingualString RequiredTo
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|RequiredTo", "Required To"); }
		}

		public static MultilingualString FCLAvailable
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|CTOAvailable", "CTO Available"); }
		}

		public static MultilingualString FCLStorage
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|CTOStorage", "CTO Storage Start"); }
		}

		public static MultilingualString FCLReceive
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|CTOReceive", "CTO Receival Start"); }
		}

		public static MultilingualString FCLCutOff
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|CTOCutOff", "CTO Cut Off"); }
		}

		public static MultilingualString LCLAvailable
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|CFSAvailable", "CFS Available"); }
		}

		public static MultilingualString LCLStorage
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|CFSStorage", "CFS Storage Start"); }
		}

		public static MultilingualString LCLReceive
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|CFSReceive", "CFS Receival Start"); }
		}

		public static MultilingualString LCLCutOff
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|CFSCutOff", "CFS Cut Off"); }
		}

		public static MultilingualString EmptyReturnBy
		{
			get { return ResString.GetMultilingualString("TransportBooking|DateAndReferenceRegistry|EmptyReturnBy", "Return By"); }
		}
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class DateAndReference : RegistryBusinessObjectTemplate
	{
		protected DateAndReferenceCollection GetParentCollection()
		{
			return (DateAndReferenceCollection)GetParentCollection(this, typeof(DateAndReferenceCollection));
		}

		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string EnglishDescription = "EnglishDescription";
			public const string OrganisationType = "OrganisationType";
			public const string ContainerMode = "ContainerMode";

			public const string AllowActualDate = "AllowActualDate";
			public const string AllowEstimatedDate = "AllowEstimatedDate";
			public const string AllowSlotDate = "AllowSlotDate";
			public const string AllowRequiredFromDate = "AllowRequiredFromDate";
			public const string AllowRequiredFromLabel = "AllowRequiredFromLabel";
			public const string EnglishAllowRequiredFromLabel = "EnglishAllowRequiredFromLabel";
			public const string AllowRequiredToDate = "AllowRequiredToDate";
			public const string AllowRequiredToLabel = "AllowRequiredToLabel";
			public const string EnglishAllowRequiredToLabel = "EnglishAllowRequiredToLabel";
			public const string AllowReference = "AllowReference";
			public const string AllowReceivedBy = "AllowReceivedBy";
			public const string AutoAdd = "AutoAdd";
			public const string BookingDirection = "BookingDirection";
			public const string InstructionType = "InstructionType";

			public const string IsSystemDefined = "IsSystemDefined";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new DateAndReference();
			using (clone.GetValidationSuspender())
			{
				clone.Code = Code;
				clone.Description = Description;

				clone.BookingDirection = BookingDirection;
				clone.InstructionType = InstructionType;
				clone.OrganisationType = OrganisationType;
				clone.ContainerMode = ContainerMode;

				clone.AutoAdd = AutoAdd;
				clone.AllowActualDate = AllowActualDate;
				clone.AllowEstimatedDate = AllowEstimatedDate;
				clone.AllowSlotDate = AllowSlotDate;
				clone.AllowRequiredFromDate = AllowRequiredFromDate;
				clone.AllowRequiredFromLabel = AllowRequiredFromLabel;
				clone.AllowRequiredToDate = AllowRequiredToDate;
				clone.AllowRequiredToLabel = AllowRequiredToLabel;
				clone.AllowReference = AllowReference;
				clone.AllowReceivedBy = AllowReceivedBy;

				clone.IsSystemDefined = IsSystemDefined;
			}
			return clone;
		}

		#endregion

		#region Properties

		#region Code

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[ResourceStringData("5cf92b60-a21e-48ec-8250-ada0d5f1dff9", ShortCaption = "Conf. Code", Caption = "Confirmation Code")]
		[MaxLength(3)]
		public ZString Code
		{
			get { return code; }
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);
				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		ZString code;

		#endregion

		#region Description

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[MaxLength(MaxDescriptionLength)]
		public MultilingualString Description
		{
			get { return description ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(DescriptionInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				EnglishDescriptionInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		//

		[MaxLength(MaxDescriptionLength)]
		[ResourceStringData("d4b5ce7f-42b1-4924-a5fe-10c445477238", ShortCaption = "Conf. Desc.", Caption = "Confirmation Description")]
		public virtual ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set { Description = (NoResString)value; }
		}

		public virtual ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDescription); }
		}

		MultilingualString description;
		internal const int MaxDescriptionLength = 60;

		#endregion

		#region BookingDirection

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[List("BookingDirections")]
		[ResourceStringData("435c8c0c-8b86-482f-9f4e-1ab34e9b3145", ShortCaption = "Dir.", MediumCaption = "Direction", Caption = "Booking Direction")]
		[MaxLength(3)]
		public ZString BookingDirection
		{
			get { return bookingDirection; }
			set
			{
				SetNonPersistentPropertyValue(BookingDirectionInfo, ref bookingDirection, value);
				if (!IsValidationSuspended)
				{
					ValidateBookingDirection();
				}
			}
		}

		ZString bookingDirection;

		public ZPropertyInfo BookingDirectionInfo
		{
			get { return GetZPropertyInfo(Schema.BookingDirection); }
		}

		#endregion

		#region InstructionType

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[List("InstructionTypes")]
		[ResourceStringData("8e83b9ef-8a6d-48fc-9e3a-66b37d797071", ShortCaption = "Ins.", MediumCaption = "Instruction", Caption = "Instruction Type")]
		[MaxLength(3)]
		public ZString InstructionType
		{
			get { return instructionType; }
			set
			{
				SetNonPersistentPropertyValue(InstructionTypeInfo, ref instructionType, value, true);
				if (!IsValidationSuspended)
				{
					ValidateInstructionType();
				}
			}
		}

		ZString instructionType;

		public ZPropertyInfo InstructionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.InstructionType); }
		}

		#endregion

		#region OrganisationType

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[List("OrganisationTypes")]
		[ResourceStringData("d4a9e326-4bda-4896-aed1-406a132656f8", ShortCaption = "Org.", MediumCaption = "Organization", Caption = "Organization Type")]
		[MaxLength(80)]
		public ZString OrganisationType
		{
			get { return organisationType; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationTypeInfo, ref organisationType, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganisationType();
				}
			}
		}

		ZString organisationType;

		public ZPropertyInfo OrganisationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationType); }
		}

		#endregion

		#region ContainerMode

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[List("ContainerModes")]
		[ResourceStringData("2bf10971-459d-441d-8205-bade6a5c03cc", Caption = "Mode")]
		[MaxLength(3)]
		public ZString ContainerMode
		{
			get { return containerMode; }
			set
			{
				SetNonPersistentPropertyValue(ContainerModeInfo, ref containerMode, value);
				if (!IsValidationSuspended)
				{
					ValidateContainerMode();
				}
			}
		}

		ZString containerMode;

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerMode); }
		}

		#endregion

		#region AutoAdd

		[ResourceStringData("346f4d75-b6fc-41cb-a6f8-94a278ccfd04", ShortCaption = "Auto", Caption = "Auto Add")]
		public ZBool AutoAdd
		{
			get { return autoAdd; }
			set
			{
				SetNonPersistentPropertyValue(AutoAddInfo, ref autoAdd, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowFields(AutoAddInfo);
				}
			}
		}

		ZBool autoAdd;

		public ZPropertyInfo AutoAddInfo
		{
			get { return GetZPropertyInfo(Schema.AutoAdd); }
		}

		#endregion

		#region AllowActualDate

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[ResourceStringData("064b8ec7-81d2-4e14-a9b2-7d2369744a48", ShortCaption = "Act. Date", Caption = "Actual Date")]
		public ZBool AllowActualDate
		{
			get { return allowActualDate; }
			set
			{
				SetNonPersistentPropertyValue(AllowActualDateInfo, ref allowActualDate, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowFields(AllowActualDateInfo);
				}
			}
		}

		ZBool allowActualDate;

		public ZPropertyInfo AllowActualDateInfo
		{
			get { return GetZPropertyInfo(Schema.AllowActualDate); }
		}

		#endregion

		#region AllowEstimatedDate

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[ResourceStringData("d2a96350-6c71-4fe4-a948-d7fb9e09afc4", ShortCaption = "Est. Date", Caption = "Estimated Date")]
		public ZBool AllowEstimatedDate
		{
			get { return allowEstimatedDate; }
			set
			{
				SetNonPersistentPropertyValue(AllowEstimatedDateInfo, ref allowEstimatedDate, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowFields(AllowEstimatedDateInfo);
				}
			}
		}

		ZBool allowEstimatedDate;

		public ZPropertyInfo AllowEstimatedDateInfo
		{
			get { return GetZPropertyInfo(Schema.AllowEstimatedDate); }
		}

		#endregion

		#region AllowSlotDate

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[ResourceStringData("68e08187-43b1-4c94-aa4f-7a1effc447a0", Caption = "Slot Date")]
		public ZBool AllowSlotDate
		{
			get { return allowSlotDate; }
			set
			{
				SetNonPersistentPropertyValue(AllowSlotDateInfo, ref allowSlotDate, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowFields(AllowSlotDateInfo);
				}
			}
		}

		ZBool allowSlotDate;

		public ZPropertyInfo AllowSlotDateInfo
		{
			get { return GetZPropertyInfo(Schema.AllowSlotDate); }
		}

		#endregion

		#region AllowRequiredFromDate

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[ResourceStringData("25e84502-7dd9-4172-a4ff-94dc8d472e04", ShortCaption = "Req. From", Caption = "Required From Date")]
		public ZBool AllowRequiredFromDate
		{
			get { return allowRequiredFromDate; }
			set
			{
				SetNonPersistentPropertyValue(AllowRequiredFromDateInfo, ref allowRequiredFromDate, value);

				if (!AllowRequiredFromDate)
				{
					AllowRequiredFromLabel = (NoResString)"";
				}
				else if (AllowRequiredFromLabel.IsEmpty)
				{
					AllowRequiredFromLabel = RequiredConstants.RequiredFrom;
				}

				if (!IsValidationSuspended)
				{
					ValidateAllowFields(AllowRequiredFromDateInfo);
				}
			}
		}

		ZBool allowRequiredFromDate;

		public ZPropertyInfo AllowRequiredFromDateInfo
		{
			get { return GetZPropertyInfo(Schema.AllowRequiredFromDate); }
		}

		#endregion

		#region AllowRequiredFromLabel

		[MaxLength(MaxAllowRequiredFromLabelLength)]
		public MultilingualString AllowRequiredFromLabel
		{
			get { return allowRequiredFromLabel ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(AllowRequiredFromLabelInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(AllowRequiredFromLabelInfo, ref allowRequiredFromLabel, value, false);
				EnglishAllowRequiredFromLabelInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AllowRequiredFromLabelInfo
		{
			get { return GetZPropertyInfo(Schema.AllowRequiredFromLabel); }
		}

		protected bool AllowRequiredFromLabel_ReadOnly
		{
			get { return !AllowRequiredFromDate; }
		}

		//

		[MaxLength(MaxAllowRequiredFromLabelLength)]
		[ResourceStringData("3cb2338e-d9d3-40e7-9999-f86ea34eb410", ShortCaption = "Req. From Label", Caption = "Required From Label")]
		public ZString EnglishAllowRequiredFromLabel
		{
			get { return AllowRequiredFromLabel.GetUnresolvedString(); }
			set { AllowRequiredFromLabel = (NoResString)value; }
		}

		public ZPropertyInfo EnglishAllowRequiredFromLabelInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishAllowRequiredFromLabel); }
		}

		MultilingualString allowRequiredFromLabel;
		internal const int MaxAllowRequiredFromLabelLength = 80;

		#endregion

		#region AllowRequiredToDate

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[ResourceStringData("91547bbd-6615-411c-a65d-958482a98611", ShortCaption = "Req. To", Caption = "Required To Date")]
		public ZBool AllowRequiredToDate
		{
			get { return allowRequiredToDate; }
			set
			{
				SetNonPersistentPropertyValue(AllowRequiredToDateInfo, ref allowRequiredToDate, value);

				if (!AllowRequiredToDate)
				{
					AllowRequiredToLabel = (NoResString)"";
				}
				else if (AllowRequiredToLabel.IsEmpty)
				{
					AllowRequiredToLabel = RequiredConstants.RequiredTo;
				}

				if (!IsValidationSuspended)
				{
					ValidateAllowFields(AllowRequiredToDateInfo);
				}
			}
		}

		ZBool allowRequiredToDate;

		public ZPropertyInfo AllowRequiredToDateInfo
		{
			get { return GetZPropertyInfo(Schema.AllowRequiredToDate); }
		}

		#endregion

		#region AllowRequiredToLabel

		[MaxLength(MaxAllowRequiredToLabelLength)]
		public MultilingualString AllowRequiredToLabel
		{
			get { return allowRequiredToLabel ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(AllowRequiredToLabelInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(AllowRequiredToLabelInfo, ref allowRequiredToLabel, value, false);
				EnglishAllowRequiredToLabelInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AllowRequiredToLabelInfo
		{
			get { return GetZPropertyInfo(Schema.AllowRequiredToLabel); }
		}

		protected bool AllowRequiredToLabel_ReadOnly
		{
			get { return !AllowRequiredToDate; }
		}

		//

		[MaxLength(MaxAllowRequiredToLabelLength)]
		[ResourceStringData("2922e351-7c6a-4886-b22c-d9462b86670d", ShortCaption = "Req. To Label", Caption = "Required To Label")]
		public ZString EnglishAllowRequiredToLabel
		{
			get { return AllowRequiredToLabel.GetUnresolvedString(); }
			set { AllowRequiredToLabel = (NoResString)value; }
		}

		public ZPropertyInfo EnglishAllowRequiredToLabelInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishAllowRequiredToLabel); }
		}

		MultilingualString allowRequiredToLabel;
		internal const int MaxAllowRequiredToLabelLength = 80;

		#endregion

		#region AllowReference

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[ResourceStringData("4eece24a-9834-47af-8de8-7cc6ebeebe25", ShortCaption = "Ref. #", Caption = "Reference Number")]
		public ZBool AllowReference
		{
			get { return allowReference; }
			set
			{
				SetNonPersistentPropertyValue(AllowReferenceInfo, ref allowReference, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowFields(AllowReferenceInfo);
				}
			}
		}

		ZBool allowReference;

		public ZPropertyInfo AllowReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.AllowReference); }
		}

		#endregion

		#region AllowReceivedBy

		[ReadOnlyMember(nameof(IsSystemDefined))]
		[ResourceStringData("7b1ff949-8eaf-44cb-92cb-220a36853361", ShortCaption = "Sign. By", Caption = "Signed By")]
		public ZBool AllowReceivedBy
		{
			get { return allowReceivedBy; }
			set
			{
				SetNonPersistentPropertyValue(AllowReceivedByInfo, ref allowReceivedBy, value);
				if (!IsValidationSuspended)
				{
					ValidateAllowFields(AllowReceivedByInfo);
				}
			}
		}

		ZBool allowReceivedBy;

		public ZPropertyInfo AllowReceivedByInfo
		{
			get { return GetZPropertyInfo(Schema.AllowReceivedBy); }
		}

		#endregion

		#region IsSystemDefined

		public ZBool IsSystemDefined
		{
			get { return isSystemDefined; }
			set { isSystemDefined = value; }
		}

		ZBool isSystemDefined;

		#endregion

		#endregion

		#region Validate

		#region ValidateCode

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);

			ValidateUniqueness(CodeInfo);
		}

		#endregion

		#region ValidateDescription

		public void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DescriptionInfo);
			ValidateForMatchingDescription(DescriptionInfo);
		}

		void ValidateForMatchingDescription(ZPropertyInfo info)
		{
			var result = this.GetParentCollection(this, typeof(DateAndReferenceCollection));
			if (result != null)
			{
				foreach (DateAndReference dateAndReference in result)
				{
					if (dateAndReference.Code == Code && !dateAndReference.Description.Equals(Description))
					{
						info.AddError(ResString.GetMultilingualString("5fd58e0d-8db9-4103-93b2-e161fa752f35",
							"This confirmation code {0} already exists with description {1}. Description must be unique per code.", dateAndReference.Code, dateAndReference.Description));
					}
				}
			}
		}

		#endregion

		#region ValidateBookingDirection

		public void ValidateBookingDirection()
		{
			BookingDirectionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BookingDirectionInfo);
			ListValidation.ErrorIfInvalidCode(BookingDirectionInfo);

			ValidateUniqueness(BookingDirectionInfo);
		}

		#endregion

		#region ValidateInstructionType

		public void ValidateInstructionType()
		{
			InstructionTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(InstructionTypeInfo);
			ListValidation.ErrorIfInvalidCode(InstructionTypeInfo);

			ValidateUniqueness(InstructionTypeInfo);
		}

		#endregion

		#region ValidateOrganisationType

		public void ValidateOrganisationType()
		{
			OrganisationTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrganisationTypeInfo);
			ListValidation.ErrorIfInvalidCode(OrganisationTypeInfo);

			ValidateUniqueness(OrganisationTypeInfo);
		}

		#endregion

		#region ValidateContainerMode

		public void ValidateContainerMode()
		{
			ContainerModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ContainerModeInfo);
			ListValidation.ErrorIfInvalidCode(ContainerModeInfo);

			ValidateUniqueness(ContainerModeInfo);
		}

		#endregion

		#region ValidateUniqueness

		void ValidateUniqueness(ZPropertyInfo info)
		{
			var result = this.GetParentCollection(this, typeof(DateAndReferenceCollection));
			if (result != null)
			{
				foreach (DateAndReference dateAndReference in result)
				{
					if (dateAndReference.Code == Code && dateAndReference.BookingDirection == BookingDirection && dateAndReference.InstructionType == InstructionType
						&& dateAndReference.OrganisationType == OrganisationType && dateAndReference.ContainerMode == ContainerMode && dateAndReference != this)
					{
						info.AddError(ResString.GetMultilingualString("73ce2cc0-1ce7-4921-8f5b-be42fcb70582", "Code with Booking Direction, Instruction Type, Organization Type and Mode must be unique."));
					}
				}
			}
		}

		#endregion

		#region ValidateAllowFields

		public void ValidateAllowFields(ZPropertyInfo info)
		{
			info.ClearAllNotifications();

			if (!AllowActualDate &&
				!AllowEstimatedDate &&
				!AllowSlotDate &&
				!AllowRequiredFromDate &&
				!AllowRequiredToDate &&
				!AllowReference &&
				!AllowReceivedBy)
			{
				info.AddError(ResString.GetMultilingualString("95ee44f2-44d0-4814-9e6f-89a909872a2b", "At least one attribute must be editable."));
			}
		}

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCode();
			ValidateDescription();
			ValidateForMatchingDescription(DescriptionInfo);
			ValidateBookingDirection();
			ValidateInstructionType();
			ValidateOrganisationType();
			ValidateContainerMode();
			ValidateAllowFields(AllowActualDateInfo);
			ValidateAllowFields(AllowEstimatedDateInfo);
			ValidateAllowFields(AllowSlotDateInfo);
			ValidateAllowFields(AllowRequiredFromDateInfo);
			ValidateAllowFields(AllowRequiredToDateInfo);
			ValidateAllowFields(AllowReferenceInfo);
			ValidateAllowFields(AllowReceivedByInfo);
		}

		#endregion

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return !IsSystemDefined; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("ba78c7a5-563d-44f1-8326-a5ef224aad78", "This is a system defined value and cannot be deleted."); }
		}

		#endregion

		#region Lists

		#region BookingDirections

		public CodeDescriptionPairList BookingDirections
		{
			get
			{
				if (directions == null)
				{
					directions = new CodeDescriptionPairList();
					directions.AddPair(DatesAndReference.Any, Res.GetString("902ca970-2ca6-4492-ac5c-908519e509fb", "Applies to ANY Booking Direction, unless overridden."));

					foreach (CodeDescriptionPair direction in new Directions().List)
					{
						directions.AddPair(direction.Code, Res.GetString("b2e9a64d-6680-4dba-947e-292cab16ed49", "Applies to '{0}' Only", direction.Description));
					}
				}

				return directions;
			}
		}
		CodeDescriptionPairList directions;

		#endregion

		#region InstructionTypes

		public CodeDescriptionPairList InstructionTypes
		{
			get
			{
				var instructionTypeCodeDescription = new CodeDescriptionPairList();
				instructionTypeCodeDescription.AddPair(DatesAndReference.Any, Res.GetString("55d26e13-93da-4a80-9db8-3c1514095fa2", "Applies to ANY Instruction Types, unless overridden."));

				foreach (CodeDescriptionPair jobInstructionType in new InstructionTypes().List)
				{
					instructionTypeCodeDescription.AddPair(jobInstructionType.Code, Res.GetString("19a5a19b-a3ee-499d-bcc7-31281869a522", "Applies to '{0}' Only", jobInstructionType.Description));
				}

				return instructionTypeCodeDescription;
			}
		}

		#endregion

		#region OrganisationTypes

		public CodeDescriptionPairList OrganisationTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(DatesAndReference.Any, Res.GetString("c221c6ea-d41f-43ea-b39f-5f202128b171", "Applies to ANY Organization Types, unless overridden."));

				foreach (CodeDescriptionPair orgType in OrganisationTypesList.Instance)
				{
					result.AddPair(orgType.Code, Res.GetString("19a5a19b-a3ee-499d-bcc7-31281869a522", "Applies to '{0}' Only", orgType.Description));
				}

				return result;
			}
		}

		#endregion

		#region ContainerModes

		public CodeDescriptionPairList ContainerModes
		{
			get
			{
				if (containerModes == null)
				{
					containerModes = new CodeDescriptionPairList();
					containerModes.AddPair(DatesAndReference.Any, Res.GetString("6A1383D3-F843-4853-8DA5-FBA4297E1A5C", "Applies to ANY Container Modes, unless overridden."));

					var containerModeDescription = Res.GetString("7038D77F-212D-4E4B-86CC-A5C14953F426", "Applies to Container Mode of '{0}' Only.", "{0}");
					containerModes.AddPair(Constants.CartageContainerMode.Loose, string.Format(Culture.Invariant, containerModeDescription, Constants.CartageContainerModeDescription.Loose));
					containerModes.AddPair(Constants.CartageContainerMode.Containerized, string.Format(Culture.Invariant, containerModeDescription, Constants.CartageContainerModeDescription.Containerized));
				}

				return containerModes;
			}
		}

		CodeDescriptionPairList containerModes;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, EnglishDescription);
			writer.WriteElementString(Schema.BookingDirection, BookingDirection);
			writer.WriteElementString(Schema.InstructionType, InstructionType);
			writer.WriteElementString(Schema.OrganisationType, OrganisationType);
			writer.WriteElementString(Schema.ContainerMode, ContainerMode);
			writer.WriteElementString(Schema.AutoAdd, AutoAdd.ToString());
			writer.WriteElementString(Schema.AllowActualDate, AllowActualDate.ToString());
			writer.WriteElementString(Schema.AllowEstimatedDate, AllowEstimatedDate.ToString());
			writer.WriteElementString(Schema.AllowSlotDate, AllowSlotDate.ToString());
			writer.WriteElementString(Schema.AllowRequiredFromDate, AllowRequiredFromDate.ToString());
			writer.WriteElementString(Schema.AllowRequiredFromLabel, EnglishAllowRequiredFromLabel);
			writer.WriteElementString(Schema.AllowRequiredToDate, AllowRequiredToDate.ToString());
			writer.WriteElementString(Schema.AllowRequiredToLabel, EnglishAllowRequiredToLabel);
			writer.WriteElementString(Schema.AllowReference, AllowReference.ToString());
			writer.WriteElementString(Schema.AllowReceivedBy, AllowReceivedBy.ToString());
			writer.WriteElementString(Schema.IsSystemDefined, IsSystemDefined.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			using (GetValidationSuspender())
			{
				Code = wrapper.ReadElementString(Schema.Code);
				EnglishDescription = wrapper.ReadElementString(Schema.Description);
				BookingDirection = wrapper.ReadElementString(Schema.BookingDirection);
				InstructionType = wrapper.ReadElementString(Schema.InstructionType);
				OrganisationType = wrapper.ReadElementString(Schema.OrganisationType);
				ContainerMode = wrapper.ReadElementString(Schema.ContainerMode);
				AutoAdd = wrapper.ReadElementStringAsZBool(Schema.AutoAdd);
				AllowActualDate = wrapper.ReadElementStringAsZBool(Schema.AllowActualDate);
				AllowEstimatedDate = wrapper.ReadElementStringAsZBool(Schema.AllowEstimatedDate);
				AllowSlotDate = wrapper.ReadElementStringAsZBool(Schema.AllowSlotDate);
				AllowRequiredFromDate = wrapper.ReadElementStringAsZBool(Schema.AllowRequiredFromDate);
				EnglishAllowRequiredFromLabel = wrapper.ReadElementString(Schema.AllowRequiredFromLabel);
				AllowRequiredToDate = wrapper.ReadElementStringAsZBool(Schema.AllowRequiredToDate);
				EnglishAllowRequiredToLabel = wrapper.ReadElementString(Schema.AllowRequiredToLabel);
				AllowReference = wrapper.ReadElementStringAsZBool(Schema.AllowReference);
				AllowReceivedBy = wrapper.ReadElementStringAsZBool(Schema.AllowReceivedBy);
				IsSystemDefined = wrapper.ReadElementStringAsZBool(Schema.IsSystemDefined);
			}
		}

		#endregion
	}
}
