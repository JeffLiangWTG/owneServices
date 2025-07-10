
using System.Reflection;

namespace CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions
{
	public sealed class CusConditionCodes
	{
		public static RefCusCodeConditionItems GetCusConditionData()
		{
			return XmlHelper.ReadDeserializedManifestResourceContent<RefCusCodeConditionItems>("CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions.RefCusConditions.xml");
		}
	}
}
