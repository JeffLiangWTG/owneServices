using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class CargoInfoTypeProvider : ICargoInfoType
	{
		readonly USExportAsycudaPack pack;

		public CargoInfoTypeProvider(USExportAsycudaPack pack)
		{
			this.pack = pack;
		}

		public IManifestStringType Quantity => new ManifestStringTypeProvider(pack.APA_PackQty.ToString());

		public IManifestStringType QuantityUnitOfMeasure => new ManifestStringTypeProvider(pack.APA_PackUQ);

		public IManifestStringType CargoDescription => new ManifestStringTypeProvider(pack.APA_GoodsDescription);

		public IManifestStringType MarksAndNumbers => new ManifestStringTypeProvider(pack.APA_MarksAndNumbers);

		public IManifestStringType CommodityCode => new ManifestStringTypeProvider("");

		public IManifestStringType CountryOfOrigin => new ManifestStringTypeProvider("US");

		public IManifestStringType Value => new ManifestStringTypeProvider("");

		public IManifestStringType Weight => new ManifestStringTypeProvider("");

		public IManifestStringType WeightUnitOfMeasure => new ManifestStringTypeProvider("");

		public IVINInfoType VINInfoList => new VINInfoTypeProvider(pack.APA_VINNumber);

		public Collection<IHazmatInfoType> HazmatInfoList
		{
			get
			{
				var result = new Collection<IHazmatInfoType>();

				foreach (var undg in pack.UNDGs)
				{
					result.Add(new HazmatInfoTypeProvider(undg));
				}

				return result;
			}
		}

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());
	}
}
