using System.Linq;
using Enterprise.ZArchitecture.Core;
using FluentValidation;
using static Enterprise.Core.Constants;

namespace Enterprise.Services.ServiceHost
{
	public class DeliveryDueDateQueryValidator : AbstractValidator<DeliveryDueDateQuery>
	{
		public DeliveryDueDateQueryValidator()
		{
			DefineTransportModeValidationRules();

			DefinePickupDateValidationRules();

			DefineServiceLevelValidationRules();

			DefineHBLDlvModeValidationRules();

			DefinePickupOrgValidationRules();

			DefinePickupAddrValidationRules();

			DefinePickupCFSOrgValidationRules();

			DefinePickupCFSAddrValidationRules();

			DefineDeliveryOrgValidationRules();

			DefineDeliveryAddrValidationRules();

			DefineDeliveryCFSOrgValidationRules();

			DefineDeliveryCFSAddrValidationRules();

			DefineDeliveryTypeValidationRules();
		}

		void DefineTransportModeValidationRules()
		{
			RuleFor(q => q.TransportMode)
			.NotEmpty()
			.WithMessage(Res.GetString("D9F3E866-0F4C-42E5-81D8-60AAE9A48855", "Providing {0} is mandatory.", nameof(DeliveryDueDateQuery.TransportMode)));

			var ratingTransportModes = new CodeDescriptionPairList(OLookUpEditType.RateModes).GetAllCodes();

			RuleFor(q => q.TransportMode)
			.Must(q => ratingTransportModes.Contains(q))
			.WithMessage
				(
					Res.GetString
						(
							"229B2AF5-A26F-4D6C-BC65-448353E3EBF3",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(DeliveryDueDateQuery.TransportMode),
							string.Join(", ", ratingTransportModes.Select(t => $"'{t}'"))
						)
				)
			.When(q => !string.IsNullOrEmpty(q.TransportMode));
		}

		void DefinePickupDateValidationRules()
		{
			RuleFor(q => q.PickupDate)
			.NotEmpty()
			.WithMessage(Res.GetString("FC40B87F-B08A-48A8-AA1B-08A5DF5729D4", "Providing {0} is mandatory.", nameof(DeliveryDueDateQuery.PickupDate)));
		}

		void DefineServiceLevelValidationRules()
		{
			RuleFor(q => q.ServiceLevel)
			.NotEmpty()
			.WithMessage(Res.GetString("1E83957B-BEF2-4F35-9631-B3D3214F9FF7", "Providing {0} is mandatory.", nameof(DeliveryDueDateQuery.ServiceLevel)));
		}

		void DefineHBLDlvModeValidationRules()
		{
			RuleFor(q => q.HBLDlvMode)
			.NotEmpty()
			.WithMessage(Res.GetString("BA39228C-86BA-4144-8167-911B9F540DB3", "Providing {0} is mandatory.", nameof(DeliveryDueDateQuery.HBLDlvMode)));

			var validModes = new string[] {
					HBLDeliveryModes.Codes.DOOR_DOOR,
					HBLDeliveryModes.Codes.DOOR_CFS,
					HBLDeliveryModes.Codes.CFS_DOOR,
					HBLDeliveryModes.Codes.CFS_CFS,
					HBLDeliveryModes.Codes.ARPT_ARPT,
					HBLDeliveryModes.Codes.DOOR_ARPT,
					HBLDeliveryModes.Codes.CFS_ARPT,
					HBLDeliveryModes.Codes.ARPT_DOOR,
					HBLDeliveryModes.Codes.ARPT_CFS
			};
			RuleFor(l => l.HBLDlvMode)
			.Must(t => validModes.Contains(t))
			.WithMessage
				(
					Res.GetString
						(
							"301B8A66-75FB-422D-A1A8-F643298AB080",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(DeliveryDueDateQuery.HBLDlvMode),
							string.Join(", ", validModes.Select(t => $"'{t}'"))
						)
				)
			.When(t => !string.IsNullOrEmpty(t.HBLDlvMode));
		}

		void DefinePickupOrgValidationRules()
		{
			var mandatoryModes = new string[] { HBLDeliveryModes.Codes.DOOR_DOOR, HBLDeliveryModes.Codes.DOOR_CFS, HBLDeliveryModes.Codes.DOOR_ARPT };
			RuleFor(q => q.PickupOrg)
			.NotEmpty()
			.WithMessage
				(
					Res.GetString
						(
							"60680944-58A4-414F-B619-998819208EE7",
							"{0} is mandatory when {1} is one of these values: {2}.",
							nameof(DeliveryDueDateQuery.PickupOrg),
							nameof(DeliveryDueDateQuery.HBLDlvMode),
							string.Join(", ", mandatoryModes.Select(t => $"'{t}'"))
						)
				)
			.When(t => mandatoryModes.Contains(t.HBLDlvMode));
		}

		void DefinePickupAddrValidationRules()
		{
			var mandatoryModes = new string[] { HBLDeliveryModes.Codes.DOOR_DOOR, HBLDeliveryModes.Codes.DOOR_CFS, HBLDeliveryModes.Codes.DOOR_ARPT };
			RuleFor(q => q.PickupAddr)
			.NotEmpty()
			.WithMessage
				(
					Res.GetString
						(
							"35B3EECA-9718-4AFF-9FFC-5736812768ED",
							"{0} is mandatory when {1} is one of these values: {2}.",
							nameof(DeliveryDueDateQuery.PickupAddr),
							nameof(DeliveryDueDateQuery.HBLDlvMode),
							string.Join(", ", mandatoryModes.Select(t => $"'{t}'"))
						)
				)
			.When(t => mandatoryModes.Contains(t.HBLDlvMode));
		}

		void DefinePickupCFSOrgValidationRules()
		{
			RuleFor(q => q.PickupCFSOrg)
			.NotEmpty()
			.WithMessage(Res.GetString("52D33519-3645-4B7A-A1EC-E2C140D30082", "Providing {0} is mandatory.", nameof(DeliveryDueDateQuery.PickupCFSOrg)));
		}

		void DefinePickupCFSAddrValidationRules()
		{
			RuleFor(q => q.PickupCFSAddr)
			.NotEmpty()
			.WithMessage(Res.GetString("32017D47-A261-4465-9CA1-DA00486EC6EE", "Providing {0} is mandatory.", nameof(DeliveryDueDateQuery.PickupCFSAddr)));
		}
		void DefineDeliveryOrgValidationRules()
		{
			var mandatoryModes = new string[] { HBLDeliveryModes.Codes.DOOR_DOOR, HBLDeliveryModes.Codes.CFS_DOOR, HBLDeliveryModes.Codes.ARPT_DOOR };
			RuleFor(q => q.DeliveryOrg)
			.NotEmpty()
			.WithMessage
				(
					Res.GetString
						(
							"F7B0B72E-2805-4C74-9A22-96B6031D0A57",
							"{0} is mandatory when {1} is one of these values: {2}.",
							nameof(DeliveryDueDateQuery.DeliveryOrg),
							nameof(DeliveryDueDateQuery.HBLDlvMode),
							string.Join(", ", mandatoryModes.Select(t => $"'{t}'"))
						)
				)
			.When(t => mandatoryModes.Contains(t.HBLDlvMode));
		}

		void DefineDeliveryAddrValidationRules()
		{
			var mandatoryModes = new string[] { HBLDeliveryModes.Codes.DOOR_DOOR, HBLDeliveryModes.Codes.CFS_DOOR, HBLDeliveryModes.Codes.ARPT_DOOR };
			RuleFor(q => q.DeliveryAddr)
			.NotEmpty()
			.WithMessage
				(
					Res.GetString
						(
							"AFB8CEC3-E668-4FF6-ABA4-EDA28B343974",
							"{0} is mandatory when {1} is one of these values: {2}.",
							nameof(DeliveryDueDateQuery.DeliveryAddr),
							nameof(DeliveryDueDateQuery.HBLDlvMode),
							string.Join(", ", mandatoryModes.Select(t => $"'{t}'"))
						)
				)
			.When(t => mandatoryModes.Contains(t.HBLDlvMode));
		}

		void DefineDeliveryCFSOrgValidationRules()
		{
			RuleFor(q => q.DeliveryCFSOrg)
			.NotEmpty()
			.WithMessage(Res.GetString("FB7558B8-D7DB-4C5E-A927-0AC14A430032", "Providing {0} is mandatory.", nameof(DeliveryDueDateQuery.DeliveryCFSOrg)));
		}

		void DefineDeliveryCFSAddrValidationRules()
		{
			RuleFor(q => q.DeliveryCFSAddr)
			.NotEmpty()
			.WithMessage(Res.GetString("D360F595-5AE0-446A-B803-70DD26485256", "Providing {0} is mandatory.", nameof(DeliveryDueDateQuery.DeliveryCFSAddr)));
		}

		void DefineDeliveryTypeValidationRules()
		{
			RuleFor(q => q.DeliveryType)
			.Must(t => t.Equals(DeliveryTypes.DirectToCNE, System.StringComparison.OrdinalIgnoreCase))
			.When(t => !string.IsNullOrWhiteSpace(t.DeliveryType))
			.WithMessage(Res.GetString("C2113EC7-58D4-4975-91A7-4760B4F46DF7", "Provided {0} ('{{PropertyValue}}') is not valid. It can only be '{1}' or blank.", nameof(DeliveryDueDateQuery.DeliveryType), DeliveryTypes.DirectToCNE));

			var validModes = new string[] { HBLDeliveryModes.Codes.DOOR_DOOR, HBLDeliveryModes.Codes.CFS_DOOR };
			RuleFor(q => q.DeliveryType)
			.Must(t => !t.Equals(DeliveryTypes.DirectToCNE, System.StringComparison.OrdinalIgnoreCase))
			.When(t => !string.IsNullOrWhiteSpace(t.DeliveryType) && !validModes.Contains(t.HBLDlvMode))
			.WithMessage(q => Res.GetString(
				"32617110-78C6-4A3E-9026-9CC5EFFC1729",
				"{0} '{1}' is only valid when {2} is one of these values: {3}, but '{4}' was provided.",
				nameof(DeliveryDueDateQuery.DeliveryType),
				DeliveryTypes.DirectToCNE,
				nameof(DeliveryDueDateQuery.HBLDlvMode),
				string.Join(", ", validModes.Select(t => $"'{t}'")),
				q.HBLDlvMode));
		}
	}
}
