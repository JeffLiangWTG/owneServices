using CargoWise.RefDbRepo.ESReferenceData.Business;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

class C44AddInfoParserTests : CodeListParserTests<C44AddInfoItem, C44AddInfoItemMap>,
	ITestEmptyCodeErrorMessage,
	ITestEmptyDescriptionErrorMessage,
	ITestEmptyCharacterIndicationErrorMessage,
	ITestEmptySpecialIndicationErrorMessage,
	ITestEmptyStartDateErrorMessage,
	ITestInvalidStartDateErrorMessage,
	ITestEmptyEndDateErrorMessage,
	ITestInvalidEndDateErrorMessage
{
	protected override string TestClassName => nameof(C44AddInfoParserTests);
	protected override string CodeListID => "C44AddInfo";
	protected override string System => "C44AddInfo";
	protected override string CodeListName => "C44 AddInfo";
	protected override CodeListParser<C44AddInfoItem, C44AddInfoItemMap> ParserToRun => new C44AddInfoParser(DateProvider.Object);

	public string ExpectedEmptyCodeErrorMessage => @"Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:
Code: 
Description: Sol autoriz en la declaración de un régimen especial
Special Indication: AUTORIZACION SIMPLIFICADA
Character Indication: GENERAL
Start Date: 01/01/2005
End Date: 31/12/2099";

	public string ExpectedEmptyDescriptionErrorMessage => @"Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:
Code: 00200
Description: 
Special Indication: OTROS
Character Indication: GENERAL
Start Date: 01/01/2005
End Date: 31/12/2099";

	public string ExpectedEmptyCharacterIndicationErrorMessage => @"Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:
Code: 00501
Description: Identidad de declarante y destinatario
Special Indication: EXPORTADOR
Character Indication: 
Start Date: 01/01/2005
End Date: 31/12/2099";

	public string ExpectedEmptySpecialIndicationErrorMessage => @"Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:
Code: 00500
Description: Identidad de declarante y destinatario
Special Indication: 
Character Indication: GENERAL
Start Date: 01/01/2005
End Date: 31/12/2099";

	public string ExpectedEmptyStartDateErrorMessage => @"Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:
Code: 00300
Description: Identidad de declarante y expedidor
Special Indication: EXPEDIDOR
Character Indication: GENERAL
Start Date: 
End Date: 31/12/2099";

	public string ExpectedEmptyEndDateErrorMessage => @"Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:
Code: 00400
Description: Identidad de declarante y exportador
Special Indication: EXPORTADOR
Character Indication: GENERAL
Start Date: 01/01/2005
End Date: ";

	public string ExpectedInvalidStartDateErrorMessage => @"Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:
Code: 00601
Description: Identidad de declarante y destinatario
Special Indication: EXPORTADOR
Character Indication: GENERAL
Start Date: 01/01/005
End Date: 31/12/2099";

	public string ExpectedInvalidEndDateErrorMessage => @"Unable to import C44 AddInfo due to empty 'code', 'special indication' 'description', 'start date', 'end date'. Details:
Code: 00702
Description: Identidad de declarante y destinatario
Special Indication: EXPORTADOR
Character Indication: GENERAL
Start Date: 01/01/2005
End Date: 31/12/209";
}
