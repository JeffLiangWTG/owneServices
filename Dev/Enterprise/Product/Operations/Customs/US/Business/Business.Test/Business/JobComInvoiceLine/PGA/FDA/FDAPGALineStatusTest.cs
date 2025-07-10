using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class FDAPGALineStatusTest : PGALineStatusTest
	{
		public override BusinessObject CollectionMaster
		{
			get { return Header; }
		}

		public override ZString GovernmentAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.FDA; }
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
			get { return "FOO"; }
		}

		ACEFDA Header
		{
			get
			{
				if (header == null)
				{
					header = InvoiceLine.ACE_FDALines.AddNew();
					header.US_ProgramCode = "FOO";
					header.US_LineNo = 1;
				}
				return header;
			}
		}
		ACEFDA header;
	}
}
