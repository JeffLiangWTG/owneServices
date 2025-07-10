using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Modules.DocumentScanning
{
	public interface IEDocsViaUniversalXmlSupport
	{
		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code);

		public ZString ExpectedCodeFormat { get; }
		public ZString ExampleCodeFormat { get; }
	}
}
