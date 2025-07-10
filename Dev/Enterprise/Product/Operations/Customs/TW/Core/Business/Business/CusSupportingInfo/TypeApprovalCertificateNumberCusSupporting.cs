using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class TypeApprovalCertificateNumberCusSupporting : SingleCusSupportingInfo
	{
		public TypeApprovalCertificateNumberCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.TypeApprovalCertificateNumberCusSupporting|CSI_Description", Caption = "Party Identifier")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set
			{
				var oldValue = base.CSI_Description;
				base.CSI_Description = value;
				if (!IsCopying && oldValue != value)
				{
					if (CSI_Description.IsEmpty)
					{
						CSI_ReferenceNumber2 = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.TypeApprovalCertificateNumberCusSupporting|CSI_ReferenceNumber2", Caption = "Authorized Party")]
		public override ZString CSI_ReferenceNumber2 { get => base.CSI_ReferenceNumber2; set => base.CSI_ReferenceNumber2 = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.TypeApprovalCertificateNumber;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new TypeApprovalCertificateNumberCusSupportingValidation Validation => (TypeApprovalCertificateNumberCusSupportingValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new TypeApprovalCertificateNumberCusSupportingValidation(this);
		}

		public new JobComInvoiceLine Parent => base.Parent as JobComInvoiceLine;
	}
}
