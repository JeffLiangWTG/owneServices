using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.ServiceTasks.PreDriveChecklist
{
	class TelematicsChecklistProcessor : IChecklistProcessor
	{
		public TelematicsChecklistProcessor()
			: this(new Lazy<IOutgoingMailManager>(ObjectFactory.Get<IOutgoingMailManager>))
		{
		}

		internal TelematicsChecklistProcessor(Lazy<IOutgoingMailManager> outgoingMailManager)
		{
			this.outgoingMailManager = outgoingMailManager ?? throw new ArgumentNullException(nameof(outgoingMailManager));
		}

		readonly Lazy<IOutgoingMailManager> outgoingMailManager;

		public int ProcessChecklists(BusinessObjectFactory factory, ICollection<TelPreDriveChecklistHeader> checklists, CancellationToken cancellationToken)
		{
			var processed = 0;
			foreach (var checklist in checklists)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				if (!checklist.AllEntriesCompleted)
				{
					SendEmail(factory, checklist);
				}
				checklist.TPH_IsProcessed = true;
				processed++;
			}

			return processed;
		}

		void SendEmail(BusinessObjectFactory factory, TelPreDriveChecklistHeader checklist)
		{
			var entries = string.Join(
				"\r\n",
				checklist.Entries
					.OrderBy(entry => entry.TPE_Index)
					.Select(
						entry =>
						{
							string agreedDescription;
							switch (entry.TPE_IsAgreed)
							{
								case TelPreDriveChecklistEntryValueTypes.Codes.Yes:
									agreedDescription = TelPreDriveChecklistEntryValueTypes.Descriptions.Yes + ":       ";
									break;
								case TelPreDriveChecklistEntryValueTypes.Codes.No:
									agreedDescription = TelPreDriveChecklistEntryValueTypes.Descriptions.No + ":        ";
									break;
								default:
									agreedDescription = TelPreDriveChecklistEntryValueTypes.Descriptions.Unknown + ":  ";
									break;
							}
							return Res.GetString("71326D6E-28DD-441B-8B4E-4EF1AC2F6D8E", "{0}{1}", agreedDescription, entry.TPE_Description);
						}));

			var email = new EmailDef
			{
				FromDisplayName = System.Environment.MachineName,
				Subject = Res.GetString(
					"E48CC0A4-0964-4EE9-B6B2-FE6736B1CE5C",
					"Incomplete Pre-Drive Checklist: {0} - {1}",
					checklist.TPH_GS_NKDriver,
					checklist.TPH_ChecklistCreateTimeUtc),
				Body = Res.GetString(
					"2D639C2C-F3BD-4775-ABE9-B4C70E605144",
					@"Driver {0} has submitted an incomplete {1} Checklist

Time: {2}

Checklist:
{3}

Notes:
{4}",
					checklist.TPH_GS_NKDriver,
					checklist.TPH_Type,
					checklist.TPH_ChecklistCreateTimeUtc,
					entries,
					checklist.TPH_Notes),
			};

			var group = factory.Load<GlbGroup>(
					new ZQuery(
						GlbGroupSchema.PK,
						TelematicsConfigurationRegistry.Instance.TelematicsChecklistEmailNotificationGroup.Value))
				.SingleOrDefault();

			if (group == null)
			{
				return;
			}

			var recipients = group.Staff
				.Select(staff => ((GlbStaff)staff).GS_EmailAddress.ToString())
				.ToArray();

			if (recipients.Length == 0)
			{
				return;
			}

			email.AddRecipientForSystemCommunication(recipients);
			outgoingMailManager.Value.Create(factory, email);
		}
	}
}
