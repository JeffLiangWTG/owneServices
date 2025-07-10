using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	/// <remarks>DO NOT MAKE ANY NEW CHAGNES TO THIS CLASS.  This class has been replaced by WhsDocket[s]LabelControl.cs.</remarks>
	public class WhsDocketLabelControl : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string NumberOfLabels = "NumberOfLabels";
			public const string NumberOfLabelsDescription = "NumberOfLabelsDescription";
			public const string NumberOfLabelsToPrint = "NumberOfLabelsToPrint";
		}

		#endregion

		#region Constructors

		public WhsDocketLabelControl(WhsDocket docket, ZInt numberOfLabels)
			: base(docket.Factory)
		{
			this.docket = docket;
			this.numberOfLabels = numberOfLabels;
			NumberOfLabelsToPrint = numberOfLabels;
		}

		#endregion

		#region Related Business Objects

		public WhsDocket Docket
		{
			get { return docket; }
		}

		#endregion

		#region Properties

		#region Labels

		public ZString NumberOfLabelsDescription
		{
			get { return Res.GetString("6ec87568-4664-4135-b759-ac38bb2041b0", "of {0}", NumberOfLabels.ToString()); }
		}

		public ZPropertyInfo NumberOfLabelsDescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NumberOfLabelsDescription); }
		}

		public ZInt NumberOfLabels
		{
			get { return this.numberOfLabels; }
		}

		public ZPropertyInfo NumberOfLabelsInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NumberOfLabels); }
		}

		public ZInt NumberOfLabelsToPrint
		{
			get { return numberOfLabelsToPrint; }
			set
			{
				numberOfLabelsToPrint = value;
				if (!IsValidationSuspended)
				{
					ValidateNumberOfLabelsToPrint();
				}
				NumberOfLabelsToPrintInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NumberOfLabelsToPrintInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.NumberOfLabelsToPrint); }
		}

		#endregion

		#region PackageLabel

		public ZInt NumberOfPackageLabels
		{
			get
			{
				ZInt result = 0;
				WhsOrder order = Docket as WhsOrder;
				if (order != null)
				{
					foreach (WhsOrderLine line in order.ParentLines)
					{
						result += (ZInt)Math.Ceiling(line.WE_PackQuantity);
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Validation

		public void ValidateNumberOfLabelsToPrint()
		{
			NumberOfLabelsToPrintInfo.ClearAllNotifications();
			if (NumberOfLabelsToPrint <= 0)
			{
				NumberOfLabelsToPrintInfo.AddError(Res.GetString("4e78a4a1-b068-459c-af7e-bf25faa8b24d", "Please enter the number of labels to print"));
			}

			if (NumberOfLabelsToPrint > NumberOfLabels)
			{
				NumberOfLabelsToPrintInfo.AddError(Res.GetString("127cd7ca-779b-42ad-870e-c426684e3526", "You cannot print more labels than available"));
			}
		}

		#endregion

		#region Implementation

		readonly WhsDocket docket;
		ZInt numberOfLabelsToPrint;
		readonly ZInt numberOfLabels;

		#endregion
	}
}
