using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	class LaceyPGALineStatusTest : PGALineStatusTest
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<PGA>();
			originalBO.PG04ConstituentElements.AddNew();
			originalBO.Licenses.AddNew();
			originalBO.LaceyCountries.AddNew();

			var newBO = (PGA)originalBO.Clone();

			AssertEquals(1, newBO.PG04ConstituentElements.Count);
			AssertEquals(1, newBO.Licenses.Count);
			AssertEquals(1, newBO.LaceyCountries.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (PGA)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(PGA), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.PG04ConstituentElements[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Licenses[0].Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.LaceyCountries[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.PG04ConstituentElements[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.Licenses[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.LaceyCountries[0].Factory.GetHashCode());
		}

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
			get { return "APL"; }
		}

		PGA Header
		{
			get
			{
				if (header == null)
				{
					header = InvoiceLine.LaceyActLines.AddNew();
					header.US_PGALineItemNumber = 1;
				}
				return header;
			}
		}
		PGA header;
	}
}
