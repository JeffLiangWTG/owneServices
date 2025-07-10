using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class DutyRebateCalculator : RebateCalculator<DutyRebateCertificate>
	{
		public DutyRebateCalculator(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString PermitTypeCore => PermitTypeList.Codes.PRC;

		protected override IEnumerable<DutyRebateCertificate> GetAllCertificates(JobDeclaration declaration)
															=> declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(h => h.MergedLines)
																													.Select(l => l.EntryInstruction)
																													.SelectMany(i => i?.DutyRebateCertificates ?? Enumerable.Empty<DutyRebateCertificate>())
																													.Cast<DutyRebateCertificate>();

		protected override IEnumerable<DutyRebateCertificate> GetCertificatesForEntryLine(CusEntryLine entryLine) => entryLine.EntryInstruction.DutyRebateCertificates.Cast<DutyRebateCertificate>();

		protected override ZString GetPRVValue(decimal value)
		{
			return (value * 100).ToString();
		}
	}
}
