using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ReconEntryHeaderValidation : ZValidation
	{
		public ReconEntryHeaderValidation(ReconEntryHeader reconEntry)
			: base(reconEntry)
		{
		}

		public override void ValidateAll()
		{
		}

		public override Type AutoValidationType
		{
			get { return typeof(ReconEntryHeaderValidation); }
		}
	}
}
