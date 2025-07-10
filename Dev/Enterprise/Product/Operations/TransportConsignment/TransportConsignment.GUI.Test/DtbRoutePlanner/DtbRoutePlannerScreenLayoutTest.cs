using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public sealed class DtbRoutePlannerScreenLayoutTest : TestCase
	{
		const string _xmlGeneratedWhenTypeWasNestedInsideDtbRoutePlannerForm = @"<?xml version=""1.0""?>
<DtbRoutePlannerScreenLayout xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <PlannerViewMode>Direct</PlannerViewMode>
  <RunSheetView>Carriers</RunSheetView>
  <RunSheetDay>Today</RunSheetDay>
  <CustomDate>2010-01-01</CustomDate>
  <IsConsignmentDetailsButtonChecked>true</IsConsignmentDetailsButtonChecked>
  <IsRunSheetDetailsButtonChecked>false</IsRunSheetDetailsButtonChecked>
  <IsFiltersButtonChecked>true</IsFiltersButtonChecked>
  <IsRunSheetsArrowButtonChecked>true</IsRunSheetsArrowButtonChecked>
  <ConsignmentHorizontalSplitterPosition>1</ConsignmentHorizontalSplitterPosition>
  <ConsignmentVerticalSplitterPosition>2</ConsignmentVerticalSplitterPosition>
  <RunSheetVerticalSplitterPosition>4</RunSheetVerticalSplitterPosition>
  <RunSheetHorizontalSplitterPosition>3</RunSheetHorizontalSplitterPosition>
</DtbRoutePlannerScreenLayout>";

		DtbRoutePlannerScreenLayout _layout;

		protected override void SetUp()
		{
			base.SetUp();

			_layout = new DtbRoutePlannerScreenLayout
			{
				PlannerViewMode = DtbRoutePlannerViewMode.Direct,
				RunSheetView = RunSheetView.Carriers,
				RunSheetDay = RunSheetDay.Today,
				CustomDate = new ZDate(2010, 1, 1),
				IsConsignmentDetailsButtonChecked = true,
				IsRunSheetDetailsButtonChecked = false,
				IsFiltersButtonChecked = true,
				IsRunSheetsArrowButtonChecked = true,
				ConsignmentHorizontalSplitterPosition = 1,
				ConsignmentVerticalSplitterPosition = 2,
				RunSheetVerticalSplitterPosition = 4,
				RunSheetHorizontalSplitterPosition = 3,
			};
		}

		public void TestCanDeserialiseXmlGeneratedWhenTypeWasNestedInDtbRoutePlannerForm()
		{
			using (var xmlReader = new StringReader(_xmlGeneratedWhenTypeWasNestedInsideDtbRoutePlannerForm))
			{
				var deserialised = (DtbRoutePlannerScreenLayout)ZXmlSerializer.New(typeof(DtbRoutePlannerScreenLayout)).Deserialize(xmlReader);

				AssertEquals(nameof(_layout.PlannerViewMode), _layout.PlannerViewMode, deserialised.PlannerViewMode);
				AssertEquals(nameof(_layout.RunSheetView), _layout.RunSheetView, deserialised.RunSheetView);
				AssertEquals(nameof(_layout.RunSheetDay), _layout.RunSheetDay, deserialised.RunSheetDay);
				AssertEquals(nameof(_layout.CustomDate), _layout.CustomDate, deserialised.CustomDate);
				AssertEquals(nameof(_layout.IsConsignmentDetailsButtonChecked), _layout.IsConsignmentDetailsButtonChecked, deserialised.IsConsignmentDetailsButtonChecked);
				AssertEquals(nameof(_layout.IsRunSheetDetailsButtonChecked), _layout.IsRunSheetDetailsButtonChecked, deserialised.IsRunSheetDetailsButtonChecked);
				AssertEquals(nameof(_layout.IsFiltersButtonChecked), _layout.IsFiltersButtonChecked, deserialised.IsFiltersButtonChecked);
				AssertEquals(nameof(_layout.IsRunSheetsArrowButtonChecked), _layout.IsRunSheetsArrowButtonChecked, deserialised.IsRunSheetsArrowButtonChecked);
				AssertEquals(nameof(_layout.ConsignmentHorizontalSplitterPosition), _layout.ConsignmentHorizontalSplitterPosition, deserialised.ConsignmentHorizontalSplitterPosition);
				AssertEquals(nameof(_layout.ConsignmentVerticalSplitterPosition), _layout.ConsignmentVerticalSplitterPosition, deserialised.ConsignmentVerticalSplitterPosition);
				AssertEquals(nameof(_layout.RunSheetVerticalSplitterPosition), _layout.RunSheetVerticalSplitterPosition, deserialised.RunSheetVerticalSplitterPosition);
				AssertEquals(nameof(_layout.RunSheetHorizontalSplitterPosition), _layout.RunSheetHorizontalSplitterPosition, deserialised.RunSheetHorizontalSplitterPosition);
			}
		}

		public void TestSerialisedXmlMatchesXmlGeneratedWhenTypeWasNestedInDtbRoutePlannerForm()
		{
			var expectedXml = _xmlGeneratedWhenTypeWasNestedInsideDtbRoutePlannerForm;

			var serializer = ZXmlSerializer.New(_layout.GetType());

			using var stream = new MemoryStream();
			serializer.Serialize(stream, _layout);

			var bytes = stream.ToArray();
			var actualXml = Encoding.UTF8.GetString(bytes);

			XmlComparison.CompareAndAssertXml(expectedXml, actualXml, "generated xml does not match expected xml");
		}
	}
}
