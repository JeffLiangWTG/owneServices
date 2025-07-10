
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateOneOffPackLineLookups : AutoRateOneOffPackLineLookups
	{
		public RateOneOffPackLineLookups(AutoRateOneOffPackLine parent) : base(parent)
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

		public CodeDescriptionPairList LooseCargoContainerTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if
					(
						Parent is RateOneOffPackLine looseCargo
						&& looseCargo?.Parent?.Containers?.Count > 0
					)
				{
					foreach (var container in looseCargo.Parent.Containers.Cast<RateOneOffContainers>())
					{
						if (container.RefContainer != null)
						{
							result.Add(new CodeDescriptionPair(container.RefContainer.RC_Code.ToString(), container.RefContainer.RC_DescriptionMultilingual.ToString()));
						}
					}
				}

				return result;
			}
		}

		public CodeDescriptionPairList TPL_VehicleTransmission_List
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Core.Constants.VehicleTransmissionType.Automatic, ResString.GetMultilingualString("d63d108c-3e27-4e3f-92a9-ddcb63d02de9", "Automatic"));
				result.AddPair(Core.Constants.VehicleTransmissionType.Manual, ResString.GetMultilingualString("4773085b-bb81-4443-a83c-fd2268a58a84", "Manual"));
				return result;
			}
		}
	}
}
