
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		DocCusEntryLine(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryLine == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryLine(cusEntryLine, factoryToWrap);
			}
		}

		#region Overrides

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Enterprise.Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap)
		{
			return DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);
		}

		#endregion

		public DocJobComInvoiceLine InvoiceLine
		{
			get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
		}

		public ZString ExportCode
		{
			get { return CusEntryLine.ExportCode; }
		}

		public ZLong ValueInWholeUSDollars
		{
			get { return RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(LinePriceInLocalCurrency); }
		}

		public ZString FirstCustomsQtyAndUnit
		{
			get { return ZString.Format("{0} {1}", FirstQuantity, CustomsUnitQty); }
		}

		public ZLong FirstQuantity
		{
			get { return RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(CustomsQuantity); }
		}

		public ZLong RoundedSecondCustomsQuantity
		{
			get { return RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(CusEntryLine.SecondCustomsQuantity); }
		}

		public ZString SecondCustomsUnitQty
		{
			get { return CusEntryLine.SecondCustomsUnitQty; }
		}

		public ZLong RoundedGrossWeightInKilograms
		{
			get { return RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(CusEntryLine.GrossWeightInKilograms); }
		}

		public ZLong RoundedGrossWeightInPounds
		{
			get { return RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(CusEntryLine.GrossWeightInPounds); }
		}

		public ZLong RoundedVolumeInCubicMeters
		{
			get { return RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(CusEntryLine.VolumeInCubicMeters); }
		}

		public ZString LicenseType
		{
			get { return CusEntryLine.LicenseType; }
		}

		public ZString LicenseNumber
		{
			get { return CusEntryLine.LicenseNumber; }
		}

		public ZString LicenseNumberAndLicenseException
		{
			get { return CusEntryLine.LicenseNumberAndExemptionCode; }
		}

		public ZString MarksAndNumbers
		{
			get { return CusEntryLine.MarksAndNumbers; }
		}

		public ZBool IsUsedVehicle
		{
			get { return CusEntryLine.IsUsedVehicle; }
		}

		public ZString VehicleIDType
		{
			get { return CusEntryLine.VehicleIDType; }
		}

		public ZString VehicleID
		{
			get { return CusEntryLine.VehicleID; }
		}

		public ZString VehicleTitle
		{
			get { return CusEntryLine.VehicleTitleNumber; }
		}

		public ZString VehicleTitleState
		{
			get { return CusEntryLine.VehicleTitleState; }
		}

		public ZString UsedVehicleDetails
		{
			get { return (IsUsedVehicle) ? ZString.Format("{0}/{1}", VehicleID, VehicleTitle) : ZString.Empty; }
		}

		public ZString ECCN
		{
			get { return CusEntryLine.ECCN; }
		}

		public ZString AESOriginIndicator
		{
			get { return CusEntryLine.AESOriginIndicator; }
		}

		ZLong RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(ZDecimal value)
		{
			return value.RoundDownValueIfLessThanCriticalValueRoundUpOtherwise().ToZLong();
		}

		#region Implementation

		CusEntryLine CusEntryLine
		{
			get { return (CusEntryLine)WrappedObject; }
		}

		#endregion
	}
}
