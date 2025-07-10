using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Windows.Forms;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.Cryptoki.Signing.API;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.GUI;
using Enterprise.Customs.TR.Manifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using AsycudaContainer = Enterprise.Customs.TR.Manifest.Business.AsycudaContainer;
using AsycudaPack = Enterprise.Customs.TR.Manifest.Business.AsycudaPack;
using AsycudaPackedItem = Enterprise.Customs.TR.Manifest.Business.AsycudaPackedItem;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
			yield return new TRBillAdditionalUserControl();
			yield return new VisitedPortsForBillUserControl();
		}

		static ISignatureAlgorithm GetSignatureAlgorithm()
		{
			var externalPassword = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			if (string.IsNullOrEmpty(externalPassword.TR_Chipset))
			{
				throw new CryptographicException("Chipset not specified");
			}

			if (!TryGetHexValue(externalPassword.GP_CertificateSerialNumber, out var certificateSerialNumber))
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Invalid certificate serial number: '{externalPassword.GP_CertificateSerialNumber}'."));
			}

			ISignatureAlgorithm algorithm;
			switch (externalPassword.TR_Chipset)
			{
				case ChipsetList.Codes.WINDOWS:
					algorithm = X509Certificate2CryptoApiSignatureAlgorithm.Create(RemoteCryptoApi.Instance, certificateSerialNumber);
					break;
				default:
					algorithm = Pkcs11CryptoApiSignatureAlgorithm.Create
					(
						RemoteCryptoApi.Instance,
						(Chipset)Enum.Parse(typeof(Chipset), externalPassword.TR_Chipset),
						externalPassword.CurrentDecryptedPassword,
						certificateSerialNumber
					);
					break;
			}

			return algorithm;
		}

		static bool TryGetHexValue(ZString str, out byte[] value)
		{
			if (str.Length % 2 != 0)
			{
				value = null;
				return false;
			}

			byte[] res = new byte[str.Length / 2];
			for (int vOfs = 0, sOfs = 0; vOfs < res.Length; vOfs++, sOfs += 2)
			{
				if (!byte.TryParse(str.Substring(sOfs, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
				{
					value = null;
					return false;
				}

				res[vOfs] = b;
			}

			value = res;
			return true;
		}

		static byte[] SignMessage(string message)
		{
			var algorithm = GetSignatureAlgorithm();
			var signatureBuilder = new SignatureBuilder();
			return signatureBuilder.Sign(Encoding.ASCII.GetBytes(message), algorithm);
		}

		void ManualRegistrationNoEntry(object sender, EventArgs args)
		{
			var menu = sender as ZMenuItem;
			var mainForm = menu.GetMainMenu()?.GetForm() as ZForm;
			var header = mainForm.BusinessEntity as IRegistrationNoEntryProvider;
			ManualRegistrationHelper.ManualRegistrationNoEntry(header, mainForm);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "For Developers Only")]
		protected override IEnumerable<ZMenuItem> GetActionsExtraMenuItemsCore()
		{
			var caption = ResString.GetMultilingualString("5A89BFBB-5C1E-48CA-95E4-87CF24E9B465", "Manual Registration No Entry");
			yield return new ZMenuItem(caption, ManualRegistrationNoEntry);

			if (Env.CurrentUser.IsDeveloper)
			{
				var externalPassword = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
				if (string.IsNullOrEmpty(externalPassword.TR_Chipset))
				{
					yield break;
				}

				var menuItemCaption = (externalPassword.TR_Chipset == ChipsetList.Codes.WINDOWS) ? "[DEV] X509CryptoApi" : "[DEV] PKCS11CryptoApi";
				yield return new ZMenuItem(menuItemCaption, (sender, args) =>
				{
					var sb = new StringBuilder();
					sb.Append("Sign Message: ");
					try
					{
						var signature = SignMessage("Hello World!");
						var cms = new SignedCms();
						cms.Decode(signature);
						cms.CheckSignature(true);

						if (cms.SignerInfos.Count != 1)
						{
							throw new CryptographicException(FormattableString.Invariant($"Expect 1 SignerInfo in signature, but there are {cms.SignerInfos.Count}."));
						}

						sb.Append("success\r\n");
						var signerInfo = cms.SignerInfos[0];
						sb.Append(FormattableString.Invariant($"Digest Algorithm: {signerInfo.DigestAlgorithm.FriendlyName}\r\n"));
						sb.Append(FormattableString.Invariant($"Signature Algorithm: {signerInfo.SignatureAlgorithm.FriendlyName}\r\n"));

						var certificate = signerInfo.Certificate;
						sb.Append(FormattableString.Invariant($"Certificate Serial Number: {certificate.SerialNumber}\r\n"));
						sb.Append(FormattableString.Invariant($"Valid From: {certificate.NotBefore.ToString("D", CultureInfo.InvariantCulture)}\r\n"));
						sb.Append(FormattableString.Invariant($"Valid To: {certificate.NotAfter.ToString("D", CultureInfo.InvariantCulture)}\r\n"));
						sb.Append(FormattableString.Invariant($"Subject: {certificate.Subject}\r\n"));
						sb.Append(FormattableString.Invariant($"Thumbprint: {certificate.Thumbprint}\r\n"));
					}
					catch (Exception e) when (e is CryptographicException || e is IOException)
					{
						sb.Append("failed\r\n");
						sb.Append("Exception:\r\n");
						sb.Append(e.Message);
					}

					Globals.Message.Show(
						sb.ToString(),
						menuItemCaption, MessageBoxButtons.OK, MessageBoxIcon.Information
					);
				});
			}
		}

		protected override IPanelLayoutProvider GetManifestLayoutCore()
		{
			return new TRManifestLayouts();
		}

		protected override IPanelLayoutProvider GetBillLayoutCore()
		{
			return new TRBillLayouts();
		}

		protected override string[] GetPackedItemColumnsCore()
		{
			return new string[]
			{
				AsycudaPackedItem.Schema.API_FormattedTariff,
				AsycudaPackedItem.Schema.API_GoodsDescription,
				AsycudaPackedItem.Schema.API_CustomsQty,
				AsycudaPackedItem.Schema.API_CustomsUQ,
				AsycudaPackedItem.Schema.API_GrossWeight,
				AsycudaPackedItem.Schema.API_GrossWeightUQ,
				AsycudaPackedItem.Schema.API_NetWeight,
				AsycudaPackedItem.Schema.API_NetWeightUQ,
				AsycudaPackedItem.Schema.API_GoodsValue,
				AsycudaPackedItem.Schema.API_RX_NKGoodsValueCurrency,
				UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
				UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
			};
		}

		protected override string[] GetPacksGridColumnsOrderCore()
		{
			return new string[]
			{
				AsycudaPack.Schema.ContainerPK,
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_CommodityCode,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.APA_MarksAndNumbers,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
			};
		}

		protected override IEnumerable<string> GetContainersGridColumnsOrderCore()
		{
			yield return AsycudaContainer.Schema.ACN_ContainerNumber;
			yield return AsycudaContainer.Schema.ACN_EmptyFullIndicator;
			yield return AsycudaContainer.Schema.ACN_RC_ContainerType;
			yield return AsycudaContainer.Schema.Relation;
			yield return AsycudaContainer.Schema.ACN_Seal1;
			yield return AsycudaContainer.Schema.ACN_SealType1;
			yield return AsycudaContainer.Schema.ACN_SealingPartyType;
			yield return AsycudaContainer.Schema.ACN_SealingPartyName;
			yield return AsycudaContainer.Schema.ACN_NumberOfPackages;
			yield return AsycudaContainer.Schema.ACN_CommodityCode;
			yield return AsycudaContainer.Schema.ACN_GoodsWeight;
			yield return AsycudaContainer.Schema.ACN_GoodsWeightUQ;
			yield return AsycudaContainer.Schema.ACN_StowageLocation;
		}

		protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			yield return new VisitedPortsForManifestHeaderUserControl();
			yield return new ManifestToOpenForManifestHeaderUserControl();
		}

		protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var relationDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			relationDropEditColumnStyleInfo.ColumnName = Business.AsycudaContainer.Schema.Relation;
			relationDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			relationDropEditColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			yield return relationDropEditColumnStyleInfo;
		}

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new TRBillPartiesLayouts();

		protected override bool IsMessageGridUserFullNameVisibleCore => true;
	}
}
