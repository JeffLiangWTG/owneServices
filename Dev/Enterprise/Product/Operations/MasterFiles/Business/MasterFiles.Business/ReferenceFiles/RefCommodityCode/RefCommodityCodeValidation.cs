using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityCodeValidation : AutoRefCommodityCodeValidation
	{
		public RefCommodityCodeValidation(AutoRefCommodityCode parent)
			: base(parent)
		{
		}

		new RefCommodityCode Parent
		{
			get { return (RefCommodityCode)base.Parent; }
		}

		protected override void CheckRH_Code()
		{
			base.ValidateRH_Code();
			MandatoryValidation.CheckEntered(Parent.RH_CodeInfo);
			var code = Parent.RH_Code;
			if (code.Length < 2)
			{
				Parent.RH_CodeInfo.AddError(Res.GetString("4d065a2e-ba5f-48f9-8179-ec3c0026b4e4", "Commodity code must be at least 2 characters long."));
			}
			else if (!Parent.IsInDatabase || Parent.RH_CodeInfo.HasChanges)
			{
				var query = new ZQuery(RefCommodityCodeSchema.RH_Code, code);
				query.AddToFilter(RefCommodityCodeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				if (Parent.Factory.LoadTop1<RefCommodityCode>(query) != null)
				{
					Parent.RH_CodeInfo.AddError(Res.GetString("A48A1601-4A69-42C6-BAB6-F62560E4D6AC", "Another Commodity with this code {0} already exists.", code));
				}
			}
		}

		protected override void CheckRH_Description()
		{
			base.ValidateRH_Description();
			MandatoryValidation.CheckEntered(Parent.RH_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.RH_DescriptionInfo);
		}

		protected override void CheckRH_ReeferMinTemperature()
		{
			base.ValidateRH_ReeferMinTemperature();
			if (Parent.RH_ReeferMinTemperature > Parent.RH_ReeferMaxTemperature)
			{
				Parent.RH_ReeferMinTemperatureInfo.AddError(Res.GetString("e0144939-2a9c-42ee-82e9-e15b392903ba", "Minimum Temperature Must Be Less Than Or Equal To The Maximum Temperature"));
			}
		}

		protected override void CheckRH_ReeferMaxTemperature()
		{
			base.ValidateRH_ReeferMaxTemperature();
			if (Parent.RH_ReeferMaxTemperature < Parent.RH_ReeferMinTemperature)
			{
				Parent.RH_ReeferMaxTemperatureInfo.AddError(Res.GetString("901e8feb-27e7-43ba-a885-1f03809d1786", "Maximum Temperature Must Be Greater Than Or Equal To The Minimum Temperature"));
			}
		}

		protected override void CheckRH_IATACommodityItem()
		{
			base.CheckRH_IATACommodityItem();
			if (!Parent.RH_IATACommodityItem.IsEmpty)
			{
				if (!Regex.IsMatch(Parent.RH_IATACommodityItem, @"^\d{4,7}$"))
				{
					Parent.RH_IATACommodityItemInfo.AddWarning(Res.GetString("47fd1e15-2327-4efa-ab10-87123d26cb85", "Enter an IATA Commodity between 4 and 7 digits long."));
				}
				else
				{
					var query = new ZQuery(RefAirlineCommodityCodeSchema.RAC_Code, Parent.RH_IATACommodityItem);
					query.AddToFilter(RefAirlineCommodityCodeSchema.RAC_AirlineID, SQLComparisonOperator.Equal, ZString.Empty);
					if (Parent.Factory.LoadTop1<RefAirlineCommodityCode>(query) == null)
					{
						Parent.RH_IATACommodityItemInfo.AddWarning(Res.GetString("9BA666DB-2E03-478E-9830-033DA9AC70F3", "This code does not match IATA Specific Commodity."));
					}
				}
			}
		}

		protected override void CheckRH_IsForwarding()
		{
			base.CheckRH_IsForwarding();
			CheckCommodityTypeIsSet(Parent.RH_IsForwardingInfo);
		}

		protected override void CheckRH_IsShipping()
		{
			base.CheckRH_IsShipping();
			CheckCommodityTypeIsSet(Parent.RH_IsShippingInfo);
		}

		protected override void CheckRH_IsLandTransport()
		{
			base.CheckRH_IsLandTransport();
			CheckCommodityTypeIsSet(Parent.RH_IsLandTransportInfo);
		}

		protected override void CheckRH_IsPersonalEffects()
		{
			base.CheckRH_IsPersonalEffects();
			CheckCommodityTypeIsSet(Parent.RH_IsPersonalEffectsInfo);
		}

		void CheckCommodityTypeIsSet(ZPropertyInfo propertyInfo)
		{
			if (!(Parent.RH_IsForwarding || Parent.RH_IsShipping || Parent.RH_IsLandTransport || Parent.RH_IsPersonalEffects))
			{
				string errorMessage = Res.GetString("1BF76F84-74F5-49FD-952B-909531291FB7", "A commodity code must at least be configured to apply to one of Forwarding, Shipping, Land Transport or Personal Effects.");
				propertyInfo.AddError(errorMessage);
			}
		}
	}
}
