using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public class PreviousDocument : CusSupportingInfo
{
	public PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : AutoCusSupportingInfo.Schema
	{
		public const int ReferenceNumberMaxLength = 35;
	}

	[ResourceStringData("NO.PreviousDocument|CSI_Code", Caption = "Type")]
	public override ZString CSI_Code
	{
		get => base.CSI_Code;
		set => base.CSI_Code = value;
	}

	[ResourceStringData("NO.PreviousDocument|CSI_Reference", Caption = "Reference")]
	[MaxLength(Schema.ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber
	{
		get => base.CSI_ReferenceNumber;
		set => base.CSI_ReferenceNumber = value;
	}

	[ResourceStringData("NO.PreviousDocument|CSI_LineNo", Caption = "Line No.")]
	public override ZInt CSI_LineNo
	{
		get => base.CSI_LineNo;
		set => base.CSI_LineNo = value;
	}

	[ResourceStringData("NO.PreviousDocument|CSI_Quantity", Caption = "Package Qty.")]
	public override ZDecimal CSI_Quantity
	{
		get => base.CSI_Quantity;
		set => base.CSI_Quantity = value;
	}

	[ResourceStringData("NO.PreviousDocument|CSI_Reference2", Caption = "Reference 2")]
	[MaxLength(Schema.ReferenceNumberMaxLength)]
	public override ZString CSI_ReferenceNumber2
	{
		get => base.CSI_ReferenceNumber2;
		set => base.CSI_ReferenceNumber2 = value;
	}
}
