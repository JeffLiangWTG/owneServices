using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Documents;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class DocumentUploadSupport : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string DocType = "DocType";
		}

		#endregion

		#region Constructors

		public DocumentUploadSupport(BusinessObjectFactory factory)
				: base(factory)
		{
		}

		public DocumentUploadSupport(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		#endregion

		#region Properties

		[CargoWise.ComponentModel.MaxLength(AutoStorageDocs.Schema.SC_DocTypeMaxLength)]
		public ZString DocType
		{
			get
			{
				return docType;
			}
			set
			{
				if (docType != value)
				{
					CheckMaximumLength(DocTypeInfo, value);
					docType = value;
					DocTypeInfo.RefreshBinding();
				}
			}
		}
		ZString docType;

		public ZPropertyInfo DocTypeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.DocType);
			}
		}

		public RefDocTypeCollection DocTypes => GetDocTypes();

		protected virtual RefDocTypeCollection GetDocTypes()
		{
			RefDocTypeCollection result;
			var query = new ZDBOnlyQuery(typeof(RefDocType));

			var queryOr = new ZDBOnlyQuery(typeof(RefDocType));
			queryOr.AddToFilter(RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.All);
			queryOr.AddToFilter(JoinCondition.Or, RefDocTypeSchema.RT_ReferenceType, Core.Constants.ReferenceTypes.SupplyChainLogistics);

			query.AddToFilter(queryOr, JoinCondition.And);
			query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_IsActive, ZBool.True);
			query.AddToFilter(JoinCondition.And, RefDocTypeSchema.RT_IsPublished, ZBool.True);

			var siteUser = WebEnv.AppInstance?.SiteUser as TrackingSiteUser;
			if (siteUser != null)
			{
				result = new PermittedRefDocTypeCollection(Factory, query, siteUser);
			}
			else
			{
				result = new RefDocTypeCollection(Factory, query);
			}

			result.ApplySort(AutoRefDocType.Schema.RT_DocType, System.ComponentModel.ListSortDirection.Ascending);
			return result;
		}

		#endregion
	}
}
