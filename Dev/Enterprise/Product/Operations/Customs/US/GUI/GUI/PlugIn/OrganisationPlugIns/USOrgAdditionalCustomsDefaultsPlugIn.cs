using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.GUI
{
	public class USOrgAdditionalCustomsDefaultsPlugIn : ZPlugIn
	{
		public USOrgAdditionalCustomsDefaultsPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		public override string Name
		{
			get { return "Additional Customs Defaults"; }// Plugin Tab Name
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Broker; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return LinkCollection;
		}

		protected OrgSupplierBuyerLink CurrentLink
		{
			get { return (OrgSupplierBuyerLink)Current; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new USOrgAdditionalCustomsDefaultsControl();
		}

		OrgSupplierBuyerLink previousLink;
		protected override void OnCurrentChanged()
		{
			if (previousLink != null)
			{
				previousLink.OL_RN_NKImporterCountryInfo.ValueChanged -= new System.EventHandler(OL_RN_NKImporterCountryInfo_ValueChanged);
			}

			base.OnCurrentChanged();

			previousLink = CurrentLink;

			UpdateCollectionWithCurrent();

			UpdateControlVisibility();

			if (CurrentLink != null)
			{
				CurrentLink.OL_RN_NKImporterCountryInfo.ValueChanged += new System.EventHandler(OL_RN_NKImporterCountryInfo_ValueChanged);
			}
		}

		void OL_RN_NKImporterCountryInfo_ValueChanged(object sender, System.EventArgs e)
		{
			UpdateCollectionWithCurrent();
			UpdateControlVisibility();
		}

		void UpdateControlVisibility()
		{
			var currentLink = CurrentLink;
			var controlsVisible = currentLink != null && Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(currentLink.OL_RN_NKImporterCountry) == Core.Constants.CountryCodes.UnitedStates;
			((USOrgAdditionalCustomsDefaultsControl)UserControl).ControlsVisibility(controlsVisible);
		}

		void UpdateCollectionWithCurrent()
		{
			var addInfo = FindCurrentAddInfoAndUpdateCollection();
			if (addInfo != null)
			{
				if (LinkCollection.Count > 0)
				{
					LinkCollection.RemoveAll();
				}
				LinkCollection.Add(addInfo);
			}
		}

		USOrgSupplierBuyerLinkAddInfo FindCurrentAddInfoAndUpdateCollection()
		{
			USOrgSupplierBuyerLinkAddInfo addInfo = null;
			if (CurrentLink != null)
			{
				addInfo = (from USOrgSupplierBuyerLinkAddInfo existingAddInfo in ExistinAddInfoDataLinkCollection
						   where existingAddInfo.ParentPK == CurrentLink.PK
						   select existingAddInfo).FirstOrDefault();
				if (addInfo == null)
				{
					addInfo = CurrentLink.GetAddInfo() is OrgSupplierBuyerLinkAddInfo addInfoBO ? new USOrgSupplierBuyerLinkAddInfo(addInfoBO) : null;
					if (addInfo != null)
					{
						ExistinAddInfoDataLinkCollection.Add(addInfo);
					}
				}
			}
			return addInfo;
		}

		protected USOrgSupplierBuyerLinkAddInfoCollection LinkCollection
		{
			get
			{
				if (linkCollection == null)
				{
					linkCollection = new USOrgSupplierBuyerLinkAddInfoCollection(Factory);
					UpdateCollectionWithCurrent();
				}
				return linkCollection;
			}
		}
		USOrgSupplierBuyerLinkAddInfoCollection linkCollection;

		protected USOrgSupplierBuyerLinkAddInfoCollection ExistinAddInfoDataLinkCollection
		{
			get
			{
				if (existinAddInfoDataLinkCollection == null)
				{
					existinAddInfoDataLinkCollection = new USOrgSupplierBuyerLinkAddInfoCollection(Factory);
					FindCurrentAddInfoAndUpdateCollection();
				}
				return existinAddInfoDataLinkCollection;
			}
		}
		USOrgSupplierBuyerLinkAddInfoCollection existinAddInfoDataLinkCollection;
	}
}
