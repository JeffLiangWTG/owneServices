using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.MasterFiles.Business.Macros
{
	public class RegistrationNumberType : ICodeDescription
	{
		public ZString Code { get; set; }
		public ZString Description { get; set; }
		public object Codes { get; set; }
	}
}