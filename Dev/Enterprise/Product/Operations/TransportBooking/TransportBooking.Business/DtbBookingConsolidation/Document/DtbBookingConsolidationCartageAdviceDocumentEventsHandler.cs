using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.TransportBookings.Business
{
	class DtbBookingConsolidationCartageAdviceDocumentEventsHandler : CartageAdviceDocumentEventsHandler
	{
		public override bool CanHandleMenuItem(IStmMenuItem menuItem)
		{
			return ContainsCartageAdvice(menuItem.Documents) ||
				menuItem.ChildMenus.OfType<IStmMenuMenuPivotBase>().Any(pivot =>
					ContainsCartageAdvice(pivot.Outward.Documents)
				);
		}

		protected override IDocumentSupportable[] Bookings { get => ((DtbBookingConsolidationDocumentSupporter)DocumentSupporter).Bookings; }
	}
}
