using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public class UtcOffsetFilterValidation : ModuleTextFilterValidation
	{
		public UtcOffsetFilterValidation(UtcOffsetFilter parent, IEnumerable<ZShort> utcOffsetList)
			: base(parent)
		{
			UtcFilter = parent;
			UtcOffsetList = utcOffsetList;
		}

		IEnumerable<ZShort> UtcOffsetList { get; }

		protected virtual void CheckUtcOffsetFrom()
		{
			if (UtcFilter.PropertyValidation != null)
			{
				UtcFilter.PropertyValidation(UtcFilter.UtcOffsetFromInfo);
				UtcFilter.PropertyValidation(UtcFilter.UtcOffsetToInfo);
			}

			if (UtcFilter.UtcOffsetFrom.IsEmpty
				|| !short.TryParse(UtcFilter.UtcOffsetFrom, out var from)
				|| !UtcOffsetList.Contains(from))
			{
				UtcFilter.UtcOffsetFromInfo.AddError(ErrorMessageInvalidUtcOffset);
			}
		}

		protected virtual void CheckUtcOffsetTo()
		{
			if (UtcFilter.PropertyValidation != null)
			{
				UtcFilter.PropertyValidation(UtcFilter.UtcOffsetFromInfo);
				UtcFilter.PropertyValidation(UtcFilter.UtcOffsetToInfo);
			}

			if (UtcFilter.UtcOffsetTo.IsEmpty
				|| !short.TryParse(UtcFilter.UtcOffsetTo, out var to)
				|| !UtcOffsetList.Contains(to))
			{
				UtcFilter.UtcOffsetToInfo.AddError(ErrorMessageInvalidUtcOffset);
			}
		}

		public override void ValidateAll()
		{
			ValidateUtcOffsetFrom();
			ValidateUtcOffsetTo();
		}

		public void ValidateUtcOffsetFrom()
		{
			ValidateCalculatedProperty(UtcFilter.UtcOffsetFromInfo);
			ValidateCalculatedProperty(UtcFilter.UtcOffsetToInfo);
		}

		public void ValidateUtcOffsetTo()
		{
			ValidateCalculatedProperty(UtcFilter.UtcOffsetFromInfo);
			ValidateCalculatedProperty(UtcFilter.UtcOffsetToInfo);
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		protected readonly UtcOffsetFilter UtcFilter;
		static string ErrorMessageInvalidUtcOffset { get { return Res.GetString("35924C5A-C367-477C-AA2D-48FBDCC51E27", "Please select a valid UTC offset from the list."); } }
	}
}
