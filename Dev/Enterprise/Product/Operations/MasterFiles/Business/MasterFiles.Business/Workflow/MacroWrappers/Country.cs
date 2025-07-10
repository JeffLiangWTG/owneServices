using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class Country : ICountry
	{
		public Country(RefCountry country)
		{
			this.country = country;
		}

		readonly RefCountry country;

		public ZString Code
		{
			get
			{
				if (!code.HasValue)
				{
					code = country?.Code ?? ZString.Empty;
				}
				return code.Value;
			}
			set => code = value;
		}

		ZString? code;

		public ZString Name
		{
			get
			{
				if (!name.HasValue)
				{
					name = country?.Description ?? ZString.Empty;
				}
				return name.Value;
			}
			set => name = value;
		}

		ZString? name;

		public bool IsInEU => country?.IsPartOfEuropeanUnion ?? false;

		public bool IsInEFTA => country?.IsInEFTA ?? false;

		[MacroIgnore]
		public IRefCountryCollection Countries => countries ?? (countries = new RefCountryCollection(country?.Factory ?? new BusinessObjectFactory()));
		IRefCountryCollection countries;

		public override string ToString() => Code;
	}
}
