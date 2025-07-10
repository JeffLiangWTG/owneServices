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
	public class PreviousDocumentNumberCusSupporting : CusSupportingInfo
	{
		public PreviousDocumentNumberCusSupporting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZBool IsRowEmpty => CSI_ReferenceNumber.IsEmpty;

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.TW.Business.PreviousDocumentNumberCusSupporting|CSI_ReferenceNumber", Caption = "Previous Document Number", MediumCaption = "Previous Document Number", ShortCaption = "Previous Document Number")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.PreviousDocumentNumber;
			CSI_ParentTableCode = CusTWControllingMessageHeaderSchema.Constants.Prefix;
		}

		public new CusTWControllingMessageHeader Parent => base.Parent as CusTWControllingMessageHeader;

		protected override ZString HumanReadableNameCore => Res.GetString("43841588-776B-496A-B66E-E64E1F53A3E5", "Previous Document Number");
	}
}
