using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.Business
{
	public static class DocAddressesCreationHelper
	{
		/// <summary>
		/// Purpose : Maintain a white list of which Doc Addresses is allowed to be created for US Doc Address by non-GUI operation.
		/// </summary>
		public static bool ShouldBeCreated(JobDeclaration declaration, DocAddressType addressType)
		{
			var isSupportedAddressType = ((IDocAddresses)declaration).SupportedAddressTypes.Contains(addressType);
			if (isSupportedAddressType)
			{
				if (declaration.IsStandAlone)
				{
					return true;
				}

				switch (addressType)
				{
					case DocAddressType.SupplierDocumentaryAddress:
					case DocAddressType.ImporterDocumentaryAddress:
					case DocAddressType.CustomsContainerTerminalOperatorAddress:
					case DocAddressType.CustomsContainerYardAddress:
					case DocAddressType.CustomsDepotAddress:
					case DocAddressType.CBPBroker:
					case DocAddressType.FDASubmitter:
						return true;
					case DocAddressType.InvoicerAddress:
					case DocAddressType.SupplierPickupDeliveryAddress:
					case DocAddressType.ImporterPickupDeliveryAddress:
					case DocAddressType.NotifyParty:
					case DocAddressType.NotifyParty2:
					case DocAddressType.NotifyParty3:
					case DocAddressType.BuyerDocumentaryAddress:
					case DocAddressType.InsuredByDocumentaryAddress:
					case DocAddressType.AssuredPartyDocumentaryAddress:
					case DocAddressType.SurveyReportPartyDocumentaryAddress:
					case DocAddressType.ClaimsPayableByDocumentaryAddress:
						return false;
					case DocAddressType.CustomsWarehouseAddress:
						return declaration.BondedWarehouseEditable;
					default:
						throw new System.NotImplementedException(string.Format("A new doc address type {0} is defined in IDocAddresses.SupportedAddressTypes but not yet defined accessibility in this class {1}.", addressType.ToString(), typeof(DocAddressesCreationHelper).AssemblyQualifiedName));
				}
			}

			return false;
		}
	}
}
