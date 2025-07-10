//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobComInvoiceHeaderRefsValidation
//
//    This class should be used for overriding validation in AutoJobComInvoiceHeaderRefsValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceHeaderRefsValidation : AutoJobComInvoiceHeaderRefsValidation
	{
		public JobComInvoiceHeaderRefsValidation(AutoJobComInvoiceHeaderRefs parent) : base(parent)
		{
		}

		protected new JobComInvoiceHeaderRefs Parent => (JobComInvoiceHeaderRefs)base.Parent;

		protected override void CheckJ2_ReferenceType()
		{
			base.CheckJ2_ReferenceType();
			ListValidation.WarnIfInvalidCode(Parent.J2_ReferenceTypeInfo, Parent.Lookups.ReferenceTypeList);

			if (Parent.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.RP
				&& Parent.InvoiceHeader.InvoiceHeaderRefs.Count(x => x.J2_ReferenceType == InvoiceHeaderRefsTypeList.Codes.RP) > 1)
			{
				Parent.J2_ReferenceTypeInfo.AddError(OnlyOneRPAllowedMessage);
			}
		}

		internal static string OnlyOneRPAllowedMessage => Res.GetString("05399fa2-920c-4700-8174-d1ba13b21ce0", "Only one {0} is allowed per commercial invoice.", InvoiceHeaderRefsTypeList.Descriptions.RP);

		protected override void CheckJ2_ReferenceNumber()
		{
			base.CheckJ2_ReferenceNumber();
			if (Parent.J2_ReferenceNumber.IsEmpty)
			{
				Parent.J2_ReferenceNumberInfo.AddWarning(NumberIsEmpty);
			}
		}
		internal static string NumberIsEmpty
		{
			get { return Res.GetString("21be2c7a-7d1a-450e-bf05-48550e4a2d1b", "Reference Number should not be empty."); }
		}
	}
}
