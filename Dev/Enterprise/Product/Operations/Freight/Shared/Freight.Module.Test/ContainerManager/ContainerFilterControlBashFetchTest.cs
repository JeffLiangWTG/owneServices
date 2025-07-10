using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module.Testing
{
	sealed class ContainerFilterControlBashFetchTest : FilterControlBashFetchHintTest<CommonContainer>
	{
		#region BashFetchTest

		public void TestBashFetchForView_JC_IsEmptyContainer()
		{
			BashFetchForView("JC_IsEmptyContainer", 0);
		}

		public void TestBashFetchForView_JC_IsDamaged()
		{
			BashFetchForView("JC_IsDamaged", 0);
		}

		public void TestBashFetchForView_JC_IsSealOk()
		{
			BashFetchForView("JC_IsSealOk", 0);
		}

		public void TestBashFetchForView_JC_IsControlledAtmosphere()
		{
			BashFetchForView("JC_IsControlledAtmosphere", 0);
		}

		public void TestBashFetchForView_JC_SetPointTemp()
		{
			BashFetchForView("JC_SetPointTemp", 0);
		}

		public void TestBashFetchForView_JC_SetPointTempUnit()
		{
			BashFetchForView("JC_SetPointTempUnit", 0);
		}

		public void TestBashFetchForView_JC_TempRecorderSerialNo()
		{
			BashFetchForView("JC_TempRecorderSerialNo", 0);
		}

		public void TestBashFetchForView_JC_RH_NKContainerCommodityCode()
		{
			BashFetchForView("JC_RH_NKContainerCommodityCode", 0);
		}

		public void TestBashFetchForView_JC_SystemCreateUser()
		{
			BashFetchForView("JC_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_JC_SystemCreateBranch()
		{
			BashFetchForView("JC_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_JC_SystemCreateDepartment()
		{
			BashFetchForView("JC_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_JC_SystemCreateTimeUtc()
		{
			BashFetchForView("JC_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_JC_SystemLastEditUser()
		{
			BashFetchForView("JC_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_JC_SystemLastEditTimeUtc()
		{
			BashFetchForView("JC_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_JC_ContainerNum()
		{
			BashFetchForView("JC_ContainerNum", 0);
		}

		public void TestBashFetchForView_JC_SealNum()
		{
			BashFetchForView("JC_SealNum", 0);
		}

		public void TestBashFetchForView_JC_ReleaseNum()
		{
			BashFetchForView("JC_ReleaseNum", 0);
		}

		public void TestBashFetchForView_JC_DepartureSlotReference()
		{
			BashFetchForView("JC_DepartureSlotReference", 0);
		}

		public void TestBashFetchForView_JC_ArrivalSlotReference()
		{
			BashFetchForView("JC_ArrivalSlotReference", 0);
		}

		public void TestBashFetchForView_Container_RC_ShippingMode()
		{
			// RefContainer: 1

			BashFetchForView("Container+RC_ShippingMode", 1);
		}

		public void TestBashFetchForView_JC_FCLAvailable()
		{
			// JobConsolTransport: 1

			BashFetchForView("JC_FCLAvailable", 1);
		}

		public void TestBashFetchForView_JC_ArrivalCTOStorageStartDate()
		{
			// JobBookedCtgMove: 12
			// JobConsolTransport: 1

			BashFetchForView("JC_ArrivalCTOStorageStartDate", 13);
		}

		public void TestBashFetchForView_JC_FCLOnBoardVessel()
		{
			BashFetchForView("JC_FCLOnBoardVessel", 0);
		}

		public void TestBashFetchForView_JC_FCLUnloadFromVessel()
		{
			BashFetchForView("JC_FCLUnloadFromVessel", 0);
		}

		public void TestBashFetchForView_JC_FCLWharfGateIn()
		{
			BashFetchForView("JC_FCLWharfGateIn", 0);
		}

		public void TestBashFetchForView_JC_FCLWharfGateOut()
		{
			BashFetchForView("JC_FCLWharfGateOut", 0);
		}

		public void TestBashFetchForView_JC_ContainerYardEmptyPickupGateOut()
		{
			BashFetchForView("JC_ContainerYardEmptyPickupGateOut", 0);
		}

		public void TestBashFetchForView_JC_ContainerYardEmptyReturnGateIn()
		{
			BashFetchForView("JC_ContainerYardEmptyReturnGateIn", 0);
		}

		public void TestBashFetchForView_JC_ContainerStatus()
		{
			BashFetchForView("JC_ContainerStatus", 0);
		}

		public void TestBashFetchForView_JC_ContainerQuality()
		{
			BashFetchForView("JC_ContainerQuality", 0);
		}

		public void TestBashFetchForView_JC_AdditionalSealNum()
		{
			BashFetchForView("JC_AdditionalSealNum", 0);
		}

		public void TestBashFetchForView_JC_Additional2SealNum()
		{
			BashFetchForView("JC_Additional2SealNum", 0);
		}

		public void TestBashFetchForView_JC_DeliverySequence()
		{
			BashFetchForView("JC_DeliverySequence", 0);
		}

		public void TestBashFetchForView_JC_SealParty()
		{
			BashFetchForView("JC_SealParty", 0);
		}

		public void TestBashFetchForView_JC_AdditionalSealParty()
		{
			BashFetchForView("JC_AdditionalSealParty", 0);
		}

		public void TestBashFetchForView_JC_Additional2SealParty()
		{
			BashFetchForView("JC_Additional2SealParty", 0);
		}

		public void TestBashFetchForView_JC_GrossWeightVerificationStatus()
		{
			BashFetchForView("JC_GrossWeightVerificationStatus", 0);
		}

		public void TestBashFetchForView_JC_EmptyReturnReference()
		{
			BashFetchForView("JC_EmptyReturnReference", 0);
		}

		public void TestBashFetchForView_AdditionalReferenceNumbersAsString()
		{
			BashFetchForView("AdditionalReferenceNumbersAsString", 1);
		}

		public void TestBashFetchForView_JC_RCA_AllocationLine()
		{
			BashFetchForView("JC_RCA_AllocationLine", 0);
		}

		#endregion

		protected override SchemaPKColumn PkColumn => JobContainerSchema.PK;

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			for (var i = 0; i < 12; i++)
			{
				var consol = factory.NewWithValidTestData<CommonConsol>();

				var container = consol.Containers.AddNew();
				container.JC_RC = refContainer.PK;

				result.Add(container.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = new ContainerNonDependentCollection(Factory);
			var filterBo = new ContainerManagerFilterStrip();
			return new ContainersFilterControl(collection, filterBo);
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new ContainerNonDependentCollection(Factory);
		}
	}
}
