using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class MedicalInstrumentOrFoodCusSupporting : SingleCusSupportingInfo
	{
		public MedicalInstrumentOrFoodCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.MedicalInstrumentPartyIdentifier;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		public override System.Collections.Generic.IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_CodeInfo;
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_ReferenceNumber2Info;
		}
	}
}
