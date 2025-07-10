using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public sealed class DeclarationEventLockInfo : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string EventType = "EventType";
			public const string EventReference = "EventReference";
			public const string EventSource = "EventSource";
			public const string EntryType = "EntryType";
		}

		public const string DefaultMatchCharacter = "*";

		#endregion

		public DeclarationEventLockInfo()
		{
		}

		public DeclarationEventLockInfo(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Properties

		#region EventType

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(DeclarationEventLockInfoLookups.EventTypeList))]
		public ZString EventType
		{
			get => eventType;
			set
			{
				CheckMaximumLength(EventTypeInfo, value);
				if (SetNonPersistentPropertyValue(EventTypeInfo, ref eventType, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateEventType();
					}
				}
			}
		}

		public ZPropertyInfo EventTypeInfo => GetZPropertyInfo(Schema.EventType);

		public void ValidateEventType()
		{
			EventTypeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(EventTypeInfo);
			ListValidation.ErrorIfInvalidCode(EventTypeInfo, Lookups.EventTypeList);

			if (!EventTypeInfo.HasErrors())
			{
				UniqueCheck();
			}
		}

		void UniqueCheck()
		{
			if (GetParentCollection(this, typeof(DeclarationEventLockInfoCollection)) is DeclarationEventLockInfoCollection collection
				&& collection.Cast<DeclarationEventLockInfo>()
					.Any(c => c != this && c.EventType == EventType && c.EventReference == EventReference && c.EventSource == EventSource && c.CurrentFallbackLevel == CurrentFallbackLevel))
			{
				var message = Enterprise.Customs.Business.Res.GetString("B8B62645-1FB5-4ADF-A89A-9ADDAEEC431C", "There is an existing item with same data.");
				EventTypeInfo.AddError(message);
			}
		}

		ZString eventType;

		#endregion

		#region EventReference

		public ZString EventReference
		{
			get => eventReference;
			set
			{
				CheckMaximumLength(EventReferenceInfo, value);
				if (SetNonPersistentPropertyValue(EventReferenceInfo, ref eventReference, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateEventReference();
						ValidateEventType();
					}
				}
			}
		}

		public ZPropertyInfo EventReferenceInfo => GetZPropertyInfo(Schema.EventReference);

		public void ValidateEventReference()
		{
			EventReferenceInfo.ClearAllNotifications();
		}

		ZString eventReference;

		#endregion

		#region EventSource

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(DeclarationEventLockInfoLookups.EventSourceList))]
		public ZString EventSource
		{
			get => eventSource;
			set
			{
				if (eventSource != value)
				{
					if (eventSource == Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader)
					{
						EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All;
					}

					CheckMaximumLength(EventSourceInfo, value);
					SetNonPersistentPropertyValue(EventSourceInfo, ref eventSource, value);

					if (!IsValidationSuspended)
					{
						ValidateEventSource();
						ValidateEventType();
					}
				}
			}
		}

		public ZPropertyInfo EventSourceInfo => GetZPropertyInfo(Schema.EventSource);

		public void ValidateEventSource()
		{
			EventSourceInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(EventSourceInfo);
			ListValidation.ErrorIfInvalidCode(EventSourceInfo, Lookups.EventSourceList);
		}

		ZString eventSource;

		#endregion

		#region EntryType

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(DeclarationEventLockInfoLookups.EntryTypeList))]
		[ReadOnlyMember(nameof(IsEntryTypeReadOnly))]
		public ZString EntryType
		{
			get => entryType;
			set
			{
				CheckMaximumLength(EntryTypeInfo, value);

				if (SetNonPersistentPropertyValue(EntryTypeInfo, ref entryType, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateEntryType();
					}
				}
			}
		}

		public bool IsEntryTypeReadOnly => EventSource != Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader;

		public ZPropertyInfo EntryTypeInfo => GetZPropertyInfo(Schema.EntryType);

		public void ValidateEntryType()
		{
			EntryTypeInfo.ClearAllNotifications();

			if (EventSource == Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader)
			{
				MandatoryValidation.CheckEntered(EntryTypeInfo);
				ListValidation.ErrorIfInvalidCode(EntryTypeInfo, Lookups.EntryTypeList);
			}
		}

		ZString entryType;

		#endregion

		public DeclarationLockConfig LockConfig => ((DeclarationEventLockInfoCollection)GetParentCollection(this, typeof(DeclarationEventLockInfoCollection)))?.LockConfig;

		#endregion

		#region Lookups

		public DeclarationEventLockInfoLookups Lookups => lookups ?? (lookups = new DeclarationEventLockInfoLookups(this, CurrentFactory));
		DeclarationEventLockInfoLookups lookups;

		public void RefreshLookups()
		{
			lookups = null;
		}
		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateEventType();
			ValidateEventReference();
			ValidateEventSource();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.EventType, EventType);
			writer.WriteElementString(Schema.EventReference, EventReference);
			writer.WriteElementString(Schema.EventSource, EventSource);
			writer.WriteElementString(Schema.EntryType, EntryType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EventType = reader.ReadElementString(Schema.EventType);
			EventReference = reader.ReadElementString(Schema.EventReference);
			EventSource = reader.ReadElementString(Schema.EventSource);
			EntryType = reader.ReadElementString(Schema.EntryType);
		}

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DeclarationEventLockInfo(fallbackLevel, factory);

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			EventReference = DefaultMatchCharacter;
			EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All;
		}

		#endregion
	}
}
