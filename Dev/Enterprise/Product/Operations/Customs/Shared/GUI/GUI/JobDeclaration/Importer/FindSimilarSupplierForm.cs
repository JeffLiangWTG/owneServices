using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DataTransfer;

namespace Enterprise.Customs.GUI
{
	public class FindSimilarSupplierForm : FindSimilarOrganisationForm
	{
		public FindSimilarSupplierForm()
		{
		}

		public FindSimilarSupplierForm(OrganisationFinder finder) : base(finder)
		{
		}

		public override string FormHeading => Res.GetString("8f07f8c7-1b90-4807-b243-80bd580ad6d5", "Select Supplier");

		protected override string HeadingLabelText => Res.GetString("21d7ce68-f426-45e2-aabe-2d2b91098bbc", "Cannot find supplier. No supplier code provided.");

		protected override ResourceStringData OrganisationLabelResourceString => Res.GetData("057B431B-2FA4-42F3-809F-89B2F830AEF5", "Organizations similar to the supplier details provided. Select an organization to set the supplier.");
	}
}
