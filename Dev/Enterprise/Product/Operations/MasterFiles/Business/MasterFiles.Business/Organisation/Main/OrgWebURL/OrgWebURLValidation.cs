//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgWebURLValidation
//
//    This class should be used for overriding validation in AutoOrgWebURLValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgWebURLValidation : AutoOrgWebURLValidation
	{
		public OrgWebURLValidation(AutoOrgWebURL parent)
			: base(parent)
		{
			this.Parent = (OrgWebURL)parent;
		}

		public new OrgWebURL Parent;

		#region IsPrimary

		protected override void CheckPU_IsPrimary()
		{
			base.CheckPU_IsPrimary();
			if (Parent.PU_IsPrimary && Parent.Header != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.PU_IsPrimaryInfo, Parent.Header.OrgWebURLs);
			}
		}

		#endregion

		#region PU_URL

		protected override void CheckPU_URL()
		{
			base.CheckPU_URL();
			if (!Parent.PU_URL.IsEmpty || !Parent.MainDefaultAdded)
			{
				if (Parent.PU_URL.IsEmpty)
				{
					Parent.PU_URLInfo.AddError(Res.GetString("88e37213-6d6d-4f21-96e3-ccd5a0033fc6", "Please enter a value. Also, you can go to Web tab to delete a row."));
				}
				if (!UrlValidation.IsValidUrl(Parent.PU_URL) && !Parent.PU_URLInfo.HasErrors())
				{
					Parent.PU_URLInfo.AddError(Res.GetString("02093877-0be0-45d7-8262-d169fd19c01c", "Please enter a valid website address (URL).\r\n\r\nA valid address is commonly found in the format \"{0}\" or \"{1}\"", "http://", "www."));
				}
			}
			if (Parent.Header != null)
			{
				if (Parent.Header.RequiredFieldsForOrg.RequireWebAddress)
				{
					MandatoryValidation.CheckEntered(Parent.PU_URLInfo);
				}
				if (Parent.Header.RequiredFieldsForOrg.RequireFaxEmailOrWeb)
				{
					if (Parent.Header.MainAddress.OA_Fax.IsEmpty && Parent.Header.MainAddress.OA_Email.IsEmpty && Parent.PU_URL.IsEmpty)
					{
						Parent.PU_URLInfo.AddError(RequiredFieldsMessage);
					}
					else
					{
						if (Parent.Header.MainAddress.OA_Fax.IsEmpty && Parent.Header.MainAddress.OA_FaxInfo.HasErrors())
						{
							Parent.Header.MainAddress.Validation.ValidateOA_Fax();
						}
						if (Parent.Header.MainAddress.OA_Email.IsEmpty && Parent.Header.MainAddress.OA_EmailInfo.HasErrors())
						{
							Parent.Header.MainAddress.Validation.ValidateOA_Email();
						}
					}
				}
			}

			if (!Parent.PU_URLInfo.HasErrors() &&
				Parent.PU_Type.EqualsIgnoringCase(OrgWebUrlList.Codes.CartageTracking) &&
				!Parent.PU_URL.ToLower().Contains(Constants.TransportCoHotlinkOpener.CargoWiseREF.ToLower()))
			{
				string quickViewNumberHTMLAddress = string.Format("http://tracking.cargowise.com/Login/Login.aspx?QuickViewNumber={0}", Constants.TransportCoHotlinkOpener.CargoWiseREF);
				Parent.PU_URLInfo.AddError(
					Res.GetString("b7f1359e-ac80-4040-8fd2-d8640796a371", "The {0} web address is missing the text {1}.\r\nThe text is required to enable requests' forwarding to {2} site.\r\nFor example: \"{3}\".", OrgWebUrlList.Descriptions.CartageTracking, Constants.TransportCoHotlinkOpener.CargoWiseREF, OrgWebUrlList.Descriptions.CartageTracking, quickViewNumberHTMLAddress));
			}
		}

		#endregion

		#region PU_Type

		protected override void CheckPU_Type()
		{
			base.CheckPU_Type();
			MandatoryValidation.CheckEntered(Parent.PU_TypeInfo);
		}

		#endregion

		#region MainDefaultAdded

		public void ValidateMainDefaultAdded()
		{
			ValidateCalculatedProperty(Parent.MainDefaultAddedInfo);
		}

		protected void CheckMainDefaultAdded()
		{
			if (Parent.Header != null && Parent.Header.OrgWebURLs.Count > 0 &&
				((Parent.Header.OrgWebURLs.Count == 1 && !Parent.Header.OrgWebURLs[0].MainDefaultAdded) || Parent.Header.OrgWebURLs.Count > 1))
			{
				bool hasMain = false;
				foreach (OrgWebURL url in Parent.Header.OrgWebURLs)
				{
					if (url.PU_IsPrimary && !url.PU_URL.IsEmpty)
					{
						hasMain = true;
						break;
					}
				}
				if (!hasMain)
				{
					Parent.MainDefaultAddedInfo.AddError(Res.GetString("cb9ca9c4-b26a-4243-8c9a-5984a7ee9add", "One of the organization's websites must be marked as the Primary Website."));
				}
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMainDefaultAdded();
		}

		public static string RequiredFieldsMessage
		{
			get { return Res.GetString("46e108c0-ee09-482a-95a6-dbe8453c43ae", "Either a fax number, email address or web address must be entered for this organization."); }
		}
	}
}
