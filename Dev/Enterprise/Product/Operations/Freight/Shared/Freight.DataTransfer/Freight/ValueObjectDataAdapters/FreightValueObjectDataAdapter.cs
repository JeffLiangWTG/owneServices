using System;
using CargoWise.Application;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public abstract class FreightValueObjectDataAdapter<TBusinessObject, TValueObject> : ValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : BusinessObject
		where TValueObject : IValueObject
	{
		protected override bool ShouldUpdateExistingObject(TBusinessObject bizObj, INotifications notifications)
		{
			return IsBatchJob ? RegistryDefaultForImporting : base.ShouldUpdateExistingObject(bizObj, notifications);
		}

		protected bool IsBatchJob
		{
			get { return Env.CurrentUser.IsBatchProcessor; }
		}

		protected abstract bool RegistryDefaultForImporting
		{
			get;
		}

		protected SystemDataRegistry SystemRegistry
		{
			get { return SystemDataRegistry.Instance; }
		}

		protected virtual IValueObjectDataAdapter CreateStorateDocsDataAdapter()
		{
			return (IValueObjectDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<DocumentScanning.Integration.IStorageDocsValueObjectDataAdapter>());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "file extenstion")]
		protected void ImporteDocs(BusinessObject bizObj, Xsd.DocumentCollection documents, IValueObjectImportContext importContext)
		{
			Type sdAdapterType = TypeDecider.GetTypeForBinding(ObjectFactory.GetType<DocumentScanning.Integration.IStorageDocsValueObjectDataAdapter>());
			Type sfAdapterType = TypeDecider.GetTypeForBinding(ObjectFactory.GetType<DocumentScanning.Integration.IStorageFilesValueObjectDataAdapter>());
			IValueObjectDataAdapter storageDocsDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(sdAdapterType, new object[] { bizObj });
			IValueObjectDataAdapter storageFilesDataAdapter = (IValueObjectDataAdapter)Activator.CreateInstance(sfAdapterType, new object[] { bizObj });

			foreach (Xsd.Document document in documents)
			{
				if (document.DataType.ToLower() == "pdf")
				{
					storageFilesDataAdapter.CreateOrUpdateFromValueObject(document, importContext);
				}
				else
				{
					storageDocsDataAdapter.CreateOrUpdateFromValueObject(document, importContext);
				}
			}
		}

		#region Implementation

		internal OrganisationValueObjectDataAdapter GetNewOrganisationValueObjectDataAdapter(TBusinessObject bizObj)
		{
			return GetNewOrganisationValueObjectDataAdapterCore(bizObj);
		}

		protected virtual OrganisationValueObjectDataAdapter GetNewOrganisationValueObjectDataAdapterCore(TBusinessObject bizObj)
		{
			if (bizObj is IDocAddresses)
			{
				return new OrganisationValueObjectDataAdapter(bizObj as IDocAddresses);
			}
			else
			{
				return new OrganisationValueObjectDataAdapter();
			}
		}

		#endregion
	}
}
