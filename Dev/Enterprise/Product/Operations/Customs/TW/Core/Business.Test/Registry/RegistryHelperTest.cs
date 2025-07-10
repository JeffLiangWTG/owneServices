using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class RegistryHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetNXMRegistries()
		{
			CombineAssertions(() =>
			{
				using (TWCustomsDataRegistry.Instance.EnableNX201_01.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					NUnit.Framework.Assert.That(RegistryHelper.EnableNX201_01, NUnit.Framework.Is.True, "EnableNX201_01 should be true");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX201_01.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					NUnit.Framework.Assert.That(!RegistryHelper.EnableNX201_01, NUnit.Framework.Is.True, "EnableNX201_01 should be false");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX201_07.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					NUnit.Framework.Assert.That(RegistryHelper.EnableNX201_07, NUnit.Framework.Is.True, "EnableNX201_07 should be true");
				}
				using (TWCustomsDataRegistry.Instance.EnableNX201_07.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					NUnit.Framework.Assert.That(!RegistryHelper.EnableNX201_07, NUnit.Framework.Is.True, "EnableNX201_07 should be false");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX301.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					NUnit.Framework.Assert.That(RegistryHelper.EnableNX301, NUnit.Framework.Is.True, "EnableNX301 should be true");
				}
				using (TWCustomsDataRegistry.Instance.EnableNX301.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					NUnit.Framework.Assert.That(!RegistryHelper.EnableNX301, NUnit.Framework.Is.True, "EnableNX301 should be false");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX301_AX.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					NUnit.Framework.Assert.That(RegistryHelper.EnableNX301_AX, NUnit.Framework.Is.True, "EnableNX301_AX should be true");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX301_AX.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					NUnit.Framework.Assert.That(!RegistryHelper.EnableNX301_AX, NUnit.Framework.Is.True, "EnableNX301_AX should be false");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX301_DN.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					NUnit.Framework.Assert.That(RegistryHelper.EnableNX301_DN, NUnit.Framework.Is.True, "EnableNX301_DN should be true");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX301_DN.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					NUnit.Framework.Assert.That(!RegistryHelper.EnableNX301_DN, NUnit.Framework.Is.True, "EnableNX301_DN should be false");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX401.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					NUnit.Framework.Assert.That(RegistryHelper.EnableNX401, NUnit.Framework.Is.True, "EnableNX401 should be true");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX401.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					NUnit.Framework.Assert.That(!RegistryHelper.EnableNX401, NUnit.Framework.Is.True, "EnableNX401 should be false");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX601.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					NUnit.Framework.Assert.That(RegistryHelper.EnableNX601, NUnit.Framework.Is.True, "EnableNX601 should be true");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX601.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					NUnit.Framework.Assert.That(!RegistryHelper.EnableNX601, NUnit.Framework.Is.True, "EnableNX601 should be false");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX603.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					NUnit.Framework.Assert.That(RegistryHelper.EnableNX603, NUnit.Framework.Is.True, "EnableNX603 should be true");
				}

				using (TWCustomsDataRegistry.Instance.EnableNX603.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					NUnit.Framework.Assert.That(!RegistryHelper.EnableNX603, NUnit.Framework.Is.True, "EnableNX603 should be false");
				}
			});
		}

		[ExpectNoExceptions]
		public void TestValidateEntryNumber()
		{
			using (TWCustomsDataRegistry.Instance.ValidateEntryNumber.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				NUnit.Framework.Assert.That(RegistryHelper.ValidateEntryNumber, NUnit.Framework.Is.True, "ValidateEntryNumber should be true");
			}
		}

		[ExpectNoExceptions]
		public void TestDefaultPrintingGoodsLocationDescription()
		{
			using (TWCustomsDataRegistry.Instance.DefaultPrintingGoodsLocationDescription.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				NUnit.Framework.Assert.That(RegistryHelper.DefaultPrintingGoodsLocationDescription, NUnit.Framework.Is.True, "DefaultPrintingGoodsLocationDescription should be true");
			}
		}

		[ExpectNoExceptions]
		public void TestGetBoxNumberList()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			var list = RegistryHelper.GetBoxNumberList(Factory);
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(3));
			list = RegistryHelper.GetBoxNumberList(Factory, "A");
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo("600 -"));
			list = RegistryHelper.GetBoxNumberList(Factory, "B");
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(list.ElementsAsString, NUnit.Framework.Is.EqualTo(@"100 - 
300 -"));
			list = RegistryHelper.GetBoxNumberList(Factory, "C");
			NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestGetDefaultValueForBoxNumber()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			NUnit.Framework.Assert.That(RegistryHelper.GetDefaultValueForBoxNumber("AA"), NUnit.Framework.Is.EqualTo("600").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(RegistryHelper.GetDefaultValueForBoxNumber("BA"), NUnit.Framework.Is.EqualTo("100").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(RegistryHelper.GetDefaultValueForBoxNumber("XX"), NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestNewOrUpdateIfExist()
		{
			var dictionaryForTest = new Dictionary<Guid, ZString>();
			var testGuid = Guid.NewGuid();
			dictionaryForTest.NewOrUpdateIfExist(testGuid, "XX1");
			NUnit.Framework.Assert.That(dictionaryForTest[testGuid], NUnit.Framework.Is.EqualTo("XX1").Using(CustomComparers.TypeComparison));
			dictionaryForTest.NewOrUpdateIfExist(testGuid, "XX2");
			NUnit.Framework.Assert.That(dictionaryForTest.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(dictionaryForTest[testGuid], NUnit.Framework.Is.EqualTo("XX2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestGetDefaultGoodsLocation()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
			NUnit.Framework.Assert.That(RegistryHelper.GetDefaultGoodsLocation("CC", "IMP"), NUnit.Framework.Is.EqualTo("ANP0060D").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(RegistryHelper.GetDefaultGoodsLocation("DD", "IMP"), NUnit.Framework.Is.EqualTo("ANP0061D").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(RegistryHelper.GetDefaultGoodsLocation("DD", "EXP"), NUnit.Framework.Is.EqualTo("ANP0062D").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(RegistryHelper.GetDefaultGoodsLocation("XX", "IMP"), NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestDefaultBrokerStaff()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerStaff();
			var defaultBrokerStaff = RegistryHelper.DefaultBrokerStaff;
			NUnit.Framework.Assert.That(defaultBrokerStaff.BrokerStaffCode, NUnit.Framework.Is.EqualTo("CYO").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(defaultBrokerStaff.Mailbox, NUnit.Framework.Is.EqualTo("TBK0461-0").Using(CustomComparers.TypeComparison));
			var companyPk = Env.CurrentCompany.PK;
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = companyPk;
			var branchB = Factory.NewWithValidTestData<GlbBranch>();
			branchB.GB_GC = companyPk;
			var branchC = Factory.NewWithValidTestData<GlbBranch>();
			branchC.GB_GC = companyPk;
			CreateRegistryItemCusBrokerStaff("A01", "ABK0461-0", branchA.PK.ToGuid(), Guid.Empty);
			CreateRegistryItemCusBrokerStaff("B01", "BBK0461-0", branchB.PK.ToGuid(), Guid.Empty);
			CreateRegistryItemCusBrokerStaff("C01", "CBK0461-0", branchC.PK.ToGuid(), companyPk);
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchA.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				defaultBrokerStaff = RegistryHelper.DefaultBrokerStaff;
				NUnit.Framework.Assert.That(defaultBrokerStaff.BrokerStaffCode, NUnit.Framework.Is.EqualTo("A01").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(defaultBrokerStaff.Mailbox, NUnit.Framework.Is.EqualTo("ABK0461-0").Using(CustomComparers.TypeComparison));
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchB.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				defaultBrokerStaff = RegistryHelper.DefaultBrokerStaff;
				NUnit.Framework.Assert.That(defaultBrokerStaff.BrokerStaffCode, NUnit.Framework.Is.EqualTo("B01").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(defaultBrokerStaff.Mailbox, NUnit.Framework.Is.EqualTo("BBK0461-0").Using(CustomComparers.TypeComparison));
			}

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchC.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				defaultBrokerStaff = RegistryHelper.DefaultBrokerStaff;
				NUnit.Framework.Assert.That(defaultBrokerStaff.BrokerStaffCode, NUnit.Framework.Is.EqualTo("C01").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(defaultBrokerStaff.Mailbox, NUnit.Framework.Is.EqualTo("CBK0461-0").Using(CustomComparers.TypeComparison));
			}
		}

		void CreateRegistryItemCusBrokerStaff(ZString brokerStaffCode, ZString mailbox, Guid branchPK, Guid companyPk)
		{
			var broker1 = Factory.NewWithValidTestData<GlbStaff>();
			broker1.GS_GB_HomeBranch = branchPK;
			broker1.GS_LoginName = brokerStaffCode;
			broker1.GS_FullName = brokerStaffCode;
			broker1.GS_Code = brokerStaffCode;
			var certificateBrkTw = broker1.Certificates.AddNew();
			certificateBrkTw.XZ_Type = Enterprise.Core.Constants.StaffDefaultCertificateIDAndTrainingTypes.BRK;
			certificateBrkTw.XZ_RN_NKCountryOfIssuance = "TW";
			certificateBrkTw.XZ_RefNumber = "CCC";
			var currentCompanyPk = GlbCompany.CurrentCompany.PK;
			var extPswUvc = Factory.New<GlbExternalPassword>();
			extPswUvc.GP_GC = currentCompanyPk;
			extPswUvc.GP_GS = broker1.PK;
			extPswUvc.GP_PasswordType = PasswordTypesList.Codes.UVC;
			extPswUvc.GP_MailBoxID = mailbox;
			Factory.Save();
			var registryTemplate = new CusBrokerStaff();
			registryTemplate.BrokerStaffCode = brokerStaffCode;
			registryTemplate.Mailbox = mailbox;
			if (companyPk != Guid.Empty)
			{
				branchPK = Guid.Empty;
			}

			TWCustomsDataRegistry.Instance.CusBrokerStaff.SetTemporaryValue(companyPk, branchPK, Env.CurrentDepartmentPK, registryTemplate);
		}

		[ExpectNoExceptions]
		public void TestDefaultCustomsOffice()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusCustomsOffice();
			var registryItem = RegistryHelper.DefaultCustomsOffice;
			NUnit.Framework.Assert.That(registryItem.CustomsOfficeCode, NUnit.Framework.Is.EqualTo("CE").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDefaultCustomsOfficeCode()
		{
			NUnit.Framework.Assert.That(RegistryHelper.DefaultCustomsOfficeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));

			new TestTWCreator(Factory).CreateRegistryItemCusCustomsOffice();
			NUnit.Framework.Assert.That(RegistryHelper.DefaultCustomsOfficeCode, NUnit.Framework.Is.EqualTo("CE").Using(CustomComparers.TypeComparison));
		}
	}
}
