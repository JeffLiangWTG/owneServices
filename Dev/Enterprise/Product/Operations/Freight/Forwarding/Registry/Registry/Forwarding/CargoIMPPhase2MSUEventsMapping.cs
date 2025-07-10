using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class CargoIMPPhase2MSUEventsMapping : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string CargoIMPPhase2MSUEvent = "CargoIMPPhase2MSUEvent";
			public const string EnterpriseEvent = "EnterpriseEvent";
			public const string EnterpriseEventReference = "EnterpriseEventReference";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CargoIMPPhase2MSUEventsMapping();
		}

		#endregion

		#region Properties

		#region CargoIMPPhase2MSUEvent

		[List("CargoIMPPhase2MSUEventList")]
		[MaxLength(3)]
		public ZString CargoIMPPhase2MSUEvent
		{
			get { return this.cargoIMPPhase2MSUEvent; }
			set
			{
				SetNonPersistentPropertyValue(CargoIMPPhase2MSUEventInfo, ref this.cargoIMPPhase2MSUEvent, value);
				if (!IsValidationSuspended)
				{
					ValidateCargoIMPPhase2MSUEvent();
				}
			}
		}
		ZString cargoIMPPhase2MSUEvent;

		public ZPropertyInfo CargoIMPPhase2MSUEventInfo
		{
			get { return GetZPropertyInfo(Schema.CargoIMPPhase2MSUEvent); }
		}

		public void ValidateCargoIMPPhase2MSUEvent()
		{
			CargoIMPPhase2MSUEventInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CargoIMPPhase2MSUEventInfo);
			ListValidation.ErrorIfInvalidCode(CargoIMPPhase2MSUEventInfo, CargoIMPPhase2MSUEventList);
		}

		#endregion

		#region CargoIMPPhase2MSUEventDescription

		public ZString CargoIMPPhase2MSUEventDescription
		{
			get { return CargoIMPPhase2MSUEventList.GetDescriptionFromCode(CargoIMPPhase2MSUEvent); }
		}

		public ZPropertyInfo CargoIMPPhase2MSUEventDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CargoIMPPhase2MSUEventDescription)); }
		}

		#endregion

		#region EnterpriseEvent

		[List("EnterpriseEvents")]
		[MaxLength(ProcessTask.Schema.P9_SE_NKMilestoneEventMaxLength)]
		public ZString EnterpriseEvent
		{
			get { return this.enterpriseEvent; }
			set
			{
				SetNonPersistentPropertyValue(EnterpriseEventInfo, ref this.enterpriseEvent, value);
				if (!IsValidationSuspended)
				{
					ValidateEnterpriseEvent();
				}
			}
		}
		ZString enterpriseEvent;

		public ZPropertyInfo EnterpriseEventInfo
		{
			get { return GetZPropertyInfo(Schema.EnterpriseEvent); }
		}

		public void ValidateEnterpriseEvent()
		{
			EnterpriseEventInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(EnterpriseEventInfo);
			ListValidation.ErrorIfInvalidCode(EnterpriseEventInfo, EnterpriseEvents);
			CheckIsUniqueInCollectionForEventAndReference(EnterpriseEventInfo);
			ValidateNearExpirationEnterpriseEvent();
		}

		void ValidateNearExpirationEnterpriseEvent()
		{
			if (EnterpriseEvent == AutoEvents.GateInCFSReplacedByGINEventCode || EnterpriseEvent == AutoEvents.GateOutCFSReplacedByGOUEventCode)
			{
				var message = Res.GetString("ca94a254-70c7-4a50-b2fb-55f1ff610b7f", "This event code is no longer used.");
				EnterpriseEventInfo.AddWarning(message);
			}
		}

		#endregion

		#region EnterpriseEventDescription

		public ZString EnterpriseEventDescription
		{
			get { return EnterpriseEvents.GetDescriptionFromCode(EnterpriseEvent); }
		}

		public ZPropertyInfo EnterpriseEventDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(EnterpriseEventDescription)); }
		}

		#endregion

		#region EnterpriseEventReference

		public ZString EnterpriseEventReference
		{
			get { return this.enterpriseEventReference; }
			set
			{
				SetNonPersistentPropertyValue(EnterpriseEventReferenceInfo, ref this.enterpriseEventReference, value);
				if (!IsValidationSuspended)
				{
					ValidateEnterpriseEventReference();
				}
			}
		}

		ZString enterpriseEventReference;

		public ZPropertyInfo EnterpriseEventReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.EnterpriseEventReference); }
		}

		public ZString EnterpriseEventReferenceType
		{
			get { return nameof(FieldType.TextCodeFindBox); }
		}

		public void ValidateEnterpriseEventReference()
		{
			EnterpriseEventReferenceInfo.ClearAllNotifications();
			CheckIsUniqueInCollectionForEventAndReference(EnterpriseEventReferenceInfo);
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCargoIMPPhase2MSUEvent();
			ValidateEnterpriseEvent();
			ValidateEnterpriseEventReference();
		}

		void CheckIsUniqueInCollectionForEventAndReference(ZPropertyInfo propertyInfo)
		{
			foreach (var parentCollection in ParentCollections)
			{
				if (parentCollection != null && parentCollection.Any())
				{
					var existentCount = parentCollection
						.Cast<CargoIMPPhase2MSUEventsMapping>()
						.Count(c => c.EnterpriseEvent == EnterpriseEvent
							&& (string.IsNullOrWhiteSpace(c.EnterpriseEventReference) || c.EnterpriseEventReference == EnterpriseEventReference));

					if (existentCount > 1)
					{
						var errorMessage = ResString.GetMultilingualString("d0a81179-d0e7-43ad-8a05-f1c19d705f90",
							"This event code is duplicated. For duplicate events, Event References must be unique and must not be blank.");

						propertyInfo.AddError(errorMessage);
					}
				}
			}
		}

		#endregion

		#region Lookups

		public ICodeDescriptionPairList EnterpriseEvents
		{
			get
			{
				if (this.enterpriseEvents == null)
				{
					this.enterpriseEvents = new CodeDescriptionPairList();
					foreach (Event type in Events.All)
					{
						if (!Events.ChangeLogs.Contains(type) && type != Events.WorkflowTriggerEvent)
						{
							this.enterpriseEvents.AddPair(type.Code, type.MultilingualDescription);
						}
						else if (type == Events.AddedARecordToTheSystem ||
								type == Events.SetToActive || type == Events.SetToInactive)
						{
							this.enterpriseEvents.AddPair(type.Code, type.MultilingualDescription);
						}
					}

					this.enterpriseEvents.SortByDescription();
				}

				return this.enterpriseEvents;
			}
		}

		CodeDescriptionPairList enterpriseEvents;

		public CargoIMPPhase2MSUEventCodeList CargoIMPPhase2MSUEventList
		{
			get { return this.cargoIMPPhase2MSUEventList ?? (this.cargoIMPPhase2MSUEventList = new CargoIMPPhase2MSUEventCodeList()); }
		}

		CargoIMPPhase2MSUEventCodeList cargoIMPPhase2MSUEventList;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.CargoIMPPhase2MSUEvent, CargoIMPPhase2MSUEvent);
			writer.WriteElementString(Schema.EnterpriseEvent, EnterpriseEvent);
			writer.WriteElementString(Schema.EnterpriseEventReference, EnterpriseEventReference);
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			CargoIMPPhase2MSUEvent = wrapper.ReadElementString(Schema.CargoIMPPhase2MSUEvent);
			EnterpriseEvent = wrapper.ReadElementString(Schema.EnterpriseEvent);
			EnterpriseEventReference = wrapper.ReadElementString(Schema.EnterpriseEventReference);
		}

		#endregion
	}
}
