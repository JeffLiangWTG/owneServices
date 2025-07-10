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
	public class EthanolPermitNumberCusSupporting : CusSupportingInfo
	{
		public EthanolPermitNumberCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(14)]
		[ResourceStringData("Enterprise.Customs.TW.Business.EthanolPermitNumberCusSupporting.CSI_ReferenceNumber", Caption = "Ethanol Permit Number", FullDescription = "Ethanol Permit Number: Undenatured ethanol used for alcohol production should be enclosed with the approval documents issued by the Ministry of Finance.")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		public ZBool IsRowEmpty => CSI_ReferenceNumber.IsEmpty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			CSI_Type = CusSupportingInfoTypeList.Codes.EthanolPermitNumber;
			CSI_ParentTableCode = CusTWControllingMessageHeaderSchema.Constants.Prefix;
		}

		public new EthanolPermitNumberCusSupportingValidation Validation => (EthanolPermitNumberCusSupportingValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new EthanolPermitNumberCusSupportingValidation(this);
		}

		public new CusTWControllingMessageHeader Parent => base.Parent as CusTWControllingMessageHeader;

		protected override ZString HumanReadableNameCore => Res.GetString("aa3944d5-6918-41f5-8599-6a2cd42edada", "Ethanol Permit Number");
	}
}
