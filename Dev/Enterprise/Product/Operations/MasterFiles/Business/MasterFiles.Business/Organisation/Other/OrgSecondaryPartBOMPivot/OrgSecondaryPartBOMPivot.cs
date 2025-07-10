using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecondaryPartBOMPivot : AutoOrgSecondaryPartBOMPivot
	{
		public OrgSecondaryPartBOMPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZGuid OPP_OE_Component
		{
			get => base.OPP_OE_Component;
			set
			{
				base.OPP_OE_Component = value;

				if (!IsValidationSuspended)
				{
					// tested in OrgSecondaryPartBOMPivotValidationTest
					Validation.ValidateOPP_ComponentQuantity();
				}
			}
		}

		#region FillWithValidTestData
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			OPP_ComponentQuantity = 1m;
		}
#endif
		#endregion
	}
}
