using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace SkillCity {
	public static partial class Api {
		[System.Serializable]
		public struct Job {
			public int id;
			public string name;
			public string code;
			public bool active;
			public string description;
			public int price;
			public List<SkillSummary> skills;
			public Game game;
		}
		[System.Serializable]
		public struct SkillSummary {
			public int id;
			public string name;
			public string description;
			public bool userHas;
			public Ico ico;
		}
		[System.Serializable]
		public struct Game {
			public int id;
			public string head;
			public string text;
			public List<string> poster;
			public List<string> facts;
			public int lastPlay;
		}

		public static Dictionary<int, List<Job>> jobsByCompany { get; private set; } = new Dictionary<int, List<Job>>();
		public static Dictionary<int, Job> jobs { get; private set; } = new Dictionary<int, Job>();
		

		public static IEnumerator LoadJobsAsync(int companyId, System.Func<int, Api.Request.Status, Api.Error, bool> handler = null, int tries = 1000) {
			var request = Api.JobGet(companyId);
			for (int i = 0; i < tries; i++) {
				yield return request.Execute();
				if (request.status == Api.Request.Status.OK) {
					jobsByCompany[companyId] = request.apiResponse;
					foreach (var j in request.apiResponse) {
						jobs[j.id] = j;
					}
					logger.Log($"Successfully loaded {request.apiResponse.Count} jobs for company {companyId}");
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

		struct JobGetRequestData {
			public int idUser;
			public string token;
			public int idCompany;

			public JobGetRequestData(int companyId) {
				idUser = GameState.user.idUser;
				token = GameState.user.token;
				idCompany = companyId;
			}
		}

		public static ApiRequest<List<Job>> JobGet(int companyId) {
			return new ApiRequest<List<Job>>("Job.get", JsonUtility.ToJson(new JobGetRequestData(companyId)), (ApiRequest<List<Job>>  ar) => {
				var response = ar.responseJson["response"];
				List<Job>  l = new List<Job>();
				var responseArray = response.AsArray;
				for (int i = 0; i < responseArray.Count; i++) {
					try {
						l.Add(JsonUtility.FromJson<Job>(responseArray[i].ToString()));
					} catch (System.ArgumentException ae) {
						logger.LogError("Job.get", $"Failed to parse {i}th job in response: {ae}");
					}
				}
				return l;
			});
		}

		struct JobWorkedRequestData {
			public int idUser;
			public string token;
			public int idGame;
			public int score;
			public string sig;
			public int key;
			public Identity identity;

			public static JobWorkedRequestData Assemble(int gameId, int score) {
				return new JobWorkedRequestData() {
					idUser = GameState.user.idUser,
					token = GameState.user.token,
					idGame = gameId,
					score = score,
					sig = GetMd5Hash(GameState.user.idUser.ToString() + gameId.ToString() + score.ToString() + GameState.user.token + System.DateTime.Now.ToString("dd.MM.yyyy")),
					key = Random.Range(1000000, 99999999),
					identity = Identity.Assemble(),
				};
			}
		}

		public static ApiRequest<bool> JobWorked(int gameId, int score) {
			Debug.Log(JsonUtility.ToJson(JobWorkedRequestData.Assemble(gameId, score)));
			return new ApiRequest<bool>("Job.worked", JsonUtility.ToJson(JobWorkedRequestData.Assemble(gameId, score)), (ApiRequest<bool>  ar) => {
				var response = ar.responseJson["response"];
				if (response != null) {
					return response["status"].AsBool;
				}
				return false;
			});
		}
	}
}
