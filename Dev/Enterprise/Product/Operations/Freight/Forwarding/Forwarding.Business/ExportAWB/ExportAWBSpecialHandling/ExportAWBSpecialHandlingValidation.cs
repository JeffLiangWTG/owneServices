using CargoWise.Application;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Integration.Reference;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBSpecialHandlingValidation : Forwarding.AWB.Business.ExportAWBSpecialHandlingValidation
	{
		public ExportAWBSpecialHandlingValidation(ExportAWBSpecialHandling parent)
			: base(parent)
		{
		}

		protected override void CheckEP_SpecialHandling()
		{
			base.CheckEP_SpecialHandling();

			if (Parent.EP_SpecialHandling == AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill)
			{
				var error = Res.GetString("3834ef3b-04c7-4b7b-aad8-e02818d1bef5", "ECC cannot be chosen here. It is to be used only by the airline.");

				if (!Parent.IsInDatabase || Parent.EP_SpecialHandlingInfo.HasChanges)
				{
					Parent.EP_SpecialHandlingInfo.AddError(error);
				}
				else
				{
					Parent.EP_SpecialHandlingInfo.AddMessageError(error);
				}
			}

			if (Parent.IsSecurityStatus && Parent.Master != null)
			{
				if (Parent.Master is ConsolExportAWBHeader consolExportAWBHeader
					&& consolExportAWBHeader.Consol is ForwardingConsol consol)
				{
					var consolSecurityStatusValidationHelper = new ConsolSecurityStatusValidationHelper(consol);
					consolSecurityStatusValidationHelper.CheckSecurityStatusCode(Parent.EP_SpecialHandling, Parent.EP_SpecialHandlingInfo);
					consolSecurityStatusValidationHelper.ValidateSpecialHandlingCodeForUncertifiedUser(Parent.EP_SpecialHandling, Parent.EP_SpecialHandlingInfo, Parent.EP_SpecialHandlingHasChanges);
				}
			}

			if (!Parent.IsSecurityStatus && Parent.Master != null)
			{
				if (Parent.Master is ConsolExportAWBHeader consolExportAWBHeader && consolExportAWBHeader.Consol is ForwardingConsol consol)
				{
					if (consol.JK_TransportMode == Core.Constants.TransportModes.Air && !consol.GetShipmentSpecialHandlingCodes().Contains(Parent.EP_SpecialHandling) && Parent.EP_SpecialHandling != consol.GetEFreightStatus())
					{
						Parent.EP_SpecialHandlingInfo.AddWarning(Res.GetString("c60c51e6-733f-46f7-b894-646ef6d90222", "Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances."));
					}
				}
			}

			CheckJKHCodeIsFromAirlineSpecificAndIsNotAddedToIATA();
		}

		void CheckJKHCodeIsFromAirlineSpecificAndIsNotAddedToIATA()
		{
			var hasAirlineSpecificSpecialHandlingCode = Parent.Lookups.SpecialHandlingCodeDescriptionListInAirLine.ContainsCode(Parent.EP_SpecialHandling);
			var hasIATASpecificSpecialHandlingCode = ObjectFactory.Get<IIATASpecialHandlingCodesProvider>().GetCodeDescriptionPairList().ContainsCode(Parent.EP_SpecialHandling);
			if (hasAirlineSpecificSpecialHandlingCode && !hasIATASpecificSpecialHandlingCode)
			{
				Parent.EP_SpecialHandlingInfo.AddWarning(Res.GetString("0C37CAED-CAE7-4C48-B4EC-0A00EF24C261", "A non-IATA approved Special Handling Code has been selected which may not be supported or accepted by other industry recipients."));
			}
		}

		#region Implementation

		public new ExportAWBSpecialHandling Parent => (ExportAWBSpecialHandling)base.Parent;

		#endregion
	}
}
