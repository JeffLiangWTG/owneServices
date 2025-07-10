using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class ForwardingPackLineWithPortMessaging : ForwardingPackLine
	{
		public ForwardingPackLineWithPortMessaging(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public PackLinePortMessaging PortMessaging
		{
			get
			{
				if (portMessaging == null)
				{
					portMessaging = PackLinePortMessaging.LoadOrCreate(this);
					RegisterEditableChildObject(portMessaging);
					portMessaging?.Validation.ValidateDGTechnicalName();
				}

				return portMessaging;
			}
		}
		PackLinePortMessaging portMessaging;

		#region Implementation

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;
			if (!property.Name.Contains("PortMessaging"))
			{
				result = true;
			}

			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
