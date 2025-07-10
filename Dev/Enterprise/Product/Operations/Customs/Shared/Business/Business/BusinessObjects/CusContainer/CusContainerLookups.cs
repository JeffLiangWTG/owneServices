//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusContainerLookups
//
//    This class should be used for overriding collections in AutoCusContainerLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusContainerLookups : AutoCusContainerLookups
	{
		public CusContainerLookups(AutoCusContainer parent)
			: base(parent)
		{
		}

		internal protected new BaseCusContainer Parent
		{
			get { return (BaseCusContainer)base.Parent; }
		}

		#region Lists

		RefContainerCollection fContainerTypeCollection;
		public RefContainerCollection ContainerTypeCollection
		{
			get
			{
				if (fContainerTypeCollection == null)
				{
					fContainerTypeCollection = new RefContainerCollection(Factory);
				}
				return fContainerTypeCollection;
			}
		}

		public ContainerYardCollection ContainerYard_List
		{
			get { return Parent.JobContainer.Lookups.ContainerYardList; }
		}

		public CodeDescriptionPairList TemperatureUnit_List
		{
			get { return Parent.JobContainer.JC_TemperatureUnit_List; }
		}

		public CodeDescriptionPairList AirVentFlowRateUnit_List
		{
			get { return Parent.JobContainer.BindToLists.AirVentFlowRateUnits; }
		}

		public RefCommodityCodeCollection ContainerCommodityCode_List
		{
			get { return Parent.JobContainer.ContainerCommodityCode_List; }
		}

		public virtual CodeDescriptionPairList MessageStatusList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList WeightUnits
		{
			get { return Parent.JobContainer.TotalWeightUnit_List; }
		}

		public virtual CodeDescriptionPairList CO_FCL_LCL_NCT_List
		{
			get
			{
				CodeDescriptionPairList newList = new CodeDescriptionPairList();
				newList.AddPair(BaseCusContainer.ContainerModes.FullContainerLoad, Res.GetString("23053664-2777-4d4f-a89b-cacdd7084778", "Full Container Load"));
				newList.AddPair(BaseCusContainer.ContainerModes.LessContainerLoad, Res.GetString("36855d9b-d04e-4386-989b-3043e5ff8ab8", "Less Container Load"));
				newList.AddPair(BaseCusContainer.ContainerModes.FCX, Res.GetString("4be8ffb5-9c03-4344-b517-3f1280ca4508", "Full Container Load - Multiple Bills"));
				newList.AddPair(BaseCusContainer.ContainerModes.BreakBulk, Res.GetString("548a61c0-1606-4ac6-8dc2-6578418fd750", "Break Bulk"));
				return newList;
			}
		}

		public virtual CodeDescriptionPairList ContainerSizeList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public virtual CodeDescriptionPairList TotalPackagesUnit_List
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.Packages.Count > 0)
				{
					var package = Parent.Packages[0];
					result = package.PackTypeList;
				}
				else
				{
					result = new CodeDescriptionPairList();
				}
				return result;
			}
		}

		public RefCountryCollection CountryList => new RefCountryCollection(Factory);

		#endregion
	}
}
