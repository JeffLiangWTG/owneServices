using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business
{
	class CartageLegProcessTaskCollection : ProcessTaskCollection
	{
		public CartageLegProcessTaskCollection(CommonCartageLeg cartageLeg)
			: base(cartageLeg)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public new CartageLegProcessTask this[int index]
		{
			get { return (CartageLegProcessTask)Elements[index]; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		public new CartageLegProcessTask AddNew()
		{
			return (CartageLegProcessTask)base.AddNew();
		}

		new CommonCartageLeg Parent
		{
			get { return (CommonCartageLeg)base.Parent; }
		}

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			switch (conditionCode)
			{
				case CartageLegWorkflowCondition2CodeList.Codes.CFS:
					return Parent.DeliverToDocAddressType == DocAddressType.LocalCartageCFS;
				case CartageLegWorkflowCondition2CodeList.Codes.Consignee:
					return Parent.DeliverToDocAddressType == DocAddressType.LocalCartageImporter;
				case CartageLegWorkflowCondition2CodeList.Codes.CTO:
					return Parent.DeliverToDocAddressType == DocAddressType.LocalCartageCTO;
				case CartageLegWorkflowCondition2CodeList.Codes.CYD:
					return Parent.DeliverToDocAddressType == DocAddressType.LocalCartageYard;
				case CartageLegWorkflowCondition2CodeList.Codes.Warehouse:
					return Parent.DeliverToDocAddressType == DocAddressType.LocalCartageWarehouse;
				default:
					return base.IsCondition2MetCore(conditionCode, value);
			}
		}
	}
}
