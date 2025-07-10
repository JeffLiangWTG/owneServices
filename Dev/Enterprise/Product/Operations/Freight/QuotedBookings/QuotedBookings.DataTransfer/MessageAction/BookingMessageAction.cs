using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	class BookingMessageAction : ImportMessageAction
	{
		public BookingMessageAction(BusinessObjectFactoryProvider factoryProvider) : base(factoryProvider)
		{
			OnlySaveDataWhenNoRecordsHaveErrors = true;
		}

		protected override IGlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(
				  NotificationDataRegistry.Instance.BookingImportNotificationGroup.GetFallBackValueAtAllLevels(
				  GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty)));
			}
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("422b18d8-b1b8-4990-9074-ea53ec1d25eb", "System->Registry->Notification->Booking Import Notification Group"); }
		}

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new QuotedBookingValueObjectDataAdapter());
		}
	}
}
