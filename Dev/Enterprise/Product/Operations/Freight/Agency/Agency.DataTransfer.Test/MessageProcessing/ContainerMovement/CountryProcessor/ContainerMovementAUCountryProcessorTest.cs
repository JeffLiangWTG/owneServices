using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer.MessageProcessing.TestFiles;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	abstract class ContainerMovementAUCountryProcessorTest : ContainerMovementBaseCountryProcessorTest<CMMAUCountryProcessor>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateContainer_Update()
		{
			var shipment = Factory.New<AgencyShipment>();
			ClearAUEntryNumber(shipment);
			var container = shipment.BookedContainers.AddNew();
			ClearAUEntryNumber(container);
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);

			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.UpdateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, "", "");
				AssertEntryNumber("Container", container, CusEntryNumberTypes.Australia.CAN, "freadofrog");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetNoNotifications(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateContainer_ShipmentLevelMatch()
		{
			var shipment = Factory.New<AgencyShipment>();
			SetAUEntryNumber(shipment, CusEntryNumberTypes.Australia.CAN, "freadofrog");
			var container = shipment.BookedContainers.AddNew();
			ClearAUEntryNumber(container);
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);

			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.UpdateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, CusEntryNumberTypes.Australia.CAN, "freadofrog");
				AssertEntryNumber("Container", container, "", "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetNoNotifications(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateContainer_ShipmentLevelCANConflict()
		{
			var shipment = Factory.New<AgencyShipment>();
			SetAUEntryNumber(shipment, CusEntryNumberTypes.Australia.CAN, "incumbant");
			var container = shipment.BookedContainers.AddNew();
			ClearAUEntryNumber(container);
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);

			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.UpdateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, CusEntryNumberTypes.Australia.CAN, "incumbant");
				AssertEntryNumber("Container", container, "", "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetShipmentLevelCANConflict(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateContainer_ShipmentLevelExConflict()
		{
			var shipment = Factory.New<AgencyShipment>();
			SetAUEntryNumber(shipment, CMRExportExemptionCodes.EXPE.Code, "");
			var container = shipment.BookedContainers.AddNew();
			ClearAUEntryNumber(container);
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);

			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.UpdateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, CMRExportExemptionCodes.EXPE.Code, "");
				AssertEntryNumber("Container", container, "", "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetShipmentLevelExConflict(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateContainer_ContainerLevelCANConflict()
		{
			var shipment = Factory.New<AgencyShipment>();
			ClearAUEntryNumber(shipment);
			var container = shipment.BookedContainers.AddNew();
			SetAUEntryNumber(container, CusEntryNumberTypes.Australia.CAN, "incumbant");
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);

			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.UpdateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, "", "");
				AssertEntryNumber("Container", container, CusEntryNumberTypes.Australia.CAN, "incumbant");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetContainerLevelCANConflict(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateContainer_ContainerLevelExConflict()
		{
			var shipment = Factory.New<AgencyShipment>();
			ClearAUEntryNumber(shipment);
			var container = shipment.BookedContainers.AddNew();
			SetAUEntryNumber(container, CMRExportExemptionCodes.EXPE.Code, "");
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);

			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.UpdateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, "", "");
				AssertEntryNumber("Container", container, CMRExportExemptionCodes.EXPE.Code, "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetContainerLevelExConflict(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateContainer_ContainerLevelMatch()
		{
			var shipment = Factory.New<AgencyShipment>();
			ClearAUEntryNumber(shipment);
			var container = shipment.BookedContainers.AddNew();
			SetAUEntryNumber(container, CusEntryNumberTypes.Australia.CAN, "freadofrog");
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);

			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.UpdateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, "", "");
				AssertEntryNumber("Container", container, CusEntryNumberTypes.Australia.CAN, "freadofrog");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetNoNotifications(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateContainer_NoUpdate()
		{
			var shipment = Factory.New<AgencyShipment>();
			ClearAUEntryNumber(shipment);
			var container = shipment.BookedContainers.AddNew();
			ClearAUEntryNumber(container);
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);

			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.ValidateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, "", "");
				AssertEntryNumber("Container", container, "", "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetNoUpdate(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateContainer_ShipmentLevelMatch()
		{
			var shipment = Factory.New<AgencyShipment>();
			SetAUEntryNumber(shipment, CusEntryNumberTypes.Australia.CAN, "freadofrog");
			var container = shipment.BookedContainers.AddNew();
			ClearAUEntryNumber(container);
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);
			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.ValidateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, CusEntryNumberTypes.Australia.CAN, "freadofrog");
				AssertEntryNumber("Container", container, "", "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetNoNotifications(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateContainer_ShipmentLevelCANConflict()
		{
			var shipment = Factory.New<AgencyShipment>();
			SetAUEntryNumber(shipment, CusEntryNumberTypes.Australia.CAN, "incumbant");
			var container = shipment.BookedContainers.AddNew();
			ClearAUEntryNumber(container);
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);
			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.ValidateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, CusEntryNumberTypes.Australia.CAN, "incumbant");
				AssertEntryNumber("Container", container, "", "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetShipmentLevelCANConflict(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateContainer_ShipmentLevelExConflict()
		{
			var shipment = Factory.New<AgencyShipment>();
			SetAUEntryNumber(shipment, CMRExportExemptionCodes.EXPE.Code, "");
			var container = shipment.BookedContainers.AddNew();
			ClearAUEntryNumber(container);
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);
			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.ValidateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, CMRExportExemptionCodes.EXPE.Code, "");
				AssertEntryNumber("Container", container, "", "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetShipmentLevelExConflict(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateContainer_ContainerLevelCANConflict()
		{
			var shipment = Factory.New<AgencyShipment>();
			ClearAUEntryNumber(shipment);
			var container = shipment.BookedContainers.AddNew();
			SetAUEntryNumber(container, CusEntryNumberTypes.Australia.CAN, "incumbant");
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);
			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.ValidateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, "", "");
				AssertEntryNumber("Container", container, CusEntryNumberTypes.Australia.CAN, "incumbant");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetContainerLevelCANConflict(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateContainer_ContainerLevelExConflict()
		{
			var shipment = Factory.New<AgencyShipment>();
			ClearAUEntryNumber(shipment);
			var container = shipment.BookedContainers.AddNew();
			SetAUEntryNumber(container, CMRExportExemptionCodes.EXPE.Code, "");
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);
			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.ValidateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, "", "");
				AssertEntryNumber("Container", container, CMRExportExemptionCodes.EXPE.Code, "");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetContainerLevelExConflict(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestValidateContainer_ContainerLevelMatch()
		{
			var shipment = Factory.New<AgencyShipment>();
			ClearAUEntryNumber(shipment);
			var container = shipment.BookedContainers.AddNew();
			SetAUEntryNumber(container, CusEntryNumberTypes.Australia.CAN, "freadofrog");
			var equipmentData = new Mock<CMMMessageContainer>(MockBehavior.Strict);
			Generator.SetSubjectDetail("TypeDescription CMM", "SenderName");
			Generator.WriteContainerHeader("ContainerNum", null);
			equipmentData.Setup(e => e.GoodsDeclarationNumber).Returns("freadofrog");

			Processor.ValidateContainer(equipmentData.Object, container);

			Generator.WriteContainerFooter();
			CombineAssertions(delegate
			{
				AssertEntryNumber("Shipment", shipment, "", "");
				AssertEntryNumber("Container", container, CusEntryNumberTypes.Australia.CAN, "freadofrog");
				AssertMultilineASCIIEquals("Email", CMMAUProcessor.GetNoNotifications(), Generator.ToString());
			});
			equipmentData.Verify(e => e.GoodsDeclarationNumber, Times.AtLeastOnce());
		}

		protected override CMMAUCountryProcessor NewProcessor() => new CMMAUCountryProcessor(Generator, GetAdapter());

		static ZQuery AUEntryNumberFilter(BusinessObject parent)
		{
			var filter = new ZQuery();
			filter.AddToFilter(CusEntryNumSchema.CE_ParentID, parent.PK);
			filter.AddToFilter(CusEntryNumSchema.CE_ParentTable, parent.TableName);
			filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Australia);
			return filter;
		}

		static void ClearAUEntryNumber(BusinessObject parent)
		{
			var numbers = parent.Factory.Load<CusEntryNumber>(AUEntryNumberFilter(parent));
			for (var i = 0; i < numbers.Length; i++)
			{
				numbers[i].Delete();
			}
		}

		static void SetAUEntryNumber(BusinessObject parent, string type, string number)
		{
			ClearAUEntryNumber(parent);
			var num = parent.Factory.New<CusEntryNumber>();
			num.CE_ParentID = parent.PK;
			num.CE_ParentTable = parent.TableName;
			num.CE_RN_NKCountryCode = Constants.CountryCodes.Australia;
			num.CE_EntryType = CMRExportExemptionCodes.Get3CharCode(type);
			num.CE_EntryNum = number;
		}

		static void AssertEntryNumber(string message, BusinessObject parent, string expectedType, string expectedNumber)
		{
			var numbers = parent.Factory.Load<CusEntryNumber>(AUEntryNumberFilter(parent));
			if (numbers.Length > 1)
			{
				Fail(message + ": too many entry numbers");
			}
			else
			{
				string actualType;
				string actualNumber;
				if (numbers.Length == 1)
				{
					actualType = CMRExportExemptionCodes.Get4CharCode(numbers[0].CE_EntryType);
					actualNumber = numbers[0].CE_EntryNum;
				}
				else
				{
					actualType = "";
					actualNumber = "";
				}

				AssertEquals(message + ": Entry Type", expectedType, actualType);
				AssertEquals(message + ": Entry Number", expectedNumber, actualNumber);
			}
		}

		static class CMMAUProcessor
		{
			const string prefix = "CMMAUProcessor\\";
			public static string GetContainerLevelCANConflict()
			{
				return TestFileHelper.GetText(prefix + "ContainerLevelCANConflict.htm");
			}

			public static string GetContainerLevelExConflict()
			{
				return TestFileHelper.GetText(prefix + "ContainerLevelExConflict.htm");
			}

			public static string GetNoNotifications()
			{
				return TestFileHelper.GetText(prefix + "NoNotifications.htm");
			}

			public static string GetNoUpdate()
			{
				return TestFileHelper.GetText(prefix + "NoUpdate.htm");
			}

			public static string GetShipmentLevelCANConflict()
			{
				return TestFileHelper.GetText(prefix + "ShipmentLevelCANConflict.htm");
			}

			public static string GetShipmentLevelExConflict()
			{
				return TestFileHelper.GetText(prefix + "ShipmentLevelExConflict.htm");
			}
		}
	}
}
