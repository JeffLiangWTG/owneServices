using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderManufacturerAddresses : ActiveBusinessObjectCollection<ISFDocAddress>
	{
		public CusISFHeaderManufacturerAddresses(CusISFHeader header)
			: base(header.Factory, header, GetManufacturerFilter(header.Factory), JobDocAddressSchema.E2_ParentID)
		{
		}

		static ZQuery GetManufacturerFilter(BusinessObjectFactory factory)
		{
			ZQuery result = new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(factory, DocAddressType.Manufacturer));
			result.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
			return result;
		}

		public CusISFHeader Master
		{
			get { return (CusISFHeader)Relationship.Master; }
		}

		public bool IsEmpty
		{
			get
			{
				bool result = true;
				foreach (ISFDocAddress docAddress in this)
				{
					if (!docAddress.IsEmpty)
					{
						result = false;
						break;
					}
				}
				return result;
			}
		}

		protected override void SetDefaultsForNewElementCore(ISFDocAddress newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.E2_GovRegNumType = CodeTypeList.Codes.DUNS;
			newElement.E2_AddressSequence = GetNextSequence();
		}

		ZByte GetNextSequence()
		{
			ZByte highestSequence = 0;
			var listOfSequences = new List<ZByte>();
			foreach (ISFDocAddress docAddress in this)
			{
				if (!listOfSequences.Contains(docAddress.E2_AddressSequence))
				{
					listOfSequences.Add(docAddress.E2_AddressSequence);
				}
				if (docAddress.E2_AddressSequence > highestSequence)
				{
					highestSequence = docAddress.E2_AddressSequence;
				}
			}
			ZByte nextSequence = ZByte.Zero;
			for (; nextSequence <= highestSequence; nextSequence++)
			{
				if (!listOfSequences.Contains(nextSequence) || nextSequence == byte.MaxValue)
				{
					break;
				}
			}
			return nextSequence;
		}

		protected override void OnLoadedIntoCollectionCore(ISFDocAddress loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.OverrideRequirement = Master.ISFDocAddressRequirementProvider.ManufacturerDocAddressRequirement;
			loadedObject.DefaultAddressType = ZArchitecture.Business.AddressType.NoDefault;
		}

		protected override void SetRelationshipDefaultsForElementCore(ISFDocAddress newElement, bool throwIfRelationshipNotSupported)
		{
			using (newElement.SuspendSettingHasChanges())
			{
				base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
				newElement.E2_AddressType = DocAddressTypes.GetCode(newElement.Factory, DocAddressType.Manufacturer);
				newElement.E2_ParentTableCode = Master.TablePrefix;
			}
		}

		protected override void EndNew(int index)
		{
			base.EndNew(index);
			if (index >= 0 && index < Count)
			{
				ISFDocAddress address = this[index];
				if (address != null && !address.IsDeleted)
				{
					if (Master.fDocAddresses != null && !Master.fDocAddresses.Contains(address))
					{
						Master.fDocAddresses.Add(address);
						address.CountryCodeChanged += Master.Manufacturer_CountryCodeChanged;
					}
				}
			}
		}

		protected override void OnAdded(ISFDocAddress businessObject)
		{
			base.OnAdded(businessObject);
			if (!IsNonCommittedElement(businessObject) && Master.fDocAddresses != null && !Master.fDocAddresses.Contains(businessObject))
			{
				Master.fDocAddresses.Add(businessObject);
			}
		}

		public override void Delete(ISFDocAddress docAddress)
		{
			var header = this.Master;
			foreach (var line in header.Lines)
			{
				if (line.BL_ManufacturerDocAddressPK == docAddress.PK)
				{
					line.BL_ManufacturerDocAddressPK = ZGuid.Empty;
				}
			}
			base.Delete(docAddress);

			JobDocAddress.ReinitializeAddressSequenceNumber(this);
		}
	}
}
