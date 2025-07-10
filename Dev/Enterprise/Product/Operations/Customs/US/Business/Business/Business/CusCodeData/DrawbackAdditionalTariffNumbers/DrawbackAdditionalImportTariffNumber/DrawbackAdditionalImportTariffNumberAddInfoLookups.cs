//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDrawbackAdditionalImportTariffNumberAddInfoLookups
//
//    This class should be used for overriding collections in AutoDrawbackAdditionalImportTariffNumberAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class DrawbackAdditionalImportTariffNumberAddInfoLookups : AutoDrawbackAdditionalImportTariffNumberAddInfoLookups
	{
		public DrawbackAdditionalImportTariffNumberAddInfoLookups(AutoDrawbackAdditionalImportTariffNumberAddInfo parent) : base(parent)
		{
		}
		protected new DrawbackAdditionalImportTariffNumberAddInfo Parent
		{
			get { return (DrawbackAdditionalImportTariffNumberAddInfo)base.Parent; }
		}

		public USCTariffCollection ImportTariffs
		{
			get
			{
				var result = new USCTariffCollection(Factory);
				var filter = new FilterBusinessObjectDefault(USCTariff.FilterSchema.Tariff, "Property", Parent.US_Tariff);
				result.FilterBusinessObjectDefaults.Add(filter);
				return result;
			}
		}

		public CodeDescriptionPairList UnitOfMeasureCodes
		{
			get { return Factory.GetCachedValue<ACEDrawbackUnitOfMeasureList>(); }
		}
	}
}
