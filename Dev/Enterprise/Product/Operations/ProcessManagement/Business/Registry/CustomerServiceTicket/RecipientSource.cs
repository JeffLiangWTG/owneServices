using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	[XmlSerializerAssembly("Enterprise.ProcessManagement.Business.XmlSerializers")]
	public class RecipientSource : RegistryBusinessObjectTemplate
	{
		#region Properties

		#region FallbackSequence

		[ResourceStringData("RecipientSource.FallbackSequence", Caption = "Sequence", ShortCaption = "Seq.", FullDescription = "The sequence used when locating email addresses to send eConversation notifications (smallest to largest).")]
		public ZInt FallbackSequence
		{
			get => fallbackSequence;
			set
			{
				SetNonPersistentPropertyValue(FallbackSequenceInfo, ref fallbackSequence, value);

				if (!IsValidationSuspended)
				{
					ValidateFallbackSequence();
				}
			}
		}

		ZInt fallbackSequence;

		public ZPropertyInfo FallbackSequenceInfo => GetZPropertyInfo(nameof(FallbackSequence));

		#endregion

		#region SourceType

		[MaxLength(3)]
		[List(nameof(SourceTypeList))]
		[ResourceStringData("RecipientSource.SourceType", Caption = "Source Type", FullDescription = "The source from which to find email addresses to send eConversation notifications.")]
		public ZString SourceType
		{
			get => sourceType;
			set
			{
				SetNonPersistentPropertyValue(SourceTypeInfo, ref sourceType, value);

				if (!IsValidationSuspended)
				{
					ValidateSourceType();
				}
			}
		}

		ZString sourceType;

		public ZPropertyInfo SourceTypeInfo => GetZPropertyInfo(nameof(SourceType));

		public ICodeDescriptionPairList SourceTypeList => new RecipientSourceTypeList();

		#endregion

		#region SourceTypeDescription

		[ResourceStringData("RecipientSource.SourceTypeDescription", Caption = "Source Description")]
		public ZString SourceTypeDescription => SourceTypeList.GetDescriptionFromCode(SourceType);

		public ZPropertyInfo SourceTypeDescriptionInfo => GetZPropertyInfo(nameof(SourceTypeDescription));

		#endregion

		[BusinessObjectTestExclude] // There are null checks where this could be null.
		public RecipientSourceCollection ParentCollection { get; internal set; }

		#endregion

		#region Validation

		public void ValidateFallbackSequence()
		{
			if (ParentCollection != null)
			{
				FallbackSequenceInfo.ClearAllNotifications();

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(FallbackSequenceInfo, ParentCollection);
			}
		}

		public void ValidateSourceType()
		{
			if (ParentCollection != null)
			{
				SourceTypeInfo.ClearAllNotifications();

				MandatoryValidation.CheckEntered(SourceTypeInfo);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(SourceTypeInfo, ParentCollection);
			}
		}

		#endregion

		#region RegistryBusinessObjectTemplate Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateFallbackSequence();
			ValidateSourceType();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RecipientSource
			{
				FallbackSequence = FallbackSequence,
				SourceType = SourceType,
				ParentCollection = ParentCollection,
			};
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(nameof(FallbackSequence), FallbackSequence.ToString());
			writer.WriteElementString(nameof(SourceType), SourceType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FallbackSequence = reader.ReadElementStringAsZInt(nameof(FallbackSequence));
			SourceType = reader.ReadElementString(nameof(SourceType));
		}

		#endregion
	}
}
