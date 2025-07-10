using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	[System.Diagnostics.DebuggerDisplay("{TransportMode}/{Module}/{Direction} -> {Deliver}")]
	public class DelayAlertDeliveryRule : AutoDelayAlertDeliveryRule
	{
		[List("Lookups.TransportModes")]
		public override ZString TransportMode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.TransportMode; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.TransportMode = value; }
		}

		[List("Lookups.Modules")]
		public override ZString Module
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Module; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Module = value; }
		}

		[List("Lookups.Directions")]
		public override ZString Direction
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.Direction; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Direction = value; }
		}

		public override void ValidateDirection()
		{
			base.ValidateDirection();
			MandatoryValidation.CheckEntered(DirectionInfo);
			ListValidation.ErrorIfInvalidCode(DirectionInfo);
			ValidateDuplicate(DirectionInfo);
		}

		public override void ValidateTransportMode()
		{
			base.ValidateTransportMode();
			MandatoryValidation.CheckEntered(TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(TransportModeInfo);
			ValidateDuplicate(TransportModeInfo);
		}

		public override void ValidateModule()
		{
			base.ValidateModule();
			MandatoryValidation.CheckEntered(ModuleInfo);
			ListValidation.ErrorIfInvalidCode(ModuleInfo);
			ValidateDuplicate(ModuleInfo);
		}

		public bool Matches(string transportMode, string module, string direction)
		{
			return (TransportMode == Constants.TransportModes.All || TransportMode == transportMode)
				&& (Module == DelayAlertDeliveryModules.Codes.All || Module == module)
				&& (Direction == DelayAlertDeliveryDirections.Codes.All || Direction == direction);
		}

		public DelayAlertDeliveryRuleLookups Lookups
		{
			get { return lookups ?? (lookups = new DelayAlertDeliveryRuleLookups(this)); }
		}
		DelayAlertDeliveryRuleLookups lookups;

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DelayAlertDeliveryRule();
		}
		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			TransportMode = Constants.TransportModes.All;
			Module = DelayAlertDeliveryModules.Codes.All;
			Direction = DelayAlertDeliveryDirections.Codes.All;
		}

		static void ValidateDuplicate(ZPropertyInfo info)
		{
			if (IsDuplicated((DelayAlertDeliveryRule)info.BizObj))
			{
				info.AddError(Res.GetString("8f7d9405-426a-4a1d-9729-0b38ad58a474", "This rule is duplicated."));
			}
		}
		static bool IsDuplicated(DelayAlertDeliveryRule currentDelivery)
		{
			BusinessObjectCollection[] parentCollections = ((IBusinessObjectInternals)currentDelivery).ParentCollections;

			if (parentCollections.Length == 1)
			{
				foreach (DelayAlertDeliveryRule delivery in parentCollections[0])
				{
					if (object.ReferenceEquals(currentDelivery, delivery))
					{
						continue;
					}

					if (currentDelivery.TransportMode == delivery.TransportMode &&
						currentDelivery.Module == delivery.Module &&
						currentDelivery.Direction == delivery.Direction)
					{
						return true;
					}
				}
			}

			return false;
		}

		#endregion

	}
}
