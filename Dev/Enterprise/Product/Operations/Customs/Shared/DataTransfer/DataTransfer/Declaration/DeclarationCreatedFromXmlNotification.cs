using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer
{
	public class DeclarationCreatedFromXmlNotification : BusinessObjectAfterSaveNotification
	{
		public DeclarationCreatedFromXmlNotification(BaseJobDeclaration declaration)
			: base(declaration, NotificationSubscriberType.Info)
		{
		}

		public new BaseJobDeclaration BusinessEntity
		{
			get { return (BaseJobDeclaration)base.BusinessEntity; }
		}

		protected override string DisplayMessageCore
		{
			get { return string.Format((NoResString)"Declaration {0} (HouseBill='{1}' Branch='{2}') Created.", BusinessEntity.JE_DeclarationReference, BusinessEntity.JE_HouseBill, BusinessEntity.Branch.GB_Code); }
		}
	}
}
