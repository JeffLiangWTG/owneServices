using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryLineChargeDataObjectReader : DataObjectReader<UniversalCustoms.EntryLineCharge, CusEntryLineFee>
	{
		public CustomsEntryLineChargeDataObjectReader(UniversalCustoms.EntryLineCharge entryLineChargeDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, CusEntryLine entryLine)
			: base(entryLineChargeDataObject, logger, helper.Factory)
		{
			this.entryLine = Argument.NotNull(entryLine, "entryLine");
			this.helper = helper;
		}
		protected readonly CusEntryLine entryLine;
		protected readonly UniversalDataObjectReaderHelper helper;

		protected override CusEntryLineFee GetNewBusinessObject()
		{
			return (CusEntryLineFee)factory.New(entryLine.Fees.TypeOfElements);
		}

		protected override CusEntryLineFee GetExistingBusinessObject()
		{
			return null; // no matching as CusEntryLine is not matched
		}

		protected override void PopulateBusinessObject(CusEntryLineFee entryLineCharge)
		{
			var entryLineChargeRow = GetColumnIndexer(entryLineCharge);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_CL, entryLine.PK);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_ChargeType, dataObject.Type);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_RateOverrideReasonCode, dataObject.RateOverrideReason);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_BaseValue, dataObject.BaseValue);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_Rate, dataObject.Rate);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_Source, dataObject.Source);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_ChargeAmount, dataObject.Amount);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_MethodOfPayment, dataObject.MethodOfPayment);
			SetValue(entryLineChargeRow, CusEntryLineFeeSchema.CF_MethodOfCalculation, dataObject.MethodOfCalculation);
		}
	}
}
