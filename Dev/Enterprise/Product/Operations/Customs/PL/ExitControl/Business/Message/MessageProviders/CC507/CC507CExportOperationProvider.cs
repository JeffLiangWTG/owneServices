using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC507CExportOperationProvider(CusExitReport exitReport) : ICC507CExportOperation
{
	readonly CusExitReport cusExitReport = Argument.NotNull(exitReport, nameof(exitReport));
	readonly CusExitConsignment cusExitConsignment = Argument.NotNull(exitReport.Consignment, $"{nameof(exitReport)}.{nameof(CusExitReport.Consignment)}");

	public bool? IssuedByAES => true;

	public string MRN => cusExitConsignment.CXC_MovementReference;

	public DateTime ArrivalNotificationDateAndTime => CachedValueHelper.GetValue(ref arrivalNotificationDateAndTime,
		() => cusExitReport.CER_DateTime.IsEmpty
			? ZDateTimeOffset.Now.ToUtcDateTime()
			: cusExitReport.CER_DateTime.ToUtcDateTime());
	CachedValue<DateTime> arrivalNotificationDateAndTime;

	public string ArrivalNotificationPlace => cusExitReport.CER_Location;

	public int? StoringFlag => cusExitReport.Header.StoringFlag ? 1 : 0;

	public int? DiscrepanciesExist => CachedValueHelper.GetValue(ref discrepanciesExist, () => cusExitReport.CER_Calc_Discrepancies
		? 1
		: 0);
	CachedValue<int?> discrepanciesExist;
}
