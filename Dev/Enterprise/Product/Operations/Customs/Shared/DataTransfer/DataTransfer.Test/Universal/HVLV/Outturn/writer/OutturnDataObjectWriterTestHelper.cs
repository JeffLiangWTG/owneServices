using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	public class OutturnDataObjectWriterTestHelper : OrganizationAddressTestHelper
	{
		protected void AssertTestOutturn(CusOutturn outturn, UShipment outturnData)
		{
			CombineAssertions(delegate
			{
				var description = outturn.Lookups.PackageTypes.GetDescriptionFromCode("PK");
				var expectedDescription = description ?? "";
				AssertEquals("OuterPacks", 9, outturnData.OuterPacks);
				AssertEquals("OuterPacksPackageType.Code", "PK", outturnData.OuterPacksPackageType.Code.GetValueOrDefault());
				AssertEquals("OuterPacksPackageType.Description", expectedDescription, outturnData.OuterPacksPackageType.Description.GetValueOrDefault());

				AssertEquals("TotalNoOfPacks", 10, outturnData.TotalNoOfPacks);
				AssertEquals("OuterPacksPackageType.Code", "PK", outturnData.OuterPacksPackageType.Code.GetValueOrDefault());
				AssertEquals("OuterPacksPackageType.Description", expectedDescription, outturnData.OuterPacksPackageType.Description.GetValueOrDefault());

				expectedDescription = outturn.CustomsStatus.Description;
				AssertEquals("EntryStatus.Code", "HLD", outturnData.EntryStatus.Code.GetValueOrDefault());
				AssertEquals("EntryStatus.Description", expectedDescription, outturnData.EntryStatus.Description.GetValueOrDefault());

				expectedDescription = outturn.MessageStatus.Description;
				AssertEquals("MessageStatus.Code", "REJ", outturnData.MessageStatus.Code.GetValueOrDefault());
				AssertEquals("MessageStatus.Description", expectedDescription, outturnData.MessageStatus.Description.GetValueOrDefault());

				description = outturn.Lookups.CommercialStatusList.GetDescriptionFromCode("CMS");
				expectedDescription = description ?? "";
				AssertEquals("OperationalStatus.Code", "CMS", outturnData.OperationalStatus.Code.GetValueOrDefault());
				AssertEquals("OperationalStatus.Description", expectedDescription, outturnData.OperationalStatus.Description.GetValueOrDefault());

				AssertAddInfo("IsDamage", outturnData, Constants.AddInfoKeys.Outturn.IsDamage, "Y");
				AssertAddInfo("IsPillage", outturnData, Constants.AddInfoKeys.Outturn.IsPillage, "Y");

				AssertNote("GoodsDescription", outturnData, Constants.Note.Descriptions.GoodsDescription, "Cuckoo Squeakers");
				AssertNote("MarksAndNumbersDescription", outturnData, Constants.Note.Descriptions.MarksAndNumbersDescription, "marks");

				description = outturn.Lookups.CargoTypes.GetDescriptionFromCode("LCL");
				expectedDescription = description ?? "";
				AssertContainer(outturnData, "OCLU1233510", "seal", true, "LCL", expectedDescription);

				AssertDate("Received", outturnData, DateType.Received, new ZDateTime(2019, 07, 25));
				AssertDate("Unpack", outturnData, DateType.Unpack, new ZDateTime(2019, 07, 28));

				AssertPackingLine("PackingLine", outturnData, 10.00D, "HB1");

				AssertAdditionalBill("AdditionalBill", outturnData, "MB1", "HB1");
			});
		}

		protected void AssertContainer(UShipment shipment, ZString expectedContainerNumber, ZString expectedSeal, ZBool expectedIsSealOk, ZString expectedContainerTypeCode, ZString expectedContainerTypeDesc)
		{
			var container = shipment.ContainerCollection.FirstOrDefault();
			AssertEquals("container.ContainerNumber", expectedContainerNumber, container.ContainerNumber);
			AssertEquals("container.Seal", expectedSeal, container.Seal);
			AssertEquals("container.IsSealOk", expectedIsSealOk, container.IsSealOk);
			AssertEquals("container.ContainerType.Code", expectedContainerTypeCode, container.ContainerType.Code);
			AssertEquals("container.ContainerType.Description", expectedContainerTypeDesc, container.ContainerType.Description);
		}

		protected void AssertAddInfo(ZString message, UShipment shipment, ZString addInfoType, ZString expectedValue)
		{
			var addinfo = shipment.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == addInfoType);
			AssertNotNull(message, addinfo);
			AssertEquals(message, expectedValue, addinfo.Value);
		}

		protected void AssertNote(ZString message, UShipment shipment, ZString descripton, ZString expectedNoteText, bool expectedIsCustomDescription = true)
		{
			var addinfo = shipment.NoteCollection.FirstOrDefault(x => x.Description.GetValueOrDefault() == descripton);
			AssertNotNull(message, addinfo);
			AssertEquals(message, expectedNoteText, addinfo.NoteText.GetValueOrDefault());
			AssertEquals(message, expectedIsCustomDescription, addinfo.IsCustomDescription.GetValueOrDefault());
		}

		protected void AssertDate(string message, UShipment shipment, DateType dateType, ZDateTime expectedDate, bool expectedIsEstimate = false)
		{
			var date = shipment.DateCollection.First(x => x.Type.GetValueOrDefault() == dateType);
			AssertEquals(message, expectedDate, date.Value.GetValueOrDefault());
			AssertEquals(message, expectedIsEstimate, date.IsEstimate.GetValueOrDefault());
		}

		protected void AssertPackingLine(string message, UShipment shipment, ZDecimal expectedVolume, ZString expectedBillnumber)
		{
			var packingLine = shipment.PackingLineCollection.FirstOrDefault();
			AssertEquals(message, expectedVolume, packingLine.OutturnedVolume);
			AssertEquals(message, expectedBillnumber, packingLine.BillNumber);
			AssertEquals(message, "HWB", packingLine.BillType.Code);
			AssertEquals(message, "House Waybill", packingLine.BillType.Description);
		}

		protected void AssertAdditionalBill(string message, UShipment shipment, ZString expectedParentBillnumber, ZString expectedBillnumber)
		{
			var additionBill = shipment.AdditionalBillCollection.FirstOrDefault();
			AssertEquals(message, expectedParentBillnumber, additionBill.ParentBillNumber);
			AssertEquals(message, expectedBillnumber, additionBill.BillNumber);
			AssertEquals(message, "HWB", additionBill.BillType.Code);
			AssertEquals(message, "House Waybill", additionBill.BillType.Description);
		}

		protected void AssertAdditionalReference(AdditionalReference additionalReferenceDataObject, ZString referenceNumber, ICodeDescription type, string contextInformation)
		{
			AssertNotNull("Precondition: additionalReferenceDataObject", additionalReferenceDataObject);
			AssertEquals("additionalReferenceDataObject.ContextInformation", contextInformation, additionalReferenceDataObject.ContextInformation);
			AssertEquals("additionalReferenceDataObject.ReferenceNumber", referenceNumber, additionalReferenceDataObject.ReferenceNumber);
			AssertNotNull("additionalReferenceDataObject.Type", additionalReferenceDataObject.Type);
			AssertEquals("additionalReferenceDataObject.Type.Code", type.Code, additionalReferenceDataObject.Type.Code);
			AssertEquals("additionalReferenceDataObject.Type.Description", type.Description, additionalReferenceDataObject.Type.Description);
		}
	}
}
