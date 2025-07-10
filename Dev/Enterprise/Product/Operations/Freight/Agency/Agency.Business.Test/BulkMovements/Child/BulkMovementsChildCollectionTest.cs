using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BulkMovementsChildCollection))]
	internal class BulkMovementsChildCollectionTest : NonPersistentBusinessObjectCollectionTestCase<BulkMovementsChildCollection>
	{
		public void TestDefaults()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			OrgHeader depot = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader responsible = Factory.NewWithValidTestData<OrgHeader>();
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			header.ContainerIsEmpty = true;
			header.DepotAddressPK = depot.MainAddress.PK;
			header.MovementDate = ZDateTime.Now;
			header.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			header.PrincipalPK = principal.PK;
			header.ResponsiblePartyPK = responsible.PK;
			BulkMovementsChildCollection collection = new BulkMovementsChildCollection(header);
			BulkMovementsChild child = collection.AddNew();
			CombineAssertions(delegate
			{
				AssertEquals("Contanier Is Empty", true, child.ContainerIsEmpty);
				AssertEquals("Depot", depot.MainAddress.PK, child.DepotAddressPK);
				AssertEquals("Movement Date", header.MovementDate, child.MovementDate);
				AssertEquals("Movement Type", ContainerMovementTypes.Codes.WharfGateIn, child.MovementType);
				AssertEquals("Principal", principal.PK, child.PrincipalPK);
				AssertEquals("Responsible Party", responsible.PK, child.ResponsiblePartyPK);
			});
		}

		public void TestSuspendValidationOnNewChild()
		{
			BulkMovementsHeader header = new BulkMovementsHeader(Factory);
			BulkMovementsChild child = header.Children.AddNew();
			AssertNoNotifications("validation should have been suspended", child);
			child.Validation.ValidateAll();
			Assert("postcondition: the data should be such that an error would have been added if it were validated", child.HasNotifications());
		}

		#region Implementation
		BulkMovementsHeader Header
		{
			get
			{
				return header ?? (header = new BulkMovementsHeader(Factory));
			}
		}

		BulkMovementsHeader header;
		protected override BulkMovementsChildCollection GetCollectionToTest()
		{
			return new BulkMovementsChildCollection(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BulkMovementsChild(Header);
		}
		#endregion
	}
}
