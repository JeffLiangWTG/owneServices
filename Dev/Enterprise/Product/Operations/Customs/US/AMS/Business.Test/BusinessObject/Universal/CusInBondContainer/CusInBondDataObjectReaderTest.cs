using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectReaderTest
	{
		Container SetupContainer(ZString containerNumber, ContainerType containerType, ZString sealNumber, ZString secondSealNumber, ZBool isEmpty, ZString foreignPort, ZString scheduleK, ZString serviceType, ZInt link)
		{
			var container = SetupContainer(containerNumber, sealNumber, secondSealNumber, containerType, link);
			container.IsEmptyContainer = isEmpty;
			container.AddInfoCollection = new List<AddInfo>()
			{
				new AddInfo() { Key = Constants.Container.AddInfo.ForeignPort, Value = foreignPort },
				new AddInfo() { Key = Constants.Container.AddInfo.ForeignPortScheduleK, Value = scheduleK },
				new AddInfo() { Key = Constants.Container.AddInfo.TypeOfService, Value = serviceType }
			};
			return container;
		}

		UNDG SetupUNDG(ZString subs, ZString flashPoint)
		{
			return new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				UNDGCode = subs,
				FlashPoint = flashPoint
			};
		}

		void SetupVehicle(Container container, ZString vehicleNumber)
		{
			var vehicleCollection = container.CustomsReferenceCollection ?? new List<UniversalCustoms.CustomsReference>();
			vehicleCollection.Add(new UniversalCustoms.CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = Constants.VehicleReference.Type },
				Reference = vehicleNumber
			});
			container.CustomsReferenceCollection = vehicleCollection;
		}

		void AssertCusInBondContainerContents(CusInBondContainer containerBO, ZString containerNumber, ZGuid containerTypePK, ZString sealNumber, ZString secondSealNumber, ZBool isEmpty, ZString foreignPort, ZString scheduleK, ZString serviceType)
		{
			AssertEquals("containerBO.BC_ContainerNum", containerNumber, containerBO.BC_ContainerNum);
			AssertEquals("containerBO.BC_RC", containerTypePK, containerBO.BC_RC);
			AssertEquals("containerBO.BC_Seal1", sealNumber, containerBO.BC_Seal1);
			AssertEquals("containerBO.BC_Seal2", secondSealNumber, containerBO.BC_Seal2);
			AssertEquals("containerBO.BC_IsEmpty", isEmpty, containerBO.BC_IsEmpty);
			AssertEquals("containerBO.BC_RL_NKForeignPort", foreignPort, containerBO.BC_RL_NKForeignPort);
			AssertEquals("containerBO.BC_ForeignPortKCode", scheduleK, containerBO.BC_ForeignPortKCode);
			AssertEquals("containerBO.BC_ForeignPortKCode", serviceType, containerBO.BC_TypeOfService);
		}

		void AssertUNDGContents(UNDGDataItem undgBO, ZString subs, ZDecimal flashPoint)
		{
			AssertEquals("undgBO.Substance.DG_Code", subs, undgBO.Substance?.DG_Code);
			AssertEquals("undgBO.DI_DGFlashPoint", flashPoint, undgBO.DI_DGFlashPoint);
		}
	}
}
