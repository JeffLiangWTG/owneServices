using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryInstructionAsTypeSupporter : CusEntryInstruction, ICusStorageDocPivotTypeSupporter
	{
		public CusEntryInstructionAsTypeSupporter(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		Type ICusStorageDocPivotTypeSupporter.CusStorageDocPivotType => typeof(CusStorageDocPivotForTest);

		void ICusStorageDocPivotTypeSupporter.ReloadCollection()
		{
		}

		IEnumerable<IStorageDocsBaseCollection> ICusStorageDocPivotTypeSupporter.EDocCollections
		{
			get
			{
				var declaration = JobDeclaration;
				if (declaration != null)
				{
					foreach (var eDocCollection in EDocsHelper.GetEDocCollections(declaration))
					{
						yield return eDocCollection;
					}
				}
			}
		}
	}
}
