using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class DeclarationLockConfig : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string DeclarationType = "DeclarationType";
			public const string DeclarationTypeDescription = "DeclarationTypeDescription";
			public const string LockMode = "LockMode";
		}

		#endregion

		public DeclarationLockConfig()
		{
		}

		public DeclarationLockConfig(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Declaration Type

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(DeclarationLockConfigLookups.DeclarationTypeList))]
		public ZString DeclarationType
		{
			get => declarationType;
			set
			{
				CheckMaximumLength(DeclarationTypeInfo, value);
				if (SetNonPersistentPropertyValue(DeclarationTypeInfo, ref declarationType, value))
				{
					UpdateDescription();
					UpdateTabInfos();
					UpdateEventInfos();

					if (!IsValidationSuspended)
					{
						ValidateDeclarationType();
					}
				}
			}
		}

		public ZPropertyInfo DeclarationTypeInfo => GetZPropertyInfo(Schema.DeclarationType);

		public void ValidateDeclarationType()
		{
			DeclarationTypeInfo.ClearAllNotifications();

			if (!TabInfos.Any())
			{
				var message = Enterprise.Customs.Business.Res.GetString("3aaf02d7-f858-4d5e-b814-1c83af44fb54", @"Should have at least one row on ""Tabs that become locked""");
				DeclarationTypeInfo.AddError(message);
			}

			var list = Lookups.DeclarationTypeList;
			if (list.Count > 0)
			{
				MandatoryValidation.CheckEntered(DeclarationTypeInfo);
				ListValidation.ErrorIfInvalidCode(DeclarationTypeInfo, list);

				if (!DeclarationTypeInfo.HasErrors())
				{
					UniqueCheck();
				}
			}
		}

		void UniqueCheck()
		{
			var collection = GetParentCollection(this, typeof(DeclarationLockConfigCollection)) as DeclarationLockConfigCollection;

			if (collection != null && collection.Cast<DeclarationLockConfig>().Any(c => c != this && c.DeclarationType == DeclarationType))
			{
				var message = Enterprise.Customs.Business.Res.GetString("C3E72C07-2E05-4DCF-9B94-64372A6FC8B9", "There is an existing item with {0}.", DeclarationType);
				DeclarationTypeInfo.AddError(message);
			}
		}

		ZString declarationType;

		#endregion

		#region DeclarationTypeDescription

		[MaxLength(255)]
		public ZString DeclarationTypeDescription
		{
			get => declarationTypeDescription;
			set
			{
				CheckMaximumLength(DeclarationTypeDescriptionInfo, value);
				SetNonPersistentPropertyValue(DeclarationTypeDescriptionInfo, ref declarationTypeDescription, value);
			}
		}

		ZString declarationTypeDescription;

		void UpdateDescription()
		{
			DeclarationTypeDescription = string.IsNullOrWhiteSpace(DeclarationType) ? string.Empty : Lookups.DeclarationTypeList.GetDescriptionFromCode(DeclarationType);
		}

		public ZPropertyInfo DeclarationTypeDescriptionInfo => GetZPropertyInfo(Schema.DeclarationTypeDescription);

		#endregion

		#region LockMode

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(DeclarationLockConfigLookups.LockModeList))]
		public ZString LockMode
		{
			get => lockMode;
			set
			{
				CheckMaximumLength(LockModeInfo, value);
				if (SetNonPersistentPropertyValue(LockModeInfo, ref lockMode, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateLockMode();
					}
				}
			}
		}

		public ZPropertyInfo LockModeInfo => GetZPropertyInfo(Schema.LockMode);

		public void ValidateLockMode()
		{
			LockModeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(LockModeInfo);
			ListValidation.ErrorIfInvalidCode(LockModeInfo, Lookups.LockModeList);
		}

		ZString lockMode;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.DeclarationType, DeclarationType);
			writer.WriteElementString(Schema.DeclarationTypeDescription, DeclarationTypeDescription);
			writer.WriteElementString(Schema.LockMode, LockMode);

			EventLockInfoCollectionSerialiser.Serialize(writer, eventInfos);
			TabLockInfoCollectionSerialiser.Serialize(writer, tabInfos);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DeclarationType = reader.ReadElementString(Schema.DeclarationType);
			DeclarationTypeDescription = reader.ReadElementString(Schema.DeclarationTypeDescription);
			LockMode = reader.ReadElementString(Schema.LockMode);

			var newEvents = (DeclarationEventLockInfoCollection)EventLockInfoCollectionSerialiser.Deserialize(reader);
			CloneEventsFrom(this, newEvents);

			var newTabs = (DeclarationTabLockInfoCollection)TabLockInfoCollectionSerialiser.Deserialize(reader);
			CloneTabsFrom(this, newTabs);
		}

		#endregion

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DeclarationLockConfig(fallbackLevel, factory);

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var newConfig = clone as DeclarationLockConfig;
			newConfig?.CloneEventsFrom(newConfig, EventInfos);
			newConfig?.CloneTabsFrom(newConfig, TabInfos);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDeclarationType();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			LockMode = Core.Constants.Customs.DeclarationLockModes.Codes.All;
		}

		#endregion

		#region Lookups

		public DeclarationLockConfigLookups Lookups => lookups ?? (lookups = new DeclarationLockConfigLookups(this, CurrentFactory));
		DeclarationLockConfigLookups lookups;

		#endregion

		#region EventInfos

		public DeclarationEventLockInfoCollection EventInfos
		{
			get
			{
				if (eventInfos == null)
				{
					eventInfos = new DeclarationEventLockInfoCollection(this, CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(eventInfos);
				}

				return eventInfos;
			}
		}
		DeclarationEventLockInfoCollection eventInfos;

		ZXmlSerializer EventLockInfoCollectionSerialiser => fEventLockInfoCollectionSerialiser ?? (fEventLockInfoCollectionSerialiser = ZXmlSerializer.New(typeof(DeclarationEventLockInfoCollection)));
		ZXmlSerializer fEventLockInfoCollectionSerialiser;

		void CloneEventsFrom(DeclarationLockConfig config, DeclarationEventLockInfoCollection collection)
		{
			eventInfos = collection.Clone(config, CurrentFallbackLevel, CurrentFactory);
			RegisterEditableChildObject(eventInfos);
		}

		void UpdateEventInfos()
		{
			foreach (DeclarationEventLockInfo eventInfo in EventInfos)
			{
				eventInfo.RefreshLookups();
			}
		}
		#endregion

		#region TabInfos

		public DeclarationTabLockInfoCollection TabInfos
		{
			get
			{
				if (tabInfos == null)
				{
					tabInfos = new DeclarationTabLockInfoCollection(this, CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(tabInfos);
				}

				return tabInfos;
			}
		}
		DeclarationTabLockInfoCollection tabInfos;

		ZXmlSerializer TabLockInfoCollectionSerialiser => ftabLockInfoCollectionSerialiser ?? (ftabLockInfoCollectionSerialiser = ZXmlSerializer.New(typeof(DeclarationTabLockInfoCollection)));
		ZXmlSerializer ftabLockInfoCollectionSerialiser;

		void CloneTabsFrom(DeclarationLockConfig config, DeclarationTabLockInfoCollection collection)
		{
			tabInfos = collection.Clone(config, CurrentFallbackLevel, CurrentFactory);
			RegisterEditableChildObject(tabInfos);
		}

		void UpdateTabInfos()
		{
			foreach (DeclarationTabLockInfo tabInfo in TabInfos)
			{
				tabInfo.RefreshLookups();
			}
		}
		#endregion
	}
}
