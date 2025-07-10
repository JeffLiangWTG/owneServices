using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.GlobalCommercialInvoice.Business
{
	/// <summary>
	/// Global Commercial Invoice line business logic.
	/// </summary>
	/// <param name="factory"><see cref="BusinessObjectFactory"/> object instance.</param>
	/// <param name="row"><see cref="DataRow"/> containing the header data.</param>
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public sealed class GlobalCommercialInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: AutoGlobalCommercialInvoiceLine(factory, row)
	{
		/// <summary>
		/// The header of this line.
		/// </summary>
		GlobalCommercialInvoiceHeader header;

		/// <summary>
		/// This value is initialized with an empty collection value because
		/// we cannot create a correct list at this point, but it is required as a lookup.
		/// </summary>
		GlobalCommercialInvoiceHeaderCollection headers = GlobalCommercialInvoiceHeaderCollection.GetEmpty(factory);

		/// <summary>
		/// Tariff formatter.
		/// It is thread-safe because the class contains no data, only methods.
		/// </summary>
		[ThreadSafe]
		static readonly ITariffFormatter tariffFormatter = new TariffFormatter();

		/// <summary>
		/// The collection of invoice headers.
		/// Initially, we assign a fake value because we cannot create a correct list at this point.
		/// See <see cref="GlobalCommercialInvoiceLineIntegratedCollection" where the real value is assigned. />
		/// </summary>
		public GlobalCommercialInvoiceHeaderCollection Headers
		{
			get => headers;
			set
			{
				headers = value;
				header = GetParentInvoiceLineHeader();
			}
		}

		/// <summary>
		/// A method that will provide a value to <see cref="ProvideMetaDataPropertyAttribute"/>.
		/// </summary>
		/// <param name="property">Property to get.</param>
		/// <returns></returns>
		public bool GetReadOnlySecurity(PropertyDescriptor property) => InvoiceSecurityProvider.IsReadOnly(Factory, headers.ParentID);

		/// <summary>
		/// The purpose of this override is to provide the list of the invoice headers in the attribute.
		/// Also, the last selected header ID is set.
		/// </summary>
		[List(nameof(Headers))]
		public override ZGuid GIL_GIH_Header
		{
			get => base.GIL_GIH_Header;
			set
			{
				if (headers.Any(x => x.PK == value))
				{
					base.GIL_GIH_Header = value;
				}
				header = GetParentInvoiceLineHeader();
				GIL_GIH_HeaderInfo.RefreshBinding();
			}
		}

		[List(nameof(Lookups) + "." + nameof(Lookups.TariffCodeList))]
		public override ZString GIL_Tariff1
		{
			get => base.GIL_Tariff1;
			set => base.GIL_Tariff1 = tariffFormatter.DisplayFormat(value).Left(GIL_Tariff1Info.MaxLength);
		}

		[List(nameof(Lookups) + "." + nameof(Lookups.TariffCodeList))]
		public override ZString GIL_Tariff2
		{
			get => base.GIL_Tariff2;
			set => base.GIL_Tariff2 = tariffFormatter.DisplayFormat(value).Left(GIL_Tariff2Info.MaxLength);
		}

		[List(nameof(Lookups) + "." + nameof(Lookups.ProductCodeList))]
		public override ZString GIL_Product
		{
			get => base.GIL_Product;
			set => base.GIL_Product = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GIL_GrossWeightUQ = Env.Registry.FreightWeightUnit;
			GIL_NetWeightUQ = Env.Registry.FreightWeightUnit;
			GIL_VolumeUQ = Env.Registry.FreightVolumeUnit;
		}

		/// <summary>
		/// A shortcut to get line price currency.
		/// </summary>
		public ZString LinePriceCurrency => header?.GIH_RX_NKInvoiceCurrency ?? ZString.Empty;

		/// <summary>
		/// Gets the header of this line.
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		GlobalCommercialInvoiceHeader GetParentInvoiceLineHeader() => headers.Count > 0 && !base.GIL_GIH_Header.IsEmpty
			? headers.FirstOrDefault((h) => h.PK == base.GIL_GIH_Header) : default;
	}
}
