using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// IComparer implementation for RateAttachmentSet objects.
	/// </summary>
	public class RateAttachmentComparer : IComparer<IRateAttachment>, IComparer<RateAttachmentSet>, System.Collections.IComparer
	{
		#region IComparer<IRateAttachment> Members

		public int Compare(IRateAttachment lhs, IRateAttachment rhs)
		{
			var result = GetTemplateTypeOrderingIndex(lhs.TemplateType) - GetTemplateTypeOrderingIndex(rhs.TemplateType);
			if (result == 0)
			{
				result = lhs.Sequence - rhs.Sequence;
			}

			return result;
		}

		#endregion

		#region IComparer<RateAttachmentSet> Members

		public int Compare(RateAttachmentSet x, RateAttachmentSet y)
		{
			return Compare(x, (IRateAttachment)y);
		}

		#endregion

		#region IComparer Members

		int System.Collections.IComparer.Compare(object x, object y)
		{
			return Compare((IRateAttachment)x, (IRateAttachment)y);
		}

		#endregion

		#region Implementation

		int GetTemplateTypeOrderingIndex(ZString templateType)
		{
			var result = 0;
			if (templateType == RatingConstants.DocTemplateTypes.CoverPage)
			{
				return 1;
			}

			if (RatingConstants.DocTemplateTypes.IsPricingPage(templateType))
			{
				return 2;
			}

			if (templateType == RatingConstants.DocTemplateTypes.TrailingPage)
			{
				return 3;
			}

			return result;
		}

		#endregion
	}
}

