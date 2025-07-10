using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Rating.Business
{
	public class RateUpdater : AutoRateUpdater
	{
		public RateUpdater()
			: base(new BusinessObjectFactory())
		{
			ZDateTime proposedLastRunDate = Env.Registry.Rating.GRINotificationLastRunDate;

			if (proposedLastRunDate.IsValid)
			{
				LastRunDate = proposedLastRunDate;
			}
		}

		[List("Lookups.Printers")]
		public override ZGuid PrinterPK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.PrinterPK; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.PrinterPK = value; }
		}

		public override ZDateTime LastRunDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.LastRunDate; }
			set
			{
				base.LastRunDate = value;

				if (rates != null)
				{
					rates.Load(LastRunDate);
				}
			}
		}

		public UpdateRateCollection Rates
		{
			get
			{
				if (rates == null)
				{
					rates = new UpdateRateCollection(Factory);
					rates.Load(LastRunDate);
					RegisterEditableChildObject(rates);
				}

				return rates;
			}
		}
		UpdateRateCollection rates;

		public bool SendNotifications()
		{
			var notificationsSent = false;

			foreach (UpdateRate rate in Rates)
			{
				if (rate.IncludeInUpdate && rate != null && rate.ClientRate != null)
				{
					DocumentPack documentPack;
					DeliveryInstructions instructions;
					using (var printTask = GetPrintTask(rate, out documentPack, out instructions))
					{
						if (instructions.Recipients.Count > 0 && documentPack.Count > 0)
						{
							printTask.Run(instructions);
							notificationsSent = true;
						}
					}
				}
			}

			return notificationsSent;
		}

		public PrintTask GetPrintTask(UpdateRate rate, out DocumentPack documentPack, out DeliveryInstructions deliveryInstructions)
		{
			if (rate == null)
			{
				throw new ArgumentNullException(nameof(rate));
			}

			if (rate.ClientRate == null)
			{
				throw new ArgumentException("The ClientRate is not specified", nameof(rate));
			}

			documentPack = ((RatingHeaderDocumentSupporter)((IDocumentSupportable)rate.ClientRate).DocumentSupporter).BuildDocumentPack(PricingPageMenuItem);

			deliveryInstructions = BuildDeliveryInstructions(documentPack, rate);
			deliveryInstructions.ExcludeDocumentsThatHaveSectionsWhichContainNoDataRows = true;

			var printTask = new PrintTask();
			printTask.Add(documentPack);

			return printTask;
		}

		public void UpdateRegistryLastRunDate()
		{
			Env.Registry.Rating.GRINotificationLastRunDate = ZDateTime.Now.ToDateTime();
		}

		public RateUpdaterLookups Lookups
		{
			get { return lookups ?? (lookups = new RateUpdaterLookups(this)); }
		}
		RateUpdaterLookups lookups;

		#region Implementation

		DocumentCommand PricingPageMenuItem
		{
			get
			{
				if (pricingPageMenuItem == null)
				{
					ZString menuName = IncludeCoverPage ? (NoResString)"Pricing Page with Cover Page" : (NoResString)"Pricing Page"; // Filter related.
					var filter = new DocumentZQuery(BusinessContext.Rating, menuName);
					pricingPageMenuItem = Factory.LoadTop1<DocumentCommand>(filter);
				}

				return pricingPageMenuItem;
			}
		}
		DocumentCommand pricingPageMenuItem;

		DeliveryInstructions BuildDeliveryInstructions(DocumentPack documentPack, UpdateRate rate)
		{
			var instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.Auto;
			instructions.PrinterDelivery.PrintQueuePK = PrinterPK;

			foreach (DocDeliveryContact organisationAppointed in GetOrganisationAppointedContacts(documentPack))
			{
				AddRecipientIfValid(instructions, organisationAppointed);
			}

			AddRecipientIfValid(instructions, GetContactFromGlbStaff(rate.Client.StaffAssignments.OverallSalesRepStaff));
			AddRecipientIfValid(instructions, GetContactFromGlbStaff(rate.Client.StaffAssignments.OverallAccountManagerStaff));

			return instructions;
		}

		void AddRecipientIfValid(DeliveryInstructions instructions, DocDeliveryContact contact)
		{
			if (contact != null)
			{
				bool isSafe;

				switch (contact.DeliveryMethod)
				{
					case ContactNotifyModes.Email:
						isSafe = !contact.Email.IsEmpty;
						break;

					case ContactNotifyModes.Print:
						isSafe = !PrinterPK.IsEmpty;
						break;

					case ContactNotifyModes.Fax:
						isSafe = !contact.Fax.IsEmpty;
						break;

					default:
						isSafe = false;
						break;
				}

				if (isSafe)
				{
					instructions.Recipients.Add(contact);
				}
			}
		}

		DocDeliveryContactCollection GetOrganisationAppointedContacts(DocumentPack documentPack)
		{
			var docAutoDelivery = new DocAutoDelivery();
			var result = docAutoDelivery.GetDeliveryContacts(PricingPageMenuItem, documentPack.DocumentSupporter);

			return result;
		}

		DocDeliveryContact GetContactFromGlbStaff(GlbStaff staff)
		{
			if (staff == null)
			{
				return null;
			}
			else
			{
				var contact = new DocDeliveryContact(Factory);
				contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
				contact.AttachmentType = OrgConstants.AttachmentType.PDF;
				contact.Email = staff.GS_EmailAddress;
				contact.Name = staff.GS_FullName;
				return contact;
			}
		}

		#endregion
	}
}

