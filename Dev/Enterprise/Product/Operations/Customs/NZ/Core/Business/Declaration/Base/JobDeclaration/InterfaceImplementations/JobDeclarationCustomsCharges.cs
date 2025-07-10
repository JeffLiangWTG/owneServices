using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Declaration.InterfaceImplementations
{
	class JobDeclarationCustomsCharges : Customs.Business.InterfaceImplementations.JobDeclarationCustomsCharges
	{
		public JobDeclarationCustomsCharges(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override ZBool IsCustomsChargesActiveCore
		{
			get
			{
				var isExportRefundEntry = declaration.IsExport && (declaration.IsDrawback || declaration.IsCompletion);
				return !declaration.IsECIWriteoff && !isExportRefundEntry;
			}
		}
	}
}
