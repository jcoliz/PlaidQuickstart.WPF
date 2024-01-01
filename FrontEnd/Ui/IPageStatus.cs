namespace FrontEnd.Ui;

/// <summary>
/// Defines how the Link web page reports its status to the native code
/// </summary>
internal interface IPageStatus
{
    /// <summary>
    /// Report that the page is loading
    /// </summary>
    void LinkLoading();

    /// <summary>
    /// Report that the operation is running 
    /// </summary>
    void LinkRunning();

    /// <summary>
    /// Report that the operation has completed successfully
    /// </summary>
    void LinkSuccess();

    /// <summary>
    /// Report that the operation has failed
    /// </summary>
    /// <param name="reason">Information about what went wrong</param>
    void LinkFailed(string reason);
}
