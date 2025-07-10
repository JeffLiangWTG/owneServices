using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C01")]
	public partial class AIIC01 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C02")]
	public partial class AIIC02 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C05")]
	public partial class AIIC05 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C06")]
	public partial class AIIC06 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C07")]
	public partial class AIIC07 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("CA1")]
	public partial class AIICA1 : MessageBlock, IShippingSymbolDescription
	{
		#region IShippingSymbolDescription Members

		ZString IShippingSymbolDescription.ShippingSymbolDescription
		{
			set { ShippingSymbolDescription = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("CB1")]
	public partial class AIICB1 : MessageBlock, IMarksNumbers
	{
		#region IMarksNumbers Members

		ZString IMarksNumbers.MarksNumbers
		{
			set { MarksNumbers = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C08")]
	public partial class AIIC08 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C09")]
	public partial class AIIC09 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C10")]
	public partial class AIIC10 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C11")]
	public partial class AIIC11 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C13")]
	public partial class AIIC13 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C17")]
	public partial class AIIC17 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C18")]
	public partial class AIIC18 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C19")]
	public partial class AIIC19 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C31")]
	public partial class AIIC31 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C33")]
	public partial class AIIC33 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C34")]
	public partial class AIIC34 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C35")]
	public partial class AIIC35 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C37")]
	public partial class AIIC37 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C39")]
	public partial class AIIC39 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C42")]
	public partial class AIIC42 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C43")]
	public partial class AIIC43 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C47")]
	public partial class AIIC47 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C49")]
	public partial class AIIC49 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("CC1")]
	public partial class AIICC1 : MessageBlock, IShippingSymbolDescription
	{
		#region IShippingSymbolDescription Members

		ZString IShippingSymbolDescription.ShippingSymbolDescription
		{
			set { ShippingSymbolDescription = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("CD1")]
	public partial class AIICD1 : MessageBlock, IMarksNumbers
	{
		#region IMarksNumbers Members

		ZString IMarksNumbers.MarksNumbers
		{
			set { MarksNumbers = value; }
		}

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C51")]
	public partial class AIIC51 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C53")]
	public partial class AIIC53 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C58")]
	public partial class AIIC58 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C59")]
	public partial class AIIC59 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C60")]
	public partial class AIIC60 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C61")]
	public partial class AIIC61 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C81")]
	public partial class AIIC81 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C84")]
	public partial class AIIC84 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ElectronicInvoiceResponse)]
	[OutputBlock("C95")]
	public partial class AIIC95 : MessageBlock { }
}