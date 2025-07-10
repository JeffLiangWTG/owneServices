using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class BondedFactoryCollection : ActiveBusinessObjectCollection<BondedFactory>
	{
		public BondedFactoryCollection(JobDeclaration declaration)
			: base(declaration.Factory,
						declaration,
						new ZQuery(new ZQuery(JobDocAddressSchema.E2_ParentTableCode, declaration.TablePrefix), new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BondedFactory)),
						JobDocAddressSchema.E2_ParentID)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		protected override void SetDefaultsForNewElementCore(BondedFactory newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.E2_AddressSequence = GetNextSequence();
		}

		protected override void SetRelationshipDefaultsForElementCore(BondedFactory newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.E2_AddressType = DocAddressTypes.Codes.BondedFactory;
		}

		protected override bool AllowNew => declaration.IsImport ? Count < GetMaximumNumberOfImportBondedFactories()
												: Count < GetMaximumNumberOfExportBondedFactories();

		ZByte GetNextSequence()
		{
			ZByte highestSequence = 0;
			var listOfSequences = new List<ZByte>();
			foreach (var docAddress in this)
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

		public override void Delete(BondedFactory businessObject)
		{
			base.Delete(businessObject);
			JobDocAddress.ReinitializeAddressSequenceNumber(this);
		}

		internal static int GetMaximumNumberOfImportBondedFactories() => 9;
		internal static int GetMaximumNumberOfExportBondedFactories() => 10;
	}
}
