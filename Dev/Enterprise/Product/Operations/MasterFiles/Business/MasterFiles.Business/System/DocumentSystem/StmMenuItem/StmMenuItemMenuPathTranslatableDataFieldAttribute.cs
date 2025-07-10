using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	sealed class StmMenuItemMenuPathTranslatableDataFieldAttribute : StmMenuItemTranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public StmMenuItemMenuPathTranslatableDataFieldAttribute()
			: base(StmMenuItem.Schema.SU_MenuPath)
		{
			MaxLength = StmMenuItem.Schema.SU_MenuPathMaxLength;
		}

		public override IEnumerable<IResString> GetCompileTimeSystemCaptions()
		{
			return Split(base.GetCompileTimeSystemCaptions().Select(r => r.EnglishText)).Select(c => CustomizableDataResourceStrings.GetMultilingualString(this, null, c));
		}

		public override IEnumerable<IResString> GetRuntimeCaptions(IResString userCaption = null, object context = null)
		{
			return Split(base.GetRuntimeCaptions(userCaption).Select(r => r.EnglishText)).Select(c => CustomizableDataResourceStrings.GetMultilingualString(this, null, c));
		}

		IEnumerable<string> Split(IEnumerable<string> captions)
		{
			return captions.SelectMany(caption => caption.Trim('/').Split('/')).Select(caption => caption.Trim()).Where(caption => !string.IsNullOrEmpty(caption)).Distinct();
		}
	}
}
