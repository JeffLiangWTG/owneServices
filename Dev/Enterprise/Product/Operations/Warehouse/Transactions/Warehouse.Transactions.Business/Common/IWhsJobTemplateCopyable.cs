using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IWhsJobTemplateCopyable : ITemplateCopyable
	{
		IBusiness TemplateCopyWithoutLines();
	}
}
