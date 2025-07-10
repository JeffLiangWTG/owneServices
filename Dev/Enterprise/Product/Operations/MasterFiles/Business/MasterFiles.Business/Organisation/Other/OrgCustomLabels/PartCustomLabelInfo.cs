using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class PartCustomLabelInfo : CustomLabelInfoBase
	{
		public PartCustomLabelInfo(string partCaption, string partType, string propertyName, Type propertyType, MultilingualString defaultCaption, OrgHeader org, BusinessObjectFactory factory)
			: this(partCaption, partType, propertyName, propertyType, defaultCaption, defaultCaption, CustomLabelStyles.None, org, factory)
		{
		}

		public PartCustomLabelInfo(
			string partCaption, string partType, string propertyName, Type propertyType, MultilingualString defaultCaption, MultilingualString defaultHint, CustomLabelStyles styles, OrgHeader org, BusinessObjectFactory factory)
			: base(propertyName, propertyType, defaultCaption, defaultHint, styles, org, factory)
		{
			this.PartCaption = partCaption;
		}

		public override MultilingualString Caption
		{
			get { return PartCaption.IsEmpty ? DefaultCaption : (NoResString)PartCaption; }
		}

		public override string Hint
		{
			get { return Caption; }
		}

		public override bool IsEnabled
		{
			get { return Caption != DefaultCaption; }
		}

		public override bool IsMandatory
		{
			get { return false; }
		}

		public override int Position
		{
			get { return 0; }
		}

		#region Implementation

		readonly ZString PartCaption;

		#endregion
	}
}
