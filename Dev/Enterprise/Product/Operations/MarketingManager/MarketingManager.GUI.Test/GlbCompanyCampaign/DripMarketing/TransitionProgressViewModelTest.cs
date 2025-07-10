using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using OxyPlot;
using static Enterprise.MarketingManager.GUI.TransitionProgressViewModelDataAdapter;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TransitionProgressViewModelTest : TestCaseWithFactory
	{
		public void TestTouchStatsCategoryAxis_ActualLabels()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_CampaignName = "Master Campaign";
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			AddTouch(master, 1);
			Factory.Save();

			var actualLabels = new TransitionProgressViewModel(GetTouchStatsData(master)).TouchStatsCategoryAxis.ActualLabels.Distinct().ToList();
			AssertEquals(5, actualLabels.Count);

			var expectedLabels = new List<string> { TrackingSummaryConstants.Descriptions.SNT, TrackingStatusCodes.Descriptions.QUE, TrackingSummaryConstants.Descriptions.FAI, TrackingStatusCodes.Descriptions.OPQ, TrackingStatusCodes.Descriptions.OPC };
			AssertContainsExactElementsInAnyOrder(expectedLabels, actualLabels);

			AddTouch(master, 2);
			AddTouch(master, 3);
			AddTouch(master, 4);
			Factory.Save();

			actualLabels = new TransitionProgressViewModel(GetTouchStatsData(master)).TouchStatsCategoryAxis.ActualLabels.Distinct().ToList();
			AssertEquals(5, actualLabels.Count);

			expectedLabels = new List<string> { TrackingSummaryConstants.Descriptions.SNT, TrackingStatusCodes.Descriptions.QUE, TrackingSummaryConstants.Descriptions.FAI, TrackingStatusCodes.Codes.OPQ, TrackingStatusCodes.Codes.OPC };
			AssertContainsExactElementsInAnyOrder(expectedLabels, actualLabels);

			AddTouch(master, 5);
			AddTouch(master, 6);
			AddTouch(master, 7);
			Factory.Save();

			actualLabels = new TransitionProgressViewModel(GetTouchStatsData(master)).TouchStatsCategoryAxis.ActualLabels.Distinct().ToList();
			AssertEquals(5, actualLabels.Count);

			expectedLabels = new List<string> { TrackingSummaryConstants.Codes.SNT, TrackingStatusCodes.Codes.QUE, TrackingSummaryConstants.Codes.FAI, TrackingStatusCodes.Codes.OPQ, TrackingStatusCodes.Codes.OPC };
			AssertContainsExactElementsInAnyOrder(expectedLabels, actualLabels);
		}

		public void TestEmptyCampaign()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			AddNewItem(master);

			Factory.Save();

			var model = new TransitionProgressViewModel(TransitionProgressViewModelDataAdapter.GetTouchStatsData(master));
			AssertEquals(0, model.ChartModel.Series.Count);

			var svg = SvgExporter.ExportToString(model.ChartModel, 800, 600, false);
			var svgExpected =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<svg width=\"800\" height=\"600\" version=\"1.1\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns=\"http://www.w3.org/2000/svg\">\r\n  <defs>\r\n    <clipPath id=\"clipPath1\">\r\n      <rect x=\"0\" y=\"0\" width=\"800\" height=\"600\" />\r\n    </clipPath>\r\n  </defs>\r\n  <g clip-path=\"url(#clipPath1)\" />\r\n  <defs>\r\n    <clipPath id=\"clipPath2\">\r\n      <rect x=\"800\" y=\"8\" width=\"0\" height=\"0\" />\r\n    </clipPath>\r\n  </defs>\r\n  <g clip-path=\"url(#clipPath2)\" />\r\n  <defs>\r\n    <clipPath id=\"clipPath3\">\r\n      <rect x=\"0\" y=\"0\" width=\"800\" height=\"600\" />\r\n    </clipPath>\r\n  </defs>\r\n  <g clip-path=\"url(#clipPath3)\" />\r\n</svg>";
			AssertEquals(svgExpected, svg);
		}

		public void TestCampaignWithOneTouch()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_CampaignName = "master list";
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			AddNewItem(master);

			var touch1a = master.AllTouches.AddNew();
			touch1a.G0_CampaignName = "touch 1a";
			AddNewItem(touch1a).G8_TrackingStatus = "QUE";
			AddNewItem(touch1a).G8_TrackingStatus = "VER";
			AddNewItem(touch1a).G8_TrackingStatus = "NDR";

			Factory.Save();

			var model = new TransitionProgressViewModel(TransitionProgressViewModelDataAdapter.GetTouchStatsData(master));
			var svg = SvgExporter.ExportToString(model.ChartModel, 800, 600, false);
			AssertEquals(7, model.ChartModel.Series.Count);

			var svgExpected = @"<?xml version=""1.0"" encoding=""utf-8""?>
<svg width=""800"" height=""600"" version=""1.1"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns=""http://www.w3.org/2000/svg"">
  <defs>
    <clipPath id=""clipPath1"">
      <rect x=""0"" y=""0"" width=""800"" height=""600"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath1)"">
    <text dominant-baseline=""middle"" text-anchor=""end"" transform=""translate(12.587,548.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">0</text>
    <text dominant-baseline=""middle"" text-anchor=""end"" transform=""translate(12.587,369.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">1</text>
    <text dominant-baseline=""middle"" text-anchor=""end"" transform=""translate(12.587,191.2323)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">2</text>
    <text dominant-baseline=""middle"" text-anchor=""end"" transform=""translate(12.587,12.6076)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">3</text>
    <polyline points=""23.587,548.4818 23.587,12.6076"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""23.587,459.1694 16.587,459.1694"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""23.587,280.5447 16.587,280.5447"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""23.587,101.92 16.587,101.92"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(273.3212,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Sent</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(388.5832,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Queued</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(503.8451,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Failed</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(619.1071,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Opp Queued</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(734.369,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Opp Created</text>
    <polyline points=""23.587,548.4818 792,548.4818"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:0.5"" shape-rendering=""crispEdges"" />
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(407.7935,568.5348)"" font-family=""Segoe UI"" font-size=""1"" font-weight=""400"" fill=""black""> </text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(503.8451,578.7848)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">Touch 0</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(119.6386,578.7848)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">Master</text>
    <polyline points=""23.587,572.5348 792,572.5348"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:0.5"" shape-rendering=""crispEdges"" />
    <polyline points=""215.6903,570.2848 215.6903,574.7848"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""792,570.2848 792,574.7848"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""23.587,570.2848 23.587,574.7848"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
  </g>
  <defs>
    <clipPath id=""clipPath2"">
      <rect x=""23.587"" y=""12.6076"" width=""768.413"" height=""535.8741"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath2)"">
    <polyline points=""167.6644,548.4818 167.6644,369.857 71.6128,369.857 71.6128,548.4818"" style=""fill:none;stroke:rgb(241,241,241);stroke-width:2;stroke-linejoin:bevel"" />
    <polyline points=""71.6128,548.4818 167.6644,548.4818"" style=""fill:none;stroke:rgb(241,241,241);stroke-width:2;stroke-linejoin:bevel"" />
    <polygon points=""71.6128,548.4818 167.6644,548.4818 167.6644,369.857 71.6128,369.857 71.6128,548.4818"" style=""fill:rgb(241,241,241);stroke:none;"" />
    <polyline points=""215.6903,548.4818 215.6903,12.6076 792,548.4818 792,548.4818"" style=""fill:none;stroke:rgb(241,241,241);stroke-width:2;stroke-linejoin:bevel"" />
    <polyline points=""792,548.4818 215.6903,548.4818"" style=""fill:none;stroke:rgb(241,241,241);stroke-width:2;stroke-linejoin:bevel"" />
    <polygon points=""792,548.4818 215.6903,548.4818 215.6903,12.6076 792,548.4818 792,548.4818"" style=""fill:rgb(241,241,241);stroke:none;"" />
    <polyline points=""215.6903,548.4818 215.6903,-246.3982"" style=""fill:none;stroke:rgb(255,255,255);stroke-width:5;stroke-linejoin:bevel"" />
    <polyline points=""215.6903,548.4818 215.6903,-246.3982"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1;stroke-linejoin:bevel"" />
    <polyline points=""792,548.4818 792,-246.3982"" style=""fill:none;stroke:rgb(255,255,255);stroke-width:5;stroke-linejoin:bevel"" />
    <polyline points=""792,548.4818 792,-246.3982"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1;stroke-linejoin:bevel"" />
    <rect x=""244.5057"" y=""191.2323"" width=""57.631"" height=""357.2494"" style=""fill:rgb(255,165,0);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""359.7677"" y=""369.857"" width=""57.631"" height=""178.6247"" style=""fill:rgb(154,205,50);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""475.0296"" y=""369.857"" width=""57.631"" height=""178.6247"" style=""fill:rgb(233,150,122);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""590.2916"" y=""548.4818"" width=""57.631"" height=""0"" style=""fill:rgb(205,92,92);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""705.5535"" y=""548.4818"" width=""57.631"" height=""0"" style=""fill:rgb(50,205,50);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
  </g>
  <defs>
    <clipPath id=""clipPath3"">
      <rect x=""0"" y=""0"" width=""800"" height=""600"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath3)"" />
  <defs>
    <clipPath id=""clipPath4"">
      <rect x=""23.587"" y=""12.6076"" width=""768.413"" height=""535.8741"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath4)"">
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(119.6386,371.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(141,141,141)"">1</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(215.6903,14.6076)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(141,141,141)"">3</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(792,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(141,141,141)"">0</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(273.3212,193.2323)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">2</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(388.5832,371.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">1</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(503.8451,371.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">1</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(619.1071,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">0</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(734.369,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">0</text>
  </g>
  <defs>
    <clipPath id=""clipPath5"">
      <rect x=""0"" y=""0"" width=""800"" height=""600"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath5)"">
    <rect x=""23.587"" y=""12.6076"" width=""768.413"" height=""535.8741"" style=""fill:none;stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
  </g>
  <defs>
    <clipPath id=""clipPath6"">
      <rect x=""800"" y=""12.6076"" width=""0"" height=""0"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath6)"" />
  <defs>
    <clipPath id=""clipPath7"">
      <rect x=""0"" y=""0"" width=""800"" height=""600"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath7)"" />
</svg>";
			AssertEquals(svgExpected, svg);
		}

		public void TestCampaignWithMultipleTouches()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_CampaignName = "master list";
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			AddNewItem(master);

			var touch1a = master.AllTouches.AddNew();
			touch1a.G0_CampaignName = "touch 1a";
			AddNewItem(touch1a).G8_TrackingStatus = "QUE";
			AddNewItem(touch1a).G8_TrackingStatus = "VER";
			AddNewItem(touch1a).G8_TrackingStatus = "NDR";
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "a";

			var touch1b = master.AllTouches.AddNew();
			touch1b.G0_CampaignName = "touch 1b";
			AddNewItem(touch1b).G8_TrackingStatus = "QUE";
			AddNewItem(touch1b).G8_TrackingStatus = "VER";
			AddNewItem(touch1b).G8_TrackingStatus = "NDR";
			touch1b.G0_HorizontalId = 2;
			touch1b.G0_VerticalId = "b";

			Factory.Save();

			var model = new TransitionProgressViewModel(TransitionProgressViewModelDataAdapter.GetTouchStatsData(master));
			AssertEquals(9, model.ChartModel.Series.Count);

			var svg = SvgExporter.ExportToString(model.ChartModel, 800, 600, false);
			var svgExpected = @"<?xml version=""1.0"" encoding=""utf-8""?>
<svg width=""800"" height=""600"" version=""1.1"" xmlns:xlink=""http://www.w3.org/1999/xlink"" xmlns=""http://www.w3.org/2000/svg"">
  <defs>
    <clipPath id=""clipPath1"">
      <rect x=""0"" y=""0"" width=""800"" height=""600"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath1)"">
    <text dominant-baseline=""middle"" text-anchor=""end"" transform=""translate(12.587,548.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">0</text>
    <text dominant-baseline=""middle"" text-anchor=""end"" transform=""translate(12.587,369.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">1</text>
    <text dominant-baseline=""middle"" text-anchor=""end"" transform=""translate(12.587,191.2323)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">2</text>
    <text dominant-baseline=""middle"" text-anchor=""end"" transform=""translate(12.587,12.6076)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">3</text>
    <polyline points=""23.587,548.4818 23.587,12.6076"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""23.587,459.1694 16.587,459.1694"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""23.587,280.5447 16.587,280.5447"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""23.587,101.92 16.587,101.92"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(166.2923,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Sent</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(232.1562,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Queued</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(298.0202,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Failed</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(363.8842,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Opp Queued</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(429.7482,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Opp Created</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(495.6121,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Sent</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(561.4761,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Queued</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(627.3401,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Failed</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(693.204,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Opp Queued</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(759.068,552.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(128,128,128)"">Opp Created</text>
    <polyline points=""23.587,548.4818 792,548.4818"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:0.5"" shape-rendering=""crispEdges"" />
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(407.7935,568.5348)"" font-family=""Segoe UI"" font-size=""1"" font-weight=""400"" fill=""black""> </text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(298.0202,578.7848)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">Touch 1</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(627.3401,578.7848)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">Touch 2</text>
    <text dominant-baseline=""hanging"" text-anchor=""middle"" transform=""translate(78.4736,578.7848)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">Master</text>
    <polyline points=""23.587,572.5348 792,572.5348"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:0.5"" shape-rendering=""crispEdges"" />
    <polyline points=""133.3603,570.2848 133.3603,574.7848"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""462.6801,570.2848 462.6801,574.7848"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""792,570.2848 792,574.7848"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
    <polyline points=""23.587,570.2848 23.587,574.7848"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1"" shape-rendering=""crispEdges"" />
  </g>
  <defs>
    <clipPath id=""clipPath2"">
      <rect x=""23.587"" y=""12.6076"" width=""768.413"" height=""535.8741"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath2)"">
    <polyline points=""105.917,548.4818 105.917,369.857 51.0303,369.857 51.0303,548.4818"" style=""fill:none;stroke:rgb(241,241,241);stroke-width:2;stroke-linejoin:bevel"" />
    <polyline points=""51.0303,548.4818 105.917,548.4818"" style=""fill:none;stroke:rgb(241,241,241);stroke-width:2;stroke-linejoin:bevel"" />
    <polygon points=""51.0303,548.4818 105.917,548.4818 105.917,369.857 51.0303,369.857 51.0303,548.4818"" style=""fill:rgb(241,241,241);stroke:none;"" />
    <polyline points=""133.3603,548.4818 133.3603,12.6076 462.6801,548.4818 792,548.4818 792,548.4818"" style=""fill:none;stroke:rgb(241,241,241);stroke-width:2;stroke-linejoin:bevel"" />
    <polyline points=""792,548.4818 133.3603,548.4818"" style=""fill:none;stroke:rgb(241,241,241);stroke-width:2;stroke-linejoin:bevel"" />
    <polygon points=""792,548.4818 133.3603,548.4818 133.3603,12.6076 462.6801,548.4818 792,548.4818 792,548.4818"" style=""fill:rgb(241,241,241);stroke:none;"" />
    <polyline points=""133.3603,548.4818 133.3603,-246.3982"" style=""fill:none;stroke:rgb(255,255,255);stroke-width:5;stroke-linejoin:bevel"" />
    <polyline points=""133.3603,548.4818 133.3603,-246.3982"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1;stroke-linejoin:bevel"" />
    <polyline points=""462.6801,548.4818 462.6801,-246.3982"" style=""fill:none;stroke:rgb(255,255,255);stroke-width:5;stroke-linejoin:bevel"" />
    <polyline points=""462.6801,548.4818 462.6801,-246.3982"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1;stroke-linejoin:bevel"" />
    <polyline points=""792,548.4818 792,-246.3982"" style=""fill:none;stroke:rgb(255,255,255);stroke-width:5;stroke-linejoin:bevel"" />
    <polyline points=""792,548.4818 792,-246.3982"" style=""fill:none;stroke:rgb(211,211,211);stroke-width:1;stroke-linejoin:bevel"" />
    <rect x=""149.8263"" y=""191.2323"" width=""32.932"" height=""357.2494"" style=""fill:rgb(255,165,0);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""215.6903"" y=""369.857"" width=""32.932"" height=""178.6247"" style=""fill:rgb(154,205,50);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""281.5542"" y=""191.2323"" width=""32.932"" height=""357.2494"" style=""fill:rgb(233,150,122);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""347.4182"" y=""548.4818"" width=""32.932"" height=""0"" style=""fill:rgb(205,92,92);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""413.2822"" y=""548.4818"" width=""32.932"" height=""0"" style=""fill:rgb(50,205,50);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""479.1461"" y=""191.2323"" width=""32.932"" height=""357.2494"" style=""fill:rgb(255,165,0);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""545.0101"" y=""369.857"" width=""32.932"" height=""178.6247"" style=""fill:rgb(154,205,50);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""610.8741"" y=""369.857"" width=""32.932"" height=""178.6247"" style=""fill:rgb(233,150,122);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""676.7381"" y=""548.4818"" width=""32.932"" height=""0"" style=""fill:rgb(205,92,92);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
    <rect x=""742.602"" y=""548.4818"" width=""32.932"" height=""0"" style=""fill:rgb(50,205,50);stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
  </g>
  <defs>
    <clipPath id=""clipPath3"">
      <rect x=""0"" y=""0"" width=""800"" height=""600"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath3)"" />
  <defs>
    <clipPath id=""clipPath4"">
      <rect x=""23.587"" y=""12.6076"" width=""768.413"" height=""535.8741"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath4)"">
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(78.4736,371.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(141,141,141)"">1</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(133.3603,14.6076)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(141,141,141)"">3</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(462.6801,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(141,141,141)"">0</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(792,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""rgb(141,141,141)"">0</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(166.2923,193.2323)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">2</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(232.1562,371.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">1</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(298.0202,193.2323)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">2</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(363.8842,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">0</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(429.7482,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">0</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(495.6121,193.2323)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">2</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(561.4761,371.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">1</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(627.3401,371.857)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">1</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(693.204,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">0</text>
    <text dominant-baseline=""baseline"" text-anchor=""middle"" transform=""translate(759.068,550.4818)"" font-family=""Segoe UI"" font-size=""11"" font-weight=""400"" fill=""black"">0</text>
  </g>
  <defs>
    <clipPath id=""clipPath5"">
      <rect x=""0"" y=""0"" width=""800"" height=""600"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath5)"">
    <rect x=""23.587"" y=""12.6076"" width=""768.413"" height=""535.8741"" style=""fill:none;stroke:black;stroke-width:0"" shape-rendering=""crispEdges"" />
  </g>
  <defs>
    <clipPath id=""clipPath6"">
      <rect x=""800"" y=""12.6076"" width=""0"" height=""0"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath6)"" />
  <defs>
    <clipPath id=""clipPath7"">
      <rect x=""0"" y=""0"" width=""800"" height=""600"" />
    </clipPath>
  </defs>
  <g clip-path=""url(#clipPath7)"" />
</svg>";

			AssertEquals(svgExpected, svg);
		}

		GlbCompanyCampaignItem AddNewItem(GlbCompanyCampaign campaign)
		{
			var result = campaign.CampaignsItemsSent.AddNew();
			result.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			result.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			return result;
		}

		void AddTouch(GlbCompanyCampaign masterCampaign, ZByte horizontalId)
		{
			var touch = masterCampaign.AllTouches.AddNew();
			touch.G0_CampaignName = "Touch " + horizontalId;
			touch.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch.G0_HorizontalId = horizontalId;
			touch.G0_VerticalId = "A";
		}
	}
}
