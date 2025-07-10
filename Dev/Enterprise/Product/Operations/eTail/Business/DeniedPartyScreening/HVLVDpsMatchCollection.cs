using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class HVLVDpsMatchCollection : NonPersistentBusinessObjectCollection<HVLVDpsMatch>
	{
		public HVLVDpsMatchCollection(List<DpsResponseWithScreeningParty> responseWithScreeningParties, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(responseWithScreeningParties, nameof(responseWithScreeningParties));
			Argument.NotNull(factory, nameof(factory));

			AddRange(responseWithScreeningParties.Select(x => new HVLVDpsMatch(x, Factory)));
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		public override bool ReadOnly => true;
	}
}
