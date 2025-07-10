using System.Xml.Serialization;

namespace NetCore.PackageCopier;

/// <summary>
/// A class to avoid build failures. PackageCopier will reference all dependencies
/// in every NetCore project, which includes `CargoWise.XmlSerializer.Generator`.
/// Without this class the build will fail because the generator requires at least
/// one serialisable class.
/// </summary>
[XmlSerializerAssembly("NetCore.PackageCopier.XmlSerializers")]
public class TokenXmlSerialisable
{    
}