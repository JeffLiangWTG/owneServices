using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public class PortMessageIssue : AutoPortMessageIssue
	{
		public PortMessageIssue(ZGuid targetPK, ZString targetCode, ZString text, ZString detail)
			: base(targetPK, targetCode, text, detail) { }
	}
}



