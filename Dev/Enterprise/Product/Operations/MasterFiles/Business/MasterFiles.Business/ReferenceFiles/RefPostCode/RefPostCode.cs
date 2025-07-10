using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefPostCodeSchema.Constants.RK_CityTownPostCode), DescriptionProperty(RefPostCodeSchema.Constants.RK_CityTownPostCode)]
	public class RefPostCode : AutoRefPostCode, IRefPostCode
	{
		public RefPostCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Load Postcode

		public static RefPostCode Load(BusinessObjectFactory factory, ZString postcode, ZString countryCode)
		{
			if (string.IsNullOrEmpty(countryCode))
			{
				throw new ArgumentNullException(nameof(countryCode), "A country must be specified when attempting to load a specific Postcode");
			}
			if (string.IsNullOrEmpty(postcode))
			{
				throw new ArgumentNullException(nameof(postcode), "A postcode must be specified");
			}

			var filter = new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, postcode);
			filter.AddToFilter(RefPostCodeSchema.RK_RN_NKCountry, countryCode);
			filter.AddToFilter(RefPostCodeSchema.RK_IsActive, true);
			return factory.LoadTop1<RefPostCode>(filter);
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DataRow row = ((IBusinessObjectInternals)this).Row;
			row[RefPostCodeSchema.Constants.RK_IsSystem] = false;
		}

		#region Readonly Attributes

		[ReadOnlyMember(Schema.RK_IsSystem)]
		public override ZString RK_CityTownPostCode
		{
			get { return base.RK_CityTownPostCode; }
			set { base.RK_CityTownPostCode = value; }
		}

		[ReadOnlyMember(Schema.RK_IsSystem)]
		public override ZDecimal RK_Lattitude
		{
			get { return base.RK_Lattitude; }
			set { base.RK_Lattitude = value; }
		}

		[ReadOnlyMember(Schema.RK_IsSystem)]
		public override ZDecimal RK_Longitude
		{
			get { return base.RK_Longitude; }
			set { base.RK_Longitude = value; }
		}

		[ReadOnlyMember(Schema.RK_IsSystem)]
		public override ZString RK_RN_NKCountry
		{
			get { return base.RK_RN_NKCountry; }
			set { base.RK_RN_NKCountry = value; }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Related Objects

		[ChildEditable]
		public RefCityTownCollection CityTowns
		{
			get
			{
				if (cityTowns == null)
				{
					cityTowns = new RefCityTownCollection(this);
					RegisterEditableChildObject(cityTowns);
				}
				return cityTowns;
			}
		}

		RefCityTownCollection cityTowns;

		#endregion
	}
}
