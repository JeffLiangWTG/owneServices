using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	[TestedType(typeof(ADDCVDAppliesApplicator))]
	public class ADDCVDAppliesApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestADDCVDApplies()
		{
			var clearance1 = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			consignment1.CusUSLVItems.AddNew();
			consignment1.CusUSLVItems.AddNew();

			var clearance2 = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			consignment2.CusUSLVItems.AddNew();

			Factory.Save();

			var view1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var view2 = Factory.Load<USConsignmentCombined>(consignment2.PK);

			ApplyApplicator(new BusinessObject[] { view1, view2 }, ZString.Empty);

			Assert("Should untick all ADD/CVD for consignment1", consignment1.CusUSLVItems.OfType<CusUSLVItem>().All(x => !x.ULI_AntiDumping && !x.ULI_Countervailing));
			Assert("Should untick all ADD/CVD for consignment2", consignment2.CusUSLVItems.OfType<CusUSLVItem>().All(x => !x.ULI_AntiDumping && !x.ULI_Countervailing));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ADDCVDAppliesApplicator();
		}

		#endregion
	}
}
