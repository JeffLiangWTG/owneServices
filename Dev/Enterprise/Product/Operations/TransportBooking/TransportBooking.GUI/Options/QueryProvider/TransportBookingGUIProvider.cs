using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingGUIProvider : ITransportBookingGUIProvider
	{
		public TransportBookingGUIProvider()
		{
		}

		IEnumerable<IDtbBooking> ITransportBookingGUIProvider.GetBookingsToDeliver(IDtbBookingParent parent, IStmMenuItem menu)
		{
			return ((ITransportBookingGUIProvider)this).GetBookingsToDeliver(new[] { parent }, menu);
		}

		IEnumerable<IDtbBooking> ITransportBookingGUIProvider.GetBookingsToDeliver(IDtbBookingParent[] parents, IStmMenuItem menu)
		{
			Argument.NotNull(menu, "menu");
			IEnumerable<IDtbBooking> result = Array.Empty<IDtbBooking>();

			var direction = GetDirection(parents, menu);
			if (direction != DtbBookingDirection.None)
			{
				var combineContainers = menu != null && menu.SU_MenuName.Contains((NoResString)"Multi", StringComparison.Ordinal);
				var factory = new BusinessObjectFactory();
				var manager = new DtbDeliveryManager(factory, parents, direction, combineContainers);
				manager.ShowFormsFromMainThread = (menu as IZFilterModuleThreadSafe)?.ShowFormsFromMainThread ?? false;

				SetLastDeliveryManager(manager);

				result = manager.DeliverTransportBooking();
			}

			return result;
		}

		IEnumerable<IDtbBooking> ITransportBookingGUIProvider.GetBookingsToDeliver(IDtbBookingParent parents, DtbBookingDirection direction, bool combineContainers)
		{
			var factory = new BusinessObjectFactory();
			var manager = new DtbDeliveryManager(factory, parents, direction, combineContainers);

			SetLastDeliveryManager(manager);

			return manager.DeliverTransportBooking();
		}

		DtbBookingDirection GetDirection(IDtbBookingParent[] parents, IStmMenuItem menu)
		{
			DtbBookingDirection result;

			var documentDirection = menu.GetDocumentDirection();
			if (documentDirection != DocumentDirection.ANY)
			{
				result = documentDirection.ToDtbBookingDirection();
			}
			else
			{
				result = GetOnlySupportedDirectionIfAvailable(parents, menu);
			}

			return result;
		}

		DtbBookingDirection GetOnlySupportedDirectionIfAvailable(IDtbBookingParent[] parents, IStmMenuItem menu)
		{
			DtbBookingDirection result;

			var supportedDirections = parents.SelectMany(p => p.GetSupportedDirections()).Distinct();
			if (supportedDirections.Count() == 1)
			{
				result = supportedDirections.First();
			}
			else
			{
				result = DtbBookingDirection.None;
				ShowDirectionError(parents, menu, supportedDirections);
			}

			return result;
		}

		void ShowDirectionError(IDtbBookingParent[] parents, IStmMenuItem menu, IEnumerable<DtbBookingDirection> supportedDirections)
		{
			string errorMessage;
			var parentJobDescriptions = new ZStringBuilder(parents.Select(p => p.HumanReadableName).Distinct()).ToStringWithDelimiterBetweenAppends(", ");
			if (!supportedDirections.Any())
			{
				errorMessage = GetNoDirectionsSupportedMessage(menu, parentJobDescriptions);
			}
			else
			{
				errorMessage = GetTooManyDirectionsSupportedMessage(menu, parentJobDescriptions);
			}

			Globals.Message.ShowError(errorMessage, Res.GetString("TransportBookingGUIProvider|DirectionError|Caption", "Invalid Document Menu"));
		}

		string GetTooManyDirectionsSupportedMessage(IStmMenuItem menu, string parentJobDescriptions)
		{
			return Res.GetString("TransportBookingGUIProvider|TooManyDirectionsSupportedMessage",
@"This menu {0} has a Direction of ANY, but the Job(s) {1} support both Origin and Destination Transport Bookings.

Ensure the menu has a Direction set.", menu.SU_MenuName, parentJobDescriptions);
		}

		string GetNoDirectionsSupportedMessage(IStmMenuItem menu, string parentJobDescriptions)
		{
			return Res.GetString("TransportBookingGUIProvider|NoDirectionsSupportedMessage",
@"This menu {0} has a Direction of ANY, but the Job(s) {1} don't support any Transport Booking Direction.

Ensure the menu has a Direction set.", menu.SU_MenuName, parentJobDescriptions);
		}

		partial void SetLastDeliveryManager(DtbDeliveryManager deliveryManager);
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingGUIProvider
	{
		partial void SetLastDeliveryManager(DtbDeliveryManager deliveryManager)
		{
			LastDeliveryManagerForTest = deliveryManager;
		}

		public DtbDeliveryManager LastDeliveryManagerForTest { get; set; }
	}
}

#endif
