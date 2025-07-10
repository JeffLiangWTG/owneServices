using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ReactivateBranchOrAddressItem : NonPersistentBusinessObject
	{
		readonly OrgAddress address;
		readonly GlbBranch branch;
		readonly ReactivateBranchOrAddressModel parent;
		ZBool selected;

		public ReactivateBranchOrAddressItem() { }

		public ReactivateBranchOrAddressItem(OrgAddress address, ReactivateBranchOrAddressModel parent)
		{
			this.parent = parent;
			this.address = address;
			selected = address.OA_IsActive;
		}

		public ReactivateBranchOrAddressItem(GlbBranch branch, ReactivateBranchOrAddressModel parent)
		{
			this.parent = parent;
			this.branch = branch;
			selected = branch.GB_IsActive;
		}

		public ZString Code => branch?.GB_Code ?? address?.OA_Code ?? ZString.Empty;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(Schema.Code);

		public ZString Name => branch?.GB_BranchName ?? ZString.Empty;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(Schema.Name);

		public ZString Address => branch?.GetFullAddressString() ?? address?.GetFullAddressString() ?? ZString.Empty;

		public ZPropertyInfo AddressInfo => GetZPropertyInfo(Schema.Address);

		public ZBool Selected
		{
			get => selected;
			set
			{
				selected = value;
				SelectedInfo.RefreshBinding();
				parent?.SelectedAllInfo.RefreshBinding();
				parent?.HasSelectedItemsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SelectedInfo => GetZPropertyInfo(Schema.Selected);

		public static class Schema
		{
			public const string Code = "Code";
			public const string Name = "Name";
			public const string Address = "Address";
			public const string Selected = "Selected";
		}

		public void ApplyActiveStatus()
		{
			if (address != null)
			{
				address.OA_IsActive = Selected;
			}
			else if (branch != null)
			{
				branch.GB_IsActive = Selected;
			}
		}
	}
}
