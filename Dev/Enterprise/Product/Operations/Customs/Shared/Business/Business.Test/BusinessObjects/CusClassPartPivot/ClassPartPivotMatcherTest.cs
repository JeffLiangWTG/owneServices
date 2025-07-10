using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	class ClassPartPivotMatcherTest : TestCaseWithFactory
	{
		public void TestGetMatchesSkipTypeFallback()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();

			var ownerOrg = Factory.New<OrgHeader>();
			var supplierOrg = Factory.New<OrgHeader>();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			var supplierRelation = part.RelatedOrganisations.AddNew();
			supplierRelation.OU_Relationship = "SUP";
			supplierRelation.OU_OH = supplierOrg.PK;

			var htiPivot1 = part.PivotsForBinding.AddNew();
			htiPivot1.CI_ChildType = classificationTypeProvider.HTICode;
			var htiPivot2 = part.PivotsForBinding.AddNew();
			htiPivot2.CI_ChildType = classificationTypeProvider.HTICode;

			var htePivot1 = part.PivotsForBinding.AddNew();
			htePivot1.CI_ChildType = classificationTypeProvider.HTECode;
			var htePivot2 = part.PivotsForBinding.AddNew();
			htePivot2.CI_ChildType = classificationTypeProvider.HTECode;

			var schBPivot1 = part.PivotsForBinding.AddNew();
			schBPivot1.CI_ChildType = classificationTypeProvider.SHBCode;
			var schBPivot2 = part.PivotsForBinding.AddNew();
			schBPivot2.CI_ChildType = classificationTypeProvider.SHBCode;

			var matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.SHBCode);
			AssertContainsExactElementsInAnyOrder(new[] { htiPivot1, htiPivot2, schBPivot1, schBPivot2 }, matcher.GetMatchListSkipTypeFallback(classificationTypeProvider.HTICode, ZGuid.Empty, ZGuid.Empty, false));
			AssertContainsExactElementsInAnyOrder(new[] { htePivot1, htePivot2, schBPivot1, schBPivot2 }, matcher.GetMatchListSkipTypeFallback(classificationTypeProvider.HTECode, ZGuid.Empty, ZGuid.Empty, false));
		}

		public void TestCanMatchEvenInvalidPivots()
		{
			var classification = Factory.New<BaseCusClassification>();
			classification.CC_LookupCode = "1111.1111.1111";

			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();

			var htePivot1 = part.PivotsForBinding.AddNew();
			htePivot1.CI_ChildType = classificationTypeProvider.HTECode;
			htePivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			htePivot1.CI_TariffNum = "000014";

			var htiPivot1 = part.PivotsForBinding.AddNew();
			htiPivot1.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			htiPivot1.CI_CC = classification.PK;

			var matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTBCode);
			AssertEquals("Valid pivot with Tariff No.:", htePivot1, matcher.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, ZGuid.Empty, ZDate.Today, false));
			AssertEquals("Valid pivot with CC:", htiPivot1, matcher.GetMatch(classificationTypeProvider.HTICode, ZGuid.Empty, ZGuid.Empty, ZDate.Today, false));

			var htiPivot2 = part.PivotsForBinding.AddNew();
			htiPivot2.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTBCode);
			AssertEquals("Invalid pivot matched with no CC and no Tariff No. (Could be valid in EU):", htiPivot2, matcher.GetMatch(classificationTypeProvider.HTICode, ZGuid.Empty, ZGuid.Empty, ZDate.Today, false));
		}

		public void TestGetMatch()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			var ownerOrg = Factory.New<OrgHeader>();
			var supplierOrg = Factory.New<OrgHeader>();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			var ownerRelation1 = part.RelatedOrganisations.AddNew();
			ownerRelation1.OU_Relationship = "SUP";
			ownerRelation1.OU_OH = ownerOrg.PK;

			var supplierRelation1 = part.RelatedOrganisations.AddNew();
			supplierRelation1.OU_Relationship = "OWN";
			supplierRelation1.OU_OH = supplierOrg.PK;

			var supplierRelation2 = part.RelatedOrganisations.AddNew();
			supplierRelation2.OU_Relationship = "SUP";
			supplierRelation2.OU_OH = supplierOrg.PK;

			var htiPivot = part.PivotsForBinding.AddNew();
			htiPivot.CI_ChildType = classificationTypeProvider.HTICode;
			htiPivot.CI_TariffNum = "000013";

			var htePivot1 = part.PivotsForBinding.AddNew();
			htePivot1.CI_ChildType = classificationTypeProvider.HTECode;
			htePivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			htePivot1.CI_TariffNum = "000014";

			var htePivot2 = part.PivotsForBinding.AddNew();
			htePivot2.CI_ChildType = classificationTypeProvider.HTECode;
			htePivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			htePivot2.CI_TariffNum = "000015";

			var schBPivot1 = part.PivotsForBinding.AddNew();
			schBPivot1.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot1.CI_DateStart = ZDateTime.Now.AddDays(-20);
			schBPivot1.CI_TariffNum = "000024";

			var schBPivot2 = part.PivotsForBinding.AddNew();
			schBPivot2.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot2.CI_DateStart = ZDateTime.Now.AddDays(-10);
			schBPivot2.CI_TariffNum = "000025";
			var matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTBCode);
			AssertEquals(schBPivot1, matcher.GetMatch(classificationTypeProvider.SHBCode, ZGuid.Empty, ZGuid.Empty, ZDate.Empty, false));
			AssertEquals(schBPivot1, matcher.GetMatch(classificationTypeProvider.SHBCode, ownerOrg.PK, ZGuid.Empty, ZDate.Empty, false));
			AssertEquals(schBPivot1, matcher.GetMatch(classificationTypeProvider.SHBCode, ZGuid.Empty, supplierOrg.PK, ZDate.Empty, false));
			AssertEquals(schBPivot1, matcher.GetMatch(classificationTypeProvider.SHBCode, ownerOrg.PK, supplierOrg.PK, ZDate.Empty, false));

			AssertEquals(htePivot1, matcher.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, ZGuid.Empty, ZDate.Empty, false));
			AssertEquals(htePivot1, matcher.GetMatch(classificationTypeProvider.HTECode, ownerOrg.PK, ZGuid.Empty, ZDate.Empty, false));
			AssertEquals(htePivot1, matcher.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, supplierOrg.PK, ZDate.Empty, false));
			AssertEquals(htePivot1, matcher.GetMatch(classificationTypeProvider.HTECode, ownerOrg.PK, supplierOrg.PK, ZDate.Empty, false));

			var schBPivot3 = part.PivotsForBinding.AddNew();
			schBPivot3.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot3.CI_DateStart = ZDateTime.Now.AddDays(-30);
			schBPivot3.CI_TariffNum = "000026";
			schBPivot3.CI_OH = supplierRelation1.OU_OH;

			var schBPivot4 = part.PivotsForBinding.AddNew();
			schBPivot4.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot4.CI_DateStart = ZDateTime.Now.AddDays(-20);
			schBPivot4.CI_TariffNum = "000027";
			schBPivot4.CI_OH = supplierRelation2.OU_OH;

			var schBPivot5 = part.PivotsForBinding.AddNew();
			schBPivot5.CI_ChildType = classificationTypeProvider.SHBCode;
			schBPivot5.CI_DateStart = ZDateTime.Now.AddDays(-5);
			schBPivot5.CI_TariffNum = "000028";
			schBPivot5.CI_OH = ownerRelation.OU_OH;
			schBPivot2.CI_OH = ownerRelation1.OU_OH;

			matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTBCode);
			AssertEquals(schBPivot1, matcher.GetMatch(classificationTypeProvider.SHBCode, ZGuid.Empty, ZGuid.Empty, ZDate.Today, true));
			AssertEquals(schBPivot5, matcher.GetMatch(classificationTypeProvider.SHBCode, ownerOrg.PK, ZGuid.Empty, ZDate.Today, true));
			AssertEquals(schBPivot4, matcher.GetMatch(classificationTypeProvider.SHBCode, ZGuid.Empty, supplierOrg.PK, ZDate.Today, true));
			AssertEquals(schBPivot5, matcher.GetMatch(classificationTypeProvider.SHBCode, ownerOrg.PK, supplierOrg.PK, ZDate.Today, true));

			AssertEquals(htePivot2, matcher.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, ZGuid.Empty, ZDate.Today, true));
			AssertEquals(htePivot2, matcher.GetMatch(classificationTypeProvider.HTECode, ownerOrg.PK, ZGuid.Empty, ZDate.Today, true));
			AssertEquals(htePivot2, matcher.GetMatch(classificationTypeProvider.HTECode, ZGuid.Empty, supplierOrg.PK, ZDate.Today, true));
			AssertEquals(htePivot2, matcher.GetMatch(classificationTypeProvider.HTECode, ownerOrg.PK, supplierOrg.PK, ZDate.Today, true));
		}

		public void TestGetMatch_Attribute1()
		{
			AssertGetMatchWithAttributes(y => y.Attributes1.AddNew(), CusAttributeFilter.AttributeFilterName.AT1);
		}

		public void TestGetMatch_Attribute2()
		{
			AssertGetMatchWithAttributes(y => y.Attributes2.AddNew(), CusAttributeFilter.AttributeFilterName.AT2);
		}

		public void TestGetMatch_Attribute3()
		{
			AssertGetMatchWithAttributes(y => y.Attributes3.AddNew(), CusAttributeFilter.AttributeFilterName.AT3);
		}

		public void TestGetMatches()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();

			var ownerOrg = Factory.New<OrgHeader>();
			var supplierOrg = Factory.New<OrgHeader>();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			var supplierRelation = part.RelatedOrganisations.AddNew();
			supplierRelation.OU_Relationship = "SUP";
			supplierRelation.OU_OH = supplierOrg.PK;

			var htiPivot1 = part.PivotsForBinding.AddNew();
			htiPivot1.CI_ChildType = classificationTypeProvider.HTICode;
			var htiPivot2 = part.PivotsForBinding.AddNew();
			htiPivot2.CI_ChildType = classificationTypeProvider.HTICode;
			var expectedHtiPivot = new BaseCusClassPartPivot[] { htiPivot1, htiPivot2 };

			var htePivot1 = part.PivotsForBinding.AddNew();
			htePivot1.CI_ChildType = classificationTypeProvider.HTECode;
			var htePivot2 = part.PivotsForBinding.AddNew();
			htePivot2.CI_ChildType = classificationTypeProvider.HTECode;
			var expectedHtePivot = new BaseCusClassPartPivot[] { htePivot1, htePivot2 };

			var schBPivot1 = part.PivotsForBinding.AddNew();
			schBPivot1.CI_ChildType = classificationTypeProvider.SHBCode;
			var schBPivot2 = part.PivotsForBinding.AddNew();
			schBPivot2.CI_ChildType = classificationTypeProvider.SHBCode;
			var expectedSchBPivot = new BaseCusClassPartPivot[] { schBPivot1, schBPivot2 };
			var matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTBCode);
			AssertGetMatches(matcher, classificationTypeProvider.HTICode, ownerOrg, supplierOrg, expectedHtiPivot);
			AssertGetMatches(matcher, classificationTypeProvider.HTECode, ownerOrg, supplierOrg, expectedHtePivot);
			AssertGetMatches(matcher, classificationTypeProvider.SHBCode, ownerOrg, supplierOrg, expectedSchBPivot);

			var schbOwnerPivot1 = part.PivotsForBinding.AddNew();
			schbOwnerPivot1.CI_ChildType = classificationTypeProvider.SHBCode;
			schbOwnerPivot1.CI_OH = ownerRelation.OU_OH;
			var schbOwnerPivot2 = part.PivotsForBinding.AddNew();
			schbOwnerPivot2.CI_ChildType = classificationTypeProvider.SHBCode;
			schbOwnerPivot2.CI_OH = ownerRelation.OU_OH;
			var expectedSchbOwnerPivot = new BaseCusClassPartPivot[] { schbOwnerPivot1, schbOwnerPivot2 };

			var schbSuppliertPivot1 = part.PivotsForBinding.AddNew();
			schbSuppliertPivot1.CI_ChildType = classificationTypeProvider.SHBCode;
			schbSuppliertPivot1.CI_OH = supplierRelation.OU_OH;
			var schbSuppliertPivot2 = part.PivotsForBinding.AddNew();
			schbSuppliertPivot2.CI_ChildType = classificationTypeProvider.SHBCode;
			schbSuppliertPivot2.CI_OH = supplierRelation.OU_OH;
			var expectedSchbSuppliertPivot = new BaseCusClassPartPivot[] { schbSuppliertPivot1, schbSuppliertPivot2 };

			var hteOwnerPivot1 = part.PivotsForBinding.AddNew();
			hteOwnerPivot1.CI_ChildType = classificationTypeProvider.HTECode;
			hteOwnerPivot1.CI_OH = ownerRelation.OU_OH;
			var hteOwnerPivot2 = part.PivotsForBinding.AddNew();
			hteOwnerPivot2.CI_ChildType = classificationTypeProvider.HTECode;
			hteOwnerPivot2.CI_OH = ownerRelation.OU_OH;
			var expectedHteOwnerPivot = new BaseCusClassPartPivot[] { hteOwnerPivot1, hteOwnerPivot2 };

			var hteSuppliertPivot1 = part.PivotsForBinding.AddNew();
			hteSuppliertPivot1.CI_ChildType = classificationTypeProvider.HTECode;
			hteSuppliertPivot1.CI_OH = supplierRelation.OU_OH;
			var hteSuppliertPivot2 = part.PivotsForBinding.AddNew();
			hteSuppliertPivot2.CI_ChildType = classificationTypeProvider.HTECode;
			hteSuppliertPivot2.CI_OH = supplierRelation.OU_OH;
			var expectedHteSuppliertPivot = new BaseCusClassPartPivot[] { hteSuppliertPivot1, hteSuppliertPivot2 };

			var htiOwnerPivot1 = part.PivotsForBinding.AddNew();
			htiOwnerPivot1.CI_ChildType = classificationTypeProvider.HTICode;
			htiOwnerPivot1.CI_OH = ownerRelation.OU_OH;
			var htiOwnerPivot2 = part.PivotsForBinding.AddNew();
			htiOwnerPivot2.CI_ChildType = classificationTypeProvider.HTICode;
			htiOwnerPivot2.CI_OH = ownerRelation.OU_OH;
			var expectedHtiOwnerPivot = new BaseCusClassPartPivot[] { htiOwnerPivot1, htiOwnerPivot2 };

			var htiSuppliertPivot1 = part.PivotsForBinding.AddNew();
			htiSuppliertPivot1.CI_ChildType = classificationTypeProvider.HTICode;
			htiSuppliertPivot1.CI_OH = supplierRelation.OU_OH;
			var htiSuppliertPivot2 = part.PivotsForBinding.AddNew();
			htiSuppliertPivot2.CI_ChildType = classificationTypeProvider.HTICode;
			htiSuppliertPivot2.CI_OH = supplierRelation.OU_OH;
			var expectedHtiSuppliertPivot = new BaseCusClassPartPivot[] { htiSuppliertPivot1, htiSuppliertPivot2 };
			var expectedOwnerPivot = new BaseCusClassPartPivot[] { schbOwnerPivot1, schbOwnerPivot2, hteOwnerPivot1, hteOwnerPivot2, htiOwnerPivot1, htiOwnerPivot2 };
			var expectedSupplierPivot = new BaseCusClassPartPivot[] { schbSuppliertPivot1, schbSuppliertPivot2, hteSuppliertPivot1, hteSuppliertPivot2, htiSuppliertPivot1, htiSuppliertPivot2 };

			matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTBCode);
			AssertContainsExactElementsInAnyOrder(expectedSchBPivot, matcher.GetMatches(classificationTypeProvider.SHBCode, ZGuid.Empty, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(expectedSchbOwnerPivot, matcher.GetMatches(classificationTypeProvider.SHBCode, ownerOrg.PK, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(expectedSchbSuppliertPivot, matcher.GetMatches(classificationTypeProvider.SHBCode, ZGuid.Empty, supplierOrg.PK));
			AssertContainsExactElementsInAnyOrder(expectedSchbOwnerPivot, matcher.GetMatches(classificationTypeProvider.SHBCode, ownerOrg.PK, supplierOrg.PK));

			AssertContainsExactElementsInAnyOrder(expectedHtePivot, matcher.GetMatches(classificationTypeProvider.HTECode, ZGuid.Empty, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(expectedHteOwnerPivot, matcher.GetMatches(classificationTypeProvider.HTECode, ownerOrg.PK, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(expectedHteSuppliertPivot, matcher.GetMatches(classificationTypeProvider.HTECode, ZGuid.Empty, supplierOrg.PK));
			AssertContainsExactElementsInAnyOrder(expectedHteOwnerPivot, matcher.GetMatches(classificationTypeProvider.HTECode, ownerOrg.PK, supplierOrg.PK));

			AssertContainsExactElementsInAnyOrder(expectedHtiPivot, matcher.GetMatches(classificationTypeProvider.HTICode, ZGuid.Empty, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(expectedHtiOwnerPivot, matcher.GetMatches(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(expectedHtiSuppliertPivot, matcher.GetMatches(classificationTypeProvider.HTICode, ZGuid.Empty, supplierOrg.PK));
			AssertContainsExactElementsInAnyOrder(expectedHtiOwnerPivot, matcher.GetMatches(classificationTypeProvider.HTICode, ownerOrg.PK, supplierOrg.PK));
		}

		public void TestGetMatches_OrgFallBack()
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();

			var ownerMatch = Factory.New<OrgHeader>();
			var ownerNoMatch = Factory.New<OrgHeader>();
			var supplierMatch = Factory.New<OrgHeader>();
			var supplierNoMatch = Factory.New<OrgHeader>();

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = classificationTypeProvider.HTICode;
			pivot1.CI_OH = ownerMatch.PK;

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = classificationTypeProvider.HTICode;
			pivot2.CI_OH = supplierMatch.PK;

			var pivot3 = part.PivotsForBinding.AddNew();
			pivot3.CI_ChildType = classificationTypeProvider.HTICode;
			pivot3.CI_OH = ZGuid.Empty;

			var matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTICode);
			AssertContainsExactElementsInAnyOrder(new[] { pivot1 }, matcher.GetMatches(classificationTypeProvider.HTICode, ownerMatch.PK, supplierMatch.PK));
			AssertContainsExactElementsInAnyOrder(new[] { pivot1 }, matcher.GetMatchListSkipTypeFallback(classificationTypeProvider.HTICode, ownerMatch.PK, supplierMatch.PK, false));

			AssertContainsExactElementsInAnyOrder(new[] { pivot2 }, matcher.GetMatches(classificationTypeProvider.HTICode, ownerNoMatch.PK, supplierMatch.PK));
			AssertContainsExactElementsInAnyOrder(new[] { pivot2 }, matcher.GetMatchListSkipTypeFallback(classificationTypeProvider.HTICode, ownerNoMatch.PK, supplierMatch.PK, false));

			AssertContainsExactElementsInAnyOrder(new[] { pivot3 }, matcher.GetMatches(classificationTypeProvider.HTICode, ownerNoMatch.PK, supplierNoMatch.PK));
			AssertContainsExactElementsInAnyOrder(new[] { pivot3 }, matcher.GetMatchListSkipTypeFallback(classificationTypeProvider.HTICode, ownerNoMatch.PK, supplierNoMatch.PK, false));
		}

		void AssertGetMatches(ClassPartPivotMatcher matcher, string typeOfPivot, OrgHeader ownerOrg, OrgHeader supplierOrg, BaseCusClassPartPivot[] expectedPivots)
		{
			AssertContainsExactElementsInAnyOrder(expectedPivots, matcher.GetMatches(typeOfPivot, ZGuid.Empty, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(expectedPivots, matcher.GetMatches(typeOfPivot, ownerOrg.PK, ZGuid.Empty));
			AssertContainsExactElementsInAnyOrder(expectedPivots, matcher.GetMatches(typeOfPivot, ZGuid.Empty, supplierOrg.PK));
			AssertContainsExactElementsInAnyOrder(expectedPivots, matcher.GetMatches(typeOfPivot, ownerOrg.PK, supplierOrg.PK));
		}

		public void TestGetMatches_Attribute1()
		{
			AssertGetMatches_WithAttributes(y => y.Attributes1.AddNew(), CusAttributeFilter.AttributeFilterName.AT1);
		}

		public void TestGetMatches_Attribute2()
		{
			AssertGetMatches_WithAttributes(y => y.Attributes2.AddNew(), CusAttributeFilter.AttributeFilterName.AT2);
		}

		public void TestGetMatches_Attribute3()
		{
			AssertGetMatches_WithAttributes(y => y.Attributes3.AddNew(), CusAttributeFilter.AttributeFilterName.AT3);
		}

		protected override void SetUp()
		{
			base.SetUp();
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			classificationTypeProvider = ClassificationTypeProvider.GetProviderFor(Core.Constants.CountryCodes.UnitedStates);
		}
		IClassificationTypeProvider classificationTypeProvider;

		protected override void TearDown()
		{
			if (countrySetter != null)
			{
				countrySetter.Dispose();
				countrySetter = null;
			}
			base.TearDown();
		}
		IDisposable countrySetter;

		void AssertGetMatchWithAttributes(Func<BaseCusClassPartPivot, CusAttributeFilter> addAttribute, CusAttributeFilter.AttributeFilterName attributeFilterName)
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			var ownerOrg = Factory.New<OrgHeader>();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			ownerRelation.OU_OH = ownerOrg.PK;

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = classificationTypeProvider.HTICode;
			pivot.CI_TariffNum = "000013";
			pivot.CI_OH = ownerOrg.PK;

			var attrib1 = addAttribute(pivot);
			attrib1.BG_AttributeValue1 = "AttVal1";

			var attrib2 = addAttribute(pivot);
			attrib2.BG_AttributeValue1 = "AttVal2";

			var matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTBCode);
			CombineAssertions(() =>
			{
				AssertEquals("Pivot matches attribute filter 1", pivot, matcher.GetMatch(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty, ZDate.Empty, false,
					new[] { new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal1" } }));
				AssertEquals("Pivot matches both attribute filters", pivot, matcher.GetMatch(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty, ZDate.Empty, false,
					new[]
					{
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal1" },
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal2" }
					}));
				AssertEquals("Pivot does not match attribute filter 3", null, matcher.GetMatch(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty, ZDate.Empty, false,
					new[]
					{
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal1" },
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal2" },
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal3" }
					}));
			});
		}

		void AssertGetMatches_WithAttributes(Func<BaseCusClassPartPivot, CusAttributeFilter> addAttribute, CusAttributeFilter.AttributeFilterName attributeFilterName)
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();
			var ownerOrg = Factory.New<OrgHeader>();

			var ownerRelation = part.RelatedOrganisations.AddNew();
			ownerRelation.OU_Relationship = "OWN";
			ownerRelation.OU_OH = ownerOrg.PK;

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = classificationTypeProvider.HTICode;
			pivot1.CI_TariffNum = "000013";
			pivot1.CI_OH = ownerOrg.PK;

			var attrib1 = addAttribute(pivot1);
			attrib1.BG_AttributeValue1 = "AttVal1";
			var attrib2 = addAttribute(pivot1);
			attrib2.BG_AttributeValue1 = "AttVal2";

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = classificationTypeProvider.HTICode;
			pivot2.CI_TariffNum = "000013";
			pivot2.CI_OH = ownerOrg.PK;

			var attrib3 = addAttribute(pivot2);
			attrib3.BG_AttributeValue1 = "AttVal1";
			var attrib4 = addAttribute(pivot2);
			attrib4.BG_AttributeValue1 = "AttVal2";
			var attrib5 = addAttribute(pivot2);
			attrib5.BG_AttributeValue1 = "AttVal3";

			var allPivots = new[] { pivot1, pivot2 };
			var matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTBCode);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("All pivots have attribute 1", allPivots, matcher.GetMatches(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty,
					new[] { new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal1" } }));
				AssertContainsExactElementsInAnyOrder("All pivots have attribute 1 and 2", allPivots, matcher.GetMatches(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty,
					new[]
					{
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal1" },
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal2" }
					}));
				AssertContainsExactElementsInAnyOrder("Only one pivot has attribute 3", new[] { pivot2 }, matcher.GetMatches(classificationTypeProvider.HTICode, ownerOrg.PK, ZGuid.Empty,
					new[]
					{
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal1" },
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal2" },
						new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "AttVal3" }
					}));
			});
		}

		public void TestGetMatches_Attributes1Fallback()
		{
			var ownerMatch = Factory.New<OrgHeader>();
			var ownerNoMatch = Factory.New<OrgHeader>();
			var supplierMatch = Factory.New<OrgHeader>();
			var supplierNoMatch = Factory.New<OrgHeader>();
			AssertGetMatches_AttributesFallback(ownerMatch.PK, ownerMatch.PK, supplierMatch.PK, y => y.Attributes1.AddNew(), CusAttributeFilter.AttributeFilterName.AT1);
			AssertGetMatches_AttributesFallback(supplierMatch.PK, ownerNoMatch.PK, supplierMatch.PK, y => y.Attributes1.AddNew(), CusAttributeFilter.AttributeFilterName.AT1);
			AssertGetMatches_AttributesFallback(ZGuid.Empty, ownerNoMatch.PK, supplierNoMatch.PK, y => y.Attributes1.AddNew(), CusAttributeFilter.AttributeFilterName.AT1);
		}

		public void TestGetMatches_Attributes2Fallback()
		{
			var ownerMatch = Factory.New<OrgHeader>();
			var ownerNoMatch = Factory.New<OrgHeader>();
			var supplierMatch = Factory.New<OrgHeader>();
			var supplierNoMatch = Factory.New<OrgHeader>();
			AssertGetMatches_AttributesFallback(ownerMatch.PK, ownerMatch.PK, supplierMatch.PK, y => y.Attributes2.AddNew(), CusAttributeFilter.AttributeFilterName.AT2);
			AssertGetMatches_AttributesFallback(supplierMatch.PK, ownerNoMatch.PK, supplierMatch.PK, y => y.Attributes2.AddNew(), CusAttributeFilter.AttributeFilterName.AT2);
			AssertGetMatches_AttributesFallback(ZGuid.Empty, ownerNoMatch.PK, supplierNoMatch.PK, y => y.Attributes2.AddNew(), CusAttributeFilter.AttributeFilterName.AT2);
		}

		public void TestGetMatches_Attributes3Fallback()
		{
			var ownerMatch = Factory.New<OrgHeader>();
			var ownerNoMatch = Factory.New<OrgHeader>();
			var supplierMatch = Factory.New<OrgHeader>();
			var supplierNoMatch = Factory.New<OrgHeader>();
			AssertGetMatches_AttributesFallback(ownerMatch.PK, ownerMatch.PK, supplierMatch.PK, y => y.Attributes3.AddNew(), CusAttributeFilter.AttributeFilterName.AT3);
			AssertGetMatches_AttributesFallback(supplierMatch.PK, ownerNoMatch.PK, supplierMatch.PK, y => y.Attributes3.AddNew(), CusAttributeFilter.AttributeFilterName.AT3);
			AssertGetMatches_AttributesFallback(ZGuid.Empty, ownerNoMatch.PK, supplierNoMatch.PK, y => y.Attributes3.AddNew(), CusAttributeFilter.AttributeFilterName.AT3);
		}

		void AssertGetMatches_AttributesFallback(ZGuid relatedOrgPK, ZGuid ownerPK, ZGuid supplierPK, Func<BaseCusClassPartPivot, CusAttributeFilter> addAttribute, CusAttributeFilter.AttributeFilterName attributeFilterName)
		{
			var part = (OrgSupplierPart)Factory.New<Integration.Customs.US.IOrgSupplierPart>();

			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = classificationTypeProvider.HTICode;
			pivot1.CI_TariffNum = "000013";
			pivot1.CI_OH = relatedOrgPK;
			var attrib1 = addAttribute(pivot1);
			attrib1.BG_AttributeValue1 = "A";

			var pivot2 = part.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = classificationTypeProvider.HTICode;
			pivot2.CI_TariffNum = "000014";
			pivot2.CI_OH = relatedOrgPK;

			var matcher = new ClassPartPivotMatcher(part.PivotsForBinding.ToArray<BaseCusClassPartPivot>(), classificationTypeProvider.HTICode);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("No attribute used in matching, should match on pivot2 with no attribute",
					new[] { pivot2 }, matcher.GetMatches(classificationTypeProvider.HTICode, ownerPK, supplierPK, []));

				AssertContainsExactElementsInAnyOrder("No attribute used in matching, should match on pivot2 with no attribute",
					new[] { pivot2 }, matcher.GetMatchListSkipTypeFallback(classificationTypeProvider.HTICode, ownerPK, supplierPK, false, []));

				AssertContainsExactElementsInAnyOrder("Atrribute A used in matching, should match on pivot1 with attribute 1",
					new[] { pivot1 }, matcher.GetMatches(classificationTypeProvider.HTICode, ownerPK, supplierPK, [new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "A" }]));

				AssertContainsExactElementsInAnyOrder("Atrribute A used in matching, should match on pivot1 with attribute 1",
					new[] { pivot1 }, matcher.GetMatchListSkipTypeFallback(classificationTypeProvider.HTICode, ownerPK, supplierPK, false, [new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "A" }]));

				AssertContainsExactElementsInAnyOrder("Atrribute B used in matching, no match, should fallback to pivot2 with no attribute",
					new[] { pivot2 }, matcher.GetMatches(classificationTypeProvider.HTICode, ownerPK, supplierPK,[new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "B" }]));

				AssertContainsExactElementsInAnyOrder("Atrribute B used in matching, no match, should fallback to pivot2 with no attribute",
					new[] { pivot2 }, matcher.GetMatchListSkipTypeFallback(classificationTypeProvider.HTICode, ownerPK, supplierPK, false, [new CusAttributeFilter.AttributeValue() { Name = attributeFilterName, Value = "B" }]));
			});
		}
	}
}
