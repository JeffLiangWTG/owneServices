using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardRatingDatesProvider<T> : JobDatesProvider<T>
		where T : BusinessObject, IJobInvoicingPlugIn
	{
		public CYDYardRatingDatesProvider(T parent, CYDYardUnitState yardUnit) : base(parent)
		{
			YardUnit = yardUnit;
		}

		readonly CYDYardUnitState YardUnit;

		protected override ZDateTime GetArrivalDateCore()
		{
			if (Parent.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.CYDReleaseAdvice)
			{
				return GetYardOutDateOverrideCore();
			}
			else
			{
				return GetYardInDateOverrideCore();
			}
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			if (Parent.InvoicingSupporter.ConsumerType == JobInvoicingConsumerTypes.CYDReceiveAdvice)
			{
				return GetYardInDateOverrideCore();
			}
			else
			{
				return GetYardOutDateOverrideCore();
			}
		}

		protected override ZDateTime GetYardInDateOverrideCore()
		{
			return YardUnit.ReceiveTransportationUnit?.YTU_GateInTime.ToLocalZDateTime() ?? ZDateTime.Empty;
		}

		protected override ZDateTime GetYardOutDateOverrideCore()
		{
			return YardUnit.DispatchTransportationUnit?.YTU_GateOutTime.ToLocalZDateTime() ?? ZDateTime.Empty;
		}
	}
}
