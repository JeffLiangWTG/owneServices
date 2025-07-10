using CargoWise.RefDbRepo.AUReferenceData.Business;
using Moq;
using System;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	sealed class RefundReasonCodesParserTest : CMRReferenceDataParserAbstractTest
	{
		protected override string TestFileFolderName => "RefundReasonCodes";

		protected override string TextFileName => "RFNRSNTP-P1-EDMAIN-202408050330.txt";

		protected override string XMLFileName => "RefCusCodeList_AU_RefundReasonCodes.xml";

		protected override DateTime PublishedDate => new DateTime(2024, 08, 05, 03, 30, 00);

		protected override ICMRDataParser Parser
		{
			get
			{
				var mockDateTimeProvider = new Mock<IDateTimeProvider>();
				mockDateTimeProvider.Setup(x => x.CurrentLocalDateTime).Returns(new DateTime(2024, 07, 28, 20, 48, 36));
				return new RefundReasonCodesParser(mockDateTimeProvider.Object);
			}
		}
	}
}
