using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFHeaderExtraShipToPartyAddresses : ActiveBusinessObjectCollection<ISFDocAddress>
	{
		public CusISFHeaderExtraShipToPartyAddresses(CusISFHeader header)
			: base(header.Factory, header, GetExtraShipToPartyFilter(header.Factory), JobDocAddressSchema.E2_ParentID)
		{
		}

		static ZQuery GetExtraShipToPartyFilter(BusinessObjectFactory factory)
		{
			ZQuery result = new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(factory, DocAddressType.ShipToParty));
			result.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusISFHeaderSchema.Constants.Prefix);
			result.AddToFilter(JobDocAddressSchema.E2_AddressSequence, SQLComparisonOperator.NotEqual, ZByte.Zero);
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
			newElement.E2_GovRegNumType = CodeTypeList.Codes.FIRMS;
			int sequence = 1;
			foreach (ISFDocAddress docAddress in this)
			{
				docAddress.E2_AddressSequence = ZByte.ParseSafe(sequence.ToString(), 0);
				sequence++;
			}
			newElement.E2_AddressSequence = ZByte.ParseSafe(sequence.ToString(), 0);
		}

		protected override void OnLoadedIntoCollectionCore(ISFDocAddress loadedObject)
		{
			base.OnLoadedIntoCollectionCore(loadedObject);
			loadedObject.OverrideRequirement = Master.ISFDocAddressRequirementProvider.ShipToPartyDocAddressRequirement;
		}

		protected override void SetRelationshipDefaultsForElementCore(ISFDocAddress newElement, bool throwIfRelationshipNotSupported)
		{
			using (newElement.SuspendSettingHasChanges())
			{
				base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
				newElement.E2_AddressType = DocAddressTypes.GetCode(newElement.Factory, DocAddressType.ShipToParty);
				newElement.E2_ParentTableCode = Master.TablePrefix;
			}
		}

		protected override void EndNew(int index)
		{
			base.EndNew(index);
			if (index != -1 && index < this.Count)
			{
				ISFDocAddress address = this[index];
				if (Master.fDocAddresses != null && !Master.fDocAddresses.Contains(address))
				{
					Master.fDocAddresses.Add(address);
				}
			}
		}

		protected override void OnAdded(ISFDocAddress businessObject)
		{
			base.OnAdded(businessObject);
			if (!IsNonCommittedElement(businessObject) && Master.fDocAddresses != null && !Master.fDocAddresses.Contains(businessObject))
			{
				if (businessObject.IsDeleted)
				{
#if DEBUG
					if (ZArchitecture.Environment.Globals.IsTest) // such as ISFForm bashing tests
					{
						return;
					}
#endif
					ErrorReporter.ReportOnce("ISFDocAddress row has already been removed"); // TODO: remove when solved
				}

				Master.fDocAddresses.Add(businessObject);
			}
		}
	}
}
