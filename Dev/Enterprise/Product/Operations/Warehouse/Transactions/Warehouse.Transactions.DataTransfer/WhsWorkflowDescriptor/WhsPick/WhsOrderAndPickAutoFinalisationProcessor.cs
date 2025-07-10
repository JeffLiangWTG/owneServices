using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsOrderAndPickAutoFinalisationProcessor : IProcessor
	{
		internal WhsOrderAndPickAutoFinalisationProcessor(WhsPick pick)
		{
			Pick = Argument.NotNull(pick, nameof(pick));
		}

		WhsPick Pick { get; }

		public void Process(INotifications notifications, CancellationToken cancellationToken = default)
		{
			foreach (var order in Pick.Orders.Cast<WhsPickableDocket>().Where(HasNoShortfallAndFullyPicked))
			{
				order.FinaliseDocket();
			}

			if (Pick.AllOrdersAreFinalised)
			{
				Pick.FinalisePick();

				if (!Pick.IsFinalised)
				{
					var pickFinalizationErrorMessage = new WhsPickFinalisationErrorReportingHelper(Pick).ReportPickFinalisationErrorMessage();
					notifications.AddError(pickFinalizationErrorMessage);
				}
			}
		}

		bool HasNoShortfallAndFullyPicked(WhsPickableDocket order) => !order.ShortfallExists && order.Lines.All(ol => ol.PickLines.All(pl => pl.IsPickedFromPutawayLocation));
	}
}
