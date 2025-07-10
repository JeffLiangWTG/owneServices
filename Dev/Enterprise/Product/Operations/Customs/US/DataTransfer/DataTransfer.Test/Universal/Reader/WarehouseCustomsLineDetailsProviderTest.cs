using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsProviderTest : TestCaseWithFactory
	{
		public void TestIWarehouseCustomsLineDetailsMembersForEntrySummary()
		{
			CombineAssertions(() =>
			{
				var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(Shipment);
				var lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				var line1 = lines[0];
				AssertEquals(EntryLine2.LineNumber, line1.EntryLineNumber);
				AssertEquals("XJ5-ENT328434", line1.EntryNumber);
				AssertNull(line1.PreviousEntryLineNumber);
				AssertNull(line1.PreviousEntryNumber);
				AssertNull("ZoneStatus", line1.ZoneStatus);
				AssertNull("FromOtherFTZ", line1.FromOtherFTZ);
				AssertNull("OutwardType", line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(2, line1PackDetails.Count);
				var line1PackDetail1 = line1PackDetails[0];
				AssertEquals("1", line1PackDetail1.PackID);
				AssertEquals(10, line1PackDetail1.PackageQty);
				AssertEquals(50m, line1PackDetail1.PackedQty);
				var line1PackDetail2 = line1PackDetails[1];
				AssertEquals("2", line1PackDetail2.PackID);
				AssertEquals(3, line1PackDetail2.PackageQty);
				AssertEquals(6m, line1PackDetail2.PackedQty);

				var line2 = lines[1];
				AssertEquals(EntryLine1.LineNumber, line2.EntryLineNumber);
				AssertEquals("XJ5-ENT328434", line2.EntryNumber);
				AssertNull(line2.PreviousEntryLineNumber);
				AssertNull(line2.PreviousEntryNumber);
				AssertNull("ZoneStatus", line2.ZoneStatus);
				AssertNull("FromOtherFTZ", line2.FromOtherFTZ);
				AssertNull("OutwardType", line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(2, line2PackDetails.Count);
				var line2PackDetail1 = line2PackDetails[0];
				AssertEquals("1", line2PackDetail1.PackID);
				AssertEquals(10, line2PackDetail1.PackageQty);
				AssertEquals(60m, line2PackDetail1.PackedQty);
				var line2PackDetail2 = line2PackDetails[1];
				AssertEquals("2", line2PackDetail2.PackID);
				AssertEquals(3, line2PackDetail2.PackageQty);
				AssertEquals(9m, line2PackDetail2.PackedQty);
			});
		}

		public void TestIWarehouseCustomsLineDetailsMembersForImportByExternalBroker()
		{
			CombineAssertions(() =>
			{
				Shipment.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.ImportByExternalBroker };
				var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(Shipment);
				var lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				var line1 = lines[0];
				AssertEquals((ZShort?)4, line1.EntryLineNumber);
				AssertEquals("XJ5-ENT328434", line1.EntryNumber);
				AssertNull(line1.PreviousEntryLineNumber);
				AssertNull(line1.PreviousEntryNumber);
				AssertNull("ZoneStatus", line1.ZoneStatus);
				AssertNull("FromOtherFTZ", line1.FromOtherFTZ);
				AssertNull("OutwardType", line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(2, line1PackDetails.Count);
				var line1PackDetail1 = line1PackDetails[0];
				AssertEquals("1", line1PackDetail1.PackID);
				AssertEquals(10, line1PackDetail1.PackageQty);
				AssertEquals(50m, line1PackDetail1.PackedQty);
				var line1PackDetail2 = line1PackDetails[1];
				AssertEquals("2", line1PackDetail2.PackID);
				AssertEquals(3, line1PackDetail2.PackageQty);
				AssertEquals(6m, line1PackDetail2.PackedQty);

				var line2 = lines[1];
				AssertEquals((ZShort?)6, line2.EntryLineNumber);
				AssertEquals("XJ5-ENT328434", line2.EntryNumber);
				AssertNull(line2.PreviousEntryLineNumber);
				AssertNull(line2.PreviousEntryNumber);
				AssertNull("ZoneStatus", line2.ZoneStatus);
				AssertNull("FromOtherFTZ", line2.FromOtherFTZ);
				AssertNull("OutwardType", line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(2, line2PackDetails.Count);
				var line2PackDetail1 = line2PackDetails[0];
				AssertEquals("1", line2PackDetail1.PackID);
				AssertEquals(10, line2PackDetail1.PackageQty);
				AssertEquals(60m, line2PackDetail1.PackedQty);
				var line2PackDetail2 = line2PackDetails[1];
				AssertEquals("2", line2PackDetail2.PackID);
				AssertEquals(3, line2PackDetail2.PackageQty);
				AssertEquals(9m, line2PackDetail2.PackedQty);
			});
		}

		public void TestIWarehouseCustomsLineDetailsMembersForAdmissionFTZ()
		{
			CombineAssertions(() =>
			{
				Shipment.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.FTZ };
				var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(Shipment);
				var lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				var line1 = lines[0];
				AssertEquals((ZShort?)2, line1.EntryLineNumber);
				AssertEquals("832545315FTZ32342", line1.EntryNumber);
				AssertNull(line1.PreviousEntryLineNumber);
				AssertNull(line1.PreviousEntryNumber);
				AssertEquals("ZoneStatus", ZoneStatusList.Codes.Domestic, line1.ZoneStatus);
				AssertEquals("FromOtherFTZ", ZBool.True, line1.FromOtherFTZ);
				AssertNull("OutwardType", line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(2, line1PackDetails.Count);
				var line1PackDetail1 = line1PackDetails[0];
				AssertEquals("1", line1PackDetail1.PackID);
				AssertEquals(10, line1PackDetail1.PackageQty);
				AssertEquals(50m, line1PackDetail1.PackedQty);
				var line1PackDetail2 = line1PackDetails[1];
				AssertEquals("2", line1PackDetail2.PackID);
				AssertEquals(3, line1PackDetail2.PackageQty);
				AssertEquals(6m, line1PackDetail2.PackedQty);

				var line2 = lines[1];
				AssertEquals((ZShort?)1, line2.EntryLineNumber);
				AssertEquals("832545315FTZ32342", line2.EntryNumber);
				AssertNull(line2.PreviousEntryLineNumber);
				AssertNull(line2.PreviousEntryNumber);
				AssertEquals("ZoneStatus", ZoneStatusList.Codes.PrivilegedForeign, line2.ZoneStatus);
				AssertEquals("FromOtherFTZ", ZBool.True, line2.FromOtherFTZ);
				AssertNull("OutwardType", line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(2, line2PackDetails.Count);
				var line2PackDetail1 = line2PackDetails[0];
				AssertEquals("1", line2PackDetail1.PackID);
				AssertEquals(10, line2PackDetail1.PackageQty);
				AssertEquals(60m, line2PackDetail1.PackedQty);
				var line2PackDetail2 = line2PackDetails[1];
				AssertEquals("2", line2PackDetail2.PackID);
				AssertEquals(3, line2PackDetail2.PackageQty);
				AssertEquals(9m, line2PackDetail2.PackedQty);

				AdmissionTypeAddInfo.Value = ZoneStatusList.Codes.Domestic;
				provider = new WarehouseCustomsLineDetailsProvider(Shipment);
				lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				line1 = lines[0];
				AssertEquals((ZShort?)2, line1.EntryLineNumber);
				AssertEquals("832545315FTZ32342", line1.EntryNumber);
				AssertNull(line1.PreviousEntryLineNumber);
				AssertNull(line1.PreviousEntryNumber);
				AssertEquals("ZoneStatus", ZoneStatusList.Codes.Domestic, line1.ZoneStatus);
				AssertEquals("FromOtherFTZ", ZBool.False, line1.FromOtherFTZ);
				AssertNull("OutwardType", line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(2, line1PackDetails.Count);
				line1PackDetail1 = line1PackDetails[0];
				AssertEquals("1", line1PackDetail1.PackID);
				AssertEquals(10, line1PackDetail1.PackageQty);
				AssertEquals(50m, line1PackDetail1.PackedQty);
				line1PackDetail2 = line1PackDetails[1];
				AssertEquals("2", line1PackDetail2.PackID);
				AssertEquals(3, line1PackDetail2.PackageQty);
				AssertEquals(6m, line1PackDetail2.PackedQty);

				line2 = lines[1];
				AssertEquals((ZShort?)1, line2.EntryLineNumber);
				AssertEquals("832545315FTZ32342", line2.EntryNumber);
				AssertNull(line2.PreviousEntryLineNumber);
				AssertNull(line2.PreviousEntryNumber);
				AssertEquals("ZoneStatus", ZoneStatusList.Codes.PrivilegedForeign, line2.ZoneStatus);
				AssertEquals("FromOtherFTZ", ZBool.False, line2.FromOtherFTZ);
				AssertNull("OutwardType", line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(2, line2PackDetails.Count);
				line2PackDetail1 = line2PackDetails[0];
				AssertEquals("1", line2PackDetail1.PackID);
				AssertEquals(10, line2PackDetail1.PackageQty);
				AssertEquals(60m, line2PackDetail1.PackedQty);
				line2PackDetail2 = line2PackDetails[1];
				AssertEquals("2", line2PackDetail2.PackID);
				AssertEquals(3, line2PackDetail2.PackageQty);
				AssertEquals(9m, line2PackDetail2.PackedQty);
			});
		}

		public void TestIWarehouseCustomsLineDetailsMembersForConsumptionFTZ()
		{
			CombineAssertions(() =>
			{
				Shipment.MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import };
				EntryTypeAddInfo.Value = EntryTypeList.Codes.ConsumptionFTZ;
				var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(Shipment);
				var lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				var line1 = lines[0];
				AssertEquals((ZShort?)2, line1.EntryLineNumber);
				AssertEquals("XJ5-ENT328434", line1.EntryNumber);
				AssertEquals((ZShort?)4, line1.PreviousEntryLineNumber);
				AssertEquals("ENT3239987", line1.PreviousEntryNumber);
				AssertNull("ZoneStatus", line1.ZoneStatus);
				AssertNull("FromOtherFTZ", line1.FromOtherFTZ);
				AssertEquals("OutwardType", Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.OutwardType.Consumption, line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(0, line1PackDetails.Count);

				var line2 = lines[1];
				AssertEquals((ZShort?)1, line2.EntryLineNumber);
				AssertEquals("XJ5-ENT328434", line2.EntryNumber);
				AssertEquals((ZShort?)6, line2.PreviousEntryLineNumber);
				AssertEquals("NDK3233", line2.PreviousEntryNumber);
				AssertNull("ZoneStatus", line2.ZoneStatus);
				AssertNull("FromOtherFTZ", line2.FromOtherFTZ);
				AssertEquals("OutwardType", Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.OutwardType.Consumption, line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(0, line2PackDetails.Count);
			});
		}

		public void TestIWarehouseCustomsLineDetailsMembersForInBond()
		{
			CombineAssertions(() =>
			{
				Shipment.MessageType = new CodeDescriptionPair() { Code = CusInBondApplicationCodeList.Codes.InBond };
				var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(Shipment);
				var lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				var line1 = lines[0];
				AssertEquals((ZShort?)2, line1.EntryLineNumber);
				AssertEquals("INB499345", line1.EntryNumber);
				AssertEquals((ZShort?)4, line1.PreviousEntryLineNumber);
				AssertEquals("INV-ENT3239987", line1.PreviousEntryNumber);
				AssertNull("ZoneStatus", line1.ZoneStatus);
				AssertNull("FromOtherFTZ", line1.FromOtherFTZ);
				AssertEquals("OutwardType", Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.OutwardType.Exports, line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				var line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(0, line1PackDetails.Count);

				var line2 = lines[1];
				AssertEquals((ZShort?)1, line2.EntryLineNumber);
				AssertEquals("INB499345", line2.EntryNumber);
				AssertEquals((ZShort?)6, line2.PreviousEntryLineNumber);
				AssertEquals("NDK3233", line2.PreviousEntryNumber);
				AssertNull("ZoneStatus", line2.ZoneStatus);
				AssertNull("FromOtherFTZ", line2.FromOtherFTZ);
				AssertEquals("OutwardType", Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.OutwardType.Exports, line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				var line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(0, line2PackDetails.Count);

				Shipment.InBondMoveHeaderCollection[0].EntryType.Code = InbondCommonTypeList.Codes._3ImmediateExport;
				provider = new WarehouseCustomsLineDetailsProvider(Shipment);
				lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				line1 = lines[0];
				AssertEquals((ZShort?)2, line1.EntryLineNumber);
				AssertEquals("INB499345", line1.EntryNumber);
				AssertEquals((ZShort?)4, line1.PreviousEntryLineNumber);
				AssertEquals("INV-ENT3239987", line1.PreviousEntryNumber);
				AssertNull("ZoneStatus", line1.ZoneStatus);
				AssertNull("FromOtherFTZ", line1.FromOtherFTZ);
				AssertEquals("OutwardType", Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.OutwardType.Exports, line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(0, line1PackDetails.Count);

				line2 = lines[1];
				AssertEquals((ZShort?)1, line2.EntryLineNumber);
				AssertEquals("INB499345", line2.EntryNumber);
				AssertEquals((ZShort?)6, line2.PreviousEntryLineNumber);
				AssertEquals("NDK3233", line2.PreviousEntryNumber);
				AssertNull("ZoneStatus", line2.ZoneStatus);
				AssertNull("FromOtherFTZ", line2.FromOtherFTZ);
				AssertEquals("OutwardType", Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.OutwardType.Exports, line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(0, line2PackDetails.Count);

				Shipment.InBondMoveHeaderCollection[0].EntryType = null;
				provider = new WarehouseCustomsLineDetailsProvider(Shipment);
				lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				line1 = lines[0];
				AssertEquals((ZShort?)2, line1.EntryLineNumber);
				AssertEquals("INB499345", line1.EntryNumber);
				AssertEquals((ZShort?)4, line1.PreviousEntryLineNumber);
				AssertEquals("INV-ENT3239987", line1.PreviousEntryNumber);
				AssertNull("ZoneStatus", line1.ZoneStatus);
				AssertNull("FromOtherFTZ", line1.FromOtherFTZ);
				AssertEquals("OutwardType", Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.OutwardType.ToOtherFTZ, line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(0, line1PackDetails.Count);

				line2 = lines[1];
				AssertEquals((ZShort?)1, line2.EntryLineNumber);
				AssertEquals("INB499345", line2.EntryNumber);
				AssertEquals((ZShort?)6, line2.PreviousEntryLineNumber);
				AssertEquals("NDK3233", line2.PreviousEntryNumber);
				AssertNull("ZoneStatus", line2.ZoneStatus);
				AssertNull("FromOtherFTZ", line2.FromOtherFTZ);
				AssertEquals("OutwardType", Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.OutwardType.ToOtherFTZ, line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(0, line2PackDetails.Count);

				Shipment.InBondMoveHeaderCollection[0].MoveToFTZ.Code = YesNoDefaultList.Codes.No;
				provider = new WarehouseCustomsLineDetailsProvider(Shipment);
				lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				line1 = lines[0];
				AssertEquals((ZShort?)2, line1.EntryLineNumber);
				AssertEquals("INB499345", line1.EntryNumber);
				AssertEquals((ZShort?)4, line1.PreviousEntryLineNumber);
				AssertEquals("INV-ENT3239987", line1.PreviousEntryNumber);
				AssertNull("ZoneStatus", line1.ZoneStatus);
				AssertNull("FromOtherFTZ", line1.FromOtherFTZ);
				AssertNull("OutwardType", line1.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN1", line1.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine1, line1.InvoiceLine);
				line1PackDetails = new List<IWarehouseCustomsLinePackDetails>(line1.PackDetails);
				AssertEquals(0, line1PackDetails.Count);

				line2 = lines[1];
				AssertEquals((ZShort?)1, line2.EntryLineNumber);
				AssertEquals("INB499345", line2.EntryNumber);
				AssertEquals((ZShort?)6, line2.PreviousEntryLineNumber);
				AssertEquals("NDK3233", line2.PreviousEntryNumber);
				AssertNull("ZoneStatus", line2.ZoneStatus);
				AssertNull("FromOtherFTZ", line2.FromOtherFTZ);
				AssertNull("OutwardType", line2.OutwardType);
				AssertEquals("ManufacturerAddress.OrganizationCode", "MAN2", line2.ManufacturerAddress.OrganizationCode);
				AssertEquals(InvoiceLine2, line2.InvoiceLine);
				line2PackDetails = new List<IWarehouseCustomsLinePackDetails>(line2.PackDetails);
				AssertEquals(0, line2PackDetails.Count);
			});
		}

		public void TestSupplierAddressIsCorrectyReturn()
		{
			CombineAssertions(() =>
			{
				var provider = (IWarehouseCustomsLineDetailsProvider)new WarehouseCustomsLineDetailsProvider(Shipment);
				var lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				var line1 = lines[0];
				AssertNull(line1.SupplierAddress);
				var line2 = lines[1];
				AssertNull(line2.SupplierAddress);
				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
				var org1 = Factory.New<OrgHeader>();
				var org1AddressData = Shipment.AddOrgAddress(writeManager, org1, nameof(DocAddressType.SupplierDocumentaryAddress));
				provider = new WarehouseCustomsLineDetailsProvider(Shipment);
				lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				line1 = lines[0];
				AssertEquals(org1AddressData, line1.SupplierAddress);
				line2 = lines[1];
				AssertEquals(org1AddressData, line2.SupplierAddress);

				var invoiceData = Shipment.CommercialInfo.CommercialInvoiceCollection[0];
				invoiceData.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
				var org2 = Factory.New<OrgHeader>();
				var org2AddressData = invoiceData.AddOrgAddress(writeManager, org1, nameof(DocAddressType.ConsignorDocumentaryAddress));
				var org3 = Factory.New<OrgHeader>();
				var org3AddressData = invoiceData.AddOrgAddress(writeManager, org1, nameof(DocAddressType.Consolidator));
				provider = new WarehouseCustomsLineDetailsProvider(Shipment);
				lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				line1 = lines[0];
				AssertEquals(org2AddressData, line1.SupplierAddress);
				line2 = lines[1];
				AssertEquals(org2AddressData, line2.SupplierAddress);

				invoiceData.Supplier = org3AddressData;
				provider = new WarehouseCustomsLineDetailsProvider(Shipment);
				lines = provider.GetLineDetails().OfType<IUSWarehouseCustomsLineDetails>().ToList();
				AssertEquals(2, lines.Count);
				line1 = lines[0];
				AssertEquals(org3AddressData, line1.SupplierAddress);
				line2 = lines[1];
				AssertEquals(org3AddressData, line2.SupplierAddress);
			});
		}

		//10 boxes of 5 dongs and 6 books
		//3 box of 2 dongs and 3 books
		Shipment shipment;
		Shipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
					shipment.SetAddInfoCollection(() => new List<UniversalAddInfo>(new[]
					{
						new UniversalAddInfo() { Key = JobDeclaration.Schema.US_EntryFilerCode.Substring(3), Value = "XJ5" },
						new UniversalAddInfo() { Key = JobDeclaration.Schema.US_EnableENS.Substring(3), Value = "Y" },
						AdmissionTypeAddInfo,
						EntryTypeAddInfo
					}));
					shipment.SetEntryNumberCollection(() => new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>(new[]
					{
						new UniversalDataBuss.DataObjects.Universal.EntryNumber()
						{
							Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
							Number = "ENT98654",
							CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Australia }
						},
						new UniversalDataBuss.DataObjects.Universal.EntryNumber()
						{
							Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone },
							Number = "FTZ986554",
							CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Australia }
						},
						new UniversalDataBuss.DataObjects.Universal.EntryNumber()
						{
							Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.EntrySummary },
							Number = "ENT328434",
							CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.UnitedStates }
						},
						new UniversalDataBuss.DataObjects.Universal.EntryNumber()
						{
							Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone },
							Number = "8325453|15|FTZ32342",
							CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.UnitedStates }
						}
					}));
					shipment.SetEntryHeaderCollection(() => new List<EntryHeader>(new[]
					{
						new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							EntryLineCollection = new List<EntryLine>(new[] { EntryLine1, EntryLine2 })
						}
					}));
					shipment.SetAddInfoGroupCollection(() => new List<AddInfoGroup>(new[]
					{
						new AddInfoGroup()
						{
							Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPack },
							AddInfoCollection = new List<UniversalAddInfo>(new[]
							{
								new UniversalAddInfo() { Key = USWHSPackAddInfo.Schema.US_PackageReference.Substring(3), Value = "1" },
								new UniversalAddInfo() { Key = USWHSPackAddInfo.Schema.US_PackageQty.Substring(3), Value = "10" }
							})
						},
						new AddInfoGroup()
						{
							Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPack },
							AddInfoCollection = new List<UniversalAddInfo>(new[]
							{
								new UniversalAddInfo() { Key = USWHSPackAddInfo.Schema.US_PackageReference.Substring(3), Value = "2" },
								new UniversalAddInfo() { Key = USWHSPackAddInfo.Schema.US_PackageQty.Substring(3), Value = "3" }
							})
						}
					}));
					shipment.CommercialInfo = new CommercialInfo()
					{
						Name = "GROUPINV",
						CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								InvoiceNumber = "INV123",
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[] { InvoiceLine1, InvoiceLine2 })))
						})
					};
					shipment.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>(new[]
					{
						new InBondMoveHeader()
						{
							EntryNumberCollection = new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>(new[]
							{
								new UniversalDataBuss.DataObjects.Universal.EntryNumber() { Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.InBond }, Number = "INB965545", CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.Australia } },
								new UniversalDataBuss.DataObjects.Universal.EntryNumber() { Type = new EntryType() { Code = CusEntryHeaderMessageTypeList.Codes.InBond }, Number = "INB499345", CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.UnitedStates }  }
							}),
							EntryType = new CodeDescriptionPair9Char() { Code = InbondCommonTypeList.Codes._2TransportandExport },
							MoveToFTZ = new CodeDescriptionPair1Char() { Code = YesNoDefaultList.Codes.Yes }
						}
					}));
				}
				return shipment;
			}
		}

		EntryLine entryLine1;
		EntryLine EntryLine1 => entryLine1 ?? (entryLine1 = new EntryLine() { LineNumber = 1 });

		EntryLine entryLine2;
		EntryLine EntryLine2 => entryLine2 ?? (entryLine2 = new EntryLine() { LineNumber = 2 });

		UniversalAddInfo entryTypeAddInfo;
		UniversalAddInfo EntryTypeAddInfo => entryTypeAddInfo ?? (entryTypeAddInfo = new UniversalAddInfo() { Key = JobDeclaration.Schema.US_EntryType.Substring(3) });

		UniversalAddInfo admissionTypeAddInfo;
		UniversalAddInfo AdmissionTypeAddInfo => admissionTypeAddInfo ?? (admissionTypeAddInfo = new UniversalAddInfo() { Key = JobDeclaration.Schema.US_F_AdmissionType.Substring(3), Value = FTZAdmissionTypeCodeList.Codes.ZoneToZone });

		CommercialInvoiceLine invoiceLine1;
		CommercialInvoiceLine InvoiceLine1
		{
			get
			{
				if (invoiceLine1 == null)
				{
					invoiceLine1 = new CommercialInvoiceLine()
					{
						LineNo = 1,
						EntryLineNumber = 2,
						EntryNumber = "ENT328434",
						Description = "DONGS",
						BondedWarehouseQuantity = 56,
						AddInfoGroupCollection = new List<AddInfoGroup>(new[]
										{
											new AddInfoGroup()
											{
												Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine },
												AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=1*{1}=50", CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty))
											},
											new AddInfoGroup()
											{
												Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine },
												AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=2*{1}=6", CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty))
											}
										}),
						AddInfoCollection = new List<UniversalAddInfo>(new[]
						{
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3), Value = "4" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_WHSEntryNumber.Substring(3), Value = "ENT3239987" },
							new UniversalAddInfo() { Key = JobDeclaration.Schema.US_WHSEntryFilerCode.Substring(3), Value = "INV" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3), Value = ZoneStatusList.Codes.Domestic }
						}),
						OrganizationAddressCollection = new List<OrganizationAddress>(new[]
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeAddress), OrganizationCode = "ABC32" },
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.Manufacturer), OrganizationCode = "MAN1" }
						})
					};
				}
				return invoiceLine1;
			}
		}

		CommercialInvoiceLine invoiceLine2;
		CommercialInvoiceLine InvoiceLine2
		{
			get
			{
				if (invoiceLine2 == null)
				{
					invoiceLine2 = new CommercialInvoiceLine()
					{
						LineNo = 2,
						EntryLineNumber = 1,
						Description = "BOOKS",
						BondedWarehouseQuantity = 69,
						AddInfoGroupCollection = new List<AddInfoGroup>(new[]
										{
											new AddInfoGroup()
											{
												Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine },
												AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=1*{1}=60", CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty))
											},
											new AddInfoGroup()
											{
												Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USWHSPackLine },
												AddInfoCollection = Enterprise.Customs.DataTransfer.Universal.AddInfoCollectionCreator.CreateCollection(string.Format("{0}=2*{1}=9", CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackageID, CusAddInfoTypeListProvider.AdditionalAddInfoType.InvoiceLinePackData.PackedQty))
											}
										}),
						AddInfoCollection = new List<UniversalAddInfo>(new[]
						{
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_WHSEntryLineNo.Substring(3), Value = "6" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_WHSEntryNumber.Substring(3), Value = "NDK3233" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.US_ZoneStatus.Substring(3), Value = ZoneStatusList.Codes.PrivilegedForeign }
						}),
						OrganizationAddressCollection = new List<OrganizationAddress>(new[]
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsigneeAddress), OrganizationCode = "ABC32" },
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.Manufacturer), OrganizationCode = "MAN2" }
						})
					};
				}
				return invoiceLine2;
			}
		}
	}
}
