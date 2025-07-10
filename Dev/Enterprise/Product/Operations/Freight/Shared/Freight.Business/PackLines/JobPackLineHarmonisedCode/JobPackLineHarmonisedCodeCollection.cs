using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobPackLineHarmonisedCodeCollection : ActiveBusinessObjectCollection<JobPackLineHarmonisedCode>
	{
		public JobPackLineHarmonisedCodeCollection(PackLine parent)
			: base(parent.Factory, parent, new ZQuery(), JobPackLineHarmonisedCodeSchema.JLH_JL)
		{ }

		#region Binding Item

		/// <summary>
		/// This is used to allow binding to this item on a panel (not grid). 
		/// This item will automatically translate into a real item when data is entered.
		/// </summary>
		public PackLineHarmonisedCodeStandAloneCollection FirstItemForBinding
		{
			get
			{
				if (Count > 0)
				{
					this[0].CountryOrCodeUpdated -= PackLineHarmonisedCodeCollection_CountryOrCodeUpdated;
					BindingItems.AddIfNotExists(this[0]);
					BindingItems.RemoveAllExceptNewItem(this[0]);
				}
				else
				{
					var autoAddedItem = BindingItems.Count > 0 ? BindingItems[0] : null;
					if (autoAddedItem == null || !autoAddedItem.IsAutoAddedItem || autoAddedItem.IsDeleted)
					{
						autoAddedItem = BindingItems.AddNew();
						BindingItems.RemoveAllExceptNewItem(autoAddedItem);
						autoAddedItem.IsAutoAddedItem = true;
						autoAddedItem.AutoAddedItemNowValid += new EventHandler(AutoAddedItem_AutoAddedItemNowValid);
					}
				}

				BindingItems[0].CountryOrCodeUpdated += PackLineHarmonisedCodeCollection_CountryOrCodeUpdated;

				return BindingItems;
			}
		}

		void PackLineHarmonisedCodeCollection_CountryOrCodeUpdated(object sender, EventArgs e)
		{
			var item = FirstItemForBinding[0];
			if (!item.IsSettingHasChangesSuspended)
			{
				if (Count == 1
					&& !item.IsAutoAddedItem
					&& item.JLH_RN_NKCountry.IsEmpty
					&& item.JLH_Code.IsEmpty)
				{
					item.CountryOrCodeUpdated -= PackLineHarmonisedCodeCollection_CountryOrCodeUpdated;
					DeleteAll();
				}

				FirstItemForBinding.RefreshBinding();
			}
		}

		void AutoAddedItem_AutoAddedItemNowValid(object sender, EventArgs e)
		{
			var item = (JobPackLineHarmonisedCode)sender;

			item.AutoAddedItemNowValid -= AutoAddedItem_AutoAddedItemNowValid;

			if (item.IsAutoAddedItem && !item.IsDeleted)
			{
				if (!item.IsEmptyItem)
				{
					item.IsAutoAddedItem = false;
					Add(item);
				}
			}
		}

		PackLineHarmonisedCodeStandAloneCollection BindingItems
		{
			get
			{
				if (bindingItems == null)
				{
					bindingItems = new PackLineHarmonisedCodeStandAloneCollection(Factory);
					CountChanged += delegate
					{
						((IActiveBusinessObjectCollection)FirstItemForBinding).FireListResetEvent();
						FirstItemForBinding.RefreshBinding();
					};

					if (ReadOnly)
					{
						bindingItems.SetReadOnlyIncludingChildren(true);
					}
				}
				return bindingItems;
			}
		}
		PackLineHarmonisedCodeStandAloneCollection bindingItems;

		public class PackLineHarmonisedCodeStandAloneCollection : ActiveBusinessObjectCollection<JobPackLineHarmonisedCode>
		{
			public PackLineHarmonisedCodeStandAloneCollection(BusinessObjectFactory factory)
				: base(factory, new AdhocCollectionRelationship(typeof(JobPackLineHarmonisedCode)))
			{
			}

			public void AddIfNotExists(JobPackLineHarmonisedCode newItem)
			{
				Argument.NotNull(newItem, nameof(newItem));

				if (!Contains(newItem))
				{
					Add(newItem);
				}
			}

			public void RemoveAllExceptNewItem(JobPackLineHarmonisedCode newItem)
			{
				Argument.NotNull(newItem, nameof(newItem));

				foreach (var existingItem in Enumerable.Except(this, new[] { newItem }))
				{
					RemoveFromRelationship(existingItem);
				}
			}
		}

		#endregion

		#region Set Read Only Including Children

		protected override void SetReadOnlyIncludingChildrenCore(bool readOnly)
		{
			base.SetReadOnlyIncludingChildrenCore(readOnly);

			if (bindingItems != null)
			{
				bindingItems.SetReadOnlyIncludingChildren(readOnly);
			}

			if (HSCountryManager != null)
			{
				HSCountryManager.SetReadOnlyIncludingChildren(readOnly);
			}

			if (HSCodeManager != null)
			{
				HSCodeManager.SetReadOnlyIncludingChildren(readOnly);
			}
		}

		#endregion

		#region Multiple Item Managers

		#region HSCountryManager

		public MultipleItemManager HSCountryManager
		{
			get
			{
				if (hsCountryManager == null)
				{
					hsCountryManager = new MultipleItemManager(this, JobPackLineHarmonisedCodeSchema.JLH_RN_NKCountry, true);
					if (ReadOnly)
					{
						hsCountryManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return hsCountryManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager hsCountryManager;

		#endregion

		#region HSCodeManager

		public MultipleItemManager HSCodeManager
		{
			get
			{
				if (hsCodeManager == null)
				{
					hsCodeManager = new MultipleItemManager(this, JobPackLineHarmonisedCodeSchema.JLH_Code, true);
					hsCodeManager.ReadOnlyWhenMany = true;

					if (ReadOnly)
					{
						hsCodeManager.SetReadOnlyIncludingChildren(true);
					}
				}

				return hsCodeManager;
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		MultipleItemManager hsCodeManager;

		#endregion

		#endregion

		#region Lookups

		#region Countries

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		#endregion
	}
}
