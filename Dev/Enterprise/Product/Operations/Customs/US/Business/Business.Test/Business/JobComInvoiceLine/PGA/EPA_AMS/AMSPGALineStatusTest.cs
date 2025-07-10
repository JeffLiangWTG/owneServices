using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class AMSPGALineStatusTest : PGALineStatusTest
	{
		public override BusinessObject CollectionMaster
		{
			get { return Header; }
		}

		public override ZString GovernmentAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.AMS; }
		}

		public override IPGALineStatus Master
		{
			get { return Header; }
		}

		public override ZString ParentTableCode
		{
			get { return CusAddInfoSchema.Constants.Prefix; }
		}

		public override ZString PGAType
		{
			get { return CusDispositionTypeCodeList.Codes.USPGALineStatus; }
		}

		public override ZString ProgramCode
		{
			get { return "MO"; }
		}

		AMS Header
		{
			get
			{
				if (header == null && InvoiceLine is JobComInvoiceLine invoiceLine)
				{
					header = invoiceLine.AMSLines.AddNew();
					header.US_Program = "MO1";
					header.US_LineNo = 1;
				}
				return header;
			}
		}
		AMS header;
	}
}
