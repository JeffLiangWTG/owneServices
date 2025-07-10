using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business
{
	[TestedType(typeof(DocumentCartageLeg))]
	public class DocumentCartageLegTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cartageLeg = Factory.New<CommonCartageLeg>();
			return new DocumentCartageLeg(cartageLeg);
		}

		public void TestTotalPackCount()
		{
			var bookMove1 = Factory.NewWithValidTestData<CommonBookedCtgMove>();
			var bookMove2 = Factory.NewWithValidTestData<CommonBookedCtgMove>();
			var leg1 = bookMove1.CartageLegs.AddNew();
			var leg2 = bookMove2.CartageLegs.AddNew();
			var documentLeg = new DocumentCartageLeg(new CommonCartageLeg[] { leg1, leg2 });
			AssertEquals("There are no packs for booked moves so document leg should return zero.", 0, documentLeg.TotalPackCount);
			documentLeg = new DocumentCartageLeg(leg1);
			AssertEquals("There are no packs for the booked move so document leg should return zero.", 0, documentLeg.TotalPackCount);
			bookMove1.EW_BookedPackCount = 5;
			bookMove2.EW_BookedPackCount = 10;
			documentLeg = new DocumentCartageLeg(new CommonCartageLeg[] { leg1, leg2 });
			AssertEquals("Total pack count should be 15.", 15, documentLeg.TotalPackCount);
			documentLeg = new DocumentCartageLeg(leg1);
			AssertEquals("Since documnet Leg is having leg1 Total pack count should be 5.", 5, documentLeg.TotalPackCount);
		}

		public void TestTotalPackType()
		{
			var bookMove1 = Factory.NewWithValidTestData<CommonBookedCtgMove>();
			var bookMove2 = Factory.NewWithValidTestData<CommonBookedCtgMove>();
			var leg1 = bookMove1.CartageLegs.AddNew();
			var leg2 = bookMove2.CartageLegs.AddNew();
			var documentLeg = new DocumentCartageLeg(new CommonCartageLeg[] { leg1, leg2 });
			AssertEquals("There are no packs for booked moves therefore total pack type should be pieces.", Core.Constants.PkgUnit.Pallet, documentLeg.TotalPackType);
			documentLeg = new DocumentCartageLeg(leg1);
			AssertEquals("There are no packs for the booked move therefore total pack type should be pallet.", Core.Constants.PkgUnit.Pallet, documentLeg.TotalPackType);
			bookMove1.EW_F3_NKPackType = Core.Constants.PkgUnit.Box;
			bookMove2.EW_F3_NKPackType = Core.Constants.PkgUnit.Box;
			documentLeg = new DocumentCartageLeg(new CommonCartageLeg[] { leg1, leg2 });
			AssertEquals("All pack types are boxes therefore TotalPackType should be box.", Core.Constants.PkgUnit.Box, documentLeg.TotalPackType);
			documentLeg = new DocumentCartageLeg(leg1);
			AssertEquals("There is a one pack with box type therefore total packtype should be Box.", Core.Constants.PkgUnit.Box, documentLeg.TotalPackType);
			bookMove1.EW_F3_NKPackType = Core.Constants.PkgUnit.Carton;
			bookMove2.EW_F3_NKPackType = Core.Constants.PkgUnit.Box;
			documentLeg = new DocumentCartageLeg(new CommonCartageLeg[] { leg1, leg2 });
			AssertEquals("Packtypes are different in cartage legs therefore, TotalPackType should be pieces.", Core.Constants.PkgUnit.Piece, documentLeg.TotalPackType);
		}
	}
}
