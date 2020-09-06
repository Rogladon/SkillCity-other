using UnityEngine;
using SimpleJSON;

namespace SkillCity {
	public static partial class Api {
		[System.Serializable]
		public struct Error {
			public enum Code : int {
				UNKNOWN_ERROR = 0, // 'Произошла неизвестная ошибка'
				UNKNOWN_VERSION = 1,  // 'Передана неизвестная версия'
				API_NOT_EXISTS = 2, // 'API не существует'
				INVALID_METHOD = 3, // 'Передан неизвестный метод'
				INVALID_REQUEST = 4, // 'Неверный запрос'
				INVALID_SIGNATURE = 5, // 'Неверная подпись'
				NOT_AUTHORIZED = 6, // 'Пользователь не авторизован'
				INVALID_PHONE_NUMBER = 7, // 'Неверный формат номера телефона'
				INVALID_VALIDATION_CODE = 8, // 'Неверный код подтверждения'
				USER_NOT_EXISTS = 9, // 'Пользователь не существует'
				EMPTY_RESULT = 10, // 'Результат пустой'
				GAME_ALREADY_PLAYED = 11, // 'Вы уже играли сегодня в эту игру'
				NOT_ENOUGH_MONEY = 12, // 'Не достаточно игровой валюты'
				INVALID_EMAIL = 13, // 'Не корректный email адрес'
				TOO_SHOW_TEXT = 14, // 'Введен слишком короткий текст'
				COMPANY_DOES_NOT_EXIST = 15, // 'Данной компании не существует'
				__max_known,
			}

			public int code;
			public string msg;

			public override string ToString() {
				return $"API error: {(code < 0 || code >= (int)Code.__max_known ? "?" : ((Code)code).ToString())} (#{code})\tmsg: \"{msg}\"";
			}

			public static Error FromJson(string json) {
				return JsonUtility.FromJson<Error>(json);
			}

			public static Error FromJson(JSONNode json) {
				return JsonUtility.FromJson<Error>(json.ToString());
			}

			public static bool operator==(Error e, Code c) => e.code == (int)c;
			public static bool operator!=(Error e, Code c) => e.code != (int)c;
		}

		[System.Serializable]
		struct Identity {
			public string v;
			public string b;
			public string p;
			public string d;

			public static Identity Assemble() {
				return new Identity() {
					v = Application.version,
					b = Application.buildGUID,
					p = Application.platform.ToString(),
					d = SystemInfo.deviceUniqueIdentifier,
				};
			}
		}

		public static void Clear() {
			companiesList.Clear();
			jobsByCompany.Clear();
			jobs.Clear();
			skills.Clear();
		}
	}

	public static partial class Api {
		public static Error? ParseErrorResponse(JSONNode json) {
			var error = json["error"];
			if (error == null) {
				return null;
			} else {
				return Error.FromJson(error.ToString());
			}
		}
		public static Error? ParseErrorResponse(string str) {
			return ParseErrorResponse(JSON.Parse(str));
		}
		
		static object obj = new System.Security.Cryptography.MD5CryptoServiceProvider();
		private static string GetMd5Hash(string strToEncrypt) {
	
			var encoding = new System.Text.UTF8Encoding();
			var bytes = encoding.GetBytes(strToEncrypt);

			// encrypt bytes
			var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] hashBytes = md5.ComputeHash(bytes);

			// Convert the encrypted bytes back to a string (base 16)
			var hashString = "";

			for (var i = 0; i < hashBytes.Length; i++) {
				hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, "0"[0]);
			}

			return hashString.PadLeft(32, "0"[0]);
		}
	}
}
