using System;
using System.Xml;

namespace CargoWise.RefDbRepo.UniversalXmlParser
{
	public interface IUniversalXmlTransformerValidator
	{
		TransformMode GetTransformMode(XmlDocument xmlDoc);
	}

	[Flags]
	public enum TransformMode
	{
		None = 0,
		Transform_RateAndApp = 1 << 0,
		Transform_CondAndApp = 1 << 1,
		Transform_KeepApp = 1 << 2
	}
}
