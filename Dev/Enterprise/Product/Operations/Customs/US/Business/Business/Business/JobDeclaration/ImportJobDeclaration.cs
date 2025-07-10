using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ImportJobDeclaration : Customs.Business.ImportJobDeclaration
	{
		public ImportJobDeclaration(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override void PerformCountrySpecificImporting()
		{
			base.PerformCountrySpecificImporting();
			var importedDeclaration = ImportedDeclaration as JobDeclaration;
			if (importedDeclaration.IsFormalImport)
			{
				new PaymentDetailsDefaulter().Default(importedDeclaration);
			}
		}
	}
}
