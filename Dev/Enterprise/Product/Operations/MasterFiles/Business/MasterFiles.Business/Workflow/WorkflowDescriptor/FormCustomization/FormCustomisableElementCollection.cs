using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public sealed class FormCustomisableElementCollection : NonPersistentBusinessObjectCollection<FormCustomisableElement>
	{
		public FormCustomisableElementCollection()
		{
		}

		public FormCustomisableElementCollection(FormCustomisationSettings settings)
			: this()
		{
			Settings = settings;
		}

		public readonly FormCustomisationSettings Settings;

		#region Element Management

		public bool? IsElementVisible(string elementName)
		{
			FormCustomisableElement element = GetElement(elementName) ?? GetElementRepresentingGroup(elementName);

			return element != null ? new bool?(element.IsAvailable && element.IsInTemplate && element.Visible) : null;
		}

		public TabPlacement GetTabPlacement(string elementName)
		{
			FormCustomisableElement element = GetElement(elementName) ?? GetElementRepresentingGroup(elementName);
			return element != null ? new TabPlacement(element.DisplayTabCode, element.Placement, element.RowNumber) : new TabPlacement();
		}

		internal FormCustomisableElement GetElement(string elementName)
		{
			foreach (FormCustomisableElement element in this)
			{
				if (element.ElementName == elementName)
				{
					return element;
				}
			}

			return null;
		}

		FormCustomisableElement GetElementRepresentingGroup(string elementName)
		{
			FormCustomisableElement result = null;

			foreach (FormCustomisableElement element in this)
			{
				if (element.ElementGroup == elementName)
				{
					result = element;

					if (!string.IsNullOrEmpty(element.DisplayTabCode) && !string.IsNullOrEmpty(element.Placement) && element.IsInTemplate && element.IsAvailable)
					{
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region Add

		public FormCustomisableElement Add(MultilingualString elementDescription, string elementName, bool canContainOtherElements = false, MultilingualString groupName = null, string defaultTabPage = "", string placement = "", int rowNumber = 0, bool isVisible = true)
		{
			FormCustomisableElement element = AddNew();

			element.ElementDescription = elementDescription.GetUnresolvedString();
			element.ElementDescriptionMultilingual = elementDescription;
			element.ElementName = elementName;
			element.ElementGroupMultilingual = groupName ?? (NoResString)string.Empty;
			element.ElementGroup = element.ElementGroupMultilingual.GetUnresolvedString();
			element.CanContainOtherElements = canContainOtherElements;
			element.DisplayTabCode = defaultTabPage;
			element.Placement = placement;
			element.RowNumber = rowNumber;
			element.Visible = isVisible;

			return element;
		}

		protected override BusinessObject AddNewCore()
		{
			FormCustomisableElement element = (FormCustomisableElement)base.AddNewCore();
			element.ParentElements = this;
			return element;
		}

		public override void Add(BusinessObject businessObject)
		{
			FormCustomisableElement element = (FormCustomisableElement)businessObject;
			base.Add(element);
			element.ParentElements = this;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FormCustomisableElement();
		}

		internal bool IsValidatingPlacementRecursively { get; set; }
		internal bool IsValidatingRowNumberRecursively { get; set; }

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);

			if (!HasChanges)
			{
				IBusinessObjectCollectionInternals internals = this;
				internals.HasChangesFromDelete = true;
			}
		}

		#endregion

		internal void Hydrate(FormCustomisationSettingsStorageTabCollection tabsCollection)
		{
			if (!ReferenceEquals(null, tabsCollection))
			{
				SuspendValidation();
				try
				{
					foreach (FormCustomisationSettingsStorageTab tab in tabsCollection)
					{
						FormCustomisableElement element = AddNew();
						element.Hydrate(tab);
					}
				}
				finally
				{
					ResumeValidation();
				}
			}
		}

		internal void DeHydrate(FormCustomisationSettingsStorageTabCollection tabsCollection)
		{
			if (!ReferenceEquals(null, tabsCollection))
			{
				foreach (FormCustomisableElement element in this)
				{
					FormCustomisationSettingsStorageTab tab = tabsCollection.AddNew();
					element.DeHydrate(tab);
				}
			}
		}

		internal void Hydrate(FormCustomisationSettingsStorageFieldCollection fieldCollection)
		{
			if (!ReferenceEquals(null, fieldCollection))
			{
				SuspendValidation();
				try
				{
					foreach (FormCustomisationSettingsStorageField field in fieldCollection)
					{
						FormCustomisableElement element = AddNew();
						element.Hydrate(field);
					}
				}
				finally
				{
					ResumeValidation();
				}
			}
		}

		internal void DeHydrate(FormCustomisationSettingsStorageFieldCollection fieldCollection)
		{
			if (!ReferenceEquals(null, fieldCollection))
			{
				foreach (FormCustomisableElement element in this)
				{
					FormCustomisationSettingsStorageField field = fieldCollection.AddNew();
					element.DeHydrate(field);
				}
			}
		}

		public override bool ReadOnly => base.ReadOnly || (Settings?.Template?.P0_IsScreenLayoutFallback ?? false);
	}
}
