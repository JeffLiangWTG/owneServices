using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DangerousGoodsManifestPortLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestValidatePortLookups_System()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();
			var port = portCollection.AddNew();
			port.Port = "AUSYD";
			port.PrincipalPK = ZGuid.NewZGuid();
			port.SenderID = "SenderID_1";
			port.Enabled = true;
			port.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
			AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection);
			var lookups = new DangerousGoodsManifestPortLookups(port, Factory);

			var result = string.Join(",", lookups.Port_List.Select(x => x.Code).OrderBy(s => s));
			AssertEquals("AUBNE,AUMEL,AUSYD", result);
		}

		public void TestValidatePortLookups_Australia()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();
			var port = portCollection.AddNew();
			port.Port = "AUSYD";
			port.PrincipalPK = ZGuid.NewZGuid();
			port.SenderID = "SenderID_1";
			port.Enabled = true;
			port.CurrentFallbackLevel = new FallbackLevel(CompanyAU.PK.ToGuid(), Guid.Empty, Guid.Empty);
			AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetValue(CompanyAU.PK.ToGuid(), Guid.Empty, Guid.Empty, portCollection);
			var lookups = new DangerousGoodsManifestPortLookups(port, Factory);

			var result = string.Join(",", lookups.Port_List.Select(x => x.Code).OrderBy(s => s));
			AssertEquals("AUBNE,AUMEL,AUSYD", result);
		}

		#region Implementation

		GlbCompany CompanyAU
		{
			get { return companyAU ?? (companyAU = Factory.NewWithValidTestData<GlbCompany>()); }
		}
		GlbCompany companyAU;

		#endregion
	}
}
