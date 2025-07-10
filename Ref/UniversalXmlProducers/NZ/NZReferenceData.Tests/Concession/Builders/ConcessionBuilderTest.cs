using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using ILogger = CargoWise.RefDbRepo.NZReferenceData.Business.ILogger;
using Logger = CargoWise.RefDbRepo.NZReferenceData.Business.Logger;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests.Concessions
{
	class ConcessionBuilderTest
	{
		[Test]
		public void TestGetNewRefCusTariff()
		{
			var tariff = ConcessionBuilder.GetNewRefCusTariff(ConcessionDetails, "description");
			Assert.That(tariff.ZZ1_TariffCode, Is.EqualTo("311299D"));
			Assert.That(tariff.ZZ1_StartDate, Is.EqualTo(new DateTime(2022, 4, 25, 0, 0, 0)));
			Assert.That(tariff.ZZ1_EndDate, Is.EqualTo(Constants.MaxSmallDateTime));
			Assert.That(tariff.ZZ1_Description, Is.EqualTo("description"));
		}

		[Test]
		public void TestCreateRate()
		{
			var rates = CreateTestConcessionRates([
				"311299D~NML~Dec 31 3000 12:00AM~~3~35.000000~~~~~",
				"311299D~NML~Dec 31 3000 12:00AM~~3~35.000000~~~~~",
				"311299D~MY~Dec 31 3000 12:00AM~~3~35.000000~~~~~",
				"311299D~MY~Apr 25 2021 12:00AM~~3~35.000000~~~~~"
			]);
			ConcessionBuilder.CreateRate(Tariff, rates, ConcessionDetails);
			Assert.That(Tariff.RefCusRates, Has.Length.EqualTo(3));
			Assert.That(Tariff.RefCusRates.All(r => r.RefCusApplicabilities.Length == 1), Is.True,
				"There should be a one-to-one relationship between RefCusRate and RefCusApplicability");
			AssertRate("NML", Tariff.RefCusRates[0]);
			AssertRate("NML", Tariff.RefCusRates[1]);
			AssertRate("MY", Tariff.RefCusRates[2]);

			void AssertRate(string rateGroup, RefCusRate rate)
			{
				Assert.AreEqual(rateGroup, rate.RefCusApplicabilities.Single().ZZT_ZZA_NKTradeGroup);
				Assert.AreEqual(rateGroup, rate.ZZ2_ZZS_NKPreference);
			}
		}

		[Test]
		public void TestCreateOrUpdateRelationship()
		{
			var concessionToTariffsTestCaseStringsForAllTariffLevelAreEmpty = new[]
			{
				"311299D~**~**~**~**~**~1",
				"311299D~**~**~**~**~**~2",
				"311299D~**~**~**~**~**~3",
				"311299D~**~**~**~**~**~4",
				"311299D~**~**~**~**~**~5",
				"311299D~**~**~**~**~**~6",
				"311299D~**~**~**~**~**~7",
				"311299D~**~**~**~**~**~8",
				"311299D~**~**~**~**~**~9",
				"311299D~**~**~**~**~**~10",
				"311299D~**~**~**~**~**~11",
				"311299D~**~**~**~**~**~12",
				"311299D~**~**~**~**~**~13",
				"311299D~**~**~**~**~**~14",
				"311299D~**~**~**~**~**~15",
				"311299D~**~**~**~**~**~16",
				"311299D~**~**~**~**~**~17",
				"311299D~**~**~**~**~**~18",
				"311299D~**~**~**~**~**~19",
				"311299D~**~**~**~**~**~20",
				"311299D~**~**~**~**~**~21",
			};

			var concessionToTariffsTestCaseStringsForHasTariffLevels = new[]
			{
				"311299D~**~**~**~**~**~13",
				"311299D~**~**~**~**~**~16",
				"311299D~85~16~79~19~**~16",
			};

			var loggerMock = new Mock<ILogger>();
			var builder = new ConcessionBuilder(DateProvider, logger: loggerMock.Object);
			var tariff = new RefCusTariff()
			{
				ZZ1_TariffCode = "311299D"
			};

			var concessionToTariffs = CreateTestConcessionToTariffs(concessionToTariffsTestCaseStringsForAllTariffLevelAreEmpty);
			builder.CreateOrUpdateRelationship(tariff, concessionToTariffs);
			Assert.That(tariff.RefCusTariffRelationships, Has.Length.EqualTo(1));
			Assert.IsEmpty(tariff.RefCusTariffRelationships[0].ZZH_TariffCode);

			concessionToTariffs = concessionToTariffs.Skip(1).ToArray();
			builder.CreateOrUpdateRelationship(tariff, concessionToTariffs);
			Assert.That(tariff.RefCusTariffRelationships, Has.Length.EqualTo(92));

			concessionToTariffs = CreateTestConcessionToTariffs(concessionToTariffsTestCaseStringsForHasTariffLevels);
			builder.CreateOrUpdateRelationship(tariff, concessionToTariffs);
			var expected = new List<string>(Helper.SectionToChapters[13]);
			expected.AddRange(Helper.SectionToChapters[16]);
			expected.Add("85167919");
			CollectionAssert.AreEquivalent(expected, tariff.RefCusTariffRelationships.Select(r => r.ZZH_TariffCode));

			var testCaseStrings = new List<string>(concessionToTariffsTestCaseStringsForAllTariffLevelAreEmpty);
			testCaseStrings.Add("311299D~85~16~79~19~**~16");
			concessionToTariffs = CreateTestConcessionToTariffs(testCaseStrings);
			builder.CreateOrUpdateRelationship(tariff, concessionToTariffs);
			Assert.That(tariff.RefCusTariffRelationships, Has.Length.EqualTo(0));
			loggerMock.Verify(x => x.LogError("311299D has 21 ConcessionToTariffs which Tariff both are empty, but it also has another ConcessionToTariff which Tariff Level is not empty"), Times.Once);
		}

		[Test]
		public void TestBuild()
		{
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				ConsolidatedListOfApprovalsJsonLinesTestCaseStringsForSimpleTest
			);

			var dataRepo = new TariffDataRepo() as ITopLevelDataRepo<RefCusTariff>;
			var concessionBuilder = new ConcessionBuilder(DateProvider, new Logger(), fileReader);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			CollectionAssert.AreEquivalent(new[] { "100001C", "100019F", "311299D" }, tariffs.Select(t => t.ZZ1_TariffCode));
			var tariff = tariffs.First();
			Assert.That(tariff.ZZ1_Description, Is.EqualTo("100001C description"));
			var rates = tariff.RefCusRates;
			CollectionAssert.AreEquivalent(new[] { "2079-06-06 23:59:00: 0.35*VFD" }, rates.Select(r => $"{r.ZZ2_EndDate:yyyy-MM-dd HH:mm:ss}: {r.ZZ2_RateFormula}"));
			var relationships = tariff.RefCusTariffRelationships;
			CollectionAssert.AreEquivalent(new[] { "01", "02", "03", "04", "05" }, relationships.Select(r => r.ZZH_TariffCode));
		}

		[Test]
		public void TestBuild_NotFindDescription()
		{
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				new[] {
					"{\"concessionCode\" : \"100019F\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
					"{\"concessionCode\" : \"311299D\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"description\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
				}
			);

			var loggerMock = new Mock<ILogger>();
			var dataRepo = new TariffDataRepo() as ITopLevelDataRepo<RefCusTariff>;

			var concessionBuilder = new ConcessionBuilder(DateProvider,loggerMock.Object, fileReader);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			CollectionAssert.AreEquivalent(new[] { "311299D" }, tariffs.Select(t => t.ZZ1_TariffCode));
			loggerMock.Verify(x => x.LogError("100001C: Cannot find description"), Times.Once);
			loggerMock.Verify(x => x.LogError("100019F: Cannot find description"), Times.Once);
		}

		[Test]
		public void TestBuild_NotFindRate()
		{
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				new[]
				{
					"100019F~NML~Dec 31 2000 11:59PM~~1~~~~~~",
					"311299D~NML~Dec 31 3000 12:00AM~~1~~~~~~",
				},
				ConsolidatedListOfApprovalsJsonLinesTestCaseStringsForSimpleTest
			);

			var loggerMock = new Mock<ILogger>();
			var dataRepo = new TariffDataRepo() as ITopLevelDataRepo<RefCusTariff>;

			var concessionBuilder = new ConcessionBuilder(DateProvider, loggerMock.Object, fileReader);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			CollectionAssert.AreEquivalent(new[] { "100001C", "100019F", "311299D" }, tariffs.Select(t => t.ZZ1_TariffCode));
			loggerMock.Verify(x => x.LogError("100001C: Cannot find an active rate"), Times.Once);
			loggerMock.Verify(x => x.LogError("100019F: Cannot find an active rate"), Times.Once);
		}

		[Test]
		public void TestBuild_NotFindToTariff()
		{
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest.Skip(1).ToArray(),
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				ConsolidatedListOfApprovalsJsonLinesTestCaseStringsForSimpleTest
			);

			var loggerMock = new Mock<ILogger>();
			var dataRepo = new TariffDataRepo() as ITopLevelDataRepo<RefCusTariff>;

			var concessionBuilder = new ConcessionBuilder(DateProvider, loggerMock.Object, fileReader);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			CollectionAssert.AreEquivalent(new[] { "100001C", "100019F", "311299D" }, tariffs.Select(t => t.ZZ1_TariffCode));
			loggerMock.Verify(x => x.LogError("100001C: Cannot find relationship"), Times.Once);
		}

		[Test]
		public void TestBuild_TruncatesLongDescriptions()
		{
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				ConsolidatedListOfApprovalsJsonLinesWithLongDescriptions
			);

			var loggerMock = new Mock<ILogger>();
			var emailServiceMock = new Mock<IEmailService>();
			ITopLevelDataRepo<RefCusTariff> dataRepo = new TariffDataRepo();

			var concessionBuilder = new ConcessionBuilder(DateProvider, loggerMock.Object, fileReader, emailService: emailServiceMock.Object);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			var truncatedDescription = new string('a', 4000);
			CollectionAssert.AreEquivalent((string[])[truncatedDescription, truncatedDescription, "311299D description"], tariffs.Select(t => t.ZZ1_Description));

			emailServiceMock.Verify(x => x.SendEmail("NZ RefCusTariffConcession Truncated Descriptions", "The following concession codes have truncated descriptions:\n100001C\n100019F", false), Times.Once);

			loggerMock.Verify(x => x.LogError("100001C: Description truncated (exceeds 4000 chars)"), Times.Once);
			loggerMock.Verify(x => x.LogError("100019F: Description truncated (exceeds 4000 chars)"), Times.Once);
			loggerMock.Verify(x => x.LogError("311299D: Description truncated (exceeds 4000 chars)"), Times.Never);
		}

		[Test]
		public void TestBuild_Description_RemovesHtmlTags()
		{
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				[
					"{\"concessionCode\" : \"100001C\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"<div>100001C</div> description\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
					"{\"concessionCode\" : \"100019F\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"100019F <span>description</span>\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}"
				]
			);

			var loggerMock = new Mock<ILogger>();
			var emailServiceMock = new Mock<IEmailService>();
			ITopLevelDataRepo<RefCusTariff> dataRepo = new TariffDataRepo();

			var concessionBuilder = new ConcessionBuilder(DateProvider, loggerMock.Object, fileReader, emailService: emailServiceMock.Object);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			CollectionAssert.AreEquivalent((string[])["100001C description", "100019F description"], tariffs.Select(t => t.ZZ1_Description));
		}

		[Test]
		public void TestBuild_Description_DecodesStandardHtmlEntitiesExcludingIllegalXMLChars()
		{
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				[
					"{\"concessionCode\" : \"100001C\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"100001C &deg; &plusmn; &gt; &lt; &amp; &quot; &apos; &cent; &pound; &yen; &euro; &copy; &reg; &trade; &sect; &para; &middot; &bull; &hellip; &prime; &Prime; &oline; &frasl; &larr; &uarr; &rarr; &darr; &harr; &crarr; &lceil; &rceil; &lfloor; &rfloor; &loz; &spades; &clubs; &hearts; &diams; &forall; &part; &exist; &empty; &nabla; &isin; &notin; &ni; &prod; &sum; &minus; &lowast; &radic; &prop; &infin; &ang; &and; &or; &cap; &cup; &int; &there4; &sim; &cong; &asymp; &ne; &equiv; &le; &ge; &sub; &sup; &nsub; &sube; &supe; &oplus; &otimes; &perp; &sdot; &Alpha; &Beta; &Gamma; &Delta; &Epsilon; &Zeta; &Eta; &Theta; &Iota; &Kappa; &Lambda; &Mu; &Nu; &Xi; &Omicron; &Pi; &Rho; &Sigma; &Tau; &Upsilon; &Phi; &Chi; &Psi; &Omega; &alpha; &beta; &gamma; &delta; &epsilon; &zeta; &eta; &theta; &iota; &kappa; &lambda; &mu; &nu; &xi; &omicron; &pi; &rho; &sigmaf; &sigma; &tau; &upsilon; &phi; &chi; &psi; &omega; &thetasym; &upsih; &piv; &OElig; &oelig; &Scaron; &scaron; &Yuml; &fnof; &circ; &tilde; &thinsp; &thinsp; &thinsp; &zwnj; &zwj; &lrm; &rlm; &ndash; &mdash; &lsquo; &rsquo; &sbquo; &ldquo; &rdquo; &bdquo; &dagger; &Dagger; &bull; &hellip; &permil; &lsaquo; &rsaquo; &euro; &image; &weierp; &real; &trade; &alefsym; &larr; &uarr; &rarr; &darr; &harr; &crarr; &lceil; &rceil; &lfloor; &rfloor; &loz; &spades; &clubs; &hearts; &diams; description\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}"
				]
			);

			var loggerMock = new Mock<ILogger>();
			var emailServiceMock = new Mock<IEmailService>();
			ITopLevelDataRepo<RefCusTariff> dataRepo = new TariffDataRepo();

			var concessionBuilder = new ConcessionBuilder(DateProvider, loggerMock.Object, fileReader, emailService: emailServiceMock.Object);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			CollectionAssert.AreEquivalent((string[])
			[
				"100001C ° ± &gt; &lt; &amp; \" ' ¢ £ ¥ € © ® ™ § ¶ · • … ′ ″ ‾ ⁄ ← ↑ → ↓ ↔ ↵ ⌈ ⌉ ⌊ ⌋ ◊ ♠ ♣ ♥ ♦ ∀ ∂ ∃ ∅ ∇ ∈ ∉ ∋ ∏ ∑ − ∗ √ ∝ ∞ ∠ ∧ ∨ ∩ ∪ ∫ ∴ ∼ ≅ ≈ ≠ ≡ ≤ ≥ ⊂ ⊃ ⊄ ⊆ ⊇ ⊕ ⊗ ⊥ ⋅ Α Β Γ Δ Ε Ζ Η Θ Ι Κ Λ Μ Ν Ξ Ο Π Ρ Σ Τ Υ Φ Χ Ψ Ω α β γ δ ε ζ η θ ι κ λ μ ν ξ ο π ρ ς σ τ υ φ χ ψ ω ϑ ϒ ϖ Œ œ Š š Ÿ ƒ ˆ ˜       ‌ ‍ ‎ ‏ – — ‘ ’ ‚ “ ” „ † ‡ • … ‰ ‹ › € ℑ ℘ ℜ ™ ℵ ← ↑ → ↓ ↔ ↵ ⌈ ⌉ ⌊ ⌋ ◊ ♠ ♣ ♥ ♦ description"
			], tariffs.Select(t => t.ZZ1_Description));
		}


		[Test]
		public void TestBuild_Description_RemovesNonStandardHtmlEntities()
		{
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				[
					"{\"concessionCode\" : \"100001C\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"&amp;OpenCurlyDoubleQuote;100001C description&CloseCurlyDoubleQuote;\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}"
				]
			);

			var loggerMock = new Mock<ILogger>();
			var emailServiceMock = new Mock<IEmailService>();
			ITopLevelDataRepo<RefCusTariff> dataRepo = new TariffDataRepo();

			var concessionBuilder = new ConcessionBuilder(DateProvider, loggerMock.Object, fileReader, emailService: emailServiceMock.Object);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			CollectionAssert.AreEquivalent((string[]) ["100001C description"], tariffs.Select(t => t.ZZ1_Description));
		}

		[Test]
		public void TestBuild_Description_KeepsIllegalBodyXMLCharsEscaped()
		{
			string[] illegalBodyXmlCharsArray = ["&gt;", "&lt;", "&amp;"];
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				[
					"{\"concessionCode\" : \"100001C\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"100001C " + string.Join(" ", illegalBodyXmlCharsArray) + "\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}"
				]

			);

			var loggerMock = new Mock<ILogger>();
			var emailServiceMock = new Mock<IEmailService>();
			ITopLevelDataRepo<RefCusTariff> dataRepo = new TariffDataRepo();

			var concessionBuilder = new ConcessionBuilder(DateProvider, loggerMock.Object, fileReader, emailService: emailServiceMock.Object);
			concessionBuilder.Build(dataRepo, TestConstants.ConcessionFilePaths, ProcessingData);

			var tariffs = dataRepo.Get();
			CollectionAssert.AreEquivalent((string[]) ["100001C &gt; &lt; &amp;"], tariffs.Select(t => t.ZZ1_Description));
		}

		[Test]
		public void TestBuild_UsesDescriptionOverride_WhenProvided()
		{
			var concessionOverrideFileName = Path.GetTempFileName();
			var portalDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var concessionOverridePath = Path.Combine(portalDirPath, concessionOverrideFileName);
			var concessionOverrideBuildFilePath = new BuildersFilePath(concessionOverridePath, BuilderFilePathSymbol.ConcessionOverride);
			const string concessionOverrideJson = "[{ \"Code\": \"100001C\", \"Description\": \"Overridden description\" }]";
			File.WriteAllText(concessionOverridePath, concessionOverrideJson);
			
			var fileReader = ConcessionTestHelper.GetConcessionFileReaderForTest(
				ConcessionToTariffsTestCaseStringsForSimpleTest,
				ConcessionDetailsTestCaseStringsForSimpleTest,
				ConcessionRatesTestCaseStringsForSimpleTest,
				ConsolidatedListOfApprovalsJsonLinesTestCaseStringsForSimpleTest
			);

			var dataRepo = new TariffDataRepo() as ITopLevelDataRepo<RefCusTariff>;
			var concessionBuilder = new ConcessionBuilder(DateProvider, new Logger(), fileReader);

			var filePaths = TestConstants.ConcessionFilePaths.Append(concessionOverrideBuildFilePath).ToArray();

			concessionBuilder.Build(dataRepo, filePaths, ProcessingData);

			var tariff = dataRepo.Load("100001C");
			Assert.That(tariff, Is.Not.Null);
			Assert.That(tariff.ZZ1_Description, Is.EqualTo("Overridden description"));
		}

		static string[] ConcessionToTariffsTestCaseStringsForSimpleTest { get; } = new[]
		{
			"100001C~**~**~**~**~**~1",
			"100019F~13~02~32~**~**~2",
			"311299D~04~02~10~00~**~1",
		};

		static string[] ConcessionDetailsTestCaseStringsForSimpleTest { get; } = new[]
		{
			"100001C~Apr 25 2022 12:00AM~Dec 31 3000 12:00AM",
			"100019F~Dec 31 1979 12:00AM~Dec 31 3000 12:00AM",
			"311299D~Apr  1 2021 12:00AM~Dec 31 3000 12:00AM",
		};

		static string[] ConcessionRatesTestCaseStringsForSimpleTest { get; } = new[]
		{
			"100001C~NML~Dec 31 3000 12:00AM~~3~35.000000~~~~~",
			"100019F~NML~Dec 31 3000 11:59PM~~1~~~~~~",
			"311299D~NML~Dec 31 3000 12:00AM~~1~~~~~~",
		};

		static string[] ConsolidatedListOfApprovalsJsonLinesTestCaseStringsForSimpleTest { get; } = new[]
		{
			"{\"concessionCode\" : \"100001C\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"100001C description\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
			"{\"concessionCode\" : \"100019F\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"100019F description\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
			"{\"concessionCode\" : \"311299D\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"311299D description\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
		};

		static string[] ConsolidatedListOfApprovalsJsonLinesWithLongDescriptions { get; } =
		[
			"{\"concessionCode\" : \"100001C\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"" + new string('a', 5000) + "\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
			"{\"concessionCode\" : \"100019F\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"" + new string('a', 5000) + "\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
			"{\"concessionCode\" : \"311299D\",\"tariffItem\" : \"0402.10.00)\",\"description\" : \"311299D description\",\"normalTariff\" : \"Free\",\"preferentialTariff\" : \"Free\",\"part2Ref\" : \"99\",\"effectiveFrom\" : \"04/21\", \"effectiveTo\" : \"..\",\"scheduleNo\" : \"1\",\"docTitle\" : \"CH03-38 | tariff-concession-approval-notice-10-2021\",\"docUrl\" : \"\"}",
		];


		ConcessionToTariff[] CreateTestConcessionToTariffs(IEnumerable<string> testCaseStrings)
		{
			var results = new List<ConcessionToTariff>();
			foreach (var testCaseString in testCaseStrings)
			{
				results.Add(new ConcessionToTariff(testCaseString));
			}
			return results.ToArray();
		}

		ConcessionRates[] CreateTestConcessionRates(IEnumerable<string> testCaseStrings)
		{
			var results = new List<ConcessionRates>();
			foreach (var testCaseString in testCaseStrings)
			{
				results.Add(new ConcessionRates(testCaseString));
			}
			return results.ToArray();
		}

		IDateProvider DateProvider { get; } = Mock.Of<IDateProvider>(x => x.Today == new DateTime(2025, 1, 1) && x.ActiveDate == x.Today.AddYears(-5));

		const string ConcessionDetailsTestCaseString = "311299D~Apr 25 2022 12:00AM~Dec 31 3000 12:00AM";

		ConcessionDetails ConcessionDetails { get; } = new ConcessionDetails(ConcessionDetailsTestCaseString);

		RefCusTariff Tariff { get; } = ConcessionBuilder.GetNewRefCusTariff(new ConcessionDetails(ConcessionDetailsTestCaseString), "description");

		NZConcessionProcessingData ProcessingData { get; } = new NZConcessionProcessingData();
	}
}
