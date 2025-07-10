using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingUNDGDataItemValidation : UNDGDataItemValidation
	{
		protected ForwardingUNDGDataItemValidation(AutoUNDGDataItem parent)
			: base(parent)
		{
		}

		public new ForwardingUNDGDataItem Parent => (ForwardingUNDGDataItem)base.Parent;

		#region Static New

		public static ForwardingUNDGDataItemValidation New(UNDGDataItem parent)
		{
			var substance = parent?.Substance;
			if (substance is null)
			{
				return new ForwardingUNDGDataItemValidation(parent);
			}

			switch (parent.Substance.DG_Standard)
			{
				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA:
					return new ForwardingUNDGDataItemIATAValidation(parent);

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.JTT:
					return new ForwardingUNDGDataItemJTTValidation(parent);

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR:
					return new ForwardingUNDGDataItemCFRValidation(parent);

				case UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO:
					return new ForwardingUNDGDataItemIMOValidation(parent);

				default:
					return new ForwardingUNDGDataItemValidation(parent);
			}
		}

		#endregion

		#region CheckDI_DG

		protected override void CheckDI_DG()
		{
			base.CheckDI_DG();

			var substance = Parent?.Substance;
			var shipment = Parent?.Shipment;
			if (substance == null
				|| shipment == null)
			{
				return;
			}

			ValidateForStandardMatchesAgainstShipmentTransportMode(shipment, substance);
		}

		void ValidateForStandardMatchesAgainstShipmentTransportMode(ForwardingShipment shipment, UNDGSubstance substance)
		{
			if (!ShouldCheckStandardAgainstShipment(substance))
			{
				return;
			}

			(bool standardIsMismatched, ZString correctStandard) = CheckIfStandardIsMismatched(shipment, substance);
			if (standardIsMismatched)
			{
				Parent.DI_DGInfo.AddWarning(Res.GetString("0d864f48-3d71-d292-4e48-315ca01624f8",
					"The Transport Mode of the Shipment does not match the dangerous goods Standard selected. Please reselect the dangerous goods substance from the {0} Standard.",
					correctStandard));
			}
		}

		bool ShouldCheckStandardAgainstShipment(UNDGSubstance substance)
		{
			return DGStandardCalculator
				.StandardsWithCorrespondingModes
				.Contains(substance.DG_Standard);
		}

		(bool isMismatched, ZString correctStandard) CheckIfStandardIsMismatched(ForwardingShipment parentShipment, UNDGSubstance substance)
		{
			var correspondingStandard = DGStandardCalculator
				.GetCorrespondingStandardForShipmentMode(parentShipment);

			return (substance.DG_Standard != correspondingStandard, correspondingStandard);
		}

		#endregion

		#region Class 7 (Radioactive) Substances

		bool OriginalValueIsClass7(ForwardingUNDGDataItem dataItem) =>
			dataItem.IsInDatabase && ((ZString)dataItem.DI_IMOClassInfo.OriginalValue) == "7";

		bool NewValueIsClass7(ForwardingUNDGDataItem dataItem) =>
			dataItem.DI_IMOClass == "7";

		bool IsClass7RadioactiveSubstanceForValidation() =>
			OriginalValueIsClass7(Parent) ^ NewValueIsClass7(Parent);

		public bool IsClass7RadioactiveSubstance() =>
			OriginalValueIsClass7(Parent) || NewValueIsClass7(Parent);

		protected override void CheckDI_RadionuclideElement()
		{
			base.CheckDI_RadionuclideElement();
			if (Parent.Lookups is ForwardingUNDGDataItemLookups lookup)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DI_RadionuclideElementInfo, lookup.RadionuclideElementList);
			}
		}

		protected override void CheckDI_RadionuclideElementSuffix()
		{
			base.CheckDI_RadionuclideElementSuffix();
			if (Parent.Lookups is ForwardingUNDGDataItemLookups lookup)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DI_RadionuclideElementSuffixInfo, lookup.RadionuclideElementSuffixList);
			}
		}

		protected override void CheckDI_RadioactiveMaximumActivityUnit()
		{
			base.CheckDI_RadioactiveMaximumActivityUnit();
			if (Parent.Lookups is ForwardingUNDGDataItemLookups lookup)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DI_RadioactiveMaximumActivityUnitInfo, lookup.RadioactiveMaximumActivityUnitList);
			}
		}

		protected override void CheckDI_MaterialFormDescription()
		{
			base.CheckDI_MaterialFormDescription();

			if (Parent.IsMaterialFormDescriptionRequired() && Parent.DI_MaterialFormDescription.IsEmpty)
			{
				Parent.DI_MaterialFormDescriptionInfo.AddError(Res.GetString("0d67e1ef-be97-91a0-42f0-32fe3f23fc0d",
					"Class 7 substances that are not special form require a description of the physical or chemical form to be entered."));
			}
		}

		#endregion

		protected override void CheckDI_IMOClass()
		{
			base.CheckDI_IMOClass();

			if (Parent.HasChanges
				&& IsClass7RadioactiveSubstanceForValidation()
				&& !Env.Security.UNDGSubstanceClass7RadioactiveMaterialsHandling.IsAllowed)
			{
				Parent.DI_IMOClassInfo.AddError(Res.GetString("7f2aa53a-7f13-4878-8b50-65a4569fb90c",
					"You do not have the security rights to add or remove Class 7 (Radioactive) materials on a Shipment."));
			}

			CheckConsolAcceptsDangerousGoodsCargo();
		}

		void CheckConsolAcceptsDangerousGoodsCargo()
		{
			if (!FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.Value)
			{
				return;
			}

			var shipment = Parent?.Shipment;
			var consolidations = shipment?.Consols.OfType<ForwardingConsol>() ?? Enumerable.Empty<ForwardingConsol>();

			var invalidConsolidations = consolidations.Where(c => !c.AllowsForShipmentsDangerousGoods(shipment)).ToList();
			if (invalidConsolidations.Count > 0)
			{
				Parent.DI_IMOClassInfo.AddError(Res.GetString("01426ee7-9272-6cbb-48b2-5a5fdc6bb8bd",
					"The following Consol(s) do not accept this dangerous cargo: {0}",
					string.Join(System.Environment.NewLine, invalidConsolidations.Select(c => c.HumanReadableName))));
			}
		}

		#region DI_DGFlashPoint

		protected override void CheckDI_DGFlashPoint()
		{
			base.CheckDI_DGFlashPoint();
			ValidateEmptyFlashPoint();
			ValidatePacklineMaxTemperature();
		}

		void ValidateEmptyFlashPoint()
		{
			if (Parent.DI_DGFlashPoint.IsEmpty
				&& Parent.Substance != null
				&& !Parent.Substance.DG_FlashPoint.IsEmpty)
			{
				Parent.DI_DGFlashPointInfo.AddWarning(Res.GetString("9f60622b-fe19-9f8e-4fd2-8539751dd25b",
					"Ensure that the temperature range specified for the Dangerous Goods substance is less than its Flash Point \"{0}\"",
					Parent.Substance.DG_FlashPoint));
			}
		}

		void ValidatePacklineMaxTemperature()
		{
			if (!(Parent?.ParentPackLine is ForwardingPackLine)
				|| !Parent.ParentPackLine.JL_RequiresTemperatureControl
				|| !DataItemHasFlashPointToValidate()
				|| !ParentPacklineHasValidTemperatureUnit())
			{
				return;
			}

			var convertedTemperature = Constants.Temperature.Convert(
				Parent.ParentPackLine.JL_RequiredTemperatureMaximum,
				Parent.ParentPackLine.JL_RequiredTemperatureUnit,
				Constants.Temperature.Centigrade);

			if (Parent.DI_DGFlashPoint < convertedTemperature)
			{
				Parent.DI_DGFlashPointInfo.AddError(Res.GetString("73d4cca8-abce-d087-43c0-74e3e36e4ee0",
					"Ensure maximum temperature of the packline’s dangerous goods is lower than the Flash Point {0}C set for this substance.",
					Parent.DI_DGFlashPoint));
			}
		}

		bool ParentPacklineHasValidTemperatureUnit()
		{
			return Parent.ParentPackLine.JL_RequiredTemperatureUnit == Constants.Temperature.Fahrenheit
				|| Parent.ParentPackLine.JL_RequiredTemperatureUnit == Constants.Temperature.Centigrade;
		}

		bool DataItemHasFlashPointToValidate()
		{
			return !Parent.DI_DGFlashPoint.IsEmpty
				|| (Parent.Substance != null && !Parent.Substance.DG_FlashPoint.IsEmpty);
		}

		#endregion

		protected override void CheckDI_DG_NKSubs()
		{
			base.CheckDI_DG_NKSubs();

			var shipment = Parent?.Shipment;
			var consolidations = shipment?.Consols.OfType<ForwardingConsol>() ?? Enumerable.Empty<ForwardingConsol>();

			var invalidConsolidations = consolidations.Where(c => !c.AllowsForShipmentsLithiumBatteries(shipment)).ToList();
			if (invalidConsolidations.Count > 0)
			{
				Parent.DI_DG_NKSubsInfo.AddError(Res.GetString("84a8271c-3201-ba87-4e17-165cc46a0ec3",
					"{0} contains lithium batteries which cannot be uplifted on a passenger flight. Either ensure that the following Consolidation(s) uses cargo only flights or remove the lithium batteries from this Shipment: {1}",
					shipment.HumanReadableName,
					string.Join(System.Environment.NewLine, invalidConsolidations.Select(c => c.HumanReadableName))
				));
			}
		}

		protected override void CheckDI_TechnicalName()
		{
			base.CheckDI_TechnicalName();

			if (!Parent.DI_TechnicalName.IsEmpty)
			{
				return;
			}

			var substance = Parent?.Substance;
			if (substance == null)
			{
				return;
			}

			if (!substance.DG_TechName.IsEmpty)
			{
				Parent.DI_TechnicalNameInfo.AddWarning(Res.GetString("5BCF378D-3050-48CF-99C8-FF630D7BF403",
					"Technical Name is required for this substance."));
			}
		}

		protected override void CheckDI_DGIsValidZGuid()
		{
			if (!(Parent.DI_DGInfo.IsNullable && Parent.DI_DG.IsEmpty || Parent.DI_DG.IsValid))
			{
				Parent.DI_DGInfo.AddError(GetOverridenInvalidSubstanceError().ToString());
			}
		}

		protected override void CheckDI_SpecialPermitIssueDate()
		{
			base.CheckDI_SpecialPermitIssueDate();

			if (!Parent.DI_SpecialPermitNumber.IsEmpty)
			{
				MandatoryValidation.CheckEntered(
					Parent.DI_SpecialPermitIssueDateInfo,
					Res.GetString("EE293F99-69C3-4C83-961E-4497C6F40AFD", "Special Issue Date")
				);
			}
		}
	}
}
