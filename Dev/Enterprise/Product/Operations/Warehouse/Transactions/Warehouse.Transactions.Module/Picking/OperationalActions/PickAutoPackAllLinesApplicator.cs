using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickAutoPackAllLinesApplicator : WhsOperationalActionMethodApplicator
	{
		public PickAutoPackAllLinesApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("b06bc76b-d2c3-4ca7-ab5f-17640c00f912", "Auto-Pack Picks"), factory) // text used for logging
		{
		}

		const string OutputTextFormat = "{0} {1} - {2}"; // eg. Pick 123 - Was Packed ok.

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] picks)
		{
			log.SetSectionProgressMax(picks.Length);
			var currentSection = 0;

			var ignorePicks = false;
			if (picks.Cast<WhsPick>().Any(p => !p.IsWorkOrderPick && p.Orders.Cast<WhsOrder>().Any(o => o.PackageJob != null && o.PackageJob.Packages.Count > 0)))
			{
				ignorePicks = AskUserWhetherToIgnorePicks() == DialogResult.No;
			}

			foreach (WhsPick pick in picks)
			{
				if (pick != null)
				{
					BumpSectionProgress();
					var continueBatch = TryToAutoPack(pick, ignorePicks, log);

					if (!continueBatch)
					{
						while (currentSection < picks.Length)
						{
							BumpSectionProgress();
						}
						break;
					}
				}
			}

			void BumpSectionProgress()
			{
				currentSection++;
				log.BumpSectionProgress();
			}
		}

		DialogResult AskUserWhetherToIgnorePicks()
		{
			var message = Res.GetString("8ff82a0e-dc13-4a42-9d20-885b2db83e04",
@"Some picks have orders that are already packed, Do you wish to Auto-Pack the remaining orders on these picks? 
Choosing No will skip these picks.");
			return Globals.Message.Show(message, Res.GetString("2f243e33-41ea-42b2-a3b8-27642af75bab", "Auto-Pack Pick"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
		}

		#region Auto-Pack Picks

		bool TryToAutoPack(WhsPick pick, bool ignorePicks, IOperationalActionSectionLog log)
		{
			var continueBatch = true;

			var pickLink = GetPickNoLink(pick);
			if (pick.IsWorkOrderPick)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pick.HumanReadableName, pickLink,
					Res.GetString("49439aaf-72dc-48ad-8c26-e90567448e58", "is a Work Order Pick and cannot be Auto-Packed"));
			}
			else if (pick.IsCancelled)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pick.HumanReadableName, pickLink,
					Res.GetString("ddbddb57-91af-asdf-86f5-bfdab319651g", "is canceled and cannot be Auto-Packed"));
			}
			else if (pick.IsFinalised)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pick.HumanReadableName, pickLink,
					Res.GetString("ca52a92b-fdasf-4f97-8fad-422g5c4c9e43", "is already finalized and cannot be Auto-Packed."));
			}
			else if (pick.Orders.Count == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pick.HumanReadableName, pickLink,
					Res.GetString("02928247-d4c2-4b0a-83b6-sdagasdf", "has no Orders attached and cannot be Auto-Packed."));
			}
			else if (ignorePicks && pick.Orders.Cast<WhsOrder>().Any(o => o.PackageJob != null && o.PackageJob.Packages.Count > 0))
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pick.HumanReadableName, pickLink,
					Res.GetString("cfdc4145-3605-4563-8f08-4b6760e8f9ef", "has no Orders that have been packed and was skipped."));
			}
			else
			{
				continueBatch = AutoPackPick(pick, log, pickLink);
			}

			return continueBatch;
		}

		bool AutoPackPick(WhsPick pick, IOperationalActionSectionLog log, LogControllerLink pickLink)
		{
			var continueBatch = true;
			var notify = new NotificationBuffer();
			if (pick.AutoPackOrdersAndPrintLabels(notify))
			{
				if (notify.HasWarnings)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pick.HumanReadableName, pickLink, notify.AsString);
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, OutputTextFormat, pick.HumanReadableName, pickLink,
						Res.GetString("e96d16bd-9f0d-asdfasdf-9146-00563893ce45", "was successfully Auto-Packed."));
				}
			}
			else
			{
				if (notify.HasErrors)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Error, OutputTextFormat, pick.HumanReadableName, pickLink,
					Res.GetString("5d864119-8fff-4c37-923a-69d787bb45f0", "had the following errors when trying to Auto-Pack:\r\n{0}", notify.AsString));

					continueBatch = false;
				}
				else
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, pick.HumanReadableName, pickLink,
					Res.GetString("5d864119-8fff-4c37-923a-69d787bb45f0", "had the following errors when trying to Auto-Pack:\r\n{0}", notify.AsString));
				}
			}

			return continueBatch;
		}

		#endregion

		#region Validation

		public PickAutoPackAllLinesApplicatorValdidation Validation => new PickAutoPackAllLinesApplicatorValdidation(this);

		#endregion
	}

	#region FinalisePicksActionMethodApplicatorValidation

	/// <summary>
	/// This class is unnecessary and only exists to satisfy the Z-test for IObsoleteValidation.
	/// </summary>
	public class PickAutoPackAllLinesApplicatorValdidation : ZValidation
	{
		public PickAutoPackAllLinesApplicatorValdidation(PickAutoPackAllLinesApplicator parent)
			: base(parent)
		{
		}

		public override Type AutoValidationType => typeof(PickAutoPackAllLinesApplicator);

		public override void ValidateAll()
		{
		}
	}

	#endregion
}
