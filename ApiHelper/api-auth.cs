using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace SkillCity {
	public static partial class Api {
		struct AuthRegRequestData {
			public int idUser;
			public string token;
			public string phone;

			public static AuthRegRequestData Assemble(string phoneNumber) {
				return new AuthRegRequestData() {
					idUser = GameState.user.idUser,
					token = GameState.user.token,
					phone = phoneNumber,
				};
			}
		}

		struct AuthRegCheckRequestData {
			public int code;

			public static AuthRegCheckRequestData Assemble(int code) {
				return new AuthRegCheckRequestData() {
					code = code,
				};
			}
		}

		public static ApiRequest<bool> AuthReg(string phoneNumber) {
			return new ApiRequest<bool>("Auth.reg", JsonUtility.ToJson(AuthRegRequestData.Assemble(phoneNumber)), (ApiRequest<bool>  ar) => {
				var response = ar.responseJson["response"];
				if (response != null) {
					return response["status"].AsBool;
				}
				return false;
			});
		}

		public static ApiRequest<User> AuthRegCheck(int code) {
			return new ApiRequest<User>("Auth.regCheck", JsonUtility.ToJson(AuthRegCheckRequestData.Assemble(code)), (ApiRequest<User>  ar) => {
				var response = ar.responseJson["response"];
				if (response != null) {
					var status = response["status"];
					if (status.AsBool) {
						var user = response["user"];
						if (user == null) {
							logger.LogError("api:Auth.regCheck", "Got non-error response with \"status\": true and without \"user\"");
						} else {
							return JsonUtility.FromJson<User>(user.ToString());
						}
					} else {
						logger.LogError("api:Auth.regCheck", "Got non-error response with \"status\": false");
					}
				}
				return new User();
			});
		}
	}
}