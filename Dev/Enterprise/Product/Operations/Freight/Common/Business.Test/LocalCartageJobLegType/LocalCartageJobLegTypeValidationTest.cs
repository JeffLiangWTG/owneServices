using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class LocalCartageJobLegTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDisplayOrder()
		{
			CommonCartageType type = Factory.New<CommonCartageType>();
			CommonCartageLegType leg1 = type.LooseCartageLegTypes.AddNew();
			leg1.E4_DisplayOrder = 0;
			AssertHasError(leg1.E4_DisplayOrderInfo, "Display Order of the first Leg should be 1.");
			leg1.E4_DisplayOrder = 1;

			CommonCartageLegType leg2 = type.LooseCartageLegTypes.AddNew();
			AssertEquals((ZShort)2, leg2.E4_DisplayOrder);
			AssertNoErrors(leg2.E4_DisplayOrderInfo);

			CommonCartageLegType leg3 = type.LooseCartageLegTypes.AddNew();
			leg3.E4_DisplayOrder = 2;
			AssertHasError(leg3.E4_DisplayOrderInfo, "Display Orders must be sequential integers, continuously increasing by one.");

			leg3.E4_DisplayOrder = 4;
			AssertHasError(leg3.E4_DisplayOrderInfo, "Display Orders must be sequential integers, continuously increasing by one.");

			leg3.E4_DisplayOrder = 3;
			AssertNoErrors(leg3.E4_DisplayOrderInfo);
		}

		public void TestContainerMode()
		{
			LegType.E4_ContainerMode = "";
			AssertHasError(LegType.E4_ContainerModeInfo, "Container mode must have a value.");
		}

		public void TestFromOrg()
		{
			LegType.E4_E5_FromOrg = ZGuid.Empty;
			AssertHasError(LegType.E4_E5_FromOrgInfo, "From organization must have a value.");
		}

		public void TestToOrg()
		{
			LegType.E4_E5_ToOrg = ZGuid.Empty;
			AssertHasError(LegType.E4_E5_ToOrgInfo, "To organization must have a value.");
		}

		#region Implementation

		CommonCartageLegType LegType
		{
			get
			{
				if (fLegType == null)
				{
					fLegType = Factory.New<CommonCartageLegType>();
				}
				return fLegType;
			}
		}

		CommonCartageLegType fLegType;

		#endregion

	}
}
