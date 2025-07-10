using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using static Enterprise.Integration.Customs.US.ISF;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class USISFWebMessageSender : IUSISFWebMessageSender
	{
		public USISFWebMessageSender() : this(new ISFMessageBuilderFactory())
		{
		}

		public USISFWebMessageSender(IISFMessageBuilderFactory messageBuilderFactory)
		{
			this.messageBuilderFactory = messageBuilderFactory;
		}
		readonly IISFMessageBuilderFactory messageBuilderFactory;

		public string SendUpsertMessage(ZGuid headerPK)
		{
			return SendMessageCore(headerPK, false);
		}

		public string SendDeleteMessage(ZGuid headerPK)
		{
			return SendMessageCore(headerPK, true);
		}

		string SendMessageCore(ZGuid headerPK, bool delete)
		{
			var factory = new BusinessObjectFactory();
			var header = factory.Load<CusISFHeader>(headerPK);

			var result = SendMessageFromHeader(header, delete);

			if (result == null)
			{
				try
				{
					factory.Save();
				}
				catch (ZSaveException ex)
				{
					return string.IsNullOrEmpty(ex.FriendlyMessage) ? ex.GetInnermostException().Message : ex.FriendlyMessage;
				}
			}

			return result;
		}

		string SendMessageFromHeader(CusISFHeader header, bool delete)
		{
			if (header == null)
			{
				return Res.GetString("13139C9F-5CEC-436C-8B0B-F581BEB9F3E3", "Header was not found");
			}

			if (!CanSendMessage(header, out var validationResult))
			{
				return validationResult;
			}

			if (header.BF_CustomsStatus == MessageStatusList.Codes.ClearISFDelete)
			{
				return Res.GetString("57AFFF1B-4BD5-41E0-A00B-1D513DD13663", "Cannot send further messages as this Customs Reference '{0}' has been deleted from Customs system", header.BF_CustomsReference);
			}

			if (delete && !header.CanSendDelete)
			{
				return Res.GetString("58A53B3F-C649-4F37-AEA5-67E23A4DB07F", "Cannot send 'Delete' message as message hasn't been cleared yet.");
			}

			var updateActionCode = GetUpdateActionCode(header, delete);
			var builder = messageBuilderFactory.CreateWebMessageBuilder(header, updateActionCode);
			var message = builder.PopulateMessage();

			if (message != null)
			{
				message.EM_SendWithMessageErrors = false;
				message.EM_ApplicationReference = header.BF_CustomsReference;

				return null;
			}

			return Res.GetString("9FD6185B-7828-4D25-9577-237DFF02A3AD", "Message was not created");
		}

		bool CanSendMessage(CusISFHeader header, out string validationResult)
		{
			header.LoadChildEditableObjects();
			header.RunPreSaveValidation();

			if (header.HasErrors || header.HasMessageErrors)
			{
				var collector = new CustomsNotificationCollector(header, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName);
				var errors = collector.GetErrors();
				var messageErrors = collector.GetMessageErrors();
				var allErrors = errors.Concat(messageErrors);

				validationResult = Res.GetString("F270E56F-FFDF-4D8A-9D30-FC296E0EC953", "Please fix the following errors before continuing: \r\n {0}", allErrors.ToUniqueMessageListString());

				return false;
			}

			validationResult = null;

			return true;
		}

		UpdateActionCode GetUpdateActionCode(CusISFHeader header, bool delete)
		{
			if (delete)
			{
				return UpdateActionCode.Delete;
			}

			return header.ShouldSendAdd ? UpdateActionCode.Add : UpdateActionCode.Replace;
		}
	}
}
