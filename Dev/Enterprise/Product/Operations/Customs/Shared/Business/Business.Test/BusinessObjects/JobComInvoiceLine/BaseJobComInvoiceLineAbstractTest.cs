using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(BaseJobComInvoiceLine))]
	public abstract class BaseJobComInvoiceLineAbstractTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestGetJI_Calc_CIF_NoNullReferenceException()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
				AssertEquals("No JobComInvoiceHeader", 0m, invoiceLine.JI_Calc_CIF);

				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				AssertEquals("No JobDeclaration", 0m, invoiceLine2.JI_Calc_CIF);

				var declaration = Factory.New<BaseJobDeclaration>();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.WipeJzForTesting();
				AssertEquals("No JobDeclaration + JobComInvoiceHeader linked", 0m, invoiceLine3.JI_Calc_CIF);
			});
		}

		public void TestVehicleRelationship_One_HandleUniqueIndex()
		{
			if (InvoiceLine.VehicleRelationship == VehicleRelationshipType.One)
			{
				Factory.RefreshEnabled = false;

				// Get the InvoiceLine to create the row and save in database
				_ = InvoiceLine;
				Factory.Save();

				var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var invoiceLineInAnotherFactory = anotherFactory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);

				var vehicle = invoiceLine.Vehicles.AddNew();
				vehicle.CVH_VehicleIdentificationNumber = "VIN1";
				var vehicleInAnotherFactory = invoiceLineInAnotherFactory.Vehicles.AddNew();
				vehicleInAnotherFactory.CVH_VehicleIdentificationNumber = "VIN2";

				Factory.Save();

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					try
					{
						anotherFactory.Save();
						Fail($"First save should not have succeeded, please make sure that there is a unique index on CVH_ParentID for CVH_DataModel = '{invoiceLine.JI_DataModel}'.");
					}
					catch (Exception ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}

					AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals("Vehicle from another factory should be deleted", true, vehicleInAnotherFactory.IsDeleted);

					anotherFactory.Save();

					AssertEquals("Invoice line in another factory should have the saved vehicle", 1, invoiceLineInAnotherFactory.Vehicles.Count);
					AssertEquals("Vehicle in another factory should be using the existing vehicle now", vehicle.PK, invoiceLineInAnotherFactory.Vehicles[0].PK);
				});
			}
			else
			{
				Assert("All good", true);
			}
		}

		public void TestOverridingLinePriceForBalanceCalcShouldOverride_GetJZ_Calc_LinesEnteredRelatedProperties()
			=> AssertOverridingPropertyShouldOverride_GetJZ_Calc_LinesEnteredRelatedProperties(
				nameof(BaseJobComInvoiceLine.LinePriceForBalanceCalc));

		public void TestOverridingIsValidForLineTotalCalculationShouldOverride_GetJZ_Calc_LinesEnteredRelatedProperties()
			=> AssertOverridingPropertyShouldOverride_GetJZ_Calc_LinesEnteredRelatedProperties(
				nameof(BaseJobComInvoiceLine.IsValidForLineTotalCalculation));

		void AssertOverridingPropertyShouldOverride_GetJZ_Calc_LinesEnteredRelatedProperties(string propertyName)
		{
			var isOverridingLinePriceForBalance = GetExpectedBusinessObjectType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null;

			if (isOverridingLinePriceForBalance)
			{
				var allCustomsAssemblies = BuildXml.Instance
					.GetAllAssembliesToBuild(false)
					.Where(c => c.StartsWith("Enterprise.Customs.", StringComparison.OrdinalIgnoreCase))
					.ToDictionary(c => System.IO.Path.GetFileNameWithoutExtension(c))
					.Keys
					.ToArray();
				var invoiceHeaderType = GetExpectedBusinessObjectType().GetProperties().Where(x => x.Name.Equals("InvoiceHeader")).First().PropertyType;
				var invoiceHeaderTestGenericType = typeof(BaseJobComInvoiceHeaderAbstractTest<,>);
				var retriever = new SubClassRetriever(allCustomsAssemblies, invoiceHeaderTestGenericType)
				{
					IncludeAbstractClasses = true,
					IncludeAutoGeneratedCode = true,
					IncludeClientDlls = true,
					IncludeNestedClasses = true,
					IncludePrivateNestedClasses = true,
					IncludeTestClasses = true,
					IncludeNonAutoGeneratedCode = true,
				};

				var invoiceHeaderTestType = retriever.Retrieve().Where(t =>
				{
					var genericType = GetGenericBaseType(t);
					return genericType.GenericTypeArguments.Length > 1 && genericType.GenericTypeArguments[1].Equals(invoiceHeaderType);
				}).First();

				Assert(
					$"When overriding {propertyName} please make sure you invalidate the cache appropriately, and include related properties by overriding GetJZ_Calc_LinesEnteredRelatedProperties in {invoiceHeaderTestType}",
					HasOverriddenProperty(invoiceHeaderTestType, invoiceHeaderTestGenericType));
			}
			else
			{
				Assert(true);
			}
		}

		bool HasOverriddenProperty(Type type, Type invoiceHeaderTestGenericType)
		{
			var workingWithBaseType = type.FullName.Equals("Enterprise.Customs.Business.Testing.BaseJobComInvoiceHeaderBaseOnlyTest");
			while (type.BaseType != null && (workingWithBaseType || type != invoiceHeaderTestGenericType))
			{
				if (type.GetMethod("GetJZ_Calc_LinesEnteredRelatedProperties", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null)
				{
					return true;
				}

				type = type.BaseType;
			}

			return false;
		}

		Type GetGenericBaseType(Type type)
		{
			while (!type.IsGenericType && type.BaseType != null)
			{
				type = type.BaseType;
			}

			return type;
		}

		public void TestFinishUniversalCopy()
		{
			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobComInvoiceLineSchema.Constants.JI_JZ, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var invoiceHeader = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			var line = invoiceHeader.InvoiceLines.AddNew();
			AssertEquals((ZShort)1, line.JI_LineNo);

			var copyline = (BaseJobComInvoiceLine)new BusinessObjectCopyManager().Copy(line, copyTree).Object;
			AssertEquals((ZShort)2, copyline.JI_LineNo);
		}

		public void TestFinishUniversalCopyWhenCopyFromParent()
		{
			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobComInvoiceHeaderSchema.Constants.JZ_Description, CopyMethod = CopyMethod.Copy });
			var invoiceLineEntityNode = new EntityCopyTemplateNode { Name = "InvoiceLines" };
			invoiceLineEntityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = JobComInvoiceLineSchema.Constants.JI_LineNo, CopyMethod = CopyMethod.Copy });
			var collectionNode = new CollectionCopyTemplateNode
			{
				Name = "InvoiceLines",
				ItemPropertyName = JobComInvoiceLineSchema.Constants.JI_JZ,
				ItemsTableName = JobComInvoiceLineSchema.Constants.TableName,
				InnerNode = invoiceLineEntityNode,
				CopyMethod = CollectionCopyMethod.All
			};
			entityNode.Nodes.Add(collectionNode);
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var invoiceHeader = Factory.NewWithValidTestData<BaseJobComInvoiceHeader>();
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			var copyHeader = (BaseJobComInvoiceHeader)new BusinessObjectCopyManager().Copy(invoiceHeader, copyTree).Object;
			CombineAssertions(() =>
			{
				AssertEquals("line 1 number not changed after copy", (ZShort)1, copyHeader.InvoiceLines[0].JI_LineNo);
				AssertEquals("line 2 number not changed after copy", (ZShort)2, copyHeader.InvoiceLines[1].JI_LineNo);
			});
		}

		public void TestCustomsUnitDefaultingStrategyIsInitialised()
		{
			var invoiceLine = Factory.NewWithValidTestData<BaseJobComInvoiceLine_ForCustomsUnitDefaultingStrategyTest>();
			AssertEquals("Should initialise CustomsUnitDefaultingStrategy during SetDefaultValues for new invoice lines.", true, invoiceLine.CustomsUnitDefaultingStrategyInitialised);

			Factory.Save();

			try
			{
				TypeDecider.AddSubstitution(typeof(BaseJobDeclaration), typeof(BaseJobComInvoiceLine_ForCustomsUnitDefaultingStrategyTest));
				var invoiceLineInOtherFactory = new BusinessObjectFactory().Load<BaseJobComInvoiceLine_ForCustomsUnitDefaultingStrategyTest>(invoiceLine.PK);
				AssertEquals("Should initialise CustomsUnitDefaultingStrategy OnLoad.", true, invoiceLineInOtherFactory.CustomsUnitDefaultingStrategyInitialised);
			}
			finally
			{
				TypeDecider.RemoveSubstitution(typeof(BaseJobDeclaration));
			}
		}

		public virtual void TestDefaultDataGroupingCode()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Argentina;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				var expectedCountry = GlbCompany.CurrentCompany.Country.Code;
				AssertEquals("Test Default Data Grouping Code - no linked to invoice", expectedCountry, invoiceLine1.GetDefaultDataGroupingCode());
				AssertEquals("Test Default Tariff Data Grouping Code - no linked to invoice", expectedCountry, invoiceLine1.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
				AssertEquals("Test Default Duty Data Grouping Code - no linked to invoice", expectedCountry, invoiceLine1.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
				AssertEquals("Test Default CusProcedure Data Grouping Code - no linked to invoice", expectedCountry, invoiceLine1.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
				AssertEquals("Test Default AdditionalDocumentCodes Data Grouping Code - no linked to invoice", expectedCountry, invoiceLine1.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				expectedCountry = NoVariableDefaultDataGroupingCodeCountry ?? GlbCompany.CurrentCompany.Country.Code;
				AssertEquals("Test Default Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode());
				AssertEquals("Test Default Tariff Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
				AssertEquals("Test Default Duty Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
				AssertEquals("Test Default CusProcedure Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
				AssertEquals("Test Default AdditionalDocumentCodes Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));

				declaration.JE_GB = branch.PK;
				expectedCountry = NoVariableDefaultDataGroupingCodeCountry ?? Core.Constants.CountryCodes.Argentina;
				AssertEquals("Test Default Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode());
				AssertEquals("Test Default Tariff Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff));
				AssertEquals("Test Default Duty Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.DutyRateCodes));
				AssertEquals("Test Default CusProcedure Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.CusProcedure));
				AssertEquals("Test Default AdditionalDocumentCodes Data Grouping Code", expectedCountry, invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.AdditionalDocumentCodes));
			});
		}
		protected virtual ZString? NoVariableDefaultDataGroupingCodeCountry => null;

		public void TestUniversalCopyAddInfo()
		{
			AssertNotNull(typeof(BaseJobComInvoiceLine).GetCustomAttribute<UniversalCopyAddInfoAttribute>());
		}

		public void TestOnLoaded_NoErrorForLoadingDeletedInvoiceLine()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.InvoiceLines.AddNew();
			var childLine = invoice.InvoiceLines.AddNew();
			childLine.JI_ParentID = parentLine.PK;
			parentLine.JI_PartNo = "TEST";
			parentLine.JI_OP = ZGuid.Empty;
			parentLine.JI_Tariff = "";
			parentLine.JI_CC = ZGuid.Empty;
			parentLine.JI_CustomsQuantity = 0;
			parentLine.JI_AddInfo = "CI_PreviousPivot=9a3dcc83-eb90-4bcd-876d-ea5b91815eb6*IsParent=Y";
			Factory.Save();

			AssertNoExceptionThrown(delegate
			{
				parentLine.OnLoaded();
				childLine.OnLoaded();
			});
		}

		public void TestDistinctPackageTypes()
		{
			var declaration = GetJobDeclaration();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			bill.PackingGroups.AddNew();

			var pac1 = bill.PackingGroups[0].Packages.AddNew();
			pac1.CW_PackType = Core.Constants.PkgUnit.Bag;
			var pac2 = bill.PackingGroups[0].Packages.AddNew();
			pac2.CW_PackType = Core.Constants.PkgUnit.BaleCompressed;
			var pac3 = bill.PackingGroups[0].Packages.AddNew();
			pac3.CW_PackType = Core.Constants.PkgUnit.Carton;

			invoiceLine.ToggleLinkageWithPackage(pac1, true);
			invoiceLine.ToggleLinkageWithPackage(pac2, true);
			invoiceLine.ToggleLinkageWithPackage(pac3, true);
			AssertContainsExactElementsInAnyOrder(new ZString[] { Core.Constants.PkgUnit.Bag, Core.Constants.PkgUnit.BaleCompressed, Core.Constants.PkgUnit.Carton }, ((ICusLinkPackageSupporter)invoiceLine).DistinctPackageTypes);

			var pac4 = bill.PackingGroups[0].Packages.AddNew();
			pac4.CW_PackType = Core.Constants.PkgUnit.Bag;
			invoiceLine.ToggleLinkageWithPackage(pac4, true);
			AssertContainsExactElementsInAnyOrder(new ZString[] { Core.Constants.PkgUnit.Bag, Core.Constants.PkgUnit.BaleCompressed, Core.Constants.PkgUnit.Carton }, ((ICusLinkPackageSupporter)invoiceLine).DistinctPackageTypes);

			pac2.CW_MarksAndNos = pac3.CW_MarksAndNos = "N/M";
			AssertContainsExactElementsInAnyOrder(new ZString[] { Core.Constants.PkgUnit.Bag, pac2.PK.ToString(), pac3.PK.ToString() }, ((ICusLinkPackageSupporter)invoiceLine).DistinctPackageTypes);
		}

		public void TestWeightAndVolumnConvertionForLargeNumber()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_Code = "ORGAUSYD";
			consignor.OH_IsConsignor = ZBool.True;
			consignor.OH_RL_NKClosestPort = "AUSYD";
			var consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AMYT";
			commodity.RH_IsPerishable = true;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PART1";
			product.OP_Weight = 999999.99m;
			product.OP_WeightUQ = "KG";
			product.OP_NetWeight = 999999.99m;
			product.OP_Cubic = 999999.99m;
			product.OP_CubicUQ = "M3";
			product.RelatedOrganisations.AddSupplier(consignor);
			product.RelatedOrganisations.AddOwner(consignee);
			product.OP_RH_NKCommodityCode = commodity.RH_Code;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.MoreCodes.AdvanceShippingNotice;
			invoice.JZ_OH_Buyer = consignee.PK;
			invoice.JZ_OH_Supplier = consignor.PK;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 10m;
			invoiceLine.JI_PartNo = product.OP_PartNum;
			AssertNotNull(invoiceLine.Part);
			AssertNoExceptionThrown(() => { Factory.Save(); });

			var reLoadLine = Factory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
			AssertEquals("JI_Weight", 0m, reLoadLine.JI_Weight);
			AssertEquals("JI_WeightUQ", "KG", reLoadLine.JI_WeightUQ);
			AssertEquals("JI_NetWeight", 0m, reLoadLine.JI_NetWeight);
			AssertEquals("JI_NetWeightUQ", "KG", reLoadLine.JI_NetWeightUQ);
			AssertEquals("JI_Volume", 0m, reLoadLine.JI_Volume);
			AssertEquals("JI_VolumeUQ", "M3", reLoadLine.JI_VolumeUQ);
		}

		public void TestSuspendSettingOfSetterSuspender()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "BOX";

			var setterSuspender = invoiceLine.SetterSuspender;

			using (setterSuspender.SuspendSetting(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ))
			{
				invoiceLine.JI_InvoiceUQ = "UNT";

				Assert("Should is suspended.", invoiceLine.SetterSuspender.IsSetterSuspended(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
				AssertEquals("Invoice Quantity Unit setter should be suspended", "BOX", invoiceLine.JI_InvoiceUQ);

				using (setterSuspender.SuspendSetting(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ))
				{
					invoiceLine.JI_InvoiceUQ = "UNT";

					Assert("Should is suspended.", invoiceLine.SetterSuspender.IsSetterSuspended(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
					AssertEquals("Invoice Quantity Unit setter should still be suspended", "BOX", invoiceLine.JI_InvoiceUQ);
				}

				invoiceLine.JI_InvoiceUQ = "UNT";

				Assert("Should is suspended.", invoiceLine.SetterSuspender.IsSetterSuspended(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
				AssertEquals("Invoice Quantity Unit setter should still be suspended", "BOX", invoiceLine.JI_InvoiceUQ);
			}

			invoiceLine.JI_InvoiceUQ = "UNT";

			Assert("Should is not suspended.", !invoiceLine.SetterSuspender.IsSetterSuspended(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
			AssertEquals("Invoice Quantity Unit setter should not be suspended anymore", "UNT", invoiceLine.JI_InvoiceUQ);
		}

		public void TestResumeSettingOfSetterSuspender()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_InvoiceUQ = "BOX";

			var setterSuspender = invoiceLine.SetterSuspender;

			using (setterSuspender.SuspendSetting(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ))
			{
				invoiceLine.JI_InvoiceUQ = "UNT";

				Assert("Should is suspended.", invoiceLine.SetterSuspender.IsSetterSuspended(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
				AssertEquals("Invoice Quantity Unit setter should be suspended", "BOX", invoiceLine.JI_InvoiceUQ);

				using (setterSuspender.ResumeSetting(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ))
				{
					invoiceLine.JI_InvoiceUQ = "UNT";

					Assert("Should is not suspended from the method - ResumeSetting.", !invoiceLine.SetterSuspender.IsSetterSuspended(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
					AssertEquals("Invoice Quantity Unit setter should not be suspended", "UNT", invoiceLine.JI_InvoiceUQ);
				}

				invoiceLine.JI_InvoiceUQ = "";

				Assert("Should is suspended.", invoiceLine.SetterSuspender.IsSetterSuspended(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
				AssertEquals("Invoice Quantity Unit setter should still be suspended", "UNT", invoiceLine.JI_InvoiceUQ);
			}

			invoiceLine.JI_InvoiceUQ = "";

			Assert("Should is not suspended.", !invoiceLine.SetterSuspender.IsSetterSuspended(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ));
			AssertEquals("Invoice Quantity Unit setter should not be suspended anymore", "", invoiceLine.JI_InvoiceUQ);
		}

		public void TestJI_ClassUsageComment()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			Assert("Should always be read only.", invoiceLine.JI_ClassUsageCommentInfo.ReadOnly);

			invoiceLine.JI_ClassUsageComment = "AAA";
			invoiceLine.JI_GS_NKClassUsageCommentReviewer = "TST";

			invoiceLine.JI_ClassUsageComment = "BBB";
			Assert("Should clear the value on JI_GS_NKClassUsageCommentReviewer.", invoiceLine.JI_GS_NKClassUsageCommentReviewer.IsEmpty);
		}

		public void TestJI_GS_NKClassUsageCommentReviewer()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			Assert("Should always be read only.", invoiceLine.JI_GS_NKClassUsageCommentReviewerInfo.ReadOnly);
		}

		public void TestJI_IsClassUsageCommentRead()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			Assert("Should default to true.", invoiceLine.JI_IsClassUsageCommentReadInfo.ReadOnly);

			invoiceLine.JI_ClassUsageComment = "AAA";
			Assert("Should be false as the JI_ClassUsageComment is not empty.", !invoiceLine.JI_IsClassUsageCommentReadInfo.ReadOnly);

			invoiceLine.JI_GS_NKClassUsageCommentReviewer = "TST";
			Assert("Should be false as the JI_GS_NKClassUsageCommentReviewer is not empty.", !invoiceLine.JI_IsClassUsageCommentReadInfo.ReadOnly);

			invoiceLine.JI_IsClassUsageCommentRead = true;
			AssertEquals("Should set the JI_GS_NKClassUsageCommentReviewer with the current staff code.", GlbStaff.CurrentUser.GS_Code, invoiceLine.JI_GS_NKClassUsageCommentReviewer);
			Assert("Should be false as the JI_GS_NKClassUsageCommentReviewer is not empty.", !invoiceLine.JI_IsClassUsageCommentReadInfo.ReadOnly);
		}

		public virtual void TestConsigneeAddressForDocument()
		{
			var consigneeAddress = Factory.New<OrgAddress>();
			InvoiceLine.JI_OA_ConsigneeAddress = consigneeAddress.PK;
			AssertEquals(InvoiceLine.ConsigneeAddressForDocument, InvoiceLine.ConsigneeAddress);
		}

		public virtual void TestWipeNKTaxType()
		{
			const string NKTaxType = "6";

			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "IFD");
			procedure1.ZZ6_CalculateVAT = false;
			Factory.Save();

			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "IMP";

			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.JI_ZZF_NKTaxType = NKTaxType;
			line.JI_Procedure = "1111111";
			Assert(line.ShouldWipeNKTaxType);
			AssertEquals(line.ShouldWipeNKTaxType ? "" : NKTaxType, line.JI_ZZF_NKTaxType);

			line.JI_ZZF_NKTaxType = NKTaxType;
			line.JI_Procedure = "1111222";
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals(NKTaxType, line.JI_ZZF_NKTaxType);

			procedure1.ZZ6_CalculateVAT = true;
			Factory.Save();
			line.JI_Procedure = "1111111";
			Assert(!line.ShouldWipeNKTaxType);
			AssertEquals(NKTaxType, line.JI_ZZF_NKTaxType);
		}

		public void TestProvProgTariffDuty()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals(ZString.Empty, commonInvoice.ProvProgTariff);
			AssertEquals(ZString.Empty, commonInvoice.ProvProgDutyRate);
		}

		public void TestIBOMExpanderForNonSupportInvoiceLines()
		{
			var zaInvoiceLine = (BaseJobComInvoiceLine)Factory.New<Integration.Customs.ZA.IJobComInvoiceLine>();
			var auInvoiceLine = (BaseJobComInvoiceLine)Factory.New<Integration.Customs.AU.IJobComInvoiceLine>();

			AssertNoExceptionThrown(delegate
			{ var accessed = (zaInvoiceLine).BOMParentLineNumber; });
			AssertNoExceptionThrown(delegate
			{ var accessed = (auInvoiceLine).BOMParentLineNumber; });
		}

		public virtual void TestChargeTypeList()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			ICommonInvoice commonInvoice = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			chargeTypeList1 = commonInvoice.ChargeTypeList;
			chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);
		}

		public void TestSupportInvoiceLinePackage()
		{
			AssertEquals("There are two virtual properties for SupportsChcPivotBetweenInvoiceLineAndPacking, one in JobDeclaration and another in InvoiceLine. They should return an identical value", InvoiceLine.Declaration.SupportsChcPivotBetweenInvoiceLineAndPacking, InvoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking);
		}

		public virtual void TestJI_PreviousProcedure()
		{
			void TestRunner(ZString fullCode, ZString expectedPPC)
			{
				InvoiceLine.JI_Procedure = fullCode;
				AssertEquals(fullCode, InvoiceLine.JI_Procedure);
				AssertEquals(expectedPPC, InvoiceLine.JI_Calc_PreviousProcedure);
			}

			CombineAssertions(delegate
			{
				TestRunner("1234", "34");
				TestRunner("12", "");
				TestRunner("", "");
				TestRunner("  34", "34");
			});
		}

		public void TestGSTVATAmountForLandedCost()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();

			var fee2 = entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 200m);
			AssertEquals("invoiceLine.GSTVATAmount", 200m, invoiceLine.GSTVATAmountForLandedCost);
			fee2.CF_IsLandedCostOnly = true;
			AssertEquals("invoiceLine.GSTVATAmount", 200m, invoiceLine.GSTVATAmountForLandedCost);

			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.VAT, 400m);
			AssertEquals("entryLine.GSTVATAmount", 200m, invoiceLine.GSTVATAmountForLandedCost);
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals("entryLine.GSTVATAmount", 400m, invoiceLine.GSTVATAmountForLandedCost);
		}

		public void TestGetPivotByTypeAndOwnerSupplier()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Chad))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_OH_Supplier = supplier.PK;

				var invoice = declaration.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				line.JI_PartNo = "1234";

				var part = Factory.New<OrgSupplierPart>();
				var relatedOrg = part.RelatedOrganisations.AddNew();
				relatedOrg.OU_OH = supplier.PK;
				relatedOrg.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
				part.OP_PartNum = "1234";
				line.SetPartForTesting(part);
				AssertNull("pre-req.", line.GetPivot());

				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				AssertEquals(pivot, line.GetPivot());

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				declaration.JE_OH_Importer = importer.PK;
				AssertNull(line.GetPivot());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals(pivot, line.GetPivot());

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals(null, line.GetPivot());

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				AssertNull(line.GetPivot());

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				AssertNull(line.GetPivot());
			}
		}

		public void TestDelete_ResetInvoiceHeadersAndLinesOfEachEntryInDeclaration()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			var invoice2 = Declaration.Invoices.AddNew();
			var invoiceLine11 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine12 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine13 = invoice1.JobComInvoiceLines.AddNew();
			var invoiceLine21 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine22 = invoice2.JobComInvoiceLines.AddNew();
			var invoiceLine23 = invoice2.JobComInvoiceLines.AddNew();

			var entry1 = Declaration.CustomsEntryHeaders.AddNew();
			var entry2 = Declaration.CustomsEntryHeaders.AddNew();
			var entry3 = Declaration.CustomsEntryHeaders.AddNew();
			var entryLine11 = entry1.MergedLines.AddNew();
			var entryLine12 = entry1.MergedLines.AddNew();
			var entryLine21 = entry2.MergedLines.AddNew();
			var entryLine22 = entry2.MergedLines.AddNew();
			var entryLine31 = entry3.MergedLines.AddNew();
			var entryLine32 = entry3.MergedLines.AddNew();

			invoiceLine11.JI_CL = entryLine11.PK;
			invoiceLine12.JI_CL = entryLine21.PK;
			invoiceLine13.JI_CL = entryLine31.PK;
			invoiceLine21.JI_CL = entryLine12.PK;
			invoiceLine22.JI_CL = entryLine22.PK;
			invoiceLine23.JI_CL = entryLine32.PK;

			if (Declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine)
			{
				var link111 = invoiceLine11.AdditionalEntryLineLinks.AddNew();
				link111.BU_CL = entryLine31.PK;
				var link112 = invoiceLine11.AdditionalEntryLineLinks.AddNew();
				link112.BU_CL = entryLine32.PK;
			}

			CombineAssertions(() =>
			{
				AssertEquals("entry1 before invoice delete", 2, entry1.InvoiceHeaders.Length);
				AssertEquals("entry2 before invoice delete", 2, entry2.InvoiceHeaders.Length);
				AssertEquals("entry3 before invoice delete", 2, entry3.InvoiceHeaders.Length);
				AssertEquals("entry1 beforce line delete", 2, entry1.InvoiceLines.Count());
				AssertEquals("entry2 beforce line delete", 2, entry2.InvoiceLines.Count());
				AssertEquals("entry3 beforce line delete", Declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine ? 4 : 2, entry3.InvoiceLines.Count());
				invoice1.Delete();
				AssertEquals("entry1 after invoice delete", 1, entry1.InvoiceHeaders.Length);
				AssertEquals("entry2 after invoice delete", 1, entry2.InvoiceHeaders.Length);
				AssertEquals("entry3 after invoice delete", 1, entry3.InvoiceHeaders.Length);
				AssertEquals("entry1 after line delete", 1, entry1.InvoiceLines.Count());
				AssertEquals("entry2 after line delete", 1, entry2.InvoiceLines.Count());
				AssertEquals("entry3 after line delete", 1, entry3.InvoiceLines.Count());
			});
		}

		public void TestInvoicesSortedByDisplaySequence()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "Z";
			Declaration.InvoiceLines.AddNew();

			var invoice2 = Declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "J";

			var invoice3 = Declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "F";

			AssertEquals(invoice1.JZ_InvoiceNumber, Declaration.SortedInvoiceList[0].Code);
			AssertEquals(invoice2.JZ_InvoiceNumber, Declaration.SortedInvoiceList[1].Code);
			AssertEquals(invoice3.JZ_InvoiceNumber, Declaration.SortedInvoiceList[2].Code);

			invoice2.JZ_InvoiceDisplaySequence = 10;

			AssertEquals(invoice1.JZ_InvoiceNumber, Declaration.SortedInvoiceList[0].Code);
			AssertEquals(invoice3.JZ_InvoiceNumber, Declaration.SortedInvoiceList[1].Code);
			AssertEquals(invoice2.JZ_InvoiceNumber, Declaration.SortedInvoiceList[2].Code);

			invoice1.JZ_InvoiceDisplaySequence = 5;
			AssertEquals(invoice3.JZ_InvoiceNumber, Declaration.SortedInvoiceList[0].Code);
			AssertEquals(invoice2.JZ_InvoiceNumber, Declaration.SortedInvoiceList[1].Code);
			AssertEquals(invoice1.JZ_InvoiceNumber, Declaration.SortedInvoiceList[2].Code);
		}

		public void TestCostInLocalCurrency()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_DateOfArrival = new ZDateTime(2016, 01, 08);
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			Declaration.JE_OH_Importer = importer.PK;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "Supplier";
			Declaration.JE_OH_Supplier = supplier.PK;

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "GHS";
			invoice.JZ_InvoiceCurrLandedCostExRate = RatesAreReciprocal ? 2m : 0.5m;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;

			var landedCostHeader = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHeader>();
			landedCostHeader[LandedCostHeaderSchema.LT_ParentID.Name] = Declaration.PK;
			landedCostHeader[LandedCostHeaderSchema.LT_ParentTableCode.Name] = "JE";

			var landedCostHistory = (BusinessObject)Factory.New<Integration.LandedCosting.ILandedCostHistory>();
			landedCostHistory[LandedCostHistorySchema.LH_LT.Name] = landedCostHeader.PK;
			landedCostHistory[LandedCostHistorySchema.LH_ParentID.Name] = invoiceLine.PK;
			landedCostHistory[LandedCostHistorySchema.LH_ParentTableCode.Name] = JobComInvoiceLineSchema.Constants.Prefix;
			Factory.Save();
			var sql = $"SELECT JI_LocalLinePrice FROM dbo.Report_LandedCostSupplierTariffProductDetail('{GlbCompany.CurrentCompany.PK}', '{Declaration.JE_OH_Importer}', '2016-01-07', '2016-01-09', '', '', '', '{ZDateTime.Today.SqlFormat}')";

			var result = (decimal)Db.Connection.ExecuteScalar(sql);

			AssertEquals(result, ((IUltimateDistributee)invoiceLine).CostInLocalCurrency);
		}

		public virtual void TestPivot()
		{
			Declaration.JE_MessageType = DeclarationExportMessageType;
			var provider = InvoiceLine.GetClassificationTypeProvider();
			var hteCode = provider.HTECode;
			var htiCode = provider.HTICode;
			var htbCode = provider.HTBCode;

			OrgPartRelation AddANewRelatedOrg(OrgSupplierPart parentPart, ZString relationshipType, ZGuid ohpk)
			{
				var result = parentPart.RelatedOrganisations.AddNew();
				result.OU_Relationship = relationshipType;
				result.OU_OH = ohpk;
				return result;
			}

			BaseCusClassPartPivot AddACusClassPartPivot(OrgSupplierPart parentPart, ZString childType, ZString tariffNum, ZGuid? ohpk)
			{
				var result = parentPart.PivotsForBinding.AddNew();
				result.CI_ChildType = childType;
				if (ohpk.HasValue)
				{
					result.CI_OH = ohpk.Value;
				}
				result.CI_TariffNum = tariffNum;
				result.CI_UsageComment = "Test Usage Comment On" + tariffNum;

				return result;
			}

			var oneSharedSUP = Factory.New<OrgHeader>();

			#region Preparation Part 1
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PARTNUM";
			part.OP_Desc = "PART 1";

			var supRelation11 = AddANewRelatedOrg(part, OrgPartRelation.RelationshipTypes.Supplier, Factory.New<OrgHeader>().PK);
			var supRelation12 = AddANewRelatedOrg(part, OrgPartRelation.RelationshipTypes.Supplier, oneSharedSUP.PK);
			var ownRelation11 = AddANewRelatedOrg(part, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>().PK);
			var ownRelation12 = AddANewRelatedOrg(part, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>().PK);

			var pivotHTI11 = AddACusClassPartPivot(part, htiCode, "123", ownRelation11.OU_OH);
			var pivotHTI12 = AddACusClassPartPivot(part, htiCode, "223", supRelation12.OU_OH);
			var pivotHTE11 = AddACusClassPartPivot(part, hteCode, "323", supRelation11.OU_OH);
			var pivotHTE12 = AddACusClassPartPivot(part, hteCode, "423", supRelation12.OU_OH);
			var pivotHTB11 = AddACusClassPartPivot(part, htbCode, "523", ownRelation12.OU_OH);
			var pivotHTB12 = AddACusClassPartPivot(part, htbCode, "623", supRelation11.OU_OH);

			#endregion

			#region Preparation Part 2
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PARTNUM";
			part2.OP_Desc = "PART 2";
			var supRelation21 = AddANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Supplier, Factory.New<OrgHeader>().PK);
			var supRelation22 = AddANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Supplier, oneSharedSUP.PK);
			var supRelation23 = AddANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Supplier, Factory.New<OrgHeader>().PK);
			var ownRelation21 = AddANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>().PK);
			var ownRelation22 = AddANewRelatedOrg(part2, OrgPartRelation.RelationshipTypes.Owner, Factory.New<OrgHeader>().PK);

			var pivotHTI21 = AddACusClassPartPivot(part2, htiCode, "1234", ownRelation21.OU_OH);
			var pivotHTI22 = AddACusClassPartPivot(part2, htiCode, "2234", supRelation22.OU_OH);
			var pivotHTE21 = AddACusClassPartPivot(part2, hteCode, "3234", supRelation21.OU_OH);
			var pivotHTE22 = AddACusClassPartPivot(part2, hteCode, "4234", supRelation22.OU_OH);
			#endregion

			CombineAssertions("IMP", () =>
			{
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = oneSharedSUP.PK;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				Assert("Multi Part Match, pick Random 1", InvoiceLine.Pivot.PK == pivotHTI12.PK || InvoiceLine.Pivot.PK == pivotHTI22.PK);
				AssertEquals("Should sync the usage comment from the current pivot.", InvoiceLine.JI_ClassUsageComment, InvoiceLine.Pivot.CI_UsageComment);

				Declaration.JE_OH_Importer = ZGuid.Empty;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation11.OU_OH;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				if (ShouldMatchHTBForTestPivot)
				{
					AssertEquals(pivotHTB12.PK, InvoiceLine.Pivot.PK);
					AssertEquals("Should sync the usage comment from the current pivot.", InvoiceLine.JI_ClassUsageComment, InvoiceLine.Pivot.CI_UsageComment);
				}
				else
				{
					AssertNull(InvoiceLine.Pivot);
				}

				Declaration.JE_OH_Importer = ownRelation21.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation22.OU_OH;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals(pivotHTI21.PK, InvoiceLine.Pivot.PK);
				AssertEquals("Should sync the usage comment from the current pivot.", InvoiceLine.JI_ClassUsageComment, InvoiceLine.Pivot.CI_UsageComment);

				var pivotHTB2 = AddACusClassPartPivot(part2, htbCode, "5234", null);
				Declaration.JE_OH_Importer = ownRelation22.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation21.OU_OH;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				if (ShouldMatchHTBForTestPivot)
				{
					AssertEquals(pivotHTB2.PK, InvoiceLine.Pivot.PK);
					AssertEquals("Should sync the usage comment from the current pivot.", InvoiceLine.JI_ClassUsageComment, InvoiceLine.Pivot.CI_UsageComment);
				}
				else
				{
					AssertNull(InvoiceLine.Pivot);
				}
				pivotHTB2.Delete();
			});

			CombineAssertions("EXP", () =>
			{
				Declaration.JE_OH_Importer = ZGuid.Empty;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationExportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = oneSharedSUP.PK;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();

				AssertEquals("Multi Part Match, pick Random 1", true, InvoiceLine.Pivot.PK == pivotHTE12.PK || InvoiceLine.Pivot.PK == pivotHTE22.PK);
				AssertEquals("Should sync the usage comment from the current pivot.", InvoiceLine.JI_ClassUsageComment, InvoiceLine.Pivot.CI_UsageComment);

				Declaration.JE_OH_Importer = ownRelation11.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationExportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation11.OU_OH;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				if (ShouldMatchHTBForTestPivot)
				{
					AssertNull("When ambiguity exist, no pivot should be picked.", InvoiceLine.Pivot);
				}
				else
				{
					AssertEquals(pivotHTE11.PK, InvoiceLine.Pivot.PK);
				}

				var pivotHTB2 = AddACusClassPartPivot(part2, htbCode, "5234", null);
				Declaration.JE_OH_Importer = ownRelation22.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationExportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = supRelation23.OU_OH;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				if (ShouldMatchHTBForTestPivot)
				{
					AssertEquals(pivotHTB2.PK, InvoiceLine.Pivot.PK);
					AssertEquals("Should sync the usage comment from the current pivot.", InvoiceLine.JI_ClassUsageComment, InvoiceLine.Pivot.CI_UsageComment);
				}
				else
				{
					AssertNull(InvoiceLine.Pivot);
				}
			});

			CombineAssertions("Date filters", () =>
			{
				Declaration.JE_OH_Importer = ownRelation11.OU_OH;
				Declaration.JE_OH_Supplier = ZGuid.Empty;
				Declaration.JE_MessageType = DeclarationImportMessageType;
				InvoiceLine.InvoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
				InvoiceLine.JI_PartNo = "PARTNUM";
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals(pivotHTI11.PK, InvoiceLine.Pivot.PK);

				pivotHTI11.CI_DateEnd = ZDate.Today.AddDays(-2);
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertNull("No pivot matched as End Date is not valid.", invoiceLine.Pivot);

				pivotHTI11.CI_DateEnd = ZDate.Empty;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals("Match pivot with empty End Date", pivotHTI11.PK, InvoiceLine.Pivot.PK);

				pivotHTI11.CI_DateStart = ZDate.Today.AddDays(2);
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertNull("No pivot matched as Start Date is not valid.", invoiceLine.Pivot);

				pivotHTI11.CI_DateStart = ZDate.Empty;
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertEquals("Match pivot with empty Start Date", pivotHTI11.PK, InvoiceLine.Pivot.PK);

				var pivotHTI13 = AddACusClassPartPivot(part, htiCode, "123", ownRelation11.OU_OH);
				Factory.ClearCachedValue<BaseCusClassPartPivot>();
				AssertNull("Multiple pivot with empty Start Date", InvoiceLine.Pivot);
			});
		}

		public void TestDeleteWorkflowItems()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "IMP";
			var invoice = dec.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			var workflowItem = line.WorkflowItems.AddNew();
			line.Delete();

			Assert(line.IsDeleted);
			Assert(workflowItem.IsDeleted);
		}

		public void TestPropertyThatAffectWorkflowChanged()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "IMP";
			var invoice = dec.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			var expectedInfosFromInvoiceHeader = new[]
			{
				invoice.JZ_OH_BuyerInfo,
				invoice.JZ_OH_SupplierInfo,
				invoice.JZ_MessageTypeInfo
			};
			AssertContainsExactElementsInAnyOrder(expectedInfosFromInvoiceHeader, BaseJobComInvoiceLine.PropertyThatAffectWorkflowChangedFromInvoiceHeader(invoice));

			var expectedInfosFromDeclaration = new[]
			{
				dec.JE_MessageTypeInfo,
				dec.JE_OH_ImporterInfo,
				dec.JE_OH_SupplierInfo
			};
			AssertContainsExactElementsInAnyOrder(expectedInfosFromDeclaration, BaseJobComInvoiceLine.PropertyThatAffectWorkflowChangedFromDeclaration(dec));

			var workflowAffectedPropertyProvider = (IWorkflowAffectedPropertyProvider)line;
			var expectedInfos = expectedInfosFromInvoiceHeader.Concat(expectedInfosFromDeclaration).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedInfos, workflowAffectedPropertyProvider.PropertyThatAffectWorkflowChanged);
		}

		public void TestIssue00175263()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var invoiceLine = declaration.InvoiceLines.AddNew();

			AssertNoExceptionThrown(delegate
			{ var accessed = invoiceLine.JI_OverseasFreight; });
		}

		public void TestClassification()
		{
			var country = Factory.New<RefCountry>();
			var classificaiton = Factory.New<BaseCusClassification>();

			classificaiton.CC_RN_NKCountryCode = country.Code;
			InvoiceLine.JI_CC = classificaiton.PK;
			AssertEquals(classificaiton.PK, InvoiceLine.JI_CC);
			AssertNull(InvoiceLine.Classification);

			classificaiton.CC_RN_NKCountryCode = InvoiceLine.InvoiceHeader.CountryCode;
			InvoiceLine.JI_CC = classificaiton.PK;
			AssertEquals(classificaiton.PK, InvoiceLine.JI_CC);
			AssertNotNull(InvoiceLine.Classification);
		}

		public void TestIWeightApportioneeRoundingIssue()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var foreignCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, declaration.LocalCurrencyCode));
			foreignCurrency.SetCustomsRate(ZDateTime.Today, ZDateTime.Today.AddDays(1), 0.8119m);
			declaration.JE_AutoWeightApportion = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_Weight = 10000m;
			invoice.JZ_WeightUQ = Core.Constants.Weight.Kilograms;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 8000m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2000m;

			AssertEquals("Weight", 8000m, invoiceLine1.JI_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine1.JI_WeightUQ);

			AssertEquals("Weight", 2000m, invoiceLine2.JI_Weight);
			AssertEquals("WeightUQ", Core.Constants.Weight.Kilograms, invoiceLine2.JI_WeightUQ);
		}

		public virtual void TestJI_FormattedTariff()
		{
			var tariff = "1234567890";
			InvoiceLine.JI_Tariff = tariff;
			AssertEquals("JI_FormattedTariff", "1234.56.78 90", InvoiceLine.JI_FormattedTariff);
			tariff = "9876.54.32 10";
			InvoiceLine.JI_FormattedTariff = "9876.54.32 10";
			AssertEquals("JI_FormattedTariff", tariff, InvoiceLine.JI_FormattedTariff);
		}

		public virtual void TestEffectiveDateForDutyRate()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(invoice.EffectiveDateForDutyRate, invoiceLine.EffectiveDateForDutyRate);
			InvoiceLine.JI_JZ = ZGuid.Invalid;
			AssertEquals(ZDate.Today, invoiceLine.EffectiveDateForDutyRate);
		}

		public void TestBooleans()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.CusContainers.AddNew();
			declaration.CusContainers.AddNew();
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			AssertEquals("IsSingleAssociationWithContainer", false, invoiceLine.IsSingleAssociationWithContainer);
			AssertEquals("IsSplitToMultiContainers", true, invoiceLine.IsSplitToMultiContainers);

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = false;
			AssertEquals("IsSingleAssociationWithContainer", true, invoiceLine.IsSingleAssociationWithContainer);
			AssertEquals("IsSplitToMultiContainers", false, invoiceLine.IsSplitToMultiContainers);
		}

		public void TestLightValidationForDifferentMessageTypesAndTransportModes()
		{
			new LightValidationForDifferentMessageTypesAndTransportModesTester(
				() => { return GetJobDeclaration(); },
				(LightValidationForDifferentMessageTypesAndTransportModesTester tester) => { TestLightValidation(tester.InvoiceLine); },
				(BaseJobDeclaration declaration) => { AfterInitialise(declaration); }
				).Test();
		}

		public void TestNetWeightInKG()
		{
			AssertEquals("NetWeightInKG", ZDecimal.Zero, InvoiceLine.NetWeightInKG);

			InvoiceLine.JI_NetWeight = 10m;
			InvoiceLine.JI_NetWeightUQ = "";
			AssertEquals("NetWeightInKG", ZDecimal.Zero, InvoiceLine.NetWeightInKG);

			InvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Ounces;
			AssertEquals("NetWeightInKG", Core.Constants.Weight.Convert(10m, Core.Constants.Weight.Ounces, Core.Constants.Weight.Kilograms), InvoiceLine.NetWeightInKG);
		}

		public virtual void TestEffectiveCountryOfOrigin()
		{
			InvoiceLine.InvoiceHeader.JZ_RN_NKDefaultOrigin = "KR";
			InvoiceLine.JI_CountryOfOrigin = "";
			AssertEquals("EffectiveCountryOfOrigin", "KR", InvoiceLine.EffectiveCountryOfOrigin);

			InvoiceLine.JI_CountryOfOrigin = "US";
			AssertEquals("EffectiveCountryOfOrigin", "US", InvoiceLine.EffectiveCountryOfOrigin);
		}

		public virtual void TestMakeCustomsQuantityReadOnly()
		{
			InvoiceLine.JI_Tariff = "";
			AssertEquals("There is no tariff and CustomsQuantity should not be readonly", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_Tariff = "00000000 00";
			AssertEquals("Invalid tariff and there is no Customs UQ involved", true, InvoiceLine.JI_CustomsUnitQty.IsEmpty);

			InvoiceLine.JI_CustomsUnitQty = "NO";
			AssertEquals("Customs unit qty exists and Qty field should be open", false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public virtual void TestCustomsQtyCalculatedWhenInvoiceQtySet()
		{
			using (UnitConverter.TemporarySetupCachedConvertion(Factory))
			{
				var refPacks = Factory.New<CusRefPacks>();
				refPacks.RP_ConversionFactor = 12m;
				refPacks.RP_CustomsPack = "NO";
				refPacks.RP_CommercialPack = "DOZ";

				InvoiceLine.JI_InvoiceUQ = "";
				InvoiceLine.JI_Tariff = "0001.01.01 1";
				InvoiceLine.JI_CustomsUnitQty = "NO";
				AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsQuantity);

				InvoiceLine.JI_InvoiceUQ = "DOZ";
				AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsQuantity);
				InvoiceLine.JI_InvoiceQuantity = 18m;
				AssertEquals(216m, InvoiceLine.JI_CustomsQuantity);

				var invoiceline2 = InvoiceLine.InvoiceHeader.InvoiceLines.AddNew();
				invoiceline2.JI_InvoiceUQ = "";
				invoiceline2.JI_CustomsUnitQty = "NO";
				invoiceline2.JI_InvoiceUQ = "DOZ";
				invoiceline2.JI_InvoiceQuantity = 18m;
				AssertEquals(216m, invoiceline2.JI_CustomsQuantity);
				AssertEquals("Conversion between the same UQs should use the cached value", 0, invoiceline2.UnitConverter.ConversionFactorInvokedCountForTest);
			}
		}

		public virtual void TestCustomsQtyCalculatedByNetWeightOfProductWhenInvoiceQtySet()
		{
			using (UnitConverter.TemporarySetupCachedConvertion(Factory))
			{
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var supplier = Factory.New<OrgHeader>();
				InvoiceHeader.JZ_OH_Supplier = supplier.PK;

				var product1 = Factory.New<OrgSupplierPart>();
				product1.OP_PartNum = "TESTTEST1";
				product1.OP_StockKeepingUnit = "NO";
				product1.OP_NetWeight = 3m;
				product1.OP_WeightUQ = "KG";//3 kg per NO

				var relation1 = product1.RelatedOrganisations.AddNew();
				relation1.OU_OH = supplier.PK;
				relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
				product1.PivotsForBinding.AddNew();

				var product2 = Factory.New<OrgSupplierPart>();
				product2.OP_PartNum = "TESTTEST2";
				product2.OP_StockKeepingUnit = "NO";
				product2.OP_NetWeight = 4m;
				product2.OP_WeightUQ = "KG";//4 kg per NO

				var relation2 = product2.RelatedOrganisations.AddNew();
				relation2.OU_OH = supplier.PK;
				relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
				product2.PivotsForBinding.AddNew();

				InvoiceLine.JI_CustomsUnitQty = "KG";
				InvoiceLine.JI_PartNo = product1.OP_PartNum;
				InvoiceLine.JI_InvoiceUQ = "NO";
				InvoiceLine.JI_InvoiceQuantity = 18m;

				var invoiceline2 = InvoiceLine.InvoiceHeader.InvoiceLines.AddNew();
				invoiceline2.JI_CustomsUnitQty = "KG";
				invoiceline2.JI_PartNo = product2.OP_PartNum;
				invoiceline2.JI_InvoiceUQ = "NO";
				invoiceline2.JI_InvoiceQuantity = 18m;

				AssertEquals(54m, InvoiceLine.JI_NetWeight);
				AssertEquals(72m, invoiceline2.JI_NetWeight);
				AssertEquals(54m, InvoiceLine.JI_CustomsQuantity);
				AssertEquals(72m, invoiceline2.JI_CustomsQuantity);
			}
		}

		public void TestForceUpdateInvoiceQuantityWithoutDefaulting()
		{
			using (UnitConverter.TemporarySetupCachedConvertion(Factory))
			{
				var refPacks = Factory.New<CusRefPacks>();
				refPacks.RP_ConversionFactor = 12m;
				refPacks.RP_CustomsPack = "NO";
				refPacks.RP_CommercialPack = "DOZ";

				InvoiceLine.JI_CustomsUnitQty = "NO";
				InvoiceLine.JI_InvoiceUQ = "DOZ";
				AssertEquals(ZDecimal.Zero, InvoiceLine.JI_CustomsQuantity);

				using (InvoiceLine.ForceUpdateInvoiceQuantityWithoutDefaulting())
				{
					InvoiceLine.JI_InvoiceQuantity = 18m;
				}

				AssertEquals(0m, InvoiceLine.JI_CustomsQuantity);
			}
		}

		public void TestMovingInvoiceLineToAnotherDeclaration()
		{
			var dec1 = Factory.New<BaseJobDeclaration>();
			var invoice = dec1.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var dec2 = Factory.New<BaseJobDeclaration>();
			var invoice2 = dec2.Invoices.AddNew();

			AssertEquals("Precondition", dec1, invoiceLine.Declaration);
			AssertEquals("Precondition", invoice, invoiceLine.InvoiceHeader);

			invoiceLine.JI_JZ = invoice2.PK;
			AssertEquals(invoice2, invoiceLine.InvoiceHeader);
			AssertEquals(dec2, invoiceLine.Declaration);
		}

		public void TestUpdateWeightVolumeWhenProductIsUpdated()
		{
			var supplier = Factory.New<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TESTTEST";
			product.OP_StockKeepingUnit = "NO";
			product.OP_Weight = 2m;
			product.OP_WeightUQ = "KG"; // 2 kg per NO
			product.OP_Cubic = 0.5m;
			product.OP_CubicUQ = "M3"; // 0.5 m3 per unit

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = supplier.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			product.PivotsForBinding.AddNew();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;

			InvoiceLine.JI_PartNo = product.OP_PartNum;
			InvoiceLine.JI_InvoiceUQ = "NO";

			InvoiceLine.JI_InvoiceQuantity = 10m;
			AssertEquals("Weight should be Invoice Qty x Product Weight", 20m, InvoiceLine.JI_Weight);
			AssertEquals("Weight should be Invoice Qty x Product Weight", "KG", InvoiceLine.JI_WeightUQ);
			AssertEquals("Volume should be Invoice Qty x Product Volume", 5m, InvoiceLine.JI_Volume);
			AssertEquals("Volume", "M3", InvoiceLine.JI_VolumeUQ);

			InvoiceLine.JI_PartNo = "";

			product.OP_StockKeepingUnit = "KG";
			product.OP_Weight = 1m;
			product.OP_WeightUQ = "KG"; // 1 kg per NO

			InvoiceLine.JI_PartNo = product.OP_PartNum;
			InvoiceLine.JI_InvoiceQuantity = 10m;
			InvoiceLine.JI_InvoiceUQ = "T";
			AssertEquals("Weight should be unchanged, beasue KGxKG is meaningless", 20m, InvoiceLine.JI_Weight);
			AssertEquals("Weight should be Invoice Qty x Product Weight", "KG", InvoiceLine.JI_WeightUQ);
			AssertEquals("Volume should be Invoice Qty x Product Volume", 5000m, InvoiceLine.JI_Volume);
			AssertEquals("Volume", "M3", InvoiceLine.JI_VolumeUQ);
		}

		public void TestCheckPartAttributesOrSerialNumber_AttributeIsReleaseCaptured()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "P1";

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			importer.MiscServ.OM_IMUseSerialNumber = true;
			importer.MiscServ.OM_IMPartAttrib1Name = "Attr1";
			importer.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			importer.MiscServ.OM_IMPartAttrib2Name = "Attr2";
			importer.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			importer.MiscServ.OM_IMPartAttrib3Name = "Attr3";
			importer.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;
			relation.OU_UseSerialNumber = true;
			relation.OU_UsePartAttrib1 = true;
			relation.OU_UsePartAttrib2 = true;
			relation.OU_UsePartAttrib3 = true;
			relation.OU_IsSerialNumberReleaseCaptured = true;
			relation.OU_IsPartAttrib1ReleaseCaptured = true;
			relation.OU_IsPartAttrib2ReleaseCaptured = true;
			relation.OU_IsPartAttrib3ReleaseCaptured = true;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_OH_Importer = importer.PK;
			InvoiceHeader.JZ_OH_Supplier = supplier.PK;
			InvoiceLine.JI_PartNo = product.OP_PartNum;
			InvoiceLine.JI_OP = product.PK;
			InvoiceLine.Validation.ValidateAll();
			Assert(!InvoiceLine.JI_SerialNumberInfo.HasErrors());
			Assert(!InvoiceLine.JI_PartAttrib1Info.HasErrors());
			Assert(!InvoiceLine.JI_PartAttrib2Info.HasErrors());
			Assert(!InvoiceLine.JI_PartAttrib3Info.HasErrors());
		}

		public void TestIsNonContainersed_IsContainedIn()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var container = declaration.CusContainers.AddNew();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;

			AssertEquals("IsNonContainerised", false, invoiceLine.IsNonContainerised);
			AssertEquals("IsContainedIn", true, invoiceLine.IsContainedIn(container));
			AssertEquals("with null", false, invoiceLine.IsContainedIn(null));

			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = false;

			AssertEquals("IsContainedIn", false, invoiceLine.IsContainedIn(container));
			AssertEquals("with null", false, invoiceLine.IsContainedIn(null));
			AssertEquals("IsNonContainerised", true, invoiceLine.IsNonContainerised);
		}

		public void TestIsContainerisedMode()
		{
			var containerisedModes = new string[] {
					Core.Constants.ContainerModes.FCL,
					Core.Constants.ContainerModes.LCL,
					Core.Constants.ContainerModes.FCLMixedShipper,
					Core.Constants.ContainerModes.Containerised
			};

			var someNotContainerisedModes = new string[] {
					Core.Constants.ContainerModes.Combination,
					Core.Constants.ContainerModes.Bulk,
					Core.Constants.ContainerModes.Liquid,
			};

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.CusContainers.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			foreach (var mode in containerisedModes)
			{
				invoiceLine.JI_ContainerMode = mode;
				AssertEquals(string.Format("{0} - containerised mode", mode), true, invoiceLine.IsContainerisedMode);
			}

			foreach (var mode in someNotContainerisedModes)
			{
				invoiceLine.JI_ContainerMode = mode;
				AssertEquals(string.Format("{0} - not containerised mode", mode), false, invoiceLine.IsContainerisedMode);
			}

			invoiceLine.JI_ContainerMode = string.Empty;
			declaration.JE_ContainerMode = containerisedModes[0];
			AssertEquals(string.Format("Should use JE_ContainerMode if JI_ContainerMode not specified: {0} - containerised mode", declaration.JE_ContainerMode),
				true, invoiceLine.IsContainerisedMode);

			declaration.JE_ContainerMode = someNotContainerisedModes[0];
			AssertEquals(string.Format("Should use JE_ContainerMode if JI_ContainerMode not specified: {0} - not containerised mode", declaration.JE_ContainerMode),
				false, invoiceLine.IsContainerisedMode);
		}

		[ExpectNoExceptions()]
		public void TestIsContainerisedMode_CS00142301()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var containerMode = invoiceLine.IsContainerisedMode;
		}

		[ExpectNoExceptions]
		public void TestInvoiceHeaderDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.CusContainers.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.Delete();

			var headerOnDeletedLine = invoiceLine.InvoiceHeader;
		}

		public void TestAddingPackage_AddsParentPackages()
		{
			var declaration = GetJobDeclaration();
			if (!declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				Assert(true);
				return;
			}
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			bill.PackingGroups.AddNew();

			var package1 = bill.PackingGroups[0].Packages.AddNew();
			var package1_1 = package1.AddChild();
			var package1_1_1 = package1_1.AddChild();
			var package1_1_2 = package1_1.AddChild();
			var package1_2 = package1.AddChild();
			package1_2.AddChild();
			package1_2.AddChild();
			var package1_2_3 = package1_2.AddChild();

			var package2 = bill.PackingGroups[0].Packages.AddNew();
			var package2_1 = package2.AddChild();
			var package2_2 = package2.AddChild();
			var package2_2_1 = package2_2.AddChild();
			var package2_2_2 = package2_2.AddChild();
			var package2_3 = package2.AddChild();
			var package2_3_1 = package2_3.AddChild();

			invoiceLine.LoadChildEditableObjects();
			invoiceLine.ToggleLinkageWithPackage(package1_2_3, true);
			invoiceLine.ToggleLinkageWithPackage(package2_2_1, true);
			invoiceLine.ToggleLinkageWithPackage(package2_2_2, true);

			AssertPackageAddedToInvoiceLine(invoiceLine, package1, nameof(package1));
			AssertPackageAddedToInvoiceLine(invoiceLine, package1_2, nameof(package1_2));
			AssertPackageAddedToInvoiceLine(invoiceLine, package1_2_3, nameof(package1_2_3));
			AssertPackageAddedToInvoiceLine(invoiceLine, package2, nameof(package2));
			AssertPackageAddedToInvoiceLine(invoiceLine, package2_2, nameof(package2_2));
			AssertPackageAddedToInvoiceLine(invoiceLine, package2_2_2, nameof(package2_2_2));

			AssertPackageNotAddedToInvoiceLine(invoiceLine, package1_1, nameof(package1_1));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package1_1_1, nameof(package1_1_1));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package1_1_2, nameof(package1_1_2));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package2_1, nameof(package2_1));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package2_3, nameof(package2_3));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package2_3_1, nameof(package2_3_1));
		}

		public void TestRemovePackage_RemovesParentsWithNoAddedChildPackages()
		{
			var declaration = GetJobDeclaration();
			if (!declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				Assert(true);
				return;
			}
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			bill.PackingGroups.AddNew();

			var package1 = bill.PackingGroups[0].Packages.AddNew();
			var package1_1 = package1.AddChild();
			package1_1.AddChild();
			var package1_1_2 = package1_1.AddChild();
			var package1_2 = package1.AddChild();
			package1_2.AddChild();
			var package1_2_2 = package1_2.AddChild();
			package1_2.AddChild();

			var package2 = bill.PackingGroups[0].Packages.AddNew();
			package2.AddChild();
			var package2_2 = package2.AddChild();
			package2_2.AddChild();
			var package2_2_2 = package2_2.AddChild();
			var package2_3 = package2.AddChild();
			package2_3.AddChild();

			invoiceLine.ToggleLinkageWithPackage(package1_1_2, true);
			invoiceLine.ToggleLinkageWithPackage(package1_2_2, true);
			invoiceLine.ToggleLinkageWithPackage(package1_2_2, false);

			AssertPackageAddedToInvoiceLine(invoiceLine, package1, nameof(package1));
			AssertPackageAddedToInvoiceLine(invoiceLine, package1_1, nameof(package1_1));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package1_2, nameof(package1_2));

			invoiceLine.ToggleLinkageWithPackage(package2_2_2, true);
			invoiceLine.ToggleLinkageWithPackage(package2_2_2, false);

			AssertPackageNotAddedToInvoiceLine(invoiceLine, package2, nameof(package2));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package2_2, nameof(package2_2));
		}

		public void TestRemovePackage_DoesNotRemoveParentsWithAddedChildPackages()
		{
			var declaration = GetJobDeclaration();
			if (!declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				Assert(true);
				return;
			}

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			bill.PackingGroups.AddNew();

			var package1 = bill.PackingGroups[0].Packages.AddNew();
			var package1_1 = package1.AddChild();
			package1_1.AddChild();
			var package1_1_2 = package1_1.AddChild();
			var package1_2 = package1.AddChild();
			package1_2.AddChild();
			var package1_2_2 = package1_2.AddChild();
			var package1_2_3 = package1_2.AddChild();

			var package2 = bill.PackingGroups[0].Packages.AddNew();
			package2.AddChild();
			var package2_2 = package2.AddChild();
			package2_2.AddChild();
			var package2_2_2 = package2_2.AddChild();
			var package2_3 = package2.AddChild();
			var package2_3_1 = package2_3.AddChild();

			invoiceLine.ToggleLinkageWithPackage(package1_1_2, true);
			invoiceLine.ToggleLinkageWithPackage(package1_2_2, true);
			invoiceLine.ToggleLinkageWithPackage(package1_2_3, true);
			invoiceLine.ToggleLinkageWithPackage(package1_2_2, false);

			invoiceLine.ToggleLinkageWithPackage(package2_2_2, true);
			invoiceLine.ToggleLinkageWithPackage(package2_3_1, true);
			invoiceLine.ToggleLinkageWithPackage(package2_2_2, false);

			AssertPackageAddedToInvoiceLine(invoiceLine, package2, nameof(package2));
			AssertPackageAddedToInvoiceLine(invoiceLine, package2_3, nameof(package2_3));
		}

		public void TestChangePackageParent_UpdatesLinkageWithInvoice()
		{
			var declaration = GetJobDeclaration();
			if (!declaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				Assert(true);
				return;
			}
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			bill.PackingGroups.AddNew();

			var package1 = bill.PackingGroups[0].Packages.AddNew();
			var package1_1 = package1.AddChild();
			var package1_1_1 = package1_1.AddChild();
			var package1_1_2 = package1_1.AddChild();
			var package1_2 = package1.AddChild();
			var package1_2_1 = package1_2.AddChild();
			var package1_2_2 = package1_2.AddChild();
			var package1_2_3 = package1_2.AddChild();

			var package2 = bill.PackingGroups[0].Packages.AddNew();
			var package2_1 = package2.AddChild();
			var package2_2 = package2.AddChild();
			var package2_2_1 = package2_2.AddChild();
			var package2_2_2 = package2_2.AddChild();
			var package2_3 = package2.AddChild();
			var package2_3_1 = package2_3.AddChild();

			invoiceLine.ToggleLinkageWithPackage(package1_1_2, true);

			package1_1_2.CW_CW_Parent = package2.PK;

			invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().ForEach(p => p.CHC_NumberOfPacks = 1);
			AssertPackageAddedToInvoiceLine(invoiceLine, package2, nameof(package2));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package1_1, nameof(package1_1));
			AssertPackageNotAddedToInvoiceLine(invoiceLine, package1, nameof(package1));
		}

		public void TestPartCreatedAfterLineFirstCreated()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "TestImporter";
			importer.OH_IsConsignor = true;

			var classificationGranny = Factory.New<BaseCusClassification>();
			classificationGranny.FillWithValidTestData();
			classificationGranny.CC_TariffNum = "00010101";
			classificationGranny.CC_LookupCode = "GRANNY";
			classificationGranny.CC_ClassificationType = "BTH";

			var classificationJonno = Factory.New<BaseCusClassification>();
			classificationJonno.FillWithValidTestData();
			classificationJonno.CC_TariffNum = "00020202";
			classificationJonno.CC_LookupCode = "JONNO";
			classificationJonno.CC_ClassificationType = "BTH";

			var dec = Factory.NewWithValidTestData<BaseJobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var line1 = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line1.JI_PartNo = "APPLE";
			line1.JI_CC = classificationGranny.PK;
			line1.JI_InvoiceQuantity = 144;
			line1.JI_Tariff = "00010101";

			Factory.Save();

			var factory2 = new BusinessObjectFactory
			{
				RefreshEnabled = false
			};

			var partApple = factory2.New<OrgSupplierPart>();
			partApple.FillWithValidTestData();
			partApple.OP_PartNum = "APPLE";
			partApple.OP_Desc = "Jonno Apple";
			partApple.OP_LastCost = 0.65m;
			partApple.RelatedOrganisations.RemoveAndDeleteAll();
			partApple.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var appleJonnoPivot = factory2.New<BaseCusClassPartPivot>();
			appleJonnoPivot.CI_CC = classificationJonno.PK;
			appleJonnoPivot.CI_OP = partApple.PK;

			factory2.Save();

			// should refresh as we go now...
			//			AssertEquals("JI_OP is empty initially", ZGuid.Empty, Line1.JI_OP);
			//			AssertEquals("Class is Granny", ClassificationGranny.PK, Line1.JI_CC);
			//			Assert("Tariff is Grannys", Line1.JI_Tariff.Contains("0001.01.01") || Line1.JI_Tariff.Contains("00010101"));

			var line2 = factory2.LoadTop1<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.PK, line1.PK));

			AssertEquals("JI_OP is now set", partApple.PK, line2.JI_OP);
			AssertEquals("Class is still Granny", classificationGranny.PK, line2.JI_CC);
			Assert("Tariff is still Grannys", line2.JI_Tariff.Equals("0001.01.01") || line2.JI_Tariff.Equals("00010101"));
		}

		public void TestReAssigningLineNumbers()
		{
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);

			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);

			var invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);

			invoiceLine3.JI_LineNo = 2;
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)3, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)2, invoiceLine3.JI_LineNo);

			InvoiceLine.JI_LineNo = 3;
			AssertEquals("Invoice Line Number for Line 1", (short)3, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)1, invoiceLine3.JI_LineNo);

			invoiceLine2.JI_LineNo = 8;
			AssertEquals("Invoice Line Number for Line 1", (short)2, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)3, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)1, invoiceLine3.JI_LineNo);

			var invoiceLine4 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 4", (short)4, invoiceLine4.JI_LineNo);

			invoiceLine4.JI_LineNo = 0;
			AssertEquals("Invoice Line Number for Line 1", (short)2, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)3, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)1, invoiceLine3.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 4", (short)4, invoiceLine4.JI_LineNo);

			invoiceLine4.JI_LineNo = 1;
			AssertEquals("Invoice Line Number for Line 1", (short)3, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)4, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)2, invoiceLine3.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 4", (short)1, invoiceLine4.JI_LineNo);

			invoiceLine3.JI_LineNo = 8;
			AssertEquals("Invoice Line Number for Line 1", (short)2, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)3, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)4, invoiceLine3.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 4", (short)1, invoiceLine4.JI_LineNo);

			var invoice2 = Declaration.Invoices.AddNew();
			invoiceLine4.JI_JZ = invoice2.PK;
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 4", (short)1, invoiceLine4.JI_LineNo);

			invoiceLine4.JI_JZ = InvoiceHeader.PK;
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 4", (short)4, invoiceLine4.JI_LineNo);

			invoiceLine4.JI_LineNo = 3;
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)4, invoiceLine3.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 4", (short)3, invoiceLine4.JI_LineNo);
		}

		public void TestReAssigningLineNumbersWithMoreInvoiceLines()
		{
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);
			var invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);
			var invoiceLine4 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 4", (short)4, invoiceLine4.JI_LineNo);
			var invoiceLine5 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 5", (short)5, invoiceLine5.JI_LineNo);

			invoiceLine3.JI_LineNo = 2;
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)3, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)2, invoiceLine3.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 4", (short)4, invoiceLine4.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 5", (short)5, invoiceLine5.JI_LineNo);
		}

		public void TestReAssigningLineNumbersWithMultiHeaders()
		{
			InvoiceHeader.JZ_InvoiceNumber = "InvHeader1";
			AssertEquals("InvHeader1 - Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("InvHeader1 - Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);
			var invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("InvHeader1 - Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);

			var invoiceHeader2 = Declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "InvHeader2";
			var invHeader2InvLine1 = invoiceHeader2.JobComInvoiceLines.AddNew();
			AssertEquals("InvHeader2 - Invoice Line Number for Line 1", (short)1, invHeader2InvLine1.JI_LineNo);
			var invHeader2InvLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			AssertEquals("InvHeader2 - Invoice Line Number for Line 2", (short)2, invHeader2InvLine2.JI_LineNo);
			var invHeader2InvLine3 = invoiceHeader2.JobComInvoiceLines.AddNew();
			AssertEquals("InvHeader2 - Invoice Line Number for Line 3", (short)3, invHeader2InvLine3.JI_LineNo);

			invoiceLine2.JI_LineNo = 1;
			AssertEquals("InvHeader1 - Invoice Line Number for Line 1", (short)2, InvoiceLine.JI_LineNo);
			AssertEquals("InvHeader1 - Invoice Line Number for Line 2", (short)1, invoiceLine2.JI_LineNo);
			AssertEquals("InvHeader1 - Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);
			AssertEquals("InvHeader2 - Invoice Line Number for Line 1", (short)1, invHeader2InvLine1.JI_LineNo);
			AssertEquals("InvHeader2 - Invoice Line Number for Line 2", (short)2, invHeader2InvLine2.JI_LineNo);
			AssertEquals("InvHeader2 - Invoice Line Number for Line 3", (short)3, invHeader2InvLine3.JI_LineNo);

			invHeader2InvLine1.JI_LineNo = 3;
			AssertEquals("InvHeader1 - Invoice Line Number for Line 1", (short)2, InvoiceLine.JI_LineNo);
			AssertEquals("InvHeader1 - Invoice Line Number for Line 2", (short)1, invoiceLine2.JI_LineNo);
			AssertEquals("InvHeader1 - Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);
			AssertEquals("InvHeader2 - Invoice Line Number for Line 1", (short)3, invHeader2InvLine1.JI_LineNo);
			AssertEquals("InvHeader2 - Invoice Line Number for Line 2", (short)1, invHeader2InvLine2.JI_LineNo);
			AssertEquals("InvHeader2 - Invoice Line Number for Line 3", (short)2, invHeader2InvLine3.JI_LineNo);
		}

		public void TestResAssigningLineNumbersWithWeirdNumbers()
		{
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			var invoiceLine2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);
			var invoiceLine3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals("Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);

			invoiceLine3.JI_LineNo = 55;
			AssertEquals("Invoice Line Number for Line 1", (short)1, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)2, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)3, invoiceLine3.JI_LineNo);

			InvoiceLine.JI_LineNo = 65;
			AssertEquals("Invoice Line Number for Line 1", (short)3, InvoiceLine.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 2", (short)1, invoiceLine2.JI_LineNo);
			AssertEquals("Invoice Line Number for Line 3", (short)2, invoiceLine3.JI_LineNo);
		}

		public void TestImporter_Effective()
		{
			AssertEquals("Before Importer Filled In", null, InvoiceLine.Importer_Effective);
			var org = OrgHeader.New(Factory);
			InvoiceHeader.JZ_OH_Buyer = org.PK;
			AssertEquals("After Importer Filled In", org, InvoiceLine.Importer_Effective);
		}

		public void TestSupplier_Effective()
		{
			AssertEquals("Before Supplier Filled In", null, InvoiceLine.Supplier_Effective);
			var org = OrgHeader.New(Factory);
			InvoiceHeader.JZ_OH_Supplier = org.PK;
			AssertEquals("After Supplier Filled In", org, InvoiceLine.Supplier_Effective);
		}

		public void TestImporterPK_Effective()
		{
			var invoiceLineDetatched = Factory.New<BaseJobComInvoiceLine>();
			AssertEquals(ZGuid.Empty, invoiceLineDetatched.ImporterPK_Effective);

			AssertEquals(ZGuid.Empty, InvoiceLine.ImporterPK_Effective);
			InvoiceHeader.JZ_OH_Buyer = ZGuid.NewZGuid();
			AssertEquals(InvoiceHeader.JZ_OH_Buyer, InvoiceLine.ImporterPK_Effective);
		}

		public void TestSupplierPK_Effective()
		{
			var invoiceLineDetatched = Factory.New<BaseJobComInvoiceLine>();
			AssertEquals(ZGuid.Empty, invoiceLineDetatched.SupplierPK_Effective);

			AssertEquals(ZGuid.Empty, InvoiceLine.SupplierPK_Effective);
			InvoiceHeader.JZ_OH_Supplier = ZGuid.NewZGuid();
			AssertEquals(InvoiceHeader.JZ_OH_Supplier, InvoiceLine.SupplierPK_Effective);
		}

		public virtual void TestFetchStrategy()
		{
			AssertEquals("InvoiceLine.FetchStrategy type", true, InvoiceLine.FetchStrategy is FetchStrategies.JobComInvoiceLineFetchStrategy);
		}

		public virtual void TestMergedLineNumber()
		{
			var entryHeader = Declaration.ActiveEntryHeaders.AddNew();
			var mergedLine = entryHeader.MergedLines.AddNew();
			AssertEquals("Prior to merge", "Not Merged", InvoiceLine.MergedLineNumber);
			InvoiceLine.JI_CL = mergedLine.PK;
			mergedLine.CL_LineNumber = 456;
			AssertEquals("After merge without entry number", "456", InvoiceLine.MergedLineNumber);
			mergedLine.Header.EntryNumber = "EntryNumber";
			AssertEquals("After merge with entry number", "456/EntryNumber", InvoiceLine.MergedLineNumber);
		}

		public virtual void TestJI_Calc_MergedLineNumber()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var mergedLine = entryHeader.MergedLines.AddNew();
			AssertEquals("Prior to merge", "Not Merged", InvoiceLine.JI_Calc_MergedLineNumber);
			InvoiceLine.JI_CL = mergedLine.PK;
			mergedLine.CL_LineNumber = 1;
			AssertEquals("After merge", "1", InvoiceLine.JI_Calc_MergedLineNumber);
		}

		public void TestICommonInvoiceInvoiceLines()
		{
			var line1 = InvoiceLine;
			var line2 = InvoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("Line1 InvoiceLines with Line1", true, ((ICommonInvoice)line1).InvoiceLines.Contains(line1));
			AssertEquals("Line1 InvoiceLines with Line1", 1, ((ICommonInvoice)line1).InvoiceLines.Count);
			AssertEquals("Line2 InvoiceLines with Line1", false, ((ICommonInvoice)line1).InvoiceLines.Contains(line2));

			foreach (var iLine in line1)
			{
				AssertEquals("Only itself should be in the list", iLine, line1);
			}
		}

		public void TestILandedCostDistributeTo()
		{
			InvoiceHeader.JZ_InvoiceNumber = "007";

			AssertEquals("ILandedCostDistributeTo.PK", InvoiceLine.PK, ((ILandedCostDistributeTo)InvoiceLine).PK);
			AssertEquals("ILandedCostDistributeTo.UniqueCode", BaseJobComInvoiceLine.InvoiceLineConstString + InvoiceLine.JI_LineNo + ": INVOICE " + InvoiceHeader.JZ_InvoiceNumber, ((ILandedCostDistributeTo)InvoiceLine).UniqueCode);
			AssertEquals("ILandedCostDistributeTo.Description", ((ILandedCostDistributeTo)InvoiceLine).UniqueCode, ((ILandedCostDistributeTo)InvoiceLine).Description);
			AssertEquals("ILandedCostDistributeTo.TableCode", JobComInvoiceLineSchema.Constants.Prefix, ((ILandedCostDistributeTo)InvoiceLine).TableCode);
			AssertEquals("ILandedCostDistributeTo.UltimateDistributee", 1, new List<IUltimateDistributee>(((ILandedCostDistributeTo)InvoiceLine).UltimateDistributees).Count);

			var detachedInvoiceLine = Factory.New<BaseJobComInvoiceLine>();
			AssertEquals("Error message instead of exception", BaseJobComInvoiceLine.InvoiceLineConstString + detachedInvoiceLine.JI_LineNo + ": INVOICE " + "<NO HEADER>", ((ILandedCostDistributeTo)detachedInvoiceLine).UniqueCode);
		}

		public void TestWeightAndVolumeUQConversion()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceLine.JI_Weight = 1;
			InvoiceLine.JI_WeightUQ = "kg";
			InvoiceLine.JI_Volume = 200;
			InvoiceLine.JI_VolumeUQ = "m3";
			AssertEquals("Weight in KG", 1m, ((IUltimateDistributee)InvoiceLine).ActualWeightInKG);
			AssertEquals("Volume in KG", 200m, ((IUltimateDistributee)InvoiceLine).ActualVolumeInM3);

			InvoiceLine.JI_WeightUQ = "AA";
			AssertEquals("Weight UQ is invalid", 0m, ((IUltimateDistributee)InvoiceLine).ActualWeightInKG);
			InvoiceLine.JI_VolumeUQ = "BB";
			AssertEquals("Volume UQ is invalid", 0m, ((IUltimateDistributee)InvoiceLine).ActualVolumeInM3);
		}

		public void TestItemCount()
		{
			InvoiceLine.JI_InvoiceQuantity = 1;
			InvoiceLine.JI_CustomsQuantity = 2;
			AssertEquals("ItemCount", 1m, ((IUltimateDistributee)InvoiceLine).ItemCount);
			InvoiceLine.JI_InvoiceQuantity = 0;
			InvoiceLine.JI_CustomsQuantity = 2;
			AssertEquals("ItemCount", 2m, ((IUltimateDistributee)InvoiceLine).ItemCount);
		}

		protected virtual void SetupInvoiceLineToTestIUltimateDistributee()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceLine.JI_Weight = 1;
			InvoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			InvoiceLine.JI_Volume = 200;
			InvoiceLine.JI_VolumeUQ = Core.Constants.Volume.Litre;
			InvoiceLine.JI_InvoiceQuantity = 100m;
			InvoiceLine.JI_LinePrice = 20000m;
		}

		public void TestIUltimateDistributeeWithInvoiceLine()
		{
			SetupInvoiceLineToTestIUltimateDistributee();

			AssertEquals("IUltimateDistributee.PK", InvoiceLine.PK, ((IUltimateDistributee)InvoiceLine).PK);
			AssertEquals("IUltimateDistributee.TableCode", JobComInvoiceLineSchema.Constants.Prefix, ((IUltimateDistributee)InvoiceLine).TableCode);
			AssertEquals("IUltimateDistributee.WeightInKG", 1000m, ((IUltimateDistributee)InvoiceLine).ActualWeightInKG);
			AssertEquals("IUltimateDistributee.VolumeInM3", 0.2m, ((IUltimateDistributee)InvoiceLine).ActualVolumeInM3);
			AssertEquals("IUltimateDistributee.UnitPriceInInvoiceCurrency", 200m, ((IUltimateDistributee)InvoiceLine).UnitPriceInInvoiceCurrency);
			InvoiceLine.JI_OP = ZGuid.NewZGuid();

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			AssertEquals("IUltimateDistributee.Actual for Air", 1000m, ((IUltimateDistributee)InvoiceLine).Actual);

			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			AssertEquals("IUltimateDistributee.Actual for SEA", 0.2m, ((IUltimateDistributee)InvoiceLine).Actual);

			AssertEquals("IUltimateDistributee.FKToProduct", InvoiceLine.JI_OP, ((IUltimateDistributee)InvoiceLine).FKToProduct);
		}

		public virtual void TestMoneyFOB_CIF_LinePriceIsValid()
		{
			InvoiceHeader.JZ_IncoTerm = "FOB";
			InvoiceHeader.JZ_InvoiceAmount = 1000m;
			InvoiceLine.JI_LinePrice = 1000m;

			InvoiceHeader.JZ_RX_NKInvoice_Currency = ZString.Empty;
			if (Declaration.IsDeclarationIntegrated)
			{
				AssertEquals("JI_FOB is valid", true, InvoiceLine.JI_FOB.IsValid);
			}
			else
			{
				AssertEquals("JI_FOB is invalid : No Currency", false, InvoiceLine.JI_FOB.IsValid);
			}

			AssertEquals("JI_CIF is invalid : No Currency", false, InvoiceLine.JI_CIF.IsValid);
			AssertEquals("JI_LinePriceMoney is invalid : No Currency", false, InvoiceLine.JI_LinePriceMoney.IsValid);

			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			AssertEquals("JI_FOB is valid", true, InvoiceLine.JI_FOB.IsValid);
			AssertEquals("JI_CIF is valid", true, InvoiceLine.JI_CIF.IsValid);
			AssertEquals("JI_LinePriceMoney is valid", true, InvoiceLine.JI_LinePriceMoney.IsValid);
		}

		public void TestFOBInLocalCurrencyWithoutInvoiceHeader()
		{
			AssertEquals("Without currency", ZDecimal.Zero, InvoiceLine.JI_Calc_FOB_InLocalCurrency);
		}

		public void TestEffectiveGrossWeightReturnsActualWeight()
		{
			InvoiceLine.JI_Weight = 500m;
			InvoiceLine.JI_WeightUQ = "KG";
			AssertEquals(new ZWeight(500, "KG"), InvoiceLine.EffectiveGrossWeight);
		}

		public virtual void TestEffectiveGrossWeightReturnsApportionedWeightWhenSomeWeightKnown()
		{
			InvoiceHeader.JZ_InvoiceAmount = 300m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceHeader.JZ_Weight = 1000m;
			InvoiceHeader.JZ_WeightUQ = "KG";
			var line1 = InvoiceLine;
			line1.JI_Weight = 500m;
			line1.JI_WeightUQ = "KG";
			line1.JI_LinePrice = 100m;
			var line2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Weight = 0m;
			line2.JI_WeightUQ = "KG";
			line2.JI_LinePrice = 120m;
			var line3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_Weight = 0m;
			line3.JI_WeightUQ = "KG";
			line3.JI_LinePrice = 80m;

			AssertEquals(new ZWeight(500, "KG"), line1.EffectiveGrossWeight);
			AssertEquals(new ZWeight(300, "KG"), line2.EffectiveGrossWeight);
			AssertEquals(new ZWeight(200, "KG"), line3.EffectiveGrossWeight);
		}

		public virtual void TestEffectiveGrossWeightReturnsApportionedWeightWhenNoWeightKnown()
		{
			InvoiceHeader.JZ_InvoiceAmount = 300m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceHeader.JZ_Weight = 300m;
			InvoiceHeader.JZ_WeightUQ = "KG";
			var line1 = InvoiceLine;
			line1.JI_Weight = 0m;
			line1.JI_WeightUQ = "KG";
			line1.JI_LinePrice = 100m;
			var line2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Weight = 0m;
			line2.JI_WeightUQ = "KG";
			line2.JI_LinePrice = 120m;
			var line3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_Weight = 0m;
			line3.JI_WeightUQ = "KG";
			line3.JI_LinePrice = 80m;

			AssertEquals(new ZWeight(100, "KG"), line1.EffectiveGrossWeight);
			AssertEquals(new ZWeight(120, "KG"), line2.EffectiveGrossWeight);
			AssertEquals(new ZWeight(80, "KG"), line3.EffectiveGrossWeight);
		}

		public void TestEffectiveVolumeReturnsActualVolume()
		{
			InvoiceLine.JI_Volume = 500m;
			InvoiceLine.JI_VolumeUQ = "M3";
			AssertEquals(new ZVolume(500, "M3"), InvoiceLine.EffectiveVolume);
		}

		public void TestEffectiveVolumeReturnsApportionedVolumeWhenSomeVolumeKnown()
		{
			InvoiceHeader.JZ_InvoiceAmount = 300m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceHeader.JZ_Volume = 1000m;
			InvoiceHeader.JZ_VolumeUQ = "M3";
			var line1 = InvoiceLine;
			line1.JI_Volume = 500m;
			line1.JI_VolumeUQ = "M3";
			line1.JI_LinePrice = 100m;
			var line2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Volume = 0m;
			line2.JI_VolumeUQ = "M3";
			line2.JI_LinePrice = 120m;
			var line3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_Volume = 0m;
			line3.JI_VolumeUQ = "M3";
			line3.JI_LinePrice = 80m;

			AssertEquals(new ZVolume(500, "M3"), line1.EffectiveVolume);
			AssertEquals(new ZVolume(300, "M3"), line2.EffectiveVolume);
			AssertEquals(new ZVolume(200, "M3"), line3.EffectiveVolume);
		}

		public void TestEffectiveVolumeReturnsApportionedVolumeWhenNoVolumeKnown()
		{
			InvoiceHeader.JZ_InvoiceAmount = 300m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			InvoiceHeader.JZ_Volume = 300m;
			InvoiceHeader.JZ_VolumeUQ = "M3";
			var line1 = InvoiceLine;
			line1.JI_Volume = 0m;
			line1.JI_VolumeUQ = "M3";
			line1.JI_LinePrice = 100m;
			var line2 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line2.JI_Volume = 0m;
			line2.JI_VolumeUQ = "M3";
			line2.JI_LinePrice = 120m;
			var line3 = InvoiceHeader.JobComInvoiceLines.AddNew();
			line3.JI_Volume = 0m;
			line3.JI_VolumeUQ = "M3";
			line3.JI_LinePrice = 80m;

			AssertEquals(new ZVolume(100, "M3"), line1.EffectiveVolume);
			AssertEquals(new ZVolume(120, "M3"), line2.EffectiveVolume);
			AssertEquals(new ZVolume(80, "M3"), line3.EffectiveVolume);
		}

		public void TestOrderNumberTransfersToJobOrderItemOnceOnly()
		{
			InvoiceLine.JI_OrderNumber = "XXX";
			AssertEquals("Should contain the order number now", true, Declaration.DocsAndCartage.OrderItems.AsString.IndexOf("XXX") != -1);
			InvoiceLine.JI_OrderNumber = "xxx";
			AssertEquals("Item Count", 1, Declaration.DocsAndCartage.OrderItems.Count);
			InvoiceLine.JI_OrderNumber = "YYY";
			AssertEquals("Item Count", 2, Declaration.DocsAndCartage.OrderItems.Count);
		}

		public virtual void TestCustomsQuantityIsReadonlyWhenUnitQtyEmpty()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_CustomsUnitQty = "";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			AssertEquals(false, InvoiceLine.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestJI_OrderNumber()
		{
			var line = (BaseJobComInvoiceLine)GetNewBusinessObject();
			AssertEquals("JI_OrderNumber should not be read only when invoice line is not from OrderLine", false, line.JI_OrderNumberInfo.ReadOnly);
			line.JI_OrderNumber = "Order";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "X1!2";
			org.OH_FullName = org.OH_Code;
			org.MainAddress.Address1 = "XXX";
			var order = Factory.New<Order>();
			order.BuyerPK = org.PK;
			order.SupplierPK = org.PK;
			order.JD_OrderNumber = "ActualOrder";
			var orderLine = order.OrderLines.AddNew();

			line.JI_JO = orderLine.PK;
			AssertEquals("JI_OrderNumber should now be the order that the line is attached to", order.JD_OrderNumber, line.JI_OrderNumber);
			AssertEquals("JI_OrderNumber should be read only when invoice line is from OrderLine", true, line.JI_OrderNumberInfo.ReadOnly);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var line2 = factory2.LoadTop1<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.PK, line.PK));
			AssertEquals("JI_OrderNumber should be read only when loaded if attatched to a order line", true, line2.JI_OrderNumberInfo.ReadOnly);
			Assert("Line should have valid Order", line.HasValidOrder);

			line.JI_JO = ZGuid.Empty;
			AssertEquals("JI_OrderNumber should not be read only when invoice line is not attatched to OrderLine", false, line.JI_OrderNumberInfo.ReadOnly);
			AssertEquals("Line should not have valid Order", false, line.HasValidOrder);

			line.JI_JO = ZGuid.Invalid;
			AssertEquals("JI_OrderNumber should not be read only when invoice line is not attatched to OrderLine", false, line.JI_OrderNumberInfo.ReadOnly);
			AssertEquals("Line should not have valid Order", false, line.HasValidOrder);
		}

		public void TestJI_OrderNumberOnNormalDeclaration()
		{
			InvoiceLine.JI_OrderNumber = "123456";
			AssertEquals("One Order Item should exist", 1, Declaration.DocsAndCartage.OrderItems.Count);
			AssertEquals("Order Number", "123456", Declaration.DocsAndCartage.OrderItems[0].JT_OrderReference);

			var order = Declaration.AttachedOrders.AddNew();
			order.JD_OrderNumber = "AAA";
			InvoiceLine.JI_OrderNumber = "aaa";
			AssertEquals("Item Count", 1, Declaration.DocsAndCartage.OrderItems.Count);
			InvoiceLine.JI_OrderNumber = "bbb";
			AssertEquals("Item Count", 2, Declaration.DocsAndCartage.OrderItems.Count);
		}

		public void TestJI_OrderNumberOnCommervialInvoiceOnly()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			var declaration = (BaseJobDeclaration)new FakeDeclarationCreatorForInvoice(invoiceHeader).HeaderData;
			var line3 = invoiceHeader.InvoiceLines.AddNew();
			line3.JI_OrderNumber = "000003";
			AssertEquals("Should not contain the order number now", false, declaration.DocsAndCartage.OrderItems.AsString.Contains("000003"));
		}

		public void TestOrderLines()
		{
			AssertNull("OrderLine should be null", InvoiceLine.OrderLine);

			var orderLine = Factory.New<OrderLine>();
			InvoiceLine.JI_JO = orderLine.PK;
			AssertNotNull("Orderline should not be null", InvoiceLine.OrderLine);
		}

		public void TestJI_Calc_OrderLineNumberAndSubLine()
		{
			AssertEquals("Calc OrderLineNumber should be empty", ZString.Empty, InvoiceLine.JI_Calc_OrderLineNumberAndSubLine);
			var orderLine = Factory.New<OrderLine>();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 1;
			InvoiceLine.JI_JO = orderLine.PK;

			AssertEquals("Calc OrderLineNumberAndSubLine should be '1'", "1", InvoiceLine.JI_Calc_OrderLineNumberAndSubLine);
		}

		public void TestAdValoremDutyPercent()
		{
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_DutyPercent = 0.5m;

			AssertEquals("Duty percent", 0m, InvoiceLine.AdValoremDutyPercent);

			InvoiceLine.JI_CL = entryLine.PK;
			AssertEquals("Duty Percent", 0.5m, InvoiceLine.AdValoremDutyPercent);
		}

		public void TestFieldsThatShouldNotAffectMerge()
		{
			var declaration = ImportJobDeclaration;

			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(declaration.CountryCode) || declaration.IsDeclarationIntegrated)
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError
			(
				JobComInvoiceLineSchema.Constants.JI_RH_NKCommodity_Code,
				Factory.NewWithValidTestData<RefCommodityCode>().RH_Code
			);
		}

		public void TestJI_Calc_Invoice()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			var invoiceLine = invoice1.JobComInvoiceLines.AddNew();
			AssertEquals("JI_JZ is set", invoice1.PK, invoiceLine.JI_JZ);
			invoiceLine.JI_Calc_Invoice = "2";
			AssertEquals("JI_JZ is set", invoice2.PK, invoiceLine.JI_JZ);
		}

		public void TestIUnitConverterDataProviderType()
		{
			var line = Factory.New<BaseJobComInvoiceLine>();
			AssertEquals("Pack Conversion Type should be Commercial Invoice", RPTypeList.Codes.CommercialInvoice, ((IUnitConverterDataProvider)line).Type);
		}

		public virtual void TestJI_DutiableAdditions()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			var charges = InvoiceLine.Charges;
			var includedApplicableCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 100m, Declaration.LocalCurrencyCode);
			includedApplicableCharge.J7_IsIncludedInITOT = true;
			includedApplicableCharge.J7_IsDutiable = true;

			var includedNotApplicableCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 200m, Declaration.LocalCurrencyCode);
			includedNotApplicableCharge.J7_IsIncludedInITOT = true;
			includedNotApplicableCharge.J7_IsDutiable = false;

			var notIncludedApplicableCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 300m, Declaration.LocalCurrencyCode);
			notIncludedApplicableCharge.J7_IsDutiable = true;

			var discountNotApplicable = charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400m, Declaration.LocalCurrencyCode);
			discountNotApplicable.J7_IsDutiable = false;

			AssertEquals(300m, InvoiceLine.JI_DutiableAdditions.Amount);
		}

		public virtual void TestJI_NonDutiableDeductions()
		{
			InvoiceHeader.JZ_RX_NKInvoice_Currency = Declaration.LocalCurrencyCode;
			var charges = InvoiceLine.Charges;
			var includedApplicableCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 100m, Declaration.LocalCurrencyCode);
			includedApplicableCharge.J7_IsIncludedInITOT = true;
			includedApplicableCharge.J7_IsDutiable = true;

			var includedNotApplicableCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 200m, Declaration.LocalCurrencyCode);
			includedNotApplicableCharge.J7_IsIncludedInITOT = true;
			includedNotApplicableCharge.J7_IsDutiable = false;

			var notIncludedApplicableCharge = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 300m, Declaration.LocalCurrencyCode);
			notIncludedApplicableCharge.J7_IsDutiable = true;

			var discountNotApplicable = charges.AddNew(Common.CustomsChargeTypeList.Codes.Discount, 400m, Declaration.LocalCurrencyCode);
			discountNotApplicable.J7_IsDutiable = false;

			AssertEquals(200m, InvoiceLine.JI_NonDutiableDeductions.Amount);
		}

		public void TestChargesToImportForLandedCosting()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var chargeTypeList = ((ICommonInvoice)invoiceLine).ChargeTypeList;

			var containsDiscount = false;
			var containsOverseasFreight = false;
			foreach (CodeDescriptionPair charge in chargeTypeList)
			{
				if (charge.Code == Common.CustomsChargeTypeList.Codes.Discount)
				{
					var lineCharge = invoiceLine.Charges.AddNew(charge.Code, 100m, dec.LocalCurrencyCode);
					containsDiscount = ((IDefaultLandedCostInput)lineCharge).IsValidToImport;
				}
				else if (charge.Code == Common.CustomsChargeTypeList.Codes.OverseasFreight)
				{
					var lineCharge = invoiceLine.Charges.AddNew(charge.Code, 200m, dec.LocalCurrencyCode);
					containsOverseasFreight = ((IDefaultLandedCostInput)lineCharge).IsValidToImport;
				}
			}
			invoiceLine.Charges.AddNew("AA!", 400m, dec.LocalCurrencyCode);
			dec.ApportionmentDirty = false;

			int expectedCount = (containsDiscount ? 1 : 0) + (containsOverseasFreight ? 1 : 0);
			var chargesToImportForLandedCosting = ((ILandedCostChargeHolder)invoiceLine).ChargesToImportForLandedCosting;
			AssertEquals("ChargesToImportForLandedCosting count", expectedCount, chargesToImportForLandedCosting.Count());

			if (containsDiscount)
			{
				var landedCostInput = chargesToImportForLandedCosting.Cast<IDefaultLandedCostInput>().FirstOrDefault(c => c.ChargeDescription == "Deduction (or Discount) from Entry");
				AssertEquals("Amount", -100M, landedCostInput.AmountToDistribute.Amount);
			}
			if (containsOverseasFreight)
			{
				var landedCostInput = chargesToImportForLandedCosting.Cast<IDefaultLandedCostInput>().FirstOrDefault(c => c.ChargeDescription == OFTChargeDescription);
				AssertEquals("Amount", 200M, landedCostInput.AmountToDistribute.Amount);
			}
		}

		protected virtual ZString OFTChargeDescription => "INTERNATIONAL FREIGHT from Entry";

		public void TestComponentInventoryCollection()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			var inventory1 = Factory.New<JobComInvLineComponentInventory>();
			inventory1.JIV_JI = invoiceLine.PK;
			var inventory2 = Factory.New<JobComInvLineComponentInventory>();
			inventory2.JIV_JI = invoiceLine.PK;

			AssertEquals(2, invoiceLine.ComponentInventoryCollection.Count);

			var inventoryCollection = invoiceLine.ComponentInventoryCollection.Cast<JobComInvLineComponentInventory>();
			Assert(inventoryCollection.Contains(inventory1));
			Assert(inventoryCollection.Contains(inventory2));
		}

		public void TestDelelePackableItems()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "EXP";
			var invoice = dec.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			var packableItem = line.CreateNewCusPackableItem();
			dec.LoadOrCreateCusPackingList(Factory).PackableItems.Add(packableItem);
			line.Delete();

			Assert(line.IsDeleted);
			Assert(packableItem.IsDeleted);
		}

		public virtual void TestGetPackableItemUQList()
		{
			AssertEquals(InvoiceLine.Lookups.CustomsUQList.CodesAsString, InvoiceLine.GetPackableItemUQList().CodesAsString);
		}

		public void TestSynchroniseFromForwardingOrderLineCore()
		{
			var invoiceLine = (BaseJobComInvoiceLine)GetNewBusinessObject();
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();
			PrepareOrderLine(order, orderLine);

			((Integration.Customs.IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);

			AssertEquals("PAK", invoiceLine.JI_InvoiceUQ);
			AssertEquals("STUFF", invoiceLine.JI_Description);
			AssertEquals("PartNo", invoiceLine.JI_PartNo);
			AssertEquals("CA1", invoiceLine.JI_CustomAttrib1);
			AssertEquals("CA2", invoiceLine.JI_CustomAttrib2);
			AssertEquals("CA3", invoiceLine.JI_CustomAttrib3);
			AssertEquals("CA4", invoiceLine.JI_CustomAttrib4);
			AssertEquals("CA5", invoiceLine.JI_CustomAttrib5);
			AssertEquals("CA6", invoiceLine.JI_CustomAttrib6);
			AssertEquals("CB1", invoiceLine.JI_CustomTextBlob1);
			AssertEquals("PA1", invoiceLine.JI_PartAttrib1);
			AssertEquals("PA2", invoiceLine.JI_PartAttrib2);
			AssertEquals("PA3", invoiceLine.JI_PartAttrib3);
			AssertEquals("SN1", invoiceLine.JI_SerialNumber);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), invoiceLine.JI_CustomDate1);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(2), invoiceLine.JI_CustomDate2);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(3), invoiceLine.JI_CustomDate3);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(4), invoiceLine.JI_CustomDate4);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(5), invoiceLine.JI_CustomDate5);
			AssertEquals(1m, invoiceLine.JI_CustomDecimal1);
			AssertEquals(2m, invoiceLine.JI_CustomDecimal2);
			AssertEquals(3m, invoiceLine.JI_CustomDecimal3);
			AssertEquals(4m, invoiceLine.JI_CustomDecimal4);
			AssertEquals(5m, invoiceLine.JI_CustomDecimal5);
			AssertEquals(true, invoiceLine.JI_CustomFlag1);
			AssertEquals(true, invoiceLine.JI_CustomFlag2);
			AssertEquals(true, invoiceLine.JI_CustomFlag3);
			AssertEquals(true, invoiceLine.JI_CustomFlag4);
			AssertEquals(true, invoiceLine.JI_CustomFlag5);
			AssertEquals(orderLine.PK, invoiceLine.JI_JO);
			AssertEquals("Order 1", invoiceLine.JI_OrderNumber);
			AssertEquals(600m, invoiceLine.JI_InvoiceQuantity);
			AssertEquals(1200m, invoiceLine.JI_LinePrice);
			AssertEquals("SynchroniseFromForwardingOrderLine does not set invoice header's amount, that's done by the reconciliator", 0m, invoiceLine.InvoiceHeader.JZ_InvoiceAmount);
			AssertEquals("GB", invoiceLine.CountryOfOriginFieldInfo.Value);

			orderLine.JO_QtyInvoiced = 0m;
			orderLine.JO_LinePrice = 999m;
			((Integration.Customs.IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			AssertEquals("When JO_QtyInvoiced is missing, fall back JO_LinePrice", 999m, invoiceLine.JI_LinePrice);
		}

		public void TestSynchroniseFromForwardingOrderLineCorePackType()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "Com1";
			var address = organisation.AddressesActive.AddNew();
			address.OA_Address1 = "ADDRESS 1";
			address.OA_City = "CHICAGO";
			address.OA_State = "IL";
			address.OA_PostCode = "60010";
			address.OA_RL_NKRelatedPortCode = "USCHI";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "KG";
			part.OP_PartNum = "Part";
			part.RelatedOrganisations.AddOrganisationIfNotExist(organisation.PK, OrgPartRelation.RelationshipTypes.Owner);
			var order = Factory.New<Order>();
			order.BuyerPK = organisation.PK;
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_Partno = part.OP_PartNum;
			orderLine.JO_F3_NKPackType = "NO";
			var invoiceLine = (BaseJobComInvoiceLine)GetNewBusinessObject();
			invoiceLine.Declaration.JE_OH_Importer = organisation.PK;
			((Integration.Customs.IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			AssertEquals("NO", invoiceLine.JI_InvoiceUQ);
		}

		public void TestSynchroniseFromForwardingOrderLineCoreOrigin()
		{
			var invoiceLine = (BaseJobComInvoiceLine)GetNewBusinessObject();
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "090909";
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_F3_NKPackType = "UNT";
			orderLine.JO_QtyInvoiced = 1;

			((Integration.Customs.IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			AssertEquals(ZString.Empty, invoiceLine.CountryOfOriginFieldInfo.Value);

			invoiceLine.CountryOfOriginFieldInfo.Value = new ZString("US");
			((Integration.Customs.IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			AssertEquals("US", invoiceLine.CountryOfOriginFieldInfo.Value);

			orderLine.JO_RN_NKCountryOfOrigin = "CN";
			((Integration.Customs.IBaseJobComInvoiceLine)invoiceLine).SynchroniseFromForwardingOrderLine(orderLine, true);
			AssertEquals("CN", invoiceLine.CountryOfOriginFieldInfo.Value);
		}

		public void TestJI_AddInfo()
		{
			var line = (BaseJobComInvoiceLine)GetNewBusinessObject();
			var addInfo = (line as IAddInfoManager)?.AddInfo as BaseAddInfo;
			if (addInfo != null && addInfo.ZPropertyInfoHash.Count > 0)
			{
				foreach (var properyInfos in addInfo.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(x => line.FindPropertyInfo(x.Name) != null).Batch(20))
				{
					line.JI_AddInfo = string.Join("*", properyInfos.Select(x => x.Name.Remove(0, 3) + "=").ToArray());
					foreach (var propertyInfo in properyInfos)
					{
						if (propertyInfo.IsNAddInfoField())
						{
							Assert(propertyInfo.Name + " should be stored in JI_NAddInfo", line.JI_NAddInfo.Contains(propertyInfo.Name.Remove(0, 3) + "="));
						}
						else
						{
							Assert(propertyInfo.Name + " should NOT be stored in JI_NAddInfo", !line.JI_NAddInfo.Contains(propertyInfo.Name.Remove(0, 3) + "="));
						}
					}
				}
			}

			Assert(true);
		}

		public void TestTypeOfApportionedCharges()
		{
			AssertEquals("ApportionedCharges' type should be expected", ExpectedTypeOfApportionedCharges, InvoiceLine.ApportionedCharges.GetType());
		}

		public void TestTypeOfCharges()
		{
			AssertEquals("Charges' type should be expected", ExpectedTypeOfCharges, InvoiceLine.Charges.GetType());
		}

		protected virtual Type ExpectedTypeOfApportionedCharges => typeof(JobComInvApportionedChargeCollection<BaseInvoiceLineApportionedCharge>);

		protected virtual Type ExpectedTypeOfCharges => typeof(JobComInvChargeCollection<BaseInvoiceLineCharge>);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			return invoiceLine;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<BaseJobDeclaration>();

			var importer = factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var result = invoiceHeader.InvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			if (declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine)
			{
				var link = (result.AdditionalEntryLineLinks.Count > 0 ? result.AdditionalEntryLineLinks[0] : null)
					?? result.AdditionalEntryLineLinks.AddNew();
				link.BU_CL = entryLine.PK;
				link.BU_JI = result.PK;
			}

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			return result;
		}

		BaseJobDeclaration declaration;
		protected BaseJobDeclaration Declaration => declaration ?? (declaration = GetJobDeclaration());

		BaseJobComInvoiceHeader invoiceHeader;
		protected BaseJobComInvoiceHeader InvoiceHeader => invoiceHeader ?? (invoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew());

		BaseJobComInvoiceLine invoiceLine;
		protected BaseJobComInvoiceLine InvoiceLine => invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());

		protected virtual BaseJobDeclaration GetJobDeclaration() => Factory.New<BaseJobDeclaration>();

		protected void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency)
		{
			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency.RX_Code);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, "CUS");
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sQLFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);

			var exchangeRateDuty = Factory.LoadTop1<RefExchangeRate>(sQLFilter);
			if (exchangeRateDuty != null)
			{
				exchangeRateDuty.Delete();
			}

			RefExchangeRate newOne = Factory.New<RefExchangeRate>();
			newOne.RE_ExpiryDate = endDate;
			newOne.RE_ExRateType = "CUS";
			newOne.RE_GC = GlbCompany.CurrentCompany.PK;
			newOne.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			newOne.RE_StartDate = startDate;
			newOne.RE_SellRate = exchangeRate;
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var invoiceLine = (BaseJobComInvoiceLine)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			return invoiceLine;
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				Dictionary<string, IZType> result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result.Add(BaseJobComInvoiceLine.Schema.JI_LineNo, (ZShort)1);
				return result;
			}
		}

		protected virtual bool RatesAreReciprocal => false;

		protected virtual ZString TariffDataGrouping => ZString.Empty;

		protected virtual ZString UniversalTariffTypeForDefaultTaxOrFeeCode => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected virtual bool ShouldMatchHTBForTestPivot => true;

		protected virtual ZString DeclarationImportMessageType => JobMessageTypeList.Codes.Import;

		protected virtual ZString DeclarationExportMessageType => JobMessageTypeList.Codes.Export;

		protected virtual void AfterInitialise(BaseJobDeclaration declaration)
		{
		}

		protected virtual bool UseUniversalTariff => true;

		protected virtual ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected virtual void DoMerge(BaseJobDeclaration declaration)
		{
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected virtual BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = Factory.New<BaseJobDeclaration>();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				return result;
			}
		}

		protected virtual string OverseasFreightCode => CustomsChargeTypeList.Codes.OverseasFreight;

		protected virtual string OverseasInsuranceCode => CustomsChargeTypeList.Codes.OverseasInsurance;

		static void AssertPackageAddedToInvoiceLine(BaseJobComInvoiceLine invoiceLine, BasePackage package, string packgeName)
		{
			Assert($"Package ({packgeName}) must be added once", package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().Count(pivot => pivot.InvoiceLine.PK == invoiceLine.PK) == 1);
			Assert($"Package ({packgeName}) must be added once", invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Count(pivot => pivot.Package.PK == package.PK) == 1);
		}

		static void AssertPackageNotAddedToInvoiceLine(BaseJobComInvoiceLine invoiceLine, BasePackage package, string packgeName)
		{
			Assert($"Package ({packgeName}) must not be added", !package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().Any(pivot => pivot.InvoiceLine.PK == invoiceLine.PK));
			Assert($"Package ({packgeName}) must not be added", !invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(pivot => pivot.Package.PK == package.PK));
		}

		void SettingMergeAffectingFieldsShouldNotThrowExceptionOrDeveloperError(ZString fieldName, object value)
		{
			SettingMergeAffectingFieldsWhenNoEntryHeader(fieldName, value);
			SettingMergeAffectingFieldsWhenThereAreMergedEntries(fieldName, value);
		}

		void SettingMergeAffectingFieldsWhenNoEntryHeader(ZString fieldName, object value)
		{
			ErrorReporter.Clear();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AssertEquals("No CusEntryHeaders expected", 0, declaration.CustomsEntryHeaders.Count);
			AssertNotEquals("Preconditions: New value should be different from the initial value", value, line[fieldName]);
			line[fieldName] = value;
			Factory.Save();
			AssertEquals("No Developer Error Expected when no CusEntryHeader", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		void SettingMergeAffectingFieldsWhenThereAreMergedEntries(ZString fieldName, object value)
		{
			ErrorReporter.Clear();
			var declaration = ImportJobDeclaration;

			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			DoMerge(declaration);
			Factory.Save();
			AssertEquals("Preconditions: Non-zero EntryHeaders expected on Declaration", true, declaration.CustomsEntryHeaders.Count > 0);
			AssertEquals("Precondtions: No Developer Error Expected", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			AssertNotEquals("Preconditions: New value should be different from the initial value", value, line[fieldName]);
			line[fieldName] = value;
			Factory.Save();
			AssertEquals("No Developer Error Expected when there is at least one CusEntryHeader", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		void PrepareOrderLine(Order order, OrderLine orderLine)
		{
			orderLine.JO_F3_NKPackType = "PAK";
			orderLine.JO_Description = "STUFF";
			orderLine.JO_Partno = "PartNo";
			orderLine.JO_CustomAttrib1 = "CA1";
			orderLine.JO_CustomAttrib2 = "CA2";
			orderLine.JO_CustomAttrib3 = "CA3";
			orderLine.JO_CustomAttrib4 = "CA4";
			orderLine.JO_CustomAttrib5 = "CA5";
			orderLine.JO_CustomAttrib6 = "CA6";
			orderLine.JO_CustomTextBlob1 = "CB1";
			orderLine.JO_PartAttrib1 = "PA1";
			orderLine.JO_PartAttrib2 = "PA2";
			orderLine.JO_PartAttrib3 = "PA3";
			orderLine.JO_SerialNumber = "SN1";
			orderLine.JO_CustomDate1 = ZDateTime.BrettsBirthday.AddDays(1);
			orderLine.JO_CustomDate2 = ZDateTime.BrettsBirthday.AddDays(2);
			orderLine.JO_CustomDate3 = ZDateTime.BrettsBirthday.AddDays(3);
			orderLine.JO_CustomDate4 = ZDateTime.BrettsBirthday.AddDays(4);
			orderLine.JO_CustomDate5 = ZDateTime.BrettsBirthday.AddDays(5);
			orderLine.JO_CustomDecimal1 = 1m;
			orderLine.JO_CustomDecimal2 = 2m;
			orderLine.JO_CustomDecimal3 = 3m;
			orderLine.JO_CustomDecimal4 = 4m;
			orderLine.JO_CustomDecimal5 = 5m;
			orderLine.JO_CustomFlag1 = true;
			orderLine.JO_CustomFlag2 = true;
			orderLine.JO_CustomFlag3 = true;
			orderLine.JO_CustomFlag4 = true;
			orderLine.JO_CustomFlag5 = true;
			order.JD_OrderNumber = "Order 1";
			orderLine.JO_ItemPrice = 2m;
			orderLine.JO_Quantity = 1000m;
			// orderLine.JO_LinePrice is set to 2000m implicitly
			orderLine.JO_QtyInvoiced = 600m;
			orderLine.JO_RN_NKCountryOfOrigin = "GB";
			orderLine.JO_ActualWeight = 6900m;
			orderLine.JO_ActualVolume = 7000m;
			orderLine.JO_UnitOfWeight = "KG";
			orderLine.JO_UnitOfVolume = "M3";
		}

		sealed class BaseJobComInvoiceLine_ForCustomsUnitDefaultingStrategyTest : BaseJobComInvoiceLineForTesting
		{
			public BaseJobComInvoiceLine_ForCustomsUnitDefaultingStrategyTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool CustomsUnitDefaultingStrategyInitialised { get; private set; }

			protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy()
			{
				var uomDefaultingStrategyMock = new Mock<ICustomsUnitDefaultingStrategy>();
				uomDefaultingStrategyMock
					.Setup(m => m.Initialise(It.IsAny<BaseJobComInvoiceLine>()))
					.Callback(() => CustomsUnitDefaultingStrategyInitialised = true);
				return uomDefaultingStrategyMock.Object;
			}
		}
	}
}
