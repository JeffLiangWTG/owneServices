using System.IO;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.MasterFiles.Integration
{
	public interface IXmlValueObjectSerializer
	{
		void Serialize(TextWriter writer, IValueObject value);
	}
}
