using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using CodeDescriptionPairForTesting = Enterprise.Customs.DataTransfer.Universal.Testing.CodeDescriptionPairForTesting;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectWriterTest
	{
		void AssertContainUNDG(List<UNDG> undgDataCollection, ZString technicalName)
		{
			AssertContainUNDG(undgDataCollection, technicalName, Substance.DG_Code, "20", StaffBob.OC_ContactName);
		}

		void AssertContainUNDG(List<UNDG> undgDataCollection, ZString technicalName, ZString? code, ZString? flashPoint, ZString? contactName)
		{
			var undgData = undgDataCollection.FirstOrDefault(x => x.TechicalName.GetValueOrDefault() == technicalName);
			AssertNotNull(string.Format("Precondition: undgDataCollection should contain TechicalName({0})", technicalName), undgData);
			AssertEquals("undgData.UNDGCode", code, undgData.UNDGCode);
			AssertEquals("undgData.FlashPoint", flashPoint, undgData.FlashPoint);
			if (contactName.HasValue)
			{
				AssertNotNull("undgData.Contact", undgData.Contact);
				AssertEquals("undgData.Contact.FullName", contactName, undgData.Contact.FullName);
			}
			else
			{
				AssertNull("undgData.Contact", undgData.Contact);
			}
		}

		UNDGDataItem SetupUNDGDataItem(UNDGDataItem undg, ZString technicalName)
		{
			return SetupUNDGDataItem(undg, Substance.PK, 20m, StaffBob.PK, technicalName);
		}

		UNDGDataItem SetupUNDGDataItem(UNDGDataItem undg, ZGuid subsPK, ZDecimal flashPoint, ZGuid contactPK, ZString technicalName)
		{
			undg.DI_DG = subsPK;
			undg.DI_DGFlashPoint = flashPoint;
			undg.DI_OC_DGContact = contactPK;
			undg.DI_TechnicalName = technicalName;
			return undg;
		}

		void AssertInBondContainerContents(Container containerData, ZString? containerNumber, ZInt? containerLink)
		{
			AssertInBondContainerContents(containerData, containerNumber, containerLink, "S12", "S54", CodeDescriptionPairForTesting.New(ContainerTypeBO1.RC_Code, ContainerTypeBO1.RC_Description));
			AssertNotNull("containerData.UNDGCollection", containerData.UNDGCollection);
			AssertEquals("containerData.UNDGCollection.Count", 2, containerData.UNDGCollection.Count);
			AssertContainUNDG(containerData.UNDGCollection, "TECH121");
			AssertContainUNDG(containerData.UNDGCollection, "TECH212");
			AssertNotNull("containerData.AddInfoGroupCollection", containerData.AddInfoGroupCollection);
			var dispositionDataCollection = containerData.AddInfoGroupCollection.Where(x => x.Type.GetCodeAsUpperCase() == CusAddInfoTypeAttribute.Codes.USDisposition).ToArray();
			AssertEquals("dispositionDataCollection.Length", 3, dispositionDataCollection.Length);
			AssertInBondDisposition(dispositionDataCollection[0], DispositionList.Codes._2I, new ZDateTime(2014, 3, 6), 1);
			AssertInBondDisposition(dispositionDataCollection[1], DispositionList.Codes._2F, new ZDateTime(2014, 3, 5), 2);
			AssertInBondDisposition(dispositionDataCollection[2], DispositionList.Codes._3G, new ZDateTime(2014, 3, 7), 3);
		}

		void AssertInBondContainerContents(Container containerData, ZString? containerNumber, ZInt? containerLink, ZString? seal1, ZString? seal2, ICodeDescription containerType)
		{
			AssertNotNull("Precondition: containerData", containerData);
			CombineAssertions(delegate
			{
				AssertEquals("containerData.ContainerNumber", containerNumber, containerData.ContainerNumber);
				AssertEquals("containerData.Link", containerLink, containerData.Link);
				AssertEquals("containerData.Seal", seal1, containerData.Seal);
				AssertEquals("containerData.SecondSeal", seal2, containerData.SecondSeal);
				AssertNotNull("containerData.ContainerType", containerData.ContainerType);
				AssertEquals("containerData.ContainerType.Code", containerType.Code, containerData.ContainerType.Code);
				AssertEquals("containerData.ContainerType.Description", containerType.Description, containerData.ContainerType.Description);
			});
		}

		CusInBondContainer SetupCusInBondContainer(CusInBondContainer container, ZString containerNumber, ZString? warehouseEntryNumber = null)
		{
			SetupCusInBondContainer(container, containerNumber, "S12", "S54", ContainerTypeBO1.PK);
			var uNDG1 = SetupUNDGDataItem(container.UNDGs.AddNew(), "TECH212");
			var uNDG2 = SetupUNDGDataItem(container.UNDGs.AddNew(), "TECH121");
			var cargoDesc1 = SetupCusInBondCargoDesc(container.Commodities.AddNew(), "304050");
			var cargoDesc2 = SetupCusInBondCargoDesc(container.Commodities.AddNew(), "203040");
			if (warehouseEntryNumber.HasValue)
			{
				var bill = container.Bill;
				cargoDesc1 = SetupCusInBondCargoDescWarehouseData(cargoDesc1, warehouseEntryNumber.Value, 1);
				cargoDesc2 = SetupCusInBondCargoDescWarehouseData2(cargoDesc2, warehouseEntryNumber.Value, 2);
			}

			SetupDisposition(container.DispositionCodes.AddNew(), DispositionList.Codes._2F, new ZDateTime(2014, 3, 5), 2);
			SetupDisposition(container.DispositionCodes.AddNew(), DispositionList.Codes._2I, new ZDateTime(2014, 3, 6), 1);
			SetupDisposition(container.DispositionCodes.AddNew(), DispositionList.Codes._3G, new ZDateTime(2014, 3, 7), 3);
			return container;
		}

		CusInBondContainer SetupCusInBondContainer(CusInBondContainer container, ZString containerNumber, ZString seal1, ZString seal2, ZGuid containerTypePK)
		{
			container.BC_ContainerNum = containerNumber;
			container.BC_Seal1 = seal1;
			container.BC_Seal2 = seal2;
			container.BC_RC = containerTypePK;
			return container;
		}
	}
}
