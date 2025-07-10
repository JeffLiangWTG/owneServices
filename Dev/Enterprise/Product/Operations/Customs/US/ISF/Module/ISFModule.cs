using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using AutoJobDeclaration = Enterprise.Customs.Business.AutoJobDeclaration;

namespace Enterprise.Customs.US.ISF.Module
{
	public class ISFModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ImporterSecurityFiling;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.ImporterSecurityFiling);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ISFFilterControl((CusISFHeaderCollection)GridCollection, (ISFFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusISFHeaderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ISFFilterBusinessObject();
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem("From &XML", ImportXmlISFs);
			AddInterfaceConnectorExportMenuItem("To &XML", ExportXmlISFs);
		}

		void ImportXmlISFs(object sender, EventArgs args)
		{
			var director = new XmlDataTransferDirector(new ImporterSecurityFilingDataAdapter(), true);
			director.PromptUserAndImport(BillingInterfaceName.ISFXmlImport);
		}

		void ExportXmlISFs(object sender, EventArgs args)
		{
			var exporter = new XmlDataTransferExporter(new ImporterSecurityFilingDataAdapter(), true);
			var businessObjects = GetSelectedBusinessObjects();
			if (businessObjects.Length == 0)
			{
				businessObjects = GridCollection.ToArray();
			}
			exporter.PromptUserAndExport(businessObjects);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem(CreateFromShipmentMenuName, OnCreateFromShipment_Click));
			if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
			{
				result.Add(new ZMenuItem(CreateNewDeclarationMenuName, OnCreateNewDeclaration_Click));
				result.Add(new ZMenuItem(CreateNewShipmentMenuName, new EventHandler(OnCreateNewShipment_Click)));
			}
			return result.ToArray();
		}
		internal const string CreateFromShipmentMenuName = "Create From Shipment";
		internal const string CreateNewDeclarationMenuName = "Create A New Declaration";
		internal const string CreateNewShipmentMenuName = "Create A New Shipment";
		internal const string SelectOneImporterSecurityFiling = "Please select one Importer Security Filing for Shipment creation.";
		internal const string SelectAtLeastOneImporterSecurityFiling = "Please select at least one Importer Security Filing";

		void OnCreateFromShipment_Click(object sender, EventArgs e)
		{
			ShipmentCreator.Reset();

			using (var creatorDialog = new ISFFromShipmentCreatorDialog(ShipmentCreator))
			{
				ZFormModaliser.ShowDialogWithoutDispose(creatorDialog);
				if (creatorDialog.CreateClicked)
				{
					var header = ShipmentCreator.Create(new BusinessObjectFactory());
					if (header != null)
					{
						var controller = ZControllerFactory.Create(ControllerIDs.ImporterSecurityFiling);
						controller.ShowFormForNewEntity(header);
						header.HasChanges = true;
					}
				}
			}
		}

		ISFFromShipmentCreator ShipmentCreator => shipmentCreator ?? (shipmentCreator = new ISFFromShipmentCreator(Factory));
		ISFFromShipmentCreator shipmentCreator;

		void OnCreateNewShipment_Click(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(SelectOneImporterSecurityFiling);
			}
			else
			{
				ZFormModaliser.ShowDialogAndDispose(new ISFHeaderAndBIllSelectorForm(new ISFHeaderRow((CusISFHeader)Grid.SelectedElements[0])));
			}
		}

		void OnCreateNewDeclaration_Click(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowError(SelectAtLeastOneImporterSecurityFiling);
			}
			else
			{
				foreach (CusISFHeader selectedHeader in Grid.SelectedElements)
				{
					if (IsOkToCreateNewDeclaration(selectedHeader))
					{
						var headerPK = selectedHeader.PK;

						HandleShowingFormSafely(() =>
						{
							var declaration = new JobDeclarationCreator(selectedHeader.PK).CreateDeclaration();
							if (declaration != null)
							{
								var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
								controller.ShowFormForNewEntity(declaration);
								declaration.HasChanges = true;
							}
						});
					}
				}
			}
		}

		bool IsOkToCreateNewDeclaration(CusISFHeader selectedHeader)
		{
			var existingDeclarations = new Dictionary<CusISFBill, ZString[]>();
			foreach (var bill in selectedHeader.ReferenceDatas)
			{
				if (bill.IsOceanBillOfLading || bill.IsHouseBillOfLading || bill.IsMasterBillOfLading)
				{
					var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
					var billQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
					billQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, bill.BB_BillNum);
					declarationQuery.AddSubQuery(billQuery, JoinCondition.Or);
					var filter = JobDeclarationFilter.ForCountry(false, Constants.CountryCodes.UnitedStates, Factory);
					filter.AddToFilter(declarationQuery);
					filter.OrderBy = AutoJobDeclaration.Schema.JE_DeclarationReference;
					var declarationList = new List<ZString>();
					foreach (var existingDeclaration in Factory.Load<JobDeclaration>(filter))
					{
						declarationList.Add(existingDeclaration.JE_DeclarationReference);
					}

					if (declarationList.Count > 0)
					{
						existingDeclarations.Add(bill, declarationList.ToArray());
					}
				}
			}
			return existingDeclarations.Count == 0 || Globals.Message.ShowConfirmation(GetExistingDeclarationMessage(existingDeclarations), "Declaration Matching Bill Number Exist", "Please type the following if you still want to create a new declaration: ", "yes", MessageBoxIcon.Warning) == DialogResult.OK;
		}

		string GetExistingDeclarationMessage(Dictionary<CusISFBill, ZString[]> existingDeclarations)
		{
			var builder = new ZStringBuilder();
			builder.Append(string.Format("The following bill number{0} already in use on following declaration(s):", existingDeclarations.Count > 1 ? "s are" : " is"));
			builder.Append("");
			foreach (var pair in existingDeclarations)
			{
				builder.Append(pair.Key.BB_BillTypeDescription + ":" + pair.Key.BB_BillNum);
				var declarationReferences = new ZStringBuilder();
				var declarationCount = 0;
				foreach (var declarationReference in pair.Value)
				{
					declarationReferences.Append(declarationReference);
					if (++declarationCount % 10 == 0)
					{
						builder.Append(declarationReferences.ToStringWithDelimiterBetweenAppends(", "));
						declarationReferences = new ZStringBuilder();
					}
				}
				if (!declarationReferences.IsEmpty)
				{
					builder.Append(declarationReferences.ToStringWithDelimiterBetweenAppends(", "));
				}
				builder.Append("");
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		#region Licence checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImporterSecurityFiling;

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ImporterSecurityFiling;

		#endregion

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode;
	}
}
