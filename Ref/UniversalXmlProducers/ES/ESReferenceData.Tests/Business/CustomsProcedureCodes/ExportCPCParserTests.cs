using System;
using CargoWise.RefDbRepo.ESReferenceData.Business;

namespace CargoWise.RefDbRepo.ESReferenceData.Tests;

class ExportCPCParserTests : CPCParserTests<ExportCPCItem, ExportCPCItemMap, ExportCPCConcessionItem, ExportCPCConcessionItemMap>
{
	protected override string TestClassName => nameof(ExportCPCParserTests);

	protected override Func<IDateTimeProvider, CPCParser<ExportCPCItemMap, ExportCPCConcessionItemMap>> ParserToRun => (dateTimeProvider) => new ExportCPCParser(dateTimeProvider);

	protected override string System => "EXPORT";

	protected override string ExpectedEmptyCPCCodeMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date' or 'end date'. Details:
Code: 
Description: expedición merc.libre práctica
Start Date: 01-07-2008
End Date: 31-12-2099";

	protected override string ExpectedEmptyCPCDescriptionMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date' or 'end date'. Details:
Code: 1002
Description: 
Start Date: 01-01-2010
End Date: 31-12-2099";

	protected override string ExpectedEmptyConcessionCPCDescriptionMessage => @"Unable to import Concession CPC as there is a missing or wrong attribute 'code', 'description', 'start date' or 'end date'. Details:
Code: 802
Description: 
Start Date: 07-03-2014
End Date: 31-12-2099";

	protected override string ExpectedEmptyCPCStartDateMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date' or 'end date'. Details:
Code: 1021
Description: export.definitiva mercancia en RPP
Start Date: 
End Date: 31-12-2099";

	protected override string ExpectedEmptyConcessionCPCStartDateMessage => @"Unable to import Concession CPC as there is a missing or wrong attribute 'code', 'description', 'start date' or 'end date'. Details:
Code: 803
Description: REA: Prod. Art. 13.4 Rgto 180/2014
Start Date: 
End Date: 31-12-2099";

	protected override string ExpectedInvalidCPCEndDateMessage => @"Unable to import CPC as there is a missing or wrong attribute 'code', 'description', 'start date' or 'end date'. Details:
Code: 1022
Description: exp.definitiva mercancia en RPP econom.
Start Date: 01-01-1997
End Date: 2099-31-12";

	protected override string ExpectedInvalidConcessionCPCEndDateMessage => @"Unable to import Concession CPC as there is a missing or wrong attribute 'code', 'description', 'start date' or 'end date'. Details:
Code: 804
Description: REA: Prod. Art. 13.5 Rgto 180/2014
Start Date: 07-03-2014
End Date: 12-31-2031";
}
