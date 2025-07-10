using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class CMCertificateOfOriginCusSupporting : CusSupportingInfo
	{
		public CMCertificateOfOriginCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZBool IsRowEmpty => CSI_ReferenceNumber.IsEmpty;

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CMCertificateOfOriginCusSupporting|CSI_ReferenceNumber", Caption = "Certificate of Origin Number", MediumCaption = "COO Number", ShortCaption = "COO No.", FullDescription = "Indicates the certificate of origin number from the country of origin. When Certificate Type is '17', this column must be filled in.")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.CmCertificateOfOriginNumber;
			CSI_ParentTableCode = CusTWControllingMessageHeaderSchema.Constants.Prefix;
		}

		public new CusTWControllingMessageHeader Parent => base.Parent as CusTWControllingMessageHeader;

		protected override ZString HumanReadableNameCore => Res.GetString("E2FFAFD6-5D9B-475D-98E7-92CE909864F9", "Certificate of Origin Number");
	}
}
