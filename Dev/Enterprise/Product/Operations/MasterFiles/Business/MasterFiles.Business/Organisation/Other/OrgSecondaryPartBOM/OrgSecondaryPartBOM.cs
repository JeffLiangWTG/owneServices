using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecondaryPartBOM : AutoOrgSecondaryPartBOM
	{
		public OrgSecondaryPartBOM(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ComponentUsages

		[ChildEditable(true)]
		public OrgSecondaryPartBOMPivotCollection ComponentUsages
		{
			get
			{
				if (componentUsages == null)
				{
					componentUsages = new OrgSecondaryPartBOMPivotCollection(this);
					RegisterEditableChildObject(componentUsages);
				}

				return componentUsages;
			}
		}

		OrgSecondaryPartBOMPivotCollection componentUsages;

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			ComponentUsages.DeleteAll(); // Tested in SaveAndDelete
		}

		#endregion

		#region FillWithValidTestData
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			OSB_ProductQuantity = 1m;
		}
#endif
		#endregion
	}
}
