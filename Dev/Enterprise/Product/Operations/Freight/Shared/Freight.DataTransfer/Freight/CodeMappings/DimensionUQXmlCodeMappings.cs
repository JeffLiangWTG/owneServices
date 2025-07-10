using System.Collections.Generic;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.DataTransfer
{
	[Immutable]
	public class DimensionUQXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		DimensionUQXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.Length.Millimetres, "MM");
			yield return new Mapping(Core.Constants.Length.Centimetres, "CM");
			yield return new Mapping(Core.Constants.Length.Metres, "M");
			yield return new Mapping(Core.Constants.Length.Inches, "IN");
			yield return new Mapping(Core.Constants.Length.Feet, "FT");
			yield return new Mapping(Core.Constants.Length.Yards, "YD");
			yield return new Mapping(Core.Constants.Length.Kilometres, "KM");
			yield return new Mapping(Core.Constants.Length.Miles, "MI");
		}

		public static readonly DimensionUQXmlCodeMappings Instance = new DimensionUQXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("f016b2a2-ef5a-489e-99c5-813251e0ff36", "Dimension Unit"); }
		}
	}
}
