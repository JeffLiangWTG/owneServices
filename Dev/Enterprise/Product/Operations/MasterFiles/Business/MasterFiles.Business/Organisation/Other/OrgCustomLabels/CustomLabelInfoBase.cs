using System;
using System.Diagnostics;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Information about a custom field that shows up on the CustomFieldsUserControl.
	/// </summary>
	[DebuggerDisplay("PropertyName = {PropertyName}, DefaultCaption = {DefaultCaption}")]
	public abstract class CustomLabelInfoBase
	{
		public CustomLabelInfoBase(
			string propertyName, Type propertyType, MultilingualString defaultCaption, MultilingualString defaultHint,
			CustomLabelStyles styles, OrgHeader org, BusinessObjectFactory factory)
		{
			this.PropertyName = propertyName;
			this.PropertyType = propertyType;
			this.DefaultCaption = defaultCaption;
			this.DefaultHint = defaultHint;
			this.Styles = styles;
			this.Organisation = org;
			this.Factory = factory;
		}

		protected readonly OrgHeader Organisation;
		protected readonly BusinessObjectFactory Factory;
		public readonly CustomLabelStyles Styles;
		public readonly string PropertyName;
		public readonly Type PropertyType;

		public abstract MultilingualString Caption { get; }
		public abstract string Hint { get; }
		public abstract bool IsEnabled { get; }
		public abstract bool IsMandatory { get; }
		public abstract int Position { get; }

		public void OnQueryToolTip(object sender, ToolTipInfo info)
		{
			info.ToolTipCaption = Caption;
			info.ToolTipText = Hint;
		}

		#region Implementation

		protected readonly MultilingualString DefaultCaption;
		protected readonly MultilingualString DefaultHint;

		#endregion
	}
}
