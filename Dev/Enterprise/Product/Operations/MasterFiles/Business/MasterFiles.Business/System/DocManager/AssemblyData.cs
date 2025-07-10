using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Modules.DocumentScanning
{
	/// <summary>
	/// Represents a single entity that documents can be assigned to - Document Scanning / eDocs.
	/// </summary>
	public abstract class AssemblyData
	{
		/// <summary>
		/// BusinessObject type associated with this code. 
		/// Must have a Code property marked with [CodeProperty(BizO.Schema.XXXX)] at the top of the class.
		/// The BusinessObject must implement Enterprise.MasterFiles.Business.IDocManagerSupport
		/// </summary>
		public abstract Type BusinessObjectType { get; }

		/// <summary>
		/// This sets the reference type that this business object belongs to, and is used to determine which set of doc types should be loaded,
		/// based on RefDocType.RT_ReferenceType.
		/// </summary>
		public abstract string ReferenceType { get; }

		/// <summary>
		/// Collection to use for the findbox on the Allocate eDocs form. Must be non dependent with a constructor to accept a factory only
		/// </summary>
		public virtual IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			isLoadingWithEmptyAssemblyDataParams = true;
			try
			{
				return GetBusinessObjectCollection(factory, new AssemblyDataParams());
			}
			finally
			{
				isLoadingWithEmptyAssemblyDataParams = false;
			}
		}

		bool isLoadingWithEmptyAssemblyDataParams;

		public virtual IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory, AssemblyDataParams assemblyDataParams)
		{
			var collectionType = CollectionType;
			if (collectionType != null)
			{
				if (!string.IsNullOrEmpty(assemblyDataParams.CompanyCode))
				{
					var company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, assemblyDataParams.CompanyCode);
					if (company != null)
					{
						try
						{
							return (IBusinessObjectCollection)Activator.CreateInstance(collectionType, factory, company);
						}
						catch (MissingMethodException)
						{
						}
					}
				}
				return isLoadingWithEmptyAssemblyDataParams ? null : GetBusinessObjectCollection(factory);
			}
			else
			{
				return null;
			}
		}

		protected abstract Type CollectionType { get; }

		/// <summary>
		/// ModuleID used for the findbox on the Allocate eDocs form
		/// </summary>
		public virtual ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		/// <summary>
		/// Whether or not this data should be visible on the Allocate eDocs form so that users can allocate files to the business object.
		/// </summary>
		public virtual bool IsAllowedForUnallocatedeDocs
		{
			get { return false; }
		}

		/// <summary>
		/// Human readable name used in drop-down lists
		/// </summary>
		public virtual MultilingualString HumanReadableName { get; private set; }

		/// <summary>
		/// Provides query for selecting business objects for ArchiveEDocsManager to be added to ArchiveCD.
		/// </summary>
		public virtual ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			return null;
		}

		/// <summary>
		/// Overridce and set to true if you intend to use bar codes that contain the PK of your bizO, rather than a serial number. This allows better exact matching of records.  If this is false (default) then the scanning engine will assume that the value in the bar code represents a (single) row in the database that has this value in the column inidicated by the bizO's [CodeProperty] attribute. 
		/// </summary>
		public virtual bool AllowLookupOfBizOFromPk
		{
			get { return false; }
		}

		/// <summary>
		/// Used to export eDocs via Universal XML using a DocManagerCode and unique identifier for a business object.
		/// Any BusinessObject with an AssemblyData implementation has an attribute [CodeProperty(BizO.Schema.XXXX)].
		/// If the code property is a unique DB column you do not need to implement this method. AssemblyDataLookup.GetBusinessObjectFromCode will be used to load the business object.
		/// If the code property is a calculated/non unique property you can implement IEDocsViaUniversalXmlSupport and define how this object is loaded for a given key.
		/// </summary>
		/// <returns></returns>
		public virtual IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport()
		{
			return null;
		}

		public virtual ZString GetFriendlyName(BusinessObject businessObject)
		{
			if (businessObject != null && !businessObject.IsDeleted)
			{
				return HumanReadableName + " " + CodePropertyAttribute.CodeFromBusinessObject(businessObject);
			}
			else
			{
				return HumanReadableName;
			}
		} 
	}
}
