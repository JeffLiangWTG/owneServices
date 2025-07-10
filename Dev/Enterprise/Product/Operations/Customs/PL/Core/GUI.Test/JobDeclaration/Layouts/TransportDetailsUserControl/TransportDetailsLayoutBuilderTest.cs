using System;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(TransportDetailsLayoutBuilder))]
sealed class TransportDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TransportDetailsLayoutBuilder, JobDeclaration, Customs.GUI.TransportDetailsControlBag>
{
	public void TestTransportInlandRailUserControlVisible()
	{
		AssertControlVisibility(TransportDetailsControlBag.Instance.TransportInlandRailUserControl, x => x.JE_TransportModeInland == TransportModes.Rail);
	}

	public void TestVesselUserControlVisible()
	{
		AssertControlVisibility(TransportDetailsControlBag.Instance.VesselUserControl, x => x.IsSea);
	}

	public void TestTransportInlandIDAndNationalityUserControlVisible()
	{
		AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, x => x.JE_TransportModeInland == TransportModes.Mail);
		AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, x => x.JE_TransportModeInland == TransportModes.OwnPropulsion);
		AssertControlVisibility(Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandIDAndNationalityUserControl, x => x.JE_TransportModeInland == TransportModes.FixedTransportInstallations);
	}

	protected override int ExpectedMaxColumns => 1;

	protected override TransportDetailsLayoutBuilder GetColumnLayoutBuilderForTesting()
	{
		var builder = new TransportDetailsLayoutBuilder();
		builder.AddControlBag(TransportDetailsControlBag.Instance);
		return builder;
	}

	void AssertControlVisibility(ControlReference controlReference, Func<JobDeclaration, bool> visible)
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			AssertEquals("Is UCC6", true, declaration.Configuration.IsUCC6(declaration));

			foreach (var messageType in messageTypesWithoutWAD)
			{
				declaration.JE_MessageType = messageType;
				foreach (var transportMode in allTransportModes)
				{
					declaration.JE_TransportMode = transportMode;
					AssertEquals($"MessageType: {messageType}; TransportMode: {transportMode}", visible(declaration), Layout.IsVisible(controlReference, declaration));
				}
			}
		});
	}

	string[] messageTypesWithoutWAD => new[] { Common.EU.EUJobMessageTypeList.Codes.Import, Common.EU.EUJobMessageTypeList.Codes.Export, Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms };

	string[] allTransportModes => new[]
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

	PanelLayout Layout => layout ?? (layout = ((IPanelLayoutProvider)new TransportDetailsLayout()).Layout);
	PanelLayout layout;
}
