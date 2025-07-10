using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class LineToPrint : AutoLineToPrint
	{
		protected LineToPrint(BusinessObject bizObj)
			: base(bizObj.Factory)
		{
			this.bizObj = bizObj;
			ShouldBePrinted = LastPrintDate.IsEmpty;
		}
		protected readonly BusinessObject bizObj;
	}
}
