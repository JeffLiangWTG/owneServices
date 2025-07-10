using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectWriterTest
	{
		void AssertInBondContainerContents(Container containerData, ZInt? link, ZString? containerNumber, ZString? containerType, ZString? sealNo, ZString? secondSealNo, ZBool? isEmptyContainer, ZString? foreignPort, ZString? foreignPortSchK, ZString? serviceType)
		{
			AssertNotNull("Precondition: containerData", containerData);
			CombineAssertions(() =>
			{
				AssertEquals("containerData.Link", link, containerData.Link);
				AssertEquals("containerData.ContainerNumber", containerNumber, containerData.ContainerNumber);
				AssertEquals("containerData.ContainerType", containerType.GetValueOrDefault(), containerData.ContainerType.GetCodeAsUpperCase());
				AssertEquals("containerData.Seal", sealNo, containerData.Seal);
				AssertEquals("containerData.SecondSeal", secondSealNo, containerData.SecondSeal);
				AssertEquals("containerData.IsEmptyContainer", isEmptyContainer, containerData.IsEmptyContainer);
				if (foreignPort.HasValue)
				{
					AssertCollectionContains(containerData.AddInfoCollection, Constants.Container.AddInfo.ForeignPort, foreignPort);
				}

				if (foreignPortSchK.HasValue)
				{
					AssertCollectionContains(containerData.AddInfoCollection, Constants.Container.AddInfo.ForeignPortScheduleK, foreignPortSchK);
				}

				if (serviceType.HasValue)
				{
					AssertCollectionContains(containerData.AddInfoCollection, Constants.Container.AddInfo.TypeOfService, serviceType);
				}
			});
		}

		void AssertInBondContainerContents(Container containerData, ZInt? link, ZString containerNumber)
		{
			AssertInBondContainerContents(containerData, link, containerNumber, ContainerTypeBO1.RC_Code, "S001", "S002", false, null, null, ServiceTypeList.Codes.PierToPier);
		}

		void AssertContainUNDG(List<UNDG> undgCollection, ZString subs, ZString flashPoint)
		{
			var undgData = undgCollection.FirstOrDefault(x => x.UNDGCode.GetValueOrDefault() == subs);
			AssertNotNull(string.Format("Precondition: undgDataCollection should contain Subs({0})", subs), undgData);
			AssertEquals("undgData.FlashPoint", flashPoint, undgData.FlashPoint);
		}

		void AssertContainVehicleNumber(List<UniversalCustoms.CustomsReference> customsReferences, ZString vin)
		{
			var matchedVIN = customsReferences.FirstOrDefault(x => x.Reference.GetValueOrDefault() == vin);
			AssertNotNull(string.Format("Precondition: CustomsReferenceCollection contains VIN({0})", vin), matchedVIN);
		}

		void SetupCusInBondContainer(CusInBondContainer container, ZString containerNum)
		{
			SetupCusInBondContainer(container, containerNum, ContainerTypeBO1.PK, "S001", "S002", false, "", "", ServiceTypeList.Codes.PierToPier);
		}

		void SetupUNDGForContainer(CusInBondContainer container, ZGuid subsPK, ZDecimal flashPoint)
		{
			var undg = container.UNDGs.AddNew();
			undg.DI_DG = subsPK;
			undg.DI_DGFlashPoint = flashPoint;
		}

		void SetupVehicleDetail(CusInBondContainer container, ZString vehicleNumber)
		{
			var vehicle = container.Vehicles.AddNew();
			vehicle.BV_VIN = vehicleNumber;
		}

		void SetupCusInBondContainer(CusInBondContainer container, ZString containerNum, ZGuid containerTypePK, ZString seal, ZString seal2, ZBool isEmpty, ZString foreignPort, ZString scheduleK, ZString serviceType)
		{
			container.BC_ContainerNum = containerNum;
			container.BC_RC = containerTypePK;
			container.BC_Seal1 = seal;
			container.BC_Seal2 = seal2;
			container.BC_IsEmpty = isEmpty;
			container.BC_RL_NKForeignPort = foreignPort;
			container.BC_ForeignPortKCode = scheduleK;
			container.BC_TypeOfService = serviceType;
		}
	}
}
