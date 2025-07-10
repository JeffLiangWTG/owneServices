using CargoWise.Common;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	sealed class UCMPBusinessObjectBinding : IHostedServiceBusinessObjectBinding
	{
		public UCMPBusinessObjectBinding(string table, string serviceTaskCode, string queueName, string[] predicates)
		{
			this.table = Argument.NotNullOrEmpty(table, nameof(table));
			this.serviceTaskCode = Argument.NotNullOrEmpty(serviceTaskCode, nameof(serviceTaskCode));
			this.queueName = Argument.NotNullOrEmpty(queueName, nameof(queueName));
			this.predicates = Argument.NotNull(predicates, nameof(predicates));
		}

		readonly string table;
		readonly string serviceTaskCode;
		readonly string queueName;
		readonly string[] predicates;

		string IHostedServiceBusinessObjectBinding.Table => table;

		string[] IHostedServiceBusinessObjectBinding.Predicates => predicates;

		string IHostedServiceBusinessObjectBinding.ServiceTaskCode => serviceTaskCode;

		string IHostedServiceBusinessObjectBinding.QueueName => queueName;
	}
}
