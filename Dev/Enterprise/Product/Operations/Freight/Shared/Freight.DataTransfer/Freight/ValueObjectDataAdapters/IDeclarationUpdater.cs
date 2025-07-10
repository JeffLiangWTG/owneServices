using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public interface IDeclarationUpdater
	{
		void Update(BusinessObject bizObj, Xsd.CartageJob xsdCartage, IValueObjectImportContext context);
	}
}
