using CefSharp;

namespace SharpBrowser {
	internal class LifeSpanHandler : ILifeSpanHandler {
		MainForm myForm;

		public LifeSpanHandler(MainForm form) {
			myForm = form;
		}


		public bool DoClose(IWebBrowser browserControl, IBrowser browser) {
			return false;
		}
		
		public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser) {
		}
		
		public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser) {
		}
		
		public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser) {

			newBrowser = null;
			myForm.AddNewBrowserTab(targetUrl);

			return true;

		}
	}
}