using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DummyBizOWithIncoTermValidation : DummyBizoValidation
	{
		public DummyBizOWithIncoTermValidation(DummyBizOWithIncoTerm bizO)
			: base(bizO)
		{
		}

		public new DummyBizOWithIncoTerm Parent
		{
			get { return (DummyBizOWithIncoTerm)base.Parent; }
		}

		public void ValidateZ0_Incoterm()
		{
			ValidateCalculatedProperty(Parent.Z0_IncotermInfo);
		}

		protected void CheckZ0_Incoterm()
		{
			IncotermValidation.Instance.WarningIfExpired(Parent.Z0_IncotermInfo);
		}
	}
}
