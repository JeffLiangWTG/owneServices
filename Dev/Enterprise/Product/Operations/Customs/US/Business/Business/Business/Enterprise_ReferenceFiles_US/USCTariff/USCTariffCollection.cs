using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.USCTariff)]
	public class USCTariffCollection : BusinessObjectCollection<USCTariff>
	{
		internal static class DateRangeSearchTexts
		{
			//if you can, you should use Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.Today instead of this const(s)/class
			//While writing this class there was no possibility to use Enterprise.ZArchitecture.Business.ModuleDateFilter.DateRangeSearchTexts.today. 
			//Because the reference to the Enterprise.ZArchitecture.GUI in which the class Enterprise.ZArchitecture.Business.ModuleDateFilter was not present there
			public const string Today = "Today";
		}

		public USCTariffCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(USCTariff.FilterSchema.Date, USCTariff.FilterSchema.DatePropertyNameToDefault, new ZString(DateRangeSearchTexts.Today)));
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new TariffFormattedFindBoxListProvider(this); }
		}
	}
}
