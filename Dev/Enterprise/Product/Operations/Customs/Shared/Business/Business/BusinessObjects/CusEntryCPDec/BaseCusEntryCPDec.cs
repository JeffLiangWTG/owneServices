using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class BaseCusEntryCPDec : AutoCusEntryCPDec
	{
		public BaseCusEntryCPDec(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
