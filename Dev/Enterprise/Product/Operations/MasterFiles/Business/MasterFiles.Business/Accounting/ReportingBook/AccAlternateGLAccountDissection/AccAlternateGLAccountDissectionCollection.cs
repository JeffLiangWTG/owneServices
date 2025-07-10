using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateGLAccountDissectionCollection : DependentBusinessObjectCollection<AccAlternateGLAccountDissection, AccGLHeader>
	{
		public AccAlternateGLAccountDissectionCollection(AccGLHeader gLHeader) : base(gLHeader)
		{
		}

		protected override string FkColumnName => AccAlternateGLAccountDissectionSchema.ADC_AG_GLHeader.Name;
	}
}
