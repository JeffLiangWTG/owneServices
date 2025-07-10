using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class PackUnpackShipment : CFSShipment
	{
		#region Schema

		public new class Schema : CFSShipment.Schema
		{
			public const string JS_Calc_InStock = "JS_Calc_InStock";
			[Obsolete("Obsolete", true)]
			public const string JS_Calc_TotalDelivered = "JS_Calc_TotalDelivered";
			public const string LinkedToCustoms = "LinkedToCustoms";
			public const string UnpackDate = "UnpackDate";
		}

		#endregion

		public PackUnpackShipment(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ValidateTotalsAgainstPackLines = true;
		}

		#region Validation

		protected override JobShipmentValidation GetNewValidation()
		{
			return new PackUnpackShipmentValidation(this);
		}

		public new PackUnpackShipmentValidation Validation
		{
			get { return (PackUnpackShipmentValidation)base.Validation; }
		}

		#endregion

		#region overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JS_TranshipToOtherCFS = true; // gate pass required
		}

		public override ZGuid GetContainerPKForNewChild()
		{
			if (ParentContainerRegistration == null)
			{
				return ZGuid.Empty;
			}
			else
			{
				return ParentContainerRegistration.PK;
			}
		}

		public TallyContainer CurrentContainer
		{
			get { return (TallyContainer)ParentContainerRegistration; }
		}

		public override void OnLoaded()
		{
			AllowSurplusPacks = false;
			base.OnLoaded();
		}

		#endregion

		#region Related Business Objects

		#region Customs

		protected internal IOutturn CustomsOutturnListener
		{
			get
			{
				if (customsOutturnListener != null && customsOutturnListener.IsDeleted)
				{
					customsOutturnListener = null;
				}

				if (customsOutturnListener == null && CurrentContainer != null)
				{
					customsOutturnListener = ((IOutturnProvider)CurrentContainer).GetOutturnFor(this);
				}

				return customsOutturnListener;
			}
		}
		protected internal IOutturn customsOutturnListener;

		public ZBool LinkedToCustoms
		{
			get { return CustomsOutturnListener != null; }
		}

		public ZPropertyInfo LinkedToCustomsInfo
		{
			get { return GetZPropertyInfo(Schema.LinkedToCustoms); }
		}

		internal void NotifyCustomsListener()
		{
			if (CustomsOutturnListener != null)
			{
				ZInt totalOutturned = 0;
				ZBool pillaged = false;
				ZBool damaged = false;
				ZString packageType = ZString.Empty;

				foreach (TallyPackLine line in OuterPackLines)
				{
					totalOutturned += line.JL_Outturn;

					if (!line.JL_Pillaged.IsEmpty)
					{
						pillaged = true;
					}

					if (!line.JL_Damaged.IsEmpty)
					{
						damaged = true;
					}

					packageType = line.JL_F3_NKPackType;
				}

				CustomsOutturnListener.SetPackagesOutturned(totalOutturned);
				CustomsOutturnListener.SetPillaged(pillaged);
				CustomsOutturnListener.SetDamaged(damaged);
				CustomsOutturnListener.SetPackageType(packageType);
			}
		}

		[BusinessObjectTestExclude()]
		public ZDateTime UnpackDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (CustomsOutturnListener != null)
				{
					result = CustomsOutturnListener.UnpackDate;
				}

				return result;
			}
			set
			{
				if (value.IsValid)
				{
					if (CustomsOutturnListener != null)
					{
						CustomsOutturnListener.UnpackDate = value;
					}
				}
				UnpackDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnpackDateInfo
		{
			get { return GetZPropertyInfo(Schema.UnpackDate); }
		}

		protected bool UnpackDate_ReadOnly
		{
			get { return CustomsOutturnListener == null; }
		}

		#endregion

		#region CoLoads

		protected override CoLoadShipmentCollection GetNewCoLoadShipmentCollection()
		{
			CoLoadShipmentCollection result = new CoLoadShipmentCollection(this, Factory);
			result.CountChanged += new CollectionCountChangedEventHandler(CoLoadShipments_CountChanged);
			return result;
		}

		#endregion

		#region Consol
		protected override ConsolCollection GetNewConsolCollection()
		{
			return new PackUnpackLoadListConsolManyToManyCollection(this);
		}

		#endregion

		#region PackLines

		public new TallyPackLineCollection OuterPackLines
		{
			get { return (TallyPackLineCollection)base.OuterPackLines; }
		}

		protected override OuterPackLineCollection GetNewOuterPackLineCollectionCore()
		{
			return new TallyPackLineCollection(this, Factory);
		}

		protected override InnerPackLineCollection GetNewInnerPackLinesCollection()
		{
			return new TallyInnerPackLineCollection(this);
		}

		#endregion

		#region JobDocsAndCartage

		public override Type DocsAndCartageType
		{
			get { return typeof(PackUnpackDocsAndCartage); }
		}

		public new PackUnpackDocsAndCartage DocsAndCartage
		{
			get { return (PackUnpackDocsAndCartage)base.DocsAndCartage; }
		}

		//		protected override JobDocsAndCartage GetNewJobDocsAndCartage()
		//		{
		//			return Factory.New<PackUnpackDocsAndCartage>();
		//		}
		//
		//		protected override JobDocsAndCartage LoadJobDocsAndCartage()
		//		{
		//			return Factory.Load<PackUnpackDocsAndCartage>(JS_JP);
		//		}
		//
		#endregion

		#endregion

		#region Calculated Properties

		#region JS_Calc_TotalDelivered

		[Obsolete("Obsolete", true)]
		public ZInt JS_Calc_TotalDelivered
		{
			get { return OuterPackLines.TotalDelivered; }
		}

		[Obsolete("Obsolete", true)]
		public ZPropertyInfo JS_Calc_TotalDeliveredInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_TotalDelivered); }
		}

		#endregion

		#region JS_Calc_InStock

		public ZInt JS_Calc_InStock
		{
			get { return OuterPackLines.TotalInStock; }
		}

		public ZPropertyInfo JS_Calc_InStockInfo
		{
			get { return GetZPropertyInfo(Schema.JS_Calc_InStock); }
		}

		#endregion

		#endregion

		#region Property Overrides

		public override ZInt JS_OuterPacks
		{
			get { return base.JS_OuterPacks; }
			set
			{
				ZInt result = value;
				if (result.IsEmpty && CustomsOutturnListener != null)
				{
					result = CustomsOutturnListener.NumberOfPackages;
				}

				base.JS_OuterPacks = result;
			}
		}

		#endregion

	}
}
