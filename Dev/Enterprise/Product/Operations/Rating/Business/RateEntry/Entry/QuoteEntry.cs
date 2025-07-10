using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class QuoteEntry : RateEntry
	{
		#region Schema

		public new abstract class Schema : RateEntry.Schema
		{
			public const string IncludeCurrentUserDetailsOnDocumentation = "IncludeCurrentUserDetailsOnDocumentation";
			public const string IncludeOverallRepDetailsOnDocumentation = "IncludeOverallRepDetailsOnDocumentation";
		}

		#endregion

		public QuoteEntry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public new Quote Parent
		{
			get { return (Quote)base.Parent; }
			set { base.Parent = value; }
		}

		#region TI_OH_AgentOverride

		/// <summary>
		/// The agent override for this rate entry.
		/// When set, it will also set the agent override on all other entries that have the same:
		///		- origin
		///		- destination
		///		- transport mode
		///	and currently don't have an agent override specified
		/// </summary>
		public override ZGuid TI_OH_AgentOverride
		{
			get { return base.TI_OH_AgentOverride; }
			set
			{
				base.TI_OH_AgentOverride = value;

				if (value.IsValid && ((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					foreach (RateEntry entry in ((IBusinessObjectInternals)this).ParentCollections[0])
					{
						if (entry.TI_OH_AgentOverride.IsEmpty &&
							entry.TI_OriginLRC == TI_OriginLRC &&
							entry.TI_DestinationLRC == TI_DestinationLRC &&
							entry.TI_Mode == TI_Mode &&
							entry.TI_RateCategory == TI_RateCategory)
						{
							entry.TI_OH_AgentOverride = value;
						}
					}
				}
			}
		}

		#endregion

		#region Heading Text

		public ZString QuotationHeader
		{
			get
			{
				ZString result;
				if (this.IsShipping())
				{
					result = new CompanyTariffCodes().GetCompanyTariffDiscountDescription(TI_RateCategory);
				}
				else if (TI_Mode == Core.Constants.RateMode.COU)
				{
					result = Res.GetString("71df090b-1c17-4e62-b56e-f9abf67ee6f3", "Courier");
				}
				else
				{
					result = FreightType == Core.Constants.RateMode.ALL ? (NoResString)"Freight" : Res.GetString("d987216c-349b-488e-ae43-2c9e3fcb8058", "{0} Freight", FreightType); // Hard-coded header constant
				}
				if (OriginForDescription != null)
				{
					result += " " + Res.GetString("697a7dd6-2316-489d-8f35-bd6cdad257db", "from") + " ";
				}

				return QuotationIndexDescriptionInternal(result);
			}
		}

		protected override ILocation OriginForDescription
		{
			get { return Parent.TH_OneTimeQuote && Parent.CurrentOneOffQuote != null ? Parent.CurrentOneOffQuote.ReceivalLocation : base.OriginForDescription; }
		}

		protected override ILocation DestinationForDescription
		{
			get { return Parent.TH_OneTimeQuote && Parent.CurrentOneOffQuote != null ? Parent.CurrentOneOffQuote.DeliveryLocation : base.DestinationForDescription; }
		}

		protected override ILocation ViaForDescription
		{
			get { return Parent.TH_OneTimeQuote && Parent.CurrentOneOffQuote != null ? Parent.CurrentOneOffQuote.ViaLocation : base.ViaForDescription; }
		}

		public override ZString PageHeader
		{
			get { return TI_PageHeading.IsEmpty ? (ZString)Env.Registry.Rating.QuoteHeaderText : TI_PageHeading; }
		}

		#endregion

		#region Opening Text

		[CargoWise.ComponentModel.MaxLength(3000)]
		public ZString OpeningText
		{
			get { return TI_PageOpeningText.IsEmpty ? (ZString)DocumentsDataRegistry.Instance.QuoteOpeningText.Value : TI_PageOpeningText; }
			set
			{
				CheckMaximumLength(OpeningTextInfo, value);
				TI_PageOpeningText = value;
				OpeningTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OpeningTextInfo
		{
			get { return GetZPropertyInfo(nameof(OpeningText)); }
		}

		#endregion

		#region Closing Text

		[CargoWise.ComponentModel.MaxLength(3000)]
		public ZString ClosingText
		{
			get { return TI_PageClosingText.IsEmpty ? (ZString)DocumentsDataRegistry.Instance.QuoteClosingText.Value : TI_PageClosingText; }
			set
			{
				CheckMaximumLength(ClosingTextInfo, value);
				TI_PageClosingText = value;
				ClosingTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ClosingTextInfo
		{
			get { return GetZPropertyInfo(nameof(ClosingText)); }
		}

		#endregion

		#endregion

		#region Validation Provider

		protected override RateEntryValidation GetNewValidation()
		{
			return new QuoteRateEntryValidation(this);
		}

		#endregion
	}
}

