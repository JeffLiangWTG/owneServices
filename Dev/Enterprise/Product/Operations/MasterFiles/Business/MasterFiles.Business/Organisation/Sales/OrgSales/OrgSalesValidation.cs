//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSalesValidation
//
//    This class should be used for overriding validation in AutoOrgSalesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using Enterprise.MarketingManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesValidation : AutoOrgSalesValidation
	{
		public OrgSalesValidation(AutoOrgSales parent)
			: base(parent)
		{
		}

		new OrgSales Parent
		{
			get { return (OrgSales)base.Parent; }
		}

		public override void ValidateAll()
		{
			if (!Parent.ReadOnly) // Header.Clients is a readonly collection and should not be validated
			{
				base.ValidateAll();
			}
		}

		#region OW_OriginID

		protected override void CheckOW_OriginID()
		{
			base.CheckOW_OriginID();

			var product = Parent.Product;
			if (product != null && product.MP_IsSystemDefined && Parent.OW_OriginID.IsEmpty)
			{
				if (product.LocationArrangement == OrgSalesProductLocationArrangement.SingleLocation)
				{
					if (product.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
					{
						if (Parent.OW_WW.IsEmpty)
						{
							Parent.OW_OriginIDInfo.AddError(GetWarehouseOrLocationMustBeEnteredMessage());
						}
					}
					else
					{
						MandatoryValidation.CheckEntered(Parent.OW_OriginIDInfo);
					}
				}
				else if (Parent.OW_DestinationID.IsEmpty)
				{
					Parent.OW_OriginIDInfo.AddError(GetOriginOrDestinationMustBeEnteredMessage());
				}
			}

			if (Parent.Origin != null && Parent.Origin.VLO_TableCode == RefCountrySchema.Constants.Prefix)
			{
				Parent.OW_OriginIDInfo.AddWarning(OriginDestinationCountryWarningMessage);
			}

			ListValidation.ErrorIfInvalidPK(Parent.OW_OriginIDInfo);
		}

		protected override void CheckOW_OriginTableCode()
		{
			base.CheckOW_OriginTableCode();
			if (!Parent.OW_OriginID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.OW_OriginTableCodeInfo);
			}
		}

		#endregion

		#region OW_DestinationID

		protected override void CheckOW_DestinationID()
		{
			base.CheckOW_DestinationID();

			var product = Parent.Product;
			if (product != null && product.MP_IsSystemDefined)
			{
				if (product.LocationArrangement == OrgSalesProductLocationArrangement.SingleLocation)
				{
					// Do not check entered because 'location' is stored in OW_OriginID
				}
				else if (Parent.OW_OriginID.IsEmpty && Parent.OW_DestinationID.IsEmpty)
				{
					Parent.OW_DestinationIDInfo.AddError(GetOriginOrDestinationMustBeEnteredMessage());
				}
			}

			if (Parent.Destination != null && Parent.Destination.VLO_TableCode == RefCountrySchema.Constants.Prefix)
			{
				Parent.OW_DestinationIDInfo.AddWarning(OriginDestinationCountryWarningMessage);
			}

			ListValidation.ErrorIfInvalidPK(Parent.OW_DestinationIDInfo);
		}

		protected override void CheckOW_DestinationTableCode()
		{
			base.CheckOW_DestinationTableCode();
			if (!Parent.OW_DestinationID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.OW_DestinationTableCodeInfo);
			}
		}

		#endregion

		#region OW_WW

		protected override void CheckOW_WW()
		{
			base.CheckOW_WW();

			var product = Parent.Product;
			if (product != null && product.MP_IsSystemDefined && Parent.OW_WW.IsEmpty)
			{
				if (product.MP_Code == SystemDefinedSalesProductList.Codes.Warehouse && Parent.OW_OriginID.IsEmpty)
				{
					Parent.OW_WWInfo.AddError(GetWarehouseOrLocationMustBeEnteredMessage());
				}
			}
		}

		#endregion

		#region OW_Service

		protected override void CheckOW_Service()
		{
			base.CheckOW_Service();

			var product = Parent.Product;
			if (product != null && product.ServiceIsMandatory)
			{
				MandatoryValidation.CheckEntered(Parent.OW_ServiceInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.OW_ServiceInfo);
		}

		#endregion

		#region Messages

		string GetOriginOrDestinationMustBeEnteredMessage()
		{
			var originName = Parent.OW_OriginIDInfo.HumanReadableName;
			var destinationName = Parent.OW_DestinationIDInfo.HumanReadableName;
			return GetAtLeastOneMustBeEnteredMessage(originName, destinationName);
		}

		string GetWarehouseOrLocationMustBeEnteredMessage()
		{
			return GetAtLeastOneMustBeEnteredMessage(Parent.OW_WWInfo.HumanReadableName, Res.GetString("19d9c071-3aa6-4ed6-ba65-7b5f18973572", "Location"));
		}

		static string GetAtLeastOneMustBeEnteredMessage(string a, string b)
		{
			return Res.GetString("ec6f94d6-95fa-4a0c-90c9-4e1154b620a4", "Please enter {0}{1} or {2}{3}.",
				Grammar.Instance.IndefiniteArticlePrefix(a),
				a,
				Grammar.Instance.IndefiniteArticlePrefix(b),
				b);
		}

		static string OriginDestinationCountryWarningMessage => Res.GetString("0d9e3135-0760-408d-ade6-d0378807e03a", "When Estimates are committed, forecasting results are best achieved when Origin / Destination port pairs are defined. Consider setting port instead of country/region.");

		#endregion
	}
}
