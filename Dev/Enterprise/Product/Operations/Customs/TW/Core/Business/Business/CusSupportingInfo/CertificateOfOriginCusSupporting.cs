using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	public class CertificateOfOriginCusSupporting : SingleCusSupportingInfo
	{
		public CertificateOfOriginCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CertificateOfOriginCusSupporting|CSI_ReferenceNumber", Caption = "Certificate of Origin")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CertificateOfOriginCusSupporting|CSI_LineNo", Caption = "Certificate of Origin Line No.")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.CertificateOfOriginNumber;
		}

		public new CertificateOfOriginCusSupportingValidation Validation => (CertificateOfOriginCusSupportingValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new CertificateOfOriginCusSupportingValidation(this);
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;

		public override System.Collections.Generic.IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_LineNoInfo;
		}
	}
}
