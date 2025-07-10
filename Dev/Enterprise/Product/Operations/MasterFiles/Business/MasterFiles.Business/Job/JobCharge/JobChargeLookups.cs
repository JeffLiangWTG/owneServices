using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeLookups : AutoJobChargeLookups
	{
		public JobChargeLookups(AutoJobCharge parent)
			: base(parent)
		{
		}

		public override AccInvMsgCollection CostVATClasses
		{
			get { return new AccInvMsgCollection(Factory, Parent.Company?.GC_RN_NKCountryCode ?? ZString.Empty); }
		}

		public override AccInvMsgCollection SellVATClasses
		{
			get { return new AccInvMsgCollection(Factory, Parent.Company?.GC_RN_NKCountryCode ?? ZString.Empty); }
		}

		#region Creditors

		public CreditorCollection Creditors
		{
			get { return fCreditors ?? (fCreditors = new CreditorCollection(new BusinessObjectFactory())); }
		}
		CreditorCollection fCreditors;

		#endregion

		#region Invoice Types List

		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				if (Parent != null && !Parent.IsDeleted)
				{
					if (Parent.Job != null)
					{
						IJobInvoicingPlugIn plugin = Parent.Job.Parent as IJobInvoicingPlugIn;
						if (plugin != null)
						{
							if (plugin.InvoicingSupporter != null)
							{
								if (plugin.InvoicingSupporter.ConsumerType != null)
								{
									return plugin.InvoicingSupporter.ConsumerType.InvoiceTypeList;
								}
							}
						}
					}
				}

				return new InvoiceTypesList();
			}
		}

		#endregion

		#region ChargeCodes

		public override AccChargeCodeCollection ChargeCodes
		{
			get
			{
				return Factory.GetCachedValue("JobChargeLookups.ChargeCodes" + GlbCompany.CurrentCompany.PK.ToStringKey(), () => NewChargeCodeCollection);
			}
		}

		protected AccChargeCodeCollection NewChargeCodeCollection => new AccChargeCodeCollection(Factory, ChargeCodesFilter);

		public ZQuery ChargeCodesFilter
		{
			get
			{
				var result = new ZQuery(AccChargeCodeSchema.AC_IsActive, ZBool.True);
				var allowedChargeTypes = new List<string>();

				allowedChargeTypes.Add(Core.Constants.ChargeType.Comment);
				allowedChargeTypes.Add(Core.Constants.ChargeType.Margin);
				allowedChargeTypes.Add(Core.Constants.ChargeType.Disbursement);
				allowedChargeTypes.Add(Core.Constants.ChargeType.Revenue);
				allowedChargeTypes.Add(Core.Constants.ChargeType.ManualJobAccrual);

				result.AddToFilter(AccChargeCodeSchema.AC_ChargeType, allowedChargeTypes.ToArray());
				return result;
			}
		}

		#endregion

		#region Rating Behaviors

		public const string CreateNewCharge = "NEW";
		public const string ReAutorateCharge = "REA";
		public const string StopFromAutorating = "STP";
		public const string SpotCost = "SPT";

		public static ZString GetDescriptionForRatingBehaviourCode(ZString code)
		{
			var description = "";

			if (code == CreateNewCharge)
			{
				description = Res.GetString("b15f8134-384f-498e-b566-427583fb63b7", "Create new Charge during AutoRating");
			}
			else if (code == StopFromAutorating)
			{
				description = Res.GetString("665d9a52-b137-47f7-b809-139b88227982", "Stop this charge from AutoRating");
			}
			else if (code == ReAutorateCharge)
			{
				description = Res.GetString("7598708a-d264-4257-8c5d-a9a201fa281b", "Clear and Re-autorate this charge");
			}
			else if (code == SpotCost)
			{
				description = Res.GetString("e51ca740-95ac-40cf-9065-e40240df2da7", "Spot Cost Charge");
			}

			return description;
		}

		public CodeDescriptionPairList RatingBehaviors_Cost
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var parentCharge = Parent as JobCharge;
				var costIsPostedOrApportioned = parentCharge.IsCostPosted || parentCharge.JR_IsApportioned;

				result.AddPair(CreateNewCharge, GetDescriptionForRatingBehaviourCode(CreateNewCharge));

				if (parentCharge.JR_IsSpotCost)
				{
					result.AddPair(SpotCost, GetDescriptionForRatingBehaviourCode(SpotCost));
				}

				if (costIsPostedOrApportioned)
				{
					result.AddPair(StopFromAutorating, GetDescriptionForRatingBehaviourCode(StopFromAutorating));
				}
				else
				{
					result.AddPair(ReAutorateCharge, GetDescriptionForRatingBehaviourCode(ReAutorateCharge));
				}

				return result;
			}
		}

		public CodeDescriptionPairList RatingBehaviors_Sell
		{
			get
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(CreateNewCharge, GetDescriptionForRatingBehaviourCode(CreateNewCharge));
				if ((Parent as JobCharge).IsRevenuePosted)
				{
					result.AddPair(StopFromAutorating, GetDescriptionForRatingBehaviourCode(StopFromAutorating));
				}
				else
				{
					result.AddPair(ReAutorateCharge, GetDescriptionForRatingBehaviourCode(ReAutorateCharge));
				}

				return result;
			}
		}

		#endregion

		#region PlacesOfSupply

		public ReadOnlyCodeDescriptionPairList PlacesOfSupply => PlaceOfSupplyListProvider.GetPlaceOfSupplyList(Parent.Company ?? GlbCompany.CurrentCompany);

		public ReadOnlyCodeDescriptionPairList PlaceOfSupplyTypes => PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(Parent.Company ?? GlbCompany.CurrentCompany);

		#endregion

		#region Implementation

		new AutoJobCharge Parent
		{
			get { return (AutoJobCharge)base.Parent; }
		}

		#endregion
	}
}
