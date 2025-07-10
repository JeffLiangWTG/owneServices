using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCustomLabelsCollection : DependentBusinessObjectCollection<OrgCustomLabels, BusinessObject>
	{
		public OrgCustomLabelsCollection(OrgHeader parent) : base(parent)
		{
		}

		public OrgCustomLabelsCollection(OrgHeader parent, BusinessObjectFactory factory) : base(parent, factory)
		{
		}

		public OrgCustomLabels FindByFieldName(ZString fieldName)
		{
			OrgCustomLabels result = null;
			foreach (OrgCustomLabels label in this)
			{
				if (label.OT_FieldName.EqualsIgnoringCase(fieldName))
				{
					result = label;
					break;
				}
			}
			return result;
		}

		public OrgCustomLabels FindByFieldNameAndType(ZString fieldName, ZString labelType)
		{
			foreach (OrgCustomLabels label in this)
			{
				if (label.OT_FieldName.EqualsIgnoringCase(fieldName) && (labelType.IsEmpty || label.OT_Type == labelType))
				{
					return label;
				}
			}
			return null;
		}

		public bool HasLabelType(ZString labelType)
		{
			foreach (OrgCustomLabels label in this)
			{
				if (label.OT_Type.EqualsIgnoringCase(labelType))
				{
					return true;
				}
			}
			return false;
		}
	}
}
