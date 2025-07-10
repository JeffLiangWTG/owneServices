using System;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderLineToPackLineConversionEventArgs : EventArgs
	{
		public OrderLineToPackLineConversionEventArgs(OrderLineToPackLineConversionHelper helper, bool shouldCreatePacklinesByDefault)
		{
			Helper = helper;
			ShouldCreatePacklines = shouldCreatePacklinesByDefault;
		}

		public bool ShouldCreatePacklines { get; set; }

		public string ReasonForNotAbleToConvert
		{
			get
			{
				return Helper.ReasonForNotAbleToConvert;
			}
		}

		public readonly OrderLineToPackLineConversionHelper Helper;
	}
}
