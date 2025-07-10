
namespace Enterprise.Customs.US.InBond.Business.WarehouseExtensions
{
	public interface IInBondWarehouseIntegrationSupporter : US.Business.IInBondWarehouseIntegrationSupporter
	{
		bool IsExBondAutomationEnabled { get; }
	}
}
