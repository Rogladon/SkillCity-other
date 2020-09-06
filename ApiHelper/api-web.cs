using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.Events;

namespace SkillCity {
	public static partial class Api {
		public static readonly string URL = "skillcity.ru";
		public static readonly int VERSION = 1;

		public static string GetMethodUrl(string method) {
			return $"https://{URL}/api/{VERSION}/{method}/";
		}

		public class Request {
			public enum Status {
				OK,
				PENDING,
				API_ERROR,
				ERROR,
			}

			public Status status { get; private set; } = Status.PENDING;

			public readonly string method;
			public readonly string url;
			public readonly string sentData;

			private System.Func<Request, bool> successCallback;

			public Request(string method, string data, System.Func<Request, bool> successCallback = null) {
				this.method = method;
				this.url = GetMethodUrl(method);
				this.sentData = data;
				this.successCallback = successCallback;
			}

			public Request(string method, JSONNode data, System.Func<Request, bool> successCallback = null) : this(method, data.ToString(), successCallback) {}

			public string responseText { get; private set; } = null;
			private JSONNode json = null;
			public JSONNode responseJson { get {
				if (json == null && responseText != null) {
					json = JSON.Parse(responseText);
				}
				return json;
			} }

			public Error apiError { get; private set; }

			public int timout = -1;

			public IEnumerator Post() {
				var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST) {
					uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(sentData)),
					downloadHandler = new DownloadHandlerBuffer(),
				};
				if (timout > 0)
					request.timeout = timout;

				request.uploadHandler.contentType = "application/json";
				yield return request.SendWebRequest();
				if (request.isNetworkError) {
					status = Status.ERROR;
					logger.LogError(method, $"Network error");
				} else if (request.isHttpError) {
					status = Status.ERROR;
					logger.LogError(method, $"HTTP error {request.responseCode}");
				} else {
					status = Status.OK;
				}
				responseText = request.downloadHandler.text.ToString();
				if (status == Status.OK) {
					if (responseJson["error"] != null) {
						status = Status.API_ERROR;
						apiError = ParseErrorResponse(responseJson).Value;
						logger.LogError(method, apiError);
					}
				}
				request.Dispose();
				if (successCallback != null && status == Status.OK) {
					successCallback(this);
				}
			}

			public override string ToString() {
				return $"Api Request {method} ({status.ToString()})";
			}
		}

		public class ApiRequest<T> : Request {
			public class OkEvent : UnityEvent<ApiRequest<T>> {}
			public class ErrorEvent : UnityEvent<ApiRequest<T>> {}

			public OkEvent onOk { get; } = new OkEvent();
			public ErrorEvent onError { get; } = new ErrorEvent();

			public T apiResponse { get; private set; }

			public ApiRequest(string method, string data, System.Func<ApiRequest<T>, T> successCallback) : base(method, data, (Request r) => {
				ApiRequest<T>  ar = (ApiRequest<T>)r;
				return (ar.apiResponse = successCallback(ar)) != null;
			}) {}

			public ApiRequest(string method, JSONNode data, System.Func<ApiRequest<T>, T> successCallback) : this(method, data.ToString(), successCallback) {} 

			public int currentTry { get; private set; } = 0;
			private bool stopTrying = false;

			public bool stopTryingOnApiError = false;

			public IEnumerator Execute(int maxTries = 1, float pauseBetweenTries = 0.5f) {
				if (maxTries < 0) {
					maxTries = 128000;
				}
				stopTrying = false;
				for (currentTry = 0; currentTry < maxTries; currentTry++) {
					yield return Post();
					if (status == Request.Status.OK) {
						onOk.Invoke(this);
						break;
					} else {
						onError.Invoke(this);
						if (status == Api.Request.Status.API_ERROR) {
							StopTrying();
						}
						if (stopTrying)
							break;
						yield return new WaitForSeconds(pauseBetweenTries);
					}
				}
			}

			public ApiRequest<T> OnOk(UnityAction<ApiRequest<T>> a) {
				onOk.AddListener(a);
				return this;
			}

			public ApiRequest<T> OnError(UnityAction<ApiRequest<T>> a) {
				onError.AddListener(a);
				return this;
			}

			public void StopTrying() {
				stopTrying = true;
			}

			public ApiRequest<T> StopTryingOnApiError() {
				stopTryingOnApiError = true;
				return this;
			}

			public ApiRequest<T> Timeout(int to) {
				this.timout = to;
				return this;
			}
		}

		public class TextureRequest {
			public class OkEvent : UnityEvent<Texture> {}
			public class ErrorEvent : UnityEvent<TextureRequest> {}

			public OkEvent onOk { get; } = new OkEvent();
			public ErrorEvent onError { get; } = new ErrorEvent();
			public string url;

			public Texture responseTexture;

			public TextureRequest(string url) {
				this.url = url;
			}

			public int currentTry { get; private set; } = 0;
			private bool stopTrying = false;

			public IEnumerator Execute(int maxTries = 1, float pauseBetweenTries = 0.5f) {
				var request = UnityWebRequestTexture.GetTexture(url);
				if (maxTries < 0) {
					maxTries = 128000;
				}
				stopTrying = false;
				for (currentTry = 0; currentTry < maxTries; currentTry++) {
					yield return request.SendWebRequest();
					if (request.isNetworkError || request.isHttpError) {
						Api.logger.LogError("Legacy API manager", $"Failed to load texture \"{url}\": {request.error}");
						responseTexture = null;
						onError.Invoke(this);
						if (stopTrying)
							break;
						yield return new WaitForSeconds(pauseBetweenTries);
					} else {
						responseTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
						onOk.Invoke(responseTexture);
					}
				}
				request.Dispose();

			}

			public TextureRequest OnOk(UnityAction<Texture> a) {
				onOk.AddListener(a);
				return this;
			}

			public TextureRequest OnError(UnityAction<TextureRequest> a) {
				onError.AddListener(a);
				return this;
			}

			public void StopTrying() {
				stopTrying = true;
			}
		}

		public static TextureRequest GetTexture(string url) => new TextureRequest(url);
	}
}
