using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	public class Scanner : IDisposable
	{
		readonly Email<SourceData> _sourceDataEmailFactory;
		readonly Email<ProcessorStatus> _processorStatusEmailFactory;
		readonly IStagingRepository _stagingRepo;
		List<DataChangeCapture> DataChangeCaptures = new List<DataChangeCapture>();

		public Scanner(Email<SourceData> sourceDataEmailFactory, Email<ProcessorStatus> processorStatusEmailFactory, IStagingRepository stagingRepo)
		{
			Argument.NotNull(sourceDataEmailFactory, nameof(sourceDataEmailFactory));
			Argument.NotNull(stagingRepo, nameof(stagingRepo));
			Argument.NotNull(processorStatusEmailFactory, nameof(processorStatusEmailFactory));

			_sourceDataEmailFactory = sourceDataEmailFactory;
			_processorStatusEmailFactory = processorStatusEmailFactory;
			_stagingRepo = stagingRepo;
		}

		(List<SourceData> sourceDatas, List<ProcessorStatus> processorStatuses) ScanTable()
		{
			DataChangeCaptures = _stagingRepo.Get<DataChangeCapture>().Where(o => StatusProvider.GetDataChangeCaptureErrorStatuses().Contains(o.DCC_NewValue)).ToList();

			return (ScanRelatedRecords<SourceData>("SDA"),
				ScanRelatedRecords<ProcessorStatus>("PRC"));
		}

		List<T> ScanRelatedRecords<T>(string dccColumnName) where T : class
		{
			var dcc = DataChangeCaptures.Where(x => x.DCC_Column.StartsWith(dccColumnName, StringComparison.OrdinalIgnoreCase));
			if (dcc.Any())
			{
				var dccParentPksAsArray = dcc.Select(x => x.DCC_ParentPK).ToList();
				var containsExpression = ExpressionHelper.ContainsStructPropertyExpression<T, Guid>(dccParentPksAsArray, typeof(T).GetPKPropertyInfo());
				return _stagingRepo.Get<T>().Where(containsExpression).ToList();
			}
			return new List<T>();
		}

		public void Run()
		{
			var (sourceDatas, processorStatuses) = ScanTable();

			if (processorStatuses.Any())
			{
				_processorStatusEmailFactory.Send(processorStatuses);
			}

			if (sourceDatas.Any())
			{
				_sourceDataEmailFactory.Send(sourceDatas);
			}

			CleanDataFromDb();
		}

		void CleanDataFromDb()
		{
			if (DataChangeCaptures.Any())
			{
				DataChangeCaptures.ForEach(d => _stagingRepo.Remove(d));
				_stagingRepo.SaveChanges();
			}
		}

		public void Dispose()
		{
			_stagingRepo.Dispose();
		}
	}
}
