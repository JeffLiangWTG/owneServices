using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketLabelLine : NonPersistentBusinessObject
	{
		#region Constructors

		internal WhsDocketLabelLine()
			: base()
		{
		}

		public WhsDocketLabelLine(WhsDocket docket, ZInt defaultNumberOfLabelsToPrint, ZInt totalNumberOfLabelsToPrint)
			: base()
		{
			Argument.NotNull(docket, "docket");
			Argument.GreaterThanOrEqualToZero(defaultNumberOfLabelsToPrint, "defaultNumberOfLabelsToPrint");
			Argument.GreaterThanOrEqualToZero(totalNumberOfLabels, "totalNumberOfLabelsToPrint");

			this.docket = docket;
			this.numberOfLabelsToPrint = Math.Min(defaultNumberOfLabelsToPrint, totalNumberOfLabelsToPrint);
			this.totalNumberOfLabels = totalNumberOfLabelsToPrint;
		}

		#endregion

		#region Schema

		public static class Schema
		{
			public const string OrderNumber = "OrderNumber";
			public const string NumberOfLabelsToPrint = "NumberOfLabelsToPrint";
			public const string TotalNumberOfLabels = "TotalNumberOfLabels";
		}

		#endregion

		#region GUI Properties

		#region Order Number

		public WhsDocket Docket
		{
			get { return docket; }
		}

		public ZString OrderNumber
		{
			get { return docket != null ? docket.WD_ExternalReference : ZString.Empty; }
		}

		public ZPropertyInfo OrderNumberInfo
		{
			get { return GetZPropertyInfo(Schema.OrderNumber); }
		}

		#endregion

		#region Number of Labels To Print

		public ZInt NumberOfLabelsToPrint
		{
			get { return numberOfLabelsToPrint; }
			set
			{
				SetNonPersistentPropertyValue(NumberOfLabelsToPrintInfo, ref numberOfLabelsToPrint, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNumberOfLabelsToPrint();
				}
				NumberOfLabelsToPrintInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NumberOfLabelsToPrintInfo
		{
			get { return GetZPropertyInfo(Schema.NumberOfLabelsToPrint); }
		}

		#endregion

		#region Total Number of Labels

		public ZInt TotalNumberOfLabels
		{
			get { return totalNumberOfLabels; }
		}

		public ZPropertyInfo TotalNumberOfLabelsInfo
		{
			get { return GetZPropertyInfo(Schema.TotalNumberOfLabels); }
		}

		#endregion

		#endregion

		#region Validation

		public WhsDocketLabelLineValidation Validation
		{
			get { return new WhsDocketLabelLineValidation(this); }
		}

		#endregion

		#region Interfacing Properties

		public WhsDocketLabelControl LegacyDocketLabelControl
		{
			get { return new WhsDocketLabelControl(Docket, NumberOfLabelsToPrint); }
		}

		#endregion

		#region Implementation

		readonly WhsDocket docket;
		ZInt numberOfLabelsToPrint;
		readonly ZInt totalNumberOfLabels;

		#endregion
	}
}
