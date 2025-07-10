using CargoWise.Types;

namespace Enterprise.Customs.US.Business.InterfaceImplementation
{
	class JobDeclarationCustomsCharges : Customs.Business.InterfaceImplementations.JobDeclarationCustomsCharges
	{
		public JobDeclarationCustomsCharges(JobDeclaration declaration)
			: base(declaration)
		{
		}

		#region Accounting Integration

		protected override ZBool IsCustomsChargesActiveCore
		{
			get { return true; }
		}

		#endregion
	}
}
