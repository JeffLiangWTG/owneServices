using CargoWise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.Business
{
	public static class GeographyType
	{
		public static CodeDescriptionPairList CodePairList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(NationsOfUK, Res.GetString("6121576C-53B8-4371-8A99-B375475CFBD1", "Nations of UK"));

				foreach (ICodeDescription userDefinedType in SystemDataRegistry.Instance.UserDefinedGeographyType.Value)
				{
					result.AddPair(userDefinedType.Code, userDefinedType.Description);
				}

				return result;
			}
		}

		public const string NationsOfUK = "UKN";
	}
}
