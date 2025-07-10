//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoTransportBookingTemplateValidation
//
//    This class should be used for overriding validation in AutoTransportBookingTemplateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.TransportBookings.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Schema;
	using Common = TransportCommon.Business.Common;

	public sealed class DtbBookingTmplValidation : Common.DtbBookingTmplValidation
	{
		public DtbBookingTmplValidation(DtbBookingTmpl parent)
			: base(parent)
		{
		}

		protected override void CheckKT_Code()
		{
			base.CheckKT_Code();

			var info = Parent.KT_CodeInfo;

			MandatoryValidation.CheckEntered(info);

			if (!info.HasErrors())
			{
				if (Parent.KT_Code.Length != 4)
				{
					info.AddError(Res.GetString("2f974767-0b81-455f-9684-4703bb0a4268", "Transport Booking Template Code needs to be 4 characters"));
				}
			}

			if (!info.HasErrors())
			{
				var codeCount = Factory.Load<DtbBookingTmpl>(new ZQuery(DtbBookingTmplSchema.KT_Code, Parent.KT_Code)).Length;
				if (codeCount > 1)
				{
					info.AddError(Res.GetString("f29b03b0-d396-43a4-8c36-3fd5bc7da4cc", "Another Transport Booking Template is already using the Code '{0}'", Parent.KT_Code));
				}
			}
		}

		protected override void CheckKT_Description()
		{
			base.CheckKT_Description();

			MandatoryValidation.CheckEntered(Parent.KT_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.KT_DescriptionInfo);
		}

		protected override void CheckKT_Direction()
		{
			base.CheckKT_Direction();

			MandatoryValidation.CheckEntered(Parent.KT_DirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.KT_DirectionInfo);
		}

		protected override void CheckKT_IsActive()
		{
			base.CheckKT_IsActive();

			var dtbBookingTmpl = Parent;
			var info = dtbBookingTmpl.KT_IsActiveInfo;

			var isTemplateUsedInRegistry = Template.IsUsedInRegistry;
			var isChangingFromActiveToInactive = !dtbBookingTmpl.KT_IsActive && (ZBool)info.OriginalValue;

			if (isChangingFromActiveToInactive && isTemplateUsedInRegistry)
			{
				info.AddError(Res.GetString("DF50E12E-E74D-4313-BD22-63618EC5986B", "This Template is set as a default template in Registry setting 'Job Template Defaults' and therefore cannot be made Inactive."));
			}
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			base.ValidateAll();

			ValidateInstructions();
		}

		public void ValidateInstructions()
		{
			var minInstructions = 2;

			if (Template.Instructions.Count < minInstructions)
			{
				Parent.AddRowError(Res.GetString("0e801c1d-8676-413b-85d7-b388697c2bd9", "Transport Booking Template requires at least 2 instructions."));
			}
		}

		public DtbBookingTmpl Template => (DtbBookingTmpl)Parent;

		BusinessObjectFactory Factory => Parent.Factory;
	}
}
