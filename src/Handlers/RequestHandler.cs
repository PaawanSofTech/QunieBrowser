using System;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using CefSharp;

namespace SharpBrowser {
	internal class RequestHandler : IRequestHandler {
		MainForm myForm;

		public RequestHandler(MainForm form) {
			myForm = form;
		}

		public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback) {
			
			return false;
		}

		public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect) {
			return false;
		}
		
		public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback) {
			return true;
		}
		
		public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture) {
			return false;
		}
		
		public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath) {
		}

		public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback) {
			callback.Continue(true);
			return true;
		}
		
		public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status) {
		}
		
		public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser) {
		}

		public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling) {
			var rh = new ResourceRequestHandler(myForm);
			return rh;
		}
		
		public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback) {
			return false;
		}

		public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser) {

		}



	}
}