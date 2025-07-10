using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
	{
		public JobDeclarationSynchroniser(JobDeclaration destination)
			: base(destination)
		{
		}

		protected new JobDeclaration Destination
		{
			get { return (JobDeclaration)base.Destination; }
		}

		#region FieldSynchronisers

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_ECI_InvoiceAmountInfo, GetJE_ECI_InvoiceAmount, GetJE_ECI_InvoiceAmountInfos));
			Synchronisers.Add(new FieldSynchroniser(Destination.JE_ECI_InvoiceCurrencyInfo, GetJE_ECI_InvoiceCurrency, GetJE_ECI_InvoiceCurrencyInfos));

			Synchronisers.Add(new FieldSynchroniser(Destination.MiscSupplierNameInfo, GetMiscSupplierName, GetMiscSupplierNameInfos));
			Synchronisers.Add(new FieldSynchroniser(Destination.MiscImporterNameInfo, GetMiscImporterName, GetMiscImporterNameInfos));

			lastJE_MessageSubType = Destination.JE_MessageSubType;
			Destination.JE_MessageSubTypeInfo.ValueChanged -= new EventHandler(JE_MessageSubTypeInfo_ValueChanged);
			Destination.JE_MessageSubTypeInfo.ValueChanged += new EventHandler(JE_MessageSubTypeInfo_ValueChanged);
		}

		protected override IZType GetFkForOhSupplier()
		{
			return Source.ConsignorDocumentaryAddress.E2_AddressOverride ? Destination.CachedMiscOrgPK : Source.ConsignorDocumentaryAddress.OrganisationPK;
		}

		protected override IZType GetFkForOhImporter()
		{
			return Source.ConsigneeDocumentaryAddress.E2_AddressOverride ? Destination.CachedMiscOrgPK : Source.ConsigneeDocumentaryAddress.OrganisationPK;
		}

		IZType GetJE_ECI_InvoiceCurrency()
		{
			return Destination.IsECIWriteoff ? Source.GoodsValueCurrencyPK : ZGuid.Empty;
		}

		IEnumerable<ZPropertyInfo> GetJE_ECI_InvoiceCurrencyInfos()
		{
			yield return Source.GoodsValueCurrencyPKInfo;
			yield return Destination.JE_MessageSubTypeInfo;
		}

		IZType GetJE_ECI_InvoiceAmount()
		{
			return Destination.IsECIWriteoff ? Source.JS_GoodsValue : ZDecimal.Zero;
		}

		IEnumerable<ZPropertyInfo> GetJE_ECI_InvoiceAmountInfos()
		{
			yield return Source.JS_GoodsValueInfo;
			yield return Destination.JE_MessageSubTypeInfo;
		}

		IZType GetMiscSupplierName()
		{
			return Destination.JE_OH_Supplier == Destination.CachedMiscOrgPK ? Source.ConsignorDocumentaryAddress.E2_CompanyName : ZString.Empty;
		}

		IEnumerable<ZPropertyInfo> GetMiscSupplierNameInfos()
		{
			yield return Source.ConsignorDocumentaryAddress.E2_CompanyNameInfo;
			yield return Destination.JE_OH_SupplierInfo;
		}

		IZType GetMiscImporterName()
		{
			return Destination.JE_OH_Importer == Destination.CachedMiscOrgPK ? Source.ConsigneeDocumentaryAddress.E2_CompanyName : ZString.Empty;
		}

		IEnumerable<ZPropertyInfo> GetMiscImporterNameInfos()
		{
			yield return Source.ConsigneeDocumentaryAddress.E2_CompanyNameInfo;
			yield return Destination.JE_OH_ImporterInfo;
		}

		protected override void UnHookSynchronisers()
		{
			Destination.JE_MessageSubTypeInfo.ValueChanged -= new EventHandler(JE_MessageSubTypeInfo_ValueChanged);
			base.UnHookSynchronisers();
		}

		void JE_MessageSubTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Destination.JE_MessageSubType != lastJE_MessageSubType && Destination.JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff && Source != null)
			{
				Destination.InvoiceManager.SetAmountAndCurrency(Source.JS_GoodsValue, Source.JS_RX_NKGoodsValueCurr);
			}
			lastJE_MessageSubType = Destination.JE_MessageSubType;
		}
		ZString lastJE_MessageSubType;

		protected override IZType GetPortOfArrival()
		{
			ZString result = ZString.Empty;
			if (hookedConsol != null)
			{
				result = Destination.IsImport ? hookedConsol.JK_RL_NKDiscForLastImportTransport : hookedConsol.JK_RL_NKDiscForExportTransport;
			}
			return result;
		}

		protected override ZPropertyInfo[] GetPortOfArrivalRelatedInfos()
		{
			List<ZPropertyInfo> infos = new List<ZPropertyInfo>();
			infos.Add(Destination.JE_MessageTypeInfo);
			if (hookedConsol != null)
			{
				infos.Add(hookedConsol.JK_RL_NKDiscForLastImportTransportInfo);
				infos.Add(hookedConsol.JK_RL_NKDiscForExportTransportInfo);
			}
			return infos.ToArray();
		}

		protected override ZString GetCustomsUnitForThisPackType(ZString freightPackType)
		{
			return PackageTypeConverter.GetCustomsPackageType(freightPackType);
		}

		protected override ZString GetConvertedTransportMode(ZString shipmentTransportMode)
		{
			var result = base.GetConvertedTransportMode(shipmentTransportMode);

			if (shipmentTransportMode == Core.Constants.TransportModes.Courier)
			{
				result = JobTransportModeList.Codes.Post;
			}

			return result;
		}

		protected override PackingSynchroniser GetPackingSynchroniser()
		{
			return new PackLineSynchroniser(this, Destination);
		}

		#endregion
	}
}
