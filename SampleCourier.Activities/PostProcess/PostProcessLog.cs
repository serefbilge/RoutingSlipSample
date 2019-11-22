namespace SampleCourier.Activities.PostProcess
{
	public interface PostProcessLog
	{
		/// <summary>
		/// The path where the content was saved
		/// </summary>
		string LocalFilePath { get; }
	}
}