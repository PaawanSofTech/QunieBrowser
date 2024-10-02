
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CefSharp;
using System.Windows.Forms;
using System.Drawing;
using CefSharp.Callback;

namespace SharpBrowser {
	internal class SchemeHandler : IResourceHandler, IDisposable {
		private static string appPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\";

		private string mimeType;
		private Stream stream;
		MainForm myForm;
		private Uri uri;
		private string fileName;

		public SchemeHandler(MainForm form) {
			myForm = form;
		}

		public void Dispose() {

		}

		public bool Open(IRequest request, out bool handleRequest, ICallback callback) {
			uri = new Uri(request.Url);
			fileName = uri.AbsolutePath;

			// if url is blocked
			/*if (...request.Url....) {

				// cancel the request - set handleRequest to true and return false
				handleRequest = true;
				return false;
			}*/

			// if url is browser file
			if (uri.Host == "storage") {
				fileName = appPath + uri.Host + fileName;
				if (File.Exists(fileName)) {
					Task.Factory.StartNew(() => {
						using (callback) {
							FileStream fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
							mimeType = ResourceHandler.GetMimeType(Path.GetExtension(fileName));
							stream = fStream;
							callback.Continue();
						}
					});

					handleRequest = false;
					return true;
				}
			}

			if (uri.Host == "fileicon") {
				Task.Factory.StartNew(() => {
					using (callback) {
						stream = FileIconUtils.GetFileIcon(fileName, FileIconSize.Large);
						mimeType = ResourceHandler.GetMimeType(".png");
						callback.Continue();
					}
				});

				handleRequest = false;
				return true;
			}


			callback.Dispose();

			handleRequest = true;
			return false;
		}
		
		public void GetResponseHeaders(IResponse response, out long responseLength, out string redirectUrl) {

			responseLength = stream != null ? stream.Length : 0; 
			redirectUrl = null;

			response.StatusCode = (int)HttpStatusCode.OK;
			response.StatusText = "OK";
			response.MimeType = mimeType;

			//return stream;
		}
		
		public bool ReadResponse(Stream dataOut, out int bytesRead, ICallback callback) {

			callback.Dispose();

			if (stream == null) {
				bytesRead = 0;
				return false;
			}

			var buffer = new byte[dataOut.Length];
			bytesRead = stream.Read(buffer, 0, buffer.Length);

			dataOut.Write(buffer, 0, buffer.Length);

			return bytesRead > 0;

		}

		
		public bool Read(Stream dataOut, out int bytesRead, IResourceReadCallback callback) {

			bytesRead = -1;
			return false;
		}

		
		public bool Skip(long bytesToSkip, out long bytesSkipped, IResourceSkipCallback callback) {
			
			bytesSkipped = -2;
			return false;
		}

		public void Cancel() {
		}
		
		public bool CanGetCookie(CefSharp.Cookie cookie) {
			return true;
		}
		
		public bool CanSetCookie(CefSharp.Cookie cookie) {
			return true;
		}

		public bool ProcessRequest(IRequest request, ICallback callback) {
			return false;
		}


	}
}