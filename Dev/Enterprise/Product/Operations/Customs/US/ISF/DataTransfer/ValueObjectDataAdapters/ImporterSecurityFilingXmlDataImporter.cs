using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.DataTransfer
{
	public class ImporterSecurityFilingXmlDataImporter : XmlDataImporter
	{
		public ImporterSecurityFilingXmlDataImporter()
			: base(new ImporterSecurityFilingDataAdapter())
		{
		}

		public ImporterSecurityFilingXmlDataImporter(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider, new ImporterSecurityFilingDataAdapter())
		{
		}

		public new ImporterSecurityFilingDataAdapter Adapter
		{
			get { return (ImporterSecurityFilingDataAdapter)base.Adapter; }
		}

		protected override void ImportXml(System.IO.TextReader reader, BusinessObjectFactoryProvider factoryProvider, INotifications notifications)
		{
			Adapter.AutoSendToCustomsList = Env.CurrentUser.IsBatchProcessor ? new Dictionary<ZGuid, Xsd.ISFActionType>() : null;
			base.ImportXml(reader, factoryProvider, notifications);
		}

		protected override void OnAfterImportData(NotificationBuffer buffer, bool sucessfullyImported)
		{
			base.OnAfterImportData(buffer, sucessfullyImported);
			if (Adapter.AutoSendToCustomsList != null)
			{
				if (sucessfullyImported)
				{
					SendToCustomsIfNeeded(buffer);
				}
				Adapter.AutoSendToCustomsList = null;
			}
		}

		void SendToCustomsIfNeeded(NotificationBuffer buffer)
		{
			foreach (KeyValuePair<ZGuid, Xsd.ISFActionType> pair in Adapter.AutoSendToCustomsList)
			{
				ZGuid headerPK = pair.Key;
				CusISFHeader header = FactoryProvider.Current.Load<CusISFHeader>(headerPK);
				if (header != null)
				{
					UpdateActionCode actionCode = pair.Value == Xsd.ISFActionType.Delete ? UpdateActionCode.Delete : header.ShouldSendAdd ? UpdateActionCode.Add : UpdateActionCode.Replace;
					string messageError;
					if (IsOkToSendToCustoms(header, pair.Value, out messageError))
					{
						SendToCustoms(buffer, FactoryProvider, header, actionCode);
					}
					else
					{
						NotifyNotAbleToSendToCustoms(header, actionCode, messageError);
					}
				}
			}
		}

		void NotifyNotAbleToSendToCustoms(CusISFHeader header, UpdateActionCode actionCode, string messageError)
		{
			string uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ImporterSecurityFiling, header.PK.ToGuid());
			HtmlResponseEmailGenerator emailGenerator = new HtmlResponseEmailGenerator();
			EmailDef email;
			if (emailGenerator.TryGenerateEmail("Automatically Send To Customs Failed for " + header.HumanReadableName, "Send '" + GetActionCode(actionCode) + "' To Customs for <a href=\"" + uri + "\">" + header.HumanReadableName + "</a>", "The system was unable to automatically send this ISF Job to Customs as requested via the XML import interface.<br />" + messageError + "<br />Please review the ISF Job and send it to Customs if needed.", "", "", out email))
			{
				Env.OutgoingCustomsMailManager.CreateAndSave(email, ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup.GetFallBackValueAtAllLevels(header.RegistryCompanyPK, header.RegistryBranchPK, Guid.Empty),
					GroupSourceLocator.GetFromRegistryItem(ISFRegistry.Instance.ImporterSecurityFilingXMLImportNotificationGroup));
			}
		}

		string GetActionCode(UpdateActionCode actionCode)
		{
			switch (actionCode)
			{
				case UpdateActionCode.Add:
					return "ADD";
				case UpdateActionCode.Replace:
					return "REPLACE";
				case UpdateActionCode.Delete:
					return "DELETE";
				default:
					return "";
			}
		}

		void SendToCustoms(NotificationBuffer buffer, BusinessObjectFactoryProvider factoryProvider, CusISFHeader header, UpdateActionCode actionCode)
		{
			var builder = new ImporterSecurityFilingMessageBuilder<ABIInputBlockControlGenerator, APLB, APLY>(header, actionCode);
			var message = builder.PopulateMessage();

			try
			{
				ZExceptionReporting.ProcessWithConcurrencyHandling(
					() => { factoryProvider.SaveCurrentAndCreateNew(); },
					() =>
					{
						message.Delete();
						if (header.Messages.All(x => x.IsInDatabase))
						{
							builder.PopulateMessage();
						}
					}
					);
			}
			catch (ZSaveException ex)
			{
				buffer.Add(ErrorType.PostToDatabaseError, ex.Message);
				factoryProvider.CreateNewWithoutSave();
			}
		}

		bool IsOkToSendToCustoms(CusISFHeader header, Xsd.ISFActionType actionType, out string errorMessage)
		{
			bool result = true;
			errorMessage = "";
			if (header.IsWaitingForResponse)
			{
				result = false;
				errorMessage = ISFJobWatingForCustomsResponseMessageError;
			}
			else
			{
				header.LoadChildEditableObjects();
				header.RunPreSaveValidation();
				UnlinkRoutingIfThereIsError(header.Transports);
				if (header.HasErrors || (actionType != Xsd.ISFActionType.Delete && header.HasMessageErrors))
				{
					result = false;
					errorMessage = ISFJobHasErrorMessageError;
				}
				else if (actionType == Xsd.ISFActionType.Delete && !header.CanSendDelete)
				{
					result = false;
					errorMessage = ISFJobNotCleared;
				}
			}
			return result;
		}

		void UnlinkRoutingIfThereIsError(TransportCollection transports)
		{
			foreach (Transport transport in transports)
			{
				if (transport.JW_IsLinked && transport.HasErrors())
				{
					transport.JW_IsLinked = false;
					transport.Validation.ValidateAll();
					transport.JW_IsLinked = transport.HasErrors();
				}
			}
		}

		internal static string ISFJobWatingForCustomsResponseMessageError
		{
			get { return Res.GetString("e3eba3ff-c5fe-455c-b518-1d7cace20d68", "A new message to Customs cannot be sent for this ISF job till a response has been received for the existing message."); }
		}

		internal static string ISFJobHasErrorMessageError
		{
			get { return Res.GetString("a7936162-89ff-4609-b25e-7f385b4ac08d", "A message to Customs cannot be sent for this ISF job till all errors are fixed."); }
		}

		internal static string ISFJobNotCleared
		{
			get { return Res.GetString("09099da7-5121-44da-9e30-788097484a81", "A 'Delete' message to Customs cannot be sent for this ISF job till this Job is cleared by Customs."); }
		}
	}
}
