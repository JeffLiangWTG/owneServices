using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class CPSCPGALineStatusTest : PGALineStatusTest
	{
		public override BusinessObject CollectionMaster
		{
			get { return Header; }
		}

		public override ZString GovernmentAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.CPS; }
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
			get { return ACEGovernmentAgenciesCodeList.Codes.CPS; }
		}

		CPSCHeader Header
		{
			get
			{
				if (header == null)
				{
					header = InvoiceLine.CPSCHeaders.AddNew();
					header.US_LineNo = 1;
				}
				return header;
			}
		}
		CPSCHeader header;
	}
}
