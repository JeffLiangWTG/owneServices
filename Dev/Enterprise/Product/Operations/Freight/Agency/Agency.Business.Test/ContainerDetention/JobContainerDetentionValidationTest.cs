using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class JobContainerDetentionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDetentionType()
		{
			Detention.NC_DetentionType = "XXX";
			AssertHasError(Detention.NC_DetentionTypeInfo, "Enter a valid " + Detention.NC_DetentionTypeInfo.Description + ".");
			Detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
			AssertNoErrors(Detention.NC_DetentionTypeInfo);
			Detention.NC_DetentionType = "";
			AssertHasError(Detention.NC_DetentionTypeInfo, "Please enter a " + Detention.NC_DetentionTypeInfo.Description + ".");
		}

		public void TestClient()
		{
			Detention.NC_OH_Client = ZGuid.Invalid;
			AssertHasError(Detention.NC_OH_ClientInfo, "Enter a valid " + Detention.NC_OH_ClientInfo.Description + ".");
			Detention.NC_OH_Client = Client.PK;
			AssertNoErrors(Detention.NC_OH_ClientInfo);
			Detention.NC_OH_Client = ZGuid.Empty;
			AssertHasError(Detention.NC_OH_ClientInfo, "Please enter a " + Detention.NC_OH_ClientInfo.Description + ".");
		}

		public void TestPrincipal()
		{
			Principal.Factory.Save();
			Detention.NC_OH_Principal = ZGuid.Invalid;
			AssertHasError(Detention.NC_OH_PrincipalInfo, "Enter a valid " + Detention.NC_OH_PrincipalInfo.Description + ".");
			Detention.NC_OH_Principal = Principal.PK;
			AssertNoErrors(Detention.NC_OH_PrincipalInfo);
			Detention.NC_OH_Principal = ZGuid.Empty;
			AssertHasError(Detention.NC_OH_PrincipalInfo, "Please enter a " + Detention.NC_OH_PrincipalInfo.Description + ".");
		}

		#region Implementation
		ContainerDetention Detention
		{
			get
			{
				return detention ?? (detention = Factory.New<ContainerDetention>());
			}
		}

		ContainerDetention detention;
		OrgHeader Principal
		{
			get
			{
				return principal ?? (principal = BaseAgencyTest.NewPrincipal(Factory));
			}
		}

		OrgHeader principal;
		OrgHeader Client
		{
			get
			{
				return client ?? (client = Factory.New<OrgHeader>());
			}
		}

		OrgHeader client;
		#endregion
	}
}
