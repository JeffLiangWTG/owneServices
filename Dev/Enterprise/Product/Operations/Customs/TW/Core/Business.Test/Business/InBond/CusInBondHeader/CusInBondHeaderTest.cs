using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusInBondHeader))]
	sealed class CusInBondHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBH_Calc_ImportMasterBillNumber()
		{
			header.ArrivalBill.B0_MasterBillNumber = "XX1";
			AssertEquals("XX1", header.BH_Calc_ImportMasterBillNumber);
		}

		public void TestBH_Calc_ImportHouseBillNumber()
		{
			header.ArrivalBill.B0_HouseBillNumber = "XX2";
			AssertEquals("XX2", header.BH_Calc_ImportHouseBillNumber);
		}

		public void TestDocumentSupporter()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals(typeof(CusInBondHeaderDocumentSupporter), header.DocumentSupporter.GetType());
		}

		public void TestMessages()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			var msg = Factory.New<EDIMessage>();
			msg.EM_ApplicationCode = "TWC";
			msg.EM_LinkTable = "CusInBondHeader";
			msg.EM_LinkUniqueID = cusInBondHeader.PK;
			cusInBondHeader.Messages.Load();
			Assert(cusInBondHeader.Messages.Any(m => m.PK == msg.PK));
			AssertEquals(1, cusInBondHeader.Messages.Count);
			AssertType<TWMessage>(cusInBondHeader.Messages[0]);
			AssertSame(cusInBondHeader.Messages, cusInBondHeader.Messages);
		}

		public void TestImporterOrgPK()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "OH1 Name";
			org1.MainAddress.Address1 = "Address1";
			var address1 = org1.MainAddress;
			Factory.Save();
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_OA_Importer = ZGuid.NewZGuid();
			AssertEquals(ZGuid.Empty, cusInBondHeader.ImporterOrgPK);
			cusInBondHeader.BH_OA_Importer = address1.PK;
			AssertEquals(org1.PK, cusInBondHeader.ImporterOrgPK);
		}

		public void TestImporterOrg()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "OH1 Name";
			org1.MainAddress.Address1 = "Address1";
			var address1 = org1.MainAddress;
			Factory.Save();
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_OA_Importer = ZGuid.NewZGuid();
			AssertNull(cusInBondHeader.ImporterOrg);
			cusInBondHeader.BH_OA_Importer = address1.PK;
			AssertEquals(org1, cusInBondHeader.ImporterOrg);
		}

		public void TestDefaultValues()
		{
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			AssertEquals(Common.CusInBondApplicationCodeList.Codes.TWTranshipment, cusInBondHeader.BH_ApplicationCode);
			AssertEquals(TWMessageStatusCodeList.Codes.NotSent, cusInBondHeader.BH_MessageStatus);
		}

		public void TestBH_GS_NKCusAgentAndBH_CustomsProfile()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "GS1";
			AddgenRegCertAccredMaintList(staff1, Core.Constants.CountryCodes.Taiwan, "BRK");
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "GS2";
			AddgenRegCertAccredMaintList(staff2, Core.Constants.CountryCodes.Taiwan, "BRK");
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			var extPassword1 = AddMailBoxCredential(cusInBondHeader.Company, staff1, "00000000-2", PasswordTypesList.Codes.TVA);
			var extPassword2 = AddMailBoxCredential(cusInBondHeader.Company, staff1, "00000000-1", PasswordTypesList.Codes.TVA);
			var extPassword3 = AddMailBoxCredential(cusInBondHeader.Company, staff2, "00000000-3", PasswordTypesList.Codes.TVA);
			cusInBondHeader.BH_GS_NKCusAgent = "GS3";
			AssertEquals(true, cusInBondHeader.MailBoxReadOnly);
			cusInBondHeader.BH_GS_NKCusAgent = "GS1";
			AssertEquals(false, cusInBondHeader.MailBoxReadOnly);
			var collection = cusInBondHeader.MailBoxCollection;
			CombineAssertions(() =>
			{
				AssertNotNull("Collection should have extPassword1", collection.FindByPK(extPassword1.PK));
				AssertNotNull("Collection should have extPassword2", collection.FindByPK(extPassword2.PK));
				AssertNull("Collection should not have extPassword3", collection.FindByPK(extPassword3.PK));
			});
			cusInBondHeader.BH_CustomsProfile = "00000000-1";
			cusInBondHeader.BH_GS_NKCusAgent = "";
			AssertEquals(ZString.Empty, cusInBondHeader.BH_CustomsProfile);
		}

		public void TestBills()
		{
			AssertEquals(typeof(CusInBondBillCollection), header.Bills.GetType());
		}

		public void TestMovementHeaders()
		{
			AssertEquals(typeof(CusInBondMoveHeaderCollection), header.MovementHeaders.GetType());
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusInBondHeaderLookups), header.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInBondHeaderValidation), header.Validation.GetType());
		}

		public void TestIsTransportModeSea()
		{
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			Assert("Sea", header.IsTransportModeSea);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			Assert("Non Sea", !header.IsTransportModeSea);
		}

		public void TestIsTransportModeAir()
		{
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			Assert("Air", header.IsTransportModeAir);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			Assert("Non Air", !header.IsTransportModeAir);
		}

		public void TestArrivalBills()
		{
			CombineAssertions(() =>
			{
				AssertEquals(Constants.CusInBondBill.ShipmentType.Import, header.ArrivalBill.B0_ShipmentType);
				AssertEquals(Core.Constants.Weight.Kilograms, header.ArrivalBill.B0_WeightUQ);
				AssertEquals(Core.Constants.PkgUnit.Piece, header.ArrivalBill.B0_ManifestUQ);
			});
			header.ArrivalBill.B0_WeightUQ = "XX";
			header.ArrivalBill.B0_ManifestUQ = "XX";
			Factory.Save();
			var header2 = Factory.Load<CusInBondHeader>(header.PK);
			CombineAssertions(() =>
			{
				AssertEquals("XX", header2.ArrivalBill.B0_WeightUQ);
				AssertEquals("XX", header2.ArrivalBill.B0_ManifestUQ);
			});
		}

		public void TestMovementBills()
		{
			AssertEquals(Constants.CusInBondBill.ShipmentType.Export, header.MovementBill.B0_ShipmentType);
		}

		public void TestIsReceiptOfficeReadOnly()
		{
			header.EntryNumber = "666";
			header.BH_ReleaseStatus = ZString.Empty;
			header.UnladingOffice = ZString.Empty;
			Assert("Should be read only.", header.IsReceiptOfficeReadOnly);
			header.ReceiptOfficeInfo.RefreshBinding();
			Assert(header.ReceiptOfficeInfo.ReadOnly);
			header.UnladingOffice = "AA";
			Assert("Should not be read only.", !header.IsReceiptOfficeReadOnly);
			header.ReceiptOfficeInfo.RefreshBinding();
			Assert(!header.ReceiptOfficeInfo.ReadOnly);
			header.BH_ReleaseStatus = EntryStatusCodeList.Codes.ARM;
			Assert("Should not be read only.", !header.IsReceiptOfficeReadOnly);
			header.ReceiptOfficeInfo.RefreshBinding();
			Assert(!header.ReceiptOfficeInfo.ReadOnly);
			header.BH_ReleaseStatus = "66";
			Assert("Should be read only.", header.IsReceiptOfficeReadOnly);
			header.ReceiptOfficeInfo.RefreshBinding();
			Assert(header.ReceiptOfficeInfo.ReadOnly);
		}

		public void TestUnladingOfficeReadOnly()
		{
			header.EntryNumber = "666";
			header.BH_ReleaseStatus = EntryStatusCodeList.Codes.ARM;
			header.UnladingOfficeInfo.RefreshBinding();
			CombineAssertions(() =>
			{
				Assert("Should not be read only.", !header.UnladingOfficeReadOnly);
				Assert(!header.UnladingOfficeInfo.ReadOnly);
			});
			header.BH_ReleaseStatus = ZString.Empty;
			header.UnladingOfficeInfo.RefreshBinding();
			CombineAssertions(() =>
			{
				Assert("Should not be read only.", !header.UnladingOfficeReadOnly);
				Assert(!header.UnladingOfficeInfo.ReadOnly);
			});
			header.BH_ReleaseStatus = "66";
			header.UnladingOfficeInfo.RefreshBinding();
			CombineAssertions(() =>
			{
				Assert("Should be read only.", header.UnladingOfficeReadOnly);
				Assert(header.UnladingOfficeInfo.ReadOnly);
			});
		}

		public void TestUnladingOffice()
		{
			header.UnladingOffice = "XX";
			AssertEquals("XX", header.UnladingOffice);
			var typeToCheck = header.GetType();
			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<ListAttribute>(typeToCheck, CusInBondHeader.Schema.UnladingOffice, false, attrib => attrib.ListDataSourceMember == "Lookups.CustomsOfficeList");
				AssertHasCustomAttribute<MaxLengthAttribute>(typeToCheck, CusInBondHeader.Schema.UnladingOffice, false, attrib => attrib.MaxLength == 2);
			});
			header.ReceiptOffice = "BX";
			header.UnladingOffice = ZString.Empty;
			AssertEquals(ZString.Empty, header.ReceiptOffice);
			header.ReceiptOffice = "QQ";
			header.UnladingOffice = "WW";
			AssertEquals("QQ", header.ReceiptOffice);
			header.ReceiptOffice = ZString.Empty;
			header.UnladingOffice = "RR";
			AssertEquals("RR", header.ReceiptOffice);
		}

		public void TestReceiptOffice()
		{
			header.ReceiptOffice = "XX";
			AssertEquals("XX", header.ReceiptOffice);
			var typeToCheck = header.GetType();
			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<ListAttribute>(typeToCheck, CusInBondHeader.Schema.ReceiptOffice, false, attrib => attrib.ListDataSourceMember == "Lookups.CustomsOfficeList");
				AssertHasCustomAttribute<MaxLengthAttribute>(typeToCheck, CusInBondHeader.Schema.ReceiptOffice, false, attrib => attrib.MaxLength == 2);
				AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeToCheck, CusInBondHeader.Schema.ReceiptOffice, false, attrib => attrib.Member == "IsReceiptOfficeReadOnly");
			});
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = "AT";
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			customsOffice.ZZD_IsSea = true;
			Factory.Save();
			header.ReceiptOffice = "AT";
			CombineAssertions(() =>
			{
				AssertEquals(InBondTransportModeCodes.Codes.SEA, header.BH_ImportTransportMode);
				AssertEquals(InBondTransportModeCodes.Codes.SEA, header.DefaultImportTransportModeFromOffice);
			});
			header.ReceiptOffice = "XX";
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, header.BH_ImportTransportMode);
				AssertEquals(ZString.Empty, header.DefaultImportTransportModeFromOffice);
			});
			customsOffice.ZZD_IsSea = false;
			customsOffice.ZZD_IsAir = true;
			header.ReceiptOffice = "AT";
			CombineAssertions(() =>
			{
				AssertEquals(InBondTransportModeCodes.Codes.AIR, header.BH_ImportTransportMode);
				AssertEquals(InBondTransportModeCodes.Codes.AIR, header.DefaultImportTransportModeFromOffice);
			});
		}

		public void TestReceiptOfficeInfo()
		{
			header.UnladingOffice = ZString.Empty;
			Assert("Should be read only.", header.ReceiptOfficeInfo.ReadOnly);
			header.UnladingOffice = "AA";
			Assert("Should not be read only.", !header.ReceiptOfficeInfo.ReadOnly);
		}

		public void TestEntryNumber()
		{
			header.EntryNumber = "0123456789";
			AssertEquals("0123456789", header.EntryNumber);
			var typeToCheck = header.GetType();
			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<ReadOnlyAttribute>(typeToCheck, "EntryNumber", false, attrib => attrib.IsReadOnly);
				AssertHasCustomAttribute<MaxLengthAttribute>(typeToCheck, "EntryNumber", false, attrib => attrib.MaxLength == 14);
			});

			header.EntryNumber = "0123456786";
			Assert("HasChanges should be true.", header.HasChanges);
		}

		public void TestEntryNumberInfo()
		{
			Assert(header.EntryNumberInfo.ReadOnly);
		}

		public void TestCusEntryNumber()
		{
			header.EntryNumber = "0123456789";
			var bizO = header.CusEntryNumber;
			CombineAssertions(() =>
			{
				AssertNotNull("CusEntryNumber should be not null", bizO);
				AssertEquals("0123456789", bizO.CE_EntryNum);
				AssertType(typeof(CusEntryNumber), bizO);
			});
			header.EntryNumber = "";
			AssertNull("CusEntryNumber should be null because corresponding record was deleted", header.CusEntryNumber);
			var number = CusEntryNumber.New(header, "TRS", Core.Constants.CountryCodes.Taiwan);
			number.CE_EntryNum = "EX123";
			AssertEquals("CusEntryNum should have reference to EntryHeader", header.PK, number.CE_ParentID);
		}

		public void TestCusEntryNumDeleted()
		{
			CombineAssertions("CusEntryNum is deleted", () =>
			{
				var inBondHeader = (CusInBondHeader)GetNewBusinessObject();
				inBondHeader.EntryNumber = "TSTEntryNum";
				var entryNum = inBondHeader.CusEntryNumber;
				AssertEquals("TSTEntryNum", entryNum.CE_EntryNum);
				inBondHeader.Delete();
				AssertEquals(true, entryNum.IsDeleted);
			}

			);
		}

		public void TestLoadCusEntryNumber()
		{
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "BB";
			header.TW_BoxNumber = "123";
			header.EntryNumber = "98767666";
			var bizO = header.CusEntryNumber;
			CombineAssertions(() =>
			{
				AssertEquals(header.PK, bizO.CE_ParentID);
				AssertEquals("CUS", bizO.CE_Category);
				AssertEquals("TRS", bizO.CE_EntryType);
				AssertEquals(Core.Constants.CountryCodes.Taiwan, bizO.CE_RN_NKCountryCode);
			});
			header.AllocateEntryNumber();
			Factory.Save();
			var entryNumQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentID, header.PK);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, "CUS");
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, "TRS");
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Taiwan);
			var entryNumCollection = Factory.Load(typeof(CusEntryNumber), entryNumQuery);
			CombineAssertions(() =>
			{
				AssertEquals(1, entryNumCollection.Length);
				Assert(entryNumCollection.OfType<CusEntryNumber>().First().CE_EntryNum.StartsWith("AABB"));
			});
		}

		[TestDate(2019, 12, 31)]
		public void TestEntryNumberOnFactorySaving()
		{
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "BB";
			header.TW_BoxNumber = "123";
			header.EntryNumber = "98767666";
			header.BH_ReleaseStatus = "A28";
			AssertEquals("98767666", header.EntryNumber);
			header.AllocateEntryNumber(ZString.Empty);
			Factory.Save();
			var entryNumber = header.EntryNumber;
			CombineAssertions(() =>
			{
				AssertNotEquals("98767666", entryNumber);
				AssertEquals("AA", entryNumber.Substring(0, 2));
				AssertEquals("BB", entryNumber.Substring(2, 2));
				AssertEquals("08", entryNumber.Substring(4, 2));
				AssertEquals("123", entryNumber.Substring(6, 3));
				AssertEquals("00001", entryNumber.Substring(9, 5));
			});
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "AA";
			header.TW_BoxNumber = "123";
			header.AllocateEntryNumber(ZString.Empty);
			Factory.Save();
			entryNumber = header.EntryNumber;
			CombineAssertions(() =>
			{
				AssertEquals("AA", entryNumber.Substring(0, 2));
				AssertEquals("  ", entryNumber.Substring(2, 2));
				AssertEquals("08", entryNumber.Substring(4, 2));
				AssertEquals("123", entryNumber.Substring(6, 3));
				AssertEquals("00002", entryNumber.Substring(9, 5));
			});
			TestDateAttribute.AddDays(1);
			header.AllocateEntryNumber(ZString.Empty);
			Factory.Save();
			entryNumber = header.EntryNumber;
			AssertEquals("00003", entryNumber.Substring(9, 5));
			header.BH_ReleaseStatus = "REJ";
			header.AllocateEntryNumber(ZString.Empty);
			Factory.Save();
			entryNumber = header.EntryNumber;
			AssertEquals("00004", entryNumber.Substring(9, 5));
			header.UnladingOffice = "BB";
			header.AllocateEntryNumber(ZString.Empty);
			Factory.Save();
			entryNumber = header.EntryNumber;
			AssertEquals("00005", entryNumber.Substring(9, 5));
		}

		public void TestImportVessel()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL 1";
			vessel.RV_LloydsNumber = "abcdef";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;
			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_ImportConveyanceName = "VESSEL 1";
			var importConveyanceName = cusInBondHeader.ImportVessel;
			CombineAssertions(() =>
			{
				AssertEquals(vessel.PK, importConveyanceName.PK);
				AssertEquals("abcdef", importConveyanceName.RV_LloydsNumber);
				AssertEquals(Core.Constants.CountryCodes.Taiwan, importConveyanceName.RV_RN_NKCountryOfReg);
			});
		}

		public void TestImportConveyanceNameDescription()
		{
			var vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "VESSEL 1";
			vessel1.RV_LloydsNumber = "8811928";
			vessel1.RV_RadioCallSign = "8811927";
			vessel1.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VESSEL 2";
			vessel2.RV_RadioCallSign = "8811924";
			vessel2.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Code = "VESSEL 3";
			vessel3.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Taiwan;

			var cusInBondHeader = Factory.New<CusInBondHeader>();
			cusInBondHeader.BH_ImportConveyanceName = "VESSEL 1";
			AssertEquals("8811928", cusInBondHeader.ImportConveyanceNameDescription);

			cusInBondHeader.BH_ImportConveyanceName = "VESSEL 2";
			AssertEquals("8811924", cusInBondHeader.ImportConveyanceNameDescription);

			cusInBondHeader.BH_ImportConveyanceName = "VESSEL 3";
			AssertEquals("NIL", cusInBondHeader.ImportConveyanceNameDescription);

			cusInBondHeader.BH_ImportConveyanceName = ZString.Empty;
			AssertEquals(ZString.Empty, cusInBondHeader.ImportConveyanceNameDescription);
		}

		static void AddgenRegCertAccredMaintList(GlbStaff staff, ZString country, ZString type)
		{
			var genRegCertAccredMaintList = staff.Certificates.AddNew();
			genRegCertAccredMaintList.XZ_RN_NKCountryOfIssuance = country;
			genRegCertAccredMaintList.XZ_Type = type;
		}

		GlbExternalPassword AddMailBoxCredential(GlbCompany company, GlbStaff staff, ZString mailBoxID, ZString passwordType)
		{
			var extPassword2 = Factory.New<GlbExternalPassword>();
			extPassword2.GP_PasswordType = passwordType;
			extPassword2.GP_MailBoxID = mailBoxID;
			if (company != null)
			{
				extPassword2.GP_GC = company.PK;
			}

			if (staff != null)
			{
				extPassword2.GP_GS = staff.PK;
			}

			return extPassword2;
		}

		public void TestITWMessageInfoProvider()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			AddgenRegCertAccredMaintList(staff, Core.Constants.CountryCodes.Taiwan, "BRK");
			AddMailBoxCredential(header.Company, staff, "00000000-2", PasswordTypesList.Codes.TVA);
			header.BH_GS_NKCusAgent = "TT";
			header.BH_CustomsProfile = "00000000-2";

			var provider = header as ITWMessageInfoProvider;
			AssertNotNull(provider);
			AssertEquals("", provider.EntryNumber);
			var cusNum1 = Factory.NewWithValidTestData<CusEntryNumber>();
			cusNum1.CE_ParentID = header.PK;
			cusNum1.CE_Category = "CUS";
			cusNum1.CE_EntryType = "TRS";
			cusNum1.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
			cusNum1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			cusNum1.CE_EntryNum = "NO1";
			CombineAssertions(() =>
			{
				AssertEquals("TRS", provider.EntryNumberType);
				AssertEquals("NO1", provider.EntryNumber);
				AssertEquals("TT", provider.StaffCode);
				AssertEquals(GlbCompany.CurrentCompany.GC_Code, provider.CompanyID);
				AssertEquals(PasswordTypesList.Codes.TVA, provider.PasswordType);
			});
		}

		public void TestCusAgent()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			header.BH_GS_NKCusAgent = "TT";
			AssertEquals(staff, header.CusAgent);
		}

		public void TestSaveEmptyContainers()
		{
			var containerType = Factory.NewWithValidTestData<RefContainer>();
			containerType.RC_Code = "40ZZ";
			Factory.Save();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var containers = header.MovementHeader.InBondMoveDetail.Containers;
			Addcontainer(containers);
			Addcontainer(containers);
			Addcontainer(containers);
			AssertEquals(3, header.MovementHeader.InBondMoveDetail.Containers.Count);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<CusInBondHeader>(header.PK);
			AssertEquals(0, header.MovementHeader.InBondMoveDetail.Containers.Count);
			containers = header.MovementHeader.InBondMoveDetail.Containers;
			Addcontainer(containers);
			Addcontainer(containers);
			var container = Addcontainer(containers);
			container.BC_ContainerNum = "1111";
			container = Addcontainer(containers);
			container.BC_RC = containerType.PK;
			container = Addcontainer(containers);
			container.BC_Mode = CusInBondContainerModeList.Codes.FCL;
			container = Addcontainer(containers);
			container.BC_IsPart = ZBool.True;
			AssertEquals(6, header.MovementHeader.InBondMoveDetail.Containers.Count);
			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			header = newFactory.Load<CusInBondHeader>(header.PK);
			AssertEquals(4, header.MovementHeader.InBondMoveDetail.Containers.Count);
		}

		CusInBondContainer Addcontainer(CusInBondContainerCollection containers)
		{
			var container = containers.AddNew();
			containers.Add(container);
			return container;
		}

		public void TestMessageInitiator()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("HasMessageInitiator", false, header.HasMessageInitiator);
			header.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			AssertEquals("HasMessageInitiator", true, header.HasMessageInitiator);
		}

		[TestDate(2019, 09, 20)]
		public void TestCachedTodaysDate()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("CachedTodaysDate Should be ", ZDateTime.Today, header.CachedTodaysDate);
		}

		[TestDate(2019, 09, 20)]
		public void TestDateOfValuation()
		{
			var header = Factory.New<CusInBondHeader>();
			AssertEquals("DateOfValuation Should be ", ZDateTime.Today, header.DateOfValuation);
			var date = new ZDateTime(2019, 09, 19);
			header.BH_ETA = date;
			AssertEquals("DateOfValuation Should be ", date, header.DateOfValuation);
		}

		public void TestUnladingOfficeItem()
		{
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = "AT";
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			customsOffice.ZZD_IsSea = true;
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			CombineAssertions(() =>
			{
				AssertNull("UnladingOfficeItem Should be null", header.UnladingOfficeItem);
				AssertEquals("IsUnladingOfficeSea Should be", false, header.IsUnladingOfficeSea);
				AssertEquals("IsUnladingOfficeAir Should be", false, header.IsUnladingOfficeAir);
			});
			header.UnladingOffice = "AT";
			CombineAssertions(() =>
			{
				AssertNotNull("UnladingOfficeItem Should be not null", header.UnladingOfficeItem);
				AssertEquals("IsUnladingOfficeSea Should be", true, header.IsUnladingOfficeSea);
			});
			customsOffice.ZZD_IsSea = false;
			customsOffice.ZZD_IsAir = true;
			AssertEquals("IsUnladingOfficeAir Should be", true, header.IsUnladingOfficeAir);
		}

		public void TestReceiptOfficeItem()
		{
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = "AT";
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			customsOffice.ZZD_IsSea = true;
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			CombineAssertions(() =>
			{
				AssertNull("ReceiptOfficeItem Should be null", header.ReceiptOfficeItem);
				AssertEquals("IsReceiptOfficeSea Should be", false, header.IsReceiptOfficeSea);
				AssertEquals("IsReceiptOfficeAir Should be", false, header.IsReceiptOfficeAir);
			});
			header.UnladingOffice = "AT";
			CombineAssertions(() =>
			{
				AssertNotNull("ReceiptOfficeItem Should be not null", header.ReceiptOfficeItem);
				AssertEquals("IsReceiptOfficeSea Should be", true, header.IsReceiptOfficeSea);
			});
			customsOffice.ZZD_IsSea = false;
			customsOffice.ZZD_IsAir = true;
			AssertEquals("IsReceiptOfficeAir Should be", true, header.IsReceiptOfficeAir);
		}

		public void TestIsWaitingForResponse()
		{
			var heard = Factory.NewWithValidTestData<CusInBondHeader>();
			foreach (var status in heard.AwaitingStatusList)
			{
				heard.BH_MessageStatus = status;
				Assert(heard.IsWaitingForResponse);
			}

			heard.BH_MessageStatus = "AA";
			Assert(!heard.IsWaitingForResponse);
			heard.BH_MessageStatus = ZString.Empty;
			Assert(!heard.IsWaitingForResponse);
		}

		[TestDate(2019, 12, 31)]
		public void TestJobReferenceWhenSave()
		{
			string companyCode = GlbCompany.CurrentCompany.GC_Code;
			header.BH_JobReference = ZString.Empty;
			Factory.Save();
			var number = header.BH_JobReference;
			CombineAssertions(() =>
			{
				AssertNotNullOrEmpty(number);
				AssertEquals(ZString.Format("TRANS2019{0}", companyCode), number.Substring(0, 12));
				AssertEquals(20, number.Length);
			});
			header.ReceiptOffice = "AA";
			Factory.Save();
			AssertEquals(number, header.BH_JobReference);
			TestDateAttribute.AddYears(1);
			header.BH_JobReference = ZString.Empty;
			Factory.Save();
			AssertEquals("TRANS2020", header.BH_JobReference.Substring(0, 9));
		}

		public void TestBH_UniqueVoyageIdentifier_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(header.BH_UniqueVoyageIdentifierInfo);
			AssertEquals("Caption", "Flight No/Voyage", resourceStringData.Caption);
		}

		#region TestDescription
		class CusInBondHeaderForDescriptionTest : CusInBondHeader
		{
			public CusInBondHeaderForDescriptionTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString Description => HumanReadableShortcutNameCore;
		}

		public void TestDescription()
		{
			var cusInBondHeader = Factory.New<CusInBondHeaderForDescriptionTest>();
			var descriptionCustomizationCollection = TWCustomsDataRegistry.Instance.TranshipmentDescriptionCustomization.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid());
			var customization = TWCustomsDataRegistry.Instance.TranshipmentDescriptionCustomization;
			AssertDescription(customization, descriptionCustomizationCollection, cusInBondHeader);
		}

		void AssertDescription(CodeDescriptionBoolDisallowNewRegistryItem customization, CodeDescriptionBoolCollection descriptionCustomizationCollection, CusInBondHeaderForDescriptionTest cusInBondHeader)
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "OH1 Name";
			org1.MainAddress.Address1 = "Address1";
			Factory.Save();
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("JNO")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("MBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("HBL")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("ENT")).Bool = ZBool.False;
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("IMP")).Bool = ZBool.False;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			cusInBondHeader.BH_JobReference = "ABC";
			AssertEquals("Default Description ", "ABC", cusInBondHeader.Description);
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("JNO")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC", cusInBondHeader.Description);
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("MBL")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC", cusInBondHeader.Description);
			cusInBondHeader.ArrivalBill.B0_MasterBillNumber = "MBL";
			AssertEquals("ABC - MBL: MBL", cusInBondHeader.Description);
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("HBL")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC - MBL: MBL", cusInBondHeader.Description);
			cusInBondHeader.ArrivalBill.B0_HouseBillNumber = "HBL";
			AssertEquals("ABC - MBL: MBL, HBL: HBL", cusInBondHeader.Description);
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("ENT")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: ", cusInBondHeader.Description);
			cusInBondHeader.EntryNumber = "ENT";
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: ENT", cusInBondHeader.Description);
			((CodeDescriptionBoolDisallowNew)descriptionCustomizationCollection.FindByCode("IMP")).Bool = ZBool.True;
			customization.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, descriptionCustomizationCollection);
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: ENT", cusInBondHeader.Description);
			cusInBondHeader.BH_OA_Importer = org1.MainAddress.PK;
			AssertEquals("ABC - MBL: MBL, HBL: HBL, ENT: ENT, IMP: OH1", cusInBondHeader.Description);
		}

		#endregion
		#region TW_BoxNumber
		public void TestTW_BoxNumber()
		{
			header.TW_BoxNumber = "110";
			AssertEquals("110", header.TW_BoxNumber);
			var typeToCheck = header.GetType();
			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<ListAttribute>(typeToCheck, CusInBondHeader.Schema.TW_BoxNumber, false, attrib => attrib.ListDataSourceMember == "Lookups.BoxNumberList");
				AssertHasCustomAttribute<MaxLengthAttribute>(typeToCheck, CusInBondHeader.Schema.TW_BoxNumber, false, attrib => attrib.MaxLength == 3);
			});
		}

		public void TestSetDefaultValueForBoxNumber()
		{
			var header = this.header;
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			header.TW_BoxNumber = ZString.Empty;
			header.ReceiptOffice = "AA";
			AssertEquals("600", header.TW_BoxNumber);
			header.ReceiptOffice = "BA";
			AssertEquals("100", header.TW_BoxNumber);
			header.ReceiptOffice = "XX";
			AssertEquals(ZString.Empty, header.TW_BoxNumber);
		}

		#endregion
		public void TestSetDefaultValuesForDefaultBrokerStaff()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerStaff();
			var header = Factory.New<CusInBondHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("CYO", header.BH_GS_NKCusAgent);
				AssertEquals("TBK0461-0", header.BH_CustomsProfile);
			});
		}

		public void TestAllocateEntryNumberWithUserEnteredEntryNumber()
		{
			header.AllocateEntryNumber("AB  1112300006");
			AssertEquals("AB  1112300006", header.EntryNumber);
		}

		public void TestEntryNumberMutex()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var header1 = factory1.NewWithValidTestData<CusInBondHeader>();
			factory1.Save();
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var headerInFactory2 = factory2.Load<CusInBondHeader>(header1.PK);
			CombineAssertions(() =>
			{
				AssertEquals(true, headerInFactory2.LockEntryNumberAllocationMutex);
				AssertEquals(false, header1.LockEntryNumberAllocationMutex);
				AssertEquals("Still locked", true, headerInFactory2.LockEntryNumberAllocationMutex);
				AssertEquals("Still locked by another session", false, header1.LockEntryNumberAllocationMutex);
			});

			header1.UnlockEntryNumberAllocationMutex();
			AssertEquals(true, headerInFactory2.LockEntryNumberAllocationMutex);

			headerInFactory2.UnlockEntryNumberAllocationMutex();
			CombineAssertions(() =>
			{
				AssertEquals(true, header1.LockEntryNumberAllocationMutex);
				AssertEquals(false, headerInFactory2.LockEntryNumberAllocationMutex);
			});

			using (var entryNumberAllocationMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, Common.CusEntryNumberTypes.Taiwan.Transhipment + header1.PK.ToString()))
			{
				CombineAssertions(() =>
				{
					AssertEquals("EntryNumberAllocationMutex is locked", true, entryNumberAllocationMutex.IsLocked);
					AssertEquals("EntryNumberAllocationMutex has not lock", false, entryNumberAllocationMutex.HasLock);

					factory1.Save();
					AssertEquals("Lock should be released when factory is saved", false, entryNumberAllocationMutex.IsLocked);
					AssertEquals("EntryNumberAllocationMutex has not lock", false, entryNumberAllocationMutex.HasLock);
					AssertEquals("LockEntryNumberAllocationMutex is true", true, header1.LockEntryNumberAllocationMutex);
					AssertEquals("EntryNumberAllocationMutex is locked", true, entryNumberAllocationMutex.IsLocked);
					AssertEquals("EntryNumberAllocationMutex has not lock", false, entryNumberAllocationMutex.HasLock);

					header1.Delete();
					AssertEquals("Lock should be released when CusInBondHeader is deleted", false, entryNumberAllocationMutex.IsLocked);
					AssertEquals("EntryNumberAllocationMutex has not lock", false, entryNumberAllocationMutex.HasLock);
				});
			}
		}

		public void TestEntryNumberAllocationMutexLockInfo_NullUser()
		{
			var header = Factory.New<CusInBondHeader>();
			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = false;
			using (var mutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, Common.CusEntryNumberTypes.Taiwan.Transhipment + header.PK.ToString()))
			{
				Assert(mutex.Lock());
				var lockInfo = "Mutex:" + MutexIDs.CustomsTransactionIDAllocation.Name + ":" + Common.CusEntryNumberTypes.Taiwan.Transhipment + header.PK.ToString();
				var emptyGuid = Guid.Empty;
				var sql = $@"UPDATE TOP(1) StmServiceHeartBeat
									SET SV_ParentId = '{emptyGuid}',
										SV_SystemLastEditTimeUtc = GetUtcDate(),
										SV_SystemLastEditUser = 'USR'
									FROM dbo.StmServiceSemaphore
									INNER JOIN dbo.StmServiceHeartBeat ON SS_SV = SV_PK
									WHERE SS_LockInfo LIKE '%{lockInfo}%';";
				TestConnection.Command(sql).ExecuteNonQuery();
				CombineAssertions(() =>
				{
					AssertEquals(false, header.LockEntryNumberAllocationMutex);
					AssertNoExceptionThrown(() => header.GetEntryNumberAllocationMutexLockInfo());
				});
			}

			NeedUnlockAfterMergeForTestHelper.NeedUnlockAfterMergeForTest = true;
		}

		public void TestEntryNumberForSendingObject()
		{
			header.EntryNumber = "ABAC1010000";
			AssertEquals("ABAC1010000", header.EntryNumberForSendingObject);

			header.EntryNumber = ZString.Empty;
			AssertEquals(MessageConstants.EntryNumberPlaceHolder, header.EntryNumberForSendingObject);
		}

		[TestDate(2022, 06, 30)]
		public void TestAllocateEntryNumber()
		{
			using (header.GetGenerateEntryNumberExceptionSupporter())
			{
				AssertExceptionThrown<GenerateEntryNumberException>(() => header.AllocateEntryNumber());
			}

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, header.EntryNumber);
				AssertNoExceptionThrown(() => header.AllocateEntryNumber());
				AssertEquals(ZString.Empty, header.EntryNumber);
			});

			header.EntryNumber = "ABAC0001";
			header.BH_MessageStatus = MessageStatusList.Codes.NotSent;
			AssertAllocateEntryNumber(header, ZString.Empty);

			header.EntryNumber = "ABAC0001";
			header.BH_MessageStatus = MessageStatusList.Codes.AwaitingReplace;
			AssertAllocateEntryNumber(header, "ABAC0001");

			header.EntryNumber = ZString.Empty;
			header.ReceiptOffice = "AA";
			header.UnladingOffice = "BB";
			header.TW_BoxNumber = "123";
			AssertAllocateEntryNumber(header, "AABB1112300001");
		}

		void AssertAllocateEntryNumber(CusInBondHeader header, ZString expectedEntryNumber)
		{
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => header.AllocateEntryNumber());
				AssertEquals(expectedEntryNumber, header.EntryNumber);
			});
		}

		[TestDate(2023, 11, 30)]
		public void TestIEntryNumberGeneratorProviderMembers()
		{
			header.ReceiptOffice = "A";
			header.UnladingOffice = "B";
			header.TW_BoxNumber = "A11";
			CombineAssertions(() =>
			{
				var provider = (IEntryNumberGeneratorProvider)header;
				AssertEquals("EntryNumberDate", new ZDateTime(2023, 11, 30).ToShortTimeString(), provider.EntryNumberDate.ToShortTimeString());
				AssertEquals("EntryNumberType", CusEntryNumberTypes.Taiwan.Transhipment, provider.EntryNumberType);
				AssertEquals("Company", header.Company, provider.Company);
				AssertEquals("ShipmentType", CusEntryNumberTypes.Taiwan.Transhipment, provider.ShipmentType);
				AssertEquals("EntryNumberPart1Info", header.ReceiptOffice, provider.EntryNumberPart1Info.Value);
				AssertEquals("CustomsBrokerageBoxNumberInfo", header.TW_BoxNumber, provider.CustomsBrokerageBoxNumberInfo.Value);
				AssertEquals("EntryNumberPart2Info", header.UnladingOffice, provider.EntryNumberPart2Info.Value);
				AssertEquals("GetEntryNumberGeneratorCategory", EntryNumberGeneratorCategory.T, provider.GetEntryNumberGeneratorCategory());
				AssertEquals("EntryNumberGeneratorProviderBusinessObject", header, provider.EntryNumberGeneratorProviderBusinessObject);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
		}

		CusInBondHeader header;
	}
}
