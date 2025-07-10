using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[DependentBusinessObject(typeof(Order), "PlannedContainers")]
	public class OrderContainer : AutoJobOrderContainer, Integration.Forwarding.IOrderContainer
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are reviewed. All possible values are expected and handled.")]
		public OrderContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			// refresh the container number read-only
			J1_ContainerNumber = J1_ContainerNumber;
		}

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Property Overrides

		public override ZString J1_ContainerNumber
		{
			get { return base.J1_ContainerNumber; }
			set
			{
				base.J1_ContainerNumber = value;

				if (Order != null)
				{
					Order.OrderLines.MarkAsNeedingValidation();
				}

				if (value.IsEmpty)
				{
					J1_ContainerCountInfo.RefreshBinding();
				}
				else
				{
					J1_ContainerCount = 1;
				}
			}
		}

		[List("J1_RC_List")]
		public override ZGuid J1_RC
		{
			get
			{
				return base.J1_RC;
			}
			set
			{
				if (base.J1_RC != value)
				{
					base.J1_RC = value;
					if (Order != null)
					{
						Order.OrderLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected bool J1_ContainerCount_ReadOnly
		{
			get { return !J1_ContainerNumber.IsEmpty; }
		}

		#endregion

		#region Business Objects

		public Order Order => J1_ParentTableCode == JobOrderHeaderSchema.Constants.Prefix
			? Factory.Load<Order>(J1_ParentID)
			: null;

		#endregion

		#region List Properties

		RefContainerCollection fJ1_RC_List;
		public RefContainerCollection J1_RC_List
		{
			get
			{
				if (fJ1_RC_List == null)
				{
					fJ1_RC_List = new RefContainerCollection(Factory);
				}
				return fJ1_RC_List;
			}
		}

		#endregion
	}
}
