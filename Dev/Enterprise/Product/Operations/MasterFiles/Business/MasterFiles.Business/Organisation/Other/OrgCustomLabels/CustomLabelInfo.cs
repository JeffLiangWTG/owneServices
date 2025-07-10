using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Information about a custom field that shows up on the CustomFieldsUserControl.
	/// </summary>
	public class CustomLabelInfo : CustomLabelInfoBase
	{
		public CustomLabelInfo(string labelName, string propertyName, Type propertyType, MultilingualString defaultCaption, MultilingualString defaultHint, CustomLabelStyles styles, OrgHeader org, BusinessObjectFactory factory)
			: base(propertyName, propertyType, defaultCaption, defaultHint, styles, org, factory)
		{
			Argument.NotNull(factory, "factory"); // // In the case of no Org, OrgProxy is required and cannot be accessed if owned by another thread.

			this.LabelName = labelName;
		}

		public readonly string LabelName;

		public override MultilingualString Caption
		{
			get
			{
				if (Label != null && Label.OT_Caption.Trim().Length > 0)
				{
					return (NoResString)Label.OT_Caption;
				}
				else
				{
					return DefaultCaption ?? (NoResString)"";
				}
			}
		}

		public override string Hint
		{
			get
			{
				if (Label != null && Label.OT_Hint.Trim().Length > 0)
				{
					return Label.OT_Hint;
				}
				else
				{
					return DefaultHint ?? "";
				}
			}
		}

		public override bool IsEnabled
		{
			get
			{
				return
					(Styles & CustomLabelStyles.ShowByDefault) > 0 ||
					(Label != null && Label.OT_Caption.Trim().Length > 0);
			}
		}

		public override bool IsMandatory
		{
			get { return Label != null && Label.OT_IsMandatory; }
		}

		public override int Position
		{
			get { return Label != null ? Label.OT_Position : 0; }
		}

		protected OrgCustomLabels fLabel;
		protected OrgCustomLabels Label
		{
			get
			{
				if (!hasCalculatedLabel || (fLabel != null && fLabel.IsDeleted))
				{
					hasCalculatedLabel = true;
					if (Organisation != null)
					{
						fLabel = new CustomLabelProvider(Organisation.CustomLabels).GetLabelFallbackToCompanyOrgProxy(LabelName);
					}
					else
					{
						var currentCompany = GlbCompany.CurrentCompany;
						var orgProxy = currentCompany.Factory.ThreadSentry.IsOwner ? currentCompany.OrgProxy : Factory.Load<OrgHeader>(currentCompany.GC_OH_OrgProxy);
						if (orgProxy != null)
						{
							fLabel = orgProxy.CustomLabels.FindByFieldName(LabelName);
						}
					}
				}
				return fLabel;
			}
		}

#if DEBUG
		protected
#endif
		bool hasCalculatedLabel;
	}
}
