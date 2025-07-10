using CargoWise.ComponentModel;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.Business
{
	public sealed class BaseJobDeclarationTransportValidation : JobConsolTransportValidation
	{
		public BaseJobDeclarationTransportValidation(Transport transport)
			: base(transport) { }

		protected override void CheckJW_RL_NKLoadPort()
		{
			base.CheckJW_RL_NKLoadPort();

			if (!Parent.JW_RL_NKLoadPortInfo.HasErrors())
			{
				BaseJobDeclaration declaration = (BaseJobDeclaration)Parent.Parent;
				if (declaration != null && RoutingLegLoadsAtPort(declaration.JE_RL_NKFinalDestination))
				{
					Parent.JW_RL_NKLoadPortInfo.AddError(Res.GetString("3bd01a78-b176-4a30-9a9b-d005a18c7b8c", "A routing leg cannot load at the declaration's destination."));
				}
			}
		}

		protected override void CheckJW_RL_NKDiscPort()
		{
			base.CheckJW_RL_NKDiscPort();

			if (!Parent.JW_RL_NKDiscPortInfo.HasErrors())
			{
				BaseJobDeclaration declaration = (BaseJobDeclaration)Parent.Parent;
				if (declaration != null && RoutingLegDischargesAtPort(declaration.JE_RL_NKOrigin))
				{
					Parent.JW_RL_NKDiscPortInfo.AddError(Res.GetString("f9500ad7-5eac-442f-8201-3efe2106fd81", "A routing leg cannot discharge at the declaration's origin."));
				}
			}
		}
	}
}
