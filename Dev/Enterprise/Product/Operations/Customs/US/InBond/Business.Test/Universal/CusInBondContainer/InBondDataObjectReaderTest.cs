using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
{
	partial class InBondDataObjectReaderTest
	{
		public void TestImportingCusInBondContainerData()
		{
			var containerDataObject = SetupContainer("CONT323423", 1);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerDataObject }));
			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { SetupPackingLine("101010", 1), SetupPackingLine("202020", 1) })
			{ Content = CollectionContent.Complete });
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var helper = new InBondDataObjectReaderHelper(Factory);
			helper.CollectContainerPackingLineAndCommercialInvoiceLineDetails(shipment);
			Factory.SaveForTesting();
			var reader = new CusInBondContainerDataObjectReader(containerDataObject, logger, helper, moveDetail.PK);
			var containerBO = reader.ReadIntoBusinessObject();
			AssertNotNull(containerBO);
			CombineAssertions(delegate
			{
				AssertCusInBondContainerContents(containerBO, "CONT323423");
				AssertEquals("containerBO.BC_ParentID", moveDetail.PK, containerBO.BC_ParentID);
				AssertEquals("containerBO.BC_ParentTable", "B9", containerBO.BC_ParentTableCode);
				AssertEquals("containerBO.Commodities.Count", 2, containerBO.Commodities.Count);
				AssertCusInBondCargoDescContents(containerBO.Commodities[0], "101010");
				AssertCusInBondCargoDescContents(containerBO.Commodities[1], "202020");
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching CusInBondContainer found, creating new CusInBondContainer.
Information - Populating CusInBondContainer...
Information - Successfully loaded matching Container Type.
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching UNDGDataItem found, creating new UNDGDataItem.
Information - Populating UNDGDataItem...
Information - No matching CusInBondCargoDesc found, creating new CusInBondCargoDesc.
Information - Populating CusInBondCargoDesc...
Information - No matching CusInBondCargoDesc found, creating new CusInBondCargoDesc.
Information - Populating CusInBondCargoDesc...".Trim(), logger.Logs);
			});
		}

		UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup SetupDisposition()
		{
			return new UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup()
			{
				Type = new CodeDescriptionPair()
				{ Code = CusAddInfoTypeAttribute.Codes.USDisposition },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection(string.Format("{0}={1}*{2}=2013-01-31 14:56:00.000", USDispositionDataAddInfoSchema.Constants.US_Code.Substring(3), DispositionList.Codes._3I, USDispositionDataAddInfoSchema.Constants.US_DispositionDate.Substring(3)))
			};
		}

		void AssertContainUNDG(IEnumerable<UNDGDataItem> undgCollection, ZString technicalName, ZString code, ZDecimal flashPoint, ZGuid contactPK)
		{
			var undg = undgCollection.FirstOrDefault(x => x.DI_TechnicalName == technicalName);
			AssertNotNull(string.Format("Precondition: undgCollection should contain TechicalName({0})", technicalName), undg);
			AssertEquals("undg.Substance.DG_Code", code, undg.Substance?.DG_Code);
			AssertEquals("undg.DI_DGFlashPoint", flashPoint, undg.DI_DGFlashPoint);
			AssertEquals("undg.DI_OC_DGContact", contactPK, undg.DI_OC_DGContact);
		}

		UNDG SetupUNDG(ZString code, ZString technicalName, ZString flashPoint, ZString fullName, ZString phone)
		{
			return new UNDG(DefaultDataObjectWriterStrategy.TestInstance)
			{
				UNDGCode = code,
				TechicalName = technicalName,
				FlashPoint = flashPoint,
				Contact = new OrganizationContact()
				{ FullName = fullName, Phone = phone }
			};
		}

		Container SetupContainer(ZString containerNumber, ZInt link)
		{
			var result = SetupContainer(containerNumber, "SEAL1", "SEAL2", ContainerType1, link);
			result.SetUNDGCollection(() => new List<UNDG>(new[]
			{
				SetupUNDG(Substance.DG_Code, "BOB TECH", "34.3", StaffBob.OC_ContactName, StaffBob.OC_Phone),
				SetupUNDG(Substance2.DG_Code, "WENDY TECH", "-21.1", StaffWendy.OC_ContactName, StaffWendy.OC_Phone),
			}));
			result.AddInfoGroupCollection = new List<UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup>(new[] { SetupDisposition() });
			return result;
		}

		Container SetupContainer2(ZString containerNumber, ZInt link)
		{
			var result = SetupContainer(containerNumber, "SEAL4", "SEAL5", ContainerType2, link);
			result.SetUNDGCollection(() => new List<UNDG>(new[] { SetupUNDG(Substance2.DG_Code, "JACK TECH", "0", StaffJack.OC_ContactName, StaffJack.OC_Phone) }));
			return result;
		}

		void AssertCusInBondContainerContents(CusInBondContainer containerBO, ZString containerNumber)
		{
			AssertCusInBondContainerContents(containerBO, containerNumber, "SEAL1", "SEAL2", ContainerTypeBO1.PK);
			AssertEquals("containerBO.UNDGs.Count", 2, containerBO.UNDGs.Count);
			AssertContainUNDG(containerBO.UNDGs, "BOB TECH", Substance.DG_Code, 34.3, StaffBob.PK);
			AssertContainUNDG(containerBO.UNDGs, "WENDY TECH", Substance2.DG_Code, -21.1, StaffWendy.PK);
			AssertEquals("containerBO.DispositionCodes.Count", 0, containerBO.DispositionCodes.Count);
		}

		void AssertCusInBondContainerContents(CusInBondContainer containerBO, ZString containerNumber, ZString seal1, ZString seal2, ZGuid containerTypePK)
		{
			AssertEquals("containerBO.BC_ContainerNum", containerNumber, containerBO.BC_ContainerNum);
			AssertEquals("containerBO.BC_Seal1", seal1, containerBO.BC_Seal1);
			AssertEquals("containerBO.BC_Seal2", seal2, containerBO.BC_Seal2);
			AssertEquals("containerBO.BC_RC", containerTypePK, containerBO.BC_RC);
		}
	}
}
