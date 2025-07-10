using System.Collections.Generic;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Common.DataTransfer
{
	[Immutable]
	public class VolumeUQXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		VolumeUQXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.Volume.CubicMetres, "M3");
			yield return new Mapping(Core.Constants.Volume.CubicDecimetres, "D3");
			yield return new Mapping(Core.Constants.Volume.CubicFeet, "CF");
			yield return new Mapping(Core.Constants.Volume.CubicYards, "CY");
			yield return new Mapping(Core.Constants.Volume.CubicInches, "CI");
			yield return new Mapping(Core.Constants.Volume.Litre, "L");
			yield return new Mapping(Core.Constants.Volume.MegaLitre, "ML");
			yield return new Mapping(Core.Constants.Volume.TeaChest, "TE");
			yield return new Mapping(Core.Constants.Volume.CubicCentimeters, "CC");
			yield return new Mapping(Core.Constants.Volume.USGallons, "GA");
			yield return new Mapping(Core.Constants.Volume.ImperialGallons, "GI");
		}

		public static readonly VolumeUQXmlCodeMappings Instance = new VolumeUQXmlCodeMappings();

		protected override string Name
		{
			get { return Res.GetString("f3f2e168-9b9b-484a-9ad3-a9691a03c4f4", "Volume Unit"); }
		}
	}
}
