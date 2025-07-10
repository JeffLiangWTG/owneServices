using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Edifact.V902.Segments;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSDECMessageBuilder))]
sealed class CUSDECMessageBuilderTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		const MessageSubTypes messageSubType = MessageSubTypes.Create;
		var exception1 = AssertExceptionThrown<ArgumentNullException>("dataProvider is null", () =>
		{
			_ = new CUSDECMessageBuilder(null, messageSubType);
		});
		AssertEquals("dataProvider is null, ParamName", "dataProvider", exception1.ParamName);

		var dataProviderMock = new Mock<ICUSDECMessageDataProvider>();
		var exception2 = AssertExceptionThrown<ArgumentNullException>("collectionProvider is null", () =>
		{
			_ = new CUSDECMessageBuilder(dataProviderMock.Object, messageSubType);
		});
		AssertEquals("collectionProvider is null, ParamName", "collectionProvider", exception2.ParamName);

		dataProviderMock.Setup(x => x.AsMessageCollectionProvider())
			.Returns(Mock.Of<IEDIMessageCollectionProvider>);
		AssertNoExceptionThrown("happy path", () =>
		{
			_ = new CUSDECMessageBuilder(dataProviderMock.Object, messageSubType);
		});
	});

	public void TestAddNewDTMSegment_NoDateTime()
	{
		var dtmSection = new DTMSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewDTMSegment(dtmSection, ZString.Empty, DateTimePeriodQualifierList.ExpiryDate, DateTimePeriodFormatQualifierList.Yymmdd);
		AssertEquals(ZString.Empty, dtmSection.ToString(characterSet));
	}

	public void TestAddNewGISSegment_IfCustomsIndicator()
	{
		var gisSection = new GISSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewGISSegment(gisSection, true, CodeListQualifierList.CustomsIndicator, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
		AssertEquals("GIS+1:109:141'", gisSection.ToString(characterSet));
	}

	public void TestAddNewGISSegment_IfNotCustomsIndicatorAndIndicatorFalse()
	{
		var gisSection = new GISSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewGISSegment(gisSection, false, CodeListQualifierList.CustomsSpecialCodes, CodeListResponsibleAgencyCodedList.CecCommissionOfTheEuropeanCommunitiesDgXxiB1);
		AssertEquals("", gisSection.ToString(characterSet));
	}

	public void TestAddNewRFFSegment_NoSection()
	{
		AssertNoExceptionThrown(() => EDIFACTMessageSegmentBuilder.AddNewRFFSegment(null, ReferenceQualifierList.QuotaNumber, "REFNUM"));
	}

	public void TestAddNewRFFSegment_NoReferenceType()
	{
		var rffSection = new RFFSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewRFFSegment(rffSection, null, "REFNUM");
		AssertEquals("", rffSection.ToString(characterSet));
	}

	public void TestAddNewRFFSegment_NoReferenceNumber()
	{
		var rffSection = new RFFSegmentMessageSection(1);
		EDIFACTMessageSegmentBuilder.AddNewRFFSegment(rffSection, ReferenceQualifierList.QuotaNumber, ZString.Empty);
		AssertEquals("", rffSection.ToString(characterSet));
	}

	protected override void SetUp()
	{
		base.SetUp();
		characterSet = new NOCharacterSet();
	}
	NOCharacterSet characterSet;
}
