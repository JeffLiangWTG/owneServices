using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class EIDOMessagingIdentityValidation : AutoEIDOMessagingIdentityValidation
	{
		public EIDOMessagingIdentityValidation(AutoEIDOMessagingIdentity parent)
			: base(parent) { }

		protected override void CheckPrincipalPK()
		{
			base.CheckPrincipalPK();
			MandatoryValidation.CheckEntered(Parent.PrincipalPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.PrincipalPKInfo, Parent.Lookups.Principals);

			if (!Parent.PrincipalPK.IsEmpty)
			{
				foreach (EIDOMessagingIdentity otherIdentity in Parent.Parent.Identities)
				{
					if (otherIdentity.PrincipalPK == Parent.PrincipalPK && otherIdentity != Parent)
					{
						Parent.PrincipalPKInfo.AddError(Res.GetString("e5425f51-2699-4b3f-9977-4e0d248774c0", "This principal already exists in this list."));
						break;
					}
				}
			}
		}

		protected override void CheckSenderID()
		{
			base.CheckSenderID();
			MandatoryValidation.CheckEntered(Parent.SenderIDInfo);
		}

		protected override void CheckRecipientID()
		{
			base.CheckRecipientID();
			MandatoryValidation.CheckEntered(Parent.RecipientIDInfo);
		}

		protected override void CheckPassword()
		{
			base.CheckPassword();
			MandatoryValidation.CheckEntered(Parent.PasswordInfo);
		}

		#region Implementation

		public new EIDOMessagingIdentity Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (EIDOMessagingIdentity)base.Parent; }
		}

		#endregion
	}
}


