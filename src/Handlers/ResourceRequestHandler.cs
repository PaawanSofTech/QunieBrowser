
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CefSharp;
using System.Windows.Forms;
using System.Drawing;
using CefSharp.Callback;
using SharpBrowser.Browser;

namespace SharpBrowser {
	internal class ResourceRequestHandler : IResourceRequestHandler {
		readonly MainForm myForm;
		public ResourceRequestHandler(MainForm form) {
			myForm = form;
		}
		public void Dispose() {

		}

		public ICookieAccessFilter GetCookieAccessFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request) {
			return null;
		}
		
		public IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request) {
			
			return null;
		}
		
		public IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
			return null;
		}
		
		public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback) {

			var tab = myForm.GetTabByBrowser(chromiumWebBrowser);
			if (tab != null && tab.RefererURL != null) {

				request.SetReferrer(tab.RefererURL, ReferrerPolicy.Default);

			}

			return CefSharp.CefReturnValue.Continue;
		}
		
		public bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request) {
			return true;
		}
		
		public void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength) {

			int code = response.StatusCode;

			if (!frame.IsValid) {
				return;
			}

			if (code == 404) {

				if (!request.Url.IsURLLocalhost()) {

					frame.LoadUrl("http://web.archive.org/web/*/" + request.Url);

				}
				else {

					frame.LoadUrl(BrowserConfig.FileNotFoundURL + "?path=" + request.Url.EncodeURL());
				}

			}

			else if (request.Url.IsURLOfflineFile()) {
				string path = request.Url.FileURLToPath();
				if (path.FileNotExists()) {

					frame.LoadUrl(BrowserConfig.FileNotFoundURL + "?path=" + path.EncodeURL());

				}
			}
			else {


				if (code == 444 || (code >= 500 && code <= 599)) {

					frame.LoadUrl(BrowserConfig.CannotConnectURL);
				}

			}

		}
		
		public void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl) {
		}
		
		public bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response) {
			
			return false;

		}

	}
}
