using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Organisation.OrgImport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public abstract class SimpleModuleDataTransferProcessor<Header, Flattened> : MergeFlattenedDataTransferProcessor<Header, Flattened>
		where Header : BusinessObject
		where Flattened : BusinessObject
	{
		protected SimpleModuleDataTransferProcessor(IBusinessObjectCollection headerCollection, IImportCollectionInfo flattenedImportCollectionInfo)
			: base(headerCollection, flattenedImportCollectionInfo)
		{
			this.Factory = headerCollection.Factory;
		}

		protected BusinessObjectFactory Factory { get; private set; }

		public override void Import()
		{
			flattenedCollection.Factory.ActivateStringInterning();

			base.Import();

			try
			{
				if (!IsCanceled)
				{
					Factory.Save();
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		protected override string GetProgressChangedStatus(int recordsProcessed)
		{
			return Res.GetString("faddab4f-23bb-4a10-93e9-ed899fb4c2b2", "Importing records ({0} of {1}) ...", recordsProcessed, flattenedCollection.Count);
		}

		protected override IEnumerable<string> HeaderColumnsOnFlattened
		{
			get { return headerColumns ?? (headerColumns = flattenedImportCollectionInfo.Properties.Select(property => property.MappingName)); }
		}

		IEnumerable<string> headerColumns;

		protected IEnumerable<PropertyInfo> FlattenedProperties
		{
			get { return flattenedProperties ?? (flattenedProperties = typeof(Flattened).GetProperties().Where(x => !x.Name.EndsWith((NoResString)"Info", StringComparison.OrdinalIgnoreCase))); }
		}

		IEnumerable<PropertyInfo> flattenedProperties;

		protected Header GetHeader(ZString uniqueReference, Func<Header, string> uniqueReferenceFunc)
		{
			Header header = null;
			if (!uniqueReference.IsEmpty)
			{
				header = headersCreated.SingleOrDefault(x => uniqueReferenceFunc(x) == uniqueReference);
				if (header != null)
				{
					Log += Res.GetString("7f9d8d47-6b66-4e38-bcd3-055868dcf3aa", "Updating Job [Ref: {0}]", uniqueReference) + System.Environment.NewLine;
				}
			}

			if (header == null)
			{
				header = (Header)headerCollection.AddNew();
				headersCreated.Add(header);
				SetupNewHeader(header);

				Log += Res.GetString("d9050ae2-44f8-4d5f-afa0-1062eaf3e83c", "Creating Job [Ref: {0}]", uniqueReference) + System.Environment.NewLine;
			}

			return header;
		}

		readonly List<Header> headersCreated = new List<Header>();

		protected virtual void SetupNewHeader(Header header)
		{
		}

		protected void CopyIdenticallyNamedProperties(BusinessObject record, BusinessObject flattenedRecord, string prefix, string prefixToRemove = "")
		{
			foreach (var propertyInfo in FlattenedProperties.Where(x => x.Name.StartsWith(prefix + "_", StringComparison.OrdinalIgnoreCase)))
			{
				var value = (IZType)propertyInfo.GetValue(flattenedRecord);
				if (!value.IsEmpty)
				{
					var propertyName = propertyInfo.Name;
					if (!string.IsNullOrEmpty(prefixToRemove))
					{
						propertyName = propertyName.Replace(prefixToRemove + "_", string.Empty);
					}

					record.GetType().GetProperty(propertyName).SetValue(record, value);
				}
			}
		}

		protected void SetOrgReference(ZString orgCode, Action<OrgHeader> setAction)
		{
			if (!orgCode.IsEmpty)
			{
				var result = GetOrgHeaderFromCode(orgCode);
				if (result != null)
				{
					setAction(result);
				}
			}
		}

		protected void SetJobDocAddress(string prefix, BusinessObject flattenedRecord, JobDocAddress docAddress)
		{
			var address = GetAddress(flattenedRecord, prefix);
			if (address != null)
			{
				docAddress.E2_AddressOverride = false;
				docAddress.E2_OA_Address = address.PK;
			}

			if (docAddress.E2_OA_Address.IsEmpty)
			{
				foreach (var propertyInfo in FlattenedProperties.Where(x => x.Name.StartsWith(prefix + "Address_", StringComparison.OrdinalIgnoreCase) && x.Name.IndexOf("OH_Code", StringComparison.OrdinalIgnoreCase) < 0))
				{
					var value = (IZType)propertyInfo.GetValue(flattenedRecord);
					if (!value.IsEmpty)
					{
						if (!docAddress.E2_AddressOverride)
						{
							docAddress.E2_AddressOverride = true;
						}
						docAddress.GetType().GetProperty(propertyInfo.Name.Replace(prefix + "Address_", string.Empty)).SetValue(docAddress, value);
					}
				}
			}
		}

		protected OrgAddress GetAddress(BusinessObject flattenedRecord, string prefix, bool fallbackToMainAddress = false)
		{
			OrgAddress result = null;

			var orgCode = (ZString)flattenedRecord.GetType().GetProperty(prefix + "Address_OH_Code").GetValue(flattenedRecord);
			if (!orgCode.IsEmpty)
			{
				var org = GetOrgHeaderFromCode(orgCode);
				if (org != null)
				{
					var address1 = (ZString)flattenedRecord.GetType().GetProperty(prefix + "Address_E2_Address1").GetValue(flattenedRecord);
					var address2 = (ZString)flattenedRecord.GetType().GetProperty(prefix + "Address_E2_Address2").GetValue(flattenedRecord);
					var city = (ZString)flattenedRecord.GetType().GetProperty(prefix + "Address_E2_City").GetValue(flattenedRecord);
					var state = (ZString)flattenedRecord.GetType().GetProperty(prefix + "Address_E2_State").GetValue(flattenedRecord);
					var postcode = (ZString)flattenedRecord.GetType().GetProperty(prefix + "Address_E2_Postcode").GetValue(flattenedRecord);

					if (address1.IsEmpty && address2.IsEmpty && city.IsEmpty && state.IsEmpty && postcode.IsEmpty)
					{
						result = org.MainAddress;
					}
					else
					{
						result = org.Addresses.ToArray<OrgAddress>().FirstOrDefault(x =>
							x.OA_Address1 == address1 &&
							x.OA_Address2 == address2 &&
							x.OA_City == city &&
							x.OA_State == state &&
							x.OA_PostCode == postcode);

						if (result == null && fallbackToMainAddress)
						{
							result = org.MainAddress;
						}
					}
				}
			}

			return result;
		}

		protected OrgHeader GetOrgHeaderFromCode(string code)
			=> Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code)
				?? new OrgHeader.Loader(Factory).LoadFromLegacyCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, code)
				?? new OrgHeader.Loader(Factory).LoadFromLegacyCode(string.Empty, code);
	}
}
