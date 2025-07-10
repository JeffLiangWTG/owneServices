using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.PreDriveChecklist;

namespace Enterprise.Telematics.ServiceTasks.Test.PreDriveChecklist
{
	class TelematicsChecklistAccessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			checklistAccessor = new TelematicsChecklistAccessor();
		}

		public void TestChecklistAccessorRetrievesUnprocessedChecklistResults()
		{
			CombineAssertions(() =>
			{
				AssertExpectedChecklistsRetrievedFromAccessor(Enumerable.Empty<TelPreDriveChecklistHeader>());

				var unprocessedHeader1 = CreateHeader(false, "~BP", TelPreDriveChecklistHeaderTypes.Codes.PDR, ZDateTime.Now.AddDays(-10));
				AssertExpectedChecklistsRetrievedFromAccessor(new[] { unprocessedHeader1 });

				var unProcessedHeader2 = CreateHeader(false, "E", TelPreDriveChecklistHeaderTypes.Codes.PDR, ZDateTime.Now.AddDays(-8));
				AssertExpectedChecklistsRetrievedFromAccessor(new[] { unprocessedHeader1, unProcessedHeader2 });

				var unProcessedHeader3 = CreateHeader(false, "E", TelPreDriveChecklistHeaderTypes.Codes.FTD, ZDateTime.Now.AddDays(-7));
				AssertExpectedChecklistsRetrievedFromAccessor(new[] { unprocessedHeader1, unProcessedHeader2, unProcessedHeader3 });
			});
		}

		public void TestChecklistAccessorDoesNotRetrieveProcessedChecklistResults()
		{
			CombineAssertions(() =>
			{
				AssertExpectedChecklistsRetrievedFromAccessor(Enumerable.Empty<TelPreDriveChecklistHeader>());

				CreateHeader(true, "~BP", TelPreDriveChecklistHeaderTypes.Codes.FTD, ZDateTime.Now.AddDays(-9));
				AssertExpectedChecklistsRetrievedFromAccessor(Enumerable.Empty<TelPreDriveChecklistHeader>());

				CreateHeader(true, "~BP", TelPreDriveChecklistHeaderTypes.Codes.PDR, ZDateTime.Now.AddDays(-5));
				CreateHeader(true, "E", TelPreDriveChecklistHeaderTypes.Codes.FTD, ZDateTime.Now.AddDays(-5));
				AssertExpectedChecklistsRetrievedFromAccessor(Enumerable.Empty<TelPreDriveChecklistHeader>());
			});
		}

		void AssertExpectedChecklistsRetrievedFromAccessor(IEnumerable<TelPreDriveChecklistHeader> expectedChecklists)
		{
			// Arrange
			// Act
			var result = checklistAccessor.GetChecklists(Factory, CancellationToken.None);

			// Assert
			AssertContainsExactElementsInAnyOrder(expectedChecklists, result);
		}

		TelPreDriveChecklistHeader CreateHeader(bool isProcessed, string driver, string type, ZDateTime time)
		{
			var header = Factory.New<TelPreDriveChecklistHeader>();
			header.TPH_IsProcessed = isProcessed;
			header.TPH_GS_NKDriver = driver;
			header.TPH_Type = type;
			header.TPH_SystemCreateTimeUtc = ZDateTime.MinSmallDateTimeValue;
			header.TPH_ChecklistCreateTimeUtc = time;
			return header;
		}

		IChecklistAccessor checklistAccessor;
	}
}
