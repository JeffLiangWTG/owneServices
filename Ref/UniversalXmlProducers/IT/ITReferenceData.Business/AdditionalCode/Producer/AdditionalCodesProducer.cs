using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode
{
	public class AdditionalCodesProducer
	{
		public AdditionalCodesProducer(IRawAdditionalCodesLoader loader, IRawAdditionalCodeMapper mapper)
		{
			this.loader = Argument.NotNull(loader, nameof(loader));
			this.mapper = Argument.NotNull(mapper, nameof(mapper));
		}

		readonly IRawAdditionalCodesLoader loader;
		readonly IRawAdditionalCodeMapper mapper;

		public IEnumerable<RefCusCodeList> ProduceEntities()
		{
			var additionalCodeList = loader.GetRawAdditionalCodes();
			return additionalCodeList.Select(additionalCode => mapper.GetMapping(additionalCode)).ToArray();
		}
	}
}
