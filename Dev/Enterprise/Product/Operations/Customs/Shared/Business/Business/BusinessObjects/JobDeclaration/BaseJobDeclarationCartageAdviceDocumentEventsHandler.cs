using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportBookings.Shared;

namespace Enterprise.Customs.Business
{
	class BaseJobDeclarationCartageAdviceDocumentEventsHandler : CartageAdviceDocumentEventsHandler
	{
		protected override IDocumentSupportable[] Bookings => ((BaseJobDeclarationDocumentSupporter)DocumentSupporter).Bookings;

		public override bool CanHandleMenuItem(IStmMenuItem menuItem)
		{
			return ContainsCartageAdvice(menuItem.Documents) ||
				menuItem.ChildMenus.OfType<IStmMenuMenuPivotBase>().Any(pivot =>
					ContainsCartageAdvice(pivot.Outward.Documents)
				);
		}
	}
}
