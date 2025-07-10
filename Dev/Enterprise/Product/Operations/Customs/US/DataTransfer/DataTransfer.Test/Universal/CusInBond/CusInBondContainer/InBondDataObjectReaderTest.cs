using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class InBondDataObjectReaderTest<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity>
	{
		protected Container SetupContainer(ZString containerNumber, ZString sealNumber, ZString secondSealNumber, ContainerType containerType, ZInt link)
		{
			return new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = containerNumber, Seal = sealNumber, SecondSeal = secondSealNumber, ContainerType = containerType, Link = link };
		}
	}
}
