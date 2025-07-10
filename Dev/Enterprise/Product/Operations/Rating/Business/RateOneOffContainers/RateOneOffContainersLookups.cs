using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateOneOffContainersLookups : AutoRateOneOffContainersLookups
	{
		public RateOneOffContainersLookups(AutoRateOneOffContainers parent) : base(parent)
		{
		}

		public RefPackTypeCollection RefPackTypes
		{
			get { return new RefPackTypeCollection(Factory, true); }
		}

		public CodeDescriptionPairList VolumeUnits
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		public CodeDescriptionPairList WeightUnits
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList DimensionUnits
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}
	}
}

