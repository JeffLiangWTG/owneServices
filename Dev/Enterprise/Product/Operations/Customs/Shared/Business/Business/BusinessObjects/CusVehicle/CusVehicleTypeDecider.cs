using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusVehicleTypeDecider : CountrySpecificTypeDecider
	{
		protected override IEnumerable<CountrySpecificType> CountrySpecificTypesCore => new CountrySpecificType[]
		{
			new (Core.Constants.CountryCodes.KoreaSouth, ObjectFactory.GetType<Integration.Customs.KR.ICusVehicle>),
			new (Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.ICusVehicle>),
			new (Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.ICusVehicle>),
			new (Core.Constants.CountryCodes.Switzerland, ObjectFactory.GetType<Integration.Customs.CH.ICusVehicle>),
			new (Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.ICusVehicle>),
			new (Core.Constants.CountryCodes.SouthAfrica, ObjectFactory.GetType<Integration.Customs.ZA.ICusVehicle>),
			new ("ASY", ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusVehicle>),
			new (Core.Constants.CountryCodes.Mexico, ObjectFactory.GetType<Integration.Customs.MX.ICusVehicle>),
			new (Core.Constants.CountryCodes.UnitedArabEmirates, ObjectFactory.GetType<Integration.Customs.AE.ICusVehicle>),
		};

		protected override Type DefaultTypeForEuCountry => ObjectFactory.GetType<Integration.Customs.EU.ICusVehicle>();

		protected override Type DefaultTypeForUnsupportedCountry => typeof(CusVehicle);

		protected override IEnumerable<KeyValuePair<Func<ZString, bool>, Func<Type>>> DataGroupSepcificTypes
		{
			get
			{
				foreach (var datagroupSepcificType in base.DataGroupSepcificTypes)
				{
					yield return datagroupSepcificType;
				}
				yield return new (ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry, ObjectFactory.GetType<Integration.Customs.AsycudaCustoms.ICusVehicle>);
			}
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var dataModel = row[CusVehicleSchema.Constants.CVH_DataModel].ToString();
			return GetTypeForCountryCode(dataModel);
		}

		public override Type GetTypeForBinding() => null;
	}
}
