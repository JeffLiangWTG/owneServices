using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	[TestedType(typeof(SummaryForm))]
	class SummaryFormTest : ZFormBasherTest
	{
		public override void TestBashingForm()
		{
			Assert(true);
		}

		protected override Form GetFormToBashCore()
		{
			return new SummaryForm(new List<IScreenedParty>() { new ScreenedPartyWinModel(new ScreenedPartyModel(CreateDpsResponseWithScreeningParty(), Factory), a => { }) });
		}

		public void TestFormCaption()
		{
			using (var form = new SummaryForm(new List<IScreenedParty>() { new ScreenedPartyWinModel(new ScreenedPartyModel(CreateDpsResponseWithScreeningParty(), Factory), a => { }) }))
			{
				form.Show();
				AssertEquals("Screening Status Summary", form.FormCaption);
			}
		}

		DpsResponseWithScreeningParty CreateDpsResponseWithScreeningParty()
		{
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Compressor.Zip("Test"), TypeOfEntity = "PER" },
			};

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			return responseWithParty;
		}
	}
}
