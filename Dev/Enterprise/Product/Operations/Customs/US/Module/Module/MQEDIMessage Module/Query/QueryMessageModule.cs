using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class QueryMessageModule : MQEDIMessageModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.QueryMessage;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.QueryMessages;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.QueryMessages);

		protected override IBusinessObjectCollection GetNewGridCollection() => new QueryTransmitMessageCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new QueryMessageFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			var columnsToHide = new string[]
				{
					MQEDIMessage.Schema.EM_ActionStatus,
					MQEDIMessage.Schema.EM_Status,
					EDIMessage.Schema.EM_SendOrReceiveHumanReadable,
					EDIMessage.Schema.EM_ApplicationCode,
					EDIMessage.Schema.EM_ApplicationReference,
					EDIMessage.Schema.EM_MessageSubType,
					EDIMessage.Schema.EM_MessageSubTypeDescription,
					EDIMessage.Schema.EM_SystemCreateUser,
					EDIMessage.Schema.EM_DateTimeInterchangeSent,
					EDIMessage.Schema.EM_InterchangeNumber,
					EDIMessage.Schema.EM_InterchangeStatus,
					EDIMessage.Schema.EM_SystemLastEditTimeUtc,
					EDIMessage.Schema.EM_SystemLastEditUser,
					EDIMessage.Schema.EM_InterchangeReceiver,
				};

			return new MQEDIMessageFilterControl(GridCollection, FilterBusinessObject, columnsToHide);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			MenuItem sendCargoManifestQuery = new ZMenuItem("Send Cargo Manifest Query", new EventHandler(SendCargoManifestQuery_Click));
			result.Add(sendCargoManifestQuery);

			MenuItem sendCensusWarningQuery = new ZMenuItem("Send Census Warning Query");
			sendCensusWarningQuery.Click += new EventHandler(sendCensusWarningQuery_Click);
			result.Add(sendCensusWarningQuery);

			MenuItem sendEntrySummaryQuery = new ZMenuItem("Send Entry Summary Query", new EventHandler(SendEntrySummaryQuery_Click));
			result.Add(sendEntrySummaryQuery);

			MenuItem sendEstablishmentIdentifierQuery = new ZMenuItem("Send Establishment Identifier (FDA) Query");
			sendEstablishmentIdentifierQuery.Click += new EventHandler(sendEstablishmentIdentifierQuery_Click);
			result.Add(sendEstablishmentIdentifierQuery);

			MenuItem sendImporterBondQuery = new ZMenuItem("Send Importer Bond Query");
			sendImporterBondQuery.Click += new EventHandler(SendImporterBondQuery_Click);
			result.Add(sendImporterBondQuery);

			MenuItem sendManufacturerNameAndAddressQuery = new ZMenuItem("Send Manufacturer Name And Address Query");
			sendManufacturerNameAndAddressQuery.Click += new EventHandler(sendManufacturerNameAndAddressQuery_Click);
			result.Add(sendManufacturerNameAndAddressQuery);

			var sendACEQuotaQuery = new ZMenuItem("Send Quota Query");
			sendACEQuotaQuery.Click += new EventHandler(sendACEQuotaVisaQuery_Click);
			result.Add(sendACEQuotaQuery);

			return result.ToArray();
		}

		protected override bool ShowSetToCompleteMenu => false;

		protected internal virtual ManufacturerQueryForm GetManufacturerQueryForm(USMIDQuery messageData) => new ManufacturerQueryForm(messageData);

		protected internal virtual QueryImporterBondForm GetQueryImporterBondForm(string messageData) => new QueryImporterBondForm(messageData);

		protected internal virtual USMIDQuery GetManufacturerQueryMessageData() => new USMIDQuery(new BusinessObjectFactory());

		protected internal virtual CargoManifestQueryHeader GetCargoManifestQueryHeader() => new CargoManifestQueryHeader(new BusinessObjectFactory());

		protected internal virtual EntrySummaryQueryBizObj GetEntrySummaryQueryBizObj() => new EntrySummaryQueryBizObj(new BusinessObjectFactory());

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		void sendACEQuotaVisaQuery_Click(object sender, EventArgs e)
		{
			var queryOption = new QueryQuotaVisaOption(new BusinessObjectFactory(), true);
			using (var quotaQueryForm = new QueryQuotaVisaOptionForm(queryOption))
			{
				ZFormModaliser.ShowDialogAndDispose(quotaQueryForm);
			}
		}

		void sendEstablishmentIdentifierQuery_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowInformation(JobDeclarationModule.NotSupportedByCBP);
		}

		void sendManufacturerNameAndAddressQuery_Click(object sender, EventArgs e)
		{
			var messageData = GetManufacturerQueryMessageData();
			using (var form = GetManufacturerQueryForm(messageData))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				if (form.IsOKToSendMessage)
				{
					ManufacturerIdentifierQueryBuilder.Generate(messageData);
					try
					{
						messageData.Factory.Save();
						Globals.Message.ShowInformation("Manufacturer Name And Address Query sent.", "Message Sent");
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		void SendImporterBondQuery_Click(object sender, EventArgs e)
		{
			using (var queryImporterBondForm = GetQueryImporterBondForm(""))
			{
				queryImporterBondForm.NumberTextBox.ReadOnly = false;
				ZFormModaliser.ShowDialogWithoutDispose(queryImporterBondForm);

				var sendMessage = queryImporterBondForm.IsOKToSendMessage;
				ZString importerBondNumber = queryImporterBondForm.NumberTextBox.Text;
				var importerNumberRequester = new ImporterNumberRequester();

				ZString messageErrorBondNumber = "";
				if (!importerNumberRequester.IsValidImporterBondNumber(importerBondNumber))
				{
					messageErrorBondNumber = "Entered Importer Bond Number " + importerBondNumber + " is not valid.\r\n" +
						SocialSecurityNumberValidator.SocialSecurityNumberRightFormat + "\r\n" +
						EmployerIdentificationNumberValidator.EINNumberRightFormat + "\r\n" +
						CBPAssignedNumberValidator.CBPAssignedNumberRightFormat;
				}
				else if (!importerNumberRequester.HasPermissionToSendImporterBondNumber(importerBondNumber))
				{
					messageErrorBondNumber = SocialSecurityNumberValidator.DoesNotHavePermissionToSendSSNErrorMessage;
				}

				if (sendMessage)
				{
					if (messageErrorBondNumber.IsEmpty)
					{
						importerBondNumber = EmployerIdentificationNumberValidator.GetValidEINForInBondMessage(importerBondNumber);
						importerNumberRequester.RequestImporterBond(null, importerBondNumber);
						Globals.Message.ShowInformation(string.Format("A request has been sent for importer number : {0}. You will receive a response email shortly.", importerBondNumber));
					}
					else
					{
						Globals.Message.ShowInformation(messageErrorBondNumber);
					}
				}
			}
		}

		void SendCargoManifestQuery_Click(object sender, EventArgs e)
		{
			var header = GetCargoManifestQueryHeader();

			using (var form = new CargoManifestQueryGridForm(header))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					var processingPortCode = USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(GlbBranch.CurrentBranch.GB_GC.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
					var processingOfficeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(GlbBranch.CurrentBranch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty);
					var actionCode = header.ActionCode;
					if (header.IsGroupQueries)
					{
						new CargoManifestStatusQueryMessageBuilder(header.Factory, actionCode, header.SendingObjects.OfType<ICargoManifestQuerySendingObject>(), processingPortCode, processingOfficeCode).GenerateMessages();
						try
						{
							header.Factory.Save();
						}
						catch (ZSaveException exception)
						{
							ZExceptionReporting.HandleSaveException(exception);
						}
					}
					else
					{
						foreach (CargoManifestQueryBizObj bizObj in header.SendingObjects)
						{
							new CargoManifestStatusQueryMessageBuilder(bizObj.Factory, actionCode, new[] { bizObj }, processingPortCode, processingOfficeCode).GenerateMessages();
							try
							{
								bizObj.Factory.Save();
							}
							catch (ZSaveException exception)
							{
								ZExceptionReporting.HandleSaveException(exception);
							}
						}
					}
					Globals.Message.ShowInformation("Cargo/Manifest Query sent.");
				}
			}
		}

		void SendEntrySummaryQuery_Click(object sender, EventArgs e)
		{
			var bizObj = GetEntrySummaryQueryBizObj();

			using (var form = new EntrySummaryQueryForm(bizObj))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);

				if (bizObj.SendMessage)
				{
					new ACEEntrySummaryQueryMessageBuilder(bizObj).PopulateMessage();

					try
					{
						bizObj.Factory.Save();
						Globals.Message.ShowInformation("Entry Summary Query sent.");
					}
					catch (ZSaveException exception)
					{
						ZExceptionReporting.HandleSaveException(exception);
					}
				}
			}
		}

		void sendCensusWarningQuery_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory();
			var action = CensusWarningQueryForm.Action.None;
			var queryData = new CensusWarningQuery(factory);
			using (var form = new CensusWarningQueryForm(queryData))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				action = form.ActionChosenByUsers;
			}

			if (action == CensusWarningQueryForm.Action.Send)
			{
				try
				{
					factory.Save();
					Globals.Message.ShowInformation("Census Warning Query Message Sent");
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}
	}
}
