using System;
using System.ComponentModel;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[DebuggerDisplay("ElementName: {ElementName}")]
	public sealed class FormCustomisableElement : AutoFormCustomisableElement, IObsoleteValidation
	{
		public new class Schema : AutoFormCustomisableElement.Schema
		{
			public const string IsInTemplate = "IsInTemplate";
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Visible = true;
			IsInTemplate = true;
			IsAvailableFunction = DefaultIsAvailable;
		}

		#endregion

		#region Properties

		#region Element Name / Desc

		public MultilingualString ElementDescriptionMultilingual
		{
			get
			{
				return elementDescriptionMultingual == null || elementDescriptionMultingual.IsEmpty ? (NoResString)ElementDescription : elementDescriptionMultingual;
			}
			set
			{
				elementDescriptionMultingual = value;
				ElementDescriptionMultilingualInfo.RefreshBinding();
			}
		}
		MultilingualString elementDescriptionMultingual;

		public ZPropertyInfo ElementDescriptionMultilingualInfo
		{
			get { return GetZPropertyInfo(nameof(ElementDescriptionMultilingual)); }
		}

		public bool ElementName_ReadOnly
		{
			get { return true; }
		}

		public bool ElementDescription_ReadOnly
		{
			get { return true; }
		}

		public bool ElementDescriptionMultilingual_ReadOnly
		{
			get { return ElementDescription_ReadOnly; }
		}

		#endregion

		#region Element Group

		[ReadOnly(true)]
		public override ZString ElementGroup
		{
			get { return base.ElementGroup; }
			set
			{
				base.ElementGroup = value;
				if (ElementGroup.IsEmpty)
				{
					using (GetValidationSuspender())
					{
						RowNumber = 0;
					}
				}
			}
		}

		[ReadOnly(true)]
		public MultilingualString ElementGroupMultilingual
		{
			get
			{
				return elementGroupMultingual == null || elementGroupMultingual.IsEmpty ? (NoResString)ElementGroup : elementGroupMultingual;
			}
			set
			{
				elementGroupMultingual = value;
				ElementGroupMultingualInfo.RefreshBinding();
			}
		}
		MultilingualString elementGroupMultingual;

		public ZPropertyInfo ElementGroupMultingualInfo
		{
			get { return GetZPropertyInfo(nameof(ElementGroupMultilingual)); }
		}

		void SynchroniseElementsInSameGroup()
		{
			if (ParentElements != null)
			{
				foreach (FormCustomisableElement element in ParentElements)
				{
					if (element != this && !ElementGroup.IsEmpty && element.ElementGroup == ElementGroup && element.Visible)
					{
						if (!Placement.IsEmpty && element.Placement != Placement)
						{
							element.Placement = Placement;
						}

						if (!DisplayTab.IsEmpty && element.DisplayTab != DisplayTab)
						{
							element.DisplayTabCode = DisplayTabCode;
						}
					}
				}
			}
		}

		#endregion

		#region Visible

		public override ZBool Visible
		{
			get { return base.Visible; }
			set
			{
				base.Visible = value;
				if (!Visible)
				{
					RowNumber = 0;
					Placement = ZString.Empty;
					DisplayTab = ZString.Empty;
				}
			}
		}

		#endregion

		public ZBool IsInTemplate
		{
			get { return isInTemplate; }
			internal set
			{
				if (SetNonPersistentPropertyValue(IsInTemplateInfo, ref isInTemplate, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateIsInTemplate();
					}
				}
			}
		}
		ZBool isInTemplate;

		public ZPropertyInfo IsInTemplateInfo
		{
			get { return GetZPropertyInfo(Schema.IsInTemplate); }
		}

		public ZBool IsAvailable => IsAvailableFunction();

		public Func<ZBool> IsAvailableFunction { get; set; }

		ZBool DefaultIsAvailable() => true;

		public override bool ReadOnly
		{
			get { return base.ReadOnly || (ParentElements?.ReadOnly ?? false) || IsPersistedInDatabase && (!IsInTemplate || !IsAvailable); }
			set { base.ReadOnly = value; }
		}

		internal ZBool IsPersistedInDatabase { get; private set; }

		#region Placement

		[List("PlacementList")]
		public override ZString Placement
		{
			get { return base.Placement; }
			set
			{
				base.Placement = value;
				SynchroniseElementsInSameGroup();
			}
		}

		[List("PlacementList")]
		[MaxLength(Schema.PlacementMaxLength)]
		public ZString PlacementLocalized
		{
			get
			{
				var description = PlacementList.GetMultilingualDescriptionFromCode(Placement);
				return string.IsNullOrEmpty(description) ? Placement : description;
			}
			set
			{
				var code = PlacementList.GetCodeFromDescription(value);
				Placement = string.IsNullOrEmpty(code) ? value : (ZString)code;
				PlacementLocalizedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PlacementLocalizedInfo
		{
			get { return GetZPropertyInfo(nameof(PlacementLocalized)); }
		}

		public bool Placement_ReadOnly
		{
			get { return !Visible; }
		}

		public CodeDescriptionPairList PlacementList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(TabPlacement.Placements.TopLeft, ResString.GetMultilingualString("8403fdda-a9d1-4a52-bfd5-778d5d2743d0", "Left Top"));
				result.AddPair(TabPlacement.Placements.BottomLeft, ResString.GetMultilingualString("261f8e5b-52b6-4987-bb31-8300d879d261", "Left Bottom"));
				result.AddPair(TabPlacement.Placements.TopMiddle, ResString.GetMultilingualString("21ab6981-88f2-43f4-9bbb-3f323f8a944b", "Middle Top"));
				result.AddPair(TabPlacement.Placements.BottomMiddle, ResString.GetMultilingualString("1250aa71-4f01-4cdd-8348-bf2b91104a37", "Middle Bottom"));
				result.AddPair(TabPlacement.Placements.TopRight, ResString.GetMultilingualString("a1e45ccf-1828-4015-9126-d82f9315f7c8", "Right Top"));
				result.AddPair(TabPlacement.Placements.BottomRight, ResString.GetMultilingualString("3bb52c89-e343-4316-ad3c-a3d126611961", "Right Bottom"));
				return result;
			}
		}

		#endregion

		#region Row Number

		public bool RowNumber_ReadOnly
		{
			get { return ElementGroup.IsEmpty || !Visible; }
		}

		#endregion

		#region Display Tab

		[List("DisplayTabList")]
		public override ZString DisplayTab
		{
			get { return base.DisplayTab; }
			set
			{
				base.DisplayTab = value;
				if (!IsSettingDisplayTab)
				{
					if (ParentElements != null && ParentElements.Settings != null)
					{
						foreach (FormCustomisableElement element in ParentElements.Settings.DisplayTabs)
						{
							if (element.ElementDescription == value)
							{
								fDisplayTabCode = element.ElementName;
							}
						}
					}
				}
				SynchroniseElementsInSameGroup();
			}
		}

		[List("DisplayTabList")]
		[MaxLength(Schema.DisplayTabMaxLength)]
		public ZString DisplayTabLocalized
		{
			get
			{
				var description = DisplayTabList.GetMultilingualDescriptionFromCode(DisplayTab);
				return string.IsNullOrEmpty(description) ? DisplayTab : description;
			}
			set
			{
				var code = DisplayTabList.GetCodeFromDescription(value);
				DisplayTab = string.IsNullOrEmpty(code) ? value : (ZString)code;
				DisplayTabLocalizedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DisplayTabLocalizedInfo
		{
			get { return GetZPropertyInfo(nameof(DisplayTabLocalized)); }
		}

		public bool DisplayTab_ReadOnly
		{
			get { return !Visible; }
		}

		[BusinessObjectTestExclude]
		internal FormCustomisableElementCollection ParentElements { get; set; }

		public CodeDescriptionPairList DisplayTabList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				if (ParentElements != null && ParentElements.Settings != null)
				{
					foreach (FormCustomisableElement tab in ParentElements.Settings.DisplayTabs)
					{
						if (tab.CanContainOtherElements)
						{
							result.AddPair(tab.ElementDescription, tab.ElementDescriptionMultilingual);
						}
					}
				}

				return result;
			}
		}

		internal bool CanContainOtherElements { get; set; }

		[MaxLength(100)]
		public ZString DisplayTabCode
		{
			get { return fDisplayTabCode; }
			set
			{
				if (fDisplayTabCode != value)
				{
					CheckMaximumLength(DisplayTabCodeInfo, value);
					fDisplayTabCode = value;
					DisplayTabCodeInfo.RefreshBinding();

					if (!value.IsEmpty && ParentElements != null && ParentElements.Settings != null)
					{
						foreach (FormCustomisableElement element in ParentElements.Settings.DisplayTabs)
						{
							if (element.ElementName == value)
							{
								if (DisplayTab != element.ElementDescription)
								{
									IsSettingDisplayTab = true;
									DisplayTab = element.ElementDescription;
									IsSettingDisplayTab = false;
								}
							}
						}
					}

					SynchroniseElementsInSameGroup();
				}
			}
		}

		bool IsSettingDisplayTab;

		ZString fDisplayTabCode;

		public ZPropertyInfo DisplayTabCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DisplayTabCode)); }
		}

		#endregion

		#endregion

		public void CopyValues(FormCustomisableElement elementToCopyValuesFrom)
		{
			if (elementToCopyValuesFrom != null)
			{
				using (GetValidationSuspender())
				using (SuspendSettingHasChanges())
				{
					ElementName = elementToCopyValuesFrom.ElementName;
					ElementDescription = elementToCopyValuesFrom.ElementDescription;
					ElementDescriptionMultilingual = elementToCopyValuesFrom.ElementDescriptionMultilingual;
					ElementGroup = elementToCopyValuesFrom.ElementGroup;
					ElementGroupMultilingual = elementToCopyValuesFrom.ElementGroupMultilingual;
					CanContainOtherElements = elementToCopyValuesFrom.CanContainOtherElements;
					DisplayTabCode = elementToCopyValuesFrom.DisplayTabCode;
					Placement = elementToCopyValuesFrom.Placement;
					RowNumber = elementToCopyValuesFrom.RowNumber;
					IsAvailableFunction = elementToCopyValuesFrom.IsAvailableFunction;
					Visible = elementToCopyValuesFrom.Visible;
				}
			}
		}

		public void Hydrate(FormCustomisationSettingsStorageTab tab)
		{
			if (tab != null)
			{
				using (SuspendSettingHasChanges())
				{
					ElementName = tab.Name;
					ElementDescription = tab.Description;
					Visible = tab.Visible;
					IsPersistedInDatabase = true;
				}
			}
		}

		public void DeHydrate(FormCustomisationSettingsStorageTab tab)
		{
			if (tab != null)
			{
				tab.Name = ElementName;
				tab.Description = ElementDescription;
				tab.Visible = Visible;
			}
		}

		public void Hydrate(FormCustomisationSettingsStorageField field)
		{
			if (field != null)
			{
				using (SuspendSettingHasChanges())
				{
					ElementName = field.Name;
					ElementDescription = field.Description;
					ElementGroup = field.Group;
					Visible = field.Visible;
					DisplayTab = field.TabName;
					Placement = field.Placement;
					RowNumber = field.Position;
					IsPersistedInDatabase = true;
				}
			}
		}

		public void DeHydrate(FormCustomisationSettingsStorageField field)
		{
			if (field != null)
			{
				field.Name = ElementName;
				field.Description = ElementDescription;
				field.Group = ElementGroup;
				field.Visible = Visible;
				field.TabName = DisplayTab;
				field.Placement = Placement;
				field.Position = RowNumber;
			}
		}
	}
}
