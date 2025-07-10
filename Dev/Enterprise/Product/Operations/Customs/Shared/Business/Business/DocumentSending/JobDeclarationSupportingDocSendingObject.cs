using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public partial class JobDeclarationSupportingDocSendingObject : SupportingDocSendingObject
	{
		public JobDeclarationSupportingDocSendingObject(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected BaseJobDeclaration Declaraction => SupportingDocObject as BaseJobDeclaration;

		#region New Properties

		public CusEntryHeader Header => GetEntryHeader();

		protected CusEntryHeader header;

		protected virtual CusEntryHeader GetEntryHeader()
		{
			if (LocalReferenceNumber.IsEmpty)
			{
				header = null;
			}
			else if (header == null || header.MovementReferenceNumber != LocalReferenceNumber)
			{
				header = Declaraction?.ActiveEntryHeaders?.Cast<CusEntryHeader>().FirstOrDefault(x => x.MovementReferenceNumber == LocalReferenceNumber);
			}
			return header;
		}

		#endregion

		#region Lookup Lists

		public override CodeDescriptionPairList Entries
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (CusEntryHeader entryHeader in ValidEntryHeaders)
				{
					var reference = !entryHeader.MovementReferenceNumber.IsEmpty ? entryHeader.MovementReferenceNumber : entryHeader.CH_BGMReference;
					var refType = !entryHeader.MovementReferenceNumber.IsEmpty ? CusEntryNumberTypes.Standard.MovementReferenceNumber : "BGM";
					result.Add(new CodeDescriptionPair(reference.ToString(), Res.GetString("EC773E09-AD53-48F6-B66B-33024D74E437", "{0}: {1}", refType, reference)));
				}
				return result;
			}
		}

		protected override IEnumerable<IStorageDocsBaseCollection> AllEDocsList
		{
			get
			{
				var docManagerSupports = (Header as IDocManagerSupportProvider)?.DocManagerSupports.ToList() ?? new List<IDocManagerSupport>();
				if (!docManagerSupports.Contains(SupportingDocObject))
				{
					docManagerSupports.Add(SupportingDocObject);
				}
				if (Header != null)
				{
					docManagerSupports.Add(Header);
				}

				return EDocsHelper.GetEDocCollections(docManagerSupports.ToArray());
			}
		}

		IEnumerable<CusEntryHeader> ValidEntryHeaders => Declaraction?.ActiveEntryHeaders.OfType<CusEntryHeader>().Where(x => !x.MovementReferenceNumber.IsEmpty || !x.CH_BGMReference.IsEmpty) ?? Enumerable.Empty<CusEntryHeader>();

		#endregion

		#region Implementation

		protected override void DefaultLocalReferenceNumber()
		{
			LocalReferenceNumber = Entries.Count == 1 ? Entries[0]?.Code : string.Empty;
		}

		#endregion
	}
}
