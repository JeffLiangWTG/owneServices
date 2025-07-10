using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderLinkInvoiceLine : AutoControllingMessageHeaderLinkInvoiceLine
	{
		public ControllingMessageHeaderLinkInvoiceLine(CusTWControllingMessageHeader controllingMessageHeader, JobComInvoiceLine invoiceline) : base(controllingMessageHeader.Factory)
		{
			ControllingMessageHeader = Argument.NotNull(controllingMessageHeader, nameof(controllingMessageHeader));
			Invoiceline = invoiceline;
			InvoicelinePK = Invoiceline?.PK ?? ZGuid.Empty;
			InvoiceHeader = Invoiceline?.InvoiceHeader;
		}

		internal CusTWControllingMessageHeader ControllingMessageHeader { get; }

		public JobComInvoiceLine Invoiceline { get; }

		JobComInvoiceHeader InvoiceHeader { get; }

		public ZGuid InvoicelinePK { get; }

		bool LinkReadOnly => (Invoiceline?.HasLinkedToOtherSameTypeControllingMessageHeader(ControllingMessageHeader) ?? false) || ControllingMessageHeader.IsNX201_07;

		[ReadOnlyMember(nameof(LinkReadOnly))]
		public override ZBool Link
		{
			get => base.Link;
			set
			{
				if (!IsLinkSettingSuspender)
				{
					using (SuspendLinkSetting())
					{
						var hasChanged = Link != value;
						if (!IsCopying && hasChanged)
						{
							if (Invoiceline != null)
							{
								var genPivots = Invoiceline.InvoiceLineRelatedControllingMsgHeadersGenPivots;
								if (value)
								{
									genPivots.AddPivotFor(ControllingMessageHeader);
								}
								else
								{
									genPivots.DeletePivotFor(ControllingMessageHeader);
								}
							}
						}
					}
				}
				base.Link = value;
			}
		}

		#region ControllingMessageHeaderLinkInvoiceLineLinkSettingSuspender
		internal bool IsLinkSettingSuspender => linkSettingSuspenderIndex > 0;

		int linkSettingSuspenderIndex;

		public IDisposable SuspendLinkSetting()
		{
			return new ControllingMessageHeaderLinkInvoiceLineLinkSettingSuspender(this);
		}

		class ControllingMessageHeaderLinkInvoiceLineLinkSettingSuspender : IDisposable
		{
			public ControllingMessageHeaderLinkInvoiceLineLinkSettingSuspender(ControllingMessageHeaderLinkInvoiceLine controllingMessageHeaderLinkInvoiceLine)
			{
				this.controllingMessageHeaderLinkInvoiceLine = controllingMessageHeaderLinkInvoiceLine;
				this.controllingMessageHeaderLinkInvoiceLine.linkSettingSuspenderIndex++;
			}

			readonly ControllingMessageHeaderLinkInvoiceLine controllingMessageHeaderLinkInvoiceLine;

			void IDisposable.Dispose()
			{
				controllingMessageHeaderLinkInvoiceLine.linkSettingSuspenderIndex--;
			}
		}
		#endregion

		public override ZShort InvoiceSequence => InvoiceHeader?.JZ_InvoiceDisplaySequence ?? ZShort.Zero;

		public override ZString InvoiceNumber => InvoiceHeader?.JZ_InvoiceNumber ?? ZString.Empty;

		public override ZShort InvoiceLineSequence => Invoiceline?.JI_LineNo ?? ZShort.Zero;

		public override ZDecimal InvoiceQuantity => Invoiceline?.JI_InvoiceQuantity ?? ZDecimal.Zero;

		public override ZString InvoiceQuantityUQ => Invoiceline?.JI_InvoiceUQ ?? ZString.Empty;

		public override ZDecimal UnitPrice => Invoiceline?.JI_EnteredUnitPrice ?? ZDecimal.Zero;

		public override ZDecimal LinePrice => Invoiceline?.JI_LinePrice ?? ZDecimal.Zero;

		public override ZString Tariff => Invoiceline?.JI_FormattedTariff ?? ZString.Empty;

		public override ZString CountryOfOrigin => Invoiceline?.JI_CountryOfOrigin ?? ZString.Empty;

		public override ZString Model => Invoiceline?.JI_Model ?? ZString.Empty;

		public override ZString BrandName => Invoiceline?.JI_BrandName ?? ZString.Empty;

		public override ZString RH_NKCommodity_Code => Invoiceline?.JI_RH_NKCommodity_Code ?? ZString.Empty;

		public override ZDecimal Weight => Invoiceline?.JI_Weight ?? ZDecimal.Zero;

		public override ZString WeightUQ => Invoiceline?.JI_WeightUQ ?? ZString.Empty;

		public override ZDecimal Volume => Invoiceline?.JI_Volume ?? ZDecimal.Zero;

		public override ZString VolumeUQ => Invoiceline?.JI_VolumeUQ ?? ZString.Empty;

		public override ZString OrderNumber => Invoiceline?.JI_OrderNumber ?? ZString.Empty;

		public override ZString Calc_OrderLineNumberAndSubLine => Invoiceline?.JI_Calc_OrderLineNumberAndSubLine ?? ZString.Empty;

		public override ZString PartAttrib1 => Invoiceline?.JI_PartAttrib1 ?? ZString.Empty;

		public override ZString PartAttrib2 => Invoiceline?.JI_PartAttrib2 ?? ZString.Empty;

		public override ZString PartAttrib3 => Invoiceline?.JI_PartAttrib3 ?? ZString.Empty;

		public override ZString SerialNumber => Invoiceline?.JI_SerialNumber ?? ZString.Empty;

		public override ZString CustomsOwnerPartNo => Invoiceline?.JI_Calc_OwnerPartNo ?? ZString.Empty;

		public override ZString NewPartAttribute1 => Invoiceline?.JI_NewPartAttribute1 ?? ZString.Empty;

		public override ZString NewPartAttribute2 => Invoiceline?.JI_NewPartAttribute2 ?? ZString.Empty;

		public override ZString NewPartAttribute3 => Invoiceline?.JI_NewPartAttribute3 ?? ZString.Empty;

		public override ZString NewSerialNumber => Invoiceline?.JI_NewSerialNumber ?? ZString.Empty;

		public override ZString CustomAttrib1 => Invoiceline?.JI_CustomAttrib1 ?? ZString.Empty;

		public override ZString CustomAttrib2 => Invoiceline?.JI_CustomAttrib2 ?? ZString.Empty;

		public override ZString CustomAttrib3 => Invoiceline?.JI_CustomAttrib3 ?? ZString.Empty;

		public override ZString CustomAttrib4 => Invoiceline?.JI_CustomAttrib4 ?? ZString.Empty;

		public override ZString CustomAttrib5 => Invoiceline?.JI_CustomAttrib5 ?? ZString.Empty;

		public override ZString CustomAttrib6 => Invoiceline?.JI_CustomAttrib6 ?? ZString.Empty;

		public override ZString CustomTextBlob1 => Invoiceline?.JI_CustomTextBlob1 ?? ZString.Empty;

		public override ZString CustomsSupplierPartNo => Invoiceline?.JI_CustomsSupplierPartNo ?? ZString.Empty;

		public override ZString NDescription => Invoiceline?.JI_NDescription ?? ZString.Empty;

		public override ZString PartNo => Invoiceline?.JI_PartNo ?? ZString.Empty;

		public override ZString PrimaryPreference => Invoiceline?.JI_PrimaryPreference ?? ZString.Empty;

		public override ZString Procedure => Invoiceline?.JI_Procedure ?? ZString.Empty;

		public override ZString Group => Invoiceline?.JI_Group ?? ZString.Empty;

		public override ZString Description => Invoiceline?.JI_Description ?? ZString.Empty;

		public override ZDecimal NetWeight => Invoiceline?.JI_NetWeight ?? ZDecimal.Zero;

		public override ZString NetWeightUQ => Invoiceline?.JI_NetWeightUQ ?? ZString.Empty;

		public override ZDecimal CustomsSecondQuantity => Invoiceline?.JI_CustomsSecondQuantity ?? ZDecimal.Zero;

		public override ZString CustomsSecondQuantityUQ => Invoiceline?.JI_CustomsSecondUnitQty ?? ZString.Empty;

		public override ZString MatchingKey => Invoiceline?.JI_MatchingKey ?? ZString.Empty;

		public override ZString NKClassUsageCommentReviewer => Invoiceline?.JI_GS_NKClassUsageCommentReviewer ?? ZString.Empty;

		public override ZString ClassUsageComment => Invoiceline?.JI_ClassUsageComment ?? ZString.Empty;

		public override ZBool IsClassUsageCommentRead => Invoiceline?.JI_IsClassUsageCommentRead ?? ZBool.False;

		public override ZString CitesPermit => Invoiceline?.CitesPermit ?? ZString.Empty;

		public override ZString HighTechLicense => Invoiceline?.HighTechLicense ?? ZString.Empty;

		public override ZString TariffAdditionalCode => Invoiceline?.JI_TariffAdditionalCode ?? ZString.Empty;

		public override ZDecimal AlcoholPercentage => Invoiceline?.JI_AlcoholPercentage ?? ZDecimal.Zero;

		public override ZDecimal CusValueConvRatio => Invoiceline?.JI_CusValueConvRatio ?? ZDecimal.Zero;

		public override ZString CarType => Invoiceline?.JI_CarType ?? ZString.Empty;

		public override ZString Transmission => Invoiceline?.JI_Transmission ?? ZString.Empty;

		public override ZString EngineType => Invoiceline?.JI_EngineType ?? ZString.Empty;

		public override ZString LHD => Invoiceline?.JI_LHD ?? ZString.Empty;

		public override ZString HasCatalystConverter => Invoiceline?.JI_HasCatalystConverter ?? ZString.Empty;

		public override ZString CarCondition => Invoiceline?.JI_CarCondition ?? ZString.Empty;

		public override ZShort ModelYear => Invoiceline?.JI_ModelYear ?? ZShort.Zero;

		public override ZString Displacement => Invoiceline?.JI_Displacement ?? ZString.Empty;

		public override ZShort NumberOfDoor => Invoiceline?.JI_NumberOfDoor ?? ZShort.Zero;

		public override ZShort Cylinders => Invoiceline?.JI_Cylinders ?? ZShort.Zero;

		public override ZShort Seats => Invoiceline?.JI_Seats ?? ZShort.Zero;

		public override ZShort Gears => Invoiceline?.JI_Gears ?? ZShort.Zero;

		public override ZString DtyPymntMthd => Invoiceline?.JI_DtyPymntMthd ?? ZString.Empty;

		public override ZString VatPymntMthd => Invoiceline?.JI_VatPymntMthd ?? ZString.Empty;

		public override ZString FormattedAdValoremDutyRate => Invoiceline?.FormattedAdValoremDutyRate ?? ZString.Empty;

		public override ZString FormattedSpecificDutyRate => Invoiceline?.FormattedSpecificDutyRate ?? ZString.Empty;

		public override ZString BondedGoodsCode => Invoiceline?.JI_BondedGoodsCode ?? ZString.Empty;

		public override ZString TariffDescription => Invoiceline?.JI_TariffDescription ?? ZString.Empty;
	}
}
