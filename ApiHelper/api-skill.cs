using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace SkillCity {
	public static partial class Api {
		[System.Serializable]
		public struct Ico {
			public string empty;
			public string has;
		}
		[System.Serializable]
		public struct Training {
			public int id;
			public string img;
			public string head;
			public string text;
		}
		[System.Serializable]
		public struct Answer {
			public int id;
			public string text;
		}
		[System.Serializable]
		public struct Exam {
			public int id;
			public string question;
			public List<Answer> answers;
		}
		[System.Serializable]
		public struct Skill {
			public int id;
			public string name;
			public string description;
			public bool userHas;
			public Ico ico;
			public List<Training> training;
			public List<Exam> exam;
		}

		public static Dictionary<int, Skill> skills { get; private set; } = new Dictionary<int, Skill>();

		struct SkillGetRequestData {
			public int idUser;
			public string token;
			public int idSkill;

			public static SkillGetRequestData Assemble(int skillId) {
				return new SkillGetRequestData() {
					idUser = GameState.user.idUser,
					token = GameState.user.token,
					idSkill = skillId,
				};
			}
		}

		public static ApiRequest<Skill> SkillGet(int skillId) {
			return new ApiRequest<Skill>("Skill.get", JsonUtility.ToJson(SkillGetRequestData.Assemble(skillId)), (ApiRequest<Skill>  ar) => {
				var response = ar.responseJson["response"];
				if (response != null) {
					Skill s = JsonUtility.FromJson<Skill>(response.ToString());
					skills[s.id] = s;
					return s;
				}
				return new Skill();
			});
		}

		struct SkillExamCheckRequestData {
			public int idUser;
			public string token;
			public int idExam;
			public int idAnswer;

			public static SkillExamCheckRequestData Assemble(int examId, int answerId) {
				return new SkillExamCheckRequestData() {
					idUser = GameState.user.idUser,
					token = GameState.user.token,
					idExam = examId,
					idAnswer = answerId,
				};
			}
		}

		public static ApiRequest<bool> SkillExamCheck(int examId, int answerId) {
			return new ApiRequest<bool>("Skill.examCheck", JsonUtility.ToJson(SkillExamCheckRequestData.Assemble(examId, answerId)), (ApiRequest<bool>  ar) => {
				var response = ar.responseJson["response"];
				if (response != null) {
					return response["status"].AsBool;
				}
				return false;
			});
		}
	}
}

