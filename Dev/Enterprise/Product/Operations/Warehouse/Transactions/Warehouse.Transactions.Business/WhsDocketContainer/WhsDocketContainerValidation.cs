
namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketContainerValidation : AutoWhsDocketContainerValidation
	{
		public WhsDocketContainerValidation(AutoWhsDocketContainer parent)
			: base(parent)
		{
		}

		public new WhsDocketContainer Parent
		{
			get { return (WhsDocketContainer)base.Parent; }
		}

		protected override void CheckWC_ContainerNum()
		{
			if (Parent.WC_ContainerNum.IsEmpty)
			{
				Parent.WC_ContainerNumInfo.AddError(Res.GetString("40a14b57-6350-4ff5-9f3d-e4d9b2a971c0", "Please enter a container number"));
			}
			else
			{
				CheckWC_ContainerNum_IsUniqueOnParent();
			}
		}

		void CheckWC_ContainerNum_IsUniqueOnParent()
		{
			var parent = Parent;
			var docket = parent.Docket;
			if (docket != null && !docket.IsDeleted)
			{
				foreach (WhsDocketContainer container in docket.Containers)
				{
					if (container.PK != parent.PK && container.WC_ContainerNum == parent.WC_ContainerNum)
					{
						parent.WC_ContainerNumInfo.AddError(Res.GetString("5a5f0b63-45cd-46d7-8337-008dbdca5025", "Duplicate Container Number: {0}", container.WC_ContainerNum));
						break;
					}
				}
			}
		}
	}
}
