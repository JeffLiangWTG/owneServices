using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	sealed class LineReleaseDeclarationCreatorTest : MessageBuilderTestCase
	{
		public void TestTwoTariffNumbers()
		{
			OrgHeader manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_IsConsignor = true;
			manufacturer.OH_FullName = "Mr Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XOHONCAN715SCA");

			OrgHeader importer = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			importer.OH_IsConsignee = true;
			importer.OH_IsShippingLine = true;
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			helper.UpdateOrAddCustomsRegNo(importer, "95-204100600", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);

			OrgHeader billIssuer = Factory.NewWithValidTestData<OrgHeader>();
			billIssuer.OH_IsShippingProvider = true;
			billIssuer.OH_FullName = "Mr Bill Issuer";
			billIssuer.OH_Code = "ISS" + new Random().Next(1000000).ToString();
			billIssuer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CPRS");

			StringBuilder messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008888XJ5 750215401117070003L84".PadRight(80));
			messageTextBuilder.Append("X20870321    870390    HON2AMED112AUUCA        00000003NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X20870421    870490    HON2AMED112TRKCA        00000007NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X25CPRS073197847799                00000010                21".PadRight(80));
			messageTextBuilder.Append("X400001000100000000".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00005".PadRight(80));
			LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "75021540";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			Factory.Save();

			JobDeclaration declaration = new LineReleaseDeclarationCreator(message).CreateANewDeclaration();
			AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("JE_TransportMode", TransportTypeList.Codes.Rail, declaration.JE_TransportMode);
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
			AssertEquals("US_EntryFilerCode", "XJ5", declaration.US_EntryFilerCode);
			AssertEquals("US_SchDEntry", "8888", declaration.US_SchDEntry);
			AssertEquals("US_SchDArrival", "8888", declaration.US_SchDArrival);
			AssertEquals("JE_OH_Importer", importer.PK, declaration.JE_OH_Importer);
			AssertEquals("JE_OH_ShippingLine", importer.PK, declaration.JE_OH_ShippingLine);
			AssertEquals("US_US_NKLocationOfGoods", "L84", declaration.US_US_NKLocationOfGoods);
			AssertEquals("IOROrgPK", importer.PK, declaration.IOROrgPK);
			AssertEquals("JE_MasterBill", "073197847799", declaration.JE_MasterBill);
			AssertEquals("ImportEntryNumber", "75021540", declaration.ImportEntryNumber);
			AssertEquals("declaration.Bills", 1, declaration.Bills.Count);
			AssertEquals("declaration.PackingGroups", 1, declaration.PackingGroups.Count);
			AssertEquals("declaration.Packages", 1, declaration.Packages.Count);
			AssertEquals("declaration.Invoices", 1, declaration.Invoices.Count);
			AssertEquals("declaration.InvoiceLines", 4, declaration.InvoiceLines.Count);
			AssertEquals("declaration.CustomsEntryHeaders", 2, declaration.CustomsEntryHeaders.Count);

			Bill masterBill = declaration.Bills[0];
			AssertEquals("CU_BillNum", "073197847799", masterBill.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 10m, masterBill.CU_NoOfPacks);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.MasterBill, masterBill.CU_BillType);

			AssertEquals("PackingGroups", 1, masterBill.PackingGroups.Count);
			PackingGroup packingGroup = masterBill.PackingGroups[0];
			AssertEquals("Packages", 1, packingGroup.Packages.Count);
			Package package = packingGroup.Packages[0];
			AssertEquals("CW_PackQty", 10, package.CW_PackQty);

			JobComInvoiceHeader invoice = declaration.Invoices[0];
			AssertEquals("JZ_InvoiceNumber", "", invoice.JZ_InvoiceNumber);

			AssertEquals("InvoiceLines", 4, invoice.JobComInvoiceLines.Count);
			JobComInvoiceLine line1 = null;
			JobComInvoiceLine line2 = null;
			JobComInvoiceLine line3 = null;
			JobComInvoiceLine line4 = null;
			foreach (JobComInvoiceLine line in invoice.JobComInvoiceLines)
			{
				switch (line.JI_Tariff)
				{
					case "870321":
						line1 = line;
						break;
					case "870390":
						line2 = line;
						break;
					case "870421":
						line3 = line;
						break;
					case "870490":
						line4 = line;
						break;
					default:
						break;
				}
			}

			AssertNotNull(line1);
			AssertEquals("JI_Tariff", "870321", line1.JI_Tariff);
			AssertEquals("IsParentLine", true, line1.IsParentLine);
			AssertEquals("JI_InvoiceQuantity", 3m, line1.JI_InvoiceQuantity);
			AssertEquals("JI_InvoiceUQ", "NUM", line1.JI_InvoiceUQ);
			AssertEquals("US_UC_NKCountryOfOrigin", "CA", line1.US_UC_NKCountryOfOrigin);
			AssertEquals("US_UC_NKCountryOfExport", "CA", line1.US_UC_NKCountryOfExport);
			AssertEquals("ManufacturerAddress", manufacturer.MainAddress.PK, line1.JI_OA_ManufacturerAddress);
			AssertEquals("Child Lines", 1, line1.ChildLines.Count());
			AssertEquals("Line1's child line", line2, line1.ChildLines.ElementAt(0));

			AssertEquals("JI_Tariff", "870390", line2.JI_Tariff);
			AssertEquals("IsSecondaryTariffLine", true, line2.IsSecondaryTariffLine);
			AssertEquals("Child Lines", 0, line2.ChildLines.Count());

			AssertNotNull(line3);
			AssertEquals("JI_Tariff", "870421", line3.JI_Tariff);
			AssertEquals("IsParentLine", true, line3.IsParentLine);
			AssertEquals("JI_InvoiceQuantity", 7m, line3.JI_InvoiceQuantity);
			AssertEquals("JI_InvoiceUQ", "NUM", line3.JI_InvoiceUQ);
			AssertEquals("US_UC_NKCountryOfOrigin", "CA", line3.US_UC_NKCountryOfOrigin);
			AssertEquals("US_UC_NKCountryOfExport", "CA", line3.US_UC_NKCountryOfExport);
			AssertEquals("ManufacturerAddress", manufacturer.MainAddress.PK, line3.JI_OA_ManufacturerAddress);
			AssertEquals("Child Lines", 1, line3.ChildLines.Count());
			AssertEquals("line3's child line", line4, line3.ChildLines.ElementAt(0));

			AssertEquals("JI_Tariff", "870490", line4.JI_Tariff);
			AssertEquals("IsSecondaryTariffLine", true, line4.IsSecondaryTariffLine);
			AssertEquals("Child Lines", 0, line4.ChildLines.Count());
			AssertEquals("Entry Release Date", new ZDateTime(2007, 11, 17, 0, 3, 0), declaration.JE_EntryAuthorisationDate);

			LineReleaseMQEDIMessage reloadedMessage = declaration.Factory.Load<LineReleaseMQEDIMessage>(message.PK);
			AssertEquals("IsComplete", true, reloadedMessage.IsComplete);
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "The Entry Filer Code '432' does not belong to this company.\r\nPlease check Admin -> System -> Registry -> Customs -> Country or Region Specific -> United States of America -> Import -> ABI -> Entry Filer")]
		public void TestNoEntryFilerMatched()
		{
			StringBuilder messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018823432XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008823432 178237361206051019L89".PadRight(80));
			messageTextBuilder.Append("X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X25CPRS053390088346CPRS23423343343400000008CPRS634345455434".PadRight(80));
			messageTextBuilder.Append("X25UPRR106697071114UPRR72345345245600000007".PadRight(80));
			messageTextBuilder.Append("X400001000100000002".PadRight(80));
			messageTextBuilder.Append("Y018823432XR00006".PadRight(80));
			LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "17823736";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			Factory.Save();
			new LineReleaseDeclarationCreator(message);
		}

		public void TestDifferentBillLevel()
		{
			OrgHeader billIssuer1 = Factory.NewWithValidTestData<OrgHeader>();
			billIssuer1.OH_IsShippingProvider = true;
			billIssuer1.OH_FullName = "Mr Bill Issuer 1";
			billIssuer1.OH_Code = "ISS" + new Random().Next(1000000).ToString();
			billIssuer1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "UPRR");

			OrgHeader billIssuer2 = Factory.NewWithValidTestData<OrgHeader>();
			billIssuer2.OH_IsShippingProvider = true;
			billIssuer2.OH_FullName = "Mr Bill Issuer 2";
			billIssuer2.OH_Code = "ISS2" + new Random().Next(1000000).ToString();
			billIssuer2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CPRS");

			StringBuilder messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008888XJ5 165237361206051019L89".PadRight(80));
			messageTextBuilder.Append("X20870421    870490    HON2AMED112TRK01        00000008NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X20870323              HON2AMED112AUU01        00000007NO XOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X25CPRS053390088346CPRS23423343343400000008CPRS634345455434".PadRight(80));
			messageTextBuilder.Append("X25UPRR106697071114UPRR72345345245600000007".PadRight(80));
			messageTextBuilder.Append("X400001000100000002".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00006".PadRight(80));
			LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "16523736";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			Factory.Save();

			JobDeclaration declaration = new LineReleaseDeclarationCreator(message).CreateANewDeclaration();
			AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("JE_TransportMode", TransportTypeList.Codes.Truck, declaration.JE_TransportMode);
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			AssertEquals("US_EntryFilerCode", "XJ5", declaration.US_EntryFilerCode);
			AssertEquals("US_SchDEntry", "8888", declaration.US_SchDEntry);
			AssertEquals("US_SchDArrival", "8888", declaration.US_SchDArrival);
			AssertEquals("JE_OH_Importer", ZGuid.Empty, declaration.JE_OH_Importer);
			AssertEquals("US_UI_NKCarrierSCAC", "CPRS", declaration.US_UI_NKCarrierSCAC);
			AssertEquals("US_US_NKLocationOfGoods", "L89", declaration.US_US_NKLocationOfGoods);
			AssertEquals("IOROrgPK", ZGuid.Empty, declaration.IOROrgPK);
			AssertEquals("ImportEntryNumber", "16523736", declaration.ImportEntryNumber);
			AssertEquals("declaration.Bills", 5, declaration.Bills.Count);
			AssertEquals("declaration.PackingGroups", 3, declaration.PackingGroups.Count);
			AssertEquals("declaration.Packages", 3, declaration.Packages.Count);
			AssertEquals("declaration.Invoices", 2, declaration.Invoices.Count);
			AssertEquals("declaration.InvoiceLines", 4, declaration.InvoiceLines.Count);
			AssertEquals("declaration.CustomsEntryHeaders", 2, declaration.CustomsEntryHeaders.Count);

			Bill bill1 = null;
			Bill bill2 = null;
			Bill bill3 = null;
			Bill bill4 = null;
			Bill bill5 = null;
			foreach (Bill bill in declaration.Bills)
			{
				switch (bill.CU_BillNum)
				{
					case "053390088346":
						bill1 = bill;
						break;
					case "234233433434":
						bill2 = bill;
						break;
					case "634345455434":
						bill3 = bill;
						break;
					case "106697071114":
						bill4 = bill;
						break;
					case "723453452456":
						bill5 = bill;
						break;
					default:
						break;
				}
			}

			AssertNotNull(bill1);
			AssertEquals("CU_BillNum", "053390088346", bill1.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 0m, bill1.CU_NoOfPacks);
			AssertEquals("PackingGroups", 0, bill1.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.MasterBill, bill1.CU_BillType);
			AssertEquals("bill1.ChildBills", 1, bill1.ChildBills.Count);
			AssertCollectionContains("bill1.ChildBills", bill2, bill1.ChildBills);

			AssertNotNull(bill2);
			AssertEquals("CU_BillNum", "234233433434", bill2.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 0m, bill2.CU_NoOfPacks);
			AssertEquals("PackingGroups", 1, bill2.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.HouseBill, bill2.CU_BillType);
			AssertEquals("bill2.ChildBills", 1, bill2.ChildBills.Count);
			AssertCollectionContains("bill2.ChildBills", bill3, bill2.ChildBills);

			AssertNotNull(bill3);
			AssertEquals("CU_BillNum", "634345455434", bill3.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 8m, bill3.CU_NoOfPacks);
			AssertEquals("PackingGroups", 1, bill3.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.SubHouseBill, bill3.CU_BillType);
			AssertEquals("bill3.ChildBills", 0, bill3.ChildBills.Count);

			PackingGroup packingGroup1 = bill3.PackingGroups[0];
			AssertEquals("packingGroup1.Packages", 1, packingGroup1.Packages.Count);
			Package package1 = packingGroup1.Packages[0];
			AssertEquals("package1.CW_PackQty", 8, package1.CW_PackQty);

			JobComInvoiceHeader invoice1 = (JobComInvoiceHeader)(new List<Customs.Business.BaseJobComInvoiceHeader>(declaration.Invoices)[0]);
			AssertEquals("JZ_InvoiceNumber", "", invoice1.JZ_InvoiceNumber);
			AssertEquals("invoice1.JobComInvoiceLines", 3, invoice1.JobComInvoiceLines.Count);

			JobComInvoiceLine line1 = null;
			JobComInvoiceLine line2 = null;
			JobComInvoiceLine line3 = null;
			foreach (JobComInvoiceLine line in invoice1.JobComInvoiceLines)
			{
				switch (line.JI_Tariff)
				{
					case "870421":
						line1 = line;
						break;
					case "870490":
						line2 = line;
						break;
					case "870323":
						line3 = line;
						break;
					default:
						break;
				}
			}

			AssertNotNull(line1);
			AssertEquals("JI_Tariff", "870421", line1.JI_Tariff);
			AssertEquals("IsParentLine", true, line1.IsParentLine);
			AssertEquals("JI_InvoiceQuantity", 8m, line1.JI_InvoiceQuantity);
			AssertEquals("JI_InvoiceUQ", "NUM", line1.JI_InvoiceUQ);
			AssertEquals("US_UC_NKCountryOfOrigin", "01", line1.US_UC_NKCountryOfOrigin);
			AssertEquals("US_UC_NKCountryOfExport", "01", line1.US_UC_NKCountryOfExport);
			AssertEquals("ManufacturerAddress", ZGuid.Empty, line1.JI_OA_ManufacturerAddress);
			AssertEquals("Child Lines", 1, line1.ChildLines.Count());
			AssertEquals("line1's child line", line2, line1.ChildLines.ElementAt(0));

			AssertEquals("JI_Tariff", "870490", line2.JI_Tariff);
			AssertEquals("IsSecondaryTariffLine", true, line2.IsSecondaryTariffLine);
			AssertEquals("Child Lines", 0, line2.ChildLines.Count());

			AssertNotNull(line3);
			AssertEquals("JI_Tariff", "870323", line3.JI_Tariff);
			AssertEquals("IsParentLine", false, line3.IsParentLine);
			AssertEquals("JI_InvoiceQuantity", 7m, line3.JI_InvoiceQuantity);
			AssertEquals("JI_InvoiceUQ", "NO", line3.JI_InvoiceUQ);
			AssertEquals("US_UC_NKCountryOfOrigin", "01", line3.US_UC_NKCountryOfOrigin);
			AssertEquals("US_UC_NKCountryOfExport", "01", line3.US_UC_NKCountryOfExport);
			AssertEquals("ManufacturerAddress", ZGuid.Empty, line3.JI_OA_ManufacturerAddress);

			AssertNotNull(bill4);
			AssertEquals("CU_BillNum", "106697071114", bill4.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 0m, bill4.CU_NoOfPacks);
			AssertEquals("PackingGroups", 0, bill4.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.MasterBill, bill4.CU_BillType);
			AssertEquals("bill4.ChildBills", 1, bill4.ChildBills.Count);
			AssertCollectionContains("bill4.ChildBills", bill5, bill4.ChildBills);

			AssertNotNull(bill5);
			AssertEquals("CU_BillNum", "723453452456", bill5.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 7m, bill5.CU_NoOfPacks);
			AssertEquals("PackingGroups", 1, bill5.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.HouseBill, bill5.CU_BillType);
			AssertEquals("bill5.ChildBills", 0, bill5.ChildBills.Count);

			PackingGroup packingGroup2 = bill5.PackingGroups[0];
			AssertEquals("packingGroup2.Packages", 1, packingGroup2.Packages.Count);
			Package package2 = packingGroup2.Packages[0];
			AssertEquals("package2.CW_PackQty", 7, package2.CW_PackQty);

			JobComInvoiceHeader invoice2 = (JobComInvoiceHeader)(new List<Customs.Business.BaseJobComInvoiceHeader>(declaration.Invoices)[1]);
			AssertEquals("JZ_InvoiceNumber", "", invoice2.JZ_InvoiceNumber);
			AssertEquals("invoice1.JobComInvoiceLines", 1, invoice2.JobComInvoiceLines.Count);

			AssertEquals("Entries", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("EntryReleaseDate", new ZDateTime(2005, 12, 6, 10, 19, 0), declaration.JE_EntryAuthorisationDate);

			LineReleaseMQEDIMessage reloadedMessage = declaration.Factory.Load<LineReleaseMQEDIMessage>(message.PK);
			AssertEquals("IsComplete", true, reloadedMessage.IsComplete);
		}

		public void TestMultipleBills()
		{
			OrgHeader manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_FullName = "Mr Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.OH_IsConsignor = true;
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MXNISMEX1958MEX");

			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Mr Importer";
			importer.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			importer.OH_IsConsignee = true;
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "95-2108010MX");

			OrgHeader billIssuer = Factory.NewWithValidTestData<OrgHeader>();
			billIssuer.OH_IsShippingProvider = true;
			billIssuer.OH_FullName = "Mr Bill Issuer";
			billIssuer.OH_Code = "ISS" + new Random().Next(1000000).ToString();
			billIssuer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "UPRR");

			StringBuilder messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2108010MX8888XJ5 813510831117071346LR3".PadRight(80));
			messageTextBuilder.Append("X20870323              NIS1NIS5231MPVMX        00000030NUMMXNISMEX1958MEX".PadRight(80));
			messageTextBuilder.Append("X20870322              NIS1NIS5231MPVMX        00000029NUMMXNISMEX1958MEX".PadRight(80));
			messageTextBuilder.Append("X25UPRR106695071114                00000015".PadRight(80));
			messageTextBuilder.Append("X25UPRR106697071114                00000015".PadRight(80));
			messageTextBuilder.Append("X25UPRR106733071114                00000015                34".PadRight(80));
			messageTextBuilder.Append("X25UPRR106764071114                00000014".PadRight(80));
			messageTextBuilder.Append("X400001000100000000".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00008".PadRight(80));
			LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "81351083";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			Factory.Save();

			JobDeclaration declaration = new LineReleaseDeclarationCreator(message).CreateANewDeclaration();
			AssertEquals("JE_MessageType", JobMessageTypeList.Codes.Import, declaration.JE_MessageType);
			AssertEquals("JE_TransportMode", TransportTypeList.Codes.Road, declaration.JE_TransportMode);
			AssertEquals("JE_ContainerMode", Core.Constants.ContainerModes.NonContainerised, declaration.JE_ContainerMode);
			AssertEquals("US_EntryFilerCode", "XJ5", declaration.US_EntryFilerCode);
			AssertEquals("US_SchDEntry", "8888", declaration.US_SchDEntry);
			AssertEquals("US_SchDArrival", "8888", declaration.US_SchDArrival);
			AssertEquals("JE_OH_Importer", importer.PK, declaration.JE_OH_Importer);
			AssertEquals("US_UI_NKCarrierSCAC", "UPRR", declaration.US_UI_NKCarrierSCAC);
			AssertEquals("US_US_NKLocationOfGoods", "LR3", declaration.US_US_NKLocationOfGoods);
			AssertEquals("IOROrgPK", importer.PK, declaration.IOROrgPK);
			AssertEquals("ImportEntryNumber", "81351083", declaration.ImportEntryNumber);
			AssertEquals("declaration.Bills", 4, declaration.Bills.Count);
			AssertEquals("declaration.PackingGroups", 4, declaration.PackingGroups.Count);
			AssertEquals("declaration.Packages", 4, declaration.Packages.Count);
			AssertEquals("declaration.Invoices", 4, declaration.Invoices.Count);
			AssertEquals("declaration.InvoiceLines", 5, declaration.InvoiceLines.Count);
			AssertEquals("declaration.JE_TotalNoOfPacks", 59, declaration.JE_TotalNoOfPacks);
			AssertEquals("declaration.CustomsEntryHeaders", 2, declaration.CustomsEntryHeaders.Count);

			Bill bill1 = null;
			Bill bill2 = null;
			Bill bill3 = null;
			Bill bill4 = null;
			foreach (Bill bill in declaration.Bills)
			{
				switch (bill.CU_BillNum)
				{
					case "106695071114":
						bill1 = bill;
						break;
					case "106697071114":
						bill2 = bill;
						break;
					case "106733071114":
						bill3 = bill;
						break;
					case "106764071114":
						bill4 = bill;
						break;
					default:
						break;
				}
			}

			AssertNotNull(bill1);
			AssertEquals("CU_BillNum", "106695071114", bill1.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 15m, bill1.CU_NoOfPacks);
			AssertEquals("PackingGroups", 1, bill1.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.MasterBill, bill1.CU_BillType);
			AssertEquals("bill1.ChildBills", 0, bill1.ChildBills.Count);

			PackingGroup packingGroup1 = bill1.PackingGroups[0];
			AssertEquals("Packages", 1, packingGroup1.Packages.Count);
			Package package1 = packingGroup1.Packages[0];
			AssertEquals("CW_PackQty", 15, package1.CW_PackQty);

			JobComInvoiceHeader invoice1 = (JobComInvoiceHeader)(new List<Customs.Business.BaseJobComInvoiceHeader>(declaration.Invoices)[0]);
			AssertEquals("JZ_InvoiceNumber", "", invoice1.JZ_InvoiceNumber);
			AssertEquals("invoice1.JobComInvoiceLines", 2, invoice1.JobComInvoiceLines.Count);

			JobComInvoiceLine line1 = null;
			JobComInvoiceLine line2 = null;
			foreach (JobComInvoiceLine line in invoice1.JobComInvoiceLines)
			{
				switch (line.JI_Tariff)
				{
					case "870323":
						line1 = line;
						break;
					case "870322":
						line2 = line;
						break;
					default:
						break;
				}
			}

			AssertNotNull(line1);
			AssertEquals("JI_Tariff", "870323", line1.JI_Tariff);
			AssertEquals("IsParentLine", false, line1.IsParentLine);
			AssertEquals("JI_InvoiceQuantity", 30m, line1.JI_InvoiceQuantity);
			AssertEquals("JI_InvoiceUQ", "NUM", line1.JI_InvoiceUQ);
			AssertEquals("US_UC_NKCountryOfOrigin", "MX", line1.US_UC_NKCountryOfOrigin);
			AssertEquals("US_UC_NKCountryOfExport", "MX", line1.US_UC_NKCountryOfExport);
			AssertEquals("ManufacturerAddress", manufacturer.MainAddress.PK, line1.JI_OA_ManufacturerAddress);

			AssertNotNull(line2);
			AssertEquals("JI_Tariff", "870322", line2.JI_Tariff);
			AssertEquals("IsParentLine", false, line2.IsParentLine);
			AssertEquals("JI_InvoiceQuantity", 29m, line2.JI_InvoiceQuantity);
			AssertEquals("JI_InvoiceUQ", "NUM", line2.JI_InvoiceUQ);
			AssertEquals("US_UC_NKCountryOfOrigin", "MX", line2.US_UC_NKCountryOfOrigin);
			AssertEquals("US_UC_NKCountryOfExport", "MX", line2.US_UC_NKCountryOfExport);
			AssertEquals("ManufacturerAddress", manufacturer.MainAddress.PK, line2.JI_OA_ManufacturerAddress);

			AssertNotNull(bill2);
			AssertEquals("CU_BillNum", "106697071114", bill2.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 15m, bill2.CU_NoOfPacks);
			AssertEquals("PackingGroups", 1, bill2.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.MasterBill, bill2.CU_BillType);
			AssertEquals("bill2.ChildBills", 0, bill2.ChildBills.Count);

			PackingGroup packingGroup2 = bill2.PackingGroups[0];
			AssertEquals("packingGroup2.Packages", 1, packingGroup2.Packages.Count);
			Package package2 = packingGroup2.Packages[0];
			AssertEquals("CW_PackQty", 15, package2.CW_PackQty);

			JobComInvoiceHeader invoice2 = (JobComInvoiceHeader)(new List<Customs.Business.BaseJobComInvoiceHeader>(declaration.Invoices)[1]);
			AssertEquals("JZ_InvoiceNumber", "", invoice2.JZ_InvoiceNumber);
			AssertEquals("invoice2.JobComInvoiceLines", 1, invoice2.JobComInvoiceLines.Count);

			AssertNotNull(bill3);
			AssertEquals("CU_BillNum", "106733071114", bill3.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 15m, bill3.CU_NoOfPacks);
			AssertEquals("PackingGroups", 1, bill3.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.MasterBill, bill3.CU_BillType);
			AssertEquals("bill3.ChildBills", 0, bill3.ChildBills.Count);

			PackingGroup packingGroup3 = bill3.PackingGroups[0];
			AssertEquals("packingGroup3.Packages", 1, packingGroup3.Packages.Count);
			Package package3 = packingGroup3.Packages[0];
			AssertEquals("CW_PackQty", 15, package3.CW_PackQty);

			JobComInvoiceHeader invoice3 = (JobComInvoiceHeader)(new List<Customs.Business.BaseJobComInvoiceHeader>(declaration.Invoices)[2]);
			AssertEquals("JZ_InvoiceNumber", "", invoice3.JZ_InvoiceNumber);
			AssertEquals("invoice2.JobComInvoiceLines", 1, invoice3.JobComInvoiceLines.Count);

			AssertNotNull(bill4);
			AssertEquals("CU_BillNum", "106764071114", bill4.CU_BillNum);
			AssertEquals("CU_NoOfPacks", 14m, bill4.CU_NoOfPacks);
			AssertEquals("PackingGroups", 1, bill4.PackingGroups.Count);
			AssertEquals("CU_BillType", Customs.Business.BillTypeList.Codes.MasterBill, bill4.CU_BillType);
			AssertEquals("bill4.ChildBills", 0, bill4.ChildBills.Count);

			PackingGroup packingGroup4 = bill4.PackingGroups[0];
			AssertEquals("Packages", 1, packingGroup4.Packages.Count);
			Package package4 = packingGroup4.Packages[0];
			AssertEquals("CW_PackQty", 14, package4.CW_PackQty);

			JobComInvoiceHeader invoice4 = (JobComInvoiceHeader)(new List<Customs.Business.BaseJobComInvoiceHeader>(declaration.Invoices)[3]);
			AssertEquals("JZ_InvoiceNumber", "", invoice4.JZ_InvoiceNumber);
			AssertEquals("invoice1.JobComInvoiceLines", 1, invoice4.JobComInvoiceLines.Count);

			AssertEquals("Entries", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("EntryReleaseDate", new ZDateTime(2007, 11, 17, 13, 46, 0), declaration.JE_EntryAuthorisationDate);

			LineReleaseMQEDIMessage reloadedMessage = declaration.Factory.Load<LineReleaseMQEDIMessage>(message.PK);
			AssertEquals("IsComplete", true, reloadedMessage.IsComplete);
		}

		[TestDate(2007, 11, 17, 0, 3, 0)]
		public void TestCreation()
		{
			OrgHeader manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_IsConsignor = true;
			manufacturer.OH_FullName = "Mr Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XOHONCAN715SCA");

			OrgHeader importer = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			importer.OH_IsConsignee = true;
			importer.OH_IsShippingLine = true;
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			helper.UpdateOrAddCustomsRegNo(importer, "95-204100600", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);

			OrgHeader billIssuer = Factory.NewWithValidTestData<OrgHeader>();
			billIssuer.OH_IsShippingProvider = true;
			billIssuer.OH_FullName = "Mr Bill Issuer";
			billIssuer.OH_Code = "ISS" + new Random().Next(1000000).ToString();
			billIssuer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CPRS");

			StringBuilder messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008888XJ5 750215401117070003L84".PadRight(80));
			messageTextBuilder.Append("X20870321    870390    HON2AMED112AUUCA        00000003NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X20870421    870490    HON2AMED112TRKCA        00000007NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X25CPRS073197847799                00000010                21".PadRight(80));
			messageTextBuilder.Append("X400001000100000000".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00005".PadRight(80));
			LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "75021540";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			message.EM_MessageNum = "1234";
			Factory.Save();

			JobDeclaration declaration = new LineReleaseDeclarationCreator(message).CreateANewDeclaration();
			declaration.Factory.Save();

			AssertEquals("Message should be linked to declaration", "JobDeclaration", message.EM_LinkTable);
			AssertEquals("Message should be linked to declaration", declaration.PK, message.EM_LinkUniqueID);
			AssertEquals("Message Mode should be set to ACE", "ACE", declaration.JE_ApplicationCode);
			AssertEquals("Message Mode should be set to ACE", "ACE", declaration.US_CargoReleaseType);
			AssertEquals("ENT summary should be ticked", true, declaration.US_EnableENS);
			AssertEquals("Certify Cargo Release should not be ticked", false, declaration.US_CertifyCargoRelease);
			AssertEquals("PGA Expedited Release should be ticked", true, declaration.US_PGAExpeditedRelease);
			AssertEquals("Issuer should be set", "CPRS", declaration.JE_MasterBillIssuerSCAC);
			AssertEquals("Master bill should be set", "073197847799", declaration.JE_MasterBill);
			AssertEquals("CarrierSCAC should be set", "CPRS", declaration.US_UI_NKCarrierSCAC);

			var releaseTime = new ZDateTime(ZDateTime.Now);
			AssertEquals("Departure Date should be set", releaseTime.Date, declaration.JE_ExportDate);
			AssertEquals("Arrival date should be set", releaseTime.Date, declaration.JE_DateOfArrival);
			AssertEquals("Arrival Entry Port Date should be set", releaseTime, declaration.US_EntryDate);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
			query.AddToFilter(JoinCondition.And, StmALogSchema.SL_Reference, "[CreationSource = LR-" + message.EM_ApplicationReference + "]");

			AssertEquals("One Add event should be created with the right reference", 1, declaration.Logs.GetAllLogs().Find(query).Length);
		}

		[TestDate(2007, 11, 17, 0, 3, 0)]
		public void TestAddToShipment()
		{
			OrgHeader manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_IsConsignor = true;
			manufacturer.OH_FullName = "Mr Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XOHONCAN715SCA");

			OrgHeader importer = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			importer.OH_IsConsignee = true;
			importer.OH_IsShippingLine = true;
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			helper.UpdateOrAddCustomsRegNo(importer, "95-204100600", OrgCusCode.USACodeTypes.EmployerIdentificationNumber, helper.UnitedStates);

			OrgHeader billIssuer = Factory.NewWithValidTestData<OrgHeader>();
			billIssuer.OH_IsShippingProvider = true;
			billIssuer.OH_FullName = "Mr Bill Issuer";
			billIssuer.OH_Code = "ISS" + new Random().Next(1000000).ToString();
			billIssuer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CPRS");

			var shipment = Factory.New<ForwardingShipment>();

			StringBuilder messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008888XJ5 750215401117070003L84".PadRight(80));
			messageTextBuilder.Append("X20870321    870390    HON2AMED112AUUCA        00000003NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X20870421    870490    HON2AMED112TRKCA        00000007NUMXOHONCAN715SCA".PadRight(80));
			messageTextBuilder.Append("X25CPRS073197847799                00000010                21".PadRight(80));
			messageTextBuilder.Append("X400001000100000000".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00005".PadRight(80));
			LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "75021540";
			message.EM_ReceiveTransmit = LineReleaseMQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			message.EM_MessageNum = "1234";
			Factory.Save();

			JobDeclaration declaration = new LineReleaseDeclarationCreator(message).AddToShipment(shipment.PK, Factory);
			declaration.Factory.Save();

			AssertEquals("Message should be linked to declaration", "JobDeclaration", message.EM_LinkTable);
			AssertEquals("Message should be linked to declaration", declaration.PK, message.EM_LinkUniqueID);
			AssertEquals("Message Mode should be set to ACE", "ACE", declaration.JE_ApplicationCode);
			AssertEquals("Message Mode should be set to ACE", "ACE", declaration.US_CargoReleaseType);
			AssertEquals("ENT summary should be ticked", true, declaration.US_EnableENS);
			AssertEquals("Certify Cargo Release should not be ticked", false, declaration.US_CertifyCargoRelease);
			AssertEquals("PGA Expedited Release should be ticked", true, declaration.US_PGAExpeditedRelease);
			AssertEquals("Issuer should be set", "CPRS", declaration.JE_MasterBillIssuerSCAC);
			AssertEquals("Master bill should be set", "073197847799", declaration.JE_MasterBill);
			AssertEquals("CarrierSCAC should be set", "CPRS", declaration.US_UI_NKCarrierSCAC);
			AssertEquals("Declaration should be linked to the Shipment", shipment.PK, declaration.Shipment.PK);

			var releaseTime = new ZDateTime(ZDateTime.Now);
			AssertEquals("Departure Date should be set", releaseTime.Date, declaration.JE_ExportDate);
			AssertEquals("Arrival date should be set", releaseTime.Date, declaration.JE_DateOfArrival);
			AssertEquals("Arrival Entry Port Date should be set", releaseTime, declaration.US_EntryDate);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
			query.AddToFilter(JoinCondition.And, StmALogSchema.SL_Reference, "[CreationSource = LR-" + message.EM_ApplicationReference + "]");

			AssertEquals("One Add event should be created with the right reference", 1, declaration.Logs.GetAllLogs().Find(query).Length);
		}
	}
}
