using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class DAEWrapper : IDaeDeclaration
	{
		public DAEWrapper(AsycudaManifestHeader header, AsycudaBill[] bills)
		{
			Header = Argument.NotNull(header, "asycudaManifestHeader cannot be null");
			Bills = bills;
		}
		protected readonly AsycudaManifestHeader Header;
		protected readonly AsycudaBill[] Bills;

		string IDaeDeclaration.DocumentType => ConsigneeDocumentTypes.TaxID;

		string IDaeDeclaration.DocumentID => GlbCompany.CurrentCompany.GC_BusinessRegNo;

		DateTime IDaeDeclaration.DocumentDate => SafeDateTime(Env.Time.CurrentLocalDateTime);

		string IDaeDeclaration.InterchangeCode => AsycudaBill.UYConstants.InterchangeCode;

		string IDaeDeclaration.TransactionNo => UYMessage.MessageNumberPlaceHolder;

		IReadOnlyCollection<IManifest> IDaeDeclaration.Manifests => new List<IManifest> { new DAEManifestWrapper(Header, Bills) };

		static DateTime SafeDateTime(ZDateTime dateTime) => dateTime.IsValid ? Convert.ToDateTime(dateTime.ToString("yyyy-MM-ddTHH:mm:ss")) : default;
	}
}
