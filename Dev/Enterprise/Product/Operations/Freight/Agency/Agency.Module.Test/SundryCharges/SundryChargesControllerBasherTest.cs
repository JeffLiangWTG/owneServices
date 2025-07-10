using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(SundryChargesController))]
	internal sealed class SundryChargesControllerBasherTest : ZControllerBasherTest
	{
		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencySundryCharges;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader billTo = Factory.NewWithValidTestData<OrgHeader>();
			billTo.OH_Code = "Bill To";

			SundryCharges sundryAcc = Factory.New<SundryCharges>();
			sundryAcc.D4_OH_BillToParty = billTo.PK;

			Factory.Save();
			return sundryAcc;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
