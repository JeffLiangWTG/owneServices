using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AES;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AIS;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	[TestFixture]
	class DownloadCodeListsTests
	{
		// Add tests for the xlsx file type methods in DownloadCodeLists class.
		[Test]
		public void NullArg()
		{
			Assert.Throws<ArgumentException>(() => new DownloadCodeLists(null));
		}

		[Test]
		public void EmptyArrayArg()
		{
			var codeLists = new IRevenueCodeListDetails[] { };
			Assert.Throws<ArgumentException>(() => new DownloadCodeLists(codeLists));
		}

		[Test]
		public void EmptyUrl()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (errors, _) = downloader.Download(ApplicationType.AIS, "", forceLoadFromFilePath: true);

			Assert.That(errors, Does.Contain("Invalid URI: The URI is empty."));
		}

		[Test]
		public void InvalidUrl()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (errors, _) = downloader.Download(ApplicationType.AIS, "InvalidURL", forceLoadFromFilePath: true);

			Assert.That(errors, Does.Contain("Unable to Download the IE CodeLists PDF from the following URL: InvalidURL"));
		}

		[Test]
		public void ValidVersionNumber()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.That(extractedCodeLists[(dataGrouping, "FAC")].VersionNumber, Is.EqualTo("3.00"), "Version number");
		}

		[Test]
		public void MissingVersionNumber()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (errors, _) = downloader.Download(ApplicationType.AIS, Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\ais-codelists_2021June-MissingVersionNumber.pdf"), forceLoadFromFilePath: true);

			Assert.That(errors, Does.Contain("Can not find the current version number on Page 2."));
		}

		[Test]
		public void ValidVersionDate()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.That(extractedCodeLists[(dataGrouping, "FAC")].VersionDate, Is.EqualTo(new DateTime(2025, 3, 6)), "Version date");
		}

		[Test]
		public void MissingVersionDate()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (errors, _) = downloader.Download(ApplicationType.AIS, Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\ais-codelists_2021June-MissingVersionDate.pdf"), forceLoadFromFilePath: true);

			Assert.That(errors, Does.Contain("Can not find the current version date for version"));
		}

		[Test]
		public void InvalidVersionDate()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (errors, _) = downloader.Download(ApplicationType.AIS, Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\ais-codelists_2021June-InvalidVersionDate.pdf"), forceLoadFromFilePath: true);

			Assert.That(errors, Does.Contain("Can not parse the current version date for version"));
		}

		[Test]
		public void UpdateTypeDeletion()
		{
			var codeLists = new IRevenueCodeListDetails[] { new AuthorisationCodeTypesDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.That(extractedCodeLists[(dataGrouping, "AUTH")].UpdateType, Is.EqualTo(UpdateType.Deletion));
		}

		[Test]
		public void MissingTableContent()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (errors, _) = downloader.Download(ApplicationType.AIS, Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\ais-codelists_2021June-MissingTableContent.pdf"), forceLoadFromFilePath: true);

			Assert.That(errors, Does.Contain("PDF structure has changed cannot location the TABLE OF CONTENTS starting on Page 2."));
		}

		[Test]
		public void MissingCodeListsPage()
		{
			var codeLists = new IRevenueCodeListDetails[] { new LocationOfGoodsDetails() };
			var downloader = new DownloadCodeLists(codeLists);
			var (errors, _) = downloader.Download(ApplicationType.AIS, Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\ais-codelists_2021June-MissingCodeListsPage.pdf"), forceLoadFromFilePath: true);

			Assert.That(errors, Does.Contain("PDF structure has changed unable to locate the heading '2. CODE LISTS' or '3. CODE LISTS' starting after Page"));
		}

		[Test]
		public void CodeListName_Empty()
		{
			var codeLists = new IRevenueCodeListDetails[] { new CodeListEmpty() };
			var downloader = new DownloadCodeLists(codeLists);
			var (errors, _) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);

			Assert.That(errors, Does.Contain("The code list name should not be empty."));
		}

		[Test]
		public void CodeListName_Invalid()
		{
			var codeLists = new IRevenueCodeListDetails[] { new CodeListXYZ() };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);

			Assert.Multiple(() =>
			{
				Assert.That(extractedCodeLists, Is.Not.Null, "Dictionary not null");
				Assert.That(extractedCodeLists, Is.Empty, "Dictionary count");
			});
		}

		[Test]
		public void CodeListName_Valid()
		{
			var locationOfGoodsDetails = new LocationOfGoodsDetails();
			var codeLists = new IRevenueCodeListDetails[] { locationOfGoodsDetails };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.Multiple(() =>
			{
				Assert.That(extractedCodeLists.Count, Is.EqualTo(1), "Dictionary count");
				var extractedCodeList = extractedCodeLists[(dataGrouping, locationOfGoodsDetails.Code)];
				Assert.That(extractedCodeList.CodeList.Count(), Is.GreaterThan(0), "Contains Codes");
			});

			var calculationOfTaxes = new CalculationOfTaxes();
			codeLists = new IRevenueCodeListDetails[] { calculationOfTaxes };
			downloader = new DownloadCodeLists(codeLists);
			(_, extractedCodeLists) = downloader.Download(ApplicationType.AES, codeListPathAES, forceLoadFromFilePath: true);
			dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.Multiple(() =>
			{
				Assert.That(extractedCodeLists.Count, Is.EqualTo(1), "Dictionary count");
				var extractedCodeList = extractedCodeLists[(dataGrouping, calculationOfTaxes.Code)];
				Assert.That(extractedCodeList.CodeList.Count(), Is.GreaterThan(0), "Contains Codes");
			});
		}

		[Test]
		public void MultipleCodeLists()
		{
			var codeListDocType = new CodeListDocType();
			var codeListCL001 = new CodeListCL001A();
			var codeLists = new IRevenueCodeListDetails[] { codeListDocType, codeListCL001 };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.Multiple(() =>
			{
				Assert.That(extractedCodeLists, Has.Count.EqualTo(2), "Dictionary contains both code lists");
				Assert.That(extractedCodeLists, Does.ContainKey((dataGrouping, codeListDocType.Code)), "Dictionary contains key DocType");
				Assert.That(extractedCodeLists, Does.ContainKey((dataGrouping, codeListCL001.Code)), "Dictionary contains key CL001");
			});
		}

		[Test]
		public void CombinedCodeLists()
		{
			var codeListCL001A = new CodeListCL001A();
			var codeListCL001B = new CodeListCL001B();
			var codeLists = new IRevenueCodeListDetails[] { codeListCL001A, codeListCL001B };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.Multiple(() =>
			{
				var extractedCodeList = extractedCodeLists[(dataGrouping, codeListCL001A.Code)];
				Assert.That(extractedCodeLists, Has.Count.EqualTo(1), "Dictionary contains a single code list");
				Assert.That(extractedCodeLists, Does.ContainKey((dataGrouping, codeListCL001A.Code)), "Dictionary contains key CL001");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "I1").Description, Is.EqualTo("Import Simplified declaration"), "Includes codes from list A");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "G4G3").Description, Is.EqualTo("Temporary Storage Declaration and Presentation Notification"), "Includes codes from list B");
			});
		}

		[Test]
		public void MultiplePages_MultipleDescriptionLines()
		{
			var codeListDocType = new CodeListDocType();
			var codeLists = new IRevenueCodeListDetails[] { codeListDocType };
			DownloadCodeLists downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AIS, codeListPathAIS, forceLoadFromFilePath: true);

			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;
			Assert.Multiple(() =>
			{
				var extractedCodeList = extractedCodeLists[(dataGrouping, codeListDocType.Code)];
				Assert.That(extractedCodeList.CodeList.Count(), Is.EqualTo(311), "Correct Number of Codes");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "C071").Description, Is.EqualTo("Form for granting or denying written consent, pursuant to article 4(1) of Regulation (EU) 2017/852 of the European Parliament and of the Council on mercury, to the import of mercury or of the mixtures of mercury listed in Annex I to that Regulation"), "Multi Line description is correct");
			});

			var codeListCL102 = new CodeListCL102();
			codeLists = new IRevenueCodeListDetails[] { codeListCL102 };
			downloader = new DownloadCodeLists(codeLists);
			(_, extractedCodeLists) = downloader.Download(ApplicationType.AES, codeListPathAES, forceLoadFromFilePath: true);
			dataGrouping = extractedCodeLists.Values.First().DataGrouping;
			Assert.Multiple(() =>
			{
				var extractedCodeList = extractedCodeLists[(dataGrouping, codeListCL102.Code)];
				Assert.That(extractedCodeList.CodeList.Count(), Is.EqualTo(22), "Correct Number of Codes");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "B52").Description, Is.EqualTo("Imported for inward process. exported for replacement under guarantee"), "Multi Line description is correct");
			});
		}

		[Test]
		public void UnpublishedCodeList()
		{
			var unpublishedCodeListType = new CodeListCL296Unpublished();
			var codeLists = new IRevenueCodeListDetails[] { unpublishedCodeListType };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AES, codeListPathAES, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.Multiple(() =>
			{
				var extractedCodeList = extractedCodeLists[(dataGrouping, unpublishedCodeListType.Code)];
				Assert.That(extractedCodeList.CodeList.Count(), Is.EqualTo(1), "Correct Number of Codes");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "A20").Description, Is.EqualTo("Express consignments in the context of exit summary declarations"), "Code is correct");
			});
		}

		[Test]
		public void DuplicateLineCodeList()
		{
			var codeListPathAES_Duplicated = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\aes-codelists_2022September_WithDuplicate.pdf");
			var duplicatedCodeListType = new CodeListCL008WithDuplicate();
			var codeLists = new IRevenueCodeListDetails[] { duplicatedCodeListType };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AES, codeListPathAES_Duplicated, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.Multiple(() =>
			{
				var extractedCodeList = extractedCodeLists[(dataGrouping, duplicatedCodeListType.Code)];
				Assert.That(extractedCodeList.CodeList.Count(), Is.EqualTo(249), "Correct Number of Codes");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "BN").Description, Is.EqualTo("Brunei"), "Random Code is correct");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "AD").Description, Is.EqualTo("Andorra"), "First duplicate Code is correct");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "AF").Description, Is.EqualTo("Afghanistan"), "Other duplicate Code is correct");
				Assert.That(extractedCodeList.CodeList.Count(x => x.Code == "AF"), Is.EqualTo(1), "Duplicate Code appears once");
			});
		}

		[Test]
		public void CodelistCL019()
		{
			var codelist = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\ncts-codelists_2024October.pdf");
			var codelistCL019Type = new CodeListCL019_NCTS();
			var codeLists = new IRevenueCodeListDetails[] { codelistCL019Type };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.NCTS, codelist, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;
			var extractedCodeList = extractedCodeLists[(dataGrouping, codelistCL019Type.Code)];

			Assert.Multiple(() =>
			{
				Assert.That(extractedCodeList.CodeList.Count(), Is.EqualTo(6), "Correct Number of Codes");
				Assert.That(extractedCodeList.CodeList.Single(x => x.Code == "2").Description, Is.EqualTo("Seals are broken or tampered with in the course of a transport operation for reasons beyond the carrier's control."), "Random Code is correct");
			});
		}

		[Test]
		public void ValidXlsxVersionNumber()
		{
			var additionalDeclarationTypesDetails = new CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.AdditionalDeclarationTypesDetails();
			var codeLists = new IRevenueCodeListDetails[] { additionalDeclarationTypesDetails };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AISUCC5, codeListPathAISUCC5, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.That(extractedCodeLists[(dataGrouping, "ENSUB")].VersionNumber, Is.EqualTo("1.18"), "Version number");
		}

		[Test]
		public void ValidXlsxVersionDate()
		{
			var additionalDeclarationTypesDetails = new CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.AISUCC5.AdditionalDeclarationTypesDetails();
			var codeLists = new IRevenueCodeListDetails[] { additionalDeclarationTypesDetails };
			var downloader = new DownloadCodeLists(codeLists);
			var (_, extractedCodeLists) = downloader.Download(ApplicationType.AISUCC5, codeListPathAISUCC5, forceLoadFromFilePath: true);
			var dataGrouping = extractedCodeLists.Values.First().DataGrouping;

			Assert.That(extractedCodeLists[(dataGrouping, "ENSUB")].VersionDate, Is.EqualTo(new DateTime(2024, 12, 11)), "Version date");
		}

		[OneTimeSetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			codeListPathAIS = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\ais-cci-codelists_2025Mar.pdf");
			codeListPathAES = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\aes-codelists_2022September.pdf");
			codeListPathAISUCC5 = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Input\ais-codelists_UCC5_2024December.xlsx");
		}
		Assembly assembly;
		string codeListPathAIS;
		string codeListPathAES;
		string codeListPathAISUCC5;

		class CodeListEmpty : IRevenueCodeListDetails
		{
			public ApplicationType ApplicationType => ApplicationType.AIS;
			public string Code => "";
			public string NameInFile => "";
			public string TableTitleInFile => "Code Name Description ";
			public string CodeFormattingRegularExpression => @"([A-Z]{2})";
			public bool AllowCombination => false;
			public bool IsPublished => true;
			public UpdateType UpdateType => UpdateType.Full;
		}

		class CodeListXYZ : IRevenueCodeListDetails
		{
			public ApplicationType ApplicationType => ApplicationType.AIS;
			public string Code => "XYZ";
			public string NameInFile => "XYZ";
			public string TableTitleInFile => "Code Name Description ";
			public string CodeFormattingRegularExpression => @"([A-Z]{2})";
			public bool AllowCombination => false;
			public bool IsPublished => true;
			public UpdateType UpdateType => UpdateType.Full;
		}

		class CodeListDocType : IRevenueCodeListDetails, IDoNotRegexEscapeTableTitleInPdf
		{
			public ApplicationType ApplicationType => ApplicationType.AIS;
			public string Code => "DocType";
			public string NameInFile => "CL213 - Supporting Document Type";
			public string CodeFormattingRegularExpression => @"([0-9][A-Z][0-9]{2})|([0-9][A-Z]{3})|([A-Z][0-9]{3})";
			public string TableTitleInFile => "Code Name / description";
			public bool AllowCombination => false;
			public bool IsPublished => true;
			public UpdateType UpdateType => UpdateType.Full;
		}

		class CodeListCL001A : IRevenueCodeListDetails
		{
			public ApplicationType ApplicationType => ApplicationType.AIS;
			public string Code => "CL001";
			public string NameInFile => "RL001 - Import Declaration Dataset Codes";
			public string CodeFormattingRegularExpression => @"([A-Z][0-9])";
			public string TableTitleInFile => "Code Name / description ";
			public bool AllowCombination => true;
			public bool IsPublished => true;
			public UpdateType UpdateType => UpdateType.Full;
		}

		class CodeListCL001B : IRevenueCodeListDetails
		{
			public ApplicationType ApplicationType => ApplicationType.AIS;
			public string Code => "CL001";
			public string NameInFile => "RL002 - Temporary Storage Dataset Codes";
			public string CodeFormattingRegularExpression => @"([A-Z0-9]{4})";
			public string TableTitleInFile => "Code Name / description ";
			public bool AllowCombination => true;
			public bool IsPublished => true;
			public UpdateType UpdateType => UpdateType.Full;
		}

		class CodeListCL102 : IRevenueCodeListDetails
		{
			public ApplicationType ApplicationType => ApplicationType.AES;
			public string Code => "CL102";
			public string NameInFile => "CL102 – CL Additional Procedure";
			public string CodeFormattingRegularExpression => @"([A-Z][0-9]{2})";
			public string TableTitleInFile => "Code Name / description ";
			public bool AllowCombination => false;
			public bool IsPublished => true;
			public UpdateType UpdateType => UpdateType.Full;
		}

		class CodeListCL296Unpublished : IRevenueCodeListDetails
		{
			public ApplicationType ApplicationType => ApplicationType.AES;
			public string Code => "CL296";
			public string NameInFile => "CL296"; // We don't know full name
			public string CodeFormattingRegularExpression => @"([A-Z][0-9]{2})";
			public string TableTitleInFile => "Code Name / description ";
			public bool AllowCombination => false;
			public bool IsPublished => false;
			public UpdateType UpdateType => UpdateType.Full;
		}

		class CodeListCL008WithDuplicate : IRevenueCodeListDetails
		{
			public ApplicationType ApplicationType => ApplicationType.AES;
			public string Code => "CL008";
			public string NameInFile => "CL008 – CL Country";
			public string CodeFormattingRegularExpression => @"([A-Z]{2})";
			public string TableTitleInFile => "Code Name / description ";
			public bool AllowCombination => false;
			public bool IsPublished => true;
			public UpdateType UpdateType => UpdateType.Full;
		}

		class CodeListCL019_NCTS : IRevenueCodeListDetails
		{
			public ApplicationType ApplicationType => ApplicationType.NCTS;
			public string Code => "CL019";
			public string NameInFile => "CL019 – CL Incident Code";
			public string CodeFormattingRegularExpression => @"([1-9]{1})";
			public string TableTitleInFile => "Code Name / description ";
			public bool AllowCombination => false;
			public bool IsPublished => true;
			public UpdateType UpdateType => UpdateType.Full;
		}
	}
}
