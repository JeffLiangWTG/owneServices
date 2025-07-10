using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Module
{
	public class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl() : base()
		{
		}

		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(module, gridCollection, filterStripBusinessObject)
		{
			InitialiseGridColumns();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, JobInvoicingConsumerTypes.Brokerage.Code, false, GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
		}

		protected override ZBool ShouldAddCustomFieldsToGrid
		{
			get { return false; }
		}

		void InitialiseGridColumns()
		{
			var outwardTransportModeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outwardTransportModeTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("918C15C6-EB4A-4B86-AB3D-94EF146B0590", "Outward Trans.");
			ControlDpiScalingHelper.SetWidth(ref outwardTransportModeTextBoxColumnStyleInfo, 85, true);
			outwardTransportModeTextBoxColumnStyleInfo.ColumnName = SGAddInfoSchema.Constants.SG_OutwardTransportMode;
			FilteredGrid.ColumnStyles.Add(outwardTransportModeTextBoxColumnStyleInfo);

			var outwardVesselTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outwardVesselTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("C7BFFCE1-1D56-4793-B7B7-DE339C9C09B8", "Outward Vessel");
			ControlDpiScalingHelper.SetWidth(ref outwardVesselTextBoxColumnStyleInfo, 90, true);
			outwardVesselTextBoxColumnStyleInfo.ColumnName = SGAddInfoSchema.Constants.SG_OutwardVesselName;
			FilteredGrid.ColumnStyles.Add(outwardVesselTextBoxColumnStyleInfo);

			var outwardVoyageFlightNoTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outwardVoyageFlightNoTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("445F3345-200A-46A3-91EC-F62C40C12218", "Outward Voyage/Flight");
			ControlDpiScalingHelper.SetWidth(ref outwardVoyageFlightNoTextBoxColumnStyleInfo, 130, true);
			outwardVoyageFlightNoTextBoxColumnStyleInfo.ColumnName = SGAddInfoSchema.Constants.SG_OutwardVoyageFlightNo;
			FilteredGrid.ColumnStyles.Add(outwardVoyageFlightNoTextBoxColumnStyleInfo);

			var outwardHAWBTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outwardHAWBTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("649E748A-4A2B-4C6A-A823-96A8DE447396", "Outward HBL");
			outwardHAWBTextBoxColumnStyleInfo.ColumnName = SGAddInfoSchema.Constants.SG_OutwardHAWB;
			FilteredGrid.ColumnStyles.Add(outwardHAWBTextBoxColumnStyleInfo);

			var outwardMAWBTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outwardMAWBTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("83C829A9-9EDF-47B5-92E0-3C9678DDC5F6", "Outward MBL");
			outwardMAWBTextBoxColumnStyleInfo.ColumnName = SGAddInfoSchema.Constants.SG_OutwardMAWB;
			FilteredGrid.ColumnStyles.Add(outwardMAWBTextBoxColumnStyleInfo);

			var outwardCarrierAgentTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outwardCarrierAgentTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("A1F13D45-CD55-4574-BBEA-EA04815C7784", "Outward Carrier/Agent");
			outwardCarrierAgentTextBoxColumnStyleInfo.ColumnName = "OutwardShippingLineForwarderDocAddress+Organisation+OH_Code";
			ControlDpiScalingHelper.SetWidth(ref outwardCarrierAgentTextBoxColumnStyleInfo, 120, true);
			FilteredGrid.ColumnStyles.Add(outwardCarrierAgentTextBoxColumnStyleInfo);

			var applicationProductTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			applicationProductTypeTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("136AD8C5-6876-46CB-868E-EA6448AB55B6", "Product Type");
			applicationProductTypeTextBoxColumnStyleInfo.ColumnName = SGAddInfoSchema.Constants.SG_ApplicationProductType;
			FilteredGrid.ColumnStyles.Add(applicationProductTypeTextBoxColumnStyleInfo);

			var certificateNumberTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			certificateNumberTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("E9EBF768-3CA7-4849-9336-33CB1D65ABD1", "Certificate No.");
			certificateNumberTextBoxColumnStyleInfo.ColumnName = JobDeclaration.Schema.CertificateNumber;
			FilteredGrid.ColumnStyles.Add(certificateNumberTextBoxColumnStyleInfo);

			ZTextBoxColumnStyleInfo messageTypeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			messageTypeTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("24389206-7EF7-4CE6-B662-22C8FF297487", "Message Type");
			ControlDpiScalingHelper.SetWidth(ref messageTypeTextBoxColumnStyleInfo, 93, true);
			messageTypeTextBoxColumnStyleInfo.ColumnName = JobDeclaration.Schema.JE_ApplicationCode;
			FilteredGrid.ColumnStyles.Add(messageTypeTextBoxColumnStyleInfo);

			if (IsAccessEnabled)
			{
				var globalManifestStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				globalManifestStatusTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("D4C6802A-0409-461C-925B-922F64DA6625", "Manifest Status");
				globalManifestStatusTextBoxColumnStyleInfo.GroupName = Res.GetData("132D95D0-302D-49D0-BB2D-E5E59CC902EB", "Manifest Details");
				ControlDpiScalingHelper.SetWidth(ref globalManifestStatusTextBoxColumnStyleInfo, 99, true);
				globalManifestStatusTextBoxColumnStyleInfo.ColumnName = JobDeclaration.Schema.ManifestStatus;
				FilteredGrid.ColumnStyles.Add(globalManifestStatusTextBoxColumnStyleInfo);

				var globalManifestStatusDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				globalManifestStatusDescriptionTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("B1F501B4-6040-4573-9574-2ECD7972D2DB", "Manifest Status Description");
				globalManifestStatusDescriptionTextBoxColumnStyleInfo.GroupName = Res.GetData("132D95D0-302D-49D0-BB2D-E5E59CC902EB", "Manifest Details");
				ControlDpiScalingHelper.SetWidth(ref globalManifestStatusDescriptionTextBoxColumnStyleInfo, 160, true);
				globalManifestStatusDescriptionTextBoxColumnStyleInfo.ColumnName = JobDeclaration.Schema.ManifestStatusDescription;
				FilteredGrid.ColumnStyles.Add(globalManifestStatusDescriptionTextBoxColumnStyleInfo);
			}

			ReOrderGridColumns(FilteredGrid);
		}

		void ReOrderGridColumns(ZGrid grid)
		{
			var newColumnOrder = GetNewColumnOrderForGrid(grid);

			if (newColumnOrder != null && newColumnOrder.Count > 0)
			{
				grid.ColumnStyles.Clear();
				grid.ColumnStyles.AddRange(newColumnOrder);
			}
		}

		ArrayList GetNewColumnOrderForGrid(ZGrid grid)
		{
			var columnNames = ColumnNamesInSortOrder;
			var result = new ArrayList(columnNames.Length);

			foreach (string columnName in columnNames)
			{
				var column = grid.GetColumnStyle(columnName);
				ModifyColumnProperties(column);
				result.Add(column);
			}
			return result;
		}

		ZString[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					ArrayList columns = new ArrayList();
					columns.AddRange(DefaultColumnsForGrid);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_DateAtFinalDestination);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_GS_NKCusAgent);
					columns.Add((ZString)JobDeclaration.Schema.CertificateNumber);
					columns.Add((ZString)SGAddInfoSchema.Constants.SG_ApplicationProductType);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_VesselName);
					columns.Add((ZString)SGAddInfoSchema.Constants.SG_OutwardVesselName);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_VoyageFlightNo);
					columns.Add((ZString)SGAddInfoSchema.Constants.SG_OutwardVoyageFlightNo);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_DateOfArrival);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_AgentsReference);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_ApplicationCode);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_ContainerCount);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_ContainerMode);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_DateOfFirstArrival);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_EntryAuthorisationDate);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_EntrySubmittedDate);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_ExportDate);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_OwnerRef);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_RL_NKPortOfFirstArrival);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading);
					columns.Add((ZString)JobDeclaration.Schema.JE_ETAOfDischarge);
					columns.Add((ZString)JobDeclaration.Schema.JE_ETDOfLoading);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalNoOfPacks);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalNoOfPacksPackType);
					columns.Add((ZString)JobDeclaration.Schema.DeclarationNumber);
					columns.Add((ZString)JobDeclaration.Schema.OrderNumbers);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalVolume);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalVolumeUnit);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalWeight);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_TotalWeightUnit);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc);
					columns.Add((ZString)JobDeclarationSchema.Constants.JE_RS_NKServiceLevel);
					columns.Add((ZString)JobDeclaration.Schema.BrokerName);
					columns.Add((ZString)JobDeclaration.Schema.ImporterName);
					columns.Add((ZString)JobDeclaration.Schema.SupplierName);
					columns.Add((ZString)JobDeclaration.Schema.ForwarderName);
					columns.Add((ZString)JobDeclaration.Schema.JE_OH_ControllingAgent);
					columns.Add((ZString)JobDeclaration.Schema.JE_OH_ControllingCustomer);
					columns.Add((ZString)JobDeclaration.Schema.JE_OH_ExternalBroker);
					columns.Add((ZString)JobDeclaration.Schema.AuditDate);
					columns.Add((ZString)JobDeclaration.Schema.AuditReference);
					columns.Add((ZString)JobDeclaration.Schema.AuditLogUser);
					columns.Add((ZString)JobDeclaration.Schema.AuditLogUserName);
					columns.Add((ZString)JobDeclaration.Schema.BillingBranch);
					columns.Add((ZString)JobDeclaration.Schema.BillingDepartment);
					columns.Add((ZString)JobDeclaration.Schema.BillingOperator);
					columns.Add((ZString)JobDeclaration.Schema.BillingTaxBranch);
					columns.Add((ZString)"WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent");
					columns.Add((ZString)"WorkflowItems+Milestones+LastMilestone+P9_Description");
					columns.Add((ZString)"WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding");
					columns.Add((ZString)"WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent");
					columns.Add((ZString)"WorkflowItems+Milestones+NextMilestone+P9_Description");
					columns.Add((ZString)"WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding");
					columns.Add((ZString)"OutwardShippingLineForwarderDocAddress+Organisation+OH_Code");
					columns.Add((ZString)"Job+JH_Status");
					if (IsAccessEnabled)
					{
						columns.Add((ZString)JobDeclaration.Schema.ManifestStatus);
						columns.Add((ZString)JobDeclaration.Schema.ManifestStatusDescription);
					}

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemCreateUser);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemCreateTimeUtc);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemLastEditUser);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.JE_SystemLastEditTimeUtc);

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCode);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientName);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddressAsString);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddressShortCode);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddress1);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientAddress2);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCity);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientState);
					AddColumnByColumnStyle(columns, JobDeclaration.Schema.LocalClientCountry);

					AddColumnByColumnStyle(columns, JobDeclaration.Schema.RelatedTransportBookingsJobNumbers);

					columnNamesInSortOrder = (ZString[])columns.ToArray(typeof(ZString));
				}

				return columnNamesInSortOrder;
			}
		}
		ZString[] columnNamesInSortOrder;

		void AddColumnByColumnStyle(ArrayList columns, string columnName)
		{
			if (grid.GetColumnStyle(columnName) != null)
			{
				columns.Add((ZString)columnName);
			}
		}

		ZString[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					defaultColumnsForGrid = new List<ZString>();
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_MessageType);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_MessageSubType);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_DeclarationReference);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_TransportMode);
					defaultColumnsForGrid.Add(SGAddInfoSchema.Constants.SG_OutwardTransportMode);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_EntryStatus);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_OH_Importer);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_OH_Supplier);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_RL_NKOrigin);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_RL_NKFinalDestination);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_HouseBill);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_MasterBill);
					defaultColumnsForGrid.Add(SGAddInfoSchema.Constants.SG_OutwardHAWB);
					defaultColumnsForGrid.Add(SGAddInfoSchema.Constants.SG_OutwardMAWB);
					defaultColumnsForGrid.Add(JobDeclarationSchema.Constants.JE_GoodsDescription);
					if (IsAccessEnabled)
					{
						defaultColumnsForGrid.Add(JobDeclaration.Schema.ManifestStatus);
						defaultColumnsForGrid.Add(JobDeclaration.Schema.ManifestStatusDescription);
					}
				}
				return defaultColumnsForGrid.ToArray();
			}
		}
		List<ZString> defaultColumnsForGrid;

		void ModifyColumnProperties(ZGridColumnInfo column)
		{
			if (column != null)
			{
				switch (column.ColumnName)
				{
					case JobDeclarationSchema.Constants.JE_MessageType:
						ControlDpiScalingHelper.SetWidth(ref column, 40, true);
						break;
					case JobDeclarationSchema.Constants.JE_MessageSubType:
						column.CaptionResourceString = Res.GetData("5754B392-9A92-4DC6-9096-D77C8C8E803C", "Dec. Type");
						ControlDpiScalingHelper.SetWidth(ref column, 50, true);
						break;
					case JobDeclarationSchema.Constants.JE_HouseBill:
						column.CaptionResourceString = Res.GetData("405EE9C7-11BB-4FD0-B201-931105F5EEBB", "Inward HBL");
						ControlDpiScalingHelper.SetWidth(ref column, 80, true);
						break;
					case JobDeclarationSchema.Constants.JE_MasterBill:
						column.CaptionResourceString = Res.GetData("E0EFD650-D289-45BB-8073-0267F83C0D37", "Inward MBL");
						ControlDpiScalingHelper.SetWidth(ref column, 80, true);
						break;
					case JobDeclarationSchema.Constants.JE_GoodsDescription:
						ControlDpiScalingHelper.SetWidth(ref column, 130, true);
						break;
					case JobDeclarationSchema.Constants.JE_TransportMode:
						column.CaptionResourceString = Res.GetData("1BCEEE5B-26BD-483B-B251-250D11E592DA", "Inward Trans.");
						ControlDpiScalingHelper.SetWidth(ref column, 80, true);
						break;
					case JobDeclarationSchema.Constants.JE_VesselName:
						column.CaptionResourceString = Res.GetData("4F30953C-10DA-48B1-84FA-9078FC23A6DF", "Inward Vessel");
						ControlDpiScalingHelper.SetWidth(ref column, 90, true);
						break;
					case JobDeclarationSchema.Constants.JE_VoyageFlightNo:
						column.CaptionResourceString = Res.GetData("0087CCF8-D646-44E1-B8E5-AC7CFB0C5676", "Inward Voyage/Flight");
						ControlDpiScalingHelper.SetWidth(ref column, 130, true);
						break;
					case JobDeclarationSchema.Constants.JE_ContainerMode:
						column.CaptionResourceString = Res.GetData("3FB508EB-CDB1-408B-998C-F5CE012F1927", "Cargo Packing");
						ControlDpiScalingHelper.SetWidth(ref column, 80, true);
						break;
					case JobDeclaration.Schema.DeclarationNumber:
						column.CaptionResourceString = Res.GetData("384A2472-20C7-442F-9872-C6AB7A213D0D", "Permit No.");
						break;
				}
				column.IsVisible = DefaultColumnsForGrid.Contains(column.ColumnName);
			}
		}

		bool IsAccessEnabled
		{
			get
			{
				if (!isAccessEnabled.HasValue)
				{
					isAccessEnabled = SGCustomsDataRegistry.Instance.ACCESSEnable.Value;
				}
				return isAccessEnabled.Value;
			}
		}
		bool? isAccessEnabled;
	}
}
