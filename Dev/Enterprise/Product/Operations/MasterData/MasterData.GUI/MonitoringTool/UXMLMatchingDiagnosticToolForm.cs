using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using EZC = Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterData.GUI
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class UXMLMatchingDiagnosticToolForm : ZChildForm, IDeduplicationDebuggerParticipant
	{
		public UXMLMatchingDiagnosticToolForm()
		{
			InitializeComponent();
			InitializeUXMLMatchedListUserControl();
			MonitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();
			tvDelegate = AddNodes;
			DeduplicationUtils.DebuggerHubInstance.Register(this);
			InputValueTabControl.SelectedIndexChanged += InputValueTabControl_SelectedIndexChanged;
			MatchOrgButton.Visible = false; // Hide the Match Organization button for now. We may introduce it again in the future.
		}

		void InitializeUXMLMatchedListUserControl()
		{
			uxmlMatchedListUserControl = new UXMLMatchedListUserControl
			{
				Dock = DockStyle.Fill,
				AutoScroll = true
			};

			MatchResultGroupBox.Controls.Add(uxmlMatchedListUserControl);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UXMLMatchingDiagnosticUtils.Format(XMLValuesTextBox);
		}

		public string DebuggerName => DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName;

		public IDeduplicationDebuggerHub DebuggerHub { get; set; }

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		[SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static ConcurrentDictionary<string, MonitoringObjectValue> MonitoringObjects;
		int counter;
		public delegate void AddTreeNode();
		public AddTreeNode tvDelegate;

		public void Receive(object value, string methodName, TimeSpan executionTime)
		{
			IEnumerable<ScoringResult> scoringResults;
			if (methodName == "Org_ScoringResults" && (scoringResults = value as IEnumerable<ScoringResult>) != null)
			{
				DebuggerScoringResults.AddRange(scoringResults);
			}

			if (DeduplicationMonitoringUserControl != null && DeduplicationMonitoringUserControl.IsHandleCreated)
			{
				if (!MonitoringObjects.Any())
				{
					counter = 0;
				}

				counter++;

				if (MonitoringObjects.TryAdd(string.Join("_", methodName, counter.ToString(CultureInfo.InvariantCulture)), new MonitoringObjectValue { ExecutionTimeInMilliseconds = (long)executionTime.TotalMilliseconds, Value = value }))
				{
					DeduplicationMonitoringUserControl.Invoke(tvDelegate);
				}
			}
		}

		protected void AddNodes()
		{
			DeduplicationMonitoringUserControl.SetDataContext(MonitoringObjects);
		}

		protected List<ScoringResult> DebuggerScoringResults { get; } = new List<ScoringResult>();

		public void Send(string recipientName, object value, string methodName, TimeSpan executionTime, object glowBizO)
		{
			//This form not send anything
		}

		public void Send(string recipientName, object value, string methodName, TimeSpan executionTime, string prefix)
		{
		}

		#region Button Click

		void MatchOrgButton_Click(object sender, EventArgs e)
		{
			ClearResultsAndSetToParticipant();

			ThresholdsLayout.Controls.Clear();
			AddOrgMatchThresholdLabel();

			var address = OrganizationAddressInfo;
			if (address != null)
			{
				var factory = new BusinessObjectFactory();
				var matchedOrg = UXMLMatchingDiagnosticUtils.GetMatchedOrgHeader(address, factory);
				var orderedDebuggerScoringResults = DebuggerScoringResults.OrderByDescending(o => o.Score).ToList();
				var dataSource = UXMLMatchingDiagnosticUtils.CombineMatchedOrgAndScoreResultsToVMs(matchedOrg, orderedDebuggerScoringResults, factory);
				uxmlMatchedListUserControl.PopulatePanel(dataSource);
				ProcessFoundResultsLog(matchedOrg != null);

#if DEBUG
				if (Globals.IsTest)
				{
					UXMLMatchingDiagnosticModelsForTest = dataSource;
				}
#endif
			}
		}

		void MatchAddressButton_Click(object sender, EventArgs e)
		{
			ClearResultsAndSetToParticipant();

			ThresholdsLayout.Controls.Clear();
			AddOrgMatchThresholdLabel();
			AddAddressMatchThresholdLabel();

			var address = OrganizationAddressInfo;
			if (address != null)
			{
				var factory = new BusinessObjectFactory();
				var logger = new SimpleLogger();
				var matchedOrgAddress = UXMLMatchingDiagnosticUtils.GetMatchedOrgAddress(address, factory, logger);
				var orderedDebuggerScoringResults = DebuggerScoringResults.OrderByDescending(o => o.Score).ToList();
				var dataSource = UXMLMatchingDiagnosticUtils.CombineMatchedAddressAndScoreResultsToVMs(matchedOrgAddress, orderedDebuggerScoringResults, factory);
				uxmlMatchedListUserControl.PopulatePanel(dataSource);
				ProcessLoggerMessage(logger);
				ProcessFoundResultsLog(matchedOrgAddress != null);

#if DEBUG
				if (Globals.IsTest)
				{
					UXMLMatchingDiagnosticModelsForTest = dataSource;
				}
#endif
			}
		}

		protected void XMLValuesTextBox_Leave(object sender, EventArgs e)
		{
			UXMLMatchingDiagnosticUtils.Format(XMLValuesTextBox);
		}

		void ExportResultsButton_Click(object sender, EventArgs e)
		{
			SaveMonitoringObjectToFileUtils.SaveMonitoringObjectToFile(MonitoringObjects);
		}

		#endregion

		void AddOrgMatchThresholdLabel()
		{
			ThresholdsLayout.RowCount += 1;
			ThresholdsLayout.RowStyles.Add(new RowStyle());
			OrgMatchThresholdLabel.Text = Res.GetString("C8A09F79-D660-4EC1-AF0F-B0CBB72B2453", "Organization Match Threshold: {0}%",
				OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.Value.ToString(CultureInfo.InvariantCulture));
			ThresholdsLayout.Controls.Add(OrgMatchThresholdLabel);
		}

		void AddAddressMatchThresholdLabel()
		{
			ThresholdsLayout.RowCount += 1;
			ThresholdsLayout.RowStyles.Add(new RowStyle());
			AddressMatchThresholdLabel.Text = Res.GetString("9ED286A2-9592-4FFE-95E9-AF0C43C75665", "Address Match Threshold: {0}%",
				OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.Value.ToString(CultureInfo.InvariantCulture));
			ThresholdsLayout.Controls.Add(AddressMatchThresholdLabel);
		}

#if DEBUG
		public List<UXMLMatchingDiagnosticModel> UXMLMatchingDiagnosticModelsForTest { get; set; }
#endif

		protected void ClearResultsAndSetToParticipant()
		{
			MatchingLogTextBox.Text = string.Empty;
			uxmlMatchedListUserControl.ClearPanel();
			DebuggerScoringResults.Clear();
			MonitoringObjects.Clear();
			DeduplicationMonitoringUserControl.SetDataContext(MonitoringObjects);
			var participant = DebuggerHub.FindParticipant(DebuggerName);

			if (participant == null || !participant.Equals(this))
			{
				DeduplicationUtils.DebuggerHubInstance.Register(this);
			}
		}

		void ProcessFoundResultsLog(bool foundMatchedResult)
		{
			MatchingLogTextBox.Text += (foundMatchedResult ? (EZC.NoResString)"Found matched result" : (EZC.NoResString)"No matched result") + System.Environment.NewLine;
		}

		void ProcessLoggerMessage(ISimpleLogger logger)
		{
			logger.Logs.ForEach(u => MatchingLogTextBox.AppendText(u.Type + " - " + u.Message + System.Environment.NewLine));
		}

		#region Set Enter Values Text Box Max Length

		void InputValueTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (InputValueTabControl.SelectedTab == EnterValuesTabPage)
			{
				InputValueTabControl.SelectedIndexChanged -= InputValueTabControl_SelectedIndexChanged;
				SetEnterValuesTextBoxMaxLength();
			}
		}

		void SetEnterValuesTextBoxMaxLength()
		{
			var organizationProperties = typeof(OrganizationAddress).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var countryProperties = typeof(UniversalDataBuss.DataObjects.Universal.Country).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var portProperties = typeof(UNLOCO).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var regNumberTypeProperties = typeof(UNLOCO).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			SetTextBoxMaxLength(OrganizationCodeTextBox, organizationProperties, nameof(OrganizationAddress.OrganizationCode));
			SetTextBoxMaxLength(OrganizationNameTextBox, organizationProperties, nameof(OrganizationAddress.CompanyName));
			SetTextBoxMaxLength(AddressCodeTextBox, organizationProperties, nameof(OrganizationAddress.AddressShortCode));
			SetTextBoxMaxLength(AdditionalAddressTextBox, organizationProperties, nameof(OrganizationAddress.AdditionalAddressInformation));
			SetTextBoxMaxLength(Address1TextBox, organizationProperties, nameof(OrganizationAddress.Address1));
			SetTextBoxMaxLength(Address2TestBox, organizationProperties, nameof(OrganizationAddress.Address2));
			SetTextBoxMaxLength(CountryTextBox, countryProperties, nameof(OrganizationAddress.Country.Code));
			SetTextBoxMaxLength(CityTextBox, organizationProperties, nameof(OrganizationAddress.City));
			SetTextBoxMaxLength(PostCodeTextBox, organizationProperties, nameof(OrganizationAddress.Postcode));
			SetTextBoxMaxLength(StateTextBox, organizationProperties, nameof(OrganizationAddress.State));
			SetTextBoxMaxLength(EmailTextBox, organizationProperties, nameof(OrganizationAddress.Email));
			SetTextBoxMaxLength(FaxTextBox, organizationProperties, nameof(OrganizationAddress.Fax));
			SetTextBoxMaxLength(PhoneTextBox, organizationProperties, nameof(OrganizationAddress.Phone));
			SetTextBoxMaxLength(PortTextBox, portProperties, nameof(OrganizationAddress.Port.Code));
			SetTextBoxMaxLength(RegNumberType, regNumberTypeProperties, nameof(OrganizationAddress.GovRegNumType.Code));
			SetTextBoxMaxLength(RegNumberCode, organizationProperties, nameof(OrganizationAddress.GovRegNum));
			SetTextBoxMaxLength(UniversalNettingCodeTextBox, organizationProperties, nameof(OrganizationAddress.UniversalNettingCode));
			SetTextBoxMaxLength(UniversalOfficeCodeTextBox, organizationProperties, nameof(OrganizationAddress.UniversalOfficeCode));
			SetTextBoxMaxLength(ContactNameTextBox, organizationProperties, nameof(OrganizationAddress.Contact));
		}

		void SetTextBoxMaxLength(ZTextBox textBox, PropertyInfo[] properties, string propertyName)
		{
			var maxLength = 1000;

			if (properties.FirstOrDefault(u => u.Name == propertyName)?.GetCustomAttribute(typeof(MaxLengthAttribute)) is MaxLengthAttribute attribute && attribute.MaxLength > 0)
			{
				maxLength = attribute.MaxLength;
			}

			textBox.MaxLength = maxLength;
		}

		#endregion

		protected OrganizationAddress OrganizationAddressInfo
		{
			get
			{
				OrganizationAddress address;

				try
				{
					if (InputValueTabControl.SelectedTab == XmlValuesTabPage)
					{
						if (UXMLMatchingDiagnosticUtils.Format(XMLValuesTextBox))
						{
							var logger = new XMLMatchingDummyLogger(IsFromSameSystemCheckBox.Checked);
							address = UXMLMatchingDiagnosticUtils.GetOrganizationAddressFromXMLString(XMLValuesTextBox.Text, logger);

							if (logger.Logs.Any(u => u.Type == Integration.LogType.Error))
							{
								Globals.Message.Show(Res.GetString("3E7C5611-5F31-40D0-BA9B-2747A0B69742", "Please fix the XML values error before doing UXML matching."));
								address = null;
							}

							ProcessLoggerMessage(logger);
						}
						else
						{
							address = null;
						}
					}
					else
					{
						address = new OrganizationAddress(DefaultDataObjectWriterStrategy.Instance)
						{
							AddressShortCode = AddressCodeTextBox.Text,
							AdditionalAddressInformation = AdditionalAddressTextBox.Text,
							Address1 = Address1TextBox.Text,
							Address2 = Address2TestBox.Text,
							City = CityTextBox.Text,
							CompanyName = OrganizationNameTextBox.Text,
							Contact = ContactNameTextBox.Text,
							Country = new UniversalDataBuss.DataObjects.Universal.Country { Code = CountryTextBox.Text },
							Email = EmailTextBox.Text,
							Fax = FaxTextBox.Text,
							GovRegNum = RegNumberCode.Text,
							GovRegNumType = new RegistrationNumberType() { Code = RegNumberType.Text },
							OrganizationCode = OrganizationCodeTextBox.Text,
							Phone = PhoneTextBox.Text,
							Port = new UNLOCO { Code = PortTextBox.Text },
							Postcode = PostCodeTextBox.Text,
							State = StateTextBox.Text,
							UniversalNettingCode = UniversalNettingCodeTextBox.Text,
							UniversalOfficeCode = UniversalOfficeCodeTextBox.Text
						};
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(ex.Message);
					address = null;
				}

				return address;
			}
		}

		protected UXMLMatchedListUserControl uxmlMatchedListUserControl;
	}
}
