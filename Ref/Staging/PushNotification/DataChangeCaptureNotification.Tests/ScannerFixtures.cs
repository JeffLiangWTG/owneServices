using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification.Tests
{
	[TestFixture]
	public class ScannerFixtures
	{
		[Test]
		public void IdentifyingRowAndSendingEmailAndDeletingRows()
		{
			using (var scanner = new Scanner(_sourceDataEmailMock.Object, _processorStatusEmailMock.Object, _stagingRepositoryMockErrorStatus.Object))
			{
				scanner.Run();
			}

			_processorStatusEmailMock.Verify(o => o.Send(It.IsAny<IEnumerable<ProcessorStatus>>()), Times.Once);
			_sourceDataEmailMock.Verify(o => o.Send(It.IsAny<IEnumerable<SourceData>>()), Times.Once);
			_stagingRepositoryMockErrorStatus.Verify(o => o.Remove(It.IsAny<DataChangeCapture>()), Times.AtMost(2));
			_stagingRepositoryMockErrorStatus.Verify(o => o.SaveChanges(), Times.Once);
		}

		[Test]
		public void DoNotIdentifyNewRowWhenItsNotStatusError()
		{
			using (var scanner = new Scanner(_sourceDataEmailMock.Object, _processorStatusEmailMock.Object, _stagingRepositoryMockMergedStatus.Object))
			{
				scanner.Run();
			}

			_processorStatusEmailMock.Verify(o => o.Send(It.IsAny<IEnumerable<ProcessorStatus>>()), Times.Never);
			_sourceDataEmailMock.Verify(o => o.Send(It.IsAny<IEnumerable<SourceData>>()), Times.Never);
			_stagingRepositoryMockMergedStatus.Verify(o => o.Remove(It.IsAny<DataChangeCapture>()), Times.Never);
			_stagingRepositoryMockMergedStatus.Verify(o => o.SaveChanges(), Times.Never);
		}

		[Test]
		public void GoesIntoSourceDataAndProcessorStatusOnlyOnce()
		{
			var stagingRepositoryMock = new Mock<IStagingRepository>();
			var dccProcessorPK = Guid.NewGuid();
			var dccProcessorPK2 = Guid.NewGuid();
			var dccSorceDataPK = Guid.NewGuid();
			var dccSorceDataPK2 = Guid.NewGuid();
			var dccListErrors = new List<DataChangeCapture>()
			{
				new DataChangeCapture()
				{
					DCC_Column = "PRC_ANY",
					DCC_EventTimeUTC = DateTime.UtcNow,
					DCC_NewValue = StatusProvider.GetERRStatus(),
					DCC_OldValue = "any",
					DCC_ParentCode = "any",
					DCC_ParentPK = dccProcessorPK,
					DCC_PK = Guid.NewGuid()
				},
				new DataChangeCapture()
				{
					DCC_Column = "PRC_ANY",
					DCC_EventTimeUTC = DateTime.UtcNow,
					DCC_NewValue = StatusProvider.GetERRStatus(),
					DCC_OldValue = "any",
					DCC_ParentCode = "any",
					DCC_ParentPK = dccProcessorPK2,
					DCC_PK = Guid.NewGuid()
				},
				new DataChangeCapture()
				{
					DCC_Column = "SDA_ANY",
					DCC_EventTimeUTC = DateTime.UtcNow,
					DCC_NewValue = StatusProvider.GetERRStatus(),
					DCC_OldValue = "any",
					DCC_ParentCode = "any",
					DCC_ParentPK = dccSorceDataPK,
					DCC_PK = Guid.NewGuid()
				},
				new DataChangeCapture()
				{
					DCC_Column = "SDA_ANY",
					DCC_EventTimeUTC = DateTime.UtcNow,
					DCC_NewValue = StatusProvider.GetERRStatus(),
					DCC_OldValue = "any",
					DCC_ParentCode = "any",
					DCC_ParentPK = dccSorceDataPK2,
					DCC_PK = Guid.NewGuid()
				}
			};
			var sourceDataList = new List<SourceData>()
			{
				new SourceData()
				{
					SDA_PK = dccSorceDataPK,
					SDA_Source = "INT",
					SDA_Filename = "any name",
					SDA_Filetype = "XML",
					SDA_SubSource = "any"
				},
				new SourceData()
				{
					SDA_PK = dccSorceDataPK2,
					SDA_Source = "INT",
					SDA_Filename = "any name 2",
					SDA_Filetype = "XML",
					SDA_SubSource = "any 2"
				}
			};
			var processorStatusList = new List<ProcessorStatus>()
			{
				new ProcessorStatus()
				{
					PRC_PK = dccProcessorPK,
					PRC_JobGroup = "AU",
					PRC_LastDataSetUpdatedTime = DateTime.UtcNow,
					PRC_LastRunTime = DateTime.UtcNow,
					PRC_LastSuccessRecordUpdatedCount = 1,
					PRC_LastSuccessRunTime = DateTime.UtcNow
				},
				new ProcessorStatus()
				{
					PRC_PK = dccProcessorPK2,
					PRC_JobGroup = "BR",
					PRC_LastDataSetUpdatedTime = DateTime.UtcNow,
					PRC_LastRunTime = DateTime.UtcNow,
					PRC_LastSuccessRecordUpdatedCount = 1,
					PRC_LastSuccessRunTime = DateTime.UtcNow
				}
			};

			stagingRepositoryMock.Setup(x => x.Get<DataChangeCapture>()).Returns(dccListErrors.AsQueryable());
			stagingRepositoryMock.Setup(x => x.Get<SourceData>()).Returns(sourceDataList.AsQueryable());
			stagingRepositoryMock.Setup(x => x.Get<ProcessorStatus>()).Returns(processorStatusList.AsQueryable());
			using (var scanner = new Scanner(_sourceDataEmailMock.Object, _processorStatusEmailMock.Object, stagingRepositoryMock.Object))
			{
				scanner.Run();
			}

			stagingRepositoryMock.Verify(x => x.Get<SourceData>(), Times.Once);
			stagingRepositoryMock.Verify(x => x.Get<ProcessorStatus>(), Times.Once);
		}

		[SetUp]
		public void SetUp()
		{
			_stagingRepositoryMockErrorStatus = new Mock<IStagingRepository>();
			_stagingRepositoryMockMergedStatus = new Mock<IStagingRepository>();
			_processorStatusEmailMock = new Mock<Email<ProcessorStatus>>("anysmtp", It.IsAny<ICredentialsByHost>(), 25);
			_sourceDataEmailMock = new Mock<Email<SourceData>>("anysmtp", It.IsAny<ICredentialsByHost>(), 25);

			var dccProcessorPK = Guid.NewGuid();
			var dccSorceDataPK = Guid.NewGuid();
			var dccListErrors = new List<DataChangeCapture>()
			{
				new DataChangeCapture()
				{
					DCC_Column = "PRC_ANY",
					DCC_EventTimeUTC = DateTime.UtcNow,
					DCC_NewValue = StatusProvider.GetERRStatus(),
					DCC_OldValue = "any",
					DCC_ParentCode = "any",
					DCC_ParentPK = dccProcessorPK,
					DCC_PK = Guid.NewGuid()
				},
				new DataChangeCapture()
				{
					DCC_Column = "SDA_ANY",
					DCC_EventTimeUTC = DateTime.UtcNow,
					DCC_NewValue = StatusProvider.GetERRStatus(),
					DCC_OldValue = "any",
					DCC_ParentCode = "any",
					DCC_ParentPK = dccSorceDataPK,
					DCC_PK = Guid.NewGuid()
				}
			};

			var dccListMerged = new List<DataChangeCapture>()
			{
				new DataChangeCapture()
				{
					DCC_Column = "PRC_ANY",
					DCC_EventTimeUTC = DateTime.UtcNow,
					DCC_NewValue = StatusProvider.GetMERStatus(),
					DCC_OldValue = "any",
					DCC_ParentCode = "any",
					DCC_ParentPK = dccProcessorPK,
					DCC_PK = Guid.NewGuid()
				},
				new DataChangeCapture()
				{
					DCC_Column = "SDA_ANY",
					DCC_EventTimeUTC = DateTime.UtcNow,
					DCC_NewValue = StatusProvider.GetMERStatus(),
					DCC_OldValue = "any",
					DCC_ParentCode = "any",
					DCC_ParentPK = dccSorceDataPK,
					DCC_PK = Guid.NewGuid()
				}
			};

			var processorStatusList = new List<ProcessorStatus>()
			{
				new ProcessorStatus()
				{
					PRC_PK = dccProcessorPK,
					PRC_JobGroup = "AU",
					PRC_LastDataSetUpdatedTime = DateTime.UtcNow,
					PRC_LastRunTime = DateTime.UtcNow,
					PRC_LastSuccessRecordUpdatedCount = 1,
					PRC_LastSuccessRunTime = DateTime.UtcNow
				}
			};

			var sourceDataList = new List<SourceData>()
			{
				new SourceData()
				{
					SDA_PK = dccSorceDataPK,
					SDA_Source = "INT",
					SDA_Filename = "any name",
					SDA_Filetype = "XML",
					SDA_SubSource = "any"
				}
			};

			_stagingRepositoryMockErrorStatus.Setup(x => x.Get<DataChangeCapture>()).Returns(dccListErrors.AsQueryable());
			_stagingRepositoryMockErrorStatus.Setup(x => x.Get<SourceData>()).Returns(sourceDataList.AsQueryable());
			_stagingRepositoryMockErrorStatus.Setup(x => x.Get<ProcessorStatus>()).Returns(processorStatusList.AsQueryable());
			_stagingRepositoryMockErrorStatus.Setup(o => o.Remove(typeof(DataChangeCapture))).Verifiable();

			_stagingRepositoryMockMergedStatus.Setup(x => x.Get<DataChangeCapture>()).Returns(dccListMerged.AsQueryable());
			_stagingRepositoryMockMergedStatus.Setup(x => x.Get<SourceData>()).Returns(sourceDataList.AsQueryable());
			_stagingRepositoryMockMergedStatus.Setup(x => x.Get<ProcessorStatus>()).Returns(processorStatusList.AsQueryable());
			_stagingRepositoryMockMergedStatus.Setup(o => o.Remove(typeof(DataChangeCapture))).Verifiable();

			_processorStatusEmailMock.Setup(x => x.Send(processorStatusList)).Verifiable();
			_processorStatusEmailMock.Setup(x => x.GetContacts(It.IsAny<IEnumerable<ProcessorStatus>>())).Returns(new string[] { "any@any.com" });
			_sourceDataEmailMock.Setup(x => x.Send(sourceDataList)).Verifiable();
			_sourceDataEmailMock.Setup(x => x.GetContacts(It.IsAny<IEnumerable<SourceData>>())).Returns(new string[] { "any@any.com" });
		}

		Mock<IStagingRepository> _stagingRepositoryMockErrorStatus;
		Mock<IStagingRepository> _stagingRepositoryMockMergedStatus;
		Mock<Email<ProcessorStatus>> _processorStatusEmailMock;
		Mock<Email<SourceData>> _sourceDataEmailMock;
	}
}
