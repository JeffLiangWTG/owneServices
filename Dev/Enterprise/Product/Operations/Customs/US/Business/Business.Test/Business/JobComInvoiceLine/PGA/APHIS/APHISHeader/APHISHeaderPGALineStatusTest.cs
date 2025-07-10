using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class APHISHeaderPGALineStatusTest : PGALineStatusTest
	{
		public override BusinessObject CollectionMaster
		{
			get { return Header; }
		}

		public override ZString GovernmentAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.APH; }
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
			get { return APHISProgramCodeList.Codes.AVS; }
		}

		APHISHeader Header
		{
			get
			{
				if (header == null)
				{
					header = InvoiceLine.APHISHeaders.AddNew();
					header.US_LineNo = 1;
					header.US_ProgramType = APHISProgramCodeList.Codes.AVS;
				}
				return header;
			}
		}
		APHISHeader header;
	}
}
