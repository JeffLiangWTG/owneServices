using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GenRegCertAccredMaintListCollection : ActiveBusinessObjectCollection<GenRegCertAccredMaintList>
	{
		public GenRegCertAccredMaintListCollection(BusinessObject parent)
			: base(parent.Factory, parent, new ZQuery(GenRegCertAccredMaintListSchema.XZ_ParentTableCode, parent.TablePrefix), GenRegCertAccredMaintListSchema.XZ_ParentID)
		{
			this.parent = parent;
		}

		public GenRegCertAccredMaintListCollection(BusinessObjectFactory factory, BusinessObject parent)
			: base(factory)
		{
			this.parent = parent;
		}

		public ZString GetFirstCertificateNumber(ZString type)
		{
			var certificate = GetFirstCertificate(type);
			return certificate == null ? ZString.Empty : certificate.XZ_RefNumber;
		}

		public ZString GetFirstCertificateNumber(ZString type, ZDateTime expiry)
		{
			var certificate = GetFirstCertificate(type, expiry);
			return certificate == null ? ZString.Empty : certificate.XZ_RefNumber;
		}

		public GenRegCertAccredMaintList GetFirstCertificate(ZString type)
		{
			return GetFirstCertificate(type, ZDateTime.Empty);
		}

		public GenRegCertAccredMaintList GetFirstCertificate(ZString type, ZDateTime expiry)
		{
			GenRegCertAccredMaintList result = null;

			foreach (var item in this)
			{
				if (item.XZ_Type != type)
				{
					continue;
				}

				if (result == null || EmptyAsMin(item.XZ_ExpiryOrDueDate) > EmptyAsMin(result.XZ_ExpiryOrDueDate))
				{
					result = item;
				}
			}

			if (expiry.IsEmpty || result != null && EmptyAsMax(result.XZ_ExpiryOrDueDate) >= expiry)
			{
				return result;
			}

			return null;
		}

		public bool IsDuplicated(ZString type)
		{
			bool found = false;
			foreach (var item in this)
			{
				if (item.XZ_Type == type)
				{
					if (found)
					{
						return true;
					}
					else
					{
						found = true;
					}
				}
			}

			return false;
		}

		#region Implementation

		ZDateTime EmptyAsMin(ZDateTime value)
		{
			if (value.IsEmpty)
			{
				return ZDateTime.MinSmallDateTimeValue;
			}

			return value;
		}

		ZDateTime EmptyAsMax(ZDateTime value)
		{
			if (value.IsEmpty)
			{
				return ZDateTime.MaxSmallDateTimeValue;
			}

			return value;
		}

		protected override void SetRelationshipDefaultsForElementCore(GenRegCertAccredMaintList newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);

			newElement.XZ_ParentTableCode = this.parent.TablePrefix;
			newElement.XZ_ParentID = this.parent.PK;
			newElement.MasterParent = this.parent as ICertificatesProvider;
		}

		protected override void SetDefaultsForNewElementCore(GenRegCertAccredMaintList newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.XZ_ParentTableCode = this.parent.TablePrefix;
			newElement.XZ_ParentID = this.parent.PK;
			newElement.MasterParent = this.parent as ICertificatesProvider;
		}

		protected override void OnLoadedIntoCollectionCore(GenRegCertAccredMaintList loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);

			loadedObject.MasterParent = this.parent as ICertificatesProvider;
		}

		readonly BusinessObject parent;

		#endregion
	}
}
