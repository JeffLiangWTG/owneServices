using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class PreviousPermitNoCusSupporting : SingleCusSupportingInfo
	{
		public PreviousPermitNoCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.PreviousPermitNumber;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public override System.Collections.Generic.IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_ReferenceNumberInfo;
		}
	}
}
