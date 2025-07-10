using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class CashFlowActivityConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string EnglishDescription = "EnglishDescription";
			public const string ActivityType = "ActivityType";
			public const string ActivityDescription = "ActivityDescription";
			public const string EnglishActivityDescription = "EnglishActivityDescription";

			public const int CodeMaxLength = 3;
			public const int DescriptionMaxLength = 100;
			public const int ActivityTypeMaxLength = 1;
			public const int ActivityDescriptionMaxLength = 100;
		}

		static readonly MultilingualString IdenticalConfigurationExists = ResString.GetMultilingualString("40a6ce50-72ed-4e29-9ba3-948666d5b005", "cash flow type with identical code already exists.");
		static readonly MultilingualString ActivityTypeIsNotUnique = ResString.GetMultilingualString("07965c47-6969-4f13-b361-149b44362862", "Activity type X, N, C, E can only have 1 occurrence.");
		static readonly MultilingualString ReservedCodesAreNotAllowed = ResString.GetMultilingualString("34e58674-e043-4503-8540-039133fe7834", "'ZZZ' is not allowed as it is a reserved code for report use.");

		#endregion

		public CashFlowActivityConfiguration()
		{ }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clonedValue = new CashFlowActivityConfiguration();
			clonedValue.SetCustomizedDataCaptionSource(this.customizedDataCaptionSource);

			return clonedValue;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateCode();
			ValidateDescription();
			ValidateActivityType();
		}

		CashFlowActivityConfiguration[] ParentCollectionForValidation
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(CashFlowActivityConfigurationCollection));
				return parentCollection != null ? parentCollection.Cast<CashFlowActivityConfiguration>().ToArray() : System.Array.Empty<CashFlowActivityConfiguration>();
			}
		}

		#region Code

		[MaxLength(Schema.CodeMaxLength)]
		[ResourceStringData("141c4d15-5bb9-4827-85de-e528a162fdb8", Caption = "Code")]
		[List("Lookups.CodeList")]
		public ZString Code
		{
			get { return code; }
			set
			{
				var isChanged = SetNonPersistentPropertyValue(CodeInfo, ref code, value);
				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
				if (isChanged)
				{
					Description = CashFlowCodeLists.CashFlowTypeList.GetMultilingualDescriptionFromCode(Code);
				}
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo, (IMultilingualString)ResString.GetMultilingualString("65BD1C9C-DD13-488C-8C6B-94CC15ED2284", "cash flow code"));
			if (!CodeInfo.HasErrors())
			{
				if (Code.EqualsIgnoringCase(CashFlowCodeLists.ReservedReportCodes.ZZZ))
				{
					CodeInfo.AddError(ReservedCodesAreNotAllowed);
				}
			}
			if (!CodeInfo.HasErrors() && CheckIdenticalCodeExists())
			{
				CodeInfo.AddError(IdenticalConfigurationExists);
			}
		}

		public bool Code_ReadOnly
		{
			get
			{
				return Code.EqualsIgnoringCase(CashFlowCodeLists.Codes.XXX) ||
						Code.EqualsIgnoringCase(CashFlowCodeLists.Codes.NON) ||
						Code.EqualsIgnoringCase(CashFlowCodeLists.Codes.CSH) ||
						Code.EqualsIgnoringCase(CashFlowCodeLists.Codes.EXX);
			}
		}

		ZString code;

		#endregion

		#region Description

		[MaxLength(Schema.DescriptionMaxLength)]
		[ResourceStringData("040f3717-3b2a-49a9-b36f-91fee52d2c84", Caption = "Description")]
		public MultilingualString Description
		{
			get { return description ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
				DescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		public void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DescriptionInfo, (IMultilingualString)ResString.GetMultilingualString("D5194CEB-B394-4BB5-97E2-3CD4A1AB1D2A", "Description"));
			CheckMaximumLength(DescriptionInfo, Description.GetUnresolvedString());
		}

		[MaxLength(Schema.DescriptionMaxLength)]
		[BusinessObjectTestExclude]
		public ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set
			{
				if (EnglishDescription != value)
				{
					CheckMaximumLength(EnglishDescriptionInfo, value);
					Description = (NoResString)value;
				}
			}
		}

		public ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EnglishDescription); }
		}

		MultilingualString description;

		#endregion

		#region ActivityType

		[MaxLength(Schema.ActivityTypeMaxLength)]
		[ResourceStringData("ae82aaf6-97e6-4c66-a718-c46bcfe9afee", Caption = "Activity Type")]
		[List("Lookups.ActivityTypeList")]
		public ZString ActivityType
		{
			get { return activityType; }
			set
			{
				SetNonPersistentPropertyValue(ActivityTypeInfo, ref activityType, value);
				EnglishActivityDescription = ActivityType.IsEmpty ? ZString.Empty : (ZString)Lookups.ActivityTypeList.GetMultilingualDescriptionFromCode(ActivityType)?.GetUnresolvedString();
				if (!IsValidationSuspended)
				{
					ValidateActivityType();
				}
			}
		}

		public ZPropertyInfo ActivityTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ActivityType); }
		}

		public void ValidateActivityType()
		{
			ActivityTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ActivityTypeInfo, (IMultilingualString)ResString.GetMultilingualString("3738c6e4-d82c-4eb6-96d7-b39de6fbe45a", "Activity Type"));
			if (!ActivityTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ActivityTypeInfo, Lookups.ActivityTypeList);
			}
			if (!ActivityTypeInfo.HasErrors() && CheckActivityTypeUniqueness())
			{
				ActivityTypeInfo.AddError(ActivityTypeIsNotUnique);
			}
		}

		public bool ActivityType_ReadOnly
		{
			get { return ActivityType.ContainsAnyChar("XNCE") && !ActivityTypeInfo.HasErrors(); }
		}

		ZString activityType;

		#endregion

		#region ActivityDescription

		[MaxLength(Schema.ActivityDescriptionMaxLength)]
		[ResourceStringData("398904a5-ed07-49f1-816f-03d6a2ea7e0b", Caption = "Activity Description")]
		public MultilingualString ActivityDescription
		{
			get { return activityDescription ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(ActivityDescriptionInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(ActivityDescriptionInfo, ref activityDescription, value, false);
				ActivityDescriptionInfo.RefreshBinding();
			}
		}

		MultilingualString activityDescription;

		public ZPropertyInfo ActivityDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ActivityDescription); }
		}

		public void ValidateActivityDescription()
		{
			ActivityDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ActivityDescriptionInfo, (IMultilingualString)ResString.GetMultilingualString("0eb22bbe-ad84-4dd8-b3a0-892ab7b997bc", "Activity Description"));
		}

		[MaxLength(Schema.ActivityDescriptionMaxLength)]
		public ZString EnglishActivityDescription
		{
			get { return ActivityDescription.GetUnresolvedString(); }
			set
			{
				if (customizedDataCaptionSource != null)
				{
					ActivityDescription = CustomizableDataResourceStrings.GetMultilingualString(customizedDataCaptionSource, null, value);
				}
				else
				{
					ActivityDescription = (NoResString)value;
				}
			}
		}

		public void SetCustomizedDataCaptionSource(ICustomizableDataCaptionSource dataCaptionSource)
		{
			this.customizedDataCaptionSource = dataCaptionSource;
		}

		ICustomizableDataCaptionSource customizedDataCaptionSource;

		#endregion

		public override bool CanDelete
		{
			get { return !ActivityType.ContainsAnyChar("XNCE"); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("0FDA9E4D-F0D6-43FF-8901-C416187D8203", "You are not allowed to delete XXX, NON, CSH, EXX."); }
		}

		ZBool CheckIdenticalCodeExists()
		{
			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				foreach (CashFlowActivityConfiguration item in ParentCollectionForValidation)
				{
					if (PK != item.PK && Code.EqualsIgnoringCase(item.code))
					{
						result = ZBool.True;
						break;
					}
				}
			}
			return result;
		}

		ZBool CheckActivityTypeUniqueness()
		{
			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				foreach (CashFlowActivityConfiguration item in ParentCollectionForValidation)
				{
					if (PK != item.PK && ActivityType.EqualsIgnoringCase(item.ActivityType) &&
						(ActivityType.EqualsIgnoringCase(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Undefined) ||
						 ActivityType.EqualsIgnoringCase(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.NonCash) ||
						 ActivityType.EqualsIgnoringCase(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Cash) ||
						 ActivityType.EqualsIgnoringCase(CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Exchange)))
					{
						result = ZBool.True;
						break;
					}
				}
			}
			return result;
		}

		#region CashFlowActivityConfiguratonLookups

		public CashFlowActivityConfiguratonLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}

		protected CashFlowActivityConfiguratonLookups GetNewLookups()
		{
			return new CashFlowActivityConfiguratonLookups(this);
		}

		CashFlowActivityConfiguratonLookups lookups;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Description, EnglishDescription);
			writer.WriteElementString(Schema.ActivityType, ActivityType);
			writer.WriteElementString(Schema.ActivityDescription, ActivityDescription.GetUnresolvedString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			EnglishDescription = reader.ReadElementString(Schema.Description);
			ActivityType = reader.ReadElementString(Schema.ActivityType);
			EnglishActivityDescription = reader.ReadElementString(Schema.ActivityDescription);
		}

		#endregion
	}
}
