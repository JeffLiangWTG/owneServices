using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.UY.Manifest.Business
{
	internal class DAEManifestWrapper : IManifest
	{
		internal DAEManifestWrapper(AsycudaManifestHeader header, AsycudaBill[] bills)
		{
			Header = Argument.NotNull(header, "asycudaManifestHeader cannot be null");
			Bills = bills;
		}
		protected readonly AsycudaManifestHeader Header;
		protected readonly AsycudaBill[] Bills;

		string IManifest.TransportMode => Core.Constants.TransportCodes.Air;

		string IManifest.ManifestNature => AsycudaBill.UYConstants.ImportTypeCode;

		string IManifest.ManifestNo => Header.AMA_Voyage;

		string IManifest.CustomsOffice => Header.AMA_CustomsOffice;

		long IManifest.DateOfArrival => Header.AMA_DateAtCustomsOffice.IsEmpty ? ZLong.Zero : ZLong.Parse(Header.AMA_DateAtCustomsOffice.ToString("yyyyMMdd"));

		string IManifest.ShippingProviderDocType => ConsigneeDocumentTypes.TaxID;

		string IManifest.ShippingProviderDoc => Header.Carrier?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_CodeType == UruguayOrgCusCodeInfo.OrgCusCodes.RUT)?.OK_CustomsRegNo ?? string.Empty;

		string IManifest.PlaceDepartureCode => GetValidatePortCode((ZString)Header?.AMA_RL_NKPortOfLoading);

		string IManifest.PlaceOfArrivalCode => GetValidatePortCode((ZString)Header?.AMA_RL_NKPortOfDischarge);

		public string Record_Type { get => recordType; set => recordType = value; }
		string recordType;

		IReadOnlyCollection<IDaeBillOfLading> IManifest.BillsOfLading
		{
			get
			{
				var result = new List<IDaeBillOfLading>();
				foreach (AsycudaBill bill in Bills)
				{
					result.Add(new DAEBillWrapper(bill));
				}
				return result;
			}
		}

		string GetValidatePortCode(ZString portCode) => string.Concat(portCode.SubstringSafe(0, 2), " ", portCode.SubstringSafe(2, 3));
	}

	internal class DAEAmendManifestWrapper : DAEManifestWrapper, IManifest
	{
		internal DAEAmendManifestWrapper(AsycudaManifestHeader header, AsycudaBill[] bills) : base(header, bills)
		{
			manifestSent = UYHelperClass.GetDaeOriginal(DAEWrappersHelper.GetDAEObjectOriginal(header));
		}

		readonly DaeMessageBuilderLastSent manifestSent;

		IManifest Manifest => this;

		IReadOnlyCollection<IDaeBillOfLading> IManifest.BillsOfLading
		{
			get
			{
				var result = new List<IDaeBillOfLading>();
				foreach (AsycudaBill bill in Bills)
				{
					result.Add(new DAEAmendBillWrapper(bill, HeaderChange, manifestSent));
				}
				return result;
			}
		}

		ZBool HeaderChange
		{
			get
			{
				return manifestSent != null && (manifestSent.HeaderInfo.DateOfArrival != Manifest.DateOfArrival
					|| manifestSent.HeaderInfo.ShippingProviderDoc != Manifest.ShippingProviderDoc
					|| manifestSent.HeaderInfo.PlaceDepartureCode != Manifest.PlaceDepartureCode
					|| manifestSent.HeaderInfo.PlaceOfArrivalCode != Manifest.PlaceOfArrivalCode);
			}
		}
	}
}
