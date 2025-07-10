using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class CashFlowCategoryBasedOnCreditorGroup : CashFlowCategoryBasedOnOrgGroup
	{
		public CashFlowCategoryBasedOnCreditorGroup()
		{ }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CashFlowCategoryBasedOnCreditorGroup();
		}

		BusinessObjectCollection orgGroupList;
		public override BusinessObjectCollection OrgGroupList
		{
			get
			{
				if (orgGroupList == null)
				{
					orgGroupList = new OrgCreditorGroupCollection(CurrentFactory);
				}
				return orgGroupList;
			}
		}

		protected override ZString GetOrgGroupDescriptionCore()
		{
			BusinessObject orgGroup = CurrentFactory.Load<OrgCreditorGroup>(OrgGroupPK);
			return orgGroup == null ? ZString.Empty : (ZString)orgGroup[OrgCreditorGroupSchema.Constants.OG_Desc];
		}

		public CashFlowCategoryBasedOnCreditorGroupCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (CashFlowCategoryBasedOnCreditorGroupCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new CashFlowCategoryBasedOnCreditorGroupCollection();
				}
			}
		}
	}
}
