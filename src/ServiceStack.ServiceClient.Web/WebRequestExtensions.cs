using System;
using System.IO;
using System.Net;
using ServiceStack.Text;

namespace ServiceStack.ServiceClient.Web
{
	public static class WebRequestExtensions
	{
		public static WebResponse UploadFile(this WebRequest webRequest, 
			FileInfo uploadFileInfo, string uploadFileMimeType)
		{
			var boundary = "----------------------------" +
			DateTime.Now.Ticks.ToString("x");

			var httpWebRequest = (HttpWebRequest)webRequest;
			httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
			httpWebRequest.Method = "POST";
			httpWebRequest.AllowAutoRedirect = false;
			httpWebRequest.KeepAlive = false;

			Stream memStream = new MemoryStream();

			var boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary);

			var headerTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";

			var header = string.Format(headerTemplate, "upload", uploadFileInfo.Name, uploadFileMimeType);

			var headerbytes = System.Text.Encoding.ASCII.GetBytes(header);

			memStream.Write(headerbytes, 0, headerbytes.Length);
			using (var fs = uploadFileInfo.OpenRead())
			{
				fs.WriteTo(memStream);
			}

			memStream.Write(boundarybytes, 0, boundarybytes.Length);

			httpWebRequest.ContentLength = memStream.Length;

			var requestStream = httpWebRequest.GetRequestStream();

			memStream.Position = 0;
			var tempBuffer = new byte[memStream.Length];
			memStream.Read(tempBuffer, 0, tempBuffer.Length);
			memStream.Close();
			requestStream.Write(tempBuffer, 0, tempBuffer.Length);
			requestStream.Close();

			return httpWebRequest.GetResponse();
		}		

	}

}