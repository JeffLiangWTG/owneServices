using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBSpecialHandlingValidation : AutoExportAWBSpecialHandlingValidation
	{
		public ExportAWBSpecialHandlingValidation(AutoExportAWBSpecialHandling parent)
			: base(parent)
		{
		}

		protected new ExportAWBSpecialHandling Parent
		{
			get { return (ExportAWBSpecialHandling)base.Parent; }
		}

		protected override void CheckEP_SpecialHandling()
		{
			base.CheckEP_SpecialHandling();

			ListValidation.ErrorIfInvalidCode(Parent.EP_SpecialHandlingInfo);
			MandatoryValidation.CheckEntered(Parent.EP_SpecialHandlingInfo);

			CheckOneSecurityStatusInSpecialHandlingOnly();
			CheckSpecialHandlingDuplicates();
		}

		void CheckOneSecurityStatusInSpecialHandlingOnly()
		{
			if (Parent.Master != null)
			{
				if (Parent.IsSecurityStatus
					&& Parent.Master.AWBSpecialHandlingItems.Cast<ExportAWBSpecialHandling>()
						.Any(specialHandling => specialHandling.IsSecurityStatus && specialHandling.PK != Parent.PK))
				{
					Parent.EP_SpecialHandlingInfo.AddError(Res.GetString("c7d29be5-464e-4b28-bf91-96657334549f", "AWB can contain one Security Status only."));
				}

				foreach (ExportAWBSpecialHandling specialHandling in Parent.Master.AWBSpecialHandlingItems)
				{
					if (specialHandling.PK != Parent.PK)
					{
						specialHandling.Validation.ValidateEP_SpecialHandling();
					}
				}
			}
		}

		void CheckSpecialHandlingDuplicates()
		{
			if (Parent.Master == null)
			{
				return;
			}

			var alreadyContainsSpecialHandlingCode =
				Parent.Master.AWBSpecialHandlingItems
				.Cast<ExportAWBSpecialHandling>()
				.Any(specialHandlingItem =>
						!specialHandlingItem.EP_SpecialHandling.IsEmpty
						&& specialHandlingItem.PK != Parent.PK
						&& specialHandlingItem.EP_SpecialHandling == Parent.EP_SpecialHandling);

			if (alreadyContainsSpecialHandlingCode)
			{
				Parent.EP_SpecialHandlingInfo.AddError(Res.GetString("a49467db-4148-d8a8-4d39-6490c7f5fc14", "List contains duplicate Special Handling Codes."));
			}
		}
	}
}
