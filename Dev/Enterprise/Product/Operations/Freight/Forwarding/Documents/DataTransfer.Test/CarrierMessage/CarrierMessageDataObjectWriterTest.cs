using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.CodeLists;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Country = Enterprise.DocumentVisualizer.DocDataObjects.Country;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class CarrierMessageDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestBookingRequestAndShippingInstructionsForIsCoLoadAddInfo()
		{
			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = PrepareTestData();
				carrierMessageData.IsCoload = true;

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var dataObject = writer.GetDataObject(carrierMessageData);

				Assert(dataObject.AddInfoCollection.Exists(info => info.Key.HasValue && info.Value.HasValue && info.Key.Value == DocDataConstants.AddinfoTypes.IsCoLoad && info.Value.Value == "true"));
			}

			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = PrepareTestData();
				carrierMessageData.IsNVO = true;

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var dataObject = writer.GetDataObject(carrierMessageData);

				Assert(dataObject.AddInfoCollection.Exists(info => info.Key.HasValue && info.Value.HasValue && info.Key.Value == DocDataConstants.AddinfoTypes.IsCoLoad && info.Value.Value == "true"));
			}

			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = PrepareTestData();

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var dataObject = writer.GetDataObject(carrierMessageData);

				Assert(!dataObject.AddInfoCollection.Exists(info => info.Key.HasValue && info.Value.HasValue && info.Key.Value == DocDataConstants.AddinfoTypes.IsCoLoad && info.Value.Value == "true"));
			}
		}

		public void TestBookingRequestAndShippingInstructionsEnhancements()
		{
			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = PrepareTestData();
				carrierMessageData.IsDoorDelivery = false;
				carrierMessageData.IsDoorPickup = false;

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var dataObject = writer.GetDataObject(carrierMessageData);

				Assert(!dataObject.OrganizationAddressCollection.Exists(address => address.AddressType.HasValue && address.CompanyName.HasValue && address.AddressType.Value == "ConsignorPickupDeliveryAddress" && address.CompanyName.Value == "PickupFrom"));
				Assert(!dataObject.OrganizationAddressCollection.Exists(address => address.AddressType.HasValue && address.CompanyName.HasValue && address.AddressType.Value == "ConsigneePickupDeliveryAddress" && address.CompanyName.Value == "DeliverTo"));
			}
		}

		public void TestGensetInBookingRequest()
		{
			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = PrepareTestData();

				Assert("precondition", carrierMessageData.Containers.Any());

				carrierMessageData.Containers.ForEach(container =>
				{
					container.Genset = true;
				});

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var dataObject = writer.GetDataObject(carrierMessageData);

				dataObject.ContainerCollection.ForEach(container =>
				{
					var gensets = container.AddInfoCollection.FindAll(info => info.Key.ToString() == "Genset");

					AssertEquals(1, gensets.Count);

					var genset = gensets[0];

					AssertEquals("true", genset.Value);
				});
			}

			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = PrepareTestData();

				Assert("precondition", carrierMessageData.Containers.Any());

				carrierMessageData.Containers.ForEach(container =>
				{
					container.Genset = false;
				});

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var dataObject = writer.GetDataObject(carrierMessageData);

				dataObject.ContainerCollection.ForEach(container =>
				{
					var gensets = container.AddInfoCollection.FindAll(info => info.Key.ToString() == "Genset");

					AssertEquals(1, gensets.Count);

					var genset = gensets[0];

					AssertEquals("false", genset.Value);
				});
			}
		}

		public void TestPopulateDataObject()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "2.5.0", false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "2.5.0", false);

					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "2.5.0", true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "2.5.0", true);
				});
				CombineAssertions("FreightDataRegistry.Instance.EnableBookingConfirmation", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "3.0.0", false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "2.5.0", false);

					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "3.0.0", true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "2.5.0", true);
				});
			}
		}

		public void TestPopulateDataObject_IsGroupAndConsolidatePackingLines()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "4.0.0", false, true, false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "4.0.0", false, true, false);

					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "4.0.0", true, true, false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "4.0.0", true, true, false);
				});
				CombineAssertions("FreightDataRegistry.Instance.EnableBookingConfirmation", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "4.0.0", false, true, false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "4.0.0", false, true, false);

					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "4.0.0", true, true, false);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "4.0.0", true, true, false);
				});
			}
		}

		public void TestPopulateDataObject_DoNotGroup()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.EnableBookingConfirmation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CombineAssertions("FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "4.0.0", false, true, true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "4.0.0", false, true, true);

					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, true, "4.0.0", true, true, true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableChinaCustomsTaxNumberTable, false, "4.0.0", true, true, true);
				});
				CombineAssertions("FreightDataRegistry.Instance.EnableBookingConfirmation", () =>
				{
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "4.0.0", false, true, true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "4.0.0", false, true, true);

					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, true, "4.0.0", true, true, true);
					AssertPopulateDataObject(FreightDataRegistry.Instance.EnableBookingConfirmation, false, "4.0.0", true, true, true);
				});
			}
		}

		public void TestPopulateAttachedDocuments()
		{
			var carrierMessageData = PrepareTestData();

			var document = new DummyDocument();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction, document);

			carrierMessageData.IsRequiredSendAttachment = false;
			using (var dataObject = writer.GetDataObject(carrierMessageData))
			{
				AssertNull(dataObject.AttachedDocumentCollection);
			}

			carrierMessageData.IsRequiredSendAttachment = true;
			using (var dataObject = writer.GetDataObject(carrierMessageData))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(1, attachments.Count);

				DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachments[0], new DataObjectWriterHelper.FileAttributes()
				{
					Name = "Shipping Instruction",
					Description = "Shipping Instruction",
					Code = "SHI",
					IsPublished = false
				});
			}

			carrierMessageData = PrepareTestData(isShippingInstruction: false);
			writer = new CarrierMessageDataObjectWriter(manager, DataContext.BookingRequest, document);

			carrierMessageData.IsRequiredSendAttachment = false;
			using (var dataObject = writer.GetDataObject(carrierMessageData))
			{
				AssertNull(dataObject.AttachedDocumentCollection);
			}

			carrierMessageData.IsRequiredSendAttachment = true;
			using (var dataObject = writer.GetDataObject(carrierMessageData))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(1, attachments.Count);

				DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachments[0], new DataObjectWriterHelper.FileAttributes()
				{
					Name = "Booking Request",
					Description = "Booking Request",
					Code = "BKG",
					IsPublished = false
				});
			}
		}

		void AssertPopulateDataObject(BooleanRegistryItem registryItem, bool enabled, string version, bool isShippingInstruction = true, bool enablePackageGrouping = false, bool isDoNotGroup = true)
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enabled))
			{
				var hasFlashPoint = true;
				var eoriNumber = "AU12345678901234567890123456789012345";
				var carrierMessageData = PrepareTestData(hasFlashPoint: hasFlashPoint, isShippingInstruction: isShippingInstruction, isGroupAndConsolidatePackingLines: enablePackageGrouping && !isDoNotGroup);

				carrierMessageData.IsToEgypt = true;
				carrierMessageData.AcidNumber = "1234567890123456789\r\n1122334455667788990";

				carrierMessageData.IsBrazilExport = true;
				carrierMessageData.RUCNumber = "RUC111,RUC222";

				carrierMessageData.IsShowICS2 = enablePackageGrouping;
				carrierMessageData.ICS2DeclarantEORINumber = (enablePackageGrouping && isDoNotGroup) ? eoriNumber : "";

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new CarrierMessageDataObjectWriter(manager, isShippingInstruction ? DataContext.ShippingInstruction : DataContext.BookingRequest);

				var flashPoint = enablePackageGrouping ? "<FlashPoint>10</FlashPoint>" : "<FlashPoint>10</FlashPoint>".PadLeft(43, ' ');
				var dataObject = writer.GetDataObject(carrierMessageData);
				var expectedXml = GetExpectedXml(isShippingInstruction, enablePackageGrouping, isDoNotGroup, version, flashPoint);

				AssertUXml(dataObject, expectedXml);

				hasFlashPoint = false;
				carrierMessageData = PrepareTestData(hasFlashPoint: hasFlashPoint, isShippingInstruction: isShippingInstruction, isGroupAndConsolidatePackingLines: enablePackageGrouping && !isDoNotGroup);

				carrierMessageData.IsToEgypt = true;
				carrierMessageData.AcidNumber = "1234567890123456789\r\n1122334455667788990";

				carrierMessageData.IsBrazilExport = true;
				carrierMessageData.RUCNumber = "RUC111,RUC222";

				carrierMessageData.IsShowICS2 = enablePackageGrouping;
				carrierMessageData.ICS2DeclarantEORINumber = (enablePackageGrouping && isDoNotGroup) ? eoriNumber : "";

				writer = new CarrierMessageDataObjectWriter(manager, isShippingInstruction ? DataContext.ShippingInstruction : DataContext.BookingRequest);

				flashPoint = null;
				dataObject = writer.GetDataObject(carrierMessageData);
				expectedXml = GetExpectedXml(isShippingInstruction, enablePackageGrouping, isDoNotGroup, version, flashPoint);

				AssertUXml(dataObject, expectedXml);
			}
		}

		string GetExpectedXml(bool isShippingInstruction, bool enablePackageGrouping, bool isDoNotGroup, string version, string flashPoint)
		{
			if (enablePackageGrouping)
			{
				if (isDoNotGroup)
				{
					return isShippingInstruction ? GetExpectedXmlSI_DoNotGroup(version, flashPoint) : GetExpectedXmlBR_DoNotGroup(version, flashPoint);
				}
				else
				{
					return isShippingInstruction ? GetExpectedXmlSI_IsGroupAndConsolidatePackingLines(version, flashPoint) : GetExpectedXmlBR_IsGroupAndConsolidatePackingLines(version, flashPoint);
				}
			}
			else
			{
				return isShippingInstruction ? GetExpectedXmlSI(version, flashPoint) : GetExpectedXmlBR(version, flashPoint);
			}
		}

		public void TestPopulateDataObject_ForChina()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.GS_WorkingLanguage = "ZH-CN";

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = PrepareTestData();

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var dataObject = writer.GetDataObject(carrierMessageData);

				AssertNotInUXml(dataObject, "货物操作说明");
			}
		}

		public void TestNoReleaseTypeInfoForBookingRequests()
		{
			var carrierMessageData = PrepareTestData(isShippingInstruction: false);
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.BookingRequest);

			var dataObject = writer.GetDataObject(carrierMessageData);
			AssertNull(dataObject.ReleaseType);
			AssertNull(dataObject.NoOriginalBills);
			AssertNull(dataObject.NoCopyBills);
		}

		public void TestNoOriginalBillsForShippingInstruction()
		{
			var shippingInstructionReleaseTypes = new ShippingInstructionReleaseTypes();
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			var carrierMessageData = PrepareTestData();
			carrierMessageData.ReleaseType = new CodeDescription(shippingInstructionReleaseTypes)
			{
				Code = ShippingInstructionReleaseTypes.Codes.SeaWaybill,
				Description = ShippingInstructionReleaseTypes.Descriptions.SeaWaybill
			};

			var dataObject = writer.GetDataObject(carrierMessageData);
			AssertNull(dataObject.NoOriginalBills);

			carrierMessageData.ReleaseType = new CodeDescription(shippingInstructionReleaseTypes)
			{
				Code = ShippingInstructionReleaseTypes.Codes.BOLOriginal,
				Description = ShippingInstructionReleaseTypes.Descriptions.BOLOriginal
			};

			carrierMessageData.NumberOfOriginals = 12;
			dataObject = writer.GetDataObject(carrierMessageData);
			AssertEquals((byte)12, dataObject.NoOriginalBills);

			carrierMessageData.ReleaseType = null;

			dataObject = writer.GetDataObject(carrierMessageData);
			AssertNull(dataObject.NoOriginalBills);
		}

		public void TestNoCopyBillsForShippingInstruction()
		{
			var shippingInstructionReleaseTypes = new ShippingInstructionReleaseTypes();
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			var carrierMessageData = PrepareTestData();
			carrierMessageData.ReleaseType = new CodeDescription(shippingInstructionReleaseTypes)
			{
				Code = ShippingInstructionReleaseTypes.Codes.HouseBill,
				Description = ShippingInstructionReleaseTypes.Descriptions.HouseBill
			};

			var dataObject = writer.GetDataObject(carrierMessageData);
			AssertNull(dataObject.NoCopyBills);

			carrierMessageData.ReleaseType = new CodeDescription(shippingInstructionReleaseTypes)
			{
				Code = ShippingInstructionReleaseTypes.Codes.BOLOriginal,
				Description = ShippingInstructionReleaseTypes.Descriptions.BOLOriginal
			};

			carrierMessageData.NumberOfCopies = 12;
			dataObject = writer.GetDataObject(carrierMessageData);
			AssertEquals((byte)12, dataObject.NoCopyBills);

			carrierMessageData.ReleaseType = null;

			carrierMessageData.NumberOfCopies = 15;
			dataObject = writer.GetDataObject(carrierMessageData);
			AssertEquals((byte)15, dataObject.NoCopyBills);
		}

		public void TestPopulateBOLCurrency()
		{
			var carrierMessageData = PrepareTestData();
			carrierMessageData.GoodsValue.Currency.Code = "usd";

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			var dataObject = writer.GetDataObject(carrierMessageData);
			AssertEquals("USD", dataObject.GoodsValueCurrency.Code);
		}

		public void TestEstCargoPickupDateTime()
		{
			var carrierMessageData = PrepareTestData(isShippingInstruction: false);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.BookingRequest);

			var dataObject = writer.GetDataObject(carrierMessageData);
			AssertEquals(false, dataObject.DateCollection.Any(date => date.Type == UniversalDataBuss.DataObjects.Universal.DateType.Pickup && date.IsEstimate.HasValue && date.IsEstimate.Value));

			carrierMessageData.IsCoload = true;
			carrierMessageData.IsNVO = false;
			carrierMessageData.ContainerMode.Code = Core.Constants.ContainerModes.LCL;
			carrierMessageData.IsDoorPickup = true;
			carrierMessageData.EstCargoPickupDateTime = new ZDateTime(2020, 4, 27, 14, 15, 0);

			dataObject = writer.GetDataObject(carrierMessageData);
			var foundDate = dataObject.DateCollection.FirstOrDefault(date => date.Type == UniversalDataBuss.DataObjects.Universal.DateType.Pickup && date.IsEstimate.HasValue && date.IsEstimate.Value);
			AssertNotNull(foundDate);
			AssertEquals(carrierMessageData.EstCargoPickupDateTime, foundDate.Value);

			carrierMessageData.IsCoload = false;
			carrierMessageData.IsNVO = true;

			dataObject = writer.GetDataObject(carrierMessageData);
			foundDate = dataObject.DateCollection.FirstOrDefault(date => date.Type == UniversalDataBuss.DataObjects.Universal.DateType.Pickup && date.IsEstimate.HasValue && date.IsEstimate.Value);
			AssertNotNull(foundDate);
			AssertEquals(carrierMessageData.EstCargoPickupDateTime, foundDate.Value);
		}

		public void TestTransportBookingDeliveryInfosForBookingRequest()
		{
			var carrierMessageData = PrepareTestData(true, isShippingInstruction: false);
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.BookingRequest);

			var dataObject = writer.GetDataObject(carrierMessageData);
			var instructions = dataObject.InstructionCollection;

			AssertEquals(3, instructions.Count);
			AssertEquals("PICKUP1", instructions.ElementAt(0).Address.CompanyName);
			AssertEquals("PIC", instructions.ElementAt(0).Type.Code);
			AssertEquals(1, instructions.ElementAt(0).InstructionContainerLinkCollection.Count);
			AssertEquals(1, instructions.ElementAt(0).InstructionContainerLinkCollection.ElementAt(0).ContainerLink);

			AssertEquals("PICKUP2", instructions.ElementAt(1).Address.CompanyName);
			AssertEquals("PIC", instructions.ElementAt(1).Type.Code);
			AssertEquals(1, instructions.ElementAt(1).InstructionContainerLinkCollection.Count);
			AssertEquals(1, instructions.ElementAt(1).InstructionContainerLinkCollection.ElementAt(0).ContainerLink);

			AssertEquals("DELIVERY", instructions.ElementAt(2).Address.CompanyName);
			AssertEquals("DLV", instructions.ElementAt(2).Type.Code);
			AssertEquals(1, instructions.ElementAt(2).InstructionContainerLinkCollection.Count);
			AssertEquals(1, instructions.ElementAt(2).InstructionContainerLinkCollection.ElementAt(0).ContainerLink);
		}

		public void TestPopulateAddInfos_BOLDocumentationProvider()
		{
			var carrierMessageData = PrepareTestData(true, isShippingInstruction: true);
			carrierMessageData.EBLProvider.Code = ZString.Empty;

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			AssertNullOrEmpty(writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.ShippingLineMessagingRequirement.AddInfoKey.BillOfLadingProvider).Value);

			carrierMessageData.ElectronicBillOfLadingProviderMandatory = false;
			carrierMessageData.EBLProvider.Code = EBLProviderConstants.Codes.NotListed;
			AssertNull(writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.ShippingLineMessagingRequirement.AddInfoKey.BillOfLadingProvider));

			carrierMessageData.ElectronicBillOfLadingProviderMandatory = true;
			AssertNullOrEmpty(EBLProviderConstants.Codes.NotListed, writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.ShippingLineMessagingRequirement.AddInfoKey.BillOfLadingProvider).Value);

			carrierMessageData.EBLProvider.Code = EBLProviderConstants.Codes.CargoX;
			AssertEquals(EBLProviderConstants.Codes.CargoX, writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.ShippingLineMessagingRequirement.AddInfoKey.BillOfLadingProvider).Value);
		}

		public void TestPopulateAddInfos_GroupingMethod()
		{
			var carrierMessageData = new CarrierMessageData
			(
				"ForwardingConsol",
				"C00001000",
				"BookingRequest"
			);
			carrierMessageData.PackageGrouping = new CodeDescription(FreightCodePairLists.PackageGroupingList())
			{
				Code = Core.Constants.PackageGrouping.Codes.DoNotGroup
			};
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.BookingRequest);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNull(writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.AddinfoTypes.GroupingMethod));

				carrierMessageData.PackageGrouping.Code = Core.Constants.PackageGrouping.Codes.GroupByShipment;
				AssertNull(writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.AddinfoTypes.GroupingMethod));
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(Core.Constants.PackageGrouping.Codes.GroupByShipment, writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.AddinfoTypes.GroupingMethod).Value);

				carrierMessageData.PackageGrouping.Code = Core.Constants.PackageGrouping.Codes.GroupByPackLine;
				AssertEquals(Core.Constants.PackageGrouping.Codes.GroupByPackLine, writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.AddinfoTypes.GroupingMethod).Value);

				carrierMessageData.PackageGrouping.Code = Core.Constants.PackageGrouping.Codes.DoNotGroup;
				AssertEquals(Core.Constants.PackageGrouping.Codes.DoNotGroup, writer.GetDataObject(carrierMessageData).AddInfoCollection.FirstOrDefault(x => x.Key?.ToString() == DocDataConstants.AddinfoTypes.GroupingMethod).Value);
			}
		}

		public void TestPopulateCUSCodes()
		{
			var carrierMessageData = PrepareTestData(true, isShippingInstruction: true, isGroupAndConsolidatePackingLines: false);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = writer.GetDataObject(carrierMessageData);

				foreach (var shipment in data.SubShipmentCollection)
				{
					foreach (var packingLine in shipment.PackingLineCollection)
					{
						AssertNotNull(packingLine.ClassificationCollection.FirstOrDefault(x => x.Code.Value == "1" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
						AssertEquals(1, packingLine.ClassificationCollection.Count(x => x.Code.Value == "2" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
						AssertNotNull(packingLine.ClassificationCollection.FirstOrDefault(x => x.Code.Value == "5" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
					}
				}
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = writer.GetDataObject(carrierMessageData);

				foreach (var shipment in data.SubShipmentCollection)
				{
					foreach (var packingLine in shipment.PackingLineCollection)
					{
						AssertNotNull(packingLine.ClassificationCollection.FirstOrDefault(x => x.Code.Value == "1" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
						AssertEquals(1, packingLine.ClassificationCollection.Count(x => x.Code.Value == "2" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
						AssertNotNull(packingLine.ClassificationCollection.FirstOrDefault(x => x.Code.Value == "5" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
					}
				}
			}
		}

		public void TestPopulateCUSCodes_GroupingMethod()
		{
			var carrierMessageData = PrepareTestData(true, isShippingInstruction: true, isGroupAndConsolidatePackingLines: true);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = writer.GetDataObject(carrierMessageData);

				foreach (var shipment in data.SubShipmentCollection)
				{
					foreach (var packingLine in shipment.PackingLineCollection)
					{
						AssertNotNull(packingLine.ClassificationCollection.FirstOrDefault(x => x.Code.Value == "1" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
						AssertEquals(1, packingLine.ClassificationCollection.Count(x => x.Code.Value == "2" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
						AssertNotNull(packingLine.ClassificationCollection.FirstOrDefault(x => x.Code.Value == "5" && x.Type.Description.Value == FreightConstants.Classification.Description.ECICS && x.Type.Code.Value == FreightConstants.Classification.Codes.ECICS));
					}
				}
			}
		}

		public void TestPopulateCTNNumber()
		{
			var carrierMessageData = PrepareTestData(true, isShippingInstruction: true, isGroupAndConsolidatePackingLines: false, hasCTNNumber: true);
			carrierMessageData.IsCanadaExport = true;

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = writer.GetDataObject(carrierMessageData);

				foreach (var shipment in data.SubShipmentCollection)
				{
					foreach (var packingLine in shipment.PackingLineCollection)
					{
						AssertNotNull(packingLine.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "CTN"));
					}
				}
			}
		}

		public void TestPopulateCTNNumber_GroupingMethod()
		{
			var carrierMessageData = PrepareTestData(true, isShippingInstruction: true, isGroupAndConsolidatePackingLines: true, hasCTNNumber: true);
			carrierMessageData.IsCanadaExport = true;

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = writer.GetDataObject(carrierMessageData);

				foreach (var shipment in data.SubShipmentCollection)
				{
					foreach (var packingLine in shipment.PackingLineCollection)
					{
						AssertNotNull(packingLine.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "CTN"));
					}
				}
			}
		}

		public void TestExistsPaymentInstructionWhichPaymentMethodIsELS()
		{
			using (Registry.ForwardingConfigurationRegistry.Instance.ShowCountryStateBookingRequestShippingInstruction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var carrierMessageData = PrepareTestData();
				carrierMessageData.OptionalChargeBasicFreight = new OptionalCharge { IsPayableElsewhere = ZBool.True };

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var dataObject = writer.GetDataObject(carrierMessageData);

				Assert(dataObject.PaymentHandlingInstructionCollection.Exists(instruction => instruction.PaymentMethod.Code.Equals(DocDataConstants.Charges.Codes.PayableElsewhere)));
				AssertEquals(dataObject.PaymentMethod.Code, DocDataConstants.Charges.Codes.PayableElsewhere);
				AssertEquals(dataObject.PaymentMethod.Description, DocDataConstants.Charges.Descriptions.PayableElsewhere);
			}
		}

		public void TestSplitHarmonisedCodeIntoClassificationCollectionForDoNotGroup()
		{
			var carrierMessageData = PrepareTestData();
			carrierMessageData.PackageGrouping = new CodeDescription(FreightCodePairLists.PackageGroupingList())
			{
				Code = Core.Constants.PackageGrouping.Codes.DoNotGroup
			};
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.BookingRequest);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var data = writer.GetDataObject(carrierMessageData);

				foreach (var shipment in data.SubShipmentCollection)
				{
					foreach (var packingLine in shipment.PackingLineCollection)
					{
						AssertEquals("HC12345", packingLine.HarmonisedCode);
						AssertNull(packingLine.ClassificationCollection.FirstOrDefault(x => x.Code.Value == "HC12345" && x.Country.Code.Value.IsEmpty));
					}
				}
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = writer.GetDataObject(carrierMessageData);

				foreach (var shipment in data.SubShipmentCollection)
				{
					foreach (var packingLine in shipment.PackingLineCollection)
					{
						AssertEquals("HC12345", packingLine.HarmonisedCode);
						AssertNotNull(packingLine.ClassificationCollection.FirstOrDefault(x => x.Code.Value == "HC12345" && x.Country.Code.Value.IsEmpty));
					}
				}
			}
		}

		public void TestPopulateDataObject_USCanadaManifestSelfFilerID()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.BookingRequest);

			var carrierMessageData = PrepareTestData(isUSCanadaManifestSelfFilerID: true, isUSCanadaManifestSelfFilerIDValue: "US manifest ID");
			var dataObject = writer.GetDataObject(carrierMessageData);
			var noteUSCanadaManifestSelfFilerID = dataObject.NoteCollection.FirstOrDefault(x => x.Description?.ToString() == DocDataConstants.NoteTypes.USCanadaManifestSelfFilerID);

			AssertNotNull(noteUSCanadaManifestSelfFilerID);
			AssertEquals(noteUSCanadaManifestSelfFilerID.NoteText, "US manifest ID");

			carrierMessageData = PrepareTestData(isUSCanadaManifestSelfFilerID: true, isUSCanadaManifestSelfFilerIDValue: string.Empty);
			dataObject = writer.GetDataObject(carrierMessageData);
			noteUSCanadaManifestSelfFilerID = dataObject.NoteCollection.FirstOrDefault(x => x.Description?.ToString() == DocDataConstants.NoteTypes.USCanadaManifestSelfFilerID);

			AssertNotNull(noteUSCanadaManifestSelfFilerID);
			AssertEquals(noteUSCanadaManifestSelfFilerID.NoteText, string.Empty);

			carrierMessageData = PrepareTestData(isUSCanadaManifestSelfFilerID: false, isUSCanadaManifestSelfFilerIDValue: "US manifest ID");
			dataObject = writer.GetDataObject(carrierMessageData);
			noteUSCanadaManifestSelfFilerID = dataObject.NoteCollection.FirstOrDefault(x => x.Description?.ToString() == DocDataConstants.NoteTypes.USCanadaManifestSelfFilerID);

			AssertNotNull(noteUSCanadaManifestSelfFilerID);
			AssertEquals(noteUSCanadaManifestSelfFilerID.NoteText, "US manifest ID");

			carrierMessageData = PrepareTestData(isUSCanadaManifestSelfFilerID: false, isUSCanadaManifestSelfFilerIDValue: string.Empty);
			dataObject = writer.GetDataObject(carrierMessageData);
			noteUSCanadaManifestSelfFilerID = dataObject.NoteCollection.FirstOrDefault(x => x.Description?.ToString() == DocDataConstants.NoteTypes.USCanadaManifestSelfFilerID);

			AssertNull(noteUSCanadaManifestSelfFilerID);
		}

		public void TestPopulateDataObject_ExportStatementFieldsWhenITNIsEmpty()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			var carrierMessageData = PrepareTestData(isShippingInstruction: true);

			var subShipment3 = carrierMessageData.Shipments.FirstOrDefault(s => s.ShipmentID == "S00000012");
			subShipment3.ITNNumber = ZString.Empty;
			subShipment3.ExportStatement = "AESPOST";
			subShipment3.ExportStatementField1Type = SEDStatementFieldType.Codes.AgentEIN;
			subShipment3.ExportStatementField1Code = "99-12345555";
			subShipment3.ExportStatementField2Type = SEDStatementFieldType.Codes.FilerID;
			subShipment3.ExportStatementField2Code = "12346666";

			var dataObject = writer.GetDataObject(carrierMessageData);

			var subShipment = dataObject.SubShipmentCollection.FirstOrDefault(s => s.WayBillNumber.GetValueOrDefault() == "HS00000012");
			AssertNotNull(subShipment);
			var packLine = subShipment.PackingLineCollection.FirstOrDefault(p => p.ContainerNumber.GetValueOrDefault() == "BBB");
			AssertNotNull(packLine);
			var itnAddInfo = packLine.AddInfoCollection.FirstOrDefault(a => a.Key.GetValueOrDefault() == "ITN");
			AssertNotNull(itnAddInfo);

			AssertEquals("AESPOST, 99-12345555, 12346666", itnAddInfo.Value);
		}

		public void TestPopulateDataObject_ExcludeIsFreightAsAgreedForBookingRequest()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);

			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);
			var carrierMessageData = PrepareTestData(isShippingInstruction: true);
			carrierMessageData.IsFreightAsAgreed = true;
			var dataObject = writer.GetDataObject(carrierMessageData);
			var isContainsFAA = dataObject.BillOfLadingClauseCollection.Any(b => b.Type.Code.ToString() == DocDataConstants.BillOfLadingTypes.Codes.AsAgreed);
			AssertEquals(true, isContainsFAA);

			writer = new CarrierMessageDataObjectWriter(manager, DataContext.BookingRequest);
			carrierMessageData = PrepareTestData(isShippingInstruction: false);
			carrierMessageData.IsFreightAsAgreed = true;
			dataObject = writer.GetDataObject(carrierMessageData);
			isContainsFAA = dataObject.BillOfLadingClauseCollection.Any(b => b.Type.Code.ToString() == DocDataConstants.BillOfLadingTypes.Codes.AsAgreed);
			AssertEquals(false, isContainsFAA);
		}

		public void TestPopulateHBLPaymentTypeForShippingInstruction()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shippingInstructionReleaseTypes = new ShippingInstructionReleaseTypes();
				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var carrierMessageData = PrepareTestData(isShippingInstruction: true, isGroupAndConsolidatePackingLines: true);
				carrierMessageData.ReleaseType = new CodeDescription(shippingInstructionReleaseTypes)
				{
					Code = ShippingInstructionReleaseTypes.Codes.HouseBill,
					Description = ShippingInstructionReleaseTypes.Descriptions.HouseBill
				};

				var dataObject = writer.GetDataObject(carrierMessageData);
				var shipment = dataObject.SubShipmentCollection.FirstOrDefault(shipment => shipment.WayBillNumber.ToString() == "HS00000013" && shipment.PaymentHandlingInstructionCollection != null && shipment.PaymentHandlingInstructionCollection.Any(payment => payment.Category.Code.ToString() == "HPT"));
				AssertNotNull(shipment);

				var paymentHandlingInstruction = shipment.PaymentHandlingInstructionCollection.FirstOrDefault();
				AssertEquals("A", paymentHandlingInstruction.PaymentMethod.Code.Value);
				AssertEquals("Payment in cash", paymentHandlingInstruction.PaymentMethod.Description.Value);

				shipment = dataObject.SubShipmentCollection.FirstOrDefault(shipment => shipment.WayBillNumber.ToString() == "HS0000009" && shipment.PaymentHandlingInstructionCollection != null && shipment.PaymentHandlingInstructionCollection.Any(payment => payment.Category.Code.ToString() == "HPT"));
				AssertNotNull(shipment);

				paymentHandlingInstruction = shipment.PaymentHandlingInstructionCollection.FirstOrDefault();
				AssertEquals("B", paymentHandlingInstruction.PaymentMethod.Code.Value);
			}
		}

		public void TestPopulateBuyerDocumentaryAddress()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);
			var carrierMessageData = PrepareTestData(isShippingInstruction: true);

			var dataObject = writer.GetDataObject(carrierMessageData);
			foreach (var subShipment in dataObject.SubShipmentCollection)
			{
				AssertEquals(false, subShipment.OrganizationAddressCollection.Any(b => b.AddressType.ToString() == "BuyerDocumentaryAddress"));
			}

			foreach (var shipment in carrierMessageData.Shipments)
			{
				shipment.Buyer = CreateAddress("Buyer");
			}

			dataObject = writer.GetDataObject(carrierMessageData);
			foreach (var subShipment in dataObject.SubShipmentCollection)
			{
				AssertEquals(true, subShipment.OrganizationAddressCollection.Any(b => b.AddressType.ToString() == "BuyerDocumentaryAddress"));
			}
		}

		public void TestPopulateBuyerDocumentaryAddress_GroupingMethod()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);
			var carrierMessageData = PrepareTestData(isShippingInstruction: true, isGroupAndConsolidatePackingLines: true);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataObject = writer.GetDataObject(carrierMessageData);

				foreach (var subShipment in dataObject.SubShipmentCollection)
				{
					AssertEquals(false, subShipment.OrganizationAddressCollection.Any(b => b.AddressType.ToString() == "BuyerDocumentaryAddress"));
				}

				foreach (var shipment in carrierMessageData.Shipments)
				{
					shipment.Buyer = CreateAddress("Buyer");
				}

				dataObject = writer.GetDataObject(carrierMessageData);

				foreach (var subShipment in dataObject.SubShipmentCollection)
				{
					AssertEquals(true, subShipment.OrganizationAddressCollection.Any(b => b.AddressType.ToString() == "BuyerDocumentaryAddress"));
				}
			}
		}

		#region Sort PackingLines

		public void TestSortPackingLines_DoNotGroup()
		{
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

			var carrierMessageData = PrepareTestData(isShippingInstruction: true);
			PrepareDoNotGroupData(carrierMessageData);

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSortPackingLines_DoNotGroup();
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSortPackingLines_DoNotGroup();
			}

			#region Helpers

			void AssertSortPackingLines_DoNotGroup()
			{
				var dataObject = writer.GetDataObject(carrierMessageData);

				var subShipments = dataObject.SubShipmentCollection.ToArray();
				AssertEquals(3, subShipments.Length);
				AssertPackLinesSequence(subShipments[0], "SHIPMENT1", ["Test001", "Test002", "Test003"]);
				AssertPackLinesSequence(subShipments[1], "SHIPMENT2", ["Test004", "Test005", "Test006"]);
				AssertPackLinesSequence(subShipments[2], "SHIPMENT3", ["Test007", "Test008", "Test009"]);
			}

			void AssertPackLinesSequence(UniversalDataBuss.DataObjects.Universal.Shipment subShipment, string shipmentID, IEnumerable<string> packLineIds)
			{
				var subShipmentKey = subShipment.DataContext.GetMatchingDataSource(DataContextType.ForwardingShipment)?.Key;
				AssertNotNull(subShipmentKey);
				AssertEquals(shipmentID, subShipmentKey);

				Assert(subShipment.PackingLineCollection.Select(p => p.PackingLineID.ToString()).SequenceEqual(packLineIds));
			}

			void PrepareDoNotGroupData(CarrierMessageData data)
			{
				var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
				var context = new CommonContext(factory);

				var container3 = CreateContainer(context, "CCC");
				var container2 = CreateContainer(context, "BBB");
				var container1 = CreateContainer(context, "AAA");

				var packline9 = CreatePackingLine("SHIPMENT3", containerNumber: "CCC", packLineId: "Test009");
				var packline8 = CreatePackingLine("SHIPMENT3", containerNumber: "BBB", packLineId: "Test008");
				var packline7 = CreatePackingLine("SHIPMENT3", containerNumber: "AAA", packLineId: "Test007");
				var packline6 = CreatePackingLine("SHIPMENT2", containerNumber: "CCC", packLineId: "Test006");
				var packline5 = CreatePackingLine("SHIPMENT2", containerNumber: "BBB", packLineId: "Test005");
				var packline4 = CreatePackingLine("SHIPMENT2", containerNumber: "AAA", packLineId: "Test004");
				var packline3 = CreatePackingLine("SHIPMENT1", containerNumber: "CCC", packLineId: "Test003");
				var packline2 = CreatePackingLine("SHIPMENT1", containerNumber: "CCC", packLineId: "Test002");
				var packline1 = CreatePackingLine("SHIPMENT1", containerNumber: "AAA", packLineId: "Test001");

				container1.PackingLines =
				[
					packline7, packline4, packline1
				];

				container2.PackingLines =
				[
					packline8, packline5
				];

				container3.PackingLines =
				[
					packline9, packline6, packline3, packline2
				];

				var shipment3 = CreateShipments("SHIPMENT3", "STD", [packline9, packline8, packline7]);
				var shipment2 = CreateShipments("SHIPMENT2", "STD", [packline6, packline5, packline4]);
				var shipment1 = CreateShipments("SHIPMENT1", "STD", [packline3, packline2, packline1]);

				data.Containers = [container3, container2, container1];
				data.Shipments = [shipment3, shipment2, shipment1];
			}

			#endregion
		}

		public void TestSortPackingLines_GroupByShipment()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new CarrierMessageDataObjectWriter(manager, DataContext.ShippingInstruction);

				var carrierMessageData = PrepareTestData(isShippingInstruction: true, isGroupAndConsolidatePackingLines: true);
				PrepareGroupByShipmentData(carrierMessageData);

				var dataObject = writer.GetDataObject(carrierMessageData);

				var subShipments = dataObject.SubShipmentCollection.ToArray();
				AssertEquals(3, subShipments.Length);
				AssertPackLinesSequence(subShipments[0], "SHIPMENT1", ["AAA", "CCC"]);
				AssertPackLinesSequence(subShipments[1], "SHIPMENT2", ["AAA", "BBB", "CCC"]);
				AssertPackLinesSequence(subShipments[2], "SHIPMENT3", ["AAA", "BBB", "CCC"]);

				#region Helpers

				void AssertPackLinesSequence(UniversalDataBuss.DataObjects.Universal.Shipment subShipment, string shipmentID, IEnumerable<string> containerNumbers)
				{
					var subShipmentKey = subShipment.DataContext.GetMatchingDataSource(DataContextType.ForwardingShipment)?.Key;
					AssertNotNull(subShipmentKey);
					AssertEquals(shipmentID, subShipmentKey);

					AssertEquals(1, subShipment.PackingLineCollection.Count);
					Assert(subShipment.PackingLineCollection[0].PackingLineCollection.Select(p => p.ContainerNumber.ToString()).SequenceEqual(containerNumbers));
				}

				void PrepareGroupByShipmentData(CarrierMessageData data)
				{
					var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
					var context = new CommonContext(factory);

					var container3 = CreateContainer(context, "CCC");
					var container2 = CreateContainer(context, "BBB");
					var container1 = CreateContainer(context, "AAA");

					var groupedPackingLine3 = CreatePackingLine("SHIPMENT3");
					var groupedPackingLine2 = CreatePackingLine("SHIPMENT2");
					var groupedPackingLine1 = CreatePackingLine("SHIPMENT1");

					var packline9 = CreatePackingLine("SHIPMENT3", containerNumber: "CCC", packLineId: "Test009");
					var packline8 = CreatePackingLine("SHIPMENT3", containerNumber: "BBB", packLineId: "Test008");
					var packline7 = CreatePackingLine("SHIPMENT3", containerNumber: "AAA", packLineId: "Test007");
					groupedPackingLine3.PackingLines = [packline9, packline8, packline7];

					var packline6 = CreatePackingLine("SHIPMENT2", containerNumber: "CCC", packLineId: "Test006");
					var packline5 = CreatePackingLine("SHIPMENT2", containerNumber: "BBB", packLineId: "Test005");
					var packline4 = CreatePackingLine("SHIPMENT2", containerNumber: "AAA", packLineId: "Test004");
					groupedPackingLine2.PackingLines = [packline6, packline5, packline4];

					var packline2 = CreatePackingLine("SHIPMENT1", containerNumber: "CCC", packLineId: "Test002");
					var packline1 = CreatePackingLine("SHIPMENT1", containerNumber: "AAA", packLineId: "Test001");
					groupedPackingLine1.PackingLines = [packline2, packline1];

					container1.PackingLines =
					[
						packline7, packline4, packline1
					];

					container2.PackingLines =
					[
						packline8, packline5
					];

					container3.PackingLines =
					[
						packline9, packline6, packline2
					];

					var shipment3 = CreateShipments("SHIPMENT3", "STD", [groupedPackingLine3]);
					var shipment2 = CreateShipments("SHIPMENT2", "STD", [groupedPackingLine2]);
					var shipment1 = CreateShipments("SHIPMENT1", "STD", [groupedPackingLine1]);

					data.Containers = [container3, container2, container1];
					data.Shipments = [shipment3, shipment2, shipment1];
				}

				#endregion
			}
		}

		#endregion

		#region PrepareTestData

		CarrierMessageData PrepareTestData(bool hasTransportBookingPickupDeliveryInfos = false, bool hasFlashPoint = true, bool isShippingInstruction = true, bool isGroupAndConsolidatePackingLines = false, bool isUSCanadaManifestSelfFilerID = false, string isUSCanadaManifestSelfFilerIDValue = "US manifest ID", bool hasCTNNumber = false)
		{
			var carrierMessageData = new CarrierMessageData
			(
				"ForwardingConsol",
				"C00001000",
				isShippingInstruction ? DataContext.ShippingInstruction : DataContext.BookingRequest
			);

			var emptyCodeDescriptionPairList = new CodeDescriptionPairList();
			carrierMessageData.BookingReference = "BKG0001";
			carrierMessageData.CoLoadBookingReference = "BKG0002";
			carrierMessageData.MasterBillNumber = "bill of lading number";
			carrierMessageData.CoLoadMasterBillNumber = "coload bill of lading number";
			carrierMessageData.NumberOfOriginals = 1;
			carrierMessageData.NumberOfCopies = 2;
			carrierMessageData.IsDoorPickup = true;
			carrierMessageData.IsDoorDelivery = true;
			carrierMessageData.EarliestDepartureDate = new ZDateTime(2019, 10, 20);
			carrierMessageData.LatestDeliveryDate = new ZDateTime(2019, 10, 21);
			carrierMessageData.DateOfIssue = new ZDateTime(2019, 10, 25);
			carrierMessageData.PortOfFirstArrivalDate = new ZDateTime(2019, 10, 26);
			carrierMessageData.FirstForeignArrivalDate = new ZDateTime(2019, 10, 27);
			carrierMessageData.LastForeignDepartureDate = new ZDateTime(2019, 10, 28);
			carrierMessageData.PaymentMethod = new CodeDescription(emptyCodeDescriptionPairList) { Code = "PPD" };

			var eBLProviderList = new CodeDescriptionPairList();
			eBLProviderList.AddPair(EBLProviderConstants.Codes.CargoX, EBLProviderConstants.Codes.CargoX);

			carrierMessageData.EBLProvider = new CodeDescription(eBLProviderList) { Code = EBLProviderConstants.Codes.CargoX };
			carrierMessageData.ElectronicBillOfLadingProviderMandatory = true;
			carrierMessageData.IsHazardous = true;
			carrierMessageData.IsOutOfGauge = true;
			carrierMessageData.IsUSCanadaManifestSelfFilerIDSupported = isUSCanadaManifestSelfFilerID;

			var preAllocatedUNDGs = new List<DGRestriction>();
			var dangerousGood1 = new DGRestriction();
			dangerousGood1.Standard = "IMO";
			dangerousGood1.EmergencyScheduleFire = new CodeDescription(new EmergencyScheduleFireCodes()) { Code = "F-B" };
			dangerousGood1.EmergencyScheduleSpillage = new CodeDescription(new EmergencyScheduleSpillageCodes()) { Code = "S-Y" };
			dangerousGood1.ExceptedQuantityCode = string.Empty;
			dangerousGood1.FlashPoint = string.Empty;
			dangerousGood1.IMOClass = "1.1D";
			dangerousGood1.MarinePollutantCode = string.Empty;
			dangerousGood1.PackedInLimitedQuantity = false;
			dangerousGood1.PackingGroup = string.Empty;
			dangerousGood1.ProperShippingName = "AMMONIUM PICRATE";
			dangerousGood1.State = "E";
			dangerousGood1.SubLabel1 = string.Empty;
			dangerousGood1.SubLabel2 = string.Empty;
			dangerousGood1.Code = "0004a";
			dangerousGood1.Unno = "0004";
			dangerousGood1.Variant = "a";
			preAllocatedUNDGs.Add(dangerousGood1);

			var dangerousGood2 = new DGRestriction();
			dangerousGood2.Standard = "IMO";
			dangerousGood2.EmergencyScheduleFire = new CodeDescription(new EmergencyScheduleFireCodes()) { Code = "F-E" };
			dangerousGood2.EmergencyScheduleSpillage = new CodeDescription(new EmergencyScheduleSpillageCodes()) { Code = "S-C" };
			dangerousGood2.ExceptedQuantityCode = "E0";
			dangerousGood2.FlashPoint = "25 cc";
			dangerousGood2.IMOClass = "6.1";
			dangerousGood2.MarinePollutantCode = "Y";
			dangerousGood2.PackedInLimitedQuantity = false;
			dangerousGood2.PackingGroup = "I";
			dangerousGood2.ProperShippingName = "CHLOROACETONE, STABILIZED";
			dangerousGood2.State = "L";
			dangerousGood2.SubLabel1 = "3";
			dangerousGood2.SubLabel2 = "8";
			dangerousGood2.Code = "1695";
			dangerousGood2.Unno = "1695";
			dangerousGood2.Variant = "";
			preAllocatedUNDGs.Add(dangerousGood2);

			carrierMessageData.PreallocatedUNDGCollection = preAllocatedUNDGs;

			carrierMessageData.ContainerMode = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "FCL",
				Description = "Full Container Load"
			};
			carrierMessageData.AgentType = new DummyCodeDescription
			{
				Code = "AGT",
				Description = "Agent"
			};
			carrierMessageData.ReleaseType = new CodeDescription(new ShippingInstructionReleaseTypes())
			{
				Code = ShippingInstructionReleaseTypes.Codes.BOLOriginal,
				Description = ShippingInstructionReleaseTypes.Descriptions.BOLOriginal
			};
			carrierMessageData.GoodsValue = new DocDataObjects.Money()
			{
				Amount = 133.66m
			};
			carrierMessageData.GoodsValue.Currency = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "USD"
			};

			carrierMessageData.TransportMode = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "SEA",
				Description = "Sea"
			};

			carrierMessageData.HIRReference = CreateRegistrationNumber("HIR", "eHub Interchange Reference", "HIR123", "AU");

			carrierMessageData.OptionalChargeBasicFreight = new OptionalCharge { IsPrepaid = ZBool.True };
			carrierMessageData.OptionalChargeDestinationHaulage = new OptionalCharge { IsPrepaid = ZBool.True };
			carrierMessageData.OptionalChargeDestinationPort = new OptionalCharge { IsPrepaid = ZBool.True };
			carrierMessageData.OptionalChargeOriginHaulage = new OptionalCharge { IsCollect = ZBool.True };
			carrierMessageData.OptionalChargeOriginPort = new OptionalCharge { IsCollect = ZBool.True };

			PopulatePorts(carrierMessageData);
			PopulateTransports(carrierMessageData);
			PopulateOrganizations(carrierMessageData, isShippingInstruction);
			PopulateNotes(carrierMessageData, isUSCanadaManifestSelfFilerIDValue);
			PopulateBillOfLadingClauses(carrierMessageData);
			PopulateAdditionalReferenceNumbers(carrierMessageData);

			if (isGroupAndConsolidatePackingLines)
			{
				carrierMessageData.PackageGrouping = new CodeDescription(FreightCodePairLists.PackageGroupingList())
				{
					Code = Core.Constants.PackageGrouping.Codes.GroupByShipment
				};

				PopulateContainersAndGroupedAndConsolidatedPackingLines(carrierMessageData, hasFlashPoint, hasTransportBookingPickupDeliveryInfos, hasCTNNumber);
			}
			else
			{
				carrierMessageData.PackageGrouping = new CodeDescription(FreightCodePairLists.PackageGroupingList())
				{
					Code = Core.Constants.PackageGrouping.Codes.DoNotGroup
				};

				PopulateContainersAndPackingLines(carrierMessageData, hasFlashPoint, hasTransportBookingPickupDeliveryInfos, hasCTNNumber);
			}

			return carrierMessageData;
		}

		RegistrationNumber CreateRegistrationNumber(string type, string description, string value, string country)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = country
				},
				Type = new DummyCodeDescription
				{
					Code = type,
					Description = description
				},
				Value = value
			};
		}

		void PopulatePorts(CarrierMessageData carrierMessageData)
		{
			carrierMessageData.PortOfLoading = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};
			carrierMessageData.PortOfDischarge = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "SGSIN",
				Name = "Singapore"
			};

			carrierMessageData.Origin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			carrierMessageData.Destination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "CNSHA",
				Name = "Shanghai"
			};

			carrierMessageData.PlaceOfIssue = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUMEL",
				Name = "Melbourne"
			};
			carrierMessageData.PlaceOfReceipt = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};
			carrierMessageData.PlaceOfDelivery = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUPER",
				Name = "Perth"
			};
			carrierMessageData.CarrierBookingOffice = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUAUB",
				Name = "Auburn"
			};
			carrierMessageData.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};
			carrierMessageData.FreightPayableAt = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "BEANR",
				Name = "Antwerp"
			};
		}

		void PopulateTransports(CarrierMessageData carrierMessageData)
		{
			var transport = Factory.New<Freight.Business.Transport>();
			transport.ParentType = typeof(ForwardingConsol);
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_AdditionalTransportMode = Core.Constants.TransportModes.Road;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_VoyageFlight = "AAAA";
			transport.JW_ETD = new ZDateTime(2018, 6, 10);
			transport.JW_ETA = new ZDateTime(2018, 12, 1);
			transport.JW_ATD = new ZDateTime(2018, 6, 10);
			transport.JW_ATA = new ZDateTime(2018, 7, 10);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Titanic";
			vessel.RV_LloydsNumber = "12345";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_VoyageFlight = "1234567";
			transport.JW_JX = voyage.Sailings[0].PK;
			transport.Sailing.JX_DepotReceivalCommences = new ZDateTime(2019, 8, 2);
			transport.Sailing.JX_DepotCutOff = new ZDateTime(2019, 8, 1);

			var transports = Transports.Create(context, new[] { transport });
			carrierMessageData.Transports = transports;
		}

		void PopulateOrganizations(CarrierMessageData carrierMessageData, bool isShippingInstruction)
		{
			carrierMessageData.Shipper = CreateAddress("Shipper");

			var shipperTaxInfo = new List<TaxInfo>();
			shipperTaxInfo.Add(
				new TaxInfo()
				{
					Code = "GCR",
					Description = "Corporate Identification Number",
					Country = new Country(Factory, context.Countries) { Code = "BE" },
					ShortLabel = "CNO",
					LongLabel = "BELGIAN COMPANY NUMBER",
					Number = "11111"
				});
			shipperTaxInfo.Add(
				new TaxInfo()
				{
					Code = "VAT",
					Description = "Commercial Register Number",
					Country = new Country(Factory, context.Countries) { Code = "BE" },
					ShortLabel = "VAT",
					LongLabel = "CH-NUMBER",
					Number = "22222",
					RegulatingCountry = new Country(Factory, context.Countries) { Code = "EG" }
				});
			carrierMessageData.ShipperTaxInfo = shipperTaxInfo;

			carrierMessageData.Carrier = CreateAddress("Carrier");
			carrierMessageData.Consignee = CreateAddress("Consignee");

			var consigneeTaxInfo = new List<TaxInfo>();
			consigneeTaxInfo.Add(
				new TaxInfo()
				{
					Code = "GCR",
					Description = "Central Index Key",
					Country = new Country(Factory, context.Countries) { Code = "BZ" },
					ShortLabel = "CIK",
					Number = "33333"
				});

			carrierMessageData.ConsigneeTaxInfo = consigneeTaxInfo;

			carrierMessageData.Forwarder = CreateAddress("Forwarder");
			carrierMessageData.SendingForwarder = CreateAddress("SendingForwarderAddress");
			carrierMessageData.FreightPayer = CreateAddress("FreightPayer");

			carrierMessageData.NotifyParty = CreateAddress("NotifyParty");
			var notifyPartyTaxInfo = new List<TaxInfo>();
			notifyPartyTaxInfo.Add(
				new TaxInfo()
				{
					Code = "GCR",
					Description = "Business Identification Number",
					Country = new Country(Factory, context.Countries) { Code = "FI" },
					LongLabel = "BUSINESS ID",
					Number = "44444"
				});
			carrierMessageData.NotifyPartyTaxInfo = notifyPartyTaxInfo;

			carrierMessageData.NotifyParty2 = CreateAddress("NotifyParty2");
			carrierMessageData.Forwarder = CreateAddress("Forwarder");
			carrierMessageData.SendingForwarder = CreateAddress("SendingForwarderAddress");
			carrierMessageData.PickupFrom = CreateAddress("PickupFrom");
			carrierMessageData.DeliverTo = CreateAddress("DeliverTo");
			carrierMessageData.CurrentUser = CreateAddress("CurrentUser");

			if (!isShippingInstruction)
			{
				carrierMessageData.CustomsBroker = CreateAddress("CustomsBroker");
			}

			carrierMessageData.CarrierContractNumbersFormatted = "carrier contract number";
		}

		void PopulateAdditionalReferenceNumbers(CarrierMessageData carrierMessageData)
		{
			carrierMessageData.ShipperReference = "shipper reference number";
			var numberTypes = new CodeDescriptionPairList();

			ReferenceNumber CreateReferenceNumber(string type, string value)
			{
				return new ReferenceNumber
				{
					Value = value,
					Type = new CodeDescription(numberTypes)
					{
						Code = type
					}
				};
			}

			var numbers = new List<ReferenceNumber>()
			{
				CreateReferenceNumber("CQN", "carrier quote number"),
				CreateReferenceNumber("NAC", "contract named account"),
				CreateReferenceNumber("LCR", "LCR number"),
				CreateReferenceNumber("BKG", "BKG 1"),
				CreateReferenceNumber("BKG", "BKG 2"),
				CreateReferenceNumber("SLD", "SLD 1"),
				CreateReferenceNumber("SLD", "SLD 2"),
			};

			carrierMessageData.Numbers = numbers;
		}

		void PopulateNotes(CarrierMessageData carrierMessageData, string isUSCanadaManifestSelfFilerIDValue)
		{
			carrierMessageData.GoodsHandlingInstructions = "goods handling instructions";
			carrierMessageData.USCanadaManifestSelfFilerID = isUSCanadaManifestSelfFilerIDValue;
			carrierMessageData.OtherBillClauses = "other bill clauses";
			carrierMessageData.IssueFreightedBillOfLading = true;
			carrierMessageData.ForwardingInstructions = "forwarding instruction";
			carrierMessageData.SpecialInstructions = "special instruction";
			carrierMessageData.BRWoodenPackageProcessType = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "Not Applicable"
			};
		}

		void PopulateBillOfLadingClauses(CarrierMessageData carrierMessageData)
		{
			carrierMessageData.IsFreightCollect = true;
			carrierMessageData.IsFreightAsAgreed = true;
			carrierMessageData.IsReceivedForShipment = true;
			carrierMessageData.IsLadenOnBoard = true;
			carrierMessageData.IsOnBoardRail = true;
			carrierMessageData.IsOnBoardVessel = true;
			carrierMessageData.IsLadenOnBoardNamedVessel = true;
			carrierMessageData.IsShipperLoadAndCount = true;
			carrierMessageData.IsShipperLoadStowageAndCount = true;
			carrierMessageData.IsNoShipperExportDeclarationRequired = true;
		}

		void PopulateContainersAndPackingLines(CarrierMessageData carrierMessageData, bool hasFlashPoint, bool hasTransportBookingPickupDeliveryInfos = false, bool hasCTNNumber = false)
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var context = new CommonContext(factory);

			var container1 = CreateContainer(context, "AAA", "GEN", hasTransportBookingPickupDeliveryInfos);
			container1.IsNonOperativeReefer = true;
			var container2 = CreateContainer(context, "BBB");
			container2.IsNonOperativeReefer = false;

			var packline1 = CreatePackingLine("S00000010", "AAA packline 1", "AAA", 1, 10, hasFlashPoint, packLineId: "Test001");
			var packline2 = CreatePackingLine("S00000011", "AAA packline 2", "AAA", 0, 0, hasFlashPoint, packLineId: "Test002");
			var packline3 = CreatePackingLine("S00000011", "BBB packline 1", "BBB", -1, 1, hasFlashPoint, packLineId: "Test003");
			var packline4 = CreatePackingLine("S00000012", "BBB packline 2", "BBB", 0, 0, hasFlashPoint, packLineId: "Test004");
			var packline5 = CreatePackingLine("S00000013", "BBB packline 3", "BBB", 0, 0, hasFlashPoint, packLineId: "Test005");

			container1.PackingLines = new[]
			{
				packline1,
				packline2
			};

			container2.PackingLines = new[]
			{
				packline3,
				packline4
			};

			carrierMessageData.Containers = new[]
			{
				container1,
				container2
			};

			var asmShipment = CreateShipments("S0000009", "ASM", Array.Empty<PackingLine>(), "ITN010", "", "DUE010", "UCR010", "CTK010", hasCTNNumber ? "CTN010" : string.Empty);
			var subASMShipment1 = CreateShipments("S00000010", "ASM", new[] { packline1 }, "ITN020", "", "DUE020", "UCR020", "CTK020", hasCTNNumber ? "CTN020" : string.Empty);
			var subShipment2 = CreateShipments("S00000011", "STD", new[] { packline2, packline3 }, "ITN001, ITN002", "", "DUE001", "UCR001", "CTK001", hasCTNNumber ? "CTN001" : string.Empty);
			var subShipment3 = CreateShipments("S00000012", "STD", new[] { packline4 }, "ITN002, ITN003", "LOW", "DUE002", "UCR002", "CTK002", hasCTNNumber ? "CTN002" : string.Empty);
			var subShipment4 = CreateShipments("S00000013", "STD", new[] { packline5 }, "", "LOW", "", "", "", hasCTNNumber ? "CTN003" : string.Empty);

			carrierMessageData.Shipments = new[] { asmShipment, subShipment3, subShipment4 };
			subASMShipment1.Shipments = new[] { subShipment3 };
			asmShipment.Shipments = new[] { subASMShipment1, subShipment2 };
		}

		void PopulateContainersAndGroupedAndConsolidatedPackingLines(CarrierMessageData carrierMessageData, bool hasFlashPoint, bool hasTransportBookingPickupDeliveryInfos = false, bool hasCTNNumber = false)
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var context = new CommonContext(factory);

			var container1Identifier = ZGuid.NewZGuid();
			var container1 = CreateContainer(context, "AAA", "GEN", hasTransportBookingPickupDeliveryInfos, identifier: container1Identifier);
			container1.IsNonOperativeReefer = true;
			var container2Identifier = ZGuid.NewZGuid();
			var container2 = CreateContainer(context, "BBB", identifier: container2Identifier);
			container2.IsNonOperativeReefer = false;

			var groupedPackingLine1 = CreatePackingLine("S00000011, S00000012", "AAA Grouped packline 1", ZString.Empty, 2, 3, hasFlashPoint, hasDangerousGoods: false, groupITNNumber: "ITN001, ITN002", groupDUENumber: "DUE001, DUE002", groupUCRNumber: "UCR001, UCR002", groupCTKNumber: "CTK001, CTK002", groupCTNNumber: hasCTNNumber ? "CTN001, CTN002" : string.Empty);
			var consolidatedPacklineG11 = CreatePackingLine("S00000011", "AAA packline 2", "AAA", -10, 10, hasFlashPoint, identifier: container1Identifier.ToString() + "1");
			var consolidatedPacklineG12 = CreatePackingLine("S00000012", "AAA packline 2", "BBB", 2, 2, hasFlashPoint, identifier: container1Identifier.ToString() + "2");

			groupedPackingLine1.PackingLines = new[] { consolidatedPacklineG11, consolidatedPacklineG12 };

			var groupedPackingLine2 = CreatePackingLine("S00000013", "BBB Grouped packline 2", ZString.Empty, 2, 2, hasFlashPoint, hasDangerousGoods: false, groupPOFNumber: "POF003, POF004", groupCTNNumber: hasCTNNumber ? "CTN003, CTN004" : string.Empty);
			var consolidatedPacklineG21 = CreatePackingLine("S00000013", "BBB packline 2", "AAA", 1, 2, hasFlashPoint, identifier: container1Identifier.ToString() + "3");
			var consolidatedPacklineG22 = CreatePackingLine("S00000013", "BBB packline 2", "BBB", 2, 3, hasFlashPoint, identifier: container1Identifier.ToString() + "4");

			groupedPackingLine2.PackingLines = new[] { consolidatedPacklineG21, consolidatedPacklineG22 };
			groupedPackingLine2.HBLPaymentType = "A";

			consolidatedPacklineG11.HBLPaymentType = "B";
			consolidatedPacklineG22.HBLPaymentType = "C";

			container1.PackingLines = new[] { consolidatedPacklineG11, consolidatedPacklineG21 };
			container2.PackingLines = new[] { consolidatedPacklineG12, consolidatedPacklineG22 };

			carrierMessageData.Containers = new[] { container1, container2 };

			var asmShipment = CreateShipments("S0000009", "ASM", new[] { groupedPackingLine1 }, "ITN010", "", "DUE010", "UCR010", "CTK010", hasCTNNumber ? "CTN010" : string.Empty);
			var subASMShipment1 = CreateShipments("S00000011", "STD", new[] { consolidatedPacklineG11 }, "ITN021", "", "DUE021", "UCR021", "CTK021", hasCTNNumber ? "CTN021" : string.Empty);
			var subASMShipment2 = CreateShipments("S00000012", "STD", new[] { consolidatedPacklineG12 }, "ITN022", "", "DUE022", "UCR022", "CTK021", hasCTNNumber ? "CTN021" : string.Empty);
			var subShipment4 = CreateShipments("S00000013", "STD", new[] { groupedPackingLine2 }, "", "LOW", "", "", "", hasCTNNumber ? "CTN003" : string.Empty);

			carrierMessageData.Shipments = new[] { asmShipment, subShipment4 };
			asmShipment.Shipments = new[] { subASMShipment1, subASMShipment2 };
		}

		void PopulateTransportBookingPickupDeliveryInfos(Container container)
		{
			var transportBookingPickupDeliveryInfos = new List<TransportBookingPickupDeliveryInfo>();
			var package = Factory.New<PkgPackage>();
			package.KP_PackageID = container.Number;
			package.KP_F3_NKPackType = "CNT";
			package.Container.K0_RC_ContainerType = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;

			transportBookingPickupDeliveryInfos.Add(
				new TransportBookingPickupDeliveryInfo()
				{
					Address = CreateAddress("Pickup1"),
					Type = "PickupFrom",
					Packages = new PkgPackage[] { package }
				});
			transportBookingPickupDeliveryInfos.Add(
				new TransportBookingPickupDeliveryInfo()
				{
					Address = CreateAddress("Pickup2"),
					Type = "PickupFrom",
					Packages = new PkgPackage[] { package }
				});

			transportBookingPickupDeliveryInfos.Add(
				new TransportBookingPickupDeliveryInfo()
				{
					Address = CreateAddress("Delivery"),
					Type = "DeliveryTo",
					Packages = new PkgPackage[] { package }
				});

			container.TransportBookingPickupDeliveryInfos = transportBookingPickupDeliveryInfos;
		}

		Shipment CreateShipments(string shipmentID, string shipmentType, PackingLine[] packingLines, string itnNumber = "", string exportStatement = "", string dueNumber = "", string ucrNumber = "", string ctkNumber = "", string ctnNumber = "")
		{
			var shipment = new Shipment(ZGuid.NewZGuid());
			shipment.ShipmentID = shipmentID;
			shipment.HouseBillNumber = $"H{shipmentID}";
			shipment.ContainerPackingMode = new CodeDescription(FreightCodePairLists.JS_PackingModeList("SEA")) { Code = "LCL" };
			shipment.ShipperReference = "K00123";
			shipment.PickRequestedByDate = new ZDateTime(2019, 9, 15);
			shipment.DeliveryRequiredByDate = new ZDateTime(2019, 9, 20);
			shipment.ShipmentType = new CodeDescription(new CodeDescriptionPairList()) { Code = shipmentType };
			shipment.Consignor = CreateAddress("Consignor");
			shipment.Consignee = CreateAddress("Consignee");
			shipment.NotifyParty = CreateAddress("NotifyParty");
			shipment.NotifyParty2 = CreateAddress("NotifyParty2");
			shipment.NotifyParty3 = CreateAddress("NotifyParty3");
			shipment.Supplier = CreateAddress("Supplier");

			AddRegNumbers((Address)shipment.Consignee);
			AddRegNumbers((Address)shipment.NotifyParty);
			AddRegNumbers((Address)shipment.NotifyParty2);
			AddRegNumbers((Address)shipment.NotifyParty3);
			AddRegNumbers((Address)shipment.Supplier);

			void AddRegNumbers(Address address)
			{
				address.RegistrationNumbers = new[]
				{
					new DummyRegistrationNumber
					{
						Type = new CodeDescription(new CodeDescriptionPairList())
						{
							Code = "AAA"
						},
						CountryOfIssue = new Country(Context.Factory, Context.Countries)
						{
							Code = "NZ"
						},
						Value = "12345"
					},
					new DummyRegistrationNumber
					{
						Type = new CodeDescription(new CodeDescriptionPairList())
						{
							Code = "EOR"
						},
						CountryOfIssue = new Country(Context.Factory, Context.Countries)
						{
							Code = "GB"
						},
						Value = "GB12345"
					}
				};
			}

			shipment.PickupFrom = CreateAddress("PickupFrom");
			shipment.PickupCFS = CreateAddress("PickupCFS");
			shipment.DeliveryTo = CreateAddress("DeliveryTo");
			shipment.DeliveryCFS = CreateAddress("DeliveryCFS");
			shipment.PackingLines = packingLines;
			shipment.ITNNumber = itnNumber;
			shipment.ExportStatement = exportStatement;
			shipment.DUENumber = dueNumber;
			shipment.UCRNumber = ucrNumber;
			shipment.CTKNumber = ctkNumber;
			shipment.CTNNumber = ctnNumber;

			shipment.Transports = CreateTransports();

			return shipment;
		}

		Container CreateContainer(IContext context, string containerNumber, string commodityCode = "", bool hasTransportBookingPickupDeliveryInfos = false, object identifier = null)
		{
			var container = new Container(identifier ?? DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.Type = new ContainerType(context.ContainerTypes)
			{
				Code = "20FR"
			};
			container.AirVentFlow = new Measurement
			{
				Value = 12,
				Unit = new CodeDescription(context.AirVentFlow)
				{
					Code = "2L"
				}
			};

			container.ContainerCount = 1;
			container.PackCount = 3;
			container.IsEmpty = false;
			container.IsPartOf = false;
			container.IsShipperOwned = true;
			container.Seal = "SEAL1";
			container.ArrivalDeliveryRequiredBy = new ZDateTime(2019, 11, 23);

			container.SealPartyType = new DummyCodeDescription
			{
				Code = "CAR",
				Description = "Carrier"
			};
			container.SecondSeal = "SEAL2";
			container.SecondSealPartyType = new DummyCodeDescription
			{
				Code = "CUS",
				Description = "Customs"
			};
			container.ThirdSeal = "SEAL3";
			container.ThirdSealPartyType = new DummyCodeDescription
			{
				Code = "CTP",
				Description = "Terminal"
			};
			container.GoodsWeight = new Measurement()
			{
				Value = 200,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.TareWeight = new Measurement()
			{
				Value = 20,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.Dunnage = new Measurement()
			{
				Value = 30,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.GrossWeight = new Measurement()
			{
				Value = 272,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.Volume = new Measurement()
			{
				Value = 320,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangBack = new Measurement()
			{
				Value = 39,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangFront = new Measurement()
			{
				Value = 40,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangHeight = new Measurement()
			{
				Value = 41,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangLeft = new Measurement()
			{
				Value = 42,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangRight = new Measurement()
			{
				Value = 43,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.VolumeCapacity = new Measurement()
			{
				Value = 44,
				Unit = new DummyCodeDescription
				{
					Code = "M"
				}
			};

			if (!commodityCode.IsNullOrEmpty())
			{
				container.Commodity = new DummyCodeDescription
				{
					Code = commodityCode,
					Description = commodityCode + "DESC"
				};
			}

			container.ContainerQuality = new DummyCodeDescription
			{
				Code = "FOD",
				Description = "Food"
			};

			if (hasTransportBookingPickupDeliveryInfos)
			{
				PopulateTransportBookingPickupDeliveryInfos(container);
			}

			return container;
		}

		PackingLine CreatePackingLine(string shipmentID, string goodsDescription = "", string containerNumber = "", Decimal temperatureMinimum = 0m, Decimal temperatureMaximum = 0m, bool hasFlashPoint = false, object identifier = null, bool hasDangerousGoods = true, string groupITNNumber = "", string groupDUENumber = "", string groupPOFNumber = "", string groupUCRNumber = "", string groupCTKNumber = "", string groupCTNNumber = "", string packLineId = "")
		{
			var packingLine = new PackingLine(identifier ?? ZGuid.NewZGuid(), Factory);

			packingLine.ShipmentID = shipmentID;
			packingLine.ContainerNumber = containerNumber;
			packingLine.Quantity = 3;
			packingLine.GroupITNNumber = groupITNNumber;
			packingLine.GroupDUENumber = groupDUENumber;
			packingLine.GroupPOFNumber = groupPOFNumber;
			packingLine.GroupUCRNumber = groupUCRNumber;
			packingLine.GroupCTKNumber = groupCTKNumber;
			packingLine.GroupCTNNumber = groupCTNNumber;
			packingLine.PackageType = new DummyCodeDescription
			{
				Code = "PLT",
				Description = "Pallet"
			};
			packingLine.Weight = new Measurement()
			{
				Value = 88,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			packingLine.Volume = new Measurement()
			{
				Value = 55,
				Unit = new DummyCodeDescription
				{
					Code = "M3",
				}
			};
			packingLine.Height = new Measurement()
			{
				Value = 56,
				Unit = new DummyCodeDescription
				{
					Code = "M",
				}
			};
			packingLine.Width = new Measurement()
			{
				Value = 57,
				Unit = new DummyCodeDescription
				{
					Code = "M",
				}
			};
			packingLine.Length = new Measurement()
			{
				Value = 58,
				Unit = new DummyCodeDescription
				{
					Code = "M",
				}
			};

			packingLine.GoodsDescription = goodsDescription;
			packingLine.MarksAndNumbers = "marks & nums";
			packingLine.HarmonizedCode = new HarmonizedCode() { Code = "HC12345" };
			packingLine.ReferenceNumber = "reference number";
			packingLine.ImportReferenceNumber = "import reference number";
			packingLine.PackingLineID = packLineId;

			if (hasDangerousGoods)
			{
				var dangerousGoods = new List<DangerousGood>();

				var dangerousGood = new DangerousGood()
				{
					Code = "0001C",
					Unno = "0001",
					Quantity = 11,
					Variant = "C",
					ProperShippingName = "Danger",
					TechnicalName = "Technicals",
					IMOClass = "A",
					Standard = "IAT",
					PackedInLimitedQuantity = true,
				};

				if (hasFlashPoint)
				{
					dangerousGood.FlashPoint = new Measurement
					{
						Value = 10,
						Unit = new CodeDescription(context.TemperatureUnits)
						{
							Code = "C"
						}
					};
				}

				dangerousGoods.Add(dangerousGood);
				packingLine.DangerousGoods = dangerousGoods;
			}

			var hc = new HarmonizedCode();
			hc.Country = new Country(Factory, new RefCountryCollection(Factory)) { Code = "CN" };
			hc.Code = "1234.56";

			packingLine.HarmonizedCodes = new List<HarmonizedCode>() { hc };

			packingLine.RequiresTemperatureControl = true;
			packingLine.TemperatureMaximum = new Measurement
			{
				Value = temperatureMaximum,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			packingLine.TemperatureMinimum = new Measurement
			{
				Value = temperatureMinimum,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			packingLine.CUSCode1 = "1";
			packingLine.CUSCode2 = "2";
			packingLine.CUSCode3 = "2";
			packingLine.CUSCode5 = "5";

			return packingLine;
		}

		ITransports CreateTransports()
		{
			var transports = new List<DocDataObjects.Transport>();

			var transport1 = new UniversalDataBuss.DataObjects.Universal.TransportLeg()
			{
				LegOrder = 1,
				TransportMode = UniversalDataBuss.DataObjects.Universal.TransportMode.Sea,
				PortOfLoading = new UniversalDataBuss.DataObjects.Universal.UNLOCO()
				{
					Code = "CAACT",
					Name = "Acton"
				},
				PortOfDischarge = new UniversalDataBuss.DataObjects.Universal.UNLOCO()
				{
					Code = "USLAX",
					Name = "Los Angeles"
				}
			};
			transports.Add(DocDataObjects.Transport.Create(Context, transport1));

			var transport2 = new UniversalDataBuss.DataObjects.Universal.TransportLeg()
			{
				LegOrder = 2,
				TransportMode = UniversalDataBuss.DataObjects.Universal.TransportMode.Sea,
				PortOfLoading = new UniversalDataBuss.DataObjects.Universal.UNLOCO()
				{
					Code = "USLAX",
					Name = "Los Angeles"
				},
				PortOfDischarge = new UniversalDataBuss.DataObjects.Universal.UNLOCO()
				{
					Code = "SGCLE",
					Name = "Clementi"
				}
			};
			transports.Add(DocDataObjects.Transport.Create(Context, transport2));

			var transport3 = new UniversalDataBuss.DataObjects.Universal.TransportLeg()
			{
				LegOrder = 3,
				TransportMode = UniversalDataBuss.DataObjects.Universal.TransportMode.Sea,
				PortOfLoading = new UniversalDataBuss.DataObjects.Universal.UNLOCO()
				{
					Code = "SGCLE",
					Name = "Clementi"
				},
				PortOfDischarge = new UniversalDataBuss.DataObjects.Universal.UNLOCO()
				{
					Code = "DEHAM",
					Name = "Hamburg"
				}
			};
			transports.Add(DocDataObjects.Transport.Create(Context, transport3));

			var transport4 = new UniversalDataBuss.DataObjects.Universal.TransportLeg()
			{
				LegOrder = 4,
				TransportMode = UniversalDataBuss.DataObjects.Universal.TransportMode.Sea,
				PortOfLoading = new UniversalDataBuss.DataObjects.Universal.UNLOCO()
				{
					Code = "DEHAM",
					Name = "Hamburg"
				},
				PortOfDischarge = new UniversalDataBuss.DataObjects.Universal.UNLOCO()
				{
					Code = "NLRTM",
					Name = "Rotterdam"
				}
			};
			transports.Add(DocDataObjects.Transport.Create(Context, transport4));

			return Transports.Create(transports);
		}

		#endregion

		#region Expected Xml

		string GetExpectedXmlSI(string version, string flashPoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Auburn"">AUAUB</CarrierBookingOffice>
    <CoLoadBookingConfirmationReference>BKG0002</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>coload bill of lading number</CoLoadMasterBillNumber>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Door"">DTD</DeliveryMode>
    <GoodsValue>133.66</GoodsValue>
    <GoodsValueCurrency>USD</GoodsValueCurrency>
    <IsHazardous>true</IsHazardous>
    <LloydsIMO>12345</LloydsIMO>
    <NoCopyBills>2</NoCopyBills>
    <NoOriginalBills>1</NoOriginalBills>
    <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDestination Name=""Shanghai"">CNSHA</PortOfDestination>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ReleaseType Description=""BOL Original"">BOL</ReleaseType>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <TransportMode Description=""Sea"">SEA</TransportMode>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>AUAUB</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Auburn</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>{version}</Value>
      </AddInfo>
      <AddInfo>
        <Key>eBLDocumentationProvider</Key>
        <Value>Cargo X</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001000</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>HIR123</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier quote number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Letter Of Credit Number"">LCR</Type>
        <ReferenceNumber>LCR number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1234567890123456789</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1122334455667788990</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC111</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC222</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Freight As Agreed"">FAA</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Received for Shipment"">RFS</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board"">LOB</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Rail"">OBR</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Vessel"">OBV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board Named Vessel"">LNV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load, Stowage and Count"">LSC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""No Shipper's Export Declaration Required"">NSD</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>

    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <Commodity Description=""GENDESC"">GEN</Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>BillIssued</Type>
        <Value>2019-10-25T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <Value>2019-10-26T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstForeignArrival</Type>
        <Value>2019-10-27T00:00:00</Value>
      </Date>
      <Date>
        <Type>LastForeignDeparture</Type>
        <Value>2019-10-28T00:00:00</Value>
      </Date>
      <Date>
        <Type>EarliestDeparture</Type>
        <Value>2019-10-20T00:00:00</Value>
      </Date>
      <Date>
        <Type>LatestDelivery</Type>
        <Value>2019-10-21T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>US manifest ID</NoteText>
      </Note>
      <Note>
        <Description>WoodenPackageProcessType</Description>
        <NoteText>Not Applicable</NoteText>
      </Note>
      <Note>
        <Description>OtherBillClauses</Description>
        <NoteText>other bill clauses</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
      <Note>
        <Description>Forwarding Instruction Notes</Description>
        <NoteText>forwarding instruction</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instruction</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>SHIPPER ADDRESS LINE 1</Address1>
        <Address2>SHIPPER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHIPPER CITY</City>
        <CompanyName>SHIPPER</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SHIPPER PO</Postcode>
        <State>SHIPPER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CNO|BELGIAN COMPANY NUMBER"">GCR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>11111</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""VAT|CH-NUMBER"">VAT</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>22222|EG</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>CARRIER ADDRESS LINE 1</Address1>
        <Address2>CARRIER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIER CITY</City>
        <CompanyName>CARRIER</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIER PO</Postcode>
        <State>CARRIER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
        <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
        <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CONSIGNEE CITY</City>
        <CompanyName>CONSIGNEE</CompanyName>
        <Contact>Consignee contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Consignee email</Email>
        <Fax>Consignee fax</Fax>
        <GovRegNum>Consignee tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Consignee phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CONSIGNEE </Postcode>
        <State>CONSIGNEE STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIK|"">GCR</Type>
            <CountryOfIssue Name=""Belize"">BZ</CountryOfIssue>
            <Value>33333</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""|BUSINESS ID"">GCR</Type>
            <CountryOfIssue Name=""Finland"">FI</CountryOfIssue>
            <Value>44444</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Forwarder</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>SENDINGFORWARDERADDRESS ADDRESS LINE 1</Address1>
        <Address2>SENDINGFORWARDERADDRESS ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SENDINGFORWARDERADDRESS CITY</City>
        <CompanyName>SENDINGFORWARDERADDRESS</CompanyName>
        <Contact>SendingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarderAddress email</Email>
        <Fax>SendingForwarderAddr</Fax>
        <GovRegNum>SendingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarderAddr</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SENDINGFOR</Postcode>
        <State>SENDINGFORWARDERADDRESS S</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FreightPayer</AddressType>
        <AdditionalAddressInformation>FreightPayer additional info</AdditionalAddressInformation>
        <Address1>FREIGHTPAYER ADDRESS LINE 1</Address1>
        <Address2>FREIGHTPAYER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FREIGHTPAYER CITY</City>
        <CompanyName>FREIGHTPAYER</CompanyName>
        <Contact>FreightPayer contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>FreightPayer email</Email>
        <Fax>FreightPayer fax</Fax>
        <GovRegNum>FreightPayer tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>FreightPayer phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FREIGHTPAY</Postcode>
        <State>FREIGHTPAYER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
        <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
        <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>PICKUPFROM CITY</City>
        <CompanyName>PICKUPFROM</CompanyName>
        <Contact>PickupFrom contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>PickupFrom email</Email>
        <Fax>PickupFrom fax</Fax>
        <GovRegNum>PickupFrom tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>PickupFrom phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>PICKUPFROM</Postcode>
        <State>PICKUPFROM STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>DeliverTo additional info</AdditionalAddressInformation>
        <Address1>DELIVERTO ADDRESS LINE 1</Address1>
        <Address2>DELIVERTO ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DELIVERTO CITY</City>
        <CompanyName>DELIVERTO</CompanyName>
        <Contact>DeliverTo contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DeliverTo email</Email>
        <Fax>DeliverTo fax</Fax>
        <GovRegNum>DeliverTo tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DeliverTo phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DELIVERTO </Postcode>
        <State>DELIVERTO STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CURRENTUSER ADDRESS LINE 1</Address1>
        <Address2>CURRENTUSER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CURRENTUSER CITY</City>
        <CompanyName>CURRENTUSER</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CURRENTUSE</Postcode>
        <State>CURRENTUSER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000012</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000012</WayBillNumber>

        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>

        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN002, ITN003</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE002</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR002</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>

          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>

          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN002, ITN003</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR002</Value>
              </AddInfo>
            </AddInfoCollection>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000013</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000013</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>LOW</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 3</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 3</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test005</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>LOW</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0000009</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS0000009</WayBillNumber>

        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>

        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN002, ITN003, ITN001</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE002, DUE001</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR002, UCR001</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <DetailedDescription>AAA packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test002</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK001</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001</Value>
              </AddInfo>
            </AddInfoCollection>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test003</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>1</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>-1</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK001</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001</Value>
              </AddInfo>
            </AddInfoCollection>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN002, ITN003</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR002</Value>
              </AddInfo>
            </AddInfoCollection>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-12-01T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff>2019-08-01T00:00:00</LCLCutOff>
        <LCLReceivalCommences>2019-08-02T00:00:00</LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>12345</VesselLloydsIMO>
        <VesselName>Titanic</VesselName>
        <VoyageFlightNo>1234567</VoyageFlightNo>

        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		string GetExpectedXmlBR(string version, string flashPoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Auburn"">AUAUB</CarrierBookingOffice>
    <CoLoadBookingConfirmationReference>BKG0002</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>coload bill of lading number</CoLoadMasterBillNumber>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Door"">DTD</DeliveryMode>
    <GoodsValue>133.66</GoodsValue>
    <GoodsValueCurrency>USD</GoodsValueCurrency>
    <IsHazardous>true</IsHazardous>
    <IsOutOfGauge>true</IsOutOfGauge>
    <LloydsIMO>12345</LloydsIMO>
    <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDestination Name=""Shanghai"">CNSHA</PortOfDestination>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <TransportMode Description=""Sea"">SEA</TransportMode>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>AUAUB</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Auburn</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>{version}</Value>
      </AddInfo>
      <AddInfo>
        <Key>eBLDocumentationProvider</Key>
        <Value>Cargo X</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001000</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>HIR123</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier quote number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Letter Of Credit Number"">LCR</Type>
        <ReferenceNumber>LCR number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1234567890123456789</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1122334455667788990</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC111</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC222</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Received for Shipment"">RFS</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board"">LOB</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Rail"">OBR</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Vessel"">OBV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board Named Vessel"">LNV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load, Stowage and Count"">LSC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""No Shipper's Export Declaration Required"">NSD</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>

    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <Commodity Description=""GENDESC"">GEN</Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>BillIssued</Type>
        <Value>2019-10-25T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <Value>2019-10-26T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstForeignArrival</Type>
        <Value>2019-10-27T00:00:00</Value>
      </Date>
      <Date>
        <Type>LastForeignDeparture</Type>
        <Value>2019-10-28T00:00:00</Value>
      </Date>
      <Date>
        <Type>EarliestDeparture</Type>
        <Value>2019-10-20T00:00:00</Value>
      </Date>
      <Date>
        <Type>LatestDelivery</Type>
        <Value>2019-10-21T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>US manifest ID</NoteText>
      </Note>
      <Note>
        <Description>WoodenPackageProcessType</Description>
        <NoteText>Not Applicable</NoteText>
      </Note>
      <Note>
        <Description>OtherBillClauses</Description>
        <NoteText>other bill clauses</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instruction</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>SHIPPER ADDRESS LINE 1</Address1>
        <Address2>SHIPPER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHIPPER CITY</City>
        <CompanyName>SHIPPER</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SHIPPER PO</Postcode>
        <State>SHIPPER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CNO|BELGIAN COMPANY NUMBER"">GCR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>11111</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""VAT|CH-NUMBER"">VAT</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>22222</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>CARRIER ADDRESS LINE 1</Address1>
        <Address2>CARRIER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIER CITY</City>
        <CompanyName>CARRIER</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIER PO</Postcode>
        <State>CARRIER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
        <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
        <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CONSIGNEE CITY</City>
        <CompanyName>CONSIGNEE</CompanyName>
        <Contact>Consignee contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Consignee email</Email>
        <Fax>Consignee fax</Fax>
        <GovRegNum>Consignee tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Consignee phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CONSIGNEE </Postcode>
        <State>CONSIGNEE STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIK|"">GCR</Type>
            <CountryOfIssue Name=""Belize"">BZ</CountryOfIssue>
            <Value>33333</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""|BUSINESS ID"">GCR</Type>
            <CountryOfIssue Name=""Finland"">FI</CountryOfIssue>
            <Value>44444</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Forwarder</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>SENDINGFORWARDERADDRESS ADDRESS LINE 1</Address1>
        <Address2>SENDINGFORWARDERADDRESS ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SENDINGFORWARDERADDRESS CITY</City>
        <CompanyName>SENDINGFORWARDERADDRESS</CompanyName>
        <Contact>SendingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarderAddress email</Email>
        <Fax>SendingForwarderAddr</Fax>
        <GovRegNum>SendingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarderAddr</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SENDINGFOR</Postcode>
        <State>SENDINGFORWARDERADDRESS S</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FreightPayer</AddressType>
        <AdditionalAddressInformation>FreightPayer additional info</AdditionalAddressInformation>
        <Address1>FREIGHTPAYER ADDRESS LINE 1</Address1>
        <Address2>FREIGHTPAYER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FREIGHTPAYER CITY</City>
        <CompanyName>FREIGHTPAYER</CompanyName>
        <Contact>FreightPayer contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>FreightPayer email</Email>
        <Fax>FreightPayer fax</Fax>
        <GovRegNum>FreightPayer tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>FreightPayer phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FREIGHTPAY</Postcode>
        <State>FREIGHTPAYER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
        <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
        <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>PICKUPFROM CITY</City>
        <CompanyName>PICKUPFROM</CompanyName>
        <Contact>PickupFrom contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>PickupFrom email</Email>
        <Fax>PickupFrom fax</Fax>
        <GovRegNum>PickupFrom tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>PickupFrom phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>PICKUPFROM</Postcode>
        <State>PICKUPFROM STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>DeliverTo additional info</AdditionalAddressInformation>
        <Address1>DELIVERTO ADDRESS LINE 1</Address1>
        <Address2>DELIVERTO ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DELIVERTO CITY</City>
        <CompanyName>DELIVERTO</CompanyName>
        <Contact>DeliverTo contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DeliverTo email</Email>
        <Fax>DeliverTo fax</Fax>
        <GovRegNum>DeliverTo tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DeliverTo phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DELIVERTO </Postcode>
        <State>DELIVERTO STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CURRENTUSER ADDRESS LINE 1</Address1>
        <Address2>CURRENTUSER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CURRENTUSER CITY</City>
        <CompanyName>CURRENTUSER</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CURRENTUSE</Postcode>
        <State>CURRENTUSER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CustomsBroker</AddressType>
        <AdditionalAddressInformation>CustomsBroker additional info</AdditionalAddressInformation>
        <Address1>CUSTOMSBROKER ADDRESS LINE 1</Address1>
        <Address2>CUSTOMSBROKER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CUSTOMSBROKER CITY</City>
        <CompanyName>CUSTOMSBROKER</CompanyName>
        <Contact>CustomsBroker contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CustomsBroker email</Email>
        <Fax>CustomsBroker fax</Fax>
        <GovRegNum>CustomsBroker tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CustomsBroker phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CUSTOMSBRO</Postcode>
        <State>CUSTOMSBROKER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>

    <PreallocatedUNDGCollection>
      <PreallocatedUNDG>
        <EmergencyScheduleFire Description=""EXPLOSIVE SUBSTANCES AND ARTICLES"">F-B</EmergencyScheduleFire>
        <EmergencyScheduleSpillage Description=""EXPLOSIVE CHEMICALS"">S-Y</EmergencyScheduleSpillage>
        <ExceptedQuantityCode></ExceptedQuantityCode>
        <FlashPoint></FlashPoint>
        <IMOClass>1.1D</IMOClass>
        <MarinePollutant></MarinePollutant>
        <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
        <PackingGroup></PackingGroup>
        <ProperShippingName>AMMONIUM PICRATE</ProperShippingName>
        <Standard>IMO</Standard>
        <State>ExplosiveSubstance</State>
        <SubLabel1></SubLabel1>
        <SubLabel2></SubLabel2>
        <UNDGCode>0004a</UNDGCode>
      </PreallocatedUNDG>
      <PreallocatedUNDG>
        <EmergencyScheduleFire Description=""NON-WATER-REACTIVE FLAMMABLE LIQUIDS"">F-E</EmergencyScheduleFire>
        <EmergencyScheduleSpillage Description=""FLAMMABLE, CORROSIVE LIQUIDS"">S-C</EmergencyScheduleSpillage>
        <ExceptedQuantityCode>E0</ExceptedQuantityCode>
        <FlashPoint>25 cc</FlashPoint>
        <IMOClass>6.1</IMOClass>
        <MarinePollutant>Y</MarinePollutant>
        <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
        <PackingGroup>I</PackingGroup>
        <ProperShippingName>CHLOROACETONE, STABILIZED</ProperShippingName>
        <Standard>IMO</Standard>
        <State>Liquid</State>
        <SubLabel1>3</SubLabel1>
        <SubLabel2>8</SubLabel2>
        <UNDGCode>1695</UNDGCode>
      </PreallocatedUNDG>
    </PreallocatedUNDGCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000012</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000012</WayBillNumber>

        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>

        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN002, ITN003</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE002</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR002</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>

          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>

          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN002, ITN003</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR002</Value>
              </AddInfo>
            </AddInfoCollection>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000013</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000013</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>LOW</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 3</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 3</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test005</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>LOW</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0000009</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS0000009</WayBillNumber>

        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>

        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN002, ITN003, ITN001</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE002, DUE001</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR002, UCR001</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <DetailedDescription>AAA packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test002</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001</Value>
              </AddInfo>
            </AddInfoCollection>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test003</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>1</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>-1</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001</Value>
              </AddInfo>
            </AddInfoCollection>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN002, ITN003</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR002</Value>
              </AddInfo>
            </AddInfoCollection>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashPoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-12-01T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff>2019-08-01T00:00:00</LCLCutOff>
        <LCLReceivalCommences>2019-08-02T00:00:00</LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>12345</VesselLloydsIMO>
        <VesselName>Titanic</VesselName>
        <VoyageFlightNo>1234567</VoyageFlightNo>

        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		string GetExpectedXmlSI_IsGroupAndConsolidatePackingLines(string version, string flashPoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Auburn"">AUAUB</CarrierBookingOffice>
    <CoLoadBookingConfirmationReference>BKG0002</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>coload bill of lading number</CoLoadMasterBillNumber>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Door"">DTD</DeliveryMode>
    <GoodsValue>133.66</GoodsValue>
    <GoodsValueCurrency>USD</GoodsValueCurrency>
    <IsHazardous>true</IsHazardous>
    <LloydsIMO>12345</LloydsIMO>
    <NoCopyBills>2</NoCopyBills>
    <NoOriginalBills>1</NoOriginalBills>
    <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDestination Name=""Shanghai"">CNSHA</PortOfDestination>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ReleaseType Description=""BOL Original"">BOL</ReleaseType>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <TransportMode Description=""Sea"">SEA</TransportMode>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>AUAUB</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Auburn</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>{version}</Value>
      </AddInfo>
      <AddInfo>
        <Key>eBLDocumentationProvider</Key>
        <Value>Cargo X</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
      <AddInfo>
        <Key>GroupingMethod</Key>
        <Value>SHP</Value>
      </AddInfo>
      <AddInfo>
        <Key>ICS2FilingType</Key>
        <Value>Carrier</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001000</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>HIR123</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier quote number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Letter Of Credit Number"">LCR</Type>
        <ReferenceNumber>LCR number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1234567890123456789</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1122334455667788990</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC111</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC222</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Freight As Agreed"">FAA</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Received for Shipment"">RFS</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board"">LOB</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Rail"">OBR</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Vessel"">OBV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board Named Vessel"">LNV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load, Stowage and Count"">LSC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""No Shipper's Export Declaration Required"">NSD</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <Commodity Description=""GENDESC"">GEN</Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>BillIssued</Type>
        <Value>2019-10-25T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <Value>2019-10-26T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstForeignArrival</Type>
        <Value>2019-10-27T00:00:00</Value>
      </Date>
      <Date>
        <Type>LastForeignDeparture</Type>
        <Value>2019-10-28T00:00:00</Value>
      </Date>
      <Date>
        <Type>EarliestDeparture</Type>
        <Value>2019-10-20T00:00:00</Value>
      </Date>
      <Date>
        <Type>LatestDelivery</Type>
        <Value>2019-10-21T00:00:00</Value>
      </Date>
    </DateCollection>
    <NoteCollection>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>US manifest ID</NoteText>
      </Note>
      <Note>
        <Description>WoodenPackageProcessType</Description>
        <NoteText>Not Applicable</NoteText>
      </Note>
      <Note>
        <Description>OtherBillClauses</Description>
        <NoteText>other bill clauses</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
      <Note>
        <Description>Forwarding Instruction Notes</Description>
        <NoteText>forwarding instruction</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instruction</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>SHIPPER ADDRESS LINE 1</Address1>
        <Address2>SHIPPER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHIPPER CITY</City>
        <CompanyName>SHIPPER</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SHIPPER PO</Postcode>
        <State>SHIPPER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CNO|BELGIAN COMPANY NUMBER"">GCR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>11111</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""VAT|CH-NUMBER"">VAT</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>22222|EG</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>CARRIER ADDRESS LINE 1</Address1>
        <Address2>CARRIER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIER CITY</City>
        <CompanyName>CARRIER</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIER PO</Postcode>
        <State>CARRIER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
        <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
        <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CONSIGNEE CITY</City>
        <CompanyName>CONSIGNEE</CompanyName>
        <Contact>Consignee contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Consignee email</Email>
        <Fax>Consignee fax</Fax>
        <GovRegNum>Consignee tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Consignee phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CONSIGNEE </Postcode>
        <State>CONSIGNEE STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIK|"">GCR</Type>
            <CountryOfIssue Name=""Belize"">BZ</CountryOfIssue>
            <Value>33333</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""|BUSINESS ID"">GCR</Type>
            <CountryOfIssue Name=""Finland"">FI</CountryOfIssue>
            <Value>44444</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Forwarder</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>SENDINGFORWARDERADDRESS ADDRESS LINE 1</Address1>
        <Address2>SENDINGFORWARDERADDRESS ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SENDINGFORWARDERADDRESS CITY</City>
        <CompanyName>SENDINGFORWARDERADDRESS</CompanyName>
        <Contact>SendingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarderAddress email</Email>
        <Fax>SendingForwarderAddr</Fax>
        <GovRegNum>SendingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarderAddr</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SENDINGFOR</Postcode>
        <State>SENDINGFORWARDERADDRESS S</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FreightPayer</AddressType>
        <AdditionalAddressInformation>FreightPayer additional info</AdditionalAddressInformation>
        <Address1>FREIGHTPAYER ADDRESS LINE 1</Address1>
        <Address2>FREIGHTPAYER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FREIGHTPAYER CITY</City>
        <CompanyName>FREIGHTPAYER</CompanyName>
        <Contact>FreightPayer contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>FreightPayer email</Email>
        <Fax>FreightPayer fax</Fax>
        <GovRegNum>FreightPayer tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>FreightPayer phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FREIGHTPAY</Postcode>
        <State>FREIGHTPAYER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
        <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
        <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>PICKUPFROM CITY</City>
        <CompanyName>PICKUPFROM</CompanyName>
        <Contact>PickupFrom contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>PickupFrom email</Email>
        <Fax>PickupFrom fax</Fax>
        <GovRegNum>PickupFrom tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>PickupFrom phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>PICKUPFROM</Postcode>
        <State>PICKUPFROM STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>DeliverTo additional info</AdditionalAddressInformation>
        <Address1>DELIVERTO ADDRESS LINE 1</Address1>
        <Address2>DELIVERTO ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DELIVERTO CITY</City>
        <CompanyName>DELIVERTO</CompanyName>
        <Contact>DeliverTo contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DeliverTo email</Email>
        <Fax>DeliverTo fax</Fax>
        <GovRegNum>DeliverTo tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DeliverTo phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DELIVERTO </Postcode>
        <State>DELIVERTO STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CURRENTUSER ADDRESS LINE 1</Address1>
        <Address2>CURRENTUSER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CURRENTUSER CITY</City>
        <CompanyName>CURRENTUSER</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CURRENTUSE</Postcode>
        <State>CURRENTUSER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000013</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000013</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>LOW</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>BBB Grouped packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB Grouped packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>2</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>2</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>POF003, POF004</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>AAA</ContainerNumber>
                <DetailedDescription>BBB Grouped packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB Grouped packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID></PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>2</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>1</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>LOW</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB Grouped packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB Grouped packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID></PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>3</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>2</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>LOW</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
        <PaymentHandlingInstructionCollection>
          <PaymentHandlingInstruction>
            <Category Description=""HBL Payment Type"">HPT</Category>
            <PaymentMethod Description=""Payment in cash"">A</PaymentMethod>
          </PaymentHandlingInstruction>
        </PaymentHandlingInstructionCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0000009</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS0000009</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN021, ITN022</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE021, DUE022</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR021, UCR022</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA Grouped packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA Grouped packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>3</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>2</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK001, CTK002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001, DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001, UCR002</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>AAA</ContainerNumber>
                <DetailedDescription>AAA Grouped packline 1</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>AAA Grouped packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID></PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>10</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>-10</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN021</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>CTK</Key>
                    <Value>CTK021</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE021</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR021</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>AAA Grouped packline 1</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>AAA Grouped packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID></PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>2</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>2</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN022</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>CTK</Key>
                    <Value>CTK021</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE022</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR022</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
        <PaymentHandlingInstructionCollection>
          <PaymentHandlingInstruction>
            <Category Description=""HBL Payment Type"">HPT</Category>
            <PaymentMethod Description=""Payment by credit card"">B</PaymentMethod>
          </PaymentHandlingInstruction>
        </PaymentHandlingInstructionCollection>
      </SubShipment>
    </SubShipmentCollection>
    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-12-01T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff>2019-08-01T00:00:00</LCLCutOff>
        <LCLReceivalCommences>2019-08-02T00:00:00</LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>12345</VesselLloydsIMO>
        <VesselName>Titanic</VesselName>
        <VoyageFlightNo>1234567</VoyageFlightNo>
        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		string GetExpectedXmlBR_IsGroupAndConsolidatePackingLines(string version, string flashPoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Auburn"">AUAUB</CarrierBookingOffice>
    <CoLoadBookingConfirmationReference>BKG0002</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>coload bill of lading number</CoLoadMasterBillNumber>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Door"">DTD</DeliveryMode>
    <GoodsValue>133.66</GoodsValue>
    <GoodsValueCurrency>USD</GoodsValueCurrency>
    <IsHazardous>true</IsHazardous>
    <IsOutOfGauge>true</IsOutOfGauge>
    <LloydsIMO>12345</LloydsIMO>
    <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDestination Name=""Shanghai"">CNSHA</PortOfDestination>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <TransportMode Description=""Sea"">SEA</TransportMode>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>AUAUB</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Auburn</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>{version}</Value>
      </AddInfo>
      <AddInfo>
        <Key>eBLDocumentationProvider</Key>
        <Value>Cargo X</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
      <AddInfo>
        <Key>GroupingMethod</Key>
        <Value>SHP</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001000</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>HIR123</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier quote number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Letter Of Credit Number"">LCR</Type>
        <ReferenceNumber>LCR number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1234567890123456789</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1122334455667788990</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC111</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC222</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Received for Shipment"">RFS</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board"">LOB</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Rail"">OBR</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Vessel"">OBV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board Named Vessel"">LNV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load, Stowage and Count"">LSC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""No Shipper's Export Declaration Required"">NSD</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <Commodity Description=""GENDESC"">GEN</Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>BillIssued</Type>
        <Value>2019-10-25T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <Value>2019-10-26T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstForeignArrival</Type>
        <Value>2019-10-27T00:00:00</Value>
      </Date>
      <Date>
        <Type>LastForeignDeparture</Type>
        <Value>2019-10-28T00:00:00</Value>
      </Date>
      <Date>
        <Type>EarliestDeparture</Type>
        <Value>2019-10-20T00:00:00</Value>
      </Date>
      <Date>
        <Type>LatestDelivery</Type>
        <Value>2019-10-21T00:00:00</Value>
      </Date>
    </DateCollection>
    <NoteCollection>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>US manifest ID</NoteText>
      </Note>
      <Note>
        <Description>WoodenPackageProcessType</Description>
        <NoteText>Not Applicable</NoteText>
      </Note>
      <Note>
        <Description>OtherBillClauses</Description>
        <NoteText>other bill clauses</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instruction</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>SHIPPER ADDRESS LINE 1</Address1>
        <Address2>SHIPPER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHIPPER CITY</City>
        <CompanyName>SHIPPER</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SHIPPER PO</Postcode>
        <State>SHIPPER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CNO|BELGIAN COMPANY NUMBER"">GCR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>11111</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""VAT|CH-NUMBER"">VAT</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>22222</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>CARRIER ADDRESS LINE 1</Address1>
        <Address2>CARRIER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIER CITY</City>
        <CompanyName>CARRIER</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIER PO</Postcode>
        <State>CARRIER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
        <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
        <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CONSIGNEE CITY</City>
        <CompanyName>CONSIGNEE</CompanyName>
        <Contact>Consignee contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Consignee email</Email>
        <Fax>Consignee fax</Fax>
        <GovRegNum>Consignee tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Consignee phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CONSIGNEE </Postcode>
        <State>CONSIGNEE STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIK|"">GCR</Type>
            <CountryOfIssue Name=""Belize"">BZ</CountryOfIssue>
            <Value>33333</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""|BUSINESS ID"">GCR</Type>
            <CountryOfIssue Name=""Finland"">FI</CountryOfIssue>
            <Value>44444</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Forwarder</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>SENDINGFORWARDERADDRESS ADDRESS LINE 1</Address1>
        <Address2>SENDINGFORWARDERADDRESS ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SENDINGFORWARDERADDRESS CITY</City>
        <CompanyName>SENDINGFORWARDERADDRESS</CompanyName>
        <Contact>SendingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarderAddress email</Email>
        <Fax>SendingForwarderAddr</Fax>
        <GovRegNum>SendingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarderAddr</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SENDINGFOR</Postcode>
        <State>SENDINGFORWARDERADDRESS S</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FreightPayer</AddressType>
        <AdditionalAddressInformation>FreightPayer additional info</AdditionalAddressInformation>
        <Address1>FREIGHTPAYER ADDRESS LINE 1</Address1>
        <Address2>FREIGHTPAYER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FREIGHTPAYER CITY</City>
        <CompanyName>FREIGHTPAYER</CompanyName>
        <Contact>FreightPayer contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>FreightPayer email</Email>
        <Fax>FreightPayer fax</Fax>
        <GovRegNum>FreightPayer tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>FreightPayer phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FREIGHTPAY</Postcode>
        <State>FREIGHTPAYER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
        <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
        <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>PICKUPFROM CITY</City>
        <CompanyName>PICKUPFROM</CompanyName>
        <Contact>PickupFrom contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>PickupFrom email</Email>
        <Fax>PickupFrom fax</Fax>
        <GovRegNum>PickupFrom tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>PickupFrom phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>PICKUPFROM</Postcode>
        <State>PICKUPFROM STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>DeliverTo additional info</AdditionalAddressInformation>
        <Address1>DELIVERTO ADDRESS LINE 1</Address1>
        <Address2>DELIVERTO ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DELIVERTO CITY</City>
        <CompanyName>DELIVERTO</CompanyName>
        <Contact>DeliverTo contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DeliverTo email</Email>
        <Fax>DeliverTo fax</Fax>
        <GovRegNum>DeliverTo tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DeliverTo phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DELIVERTO </Postcode>
        <State>DELIVERTO STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CURRENTUSER ADDRESS LINE 1</Address1>
        <Address2>CURRENTUSER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CURRENTUSER CITY</City>
        <CompanyName>CURRENTUSER</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CURRENTUSE</Postcode>
        <State>CURRENTUSER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CustomsBroker</AddressType>
        <AdditionalAddressInformation>CustomsBroker additional info</AdditionalAddressInformation>
        <Address1>CUSTOMSBROKER ADDRESS LINE 1</Address1>
        <Address2>CUSTOMSBROKER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CUSTOMSBROKER CITY</City>
        <CompanyName>CUSTOMSBROKER</CompanyName>
        <Contact>CustomsBroker contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CustomsBroker email</Email>
        <Fax>CustomsBroker fax</Fax>
        <GovRegNum>CustomsBroker tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CustomsBroker phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CUSTOMSBRO</Postcode>
        <State>CUSTOMSBROKER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
    <PreallocatedUNDGCollection>
      <PreallocatedUNDG>
        <EmergencyScheduleFire Description=""EXPLOSIVE SUBSTANCES AND ARTICLES"">F-B</EmergencyScheduleFire>
        <EmergencyScheduleSpillage Description=""EXPLOSIVE CHEMICALS"">S-Y</EmergencyScheduleSpillage>
        <ExceptedQuantityCode></ExceptedQuantityCode>
        <FlashPoint></FlashPoint>
        <IMOClass>1.1D</IMOClass>
        <MarinePollutant></MarinePollutant>
        <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
        <PackingGroup></PackingGroup>
        <ProperShippingName>AMMONIUM PICRATE</ProperShippingName>
        <Standard>IMO</Standard>
        <State>ExplosiveSubstance</State>
        <SubLabel1></SubLabel1>
        <SubLabel2></SubLabel2>
        <UNDGCode>0004a</UNDGCode>
      </PreallocatedUNDG>
      <PreallocatedUNDG>
        <EmergencyScheduleFire Description=""NON-WATER-REACTIVE FLAMMABLE LIQUIDS"">F-E</EmergencyScheduleFire>
        <EmergencyScheduleSpillage Description=""FLAMMABLE, CORROSIVE LIQUIDS"">S-C</EmergencyScheduleSpillage>
        <ExceptedQuantityCode>E0</ExceptedQuantityCode>
        <FlashPoint>25 cc</FlashPoint>
        <IMOClass>6.1</IMOClass>
        <MarinePollutant>Y</MarinePollutant>
        <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
        <PackingGroup>I</PackingGroup>
        <ProperShippingName>CHLOROACETONE, STABILIZED</ProperShippingName>
        <Standard>IMO</Standard>
        <State>Liquid</State>
        <SubLabel1>3</SubLabel1>
        <SubLabel2>8</SubLabel2>
        <UNDGCode>1695</UNDGCode>
      </PreallocatedUNDG>
    </PreallocatedUNDGCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000013</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000013</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>LOW</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>BBB Grouped packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB Grouped packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>2</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>2</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>LOW</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>AAA</ContainerNumber>
                <DetailedDescription>BBB Grouped packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB Grouped packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID></PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>2</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>1</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>LOW</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB Grouped packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB Grouped packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID></PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>3</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>2</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>LOW</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0000009</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS0000009</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN021, ITN022</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE021, DUE022</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR021, UCR022</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA Grouped packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA Grouped packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>3</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>2</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN021, ITN022</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE021, DUE022</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR021, UCR022</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>AAA</ContainerNumber>
                <DetailedDescription>AAA Grouped packline 1</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>AAA Grouped packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID></PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>10</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>-10</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN021</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE021</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR021</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>AAA Grouped packline 1</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>AAA Grouped packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID></PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>2</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>2</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN022</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE022</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR022</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-12-01T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff>2019-08-01T00:00:00</LCLCutOff>
        <LCLReceivalCommences>2019-08-02T00:00:00</LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>12345</VesselLloydsIMO>
        <VesselName>Titanic</VesselName>
        <VoyageFlightNo>1234567</VoyageFlightNo>
        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		string GetExpectedXmlSI_DoNotGroup(string version, string flashPoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Auburn"">AUAUB</CarrierBookingOffice>
    <CoLoadBookingConfirmationReference>BKG0002</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>coload bill of lading number</CoLoadMasterBillNumber>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Door"">DTD</DeliveryMode>
    <GoodsValue>133.66</GoodsValue>
    <GoodsValueCurrency>USD</GoodsValueCurrency>
    <IsHazardous>true</IsHazardous>
    <LloydsIMO>12345</LloydsIMO>
    <NoCopyBills>2</NoCopyBills>
    <NoOriginalBills>1</NoOriginalBills>
    <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDestination Name=""Shanghai"">CNSHA</PortOfDestination>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ReleaseType Description=""BOL Original"">BOL</ReleaseType>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <TransportMode Description=""Sea"">SEA</TransportMode>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>AUAUB</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Auburn</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>{version}</Value>
      </AddInfo>
      <AddInfo>
        <Key>eBLDocumentationProvider</Key>
        <Value>Cargo X</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
      <AddInfo>
        <Key>GroupingMethod</Key>
        <Value>DNG</Value>
      </AddInfo>
      <AddInfo>
        <Key>ICS2DeclarantEORI</Key>
        <Value>AU123456789012345678901234567890123</Value>
      </AddInfo>
      <AddInfo>
        <Key>ICS2FilingType</Key>
        <Value>Declarant</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001000</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>HIR123</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier quote number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Letter Of Credit Number"">LCR</Type>
        <ReferenceNumber>LCR number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1234567890123456789</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1122334455667788990</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC111</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC222</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Freight As Agreed"">FAA</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Received for Shipment"">RFS</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board"">LOB</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Rail"">OBR</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Vessel"">OBV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board Named Vessel"">LNV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load, Stowage and Count"">LSC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""No Shipper's Export Declaration Required"">NSD</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <Commodity Description=""GENDESC"">GEN</Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>BillIssued</Type>
        <Value>2019-10-25T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <Value>2019-10-26T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstForeignArrival</Type>
        <Value>2019-10-27T00:00:00</Value>
      </Date>
      <Date>
        <Type>LastForeignDeparture</Type>
        <Value>2019-10-28T00:00:00</Value>
      </Date>
      <Date>
        <Type>EarliestDeparture</Type>
        <Value>2019-10-20T00:00:00</Value>
      </Date>
      <Date>
        <Type>LatestDelivery</Type>
        <Value>2019-10-21T00:00:00</Value>
      </Date>
    </DateCollection>
    <NoteCollection>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>US manifest ID</NoteText>
      </Note>
      <Note>
        <Description>WoodenPackageProcessType</Description>
        <NoteText>Not Applicable</NoteText>
      </Note>
      <Note>
        <Description>OtherBillClauses</Description>
        <NoteText>other bill clauses</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
      <Note>
        <Description>Forwarding Instruction Notes</Description>
        <NoteText>forwarding instruction</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instruction</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>SHIPPER ADDRESS LINE 1</Address1>
        <Address2>SHIPPER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHIPPER CITY</City>
        <CompanyName>SHIPPER</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SHIPPER PO</Postcode>
        <State>SHIPPER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CNO|BELGIAN COMPANY NUMBER"">GCR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>11111</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""VAT|CH-NUMBER"">VAT</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>22222|EG</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>CARRIER ADDRESS LINE 1</Address1>
        <Address2>CARRIER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIER CITY</City>
        <CompanyName>CARRIER</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIER PO</Postcode>
        <State>CARRIER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
        <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
        <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CONSIGNEE CITY</City>
        <CompanyName>CONSIGNEE</CompanyName>
        <Contact>Consignee contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Consignee email</Email>
        <Fax>Consignee fax</Fax>
        <GovRegNum>Consignee tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Consignee phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CONSIGNEE </Postcode>
        <State>CONSIGNEE STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIK|"">GCR</Type>
            <CountryOfIssue Name=""Belize"">BZ</CountryOfIssue>
            <Value>33333</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""|BUSINESS ID"">GCR</Type>
            <CountryOfIssue Name=""Finland"">FI</CountryOfIssue>
            <Value>44444</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Forwarder</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>SENDINGFORWARDERADDRESS ADDRESS LINE 1</Address1>
        <Address2>SENDINGFORWARDERADDRESS ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SENDINGFORWARDERADDRESS CITY</City>
        <CompanyName>SENDINGFORWARDERADDRESS</CompanyName>
        <Contact>SendingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarderAddress email</Email>
        <Fax>SendingForwarderAddr</Fax>
        <GovRegNum>SendingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarderAddr</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SENDINGFOR</Postcode>
        <State>SENDINGFORWARDERADDRESS S</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FreightPayer</AddressType>
        <AdditionalAddressInformation>FreightPayer additional info</AdditionalAddressInformation>
        <Address1>FREIGHTPAYER ADDRESS LINE 1</Address1>
        <Address2>FREIGHTPAYER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FREIGHTPAYER CITY</City>
        <CompanyName>FREIGHTPAYER</CompanyName>
        <Contact>FreightPayer contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>FreightPayer email</Email>
        <Fax>FreightPayer fax</Fax>
        <GovRegNum>FreightPayer tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>FreightPayer phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FREIGHTPAY</Postcode>
        <State>FREIGHTPAYER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
        <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
        <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>PICKUPFROM CITY</City>
        <CompanyName>PICKUPFROM</CompanyName>
        <Contact>PickupFrom contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>PickupFrom email</Email>
        <Fax>PickupFrom fax</Fax>
        <GovRegNum>PickupFrom tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>PickupFrom phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>PICKUPFROM</Postcode>
        <State>PICKUPFROM STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>DeliverTo additional info</AdditionalAddressInformation>
        <Address1>DELIVERTO ADDRESS LINE 1</Address1>
        <Address2>DELIVERTO ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DELIVERTO CITY</City>
        <CompanyName>DELIVERTO</CompanyName>
        <Contact>DeliverTo contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DeliverTo email</Email>
        <Fax>DeliverTo fax</Fax>
        <GovRegNum>DeliverTo tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DeliverTo phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DELIVERTO </Postcode>
        <State>DELIVERTO STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CURRENTUSER ADDRESS LINE 1</Address1>
        <Address2>CURRENTUSER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CURRENTUSER CITY</City>
        <CompanyName>CURRENTUSER</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CURRENTUSE</Postcode>
        <State>CURRENTUSER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000012</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000012</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN002, ITN003</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE002</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR002</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN002, ITN003</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR002</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test004</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN002, ITN003</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>CTK</Key>
                    <Value>CTK002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR002</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000013</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000013</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>LOW</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 3</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 3</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test005</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>LOW</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB packline 3</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB packline 3</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test005</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>LOW</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0000009</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS0000009</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN002, ITN003, ITN001</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE002, DUE001</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR002, UCR001</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <DetailedDescription>AAA packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test002</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK001</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>AAA</ContainerNumber>
                <DetailedDescription>AAA packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>AAA packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test002</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN001, ITN002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>CTK</Key>
                    <Value>CTK001</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE001</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR001</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test003</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>1</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>-1</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK001</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB packline 1</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test003</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>1</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>-1</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN001, ITN002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>CTK</Key>
                    <Value>CTK001</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE001</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR001</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN002, ITN003</Value>
              </AddInfo>
              <AddInfo>
                <Key>CTK</Key>
                <Value>CTK002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR002</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test004</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN002, ITN003</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>CTK</Key>
                    <Value>CTK002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR002</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-12-01T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff>2019-08-01T00:00:00</LCLCutOff>
        <LCLReceivalCommences>2019-08-02T00:00:00</LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>12345</VesselLloydsIMO>
        <VesselName>Titanic</VesselName>
        <VoyageFlightNo>1234567</VoyageFlightNo>
        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>

";

		string GetExpectedXmlBR_DoNotGroup(string version, string flashPoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <CarrierBookingOffice Name=""Auburn"">AUAUB</CarrierBookingOffice>
    <CoLoadBookingConfirmationReference>BKG0002</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>coload bill of lading number</CoLoadMasterBillNumber>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Door"">DTD</DeliveryMode>
    <GoodsValue>133.66</GoodsValue>
    <GoodsValueCurrency>USD</GoodsValueCurrency>
    <IsHazardous>true</IsHazardous>
    <IsOutOfGauge>true</IsOutOfGauge>
    <LloydsIMO>12345</LloydsIMO>
    <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDestination Name=""Shanghai"">CNSHA</PortOfDestination>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <TransportMode Description=""Sea"">SEA</TransportMode>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>1234567</VoyageFlightNo>
    <WayBillNumber>bill of lading number</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierBookingOffice_Code</Key>
        <Value>AUAUB</Value>
      </AddInfo>
      <AddInfo>
        <Key>CarrierBookingOffice_Name</Key>
        <Value>Auburn</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>{version}</Value>
      </AddInfo>
      <AddInfo>
        <Key>eBLDocumentationProvider</Key>
        <Value>Cargo X</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightPayableAt_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
      <AddInfo>
        <Key>GroupingMethod</Key>
        <Value>DNG</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Bill Of Lading Number"">BOL</Type>
        <ReferenceNumber>bill of lading number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001000</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>HIR123</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Quote Number"">CQN</Type>
        <ReferenceNumber>carrier quote number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Contract Named Account"">NAC</Type>
        <ReferenceNumber>contract named account</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Letter Of Credit Number"">LCR</Type>
        <ReferenceNumber>LCR number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BKG 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Shipping Order/Shi Lian Dan"">SLD</Type>
        <ReferenceNumber>SLD 2</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1234567890123456789</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""ACID Number"">ACI</Type>
        <ReferenceNumber>1122334455667788990</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC111</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""REFERÊNCIA ÚNICA DE CARGA"">RUC</Type>
        <ReferenceNumber>RUC222</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Received for Shipment"">RFS</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board"">LOB</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Rail"">OBR</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""On Board Vessel"">OBV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Laden on Board Named Vessel"">LNV</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Shipper's Load, Stowage and Count"">LSC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""No Shipper's Export Declaration Required"">NSD</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <Commodity Description=""GENDESC"">GEN</Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerQuality Description=""Food"">FOD</ContainerQuality>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>BillIssued</Type>
        <Value>2019-10-25T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstArrivalInCountry</Type>
        <Value>2019-10-26T00:00:00</Value>
      </Date>
      <Date>
        <Type>FirstForeignArrival</Type>
        <Value>2019-10-27T00:00:00</Value>
      </Date>
      <Date>
        <Type>LastForeignDeparture</Type>
        <Value>2019-10-28T00:00:00</Value>
      </Date>
      <Date>
        <Type>EarliestDeparture</Type>
        <Value>2019-10-20T00:00:00</Value>
      </Date>
      <Date>
        <Type>LatestDelivery</Type>
        <Value>2019-10-21T00:00:00</Value>
      </Date>
    </DateCollection>
    <NoteCollection>
      <Note>
        <Description>Goods Handling Instructions</Description>
        <NoteText>goods handling instructions</NoteText>
      </Note>
      <Note>
        <Description>USCanadaManifestSelfFilerID</Description>
        <NoteText>US manifest ID</NoteText>
      </Note>
      <Note>
        <Description>WoodenPackageProcessType</Description>
        <NoteText>Not Applicable</NoteText>
      </Note>
      <Note>
        <Description>OtherBillClauses</Description>
        <NoteText>other bill clauses</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>special instruction</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>SHIPPER ADDRESS LINE 1</Address1>
        <Address2>SHIPPER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHIPPER CITY</City>
        <CompanyName>SHIPPER</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SHIPPER PO</Postcode>
        <State>SHIPPER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CNO|BELGIAN COMPANY NUMBER"">GCR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>11111</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""VAT|CH-NUMBER"">VAT</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>22222</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>CARRIER ADDRESS LINE 1</Address1>
        <Address2>CARRIER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CARRIER CITY</City>
        <CompanyName>CARRIER</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CARRIER PO</Postcode>
        <State>CARRIER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
        <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
        <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CONSIGNEE CITY</City>
        <CompanyName>CONSIGNEE</CompanyName>
        <Contact>Consignee contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Consignee email</Email>
        <Fax>Consignee fax</Fax>
        <GovRegNum>Consignee tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Consignee phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CONSIGNEE </Postcode>
        <State>CONSIGNEE STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIK|"">GCR</Type>
            <CountryOfIssue Name=""Belize"">BZ</CountryOfIssue>
            <Value>33333</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY CITY</City>
        <CompanyName>NOTIFYPARTY</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""|BUSINESS ID"">GCR</Type>
            <CountryOfIssue Name=""Finland"">FI</CountryOfIssue>
            <Value>44444</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
        <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NOTIFYPARTY2 CITY</City>
        <CompanyName>NOTIFYPARTY2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NOTIFYPART</Postcode>
        <State>NOTIFYPARTY2 STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Forwarder</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>FORWARDER ADDRESS LINE 1</Address1>
        <Address2>FORWARDER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FORWARDER CITY</City>
        <CompanyName>FORWARDER</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FORWARDER </Postcode>
        <State>FORWARDER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>SENDINGFORWARDERADDRESS ADDRESS LINE 1</Address1>
        <Address2>SENDINGFORWARDERADDRESS ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SENDINGFORWARDERADDRESS CITY</City>
        <CompanyName>SENDINGFORWARDERADDRESS</CompanyName>
        <Contact>SendingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarderAddress email</Email>
        <Fax>SendingForwarderAddr</Fax>
        <GovRegNum>SendingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarderAddr</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SENDINGFOR</Postcode>
        <State>SENDINGFORWARDERADDRESS S</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>FreightPayer</AddressType>
        <AdditionalAddressInformation>FreightPayer additional info</AdditionalAddressInformation>
        <Address1>FREIGHTPAYER ADDRESS LINE 1</Address1>
        <Address2>FREIGHTPAYER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>FREIGHTPAYER CITY</City>
        <CompanyName>FREIGHTPAYER</CompanyName>
        <Contact>FreightPayer contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>FreightPayer email</Email>
        <Fax>FreightPayer fax</Fax>
        <GovRegNum>FreightPayer tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>FreightPayer phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>FREIGHTPAY</Postcode>
        <State>FREIGHTPAYER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
        <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
        <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>PICKUPFROM CITY</City>
        <CompanyName>PICKUPFROM</CompanyName>
        <Contact>PickupFrom contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>PickupFrom email</Email>
        <Fax>PickupFrom fax</Fax>
        <GovRegNum>PickupFrom tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>PickupFrom phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>PICKUPFROM</Postcode>
        <State>PICKUPFROM STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>DeliverTo additional info</AdditionalAddressInformation>
        <Address1>DELIVERTO ADDRESS LINE 1</Address1>
        <Address2>DELIVERTO ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DELIVERTO CITY</City>
        <CompanyName>DELIVERTO</CompanyName>
        <Contact>DeliverTo contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DeliverTo email</Email>
        <Fax>DeliverTo fax</Fax>
        <GovRegNum>DeliverTo tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DeliverTo phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DELIVERTO </Postcode>
        <State>DELIVERTO STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CURRENTUSER ADDRESS LINE 1</Address1>
        <Address2>CURRENTUSER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CURRENTUSER CITY</City>
        <CompanyName>CURRENTUSER</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CURRENTUSE</Postcode>
        <State>CURRENTUSER STATE</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CustomsBroker</AddressType>
        <AdditionalAddressInformation>CustomsBroker additional info</AdditionalAddressInformation>
        <Address1>CUSTOMSBROKER ADDRESS LINE 1</Address1>
        <Address2>CUSTOMSBROKER ADDRESS LINE 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CUSTOMSBROKER CITY</City>
        <CompanyName>CUSTOMSBROKER</CompanyName>
        <Contact>CustomsBroker contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CustomsBroker email</Email>
        <Fax>CustomsBroker fax</Fax>
        <GovRegNum>CustomsBroker tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CustomsBroker phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CUSTOMSBRO</Postcode>
        <State>CUSTOMSBROKER STATE</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Haulage"">DHC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Destination Port"">DPC</Category>
        <PaymentMethod Description=""Prepaid"">PPD</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Haulage"">OHC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
      <PaymentHandlingInstruction>
        <Category Description=""Origin Port"">OPC</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
    <PreallocatedUNDGCollection>
      <PreallocatedUNDG>
        <EmergencyScheduleFire Description=""EXPLOSIVE SUBSTANCES AND ARTICLES"">F-B</EmergencyScheduleFire>
        <EmergencyScheduleSpillage Description=""EXPLOSIVE CHEMICALS"">S-Y</EmergencyScheduleSpillage>
        <ExceptedQuantityCode></ExceptedQuantityCode>
        <FlashPoint></FlashPoint>
        <IMOClass>1.1D</IMOClass>
        <MarinePollutant></MarinePollutant>
        <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
        <PackingGroup></PackingGroup>
        <ProperShippingName>AMMONIUM PICRATE</ProperShippingName>
        <Standard>IMO</Standard>
        <State>ExplosiveSubstance</State>
        <SubLabel1></SubLabel1>
        <SubLabel2></SubLabel2>
        <UNDGCode>0004a</UNDGCode>
      </PreallocatedUNDG>
      <PreallocatedUNDG>
        <EmergencyScheduleFire Description=""NON-WATER-REACTIVE FLAMMABLE LIQUIDS"">F-E</EmergencyScheduleFire>
        <EmergencyScheduleSpillage Description=""FLAMMABLE, CORROSIVE LIQUIDS"">S-C</EmergencyScheduleSpillage>
        <ExceptedQuantityCode>E0</ExceptedQuantityCode>
        <FlashPoint>25 cc</FlashPoint>
        <IMOClass>6.1</IMOClass>
        <MarinePollutant>Y</MarinePollutant>
        <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
        <PackingGroup>I</PackingGroup>
        <ProperShippingName>CHLOROACETONE, STABILIZED</ProperShippingName>
        <Standard>IMO</Standard>
        <State>Liquid</State>
        <SubLabel1>3</SubLabel1>
        <SubLabel2>8</SubLabel2>
        <UNDGCode>1695</UNDGCode>
      </PreallocatedUNDG>
    </PreallocatedUNDGCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000012</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000012</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN002, ITN003</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE002</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR002</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN002, ITN003</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR002</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test004</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN002, ITN003</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR002</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00000013</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS00000013</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>LOW</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 3</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 3</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test005</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>LOW</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB packline 3</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB packline 3</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test005</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>LOW</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0000009</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference>K00123</BookingConfirmationReference>
        <HBLContainerPackModeOverride>LCL</HBLContainerPackModeOverride>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber>HS0000009</WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy>2019-09-20T00:00:00</DeliveryRequiredBy>
          <PickupRequiredBy>2019-09-15T00:00:00</PickupRequiredBy>
        </LocalProcessing>
        <AddInfoCollection>
          <AddInfo>
            <Key>CountriesOfRouting</Key>
            <Value>CA|US|SG|DE|NL</Value>
          </AddInfo>
        </AddInfoCollection>
        <EntryNumberCollection>
          <EntryNumber>
            <Number>ITN002, ITN003, ITN001</Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>DUE002, DUE001</Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number>UCR002, UCR001</Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignor additional info</AdditionalAddressInformation>
            <Address1>CONSIGNOR ADDRESS LINE 1</Address1>
            <Address2>CONSIGNOR ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNOR CITY</City>
            <CompanyName>CONSIGNOR</CompanyName>
            <Contact>Consignor contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignor email</Email>
            <Fax>Consignor fax</Fax>
            <GovRegNum>Consignor tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignor phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNOR </Postcode>
            <State>CONSIGNOR STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
            <Address1>CONSIGNEE ADDRESS LINE 1</Address1>
            <Address2>CONSIGNEE ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>CONSIGNEE CITY</City>
            <CompanyName>CONSIGNEE</CompanyName>
            <Contact>Consignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Consignee email</Email>
            <Fax>Consignee fax</Fax>
            <GovRegNum>Consignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Consignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>CONSIGNEE </Postcode>
            <State>CONSIGNEE STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorPickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
            <Address1>PICKUPFROM ADDRESS LINE 1</Address1>
            <Address2>PICKUPFROM ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPFROM CITY</City>
            <CompanyName>PICKUPFROM</CompanyName>
            <Contact>PickupFrom contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupFrom email</Email>
            <Fax>PickupFrom fax</Fax>
            <GovRegNum>PickupFrom tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupFrom phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPFROM</Postcode>
            <State>PICKUPFROM STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DepartureCFSAddress</AddressType>
            <AdditionalAddressInformation>PickupCFS additional info</AdditionalAddressInformation>
            <Address1>PICKUPCFS ADDRESS LINE 1</Address1>
            <Address2>PICKUPCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>PICKUPCFS CITY</City>
            <CompanyName>PICKUPCFS</CompanyName>
            <Contact>PickupCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>PickupCFS email</Email>
            <Fax>PickupCFS fax</Fax>
            <GovRegNum>PickupCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>PickupCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>PICKUPCFS </Postcode>
            <State>PICKUPCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsigneePickupDeliveryAddress</AddressType>
            <AdditionalAddressInformation>DeliveryTo additional info</AdditionalAddressInformation>
            <Address1>DELIVERYTO ADDRESS LINE 1</Address1>
            <Address2>DELIVERYTO ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYTO CITY</City>
            <CompanyName>DELIVERYTO</CompanyName>
            <Contact>DeliveryTo contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryTo email</Email>
            <Fax>DeliveryTo fax</Fax>
            <GovRegNum>DeliveryTo tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryTo phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYTO</Postcode>
            <State>DELIVERYTO STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ArrivalCFSAddress</AddressType>
            <AdditionalAddressInformation>DeliveryCFS additional info</AdditionalAddressInformation>
            <Address1>DELIVERYCFS ADDRESS LINE 1</Address1>
            <Address2>DELIVERYCFS ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DELIVERYCFS CITY</City>
            <CompanyName>DELIVERYCFS</CompanyName>
            <Contact>DeliveryCFS contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryCFS email</Email>
            <Fax>DeliveryCFS fax</Fax>
            <GovRegNum>DeliveryCFS tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryCFS phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DELIVERYCF</Postcode>
            <State>DELIVERYCFS STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY CITY</City>
            <CompanyName>NOTIFYPARTY</CompanyName>
            <Contact>NotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty email</Email>
            <Fax>NotifyParty fax</Fax>
            <GovRegNum>NotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY2 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY2 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY2 CITY</City>
            <CompanyName>NOTIFYPARTY2</CompanyName>
            <Contact>NotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty2 email</Email>
            <Fax>NotifyParty2 fax</Fax>
            <GovRegNum>NotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY2 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty3</AddressType>
            <AdditionalAddressInformation>NotifyParty3 additional info</AdditionalAddressInformation>
            <Address1>NOTIFYPARTY3 ADDRESS LINE 1</Address1>
            <Address2>NOTIFYPARTY3 ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NOTIFYPARTY3 CITY</City>
            <CompanyName>NOTIFYPARTY3</CompanyName>
            <Contact>NotifyParty3 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>NotifyParty3 email</Email>
            <Fax>NotifyParty3 fax</Fax>
            <GovRegNum>NotifyParty3 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>NotifyParty3 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>NOTIFYPART</Postcode>
            <State>NOTIFYPARTY3 STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>SupplierDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
            <Address1>SUPPLIER ADDRESS LINE 1</Address1>
            <Address2>SUPPLIER ADDRESS LINE 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SUPPLIER CITY</City>
            <CompanyName>SUPPLIER</CompanyName>
            <Contact>Supplier contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>Supplier email</Email>
            <Fax>Supplier fax</Fax>
            <GovRegNum>Supplier tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>Supplier phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SUPPLIER P</Postcode>
            <State>SUPPLIER STATE</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>EOR</Type>
                <CountryOfIssue Name=""United Kingdom"">GB</CountryOfIssue>
                <Value>GB12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <DetailedDescription>AAA packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test002</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <ContainerNumber>AAA</ContainerNumber>
                <DetailedDescription>AAA packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>AAA packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test002</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN001, ITN002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE001</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR001</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test003</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>1</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>-1</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN001, ITN002</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE001</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR001</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB packline 1</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB packline 1</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test003</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>1</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>-1</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN001, ITN002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE001</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR001</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID>Test004</PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>
            <AddInfoCollection>
              <AddInfo>
                <Key>ITN</Key>
                <Value>ITN002, ITN003</Value>
              </AddInfo>
              <AddInfo>
                <Key>DUE</Key>
                <Value>DUE002</Value>
              </AddInfo>
              <AddInfo>
                <Key>UCR</Key>
                <Value>UCR002</Value>
              </AddInfo>
            </AddInfoCollection>
            <ClassificationCollection>
              <Classification>
                <Code>HC12345</Code>
                <Country></Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
              <Classification>
                <Code>1</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>2</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
              <Classification>
                <Code>5</Code>
                <Type Description=""ECICS"">CUS</Type>
              </Classification>
            </ClassificationCollection>
            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>2</ContainerLink>
                <ContainerNumber>BBB</ContainerNumber>
                <DetailedDescription>BBB packline 2</DetailedDescription>
                <ExportReferenceNumber></ExportReferenceNumber>
                <GoodsDescription>BBB packline 2</GoodsDescription>
                <HarmonisedCode>HC12345</HarmonisedCode>
                <Height>56</Height>
                <ImportReferenceNumber>import reference number</ImportReferenceNumber>
                <Length>58</Length>
                <LengthUnit>M</LengthUnit>
                <MarksAndNos>marks &amp; nums</MarksAndNos>
                <OutturnComment></OutturnComment>
                <PackingLineID>Test004</PackingLineID>
                <PackQty>3</PackQty>
                <PackType Description=""Pallet"">PLT</PackType>
                <ReferenceNumber>reference number</ReferenceNumber>
                <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
                <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
                <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
                <RequiresTemperatureControl>true</RequiresTemperatureControl>
                <Volume>55</Volume>
                <VolumeUnit>M3</VolumeUnit>
                <Weight>88</Weight>
                <WeightUnit>KG</WeightUnit>
                <Width>57</Width>
                <AddInfoCollection>
                  <AddInfo>
                    <Key>ITN</Key>
                    <Value>ITN002, ITN003</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>DUE</Key>
                    <Value>DUE002</Value>
                  </AddInfo>
                  <AddInfo>
                    <Key>UCR</Key>
                    <Value>UCR002</Value>
                  </AddInfo>
                </AddInfoCollection>
                <ClassificationCollection>
                  <Classification>
                    <Code>HC12345</Code>
                    <Country></Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1234.56</Code>
                    <Country Name=""China"">CN</Country>
                    <Type Description=""Harmonized Code"">HSC</Type>
                  </Classification>
                  <Classification>
                    <Code>1</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>2</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                  <Classification>
                    <Code>5</Code>
                    <Type Description=""ECICS"">CUS</Type>
                  </Classification>
                </ClassificationCollection>
                <UNDGCollection>
                  <UNDG>
{flashPoint?.PadLeft(47, ' ')}
                    <IMOClass>A</IMOClass>
                    <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                    <PackingGroup></PackingGroup>
                    <PackQty>11</PackQty>
                    <ProperShippingName>Danger</ProperShippingName>
                    <Standard>IAT</Standard>
                    <SubLabel1></SubLabel1>
                    <SubLabel2></SubLabel2>
                    <TechicalName>Technicals</TechicalName>
                    <UNDGCode>0001C</UNDGCode>
                  </UNDG>
                </UNDGCollection>
              </PackingLine>
            </PackingLineCollection>
            <UNDGCollection>
              <UNDG>
{flashPoint?.PadLeft(43, ' ')}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IAT</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001C</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-12-01T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff>2019-08-01T00:00:00</LCLCutOff>
        <LCLReceivalCommences>2019-08-02T00:00:00</LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>12345</VesselLloydsIMO>
        <VesselName>Titanic</VesselName>
        <VoyageFlightNo>1234567</VoyageFlightNo>
        <AdditionalTransportModeCollection>
          <AdditionalTransportMode>
            <TransportMode>Road</TransportMode>
          </AdditionalTransportMode>
        </AdditionalTransportModeCollection>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;

		#endregion
	}
}
