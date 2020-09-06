using UnityEngine;
using SimpleJSON;

namespace SkillCity {
	public static partial class Api {
		[System.Serializable]
		public struct User {
			public int idUser;
			public bool active;
			public string avatar;
			public int money;
			public int age;
			public string token;
		}

		struct UserGetProfileRequestData {
			public int idUser;
			public string token;

			public static UserGetProfileRequestData Assemble() {
				return new UserGetProfileRequestData() {
					idUser = GameState.user.idUser,
					token = GameState.user.token,
				};
			}
		}

		struct UserEditProfileRequestData {
			public int idUser;
			public string token;
			public int userAge;
			public string userAvatar;
			public int avatarRotate;

			public static UserEditProfileRequestData Assemble(int? userAge = null, string userAvatar = null, int? avatarRotate = null) {
				return new UserEditProfileRequestData() {
					idUser = GameState.user.idUser,
					token = GameState.user.token,
					userAge = userAge.HasValue ? userAge.Value : GameState.user.age,
					userAvatar = userAvatar != null ? userAvatar : "",
					avatarRotate = avatarRotate.HasValue ? avatarRotate.Value : 0,
				};
			}
		}

		public static ApiRequest<User> UserGetProfile() {
			return new ApiRequest<User>("User.getProfile", JsonUtility.ToJson(UserGetProfileRequestData.Assemble()), (ApiRequest<User>  ar) => {
				var response = ar.responseJson["response"];
				if (response != null) {
					return JsonUtility.FromJson<User>(response.ToString());
				}
				return new User();
			});
		}

		public static ApiRequest<User> UserEditProfile(int? userAge = null, string userAvatar = null, int? avatarRotate = null) {
			return new ApiRequest<User>("User.editProfile", JsonUtility.ToJson(UserEditProfileRequestData.Assemble(userAge, userAvatar, avatarRotate)), (ApiRequest<User>  ar) => {
				var response = ar.responseJson["response"];
				if (response != null) {
					return JsonUtility.FromJson<User>(response.ToString());
				}
				return new User();
			});
		}
	}
}
