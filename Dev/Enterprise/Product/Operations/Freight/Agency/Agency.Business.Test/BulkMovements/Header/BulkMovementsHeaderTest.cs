using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BulkMovementsHeader))]
	internal class BulkMovementsHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGenerate()
		{
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			depot.OH_Code = "Depot";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			stock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			Factory.Save();
			BulkMovementsChild child = new BulkMovementsHeader(Factory).Children.AddNew();
			child.ContainerNum = stock.R6_ContainerNum;
			child.ContainerType = stock.R6_RC;
			child.OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			child.DepotAddressPK = depot.MainAddress.PK;
			child.MovementType = ContainerMovementTypes.Codes.YardGateIn;
			child.MovementDate = ZDateTime.Now.ToSmallDateTime().AddDays(-1);
			BusinessObjectFactory createFactory = new BusinessObjectFactory();
			Factory.ResetDatabaseLoadCount();
			child.Generate(createFactory);
			AssertMaxDbHits("should not hav loaded anything in the BO factory.", 0, Factory);
			ContainerMovement[] movements = createFactory.Load<ContainerMovement>(new ZQuery()
			{ FetchOnlyFromLocalCache = true });
			AssertEquals("should have created 1 movement", 1, movements.Length);
			ContainerMovement movement = movements[0];
			CombineAssertions(delegate
			{
				AssertEquals("movement should not be saved", false, movements[0].IsInDatabase);
				AssertEquals("Stock", stock.PK, movement.E9_R6);
				AssertEquals("Movement Type", ContainerMovementTypes.Codes.YardGateIn, movement.E9_MovementType);
				AssertEquals("Movement Date", child.MovementDate, movement.E9_MovementDate);
				AssertEquals("Depot", depot.MainAddress.PK, movement.E9_OA_Depot);
			});
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkMovementsHeader(Factory);
		}
		#endregion
	}
}
