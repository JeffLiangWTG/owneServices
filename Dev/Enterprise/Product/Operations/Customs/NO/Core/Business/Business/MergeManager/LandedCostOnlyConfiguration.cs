using System;
using Enterprise.Customs.Business;
using static Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business
{
	public class LandedCostOnlyConfiguration : ILandedCostOnlyConfiguration
	{
		public Func<Customs.Business.CusEntryHeader, string, bool> GetIsLandedCostingOnlyFuncForEntryHeader()
			=> (baseEntryHeader, feeCode)
			=> false;

		public Func<Customs.Business.CusEntryLine, string, bool> GetIsLandedCostingOnlyFuncForEntryLine()
			=> (baseEntryLine, feeCode)
			=> (baseEntryLine, feeCode?.ToUpper()) switch
			{
				(CusEntryLine entryLine, RefCusTaxOrFee.MV1 or RefCusTaxOrFee.MV2) => entryLine.Declaration.HasImporterWithMVARegistration,
				_ => false,
			};
	}
}
