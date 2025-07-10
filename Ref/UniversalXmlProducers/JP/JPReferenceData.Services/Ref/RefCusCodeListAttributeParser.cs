using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public abstract class RefCusCodeListAttributeParser
	{
		RefCusCodeListAttributeParserConfig[] configs;
		public RefCusCodeListAttributeParserConfig[] Configs
		{
			get
			{
				if (configs == null)
				{
					configs = GetConfigsCore();
				}
				return configs;
			}
		}
		protected abstract RefCusCodeListAttributeParserConfig[] GetConfigsCore();

		public bool TryAddRefCusCodeListAttribute(RefCusCodeList refCusCodeList, string[] columns)
		{
			var configs = Configs.Where(c => c.NeedAttributeInThisRow(columns)).ToArray();
			for (int i = 0; i < configs.Length; i++)
			{
				var refCusCodeListAttribute = new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = configs[i].ZZE_ZXE_NKName,
					ZZE_Value = configs[i].GetZZE_Value(columns)
				};

				if (configs[i].Validate(refCusCodeList, refCusCodeListAttribute))
				{
					refCusCodeList.RefCusCodeListAttributes[i] = refCusCodeListAttribute;
				}
				else
				{
					return false;
				}
			}

			return true;
		}
	}
}
