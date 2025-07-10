using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Business
{
	public class WiseHeader : IRatingHeader
	{
		public WiseHeader(BusinessObjectFactory factory)
		{
			PK = ZGuid.NewZGuid();
			Factory = factory;
			ChildRateEntries = System.Array.Empty<IRateEntry>();
		}

		public string InvalidReason
		{
			get
			{
				var reason = string.Join(System.Environment.NewLine, Errors.Select(e => e.Value));
				return reason;
			}
		}

		public IDictionary<SchemaColumn, string> Errors { get; } = new Dictionary<SchemaColumn, string>();

		public WiseRatesConverter Converter { get; }
		public ILogger Logger { get; }
		public Rate WiseRate { get; }

		public ZString TH_QuoteNumber
		{
			get { return ZString.Empty; }
		}

		public ZByte TH_GlobalRateLevel
		{
			get { return 0; }
		}

		public OrgHeader Header
		{
			get { return Factory.Load<OrgHeader>(TH_OH); }
		}

		public ZGuid TH_OH { get; set; }

		public RefCarrier WiseCarrier { get; set; }

		public GlbCompany Company
		{
			get { return GlbCompany.CurrentCompany; }
		}

		public ZString TH_GlobalRateDescription
		{
			get { return string.Empty; }
		}

		public MultilingualString TH_GlobalRateDescriptionMultilingual
		{
			get { return TH_OH.IsEmpty ? ResString.GetMultilingualString("FE1B2897-4301-4209-8D23-93C6DFB47AFA", "General Costing") : null; }   // It is translatable string
		}

		public ZString RatingHeaderTypeDescription
			=> Res.GetString("f6f9c74e-b44e-4561-b32f-10306f2a5bca", "Wise Costing");

		public ZString DisplayInfo()
			=> RatingHeader.DisplayInfoWithOrgInfo(RatingHeaderTypeDescription, Header);

		public ZBool TH_OneTimeQuote
		{
			get { return false; }
		}

		public ZGuid PK { get; }

		public BusinessObjectFactory Factory { get; }

		public IEnumerable<IRateEntry> LoadRateEntriesForAutoRater(ZQuery odFilter)
		{
			return ChildRateEntries;
		}

		public IEnumerable<IRateEntry> ChildRateEntries
		{
			get
			{
				return entries;
			}
			set
			{
				foreach (var entry in value)
				{
					if (entry is WiseEntry wiseEntry)
					{
						wiseEntry.ParentRatingHeader = this;
					}
				}

				entries = value;
			}
		}

		public ZString TH_RateType
		{
			get { return RatingConstants.RatingHeaderTypes.WiseCost; }
		}

		IEnumerable<IRateEntry> entries;
	}
}
