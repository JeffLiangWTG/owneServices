using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class DummyFindBox : IFindBox
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public IFindBoxListProvider ListProvider { get; set; }

		public IFindBoxPopup PopupForm { get; set; }
	}
}
