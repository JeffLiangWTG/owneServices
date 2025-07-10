using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusClassPartPivot))]
	class BaseCusClassPartPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIWorkflowTriggerEventSourceInterface()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part.PK;

			var iWorkflowTriggerEventSource = (IWorkflowTriggerEventSource)pivot;
			AssertEquals("ParentWorkflowProviders.Length", 1, iWorkflowTriggerEventSource.ParentWorkflowProviders.Count);
			AssertEquals("ParentWorkflowProviders", part.PK, iWorkflowTriggerEventSource.ParentWorkflowProviders[0].PK);
			AssertEquals("JobHeaderCompany", GlbCompany.CurrentCompany, iWorkflowTriggerEventSource.JobHeaderCompany);
		}

		[TestDate(2019, 12, 10, 0, 0, 0)]
		public void TestAllApplicableRateSelectionCriteria()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_RN_NKCountryOfOrigin = "AU";
			pivot.CI_PrimaryPreference = "STD";
			pivot.CI_ConcessionOrder = "Ord11";

			var rateSelectionCriteria = pivot.AllApplicableRatesSelectionCriteria;
			AssertEquals("EffectiveDate", new ZDateTime(2019, 12, 10, 0, 0, 0), rateSelectionCriteria.EffectiveDate);
			AssertEquals("CountryOfOrigin", "AU", rateSelectionCriteria.TradeGroupCountry);
			AssertEquals("SecondTradeGroups", 0, rateSelectionCriteria.SecondTradeGroups.Count);
			AssertEquals("PrimaryPreference", "STD", rateSelectionCriteria.PrimaryPreference);
			AssertEquals("AdditionalCodes count", 1, rateSelectionCriteria.AdditionalCodes.Count);
			Assert("AdditionalCodes", rateSelectionCriteria.AdditionalCodes.Contains(""));
			AssertEquals("ConcessionOrder", "Ord11", rateSelectionCriteria.ConcessionOrder);
			AssertNotNull(pivot.SetterSuspender);
			AssertType(typeof(SetterSuspender), pivot.SetterSuspender);
		}

		public void TestHumanReadableNameCore()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "00001";
			pivot.CI_CC = classification.PK;
			pivot.CI_ChildType = "HTI";
			pivot.CI_TariffNum = "7845102014";
			AssertEquals(pivot.HumanReadableName, "Classification (HTI 00001)");

			pivot.CI_CC = ZGuid.Empty;
			AssertEquals(pivot.HumanReadableName, "Classification (HTI 7845102014)");
		}

		public void TestIsTariffNumReadOnly()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = ZGuid.Empty;
			AssertEquals("TariffNum is not ReadOnly", false, pivot.IsTariffNumReadOnly);
			pivot.CI_CC = ZGuid.NewZGuid();
			AssertEquals("TariffNum is ReadOnly", true, pivot.IsTariffNumReadOnly);

			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(BaseCusClassPartPivot), "CI_TariffNum", true, attrib => attrib.Member == "IsTariffNumReadOnly");
			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(BaseCusClassPartPivot), "CI_FormattedTariffNum", true, attrib => attrib.Member == "IsTariffNumReadOnly");
		}

		public void TestCI_CC_ReadOnly()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_TariffNum = ZString.Empty;
			AssertEquals("CI_CC is not ReadOnly", false, pivot.CI_CC_ReadOnly);
			pivot.CI_TariffNum = "ABC123";
			AssertEquals("CI_CC is ReadOnly", true, pivot.CI_CC_ReadOnly);

			AssertHasCustomAttribute<ReadOnlyMemberAttribute>(typeof(BaseCusClassPartPivot), "CI_CC", true, attrib => attrib.Member == "CI_CC_ReadOnly");
		}

		public void TestLogIfPropertyValueChange()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.Logs.GetAllLogs().DeleteAll();
			pivot.LogIfPropertyValueChange("", "1", "2");
			AssertEquals(0, pivot.Logs.GetAllLogs().Count);

			pivot.LogIfPropertyValueChange("Name", "1", "1");
			AssertEquals(0, pivot.Logs.GetAllLogs().Count);

			pivot.LogIfPropertyValueChange("Name", "1", "2");
			AssertEquals(1, pivot.Logs.GetAllLogs().Count);
			AssertEquals(0, part.Logs.GetAllLogs().Count);

			pivot.LogIfPropertyValueChange("Name", "1", "2");
			AssertEquals("Name '1' changed to '2'", pivot.Logs.MostRecentLog.SL_Reference);
			pivot.Logs.GetAllLogs().DeleteAll();
			pivot.LogIfPropertyValueChange("Name", "1", string.Empty);
			AssertEquals("Name '1' removed", pivot.Logs.MostRecentLog.SL_Reference);
			pivot.Logs.GetAllLogs().DeleteAll();
			pivot.LogIfPropertyValueChange("Name", string.Empty, "1");
			AssertEquals("Name '1' added", pivot.Logs.MostRecentLog.SL_Reference);
		}

		public void TestCI_ChildTypeDescription()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			AssertEquals("Both", pivot.CI_ChildTypeDescription);
		}

		public void TestLastAuditedUserFullName()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_LastAuditedUser = "USR";
			AssertEquals("USR", pivot.LastAuditedUserFullName);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Full Name";
			staff.GS_Code = "CDE";

			pivot.CI_LastAuditedUser = "CDE";
			AssertEquals("Full Name(CDE)", pivot.LastAuditedUserFullName);
		}

		public void TestSetCI_CC()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_RN_NKCountryCode = "NZ";
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_CC = classification.PK;
			AssertEquals("Part (PARTNUM) has a ER pivot with a NZ classification attached.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestTariffNumbersAndTariffNumbersIncludingComponents()
		{
			var catalog = Factory.New<BaseCusGoodsCatalog>();
			var classification = Factory.New<BaseCusClassification>();
			var product = Factory.New<OrgSupplierPart>();

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = product.PK;
			pivot.CI_SupplementalTariff = "11111111";

			classification.CC_TariffNum = "00000000";
			AssertEquals("00000000", pivot.TariffNumber);
			AssertEquals("HTB:00000000(11111111)", pivot.TariffNumbersIncludingComponents);

			catalog.CGC_Tariff = "00000002";
			pivot.CI_CC = ZGuid.Empty;
			pivot.CI_CGC_Catalog = catalog.PK;

			AssertEquals("00000002", pivot.TariffNumber);
			AssertEquals("HTB:00000002(11111111)", pivot.TariffNumbersIncludingComponents);
		}

		public void TestTariffWithComponentsShowsTariffType()
		{
			var classification = Factory.New<BaseCusClassification>();
			var product = Factory.New<OrgSupplierPart>();

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = product.PK;
			pivot.CI_TariffNum = "1234567890";
			AssertEquals("CI_FormattedTariffNum", "HTB:1234567890", pivot.TariffNumbersIncludingComponents);

			pivot.CI_TariffNum = "9876.54.32 10";
			pivot.CI_ChildType = "HTI";
			AssertEquals("CI_FormattedTariffNum", "HTI:9876.54.32 10", pivot.TariffNumbersIncludingComponents);

			pivot.CI_TariffNum = "1234.56.78 90";
			pivot.CI_SupplementalTariff = "11111111";
			AssertEquals("CI_FormattedTariffNum", "HTI:1234.56.78 90(11111111)", pivot.TariffNumbersIncludingComponents);

			pivot.CI_TariffNum = "9876543210";
			pivot.CI_ChildType = "HTE";
			AssertEquals("CI_FormattedTariffNum", "HTE:9876543210(11111111)", pivot.TariffNumbersIncludingComponents);
		}

		public void TestCI_FormattedTariffNum()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_TariffNum = "1234567890";
			AssertEquals("CI_TariffNum", "1234567890", pivot.CI_TariffNum);
			AssertEquals("CI_FormattedTariffNum", "1234.56.78 90", pivot.CI_FormattedTariffNum);
			pivot.CI_FormattedTariffNum = "9876.54.32 10";
			AssertEquals("CI_TariffNum", "9876.54.32 10", pivot.CI_TariffNum);
			AssertEquals("CI_FormattedTariffNum", "9876.54.32 10", pivot.CI_FormattedTariffNum);
			pivot.CI_TariffNum = "1234.56.78 90";
			AssertEquals("CI_TariffNum", "1234.56.78 90", pivot.CI_TariffNum);
			AssertEquals("CI_FormattedTariffNum", "1234.56.78 90", pivot.CI_FormattedTariffNum);
			pivot.CI_FormattedTariffNum = "9876543210";
			AssertEquals("CI_TariffNum", "9876543210", pivot.CI_TariffNum);
			AssertEquals("CI_FormattedTariffNum", "9876.54.32 10", pivot.CI_FormattedTariffNum);
		}

		public void TestCI_SupplementalTariff()
		{
			var pivot2 = Factory.NewWithValidTestData<BaseCusClassPartPivot>();
			pivot2.CI_SupplementalTariff = "5555.2222 77";
			AssertEquals("Formatted supplementTariff", "5555.22.22 77", pivot2.CI_FormattedSupplementalTariff);
			AssertEquals("SupplementTariff", "5555.2222 77", pivot2.CI_SupplementalTariff);
			Factory.Save();

			var pivotLoaded2 = Factory.Load<BaseCusClassPartPivot>(pivot2.PK);
			AssertEquals("Saved as entered", "5555.2222 77", ((IBusinessObjectInternals)pivotLoaded2).Row[BaseCusClassPartPivot.Schema.CI_SupplementalTariff]);
			AssertEquals("SupplementTariff", "5555.2222 77", pivotLoaded2.CI_SupplementalTariff);
			AssertEquals("Formatted supplementTariff", "5555.22.22 77", pivotLoaded2.CI_FormattedSupplementalTariff);
		}

		public void TestCI_FormattedSupplementalTariff()
		{
			ZString tariff = "1234567890";
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_SupplementalTariff = tariff;
			AssertEquals("CI_FormattedSupplementalTariff", "1234.56.78 90", pivot.CI_FormattedSupplementalTariff);
			tariff = "9876.54.32 10";
			pivot.CI_FormattedSupplementalTariff = "9876.54.32 10";
			AssertEquals("CI_FormattedSupplementalTariff", tariff, pivot.CI_FormattedSupplementalTariff);
		}

		public void TestDelete()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			var product = Factory.New<OrgSupplierPart>();
			product.RelatedOrganisations.AddOwner(org);
			product.OP_PartNum = "DZZA";
			var pivot = product.PivotsForBinding.AddNew();

			var attribute1 = pivot.Attributes1.AddNew();
			attribute1.BG_AttributeValue1 = "A";
			var attribute2 = pivot.Attributes2.AddNew();
			attribute2.BG_AttributeValue1 = "B";
			var attribute3 = pivot.Attributes3.AddNew();
			attribute3.BG_AttributeValue1 = "C";
			Factory.Save();

			var productPK = product.PK;
			var attrib1PK = attribute1.PK;
			var attrib2PK = attribute2.PK;
			var attrib3PK = attribute3.PK;
			product.Delete();
			AssertEquals(true, attribute1.IsDeleted);
			AssertEquals(true, attribute2.IsDeleted);
			AssertEquals(true, attribute3.IsDeleted);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertNull(newFactory.Load<OrgSupplierPart>(productPK));
			AssertNull(newFactory.Load<CusAttributeFilter>(attrib1PK));
			AssertNull(newFactory.Load<CusAttributeFilter>(attrib2PK));
			AssertNull(newFactory.Load<CusAttributeFilter>(attrib3PK));
		}

		public void TestPivotRefIsNotLoad()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "111";
			var pivot = part.PivotsForBinding.AddNew();
			var pivotRef = pivot.CusClassPartPivotRefs.AddNew();
			pivotRef.CIR_ReferenceType = "AGA";
			pivotRef.CIR_ReferenceNumber = "REF";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedPivot = newFactory.Load<BaseCusClassPartPivot>(pivot.PK);
			loadedPivot.LoadChildEditableObjects();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusClassPartPivotRef.Schema.TableName));
			AssertEquals(0, loadedPivot.CusClassPartPivotRefs.Count);

			loadedPivot.CusClassPartPivotRefs.Load();
			AssertEquals("No db hits", 0, newFactory.GetTableHitCount(CusClassPartPivotRef.Schema.TableName));
		}

		public void TestAttributes1()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			var attributeFilter = pivot.Attributes1.AddNew();
			AssertEquals(pivot.PK, attributeFilter.BG_CI);
			AssertEquals(nameof(CusAttributeFilter.AttributeFilterName.AT1), attributeFilter.BG_AttributeName);
		}

		public void TestAttributes2()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			var attributeFilter = pivot.Attributes2.AddNew();
			AssertEquals(pivot.PK, attributeFilter.BG_CI);
			AssertEquals(nameof(CusAttributeFilter.AttributeFilterName.AT2), attributeFilter.BG_AttributeName);
		}

		public void TestAttributes3()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			var attributeFilter = pivot.Attributes3.AddNew();
			AssertEquals(pivot.PK, attributeFilter.BG_CI);
			AssertEquals(nameof(CusAttributeFilter.AttributeFilterName.AT3), attributeFilter.BG_AttributeName);
		}

		public void TestSetDefaultValues()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			AssertEquals("", pivot.CI_TariffNum);
			AssertEquals(GlbCompany.CurrentCompany.Country.RN_Code, pivot.CI_RN_NKCountry);
			AssertEquals(ClassificationTypeList.Codes.HTB, pivot.CI_ChildType);
		}

		public void TestClone()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_AddInfo = "blah=ha";
			pivot.CI_CC = Factory.New<BaseCusClassification>().PK;
			pivot.CI_OP = Factory.New<OrgSupplierPart>().PK;
			pivot.CI_LastAuditedDate = ZDateTime.Today;
			pivot.CI_LastAuditedUser = "E";
			pivot.CI_CI_Parent = Factory.New<BaseCusClassPartPivot>().PK;
			pivot.CI_CGC_Catalog = Factory.New<BaseCusGoodsCatalog>().PK;

			var attrib1 = pivot.Attributes1.AddNew();
			attrib1.BG_AttributeOperator = "O1";
			attrib1.BG_AttributeValue1 = "V1";
			attrib1.BG_AttributeValue2 = "V2";

			var attrib2 = pivot.Attributes1.AddNew();
			attrib2.BG_AttributeOperator = "O2";
			attrib2.BG_AttributeValue1 = "V3";
			attrib2.BG_AttributeValue2 = "V4";

			var attrib3 = pivot.Attributes2.AddNew();
			attrib3.BG_AttributeOperator = "O3";
			attrib3.BG_AttributeValue1 = "V5";
			attrib3.BG_AttributeValue2 = "V6";

			var attrib4 = pivot.Attributes2.AddNew();
			attrib4.BG_AttributeOperator = "O4";
			attrib4.BG_AttributeValue1 = "V7";
			attrib4.BG_AttributeValue2 = "V8";

			var attrib5 = pivot.Attributes3.AddNew();
			attrib5.BG_AttributeOperator = "O5";
			attrib5.BG_AttributeValue1 = "V9";
			attrib5.BG_AttributeValue2 = "V10";

			var attrib6 = pivot.Attributes3.AddNew();
			attrib6.BG_AttributeOperator = "O6";
			attrib6.BG_AttributeValue1 = "V11";
			attrib6.BG_AttributeValue2 = "V12";

			var pivotClone = (BaseCusClassPartPivot)pivot.Clone();

			AssertEquals(pivot.CI_AddInfo, pivotClone.CI_AddInfo);
			AssertEquals(pivot.CI_CC, pivotClone.CI_CC);
			AssertEquals(ZGuid.Empty, pivotClone.CI_OP);
			AssertEquals(ZDateTime.Empty, pivotClone.CI_LastAuditedDate);
			AssertEquals(ZString.Empty, pivotClone.CI_LastAuditedUser);
			AssertEquals(ZGuid.Empty, pivotClone.CI_CI_Parent);
			AssertEquals(ZGuid.Empty, pivotClone.CI_CGC_Catalog);

			AssertEquals(2, pivotClone.Attributes1.Count);
			var attribClone1 = pivotClone.Attributes1[0];
			var attribClone2 = pivotClone.Attributes1[1];
			if (attribClone1.BG_AttributeValue1 == "V3")
			{
				attribClone1 = pivotClone.Attributes1[1];
				attribClone2 = pivotClone.Attributes1[0];
			}
			AssertAttrib(attribClone1, "O1", CusAttributeFilter.AttributeFilterName.AT1, "V1", "V2");
			AssertAttrib(attribClone2, "O2", CusAttributeFilter.AttributeFilterName.AT1, "V3", "V4");

			AssertEquals(2, pivotClone.Attributes2.Count);
			attribClone1 = pivotClone.Attributes2[0];
			attribClone2 = pivotClone.Attributes2[1];
			if (attribClone1.BG_AttributeValue1 == "V7")
			{
				attribClone1 = pivotClone.Attributes1[1];
				attribClone2 = pivotClone.Attributes1[0];
			}
			AssertAttrib(attribClone1, "O3", CusAttributeFilter.AttributeFilterName.AT2, "V5", "V6");
			AssertAttrib(attribClone2, "O4", CusAttributeFilter.AttributeFilterName.AT2, "V7", "V8");

			AssertEquals(2, pivotClone.Attributes3.Count);
			attribClone1 = pivotClone.Attributes3[0];
			attribClone2 = pivotClone.Attributes3[1];
			if (attribClone1.BG_AttributeValue1 == "V11")
			{
				attribClone1 = pivotClone.Attributes1[1];
				attribClone2 = pivotClone.Attributes1[0];
			}
			AssertAttrib(attribClone1, "O5", CusAttributeFilter.AttributeFilterName.AT3, "V9", "V10");
			AssertAttrib(attribClone2, "O6", CusAttributeFilter.AttributeFilterName.AT3, "V11", "V12");
		}

		public void TestIsImportClassification()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert("Is HTI (Import Classificaton)", pivot.IsImportClassification);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert("Is Not HTI", !pivot.IsImportClassification);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			Assert("Is Import Classification", pivot.IsImportClassification);
		}

		public void TestIsExportClassification()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert("Is HTE (ExportClassification)", pivot.IsExportClassification);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert("Is Not HTE", !pivot.IsExportClassification);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			Assert("Is Export Classification", pivot.IsExportClassification);
		}

		public void TestIsHTB()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
			Assert("Is IsHTB", pivot.IsHTB);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Assert("Is Not IsHTB", !pivot.IsHTB);
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Assert("Is Not IsHTB", !pivot.IsHTB);
		}

		void AssertAttrib(CusAttributeFilter attrib, ZString attribOperator, CusAttributeFilter.AttributeFilterName attributeName, ZString attributeValue1, ZString attributeValue2)
		{
			AssertEquals(attribOperator, attrib.BG_AttributeOperator);
			AssertEquals(attributeName.ToString(), attrib.BG_AttributeName);
			AssertEquals(attributeValue1, attrib.BG_AttributeValue1);
			AssertEquals(attributeValue2, attrib.BG_AttributeValue2);
		}

		public void TestHumanReadableName()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "ABC";
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_CC = classification.PK;
			pivot.CI_TariffNum = "0.1.2.3";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("Classification (HTI ABC)", pivot.HumanReadableName);

			pivot.CI_CC = ZGuid.Empty;
			AssertEquals("Classification (HTI 0.1.2.3)", pivot.HumanReadableName);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals("Classification (HTE 0.1.2.3)", pivot.HumanReadableName);
		}

		public void TestClearAuditOnChanged()
		{
			var pivot = (BaseCusClassPartPivot)GetNewBusinessObject();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			AssertChangeOfValueIsLoggedAndClearsAuditEntry(pivot, piv => piv.CI_ChildType = ClassificationTypeList.Codes.HTE);
			AssertChangeOfValueIsLoggedAndClearsAuditEntry(pivot, piv => piv.CI_ChildType = ClassificationTypeList.Codes.HTB);
		}

		void AssertChangeOfValueIsLoggedAndClearsAuditEntry(BaseCusClassPartPivot pivot, Action<BaseCusClassPartPivot> setPropertyFunc)
		{
			pivot.CI_LastAuditedUser = "USR";
			pivot.CI_LastAuditedDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			AssertEquals("USR", pivot.CI_LastAuditedUser);
			AssertEquals(ZDateTime.BrettsBirthday, pivot.CI_LastAuditedDate);

			var timeOfChange = ZDateTime.UtcNow;
			setPropertyFunc(pivot);
			Factory.Save();

			AssertEquals("", pivot.CI_LastAuditedUser);
			AssertEquals(ZDateTime.Empty, pivot.CI_LastAuditedDate);

			var logEntry = pivot.Logs.MostRecentLog;
			AssertEquals(Events.EditedARecord, (Event)logEntry.Event);
			AssertLessThanOrEqualTo(timeOfChange, logEntry.SL_PostedTimeUtc);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "sdkfjhsdfjkh";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "SDFskdjfh";
			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			return pivot;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var pivot = factory.New<BaseCusClassPartPivot>();
			var classification = factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "sdkfjhsdfjkh";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			var part = factory.New<OrgSupplierPart>();
			part.FillWithValidTestData();

			pivot.CI_CC = classification.PK;
			pivot.CI_OP = part.PK;
			return pivot;
		}

		public void TestDataRefreshBusSubscriberIsCalled()
		{
			var pivot = (BaseCusClassPartPivot)GetNewBusinessObjectForDeleteTest(Factory);
			var subscriber = new DummyDataRefreshBusSubscriber(Factory);
			Factory.Save();
			subscriber.UpdatedByDataRefreshWasCalled = false;
			var secondFactory = new BusinessObjectFactory();
			var secondPivot = secondFactory.Load<BaseCusClassPartPivot>(pivot.PK);
			secondPivot.CI_TariffNum = "21432";
			secondFactory.Save();
			AssertEquals(true, subscriber.UpdatedByDataRefreshWasCalled);
		}

		public void TestIPartProviderInterface()
		{
			var part = Factory.New<MasterFiles.Business.OrgSupplierPart>();
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part.PK;
			AssertEquals(part, pivot.Part);
		}

		public void TestITariffFormatProvider()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			AssertType<TariffFormatter>("TariffFormatter", ((ITariffFormatProvider)pivot).TariffFormatter);
		}

		public virtual void TestCI_ChildType()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertHasError(pivot1.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);

			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertNoError(pivot1.CI_ChildTypeInfo, pivot1.Validation.DuplicateAttributeForHTIWithoutAttributes);
		}

		public void TestCI_ChildTypeMaxLength()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertNoExceptionThrown(() => pivot.CI_ChildType = "HTI:3926.90.69.79F, HTI:3926.90.69.79F");
			AssertEquals(ClassificationTypeList.Codes.HTI, pivot.CI_ChildType);
		}

		public void TestDefaultUniversalTariffProperties()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			AssertEquals("UseUniversalTariff", true, pivot.UseUniversalTariff);
			AssertEquals("UniversalTariffType", Constants.TariffTypes.HarmonizedSystem, pivot.UniversalTariffType);
		}

		public void TestCI_AddInfoSet()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();

			AssertEquals(ZString.Empty, pivot.CI_AddInfo);
			AssertEquals(ZString.Empty, pivot.CI_NAddInfo);

			pivot.CI_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", pivot.CI_AddInfo);
			AssertEquals(ZString.Empty, pivot.CI_NAddInfo);
		}

		public void TestCI_AddInfoSet_WithBaseAddInfo()
		{
			var pivot = Factory.New<BaseCusClassPartPivotWithBaseAddInfoForTesting>();

			AssertEquals(ZString.Empty, pivot.CI_AddInfo);
			AssertEquals(ZString.Empty, pivot.CI_NAddInfo);

			pivot.CI_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", pivot.CI_AddInfo);
			AssertEquals("NString=def", pivot.CI_NAddInfo);
		}

		public void TestCI_AddInfoSet_WithAddInfoWrapper()
		{
			var pivot = Factory.New<BaseCusClassPartPivotWithAddInfoWrapperForTesting>();

			AssertEquals(ZString.Empty, pivot.CI_AddInfo);
			AssertEquals(ZString.Empty, pivot.CI_NAddInfo);
			AssertEquals(ZString.Empty, pivot.CI_String);
			AssertEquals(ZString.Empty, pivot.CI_NString);

			pivot.CI_AddInfo = "String=abc*NString=def";

			AssertEquals("String=abc*NString=def", pivot.CI_AddInfo);
			AssertEquals("", pivot.CI_NAddInfo);
			AssertEquals("abc", pivot.CI_String);
			AssertEquals("", pivot.CI_NString);

			pivot.CI_NAddInfo = "String=ghi*NString=jkl";

			AssertEquals("String=abc*NString=def", pivot.CI_AddInfo);
			AssertEquals("String=ghi*NString=jkl", pivot.CI_NAddInfo);
			AssertEquals("abc", pivot.CI_String);
			AssertEquals("jkl", pivot.CI_NString);
		}

		public void TestITypeDeciderContext()
		{
			CombineAssertions(() =>
			{
				var pivot = Factory.New<BaseCusClassPartPivot>();
				pivot.CI_RN_NKCountry = ZString.Empty;
				AssertEquals("From CurrentCompany", "ER", (pivot as ITypeDeciderContext).Country);

				pivot.CI_RN_NKCountry = "CN";
				AssertEquals("From CI_RN_NKCountry", "CN", (pivot as ITypeDeciderContext).Country);
			});
		}

		public void TestGoodsCatalog()
		{
			var goodsCatalog = Factory.NewWithValidTestData<BaseCusGoodsCatalog>();
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_CGC_Catalog = goodsCatalog.PK;

			AssertSame("GoodsCatalog.PK", goodsCatalog, pivot.GoodsCatalog);
		}

		public void TestDefaultDataGroupingForTariffs()
		{
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_RN_NKCountry = Core.Constants.CountryCodes.Guadeloupe;
			AssertEquals("Default DataGrouping For Tariffs", Core.Constants.CountryCodes.France, pivot.DefaultDataGroupingForTariffs);
		}

		public void TestUniversalTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(CountryCodes.Eritrea, "HSN");
			helper.LoadOrCreateNewTariff(CountryCodes.Eritrea, tariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.LoadOrCreateNewTariff(CountryCodes.Eritrea, tariffType.PK, "56049001", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.LoadOrCreateNewTariff(CountryCodes.Eritrea, tariffType.PK, "56049002", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			var catalog = Factory.New<BaseCusGoodsCatalog>();
			catalog.CGC_Tariff = "56049000";

			var classification = Factory.New<BaseCusClassification>();
			classification.CC_TariffNum = "56049001";

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_RN_NKCountry = CountryCodes.Eritrea;
			AssertNull("UniversalTariff should be null", pivot.UniversalTariff);

			pivot.CI_CGC_Catalog = catalog.PK;
			AssertEquals("ZZ1_TariffCode should be '56049000'", "56049000", pivot.UniversalTariff.ZZ1_TariffCode);

			pivot.CI_CGC_Catalog = ZGuid.Empty;
			pivot.CI_TariffNum = ZString.Empty;
			pivot.CI_CC = classification.PK;
			AssertEquals("ZZ1_TariffCode should be '56049001'", "56049001", pivot.UniversalTariff.ZZ1_TariffCode);

			pivot.CI_CGC_Catalog = ZGuid.Empty;
			pivot.CI_CC = ZGuid.Empty;
			pivot.CI_TariffNum = "56049002";
			AssertEquals("ZZ1_TariffCode should be '56049002'", "56049002", pivot.UniversalTariff.ZZ1_TariffCode);
		}

		class DummyDataRefreshBusSubscriber : IDataRefreshBusSubscriber, IService
		{
			public DummyDataRefreshBusSubscriber(BusinessObjectFactory factory)
			{
				Factory = factory;
				var refreshManager = new DataRefreshManager();
				refreshManager.StartManaging(CusClassPartPivotSchema.Constants.TableName, this);
			}

			#region IDataRefreshBusSubscriber Members

			public BusinessObjectFactory Factory
			{
				get;
				private set;
			}

			public bool UpdatedByDataRefreshWasCalled;
			public void UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
			{
				UpdatedByDataRefreshWasCalled = true;
			}

			bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => false;

			#endregion
		}

		sealed class BaseCusClassPartPivotWithBaseAddInfoForTesting : BaseCusClassPartPivot, IAddInfoManager, INAddInfoSupporter
		{
			public BaseCusClassPartPivotWithBaseAddInfoForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public TestAddInfo AddInfo => addInfo ?? (addInfo = new TestAddInfo(this));
			TestAddInfo addInfo;

			IAddInfo IAddInfoManager.AddInfo => AddInfo;
			public ZPropertyInfoString NAddInfoProperty => CI_NAddInfoInfo as ZPropertyInfoString;
		}

		sealed class BaseCusClassPartPivotWithAddInfoWrapperForTesting : BaseCusClassPartPivot, IAddInfoManagerWithSchema
		{
			public BaseCusClassPartPivotWithAddInfoWrapperForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				addInfo = new AddInfoWrapper<BaseCusClassPartPivotWithAddInfoWrapperForTesting>(
					this,
					Schema.CI_AddInfo,
					() => AddInfoNamesMapping,
					Schema.CI_NAddInfo,
					() => NAddInfoNamesMapping
				);
			}

			public ZString CI_String
			{
				get => CI_StringData.Value;
				set
				{
					SetNonPersistentPropertyValue(CI_StringInfo, ref CI_StringData.Value, value);
				}
			}
			public ZPropertyInfo CI_StringInfo => GetZPropertyInfo(nameof(CI_String));
			AddInfoPropertyData<ZString> CI_StringData => ci_String ?? (ci_String = new AddInfoPropertyData<ZString>(nameof(CI_String)));
			AddInfoPropertyData<ZString> ci_String;

			public ZString CI_NString
			{
				get => CI_NStringData.Value;
				set
				{
					SetNonPersistentPropertyValue(CI_NStringInfo, ref CI_NStringData.Value, value);
				}
			}
			public ZPropertyInfo CI_NStringInfo => GetZPropertyInfo(nameof(CI_NString));
			AddInfoPropertyData<ZString> CI_NStringData => ci_NString ?? (ci_NString = new AddInfoPropertyData<ZString>(nameof(CI_NString)));
			AddInfoPropertyData<ZString> ci_NString;

			IDictionary<string, IAddInfoPropertyData> AddInfoNamesMapping => addInfoNamesMapping ?? (addInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData> { { "String", CI_StringData } });
			IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping;

			IDictionary<string, IAddInfoPropertyData> NAddInfoNamesMapping => nAddInfoNamesMapping ?? (nAddInfoNamesMapping = new Dictionary<string, IAddInfoPropertyData> { { "NString", CI_NStringData } });
			IDictionary<string, IAddInfoPropertyData> nAddInfoNamesMapping;

			IAddInfo IAddInfoManager.AddInfo => addInfo;
			readonly IAddInfo addInfo;

			ITableSchema IAddInfoManagerWithSchema.AddInfoSchema => TWJobDeclarationSchema.Instance;
		}
	}
}
