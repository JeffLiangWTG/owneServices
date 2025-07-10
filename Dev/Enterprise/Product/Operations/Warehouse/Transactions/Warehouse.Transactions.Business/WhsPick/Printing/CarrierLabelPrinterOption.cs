using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class CarrierLabelPrinterOption : NonPersistentBusinessObject
	{
		public CarrierLabelPrinterOption(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[List(nameof(Printers))]
		[ResourceStringData("58179073-8a1f-47d5-9984-ebebc2aa5a27", Caption = "Carrier Label Printer", ShortCaption = "Printer")]
		public ZGuid CarrierLabelPrinterPK
		{
			get => carrierLabelPrinterPK;
			set
			{
				if (SetNonPersistentPropertyValue(CarrierLabelPrinterPKInfo, ref carrierLabelPrinterPK, value)
					&& !IsValidationSuspended)
				{
					Validation.ValidateCarrierLabelPrinterPK();
				}

				CarrierLabelPrinterPKInfo.RefreshBinding();
			}
		}

		ZGuid carrierLabelPrinterPK;

		public ZPropertyInfo CarrierLabelPrinterPKInfo => GetZPropertyInfo(nameof(CarrierLabelPrinterPK));

		public IBusinessObjectCollection Printers => printers ?? (printers = WhsCommonLookups.GetPrintersList(Factory));
		IBusinessObjectCollection printers;

		#region Validation

		CarrierLabelPrinterOptionValidation Validation => new CarrierLabelPrinterOptionValidation(this);

		#endregion
	}
}
