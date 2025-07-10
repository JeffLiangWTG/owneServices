using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusDecHouseContainerPivotValidationBaseOnlyTest : CusDecHouseContainerPivotValidationTest<BaseJobDeclaration, BasePackingGroup>
	{
	}

	public abstract class CusDecHouseContainerPivotValidationTest<TJobDeclaration, TPackingGroup> : BusinessObjectValidationTestCase
		where TJobDeclaration : BaseJobDeclaration
		where TPackingGroup : BasePackingGroup
	{
		public void TestCheckCR_CO_Container()
		{
			var mockDeclaration = Factory.NewMoq<TJobDeclaration>();
			mockDeclaration.Setup(m => m.ContainersRequired).Returns(false);
			var declaration = mockDeclaration.Object;
			var container = declaration.CusContainers.AddNew();
			var packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			AssertHasMessageError(packingGroup.CR_CO_ContainerInfo, CusDecHouseContainerPivotValidation.ContainerIsNotRequired);

			mockDeclaration.Reset();
			mockDeclaration.Setup(m => m.ContainersRequired).Returns(true);
			packingGroup.CR_CO_Container = container.PK;
			AssertNoMessageError(packingGroup.CR_CO_ContainerInfo, CusDecHouseContainerPivotValidation.ContainerIsNotRequired);
			mockDeclaration.Verify();
		}

		public void TestCheckCR_CEQ_Equipment()
		{
			var mockPackingGroup = Factory.NewMoq<TPackingGroup>();
			mockPackingGroup.Setup(m => m.CR_CO_Container).Returns(ZGuid.NewZGuid());
			mockPackingGroup.Setup(m => m.CR_CEQ_Equipment).Returns(ZGuid.NewZGuid());
			var packingGroup = mockPackingGroup.Object;
			packingGroup.Validation.ValidateCR_CEQ_Equipment();
			AssertHasError(packingGroup.CR_CEQ_EquipmentInfo, CusDecHouseContainerPivotValidation.EitherEquipmentOrContainer);

			mockPackingGroup.Reset();
			mockPackingGroup.Setup(m => m.CR_CO_Container).Returns(ZGuid.Empty);
			packingGroup.Validation.ValidateCR_CEQ_Equipment();
			AssertNoError(packingGroup.CR_CEQ_EquipmentInfo, CusDecHouseContainerPivotValidation.EitherEquipmentOrContainer);
		}
	}
}
