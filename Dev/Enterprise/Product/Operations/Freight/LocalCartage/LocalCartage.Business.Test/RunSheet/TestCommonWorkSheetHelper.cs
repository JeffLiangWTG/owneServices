using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class TestCommonWorkSheetHelper
	{
		readonly BusinessObjectFactory Factory;
		public TestCommonWorkSheetHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
			RefContainer = CreateContainer("20GP-TEST");
		}

		readonly RefContainer RefContainer;
		public IEnumerable<CommonCartageLeg> CreateCartageLegs(CommonCartage cartage, int numLegs)
		{
			return Enumerable.Range(1, numLegs).Select(sequence =>
			{
				var leg = Factory.New<CommonCartageLeg>();
				leg.JU_RunSheetSequence = sequence;
				var move = Factory.New<CommonBookedCtgMove>();
				move.EW_JJ = cartage.PK;
				leg.JU_EW = move.PK;
				var commonContainer = Factory.New<CommonContainer>();
				commonContainer.JC_RC = RefContainer.PK;
				move.EW_JC_Container = commonContainer.PK;
				return leg;
			});
		}

		public CommonCartageLeg CreateCartageLeg(bool isContainerised)
		{
			var move = Factory.New<CommonBookedCtgMove>();
			if (isContainerised)
			{
				var container = Factory.New<CommonContainer>();
				container.JC_RC = RefContainer.PK;
				move.EW_JC_Container = container.PK;
			}

			var leg = Factory.New<CommonCartageLeg>();
			leg.JU_EW = move.PK;
			return leg;
		}

		public RefContainer CreateContainer(string code)
		{
			var container = Factory.New<RefContainer>();
			container.RC_Code = code;
			return container;
		}

		public OrgHeader CreateOrganisation(ZString code)
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = code;
			return organisation;
		}
	}
}
