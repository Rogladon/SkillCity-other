using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace SkillCity {
	public static partial class Api {
		[System.Serializable]
		public struct Company {
			public int id;
			public string name;
			public bool active;
			public List<string> hello;
			public bool userJob;
		}
	
		public static List<Company> companiesList { get; private set; } = new List<Company>();
		public static Dictionary<int, Company> companies { get; private set; } = new Dictionary<int, Company>();

		public static IEnumerator LoadCompaniesAsync(System.Func<int, Api.Request.Status, Api.Error, bool> handler = null, int tries = 1000) {
			var request = CompanyGet();
			for (int i = 0; i < tries; i++) {
				yield return request.Execute();
				if (request.status == Api.Request.Status.OK) {
					companiesList = request.apiResponse;
					logger.Log($"Successfully loaded {companiesList.Count} companies");
					foreach (var c in companiesList) {
						companies[c.id] = c;
					}
					if (handler != null) handler(i, request.status, request.apiError);
					break;
				} else {
					if (handler != null) {
						if (!handler(i, request.status, request.apiError)) {
							break;
						}
					} else {
						break;
					}
				}
			}
		}

		struct CompanyGetRequestData {
			public int idUser;
			public string token;

			public static CompanyGetRequestData Assemble() {
				return new CompanyGetRequestData() {
					idUser = GameState.user.idUser,
					token = GameState.user.token,
				};
			}
		}

		public static ApiRequest<List<Company>> CompanyGet() {
			return new ApiRequest<List<Company>>("Company.get", JsonUtility.ToJson(CompanyGetRequestData.Assemble()), (ApiRequest<List<Company>>  ar) => {
				var response = ar.responseJson["response"];
				List<Company>  l = new List<Company>();
				var responseArray = response.AsArray;
				for (int i = 0; i < responseArray.Count; i++) {
					try {
						l.Add(JsonUtility.FromJson<Company>(responseArray[i].ToString()));
					} catch (System.ArgumentException ae) {
						logger.LogError("api:Company.get", $"Failed to parse {i}th Company in response: {ae}");
					}
				}
				return l;
			});
		}

		struct CompanyEmploymentRequestData {
			public int idUser;
			public string token;
			public int idCompany;

			public static CompanyEmploymentRequestData Assemble(int companyId) {
				return new CompanyEmploymentRequestData() {
					idUser = GameState.user.idUser,
					token = GameState.user.token,
					idCompany = companyId,
				};
			}
		}

		public static ApiRequest<bool> CompanyEmployment(int companyId) {
			return new ApiRequest<bool>("Company.employment", JsonUtility.ToJson(CompanyEmploymentRequestData.Assemble(companyId)), (ApiRequest<bool>  ar) => {
				var response = ar.responseJson["response"];
				if (response != null) {
					return response["status"].AsBool;
				}
				return false;
			});
		}
	}
}