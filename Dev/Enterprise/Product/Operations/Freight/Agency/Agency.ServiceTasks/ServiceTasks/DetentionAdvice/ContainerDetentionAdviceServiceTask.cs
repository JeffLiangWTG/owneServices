using System;
using System.Globalization;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

//#define FunctionalTest
[assembly: HostedService
(
	ContainerDetentionAdviceServiceTask.Code,
	ContainerDetentionAdviceServiceTask.Description,
	ServiceTaskConstants.Category,
	typeof(ContainerDetentionAdviceServiceTask),
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1week",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Saturday },
	DefaultScheduleStartAtLocal = "6hours",
	CanRunInAnyBranch = true
)]

// Can't apply HostedServiceBusinessObjectBinding
// The service task uses 'overdue' logic, so it does the processing when an event didn't happen before current time.
namespace Enterprise.Freight.Agency.ServiceTasks
{
	public sealed partial class ContainerDetentionAdviceServiceTask : ServiceProviderImpl
	{
		public const string Code = "CDA";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "applied to an attribute")]
		public const string Description = "Container Detention Advice Sender";

		public override void RunTask(CancellationToken token)
		{
			ZDateTime asAt = ZDateTime.Today;

			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany())
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					delivery = AgencyRegistry.Instance.DetentionAdviceBehaviour.Value;
					if (VerifyConfigurationIsValid())
					{
						SendAllDetentionAdvicesForCompany(branch.Company, asAt);
					}
				}
			}
		}

		void SendAllDetentionAdvicesForCompany(GlbCompany company, ZDateTime asAt)
		{
			// note: the advices returned may use different factories.
			foreach (DetentionAdviceHeader advice in new ContainerDetentionAdviceProvider(company, asAt))
			{
				SendDetentionAdvice(advice);
			}
		}

		void SendDetentionAdvice(DetentionAdviceHeader advice)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.AgencyDtnAdvice));
			filter.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.Documents);
			filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Detention Advice");

			DocumentCommand command = advice.Factory.LoadTop1<DocumentCommand>(filter);
			command.Parent = advice;

			DocAutoDelivery autoDelivery = new DocAutoDelivery();
			UserControlProviderList providerList = new UserControlProviderList();

			DocDeliveryContactCollection contacts = ProcessContacts(autoDelivery.GetDeliveryContacts(command, advice.DocumentSupporter));
			if (contacts.Count > 0)
			{
				using (DocumentPack pack = new DocumentPack(command, advice, providerList, command))
				{
					DeliveryInstructions instructions = new DeliveryInstructions(pack);
					instructions.AllowAutoDelivery = true;
					instructions.Destination = DeliveryInstructionDestination.Auto;
					instructions.PrinterDelivery.PrintQueuePK = delivery.Printer;
					instructions.Recipients.RemoveAll();
					instructions.Recipients.AddRange(contacts);
					instructions.ExcludeDocumentsWhichContainNoBusinessObjectDataRows = true;

					using (DocumentPrintSet set = new DocumentPrintSet(command, providerList))
					{
						set.Run(instructions);
					}
				}
			}
		}

		DocDeliveryContactCollection ProcessContacts(DocDeliveryContactCollection inContacts)
		{
			switch (delivery.Mode)
			{
				case DetentionAdviceDeliveryMode.Codes.Auto:
					return ProcessContacts_Auto(inContacts);

				case DetentionAdviceDeliveryMode.Codes.Print:
					return ProcessContacts_Print(inContacts);

				case DetentionAdviceDeliveryMode.Codes.Notify:
					return ProcessContacts_Notify(inContacts);

				default:
					throw new InvalidOperationException("Dont know how to deliver: " + delivery.Mode);
			}
		}

		DocDeliveryContactCollection ProcessContacts_Notify(DocDeliveryContactCollection inContacts)
		{
			DocDeliveryContactCollection outContacts = new DocDeliveryContactCollection(inContacts.Factory);
			AddFromNotificationGroup(outContacts);
			return outContacts;
		}

		DocDeliveryContactCollection ProcessContacts_Print(DocDeliveryContactCollection inContacts)
		{
			foreach (DocDeliveryContact contact in inContacts)
			{
				contact.DeliveryMethod = ContactNotifyModes.Print;
			}

			if (delivery.SendToNotificationGroup)
			{
				AddFromNotificationGroup(inContacts);
			}

			return inContacts;
		}

		DocDeliveryContactCollection ProcessContacts_Auto(DocDeliveryContactCollection inContacts)
		{
			if (delivery.SendToNotificationGroup)
			{
				AddFromNotificationGroup(inContacts);
			}

			return inContacts;
		}

		void AddFromNotificationGroup(DocDeliveryContactCollection contacts)
		{
			GlbGroup group = contacts.Factory.Load<GlbGroup>(delivery.NotificationGroup);

			if (group != null)
			{
				foreach (GlbStaff staff in group.Staff)
				{
					if (staff.IsCancelled)
					{
						continue;
					}

					if (staff.GS_EmailAddress.IsEmpty)
					{
						continue;
					}

					DocDeliveryContact contact = new DocDeliveryContact(contacts.Factory);
					contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					contact.Name = staff.GS_FullName;
					contact.Email = staff.GS_EmailAddress;
					contacts.Add(contact);
				}
			}
		}

		bool VerifyConfigurationIsValid()
		{
			var factory = new BusinessObjectFactory();

			bool needsPrinter;
			bool needsGroup;

			switch (delivery.Mode)
			{
				case DetentionAdviceDeliveryMode.Codes.Auto:
				case DetentionAdviceDeliveryMode.Codes.Print:
					needsPrinter = true;
					needsGroup = delivery.SendToNotificationGroup;
					break;

				case DetentionAdviceDeliveryMode.Codes.Notify:
					needsPrinter = false;
					needsGroup = true;
					break;

				default:
					throw new InvalidOperationException("Unknown delivery mode " + delivery.Mode);
			}

			IRegistryItemInternals item = AgencyRegistry.Instance.DetentionAdviceBehaviour;
			bool success = true;

			if (needsPrinter && factory.Load<StmPrintQueue>(delivery.Printer) == null)
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Unable to find an approperate printer to print to. Please check the '{0}' registry option.", item.Location)); // Service logger
				success = false;
			}

			if (needsGroup && factory.Load<GlbGroup>(delivery.NotificationGroup) == null)
			{
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Unable to find an approperate notification group to send to. Please check the '{0}' registry option.", item.Location)); // Service logger
				success = false;
			}

			return success;
		}

		DetentionAdviceDelivery delivery;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "raise error CS0759 if removed, need to implement declaration of partial method")]
		partial void SetupLicence(GlbCompany company);
	}
}

#region Test
#if DEBUG

#region Test Helper Methods

namespace Enterprise.Freight.Agency.ServiceTasks
{
	partial class ContainerDetentionAdviceServiceTask
	{
		partial void SetupLicence(GlbCompany company)
		{
			if (SetupLicenceForTest != null)
			{
				SetupLicenceForTest(company);
			}
		}

		internal Action<GlbCompany> SetupLicenceForTest { get; set; }
	}
}

#endregion


#endif
#endregion
