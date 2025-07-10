namespace Enterprise.Customs.Business.SADH
{
	public interface ISADHFormDataManager
	{
		SADHFormData FormData { get; }
		void WriteData();
		bool ExecutedSuccessfully { get; set; }
	}
}
