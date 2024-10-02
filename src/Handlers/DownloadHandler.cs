using CefSharp;

namespace SharpBrowser {
	internal class DownloadHandler : IDownloadHandler {
		readonly MainForm myForm;

		public DownloadHandler(MainForm form) {
			myForm = form;
		}

		public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod) {
			return true;
		}

		
		public void OnBeforeDownload(IWebBrowser webBrowser, IBrowser browser, DownloadItem item, IBeforeDownloadCallback callback) {
			if (!callback.IsDisposed) {
				using (callback) {

					myForm.UpdateDownloadItem(item);

					string path = myForm.CalcDownloadPath(item);

					if (path == null) {

						callback.Continue(path, false);

					}
					else {

						myForm.OpenDownloadsTab();
						callback.Continue(path, true);
					}

				}
			}
		}

		public void OnDownloadUpdated(IWebBrowser webBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback) {
			myForm.UpdateDownloadItem(downloadItem);
			if (downloadItem.IsInProgress && myForm.CancelRequests.Contains(downloadItem.Id)) {
				callback.Cancel();
			}
			//Console.WriteLine(downloadItem.Url + " %" + downloadItem.PercentComplete + " complete");
		}
	}
}