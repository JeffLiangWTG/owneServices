using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsOrigin : NatureAndQtyOfGoods
	{
		public NatureAndQtyOfGoodsOrigin(ExportAWBRateLine parentRateLine)
			: base(parentRateLine)
		{
		}

		public static new class Schema
		{
			public const string Country = "Country";
		}

		[List("CountryList")]
		[BusinessObjectTestExclude]
		public ZString Country
		{
			get { return country; }
			set
			{
				if (SetNonPersistentPropertyValue(CountryInfo, ref country, value) && !IsValidationSuspended)
				{
					Validation.ValidateCountry();
				}
			}
		}
		ZString country;

		public RefCountryCollection CountryList
		{
			get
			{
				if (countryList == null)
				{
					countryList = new RefCountryCollection(Factory);
				}
				return countryList;
			}
		}
		RefCountryCollection countryList;

		public ZPropertyInfo CountryInfo
		{
			get { return GetZPropertyInfo(Schema.Country); }
		}

		protected override ZString TextCore
		{
			get { return Serialize(); }
			set { Deserialize(value); }
		}

		static readonly Lazy<Regex> regex = new Lazy<Regex>(() => new Regex(@"^Goods Origin: (?<Country>[0-9a-zA-Z]{2})$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

		protected ZString Serialize()
		{
			if (RefCountry.LoadFromCountryCode(Factory, Country) != null)
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"Goods Origin: {0}", Country); // SLAC Serialization
			}
			return ZString.Empty;
		}

		protected void Deserialize(ZString text)
		{
			var match = regex.Value.Match(text);
			Country = match.Success ? new ZString(match.Groups["Country"].Value) : ZString.Empty;
		}

		public static bool IsValidOrigin(ZString text, BusinessObjectFactory factory)
		{
			var match = regex.Value.Match(text);
			return match.Success && RefCountry.LoadFromCountryCode(factory, match.Groups["Country"].Value) != null;
		}

		public new NatureAndQtyOfGoodsOriginValidation Validation
		{
			get { return (NatureAndQtyOfGoodsOriginValidation)base.Validation; }
		}

		protected override NatureAndQtyOfGoodsValidation GetValidation()
		{
			return new NatureAndQtyOfGoodsOriginValidation(this);
		}
	}
}
