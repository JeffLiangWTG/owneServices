using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Bookings
{
	public class SplitOrderConfirmation : ZPageConfirmation
	{
		#region Constructors

		public SplitOrderConfirmation(ZPage page)
			: base(page)
		{
		}

		#endregion

		#region New

		public new OrderLinesToPackLinesMappingPage Page
		{
			get
			{
				return base.Page as OrderLinesToPackLinesMappingPage;
			}
		}

		#endregion

		#region Overrides

		protected new OrderLineToPackLineConversionHelper DataSource
		{
			get
			{
				return base.DataSource as OrderLineToPackLineConversionHelper;
			}
		}

		protected override ConfirmationTypes ConfirmationType
		{
			get
			{
				return ConfirmationTypes.Warning;
			}
		}

		protected override bool GetRequired()
		{
			if (DataSource != null)
			{
				if (DataSource.ShouldShowSplitOrders)
				{
					return true;
				}
			}
			return false;
		}

		protected override string GetTitle()
		{
			return Res.GetString("442c3658-8467-4b88-b79a-b1ca067150d3", "Incomplete Order");
		}

		protected override string GetConfirmationMessage()
		{
			return Res.GetString("32251711-2cd6-4196-bb06-ec3030f0dab3", "This order has some incomplete order lines. Would you like to:") + " ";
		}

		protected override string GetUserResponseHolderID()
		{
			return "SplitOrdersConfirmation";
		}

		protected static string SplitButtonCaption { get { return Res.GetString("5664cc07-5c5b-40dd-ac2a-74078329af22", "Split"); } }
		protected static string DoNothingButtonCaption { get { return Res.GetString("c8a7406a-f904-42ad-b725-cc85e8926955", "Do Nothing"); } }

		protected override List<ZPageConfirmationButton> GetButtons()
		{
			List<ZPageConfirmationButton> result = new List<ZPageConfirmationButton>();
			result.Add(new ZPageConfirmationButton(SplitButtonCaption, (NoResString)"$(\\'" + Page.SaveButtonClientID + (NoResString)"\\').click();")); // javascript code should not be translated
			result.Add(new ZPageConfirmationButton(DoNothingButtonCaption, (NoResString)"$(\\'" + Page.SaveButtonClientID + (NoResString)"\\').click();")); // javascript code should not be translated
			return result;
		}

		public bool IsSplitButtonClicked
		{
			get
			{
				return ResponseHolder.Value == SplitButtonCaption;
			}
		}

		public bool IsDoNothingButtonClicked
		{
			get
			{
				return ResponseHolder.Value == DoNothingButtonCaption;
			}
		}

		#endregion
	}
}
