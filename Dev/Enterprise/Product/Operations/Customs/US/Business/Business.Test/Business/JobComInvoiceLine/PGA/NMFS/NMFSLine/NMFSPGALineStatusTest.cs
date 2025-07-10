using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class NMFSPGALineStatusTest : PGALineStatusTest
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<NMFSLine>();
			originalBO.HarvestingDetails.AddNew();

			var newBO = (NMFSLine)originalBO.Clone();

			AssertEquals(1, newBO.HarvestingDetails.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (NMFSLine)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(NMFSLine), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.HarvestingDetails[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.HarvestingDetails[0].Factory.GetHashCode());
		}

		public override BusinessObject CollectionMaster
		{
			get { return Header; }
		}

		public override ZString GovernmentAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.NMF; }
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
			get { return NMFSProgramCodeList.Codes.AMR; }
		}

		NMFSLine Header
		{
			get
			{
				if (header == null)
				{
					header = InvoiceLine.NMFSLines.AddNew();
					header.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
					header.US_LineNo = 1;
				}
				return header;
			}
		}
		NMFSLine header;
	}
}
