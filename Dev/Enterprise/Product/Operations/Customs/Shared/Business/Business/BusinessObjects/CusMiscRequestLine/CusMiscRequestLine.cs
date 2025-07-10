using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(CusMiscRequestHeader), "RequestLines")]
	public class CusMiscRequestLine : AutoCusMiscRequestLine
	{
		public CusMiscRequestLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static readonly CusMiscRequestLineTypeDecider TypeDecider = new CusMiscRequestLineTypeDecider();

		public CusMiscRequestHeader RequestHeader => Factory.Load<CusMiscRequestHeader>(CML_CMR);
	}
}
