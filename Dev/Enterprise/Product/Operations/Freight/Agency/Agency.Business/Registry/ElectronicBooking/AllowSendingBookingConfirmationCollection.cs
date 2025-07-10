using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.Agency.Business.XmlSerializers")]
	public class AllowSendingBookingConfirmationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public AllowSendingBookingConfirmationCollection()
		{
		}

		public AllowSendingBookingConfirmationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new AllowSendingBookingConfirmation this[int index]
		{
			get { return (AllowSendingBookingConfirmation)Elements[index]; }
		}

		#region NewWithDefaultValues

		public static AllowSendingBookingConfirmationCollection NewWithDefaultValues()
		{
			var result = new AllowSendingBookingConfirmationCollection();
			var item = result.AddNew();
			item.PrincipalPK = Guid.Empty;
			item.Enabled = false;

			return result;
		}

		#endregion

		public new AllowSendingBookingConfirmation AddNew()
		{
			return (AllowSendingBookingConfirmation)base.AddNew();
		}

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AllowSendingBookingConfirmation(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AllowSendingBookingConfirmationCollection(fallbackLevel, factory);
		}

		#endregion
	}
}
