//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSHFCDetailAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSHFCDetailAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.Customs.US.Business
{
	public class USHFCDetailAddInfoValidation : AutoUSHFCDetailAddInfoValidation
	{
		public USHFCDetailAddInfoValidation(AutoUSHFCDetailAddInfo parent) : base(parent)
		{
		}

		new USHFCDetailAddInfo Parent
		{
			get { return (USHFCDetailAddInfo)base.Parent; }
		}

		protected USHFCDetail Detail
		{
			get { return (USHFCDetail)Parent.Parent; }
		}

		USHFCHeader Header
		{
			get { return Detail?.Header; }
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var hfcHeader = Header;
				if (hfcHeader != null)
				{
					var invoiceLine = hfcHeader.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		protected override void CheckUS_LPCONumber()
		{
			base.CheckUS_LPCONumber();

			if (ShouldCheckAtLeastDetailMeetsReq && IsPGAValidation)
			{
				Parent.US_LPCONumberInfo.AddMessageError(AtLeastDetailMeetsReq);
			}
		}

		bool ShouldCheckAtLeastDetailMeetsReq =>
			Header is USHFCHeader header && header.US_ASHRAENumber.IsEmpty && !header.USHFCDetails.OfType<USHFCDetail>().Any(x => !x.US_LPCONumber.IsEmpty && !x.US_ActiveIngredientPercentage.IsEmpty);

		internal const string AtLeastDetailMeetsReq = "When ASHRAE Number is empty, the Product Code Number and % of at least one row of details cannot be empty.";

		protected override void CheckUS_ActiveIngredientPercentage()
		{
			base.CheckUS_ActiveIngredientPercentage();

			var parent = Parent;
			if (Header is USHFCHeader header && IsPGAValidation)
			{
				if (header.USHFCDetails.OfType<USHFCDetail>().Sum(x => x.US_ActiveIngredientPercentage) > 100)
				{
					parent.US_ActiveIngredientPercentageInfo.AddMessageError(MustNotExceed100);
				}
				if (ShouldCheckAtLeastDetailMeetsReq)
				{
					parent.US_ActiveIngredientPercentageInfo.AddMessageError(AtLeastDetailMeetsReq);
				}
			}
		}
		internal const string MustNotExceed100 = "Constituent Element % must not exceed 100%";
	}
}

