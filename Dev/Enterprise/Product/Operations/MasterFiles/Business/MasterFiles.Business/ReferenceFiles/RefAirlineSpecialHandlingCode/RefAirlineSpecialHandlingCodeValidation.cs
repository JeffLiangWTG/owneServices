//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefAirlineSpecialHandlingCodeValidation
//
//    This class should be used for overriding validation in AutoRefAirlineSpecialHandlingCodeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration.Reference;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineSpecialHandlingCodeValidation : AutoRefAirlineSpecialHandlingCodeValidation
	{
		public RefAirlineSpecialHandlingCodeValidation(AutoRefAirlineSpecialHandlingCode parent) : base(parent)
		{
		}

		IIATASpecialHandlingCodesProvider IATASpecialHandlingCodesProvider
		{
			get
			{
				return iATASpecialHandlingCodesProvider ??= ObjectFactory.Get<IIATASpecialHandlingCodesProvider>();
			}
		}

		IIATASpecialHandlingCodesProvider iATASpecialHandlingCodesProvider;

		protected override void CheckRHC_Code()
		{
			if (Parent.RHC_Code.Length != 3)
			{
				Parent.RHC_CodeInfo.AddError(Res.GetString("26CF5FCD-AAF7-468D-9586-D1C7F7FE2E7C", "Length of Code is not equal to 3"));
			}

			if (IATASpecialHandlingCodesProvider.GetCodeDescriptionPairList().ContainsCode(Parent.RHC_Code))
			{
				Parent.RHC_CodeInfo.AddError(Res.GetString("25BDF489-2C6A-4145-8EC2-CBD2A5D2AB89", "This Special Handling Code is already defined in the IATA approved code list"));
			}
		}
		protected override void CheckRHC_DestinationPortOrCountry()
		{
			base.CheckRHC_DestinationPortOrCountry();

			if (!Parent.RHC_Code.IsEmpty)
			{
				CheckUniqueness(Parent.RHC_DestinationPortOrCountryInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.RHC_DestinationPortOrCountryInfo, Parent.Lookups.Locations);
		}

		protected override void CheckRHC_OriginPortOrCountry()
		{
			base.CheckRHC_OriginPortOrCountry();

			if (!Parent.RHC_Code.IsEmpty)
			{
				CheckUniqueness(Parent.RHC_OriginPortOrCountryInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.RHC_OriginPortOrCountryInfo, Parent.Lookups.Locations);
		}

		void CheckUniqueness(ZPropertyInfo property)
		{
			var query = new ZQuery();
			query.AddToFilter(RefAirlineSpecialHandlingCodeSchema.RHC_RM_Airline, Parent.RHC_RM_Airline);
			query.AddToFilter(RefAirlineSpecialHandlingCodeSchema.RHC_OriginPortOrCountry, Parent.RHC_OriginPortOrCountry);
			query.AddToFilter(RefAirlineSpecialHandlingCodeSchema.RHC_DestinationPortOrCountry, Parent.RHC_DestinationPortOrCountry);
			query.AddToFilter(RefAirlineSpecialHandlingCodeSchema.RHC_Code, Parent.RHC_Code);
			query.AddToFilter(RefAirlineSpecialHandlingCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			var match = Parent.Factory.LoadTop1<RefAirlineSpecialHandlingCode>(query);
			if (match != null)
			{
				property.AddError(Res.GetString("DBCD32CE-28B9-42C6-AB18-7E427BCF7A1C", "The same Product cannot be duplicated for the same Origin/Destination."));
			}
		}
	}
}

