using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.GUI;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		public JobDeclarationModule()
			: base()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("From &BIRD", BirdUpdate);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetGuiProviders(Factory);
			return new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		}

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			if (ModuleResultsPKCollection == null)
			{
				ModuleResultsPKCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.JobDeclaration);
			}
			return base.ShowEditForm(selectedBusinessObject);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = base.GetNewStandardMenuItems();

			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add("New Declaration", HandleNewClick);
				NewMenuItem.MenuItems.Add("New Protest", ShowNewProtestForm);
				NewMenuItem.MenuItems.Add("New Reconciliation", ShowNewReconForm);
				NewMenuItem.MenuItems.Add("New Drawback", ShowNewDrawbackForm);
			}

			return menuItems;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => JobDeclarationZControllerDecider.GetZController(selectedBusinessObject as JobDeclaration) ?? base.GetNewController(selectedBusinessObject);

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			MenuItem requestGroup = new ZMenuItem("Reference Files Request");
			result.Add(requestGroup);

			MenuItem requestACEAntiDumpingMenuItem = new ZMenuItem("Anti-Dumping and Countervailing(ACE)");
			requestACEAntiDumpingMenuItem.Click += new EventHandler(requestACEAntiDumpingMenuItem_Click);
			requestGroup.MenuItems.Add(requestACEAntiDumpingMenuItem);

			MenuItem requestCarrierCodeMenuItem = new ZMenuItem("Carrier Codes");
			requestCarrierCodeMenuItem.Click += new EventHandler(requestCarrierCodeMenuItem_Click);
			requestGroup.MenuItems.Add(requestCarrierCodeMenuItem);

			MenuItem requestCountryMenuItem = new ZMenuItem("Country Codes");
			requestCountryMenuItem.Click += new EventHandler(requestCountryMenuItem_Click);
			requestGroup.MenuItems.Add(requestCountryMenuItem);

			MenuItem requestFIRMSCodeMenuItem = new ZMenuItem("FIRMS Codes");
			requestFIRMSCodeMenuItem.Click += new EventHandler(requestFIRMSCodeMenuItem_Click);
			requestGroup.MenuItems.Add(requestFIRMSCodeMenuItem);

			MenuItem requestForeignPortCodesMenuItem = new ZMenuItem("Foreign Port Codes");
			requestForeignPortCodesMenuItem.Click += new EventHandler(requestForeignPortCodesMenuItem_Click);
			requestGroup.MenuItems.Add(requestForeignPortCodesMenuItem);

			MenuItem requestQueryTariffsMenuItem = new ZMenuItem("Tariffs");
			requestQueryTariffsMenuItem.Click += new EventHandler(requestQueryTariffMenuItem_Click);
			requestGroup.MenuItems.Add(requestQueryTariffsMenuItem);

			MenuItem requestSpecialistTeamAssignmentFile = new ZMenuItem("Request Specialist Team Assignment File", new EventHandler(RequestSpecialistTeamAssignmentFile_Click));
			requestGroup.MenuItems.Add(requestSpecialistTeamAssignmentFile);

			//DO NOT RELEASE TO GENERAL PUBLIC
			if (Env.CurrentUser.IsDeveloper)
			{
				MenuItem sendTariffAssociationMenuItem = new ZMenuItem("Send Tariff Association Test Messages(Dev. Only)");
				sendTariffAssociationMenuItem.Click += new EventHandler(SendTariffAssociationMenuItem_Click);
				result.Add(sendTariffAssociationMenuItem);

				MenuItem getRejectedAssociationMenuItem = new ZMenuItem("Get Rejected Associations(Dev. Only)");
				getRejectedAssociationMenuItem.Click += new EventHandler(GetRejectedAssociationMenuItem_Click);
				result.Add(getRejectedAssociationMenuItem);

				MenuItem getOtherRejectionsMenuItem = new ZMenuItem("Get Other Rejectons(Dev. Only)");
				getOtherRejectionsMenuItem.Click += new EventHandler(GetOtherRejectionMenuItem_Click);
				result.Add(getOtherRejectionsMenuItem);

				MenuItem updateBulkTariffQueryMenuItem = new ZMenuItem("Send Bulk Tariff Query(Dev. Only)");
				updateBulkTariffQueryMenuItem.Click += new EventHandler(updateBulkTariffQueryMenuItem_Click);
				result.Add(updateBulkTariffQueryMenuItem);
			}

			result.Add(new ZMenuItem("Calculate Duty Savings Data", CalculateDutySavingsMenuItem_Click));

			result.Add(new ZMenuItem("Run Landed Costing", RunLandedCosting_Click));

			return result.ToArray();
		}

		protected virtual BusinessObject[] GetSelectedBusinessObjectsForBulkLC() => SelectedBusinessObjects;

		protected override void AddMultiDeclarationEntryMenuItem(List<MenuItem> menuItems)
		{
		}

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		void ShowNewProtestForm(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.US.Protest);
			controller.ShowNewForm();
		}

		void ShowNewReconForm(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.US.Recon);
			controller.ShowNewForm();
		}

		void ShowNewDrawbackForm(object sender, EventArgs e)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.US.Drawback);
			controller.ShowNewForm();
		}

		void CalculateDutySavingsMenuItem_Click(object sender, EventArgs e)
		{
			if (!Env.Security.CustomsDeclarationEnquiryEdit.IsAllowed)
			{
				Env.Security.CustomsDeclarationEnquiryEdit.ShowError();
			}
			else
			{
				var selectedBusinessObjects = GetSelectedBusinessObjects().Cast<JobDeclaration>().ToArray();

				if (selectedBusinessObjects.Length == 0)
				{
					Globals.Message.ShowWarning("Please select at least one Customs Declaration to run this function.");
				}
				else
				{
					new BulkDutySavingsRunner().Run(selectedBusinessObjects);
				}
			}
		}

		void RunLandedCosting_Click(object sender, EventArgs e)
		{
			using (var licenceComponent = new BulkLandedCostingLicensedComponent())
			{
				if (Env.Licence.LandedCosting.Login(licenceComponent) == Integration.Licensing.LicenceLoginResponse.Denied)
				{
					Globals.Message.ShowWarning(Env.Licence.LandedCosting.LastReasonForNotAllowing);
				}
				else
				{
					var selectedBusinessObjects = GetSelectedBusinessObjectsForBulkLC();

					if (selectedBusinessObjects.Length == 0)
					{
						Globals.Message.ShowWarning("Please select at least one Customs Declaration to run this function.");
					}
					else
					{
						new BulkLandedCostingRunner().RunLandedCosting(selectedBusinessObjects);
					}
				}
			}
		}

		void BirdUpdate(object sender, EventArgs e)
		{
			using (var birdDataImportForm = new DataImporterForm("BIRD Import", BillingInterfaceName.BIRDImport))
			{
				birdDataImportForm.Importer = new DataTransfer.BIRDDataImporter(new BusinessObjectFactory());
				ZFormModaliser.ShowDialogWithoutDispose(birdDataImportForm);
			}
		}

		void GetRejectedAssociationMenuItem_Click(object sender, EventArgs e)
		{
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Messaging.EDIMessage);

			using (var popup = new EmbeddedModulePopup(module))
			{
				var strategy = new RejectedAssociationsStrategy(popup);
				popup.EmbeddedModulePopupOKButtonStrategy = strategy;
				ZFormModaliser.ShowDialogWithoutDispose(popup);
				if (strategy.SeletedBusinessObjects != null)
				{
					var messages = new List<MQEDIMessage>(new TypedEnumerable<MQEDIMessage>(strategy.SeletedBusinessObjects)).ToArray();

					if (messages != null)
					{
						using (var fileDialog = new ZOpenFileDialog())
						{
							fileDialog.Filter = "Text files (*.txt)|*.txt";
							fileDialog.RestoreDirectory = true;
							fileDialog.CheckFileExists = true;
							var result = fileDialog.ShowDialog(EmbeddedControl.FindForm());

							if (result == DialogResult.OK)
							{
								new TariffAssociationRejectionProcessor().CollectionWrongAssociationRejections(messages, fileDialog.ForceLocalFile());
							}

							Globals.Message.ShowInformation("Rejected associations collected in " + fileDialog.UnmappedFileName);
						}
					}
				}
			}
		}

		void GetOtherRejectionMenuItem_Click(object sender, EventArgs e)
		{
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Messaging.EDIMessage);

			using (var popup = new EmbeddedModulePopup(module))
			{
				var strategy = new RejectedAssociationsStrategy(popup);
				popup.EmbeddedModulePopupOKButtonStrategy = strategy;
				ZFormModaliser.ShowDialogWithoutDispose(popup);

				if (strategy.SeletedBusinessObjects != null)
				{
					var messages = new List<MQEDIMessage>(new TypedEnumerable<MQEDIMessage>(strategy.SeletedBusinessObjects)).ToArray();

					if (messages != null)
					{
						using (var fileDialog = new ZOpenFileDialog())
						{
							fileDialog.Filter = "Text files (*.txt)|*.txt";
							fileDialog.RestoreDirectory = true;
							fileDialog.CheckFileExists = true;
							var result = fileDialog.ShowDialog(EmbeddedControl.FindForm());

							if (result == DialogResult.OK)
							{
								new TariffAssociationRejectionProcessor().CollectRejectionsOtherThanWrongAssociations(messages, fileDialog.ForceLocalFile());
							}

							Globals.Message.ShowInformation("Rejected associations collected in " + fileDialog.UnmappedFileName);
						}
					}
				}
			}
		}

		void updateBulkTariffQueryMenuItem_Click(object sender, EventArgs e)
		{
			using (var fileDialog = new ZOpenFileDialog())
			{
				fileDialog.Filter = "Text files (*.txt)|*.txt";
				fileDialog.RestoreDirectory = true;
				fileDialog.CheckFileExists = true;
				var result = fileDialog.ShowDialog(this.EmbeddedControl.FindForm());

				string fileName = null;

				if (result == DialogResult.OK)
				{
					fileName = fileDialog.ForceLocalFile();
				}

				if (fileName != null)
				{
					using (var reader = new StreamReader(fileName))
					{
						var tariffs = new List<TariffDate>();
						var today = ZDate.Today;
						while (!reader.EndOfStream)
						{
							var aLine = reader.ReadLine();

							tariffs.Add(new TariffDate(aLine, ZString.Empty, today));
						}

						if (tariffs.Count > 0)
						{
							var factory = new BusinessObjectFactory();
							new ReferenceFileRequester(factory).RequestTariffs(tariffs, null);
							factory.Save();
							Globals.Message.ShowInformation("Message Sent");
						}
					}
				}
			}
		}

		void SendTariffAssociationMenuItem_Click(object sender, EventArgs e)
		{
			string fileName = null;

			using (var fileDialog = new ZOpenFileDialog())
			{
				fileDialog.Filter = "Text files (*.txt)|*.txt";
				fileDialog.RestoreDirectory = true;
				fileDialog.CheckFileExists = true;
				var result = fileDialog.ShowDialog(EmbeddedControl.FindForm());

				if (result == DialogResult.OK)
				{
					fileName = fileDialog.ForceLocalFile();
				}

				if (fileName != null)
				{
					var batchSize = 100;
					var batchIndex = 0;

					try
					{
						using (var reader = new StreamReader(fileName))
						{
							while (!reader.EndOfStream)
							{
								var index = 0;
								var associations = new List<ParentSecondaryTariffAssociation>();

								for (; index < batchSize; index++)
								{
									if (reader.EndOfStream)
									{
										break;
									}

									var aLine = reader.ReadLine();

									var split = aLine.Split(' ');

									if (split.Length > 1)
									{
										var association = new ParentSecondaryTariffAssociation();
										association.ParentTariff = split[0];
										association.SecondaryTariff = split[1];
										association.AdditionalSecondaryTariffs = split.Length > 2 ? new List<string>(split[2].Split(',')).ToArray() : Array.Empty<string>();
										associations.Add(association);
									}
								}

								if (associations.Count > 0)
								{
									var factory = new BusinessObjectFactory();
									new SendTestMessagesForAssociatedTariffs().Send(factory, associations, "Assoc" + batchIndex);
								}

								batchIndex++;
								index = 0;
							}
						}

						Globals.Message.ShowInformation(batchIndex + " Messages sent");
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						Globals.Message.ShowError("There is a problem. So far " + (batchSize * batchIndex) + " associations have been sent. Resolve the following problem and try continue from the next associations.\r\n\r\n" + exception.Message + "\r\n" + exception.StackTrace);
					}
				}
			}
		}

		void requestForeignPortCodesMenuItem_Click(object sender, EventArgs e)
		{
			if (Globals.Message.Show("Request Foreign Port Code update from Customs", "Foreign Port Codes Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes)
			{
				new ReferenceFileRequester().RequestForeignPortCodes();
			}
		}

		void requestQueryTariffMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(
				new QueryTariffsForm(
					new QueryTariffOption(new BusinessObjectFactory())));
		}

		void requestCarrierCodeMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new QueryCarrierForm(new QueryCarrierOption(Factory)));
		}

		void requestFIRMSCodeMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new QueryFIRMSForm(new QueryFIRMSOption(Factory)));
		}

		void requestCountryMenuItem_Click(object sender, EventArgs e)
		{
			if (Globals.Message.Show("Request Country Codes update from Customs", "Country Codes Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes)
			{
				new ReferenceFileRequester().RequestCountry();
			}
		}

		void requestACEAntiDumpingMenuItem_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new ACCaseQueryForm(new ACEACCaseQuery(new BusinessObjectFactory())));
		}

		void RequestSpecialistTeamAssignmentFile_Click(object sender, EventArgs e)
		{
			ShowDeactivatedMessage(NotSupportedByCBPForSpecialistTeamQuery);
		}

		void ShowDeactivatedMessage(string message)
		{
			Globals.Message.ShowWarning(message);
		}

		internal const string NotSupportedByCBP = "This message is no longer supported by CBP.";
		internal const string NotSupportedByCBPForSpecialistTeamQuery = "This message is no longer supported by CBP. Import Specialist Team Assignments can be found at https://www.cbp.gov/trade/centers-excellence-and-expertise-information/cee-directory.";

		sealed class RejectedAssociationsStrategy : IEmbeddedModulePopupOKButtonStrategy
		{
			public RejectedAssociationsStrategy(EmbeddedModulePopup popup)
			{
				this.popup = popup;
			}
			readonly EmbeddedModulePopup popup;

			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
			{
				if (selectedBusinessObjects == null || selectedBusinessObjects.Length == 0)
				{
					Globals.Message.ShowInformation("Please select at least one declaration.");
				}
				else
				{
					SeletedBusinessObjects = selectedBusinessObjects;
					popup.Close();
				}
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
			}

			public BusinessObject[] SeletedBusinessObjects { get; private set; }
		}
	}
}
