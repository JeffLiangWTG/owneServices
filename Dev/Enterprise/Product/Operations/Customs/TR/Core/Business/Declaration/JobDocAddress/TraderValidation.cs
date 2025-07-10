using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class TraderValidation : JobDocAddressValidation
	{
		public TraderValidation(Trader parent) : base(parent)
		{
		}

		public new Trader Parent => (Trader)base.Parent;

		protected override void CheckE2_AddressType()
		{
			base.CheckE2_AddressType();
			ListValidation.MessageErrorIfInvalidCode(Parent.E2_AddressTypeInfo);

			if (Parent.Declaration != null)
			{
				var traders = Parent.Declaration.Traders;
				var traderType = Parent.E2_AddressType;
				if (traders.Count(x => x.E2_AddressType == traderType) > 6)
				{
					Parent.E2_AddressTypeInfo.AddMessageError(Res.GetString("A2690F07-5861-45F2-8ABB-A6A100EAA6A5", "There can be maximum 6 company for {0} type.", traderType));
				}
			}
		}

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			if (!Parent.E2_AddressType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_OA_AddressInfo);
			}
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();
			if (Parent.OrganisationPK.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OrganisationPKInfo);
			}
			else
			{
				bool found = false;
				var traders = Parent?.Declaration?.Traders;
				if (traders != null)
				{
					foreach (Trader otherTrader in traders)
					{
						if (otherTrader != Parent && otherTrader.E2_AddressType == Parent.E2_AddressType && otherTrader.OrganisationPK == Parent.OrganisationPK)
						{
							found = true;
						}
					}

					if (found)
					{
						Parent.OrganisationPKInfo.AddMessageError(Res.GetString("577EF57F-E91A-4EC2-BAFA-0B53BBCEE635", "Duplicate Organization not allowed."));
					}
				}
			}
		}
	}
}
