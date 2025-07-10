using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.eTail.Business.DeniedPartyScreening;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.eTail.Business.DeniedPartyScreening.ProfileHeader;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVDpsResultForm : ZChildForm, IHVLVDpsResultForm
	{
		public HVLVDpsResultForm(List<DpsResponseWithScreeningParty> parties, BusinessObjectFactory factory) : this(new HVLVDpsResult(parties, factory))
		{
			loadedParties = new Dictionary<ZGuid, BusinessObject>();
		}

		public HVLVDpsResultForm(HVLVDpsResult result) : base(result)
		{
			DpsResult = result;
			InitializeComponent();
			lableRiskLevel.DataBindings.Add(new KBinding(nameof(lableRiskLevel.IsVisibleForBinding), result, "ResponseWithScreeningParties.ProfileHeaderCollection.ShowTitle"));
			gridMatchSourceListNames.MouseDoubleClick += (obj, arg) =>
			{
				if (obj is ZArchitecture.ZGrid grid)
				{
					var complianceList = grid.ListManager.GetCurrent() as RefComplianceList;
					ZControllerFactory.Create(ControllerIDs.RefComplianceList).ShowEditForm(complianceList);
				}
			};
			gridScreenedParties.ColourDeciding += GridScreenedParties_ColourDeciding;
			gridProfiles.AfterBind += GridScreenedParties_CurrentCellChanged;
		}

		readonly Dictionary<ZGuid, BusinessObject> loadedParties;

		readonly HVLVDpsResult DpsResult;

		void GridScreenedParties_CurrentCellChanged(object sender, System.EventArgs e)
		{
			if (gridScreenedParties.ListManager != null)
			{
				if (gridScreenedParties.ListManager.Count > 0)
				{
					var currentDpsMatch = gridScreenedParties.ListManager.GetCurrent() as HVLVDpsMatch;
					if (currentDpsMatch != null && currentDpsMatch.ProfileHeaderCollection.Count > 0)
					{
						GridProfiles_CurrentCellChanged(this, System.EventArgs.Empty);
					}
				}
			}
		}

		void GridScreenedParties_ColourDeciding(object sender, ZArchitecture.ColourDecidingEventArgs e)
		{
			if (e.ObjectAtRow is HVLVDpsMatch match)
			{
				switch (match.Status)
				{
					case ScreeningStatusesList.Codes.Clear:
						e.Colour = DeniedPartyConstants.GridColor.Clear;
						break;
					case ScreeningStatusesList.Codes.Matched:
						e.Colour = DeniedPartyConstants.GridColor.Matched;
						break;
					case ScreeningStatusesList.Codes.NotScreened:
						e.Colour = DeniedPartyConstants.GridColor.NotScreened;
						break;
					default:
						e.Colour = DeniedPartyConstants.GridColor.Unknown;
						break;
				}
			}
		}

		void GridProfiles_CurrentCellChanged(object sender, System.EventArgs e)
		{
			if (gridProfiles.ListManager != null)
			{
				if (gridProfiles.ListManager.Count > 0)
				{
					var header = gridProfiles.ListManager.GetCurrent() as ProfileHeader;
					switch (header.Level)
					{
						case RiskLevel.Medium:
							lableRiskLevel.BackColor = System.Drawing.Color.Orange;
							break;
						case RiskLevel.High:
							lableRiskLevel.BackColor = System.Drawing.Color.Red;
							break;
						default:
							break;
					}
				}
			}
		}

		void SaveAndCloseButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.Factory.Save();
			Close();
		}

		void SaveButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.Factory.Save();
		}

		void ClearSelectedButton_Click(object sender, System.EventArgs e)
		{
			var selectedMatches = GetSelectedMatches();

			if (selectedMatches.Length == 0)
			{
				return;
			}

			HVLVDpsClearingReason clearingReason = null;

			if (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.Value.RequireReasonForCLR)
			{
				clearingReason = new HVLVDpsClearingReason(selectedMatches, DpsResult.Factory);
				var result = ZFormModaliser.ShowDialogAndDispose(new HVLVDpsClearingReasonForm(clearingReason));

				if (result != System.Windows.Forms.DialogResult.OK)
				{
					return;
				}
			}

			foreach (var match in selectedMatches)
			{
				DpsResult.UpdateCount(match.Status, ScreeningStatusesList.Codes.Clear);

				match.Cleared = true;

				if (clearingReason != null)
				{
					match.ClearingReason = clearingReason.ClearingReason;
					match.ClearingReasonTitle = clearingReason.ClearingReasonTitle;
					match.ClearingReasonText = clearingReason.ClearingReasonText;
				}

				UpdateScreenedPartyStatus(match, ScreeningStatusesList.Codes.Clear);
			}

			DpsResult.RefreshCountPropertyInfo();
		}

		void MatchSelectedButton_Click(object sender, System.EventArgs e)
		{
			foreach (var match in GetSelectedMatches())
			{
				DpsResult.UpdateCount(match.Status, ScreeningStatusesList.Codes.Matched);

				match.Matched = true;
				match.ClearingReason = string.Empty;
				match.ClearingReasonTitle = string.Empty;
				match.ClearingReasonText = string.Empty;

				UpdateScreenedPartyStatus(match, ScreeningStatusesList.Codes.Matched);
			}

			DpsResult.RefreshCountPropertyInfo();
		}

		HVLVDpsMatch[] GetSelectedMatches()
		{
			var selectedMatches = gridScreenedParties.GetSelectedElements<HVLVDpsMatch>();

			if (selectedMatches.Length > 0)
			{
				return selectedMatches;
			}

			if (gridScreenedParties.ListManager != null && gridScreenedParties.ListManager.Count > 0)
			{
				return new[] { gridScreenedParties.ListManager.GetCurrent() as HVLVDpsMatch };
			}

			return System.Array.Empty<HVLVDpsMatch>();
		}

		void UpdateScreenedPartyStatus(HVLVDpsMatch match, string newStatus)
		{
			var parent = match.ParentBusinessObject;
			var factory = BusinessEntity.Factory;

			if (!loadedParties.TryGetValue(parent.PK, out var party))
			{
				var query = new ZQuery(parent.PKSchemaColumn, parent.PK);
				party = factory.Exists(parent.GetType(), query) || ReferenceEquals(factory, parent.Factory)
					? parent
					: factory.ImportFromAnotherFactory(parent);
				loadedParties.Add(parent.PK, party);
			}

			if (party is IScreeningPartyProvider screeningPartyProvider)
			{
				screeningPartyProvider.ScreeningStatus = newStatus;
			}
		}
	}
}
