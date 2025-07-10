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
	[TestedType(typeof(DpsResultWinform))]
	class DpsResultWinFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var wrapper = CreateDpsResponseWithScreeningParty();
			return new DpsResultWinform(new DpsResultWinModel(new DpsResultModel(new List<DpsResponseWithScreeningParty> { wrapper }, Factory)));
		}

		public void TestFormCaption()
		{
			var wrapper = CreateDpsResponseWithScreeningParty();
			using (var form = new DpsResultWinform(new DpsResultWinModel(new DpsResultModel(new List<DpsResponseWithScreeningParty> { wrapper }, Factory))))
			{
				form.Show();
				AssertEquals("Denied Party Screening Result", form.FormCaption);
			}
		}

		DpsResponseWithScreeningParty CreateDpsResponseWithScreeningParty()
		{
			var profiles = new List<ProfileHeaderInfo>()
			{
				new ProfileHeaderInfo { SourceProfileID = Guid.NewGuid(), ProfileNotes = Array.Empty<byte>(), TypeOfEntity = "PER" },
			};

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(header, "", header);
			var responseWithParty = new DpsResponseWithScreeningParty(party, new DpsResponse { Profiles = profiles }, new DpsRequestHeaderWithAddressMatching());
			return responseWithParty;
		}
	}
}
