namespace Enterprise.Customs.NZ.Business.TariffValidation
{
	public struct AllowableTariffCodeTypes
	{
		public AllowableTariffCodeTypes(Declaration.JobDeclaration declaration)
		{
			if (declaration != null && (declaration.IsImport || declaration.IsExport || declaration.IsExcise))
			{
				Import = declaration.IsImport;
				Export = declaration.IsExport;
				Excise = declaration.IsExcise;
			}
			else
			{
				Import = true;
				Export = true;
				Excise = true;
			}
		}

		public AllowableTariffCodeTypes(bool import, bool export, bool excise)
		{
			Import = import;
			Export = export;
			Excise = excise;
		}

		public readonly bool Import;
		public readonly bool Export;
		public readonly bool Excise;
		public bool ImportOrExport
		{
			get { return Import || Export; }
		}
	}
}
