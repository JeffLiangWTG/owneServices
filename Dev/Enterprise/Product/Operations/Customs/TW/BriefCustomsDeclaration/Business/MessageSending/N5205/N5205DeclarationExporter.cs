using CargoWise.Types;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class N5205DeclarationExporter : BCDExporter
	{
		public N5205DeclarationExporter(AsycudaBill masterBill) : base(masterBill)
		{
		}

		protected override ZString GetCustomsControlIDCore() => MasterBill.IsSea ? base.GetCustomsControlIDCore() : ZString.Empty;
	}
}
