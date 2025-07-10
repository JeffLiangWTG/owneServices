using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DummyBusinessObjectJobDocAddressValidation : DummyBizoValidation
	{
		protected readonly DummyBusinessObjectJobDocAddress ParentDocAddress;
		public DummyBusinessObjectJobDocAddressValidation(DummyBusinessObjectJobDocAddress parent) : base(parent)
		{
			ParentDocAddress = parent;
		}
	}
}
