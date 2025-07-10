using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Rating.Business
{
	public class RatingHeaderEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		readonly AssemblyData parent;
		readonly string ratingHeaderType;
		readonly bool isGlobal;

		public RatingHeaderEDocsViaUniversalXmlSupport(AssemblyData parent, string ratingHeaderType, bool isGlobal = false)
		{
			this.parent = parent;
			this.ratingHeaderType = ratingHeaderType;
			this.isGlobal = isGlobal;
		}

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			var collection = parent.GetBusinessObjectCollection(factory);
			var bizos = ((IFindBoxListProvider)collection).GetBusinessObjectsFromCode(code).Cast<RatingHeader>();

			var result = bizos
				.Where(ratingHeader => ratingHeader.TH_RateType == ratingHeaderType)
				.FirstOrDefault(ratingHeader => ratingHeader.IsGlobal() == isGlobal);

			return result;
		}

		public ZString ExpectedCodeFormat
		{
			get
			{
				switch (ratingHeaderType)
				{
					case RatingConstants.RatingHeaderTypes.ClientRate:
					case RatingConstants.RatingHeaderTypes.Costing:
						return (NoResString)"Client";
					case RatingConstants.RatingHeaderTypes.Tariff:
						return (NoResString)"Level";
					default:
						return (NoResString)"Unknown";
				}
			}
		}

		public ZString ExampleCodeFormat
		{
			get => ratingHeaderType == RatingConstants.RatingHeaderTypes.Tariff ? "1" : "AUTRAN_AU";
		}
	}
}
