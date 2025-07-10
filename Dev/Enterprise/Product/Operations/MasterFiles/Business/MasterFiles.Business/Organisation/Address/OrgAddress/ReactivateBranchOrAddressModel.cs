using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ReactivateBranchOrAddressModel : NonPersistentBusinessObject
	{
		public ReactivateBranchOrAddressModel(OrgAddressDependentCollection addresses)
		{
			foreach (OrgAddress address in addresses)
			{
				ReactivateBranchOrAddressCollection.Add(new ReactivateBranchOrAddressItem(address, this));
			}

			ForAddress = true;
		}

		public ReactivateBranchOrAddressModel(GlbBranchDependentCollection branches)
		{
			foreach (var branch in branches)
			{
				ReactivateBranchOrAddressCollection.Add(new ReactivateBranchOrAddressItem(branch, this));
			}

			ForAddress = false;
		}

		public ZBool SelectedAll
		{
			get => ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().All(u => u.Selected);
			set
			{
				foreach (var item in ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>())
				{
					item.Selected = value;
				}
			}
		}

		public ZPropertyInfo SelectedAllInfo => GetZPropertyInfo(Schema.SelectedAll);

		public ZBool HasSelectedItems => ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().Any(u => u.Selected);

		public ZPropertyInfo HasSelectedItemsInfo => GetZPropertyInfo(Schema.HasSelectedItems);

		ReactivateBranchOrAddressCollection reactivateBranchOrAddressCollection;

		public void ApplyActiveStatus()
		{
			ReactivateBranchOrAddressCollection.Cast<ReactivateBranchOrAddressItem>().ToList().ForEach(u => u.ApplyActiveStatus());
		}

		public ReactivateBranchOrAddressCollection ReactivateBranchOrAddressCollection
		{
			get
			{
				if (reactivateBranchOrAddressCollection == null)
				{
					reactivateBranchOrAddressCollection = new ReactivateBranchOrAddressCollection();
				}

				return reactivateBranchOrAddressCollection;
			}
		}

		public bool ForAddress { get; }

		public static class Schema
		{
			public const string SelectedAll = "SelectedAll";
			public const string HasSelectedItems = "HasSelectedItems";
		}
	}
}
