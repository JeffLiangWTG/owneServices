using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;

namespace Enterprise.Warehouse.Transactions.Business
{
	// Tested in CarrierLabelPrinterOptionTest.cs
	class CarrierLabelPrinterOptionValidation : ZValidation
	{
		public CarrierLabelPrinterOptionValidation(CarrierLabelPrinterOption parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly CarrierLabelPrinterOption Parent;

		public override Type AutoValidationType => typeof(CarrierLabelPrinterOptionValidation);

		public override void ValidateAll()
		{
			ValidateCarrierLabelPrinterPK();
		}

		public void ValidateCarrierLabelPrinterPK()
		{
			ValidateCalculatedProperty(Parent.CarrierLabelPrinterPKInfo);
		}

		protected void CheckCarrierLabelPrinterPK()
		{
			if (Parent.CarrierLabelPrinterPK.IsEmpty)
			{
				Parent.CarrierLabelPrinterPKInfo.AddError(Res.GetString("91ce37ef-30a1-4cf2-8c84-29afdee72e5e", "Please select a Carrier Label Printer."));
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(Parent.CarrierLabelPrinterPKInfo, Parent.Printers);
				PrintQueueValidation.ValidatePrintQueue(Parent.CarrierLabelPrinterPKInfo);
			}
		}
	}
}
