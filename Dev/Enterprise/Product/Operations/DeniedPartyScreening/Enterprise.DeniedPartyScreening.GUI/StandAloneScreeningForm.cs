using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class StandAloneScreeningForm : ZChildForm
	{
		public StandAloneScreeningForm(bool forceFullListRatherThanCutDownList, StandaloneScreeningSupportedCandidate initialSelection = StandaloneScreeningSupportedCandidate.Org)
		{
			InitializeComponent();
			this.forceFullListRatherThanCutDownList = forceFullListRatherThanCutDownList;
			this.initialSelection = initialSelection;

			FullNameTextBox.MaxLength = OrgHeaderSchema.OH_FullName.MaxLength;
			Address1TextBox.MaxLength = 100;
			Address2TextBox.MaxLength = 100;
			CityTextBox.MaxLength = 100;
			PostCodeTextBox.MaxLength = 50;
			StateTextBox.MaxLength = 100;
			IdNumberTextBox.MaxLength = 100;

			TextboxPanel.AllowOverlap(VesselRadioButton);
			TextboxPanel.AllowOverlap(OrgRadioButton);
			TextboxPanel.AllowOverlap(PersonRadioButton);
		}

		readonly bool forceFullListRatherThanCutDownList;
		readonly StandaloneScreeningSupportedCandidate initialSelection;

		[Obsolete("This is just for the designer")]
		public StandAloneScreeningForm()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CountryFindBox.List = new RefCountryCollection(ReadOnlyFactory);
			IdIssuingCountryFindBox.List = new RefCountryCollection(ReadOnlyFactory);
			VesselCountyOfRegFindBox.List = new RefCountryCollection(ReadOnlyFactory);

			OrgRadioButton.Checked = false;
			PersonRadioButton.Checked = false;
			VesselRadioButton.Checked = false;
			switch (initialSelection)
			{
				case StandaloneScreeningSupportedCandidate.Org:
					OrgRadioButton.Checked = true;
					break;
				case StandaloneScreeningSupportedCandidate.Person:
					PersonRadioButton.Checked = true;
					break;
				case StandaloneScreeningSupportedCandidate.Vessel:
					VesselRadioButton.Checked = true;
					break;
			}
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		ReadOnlyBusinessObjectFactory readOnlyFactory;
		public ReadOnlyBusinessObjectFactory ReadOnlyFactory => readOnlyFactory ?? (readOnlyFactory = new ReadOnlyBusinessObjectFactory());

		OrgHeader parentOrg;
		public OrgHeader ParentOrg
		{
			get
			{
				if (parentOrg == null)
				{
					parentOrg = ReadOnlyFactory.New<OrgHeader>();
					parentOrg.OH_Code = string.Empty;
				}

				return parentOrg;
			}
		}

		OrgHeader orgHeader;
		public OrgHeader OrgHeader => orgHeader ?? (orgHeader = ReadOnlyFactory.New<OrgHeader>());

		RefVessel refVessel;
		public RefVessel RefVessel => refVessel ?? (refVessel = ReadOnlyFactory.New<RefVessel>());

		async void ScreenButton_Click(object sender, EventArgs e)
		{
			await Screen();
		}

		protected async Task Screen()
		{
			try
			{
				using (new ZWaitCursorChanger())
				{
					var names = new List<DpsNameCandidate>();
					var addresses = new List<DpsAddressCandidate>();
					var codes = new List<DpsRegistrationCodeCandidate>();
					var countries = new List<string>();

					var dpsManager = ObjectFactory.Get<IDpsManager>();
					var dpsCandidateCreator = new DpsCandidateCreator();
					ScreeningParty screeningParty = null;
					var fullName = (ZString)FullNameTextBox.Text;

					var description = Res.GetString("CA4363FC-7C25-4787-BFFC-07B8516FA4D7", "Stand Alone Screening");
					if (PersonRadioButton.Checked || OrgRadioButton.Checked)
					{
						OrgHeader.OH_FullName = fullName.SubstringSafe(0, OrgHeaderSchema.OH_FullName.MaxLength);
						screeningParty = new ScreeningParty(ParentOrg, description, OrgHeader);

						if (!string.IsNullOrWhiteSpace(fullName))
						{
							if (PersonRadioButton.Checked)
							{
								names.Add(dpsCandidateCreator.NewNameCandidate(fullName, DeniedPartyConstants.ScreeningNameTypes.Person));
							}
							else
							{
								dpsCandidateCreator.NewSeparatedOrganizationNames(names, fullName, DeniedPartyConstants.ScreeningNameTypes.Organization);
							}
						}

						if (AddressNotNull())
						{
							addresses.Add(dpsCandidateCreator.NewAddressCandidate(Address1TextBox.Text, Address2TextBox.Text, CityTextBox.Text, StateTextBox.Text, PostCodeTextBox.Text, CountryFindBox.CodeBox.Text, OtherInfoTextBox.Text));
						}

						if (!string.IsNullOrWhiteSpace(IdNumberTextBox.Text))
						{
							var regCodeType = PersonRadioButton.Checked ? "PAS" : "DUN";
							codes.Add(dpsCandidateCreator.NewRegistrationCodeCandidate(IdIssuingCountryFindBox.CodeBox.Text, regCodeType, IdNumberTextBox.Text));
						}

						if (!string.IsNullOrWhiteSpace(CountryFindBox.CodeBox.Text))
						{
							countries.Add(CountryFindBox.CodeBox.Text);
						}
					}
					else if (VesselRadioButton.Checked)
					{
						RefVessel.RV_Code = fullName.SubstringSafe(0, RefVesselSchema.RV_Code.MaxLength);
						screeningParty = new ScreeningParty(ParentOrg, description, RefVessel);

						if (!string.IsNullOrWhiteSpace(fullName))
						{
							names.Add(dpsCandidateCreator.NewNameCandidate(fullName, DeniedPartyConstants.ScreeningNameTypes.Vessel));
						}

						if (!string.IsNullOrEmpty(VesselLloydsNumberTextBox.Text))
						{
							codes.Add(dpsCandidateCreator.NewRegistrationCodeCandidate(VesselCountyOfRegFindBox.CodeBox.Text, "IMO", VesselLloydsNumberTextBox.Text));
						}

						if (!string.IsNullOrWhiteSpace(VesselCountyOfRegFindBox.CodeBox.Text))
						{
							countries.Add(VesselCountyOfRegFindBox.CodeBox.Text);
						}
					}

					if (screeningParty != null && codes.Count + addresses.Count + names.Count + countries.Count > 0)
					{
						var request = dpsCandidateCreator.GetDpsRequestHeader(names, addresses, codes, countries);
						DpsResponse response;

						using (var form = new ProgressForm())
						{
							form.ShowCancelButton = false;
							form.ShowModalTo(this);
							response = await dpsManager.Screen(request, null);
						}

						DpsImageSources entityIcon;
						if (PersonRadioButton.Checked)
						{
							entityIcon = DpsImageSources.Person;
						}
						else if (OrgRadioButton.Checked)
						{
							entityIcon = DpsImageSources.Organization;
						}
						else
						{
							entityIcon = DpsImageSources.Vessel;
						}

						DpsResultsManager.ProcessResults(new List<DpsResponseWithScreeningParty> { new DpsResponseWithScreeningParty(screeningParty, response, request) }, null, true, ReadOnlyFactory, false, forceFullListRatherThanCutDownList, entityIcon);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("3A095672-B2D5-475E-8C21-D7CC13355818", "No information for screening"));
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				DpsExceptionHandler.Process(ex);
			}
		}

		bool AddressNotNull()
		{
			return !string.IsNullOrWhiteSpace(Address1TextBox.Text)
					|| !string.IsNullOrWhiteSpace(Address2TextBox.Text)
					|| !string.IsNullOrWhiteSpace(CityTextBox.Text)
					|| !string.IsNullOrWhiteSpace(StateTextBox.Text)
					|| !string.IsNullOrWhiteSpace(PostCodeTextBox.Text)
					|| !string.IsNullOrWhiteSpace(CountryFindBox.CodeBox.Text)
					|| !string.IsNullOrWhiteSpace(OtherInfoTextBox.Text);
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void RadioButtons_CheckedChanged(object sender, EventArgs e)
		{
			VesselPanel.Visible = VesselRadioButton.Checked;
			TextboxPanel.Visible = !VesselRadioButton.Checked;
		}
	}

	public enum StandaloneScreeningSupportedCandidate
	{
		Org = 0,
		Person = 1,
		Vessel = 2
	}
}
