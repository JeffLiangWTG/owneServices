using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export.Testing
{
	sealed class ExportAdditionalReferencesAR44ETransformerTest : ManyToOneCodeListsXMLTests<RefCusCodeList, IKeyValuesWithAttributes>,
		ITestManyEmptyCodeErrorMessages,
		ITestManyEmptyDescriptionErrorMessages,
		ITestManyInvalidDateErrorMessages
	{
		protected override string System => "Export";

		protected override string[] CustomsCodeListIdentifiers => new[]
		{
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0912_ADDITIONAL_REFERENCES,
			CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0911_ADDITIONAL_REFERENCES
		};

		protected override string CodeType => nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AR44E_ADDITIONAL_REFERENCES);

		protected override Func<string[], ManyToOneCodeListsParserXML<RefCusCodeList, IKeyValuesWithAttributes>> ParserToRun => downLoadLinks => new ExportAdditionalReferencesAR44ETransformer(downLoadLinks);

		protected override (string url, string system, string customsCodeListIdentifier)[] DownloadUrls => new[]
		{
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[0], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0912_ADDITIONAL_REFERENCES)),
			(CodeListsTestHelper.ExportDownloadURL(CustomsCodeListIdentifiers[1], TestConstants.AESVersion3_0), "Export", nameof(CodeListsConstants.Export.CustomsCodeListIdentifiers.EXPORT_I0911_ADDITIONAL_REFERENCES))
		};

		string[] ITestManyEmptyCodeErrorMessages.ExpectedEmptyCodeErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Dem Zolllagerverfahren ging die Aktive Veredelung voraus
StartDate: 2009-03-01T00:00:00
EndDate: 2017-10-07T23:59:59",
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 
Description: Güter und Technologien, die nicht von Teil I Abschnitt A der Ausfuhrliste erfasst sind (nicht anwendbar bei Ausfuhren in Waffenembargoländer)
StartDate: 2013-09-01T00:00:00
EndDate: 2017-10-07T23:59:59"
		};

		string[] ITestManyEmptyDescriptionErrorMessages.ExpectedEmptyDescriptionErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9DEP
Description: 
StartDate: 2017-10-08T00:00:00
EndDate: 2021-03-06T23:59:59",
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 3LNA
Description: 
StartDate: 2017-10-08T00:00:00
EndDate: 2021-03-06T23:59:59"
		};

		string[] ITestManyInvalidDateErrorMessages.ExpectedInvalidDateErrorMessages => new[]
		{
			$@"Unable to import record from {CustomsCodeListIdentifiers[1]} due to empty Code, Description or invalid Date.
DETAILS:
Code: 9ZZY
Description: Sonstige Unterlagen ZELOS (Kopie)
StartDate: 2021-13-07T00:00:00
EndDate: ",
			$@"Unable to import record from {CustomsCodeListIdentifiers[0]} due to empty Code, Description or invalid Date.
DETAILS:
Code: Y971
Description: Güter und Technologien, die keinen Einschränkungen nach Artikel 3 Abs. 1 Buchstabe a) i.V.m. Anhang II Teil VIII der Nordkorea-VO (EU) 2017/1509 unterliegen
StartDate: 2021-13-07T00:00:00
EndDate: "
		};
	}
}
