using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class ValaValueRebateCalculator : RebateCalculator<RCCCertificate>
	{
		public ValaValueRebateCalculator(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZString PermitTypeCore => PermitTypeList.Codes.VALA;

		protected override IEnumerable<RCCCertificate> GetAllCertificates(JobDeclaration declaration)
															=> declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().SelectMany(h => h.MergedLines)
																													.Select(l => l.EntryInstruction)
																													.SelectMany(i => i?.RCCCertificates ?? Enumerable.Empty<RCCCertificate>())
																													.Cast<RCCCertificate>();

		protected override IEnumerable<RCCCertificate> GetCertificatesForEntryLine(CusEntryLine entryLine) => entryLine.EntryInstruction.RCCCertificates.Cast<RCCCertificate>();

		protected override ZString GetPRVValue(decimal value)
		{
			return value.ToString("0.##");
		}
	}
}
