using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.US.Business
{
	[CodeAlive("Will be deleted when the table USCGoldPrice is dropped from the US reference file")]
	public class USCGoldPrice : AutoUSCGoldPrice
	{
		public USCGoldPrice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
