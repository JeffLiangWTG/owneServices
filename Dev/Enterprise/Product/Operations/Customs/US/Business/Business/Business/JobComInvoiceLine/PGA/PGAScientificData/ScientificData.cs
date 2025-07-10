using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ScientificData : AutoScientificData, IScientificData
	{
		public ScientificData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USScientificDataAddInfoLookups.USCountryList))]
		public override ZString US_PGACountryCode
		{
			get { return base.US_PGACountryCode; }
			set { base.US_PGACountryCode = value; }
		}

		#region IScientificData Members

		ZString IScientificData.CountryCode
		{
			get { return US_PGACountryCode; }
		}

		ZString IScientificData.GenusName
		{
			get { return US_PGAScientificGenusName; }
		}

		ZString IScientificData.SpeciesName
		{
			get { return US_PGAScientificSpeciesName; }
		}

		ZString IScientificData.SubSpeciesName
		{
			get { return US_SubSpeciesName; }
		}

		ZString IScientificData.SpeciesCode
		{
			get { return US_SpeciesCode; }
		}

		#endregion
	}
}
