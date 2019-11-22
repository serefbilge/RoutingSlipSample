namespace SampleCourier.ProcessingService.Activities.PreProcess
{
	public interface PreProcessLog
	{
		/// <summary>
		/// The path where the content was saved
		/// </summary>
		string LocalFilePath { get; }
	}
}