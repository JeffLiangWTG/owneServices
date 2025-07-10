using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class CusSealTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_EnRouteIncident()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var incident = (BusinessObject)Factory.New<Integration.Customs.EU.NCTS.IEnRouteIncident>() as CusInBondEvent;
				incident.BN_Type = CusInBondEventTypes.Codes.Incident;
				AssertGetTypeForLoad("Enterprise.Customs.EU.NCTS.Business.CusSeal", incident);
			}
		}

		public void TestGetTypeForLoad_NctsContainer()
		{
			BusinessObject CreateNctsContainer(string movementType)
			{
				var header = (CusInBondHeader)Factory.New<Integration.Customs.EU.NCTS.ICusInBondHeader>();
				header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
				header.BH_HeaderType = movementType;
				var container = Factory.New((header as ICusInBondContainerTypeSupporter).ContainerType);
				container[CusInBondContainer.Schema.BC_ParentTableCode] = header.TablePrefix;
				container[CusInBondContainer.Schema.BC_ParentID] = header.PK;
				return container;
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				AssertGetTypeForLoad("Enterprise.Customs.EU.NCTS.Business.CusSeal", CreateNctsContainer("A"));
				AssertGetTypeForLoad("Enterprise.Customs.EU.NCTS.Business.CusSeal", CreateNctsContainer("D"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
			{
				AssertGetTypeForLoad("Enterprise.Customs.CH.NCTS.Business.CusSeal", CreateNctsContainer("A"));
				AssertGetTypeForLoad("Enterprise.Customs.EU.NCTS.Business.CusSeal", CreateNctsContainer("D"));
			}
		}

		void AssertGetTypeForLoad(string typeFullName, BusinessObject parent)
		{
			var seal = Factory.New<CusSeal>();
			seal.BK_ParentTableCode = parent.TablePrefix;
			seal.BK_ParentID = parent.PK;
			var row = ((INeedRow)seal).Row;

			var typeDecider = new CusSealTypeDecider();
			var typeForLoad = typeDecider.GetTypeForLoad(row, Factory);
			AssertEquals(typeFullName, typeForLoad.FullName);
		}
	}
}
