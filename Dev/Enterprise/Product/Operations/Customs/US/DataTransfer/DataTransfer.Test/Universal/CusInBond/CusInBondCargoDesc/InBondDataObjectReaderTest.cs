using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class InBondDataObjectReaderTest<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity>
	{
		protected PackingLine SetupPackingLine(ZLong pieceCount, ZString manifestUnitCode, ZString? harmonisedTariff, ZDecimal monetaryValue, ZString description, ZDecimal weight, ZString weightUnit, ZString marksAndNumbers, ZInt containerLink)
		{
			return new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerLink = containerLink,
				PackQty = pieceCount,
				PackType = new PackageType()
				{ Code = manifestUnitCode },
				HarmonisedCode = harmonisedTariff,
				LinePrice = monetaryValue,
				GoodsDescription = description,
				Weight = weight,
				WeightUnit = new UnitOfWeight()
				{ Code = weightUnit },
				MarksAndNos = marksAndNumbers
			};
		}
	}
}
