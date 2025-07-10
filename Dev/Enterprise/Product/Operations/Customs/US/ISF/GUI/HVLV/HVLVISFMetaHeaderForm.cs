using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ISF.GUI
{
	public partial class HVLVISFMetaHeaderForm : ZChildForm, IHVLVISFMetaHeaderForm
	{
		HVLVISFMetaHeaderForm(HVLVISFMetaHeader metaHeader)
			: base(metaHeader)
		{
			InitializeComponent();
			MainMenu.MenuItems.Add(new HVLVISFMetaHeaderMessagingMenu(BusinessEntity as HVLVISFMetaHeader));

			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, SaveButtonUserControl);
			}
		}

		public HVLVISFMetaHeaderForm(BusinessObjectFactory factory, ZGuid shipmentPK)
			: this(new HVLVISFMetaHeader(factory, shipmentPK))
		{
		}

		public HVLVISFMetaHeaderForm(RelatedJobCollection relatedJobs)
			: this(new HVLVISFMetaHeader(relatedJobs))
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (MainMenu != null )
			{
				MainMenu.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void AddAdornments()
		{
			base.AddAdornments();

			var mainMenu = new ZMainMenu();
			Menu = mainMenu;
			MainMenu = mainMenu;
		}

		protected override void PerformValidation()
		{
			MetaHeader.FirstImporterSecurityFilingJob.Validation.ValidateAll();
		}

		HVLVISFMetaHeader MetaHeader => BusinessEntity as HVLVISFMetaHeader;

		public override string FormVerb => string.Empty;

		public override IBusiness BusinessEntityForHasChanges => MetaHeader?.FirstImporterSecurityFilingJob;

		protected override bool AllowNew => false;
	}
}
