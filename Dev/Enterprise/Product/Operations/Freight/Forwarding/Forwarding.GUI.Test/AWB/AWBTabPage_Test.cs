using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public class AWBTabPage_Test : TestCaseWithFactory
	{
		public void TestIsAwbVisibleForConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			AssertAWBControlVisibilityWhenAWBInterfaceNull();
			AssertAWBVisibilityWhenTransportModeChangesForConsol(consol, isDomesticFreight: false);
			AssertAWBVisibilityWhenTransportModeChangesForConsol(consol, isDomesticFreight: true);

			consol.JK_TransportMode = TransportModes.Air;

			AssertAWBVisibilityWhenLoadingConsol(consol);
			AssertAWBVisibilityWhenAgentTypeChanges(consol);
		}

		void AssertAWBControlVisibilityWhenAWBInterfaceNull()
		{
			using (AWBTabPage tab = new AWBTabPage())
			{
				AssertNull("Precondition", tab.AWBInterface);
				AssertEquals("AWBControl should not be visible", false, tab.IsAWBVisible);
			}
		}

		void AssertAWBVisibilityWhenTransportModeChangesForConsol(ForwardingConsol consol, bool isDomesticFreight)
		{
			using (ConsolFormTest form = new ConsolFormTest(consol))
			{
				form.Show();
				consol.IsDomesticFreight = isDomesticFreight;

				CombineAssertions("Pre-Conditions", delegate
				{
					Assert(!consol.IsAir);
					Assert(!form.MainTabControl.TabPages.Contains(form.AWBTabPage));
					Assert(!form.AWBTabPage.TabRelevant);
				});

				var transportModes = typeof(TransportModes).GetFields();

				foreach (var transportMode in transportModes)
				{
					consol.JK_TransportMode = transportMode.GetValue(null) as string;

					if (consol.JK_TransportMode.Equals(TransportModes.Air))
					{
						CombineAssertions("The only transport mode that should show AWB tab is AIR", delegate
						{
							Assert("AIR consol should have AWB tab.", form.MainTabControl.TabPages.Contains(form.AWBTabPage));
							Assert("View menu should show AWB", form.AWBTabPage.TabRelevant);
						});
					}
					else
					{
						CombineAssertions($"{consol.JK_TransportMode} should not have AWB tab/view menu", delegate
						{
							Assert("This consol should not have AWB tab.", !form.MainTabControl.TabPages.Contains(form.AWBTabPage));
							Assert("View menu should not show AWB", !form.AWBTabPage.TabRelevant);
						});
					}
				}
			}
		}

		void AssertAWBVisibilityWhenLoadingConsol(ForwardingConsol consol)
		{
			//testing AWB is showing when loading Domestic Air Consol
			using (ConsolFormTest form = new ConsolFormTest(consol))
			{
				form.Show();
				AssertEquals("Precondition", true, consol.IsAir);
				AssertEquals("Precondition", true, consol.IsDomesticFreight);
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));
			}

			//testing AWB is showing when loading Not Domestic Air Consol
			consol.IsDomesticFreight = false;

			using (ConsolFormTest form = new ConsolFormTest(consol))
			{
				form.Show();
				AssertEquals("Precondition", true, consol.IsAir);
				AssertEquals("Precondition", false, consol.IsDomesticFreight);
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));
			}

			//testing AWB visibility when loading Domestic Consol different than Air
			consol.IsDomesticFreight = true;
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;

			using (ConsolFormTest form = new ConsolFormTest(consol))
			{
				form.Show();
				AssertEquals("Precondition", false, consol.IsAir);
				AssertEquals("Precondition", true, consol.IsDomesticFreight);
				AssertEquals(false, form.MainTabControl.TabPages.Contains(form.AWBTabPage));
			}

			//testing AWB visibility when loading Domestic Consol different than Air
			consol.IsDomesticFreight = false;

			using (ConsolFormTest form = new ConsolFormTest(consol))
			{
				form.Show();
				AssertEquals("Precondition", false, consol.IsAir);
				AssertEquals("Precondition", false, consol.IsDomesticFreight);
				AssertEquals(false, form.MainTabControl.TabPages.Contains(form.AWBTabPage));
			}
		}

		void AssertAWBVisibilityWhenAgentTypeChanges(ForwardingConsol consol)
		{
			using (ConsolFormTest form = new ConsolFormTest(consol))
			{
				form.Show();

				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Core.Constants.AgentType.Courier;
				AssertEquals(false, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Core.Constants.AgentType.AWBCoload;
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Core.Constants.AgentType.AWBMaster;
				AssertEquals(false, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Core.Constants.AgentType.Charter;
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Core.Constants.AgentType.Direct;
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Core.Constants.AgentType.Other;
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));
			}
		}

		public void TestConsolFormUsesMAWBWithMessagesUserControl()
		{
			using (ConsolFormTest consolForm = GetNewZConsolForm())
			{
				Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				var tabPages = ((ZTabControl)consolForm.AWBTabPage.Controls[0].Controls[0].Controls[0])
					.TabPages
					.Cast<ZTabPage>()
					.Select(page => page.Name);

				AssertContainsExactElementsInAnyOrder("MAWBControl tab pages",
					new[] { "MAWBTabPage", "SecurityDeclarationTabPage", "MessagesTabPage" },
					tabPages);
			}
		}

		#region Test Classes

		class ConsolFormTest : ConsolForm
		{
			public ConsolFormTest(ForwardingConsol bO)
				: base(bO)
			{
			}

			public new ZTemplateTabControl MainTabControl
			{
				get { return base.MainTabControl; }
			}

			public new ZTabPage AWBTabPage
			{
				get { return base.AWBTabPage; }
			}
		}

		#endregion

		#region Implementation

		ForwardingConsol Consol;
		ConsolFormTest GetNewZConsolForm()
		{
			Consol = Factory.New<ForwardingConsol>();
			return new ConsolFormTest(Consol);
		}

		#endregion
	}
}
