using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class ItineraryData : CusCodeData
	{
		public ItineraryData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(2)]
		[RelatedBusinessObject(nameof(RoutingCountry))]
		[ResourceStringData("Enterprise.Customs.TW.Business.ItineraryData|CY_Code", Caption = "Routing Country Code", FullDescription = "Indicates the country code that the goods pass through from the country of export to the final destination.")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		public RefCountry RoutingCountry => Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CY_Code);

		[ResourceStringData("Enterprise.Customs.TW.Business.ItineraryData|Description", Caption = "Name")]
		public override ZString Description => RoutingCountry?.RN_Desc ?? ZString.Empty;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			CY_Type = CusCodeDataTypeList.Codes.Itinerary;
		}

		public new ItineraryDataValidation Validation => (ItineraryDataValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ItineraryDataValidation(this);
		}

		public new ItineraryDataLookups Lookups => (ItineraryDataLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new ItineraryDataLookups(this);
		}
	}
}
