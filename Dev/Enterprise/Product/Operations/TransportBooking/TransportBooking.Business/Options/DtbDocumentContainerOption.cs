using CargoWise.Types;

namespace Enterprise.TransportBookings.Business.Options
{
	public partial class DtbDocumentContainerOption : AutoDtbDocumentContainerOption
	{
		public DtbDocumentContainerOption(ZString containerNumber, ZString containerType, ZString seal, ZInt link, ZString releaseNumber) : base()
		{
			base.ContainerNumber = containerNumber;
			base.ContainerType = containerType;
			base.Seal = seal;
			base.Link = link;
			base.ReleaseNumber = releaseNumber;

			base.DeliverContainer = true;
		}
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.Business.Options
{
	using Enterprise.TransportBookings.Shared.Testing;

	public partial class DtbDocumentContainerOption : IDtbDocumentContainerOption
	{
		string IDtbDocumentContainerOption.ContainerNumber
		{
			get { return ContainerNumber; }
		}
	}
}

#endif
