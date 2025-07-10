using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class FieldSynchroniser : BaseSynchroniser
	{
		#region Constructors

		public FieldSynchroniser(ZPropertyInfo destination, ZPropertyInfo source, bool dontSetFieldsReadOnly)
			: this(destination, () => source.Value, () => new[] { source }, dontSetFieldsReadOnly)
		{
		}

		public FieldSynchroniser(ZPropertyInfo destination, ZPropertyInfo source)
			: this(destination, source, false)
		{
		}

		public FieldSynchroniser(ZPropertyInfo destination, SourceValueDelegate sourceValue, InfosToHookValueChangedEventDelegate infosToHookValueChangedEventProvider)
			: this(destination, sourceValue, infosToHookValueChangedEventProvider, () => false)
		{
		}

		public FieldSynchroniser(ZPropertyInfo destination, SourceValueDelegate sourceValue, InfosToHookValueChangedEventDelegate infosToHookValueChangedEventProvider, bool dontSetFieldsReadOnly)
			: this(destination, sourceValue, infosToHookValueChangedEventProvider, () => dontSetFieldsReadOnly)
		{
		}

		public FieldSynchroniser(ZPropertyInfo destination, SourceValueDelegate sourceValue, InfosToHookValueChangedEventDelegate infosToHookValueChangedEventProvider, bool dontSetFieldsReadOnly, DestinationEnabledDelegate destinationEnabled)
			: this(destination, sourceValue, infosToHookValueChangedEventProvider, () => dontSetFieldsReadOnly, destinationEnabled)
		{
		}

		public FieldSynchroniser(DestinationZPropertyInfoDelegate destinationDelegate, SourceValueDelegate sourceValue, InfosToHookValueChangedEventDelegate infosToHookValueChangedEventProvider, SourceReadOnlyDelegate sourceReadOnly)
		{
			this.destinationZPropertyInfoGetter = Argument.NotNull(destinationDelegate, "destinationDelegate");
			this.sourceValue = Argument.NotNull(sourceValue, "sourceValue");
			this.infosToHookValueChangedEventProvider = Argument.NotNull(infosToHookValueChangedEventProvider, "infosToHookValueChangedEventProvider");
			this.infosToHookValueChangedEvent = new Dictionary<ZPropertyInfo, IZType>();

			UpdateInfosToHookValueChangedEvent();

			dontSetFieldsReadOnly = sourceReadOnly;
			SetEnabled(true, DetectEnabled);
#if DEBUG
			SynchroniserDetectionHelper.SetupDetection(destination);
#endif
		}

		public FieldSynchroniser(ZPropertyInfo destination, SourceValueDelegate sourceValue, InfosToHookValueChangedEventDelegate infosToHookValueChangedEventProvider, SourceReadOnlyDelegate sourceReadOnly)
			: this(destination, sourceValue, infosToHookValueChangedEventProvider, sourceReadOnly, null)
		{
		}

		public FieldSynchroniser(ZPropertyInfo destination, SourceValueDelegate sourceValue, InfosToHookValueChangedEventDelegate infosToHookValueChangedEventProvider, SourceReadOnlyDelegate sourceReadOnly, DestinationEnabledDelegate destinationEnabled)
		{
			this.destination = Argument.NotNull(destination, "destination");
			this.sourceValue = Argument.NotNull(sourceValue, "sourceValue");
			this.infosToHookValueChangedEventProvider = Argument.NotNull(infosToHookValueChangedEventProvider, "infosToHookValueChangedEventProvider");
			this.infosToHookValueChangedEvent = new Dictionary<ZPropertyInfo, IZType>();

			UpdateInfosToHookValueChangedEvent();

			dontSetFieldsReadOnly = sourceReadOnly;
			destinationEnabledProvider = destinationEnabled;
			SetEnabled(true, DetectEnabled);
#if DEBUG
			SynchroniserDetectionHelper.SetupDetection(destination);
#endif
		}

		void UpdateInfosToHookValueChangedEvent()
		{
			UnHookInfosValueChangedEvents();
			infosToHookValueChangedEvent.Clear();
			foreach (var info in infosToHookValueChangedEventProvider())
			{
				var wrapped = info as ZWrappedPropertyInfo;
				var keyInfo = wrapped == null ? info : wrapped.InnerInfo;
				if (keyInfo != null)
				{
					if (infosToHookValueChangedEvent.ContainsKey(keyInfo))
					{
						infosToHookValueChangedEvent[keyInfo] = null;
					}
					else
					{
						infosToHookValueChangedEvent.Add(keyInfo, null);
					}
				}
			}
		}

		#endregion

		#region Synchronise

		public void UpdateInfoEventsAndReSynchronise()
		{
			UpdateInfosToHookValueChangedEvent();
			if (IsEnabled)
			{
				HookInfosValueChangedEvents();
			}
			Synchronise();
		}

		protected override void OnSynchronise(SynchroniseEventArgs e)
		{
			base.OnSynchronise(e);
			SetDestinationReadOnlyProperty();
		}

		protected override void ForceSynchronise()
		{
			if (destination == null && destinationZPropertyInfoGetter != null)
			{
				destination = destinationZPropertyInfoGetter();
			}
			if (!SyncChangesDetected && destination != null && destination.BizObj != null && !destination.BizObj.IsDeleted && DestinationSyncEnabled)
			{
				var formattedValue = GetFormattedValue();
				var isZString = false;
				if (formattedValue is ZString && destination.PropertyType == typeof(ZString))
				{
					var sourceString = (ZString)formattedValue;
					formattedValue = sourceString.Left(destination.MaxLength);
					isZString = true;
				}
				if (DetectEnabled)
				{
					if (isZString ? !((ZString)destination.Value).EqualsIgnoringCase(formattedValue.ToString()) : !formattedValue.Equals(destination.Value))
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					destination.Value = formattedValue;
					if (JobDocAddressPersistingSyncronizedValue != null && destination.Value.IsValid)
					{
						JobDocAddressPersistingSyncronizedValue.MakePersistentEvenIfEmpty();
					}
				}
			}
		}

		bool DestinationSyncEnabled => (destinationEnabledProvider?.Invoke() ?? true);

		protected void SetDestinationReadOnlyProperty()
		{
			if (destination == null && destinationZPropertyInfoGetter != null)
			{
				destination = destinationZPropertyInfoGetter();
			}
			if (destination != null)
			{
				var newValue = !dontSetFieldsReadOnly() && IsEnabled;
				var parent = destination.BizObj;

				if (ReadOnlyObjects.Contains(parent.TableName))
				{
					parent.ReadOnly = newValue;
				}
#if DEBUG
				else if (parent is CargoWise.EntityFramework.Testing.DummyBusinessObject)
				{
					parent.ReadOnly = newValue;
				}
#endif
				else
				{
					var bizObj = parent as ISynchroniserReadOnlyMembersProvider;
					if (bizObj != null)
					{
						var property = bizObj.SynchroniserReadOnlyMembers;
						if (newValue)
						{
							if (!property.Contains(destination.Name))
							{
								property.Add(destination.Name);
							}
						}
						else
						{
							property.Remove(destination.Name);
						}
					}
					else
					{
						ErrorReporter.ReportOnce("The business object " + parent.TableName + " is not of type ISynchroniserReadOnlyMembersProvider.");
					}
				}
			}
		}
		static readonly ImmutableArray<string> ReadOnlyObjects = new string[] { JobDocAddressSchema.Constants.TableName, UNDGDataItemSchema.Constants.TableName, CusEntryNumSchema.Constants.TableName }.ToImmutableArray();

		protected IZType GetFormattedValue()
		{
			var args = new ConvertEventArgs(sourceValue(), destination.PropertyType);
			if (Format != null)
			{
				Format(this, args);
			}
			return (IZType)args.Value;
		}

		#endregion

		#region OnEnabledChanged

		protected override void OnEnabledChanged()
		{
			if (destination == null && destinationZPropertyInfoGetter != null)
			{
				destination = destinationZPropertyInfoGetter();
			}
			if (IsEnabled && destination != null && destination.BizObj.IsDeleted)
			{
				ErrorReporter.ReportOnce(destination.BizObj.GetType().FullName, "Attempting to Synchronise field on Deleted Destination: BizO: " + destination.BizObj.GetType().FullName);
			}
			base.OnEnabledChanged();
			SetDestinationReadOnlyProperty();
		}

		protected virtual void Info_ValueChanged(object sender, EventArgs e)
		{
			var ve = e as InfoEventArgs;
			if (ve != null)
			{
				var wrapped = ve.Info as ZWrappedPropertyInfo;
				var info = (wrapped == null) ? ve.Info : wrapped.InnerInfo;
				IZType lastValue;
				if (infosToHookValueChangedEvent.TryGetValue(info, out lastValue) && !ve.Info.Value.Equals(lastValue))
				{
					Synchronise();
					infosToHookValueChangedEvent[info] = info.Value;
				}
			}
		}

		protected override void OnDetectEnabledChanged()
		{
		}

		protected override void HookEvents()
		{
			HookInfosValueChangedEvents();
		}

		void HookInfosValueChangedEvents()
		{
			foreach (var info in infosToHookValueChangedEvent.Keys)
			{
				info.ValueChanged -= Info_ValueChanged;
				info.ValueChanged += Info_ValueChanged;
			}
		}

		protected override void UnHookEvents()
		{
			UnHookInfosValueChangedEvents();
		}

		void UnHookInfosValueChangedEvents()
		{
			foreach (var info in infosToHookValueChangedEvent.Keys)
			{
				info.ValueChanged -= Info_ValueChanged;
			}
		}

		#endregion

		public event ConvertEventHandler Format;

		public delegate void ConvertEventHandler(object sender, ConvertEventArgs e);

		public class ConvertEventArgs : EventArgs
		{
			public ConvertEventArgs(object value, Type desiredType)
			{
				Value = value;
				if (desiredType == null)
				{
					throw new ArgumentNullException(nameof(desiredType));
				}
				DesiredType = desiredType;
			}

			public Type DesiredType { get; private set; }
			public object Value { get; set; }
		}

		public JobDocAddress JobDocAddressPersistingSyncronizedValue { get; set; }
		public ZPropertyInfo Destination => destination;
		protected ZPropertyInfo destination;
		readonly SourceReadOnlyDelegate dontSetFieldsReadOnly;
		readonly Dictionary<ZPropertyInfo, IZType> infosToHookValueChangedEvent;
		readonly InfosToHookValueChangedEventDelegate infosToHookValueChangedEventProvider;
		readonly SourceValueDelegate sourceValue;
		readonly DestinationEnabledDelegate destinationEnabledProvider;
		readonly DestinationZPropertyInfoDelegate destinationZPropertyInfoGetter;
		public delegate IEnumerable<ZPropertyInfo> InfosToHookValueChangedEventDelegate();
	}

	public delegate ZPropertyInfo DestinationZPropertyInfoDelegate();
	public delegate IZType SourceValueDelegate();
	public delegate bool SourceReadOnlyDelegate();
	public delegate bool DestinationEnabledDelegate();
}
