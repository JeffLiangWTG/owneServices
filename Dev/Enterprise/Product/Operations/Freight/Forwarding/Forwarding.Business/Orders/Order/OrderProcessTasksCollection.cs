using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderProcessTasksCollection : ProcessTaskCollection
	{
		public OrderProcessTasksCollection(Order order)
			: base(order)
		{
		}

		public new OrderProcessTasks this[int index]
		{
			get { return (OrderProcessTasks)Elements[index]; }
		}

		public new OrderProcessTasks AddNew()
		{
			return (OrderProcessTasks)base.AddNew();
		}

		#region OriginCountry / DestinationCountry

		public override ZString OriginCountry
		{
			get { return Parent.GoodsAvailableAt == null ? ZString.Empty : Parent.GoodsAvailableAt.RL_RN_NKCountryCode; }
		}

		public override ZString DestinationCountry
		{
			get { return Parent.GoodsDeliveredTo == null ? ZString.Empty : Parent.GoodsDeliveredTo.RL_RN_NKCountryCode; }
		}

		#endregion

		#region IsConditionMet

		public override bool IsCondition1Met(ZString conditionCode)
		{
			switch (conditionCode)
			{
				case OrderWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad:
					return Parent.JD_RL_NKPortOfLoading != Parent.JD_RL_NKGoodsAvailableAt;
				case OrderWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge:
					return Parent.JD_RL_NKPortOfDischarge != Parent.JD_RL_NKGoodsDeliveredTo;
			}
			return false;
		}

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			switch (conditionCode)
			{
				case JobShipmentWorkflowCondition2CodeList.Codes.LCL:
					return Core.Constants.ContainerModes.IsLCLType(Parent.JD_ContainerMode);
				case JobShipmentWorkflowCondition2CodeList.Codes.FCL:
					return Parent.JD_ContainerMode == Core.Constants.ContainerModes.FCL;
			}
			return conditionCode == Parent.JD_TransportMode;
		}

		#endregion

		#region Implementation

		new Order Parent
		{
			get { return (Order)base.Parent; }
		}

		#endregion
	}
}
