namespace Enterprise.Customs.US.Business
{
	public class SimplifiedEntryStrategy : CargoReleaseMergeStrategy
	{
		public SimplifiedEntryStrategy(JobDeclaration declaration)
			: base(declaration, CusEntryHeaderMessageTypeList.Codes.ACECargoRelease)
		{
		}

		protected override Customs.Business.CusEntryHeader GetExistingEntryHeader(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			return Declaration.ActiveEntryHeaders.SimplifiedEntry ?? base.GetExistingEntryHeader(invoiceLine);
		}

		public override Customs.Business.MergeKey GetKeyForLine(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForLine(invoiceLine);
			var line = (JobComInvoiceLine)invoiceLine;
			new SimplifiedEntryLineMergeKeyGenerator().AddKey(result, line);
			return result;
		}

		new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		protected override bool IsActiveCore
		{
			get
			{
				var declaration = Declaration;
				return declaration.IsFormalImport && declaration.IsACECargoCertificationMode;
			}
		}
	}

	class SimplifiedEntryLineMergeKeyGenerator : CargoReleaseLineMergeKeyGenerator
	{
		public override void AddKey(Customs.Business.MergeKey mergeKey, JobComInvoiceLine line)
		{
			base.AddKey(mergeKey, line);
			mergeKey.Add(line.JI_OA_SoldToPartyAddress);
			mergeKey.Add(line.JI_OA_Seller);
			mergeKey.Add(line.US_ZoneStatus);
			mergeKey.Add(line.US_PrivilegedStatusDate);
			mergeKey.Add(line.US_FTZCurrentTariff);
			mergeKey.Add(line.JI_OA_ShipToPartyAddress);

			mergeKey.Add(line.US_ExclusionNumber);
			mergeKey.Add(line.US_ProductExclusion);
		}
	}
}
