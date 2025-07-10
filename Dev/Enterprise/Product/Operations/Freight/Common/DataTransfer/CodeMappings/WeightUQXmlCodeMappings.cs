using System.Collections.Generic;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Common.DataTransfer
{
	[Immutable]
	public class WeightUQXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		WeightUQXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.Weight.Decitons, "DT");
			yield return new Mapping(Core.Constants.Weight.Grams, "G");
			yield return new Mapping(Core.Constants.Weight.Hectograms, "HG");
			yield return new Mapping(Core.Constants.Weight.Kilograms, "KG");
			yield return new Mapping(Core.Constants.Weight.Kilotonnes, "KT");
			yield return new Mapping(Core.Constants.Weight.LongTons, "TL");
			yield return new Mapping(Core.Constants.Weight.MetricCarat, "MC");
			yield return new Mapping(Core.Constants.Weight.Milligrams, "MG");
			yield return new Mapping(Core.Constants.Weight.Ounces, "OZ");
			yield return new Mapping(Core.Constants.Weight.OuncesTroy, "OT");
			yield return new Mapping(Core.Constants.Weight.Pounds, "LB");
			yield return new Mapping(Core.Constants.Weight.PoundsTroy, "LT");
			yield return new Mapping(Core.Constants.Weight.ShortTons, "TN");
			yield return new Mapping(Core.Constants.Weight.Tonnes, "T");
		}

		public static readonly WeightUQXmlCodeMappings Instance = new WeightUQXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("08f5a182-c664-425e-99fd-e527bf8fe922", "Weight Unit"); }
		}
	}
}
