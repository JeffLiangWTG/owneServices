using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ManyToManyOrgMergerTest : TestCaseWithFactory
	{
		public void TestContructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new ManyToManyOrgMerger(null); });
		}

		public void TestMergeOrgs()
		{
			var query = new ZQuery();
			query.AddToFilter(OrgHeaderSchema.OH_Language, SQLComparisonOperator.NotEqual, "ZZZ");

			var unlocoQuery = new ZQuery();
			unlocoQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, "UHAHA");
			unlocoQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.OH_RL_NKClosestPort, "ROHAN");
			query.AddToFilter(unlocoQuery);

			var reader = new FilteredBusinessObjectReader(query, typeof(OrgHeader));
			reader.BatchSize = 5;
			var merger = new ManyToManyOrgMerger(reader);
			merger.BatchCompleted += new ManyToManyEventHandler(merger_BatchCompleted);
			var bunch1 = new List<ZGuid>();
			ZGuid notMerged11;
			ZGuid notMerged12;
			ZGuid notMerged13;
			ZGuid notMerged14;
			ZGuid notMerged15;
			bunch1.Add(CreateOrgHeader(false, "MATCH ME ORG", "MATCH ME ADDRESS", "UHAHA"));
			bunch1.Add(notMerged11 = CreateOrgHeader(false, "MATCH ME ORG", "MATCH ME ADDRESS", "ZZZZZ"));
			bunch1.Add(notMerged12 = CreateOrgHeader(true, "MATCH ME ORG", "MATCH ME ADDRESS", "UHAHA"));
			bunch1.Add(notMerged13 = CreateOrgHeader(false, "SOMETHING TERRIBLE", "ITS NOT ME", "UHAHA"));
			bunch1.Add(notMerged14 = CreateOrgHeader(false, "SOMETHING MATCHABLE", "LIKE ME", "ZION"));
			bunch1.Add(notMerged15 = CreateOrgHeader(false, "SOMETHING MATCHABLE", "LIKE ME", "ZION"));
			bunch1.Add(CreateOrgHeader(false, "MATCH ME ORG", "MATCH ME ADDRESS", "UHAHA"));
			bunch1.Add(CreateOrgHeader(false, "MATCH ME ORG", "MATCH ME ADDRESS", "UHAHA"));
			bunch1.Add(CreateOrgHeader(false, "MATCH ME ORG", "MATCH ME ADDRESS", "UHAHA"));
			bunch1.Add(CreateOrgHeader(false, "MATCH ME ORG", "MATCH ME ADDRESS", "UHAHA"));
			bunch1.Add(CreateOrgHeader(false, "MATCH ME ORG", "MATCH ME ADDRESS", "UHAHA"));

			var orgHeader = Factory.Load<OrgHeader>(notMerged14);
			orgHeader.MiscServ.OM_IMPartAttrib1Name = "YOU SHALL NOT MERGE!";
			orgHeader.MiscServ.OM_IMPartAttrib1Type = "MAN";

			ZGuid notMerged21;
			ZGuid notMerged22;
			ZGuid notMerged23;
			var bunch2 = new List<ZGuid>();
			bunch2.Add(CreateOrgHeader(false, "I FEEL WEIRD", "WHERE IS MY TABLE", "ROHAN"));
			bunch2.Add(notMerged21 = CreateOrgHeader(false, "I FEEL WEIRD", "WHERE IS MY TABLE", "SHIRE"));
			bunch2.Add(notMerged22 = CreateOrgHeader(true, "I FEEL WEIRD", "WHERE IS MY TABLE", "ROHAN"));
			bunch2.Add(notMerged23 = CreateOrgHeader(false, "THE BEST PHOTOCAMERA", "NIKON D80", "ROHAN"));
			bunch2.Add(CreateOrgHeader(false, "I FEEL WEIRD", "WHERE IS MY TABLE", "ROHAN"));
			bunch2.Add(CreateOrgHeader(false, "I FEEL WEIRD", "WHERE IS MY TABLE", "ROHAN"));
			bunch2.Add(CreateOrgHeader(false, "I FEEL WEIRD", "WHERE IS MY TABLE", "ROHAN"));
			bunch2.Add(CreateOrgHeader(false, "I FEEL WEIRD", "WHERE IS MY TABLE", "ROHAN"));
			bunch2.Add(CreateOrgHeader(false, "I FEEL WEIRD", "WHERE IS MY TABLE", "ROHAN"));

			Factory.Save();

			merger.Merge();

			var factory = new BusinessObjectFactory();

			var assertBunch1 = factory.Load<OrgHeader>(GetQueryForPKs(bunch1));
			var assertBunch2 = factory.Load<OrgHeader>(GetQueryForPKs(bunch2));

			AssertEquals(6, assertBunch1.Length);
			AssertEquals(4, assertBunch2.Length);
			Assert(CheckOrgArrayContainsPK(assertBunch1, notMerged11));
			Assert(CheckOrgArrayContainsPK(assertBunch1, notMerged12));
			Assert(CheckOrgArrayContainsPK(assertBunch1, notMerged13));
			Assert(CheckOrgArrayContainsPK(assertBunch1, notMerged14));
			Assert(CheckOrgArrayContainsPK(assertBunch1, notMerged15));
			Assert(CheckOrgArrayContainsPK(assertBunch2, notMerged21));
			Assert(CheckOrgArrayContainsPK(assertBunch2, notMerged22));
			Assert(CheckOrgArrayContainsPK(assertBunch2, notMerged23));

			OrgHeader mergedInto1 = null;
			foreach (OrgHeader org in assertBunch1)
			{
				if (org.PK != notMerged11 && org.PK != notMerged12 && org.PK != notMerged13 && org.PK != notMerged14 && org.PK != notMerged15)
				{
					mergedInto1 = org;
					break;
				}
			}

			OrgHeader mergedInto2 = null;
			foreach (OrgHeader org in assertBunch1)
			{
				if (org.PK != notMerged21 && org.PK != notMerged22 && org.PK != notMerged23)
				{
					mergedInto2 = org;
					break;
				}
			}

			AssertEquals(1, mergedInto1.AddressesNoAutoCreate.Count);
			AssertEquals(1, mergedInto2.AddressesNoAutoCreate.Count);
			AssertEquals(4, BatchCompletedHit);
			AssertEquals(14, merger.OrganisationsProcessed);
			AssertNotEquals(ZDateTime.MinSmallDateTimeValue, merger.StartDateTime);
			AssertNotEquals(ZDateTime.MinSmallDateTimeValue, merger.EndDateTime);
			Assert(merger.EndDateTime > merger.StartDateTime);
		}

		public void TestMergeOrgsWithoutError()
		{
			for (var i = 0; i < 8; i++)
			{
				CreateOrgHeader(false, "LONG LONG AGO", "THERE WAS A LION", "JMC");
			}
			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(OrgHeaderSchema.OH_Language, SQLComparisonOperator.Equal, "YYY");
			query.AddToFilter(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.Equal, "LONG LONG AGO");
			query.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "JMC");
			query.OrderBy = AutoOrgHeader.Schema.OH_Code;
			var reader = new FilteredBusinessObjectReader(query, typeof(OrgHeader)) { BatchSize = 2 };
			var merger = new ManyToManyOrgMerger(reader);

			merger.Merge();
			AssertNoExceptionThrown(() => merger.Merge());
		}

		public void TestMergeWithDuplicateProducts()
		{
			var org11 = Factory.Load<OrgHeader>(CreateOrgHeader(false, "MERGE TEST ORGANIZATION", "NO DUPLICATE PRODUCTS", "USNYC"));
			org11.OH_Code = "MERGETST11";
			var org12 = Factory.Load<OrgHeader>(CreateOrgHeader(false, "MERGE TEST ORGANIZATION", "NO DUPLICATE PRODUCTS", "USNYC"));
			org12.OH_Code = "MERGETST12";
			var org21 = Factory.Load<OrgHeader>(CreateOrgHeader(false, "MERGE TEST ORGANIZATION", "WITH DUPLICATE PRODUCTS", "AUSYD"));
			org21.OH_Code = "MERGETST21";
			var org22 = Factory.Load<OrgHeader>(CreateOrgHeader(false, "MERGE TEST ORGANIZATION", "WITH DUPLICATE PRODUCTS", "AUSYD"));
			org22.OH_Code = "MERGETST22";

			var part11 = Factory.New<OrgSupplierPart>();
			part11.OP_PartNum = "DUPTEST-11";
			part11.RelatedOrganisations.AddOwner(org11);

			var part12 = Factory.New<OrgSupplierPart>();
			part12.OP_PartNum = "DUPTEST-12";
			part12.RelatedOrganisations.AddOwner(org12);

			var part13 = Factory.New<OrgSupplierPart>();
			part13.OP_PartNum = "DUPTEST-13";
			part13.RelatedOrganisations.AddOwner(org11);
			part13.RelatedOrganisations.AddOwner(org12);

			var part21 = Factory.New<OrgSupplierPart>();
			part21.OP_PartNum = "DUPTEST-20";
			part21.RelatedOrganisations.AddOwner(org21);

			var part22 = Factory.New<OrgSupplierPart>();
			part22.OP_PartNum = "DUPTEST-20";
			part22.RelatedOrganisations.AddOwner(org22);

			Factory.Save();

			var orgQuery = new ZQuery();
			orgQuery.AddToFilter(OrgHeaderSchema.OH_FullName, "MERGE TEST ORGANIZATION");
			orgQuery.OrderBy = AutoOrgHeader.Schema.OH_Code;
			var reader = new FilteredBusinessObjectReader(orgQuery, typeof(OrgHeader)) { BatchSize = 4 };
			var merger = new ManyToManyOrgMerger(reader);

			merger.Merge();

			var partsQuery = new ZQuery();
			partsQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.StartsWith, "DUPTEST");

			var factory2 = NewFactory();

			var mergedOrgs = factory2.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "MERGETST1"));
			AssertEquals("2 organizations should be merged", 1, mergedOrgs.Length);

			var nonMergedOrgs = factory2.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "MERGETST2"));
			AssertEquals("2 organizations should not be merged because of duplicate products", 2, nonMergedOrgs.Length);

			var mergedOrgCode = mergedOrgs[0].OH_Code;

			AssertEquals(
				"2 organizations should be merged, and 2 organizations should not be merged because of duplicate products",
				string.Join("\r\n",
					$"{mergedOrgCode} OWN DUPTEST-11",
					$"{mergedOrgCode} OWN DUPTEST-12",
					$"{mergedOrgCode} OWN DUPTEST-13",
					"MERGETST21 OWN DUPTEST-20",
					"MERGETST22 OWN DUPTEST-20"
				),
				string.Join("\r\n", factory2.Load<OrgSupplierPart>(partsQuery).SelectMany(p => p.RelatedOrganisations.Cast<OrgPartRelation>()).Select(r =>
					$"{r.Organisation.OH_Code} {r.OU_Relationship} {r.SupplierPart.OP_PartNum}"
				).OrderBy(t => t))
			);
		}

		#region TestIsAllowedToMergeOrgs 

		public void TestIsAllowedToMergeOrgs_ManyToMany_DisallowAllMerges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var orgHeader1 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST1");
				var orgHeader2 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST2");
				var orgHeader3 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST3");
				var orgHeader4 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST4");

				var orgHeaderPK1 = orgHeader1.PK;
				var orgHeaderPK2 = orgHeader2.PK;
				var orgHeaderPK3 = orgHeader3.PK;
				var orgHeaderPK4 = orgHeader4.PK;

				//Setup all have different IVA#
				AddIVACustomsCode(orgHeader1, "111");
				AddIVACustomsCode(orgHeader2, "222");
				AddIVACustomsCode(orgHeader3, "333");
				AddIVACustomsCode(orgHeader4, "444");

				CreatePostedTransaction(orgHeader1, "INV01", company);
				CreatePostedTransaction(orgHeader2, "INV01", company);
				CreatePostedTransaction(orgHeader3, "INV01", company);
				CreatePostedTransaction(orgHeader4, "INV01", company);

				Factory.Save();

				var query = new ZQuery();
				query.AddToFilter(OrgHeaderSchema.OH_Language, "YYY");
				query.AddToFilter(OrgHeaderSchema.OH_FullName, "ORG TEST");
				query.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, "PT");
				query.OrderBy = AutoOrgHeader.Schema.OH_Code;
				var reader = new FilteredBusinessObjectReader(query, typeof(OrgHeader)) { BatchSize = 4 };

				AssertEquals("Number of organizations to be processed", 4, reader.Count());

				var merger = new ManyToManyOrgMerger(reader);
				merger.Merge();

				CombineAssertions(() =>
				{
					AssertEquals("Organization 1 should not be deleted after merge", false, orgHeader1.IsDeleted);
					AssertEquals("Organization 2 should not be deleted after merge", false, orgHeader2.IsDeleted);
					AssertEquals("Organization 3 should not be deleted after merge", false, orgHeader3.IsDeleted);
					AssertEquals("Organization 4 should not be deleted after merge", false, orgHeader4.IsDeleted);
				});

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				orgHeader1 = newFactory.Load<OrgHeader>(orgHeaderPK1);
				orgHeader2 = newFactory.Load<OrgHeader>(orgHeaderPK2);
				orgHeader3 = newFactory.Load<OrgHeader>(orgHeaderPK3);
				orgHeader4 = newFactory.Load<OrgHeader>(orgHeaderPK4);

				CombineAssertions(() =>
				{
					AssertNotNull("Organization 1 should exist in the database after merge", orgHeader1);
					AssertNotNull("Organization 2 should exist in the database after merge", orgHeader2);
					AssertNotNull("Organization 3 should exist in the database after merge", orgHeader3);
					AssertNotNull("Organization 4 should exist in the database after merge", orgHeader4);
				});

				CombineAssertions(() =>
				{
					AssertEquals("Number of merges that are not allowed", 3, merger.DisallowedMergeMessages.Count); //org 2, 3 & 4 were disallowed to merge with org 1
					AssertEquals("Number of organizations processed", 1, merger.OrganisationsProcessed); //only org1 that was processed
					AssertEquals("Number of organizations deleted", 0, merger.OrganisationsDeleted);
				});
			}
		}

		public void TestIsAllowedToMergeOrgs_ManyToMany_AllowAllMerges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var orgHeader1 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST1");
				var orgHeader2 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST2");
				var orgHeader3 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST3");
				var orgHeader4 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST4");

				var orgHeaderPK1 = orgHeader1.PK;
				var orgHeaderPK2 = orgHeader2.PK;
				var orgHeaderPK3 = orgHeader3.PK;
				var orgHeaderPK4 = orgHeader4.PK;

				//Setup all have same IVA#
				AddIVACustomsCode(orgHeader1, "111");
				AddIVACustomsCode(orgHeader2, "111");
				AddIVACustomsCode(orgHeader3, "111");
				AddIVACustomsCode(orgHeader4, "111");

				CreatePostedTransaction(orgHeader1, "INV01", company);
				CreatePostedTransaction(orgHeader2, "INV01", company);
				CreatePostedTransaction(orgHeader3, "INV01", company);
				CreatePostedTransaction(orgHeader4, "INV01", company);

				Factory.Save();

				var query = new ZQuery(OrgHeaderSchema.OH_Language, "YYY");
				query.AddToFilter(OrgHeaderSchema.OH_FullName, "ORG TEST");
				query.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, "PT");
				query.OrderBy = AutoOrgHeader.Schema.OH_Code;
				var reader = new FilteredBusinessObjectReader(query, typeof(OrgHeader)) { BatchSize = 4 };

				AssertEquals("Number of organizations to be processed", 4, reader.Count());

				var merger = new ManyToManyOrgMerger(reader);
				merger.Merge();

				CombineAssertions(() =>
				{
					AssertEquals("Organization 1 should not be deleted after merge", false, orgHeader1.IsDeleted);
					AssertEquals("Organization 2 should be deleted after merge", true, orgHeader2.IsDeleted);
					AssertEquals("Organization 3 should be deleted after merge", true, orgHeader3.IsDeleted);
					AssertEquals("Organization 4 should be deleted after merge", true, orgHeader4.IsDeleted);
				});

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				orgHeader1 = newFactory.Load<OrgHeader>(orgHeaderPK1);
				orgHeader2 = newFactory.Load<OrgHeader>(orgHeaderPK2);
				orgHeader3 = newFactory.Load<OrgHeader>(orgHeaderPK3);
				orgHeader4 = newFactory.Load<OrgHeader>(orgHeaderPK4);

				CombineAssertions(() =>
				{
					AssertNotNull("Organization 1 should exist in the database after merge", orgHeader1);
					AssertNull("Organization 2 should not exist in the database after merge", orgHeader2);
					AssertNull("Organization 3 should not exist in the database after merge", orgHeader3);
					AssertNull("Organization 4 should not exist in the database after merge", orgHeader4);
				});

				CombineAssertions(() =>
				{
					AssertEquals("Number of merges that are not allowed", 0, merger.DisallowedMergeMessages.Count);
					AssertEquals("Number of organizations processed", 4, merger.OrganisationsProcessed);
					AssertEquals("Number of organizations deleted", 3, merger.OrganisationsDeleted);
				});
			}
		}

		public void TestIsAllowedToMergeOrgs_ManyToMany_DisallowSomeMerges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Portugal;

				var orgHeader1 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST1");
				var orgHeader2 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST2");
				var orgHeader3 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST3");
				var orgHeader4 = CreatePTOrgHeader("ORG TEST", "MATCH ADDRESS", "PT", "ORGTEST4");

				var orgHeaderPK1 = orgHeader1.PK;
				var orgHeaderPK2 = orgHeader2.PK;
				var orgHeaderPK3 = orgHeader3.PK;
				var orgHeaderPK4 = orgHeader4.PK;

				//Setup org1 & 4 have same IVA# and org2 & 3 have different IVA#
				AddIVACustomsCode(orgHeader1, "111");
				AddIVACustomsCode(orgHeader2, "222");
				AddIVACustomsCode(orgHeader3, "333");
				AddIVACustomsCode(orgHeader4, "111");

				CreatePostedTransaction(orgHeader1, "INV01", company);
				CreatePostedTransaction(orgHeader2, "INV01", company);
				CreatePostedTransaction(orgHeader3, "INV01", company);
				CreatePostedTransaction(orgHeader4, "INV01", company);

				Factory.Save();

				var query = new ZQuery(OrgHeaderSchema.OH_Language, "YYY");
				query.AddToFilter(OrgHeaderSchema.OH_FullName, "ORG TEST");
				query.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, "PT");
				query.OrderBy = AutoOrgHeader.Schema.OH_Code;
				var reader = new FilteredBusinessObjectReader(query, typeof(OrgHeader)) { BatchSize = 4 };

				AssertEquals("Number of organizations to be processed", 4, reader.Count());

				var merger = new ManyToManyOrgMerger(reader);
				merger.Merge();

				CombineAssertions(() =>
				{
					AssertEquals("Organization 1 should not be deleted after merge", false, orgHeader1.IsDeleted);
					AssertEquals("Organization 2 should not be deleted after merge", false, orgHeader2.IsDeleted);
					AssertEquals("Organization 3 should not be deleted after merge", false, orgHeader3.IsDeleted);
					AssertEquals("Organization 4 should be deleted after merge", true, orgHeader4.IsDeleted);
				});

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

				orgHeader1 = newFactory.Load<OrgHeader>(orgHeaderPK1);
				orgHeader2 = newFactory.Load<OrgHeader>(orgHeaderPK2);
				orgHeader3 = newFactory.Load<OrgHeader>(orgHeaderPK3);
				orgHeader4 = newFactory.Load<OrgHeader>(orgHeaderPK4);

				CombineAssertions(() =>
				{
					AssertNotNull("Organization 1 should exist in the database after merge", orgHeader1);
					AssertNotNull("Organization 2 should exist in the database after merge", orgHeader2);
					AssertNotNull("Organization 3 should exist in the database after merge", orgHeader3);
					AssertNull("Organization 4 should not exist in the database after merge", orgHeader4);
				});

				CombineAssertions(() =>
				{
					AssertEquals("Number of merges that are not allowed", 2, merger.DisallowedMergeMessages.Count); //org 2 & 3 were not allowed to merge
					AssertEquals("Number of organizations processed", 2, merger.OrganisationsProcessed); //only org1 & 4 were merged
					AssertEquals("Number of organizations deleted", 1, merger.OrganisationsDeleted); //org4 deleted after merge
				});
			}
		}

		#region Implementation

		OrgHeader CreatePTOrgHeader(ZString orgName, ZString orgAddress, ZString unloco, ZString orgCode)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsConsignee = true;
			orgHeader.OH_Language = "YYY";
			orgHeader.OH_FullName = orgName;
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_RL_NKClosestPort = unloco;
			orgHeader.MainAddress.OA_Address1 = orgAddress;
			orgHeader.MainAddress.OA_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return orgHeader;
		}

		OrgCusCode AddIVACustomsCode(OrgHeader header, string customsRegNo)
		{
			var retainedOrgCusCode = header.CustomsCodes.AddNew();
			retainedOrgCusCode.OK_OH = header.PK;
			retainedOrgCusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			retainedOrgCusCode.OK_CodeType = OrgCusCode.CodeTypes.IVA;
			retainedOrgCusCode.OK_CustomsRegNo = customsRegNo;

			var newAddress = header.Addresses.AddNew();
			newAddress.OA_Address1 = "newAddress";
			retainedOrgCusCode.OK_OA_PremisesAddress = newAddress.PK;

			return retainedOrgCusCode;
		}

		void CreatePostedTransaction(OrgHeader header, string transactionNum, GlbCompany company)
		{
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_OH = header.PK;
			transactionHeader.AH_GC = company.PK;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_OH = header.PK;
			transactionLine.AL_GC = company.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
		}

		#endregion

		#endregion

		int BatchCompletedHit;

		void merger_BatchCompleted(object sender, ManyToManyBatchPrcessedEvent e)
		{
			BatchCompletedHit++;
		}

		bool CheckOrgArrayContainsPK(OrgHeader[] array, ZGuid pk)
		{
			foreach (OrgHeader org in array)
			{
				if (org.PK == pk)
				{
					return true;
				}
			}
			return false;
		}

		ZQuery GetQueryForPKs(List<ZGuid> pks)
		{
			return new ZQuery(OrgHeaderSchema.PK, pks);
		}

		ZGuid CreateOrgHeader(bool hasWrongLang, ZString orgName, ZString orgAddress, ZString unloco)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_IsConsignee = true;
			org.OH_Language = hasWrongLang ? "ZZZ" : "YYY";
			org.OH_FullName = orgName;
			org.OH_RL_NKClosestPort = unloco;
			org.MainAddress.OA_Code = GetRandomString(OrgAddressSchema.OA_Code.MaxLength);
			org.MainAddress.OA_Address1 = orgAddress;
			org.OH_Code = GetRandomString(OrgHeaderSchema.OH_Code.MaxLength);
			return org.PK;
		}

		ZString GetRandomString(int length)
		{
			return new ZString(ZGuid.NewZGuid().ToString()).Replace("-", "").Left(length);
		}
	}
}
