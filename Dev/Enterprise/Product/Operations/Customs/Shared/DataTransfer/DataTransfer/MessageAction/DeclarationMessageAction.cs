using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationMessageAction : ImportMessageAction
	{
		public DeclarationMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new DeclarationXmlDataImporter(FactoryProvider, DeclarationValueObjectDataAdapter.New());
		}

		protected override IGlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.CustomsDeclarationImportNotificationGroup.Value));
			}
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("662a8c43-c4c6-4b74-91b6-94bd408bf5a8", "System->Registry->Notification->Custom Declaration Import Notification Group"); }
		}

		protected override bool ExecuteActionCore(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant)
		{
			if (message.Interchange != null)
			{
				string branchCode = string.Empty;

				if (!message.Interchange.EI_HeaderText.IsEmpty)
				{
					var doc = XDocument.Parse(message.Interchange.EI_HeaderText);
					var branchCodeElenent = doc.XPathSelectElement((NoResString)"/*[local-name()='InterchangeInfo' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Target' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='BranchCode' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']");
					if (branchCodeElenent != null)
					{
						branchCode = branchCodeElenent.Value;
					}
				}

				if (string.IsNullOrEmpty(branchCode))
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("df7da22b-0b10-43a1-b52a-b101e57f3411", "EDI Message {0} Branch Code is not provided - custom declaration could not be created.", message.EM_MessageNum)));
					participant = new List<ITransactionParticipant>();
					return false;
				}
				else if (!BranchExists(branchCode))
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("539a317c-4b6c-41fc-82e4-c75c22054331", "EDI Message {0} Branch Code '{1}' is not valid branch code - custom declaration could not be created.", message.EM_MessageNum, branchCode)));
					participant = new List<ITransactionParticipant>();
					return false;
				}

				return base.ExecuteActionCore(message, notifications, out participant);
			}

			participant = new List<ITransactionParticipant>();
			return false;
		}

		bool BranchExists(string branchCode)
		{
			var query = new ZQuery(GlbBranchSchema.GB_Code, branchCode);
			query.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_IsActive, true);

			return FactoryProvider.Current.Load<GlbBranch>(query).Length > 0;
		}
	}
}
