using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	public class DelayAlertDeliveryRuleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new DelayAlertDeliveryRule this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DelayAlertDeliveryRule)base[index]; }
		}

		public new DelayAlertDeliveryRule AddNew()
		{
			return (DelayAlertDeliveryRule)base.AddNew();
		}

		public bool IsEnabledForAny()
		{
			return Count > 0;
		}

		public bool IsEnabledFor(string transportMode)
		{
			foreach (DelayAlertDeliveryRule rule in this)
			{
				if (rule.TransportMode == transportMode || rule.TransportMode == Constants.TransportModes.All)
				{
					return true;
				}
			}
			return false;
		}

		public bool ShouldDeliverDelayAlert(string transportMode, string module, string direction)
		{
			foreach (DelayAlertDeliveryRule delivery in this)
			{
				if (delivery.Matches(transportMode, module, direction))
				{
					return true;
				}
			}

			return false;
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DelayAlertDeliveryRule();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DelayAlertDeliveryRuleCollection();
		}

		#endregion
	}
}
