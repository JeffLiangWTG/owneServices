using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbResourceValidation : GlbStaffValidation
	{
		public GlbResourceValidation(GlbStaff parent)
			: base(parent)
		{
		}

		#region GS_ResourceType

		protected override void CheckGS_ResourceType()
		{
			base.CheckGS_ResourceType();
			MandatoryValidation.CheckEntered(Parent.GS_ResourceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GS_ResourceTypeInfo);
		}

		#endregion

		#region GS_Code

		protected override void CheckGS_Code()
		{
			base.CheckGS_Code();
			if (Parent.GS_Code.SubstringSafe(0, 1) != "$")
			{
				Parent.GS_CodeInfo.AddError(Res.GetString("ef6ec26c-4701-43cb-b825-e2997eedeca5", "To distinguish a resource from staff, resources codes must start with the '$' character."));
			}

			if (Parent.GS_Code.Length <= 1)
			{
				Parent.GS_CodeInfo.AddError(Res.GetString("20b8e94c-7dad-47d1-bf6e-afc235662a5b", "Please specify a code of at least 2 characters."));
			}
		}

		#endregion
	}
}
