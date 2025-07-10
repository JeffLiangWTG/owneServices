using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class JobDateProviderRatingXmlSerializerTest : TestCase
	{
		public void TestSerializeTransitTime()
		{
			IJobDatesProvider getJobDatesProviderWithTransitTime(string transitTime)
			{
				var mockedProvider = new Mock<IJobDatesProvider>();
				mockedProvider.Setup(m => m.TransitTime).Returns(transitTime);
				return mockedProvider.Object;
			}

			var jobDatesProvider = getJobDatesProviderWithTransitTime("5");
			var serializedString = JobDateProviderRatingXmlSerializer.Serialize(jobDatesProvider);
			AssertEquals(true, serializedString.Contains("Transit Time: 5"));

			jobDatesProvider = getJobDatesProviderWithTransitTime("0");
			serializedString = JobDateProviderRatingXmlSerializer.Serialize(jobDatesProvider);
			AssertEquals(true, serializedString.Contains("Transit Time: 0"));

			jobDatesProvider = getJobDatesProviderWithTransitTime("");
			serializedString = JobDateProviderRatingXmlSerializer.Serialize(jobDatesProvider);
			AssertEquals(true, serializedString.Contains("Transit Time: N/A"));
		}

		public void TestSerialize_AllDateTypesShouldBeIncluded()
		{
			var mockJobDatesProvider = new Mock<IJobDatesProvider>();
			mockJobDatesProvider.Setup(m => m.TransitTime).Returns("5");
			var serializedString = JobDateProviderRatingXmlSerializer.Serialize(mockJobDatesProvider.Object);

			CombineAssertions("All JobDateTypes should be serialized", () =>
			{
				foreach (var jobDateType in JobDateTypes.JobDateTypeList.OfType<CodeDescriptionPair>())
				{
					Assert($"{jobDateType.Description} should be included but it wasn't", serializedString.Contains(jobDateType.Description));
				}
			});
		}
	}
}
