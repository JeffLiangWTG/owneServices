using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ImportTransportDetailsLayoutBuilder))]
sealed class ImportTransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ImportTransportDetailsLayoutBuilder, JobDeclaration, Customs.GUI.TransportDetailsControlBag>
{
	public void TestTransportIDAndNationalityUserControl_Visibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Sea", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("Air", false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
			AssertEquals("Rail", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, declaration));
		});
	}

	public void TestPortOfFirstArrivalUserControl_Visibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("PortOfFirstArrivalUserControl", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, declaration));
	}

	public void TestTransportInlandRoadUserControlVisible()
	{
		AssertControlVisibility(NLTransportDetailsControlBag.Instance.ImportTransportInlandRoadUserControl, x => x.JE_TransportModeInland == TransportModes.Road, messageType: MessageTypeList.Codes.Import);
	}

	public void TestTransportInlandSeaUserControlVisible()
	{
		AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, x => x.JE_TransportModeInland == TransportModes.Sea, messageType: MessageTypeList.Codes.Import);
	}

	public void TestTransportInlandIDAndNationalityUserControlVisible()
	{
		AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, x => (x.JE_TransportModeInland.IsEmpty || (!x.IsRoadInland && !x.IsSeaInland && !x.IsMailInland && !x.IsFixedInstallationInland)), messageType: MessageTypeList.Codes.Import);
	}

	public void TestTransportInlandModeAndTypeOfIdUserControlVisible()
	{
		AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, x => (!x.IsMailInland && !x.IsFixedInstallationInland), messageType: MessageTypeList.Codes.Import);
	}

	public void TestImportTransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControlVisible()
	{
		AssertControlVisibility(NLTransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdMailAndFixedInstallationUserControl, x => (x.IsMailInland || x.IsFixedInstallationInland), messageType: MessageTypeList.Codes.Import);
	}

	public void TestTransportInlandSeparatorUserControlVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;

		CombineAssertions(() =>
		{
			declaration.JE_TransportModeInland = TransportModes.Mail;
			AssertEquals($"EXP - InlandTransportMode: MAI", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.FixedTransportInstallations;
			AssertEquals($"EXP - InlandTransportMode: FIX", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Road;
			AssertEquals($"EXP - InlandTransportMode: ROA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.InlandWaterwayTransport;
			AssertEquals($"EXP - InlandTransportMode: IWT", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Rail;
			AssertEquals($"EXP - InlandTransportMode: RAI", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Sea;
			AssertEquals($"EXP - InlandTransportMode: SEA", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.OwnPropulsion;
			AssertEquals($"EXP - InlandTransportMode: OWN", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
			declaration.JE_TransportModeInland = TransportModes.Air;
			AssertEquals($"EXP - InlandTransportMode: AIR", true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, declaration));
		});
	}

	public void TestFlightAndNationalityUserControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = ''", expected: false, Layout.IsVisible(NLTransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = SEA", expected: false, Layout.IsVisible(NLTransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = true, JE_MessageType = IMP, JE_TransportMode = AIR", expected: true, Layout.IsVisible(NLTransportDetailsControlBag.Instance.FlightAndNationalityUserControl, declaration));
		});
	}

	public void TestVoyageAndNationalityUserControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = ''", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("IsVisible = false, JE_MessageType = IMP, JE_TransportMode = AIR", expected: false, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsVisible = true, JE_MessageType = IMP, JE_TransportMode = SEA", expected: true, Layout.IsVisible(Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, declaration));
		});
	}

	public void TestTransportInlandModeAndTypeOfIdUserControlResourceStringData()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("Are captions set?", true, Layout.TryGetCaptionData(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, declaration, out var captionData));
		CombineAssertions(() => AssertCaptions(captionData["TypeOfIDDropEdit"], "Code", "Code", "[19 06 061 000] type of identification of the transport"));
	}

	protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Medium;

	protected override int ExpectedMaxColumns => 1;

	protected override ImportTransportDetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		var builder = new ImportTransportDetailsLayoutBuilder();
		builder.AddControlBag(EU.GUI.TransportDetailsControlBag.Instance);
		builder.AddControlBag(NLTransportDetailsControlBag.Instance);
		return builder;
	}

	void AssertControlVisibility(ControlReference controlReference, Func<JobDeclaration, bool> isVisible, string messageType = MessageTypeList.Codes.Import)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;

			foreach (var transportMode in AllTransportModes)
			{
				declaration.JE_TransportMode = transportMode;
				foreach (var inlandTransportMode in AllTransportModes)
				{
					declaration.JE_TransportModeInland = inlandTransportMode;
					AssertEquals($"TransportMode: {transportMode}; InlandTransportMode: {inlandTransportMode}", isVisible(declaration), Layout.IsVisible(controlReference, declaration));
				}
			}
	}

	void AssertCaptions(ResourceStringData captionData, string expectedShortCaption, string expectedCaption, string expectedFullDescription)
	{
		AssertEquals("CaptionData ShortCaption", expectedShortCaption, captionData.ShortCaption);
		AssertEquals("CaptionData Caption", expectedCaption, captionData.Caption);
		AssertEquals("CaptionData FullDescription", expectedFullDescription, captionData.FullDescription);
	}

	string[] AllTransportModes => new[]
	{
		TransportModes.Air,
		TransportModes.FixedTransportInstallations,
		TransportModes.InlandWaterwayTransport,
		TransportModes.OwnPropulsion,
		TransportModes.Mail,
		TransportModes.Rail,
		TransportModes.Road,
		TransportModes.Sea,
		string.Empty
	};

	PanelLayout Layout => layout ?? (layout = new ImportTransportDetailsLayout().Layout);
	PanelLayout layout;
}
