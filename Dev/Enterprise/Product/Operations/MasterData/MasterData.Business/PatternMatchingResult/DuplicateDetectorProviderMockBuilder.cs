#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.MasterData.Business
{
	public static class DuplicateDetectorProviderMockBuilder
	{
		public static Mock<T> New<T>()
			where T : class
		{
			return new Mock<T>();
		}

		public static Mock<IDuplicateDetectorProvider> WithOutputs(
			this Mock<IDuplicateDetectorProvider> mockDuplicateDetectorProvider,
			Guid masterPk,
			IEnumerable<DuplicateDetectorProviderOutput> detectorOutputs)
		{
			mockDuplicateDetectorProvider
				.Setup(mock => mock.DetectDuplicates(It.Is<OrgHeader>(org => org.PK.ToGuid() == masterPk)))
				.Returns(detectorOutputs)
				.Verifiable();

			return mockDuplicateDetectorProvider;
		}

		public static Mock<IDuplicateDetectorProvider> ThrowsException(
			this Mock<IDuplicateDetectorProvider> mockDuplicateDetectorProvider,
			Guid masterPk,
			Exception ex)
		{
			mockDuplicateDetectorProvider
				.Setup(mock => mock.DetectDuplicates(It.Is<OrgHeader>(org => org.PK.ToGuid() == masterPk)))
				.Throws(ex)
				.Verifiable();

			return mockDuplicateDetectorProvider;
		}

		public static Mock<IDuplicateDetectorProvider> SetTimeOut(
			this Mock<IDuplicateDetectorProvider> mockProvider)
		{
			mockProvider
				.Setup(mock => mock.DetectDuplicates(It.IsAny<OrgHeader>()))
				.Returns(Enumerable.Empty<DuplicateDetectorProviderOutput>())
				.Verifiable();
			mockProvider.SetupGet(mock => mock.LastRunStatus).Returns(DuplicationStatus.Timeout);

			return mockProvider;
		}

		public static Mock<IDuplicateDetectorProvider> WithPersonOutputs(
			this Mock<IDuplicateDetectorProvider> mockDuplicateDetectorProvider,
			Guid masterPk,
			IEnumerable<DuplicateDetectorProviderOutput> detectorOutputs)
		{
			mockDuplicateDetectorProvider
				.Setup(mock => mock.DetectDuplicates(It.Is<GlbPerson>(per => per.PK.ToGuid() == masterPk)))
				.Returns(detectorOutputs)
				.Verifiable();

			return mockDuplicateDetectorProvider;
		}

		public static Mock<IDuplicateDetectorProvider> ThrowsExceptionForPerson(
			this Mock<IDuplicateDetectorProvider> mockDuplicateDetectorProvider,
			Guid masterPk,
			Exception ex)
		{
			mockDuplicateDetectorProvider
				.Setup(mock => mock.DetectDuplicates(It.Is<GlbPerson>(per => per.PK.ToGuid() == masterPk)))
				.Throws(ex)
				.Verifiable();

			return mockDuplicateDetectorProvider;
		}

		public static Mock<IDuplicateDetectorProvider> SetPersonTimeOut(
			this Mock<IDuplicateDetectorProvider> mockProvider)
		{
			mockProvider
				.Setup(mock => mock.DetectDuplicates(It.IsAny<GlbPerson>()))
				.Returns(Enumerable.Empty<DuplicateDetectorProviderOutput>())
				.Verifiable();
			mockProvider.SetupGet(mock => mock.LastRunStatus).Returns(DuplicationStatus.Timeout);

			return mockProvider;
		}
	}
}

#endif
