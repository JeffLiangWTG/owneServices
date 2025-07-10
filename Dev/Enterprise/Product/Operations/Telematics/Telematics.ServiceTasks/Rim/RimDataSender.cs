using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business.Registry;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	class RimDataSender
	{
		public RimDataSender(ILogger logger)
			: this(
				new BusinessObjectFactory { NameForDebugging = "Telematics RIM data sending task" },
				new DataAccessor(logger),
				new DataProcessor(new RimDataRecordNumberStrategy()),
				new DataSender(new EHubMessageSender()))
		{
		}

		internal RimDataSender(BusinessObjectFactory factory, IDataAccessor dataAccessor, IDataProcessor dataProcessor, IDataSender dataSender)
		{
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
			this.dataAccessor = dataAccessor ?? throw new ArgumentNullException(nameof(dataAccessor));
			this.dataProcessor = dataProcessor ?? throw new ArgumentNullException(nameof(dataProcessor));
			this.dataSender = dataSender ?? throw new ArgumentNullException(nameof(dataSender));
		}

		public void Run(CancellationToken cancellationToken)
		{
			using (var transaction = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
			{
				var deviceData = dataAccessor.GetData(factory);
				var portionedData = dataProcessor.Process(factory, deviceData, TelematicsConfigurationRegistry.Instance.RimMaximumDataBatchSize.Value, cancellationToken);
				dataSender.Send(factory, portionedData);
				factory.Save();
				transaction.CommitTransaction();
			}
		}

		readonly IDataAccessor dataAccessor;
		readonly IDataProcessor dataProcessor;
		readonly IDataSender dataSender;
		readonly BusinessObjectFactory factory;
	}
}
