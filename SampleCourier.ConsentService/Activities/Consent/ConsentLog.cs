namespace SampleCourier.ConsentService.Activities.Consent
{
	public interface ConsentLog
	{
		/// <summary>
		/// The path where the content was saved
		/// </summary>
		string LocalFilePath { get; }
	}
}