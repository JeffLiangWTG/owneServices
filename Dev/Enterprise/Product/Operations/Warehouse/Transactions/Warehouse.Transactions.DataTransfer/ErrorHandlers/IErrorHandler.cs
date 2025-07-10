using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public interface IErrorHandler
	{
		void SetDocketTypeAndLogDataErrors(BusinessObject docket, IValueObject value);
		void SetDocketTypeAndLogDataErrorsFromLine(BusinessObject docket, BusinessObject line, IValueObject value);
	}
}
