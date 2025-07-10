using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineLinkControllingMsgHeader : AutoInvoiceLineLinkControllingMsgHeader
	{
		public InvoiceLineLinkControllingMsgHeader(CusTWControllingMessageHeader controllingMessageHeader, JobComInvoiceLine invoiceline) : base(invoiceline.Factory)
		{
			ControllingMessageHeader = controllingMessageHeader;
			Invoiceline = Argument.NotNull(invoiceline, nameof(invoiceline));
			ControllingMessageHeaderPK = ControllingMessageHeader?.PK ?? ZGuid.NewZGuid();
		}
		public ZGuid ControllingMessageHeaderPK { get; }

		public CusTWControllingMessageHeader ControllingMessageHeader { get; }

		internal JobComInvoiceLine Invoiceline { get; }

		public override ZString ControllingAgency => ControllingMessageHeader?.TW1_ControllingAgency ?? ZString.Empty;

		public override ZString FunctionalReferenceID => ControllingMessageHeader?.TW1_FunctionalReferenceId ?? ZString.Empty;

		public override ZString PermitNo => ControllingMessageHeader?.PermitNumber ?? ZString.Empty;

		bool IsLinkedCMHeaderReadOnly => ControllingMessageHeader == null ? ZBool.False : Invoiceline.HasLinkedToOtherSameTypeControllingMessageHeader(ControllingMessageHeader);

		[ReadOnlyMember(nameof(IsLinkedCMHeaderReadOnly))]
		public override ZBool IsLinkedCMHeader
		{
			get => base.IsLinkedCMHeader;
			set
			{
				if (!IsLinkSettingSuspender)
				{
					using (SuspendLinkSetting())
					{
						var hasChanged = IsLinkedCMHeader != value;
						if (!IsCopying && hasChanged)
						{
							var genPivots = Invoiceline.InvoiceLineRelatedControllingMsgHeadersGenPivots;
							if (value)
							{
								if (ControllingMessageHeader.IsCertificate15)
								{
									Invoiceline.JI_PermitUnitPrice = Invoiceline.AddInfoChild.TWL_DocumentaryUnitPrice;
									Invoiceline.JI_IMPTariff = Invoiceline.JI_Tariff.Left(8);
									Invoiceline.JI_PermitUQ = CusRefPacksHelper.LoadRefPack(Invoiceline.Factory, ZString.Empty, RPTypeList.Codes.PermitQuantityUnits, Core.Constants.CountryCodes.Taiwan, customsPack: Invoiceline.JI_InvoiceUQ)?.RP_CommercialPack ?? ZString.Empty;
								}
								else
								{
									Invoiceline.JI_PermitUQ = Invoiceline.JI_InvoiceUQ;
								}

								if (ControllingMessageHeader.IsNX101)
								{
									if (ControllingMessageHeader.IsCertificate15)
									{
										Invoiceline.JI_CustomPermitUQ = ZString.Empty;
									}
									else if (Invoiceline.JI_CustomPermitUQ.IsEmpty)
									{
										Invoiceline.JI_CustomPermitUQ = Invoiceline.JI_PermitUQ;
									}
								}

								Invoiceline.JI_PermitQty = Invoiceline.JI_InvoiceQuantity;

								genPivots.AddPivotFor(ControllingMessageHeader);
							}
							else
							{
								genPivots.DeletePivotFor(ControllingMessageHeader);
							}
						}
					}
				}
				base.IsLinkedCMHeader = value;
			}
		}

		#region InvoiceLineLinkControllingMsgHeaderLinkSettingSuspender
		internal bool IsLinkSettingSuspender => linkSettingSuspenderIndex > 0;

		int linkSettingSuspenderIndex;

		public IDisposable SuspendLinkSetting()
		{
			return new InvoiceLineLinkControllingMsgHeaderLinkSettingSuspender(this);
		}

		class InvoiceLineLinkControllingMsgHeaderLinkSettingSuspender : IDisposable
		{
			public InvoiceLineLinkControllingMsgHeaderLinkSettingSuspender(InvoiceLineLinkControllingMsgHeader invoiceLineLinkControllingMsgHeader)
			{
				this.invoiceLineLinkControllingMsgHeader = invoiceLineLinkControllingMsgHeader;
				this.invoiceLineLinkControllingMsgHeader.linkSettingSuspenderIndex++;
			}

			readonly InvoiceLineLinkControllingMsgHeader invoiceLineLinkControllingMsgHeader;

			void IDisposable.Dispose()
			{
				invoiceLineLinkControllingMsgHeader.linkSettingSuspenderIndex--;
			}
		}
		#endregion

		public override ZShort Sequence => ControllingMessageHeader?.TW1_Sequence ?? ZShort.Zero;

		public override ZString MessageType => ControllingMessageHeader?.TW1_ControllingMessageType ?? ZString.Empty;

		public override ZString MessageTypeDescription => ControllingMessageHeader?.ControllingMessageTypeDescription ?? ZString.Empty;

		public override ZString ControllingAgencyDescription => ControllingMessageHeader?.ControllingAgencyDescription ?? ZString.Empty;

		public override ZString CertificateType => ControllingMessageHeader?.TW1_CertificateType ?? ZString.Empty;

		public override ZString CertificateTypeDescription => ControllingMessageHeader?.CertificateTypeDescription ?? ZString.Empty;

		public override ZString BusinessType => ControllingMessageHeader?.TW1_BusinessType ?? ZString.Empty;

		public override ZString BusinessTypeDescription => ControllingMessageHeader?.BusinessTypeDescription ?? ZString.Empty;

		public override ZString ProcessingUnit => ControllingMessageHeader?.TW1_ProcessingUnit ?? ZString.Empty;

		public override ZString ProcessingUnitDescription => ControllingMessageHeader?.ProcessingUnitDescription ?? ZString.Empty;

		public bool IsNX101ContainZZZCertificateTypes => IsLinkedCMHeader && (ControllingMessageHeader?.IsNX101ContainZZZCertificateTypes ?? false);

		public bool IsNX101NotContainZZZCertificateTypes => IsLinkedCMHeader && (ControllingMessageHeader?.IsNX101NotContainZZZCertificateTypes ?? false);
	}
}
