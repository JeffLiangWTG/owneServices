using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class PackLinePortMessagingForDakosyValidation : JobPackLinePortMessagingValidation
	{
		public PackLinePortMessagingForDakosyValidation(PackLinePortMessaging parent)
			: base(parent)
		{
		}

		protected new PackLinePortMessaging Parent
		{
			get { return base.Parent; }
		}

		protected override void CheckJLM_EntryType()
		{
			base.CheckJLM_EntryType();
			PortMessagingValidationHelper.CheckEntryType(Parent.JLM_EntryTypeInfo);

			if (!Parent.JLM_EntryTypeInfo.HasErrors())
			{
				CheckSiblingPackLinePortMessagingsForCompatibleEntryTypes();
				CheckHarmonisedCodeAndGoodsDescription();
			}
		}

		void CheckSiblingPackLinePortMessagingsForCompatibleEntryTypes()
		{
			if (Parent.PackLine != null && Parent.PackLine.Shipment != null)
			{
				var query = new ZQuery(JobPackLinePortMessagingSchema.JLM_JL_PackLine, Parent.PackLine.Shipment.OuterPackLines.Select(p => p.PK));
				var siblingPackLinePortMessagings = Parent.Factory.Load<PackLinePortMessaging>(query);

				var entryTypes = new List<ZString>();
				foreach (var packLinePortMessaging in siblingPackLinePortMessagings)
				{
					if (!packLinePortMessaging.JLM_EntryTypeInfo.HasErrors())
					{
						entryTypes.Add(packLinePortMessaging.JLM_EntryType);
					}
				}

				var compatibleEntryTypesRequiringMRN = new ZString[]
				{
					EntryTypeList.Codes.AESExportDeclaration,
					EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities,
					EntryTypeList.Codes.ExitSummaryDeclaration
				};

				if (entryTypes.Distinct().Count() > 1 && entryTypes.Except(compatibleEntryTypesRequiringMRN).Any())
				{
					Parent.JLM_EntryTypeInfo.AddError(Res.GetString("5413349e-3e54-4692-b0f7-a02e19e75cb6", "Only AES, AEM and DUX Entry Types can be submitted together. All pack lines should have an Entry Type or all must be blank."));
				}
			}
		}

		void CheckHarmonisedCodeAndGoodsDescription()
		{
			var entryTypes = new[]
			{
				EntryTypeList.Codes.Message,
				EntryTypeList.Codes.EUPortOfDestination,
				EntryTypeList.Codes.OtherExemptions,
				EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN
			};

			var entryType = Parent.JLM_EntryType;
			if (entryType.IsEmpty)
			{
				var shipment = Parent.PackLine?.Shipment;
				if (shipment != null)
				{
					var shipmentPortMessaging = PortMessagingHelper.GetShipmentPortMessaging(shipment);
					entryType = shipmentPortMessaging != null ? shipmentPortMessaging.JSM_EntryType : ZString.Empty;
				}
			}

			if (entryTypes.Any(c => c == entryType) && Parent.CommodityDescription.IsEmpty)
			{
				var message = Res.GetString(
								"6c5b3902-5ea5-4ee1-a2a8-2ceed10e7cba",
								"HS Code or Packing Goods Description is required for Entry Type {0} - {1}.",
								entryType,
								Parent.Lookups.EntryTypeList.GetDescriptionFromCode(entryType));

				Parent.JLM_EntryTypeInfo.AddError(message);
			}
		}

		protected override void CheckJLM_MovementReferenceNumber()
		{
			base.CheckJLM_MovementReferenceNumber();
			PortMessagingValidationHelper.CheckMovementReferenceNumber(Parent.JLM_MovementReferenceNumberInfo);
		}

		protected override void CheckJLM_MovementReferenceNumberComplete()
		{
			base.CheckJLM_MovementReferenceNumberComplete();
			PortMessagingValidationHelper.CheckMovementReferenceNumberComplete(Parent.JLM_MovementReferenceNumberCompleteInfo);
		}

		protected override void CheckJLM_ATBNumber()
		{
			base.CheckJLM_ATBNumber();
			PortMessagingValidationHelper.CheckATBNumber(Parent.JLM_ATBNumberInfo);
		}

		protected override void CheckJLM_ExemptionReason()
		{
			base.CheckJLM_ExemptionReason();
			PortMessagingValidationHelper.CheckExemptionReason(Parent.JLM_ExemptionReasonInfo);
		}

		protected override void CheckJLM_Annex30AType()
		{
			base.CheckJLM_Annex30AType();
			PortMessagingValidationHelper.CheckAnnex30AType(Parent.JLM_Annex30ATypeInfo);
		}

		protected override void CheckJLM_Annex30AFailureProcess()
		{
			base.CheckJLM_Annex30AFailureProcess();
			PortMessagingValidationHelper.CheckAnnex30AFailureProcess(Parent.JLM_Annex30AFailureProcessInfo);
		}

		protected override void CheckJLM_ExportDeclarationReference()
		{
			base.CheckJLM_ExportDeclarationReference();
			PortMessagingValidationHelper.CheckExportDeclarationReference(Parent.JLM_ExportDeclarationReferenceInfo);
		}

		protected override void CheckJLM_CustomsReleaseDate()
		{
			base.CheckJLM_CustomsReleaseDate();
			PortMessagingValidationHelper.CheckCustomsReleaseDate(Parent.JLM_CustomsReleaseDateInfo);
		}

		protected void CheckDGTechnicalName()
		{
			if (PackLineHasDangerousGoodWithIMOStandardAndProvision274() && Parent.DGTechnicalName == ZString.Empty)
			{
				Parent.DGTechnicalNameInfo.AddMessageError(Res.GetString("596F7D19-10DD-4A0F-AFDF-E19351B2C528", "Dangerous Goods with special provision 274 requires a Technical Name"));
			}
		}

		protected override void CheckJLM_LocalReferenceNumber()
		{
			base.CheckJLM_LocalReferenceNumber();
			PortMessagingValidationHelper.CheckLocalReferenceNumber(Parent.JLM_LocalReferenceNumberInfo);
		}

		protected override void CheckJLM_LocalReferenceNumberComplete()
		{
			base.CheckJLM_LocalReferenceNumberComplete();
			PortMessagingValidationHelper.CheckLocalReferenceNumberComplete(Parent.JLM_LocalReferenceNumberCompleteInfo);
		}

		bool PackLineHasDangerousGoodWithIMOStandardAndProvision274()
		{
			var dangerousGoodsWithProvision = Parent.PackLine.UNDGs.Where(undg =>
				undg.Substance != null
				&& undg.Substance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO
				&& undg.Substance.SpecialProvisions.Any(data => data.DC_Index == "274")).ToList();

			return dangerousGoodsWithProvision.Any();
		}
	}
}
