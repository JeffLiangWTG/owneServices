using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BulkMovementsApplicatorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDepotAddressPK()
		{
			OrgHeader yard = Factory.NewWithValidTestData<OrgHeader>();
			yard.OH_Code = "Yard";
			yard.OH_IsMiscFreightServices = true;
			yard.OH_IsContainerYard = true;
			Factory.Save();
			Applicator.DepotAddressPK = yard.MainAddress.PK;
			AssertNoNotifications(Applicator.DepotAddressPKInfo);
			Applicator.DepotAddressPK = ZGuid.Empty;
			AssertHasError(Applicator.DepotAddressPKInfo, "Please enter a Depot.");
		}

		public void TestMovementType()
		{
			Applicator.MovementType = "XXX";
			AssertHasError(Applicator.MovementTypeInfo, "Enter a valid Movement Type.");
			Applicator.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertNoNotifications(Applicator.MovementTypeInfo);
			Applicator.MovementType = "";
			AssertHasError(Applicator.MovementTypeInfo, "Please enter a Movement Type.");
		}

		public void TestCheckMovementdate()
		{
			Applicator.MovementDate = ZDateTime.Now.AddDays(-1);
			AssertNoNotifications(Applicator.MovementDateInfo);
			Applicator.MovementDate = ZDateTime.Empty;
			AssertHasError(Applicator.MovementDateInfo, "Please enter a Movement Date.");
			AssertNoWarnings(Applicator.MovementDateInfo);
			Applicator.MovementDate = ZDateTime.Now.AddDays(1);
			AssertNoErrors(Applicator.MovementDateInfo);
			AssertHasWarning(Applicator.MovementDateInfo, "This date is in the future, you should only add movements that have actually taken place.");
		}

		#region Implementation
		BulkMovementsApplicator Applicator
		{
			get
			{
				return applicator ?? (applicator = new BulkMovementsApplicator(Factory));
			}
		}

		BulkMovementsApplicator applicator;
		#endregion
	}
}
