using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsOrderWrapperStrategy
	{
		WhsDocketValidation GetNewValidation();
		WhsDocketLookups GetNewLookups();
		bool DoesNotHaveJobEnteredEvent { get; }
		WhsPickableDocketLine[] LinesForSelectedOrderLines();
		void TemplateCopyLines(WhsPickableDocket copy);
		/*
		WhsPickableDocketLineCollection GetNewOrderLineCollection();
		WhsPickableDocketLineCollection GetNewAllLines();
		WhsOrderLineCollection GetNewParentLinesCollection();
		*/
		void OrderOnSaving();
	}

	public interface IWhsOrderWrapperCallback
	{
		bool EventLogExists(ZString code);
		bool DocketSubTypeReadOnly { get; }
	}

	public interface ITrackingOrderStrategyBuilder
	{
		IWhsOrderWrapperStrategy Build(WhsOrder order);
	}
}
