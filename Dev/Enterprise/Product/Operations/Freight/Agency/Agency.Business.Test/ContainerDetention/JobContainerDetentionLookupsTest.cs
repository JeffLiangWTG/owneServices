using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class JobContainerDetentionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDetentionTypes()
		{
			AssertType(typeof(DetentionInvoiceType), Detention.Lookups.DetentionTypes);
		}

		public void TestPrincipals()
		{
			AssertType(typeof(ShipsAgencyPrincipalCollection), Detention.Lookups.Principals);
		}

		public void TestMovements()
		{
			ContainerMovement ygi = NewMovement(ContainerMovementTypes.Codes.YardGateIn);
			ContainerMovement wgi = NewMovement(ContainerMovementTypes.Codes.WharfGateIn);
			ContainerMovement lod = NewMovement(ContainerMovementTypes.Codes.Load);
			ZQuery scopeFilter = new ZQuery(JobContainerMoveSchema.PK, new ZGuid[] { ygi.PK, wgi.PK, lod.PK, });
			Factory.Save();
			ContainerMovementCollection movements;
			movements = Detention.Lookups.Movements;
			AssertType(typeof(ContainerMovementCollection), Detention.Lookups.Movements);
			AssertHasDefault("Empty Detention Type", movements, ContainerMovementDefaultFilterProviderTest.DetentionInvoiced, "Property", (ZString)"NIV");
			AssertHasDefault("Empty Detention Type", movements, ContainerMovementDefaultFilterProviderTest.Detentionable, "Property", (ZString)"DET");
			AssertHasDefault("Empty Detention Type", movements, ContainerMovementDefaultFilterProviderTest.Principal, "Property", Principal.PK);
			AssertHasDefault("Empty Detention Type", movements, ContainerMovementDefaultFilterProviderTest.Client, "Property", Client.PK);
			AssertNoDefaults("Empty Detention Type", movements, ContainerMovementDefaultFilterProviderTest.TriggeringDetentions);
			AssertContainsExactElementsInAnyOrder("Empty Detention Type", (m) => m.E9_OtherLocation, System.Array.Empty<ContainerMovement>(), Factory.Load<ContainerMovement>(new ZQuery(scopeFilter, movements.AdditionalFilter)));
			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			movements = Detention.Lookups.Movements;
			AssertHasDefault("Import Detention Type", movements, ContainerMovementDefaultFilterProviderTest.TriggeringDetentions, "Property", (ZString)DetentionInvoiceType.Codes.Import);
			AssertContainsExactElementsInAnyOrder("Import Detention Type", (m) => m.E9_OtherLocation + ":" + m.PK.ToString(), new ContainerMovement[] { ygi }, Factory.Load<ContainerMovement>(new ZQuery(scopeFilter, movements.AdditionalFilter)));
			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Export;
			movements = Detention.Lookups.Movements;
			AssertHasDefault("Export Detention Type", movements, ContainerMovementDefaultFilterProviderTest.TriggeringDetentions, "Property", (ZString)DetentionInvoiceType.Codes.Export);
			AssertContainsExactElementsInAnyOrder("Export Detention Type", (m) => m.E9_OtherLocation, new ContainerMovement[] { wgi }, Factory.Load<ContainerMovement>(new ZQuery(scopeFilter, movements.AdditionalFilter)));
		}

		#region Implementation
		OrgHeader Principal
		{
			get
			{
				if (principal == null)
				{
					principal = Factory.NewWithValidTestData<OrgHeader>();
					principal.OH_Code = "Principal";
				}

				return principal;
			}
		}

		OrgHeader principal;
		OrgHeader Client
		{
			get
			{
				if (client == null)
				{
					client = Factory.NewWithValidTestData<OrgHeader>();
					client.OH_Code = "Client";
				}

				return client;
			}
		}

		OrgHeader client;
		ContainerDetention Detention
		{
			get
			{
				if (detention == null)
				{
					detention = Factory.New<ContainerDetention>();
					detention.NC_OH_Client = Client.PK;
					detention.NC_OH_Principal = Principal.PK;
				}

				return detention;
			}
		}

		ContainerDetention detention;
		RefContainerStock Stock
		{
			get
			{
				if (stock == null)
				{
					stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = "TEST4100013";
					stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock;
			}
		}

		RefContainerStock stock;
		ContainerMovement NewMovement(string movementType)
		{
			ContainerMovement movement = Stock.Movements.AddNew();
			movement.E9_MovementType = movementType;
			movement.E9_OtherLocation = movementType;
			return movement;
		}
		#endregion
	}
}
