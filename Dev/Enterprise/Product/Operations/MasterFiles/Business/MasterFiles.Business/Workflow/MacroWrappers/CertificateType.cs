using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros
{
	class CertificateType : DocumentVisualizer.DocDataObjects.ICodeDescription
	{
		public ZString Code { get; set; }
		public ZString Description { get; set; }

		public object Codes { get; set; }
	}
}
