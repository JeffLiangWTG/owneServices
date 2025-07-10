using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public abstract class WiseRatesViewsValidation<T> : ZValidation where T : BusinessObject, INeedCodeMappings
	{
		protected WiseRatesViewsValidation(T parent) : base(parent)
		{
			Argument.NotNull(parent, nameof(parent));

			this.parent = parent;
			UnmappedForeignCodes = new List<UnmappedForeignCode>();
		}

		protected readonly T parent;
		List<UnmappedForeignCode> UnmappedForeignCodes { get; }

		public override Type AutoValidationType => typeof(T);

		public override void ValidateAll()
		{
			ValidateAllCore();
			parent.SetUnmappedCodes(UnmappedForeignCodes);
		}

		protected abstract void ValidateAllCore();

		protected void AddUnmappedForeignCode(string foreignCode, string relationship)
		{
			if (!string.IsNullOrWhiteSpace(foreignCode))
			{
				UnmappedForeignCodes.Add(new UnmappedForeignCode(foreignCode, relationship));
			}
		}
	}
}
