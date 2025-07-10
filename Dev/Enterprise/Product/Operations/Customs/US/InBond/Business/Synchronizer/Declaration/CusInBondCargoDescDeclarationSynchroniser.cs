using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondCargoDescDeclarationSynchroniser : BusinessObjectSynchroniser
	{
		internal CusInBondCargoDescDeclarationSynchroniser(CusInBondCargoDesc destination, Package source)
			: base(destination, source)
		{
		}

		public new CusInBondCargoDesc Destination
		{
			get { return (CusInBondCargoDesc)base.Destination; }
		}

		public new Package Source
		{
			get { return (Package)base.Source; }
		}

		public JobDeclaration DeclarationSource
		{
			get { return Source.Declaration; }
		}

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (!Destination.IsDeleted && Destination.ShouldSynchronise)
			{
				var marksAndNumbersSynchroniser = new FieldSynchroniser(Destination.BY_MarksAndNumbersInfo, GetMarksAndNumbers, GetInfosAffectingMarksAndNumbers);
				marksAndNumbersSynchroniser.Format += MarksAndNumbersSynchroniser_Format;
				Synchronisers.Add(marksAndNumbersSynchroniser);

				Synchronisers.Add(new FieldSynchroniser(Destination.BY_PieceCountInfo, Source.CW_PackQtyInfo));
				Synchronisers.Add(new FieldSynchroniser(Destination.BY_ManifestUnitCodeInfo, Source.CW_PackTypeInfo));
			}
		}

		IZType GetMarksAndNumbers()
		{
			var result = Source.CW_MarksAndNos;
			if (result.IsEmpty)
			{
				var declarationSource = DeclarationSource;
				if (declarationSource != null)
				{
					result = declarationSource.JE_MarksAndNumbers;
				}
			}
			return result.Left(CusInBondCargoDesc.Schema.BY_MarksAndNumbersMaxLength);
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingMarksAndNumbers()
		{
			yield return Source.CW_MarksAndNosInfo;
			var declarationSource = DeclarationSource;
			if (declarationSource != null)
			{
				yield return declarationSource.JE_MarksAndNumbersInfo;
			}
		}

		void MarksAndNumbersSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(ZString))
			{
				e.Value = ((ZString)e.Value).Left(CusInBondCargoDesc.Schema.BY_MarksAndNumbersMaxLength);
			}
		}

		#endregion
	}
}
