using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class CashFlowCategoryBasedOnDebtorGroup : CashFlowCategoryBasedOnOrgGroup
	{
		public CashFlowCategoryBasedOnDebtorGroup()
		{ }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CashFlowCategoryBasedOnDebtorGroup();
		}

		BusinessObjectCollection orgGroupList;
		public override BusinessObjectCollection OrgGroupList
		{
			get
			{
				if (orgGroupList == null)
				{
					orgGroupList = new OrgDebtorGroupCollection(CurrentFactory);
				}
				return orgGroupList;
			}
		}

		protected override ZString GetOrgGroupDescriptionCore()
		{
			BusinessObject orgGroup = CurrentFactory.Load<OrgDebtorGroup>(OrgGroupPK);
			return orgGroup == null ? ZString.Empty : (ZString)orgGroup[OrgDebtorGroupSchema.Constants.OJ_Desc];
		}

		public CashFlowCategoryBasedOnDebtorGroupCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (CashFlowCategoryBasedOnDebtorGroupCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new CashFlowCategoryBasedOnDebtorGroupCollection();
				}
			}
		}
	}
}
