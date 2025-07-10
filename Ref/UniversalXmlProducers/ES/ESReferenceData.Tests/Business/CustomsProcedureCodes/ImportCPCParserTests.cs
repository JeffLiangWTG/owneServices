using System;
using System.IO;
using CargoWise.RefDbRepo.ESReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

class ImportCPCParserTests : CPCParserTests<ImportCPCItem, ImportCPCItemMap, ImportCPCConcessionItem, ImportCPCConcessionItemMap>
{
	protected override string TestClassName => nameof(ImportCPCParserTests);

	[Test]
	public void DutyEmptyInCPCFile()
	{
		AssertCorrectMessage(Path.Combine(InputPath, $"{System}_CPC_INVALID_DATA.csv"), ValidConcessionCPCFile, ExpectedEmptyCPCDutyMessage);
	}

	[Test]
	public void DutyEmptyInConcessionCPCFile()
	{
		AssertCorrectMessage(ValidCPCFile, Path.Combine(InputPath, $"{System}_CPC_CONCESSION_INVALID_DATA.csv"), ExpectedEmptyConcessionCPCDutyMessage);
	}

	[Test]
	public void VATInvalidInCPCFile()
	{
		AssertCorrectMessage(Path.Combine(InputPath, $"{System}_CPC_INVALID_DATA.csv"), ValidConcessionCPCFile, ExpectedInvalidCPCVATMessage);
	}

	[Test]
	public void VATInvalidInConcessionCPCFile()
	{
		AssertCorrectMessage(ValidCPCFile, Path.Combine(InputPath, $"{System}_CPC_CONCESSION_INVALID_DATA.csv"), ExpectedInvalidConcessionCPCVATMessage);
	}

	protected override Func<IDateTimeProvider, CPCParser<ImportCPCItemMap, ImportCPCConcessionItemMap>> ParserToRun => (dateTimeProvider) => new ImportCPCParser(dateTimeProvider);

	protected override string System => "IMPORT";

	protected override string ExpectedEmptyCPCCodeMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 
Description: Desp.Libre Práctica Merc.enCEE Directiva
Start Date: 16-01-2009
End Date: 31-12-2099
Duty: NN NO PAGA / NO GARANTIZA
VAT: NN NO PAGA / NO GARANTIZA";

	protected override string ExpectedEmptyCPCDescriptionMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 0151
Description: 
Start Date: 01-05-2016
End Date: 31-12-2099
Duty: SS SÍ PAGA / SÍ GARANTIZA
VAT: SS SÍ PAGA / SÍ GARANTIZA";

	protected override string ExpectedEmptyConcessionCPCDescriptionMessage => @"Unable to import Concession CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 327
Description: 
Start Date: 28-05-2007
End Date: 31-12-2099
Duty: ** ASUMIDO DE CAS. 37.1 / ASUMIDO DE CAS. 37.1
VAT: NN NO PAGA / NO GARANTIZA";

	protected override string ExpectedEmptyCPCStartDateMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 0153
Description: DespachoLibre Práctica para Reexpedición
Start Date: 
End Date: 31-12-2099
Duty: SS SÍ PAGA / SÍ GARANTIZA
VAT: NS NO PAGA / SÍ GARANTIZA";

	protected override string ExpectedEmptyConcessionCPCStartDateMessage => @"Unable to import Concession CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 9AI
Description: Pdte. Conce. Franq.Arancelaria+Exenc.IVA
Start Date: 
End Date: 31-12-2099
Duty: NS NO PAGA / SÍ GARANTIZA
VAT: *S ASUMIDO DE CAS. 37.1 / SÍ GARANTIZA";

	protected override string ExpectedInvalidCPCEndDateMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 0171
Description: Libre Práctica Reexpedición Merc.Vi
Start Date: 01-01-2004
End Date: 12-31-2031
Duty: SS SÍ PAGA / SÍ GARANTIZA
VAT: NN NO PAGA / NO GARANTIZA";

	protected override string ExpectedInvalidConcessionCPCEndDateMessage => @"Unable to import Concession CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 9AR
Description: Pdte. Concesión Franquicia Arancelaria
Start Date: 01-02-2007
End Date: 12-31-2031
Duty: *S ASUMIDO DE CAS. 37.1 / SÍ GARANTIZA
VAT: ** ASUMIDO DE CAS. 37.1 / ASUMIDO DE CAS. 37.1";

	protected string ExpectedEmptyCPCDutyMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 0178
Description: Libre Práctica Reexpedición Merc.Pro
Start Date: 01-01-1990
End Date: 31-12-2099
Duty: 
VAT: SS SÍ PAGA / SÍ GARANTIZA";

	protected string ExpectedEmptyConcessionCPCDutyMessage => @"Unable to import Concession CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 9CO
Description: Pdte. Levante sin Espera Conc. Conti
Start Date: 07-07-1998
End Date: 31-12-2099
Duty: 
VAT: ** ASUMIDO DE CAS. 37.1 / ASUMIDO DE CAS. 37.1";

	protected string ExpectedInvalidCPCVATMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 0191
Description: Libre Práctica Reexpedición Merc.
Start Date: 01-01-1990
End Date: 31-12-2099
Duty: SS SÍ PAGA / SÍ GARANTIZA
VAT: N";

	protected string ExpectedInvalidConcessionCPCVATMessage => @"Unable to import Concession CPC as there is a missing or wrong attribute 'code', 'description', 'start date', 'end date', 'duty' or 'vat'. Details:
Code: 9CR
Description: Cambio residencia con compromiso de establecerse
Start Date: 14-07-2015
End Date: 31-12-2099
Duty: *S ASUMIDO DE CAS. 37.1 / SÍ GARANTIZA
VAT: *";
}
