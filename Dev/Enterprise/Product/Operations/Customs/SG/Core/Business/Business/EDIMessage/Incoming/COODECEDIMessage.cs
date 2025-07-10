using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class COODECEDIMessage : SGEDIMessage
	{
		public COODECEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get { return "CERTIFICATE OF ORIGIN APPLICATION"; }
		}
	}
}
