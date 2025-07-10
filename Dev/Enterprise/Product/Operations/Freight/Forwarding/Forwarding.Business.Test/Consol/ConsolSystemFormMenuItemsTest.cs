using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolSystemFormMenuItemsTest : TestCaseWithFactory
	{
		public void TestConstantsCoverage()
		{
			var menuItems = typeof(ConsolSystemFormMenuItems)
				.GetProperties()
				.Select(f => f.GetValue(null))
				.OfType<ZGuid>()
				.Where(g => !g.IsEmpty)
				.ToArray();

			var query = new ZQuery(StmMenuItemSchema.PK, menuItems);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Enterprise.Core.Constants.StmMenuItemTypes.Forms);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Consol);

			var formsInDatabase = Factory
				.Load<StmMenuItem>(query)
				.Select(mi => mi.PK)
				.ToArray();

			AssertContainsExactElementsInAnyOrder("constants correspond with system db data", menuItems, formsInDatabase);
		}
	}
}
