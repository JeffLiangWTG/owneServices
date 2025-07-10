using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.MessageDelivery
{
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public sealed class DeliveryContext
	{
		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "EnsureTrySave does something.")]
		public DeliveryContext(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public BusinessObjectFactory Factory { get; }
		public IEntityInfo ParentInfo { get; set; }
		public ZString ParentHumanReadableName { get; set; }
		public IEntityInfo TriggerObjectInfo { get; set; }
		public ZString ApplicationCode { get; set; }
		public ZString MessageTypeCode { get; set; }
		public ZString MessageSubTypeCode { get; set; }
		public ZString PurposeCode { get; set; }
		public INotifications Notifications { get; set; }
		public ZString ActionDescription { get; set; }
	}
}
