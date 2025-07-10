using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.MeasureInfo;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.RateSelector
{
	public class RateSelectorCommand
	{
		internal static bool CanRate(RatingCriteria criteria)
		{
			var invoicingSupporter = criteria.AutoRating?.InvoicingSupporter;
			var transportMode = invoicingSupporter?.TransportMode;
			var containerMode = invoicingSupporter?.ContainerMode;
			var canRateAir = TransportModes.Air.Equals(transportMode);

			var canRateSeaOthers = (criteria.AdapterType == AdapterType.Consolidation && ContainerModes.BuyersConsol.Equals(containerMode))
									|| ContainerModes.Groupage.Equals(containerMode);

			var canRateSeaFCL = (IsSeaFCL(criteria) || canRateSeaOthers)
								&& HasFCLContainerOrEmpty(criteria.JobMeasures.GetContainerTypePKs());

			var canRateSea = TransportModes.Sea.Equals(transportMode)
				&& (canRateSeaFCL || ContainerModes.LCL.Equals(containerMode));

			var canUseRateSelector = DataRegistryRating.Instance.RatesServiceRateSelector.IsEnabled(transportMode, containerMode);

			return canUseRateSelector && (canRateAir || canRateSea);
		}

		internal static bool HasFCLContainerOrEmpty(IEnumerable<ZGuid> containerTypePKs) =>
			containerTypePKs.Any(pk => pk != ContainerInfo.LCL) || !containerTypePKs.Any();

		static bool IsSeaFCL(RatingCriteria criteria)
		{
			var invoicingSupporter = criteria.AutoRating?.InvoicingSupporter;
			var containerMode = invoicingSupporter?.ContainerMode;

			return criteria.AdapterType == AdapterType.BookingWithQuote || criteria.AdapterType == AdapterType.OneOffQuote
				? criteria.FreightMode == FreightMode.FCL
				: ContainerModes.FCL.Equals(containerMode);
		}

		internal static (bool isValid, bool continueAutorating) Validate(RatingCriteria criteria)
		{
			var ratingAdapter = criteria.GetRatingAdapter();
			var transportMode = ratingAdapter?.InvoicingSupporter?.TransportMode;
			var containerMode = ratingAdapter?.ContainerMode;

			var isAir = TransportModes.Air.Equals(transportMode);

			var manualRateSelection = ratingAdapter as IManualRateSelectionSupporter;
			var origin = manualRateSelection != null
				? manualRateSelection.DefaultFilterValueForOrigin
				: ratingAdapter?.Origin;

			if (origin == null)
			{
				Globals.Message.Show(criteria.OriginMissingMessage);
				return (false, criteria.ContinueAutoratingWithoutRateSelector);
			}

			var destination = manualRateSelection != null
				? manualRateSelection.DefaultFilterValueForDestination
				: ratingAdapter.Destination;

			if (destination == null)
			{
				Globals.Message.Show(criteria.DestinationMissingMessage);
				return (false, criteria.ContinueAutoratingWithoutRateSelector);
			}

			if (!DataRegistryRating.Instance.IsLicenseForRatesServiceValid(out _) || !DataRegistryRating.Instance.IsRateServiceSubscriptionEnabled(transportMode, containerMode, out _))
			{
				// Even though access to rates service may not be valid we still deem the validation to 
				// succeed because the rate selectors can also be used to get CW1 rates. 
				return (true, true);
			}

			var containerisedTransportContainerModes = new (string transportMode, string containerMode)[]
				{
					(TransportModes.Sea, ContainerModes.FCL),
					(TransportModes.Sea, ContainerModes.BuyersConsol),
					(TransportModes.Sea, ContainerModes.Groupage),
					(TransportModes.Air, ContainerModes.ULD),
				};

			var mandatoryContainerType = containerisedTransportContainerModes
				.Any(x => x.transportMode.Equals(transportMode) && x.containerMode.Equals(containerMode));

			if (mandatoryContainerType && criteria.RateableMeasures.GetContainerGroups().IsNullOrEmpty())
			{
				Globals.Message.Show(Res.GetString("ff4781f3-756e-4970-bb9b-64287697be96", "Container Type is mandatory for running Autorating Costs."));
				return (false, false);
			}

			if
			(
				isAir
				&& DataRegistryRating.Instance.CargoguideIntegrationEnabled.Value
				&& criteria.AdapterType == AdapterType.Consolidation
				&& ratingAdapter?.InvoicingSupporter is JobInvoicingSupporter invoicingSupporter
			)
			{
				if (invoicingSupporter.JobHeaderParent is Forwarding.IForwardingConsol consol)
				{
					if (consol.JK_PrepaidCollect.IsEmpty)
					{
						Globals.Message.Show(Res.GetString("a11d0771-7467-47b7-bae5-cbffc5d1b4ae", "Payment is mandatory for running Autorating Costs of Consol"));
						return (false, false);
					}
				}
			}

			return (true, true);
		}

		public AutoRateInfoCollection SelectRate(IRatingContext ratingContext, RatingCriteria criteria, Form parentForm)
		{
			if (CanRate(criteria))
			{
				var validation = Validate(criteria);

				if (validation.isValid)
				{
					var useGlowRateSelector = RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection();
					if (useGlowRateSelector)
					{
						return SelectRateUsingGlow(ratingContext, criteria);
					}

					var isAir = Constants.TransportModes.Air.Equals(criteria.AutoRating?.InvoicingSupporter?.TransportMode);

					return isAir
						? SelectAirRate(ratingContext, criteria, parentForm)
						: SelectSeaRate(ratingContext, criteria, parentForm);
				}

				if (validation.continueAutorating)
				{
					//for Quick Booking or Booking with Quote, if Load/Discharge is not valid, we should continue autorating
					return null;
				}

				throw new AutoRater.RatingCancelledException();
			}

			return null;
		}

		AutoRateInfoCollection SelectSeaRate(IRatingContext ratingContext, RatingCriteria criteria, Form parentForm)
		{
			var model = new RateChooserModel(criteria, ratingContext);
			var viewModel = new RateChooserViewModel(model);
			var form = new RateChooserForm(viewModel);

			if (parentForm?.IsDisposed ?? false) //shipment or consol form was closed. We should not show Rate Selector
			{
				form.Close();
				return null;
			}

			if (ZFormModaliser.ShowDialogAndDispose(form, parentForm) == System.Windows.Forms.DialogResult.OK)
			{
				return model.GetSelectedRate();
			}

			if (form.IsRateSelectionSkipped)
			{
				return null;
			}

			throw new AutoRater.RatingCancelledException();
		}

		AutoRateInfoCollection SelectAirRate(IRatingContext ratingContext, RatingCriteria criteria, Form parentForm)
		{
			using (var form = new RateSelectorForm(criteria, ratingContext))
			{
				if (ZFormModaliser.ShowDialogAndDispose(form, parentForm) == DialogResult.OK)
				{
					return form.SelectedRates;
				}

				if (form.IsRateSelectionSkipped)
				{
					return null;
				}
			}

			throw new AutoRater.RatingCancelledException();
		}

		AutoRateInfoCollection SelectRateUsingGlow(IRatingContext ratingContext, RatingCriteria criteria)
		{
			var rateSelector = new GlowRateSelector();
			var result = rateSelector.ShowDialog(ratingContext, criteria);

			return result.Outcome switch
			{
				GlowRateSelectorOutcome.ApplyRates => result.SelectedCharges,
				GlowRateSelectorOutcome.AutorateWithoutSelection => null,
				GlowRateSelectorOutcome.AbortSession => throw new AutoRater.RatingCancelledException(),
				_ => throw new AutoRater.RatingCancelledException(),
			};
		}
	}
}
