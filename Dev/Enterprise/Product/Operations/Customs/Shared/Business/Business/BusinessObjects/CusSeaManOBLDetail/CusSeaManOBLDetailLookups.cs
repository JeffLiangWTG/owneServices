//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSeaManOBLDetailLookups
//
//    This class should be used for overriding collections in AutoCusSeaManOBLDetailLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusSeaManOBLDetailLookups : AutoCusSeaManOBLDetailLookups
	{
		public CusSeaManOBLDetailLookups(AutoCusSeaManOBLDetail parent) : base(parent)
		{
		}

		#region Properties

		#region PackageTypes

		protected virtual CodeDescriptionPairList GetNewPackageTypes()
		{
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList PackageTypes
		{
			get
			{
				if (fPackageTypes == null)
				{
					fPackageTypes = GetNewPackageTypes();
				}

				return fPackageTypes;
			}
		}
		protected CodeDescriptionPairList fPackageTypes;

		#endregion

		#region GrossWeightCodes

		protected virtual CodeDescriptionPairList GetNewGrossWeightCodes()
		{
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList GrossWeightCodes
		{
			get
			{
				if (fGrossWeightCodes == null)
				{
					fGrossWeightCodes = GetNewGrossWeightCodes();
				}

				return fGrossWeightCodes;
			}
		}
		protected CodeDescriptionPairList fGrossWeightCodes;

		#endregion

		#region QuantityUnits

		protected virtual CodeDescriptionPairList GetNewQuantityUnits()
		{
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList QuantityUnits
		{
			get
			{
				if (fQuantityUnits == null)
				{
					fQuantityUnits = GetNewQuantityUnits();
				}

				return fQuantityUnits;
			}
		}
		protected CodeDescriptionPairList fQuantityUnits;

		#endregion

		#region CargoTypes

		protected virtual CodeDescriptionPairList GetNewCargoTypes()
		{
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList CargoTypes
		{
			get
			{
				if (fCargoTypes == null)
				{
					fCargoTypes = GetNewCargoTypes();
				}

				return fCargoTypes;
			}
		}
		CodeDescriptionPairList fCargoTypes;

		#endregion

		#region ContainerSizes

		protected virtual CodeDescriptionPairList GetNewContainerSizes()
		{
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList ContainerSizes
		{
			get
			{
				if (fContainerSizes == null)
				{
					fContainerSizes = GetNewContainerSizes();
				}

				return fContainerSizes;
			}
		}
		CodeDescriptionPairList fContainerSizes;

		#endregion

		#region TypesOfContainers

		protected virtual CodeDescriptionPairList GetNewTypesOfContainers()
		{
			return new CodeDescriptionPairList();
		}

		public CodeDescriptionPairList TypesOfContainers
		{
			get
			{
				if (fTypesOfContainers == null)
				{
					fTypesOfContainers = GetNewTypesOfContainers();
				}

				return fTypesOfContainers;
			}
		}
		CodeDescriptionPairList fTypesOfContainers;

		#endregion

		#endregion
	}
}
