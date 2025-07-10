using CargoWise.EntityFramework;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingBulkSailingConsolGenerator : BulkSailingConsolGenerator
	{
		public ForwardingBulkSailingConsolGenerator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override CommonConsol GetTemplateConsol()
		{
			return Factory.Load<ForwardingConsol>(TemplateConsolPK);
		}

		protected override CommonConsol CreateConsolFromTemplate(CommonConsol templateConsol)
		{
			ForwardingConsol consol = templateConsol != null ? (ForwardingConsol)templateConsol.TemplateCopy(CopyShipments, false) : null;

			if (consol != null)
			{
				SetConsolMAWBAllocation(consol);
			}

			return consol;
		}

		protected override CommonConsol CreateNewConsol()
		{
			ForwardingConsol consol;
			Factory.SuspendValidation();

			try
			{
				consol = Factory.New<ForwardingConsol>();
			}
			finally
			{
				Factory.ResumeValidation();
			}

			SetConsolMAWBAllocation(consol);

			return consol;
		}

		void SetConsolMAWBAllocation(ForwardingConsol consol)
		{
			using (consol.GetValidationSuspender())
			{
				consol.JK_IsNeutralMaster = false;
			}
		}
	}
}
