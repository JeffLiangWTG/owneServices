using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ZA.Business
{
	class DeclarationJobDocAddressValidation : JobDocAddressValidation
	{
		public DeclarationJobDocAddressValidation(JobDocAddress address, JobDeclaration declaration)
			: base(address)
		{
			Declaration = declaration;
		}

		JobDeclaration Declaration { get; set; }

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (!Parent.OrganisationPK.IsEmpty && Parent.E2_AddressType == AutoDocAddressTypes.Codes.CustomsDepotAddress && Declaration.IsRoad)
			{
				Parent.OrganisationPKInfo.AddMessageError(DepotMustBeBlankForRoadExport);
			}
		}

		public static string DepotMustBeBlankForRoadExport
		{
			get { return Res.GetString("07271569-d1d8-4894-bc17-6ae3ae26ebff", "Depot must be blank for exports using road transport."); }
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (Parent.E2_AddressType == AutoDocAddressTypes.Codes.CustomsDepotAddress && Parent.HasRealAddress)
			{
				var depotLocalControlledPremisesID = Parent.Address?.DepotLocalControlledPremisesID ?? ZString.Empty;
				if (!depotLocalControlledPremisesID.IsEmpty && (depotLocalControlledPremisesID.Length != 2 || !depotLocalControlledPremisesID.IsLettersAndNumbersOnlyOrEmpty))
				{
					Parent.E2_OA_AddressInfo.AddMessageError(string.Format(CultureInfo.InvariantCulture, "Invalid {0} (Customs Controlled Premises Code - Depot). It has to be two alphanumeric characters.", OrgCusCode.CodeTypes.DepotControlledPremisesID));
				}
			}
		}
	}
}
