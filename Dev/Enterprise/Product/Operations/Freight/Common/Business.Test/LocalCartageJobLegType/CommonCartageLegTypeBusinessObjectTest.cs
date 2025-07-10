using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	[TestedType(typeof(CommonCartageLegType))]
	sealed class CommonCartageLegTypeBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChargeable()
		{
			CommonCartageType cartageType = Factory.New<CommonCartageType>();
			cartageType.E3_JobType = "ISM1";
			CommonCartageLegType containerizedMove = cartageType.ContainerizedBookedMoveTypes.AddNew();
			CommonCartageLegType containerizedLeg1 = cartageType.ContainerizedCartageLegTypes.AddNew();
			CommonCartageLegType containerizedLeg2 = cartageType.ContainerizedCartageLegTypes.AddNew();
			CommonCartageLegType looseMove = cartageType.LooseBookedMoveTypes.AddNew();
			CommonCartageLegType looseLeg = cartageType.LooseCartageLegTypes.AddNew();

			CommonCartageOrg wharf = cartageType.CommonCartageOrganisations.AddNew();
			CommonCartageOrg customer = cartageType.CommonCartageOrganisations.AddNew();
			CommonCartageOrg yard = cartageType.CommonCartageOrganisations.AddNew();
			CommonCartageOrg cfs = cartageType.CommonCartageOrganisations.AddNew();
			CommonCartageOrg staging = cartageType.CommonCartageOrganisations.AddNew();

			containerizedMove.E4_E5_FromOrg = wharf.PK;
			containerizedMove.E4_E5_WaitPointOrg = cfs.PK;
			containerizedMove.E4_E5_ToOrg = yard.PK;
			looseMove.E4_E5_FromOrg = cfs.PK;
			looseMove.E4_E5_WaitPointOrg = customer.PK;

			containerizedLeg1.E4_E5_FromOrg = wharf.PK;
			containerizedLeg1.E4_E5_ToOrg = cfs.PK;
			containerizedLeg2.E4_E5_FromOrg = cfs.PK;
			containerizedLeg2.E4_E5_ToOrg = yard.PK;
			looseLeg.E4_E5_FromOrg = cfs.PK;
			looseLeg.E4_E5_ToOrg = customer.PK;

			AssertEquals("Chargeable", containerizedLeg1.Chargeable);
			AssertEquals("Chargeable", containerizedLeg2.Chargeable);
			AssertEquals("Chargeable", looseLeg.Chargeable);

			containerizedMove.E4_E5_FromOrg = wharf.PK;
			containerizedMove.E4_E5_WaitPointOrg = cfs.PK;
			containerizedMove.E4_E5_ToOrg = ZGuid.Empty;
			AssertEquals("Chargeable", containerizedLeg1.Chargeable);
			AssertEquals("Not Chargeable", containerizedLeg2.Chargeable);
			AssertEquals("Chargeable", looseLeg.Chargeable);

			cartageType.E3_JobType = "ISM1";
			containerizedMove.E4_E5_FromOrg = cfs.PK;
			containerizedMove.E4_E5_WaitPointOrg = wharf.PK;
			containerizedMove.E4_E5_ToOrg = ZGuid.Empty;
			containerizedLeg1.E4_E5_FromOrg = yard.PK;
			containerizedLeg1.E4_E5_ToOrg = cfs.PK;
			containerizedLeg2.E4_E5_FromOrg = cfs.PK;
			containerizedLeg2.E4_E5_ToOrg = wharf.PK;

			AssertEquals("Not Chargeable", containerizedLeg1.Chargeable);
			AssertEquals("Chargeable", containerizedLeg2.Chargeable);
			AssertEquals("Chargeable", looseLeg.Chargeable);

			containerizedLeg1.E4_E5_FromOrg = cfs.PK;
			containerizedLeg1.E4_E5_ToOrg = staging.PK;
			containerizedLeg2.E4_E5_FromOrg = staging.PK;
			containerizedLeg2.E4_E5_ToOrg = wharf.PK;
			AssertEquals("Not Chargeable", containerizedLeg1.Chargeable);
			AssertEquals("Chargeable", containerizedLeg2.Chargeable);
			AssertEquals("Chargeable", looseLeg.Chargeable);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommonCartageType cartageType = factory.New<CommonCartageType>();
			cartageType.E3_GE = GlbDepartment.CurrentDepartment.PK;
			cartageType.E3_Description = "Some Description";
			cartageType.E3_JobType = "JOB";
			CommonCartageLegType cartageLegType = factory.New<CommonCartageLegType>();
			cartageLegType.E4_E3 = cartageType.PK;
			return cartageLegType;
		}

		#endregion
	}
}
