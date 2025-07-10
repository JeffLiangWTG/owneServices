using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BulkMovementsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMovementType()
		{
			Header.MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			AssertNoNotifications(Header.MovementTypeInfo);
			Header.MovementType = "XXX";
			AssertHasError(Header.MovementTypeInfo, "Enter a valid Default Movement Type.");
			Header.MovementType = "";
			AssertNoNotifications(Header.MovementTypeInfo);
		}

		public void TestMovementDate()
		{
			Header.MovementDate = ZDateTime.Now;
			AssertNoNotifications(Header.MovementDateInfo);
			Header.MovementDate = ZDateTime.Now.AddDays(1);
			AssertHasWarning(Header.MovementDateInfo, "This date is in the future, you should only record movements that have actually taken place.");
			Header.MovementDate = ZDateTime.Empty;
			AssertNoNotifications(Header.MovementDateInfo);
		}

		public void TestPrincipalPK()
		{
			OrgHeader principal = BaseAgencyTest.NewPrincipal(Factory);
			Factory.Save();
			Header.PrincipalPK = principal.PK;
			AssertNoNotifications(Header.PrincipalPKInfo);
			Header.PrincipalPK = ZGuid.NewZGuid();
			AssertHasError(Header.PrincipalPKInfo, "Enter a valid Default Principal.");
			Header.PrincipalPK = ZGuid.Empty;
			AssertNoNotifications(Header.PrincipalPKInfo);
		}

		public void TestResponsiblePartyPK()
		{
			OrgHeader party = Factory.NewWithValidTestData<OrgHeader>();
			party.OH_Code = "Party";
			Factory.Save();
			Header.ResponsiblePartyPK = party.PK;
			AssertNoNotifications(Header.ResponsiblePartyPKInfo);
			Header.ResponsiblePartyPK = ZGuid.NewZGuid();
			AssertHasError(Header.ResponsiblePartyPKInfo, "Enter a valid Default Responsible Party.");
			Header.ResponsiblePartyPK = ZGuid.Empty;
			AssertNoNotifications(Header.ResponsiblePartyPKInfo);
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
		#endregion
	}
}
