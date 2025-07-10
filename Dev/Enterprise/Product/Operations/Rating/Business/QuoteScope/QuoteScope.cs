using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class QuoteScope : AutoQuoteScope
	{
		public QuoteScope(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("Parent")]
		public override ZGuid QS_TH_RatingHeader
		{
			get
			{
				return base.QS_TH_RatingHeader;
			}

			set
			{
				base.QS_TH_RatingHeader = value;
			}
		}

		public Quote Parent { get; set; }

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (QS_ProductCode.IsEmpty)
			{
				QS_ProductCode = "FWD";
			}

			if (QS_Origin.IsEmpty)
			{
				QS_Origin = "US";
			}

			if (QS_Destination.IsEmpty)
			{
				QS_Destination = "US";
			}

			if (QS_TransportMode.IsEmpty)
			{
				QS_TransportMode = "ALL";
			}

			if (QS_ContainerMode.IsEmpty)
			{
				QS_ContainerMode = "ALL";
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
	}
}
