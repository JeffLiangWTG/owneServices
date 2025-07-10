using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business
{
	[ImmutableObject(true)]
	public class WebSecurityRight : CodeDescriptionPair
	{
		public WebSecurityRight(string securityRightName, MultilingualString securityRightNameForDisplay, WebSecurityApplication webApplication, bool? isGrantedByDefault = null, WebSecurityRightCategory category = WebSecurityRightCategory.None)
			: base(securityRightName, securityRightNameForDisplay)
		{
			this.webApplication = webApplication;
			this.isGrantedByDefault = isGrantedByDefault ?? webApplication.ShouldGrantAllAccessRightsByDefault;
			this.category = category;
		}

		public bool IsGrantedByDefault
		{
			get
			{
				if (OrganisationRegistry.Instance.WebSecurityRightsDeniedByDefault.Value)
				{
					return false;
				}
				return isGrantedByDefault;
			}
		}

		public bool IsWarehouse => category == WebSecurityRightCategory.Warehouse;

		public WebSecurityApplication WebApplication
		{
			get { return webApplication; }
		}

		public ZString SecurityItemName
		{
			get
			{
				return SecurityGuid != ZGuid.Empty ? string.Empty :
					(Code.Length <= OrgSecuritySchema.OX_SecurityItemName.MaxLength
						? Code
						: Code.Substring(0, OrgSecuritySchema.OX_SecurityItemName.MaxLength));
			}
		}

		public ZGuid SecurityGuid
		{
			get
			{
				ZGuid guid;
				if (ZGuid.TryParse(Code, out guid))
				{
					return guid;
				}
				else
				{
					return ZGuid.Empty;
				}
			}
		}

		public static WebSecurityRight FromWebSecurityRightShare(MultilingualString resourceKey, WebSecurityRightShare rightShare)
		{
			return new WebSecurityRight(rightShare.Code, resourceKey, rightShare.WebApplication, rightShare.IsGrantedByDefault, rightShare.IsWarehouse ? WebSecurityRightCategory.Warehouse : WebSecurityRightCategory.None);
		}

		readonly WebSecurityRightCategory category;
		readonly bool isGrantedByDefault;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule")]
		readonly WebSecurityApplication webApplication;
	}
}
