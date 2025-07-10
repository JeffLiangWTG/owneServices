using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefGlbReleaseNoteVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefGlbReleaseNote()
				{
					ZGF_PK = Guid.NewGuid(),
					ZGF_IsValid = false,
					ZGF_Category = "Cat",
					ZGF_Summary = "Summary",
					ZGF_URL = "http://www.example.com",
					ZGF_ReleaseNoteDate = DateTime.Now,
					ZGF_Section = "BOR",
					ZGF_QuickStartPK = Guid.NewGuid()
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefGlbReleaseNote)data).ZGF_Category = "Ca1";
			return true;
		}
	}
}
