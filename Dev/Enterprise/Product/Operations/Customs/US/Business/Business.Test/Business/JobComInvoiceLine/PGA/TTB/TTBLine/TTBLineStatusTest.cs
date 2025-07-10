using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class TTBLineStatusTest : PGALineStatusTest
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<TTBLine>();
			originalBO.Cigars.AddNew();
			originalBO.COLAAndCertificates.AddNew();

			var newBO = (TTBLine)originalBO.Clone();

			AssertEquals(1, newBO.Cigars.Count);
			AssertEquals(1, newBO.COLAAndCertificates.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (TTBLine)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(TTBLine), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Cigars[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.COLAAndCertificates[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Cigars[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.COLAAndCertificates[0].Factory.GetHashCode());
		}

		public override BusinessObject CollectionMaster
		{
			get { return Header; }
		}

		public override ZString GovernmentAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.TTB; }
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
			get { return TTBProgramCodeList.Codes.Tobacco; }
		}

		TTBLine Header
		{
			get
			{
				if (header == null)
				{
					header = InvoiceLine.TTBLines.AddNew();
					header.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
					header.US_LineNo = 1;
				}
				return header;
			}
		}
		TTBLine header;
	}
}
