using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class QuotaPermitNumberCusSupporting : SingleCusSupportingInfo
	{
		public QuotaPermitNumberCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.CitesImportPermit;
		}

		public new QuotaPermitNumberCusSupportingValidation Validation => (QuotaPermitNumberCusSupportingValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new QuotaPermitNumberCusSupportingValidation(this);
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		public override System.Collections.Generic.IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_LineNoInfo;
		}
	}
}
