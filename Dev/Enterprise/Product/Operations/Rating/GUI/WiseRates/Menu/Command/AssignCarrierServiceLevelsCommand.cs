using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class AssignCarrierServiceLevelsCommand : IWiseRatesCommand
	{
		public AssignCarrierServiceLevelsCommand(ZGrid gridBoundToWiseEntryViewList)
		{
			this.Grid = gridBoundToWiseEntryViewList;
		}

		public AssignCarrierServiceLevelsCommand(ZForm form) {
			this.Form = form;
		}

		readonly ZGrid Grid;
		readonly ZForm Form;

		#region IWiseRatesCommand

		bool IWiseRatesCommand.Assign(WiseEntryView wiseEntryView, object sender)
		{
			var wiseRatingHeaderView = Grid.DataSource as WiseRatingHeaderView;
			if (wiseRatingHeaderView == null)
			{
				return false;
			}

			var carrierPK = ((INeedCodeMappings)wiseEntryView).CarrierOrgHeaderPK;
			var carrier = new BusinessObjectFactory().Load<OrgHeader>(carrierPK);

			return Assign(carrier, UnmappedCarrierServiceLevels(wiseRatingHeaderView, wiseEntryView), wiseRatingHeaderView.SearchResponse?.RatesSearchResponse?.ServiceLevels);
		}

		public bool Assign(OrgHeader carrier, string universalServiceLevel) =>
			Assign(carrier, [new UnmappedForeignCode(universalServiceLevel, Core.Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel)], []);

		bool Assign(OrgHeader carrier, IEnumerable<UnmappedForeignCode> foreignCodes, WiseRates.Api.Model.RefServiceLevel[] serviceLevels)
		{
			var parentForm = Form ?? Grid.GetParent<ZForm>();

			return WiseRatesGUIHelper.EnhanceUserPermissionToRunAction
			(
				Res.GetString("0B0D1B01-305E-4DF2-BBE5-1D6778890B84", "Carrier Service Levels"),
				(securityCore) => securityCore.OrgCarrierModify,
				false,
				parentForm.CaptionResourceString.Caption,
				() =>
				{
					var form = new OrganizationFormForCarrierServiceLevelsMapping(
						parentForm,
						carrier,
						serviceLevels,
						foreignCodes
					);

					return form.ShowModal();
				}
			);
		}

		bool IWiseRatesCommand.IsEnabled(WiseEntryView wiseEntryView) => UnmappedCarrierServiceLevels(Grid?.DataSource as WiseRatingHeaderView, wiseEntryView).Any();

		#endregion

#if DEBUG
		// for testing
		public
#endif
		static IEnumerable<UnmappedForeignCode> UnmappedCarrierServiceLevels(WiseRatingHeaderView header, WiseEntryView selectedRate)
		{
			if (header == null || selectedRate == null)
			{
				return Enumerable.Empty<UnmappedForeignCode>();
			}

			var carrierPK = ((INeedCodeMappings)selectedRate).CarrierOrgHeaderPK;
			if (carrierPK.IsEmpty)
			{
				return Enumerable.Empty<UnmappedForeignCode>();
			}

			return header.WiseEntryViews
				.Cast<INeedCodeMappings>()
				.Where(x => x.CarrierOrgHeaderPK == carrierPK)
				.SelectMany(x => x.UnmappedCodes)
				.Where(x => x.Relationship == Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel)
				.Distinct()
				.OrderBy(x => x.ForeignCode);
		}
	}
}
