using CargoWise.Types;
using Enterprise.Customs.Business.InterfaceImplementations;

namespace Enterprise.Customs.US.Business
{
	class ReconDeclarationCustomsCharges : JobDeclarationCustomsCharges
	{
		public ReconDeclarationCustomsCharges(ReconDeclaration reconDeclaration) : base(reconDeclaration.ReconWrappedJobDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
		}

		readonly ReconDeclaration reconDeclaration;

		protected override Customs.Business.CusEntryHeader[] GetEntries()
		{
			return new CusEntryHeader[] { reconDeclaration.ReconEntry.GetEntry() };
		}

		protected override ZBool IsCustomsChargesActiveCore
		{
			get { return true; }
		}
	}
}
