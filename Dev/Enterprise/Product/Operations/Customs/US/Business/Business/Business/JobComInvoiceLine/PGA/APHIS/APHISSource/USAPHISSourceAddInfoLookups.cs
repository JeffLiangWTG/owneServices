//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISSourceAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAPHISSourceAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USAPHISSourceAddInfoLookups : AutoUSAPHISSourceAddInfoLookups
	{
		public USAPHISSourceAddInfoLookups(AutoUSAPHISSourceAddInfo parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList SourceTypes
		{
			get { return SourceTypeCodesList.GetListForAPHIS(Factory, Source.ProgramType); }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		public ICodeDescriptionPairList ProcessingTypes
		{
			get
			{
				var programType = ZString.Empty;
				var categoryType = ZString.Empty;

				var header = Source.Header;
				if (header != null)
				{
					programType = header.US_ProgramType;
					categoryType = header.US_CategoryType;
				}

				return APHISProcessingTypeCodeList.GetListFor(Factory, programType, categoryType);
			}
		}

		protected new USAPHISSourceAddInfo Parent
		{
			get { return (USAPHISSourceAddInfo)base.Parent; }
		}

		protected APHISSource Source
		{
			get { return Parent.Parent; }
		}
	}
}
