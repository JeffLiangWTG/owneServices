using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DeclarationDuplicateCollection))]
	sealed class DeclarationDuplicateCollectionTest : CusCodeDataCollectionTest<DeclarationDuplicate>
	{
		protected override CusCodeDataCollection<DeclarationDuplicate> GetCusCodeDataCollection()
		{
			return new DeclarationDuplicateCollection(CusEntryInstruction);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<DeclarationDuplicate>();
			result.CY_ParentID = CusEntryInstruction.PK;
			result.CY_ParentTableCode = CusEntryInstruction.TablePrefix;
			return result;
		}

		CusEntryInstruction CusEntryInstruction
		{
			get
			{
				return cusEntryInstruction ?? (cusEntryInstruction = Factory.New<CusEntryInstruction>());
			}
		}

		CusEntryInstruction cusEntryInstruction;
	}
}
