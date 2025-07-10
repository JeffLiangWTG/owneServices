using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class HighTechLicenseCusSupporting : SingleCusSupportingInfo
	{
		public HighTechLicenseCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ShtcImportPermit;
		}

		public override System.Collections.Generic.IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_ReferenceNumberInfo;
		}
	}
}
