using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	sealed class EBondMessageSendingValidation : MessageSendingValidation
	{
		public EBondMessageSendingValidation(JobDeclaration declaration)
			: base(declaration, GetMessageErrors(declaration))
		{
		}

		protected override MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore()
		{
			var message = Res.GetString("05F2A656-51E2-4F83-A4D7-4E6383EB4266", "It is likely that your message(s) will be rejected by Surety Agent, as they have the following message errors:");
			return CheckBusinessObjectLevelValidationCore(ErrorExistHeaderText, message, MessageErrorConfirmationQuestionText);
		}

		static IEnumerable<INotification> GetMessageErrors(JobDeclaration declaration)
		{
			var portCode = declaration.ProcessingPortCodeForQuery;

			if (string.IsNullOrWhiteSpace(portCode))
			{
				yield return new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("41b8a974-c6ae-4e19-9c6e-d175e36a0b16"
					, "{0}: Please add a valid processing district port code at {1}."
					, declaration.HumanReadableName
					, USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.Location()));
			}

			var filerCode = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode;

			if (filerCode.IsEmpty)
			{
				yield return new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("ae48dc88-9c6e-4005-9b8c-920982b44371"
					, "{0}: Please add a valid entry filer code at {1}."
					, declaration.HumanReadableName
					, USCustomsDataRegistry.Instance.EntryFiler.Location()));
			}

			if (declaration.US_BondAmount <= 0)
			{
				var propertyDescription = declaration.US_BondAmountInfo.HumanReadableName;

				yield return new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("ea2c07ed-d7fb-4c0a-b048-70a379ca790a"
					, "{0}: {1}"
					, propertyDescription
					, MandatoryValidation.ValueCannotBeZeroMessage(propertyDescription)));
			}

			if (declaration.DecEntryNumber.IsEmpty)
			{
				var propertyDescription = declaration.DecEntryNumberInfo.HumanReadableName;

				yield return new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("12431af4-fc5e-4979-a654-4e7e18e8ee53"
					, "{0}: {0} doesn't allocate a valid number."
					, propertyDescription));
			}

			var notifications = new EBondNotificationCollector(declaration).GetMessageErrors();

			foreach (var notification in notifications)
			{
				yield return notification;
			}
		}

		#region EBondNotificationCollector

		sealed class EBondNotificationCollector : CustomsNotificationCollector
		{
			public EBondNotificationCollector(JobDeclaration declaration)
				: base(declaration, true, false, PropertyDescriptionType.HumanReadableName)
			{
			}

			protected override bool ShouldIncludeNotificationsFromInfo(ZPropertyInfo info)
			{
				return InfosForInclude.Any(c => c == info.Name);
			}

			protected override bool ShouldIncludeNotificationsFromObject(BusinessObject businessObject)
			{
				var type = businessObject.GetType();

				return type == typeof(JobDeclaration)
					|| type == typeof(AddInfoJobDeclaration)
					|| type == typeof(JobComInvoiceLine);
			}

			string[] InfosForInclude => infosForInclude ??
										(
											infosForInclude = new[]
											{
												AutoJobDeclaration.Schema.US_SuretyCode,
												AutoJobDeclaration.Schema.US_InsuranceAgent,
												JobDeclaration.Schema.IOROrgPK,
												AutoJobDeclaration.Schema.US_EntryType,
												AutoJobDeclaration.Schema.US_BondAmount,
												AutoJobDeclaration.Schema.US_BondProducerAccNo,
												AutoJobDeclaration.Schema.US_BondDesignationCode,
												JobDeclaration.Schema.DecEntryNumber,

												AutoJobComInvoiceLine.Schema.JI_Tariff,
												AutoJobComInvoiceLine.Schema.US_UC_NKCountryOfOrigin,
												AutoJobComInvoiceLine.Schema.US_SPI
											}
										);
			string[] infosForInclude;
		}

		#endregion
	}
}
