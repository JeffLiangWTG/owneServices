using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.US.Business
{
	[SystemDefinedValues]
	[CodeProperty(Schema.US_ITNumber), DescriptionProperty(Schema.US_ITNumber)]
	public class ITAndSplitDetails : AutoITAndSplitDetails, IITNumber, IConveyanceOrSplitDetails, IBillDetails, IFTZITAndSplitDetail
	{
		public ITAndSplitDetails(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Bill Bill
		{
			get { return Factory.Load<Bill>(B7_ParentID); }
		}

		internal JobDeclaration Declaration
		{
			get
			{
				Bill bill = Bill;
				return bill == null ? null : bill.Declaration;
			}
		}

		#region overrides

		public override ZString US_ITNumber
		{
			get { return base.US_ITNumber; }
			set
			{
				ZString oldValue = US_ITNumber;
				base.US_ITNumber = value;
				if (!IsCopying && oldValue != US_ITNumber)
				{
					Bill bill = Bill;
					if (bill != null)
					{
						if (bill.ITAndSplitDetails.Count == 1 && bill.CU_NoOfPacks > 0)
						{
							US_NoOfPacks = ZInt.ParseSafe(bill.CU_NoOfPacks.ToString(0), 0);
						}
						if (!IsValidationSuspended)
						{
							bill.Validation.ValidateITNumber();
						}
						bill.ITNumberInfo.RefreshBinding();

						JobDeclaration dec = Declaration;
						if (dec != null)
						{
							dec.Bills.MarkAsNeedingValidation();
							dec.JE_PrimaryITNumberInfo.RefreshBinding();
						}
					}
				}
			}
		}

		public bool US_FlightNumber_ReadOnly
		{
			get { return SplitDetailsReadonly; }
		}

		public bool US_ArrivalDate_ReadOnly
		{
			get { return SplitDetailsReadonly; }
		}

		public bool US_CarrierCode_ReadOnly
		{
			get { return SplitDetailsReadonly; }
		}

		bool SplitDetailsReadonly
		{
			get
			{
				var bill = Bill;
				return bill == null || !bill.US_SESplitShip;
			}
		}

		public override void OnSaving()
		{
			if (!IsDeleted && US_ITNumber.IsEmpty && US_NoOfPacks.IsEmpty && US_ArrivalDate.IsEmpty && US_CarrierCode.IsEmpty && US_FlightNumber.IsEmpty && US_ITDate.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			Bill bill = Bill;
			base.Delete();
			if (bill != null)
			{
				bill.ITNumberInfo.RefreshBinding();
				JobDeclaration declaration = bill.Declaration;
				if (declaration != null)
				{
					declaration.JE_PrimaryITNumberInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region IBillDetails Members

		public ZString ITNumber
		{
			get { return US_ITNumber; }
		}

		ZDate IBillDetails.ITDate
		{
			get
			{
				JobDeclaration dec = Declaration;
				return dec != null ? dec.US_ITDate.Date : ZDate.Empty;
			}
		}

		ZString IBillDetails.MasterBillNumber
		{
			get
			{
				var bill = Bill;
				return bill != null ? bill.CU_MasterBillTruncated : ZString.Empty;
			}
		}

		ZString IBillDetails.IssuerCodeOfMasterBillNumber
		{
			get
			{
				var bill = Bill;
				var declaration = Declaration;
				return declaration != null &&
					(declaration.IsSea
					|| declaration.IsRail
					|| declaration.IsTruck) ? bill != null ? bill.EffectiveMasterBillIssuerSCAC : ZString.Empty : ZString.Empty;
			}
		}

		ZString IBillDetails.HouseBillNumber
		{
			get
			{
				var bill = Bill;
				return bill != null ? bill.CU_HouseBillTruncated : ZString.Empty;
			}
		}

		ZString IBillDetails.IssuerCodeOfHouseBillNumber
		{
			get
			{
				var bill = Bill;
				var declaration = Declaration;
				return declaration != null &&
					(declaration.IsSea
					|| declaration.IsRail
					|| declaration.IsTruck) ? bill != null ? bill.EffectiveHouseBillIssuerSCAC : ZString.Empty : ZString.Empty;
			}
		}

		ZString IBillDetails.SubHouseBillNumber
		{
			get
			{
				Bill bill = Bill;
				return bill != null ? bill.CU_SubHouseBillTruncated : ZString.Empty;
			}
		}

		ZString IBillDetails.IssuerCodeOfSubHouseBillNumber
		{
			get
			{
				var bill = Bill;
				var declaration = Declaration;
				return declaration != null &&
					(declaration.IsSea
					|| declaration.IsRail) ? bill != null ? bill.EffectiveSubHouseBillIssuerSCAC : ZString.Empty : ZString.Empty;
			}
		}

		ZInt IBillDetails.PackageQuantity
		{
			get { return US_NoOfPacks; }
		}

		ZString IBillDetails.PackageType
		{
			get
			{
				var bill = Bill;
				return bill != null ? bill.CU_PackType : ZString.Empty;
			}
		}

		IEnumerable<IConveyanceOrSplitDetails> IBillDetails.ConveyanceOrSplitDetails
		{
			get { return new TypedEnumerable<IConveyanceOrSplitDetails>(this); }
		}

		IEnumerable<IContainer> IBillDetails.Containers
		{
			get
			{
				var result = System.Array.Empty<IContainer>();

				var declaration = Declaration;
				var isSea = declaration != null && declaration.IsSea;
				if (isSea)
				{
					var bill = Bill;
					if (bill != null)
					{
						return new TypedEnumerable<IContainer>(bill.Containers);
					}
				}

				return result;
			}
		}

		ZBool IBillDetails.IsSplit
		{
			get
			{
				var bill = Bill;
				return bill != null && bill.US_SESplitShip;
			}
		}

		ZBool IBillDetails.IsNonAMS
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.US_NonAMS;
			}
		}

		ZBool IBillDetails.IsExpressTracking
		{
			get { return Bill?.US_ExpressTracking ?? false; }
		}

		#endregion

		#region ISplitDetails Members

		ZDateTime IConveyanceOrSplitDetails.ArrivalDate
		{
			get { return US_ArrivalDate; }
		}

		ZString IConveyanceOrSplitDetails.FlightNumber
		{
			get { return US_FlightNumber; }
		}

		ZInt IConveyanceOrSplitDetails.Qty
		{
			get
			{
				var bill = Bill;
				return bill != null && bill.US_SESplitShip ? US_NoOfPacks : ZInt.Zero;
			}
		}

		ZString IConveyanceOrSplitDetails.UQ
		{
			get
			{
				var bill = Bill;
				return bill != null ? bill.CU_PackType : ZString.Empty;
			}
		}

		ZString IConveyanceOrSplitDetails.CarrierCode
		{
			get { return US_CarrierCode; }
		}

		ZString IConveyanceOrSplitDetails.PipelineName
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsACECargoCertificationAndFixedTransportRelevant ? declaration.US_PipelineName : ZString.Empty;
			}
		}

		#endregion

		ZInt IITNumber.PackageQuantity
		{
			get { return US_NoOfPacks; }
		}

		ZDecimal IFTZConcurrence.FTZConcurrenceQty
		{
			get => US_FTZConcurrenceQty;
			set => US_FTZConcurrenceQty = value;
		}
	}
}
