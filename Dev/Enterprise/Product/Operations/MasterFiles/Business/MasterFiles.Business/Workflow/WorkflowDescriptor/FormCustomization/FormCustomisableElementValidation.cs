using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class FormCustomisableElementValidation : AutoFormCustomisableElementValidation
	{
		public FormCustomisableElementValidation(AutoFormCustomisableElement parent)
			: base(parent)
		{ }

		#region Placement

		protected override void CheckPlacement()
		{
			base.CheckPlacement();

			Parent.PlacementLocalizedInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.PlacementInfo);

			if (Parent.PlacementInfo.HasNotifications())
			{
				Parent.PlacementInfo.ClearAllNotifications();
				Parent.PlacementLocalizedInfo.AddError(ListValidation.GetNotificationMessage(Parent.PlacementInfo).ToString());
			}

			if (Parent.ParentElements != null)
			{
				bool isVisibleField = Parent.Visible && Parent.ParentElements.Settings != null && Parent.ParentElements.Settings.DisplayFields.Contains(Parent);
				if (isVisibleField)
				{
					MandatoryValidation.CheckEntered(Parent.PlacementLocalizedInfo);
				}

				foreach (FormCustomisableElement element in Parent.ParentElements)
				{
					if (element != Parent &&
						!Parent.DisplayTab.IsEmpty &&
						element.DisplayTab == Parent.DisplayTab &&
						element.Placement == Parent.Placement &&
						(Parent.ElementGroup.IsEmpty || element.ElementGroup != Parent.ElementGroup))
					{
						Parent.PlacementLocalizedInfo.AddError(Res.GetString("682f6836-1f3a-4204-9a5d-3d7947cdcf2c", "You can only place a single item in the {0} position on the {1} tab.", Parent.PlacementLocalized, Parent.DisplayTabLocalized));
						break;
					}
				}

				if (Parent.ParentElements.Settings != null && Parent.ParentElements.Settings.Template != null &&
					Parent.ParentElements.Settings.Template.WorkflowDescriptor != null && Parent.ParentElements.Settings.Template.WorkflowDescriptor.FormCustomisationSettings != null &&
					!Parent.ParentElements.Settings.Template.WorkflowDescriptor.FormCustomisationSettings.IsTabPlacementAllowed(Parent.DisplayTabCode, Parent.Placement))
				{
					Parent.PlacementLocalizedInfo.AddError(Res.GetString("3ad2a29f-0514-474c-aaae-91b1547c6f84", "You cannot place any items in the {0} area of the {1}.", Parent.PlacementLocalized, Parent.DisplayTabLocalized));
				}

				if (!Parent.ParentElements.IsValidatingPlacementRecursively)
				{
					Parent.ParentElements.IsValidatingPlacementRecursively = true;
					foreach (FormCustomisableElement element1 in Parent.ParentElements)
					{
						if (element1 != Parent)
						{
							element1.Validation.ValidatePlacement();
						}
					}
					Parent.ParentElements.IsValidatingPlacementRecursively = false;
				}
			}
		}

		#endregion

		#region Row Number

		protected override void CheckRowNumber()
		{
			base.CheckRowNumber();

			if (Parent.RowNumber < 0)
			{
				Parent.RowNumberInfo.AddError(Res.GetString("8F2EE590-644D-4E02-A2E2-ED5FEC1AF476", "Row Numbers cannot be negative"));
			}

			if (!Parent.ElementGroup.IsEmpty && Parent.ParentElements != null && Parent.Visible && Parent.IsAvailable && Parent.IsInTemplate &&
				!(Parent.ParentElements.Settings?.Template?.P0_IsSystem == true))
			{
				foreach (FormCustomisableElement element in Parent.ParentElements)
				{
					if (element != Parent
						&& element.ElementGroup == Parent.ElementGroup
						&& element.RowNumber == Parent.RowNumber
						&& element.Visible
						&& element.IsAvailable
						&& element.IsInTemplate)
					{
						Parent.RowNumberInfo.AddError(Res.GetString("8950fc2f-2807-45ac-a256-b8d5a8136fc3", "Row Numbers must be unique for all elements within the {0} group.", Parent.ElementGroup));
					}
				}
			}

			if (Parent.ParentElements != null && !Parent.ParentElements.IsValidatingRowNumberRecursively)
			{
				Parent.ParentElements.IsValidatingRowNumberRecursively = true;
				foreach (FormCustomisableElement element in Parent.ParentElements)
				{
					if (element != Parent)
					{
						element.Validation.ValidateRowNumber();
					}
				}
				Parent.ParentElements.IsValidatingRowNumberRecursively = false;
			}
		}

		#endregion

		#region Display Tab

		protected override void CheckDisplayTab()
		{
			base.CheckDisplayTab();
			Parent.DisplayTabLocalizedInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidCode(Parent.DisplayTabInfo);

			if (Parent.DisplayTabInfo.HasNotifications())
			{
				Parent.DisplayTabInfo.ClearAllNotifications();
				Parent.DisplayTabLocalizedInfo.AddError(ListValidation.GetNotificationMessage(Parent.DisplayTabInfo).ToString());
			}
			ValidatePlacement();
		}

		#endregion

		public void ValidateIsInTemplate()
		{
			ValidateCalculatedProperty(Parent.IsInTemplateInfo);
		}

		protected void CheckIsInTemplate()
		{
			if (Parent.IsPersistedInDatabase && Parent.Visible && !Parent.IsInTemplate)
			{
				Parent.IsInTemplateInfo.AddWarning(ElementNotValidWarning);
			}
		}

		string ElementNotValidWarning
		{
			get { return Res.GetString("95f471a1-feda-49a2-80f6-d0b73566c545", "This element is not valid for this workflow type and its visibility setting will be disregarded"); }
		}

		#region Implementation

		public new FormCustomisableElement Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (FormCustomisableElement)base.Parent; }
		}

		#endregion
	}
}
